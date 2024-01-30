


namespace jeanf.validationTools
{
    #if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    [InitializeOnLoad]
    public class ValidationHighlighter
    {
        private static readonly Color BackgroundColor = new Color(0.7843f, 0.7843f, 0.7843f);
        private static readonly Color BackgroundProColor = new Color(0.2196f, 0.2196f, 0.2196f);
        private static readonly Color BackgroundSelectedColor = new Color(0.22745f, 0.447f, 0.6902f);
        private static readonly Color BackgroundSelectedProColor = new Color(0.1725f, 0.3647f, 0.5294f);

        static ValidationHighlighter()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var isSelected = Selection.instanceIDs.Contains(instanceID);
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;
            var validatable = obj.GetComponent<IValidatable>();
            if (validatable is not { IsValid: false }) return;
            selectionRect.x += 16f; 
            selectionRect.y -= 1f;
                    
            var backgroundColor = EditorGUIUtility.isProSkin ? BackgroundProColor : ValidationHighlighter.BackgroundColor;
            if (isSelected)
                backgroundColor = EditorGUIUtility.isProSkin
                    ? BackgroundSelectedProColor
                    : BackgroundSelectedColor;
                    

            var textStyle = new GUIStyle(GUI.skin.label);
            textStyle.normal.textColor = new Color(1f,0.4705f,0.2196f);

            var width = textStyle.CalcSize(new GUIContent(obj.name)).x;
                    
            var backgroundRect = selectionRect;
            backgroundRect.width = width;
            backgroundRect.position = new Vector2(backgroundRect.position.x, backgroundRect.position.y + 1f);
                    
            EditorGUI.DrawRect(backgroundRect, backgroundColor);
            EditorGUI.LabelField(selectionRect, obj.name, textStyle);
            EditorApplication.RepaintHierarchyWindow();
        }
    }
    #endif
}  