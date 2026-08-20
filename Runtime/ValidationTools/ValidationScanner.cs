using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace jeanf.validationTools
{
    /// <summary>One concrete setup problem on a component: which field (when known) and why.</summary>
    public readonly struct ValidationIssue
    {
        public readonly Component Component;
        /// <summary>Serialized field name the issue points at — empty for component-level issues (IValidatable).</summary>
        public readonly string FieldName;
        public readonly string Message;

        public ValidationIssue(Component component, string fieldName, string message)
        {
            Component = component;
            FieldName = fieldName ?? string.Empty;
            Message = message ?? string.Empty;
        }
    }

    /// <summary>
    /// Collects setup issues from two sources, shared by every validation UI
    /// (field drawer, inspector banner, hierarchy marker, console log, build check):
    ///  1. Fields tagged [Validation("...")] that are unset (null Unity object,
    ///     fake-null destroyed reference, empty string, or empty list of either).
    ///     A RequiredIf gate on the attribute skips the check while its bool
    ///     member (on the same component) is false.
    ///  2. Components implementing IValidatable whose IsValid is false.
    /// Runtime-safe: no UnityEditor — usable for play-mode logs and build checks.
    /// </summary>
    public static class ValidationScanner
    {
        private static readonly Dictionary<Type, FieldInfo[]> ValidatedFieldsByType = new Dictionary<Type, FieldInfo[]>();
        private static readonly List<FieldInfo> FieldBuffer = new List<FieldInfo>();

        /// <summary>Appends every issue found on this component to <paramref name="results"/>.</summary>
        public static void GetIssues(Component component, List<ValidationIssue> results)
        {
            if (component == null || results == null) return;

            foreach (var field in GetValidatedFields(component.GetType()))
            {
                var attribute = (ValidationAttribute)field.GetCustomAttribute(typeof(ValidationAttribute));
                if (!IsRequired(component, attribute)) continue;
                if (IsSet(field.GetValue(component))) continue;
                results.Add(new ValidationIssue(component, field.Name, attribute?.Text ?? $"'{field.Name}' is not assigned."));
            }

            if (component is IValidatable validatable && !validatable.IsValid)
                results.Add(new ValidationIssue(component, string.Empty,
                    $"{component.GetType().Name} reports an invalid setup (IValidatable.IsValid is false)."));
        }

        /// <summary>Appends every issue found on the GameObject's own components (not children).</summary>
        public static void GetIssues(GameObject gameObject, List<ValidationIssue> results)
        {
            if (gameObject == null || results == null) return;
            foreach (var component in gameObject.GetComponents<Component>())
            {
                if (component == null) continue; // missing script — Unity already flags those
                GetIssues(component, results);
            }
        }

        /// <summary>True when this single component has an issue.</summary>
        public static bool HasIssues(Component component)
        {
            if (component == null) return false;

            foreach (var field in GetValidatedFields(component.GetType()))
                if (IsRequired(component, (ValidationAttribute)field.GetCustomAttribute(typeof(ValidationAttribute)))
                    && !IsSet(field.GetValue(component)))
                    return true;

            return component is IValidatable validatable && !validatable.IsValid;
        }

        /// <summary>True when any component ON this GameObject (not children) has an issue.</summary>
        public static bool HasIssues(GameObject gameObject)
        {
            if (gameObject == null) return false;
            foreach (var component in gameObject.GetComponents<Component>())
                if (HasIssues(component))
                    return true;
            return false;
        }

        /// <summary>
        /// A [Validation] field counts as SET unless it is: a null/destroyed Unity
        /// object, a null/empty string, a null/empty IList, or a list whose first
        /// entries include a null Unity object. Other value types always count as
        /// set — use IValidatable for custom rules on those.
        /// </summary>
        public static bool IsSet(object value)
        {
            switch (value)
            {
                case null: return false;
                case UnityEngine.Object unityObject: return unityObject != null; // fake-null (destroyed) counts as unset
                case string text: return !string.IsNullOrEmpty(text);
                case System.Collections.IList list:
                {
                    if (list.Count == 0) return false;
                    foreach (var entry in list)
                        if (entry is UnityEngine.Object o && o == null) return false;
                    return true;
                }
                default: return true;
            }
        }

        /// <summary>
        /// Evaluates the attribute's RequiredIf gate ('!name' inverts). An unresolvable
        /// or non-bool member counts as required — a typo must not silently disable a check.
        /// </summary>
        public static bool IsRequired(object target, ValidationAttribute attribute)
        {
            var condition = attribute?.RequiredIf;
            if (string.IsNullOrEmpty(condition) || target == null) return true;

            var invert = condition[0] == '!';
            if (invert) condition = condition.Substring(1);

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            for (var type = target.GetType(); type != null; type = type.BaseType)
            {
                var field = type.GetField(condition, flags | BindingFlags.DeclaredOnly);
                if (field != null && field.FieldType == typeof(bool))
                    return (bool)field.GetValue(target) != invert;

                var property = type.GetProperty(condition, flags | BindingFlags.DeclaredOnly);
                if (property != null && property.PropertyType == typeof(bool) && property.CanRead)
                    return (bool)property.GetValue(target) != invert;
            }
            return true;
        }

        private static FieldInfo[] GetValidatedFields(Type type)
        {
            if (ValidatedFieldsByType.TryGetValue(type, out var cached)) return cached;

            FieldBuffer.Clear();
            for (var current = type; current != null && current != typeof(MonoBehaviour) && current != typeof(Component); current = current.BaseType)
            {
                foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    if (field.IsDefined(typeof(ValidationAttribute), false))
                        FieldBuffer.Add(field);
            }

            var fields = FieldBuffer.ToArray();
            ValidatedFieldsByType[type] = fields;
            return fields;
        }
    }
}
