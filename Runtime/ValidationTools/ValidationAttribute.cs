namespace jeanf.validationTools
{
    using UnityEngine;

    public class ValidationAttribute : PropertyAttribute
    {
        public string Text = string.Empty;

        public ValidationAttribute(string text)
        {
            Text = text;
        }
    }
}

