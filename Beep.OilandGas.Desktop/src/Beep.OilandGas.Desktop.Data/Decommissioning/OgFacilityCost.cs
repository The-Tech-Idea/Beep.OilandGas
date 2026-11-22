using System.Globalization;
using System.Text.Json;
using Beep.OilandGas.Decommissioning.Constants;
using Beep.OilandGas.Models.Data.Decommissioning;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;
using TheTechIdeaWeb.Diagnostics;
using LifeCycleRestoration = Beep.OilandGas.Models.Data.LifeCycle.ENVIRONMENTAL_RESTORATION;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Decommissioning;

/// <summary>One line item of a cost estimate (DEC-03).</summary>
public sealed record OgCostLine(string Category, decimal Amount)
{
    public string Describe() => Category + " " + Money(Amount);

    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);
}

/// <summary>
/// The arithmetic of one estimate (DEC-03, AC-1), with each step kept so the page can show it rather than only its result.
/// </summary>
public sealed record OgCostComputation(
    decimal Base,
    decimal EscalationRate,
    decimal EscalationYears,
    decimal Escalated,
    decimal ContingencyPct,
    decimal ContingencyAmount,
    decimal Total,
    string Currency)
{
    /// <summary>False when no escalation period was given, which changes what the total means.</summary>
    public bool HasEscalationBasis => EscalationYears > 0m;

    public string StepsText
    {
        get
        {
            var steps = new List<string>
            {
                "base " + Money(Base) + " " + Currency,
            };

            if (HasEscalationBasis)
            {
                steps.Add("× (1 + " + Percent(EscalationRate) + ") ^ " + Years(EscalationYears) + " = " +
                          Money(Escalated) + " " + Currency);
            }
            else
            {
                steps.Add("no escalation basis: the escalated base is the base, " + Money(Escalated) + " " + Currency);
            }

            steps.Add("+ contingency " + Percent(ContingencyPct / 100m) + " of " + Money(Escalated) + " " +
                      Currency + " = " + Money(ContingencyAmount) + " " + Currency);
            steps.Add("total " + Money(Total) + " " + Currency +
                      (HasEscalationBasis ? string.Empty : " (base + contingency only)"));

            return string.Join(Environment.NewLine, steps);
        }
    }

    public string TotalText => Money(Total) + " " + Currency;

    private static string Money(decimal value) => value.ToString("0.00", CultureInfo.InvariantCulture);

    private static string Percent(decimal value) => (value * 100m).ToString("0.####", CultureInfo.InvariantCulture) + " %";

    private static string Years(decimal value) => value.ToString("0.####", CultureInfo.InvariantCulture) + " year(s)";
}

/// <summary>One recorded estimate revision (DEC-03).</summary>
public sealed record OgCostRevision(
    string Id,
    int RevisionNo,
    string? PreviousId,
    string? Reason,
    string Currency,
    string Status,
    string? Author,
    string? ApprovedBy,
    DateTime? ApprovedDate,
    string? Conditions,
    IReadOnlyList<OgCostLine> Lines,
    OgCostComputation Computation)
{
    public bool IsApproved => string.Equals(Status, FacilityCostRules.Approved, StringComparison.OrdinalIgnoreCase);

    public string Describe() =>
        "revision " + RevisionNo.ToString(CultureInfo.InvariantCulture) + " · " + Computation.TotalText +
        " · " + Status.ToLowerInvariant() +
        (string.IsNullOrWhiteSpace(Reason) ? string.Empty : " · " + Reason) +
        (IsApproved
            ? " · approved by " + ApprovedBy + " on " +
              (ApprovedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)")
            : string.Empty);
}

/// <summary>The facility's own decommissioning record (DEC-03's scope panel).</summary>
public sealed record OgFacilityScope(
    string FacilityId,
    string State,
    string StateLabel,
    DateTime? PlannedDate,
    string? PlannedBy,
    string? Method)
{
    public string PlannedText => PlannedDate is { } planned
        ? "planned " + planned.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
          (string.IsNullOrWhiteSpace(PlannedBy) ? string.Empty : " by " + PlannedBy)
        : "no planned date";

    public string Describe() =>
        FacilityId + " · " + StateLabel + " · " + PlannedText +
        (string.IsNullOrWhiteSpace(Method) ? string.Empty : " · " + Method);
}

