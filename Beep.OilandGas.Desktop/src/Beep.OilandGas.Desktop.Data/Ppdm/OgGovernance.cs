using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Ppdm;

/// <summary>One stored version of a row. Versions are append-only: nothing rewrites one.</summary>
public sealed record OgRowVersion(
    string Id,
    string Table,
    string Key,
    int Version,
    string Actor,
    DateTimeOffset AtUtc,
    IReadOnlyDictionary<string, object?> Values,
    string? RestoredFrom = null);

/// <summary>DAT-08: row history, so a page or a store can be swapped without the callers changing.</summary>
public interface IOgVersionStore
{
    OgRowVersion Record(
        string table,
        string key,
        IReadOnlyDictionary<string, object?> values,
        string actor,
        DateTimeOffset? at = null);

    IReadOnlyList<OgRowVersion> History(string table, string key);

    OgResult<OgRowVersion> Restore(
        string table,
        string key,
        string versionId,
        string actor,
        DateTimeOffset? at = null);
}

/// <summary>
/// DAT-08: the row history. A restore appends a new version whose values equal the old one and records which
/// version it came from, so history is never modified (AC-1).
/// </summary>
public sealed class OgVersionStore : IOgVersionStore
{
    private readonly Dictionary<string, List<OgRowVersion>> _history = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Appends a version; returns it with its number.</summary>
    public OgRowVersion Record(
        string table,
        string key,
        IReadOnlyDictionary<string, object?> values,
        string actor,
        DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(table);
        ArgumentNullException.ThrowIfNull(values);

        var versions = For(table, key);
        var version = new OgRowVersion(
            Guid.NewGuid().ToString("n")[..12],
            table,
            key,
            versions.Count + 1,
            string.IsNullOrWhiteSpace(actor) ? "(unknown)" : actor.Trim(),
            at ?? DateTimeOffset.UtcNow,
            new Dictionary<string, object?>(values, StringComparer.OrdinalIgnoreCase));

        versions.Add(version);
        return version;
    }

    public IReadOnlyList<OgRowVersion> History(string table, string key) =>
        For(table, key).ToList();

    /// <summary>Restores an old version as a new one; the old version is untouched (AC-1).</summary>
    public OgResult<OgRowVersion> Restore(
        string table,
        string key,
        string versionId,
        string actor,
        DateTimeOffset? at = null)
    {
        var versions = For(table, key);
        var source = versions.FirstOrDefault(version => version.Id == versionId);

        if (source is null)
        {
            return OgResult<OgRowVersion>.Failed(
                $"Version '{versionId}' is not in the history of {table} / {key}.",
                "version-unknown");
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgRowVersion>.Failed(
                "A restore must record who performed it.", "version-actor-required");
        }

        var restored = new OgRowVersion(
            Guid.NewGuid().ToString("n")[..12],
            table,
            key,
            versions.Count + 1,
            actor.Trim(),
            at ?? DateTimeOffset.UtcNow,
            new Dictionary<string, object?>(source.Values, StringComparer.OrdinalIgnoreCase),
            RestoredFrom: source.Id);

        versions.Add(restored);
        return OgResult<OgRowVersion>.Ok(restored);
    }

    private List<OgRowVersion> For(string table, string key)
    {
        var mapKey = $"{table}\u001f{key}";

        if (!_history.TryGetValue(mapKey, out var versions))
        {
            versions = [];
            _history[mapKey] = versions;
        }

        return versions;
    }
}

/// <summary>One read: who looked, at which table, through which filter, and how many rows came back (AC-2).</summary>
public sealed record OgAccessAuditEntry(
    string Reader,
    string Table,
    string Filter,
    int RowCount,
    DateTimeOffset AtUtc);

/// <summary>What the access-audit screen is showing; the export uses the same query (AC-5).</summary>
public sealed record OgAccessAuditQuery(
    string? Reader = null,
    string? Table = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null);

/// <summary>DAT-08: the read audit, so it can be held in memory or in the diagnostics database.</summary>
public interface IOgAccessAudit
{
    void Record(string reader, string table, string filter, int rowCount, DateTimeOffset? at = null);

    IReadOnlyList<OgAccessAuditEntry> Query(OgAccessAuditQuery? query = null);

    string ToCsv(OgAccessAuditQuery? query = null);
}

/// <summary>DAT-08 AC-2 and AC-5: the read audit and its export.</summary>
public sealed class OgAccessAudit : IOgAccessAudit
{
    private readonly List<OgAccessAuditEntry> _entries = [];

    public void Record(string reader, string table, string filter, int rowCount, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(table);

        _entries.Add(new OgAccessAuditEntry(
            reader.Trim(),
            table.Trim(),
            filter?.Trim() ?? string.Empty,
            rowCount,
            at ?? DateTimeOffset.UtcNow));
    }

