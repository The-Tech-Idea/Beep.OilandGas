using System;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Centralized value conversion utility used by PPDMEntityForm, GenericCrudPage,
    /// ImportCsvWizard, ImportCsvDialog, and PPDMTableManager.
    ///
    /// Previously each component had its own copy of this logic (M-24).
    /// </summary>
    public static class ValueConverter
    {
        /// <summary>
        /// Converts a source value to the target type, respecting Nullable&lt;T&gt; wrappers.
        /// Returns the original value if conversion fails.
        /// </summary>
        /// <param name="value">The source value (may be string, object, or target type).</param>
        /// <param name="targetType">The desired type (including nullable types).</param>
        /// <returns>The converted value, or the original if conversion is not possible.</returns>
        public static object? ConvertValue(object? value, Type targetType)
        {
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle null/empty/whitespace strings
            if (value == null || (value is string s && string.IsNullOrWhiteSpace(s)))
            {
                if (targetType.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    return Activator.CreateInstance(targetType); // default for non-nullable value types
                return null; // null for reference types and Nullable<T>
            }

            // Already the correct type — no conversion needed
            if (value.GetType() == underlyingType)
                return value;

            // String target — simple ToString
            if (underlyingType == typeof(string))
                return value.ToString();

            // Enum handling
            if (underlyingType.IsEnum)
            {
                var str = value.ToString() ?? string.Empty;
                return Enum.TryParse(underlyingType, str, ignoreCase: true, out var enumValue)
                    ? enumValue
                    : value;
            }

            // Guid handling
            if (underlyingType == typeof(Guid) && value is string guidStr)
            {
                return Guid.TryParse(guidStr, out var guid) ? guid : value;
            }

            // Specific numeric types for culture-invariant parsing (CSV import)
            if (value is string numericStr)
            {
                if (underlyingType == typeof(int))
                    return int.TryParse(numericStr, out var i) ? i : null;
                if (underlyingType == typeof(long))
                    return long.TryParse(numericStr, out var l) ? l : null;
                if (underlyingType == typeof(decimal))
                    return decimal.TryParse(numericStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var d) ? d : null;
                if (underlyingType == typeof(double))
                    return double.TryParse(numericStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var db) ? db : null;
                if (underlyingType == typeof(bool))
                    return bool.TryParse(numericStr, out var b) ? b : null;
                if (underlyingType == typeof(DateTime))
                    return DateTime.TryParse(numericStr, out var dt) ? dt : null;
                if (underlyingType == typeof(DateTimeOffset))
                    return DateTimeOffset.TryParse(numericStr, out var dto) ? dto : null;
            }

            // Fallback: Convert.ChangeType for other conversions
            try
            {
                return Convert.ChangeType(value, underlyingType);
            }
            catch
            {
                return value;
            }
        }

        /// <summary>
        /// Sets a property value on an object after converting the value to the
        /// property's type. Uses <see cref="ConvertValue"/> internally.
        /// </summary>
        /// <param name="obj">The target object (may be a Dictionary&lt;string,object&gt;).</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">The value to convert and assign.</param>
        public static void SetPropertyValue(object obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return;

            // Handle Dictionary<string, object> for dynamic entity bags
            if (obj is System.Collections.Generic.Dictionary<string, object> dict)
            {
                dict[propertyName] = value ?? string.Empty;
                return;
            }

            var prop = obj.GetType().GetProperty(propertyName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            if (prop == null || !prop.CanWrite)
                return;

            try
            {
                var converted = ConvertValue(value, prop.PropertyType);
                prop.SetValue(obj, converted);
            }
            catch
            {
                // Silently fail — the value will remain unchanged
            }
        }
    }
}