/// <summary>The facility decommissioning and cost workspace (DEC-03).</summary>
public sealed record OgFacilityWorkspace(
    OgHseScope Scope,
    string FacilityId,
    OgFacilityScope? Facility,
    IReadOnlyList<OgCostRevision> Revisions,
    IReadOnlyList<OgRestoration> Restoration,
    IReadOnlyList<OgHseLink> Links,
    IReadOnlyList<string> Blocks,
    DateTime ReadAt)
{
    /// <summary>The latest revision is the current estimate; the earlier ones are retained (AC-2).</summary>
    public OgCostRevision? Current => Revisions
        .OrderByDescending(revision => revision.RevisionNo)
        .FirstOrDefault();

    public OgCostRevision? LatestApproved => Revisions
        .Where(revision => revision.IsApproved)
        .OrderByDescending(revision => revision.RevisionNo)
        .FirstOrDefault();

    public int RestorationOpen => Restoration.Count(record => record.Completed is null);

    public bool IsComplete => Facility is not null &&
                              string.Equals(Facility.State, FacilityCostRules.Completed, StringComparison.OrdinalIgnoreCase);

    /// <summary>AC-4: the accounting period a provision on the current estimate would be recognised in.</summary>
    public string ProvisionPeriod => Facility?.PlannedDate is { } planned
        ? planned.ToString("yyyy-MM", CultureInfo.InvariantCulture)
        : Current is null
            ? "(no estimate)"
            : "(no planned date)";

    public string ProvisionText => LatestApproved is { } approved
        ? "provision preview " + approved.Computation.TotalText + " for period " + ProvisionPeriod +
          " (ACC-02 owns the posting; nothing is recognised from this screen)"
        : Current is { } current
            ? "no approved estimate: the current revision " + current.RevisionNo.ToString(CultureInfo.InvariantCulture) +
              " (" + current.Computation.TotalText + ") cannot be provided against until it is approved"
            : "no estimate is recorded, so there is no provision to preview";

    public string CostText => Current is null
        ? "No estimate is recorded for this facility."
        : Current.Computation.StepsText;

    public string RevisionsText => Revisions.Count == 0
        ? "No revision is recorded."
        : string.Join(Environment.NewLine,
            Revisions.OrderByDescending(revision => revision.RevisionNo).Select(revision => revision.Describe()));

    public string RestorationText => Restoration.Count == 0
        ? "No environmental restoration is recorded."
        : string.Join(Environment.NewLine, Restoration.Select(record => record.Describe()));

    public string LinksText => Links.Count == 0
        ? "no source is configured"
        : string.Join(Environment.NewLine, Links.Select(link =>
            link.Available ? link.Label + " · available" : link.Label + " · not available - " + link.WhyNot));

    /// <summary>The completion checklist, so the page and the command agree (AC-3's gate).</summary>
    public string ChecklistText => IsComplete
        ? "The facility decommissioning is complete."
        : Blocks.Count == 0
            ? "The facility can be completed: its restoration is complete and it is not already closed."
            : string.Join(Environment.NewLine, Blocks.Select(block => "✗ " + block));

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The cost rules DEC-03 states once.</summary>
public static class FacilityCostRules
{
    public const string CodeNoConnection = "facilitycost-no-connection";
    public const string CodeReadFailed = "facilitycost-read-failed";
    public const string CodeWriteFailed = "facilitycost-write-failed";
    public const string CodeActorRequired = "facilitycost-actor-required";
    public const string CodeFacilityRequired = "facilitycost-facility-required";
    public const string CodeCurrencyRequired = "facilitycost-currency-required";
    public const string CodeLineInvalid = "facilitycost-line-invalid";
    public const string CodeContingencyInvalid = "facilitycost-contingency-invalid";
    public const string CodeRateInvalid = "facilitycost-rate-invalid";
    public const string CodeReasonRequired = "facilitycost-reason-required";
    public const string CodeUnknownRevision = "facilitycost-unknown-revision";
    public const string CodeAlreadyApproved = "facilitycost-already-approved";
    public const string CodeSelfApproval = "facilitycost-self-approval";
    public const string CodeConditionsRequired = "facilitycost-conditions-required";
    public const string CodeRestorationIncomplete = "facilitycost-restoration-incomplete";
    public const string CodeAlreadyComplete = "facilitycost-already-complete";
    public const string CodeStatusRequired = "facilitycost-status-required";
    public const string CodeRestorationUnknown = "facilitycost-restoration-unknown";

    public const string Draft = "DRAFT";
    public const string Approved = "APPROVED";

    /// <summary>The owner's own facility states, from its reference defaults.</summary>
    public const string Planned = DecommissioningReferenceDefaults.Planned;
    public const string Completed = DecommissioningReferenceDefaults.Completed;

    /// <summary>
    /// AC-1: total = (base × escalation) + contingency, in decimal and step by step.
    /// </summary>
    /// <remarks>
    /// Escalation composes multiplicatively - (1 + rate) applied once per year, not rate × years - and the contingency is a
    /// percentage of the escalated base, not of the raw base. With no escalation period the total is base + contingency and
    /// says so, which is the feature's own "missing escalation basis" state. The multiplication is checked, so a value beyond
    /// decimal refuses rather than wrapping.
    /// </remarks>
    public static OgCostComputation Compute(
        IReadOnlyList<OgCostLine> lines,
        decimal escalationRate,
        decimal escalationYears,
        decimal contingencyPct,
        string currency)
    {
        ArgumentNullException.ThrowIfNull(lines);

        return Compute(lines.Sum(line => line.Amount), escalationRate, escalationYears, contingencyPct, currency);
    }

    /// <summary>The same arithmetic from a recorded base, so a stored revision is previewed from what it recorded.</summary>
    public static OgCostComputation Compute(
        decimal basis,
        decimal escalationRate,
        decimal escalationYears,
        decimal contingencyPct,
        string currency)
    {

        var escalated = basis;

        if (escalationYears > 0m)
        {
            var factor = 1m + escalationRate;

            // Once per whole year, with any fractional part applied as a final partial year.
            for (var year = 0; year < decimal.Truncate(escalationYears); year++)
            {
                escalated = decimal.Multiply(escalated, factor);
            }

            var fraction = escalationYears - decimal.Truncate(escalationYears);

            if (fraction > 0m)
            {
                escalated = decimal.Multiply(escalated, 1m + decimal.Multiply(escalationRate, fraction));
            }
        }

        var contingency = decimal.Multiply(escalated, contingencyPct / 100m);

        return new OgCostComputation(
            basis,
            escalationRate,
            escalationYears,
            Round(escalated),
            contingencyPct,
            Round(contingency),
            Round(escalated + contingency),
            currency);
    }

    /// <summary>Money is rounded once, to the currency's minor unit.</summary>
    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

    /// <summary>AC-5 and the open question: currency, lines and a reason from the second revision on are all explicit.</summary>
    public static (string Code, string Message)? ValidateEstimate(
        string? facilityId,
        string? currency,
        IReadOnlyList<OgCostLine> lines,
        decimal escalationRate,
        decimal escalationYears,
        decimal contingencyPct,
        string? reason,
        int revisionNo)
    {
        if (string.IsNullOrWhiteSpace(facilityId))
        {
            return (CodeFacilityRequired, "An estimate is made for one facility; none is named.");
        }

        if (string.IsNullOrWhiteSpace(currency))
        {
            return (CodeCurrencyRequired,
                "An estimate needs its currency. None is assumed from the company or the field.");
        }

        if (lines.Count == 0 || lines.Any(line => line.Amount < 0m))
        {
            return (CodeLineInvalid, "An estimate needs at least one line item, and no line may be negative.");
        }

        if (escalationRate < 0m || escalationRate >= 1m)
        {
            return (CodeRateInvalid,
                "An escalation rate is a fraction between 0 and 1 (0.03 is 3 % a year), not " +
                escalationRate.ToString(CultureInfo.InvariantCulture) + ".");
        }

        if (escalationYears < 0m)
        {
            return (CodeRateInvalid, "An escalation period cannot be negative.");
        }

        // The repository holds no configured contingency minimum, so only the arithmetic bound is enforced here.
        if (contingencyPct < 0m || contingencyPct > 100m)
        {
            return (CodeContingencyInvalid,
                "A contingency is a percentage between 0 and 100, not " +
                contingencyPct.ToString(CultureInfo.InvariantCulture) + ".");
        }

        return revisionNo > 1 && string.IsNullOrWhiteSpace(reason)
            ? (CodeReasonRequired,
                "A revision needs its reason, so the record shows why the estimate changed.")
            : null;
    }

    /// <summary>AC-6: the approver cannot be the author, and the approval records its conditions.</summary>
    public static (string Code, string Message)? ValidateApproval(
        OgCostRevision revision,
        string? actor,
        string? conditions)
    {
        ArgumentNullException.ThrowIfNull(revision);

        if (revision.IsApproved)
        {
            return (CodeAlreadyApproved,
                "Revision " + revision.RevisionNo.ToString(CultureInfo.InvariantCulture) + " is already approved.");
        }

        if (string.Equals(revision.Author?.Trim(), actor?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            return (CodeSelfApproval,
                revision.Author + " made this estimate and therefore cannot approve it.");
        }

        return string.IsNullOrWhiteSpace(conditions)
            ? (CodeConditionsRequired,
                "An approval records the conditions it was given under; say what they are, even if it is 'no conditions'.")
            : null;
    }

    /// <summary>AC-3: restoration is its own record, and it gates completion without sharing the facility's state.</summary>
    public static IReadOnlyList<string> CompletionBlocks(OgFacilityWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);

        var blocks = new List<string>();

        if (workspace.IsComplete)
        {
            blocks.Add(workspace.FacilityId + " is already completed.");
        }

        if (workspace.Facility is null)
        {
            blocks.Add("No decommissioning record is recorded for " + workspace.FacilityId + ".");
        }

        if (workspace.Restoration.Count == 0)
        {
            blocks.Add("No environmental restoration is recorded, so nothing shows the site is restored.");
        }
        else if (workspace.RestorationOpen > 0)
        {
            blocks.Add(workspace.RestorationOpen.ToString(CultureInfo.InvariantCulture) +
                       " environmental restoration record(s) are not complete, so the facility cannot reach complete.");
        }

        return blocks;
    }

    /// <summary>The owner's reference label for a facility state.</summary>
    public static string Label(string? state) => state?.Trim().ToUpperInvariant() switch
    {
        Planned => "planned",
        DecommissioningReferenceDefaults.InProgress => "in progress",
        Completed => "completed",
        DecommissioningReferenceDefaults.Cancelled => "cancelled",
        null or "" => "no state recorded",
        _ => state!.Trim().ToLowerInvariant(),
    };

    /// <summary>The decommissioning sources a person may open from this screen.</summary>
    public static IReadOnlyList<OgHseLink> LinksFor(Func<string, bool> canOpen)
    {
        ArgumentNullException.ThrowIfNull(canOpen);

        var routes = OgModuleCatalog.Pages.Select(page => page.Route).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sources = new (string Route, string Label)[]
        {
            ("decommissioning", "Decommissioning workspace"),
            ("wellpa", "Well plug and abandonment"),
            ("accounting", "Accounting"),
        };

        return sources.Select(source => routes.Contains(source.Route)
                ? canOpen(source.Route)
                    ? new OgHseLink(source.Route, source.Label, true, null)
                    : new OgHseLink(source.Route, source.Label, false, "your account may not open " + source.Route)
                : new OgHseLink(source.Route, source.Label, false, "not in this build"))
            .ToList();
    }

    /// <summary>Reads the line items a revision stored, tolerating a malformed payload by showing none rather than failing.</summary>
    public static IReadOnlyList<OgCostLine> ReadLines(
        string? json,
        IFailureReporter failures,
        string operation)
    {
        ArgumentNullException.ThrowIfNull(failures);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            var lines = JsonSerializer.Deserialize<List<OgCostLine>>(json) ?? [];

            return lines
                .Where(line => !string.IsNullOrWhiteSpace(line.Category))
                .ToList();
        }
        catch (JsonException ex)
        {
            failures.ReportHandled(
                ex, operation, "the revision is shown with its total but without its line items",
                FailureSeverity.Degraded);

            return [];
        }
    }

    public static string WriteLines(IReadOnlyList<OgCostLine> lines) =>
        JsonSerializer.Serialize(lines.Select(line => new OgCostLine(line.Category.Trim(), Round(line.Amount))));
}

