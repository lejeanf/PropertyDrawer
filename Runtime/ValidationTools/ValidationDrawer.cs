using UnityEngine;
using UnityEngine.UIElements;

namespace jeanf.validationTools
{
    #if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ValidationAttribute))]
    public class ValidationDrawer : PropertyDrawer
    {
        private const int boxPadding = 10;
        private const float padding = 10f;
        private const float offset = 15f;

        private float height = 10f;
        private float helpBoxHeight = 0f;
        
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = base.GetPropertyHeight(property, label);
            
            if (property.objectReferenceValue == null)
            {
                var customAttribute = attribute as ValidationAttribute;
                var style = EditorStyles.helpBox;
                style.alignment = TextAnchor.MiddleLeft;
                style.wordWrap = true;
                style.padding = new RectOffset(boxPadding, boxPadding, boxPadding, boxPadding);
                style.fontSize = 12;

                helpBoxHeight = style.CalcHeight(new GUIContent(customAttribute.Text), Screen.width);
                height = helpBoxHeight + propertyHeight + offset;
                return height;
            }
            else
            {
                return propertyHeight;
            }

        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue == null)
            {
                var customAttribute = attribute as ValidationAttribute;

                position.height = helpBoxHeight;
                position.y += padding *.5f;
                EditorGUI.HelpBox(position, customAttribute.Text, MessageType.Error);
                position.height = height;
                EditorGUI.DrawRect(position, new Color(1f, .2f, .2f, .1f));

                position.y += helpBoxHeight + padding;
                position.height = base.GetPropertyHeight(property, label);
                EditorGUI.PropertyField(position, property, new GUIContent(property.displayName));
            }
            else
            {
                EditorGUI.PropertyField(position, property, new GUIContent(property.displayName));
            }
        }
    }
    #endif
}
