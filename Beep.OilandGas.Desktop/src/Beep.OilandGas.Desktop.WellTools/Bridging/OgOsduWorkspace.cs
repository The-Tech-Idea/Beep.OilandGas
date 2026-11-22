using System.Net;
using System.Net.Http;
using System.Text.Json.Nodes;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.DataAI;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>One record problem, named by record id (INT-03 §5: an invalid export names the record).</summary>
public sealed record OgOsduProblem(string RecordId, string Problem);

/// <summary>The ingest plan: what would be sent, and what stops it.</summary>
public sealed record OgOsduPlan(
    int RecordCount,
    int BatchCount,
    IReadOnlyList<string> Kinds,
    IReadOnlyList<string> Problems)
{
    public bool CanSend => Problems.Count == 0 && BatchCount > 0;

    public string Describe() =>
        $"{RecordCount} record(s) · {BatchCount} batch(es) · {Kinds.Count} kind(s)";
}

public enum OgOsduBatchState
{
    Accepted,
    Rejected,

    /// <summary>Never attempted because an earlier batch failed.</summary>
    NotSent,
}

public sealed record OgOsduBatch(
    int Index,
    OgOsduBatchState State,
    int StatusCode,
    int RecordCount,
    string? Error,
    bool TokenRejected);

/// <summary>
/// The outcome of a send: per-batch state, counts, and whether the token was the problem (AC-6).
/// </summary>
public sealed record OgOsduResult(
    IReadOnlyList<OgOsduBatch> Batches,
    int Accepted,
    int Rejected,
    int NotSent,
    string? Refusal = null)
{
    public bool Succeeded =>
        Refusal is null && Batches.Count > 0 && Rejected == 0 && NotSent == 0;

    /// <summary>True when the platform answered 401/403: a re-authentication, not a retry.</summary>
    public bool TokenExpired => Batches.Any(batch => batch.TokenRejected);

    /// <summary>Only the failed and unattempted batches need sending again (AC-2).</summary>
    public bool CanRetry => Refusal is null && (Rejected > 0 || NotSent > 0);

    public string Describe() => Refusal is not null
        ? Refusal
        : $"{Accepted} accepted · {Rejected} rejected · {NotSent} not sent" +
          (TokenExpired ? " · the token was rejected" : string.Empty);
}

/// <summary>A proposed duplicate pair, with the evidence for and against. Never merged automatically.</summary>
public sealed record OgOsduCandidate(
    string A,
    string B,
    double Score,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> Conflicts);

/// <summary>
/// INT-03: OSDU record export, ingest planning/sending with per-batch outcomes, and well reconciliation.
/// </summary>
/// <remarks>
/// <para>
/// The records, the plan and the send are the tools': <see cref="OsduExport"/>,
/// <see cref="OsduIngest.Plan"/> and <see cref="OsduIngest.SendAsync"/>. This class adds the three things a
/// desktop needs: a per-record validity check that names the record, a result a person can read, and a retry
/// that resends <b>only</b> the batches that failed.
/// </para>
/// <para>
/// <b>A dry run sends nothing by construction.</b> Planning is pure; only <see cref="SendAsync"/> touches the
/// network, and it refuses a plan with problems before the first request.
/// </para>
/// <para>
/// <b>The token is never stored or echoed.</b> It is passed to the call and used for the Authorization header
/// only; no sentence here contains it. Persisting a token is the plan's DPAPI store, which does not exist
/// (ADR-014 chose a one-way derivation for sign-in), so it stays session-only.
/// </para>
/// </remarks>
/// <summary>The record kinds an export can carry (INT-03 §3: the Export tab selects them).</summary>
public enum OgOsduRecordKind
{
    Wellbore,
    WellboreTrajectory,
    WellLog,
}

/// <summary>What a checked export produced, or why it refused (INT-03's "schema-invalid record" state).</summary>
public sealed record OgOsduExport(
    string Json,
    IReadOnlyList<System.Text.Json.Nodes.JsonObject> Records,
    IReadOnlyList<OgOsduProblem> Problems,
    IReadOnlyList<string> Warnings)
{
    public bool Valid => Problems.Count == 0;

    public string KindsText => string.Join(", ", Records
        .Select(record => record["kind"]?.GetValue<string>() ?? string.Empty)
        .Distinct(StringComparer.Ordinal)
        .Select(kind => kind[(kind.LastIndexOf(':') + 1)..]));

    public string RefusalText => Problems.Count == 0
        ? string.Empty
        : Problems.Count + " record(s) would be invalid: " +
          string.Join("; ", Problems.Select(problem => problem.RecordId + ": " + problem.Problem));
}

