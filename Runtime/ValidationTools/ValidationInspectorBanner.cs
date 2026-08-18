namespace jeanf.validationTools
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Fallback inspector for every MonoBehaviour: when the component has a
    /// validation issue it draws an ORANGE banner directly under the component's
    /// title bar — "⚠ ComponentName needs setup" plus the issue list — then the
    /// normal inspector (where [Validation] fields are already tinted orange by
    /// ValidationDrawer). Valid components render exactly like the default
    /// inspector. Components with their own [CustomEditor] replace this fallback
    /// (a more specific editor always wins) — they still get the hierarchy dot,
    /// field tint and console log, and can restore the banner by calling
    /// ValidationUi.DrawIssuesBanner themselves.
    /// (Unity has no public API to recolor the title bar itself, so the banner
    /// under it is how the component gets its orange title.)
    /// </summary>
    [CustomEditor(typeof(MonoBehaviour), true), CanEditMultipleObjects]
    public class ValidationInspectorBanner : Editor
    {
        public override void OnInspectorGUI()
        {
            ValidationUi.DrawIssuesBanner(target as Component);
            DrawDefaultInspector();
        }
    }
#endif
}
