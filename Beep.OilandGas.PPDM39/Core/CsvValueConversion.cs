using System;
using System.Globalization;

namespace Beep.OilandGas.PPDM39.Core
{
    /// <summary>
    /// Converts the text of an imported file's cell to a property's type, and words what the text must be when it is not.
    /// The one owner of that rule for every importer (the generic repository's CSV import, the LOV importer).
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. The importers converted with <c>Convert.ChangeType</c> and caught the refusal — dropping the field
    /// with a debug line, or recording the converter's own words against the row — or turned a value that did not parse
    /// into 0, the minimum date or false and imported it. A value that is not of its column's type now refuses its row, in
    /// words for the person who sent the file.
    /// </remarks>
    public static class CsvValueConversion
    {
        /// <summary>
        /// Converts <paramref name="value"/> to <paramref name="targetType"/>. Answers false — never a stand-in value — when
        /// the text is not of that type. Blank text is the type's empty value (null, or the default of a non-nullable value
        /// type).
        /// </summary>
        public static bool TryConvert(string value, Type targetType, out object? converted)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (string.IsNullOrWhiteSpace(value))
            {
                converted = underlying.IsValueType && Nullable.GetUnderlyingType(targetType) == null
                    ? Activator.CreateInstance(underlying)
                    : null;
                return true;
            }

            if (underlying == typeof(string))
            {
                converted = value;
                return true;
            }
            if (underlying == typeof(int))
            {
                var parsed = int.TryParse(value, out var number);
                converted = number;
                return parsed;
            }
            if (underlying == typeof(long))
            {
                var parsed = long.TryParse(value, out var number);
                converted = number;
                return parsed;
            }
            if (underlying == typeof(decimal))
            {
                var parsed = decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
                converted = number;
                return parsed;
            }
            if (underlying == typeof(double))
            {
                var parsed = double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var number);
                converted = number;
                return parsed;
            }
            if (underlying == typeof(DateTime))
            {
                if (DateTime.TryParse(value, out var dt))
                {
                    converted = dt;
                    return true;
                }
                // An Excel date serial number, within the range an OLE Automation date can hold
                if (double.TryParse(value, out var excelDate) && excelDate > -657435.0 && excelDate < 2958466.0)
                {
                    converted = DateTime.FromOADate(excelDate);
                    return true;
                }
                converted = null;
                return false;
            }
            if (underlying == typeof(bool))
            {
                if (value.Equals("Y", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("1", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("True", StringComparison.OrdinalIgnoreCase))
                {
                    converted = true;
                    return true;
                }
                if (value.Equals("N", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("No", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("0", StringComparison.OrdinalIgnoreCase) ||
                    value.Equals("False", StringComparison.OrdinalIgnoreCase))
                {
                    converted = false;
                    return true;
                }
                converted = null;
                return false;
            }

            // Any other type: ask its converter whether the text is valid before converting, rather than converting and
            // catching the refusal.
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(underlying);
            if (converter.CanConvertFrom(typeof(string)) && converter.IsValid(value))
            {
                converted = converter.ConvertFromInvariantString(value);
                return true;
            }
            converted = null;
            return false;
        }

        /// <summary>What a cell must be for a property of this type, in words for the person who sent the file.</summary>
        public static string Describe(Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (underlying == typeof(int) || underlying == typeof(long) || underlying == typeof(short) || underlying == typeof(byte))
                return "a whole number";
            if (underlying == typeof(decimal) || underlying == typeof(double) || underlying == typeof(float))
                return "a number";
            if (underlying == typeof(DateTime) || underlying == typeof(DateTimeOffset))
                return "a date";
            if (underlying == typeof(bool))
                return "yes or no (Y, Yes, 1, True, N, No, 0, False)";
            if (underlying == typeof(Guid))
                return "an identifier (GUID)";
            if (underlying.IsEnum)
                return $"one of {string.Join(", ", Enum.GetNames(underlying))}";
            return $"a valid {underlying.Name}";
        }

        /// <summary>The sentence refusing a cell: the column, what it holds, and what it must be.</summary>
        public static string Refusal(string column, string value, Type targetType) =>
            $"Column '{column}' holds '{value}', which is not {Describe(targetType)}.";
    }
}
