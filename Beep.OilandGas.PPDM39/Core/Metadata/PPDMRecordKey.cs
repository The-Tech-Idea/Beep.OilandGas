#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Beep.OilandGas.PPDM39.Core.Metadata
{
    /// <summary>
    /// How a PPDM record is named on a data-management address (<c>api/datamanagement/{table}/{key}</c>): the values of its
    /// table's key columns, in the order the metadata lists them, written in the invariant culture and joined by commas.
    /// </summary>
    /// <remarks>
    /// The web's generic table page writes it and the API reads it, so the two cannot disagree (2026-10-10). The page found a
    /// row's key by reflecting over untyped JSON and always found none, and the API's delete passed the key where the record
    /// itself was expected; neither ever named a record.
    /// </remarks>
    public static class PPDMRecordKey
    {
        /// <summary>What separates one key column's value from the next, as it separates the columns in the metadata.</summary>
        public const char Separator = ',';

        /// <summary>The table's key columns, in order; none when its metadata names no key.</summary>
        public static IReadOnlyList<string> Columns(PPDMTableMetadata? metadata) =>
            string.IsNullOrWhiteSpace(metadata?.PrimaryKeyColumn)
                ? Array.Empty<string>()
                : metadata.PrimaryKeyColumn.Split(Separator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        /// <summary>
        /// The record's key, or null when it cannot be addressed by one: its table names no key, it has no value for a key
        /// column, or a value carries the separator.
        /// </summary>
        public static string? Of(object? record, PPDMTableMetadata? metadata)
        {
            var columns = Columns(metadata);
            if (record is null || columns.Count == 0)
                return null;

            var values = new List<string>(columns.Count);
            foreach (var column in columns)
            {
                var property = record.GetType().GetProperty(column, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                var value = property is null || property.GetIndexParameters().Length > 0 ? null : Text(property.GetValue(record));
                if (string.IsNullOrWhiteSpace(value) || value.Contains(Separator))
                    return null;
                values.Add(value);
            }

            return string.Join(Separator, values);
        }

        /// <summary>
        /// The key's value for each key column, in order, or null when it does not give one value for every column.
        /// </summary>
        public static IReadOnlyList<KeyValuePair<string, string>>? Read(string? key, PPDMTableMetadata? metadata)
        {
            var columns = Columns(metadata);
            if (columns.Count == 0 || string.IsNullOrWhiteSpace(key))
                return null;

            var values = key.Split(Separator);
            if (values.Length != columns.Count || values.Any(string.IsNullOrWhiteSpace))
                return null;

            return columns.Select((column, index) => new KeyValuePair<string, string>(column, values[index])).ToList();
        }

        /// <summary>A key value as the key carries it: a number or a date in the invariant culture, anything else as itself.</summary>
        private static string? Text(object? value) => value switch
        {
            null => null,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString()
        };
    }
}
