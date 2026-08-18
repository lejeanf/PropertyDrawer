namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>Shared colors/styles so every validation surface speaks the same orange.</summary>
    public static class ValidationUi
    {
        /// <summary>The attention color used everywhere (hierarchy, inspector, fields).</summary>
        public static readonly Color Orange = new Color(1f, 0.6f, 0.1f);
        /// <summary>Translucent wash used behind fields/banners that need attention.</summary>
        public static readonly Color OrangeWash = new Color(1f, 0.6f, 0.1f, 0.14f);

        private static GUIStyle _orangeLabel;
        public static GUIStyle OrangeLabel
        {
            get
            {
                if (_orangeLabel == null)
                {
                    _orangeLabel = new GUIStyle(GUI.skin.label);
                    _orangeLabel.normal.textColor = Orange;
                    _orangeLabel.hover.textColor = Orange;
                }
                return _orangeLabel;
            }
        }

        private static GUIStyle _orangeBoldLabel;
        public static GUIStyle OrangeBoldLabel
        {
            get
            {
                if (_orangeBoldLabel == null)
                {
                    _orangeBoldLabel = new GUIStyle(EditorStyles.boldLabel);
                    _orangeBoldLabel.normal.textColor = Orange;
                    _orangeBoldLabel.hover.textColor = Orange;
                }
                return _orangeBoldLabel;
            }
        }

        private static readonly List<ValidationIssue> Issues = new List<ValidationIssue>();

        /// <summary>
        /// Draws the orange "needs setup" banner for a component — title bar stripe plus the
        /// issue list — or nothing at all when the component is clean. ValidationInspectorBanner
        /// calls this for components using the default inspector; components with their own
        /// [CustomEditor] call it themselves so they keep the same banner instead of losing it.
        /// </summary>
        public static void DrawIssuesBanner(Component component)
        {
            if (component == null) return;

            Issues.Clear();
            ValidationScanner.GetIssues(component, Issues);
            if (Issues.Count == 0) return;

            var title = $"⚠  {component.GetType().Name} needs setup";
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var height = lineHeight + 6f + Issues.Count * lineHeight;

            var rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, OrangeWash);
            // Solid orange edge on the left, like Unity's own message stripes.
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height), Orange);

            var line = new Rect(rect.x + 8f, rect.y + 3f, rect.width - 12f, lineHeight);
            EditorGUI.LabelField(line, title, OrangeBoldLabel);

            foreach (var issue in Issues)
            {
                line.y += lineHeight;
                var text = string.IsNullOrEmpty(issue.FieldName) ? issue.Message : $"• {issue.FieldName}: {issue.Message}";
                EditorGUI.LabelField(line, text, OrangeLabel);
            }

            EditorGUILayout.Space(2f);
        }
    }
#endif
}
