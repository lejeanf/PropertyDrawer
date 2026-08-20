namespace jeanf.validationTools
{
    using UnityEngine;

    public class ValidationAttribute : PropertyAttribute
    {
        public string Text = string.Empty;

        /// <summary>
        /// Name of a bool field or property on the same object gating this requirement:
        /// the field is only required while that member is true. Prefix with '!' to
        /// invert. Empty (default) means always required.
        /// </summary>
        public string RequiredIf = string.Empty;

        public ValidationAttribute(string text)
        {
            Text = text;
        }
    }
}

