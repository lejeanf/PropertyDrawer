namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Logs every validation issue in the open scenes with a [Validation] tag.
    /// Each entry carries the offending component as its context object —
    /// CLICK the console line and Unity pings/selects the exact GameObject.
    /// Runs automatically when entering play mode; run it any time from
    /// Tools > Validation > Scan Open Scenes.
    /// </summary>
    [InitializeOnLoad]
    public static class ValidationConsoleLogger
    {
        private static readonly List<ValidationIssue> Issues = new List<ValidationIssue>();

        static ValidationConsoleLogger()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode) ScanOpenScenes();
            };
        }

        [MenuItem("Tools/Jeanf/propertyDrawer/Scan Open Scenes")]
        public static void ScanOpenScenes()
        {
            Issues.Clear();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                        ValidationScanner.GetIssues(transform.gameObject, Issues);
            }

            foreach (var issue in Issues)
            {
                var path = GetPath(issue.Component.transform);
                var field = string.IsNullOrEmpty(issue.FieldName) ? string.Empty : $".{issue.FieldName}";
                Debug.LogWarning($"[Validation] {path} — {issue.Component.GetType().Name}{field}: {issue.Message} (click this log to select the GameObject)",
                    issue.Component);
            }

            if (Issues.Count == 0)
                Debug.Log("[Validation] Scan complete — no setup issues in the open scenes.");
        }

        private static string GetPath(Transform transform)
        {
            var path = transform.name;
            for (var parent = transform.parent; parent != null; parent = parent.parent)
                path = parent.name + "/" + path;
            return path;
        }
    }
#endif
}
