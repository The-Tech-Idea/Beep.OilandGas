using System.Globalization;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.DataAI;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;

/// <summary>One QC finding (INT-06's <c>OgQcFinding</c>): what it is about, which rule raised it and how many times.</summary>
/// <param name="Scope">"well" when the finding belongs to one well, "field" when the engine only aggregates it across the scope.</param>
public sealed record OgQcFinding(
    string Scope,
    string WellId,
    string Entity,
    string Field,
    string Rule,
    string Severity,
    string Message,
    int Occurrences)
{
    public string Describe() =>
        Severity.ToLowerInvariant() + " · " + Rule + " · " + (Scope == "well" ? WellId : "field") + " · " +
        Entity + "." + Field + " · " + Message;

    public bool IsError => string.Equals(Severity, OgFieldQcRules.Error, StringComparison.Ordinal);
}

/// <summary>How many of the wells read carry one item of the model (INT-06's coverage panel).</summary>
public sealed record OgQcCompleteness(string Field, int WellsWith, int WellsTotal)
{
    public double? Fraction => WellsTotal == 0 ? null : (double)WellsWith / WellsTotal;

    public string Describe() => Field + " · " + WellsWith.ToString(CultureInfo.InvariantCulture) + " of " +
                                WellsTotal.ToString(CultureInfo.InvariantCulture) + " well(s)" +
                                (Fraction is { } fraction
                                    ? " (" + (fraction * 100d).ToString("0", CultureInfo.InvariantCulture) + " %)"
                                    : string.Empty);
}

/// <summary>A proposed duplicate, and what a person decided about it (INT-06's duplicates panel).</summary>
public sealed record OgQcDuplicate(
    string KeyA,
    string KeyB,
    string NameA,
    string NameB,
    string UwiA,
    string UwiB,
    double Score,
    IReadOnlyList<string> Reasons,
    IReadOnlyList<string> Conflicts,
    string Decision)
{
    public bool Decided => !string.IsNullOrWhiteSpace(Decision);

    public string Describe() =>
        NameA + " (" + (string.IsNullOrWhiteSpace(UwiA) ? KeyA : UwiA) + ") ↔ " +
        NameB + " (" + (string.IsNullOrWhiteSpace(UwiB) ? KeyB : UwiB) + ") · " +
        (Score * 100d).ToString("0", CultureInfo.InvariantCulture) + " %" +
        (Reasons.Count == 0 ? string.Empty : " · " + string.Join("; ", Reasons)) +
        (Conflicts.Count == 0 ? string.Empty : " · against: " + string.Join("; ", Conflicts)) +
        (Decided ? " · " + Decision.ToLowerInvariant() : " · proposed only");
}

