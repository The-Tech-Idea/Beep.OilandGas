using System.Data;
using System.Globalization;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

// Shared bounded evidence reads, not another repository or application authorization authority.
internal sealed class CompanyPPDMEvidenceReader(CompanyModuleConnection company, IDMEEditor editor,
    ICommonColumnHandler columns, IPPDM39DefaultsRepository defaults, IPPDMMetadataRepository metadata,
    AssetDatabaseScope database, CompanyScope scope, string actor)
{
    public async Task OpenAsync()
    {
        await RequireCurrentAsync();
        if (editor.OpenDataSource(database.ConnectionName) != ConnectionState.Open)
            throw new InvalidOperationException("The reviewed PPDM evidence datasource could not be opened.");
        await RequireCurrentAsync();
    }
    public async Task RequireCurrentAsync()
    {
        if (await company.ResolveAsync() != database || company.Company.Scope != scope || company.Company.UserId != actor)
            throw RefusalException.Conflict("Company authority or database scope changed while reading PPDM evidence.");
        var source = editor.GetDataSource(database.ConnectionName) ?? throw new InvalidOperationException("The reviewed PPDM evidence datasource is unavailable.");
        Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup.MigrationConnectionTarget.Validate(editor, source, database.ConnectionName);
    }
    public async Task<object[]> ReadAsync(Type type, IReadOnlyList<AppFilter> filters, IReadOnlyList<string>? requiredColumns = null,
        IReadOnlyList<string>? projectedColumns = null)
    {
        if (filters.Count == 0) throw RefusalException.Invalid("PPDM evidence reads require explicit predicates.");
        await RequireCurrentAsync();
        if (projectedColumns is not null)
        {
            var source = editor.GetDataSource(database.ConnectionName) ?? throw new InvalidOperationException("The reviewed PPDM evidence datasource is unavailable.");
            var schema = source.GetEntityStructure(new EntityStructure { EntityName = type.Name, DatasourceEntityName = type.Name }, true);
            if (schema?.Fields is null || projectedColumns.Any(column => schema.Fields.Count(field =>
                    string.Equals(field.FieldName, column, StringComparison.OrdinalIgnoreCase)) != 1))
                throw new InvalidOperationException("The PPDM evidence source is missing or has ambiguous projected columns.");
            await RequireCurrentAsync();
        }
        var repository = projectedColumns is null
            ? new PPDMGenericRepository(editor, columns, defaults, metadata, type, database.ConnectionName, type.Name, requiredReadColumns: requiredColumns)
            : new PPDMGenericRepository(editor, columns, defaults, metadata, type, database.ConnectionName, type.Name,
                logger: null, rowAccess: null, requiredReadColumns: requiredColumns, requiredProjectionColumns: projectedColumns);
        var rows = (await repository.GetAsync(filters.ToList())).ToArray();
        await RequireCurrentAsync();
        if (editor.GetDataSource(database.ConnectionName)!.ErrorObject?.Flag == Errors.Failed)
            throw new InvalidOperationException("The provider failed while reading PPDM evidence.");
        if (rows.Length > 256) throw RefusalException.Conflict("PPDM evidence exceeds the supported reviewed lookup limit.");
        if (rows.Any(row => !type.IsInstanceOfType(row) || filters.Any(filter => filter.Operator != "=" ||
            !Exact(type.GetProperty(filter.FieldName)!.GetValue(row), filter.FilterValue))))
            throw RefusalException.Conflict("The provider did not honor exact PPDM evidence predicates.");
        return rows;
    }
    public static async Task DefinitionAsync(IPPDMMetadataRepository metadata, Type type, string[] keys,
        params (string Column, string Table, string Key)[] relationships)
    {
        var definition = await metadata.GetTableMetadataAsync(type.Name);
        if (definition?.EntityTypeName != type.Name || !keys.Order(StringComparer.Ordinal).SequenceEqual((definition.PrimaryKeyColumn ?? "").Split(',').Select(key => key.Trim()).Order(StringComparer.Ordinal)) ||
            type.GetProperty("ACTIVE_IND")?.PropertyType != typeof(string) || type.GetProperty("EFFECTIVE_DATE")?.PropertyType != typeof(DateTime?) || type.GetProperty("EXPIRY_DATE")?.PropertyType != typeof(DateTime?) ||
            relationships.Any(expected => definition.ForeignKeys.Count(key => key.ForeignKeyColumn == expected.Column && key.ReferencedTable == expected.Table && key.ReferencedPrimaryKey == expected.Key) != 1))
            throw RefusalException.Conflict("PPDM evidence metadata does not match its canonical relationship keys.");
    }
    // Capture at materialization, before another provider await can mutate returned objects.
    internal static string CanonicalRecord(object row) => System.Text.Json.JsonSerializer.Serialize(new {
        Table = row.GetType().Name,
        Columns = row.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(property => property.CanRead && property.CanWrite).OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => new object?[] { property.Name, property.GetValue(row) }).ToArray()
    });
    internal static string Fingerprint(IEnumerable<object> rows) => FingerprintRecords(rows.Select(CanonicalRecord));
    internal static string FingerprintRecords(IEnumerable<string> records) => Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(
        System.Text.Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(records.Order(StringComparer.Ordinal).ToArray()))));
    public static bool Window(object row, TimeZoneInfo zone, TimeProvider time, bool required)
    {
        var type = row.GetType(); var start = (DateTime?)type.GetProperty("EFFECTIVE_DATE")!.GetValue(row); var end = (DateTime?)type.GetProperty("EXPIRY_DATE")!.GetValue(row);
        return Range(start, end, zone, time, required);
    }
    public static bool Range(DateTime? start, DateTime? end, TimeZoneInfo zone, TimeProvider time, bool required)
    {
        if (required && start is null) return false;
        DateTimeOffset Instant(DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc) return new(value);
            if (value.Kind == DateTimeKind.Local || zone.IsInvalidTime(value) || zone.IsAmbiguousTime(value))
                throw RefusalException.Conflict("A PPDM evidence boundary has an invalid or ambiguous date-zone interpretation.");
            return new(TimeZoneInfo.ConvertTimeToUtc(value, zone));
        }
        var from = start is null ? (DateTimeOffset?)null : Instant(start.Value); var until = end is null ? (DateTimeOffset?)null : Instant(end.Value);
        if (from is not null && until is not null && until <= from) throw RefusalException.Conflict("A PPDM evidence date range is inconsistent.");
        var now = time.GetUtcNow(); return (from is null || from <= now) && (until is null || until > now);
    }
    public static bool Identifier(string? value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 128 && value == value.Trim() && !value.Any(char.IsControl);
    internal static bool Exact(object? value, string expected) => value is decimal number
        ? decimal.TryParse(expected, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) && number == parsed
        : Convert.ToString(value, CultureInfo.InvariantCulture) == expected;
    public static bool Observation(decimal value) => value >= 0 && decimal.Truncate(value) == value;
    public static AppFilter Equal(string column, string value) => new() { FieldName = column, Operator = "=", FilterValue = value };
    public static AppFilter Equal(string column, decimal value) => Equal(column, value.ToString(CultureInfo.InvariantCulture));
}
