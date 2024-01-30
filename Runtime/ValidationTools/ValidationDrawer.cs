using UnityEngine;
using UnityEngine.UIElements;

namespace jeanf.validationTools
{
    #if UNITY_EDITOR
    using UnityEditor;

    [CustomPropertyDrawer(typeof(ValidationAttribute))]
    public class ValidationDrawer : PropertyDrawer
    {
        private const int BoxPadding = 10;
        private const float Padding = 10f;
        private const float Offset = 15f;

        private float _height = 10f;
        private float _helpBoxHeight = 0f;
        
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var propertyHeight = base.GetPropertyHeight(property, label);
            
            if (property.objectReferenceValue == null)
            {
                var customAttribute = attribute as ValidationAttribute;
                var style = EditorStyles.helpBox;
                style.alignment = TextAnchor.MiddleLeft;
                style.wordWrap = true;
                style.padding = new RectOffset(BoxPadding, BoxPadding, BoxPadding, BoxPadding);
                style.fontSize = 12;

                _helpBoxHeight = style.CalcHeight(new GUIContent(customAttribute.Text), Screen.width);
                _height = _helpBoxHeight + propertyHeight + Offset;
                return _height;
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

                position.height = _helpBoxHeight;
                position.y += Padding *.5f;
                EditorGUI.HelpBox(position, customAttribute.Text, MessageType.Error);
                position.height = _height;
                EditorGUI.DrawRect(position, new Color(1f, .2f, .2f, .1f));

                position.y += _helpBoxHeight + Padding;
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
