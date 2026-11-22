using System.Collections;
using System.Collections.Concurrent;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.Report;
using TheTechIdea.Beep.Utilities;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>One sort key (DAT-04 AC-2: sorting is done by the query, not in the grid).</summary>
public sealed record OgSort(string Column, bool Descending = false);

/// <summary>One filter: a column and a value it must contain. Every filter must match.</summary>
public sealed record OgFilter(string Column, string Value);

/// <summary>
/// What to read. Page size is bounded by the gateway, so a caller cannot ask for 10 000 rows (AC-1).
/// </summary>
public sealed record OgQuery(
    int Page = 0,
    int PageSize = OgPpdmTableGateway.DefaultPageSize,
    IReadOnlyList<OgSort>? Sort = null,
    IReadOnlyList<OgFilter>? Filters = null,
    string? Search = null);

/// <summary>One row: an ordered column/value map, plus the values it was read with (for a later update).</summary>
public sealed record OgRow(
    IReadOnlyDictionary<string, object?> Values,
    IReadOnlyDictionary<string, object?>? Original = null);

/// <summary>DAT-04: the table gateway the workbench uses.</summary>
public interface IOgPpdmTableGateway
{
    Task<OgResult<OgPage<OgRow>>> QueryAsync(string table, OgQuery query, CancellationToken cancellationToken = default);

    Task<OgResult<IReadOnlyList<string>>> KeyColumnsAsync(string table, CancellationToken cancellationToken = default);

    Task<OgResult<byte[]>> ExportAsync(string table, OgQuery query, CancellationToken cancellationToken = default);

    /// <summary>Updates one row, guarded by the values it was read with (AC-3, AC-4).</summary>
    Task<OgResult> UpdateAsync(
        string table,
        OgRow row,
        OgRow original,
        CancellationToken cancellationToken = default);

    /// <summary>Inserts one row.</summary>
    Task<OgResult> InsertAsync(string table, OgRow row, CancellationToken cancellationToken = default);

    /// <summary>Deletes one row, guarded by the values it was read with (AC-4).</summary>
    Task<OgResult> DeleteAsync(string table, OgRow row, CancellationToken cancellationToken = default);

    /// <summary>Builds the generated form for a table (DAT-05).</summary>
    Task<OgResult<OgFormDefinition>> FormAsync(string table, CancellationToken cancellationToken = default);
}

/// <summary>
/// The gateway over the connection's datasource and the PPDM metadata registry.
/// </summary>
/// <remarks>
/// <para>
/// Rows are read through the datasource's entity API - filters are <see cref="AppFilter"/> values and the rows are
/// the provider's own objects, converted to a column/value map. Sorting, searching and paging are LINQ over those
/// rows. No statement text is written here.
/// </para>
/// <para>
/// Identifiers are validated against a plain-name pattern before they are used as a table or column name; a table
/// or column name that is not a plain identifier is refused. A read that fails is reported and returned as a
/// failure, never as an empty page (AC-5).
/// </para>
/// </remarks>
public sealed class OgPpdmTableGateway : IOgPpdmTableGateway
{
    public const int DefaultPageSize = 200;
    public const int MaximumPageSize = 500;
    public const int DefaultCeiling = 20_000;

    /// <summary>Code returned when the table name or a column is not a plain identifier.</summary>
    public const string CodeInvalidIdentifier = "workbench-invalid-identifier";

    /// <summary>Code returned when the datasource cannot be opened.</summary>
    public const string CodeNoConnection = "workbench-no-connection";

    /// <summary>Code returned when an update or delete matched no row - the row changed or vanished (AC-4).</summary>
    public const string CodeConflict = "workbench-conflict";

    /// <summary>Code returned when a write was refused or failed.</summary>
    public const string CodeWriteFailed = "workbench-write-failed";