/// <summary>The field QC workspace (INT-06).</summary>
public sealed record OgFieldQcWorkspace(
    string Source,
    bool SourceConfigured,
    string? Reason,
    bool Cancelled,
    int CoveredWells,
    int TotalWells,
    IReadOnlyList<OgQcFinding> Findings,
    IReadOnlyList<OgQcCompleteness> Coverage,
    bool CoverageKnown,
    IReadOnlyList<OgSetupSuggestion> Suggestions,
    IReadOnlyList<string> Unresolved,
    IReadOnlyList<OgQcDuplicate> Duplicates,
    bool DuplicatesRun,
    IReadOnlyList<string> FailedWells,
    IReadOnlyList<string> Thresholds,
    IReadOnlyList<string> Notes,
    DateTime ReadAt)
{
    public bool HasFindings => Findings.Count > 0;

    public int Errors => Findings.Count(finding => finding.IsError);

    public string CountsText => !SourceConfigured
        ? "no source"
        : CoveredWells.ToString(CultureInfo.InvariantCulture) + " of " +
          TotalWells.ToString(CultureInfo.InvariantCulture) + " well(s) read" +
          (Cancelled ? " (cancelled)" : string.Empty) + " · " +
          Findings.Count.ToString(CultureInfo.InvariantCulture) + " finding(s), " +
          Errors.ToString(CultureInfo.InvariantCulture) + " error(s) · " +
          FailedWells.Count.ToString(CultureInfo.InvariantCulture) + " well(s) failed to load";

    public string SummaryText => !SourceConfigured
        ? "No data source is configured, so the run is disabled: " + (Reason ?? "none is named") + "."
        : !HasFindings && FailedWells.Count == 0
            ? "No findings for this scope: " + CoveredWells.ToString(CultureInfo.InvariantCulture) +
              " well(s) of '" + Source + "' were read and nothing was raised."
            : Findings.Count.ToString(CultureInfo.InvariantCulture) + " finding(s) over " +
              CoveredWells.ToString(CultureInfo.InvariantCulture) + " well(s) of '" + Source + "'" +
              (Cancelled
                  ? ", from a run cancelled after " + CoveredWells.ToString(CultureInfo.InvariantCulture) + " of " +
                    TotalWells.ToString(CultureInfo.InvariantCulture) + " well(s) - the findings cover only the wells read"
                  : string.Empty) + ".";

    public string CoverageText => !CoverageKnown
        ? "Coverage unknown: no well was read, so how much of the model is populated cannot be stated - this is not " +
          "the same as nothing being populated."
        : string.Join(Environment.NewLine, Coverage.Select(item => item.Describe()));

    public string SuggestionsText => Suggestions.Count == 0
        ? "No mapping suggestion is offered for this source; nothing has been applied."
        : string.Join(Environment.NewLine, Suggestions.Select(suggestion => suggestion.Describe()));

    /// <summary>
    /// The coverage's unresolved values. The proposals above are those the engine's matcher scored at least its own minimum, so a
    /// value listed here without a proposal is one it could not match - the 'show all' view of the same question.
    /// </summary>
    public string UnresolvedText => Unresolved.Count == 0
        ? "The coverage found nothing unresolved."
        : string.Join(Environment.NewLine, Unresolved.Select(value => "· " + value));

    public string DuplicatesText => !DuplicatesRun
        ? "Duplicates were not checked in this run."
        : Duplicates.Count == 0
            ? "No candidate duplicate was found at the configured threshold."
            : string.Join(Environment.NewLine, Duplicates.Select(duplicate => duplicate.Describe()));

    public string FailedText => FailedWells.Count == 0
        ? "Every well in scope loaded."
        : string.Join(Environment.NewLine, FailedWells.Select(failed => "✗ " + failed));

    public string ThresholdsText => Thresholds.Count == 0
        ? "No threshold is configured."
        : string.Join(Environment.NewLine, Thresholds.Select(threshold => "· " + threshold));

    public string NotesText => Notes.Count == 0
        ? "The run recorded nothing further."
        : string.Join(Environment.NewLine, Notes.Select(note => "! " + note));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The field QC rules INT-06 states once.</summary>
public static class OgFieldQcRules
{
    public const string CodeNoSource = "qc-no-source";
    public const string CodeSourceRequired = "qc-source-required";
    public const string CodeSourceUnknown = "qc-source-unknown";
    public const string CodeRunFailed = "qc-run-failed";
    public const string CodeActorRequired = "qc-actor-required";
    public const string CodeDuplicateUnknown = "qc-duplicate-unknown";
    public const string CodeCrosswalkFailed = "qc-crosswalk-failed";

    public const string Error = "Error";
    public const string Warning = "Warning";
    public const string Info = "Info";

    public static IReadOnlyList<string> Severities { get; } = [Error, Warning, Info];

    /// <summary>The engine's completeness codes, and the model item each one is about.</summary>
    public static IReadOnlyDictionary<string, string> CodeFields { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["QC001"] = "name",
        ["QC002"] = "survey",
        ["QC003"] = "position",
        ["QC004"] = "datum",
        ["QC005"] = "tops",
        ["QC006"] = "tubulars",
    };

    /// <summary>The eight items the engine records per well, so the coverage panel reads the same flags it judges.</summary>
    public static IReadOnlyList<string> CoverageFields { get; } =
        ["header", "position", "datum", "survey", "tubulars", "tops", "logs", "pressures"];

    /// <summary>AC-6: the findings as CSV - the same rows the grid holds, filtered or not.</summary>
    public static string Csv(IReadOnlyList<OgQcFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(findings);

        var text = new StringBuilder("scope,well,entity,field,rule,severity,occurrences,message");
        text.AppendLine();

        foreach (var finding in findings)
        {
            text.AppendLine(string.Join(",",
                Quote(finding.Scope),
                Quote(finding.WellId),
                Quote(finding.Entity),
                Quote(finding.Field),
                Quote(finding.Rule),
                Quote(finding.Severity),
                finding.Occurrences.ToString(CultureInfo.InvariantCulture),
                Quote(finding.Message)));
        }

        return text.ToString();
    }

    /// <summary>AC-6: the grid's filter, so the export can be the filtered rows rather than the whole run.</summary>
    public static IReadOnlyList<OgQcFinding> Filter(
        IReadOnlyList<OgQcFinding> findings,
        string? severity,
        string? needle)
    {
        ArgumentNullException.ThrowIfNull(findings);

        var wanted = string.IsNullOrWhiteSpace(severity) ? null : severity.Trim();

        return findings
            .Where(finding => wanted is null || string.Equals(finding.Severity, wanted, StringComparison.OrdinalIgnoreCase))
            .Where(finding => string.IsNullOrWhiteSpace(needle) ||
                              finding.Describe().Contains(needle.Trim(), StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>What a QC run is asked for (INT-06).</summary>
/// <summary>
/// What a QC run is asked for (INT-06).
/// </summary>
/// <param name="Progress">
/// Wells done and in total, so a long run can be watched and cancelled knowingly - and so a caller can stop it at a well boundary.
/// </param>
public sealed record OgFieldQcRequest(
    string Source,
    int MaxWells,
    bool WithDuplicates,
    IProgress<(int done, int total)>? Progress = null);

/// <summary>INT-06: the field's defects, the mapping gaps and the duplicate proposals.</summary>
public interface IOgFieldQc
{
    /// <summary>AC-3: runs over a source, keeping the scope actually covered when it is cancelled.</summary>
    Task<OgResult<OgFieldQcWorkspace>> RunAsync(
        OgFieldQcRequest request, CancellationToken cancellationToken = default);

    /// <summary>AC-4: records a person's decision about a proposed duplicate; the pair is not proposed again.</summary>
    Task<OgResult<OgFieldQcWorkspace>> DecideDuplicateAsync(
        OgFieldQcRequest request, string keyA, string keyB, bool confirmed, string actor,
        CancellationToken cancellationToken = default);

    /// <summary>AC-1: applies one proposal, because a person accepted it - through the setup engine, not a second path.</summary>
    Task<OgResult<IReadOnlyList<OgSetupSuggestion>>> AcceptSuggestionAsync(
        string source, OgSetupSuggestion suggestion, string actor, CancellationToken cancellationToken = default);
}

/// <summary>
/// INT-06 over the tools' <see cref="FieldQc"/>, <see cref="DataCoverage"/> and <see cref="WellReconciliation"/>, with the mapping
/// proposals applied through the setup engine INT-05 already drives.
/// </summary>
/// <remarks>
/// <para>
/// The run is the engine's: <see cref="FieldQc.Run(string, FieldQcOptions?)"/> reads every well of the source and raises its own codes
/// (QC001..QC006, LOAD and the equipment rules), and its completeness flags are what the coverage panel counts. What this service
/// adds is the cancellation behaviour the feature asks for, the per-well attribution of the engine's codes, the duplicate proposals
/// with their decisions on disk, and the export.
/// </para>
/// <para>
/// A cancelled run cannot be resumed from inside the engine - it throws before reading the next well - so the covered slice is run
/// again without cancellation to produce the report for exactly the wells that were read. That keeps the findings the engine's own
/// rather than a second implementation's.
/// </para>
/// </remarks>
public sealed class OgFieldQcService : IOgFieldQc
{
    private readonly IFailureReporter _failures;
    private readonly IOgWellboreSetup _setup;
    private readonly Func<string> _environmentFile;

    public OgFieldQcService(IFailureReporter failures, IOgWellboreSetup setup, Func<string> environmentFile)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _setup = setup ?? throw new ArgumentNullException(nameof(setup));
        _environmentFile = environmentFile ?? throw new ArgumentNullException(nameof(environmentFile));
    }

    public Task<OgResult<OgFieldQcWorkspace>> RunAsync(
        OgFieldQcRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Source))
        {
            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(
                "A QC run is over one data source; none was named.", OgFieldQcRules.CodeSourceRequired));
        }

        var source = request.Source.Trim();

        if (!WellboreEnvironment.Current.DataSources.TryGetValue(source, out var definition))
        {
            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Ok(Empty(
                source,
                configured: false,
                "'" + source + "' is not one of the environment's data sources",
                ["the run needs a configured source, so it is disabled rather than run over nothing"])));
        }

        try
        {
            var notes = new List<string>();
            var covered = 0;
            var total = 0;
            var cancelled = false;

            FieldQcReport report;

            var progress = new ProgressCounter(request.Progress);

            try
            {
                report = FieldQc.Run(source, new FieldQcOptions
                {
                    MaxWells = request.MaxWells > 0 ? request.MaxWells : int.MaxValue,
                    Cancellation = cancellationToken,
                    Progress = progress,
                });
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // AC-3: what was read is reported rather than lost. The engine throws before the next well, so the wells it finished
                // are run once more without cancellation - the same engine, over exactly the covered slice. The cancellation is
                // reported too, at Degraded, so a cancelled run is visible in the application's record rather than silently shorter.
                _failures.ReportHandled(
                    new OperationCanceledException("the field QC run was cancelled"),
                    "running field QC over " + source, "the run is cancelled and the covered scope is reported",
                    FailureSeverity.Degraded);

                cancelled = true;
                covered = progress.Done;
                total = progress.Total;

                notes.Add(
                    "the run was cancelled after " + covered.ToString(CultureInfo.InvariantCulture) + " of " +
                    total.ToString(CultureInfo.InvariantCulture) + " well(s); the findings below cover the wells that were read");

                report = SliceRun(source, covered, out var sliceNote);

                if (sliceNote is not null)
                {
                    notes.Add(sliceNote);
                }
            }

            covered = cancelled ? covered : report.Wells;
            total = cancelled && total > 0 ? total : Math.Max(report.Wells, progress.Total);

            var findings = FindingsOf(report, source, notes);
            var coverage = CoverageOf(report);

            var suggestions = _setup.SuggestAsync(source).GetAwaiter().GetResult();

            if (!suggestions.Success || suggestions.Value is null)
            {
                notes.Add("the mapping proposals could not be computed: " + suggestions.Message);
            }

            var proposals = suggestions.Value?.Suggestions ?? [];

            // The state the feature asks for: the values the engine could not match are shown rather than hidden, and the
            // threshold that decided it is on the page - the matcher keeps only what scores at least its own minimum.
            var unresolved = suggestions.Value?.Coverage
                .SelectMany(coverage => coverage.Problems)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];

            IReadOnlyList<OgQcDuplicate> duplicates = [];
            string? duplicateNote = null;

            if (request.WithDuplicates)
            {
                (duplicates, duplicateNote) = DuplicatesOf(source, request.MaxWells, out var note);
                duplicateNote = note;

                if (duplicateNote is not null)
                {
                    notes.Add(duplicateNote);
                }
            }

            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Ok(new OgFieldQcWorkspace(
                source,
                true,
                definition?.Kind,
                cancelled,
                covered,
                total,
                findings,
                coverage,
                coverage.Count > 0,
                proposals,
                unresolved,
                duplicates,
                request.WithDuplicates,
                report.Failed.ToList(),
                ThresholdsOf(),
                notes,
                DateTime.UtcNow)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "running field QC over " + source, "the run is not shown", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(
                "The QC run could not be completed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? OgFieldQcRules.CodeRunFailed));
        }
    }

    public Task<OgResult<OgFieldQcWorkspace>> DecideDuplicateAsync(
        OgFieldQcRequest request, string keyA, string keyB, bool confirmed, string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(
                "Recording a duplicate decision records who made it.", OgFieldQcRules.CodeActorRequired));
        }

        if (string.IsNullOrWhiteSpace(keyA) || string.IsNullOrWhiteSpace(keyB))
        {
            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(
                "A duplicate decision names two records; one of them is missing.", OgFieldQcRules.CodeDuplicateUnknown));
        }

        try
        {
            // The proposal is resolved against the engine's current list, so a decision can only be recorded about a pair the engine
            // still proposes.
            var proposals = ProposalsOf(request.Source.Trim(), 0, out var note);

            if (note is not null)
            {
                return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(note, OgFieldQcRules.CodeDuplicateUnknown));
            }

            var candidate = proposals.FirstOrDefault(proposal =>
                (string.Equals(proposal.A.Key, keyA, StringComparison.Ordinal) &&
                 string.Equals(proposal.B.Key, keyB, StringComparison.Ordinal)) ||
                (string.Equals(proposal.A.Key, keyB, StringComparison.Ordinal) &&
                 string.Equals(proposal.B.Key, keyA, StringComparison.Ordinal)));

            if (candidate is null)
            {
                return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(
                    "The engine no longer proposes " + keyA + " and " + keyB + " as a duplicate, so no decision was recorded.",
                    OgFieldQcRules.CodeDuplicateUnknown));
            }

            var file = CrosswalkFile();
            var crosswalk = File.Exists(file) ? WellCrosswalk.FromJson(File.ReadAllText(file)) : new WellCrosswalk();

            if (confirmed)
            {
                crosswalk.Confirm(candidate, actor.Trim());
            }
            else
            {
                crosswalk.Reject(candidate, actor.Trim());
            }

            Directory.CreateDirectory(Path.GetDirectoryName(file)!);
            var temporary = file + ".tmp";

            File.WriteAllText(temporary, crosswalk.ToJson(), new UTF8Encoding(false));
            File.Move(temporary, file, overwrite: true);

            return RunAsync(request, cancellationToken);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "recording a duplicate decision", "the decision is not recorded", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgFieldQcWorkspace>.Failed(
                "The decision could not be recorded (reference " + reference + ").",
                reference ?? OgFieldQcRules.CodeCrosswalkFailed));
        }
    }

    public async Task<OgResult<IReadOnlyList<OgSetupSuggestion>>> AcceptSuggestionAsync(
        string source, OgSetupSuggestion suggestion, string actor, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(suggestion);

        // AC-1: the acceptance goes through the setup engine's own command, which writes the mapping the manual editor would.
        var accepted = await _setup.AcceptAsync(
            source, suggestion.Area, suggestion.Where, suggestion.Value, suggestion.Target, actor, cancellationToken)
            .ConfigureAwait(false);

        if (!accepted.Success)
        {
            return OgResult<IReadOnlyList<OgSetupSuggestion>>.Failed(
                accepted.Message ?? "The proposal was refused.", accepted.Reference);
        }

        var refreshed = await _setup.SuggestAsync(source, cancellationToken).ConfigureAwait(false);

        return !refreshed.Success || refreshed.Value is null
            ? OgResult<IReadOnlyList<OgSetupSuggestion>>.Ok([])
            : OgResult<IReadOnlyList<OgSetupSuggestion>>.Ok(refreshed.Value.Suggestions);
    }

    // ------------------------------------------------------------------ the report and its parts

    /// <summary>
    /// The engine's findings, attributed to a well where its own codes name one item of the model and left at field scope where they
    /// do not - so a finding never claims a well the engine did not attribute.
    /// </summary>
    private static IReadOnlyList<OgQcFinding> FindingsOf(FieldQcReport report, string source, List<string> notes)
    {
        var findings = new List<OgQcFinding>();

        var groups = report.Issues.ToDictionary(group => group.Code, StringComparer.OrdinalIgnoreCase);

        foreach (var row in report.Rows)
        {
            void Missing(bool present, string code, string field)
            {
                if (present)
                {
                    return;
                }

                if (!groups.TryGetValue(code, out var group))
                {
                    notes.Add("the engine raised " + code + " for " + row.WellId + " without a group, so it is not attributed");
                    return;
                }

                findings.Add(new OgQcFinding(
                    "well", row.WellId, "well", field, code, group.Severity,
                    group.ExampleMessage, 1));
            }

            Missing(row.Header, "QC001", "name");
            Missing(row.Survey, "QC002", "survey");
            Missing(row.Position, "QC003", "position");
            Missing(row.Datum, "QC004", "datum");
            Missing(row.Tops, "QC005", "tops");
            Missing(row.Tubulars, "QC006", "tubulars");
        }

        foreach (var group in report.Issues
                     .Where(group => !OgFieldQcRules.CodeFields.ContainsKey(group.Code))
                     .OrderByDescending(group => group.Severity switch
                     {
                         OgFieldQcRules.Error => 3, OgFieldQcRules.Warning => 2, _ => 1
                     })
                     .ThenByDescending(group => group.Count)
                     .ThenBy(group => group.Code, StringComparer.Ordinal))
        {
            findings.Add(new OgQcFinding(
                "field", string.Join("; ", group.ExampleWells), "well", "(not attributed)", group.Code, group.Severity,
                group.ExampleMessage, group.Count));
        }

        _ = source;

        return findings
            .OrderByDescending(finding => finding.Severity switch
            {
                OgFieldQcRules.Error => 3, OgFieldQcRules.Warning => 2, _ => 1
            })
            .ThenBy(finding => finding.WellId, StringComparer.OrdinalIgnoreCase)
            .ThenBy(finding => finding.Rule, StringComparer.Ordinal)
            .ToList();
    }

    private static IReadOnlyList<OgQcCompleteness> CoverageOf(FieldQcReport report)
    {
        if (report.Rows.Count == 0)
        {
            return [];
        }

        var total = report.Rows.Count;

        return
        [
            new OgQcCompleteness("header", report.Rows.Count(row => row.Header), total),
            new OgQcCompleteness("position", report.Rows.Count(row => row.Position), total),
            new OgQcCompleteness("datum", report.Rows.Count(row => row.Datum), total),
            new OgQcCompleteness("survey", report.Rows.Count(row => row.Survey), total),
            new OgQcCompleteness("tubulars", report.Rows.Count(row => row.Tubulars), total),
            new OgQcCompleteness("tops", report.Rows.Count(row => row.Tops), total),
            new OgQcCompleteness("logs", report.Rows.Count(row => row.Logs), total),
            new OgQcCompleteness("pressures", report.Rows.Count(row => row.Pressures), total),
        ];
    }

    /// <summary>
    /// Re-runs the engine over the wells a cancelled run had finished, so the partial findings are the engine's own rather than a
    /// second implementation's.
    /// </summary>
    private static FieldQcReport SliceRun(string source, int covered, out string? note)
    {
        note = null;

        if (covered <= 0)
        {
            return new FieldQcReport { Source = source };
        }

        var provider = DataSources.OpenProvider(source);

        if (provider is null)
        {
            note = "the covered wells could not be re-read: '" + source + "' does not offer a provider, so the cancelled run " +
                   "reports its scope only";

            return new FieldQcReport { Source = source };
        }

        var ids = provider.FindWellsAsync(null).GetAwaiter().GetResult()
            .Take(covered)
            .ToList();

        return FieldQc.Run(new LimitedProvider(provider, ids), source, new FieldQcOptions { MaxWells = covered });
    }

    private (IReadOnlyList<OgQcDuplicate>, string?) DuplicatesOf(string source, int maxWells, out string? note)
    {
        note = null;

        var proposals = ProposalsOf(source, maxWells, out note);

        if (note is not null)
        {
            return ([], note);
        }

        var crosswalk = ReadCrosswalk();

        return (proposals.Select(candidate => new OgQcDuplicate(
                candidate.A.Key,
                candidate.B.Key,
                candidate.A.Name,
                candidate.B.Name,
                candidate.A.Uwi ?? string.Empty,
                candidate.B.Uwi ?? string.Empty,
                candidate.Score,
                candidate.Reasons.ToList(),
                candidate.Conflicts.ToList(),
                DecisionOf(crosswalk, candidate)))
            .ToList(), null);
    }

    private IReadOnlyList<MergeCandidate> ProposalsOf(string source, int maxWells, out string? note)
    {
        note = null;

        var provider = DataSources.OpenProvider(source);

        if (provider is null)
        {
            note = "'" + source + "' does not offer a provider, so no duplicate can be proposed";
            return [];
        }

        var wells = provider.FindWellsAsync(null).GetAwaiter().GetResult();

        if (maxWells > 0)
        {
            wells = wells.Take(maxWells).ToList();
        }

        var identities = new List<WellIdentity>();

        foreach (var well in wells)
        {
            try
            {
                identities.Add(WellIdentity.FromModel(source, provider.LoadWellAsync(well.Id).GetAwaiter().GetResult().Model));
            }
            catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
            {
                _failures.ReportHandled(
                    ex, "reading " + well.Id + " for the duplicate check", "that well is left out of the duplicate check",
                    FailureSeverity.Degraded);
            }
        }

        return WellReconciliation.Propose(identities, ReadCrosswalk());
    }

    private static string DecisionOf(WellCrosswalk crosswalk, MergeCandidate candidate) =>
        crosswalk.AreSame(candidate.A.Key, candidate.B.Key)
            ? "confirmed"
            : crosswalk.IsRejected(candidate.A.Key, candidate.B.Key)
                ? "rejected"
                : string.Empty;

    private WellCrosswalk ReadCrosswalk()
    {
        var file = CrosswalkFile();

        if (!File.Exists(file))
        {
            return new WellCrosswalk();
        }

        try
        {
            return WellCrosswalk.FromJson(File.ReadAllText(file));
        }
        catch (Exception ex) when (ex is System.Text.Json.JsonException or FormatException)
        {
            // A crosswalk that no longer parses is reported and treated as empty: the proposals are re-offered rather than the page
            // failing, and the file is left for a person to look at.
            var reference = _failures.ReportHandled(
                ex, "reading the duplicate decisions", "the decisions are treated as none", FailureSeverity.Degraded);

            _ = reference;

            return new WellCrosswalk();
        }
    }

    /// <summary>Where a person's duplicate decisions live: with the environment, because they are customer data.</summary>
    private string CrosswalkFile() =>
        Path.Combine(Path.GetDirectoryName(_environmentFile()) ?? ".", "well-crosswalk.json");

    /// <summary>
    /// The thresholds the run used, stated so a person can see why a finding or a proposal exists - the feature's own open question.
    /// </summary>
    private static IReadOnlyList<string> ThresholdsOf()
    {
        var thresholds = new List<string>
        {
            "the completeness codes (QC001..QC006) are raised by the engine whenever an item of the model is missing - there is no " +
            "count threshold to tune",
            "the duplicate candidates are those at or above dataai.minimumMatchScore = " +
            DataAIToolbox.Settings.MinimumMatchScore.ToString("0.##", CultureInfo.InvariantCulture),
            "the mapping proposals are those the engine's matcher scores at least 0.5 against a candidate",
        };

        if (DataAIToolbox.Settings.LocationTolerance is { } tolerance)
        {
            thresholds.Add("location agreement is judged within " + tolerance.Value.ToString("0", CultureInfo.InvariantCulture) +
                           " " + tolerance.Unit + "; farther than ten times that counts as a conflict");
        }
        else
        {
            thresholds.Add("location is not used as evidence: dataai.locationTolerance is not configured");
        }

        return thresholds;
    }

    private static OgFieldQcWorkspace Empty(string source, bool configured, string reason, IReadOnlyList<string> notes) =>
        new(source, configured, reason, false, 0, 0, [], [], false, [], [], [], false, [], ThresholdsOf(), notes, DateTime.UtcNow);

    /// <summary>A provider over one source limited to the wells already read, so a slice is the engine's own run.</summary>
    private sealed class LimitedProvider : IWellDataProvider
    {
        private readonly IWellDataProvider _inner;
        private readonly IReadOnlyList<WellSummary> _wells;

        public LimitedProvider(IWellDataProvider inner, IReadOnlyList<WellSummary> wells)
        {
            _inner = inner;
            _wells = wells;
        }

        public string Name => _inner.Name;

        public Task<IReadOnlyList<WellSummary>> FindWellsAsync(string? nameContains = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(_wells);

        public Task<WellLoadResult> LoadWellAsync(string wellId, CancellationToken cancellationToken = default) =>
            _inner.LoadWellAsync(wellId, cancellationToken);
    }

    /// <summary>The last progress the engine reported, so a cancelled run knows how far it got.</summary>
    private sealed class ProgressCounter : IProgress<(int done, int total)>
    {
        private readonly IProgress<(int done, int total)>? _caller;

        public ProgressCounter(IProgress<(int done, int total)>? caller) => _caller = caller;

        public int Done { get; private set; }

        public int Total { get; private set; }

        public void Report((int done, int total) value)
        {
            Done = value.done;
            Total = value.total;
            _caller?.Report(value);
        }
    }
}
