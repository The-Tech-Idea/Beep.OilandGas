using System.Globalization;
using Beep.OilandGas.Models.Data.ProspectIdentification;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Exploration;

/// <summary>One step of the prospect funnel, counted from the records (EXP-01 AC-1).</summary>
public sealed record OgFunnelStage(string Stage, int Count, int Sequence);

/// <summary>The workspace's headline counts, each read from a table rather than inferred.</summary>
public sealed record OgExplorationKpis(int Plays, int Leads, int WellsPlanned, int WellsDrilling);

/// <summary>
/// One "source" link. <see cref="Available"/> is the page's own permission answer - the same gate the shell
/// applies - so a link that is offered is one that will open (EXP-01 AC-2).
/// </summary>
public sealed record OgExplorationLink(string Route, string Label, bool Available, string? WhyNot);

/// <summary>Everything the exploration workspace shows for one field and period.</summary>
public sealed record OgExplorationSnapshot(
    string? FieldId,
    string FieldName,
    string PeriodName,
    string Persona,
    OgExplorationKpis Kpis,
    IReadOnlyList<OgFunnelStage> Funnel,
    IReadOnlyList<string> IncompleteRiskProspects,
    IReadOnlyList<OgExplorationLink> Links)
{
    /// <summary>Prospects counted across the funnel stages.</summary>
    public int ProspectsInFunnel => Funnel.Sum(stage => stage.Count);

    /// <summary>AC-3: only the prospects that are actually missing risk elements.</summary>
    public int IncompleteRiskCount => IncompleteRiskProspects.Count;

    /// <summary>True when the field has no exploration records at all.</summary>
    public bool HasActivity =>
        ProspectsInFunnel > 0
        || Kpis.Plays > 0
        || Kpis.Leads > 0
        || Kpis.WellsPlanned > 0
        || Kpis.WellsDrilling > 0;

    /// <summary>The links this account may actually open.</summary>
    public IReadOnlyList<OgExplorationLink> AvailableLinks =>
        Links.Where(link => link.Available).ToList();

    /// <summary>One sentence for the funnel, so an empty funnel is not mistaken for a failed read.</summary>
    public string FunnelText => Funnel.Count == 0
        ? "No prospect has a recorded workflow stage."
        : string.Join(
            " · ",
            Funnel.Select(stage => $"{stage.Stage} {stage.Count.ToString(CultureInfo.InvariantCulture)}"));
}

/// <summary>
/// EXP-01: the exploration persona's landing page - funnel counts, the field's plays/leads/wells, the prospects
/// with incomplete risk data, and the source links this account may open.
/// </summary>
public interface IOgExplorationWorkspace
{
    /// <param name="canOpen">
    /// The shell's page-permission answer for a route. Every source link is decided by it (AC-2), so a link that
    /// is shown is one the person can reach.
    /// </param>
    Task<OgResult<OgExplorationSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        string periodName,
        string persona,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// The workspace over <c>PROSPECT</c>, <c>PROSPECT_WORKFLOW_STAGE</c>, <c>PROSPECT_RISK_ASSESSMENT</c>,
/// <c>PLAY</c>, <c>LEAD</c> and <c>WELL</c>, read through their entity models.
/// </summary>
/// <remarks>
/// Every number is counted from the rows; none is a default. A read that fails - a missing table, a provider
/// error - returns a failed result with a reference and no counts at all, so a zero on the page always means
/// "the records say zero" (AC-1).
/// </remarks>
public sealed class OgExplorationWorkspace : IOgExplorationWorkspace
{
    public const string CodeNoConnection = "exploration-workspace-no-connection";
    public const string CodeReadFailed = "exploration-workspace-read-failed";

    /// <summary>The five elements a complete geological risk assessment carries (AC-3).</summary>
    public static IReadOnlyList<string> RiskElements { get; } =
        ["TRAP_RISK", "RESERVOIR_RISK", "SEAL_RISK", "SOURCE_RISK", "TIMING_RISK"];

