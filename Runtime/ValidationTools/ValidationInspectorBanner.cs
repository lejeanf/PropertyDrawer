namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Fallback inspector for every MonoBehaviour: when the component has a
    /// validation issue it draws an ORANGE banner directly under the component's
    /// title bar — "⚠ ComponentName needs setup" plus the issue list — then the
    /// normal inspector (where [Validation] fields are already tinted orange by
    /// ValidationDrawer). Valid components render exactly like the default
    /// inspector. Components with their own [CustomEditor] keep it (a more
    /// specific editor always wins over this fallback) — they still get the
    /// hierarchy dot, field tint and console log, just not the banner.
    /// (Unity has no public API to recolor the title bar itself, so the banner
    /// under it is how the component gets its orange title.)
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class ValidationInspectorBanner : Editor
    {
        private static readonly List<ValidationIssue> Issues = new List<ValidationIssue>();

        public override void OnInspectorGUI()
        {
            var component = target as Component;
            if (component != null)
            {
                Issues.Clear();
                ValidationScanner.GetIssues(component, Issues);
                if (Issues.Count > 0) DrawBanner(component);
            }

            DrawDefaultInspector();
        }

        private static void DrawBanner(Component component)
        {
            var title = $"⚠  {component.GetType().Name} needs setup";
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var height = lineHeight + 6f + Issues.Count * lineHeight;

            var rect = GUILayoutUtility.GetRect(0f, height, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(rect, ValidationUi.OrangeWash);
            // Solid orange edge on the left, like Unity's own message stripes.
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3f, rect.height), ValidationUi.Orange);

            var line = new Rect(rect.x + 8f, rect.y + 3f, rect.width - 12f, lineHeight);
            EditorGUI.LabelField(line, title, ValidationUi.OrangeBoldLabel);

            foreach (var issue in Issues)
            {
                line.y += lineHeight;
                var text = string.IsNullOrEmpty(issue.FieldName) ? issue.Message : $"• {issue.FieldName}: {issue.Message}";
                EditorGUI.LabelField(line, text, ValidationUi.OrangeLabel);
            }

            EditorGUILayout.Space(2f);
        }
    }
#endif
}
