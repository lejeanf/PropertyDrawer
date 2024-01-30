


namespace jeanf.validationTools
{
    #if UNITY_EDITOR
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    [InitializeOnLoad]
    public class ValidationHighlighter
    {
        private static readonly Color k_backgroundColor = new Color(0.7843f, 0.7843f, 0.7843f);
        private static readonly Color k_backgroundProColor = new Color(0.2196f, 0.2196f, 0.2196f);
        private static readonly Color k_backgroundSelectedColor = new Color(0.22745f, 0.447f, 0.6902f);
        private static readonly Color k_backgroundSelectedProColor = new Color(0.1725f, 0.3647f, 0.5294f);

        static ValidationHighlighter()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            var isSelected = Selection.instanceIDs.Contains(instanceID);
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj != null)
            {
                var validatable = obj.GetComponent<IValidatable>();
                if (validatable is { IsValid: false })
                {
                    selectionRect.x += 16.5f; 
                    var backgroundColor = EditorGUIUtility.isProSkin ? k_backgroundProColor : k_backgroundColor;
                    if (isSelected)
                        backgroundColor = EditorGUIUtility.isProSkin
                            ? k_backgroundSelectedProColor
                            : k_backgroundSelectedColor;
                    

                    var textStyle = new GUIStyle(GUI.skin.label);
                    textStyle.normal.textColor = new Color(1f,0.4705f,0.2196f);

                    var width = textStyle.CalcSize(new GUIContent(obj.name)).x;
                    
                    var backgroundRect = selectionRect;
                    backgroundRect.width = width;
                    
                    EditorGUI.DrawRect(backgroundRect, backgroundColor);
                    EditorGUI.LabelField(selectionRect, obj.name, textStyle);
                    EditorApplication.RepaintHierarchyWindow();
                }
            }
        }
    }
    #endif
}  