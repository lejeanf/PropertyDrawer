namespace jeanf.propertyDrawer
{
#if UNITY_EDITOR
    using System.Collections.Generic;
    using System.Reflection;
    using jeanf.validationTools;
    using UnityEditor;
    using UnityEngine;

    /// <summary>How a [DrawIf] field should currently appear.</summary>
    internal enum FieldVisibility { Shown, ReadOnly, Hidden }

    /// <summary>
    /// The ONE renderer behind both DrawIfPropertyDrawer and ValidationDrawer.
    /// Unity runs a single PropertyDrawer per field, so a field carrying both
    /// [DrawIf] and [Validation] only ever reaches one of the two drawers — each of
    /// them therefore reads the OTHER attribute off the field (fieldInfo) and hands
    /// everything to this class, which applies both: the DrawIf condition decides
    /// whether the field is shown / greyed / hidden, and a shown field that is
    /// required-but-unset gets the orange validation treatment.
    /// Gate the requirement with RequiredIf when the field is only needed while
    /// the DrawIf condition holds — the scanner (banner, hierarchy dot, console) does
    /// not read [DrawIf], so it would otherwise report a field the inspector hides.
    /// </summary>
    internal static class ConditionalField
    {
        private static readonly HashSet<string> WarnedLookups = new HashSet<string>();

        public static float GetHeight(SerializedProperty property, GUIContent label, FieldInfo field,
            DrawIfAttribute drawIfFallback, ValidationAttribute validationFallback)
        {
            switch (Resolve(property, field, drawIfFallback))
            {
                case FieldVisibility.Hidden: return 0f;
                case FieldVisibility.ReadOnly: return EditorGUI.GetPropertyHeight(property, label, true);
            }

            var validation = FindValidation(field, validationFallback);
            return validation != null
                ? ValidationDrawer.GetHeight(property, label, validation)
                : EditorGUI.GetPropertyHeight(property, label, true);
        }

        public static void Draw(Rect position, SerializedProperty property, GUIContent label, FieldInfo field,
            DrawIfAttribute drawIfFallback, ValidationAttribute validationFallback)
        {
            var conditional = HasDrawIf(field, drawIfFallback);
            switch (Resolve(property, field, drawIfFallback))
            {
                case FieldVisibility.Hidden:
                    return;
                case FieldVisibility.ReadOnly:
                {
                    var wasEnabled = GUI.enabled;
                    GUI.enabled = false;
                    DrawPlain(position, property, label, conditional);
                    GUI.enabled = wasEnabled;
                    return;
                }
            }

            var validation = FindValidation(field, validationFallback);
            if (validation == null)
            {
                DrawPlain(position, property, label, conditional);
                return;
            }

            if (conditional) EditorGUI.indentLevel++;
            ValidationDrawer.Draw(position, property, label, validation);
            if (conditional) EditorGUI.indentLevel--;
        }

        /// <summary>
        /// Combines every [DrawIf] on the field (all conditions must hold to show it).
        /// <paramref name="fallback"/> is the attribute Unity handed the drawer, used
        /// when fieldInfo is unavailable.
        /// </summary>
        public static FieldVisibility Resolve(SerializedProperty property, FieldInfo field, DrawIfAttribute fallback)
        {
            var visibility = FieldVisibility.Shown;
            foreach (var drawIf in DrawIfs(field, fallback))
            {
                if (IsConditionMet(property, drawIf)) continue;
                if (drawIf.disablingType == DisablingType.DontDraw) return FieldVisibility.Hidden;
                visibility = FieldVisibility.ReadOnly;
            }
            return visibility;
        }

        /// <summary>
        /// Evaluates one [DrawIf] against the compared member — a sibling of the field
        /// first (works inside nested serializable classes), then a top-level property.
        /// An unresolvable member counts as met (the field is drawn), with one warning.
        /// </summary>
        public static bool IsConditionMet(SerializedProperty property, DrawIfAttribute drawIf)
        {
            var compared = FindCompared(property, drawIf.comparedPropertyName);
            if (compared == null)
            {
                var key = $"{property.serializedObject.targetObject.GetType().Name}.{property.name}→{drawIf.comparedPropertyName}";
                if (WarnedLookups.Add(key))
                    Debug.LogWarning($"[DrawIf] '{property.name}' on {property.serializedObject.targetObject.GetType().Name} compares against '{drawIf.comparedPropertyName}', which does not exist (renamed field?). The field is drawn unconditionally.", property.serializedObject.targetObject);
                return true;
            }

            var comparedValue = compared.GetValue<object>();

            NumericType numericField = null;
            NumericType numericValue = null;
            try
            {
                numericField = new NumericType(comparedValue);
                numericValue = new NumericType(drawIf.comparedValue);
            }
            catch (NumericTypeExpectedException)
            {
                if (drawIf.comparisonType != ComparisonType.Equals && drawIf.comparisonType != ComparisonType.NotEqual)
                {
                    Debug.LogError($"[DrawIf] The only comparison types available to '{comparedValue?.GetType().Name ?? "null"}' are Equals and NotEqual (on '{property.serializedObject.targetObject.name}').", property.serializedObject.targetObject);
                    return true;
                }
            }

            switch (drawIf.comparisonType)
            {
                case ComparisonType.Equals: return Equals(comparedValue, drawIf.comparedValue);
                case ComparisonType.NotEqual: return !Equals(comparedValue, drawIf.comparedValue);
                case ComparisonType.GreaterThan: return numericField > numericValue;
                case ComparisonType.SmallerThan: return numericField < numericValue;
                case ComparisonType.SmallerOrEqual: return numericField <= numericValue;
                case ComparisonType.GreaterOrEqual: return numericField >= numericValue;
                default: return true;
            }
        }

        private static SerializedProperty FindCompared(SerializedProperty property, string name)
        {
            var path = property.propertyPath;
            var dot = path.LastIndexOf('.');
            if (dot >= 0)
            {
                var sibling = property.serializedObject.FindProperty(path.Substring(0, dot + 1) + name);
                if (sibling != null) return sibling;
            }
            return property.serializedObject.FindProperty(name);
        }

        private static void DrawPlain(Rect position, SerializedProperty property, GUIContent label, bool indent)
        {
            if (indent) EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property, label, true);
            if (indent) EditorGUI.indentLevel--;
        }

        private static IEnumerable<DrawIfAttribute> DrawIfs(FieldInfo field, DrawIfAttribute fallback)
        {
            if (field != null)
            {
                var found = false;
                foreach (var attribute in field.GetCustomAttributes<DrawIfAttribute>(true))
                {
                    found = true;
                    yield return attribute;
                }
                if (found) yield break;
            }
            if (fallback != null) yield return fallback;
        }

        private static bool HasDrawIf(FieldInfo field, DrawIfAttribute fallback)
        {
            if (fallback != null) return true;
            return field != null && field.IsDefined(typeof(DrawIfAttribute), true);
        }

        private static ValidationAttribute FindValidation(FieldInfo field, ValidationAttribute fallback)
        {
            if (fallback != null) return fallback;
            return field?.GetCustomAttribute<ValidationAttribute>(true);
        }
    }
#endif
}
