using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Diagnostics;

/// <summary>The coordinate system a location edit was made in (WEL-02 AC-3).</summary>
public sealed record OgWellLocationRecord(string Uwi, string CoordinateSystem, string Actor, DateTimeOffset AtUtc);

/// <summary>An explicitly chosen identifier for a well (WEL-02 AC-2).</summary>
public sealed record OgWellIdentifierChoice(string Uwi, string Type, string XrefId, string Actor, DateTimeOffset AtUtc);

/// <summary>
/// The header's local notes: which coordinate system the last location edit used, and which identifier a person
/// chose when two of the same type disagreed.
/// </summary>
/// <remarks>
/// PPDM's WELL table has no coordinate-system column, and an identifier conflict is a decision rather than a value:
/// both are recorded here, per well, in the per-user data root, with who decided and when.
/// </remarks>
public sealed class OgWellHeaderNotesStore
{
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

    private readonly string _filePath;
    private readonly IFailureReporter _failures;
    private readonly object _gate = new();

    public OgWellHeaderNotesStore(IFailureReporter failures, string? path = null)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _filePath = path ?? Path.Combine(OgPaths.DataRoot, "Wells", "well-header-notes.json");

        if (!OgPaths.IsWritableLocation(_filePath))
        {
            throw new InvalidOperationException(
                $"Well header notes must live under {OgPaths.DataRoot}, not '{_filePath}'.");
        }
    }

    public string FilePath => _filePath;

    public OgWellLocationRecord? LocationSystem(string uwi) =>
        Read().Locations.FirstOrDefault(record =>
            string.Equals(record.Uwi, uwi, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<OgWellIdentifierChoice> Choices(string uwi) =>
        Read().Choices
            .Where(choice => string.Equals(choice.Uwi, uwi, StringComparison.OrdinalIgnoreCase))
            .ToList();

    /// <summary>Records the coordinate system of the last location edit for this well.</summary>
    public OgWellLocationRecord RecordLocationSystem(string uwi, string coordinateSystem, string actor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uwi);
        ArgumentException.ThrowIfNullOrWhiteSpace(coordinateSystem);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);

        lock (_gate)
        {
            var notes = Read();
            var record = new OgWellLocationRecord(
                uwi.Trim(),
                coordinateSystem.Trim(),
                actor.Trim(),
                DateTimeOffset.UtcNow);

            Write(notes with
            {
                Locations = notes.Locations
                    .Where(existing => !string.Equals(existing.Uwi, uwi, StringComparison.OrdinalIgnoreCase))
                    .Append(record)
                    .ToList(),
            });

            return record;
        }
    }

    /// <summary>Records the identifier a person chose for a conflicting type.</summary>
    public OgWellIdentifierChoice Choose(string uwi, string type, string xrefId, string actor)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(uwi);
        ArgumentException.ThrowIfNullOrWhiteSpace(type);
        ArgumentException.ThrowIfNullOrWhiteSpace(xrefId);
        ArgumentException.ThrowIfNullOrWhiteSpace(actor);

        lock (_gate)
        {
            var notes = Read();
            var choice = new OgWellIdentifierChoice(
                uwi.Trim(),
                type.Trim(),
                xrefId.Trim(),
                actor.Trim(),
                DateTimeOffset.UtcNow);

            Write(notes with
            {
                Choices = notes.Choices
                    .Where(existing =>
                        !string.Equals(existing.Uwi, uwi, StringComparison.OrdinalIgnoreCase)
                        || !string.Equals(existing.Type, type, StringComparison.OrdinalIgnoreCase))
                    .Append(choice)
                    .ToList(),
            });

            return choice;
        }
    }

    private OgWellHeaderNotes Read()
    {
        if (!File.Exists(_filePath))
        {
            return new OgWellHeaderNotes([], []);
        }

        try
        {
            return JsonSerializer.Deserialize<OgWellHeaderNotes>(File.ReadAllText(_filePath), Json)
                   ?? new OgWellHeaderNotes([], []);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _failures.ReportHandled(
                ex,
                "reading the well header notes",
                "the last coordinate system and identifier choices are not available",
                FailureSeverity.Degraded);

            return new OgWellHeaderNotes([], []);
        }
    }

    private void Write(OgWellHeaderNotes notes)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
            File.WriteAllText(_filePath, JsonSerializer.Serialize(notes, Json));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _failures.ReportHandled(
                ex,
                "writing the well header notes",
                "the coordinate system or identifier choice was not stored",
                FailureSeverity.Degraded);
        }
    }
}

/// <summary>The file's shape.</summary>
public sealed record OgWellHeaderNotes(
    IReadOnlyList<OgWellLocationRecord> Locations,
    IReadOnlyList<OgWellIdentifierChoice> Choices);