    private static readonly Regex Identifier = new("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

    private readonly ConcurrentDictionary<string, Type> _types = new(StringComparer.OrdinalIgnoreCase);

    private readonly Func<IDataSource?> _source;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly IFailureReporter _failures;
    private readonly Func<string?>? _policyNote;
    private readonly Func<string, IReadOnlyList<AppFilter>?>? _rowFilter;
    private readonly IOgFormLookups? _lookups;
    private readonly IOgFormDefaults? _defaults;

    /// <param name="rowFilter">
    /// AC-6: an optional typed predicate for the signed-in person, ANDed into every read so policy-restricted rows
    /// are never returned. Supplied by the client app (which owns authorization), not by a page caller.
    /// </param>
    /// <param name="policyNote">One sentence the workbench shows while a predicate is in force.</param>
    public OgPpdmTableGateway(
        Func<IDataSource?> source,
        IPPDMMetadataRepository metadata,
        IFailureReporter failures,
        Func<string, IReadOnlyList<AppFilter>?>? rowFilter = null,
        Func<string?>? policyNote = null,
        IOgFormLookups? lookups = null,
        IOgFormDefaults? defaults = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _rowFilter = rowFilter;
        _policyNote = policyNote;
        _lookups = lookups;
        _defaults = defaults;
    }

    /// <summary>The active row-policy sentence, or null when no predicate applies (AC-6).</summary>
    public string? PolicyNote => _policyNote?.Invoke();

    public async Task<OgResult<OgPage<OgRow>>> QueryAsync(
        string table,
        OgQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (!IsIdentifier(table))
        {
            return OgResult<OgPage<OgRow>>.Failed(
                $"'{table}' is not a table this workbench will query.", CodeInvalidIdentifier);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgPage<OgRow>>.Failed(
                "No connection is open, so the table cannot be read.", CodeNoConnection);
        }

        var page = Math.Max(0, query.Page);
        var size = Math.Clamp(query.PageSize <= 0 ? DefaultPageSize : query.PageSize, 1, MaximumPageSize);

        try
        {
            var filters = await BuildFiltersAsync(table, query, cancellationToken).ConfigureAwait(false);

            if (filters.Failure is { } refusal)
            {
                return OgResult<OgPage<OgRow>>.Failed(refusal.Message, refusal.Code);
            }

            var rows = ReadRows(source, table, filters.Value).ToList();
            EnsureOk(source, table);

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var keys = (await KeyColumnsAsync(table, cancellationToken).ConfigureAwait(false)).Value ?? [];
                var searchable = keys.Where(IsIdentifier).ToList();

                if (searchable.Count == 0)
                {
                    return OgResult<OgPage<OgRow>>.Failed(
                        "Table search needs a recorded key column, and this table has none.", CodeInvalidIdentifier);
                }

                var term = query.Search.Trim();

                rows = rows
                    .Where(row => searchable.Any(column =>
                        Contains(Value(row, column), term)))
                    .ToList();
            }

            var ordered = Sort(rows, query.Sort).ToList();
            var items = ordered
                .Skip(page * size)
                .Take(size)
                .Select(values => new OgRow(values))
                .ToList();

            return OgResult<OgPage<OgRow>>.Ok(
                new OgPage<OgRow>(items, page, size, ordered.Count, DefaultCeiling));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reading table {table} for the workbench",
                "the grid is not shown",
                FailureSeverity.Error);

            return OgResult<OgPage<OgRow>>.Failed(
                $"Table '{table}' could not be read; the grid shows a failed state rather than no rows.",
                reference ?? "workbench-read-failed");
        }
    }

    public async Task<OgResult<IReadOnlyList<string>>> KeyColumnsAsync(
        string table,
        CancellationToken cancellationToken = default)
    {
        if (!IsIdentifier(table))
        {
            return OgResult<IReadOnlyList<string>>.Failed(
                $"'{table}' is not a table this workbench will query.", CodeInvalidIdentifier);
        }

        var keys = (await _metadata.GetPrimaryKeyColumnsAsync(table).ConfigureAwait(false)).ToList();

        return OgResult<IReadOnlyList<string>>.Ok(keys);
    }

    /// <summary>
    /// Exports the rows the active filter selects - not the whole table (AC-7) - as CSV, bounded by the ceiling.
    /// </summary>
    public async Task<OgResult<byte[]>> ExportAsync(
        string table,
        OgQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var exported = await QueryAsync(
            table,
            query with { Page = 0, PageSize = MaximumPageSize },
            cancellationToken).ConfigureAwait(false);

        if (!exported.Success || exported.Value is null)
        {
            return OgResult<byte[]>.Failed(
                exported.Message ?? "The table could not be read.",
                exported.Reference ?? "workbench-export-failed");
        }

        var page = exported.Value;
        var builder = new StringBuilder();
        var columns = page.Items.Count > 0 ? page.Items[0].Values.Keys.ToList() : [];

        builder.AppendLine(string.Join(",", columns.Select(Escape)));

        foreach (var row in page.Items)
        {
            builder.AppendLine(string.Join(",", columns.Select(column =>
                Escape(Convert.ToString(
                    row.Values.TryGetValue(column, out var value) ? value : null,
                    CultureInfo.InvariantCulture)))));
        }

        builder.AppendLine($"# rows: {page.Items.Count} of {page.Total} matching (filter applied)");

        return OgResult<byte[]>.Ok(Encoding.UTF8.GetBytes(builder.ToString()));
    }

    /// <summary>
    /// Updates one row. The current row is re-read and compared with what the grid read, so a row somebody else
    /// changed is reported as a conflict (AC-3, AC-4); then every column is written from <paramref name="row"/>.
    /// </summary>
    public async Task<OgResult> UpdateAsync(
        string table,
        OgRow row,
        OgRow original,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(original);

        if (!IsIdentifier(table))
        {
            return OgResult.Failed($"'{table}' is not a table this workbench will write.", CodeInvalidIdentifier);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed("No connection is open, so the row cannot be written.", CodeNoConnection);
        }

        try
        {
            var keys = await KeyColumnsAsync(table, cancellationToken).ConfigureAwait(false) is { Success: true, Value: not null } keyResult
                ? keyResult.Value!
                : [];

            var keyColumns = keys.Where(IsIdentifier).ToList();

            if (keyColumns.Count == 0)
            {
                return OgResult.Failed(
                    "This table has no recorded key columns, so the workbench cannot update a row safely.",
                    CodeWriteFailed);
            }

            var current = ReadRow(source, table, keyColumns, original.Values);

            if (current is null)
            {
                return OgResult.Failed(
                    "The row changed or was removed by someone else; reload and try again.", CodeConflict);
            }

            if (!SameAsRead(current, original.Values))
            {
                return OgResult.Failed(
                    "The row changed or was removed by someone else; reload and try again.", CodeConflict);
            }

            var type = ResolveType(source, table);
            var entity = Create(type, row.Values);
            var result = source.UpdateEntity(table, entity);

            return OgUow.IsOk(result)
                ? OgResult.Ok("Row updated.")
                : OgResult.Failed($"The row could not be updated: {result?.Message}", CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"updating a row in {table}",
                "the write was not applied",
                FailureSeverity.Error);

            return OgResult.Failed("The row could not be updated.", reference ?? CodeWriteFailed);
        }
    }

    /// <summary>Inserts one row; key columns must be present and plain identifiers.</summary>
    public Task<OgResult> InsertAsync(string table, OgRow row, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (!IsIdentifier(table))
        {
            return Task.FromResult(OgResult.Failed(
                $"'{table}' is not a table this workbench will write.", CodeInvalidIdentifier));
        }

        var source = _source();

        if (source is null)
        {
            return Task.FromResult(OgResult.Failed(
                "No connection is open, so the row cannot be written.", CodeNoConnection));
        }

        try
        {
            var entity = Create(ResolveType(source, table), row.Values);
            var result = source.InsertEntity(table, entity);

            return Task.FromResult(OgUow.IsOk(result)
                ? OgResult.Ok("Row inserted.")
                : OgResult.Failed($"The row could not be inserted: {result?.Message}", CodeWriteFailed));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"inserting a row in {table}",
                "the write was not applied",
                FailureSeverity.Error);

            return Task.FromResult(OgResult.Failed("The row could not be inserted.", reference ?? CodeWriteFailed));
        }
    }

    /// <summary>Deletes one row, guarded by its original values so a changed row is reported, not removed (AC-4).</summary>
    public async Task<OgResult> DeleteAsync(string table, OgRow row, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(row);

        if (!IsIdentifier(table))
        {
            return OgResult.Failed($"'{table}' is not a table this workbench will write.", CodeInvalidIdentifier);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult.Failed("No connection is open, so the row cannot be removed.", CodeNoConnection);
        }

        try
        {
            var keys = await KeyColumnsAsync(table, cancellationToken).ConfigureAwait(false) is { Success: true, Value: not null } keyResult
                ? keyResult.Value!
                : [];

            var keyColumns = keys.Where(IsIdentifier).ToList();

            if (keyColumns.Count == 0)
            {
                return OgResult.Failed(
                    "This table has no recorded key columns, so the workbench cannot delete a row safely.",
                    CodeWriteFailed);
            }

            var current = ReadRow(source, table, keyColumns, row.Values);

            if (current is null || !SameAsRead(current, row.Values))
            {
                return OgResult.Failed(
                    "The row changed or was removed by someone else; reload and try again.", CodeConflict);
            }

            var entity = Create(ResolveType(source, table), row.Values);
            var result = source.DeleteEntity(table, entity);

            return OgUow.IsOk(result)
                ? OgResult.Ok("Row deleted.")
                : OgResult.Failed($"The row could not be removed: {result?.Message}", CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"removing a row in {table}",
                "the write was not applied",
                FailureSeverity.Error);

            return OgResult.Failed("The row could not be removed.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFormDefinition>> FormAsync(
        string table,
        CancellationToken cancellationToken = default)
    {
        if (!IsIdentifier(table))
        {
            return OgResult<OgFormDefinition>.Failed(
                $"'{table}' is not a table this workbench will open.", CodeInvalidIdentifier);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFormDefinition>.Failed(
                "No connection is open, so the form cannot be built.", CodeNoConnection);
        }

        var structure = source.GetEntityStructure(table, refresh: true);

        if (structure?.Fields is null || structure.Fields.Count == 0)
        {
            return OgResult<OgFormDefinition>.Failed(
                $"Table '{table}' is not in this connection, so no form can be generated.",
                "workbench-table-unknown");
        }

        var keys = (await KeyColumnsAsync(table, cancellationToken).ConfigureAwait(false)).Value ?? [];

        var fields = structure.Fields
            .OrderBy(item => item.OrdinalPosition)
            .Select(item => new OgFormSourceField(
                item.FieldName,
                string.IsNullOrWhiteSpace(item.Fieldtype) ? null : Type.GetType(item.Fieldtype, throwOnError: false),
                item.AllowDBNull,
                item.Size1 > 0 ? item.Size1 : null,
                item.NumericScale > 0 ? (int)item.NumericScale : null))
            .ToList();

        // AC-3 and AC-5: reference pickers and defaults come from the owning services, not from the form.
        var lookups = new Dictionary<string, IReadOnlyList<OgLookupValue>>(StringComparer.OrdinalIgnoreCase);

        if (_lookups is not null)
        {
            foreach (var field in fields)
            {
                var values = await _lookups.ValuesAsync(table, field.Name, cancellationToken).ConfigureAwait(false);

                if (values.Success && values.Value is { Count: > 0 } choices)
                {
                    lookups[field.Name] = choices;
                }
            }
        }

        var defaults = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (_defaults is not null)
        {
            var values = await _defaults.ValuesAsync(table, cancellationToken).ConfigureAwait(false);

            if (values.Success && values.Value is not null)
            {
                foreach (var pair in values.Value)
                {
                    defaults[pair.Key] = pair.Value;
                }
            }
        }

        return OgResult<OgFormDefinition>.Ok(OgEntityForm.Build(table, fields, keys, lookups, defaults));
    }

    // ------------------------------------------------------------------ reads

    private async Task<ReadFilters> BuildFiltersAsync(
        string table,
        OgQuery query,
        CancellationToken cancellationToken)
    {
        var filters = new List<AppFilter>();

        foreach (var filter in query.Filters ?? [])
        {
            if (!IsIdentifier(filter.Column))
            {
                return ReadFilters.Failed(
                    $"'{filter.Column}' is not a column this workbench will filter on.", CodeInvalidIdentifier);
            }

            filters.Add(new AppFilter
            {
                FieldName = filter.Column,
                Operator = "contains",
                FilterValue = filter.Value ?? string.Empty,
            });
        }

        // AC-6: the client app's row policy is ANDed into the predicate so restricted rows are never read. It is a
        // typed filter list, not a statement fragment, so it travels through the same entity API.
        if (_rowFilter?.Invoke(table) is { Count: > 0 } policy)
        {
            filters.AddRange(policy);
        }

        await Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();

        return ReadFilters.Ok(filters);
    }

    private static IEnumerable<Dictionary<string, object?>> ReadRows(
        IDataSource source,
        string table,
        IReadOnlyList<AppFilter> filters) =>
        source.GetEntity(table, filters.ToList()).Select(ToValues);

    private Dictionary<string, object?>? ReadRow(
        IDataSource source,
        string table,
        IReadOnlyList<string> keyColumns,
        IReadOnlyDictionary<string, object?> values)
    {
        var filters = keyColumns
            .Select(column => new AppFilter
            {
                FieldName = column,
                Operator = "=",
                FilterValue = Convert.ToString(values.GetValueOrDefault(column), CultureInfo.InvariantCulture) ?? string.Empty,
            })
            .ToList();

        if (_rowFilter?.Invoke(table) is { Count: > 0 } policy)
        {
            filters.AddRange(policy);
        }

        return source.GetEntity(table, filters)
            .Select(ToValues)
            .FirstOrDefault(row => keyColumns.All(column =>
                string.Equals(
                    Convert.ToString(row.GetValueOrDefault(column), CultureInfo.InvariantCulture),
                    Convert.ToString(values.GetValueOrDefault(column), CultureInfo.InvariantCulture),
                    StringComparison.Ordinal)));
    }

    private static Dictionary<string, object?> ToValues(object row)
    {
        var values = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (row is IDictionary<string, object?> typed)
        {
            foreach (var pair in typed)
            {
                values[pair.Key] = pair.Value is DBNull ? null : pair.Value;
            }

            return values;
        }

        if (row is IDictionary loose)
        {
            foreach (DictionaryEntry entry in loose)
            {
                values[Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty] =
                    entry.Value is DBNull ? null : entry.Value;
            }

            return values;
        }

        foreach (var property in row.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.CanRead && property.GetIndexParameters().Length == 0)
            {
                var value = property.GetValue(row);
                values[property.Name] = value is DBNull ? null : value;
            }
        }

        return values;
    }

    private static IEnumerable<Dictionary<string, object?>> Sort(
        IEnumerable<Dictionary<string, object?>> rows,
        IReadOnlyList<OgSort>? sorts)
    {
        var applied = (sorts ?? []).Where(sort => IsIdentifier(sort.Column)).ToList();

        if (applied.Count == 0)
        {
            return rows;
        }

        IOrderedEnumerable<Dictionary<string, object?>>? ordered = null;

        foreach (var sort in applied)
        {
            object? Key(Dictionary<string, object?> row) =>
                row.TryGetValue(sort.Column, out var value) ? value : null;

            ordered = ordered is null
                ? sort.Descending
                    ? rows.OrderByDescending(Key, ValueComparer.Instance)
                    : rows.OrderBy(Key, ValueComparer.Instance)
                : sort.Descending
                    ? ordered.ThenByDescending(Key, ValueComparer.Instance)
                    : ordered.ThenBy(Key, ValueComparer.Instance);
        }

        return ordered ?? rows;
    }

    private Type ResolveType(IDataSource source, string table)
    {
        var structure = source.GetEntityStructure(table, refresh: true)
            ?? throw new InvalidOperationException($"Table '{table}' is not in this connection.");

        return _types.GetOrAdd($"{source.DatasourceName}:{table}", _ =>
        {
            var known = source.GetEntityType(table);

            if (known is not null)
            {
                return known;
            }

            var editor = source.DMEEditor
                ?? throw new InvalidOperationException(
                    $"The connection '{source.DatasourceName}' has no editor, so a row type cannot be generated.");

            return DMTypeBuilder.GetOrCreateType(
                editor,
                "TheTechIdea.Beep.OilandGas.Desktop.Generated",
                source.DatasourceName,
                table,
                structure.Fields);
        });
    }

    private static object Create(Type type, IReadOnlyDictionary<string, object?> values)
    {
        var instance = Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"A row of '{type.Name}' could not be created.");

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanWrite && property.GetIndexParameters().Length == 0)
            .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var pair in values)
        {
            if (!properties.TryGetValue(pair.Key, out var property))
            {
                continue;
            }

            property.SetValue(instance, ConvertTo(property.PropertyType, pair.Value));
        }

        return instance;
    }

    private static object? ConvertTo(Type target, object? value)
    {
        if (value is null or DBNull)
        {
            return target.IsValueType && Nullable.GetUnderlyingType(target) is null
                ? Activator.CreateInstance(target)
                : null;
        }

        var underlying = Nullable.GetUnderlyingType(target) ?? target;

        if (underlying.IsInstanceOfType(value))
        {
            return value;
        }

        if (underlying == typeof(string))
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        if (underlying == typeof(DateTime))
        {
            return value switch
            {
                DateTime moment => moment,
                string text when DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) => parsed,
                _ => Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture),
            };
        }

        return Convert.ChangeType(value, underlying, CultureInfo.InvariantCulture);
    }

    private static bool SameAsRead(
        IReadOnlyDictionary<string, object?> current,
        IReadOnlyDictionary<string, object?> original) =>
        original.All(pair => string.Equals(
            Convert.ToString(current.GetValueOrDefault(pair.Key), CultureInfo.InvariantCulture),
            Convert.ToString(pair.Value, CultureInfo.InvariantCulture),
            StringComparison.Ordinal));

    private static object? Value(Dictionary<string, object?> row, string column) =>
        row.TryGetValue(column, out var value) ? value : null;

    private static bool Contains(object? value, string term) =>
        Convert.ToString(value, CultureInfo.InvariantCulture)?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;

    private static void EnsureOk(IDataSource source, string table)
    {
        if (source.ErrorObject is { } error
            && !string.Equals(error.Flag.ToString(), "Ok", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Table '{table}' could not be read: {error.Message}");
        }
    }

    private static bool IsIdentifier(string? value) =>
        !string.IsNullOrWhiteSpace(value) && Identifier.IsMatch(value);

    private static string Escape(string? value) => (value ?? string.Empty).Replace("'", "''");

    /// <summary>Compares column values without assuming they all share one CLR type.</summary>
    private sealed class ValueComparer : IComparer<object?>
    {
        public static ValueComparer Instance { get; } = new();

        public int Compare(object? x, object? y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            if (x is null)
            {
                return -1;
            }

            if (y is null)
            {
                return 1;
            }

            if (x is IComparable comparable && x.GetType() == y.GetType())
            {
                return comparable.CompareTo(y);
            }

            if (decimal.TryParse(Convert.ToString(x, CultureInfo.InvariantCulture), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var left)
                && decimal.TryParse(Convert.ToString(y, CultureInfo.InvariantCulture), NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var right))
            {
                return left.CompareTo(right);
            }

            return string.Compare(
                Convert.ToString(x, CultureInfo.InvariantCulture),
                Convert.ToString(y, CultureInfo.InvariantCulture),
                StringComparison.OrdinalIgnoreCase);
        }
    }

    private sealed record ReadFilters(IReadOnlyList<AppFilter>? Value, (string Message, string Code)? Failure)
    {
        public static ReadFilters Ok(IReadOnlyList<AppFilter> filters) => new(filters, null);

        public static ReadFilters Failed(string message, string code) => new(null, (message, code));
    }
}
