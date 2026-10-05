using System.Text.Json;

namespace Beep.OilandGas.PPDM39.DataManagement.SeedData
{
    /// <summary>
    /// Reads values out of the shipped seed templates by asking each JSON element for its kind, for every seeder that
    /// reads one (the reference-data seeder, the standard-value importer).
    /// </summary>
    /// <remarks>
    /// OILGAS-CATCH-01. The seeders read a template's values with <c>GetString()</c>/<c>GetDecimal()</c>, which throw for a
    /// value of another kind, and caught the throw with the whole table or template; and they switched over the value's
    /// kind with a catch-all arm. Every kind is named here, so a kind added to <see cref="JsonValueKind"/> is a build error.
    /// </remarks>
    internal static class SeedTemplateJson
    {
        /// <summary>A template cell as the seeders store it.</summary>
        public static object Value(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.TryGetDecimal(out var number) ? number : value.GetRawText(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null!,
            JsonValueKind.Undefined => null!,
            JsonValueKind.Object => value.GetRawText(),
            JsonValueKind.Array => value.GetRawText(),
        };

        /// <summary>An element's string property, or null when the element is not an object or the property is not a string.</summary>
        public static string? StringProperty(JsonElement item, string name) =>
            item.ValueKind == JsonValueKind.Object && item.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }
}
