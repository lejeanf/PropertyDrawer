namespace jeanf.propertyDrawer
{
#if UNITY_EDITOR
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws a [DrawIf] field: shown when its condition holds, greyed (ReadOnly) or
    /// hidden (DontDraw) otherwise. Cooperates with [Validation] on the same field —
    /// see ConditionalField, which does the actual work for both drawers.
    /// </summary>
    [CustomPropertyDrawer(typeof(DrawIfAttribute))]
    public class DrawIfPropertyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
            => ConditionalField.GetHeight(property, label, fieldInfo, attribute as DrawIfAttribute, null);

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            => ConditionalField.Draw(position, property, label, fieldInfo, attribute as DrawIfAttribute, null);
    }
#endif
}