public static class OgOsduWorkspace
{
    /// <summary>The kinds, with the id the platform names them by.</summary>
    public static IReadOnlyList<OgOsduRecordKind> Kinds { get; } =
        [OgOsduRecordKind.Wellbore, OgOsduRecordKind.WellboreTrajectory, OgOsduRecordKind.WellLog];

    public static string KindId(OgOsduRecordKind kind) => kind switch
    {
        OgOsduRecordKind.Wellbore => OsduExport.WellboreKind,
        OgOsduRecordKind.WellboreTrajectory => OsduExport.TrajectoryKind,
        OgOsduRecordKind.WellLog => OsduExport.WellLogKind,
    };

    public static string KindLabel(OgOsduRecordKind kind) => kind switch
    {
        OgOsduRecordKind.Wellbore => "Wellbore",
        OgOsduRecordKind.WellboreTrajectory => "WellboreTrajectory",
        OgOsduRecordKind.WellLog => "WellLog",
    };

    public const string CodeInvalidRecord = "osdu-record-invalid";

    public const string CodeNoRecords = "osdu-no-records";
    public const string CodeTokenMissing = "osdu-token-missing";

    /// <summary>A deterministic well with a survey and one depth log, for export and smoke evidence.</summary>
    public static WellModel ExampleWell()
    {
        var model = new WellModel();
        model.Header.Name = "OSDU-DEMO-1";
        model.Header.Uwi = "OSDU-DEMO-1-UWI";
        model.Header.Id = "OSDU-DEMO-1";
        model.Header.DepthDatum = "KB";
        model.Header.Datums.Add(new DatumElevation
        {
            Code = "KB",
            Elevation = new Quantity(25, "m"),
            Reference = "MSL",
        });

        model.Survey.Add(new SurveyStation { Md = new Quantity(0, "m"), Inclination = 0, Azimuth = 0 });
        model.Survey.Add(new SurveyStation { Md = new Quantity(1500, "m"), Inclination = 12, Azimuth = 45 });
        model.Survey.Add(new SurveyStation { Md = new Quantity(3000, "m"), Inclination = 30, Azimuth = 45 });

        var set = new LogSet { Name = "run 1", IndexType = LogIndexType.Depth, IndexUnit = "m", IndexMnemonic = "DEPT" };
        set.Index = [2000.0, 2000.5, 2001.0];

        var gr = new LogCurveData { Mnemonic = "GR", Unit = "gAPI", Description = "Gamma ray", Values = [45, 48, 51] };
        var rhob = new LogCurveData { Mnemonic = "RHOB", Unit = "g/cm3", Description = "Bulk density", Values = [2.35, 2.36, 2.37] };
        set.Curves.Add(gr);
        set.Curves.Add(rhob);
        model.LogSets.Add(set);

        return model;
    }

    /// <summary>The OSDU records for a well, with the export's own warnings kept.</summary>
    public static IReadOnlyList<JsonObject> Records(WellModel model, out IReadOnlyList<string> warnings)
    {
        ArgumentNullException.ThrowIfNull(model);

        var collected = new List<string>();
        var records = OsduExport.Records(model, collected);
        warnings = collected;

        return records;
    }

    /// <summary>The records as indented JSON (AC-1).</summary>
    public static OgResult<string> ExportJson(WellModel model, out IReadOnlyList<string> warnings)
    {
        var records = Records(model, out warnings);

        if (records.Count == 0)
        {
            return OgResult<string>.Failed(
                "The well produced no OSDU records: it needs a wellbore, a survey or a depth log.",
                CodeNoRecords);
        }

        return OgResult<string>.Ok(OsduExport.ToJson(records));
    }

