namespace jeanf.validationTools
{
#if UNITY_EDITOR
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
                    _orangeBoldLabel = new GUIStyle(UnityEditor.EditorStyles.boldLabel);
                    _orangeBoldLabel.normal.textColor = Orange;
                    _orangeBoldLabel.hover.textColor = Orange;
                }
                return _orangeBoldLabel;
            }
        }
    }
#endif
}
