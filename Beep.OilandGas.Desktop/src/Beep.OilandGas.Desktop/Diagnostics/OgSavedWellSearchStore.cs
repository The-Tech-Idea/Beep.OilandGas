using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>One saved well search (WEL-01 AC-5).</summary>
public sealed record OgSavedWellSearch(
    string Name,
    string? Text = null,
    string? Field = null,
    string? Status = null,
    string? WellType = null,
    int? SpudYearFrom = null,
    int? SpudYearTo = null);

/// <summary>A named filter set, and whether the store could read or write the file.</summary>
public sealed record OgSavedWellSearchFile(IReadOnlyList<OgSavedWellSearch> Searches, string? Problem)
{
    public bool Readable => Problem is null;
}

/// <summary>
/// WEL-01 AC-5: saved searches in the per-user data root, so they survive a restart.
/// </summary>
/// <remarks>
/// The file is a plain JSON array. A corrupt or unreadable file is reported as a problem and offered as an empty
/// list, so the page says saved searches are unavailable instead of failing the search itself.
/// </remarks>
public sealed class OgSavedWellSearchStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly IFailureReporter _failures;
    private readonly object _gate = new();

    public OgSavedWellSearchStore(IFailureReporter failures, string? path = null)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _filePath = path ?? Path.Combine(OgPaths.DataRoot, "Searches", "well-searches.json");

        if (!OgPaths.IsWritableLocation(_filePath))
        {
            throw new InvalidOperationException(
                $"Saved well searches must live under {OgPaths.DataRoot}, not '{_filePath}'.");
        }
    }

    public string FilePath => _filePath;

    public OgSavedWellSearchFile Load()
    {
        lock (_gate)
        {
            if (!File.Exists(_filePath))
            {
                return new OgSavedWellSearchFile([], null);
            }

            try
            {
                var searches = JsonSerializer.Deserialize<List<OgSavedWellSearch>>(File.ReadAllText(_filePath), Json)
                               ?? [];
                return new OgSavedWellSearchFile(
                    searches.Where(search => !string.IsNullOrWhiteSpace(search.Name)).ToList(),
                    null);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
            {
                _failures.ReportHandled(
                    ex,
                    "reading saved well searches",
                    "saved searches are not offered; the file is kept",
                    FailureSeverity.Degraded);

                return new OgSavedWellSearchFile(
                    [],
                    "Saved searches could not be read; the file is kept and will be replaced when you save one.");
            }
        }
    }

    /// <summary>Adds or replaces a search by name (case-insensitive), then writes the file.</summary>
    public OgSavedWellSearchFile Upsert(OgSavedWellSearch search)
    {
        ArgumentNullException.ThrowIfNull(search);

        if (string.IsNullOrWhiteSpace(search.Name))
        {
            throw new ArgumentException("A saved search needs a name.", nameof(search));
        }

        lock (_gate)
        {
            var current = Load();
            var searches = current.Searches
                .Where(existing => !string.Equals(existing.Name, search.Name, StringComparison.OrdinalIgnoreCase))
                .Append(search with { Name = search.Name.Trim() })
                .OrderBy(existing => existing.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return Write(searches, current, "The saved search could not be written; it is not stored.");
        }
    }

    /// <summary>Removes a search by name.</summary>
    public OgSavedWellSearchFile Remove(string name)
    {
        lock (_gate)
        {
            var current = Load();
            var searches = current.Searches
                .Where(existing => !string.Equals(existing.Name, name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Write(searches, current, "The saved search could not be removed.");
        }
    }

    private OgSavedWellSearchFile Write(
        IReadOnlyList<OgSavedWellSearch> searches,
        OgSavedWellSearchFile current,
        string problem)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(searches, Json));
            return new OgSavedWellSearchFile(searches, null);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _failures.ReportHandled(
                ex,
                "writing saved well searches",
                "the saved search was not stored",
                FailureSeverity.Degraded);

            return new OgSavedWellSearchFile(current.Searches, problem);
        }
    }
}
