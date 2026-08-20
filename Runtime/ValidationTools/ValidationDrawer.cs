using UnityEngine;

namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using UnityEditor;

    /// <summary>
    /// Draws a [Validation("...")] field. When the field is unset (null object
    /// reference or empty string) it turns ORANGE — translucent wash + tinted
    /// label + help box with the attribute's message — so the culprit field is
    /// obvious at a glance. When set, it draws like a normal field.
    /// NOTE: do not combine [Validation] with another drawer attribute
    /// (e.g. [DrawIf]) on the same field — Unity only runs one PropertyDrawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(ValidationAttribute))]
    public class ValidationDrawer : PropertyDrawer
    {
        private const float Spacing = 4f;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = EditorGUI.GetPropertyHeight(property, label, true);
            if (!NeedsAttention(property)) return propertyHeight;
            return propertyHeight + HelpBoxHeight() + Spacing * 2f;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!NeedsAttention(property))
            {
                EditorGUI.PropertyField(position, property, label, true);
                return;
            }

            // Orange wash behind the whole block: this field needs attention.
            EditorGUI.DrawRect(position, ValidationUi.OrangeWash);

            var message = (attribute as ValidationAttribute)?.Text;
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
        private bool NeedsAttention(SerializedProperty property)
        {
            if (!IsUnset(property)) return false;
            return ValidationScanner.IsRequired(property.serializedObject.targetObject,
                attribute as ValidationAttribute);
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