    /// <summary>The rows the screen shows for this filter - and the only source of the export.</summary>
    public IReadOnlyList<OgAccessAuditEntry> Query(OgAccessAuditQuery? query = null)
    {
        query ??= new OgAccessAuditQuery();

        return _entries
            .Where(entry => query.Reader is null || string.Equals(entry.Reader, query.Reader, StringComparison.OrdinalIgnoreCase))
            .Where(entry => query.Table is null || string.Equals(entry.Table, query.Table, StringComparison.OrdinalIgnoreCase))
            .Where(entry => query.From is null || entry.AtUtc >= query.From)
            .Where(entry => query.To is null || entry.AtUtc <= query.To)
            .OrderByDescending(entry => entry.AtUtc)
            .ToList();
    }

    /// <summary>The export of exactly the rows the filter selects, so the file cannot disagree with the screen.</summary>
    public string ToCsv(OgAccessAuditQuery? query = null)
    {
        var builder = new StringBuilder("utc,reader,table,filter,rowCount\n");

        foreach (var entry in Query(query))
        {
            builder.Append(entry.AtUtc.ToString("O", CultureInfo.InvariantCulture)).Append(',')
                .Append(Quote(entry.Reader)).Append(',')
                .Append(Quote(entry.Table)).Append(',')
                .Append(Quote(entry.Filter)).Append(',')
                .Append(entry.RowCount)
                .Append('\n');
        }

        return builder.ToString();
    }

    private static string Quote(string value) => '"' + value.Replace("\"", "\"\"") + '"';
}

/// <summary>Whether a sync target has run, and how it ended.</summary>
public enum OgSyncState
{
    NeverRun,
    Succeeded,
    Failed,
}

/// <summary>DAT-08 AC-3: never-run and last-failed are different states with different sentences.</summary>
public sealed record OgSyncStatus(
    string Target,
    OgSyncState State,
    DateTimeOffset? LastAttemptUtc = null,
    DateTimeOffset? LastSuccessUtc = null,
    string? Error = null)
{
    public string Describe() => State switch
    {
        OgSyncState.NeverRun => $"{Target} has never been synchronised.",
        OgSyncState.Succeeded => $"{Target} last synchronised successfully at {LastSuccessUtc:yyyy-MM-dd HH:mm} UTC.",
        OgSyncState.Failed => $"{Target} failed at {LastAttemptUtc:yyyy-MM-dd HH:mm} UTC" +
                              (LastSuccessUtc is { } last ? $" (last success {last:yyyy-MM-dd HH:mm} UTC)" : " (it has never succeeded)") +
                              $": {Error}",
    };

    /// <summary>A word for the screen's badge, so the state is not colour alone.</summary>
    public string Badge => State switch
    {
        OgSyncState.NeverRun => "never run",
        OgSyncState.Succeeded => "in step",
        OgSyncState.Failed => "last run failed",
    };
}

/// <summary>How a conflict was settled. There is no default: a person must choose (AC-4).</summary>
public enum OgConflictChoice
{
    KeepLocal,
    KeepRemote,
}

/// <summary>DAT-08 AC-4: a conflict between this copy and the other one, resolved only by an explicit choice.</summary>
public sealed record OgSyncConflict(
    string Id,
    string Table,
    string Key,
    IReadOnlyDictionary<string, object?> Local,
    IReadOnlyDictionary<string, object?> Remote,
    DateTimeOffset DetectedAtUtc,
    string? ResolvedBy = null,
    OgConflictChoice? Resolution = null)
{
    public bool IsResolved => ResolvedBy is not null && Resolution is not null;

    public string Describe() =>
        $"{Table} / {Key}: local and remote copies differ" +
        (IsResolved ? $" - resolved as {Resolution} by {ResolvedBy}." : " - unresolved.");
}

/// <summary>Conflict resolution: explicit, attributed, and refused while the choice is missing.</summary>
public static class OgSyncConflicts
{
    public static OgResult<OgSyncConflict> Resolve(
        OgSyncConflict conflict,
        OgConflictChoice? choice,
        string actor)
    {
        ArgumentNullException.ThrowIfNull(conflict);

        if (conflict.IsResolved)
        {
            return OgResult<OgSyncConflict>.Failed(
                "This conflict has already been resolved.", "conflict-already-resolved");
        }

        if (choice is null)
        {
            return OgResult<OgSyncConflict>.Failed(
                "A sync conflict needs an explicit choice: keep the local copy or keep the remote one.",
                "conflict-choice-required");
        }

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgSyncConflict>.Failed(
                "A conflict resolution must record who resolved it.", "conflict-actor-required");
        }

        return OgResult<OgSyncConflict>.Ok(conflict with
        {
            Resolution = choice,
            ResolvedBy = actor.Trim(),
        });
    }

    public static int Unresolved(IEnumerable<OgSyncConflict> conflicts) =>
        conflicts.Count(conflict => !conflict.IsResolved);
}

/// <summary>One link of the tamper-evident audit chain.</summary>
public sealed record OgAuditChainEntry(
    int Sequence,
    string Kind,
    string Detail,
    string Actor,
    DateTimeOffset AtUtc,
    string PreviousHash,
    string Hash);

/// <summary>The result of walking the chain.</summary>
public sealed record OgChainCheck(bool IsIntact, int? BrokenAtSequence, int Entries)
{
    public string Describe() => IsIntact
        ? $"The audit chain is intact ({Entries} entries)."
        : $"The audit chain is broken at entry {BrokenAtSequence} of {Entries}; writes to governed tables are blocked.";
}

