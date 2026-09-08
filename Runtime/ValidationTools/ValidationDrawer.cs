using UnityEngine;

namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using jeanf.propertyDrawer;
    using UnityEditor;

    /// <summary>
    /// Draws a [Validation("...")] field. When the field is unset (null object
    /// reference or empty string) and currently required (RequiredIf gate) it turns
    /// ORANGE — translucent wash + tinted label + help box with the attribute's
    /// message — so the culprit field is obvious at a glance. When set, it draws
    /// like a normal field.
    /// Cooperates with [DrawIf] on the same field: whichever of the two drawers Unity
    /// picks, ConditionalField applies both attributes (hide / grey per DrawIf, orange
    /// per Validation). Gate the requirement with RequiredIf on the same condition so
    /// the scanner does not flag a field the inspector hides.
    /// </summary>
    [CustomPropertyDrawer(typeof(ValidationAttribute))]
    public class ValidationDrawer : PropertyDrawer
    {
        private const float Spacing = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => ConditionalField.GetHeight(property, label, fieldInfo, null, attribute as ValidationAttribute);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => ConditionalField.Draw(position, property, label, fieldInfo, null, attribute as ValidationAttribute);

        /// <summary>Height of the field, plus the warning block while it needs attention.</summary>
        internal static float GetHeight(SerializedProperty property, GUIContent label, ValidationAttribute attribute)
        {
            var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            if (!NeedsAttention(property, attribute)) return propertyHeight;
            return propertyHeight + HelpBoxHeight() + Spacing * 2f;
        }

        /// <summary>The orange treatment (or a plain field when nothing is wrong).</summary>
        internal static void Draw(Rect position, SerializedProperty property, GUIContent label, ValidationAttribute attribute)
        {
            if (!NeedsAttention(property, attribute))
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Orange wash behind the whole block: this field needs attention.
            EditorGUI.DrawRect(position, ValidationUi.OrangeWash);

            var message = attribute?.Text;
            if (string.IsNullOrEmpty(message)) message = $"'{property.displayName}' is not assigned.";

            var helpRect = new Rect(position.x, position.y + Spacing, position.width, HelpBoxHeight());
            EditorGUI.HelpBox(helpRect, message, MessageType.Warning);

            var fieldRect = new Rect(position.x, helpRect.yMax + Spacing, position.width,
                EditorGUI.GetPropertyHeight(property, label, true));
            var previousColor = GUI.backgroundColor;
            GUI.backgroundColor = ValidationUi.Orange;
            EditorGUI.PropertyField(fieldRect, property, new GUIContent(label.text, label.image, message), true);
            GUI.backgroundColor = previousColor;
        }

        private static float HelpBoxHeight() => EditorGUIUtility.singleLineHeight * 2f;

        /// <summary>Unset AND currently required (RequiredIf gate, evaluated on the component).</summary>
        private static bool NeedsAttention(SerializedProperty property, ValidationAttribute attribute)
        {
            if (!IsUnset(property)) return false;
            return ValidationScanner.IsRequired(property.serializedObject.targetObject, attribute);
        }

        internal static bool IsUnset(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.ObjectReference: return property.objectReferenceValue == null;
                case SerializedPropertyType.String: return string.IsNullOrEmpty(property.stringValue);
                default: return false;
            }
        }
    }
#endif
}