    /// <summary>
    /// The workspace's sources, and the feature that adds any page this build does not have. A route that is not
    /// in the catalog, or that this account may not open, is not offered and its reason is shown.
    /// </summary>
    public static IReadOnlyList<(string Route, string Label, string? ArrivesWith)> Sources { get; } =
    [
        ("wells", "Well index", null),
        ("stratigraphy", "Stratigraphy", null),
        ("wellpath", "Well path", null),
        ("prospects", "Prospect board", "EXP-02"),
        ("seismic", "Seismic surveys", "EXP-04"),
        ("wellprogram", "Well program approval", "EXP-05"),
        ("leases", "Lease register", "LSE-01"),
    ];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgExplorationWorkspace(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgExplorationSnapshot>> LoadAsync(
        string? fieldId,
        string fieldName,
        string periodName,
        string persona,
        Func<string, bool> canOpen,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        IDataSource? source;

        try
        {
            source = _source();
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "opening the connection for the exploration workspace",
                "the funnel is not shown; no counts are displayed",
                FailureSeverity.Error);

            return OgResult<OgExplorationSnapshot>.Failed(
                "The exploration workspace could not be read.", reference ?? CodeNoConnection);
        }

        if (source is null)
        {
            return OgResult<OgExplorationSnapshot>.Failed(
                "No connection is open, so the exploration workspace cannot be read.", CodeNoConnection);
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var prospects = await OgUow.ReadAllAsync<PROSPECT>(source).ConfigureAwait(false);
            var stages = await OgUow.ReadAllAsync<PROSPECT_WORKFLOW_STAGE>(source).ConfigureAwait(false);
            var assessments = await OgUow.ReadAllAsync<PROSPECT_RISK_ASSESSMENT>(source).ConfigureAwait(false);
            var plays = await OgUow.ReadAllAsync<PLAY>(source).ConfigureAwait(false);
            var leads = await OgUow.ReadAllAsync<LEAD>(source).ConfigureAwait(false);
            var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);

            cancellationToken.ThrowIfCancellationRequested();

            var scoped = prospects
                .Where(prospect => FieldMatches(prospect.FIELD_ID, fieldId, fieldName)
                                   || FieldMatches(prospect.PRIMARY_FIELD_ID, fieldId, fieldName))
                .ToList();

            var ids = scoped
                .Where(prospect => !string.IsNullOrWhiteSpace(prospect.PROSPECT_ID))
                .Select(prospect => prospect.PROSPECT_ID!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var funnel = Funnel(stages, ids);
            var playCount = plays.Count(play => FieldMatches(play.FIELD_ID, fieldId, fieldName));
            var leadCount = leads.Count(lead => FieldMatches(lead.FIELD_ID, fieldId, fieldName));
            var planned = CountWells(wells, fieldId, fieldName, "PLANNED");
            var drilling = CountWells(wells, fieldId, fieldName, "DRILLING");
            var incomplete = IncompleteRisk(assessments, ids);

            var snapshot = new OgExplorationSnapshot(
                string.IsNullOrWhiteSpace(fieldId) ? null : fieldId.Trim(),
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                string.IsNullOrWhiteSpace(periodName) ? "(no period)" : periodName.Trim(),
                string.IsNullOrWhiteSpace(persona) ? OgPersonaLanding.NoPersona : persona.Trim(),
                new OgExplorationKpis(playCount, leadCount, planned, drilling),
                funnel,
                incomplete,
                BuildLinks(canOpen));

            return OgResult<OgExplorationSnapshot>.Ok(snapshot);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the exploration workspace",
                "the funnel is not shown; no counts are displayed",
                FailureSeverity.Error);

            return OgResult<OgExplorationSnapshot>.Failed(
                "The exploration workspace could not be read.", reference ?? CodeReadFailed);
        }
    }

    /// <summary>AC-2: the links, each decided by the page's own permission.</summary>
    public static IReadOnlyList<OgExplorationLink> BuildLinks(Func<string, bool> canOpen)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        return Sources
            .Select(source =>
            {
                var available = canOpen(source.Route);

                return new OgExplorationLink(
                    source.Route,
                    source.Label,
                    available,
                    available
                        ? null
                        : source.ArrivesWith is { } feature
                            ? $"Not available in this build; the page arrives with {feature}."
                            : "Not available to this account.");
            })
            .ToList();
    }

    /// <summary>
    /// AC-3: a prospect is incomplete when it has no assessment at all, or an assessment that is missing any of
    /// the five elements. A prospect with a complete assessment is never counted.
    /// </summary>
    public static bool IsIncomplete(IReadOnlyDictionary<string, decimal?>? elements)
    {
        if (elements is null)
        {
            return true;
        }

        return RiskElements.Any(element =>
            !elements.TryGetValue(element, out var value) || value is null);
    }

    private static List<OgFunnelStage> Funnel(
        IEnumerable<PROSPECT_WORKFLOW_STAGE> stages,
        HashSet<string> prospectIds) =>
        stages
            .Where(stage => !string.IsNullOrWhiteSpace(stage.WORKFLOW_STAGE))
            .Where(stage => stage.PROSPECT_ID is not null && prospectIds.Contains(stage.PROSPECT_ID))
            .GroupBy(stage => stage.WORKFLOW_STAGE!.Trim(), StringComparer.OrdinalIgnoreCase)
            .Select(group => new OgFunnelStage(
                group.Key,
                group.Select(stage => stage.PROSPECT_ID).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                (int)group.Min(stage => stage.STAGE_SEQ_NO)))
            .OrderBy(stage => stage.Sequence)
            .ThenBy(stage => stage.Stage, StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static List<string> IncompleteRisk(
        IEnumerable<PROSPECT_RISK_ASSESSMENT> assessments,
        HashSet<string> prospectIds)
    {
        var byProspect = assessments
            .Where(row => row.PROSPECT_ID is not null && prospectIds.Contains(row.PROSPECT_ID))
            .GroupBy(row => row.PROSPECT_ID!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.ToList(), StringComparer.OrdinalIgnoreCase);

        var missing = new List<string>();

        foreach (var id in prospectIds)
        {
            // The same rule the reference query stated: no assessment, or any assessment missing an element.
            if (!byProspect.TryGetValue(id, out var rows)
                || rows.Any(row => RiskElements.Any(element => Element(row, element) is null)))
            {
                missing.Add(id);
            }
        }

        missing.Sort(StringComparer.OrdinalIgnoreCase);
        return missing;
    }

    private static decimal? Element(PROSPECT_RISK_ASSESSMENT row, string element) => element switch
    {
        "TRAP_RISK" => row.TRAP_RISK,
        "RESERVOIR_RISK" => row.RESERVOIR_RISK,
        "SEAL_RISK" => row.SEAL_RISK,
        "SOURCE_RISK" => row.SOURCE_RISK,
        "TIMING_RISK" => row.TIMING_RISK,
        _ => null,
    };

    private static int CountWells(
        IEnumerable<WELL> wells,
        string? fieldId,
        string? fieldName,
        string status) =>
        wells.Count(well =>
            FieldMatches(well.ASSIGNED_FIELD, fieldId, fieldName)
            && string.Equals(well.CURRENT_STATUS ?? string.Empty, status, StringComparison.OrdinalIgnoreCase));

    /// <summary>The field clause the workspace always used: id or name, over the named columns.</summary>
    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
         && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));
}
