using System.Globalization;
using System.Reflection;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.Report;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data;

/// <summary>
/// The one way the desktop reads and writes its tables: a BeepDM <see cref="UnitofWork{T}"/> over the entity
/// model the table already has, with LINQ deciding which rows the screen shows. No statement text is written in
/// this assembly, so a filter, a page and a write all travel through the datasource's own entity API.
/// </summary>
/// <remarks>
/// <para>
/// Every table the desktop uses has a canonical entity class in its owning project (WELL and WELL_XREF in
/// PPDM.Models, the lease and well-program tables in LeaseAcquisition and LifeCycle, the prospect tables in
/// ProspectIdentification). A unit of work binds to that class, so a column rename breaks the build instead of
/// producing an empty grid.
/// </para>
/// <para>
/// A missing table is refused before a unit of work is built: <see cref="UnitofWork{T}.Get(List{AppFilter})"/>
/// would otherwise report an empty collection for a table that does not exist, and the pages must show a failed
/// state rather than "no wells" (WEL-01 AC-3, EXP-01 AC-1).
/// </para>
/// </remarks>
public static class OgUow
{
    /// <summary>Opens a unit of work for <typeparamref name="T"/>'s own table name.</summary>
    public static UnitofWork<T> For<T>(IDataSource source) where T : Entity, new() =>
        For<T>(source, typeof(T).Name);

    /// <summary>Opens a unit of work for a table whose name differs from the entity class.</summary>
    public static UnitofWork<T> For<T>(IDataSource source, string entityName) where T : Entity, new()
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName);

        var editor = source.DMEEditor
            ?? throw new InvalidOperationException(
                $"The connection '{source.DatasourceName}' has no editor, so a unit of work cannot be opened.");

        var structure = source.GetEntityStructure(entityName, refresh: false);

        if (structure?.Fields is null || structure.Fields.Count == 0)
        {
            throw new InvalidOperationException($"Table '{entityName}' is not in this database.");
        }

        AlignDateFieldsWithEntity<T>(structure);

        var unit = new UnitofWork<T>(editor, source.DatasourceName, entityName);

        // Update(), Delete() and DocExistByKey() locate a row by PrimaryKey. The constructor only derives it
        // when a key is supplied, so the table's own declared key is copied here — otherwise a read-only page
        // looks fine and every write reports "Object not found". When the provider records no key (some
        // generated tables), the conventional "<entity>_ID" then "ID" is used.
        if (string.IsNullOrWhiteSpace(unit.PrimaryKey))
        {
            var key = structure.PrimaryKeys?.FirstOrDefault()?.FieldName
                      ?? structure.Fields.FirstOrDefault(field =>
                          string.Equals(field.FieldName, entityName + "_ID", StringComparison.OrdinalIgnoreCase))?.FieldName
                      ?? structure.Fields.FirstOrDefault(field =>
                          string.Equals(field.FieldName, "ID", StringComparison.OrdinalIgnoreCase))?.FieldName;

            if (!string.IsNullOrWhiteSpace(key))
            {
                unit.PrimaryKey = key;
            }
        }

        return unit;
    }

    /// <summary>
    /// A date column declared as TEXT in SQLite is bound with the provider's default <c>ToString()</c> - an
    /// ambiguous <c>MM/dd/yyyy</c> that a reader can parse as a different day depending on culture. The entity
    /// says the property is a <see cref="DateTime"/>, so the structure is told the same: the datasource's SQLite
    /// binder then writes the round-trip ISO form instead.
    /// </summary>
    private static void AlignDateFieldsWithEntity<T>(EntityStructure structure)
    {
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .ToDictionary(property => property.Name, StringComparer.OrdinalIgnoreCase);

        foreach (var field in structure.Fields ?? [])
        {
            if (string.IsNullOrWhiteSpace(field.FieldName)
                || !properties.TryGetValue(field.FieldName, out var property))
            {
                continue;
            }

            var target = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

            if (target == typeof(DateTime)
                && !string.Equals(field.Fieldtype, "System.DateTime", StringComparison.OrdinalIgnoreCase))
            {
                field.Fieldtype = "System.DateTime";
            }
        }
    }

    /// <summary>
    /// Reads the rows the filters select and returns them as a list the caller can query with LINQ. A provider
    /// failure is thrown, never returned as an empty list.
    /// </summary>
    public static async Task<IReadOnlyList<T>> ReadAsync<T>(
        UnitofWork<T> unit,
        List<AppFilter>? filters = null) where T : Entity, new()
    {
        ArgumentNullException.ThrowIfNull(unit);

        await unit.Get(filters ?? []).ConfigureAwait(false);

        if (unit.ReadOutcome.Flag is Errors.Failed or Errors.Critical or Errors.Fatal or Errors.Exception)
        {
            throw new InvalidOperationException(
                unit.ReadOutcome.Message ?? $"'{unit.EntityName}' could not be read.");
        }

        return unit.Units?.ToList() ?? [];
    }

    /// <summary>The same read, honouring a caller's cancellation before and after the provider call.</summary>
    public static async Task<IReadOnlyList<T>> ReadAsync<T>(
        UnitofWork<T> unit,
        List<AppFilter>? filters,
        CancellationToken cancellationToken) where T : Entity, new()
    {
        cancellationToken.ThrowIfCancellationRequested();

        var rows = await ReadAsync(unit, filters).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows;
    }

    /// <summary>Reads every row of a table, for the small reference and lookup tables.</summary>
    public static Task<IReadOnlyList<T>> ReadAllAsync<T>(IDataSource source) where T : Entity, new() =>
        ReadAsync(For<T>(source));

    /// <summary>A single equality filter, the shape the datasource turns into a bound parameter.</summary>
    public static AppFilter Equal(string field, object? value) => new()
    {
        FieldName = field,
        Operator = "=",
        FilterValue = Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
    };

    /// <summary>Ends the unit of work's read buffer. It holds no connection of its own.</summary>
    public static void Release<T>(UnitofWork<T> unit) where T : Entity, new() => unit?.Dispose();

    /// <summary>True when an errors object reports success.</summary>
    public static bool IsOk(IErrorsInfo? result) =>
        result is not null && result.Flag == Errors.Ok;

    /// <summary>The provider's message, for a refused write.</summary>
    public static string Message(IErrorsInfo? result, string fallback) =>
        string.IsNullOrWhiteSpace(result?.Message) ? fallback : result!.Message;
}

