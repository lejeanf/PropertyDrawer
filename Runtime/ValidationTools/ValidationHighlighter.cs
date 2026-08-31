namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Hierarchy feedback for ill-setup GameObjects: the name turns ORANGE and a
    /// small orange dot appears left of it whenever any component on the object
    /// has a validation issue ([Validation] field unset, or IValidatable false).
    /// ANCESTORS of an ill-setup object get a dimmed dot (name untouched), so a
    /// collapsed parent can never hide a problem in its children.
    /// Results are cached per object and invalidated on any object change, so
    /// scanning cost stays negligible even in large scenes.
    /// </summary>
    [InitializeOnLoad]
    public static class ValidationHighlighter
    {
        private static readonly Color BackgroundColor = new Color(0.7843f, 0.7843f, 0.7843f);
        private static readonly Color BackgroundProColor = new Color(0.2196f, 0.2196f, 0.2196f);
        private static readonly Color BackgroundSelectedColor = new Color(0.22745f, 0.447f, 0.6902f);
        private static readonly Color BackgroundSelectedProColor = new Color(0.1725f, 0.3647f, 0.5294f);

        private enum IssueScope { None, Self, InChildren }
        private static readonly Dictionary<EntityId, IssueScope> IssuesById = new Dictionary<EntityId, IssueScope>();

        static ValidationHighlighter()
        {
            EditorApplication.hierarchyWindowItemByEntityIdOnGUI += OnHierarchyWindowItemOnGUI;
            EditorApplication.hierarchyChanged += ClearCache;
            Undo.undoRedoPerformed += ClearCache;
            // Field edits in the inspector don't fire hierarchyChanged — this does.
            ObjectChangeEvents.changesPublished += (ref ObjectChangeEventStream stream) => ClearCache();
        }

        private static void ClearCache()
        {
            IssuesById.Clear();
            EditorApplication.RepaintHierarchyWindow();
        }

        private static void OnHierarchyWindowItemOnGUI(EntityId entityId, Rect selectionRect)
        {
            if (Event.current.type != EventType.Repaint) return;

            if (!IssuesById.TryGetValue(entityId, out var scope))
            {
                scope = ComputeScope(EditorUtility.EntityIdToObject(entityId) as GameObject);
                IssuesById[entityId] = scope;
            }
            if (scope == IssueScope.None) return;

            var obj = EditorUtility.EntityIdToObject(entityId) as GameObject;
            if (obj == null) return;

            // Little orange dot left of the name — far enough left to clear the
            // foldout arrow of expandable objects. Ancestors of an ill-setup
            // child get a DIMMED dot, so collapsing them never hides a problem.
            var dotRect = new Rect(selectionRect.x - 24f, selectionRect.y + selectionRect.height * 0.5f - 6f, 12f, 12f);
            if (scope == IssueScope.InChildren)
            {
                var previousColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.45f);
                GUI.Label(dotRect, "●", ValidationUi.OrangeLabel);
                GUI.color = previousColor;
                return; // the object itself is fine: dot only, name untouched
            }
            GUI.Label(dotRect, "●", ValidationUi.OrangeLabel);

            // Repaint the name in orange (over a row-colored patch so the default
            // white label underneath doesn't bleed through).
            var labelRect = selectionRect;
            labelRect.x += 16f; // skip the object icon

            var backgroundColor = EditorGUIUtility.isProSkin ? BackgroundProColor : BackgroundColor;
            if (Selection.Contains(entityId))
                backgroundColor = EditorGUIUtility.isProSkin ? BackgroundSelectedProColor : BackgroundSelectedColor;

            var backgroundRect = labelRect;
            backgroundRect.width = ValidationUi.OrangeLabel.CalcSize(new GUIContent(obj.name)).x;
            EditorGUI.DrawRect(backgroundRect, backgroundColor);
            EditorGUI.LabelField(labelRect, obj.name, ValidationUi.OrangeLabel);
        }

        private static IssueScope ComputeScope(GameObject gameObject)
        {
            if (gameObject == null) return IssueScope.None;
            if (ValidationScanner.HasIssues(gameObject)) return IssueScope.Self;

            foreach (var component in gameObject.GetComponentsInChildren<Component>(true))
            {
                if (component == null || component.gameObject == gameObject) continue;
                if (ValidationScanner.HasIssues(component)) return IssueScope.InChildren;
            }
            return IssueScope.None;
        }
    }
#endif
}
