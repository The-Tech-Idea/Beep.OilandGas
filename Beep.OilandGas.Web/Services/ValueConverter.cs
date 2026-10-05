using System;
using System.ComponentModel;
using System.Globalization;

namespace Beep.OilandGas.Web.Services
{
    /// <summary>
    /// Centralized value conversion utility used by PPDMEntityForm, GenericCrudPage,
    /// ImportCsvWizard, ImportCsvDialog, and PPDMTableManager.
    ///
    /// Previously each component had its own copy of this logic (M-24).
    /// </summary>
    /// <remarks>
    /// Nothing here converts by attempting and catching (OILGAS-CATCH-01): a value is asked of the target type's own
    /// converter first (<see cref="TypeConverter.IsValid(object)"/>, the framework's question), and a property is set only
    /// with a value it can hold. <see cref="SetPropertyValue"/> says whether it took the value — it dropped one it could not
    /// convert in silence, so a person's entry vanished from the record without a word.
    /// </remarks>
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

            // Anything else is asked of the target type's own converter, in the invariant culture, rather than attempted
            // and caught; a value it cannot take is returned as given.
            return TryConvertWithTypeConverter(value, underlyingType, out var converted) ? converted : value;
        }

        /// <summary>
        /// Converts <paramref name="value"/> for a property of <paramref name="targetType"/>, and says whether it could: false
        /// when a value was given (not null, not blank) and came out as nothing ("abc" for a number), or as something the
        /// property cannot hold. A blank value converts to the type's empty value and succeeds.
        /// </summary>
        public static bool TryConvertValue(object? value, Type targetType, out object? converted)
        {
            ArgumentNullException.ThrowIfNull(targetType);
            converted = ConvertValue(value, targetType);
            var given = value is not null && !(value is string text && string.IsNullOrWhiteSpace(text));
            return (!given || converted is not null) && CanAssign(converted, targetType);
        }

        /// <summary>
        /// Whether <paramref name="value"/> — as converted by <see cref="ConvertValue"/> — can be assigned to a property of
        /// <paramref name="targetType"/>. A null can: reflection sets a non-nullable value type to its default.
        /// </summary>
        public static bool CanAssign(object? value, Type targetType)
        {
            ArgumentNullException.ThrowIfNull(targetType);
            return value is null || targetType.IsInstanceOfType(value)
                || (Nullable.GetUnderlyingType(targetType) is { } underlying && underlying.IsInstanceOfType(value));
        }

        private static bool TryConvertWithTypeConverter(object value, Type targetType, out object? converted)
        {
            var text = value is IFormattable formattable
                ? formattable.ToString(null, CultureInfo.InvariantCulture)
                : value.ToString();
            var converter = TypeDescriptor.GetConverter(targetType);

            if (text is not null && converter.CanConvertFrom(typeof(string)) && converter.IsValid(text))
            {
                converted = converter.ConvertFromInvariantString(text);
                return true;
            }

            converted = null;
            return false;
        }

        /// <summary>
        /// Sets a property value on an object after converting the value to the
        /// property's type. Uses <see cref="ConvertValue"/> internally.
        /// </summary>
        /// <param name="obj">The target object (may be a Dictionary&lt;string,object&gt;).</param>
        /// <param name="propertyName">Name of the property to set.</param>
        /// <param name="value">The value to convert and assign.</param>
        /// <returns>
        /// Whether the value was taken: false when the property's type cannot hold it (the property keeps its value, and the
        /// caller tells the person), and when there is no writable property of that name.
        /// </returns>
        public static bool SetPropertyValue(object obj, string propertyName, object? value)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propertyName))
                return false;

            // Handle Dictionary<string, object> for dynamic entity bags
            if (obj is System.Collections.Generic.Dictionary<string, object> dict)
            {
                dict[propertyName] = value ?? string.Empty;
                return true;
            }

            var prop = obj.GetType().GetProperty(propertyName,
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.IgnoreCase);

            if (prop == null || !prop.CanWrite)
                return false;

            if (!TryConvertValue(value, prop.PropertyType, out var converted))
                return false;

            prop.SetValue(obj, converted);
            return true;
        }
    }
}