/// <summary>
/// The one place an acknowledgement lives, so a person who acknowledges a break on the governance page re-opens
/// writes on every page that asks the gate (AC-6), rather than only where the acknowledgement object happens to
/// be held.
/// </summary>
public sealed class OgChainGate
{
    private readonly object _lock = new();
    private OgChainAcknowledgement? _acknowledgement;

    /// <summary>Records the acknowledgement a person just made on the governance page.</summary>
    public void Acknowledge(int brokenAtSequence, string actor, DateTimeOffset? at = null)
    {
        lock (_lock)
        {
            _acknowledgement = new OgChainAcknowledgement(
                brokenAtSequence,
                actor ?? string.Empty,
                at ?? DateTimeOffset.UtcNow);
        }
    }

    /// <summary>The acknowledgement currently in force, or null.</summary>
    public OgChainAcknowledgement? Current
    {
        get
        {
            lock (_lock)
            {
                return _acknowledgement;
            }
        }
    }

    /// <summary>True when a governed write may proceed: the chain is intact, or this break was acknowledged.</summary>
    public bool Allows(IOgAuditChain chain)
    {
        ArgumentNullException.ThrowIfNull(chain);

        var check = chain.Verify();

        if (check.IsIntact)
        {
            return true;
        }

        var acknowledgement = Current;

        return acknowledgement is not null
               && acknowledgement.BrokenAtSequence == check.BrokenAtSequence
               && !string.IsNullOrWhiteSpace(acknowledgement.Actor);
    }
}

/// <summary>Acknowledgement that a person has seen the break, which is what re-opens writes (AC-6).</summary>/// <summary>Acknowledgement that a person has seen the break, which is what re-opens writes (AC-6).</summary>
public sealed record OgChainAcknowledgement(int BrokenAtSequence, string Actor, DateTimeOffset AtUtc);

/// <summary>DAT-08: the tamper-evident chain.</summary>
public interface IOgAuditChain
{
    IReadOnlyList<OgAuditChainEntry> Entries { get; }

    OgAuditChainEntry Append(string kind, string detail, string actor, DateTimeOffset? at = null);

    OgChainCheck Verify();

    bool WritesAllowed(OgChainCheck check, OgChainAcknowledgement? acknowledgement);
}

/// <summary>
/// DAT-08 AC-6: an append-only chain where each entry hashes the one before it; a broken chain is reported and
/// blocks writes to governed tables until a named person acknowledges the break.
/// </summary>
public sealed class OgAuditChain : IOgAuditChain
{
    private readonly List<OgAuditChainEntry> _entries = [];

    public IReadOnlyList<OgAuditChainEntry> Entries => _entries;

    public OgAuditChainEntry Append(string kind, string detail, string actor, DateTimeOffset? at = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);

        var previous = _entries.Count == 0 ? "genesis" : _entries[^1].Hash;
        var sequence = _entries.Count + 1;
        var when = at ?? DateTimeOffset.UtcNow;
        var payload = $"{sequence}|{kind}|{detail}|{actor}|{when:O}|{previous}";

        var entry = new OgAuditChainEntry(
            sequence,
            kind,
            detail ?? string.Empty,
            string.IsNullOrWhiteSpace(actor) ? "(unknown)" : actor.Trim(),
            when,
            previous,
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload))));

        _entries.Add(entry);
        return entry;
    }

    /// <summary>Walks the chain; a tampered entry or a broken link is reported with its sequence.</summary>
    public OgChainCheck Verify()
    {
        var previous = "genesis";

        foreach (var entry in _entries)
        {
            var payload = $"{entry.Sequence}|{entry.Kind}|{entry.Detail}|{entry.Actor}|{entry.AtUtc:O}|{previous}";
            var expected = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));

            if (!string.Equals(entry.PreviousHash, previous, StringComparison.Ordinal)
                || !string.Equals(entry.Hash, expected, StringComparison.Ordinal))
            {
                return new OgChainCheck(false, entry.Sequence, _entries.Count);
            }

            previous = entry.Hash;
        }

        return new OgChainCheck(true, null, _entries.Count);
    }

    /// <summary>Simulates tampering in a test or a drill: rewrites one entry's detail without re-hashing.</summary>
    public void Tamper(int sequence, string detail)
    {
        var index = _entries.FindIndex(entry => entry.Sequence == sequence);

        if (index < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sequence), sequence, "No such chain entry.");
        }

        _entries[index] = _entries[index] with { Detail = detail };
    }

    /// <summary>
    /// Whether a governed write may proceed: allowed while the chain is intact, and after a named person has
    /// acknowledged the specific break (AC-6).
    /// </summary>
    public bool WritesAllowed(OgChainCheck check, OgChainAcknowledgement? acknowledgement)
    {
        ArgumentNullException.ThrowIfNull(check);

        if (check.IsIntact)
        {
            return true;
        }

        return acknowledgement is not null
               && acknowledgement.BrokenAtSequence == check.BrokenAtSequence
               && !string.IsNullOrWhiteSpace(acknowledgement.Actor);
    }
}