/// <summary>DEC-03: facility decommissioning, its versioned cost estimates and its environmental restoration.</summary>
public interface IOgFacilityCost
{
    Task<OgResult<OgFacilityWorkspace>> LoadAsync(
        OgHseScope scope,
        string facilityId,
        CancellationToken cancellationToken = default);

    /// <summary>Records a new revision; the previous one is left untouched (AC-2).</summary>
    Task<OgResult<OgFacilityWorkspace>> SaveEstimateAsync(
        OgHseScope scope,
        string facilityId,
        DateTime plannedDate,
        string method,
        string currency,
        IReadOnlyList<OgCostLine> lines,
        decimal escalationRate,
        decimal escalationYears,
        decimal contingencyPct,
        string? reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Approves a revision, recording the actor, the date and the conditions (AC-6).</summary>
    Task<OgResult<OgFacilityWorkspace>> ApproveAsync(
        OgHseScope scope,
        string facilityId,
        string revisionId,
        string conditions,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Plans an environmental restoration record for the facility.</summary>
    Task<OgResult<OgFacilityWorkspace>> PlanRestorationAsync(
        OgHseScope scope,
        string facilityId,
        string restorationType,
        string method,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a restoration record's completion, which is what a facility completion needs (AC-3).</summary>
    Task<OgResult<OgFacilityWorkspace>> CompleteRestorationAsync(
        OgHseScope scope,
        string facilityId,
        string restorationId,
        string evidence,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Completes the facility, refused while any restoration record is open.</summary>
    Task<OgResult<OgFacilityWorkspace>> CompleteFacilityAsync(
        OgHseScope scope,
        string facilityId,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// DEC-03 over the owner's <c>DECOMMISSIONING_STATUS</c>, <c>ENVIRONMENTAL_RESTORATION</c> and the
/// <c>DECOMMISSIONING_COST_REVISION</c> records this feature added to the decommissioning module.
/// </summary>
/// <remarks>
/// The status row is the facility's state and its current figures; every estimate is additionally its own revision record, so
/// revising an estimate appends and never rewrites (AC-2). The status row's <c>ESTIMATED_COST</c> is therefore the current
/// revision's total - the figure DEC-01 already reads - rather than a second, separately maintained number.
/// </remarks>
public sealed class OgFacilityCostService : IOgFacilityCost
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgFacilityCostService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgFacilityWorkspace>> LoadAsync(
        OgHseScope scope,
        string facilityId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(facilityId))
        {
            return OgResult<OgFacilityWorkspace>.Failed(
                "Facility decommissioning is opened for one facility; none is named.",
                FacilityCostRules.CodeFacilityRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFacilityWorkspace>.Failed(
                "No connection is open, so the facility decommissioning record cannot be read.",
                FacilityCostRules.CodeNoConnection);
        }

        try
        {
            return await ReadAsync(source, scope, facilityId.Trim(), cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading facility decommissioning for " + facilityId,
                "the facility decommissioning record is not shown", FailureSeverity.Error);

            return OgResult<OgFacilityWorkspace>.Failed(
                "The facility decommissioning record could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? FacilityCostRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgFacilityWorkspace>> SaveEstimateAsync(
        OgHseScope scope,
        string facilityId,
        DateTime plannedDate,
        string method,
        string currency,
        IReadOnlyList<OgCostLine> lines,
        decimal escalationRate,
        decimal escalationYears,
        decimal contingencyPct,
        string? reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentNullException.ThrowIfNull(lines);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An estimate must record who made it.", FacilityCostRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the estimate cannot be saved.",
                FacilityCostRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, scope, facilityId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var workspace = read.Value;
            var revisionNo = (workspace.Current?.RevisionNo ?? 0) + 1;

            if (FacilityCostRules.ValidateEstimate(
                    facilityId, currency, lines, escalationRate, escalationYears, contingencyPct, reason, revisionNo)
                is { } refusal)
            {
                return Failure(refusal.Message, refusal.Code);
            }

            OgCostComputation computation;

            try
            {
                computation = FacilityCostRules.Compute(lines, escalationRate, escalationYears, contingencyPct,
                    currency.Trim().ToUpperInvariant());
            }
            catch (Exception ex) when (ex is OverflowException or ArgumentException)
            {
                var reference = _failures.ReportHandled(
                    ex, "computing the decommissioning estimate for " + facilityId,
                    "the estimate is refused; nothing is saved", FailureSeverity.Error);

                return Failure(
                    "The estimate cannot be computed within the supported range" +
                    (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                    FacilityCostRules.CodeWriteFailed);
            }

            var now = DateTime.UtcNow;

            // AC-2: a revision is a new record, and the one it replaces is never touched.
            var revisions = OgUow.For<DECOMMISSIONING_COST_REVISION>(source);
            revisions.PrimaryKey = nameof(DECOMMISSIONING_COST_REVISION.DECOMMISSIONING_COST_REVISION_ID);

            var revision = new DECOMMISSIONING_COST_REVISION
            {
                DECOMMISSIONING_COST_REVISION_ID = Guid.NewGuid().ToString("N"),
                FACILITY_DECOMMISSIONING_ID = ProgrammeId(facilityId),
                FACILITY_ID = facilityId.Trim(),
                FIELD_ID = workspace.Scope.FieldId,
                REVISION_NO = revisionNo,
                PREVIOUS_REVISION_ID = workspace.Current?.Id,
                REASON = string.IsNullOrWhiteSpace(reason) ? (revisionNo == 1 ? "First estimate." : null) : reason.Trim(),
                LINES_JSON = FacilityCostRules.WriteLines(lines),
                BASE_COST = computation.Base,
                ESCALATION_RATE = escalationRate,
                ESCALATION_YEARS = escalationYears,
                CONTINGENCY_PCT = contingencyPct,
                TOTAL_COST = computation.Total,
                CURRENCY = computation.Currency,
                STATUS = FacilityCostRules.Draft,
                AUTHOR = actor.Trim(),
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            };

            revisions.Add(revision);

            var committed = await revisions.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The estimate was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            // The facility's state row carries the plan and the current figure, which is what DEC-01 reads.
            var status = OgUow.For<DECOMMISSIONING_STATUS>(source);
            status.PrimaryKey = nameof(DECOMMISSIONING_STATUS.DECOMMISSIONING_STATUS_ID);

            status.Add(new DECOMMISSIONING_STATUS
            {
                DECOMMISSIONING_STATUS_ID = Guid.NewGuid().ToString("N"),
                FACILITY_DECOMMISSIONING_ID = ProgrammeId(facilityId),
                FACILITY_ID = facilityId.Trim(),
                FIELD_ID = workspace.Scope.FieldId,
                STATUS = FacilityCostRules.Planned,
                STATUS_DATE = now,
                STATUS_CHANGED_BY = actor.Trim(),
                PLANNED_DATE = plannedDate.Date,
                PLANNED_BY = actor.Trim(),
                DECOMMISSIONING_METHOD = string.IsNullOrWhiteSpace(method) ? null : method.Trim().ToUpperInvariant(),
                ESTIMATED_COST = computation.Total,
                ACTUAL_COST = workspace.Facility is null ? null : workspace.Current?.Computation.Total,
                COST_CURRENCY = computation.Currency,
                NOTES = "Revision " + revisionNo.ToString(CultureInfo.InvariantCulture) + " estimated at " +
                        computation.TotalText,
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var statusCommitted = await status.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(statusCommitted))
            {
                return Failure(OgUow.Message(statusCommitted, "The facility's plan was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, facilityId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the decommissioning estimate for " + facilityId,
                "the estimate was not saved", FailureSeverity.Error);

            return Failure(
                "The estimate could not be saved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? FacilityCostRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFacilityWorkspace>> ApproveAsync(
        OgHseScope scope,
        string facilityId,
        string revisionId,
        string conditions,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An approval must record who made it.", FacilityCostRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the estimate cannot be approved.",
                FacilityCostRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, scope, facilityId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var revision = read.Value.Revisions.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, revisionId, StringComparison.OrdinalIgnoreCase));

            if (revision is null)
            {
                return Failure("Revision '" + revisionId + "' is not recorded for " + facilityId + ".",
                    FacilityCostRules.CodeUnknownRevision);
            }

            if (FacilityCostRules.ValidateApproval(revision, actor, conditions) is { } refusal)
            {
                return Failure(refusal.Message, refusal.Code);
            }

            var unit = OgUow.For<DECOMMISSIONING_COST_REVISION>(source);
            unit.PrimaryKey = nameof(DECOMMISSIONING_COST_REVISION.DECOMMISSIONING_COST_REVISION_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(DECOMMISSIONING_COST_REVISION.DECOMMISSIONING_COST_REVISION_ID), revisionId)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return Failure("Revision '" + revisionId + "' is not recorded for " + facilityId + ".",
                    FacilityCostRules.CodeUnknownRevision);
            }

            row.STATUS = FacilityCostRules.Approved;
            row.APPROVED_BY = actor.Trim();
            row.APPROVED_DATE = DateTime.UtcNow;
            row.APPROVAL_CONDITIONS = conditions.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The approval was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The approval was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, facilityId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "approving revision " + revisionId + " for " + facilityId,
                "the approval was not saved", FailureSeverity.Error);

            return Failure(
                "The estimate could not be approved" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? FacilityCostRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFacilityWorkspace>> PlanRestorationAsync(
        OgHseScope scope,
        string facilityId,
        string restorationType,
        string method,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A restoration record must record who planned it.", FacilityCostRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(restorationType))
        {
            return Failure("A restoration record needs its type.", FacilityCostRules.CodeStatusRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the restoration cannot be planned.",
                FacilityCostRules.CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;

            var unit = OgUow.For<LifeCycleRestoration>(source);
            unit.PrimaryKey = nameof(LifeCycleRestoration.RESTORATION_ID);

            unit.Add(new LifeCycleRestoration
            {
                RESTORATION_ID = Guid.NewGuid().ToString("N"),
                SITE_ID = facilityId.Trim(),
                SITE_TYPE = "FACILITY",
                RESTORATION_TYPE = restorationType.Trim().ToUpperInvariant(),
                REMEDIATION_METHOD = string.IsNullOrWhiteSpace(method) ? null : method.Trim(),
                STATUS = FacilityCostRules.Planned,
                CERTIFICATION_STATUS = "PENDING",
                START_DATE = null,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The restoration record was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            var _ = now;

            return await AfterAsync(source, scope, facilityId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "planning environmental restoration for " + facilityId,
                "the restoration record was not saved", FailureSeverity.Error);

            return Failure(
                "The restoration could not be planned" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? FacilityCostRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFacilityWorkspace>> CompleteRestorationAsync(
        OgHseScope scope,
        string facilityId,
        string restorationId,
        string evidence,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(evidence))
        {
            return Failure(
                "Completing a restoration records who did it and the evidence that it is done.",
                FacilityCostRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the restoration cannot be completed.",
                FacilityCostRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<LifeCycleRestoration>(source);
            unit.PrimaryKey = nameof(LifeCycleRestoration.RESTORATION_ID);

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal(nameof(LifeCycleRestoration.RESTORATION_ID), restorationId)],
                cancellationToken).ConfigureAwait(false);

            var record = rows.FirstOrDefault();

            if (record is null)
            {
                return Failure("Restoration '" + restorationId + "' is not recorded.",
                    FacilityCostRules.CodeRestorationUnknown);
            }

            record.STATUS = FacilityCostRules.Completed;
            record.CERTIFICATION_STATUS = "CERTIFIED";
            record.CERTIFICATION_DATE = DateTime.UtcNow;
            record.COMPLETION_DATE = DateTime.UtcNow.Date;
            record.REMARK = evidence.Trim();

            var updated = unit.Update(record);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The restoration completion was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The restoration completion was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, facilityId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "completing restoration " + restorationId + " for " + facilityId,
                "the restoration completion was not saved", FailureSeverity.Error);

            return Failure(
                "The restoration could not be completed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? FacilityCostRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFacilityWorkspace>> CompleteFacilityAsync(
        OgHseScope scope,
        string facilityId,
        string rationale,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A completion must record who made it.", FacilityCostRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            return Failure("A completion needs its rationale.", FacilityCostRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the facility cannot be completed.",
                FacilityCostRules.CodeNoConnection);
        }

        try
        {
            var read = await ReadAsync(source, scope, facilityId.Trim(), cancellationToken).ConfigureAwait(false);

            if (!read.Success || read.Value is null)
            {
                return read;
            }

            var blocks = FacilityCostRules.CompletionBlocks(read.Value);

            if (blocks.Count > 0)
            {
                return Failure(string.Join(" ", blocks), FacilityCostRules.CodeRestorationIncomplete);
            }

            var now = DateTime.UtcNow;
            var workspace = read.Value;

            var unit = OgUow.For<DECOMMISSIONING_STATUS>(source);
            unit.PrimaryKey = nameof(DECOMMISSIONING_STATUS.DECOMMISSIONING_STATUS_ID);

            unit.Add(new DECOMMISSIONING_STATUS
            {
                DECOMMISSIONING_STATUS_ID = Guid.NewGuid().ToString("N"),
                FACILITY_DECOMMISSIONING_ID = ProgrammeId(facilityId),
                FACILITY_ID = facilityId.Trim(),
                FIELD_ID = workspace.Scope.FieldId,
                STATUS = FacilityCostRules.Completed,
                STATUS_DATE = now,
                STATUS_CHANGED_BY = actor.Trim(),
                PLANNED_DATE = workspace.Facility?.PlannedDate,
                PLANNED_BY = workspace.Facility?.PlannedBy,
                DECOMMISSIONING_METHOD = workspace.Facility?.Method,
                ESTIMATED_COST = workspace.Current?.Computation.Total,
                COST_CURRENCY = workspace.Current?.Currency,
                NOTES = rationale.Trim(),
                ACTIVE_IND = "Y",
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The completion was not saved."),
                    FacilityCostRules.CodeWriteFailed);
            }

            return await AfterAsync(source, scope, facilityId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "completing the decommissioning of " + facilityId,
                "the completion was not saved", FailureSeverity.Error);

            return Failure(
                "The facility could not be completed" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? FacilityCostRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgFacilityWorkspace>> AfterAsync(
        IDataSource source,
        OgHseScope scope,
        string facilityId,
        CancellationToken cancellationToken) =>
        await ReadAsync(source, scope, facilityId.Trim(), cancellationToken).ConfigureAwait(false);

    private async Task<OgResult<OgFacilityWorkspace>> ReadAsync(
        IDataSource source,
        OgHseScope scope,
        string facilityId,
        CancellationToken cancellationToken)
    {
        var statuses = await OgUow.ReadAllAsync<DECOMMISSIONING_STATUS>(source).ConfigureAwait(false);
        var revisions = await OgUow.ReadAllAsync<DECOMMISSIONING_COST_REVISION>(source).ConfigureAwait(false);
        var restoration = await OgUow.ReadAllAsync<LifeCycleRestoration>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var programmeId = ProgrammeId(facilityId);

        var mine = statuses
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.FACILITY_DECOMMISSIONING_ID, programmeId, StringComparison.OrdinalIgnoreCase) ||
                          string.Equals(row.FACILITY_ID?.Trim(), facilityId, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.STATUS_DATE)
            .ThenByDescending(row => row.ROW_CREATED_DATE)
            .ToList();

        var latest = mine.FirstOrDefault();
        var planned = mine.FirstOrDefault(row => row.PLANNED_DATE is not null);

        var recorded = revisions
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.Equals(row.FACILITY_DECOMMISSIONING_ID, programmeId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.REVISION_NO)
            .Select(row =>
            {
                var lines = FacilityCostRules.ReadLines(
                    row.LINES_JSON, _failures,
                    "reading the line items of revision " + row.REVISION_NO + " for " + facilityId);

                return new OgCostRevision(
                    row.DECOMMISSIONING_COST_REVISION_ID,
                    (int)row.REVISION_NO,
                    row.PREVIOUS_REVISION_ID,
                    row.REASON,
                    row.CURRENCY,
                    row.STATUS,
                    row.AUTHOR,
                    row.APPROVED_BY,
                    row.APPROVED_DATE,
                    row.APPROVAL_CONDITIONS,
                    lines,
                    // The stepped figures are recomputed from the revision's own recorded inputs, so the total is the record
                    // and the steps that produced it cannot drift from it.
                    FacilityCostRules.Compute(
                        row.BASE_COST, row.ESCALATION_RATE, row.ESCALATION_YEARS, row.CONTINGENCY_PCT,
                        row.CURRENCY) with
                    {
                        Total = row.TOTAL_COST,
                    });
            })
            .ToList();

        var restorationRows = restoration
            .Where(row => string.Equals(row.SITE_ID?.Trim(), facilityId, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.RESTORATION_TYPE, StringComparer.Ordinal)
            .Select(row => new OgRestoration(
                row.RESTORATION_ID ?? string.Empty,
                row.SITE_ID ?? string.Empty,
                row.RESTORATION_TYPE ?? string.Empty,
                row.STATUS ?? string.Empty,
                row.CERTIFICATION_STATUS ?? string.Empty,
                row.COMPLETION_DATE))
            .ToList();

        var workspace = new OgFacilityWorkspace(
            scope,
            facilityId,
            latest is null
                ? null
                : new OgFacilityScope(
                    latest.FACILITY_ID ?? facilityId,
                    latest.STATUS ?? string.Empty,
                    FacilityCostRules.Label(latest.STATUS),
                    planned?.PLANNED_DATE,
                    planned?.PLANNED_BY,
                    latest.DECOMMISSIONING_METHOD),
            recorded,
            restorationRows,
            FacilityCostRules.LinksFor(_ => true),
            [],
            DateTime.UtcNow);

        return OgResult<OgFacilityWorkspace>.Ok(workspace with
        {
            Blocks = FacilityCostRules.CompletionBlocks(workspace),
        });
    }

    private static string ProgrammeId(string facilityId) => "FD-" + facilityId.Trim();

    private static OgResult<OgFacilityWorkspace> Failure(string message, string code) =>
        OgResult<OgFacilityWorkspace>.Failed(message, code);
}