    /// <summary>
    /// The record problems that would make the platform refuse them (§5: name the record, not the file).
    /// </summary>
    /// <remarks>
    /// These are the envelope rules the platform enforces — partition-prefixed id, kind shape, ACL groups,
    /// legal tags — not the full OSDU JSON schemas, which are not shipped here.
    /// </remarks>
    public static IReadOnlyList<OgOsduProblem> Validate(IReadOnlyList<JsonObject> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var partition = WellboreEnvironment.Current.Osdu.DataPartition;
        var problems = new List<OgOsduProblem>();

        foreach (var record in records)
        {
            var id = record["id"]?.GetValue<string>() ?? "(no id)";
            var kind = record["kind"]?.GetValue<string>() ?? string.Empty;

            if (id == "(no id)")
            {
                problems.Add(new OgOsduProblem(id, "the record has no id"));
                continue;
            }

            if (partition.Length > 0 && !id.StartsWith(partition + ":", StringComparison.Ordinal))
            {
                problems.Add(new OgOsduProblem(id, $"does not start with the data partition '{partition}:'"));
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                    kind,
                    @"^[\w\-\.]+:[\w\-\.]+:[\w\-\.]+:\d+\.\d+\.\d+$"))
            {
                problems.Add(new OgOsduProblem(id, $"kind '{kind}' is not authority:source:entity:major.minor.patch"));
            }

            if (record["acl"]?["viewers"] is not JsonArray { Count: > 0 }
                || record["acl"]?["owners"] is not JsonArray { Count: > 0 })
            {
                problems.Add(new OgOsduProblem(id, "no acl viewers / owners"));
            }

            if (record["legal"]?["legaltags"] is not JsonArray { Count: > 0 })
            {
                problems.Add(new OgOsduProblem(id, "no legal tags"));
            }

            if (record["data"] is not JsonObject)
            {
                problems.Add(new OgOsduProblem(id, "no data object"));
            }
        }

