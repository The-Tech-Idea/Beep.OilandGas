using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>One choice in a reference picker.</summary>
public sealed record OgLookupValue(string Code, string Label);

/// <summary>DAT-05 AC-3: where a form's reference pickers get their values.</summary>
public interface IOgFormLookups
{
    Task<OgResult<IReadOnlyList<OgLookupValue>>> ValuesAsync(
        string table,
        string column,
        CancellationToken cancellationToken = default);
}

/// <summary>DAT-05 AC-5: where a new form's defaults come from.</summary>
public interface IOgFormDefaults
{
    Task<OgResult<IReadOnlyDictionary<string, string>>> ValuesAsync(
        string table,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The reference picker over the LOV service.
/// </summary>
/// <remarks>
/// The reader returns (code, label, active); only active codes are offered, because an inactive reference value
/// is one the database will refuse or that the business has retired (AC-3). Keeping the filtering here means the
/// rule is testable without a database and cannot be forgotten by a caller.
/// </remarks>
public sealed class OgLovFormLookups : IOgFormLookups
{
    /// <summary>How many times the reader was asked - the page reads a lookup once per field.</summary>
    public int Reads { get; private set; }

    private readonly Func<string, CancellationToken, Task<IReadOnlyList<(string Code, string Label, bool Active)>>> _read;

    public OgLovFormLookups(
        Func<string, CancellationToken, Task<IReadOnlyList<(string Code, string Label, bool Active)>>> read)
    {
        _read = read ?? throw new ArgumentNullException(nameof(read));
    }

    public async Task<OgResult<IReadOnlyList<OgLookupValue>>> ValuesAsync(
        string table,
        string column,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(column))
        {
            throw new ArgumentException("A lookup needs a column.", nameof(column));
        }

        Reads++;

        var rows = await _read(column, cancellationToken).ConfigureAwait(false);

        var active = rows
            .Where(row => row.Active && !string.IsNullOrWhiteSpace(row.Code))
            .Select(row => new OgLookupValue(
                row.Code,
                string.IsNullOrWhiteSpace(row.Label) ? row.Code : row.Label))
            .ToList();

        return OgResult<IReadOnlyList<OgLookupValue>>.Ok(active);
    }
}

/// <summary>
/// The form defaults over the defaults service. Keys are the service's own ("TABLE.COLUMN"), so a default meant
/// for another table cannot leak into this form (AC-5).
/// </summary>
public sealed class OgDefaultValueFormDefaults : IOgFormDefaults
{
    private readonly Func<CancellationToken, Task<IReadOnlyDictionary<string, string>>> _read;

    public OgDefaultValueFormDefaults(Func<CancellationToken, Task<IReadOnlyDictionary<string, string>>> read)
    {
        _read = read ?? throw new ArgumentNullException(nameof(read));
    }

    public async Task<OgResult<IReadOnlyDictionary<string, string>>> ValuesAsync(
        string table,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(table))
        {
            throw new ArgumentException("Defaults need a table.", nameof(table));
        }

        var all = await _read(cancellationToken).ConfigureAwait(false);
        var prefix = table + ".";

        var mine = all
            .Where(pair => pair.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToDictionary(
                pair => pair.Key[prefix.Length..],
                pair => pair.Value,
                StringComparer.OrdinalIgnoreCase);

        return OgResult<IReadOnlyDictionary<string, string>>.Ok(mine);
    }
}
