namespace jeanf.propertyDrawer
{
    using UnityEngine;
    using UnityEditor;

    public class ReadOnlyAttribute : PropertyAttribute
    {
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference && 
                property.objectReferenceValue is ScriptableObject)
            {
                return GetScriptableObjectHeight(property, label);
            }
            
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var previousGUIState = GUI.enabled;
            
            GUI.enabled = false;
            
            if (property.propertyType == SerializedPropertyType.ObjectReference && 
                property.objectReferenceValue is ScriptableObject)
            {
                DrawReadOnlyScriptableObject(position, property, label);
            }
            else
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
            
            GUI.enabled = previousGUIState;
        }

        private float GetScriptableObjectHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight;
            
            if (property.objectReferenceValue == null)
            {
                return totalHeight;
            }

            if (property.isExpanded && AreAnySubPropertiesVisible(property))
            {
                var data = property.objectReferenceValue as ScriptableObject;
                if (data == null) return EditorGUIUtility.singleLineHeight;
                
                SerializedObject serializedObject = new SerializedObject(data);
                serializedObject.Update();
                
                SerializedProperty prop = serializedObject.GetIterator();
                
                if (prop.NextVisible(true))
                {
                    do
                    {
                        if (prop.name == "m_Script") continue;
                        
                        float propertyHeight = EditorGUI.GetPropertyHeight(prop, true);
                        totalHeight += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                        
                    } while (prop.NextVisible(false));
                }

                totalHeight += EditorGUIUtility.standardVerticalSpacing * 2;
            }

            return totalHeight;
        }

        private void DrawReadOnlyScriptableObject(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.objectReferenceValue != null && AreAnySubPropertiesVisible(property))
            {
                var previousGUIState = GUI.enabled;
                GUI.enabled = true;
                
                property.isExpanded = EditorGUI.Foldout(
                    new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight), 
                    property.isExpanded, 
                    property.displayName, 
                    true);
                
                GUI.enabled = false;

                const int offset = 2;
                EditorGUI.ObjectField(
                    new Rect(EditorGUIUtility.labelWidth + offset, position.y,
                        position.width - EditorGUIUtility.labelWidth - offset, EditorGUIUtility.singleLineHeight),
                    property, GUIContent.none);

                GUI.enabled = previousGUIState;

                if (property.isExpanded)
                {
                    DrawReadOnlyExpandedProperties(position, property);
                }
            }
            else
            {
                EditorGUI.ObjectField(position, property, label);
            }
        }

        private void DrawReadOnlyExpandedProperties(Rect position, SerializedProperty property)
        {
            float contentY = position.y + EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            float contentHeight = position.height - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            
            var originalBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f, 0.3f); // Light gray tint
            
            GUI.Box(
                new Rect(position.x, contentY - 1, position.width, contentHeight + 1), 
                "", GUI.skin.box);
                
            GUI.backgroundColor = originalBackgroundColor;

            EditorGUI.indentLevel++;
            var data = (ScriptableObject)property.objectReferenceValue;
            SerializedObject serializedObject = new SerializedObject(data);
            serializedObject.Update();

            SerializedProperty prop = serializedObject.GetIterator();
            float currentY = contentY + EditorGUIUtility.standardVerticalSpacing;
            
            if (prop.NextVisible(true))
            {
                do
                {
                    if (prop.name == "m_Script") continue;
                    
                    float propertyHeight = EditorGUI.GetPropertyHeight(prop, true);
                    Rect propertyRect = new Rect(position.x, currentY, position.width, propertyHeight);
                    
                    EditorGUI.PropertyField(propertyRect, prop, true);
                    
                    currentY += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
                    
                } while (prop.NextVisible(false));
            }

            EditorGUI.indentLevel--;
        }

        private static bool AreAnySubPropertiesVisible(SerializedProperty property)
        {
            var data = (ScriptableObject)property.objectReferenceValue;
            if (data == null) return false;
            
            SerializedObject serializedObject = new SerializedObject(data);
            SerializedProperty prop = serializedObject.GetIterator();

            while (prop.NextVisible(true))
            {
                if (prop.name == "m_Script") continue;
                return true;
            }

            return false;
        }
    }
#endif
}