        return problems;
    }

    /// <summary>The records of the selected kinds only - the Export tab's record selection.</summary>
    public static IReadOnlyList<System.Text.Json.Nodes.JsonObject> Records(
        WellModel model, IReadOnlyList<OgOsduRecordKind> kinds, out IReadOnlyList<string> warnings)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(kinds);

        var all = Records(model, out var collected);

        warnings = collected;

        if (kinds.Count == Kinds.Count)
        {
            return all;
        }

        var wanted = kinds.Select(KindId).ToHashSet(StringComparer.Ordinal);

        return all
            .Where(record => wanted.Contains(record["kind"]?.GetValue<string>() ?? string.Empty))
            .ToList();
    }

    /// <summary>
    /// The export that refuses: the records are built, checked, and a validation problem is a refusal that names the record - the feature's
    /// "schema-invalid record" state - rather than a file written and discovered later by the platform.
    /// </summary>
    public static OgResult<OgOsduExport> Export(WellModel model, IReadOnlyList<OgOsduRecordKind> kinds)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(kinds);

        var records = Records(model, kinds, out var warnings);
        var problems = Validate(records);

        if (records.Count == 0)
        {
            return OgResult<OgOsduExport>.Failed(
                "None of the selected record kinds exists for this well, so there is nothing to export.", CodeInvalidRecord);
        }

        if (problems.Count > 0)
        {
            return OgResult<OgOsduExport>.Failed(
                "The export is refused: " + problems.Count + " record(s) would be invalid - " +
                string.Join("; ", problems.Select(problem => problem.RecordId + ": " + problem.Problem)) +
                ". The file is not written.", CodeInvalidRecord);
        }

        return OgResult<OgOsduExport>.Ok(new OgOsduExport(OsduExport.ToJson(records), records, problems, warnings));
    }

    /// <summary>The plan: batches and problems. Never sends anything (AC-5).</summary>
    public static OgOsduPlan Plan(IReadOnlyList<JsonObject> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        var plan = OsduIngest.Plan(records);
        var kinds = records
            .Select(record => record["kind"]?.GetValue<string>() ?? string.Empty)
            .Where(kind => kind.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(kind => kind, StringComparer.Ordinal)
            .ToList();

        return new OgOsduPlan(plan.RecordCount, plan.Requests.Count, kinds, [.. plan.Problems]);
    }

    /// <summary>
    /// Sends the records. A plan with problems or a missing token is refused <b>before</b> any request.
    /// </summary>
    public static async Task<OgOsduResult> SendAsync(
        IReadOnlyList<JsonObject> records,
        string? accessToken,
        HttpClient http,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(http);

        var toolsPlan = OsduIngest.Plan(records);

        if (!toolsPlan.CanSend)
        {
            return Refused(toolsPlan);
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new OgOsduResult([], 0, 0, 0, "An access token is required; nothing was sent.");
        }

        var result = await OsduIngest
            .SendAsync(toolsPlan, http, accessToken, cancellationToken)
            .ConfigureAwait(false);

        return Map(result, toolsPlan, indexMap: null);
    }

    /// <summary>
    /// Resends only the batches that failed or were never attempted (AC-2). The run is an upsert, so a batch
    /// that actually arrived is harmless to send again — but it is still not resent, so the outcome stays clear.
    /// </summary>
    public static async Task<OgOsduResult> RetryFailedAsync(
        IReadOnlyList<JsonObject> records,
        OgOsduResult previous,
        string? accessToken,
        HttpClient http,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(records);
        ArgumentNullException.ThrowIfNull(previous);
        ArgumentNullException.ThrowIfNull(http);

        var toolsPlan = OsduIngest.Plan(records);
        var indices = previous.Batches
            .Where(batch => batch.State != OgOsduBatchState.Accepted)
            .Select(batch => batch.Index)
            .Distinct()
            .OrderBy(index => index)
            .ToList();

        if (indices.Count == 0)
        {
            return previous;
        }

        if (string.IsNullOrWhiteSpace(accessToken))
        {
            return new OgOsduResult([], 0, 0, 0, "An access token is required; nothing was sent.");
        }

        var subset = new OsduIngestPlan();

        foreach (var index in indices.Where(index => index >= 1 && index <= toolsPlan.Requests.Count))
        {
            var original = toolsPlan.Requests[index - 1];
            var clone = new OsduRequest { Method = original.Method, Url = original.Url, Body = original.Body };

            foreach (var header in original.Headers)
            {
                clone.Headers[header.Key] = header.Value;
            }

            clone.RecordIds.AddRange(original.RecordIds);
            subset.Requests.Add(clone);
        }

        if (subset.Requests.Count == 0)
        {
            return previous;
        }

        var retried = await OsduIngest
            .SendAsync(subset, http, accessToken, cancellationToken)
            .ConfigureAwait(false);

        return Map(retried, subset, indices);
    }

    /// <summary>
    /// Proposed duplicate wells with their evidence. Nothing is merged: the caller must ask a person, and the
    /// crosswalk that records an acceptance needs the database path (DAT-02).
    /// </summary>
    public static IReadOnlyList<OgOsduCandidate> Reconcile(
        IEnumerable<WellModel> wells,
        string source = "desktop")
    {
        ArgumentNullException.ThrowIfNull(wells);

        var identities = wells
            .Select((well, index) => Identity(well, source, index))
            .ToList();

        return WellReconciliation.Propose(identities)
            .Select(candidate => new OgOsduCandidate(
                $"{candidate.A.Source}|{candidate.A.Id}",
                $"{candidate.B.Source}|{candidate.B.Id}",
                candidate.Score,
                [.. candidate.Reasons],
                [.. candidate.Conflicts]))
            .ToList();
    }

    private static OgOsduResult Refused(OsduIngestPlan plan) =>
        new(
            [],
            0,
            0,
            0,
            plan.Problems.Count == 0
                ? "There are no records to send."
                : string.Join("; ", plan.Problems));

    private static OgOsduResult Map(
        OsduIngestResult result,
        OsduIngestPlan plan,
        IReadOnlyList<int>? indexMap)
    {
        var batches = new List<OgOsduBatch>();
        var accepted = 0;
        var rejected = 0;
        var notSent = 0;

        for (var i = 0; i < result.Batches.Count; i++)
        {
            var batch = result.Batches[i];
            var index = indexMap is null ? batch.Batch : indexMap[i];
            var tokenRejected = batch.StatusCode is (int)HttpStatusCode.Unauthorized or (int)HttpStatusCode.Forbidden;

            if (batch.Ok)
            {
                accepted++;
            }
            else
            {
                rejected++;
            }

            batches.Add(new OgOsduBatch(
                index,
                batch.Ok ? OgOsduBatchState.Accepted : OgOsduBatchState.Rejected,
                batch.StatusCode,
                batch.RecordCount > 0 ? batch.RecordCount : plan.Requests[i].RecordIds.Count,
                batch.Error,
                tokenRejected));
        }

        for (var i = result.Batches.Count; i < plan.Requests.Count; i++)
        {
            var index = indexMap is null ? i + 1 : indexMap[i];
            notSent++;
            batches.Add(new OgOsduBatch(
                index,
                OgOsduBatchState.NotSent,
                0,
                plan.Requests[i].RecordIds.Count,
                null,
                false));
        }

        return new OgOsduResult(batches, accepted, rejected, notSent);
    }

    private static WellIdentity Identity(WellModel well, string source, int index)
    {
        var id = !string.IsNullOrWhiteSpace(well.Header.Id)
            ? well.Header.Id
            : !string.IsNullOrWhiteSpace(well.Header.Uwi)
                ? well.Header.Uwi!
                : !string.IsNullOrWhiteSpace(well.Header.Name)
                    ? well.Header.Name
                    : "well-" + index.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return new WellIdentity
        {
            Source = source,
            Id = id,
            Name = well.Header.Name,
            Uwi = well.Header.Uwi,
            Field = well.Header.Field,
            Operator = well.Header.Operator,
            Latitude = well.Header.Latitude,
            Longitude = well.Header.Longitude,
        };
    }
}
