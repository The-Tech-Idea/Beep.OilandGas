using System.Globalization;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Hse;

/// <summary>One component of an obligation (HSE-05), matching the feature's <c>OgObligationComponent</c>.</summary>
public sealed record OgObligationComponent(
    decimal SeqNo,
    decimal ObsNo,
    string Name,
    bool Required,
    DateTime? FulfilledDate,
    string? Evidence)
{
    public bool Fulfilled => FulfilledDate is not null;

    public string Describe() =>
        Name + (Required ? " (required)" : " (optional)") +
        (Fulfilled
            ? " · fulfilled " + FulfilledDate!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            : " · not fulfilled") +
        (string.IsNullOrWhiteSpace(Evidence) ? string.Empty : " · " + Evidence);
}

/// <summary>
/// One obligation (HSE-05), matching the feature's <c>OgObligation</c>. Its state is derived from the recorded dates and
/// flags only; nothing about a legal deadline is interpreted.
/// </summary>
public sealed record OgObligation(
    string Id,
    decimal SeqNo,
    string Kind,
    string Description,
    string Jurisdiction,
    DateTime? DueDate,
    string State,
    IReadOnlyList<OgObligationComponent> Components)
{
    public bool IsOverdue => string.Equals(State, ObligationStatus.Overdue, StringComparison.OrdinalIgnoreCase);

    public bool IsUnscheduled => string.Equals(State, ComplianceRules.Unscheduled, StringComparison.OrdinalIgnoreCase);

    public bool IsFulfilled => string.Equals(State, ObligationStatus.Submitted, StringComparison.OrdinalIgnoreCase);

    public bool IsWaived => string.Equals(State, ObligationStatus.Waived, StringComparison.OrdinalIgnoreCase);

    public string JurisdictionText => string.IsNullOrWhiteSpace(Jurisdiction) ? "not specified" : Jurisdiction;

    /// <summary>AC-1 and AC-2: the recorded date, or "unscheduled" - never an inferred one.</summary>
    public string DueText => DueDate is { } due
        ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
        : "unscheduled";

    public string Describe() =>
        Id + " · " + Kind + " · " + JurisdictionText + " · due " + DueText + " · " +
        State.ToLowerInvariant() + " · " + Components.Count.ToString(CultureInfo.InvariantCulture) + " component(s)";
}

/// <summary>One day of the compliance calendar (HSE-05, AC-6).</summary>
public sealed record OgCalendarDay(DateTime Date, int Due, int Overdue)
{
    public bool HasAny => Due > 0 || Overdue > 0;

    public string Describe() =>
        Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " · due " +
        Due.ToString(CultureInfo.InvariantCulture) + (Overdue == 0
            ? string.Empty
            : " · overdue " + Overdue.ToString(CultureInfo.InvariantCulture));
}

/// <summary>The compliance workspace (HSE-05).</summary>
public sealed record OgComplianceWorkspace(
    string? FieldId,
    DateTime Month,
    IReadOnlyList<OgObligation> Obligations,
    IReadOnlyList<OgCalendarDay> Calendar,
    IReadOnlyList<string> Kinds,
    DateTime ReadAt)
{
    public int Total => Obligations.Count;

    public int OverdueCount => Obligations.Count(obligation => obligation.IsOverdue);

    public int PendingCount => Obligations.Count(obligation =>
        string.Equals(obligation.State, ObligationStatus.Pending, StringComparison.OrdinalIgnoreCase));

    public int FulfilledCount => Obligations.Count(obligation => obligation.IsFulfilled);

    public int WaivedCount => Obligations.Count(obligation => obligation.IsWaived);

    /// <summary>AC-2: an obligation with no recorded due date is unscheduled, and is never counted as overdue.</summary>
    public int UnscheduledCount => Obligations.Count(obligation => obligation.IsUnscheduled);

    /// <summary>AC-6: the calendar's totals and the list's are computed from the same rows.</summary>
    public int CalendarDue => Calendar.Sum(day => day.Due);

    public int CalendarOverdue => Calendar.Sum(day => day.Overdue);

    public string ListText => Obligations.Count == 0
        ? "No obligation is recorded for this field."
        : string.Join(Environment.NewLine, Obligations.Select(obligation => obligation.Describe()));

    public string CalendarText => Calendar.Count(day => day.HasAny) == 0
        ? "No obligation is due in " + Month.ToString("yyyy-MM", CultureInfo.InvariantCulture) + "."
        : string.Join(Environment.NewLine, Calendar.Where(day => day.HasAny).Select(day => day.Describe()));

    public string CountsText =>
        $"{Total.ToString(CultureInfo.InvariantCulture)} obligation(s) · " +
        $"{PendingCount.ToString(CultureInfo.InvariantCulture)} pending · " +
        $"{OverdueCount.ToString(CultureInfo.InvariantCulture)} overdue · " +
        $"{UnscheduledCount.ToString(CultureInfo.InvariantCulture)} unscheduled · " +
        $"{FulfilledCount.ToString(CultureInfo.InvariantCulture)} fulfilled · " +
        $"{WaivedCount.ToString(CultureInfo.InvariantCulture)} waived";

    /// <summary>AC-5: what the report process is and is not.</summary>
    public const string ReportProcessText =
        "The report process starts an internal request inside this application. No regulator delivery is established: " +
        "nothing is filed with a regulator from this screen, and no submission channel is configured.";

    public string ReadText =>
        "read at " + ReadAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
}

/// <summary>The compliance rules HSE-05 states once.</summary>
public static class ComplianceRules
{
    public const string CodeNoConnection = "compliance-no-connection";
    public const string CodeReadFailed = "compliance-read-failed";
    public const string CodeWriteFailed = "compliance-write-failed";
    public const string CodeActorRequired = "compliance-actor-required";
    public const string CodeInputRequired = "compliance-input-required";
    public const string CodeJurisdictionRequired = "compliance-jurisdiction-required";
    public const string CodeUnknownObligation = "compliance-unknown-obligation";
    public const string CodeUnknownComponent = "compliance-unknown-component";
    public const string CodeNoComponents = "compliance-no-components";
    public const string CodeComponentsOutstanding = "compliance-components-outstanding";
    public const string CodeReasonRequired = "compliance-reason-required";
    public const string CodeReportNotWired = "compliance-report-not-wired";

    /// <summary>A state the owner's vocabulary does not have: a recorded obligation with no due date (AC-2).</summary>
    public const string Unscheduled = "UNSCHEDULED";

    /// <summary>The owner's states, plus the unscheduled one this feature needs.</summary>
    public static IReadOnlyList<string> States { get; } =
    [
        ObligationStatus.Pending,
        ObligationStatus.Overdue,
        Unscheduled,
        ObligationStatus.Submitted,
        ObligationStatus.Waived,
    ];

    /// <summary>The obligation kinds the owner names, plus a free one for a jurisdiction this app may not know.</summary>
    public static IReadOnlyList<string> Kinds { get; } =
    [
        ObligationType.OnrrProductionReport,
        ObligationType.OnrrRoyaltyPayment,
        ObligationType.BseeSemsAudit,
        ObligationType.BseeIncidentReport,
        ObligationType.EpaGhgReportFlare,
        ObligationType.EpaGhgReportVent,
        ObligationType.AerSt39Report,
        ObligationType.AerIncidentReport,
        ObligationType.AlbertaCrownRoyalty,
        ObligationType.EcccNirReport,
        ObligationType.NrcReport,
        ObligationType.OsparReport,
        ObligationType.EuEtsReport,
        ObligationType.IogpGhgReport,
    ];

    /// <summary>The marker a waiver writes into the obligation's remark, so the reason is its own record.</summary>
    public const string WaiverPrefix = "WAIVED: ";

    /// <summary>
    /// The state of an obligation, from its recorded facts only (AC-1, AC-2).
    /// </summary>
    /// <remarks>
    /// Fulfilment and waiver are recorded facts; otherwise a recorded due date that has passed is overdue, a due date still
    /// ahead is pending, and no due date at all is <b>unscheduled</b> - never overdue. No legal deadline is interpreted and no
    /// date is inferred from a period or a jurisdiction.
    /// </remarks>
    public static string StateOf(bool fulfilled, bool waived, DateTime? dueDate, DateTime today)
    {
        if (fulfilled)
        {
            return ObligationStatus.Submitted;
        }

        if (waived)
        {
            return ObligationStatus.Waived;
        }

        if (dueDate is not { } due)
        {
            return Unscheduled;
        }

        return due.Date < today.Date ? ObligationStatus.Overdue : ObligationStatus.Pending;
    }

    /// <summary>AC-3: a new obligation needs its type, an explicit jurisdiction, a description and at least one component.</summary>
    public static (string Code, string Message)? Validate(OgObligationInputs inputs)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (string.IsNullOrWhiteSpace(inputs.Kind))
        {
            return (CodeInputRequired, "An obligation needs its type.");
        }

        // AC-3: the jurisdiction is explicit and never defaulted to a country.
        if (string.IsNullOrWhiteSpace(inputs.Jurisdiction))
        {
            return (CodeJurisdictionRequired,
                "An obligation needs its jurisdiction. None is assumed from the field, the company or a default country.");
        }

        if (string.IsNullOrWhiteSpace(inputs.Description))
        {
            return (CodeInputRequired, "An obligation needs its description.");
        }

        return inputs.Components is null || inputs.Components.Count == 0
            ? (CodeNoComponents,
                "An obligation needs at least one component; without one it could never be recorded as fulfilled.")
            : null;
    }

    /// <summary>
    /// AC-4: an obligation can be fulfilled only when it has components and every required one is fulfilled.
    /// </summary>
    public static (string Code, string Message)? CanFulfil(OgObligation obligation)
    {
        ArgumentNullException.ThrowIfNull(obligation);

        if (obligation.Components.Count == 0)
        {
            return (CodeNoComponents,
                obligation.Id + " has no recorded component, so it cannot be marked fulfilled. Record what the " +
                "obligation consists of first.");
        }

        var outstanding = obligation.Components
            .Where(component => component.Required && !component.Fulfilled)
            .Select(component => component.Name)
            .ToList();

        return outstanding.Count == 0
            ? null
            : (CodeComponentsOutstanding,
                outstanding.Count.ToString(CultureInfo.InvariantCulture) + " component(s) of " + obligation.Id +
                " are not fulfilled: " + string.Join("; ", outstanding) + ".");
    }

    /// <summary>AC-6: the calendar for a month, built from the same rows the list shows.</summary>
    public static IReadOnlyList<OgCalendarDay> CalendarOf(
        IEnumerable<OgObligation> obligations,
        DateTime month,
        DateTime today)
    {
        ArgumentNullException.ThrowIfNull(obligations);

        var days = Enumerable.Range(1, DateTime.DaysInMonth(month.Year, month.Month))
            .Select(day => new DateTime(month.Year, month.Month, day))
            .ToList();

        var byDay = obligations
            .Where(obligation => obligation.DueDate is { } due && !obligation.IsFulfilled && !obligation.IsWaived)
            .GroupBy(obligation => obligation.DueDate!.Value.Date)
            .ToDictionary(group => group.Key, group => group.ToList());

        return days
            .Select(day => byDay.TryGetValue(day, out var dueThatDay)
                ? new OgCalendarDay(day, dueThatDay.Count,
                    dueThatDay.Count(obligation => obligation.DueDate!.Value.Date < today.Date))
                : new OgCalendarDay(day, 0, 0))
            .ToList();
    }
}

/// <summary>The inputs of a new obligation (HSE-05).</summary>
/// <param name="DueDate">Optional: an obligation without one is unscheduled, not overdue (AC-2).</param>
public sealed record OgObligationInputs(
    string? Kind,
    string? Jurisdiction,
    string? FieldId,
    DateTime? DueDate,
    string Description,
    IReadOnlyList<string> Components);

/// <summary>HSE-05: the compliance calendar, the obligation register and its commands.</summary>
public interface IOgCompliance
{
    /// <summary>The obligations for one field (or all) and the calendar for one month.</summary>
    Task<OgResult<OgComplianceWorkspace>> LoadAsync(
        string? fieldId,
        DateTime month,
        CancellationToken cancellationToken = default);

    /// <summary>Creates an obligation with its components; the jurisdiction is explicit (AC-3).</summary>
    Task<OgResult<OgComplianceWorkspace>> CreateAsync(
        OgObligationInputs inputs,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records one component's fulfilment, with the evidence that discharged it.</summary>
    Task<OgResult<OgComplianceWorkspace>> FulfilComponentAsync(
        string obligationId,
        decimal seqNo,
        decimal componentObsNo,
        DateTime fulfilledDate,
        string? evidence,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Marks the obligation fulfilled; refused with components outstanding or none recorded (AC-4).</summary>
    Task<OgResult<OgComplianceWorkspace>> FulfilAsync(
        string obligationId,
        decimal seqNo,
        DateTime fulfilledDate,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Waives the obligation with its reason, which is kept as its own record.</summary>
    Task<OgResult<OgComplianceWorkspace>> WaiveAsync(
        string obligationId,
        decimal seqNo,
        string reason,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the internal report process. Refused in this build: no process engine is wired, and no regulator delivery is
    /// established (AC-5).
    /// </summary>
    Task<OgResult<OgComplianceWorkspace>> RequestReportAsync(
        string obligationId,
        decimal seqNo,
        string actor,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// HSE-05 over the owner's <c>OBLIGATION</c> and <c>OBLIGATION_COMPONENT</c>.
/// </summary>
/// <remarks>
/// The PPDM obligation carries the due date in <c>CRITICAL_DATE</c>, the kind in <c>OBLIGATION_TYPE</c>, the description in
/// <c>DESCRIPTION</c> and fulfilment in <c>FULFILLED_IND</c>/<c>FULFILLED_DATE</c>; its components carry their own
/// fulfilment dates and evidence in <c>FULFILLED_REMARK</c>. The two columns the feature needs but the table lacked - the
/// field and the jurisdiction - are additive columns, so a jurisdiction is always explicit (AC-3) and an obligation is
/// scoped to a field.
/// </remarks>
public sealed class OgCompliance : IOgCompliance
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgCompliance(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgComplianceWorkspace>> LoadAsync(
        string? fieldId,
        DateTime month,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgComplianceWorkspace>.Failed(
                "No connection is open, so the compliance register cannot be read.",
                ComplianceRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, fieldId, month, cancellationToken).ConfigureAwait(false);

            return OgResult<OgComplianceWorkspace>.Ok(workspace);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "reading the compliance register", "the compliance register is not shown; no deadline is inferred",
                FailureSeverity.Error);

            return OgResult<OgComplianceWorkspace>.Failed(
                "The compliance register could not be read" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ComplianceRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgComplianceWorkspace>> CreateAsync(
        OgObligationInputs inputs,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("An obligation must record who created it.", ComplianceRules.CodeActorRequired);
        }

        if (ComplianceRules.Validate(inputs) is { } refusal)
        {
            return Failure(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the obligation cannot be created.",
                ComplianceRules.CodeNoConnection);
        }

        try
        {
            var obligations = await OgUow.ReadAllAsync<OBLIGATION>(source).ConfigureAwait(false);

            var sequence = obligations
                .Where(row => string.Equals(row.FIELD_ID, inputs.FieldId, StringComparison.OrdinalIgnoreCase) ||
                              string.IsNullOrWhiteSpace(inputs.FieldId))
                .Select(row => row.OBLIGATION_SEQ_NO)
                .DefaultIfEmpty(0m)
                .Max() + 1m;

            // The owner's id shape: OBL-<field>-<due:yyyyMMdd>-<seq:000>; an unscheduled one says so rather than using today.
            var stamp = inputs.DueDate is { } due
                ? due.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                : "UNSCHEDULED";
            var id = "OBL-" + (string.IsNullOrWhiteSpace(inputs.FieldId) ? "ALL" : inputs.FieldId!.Trim()) + "-" +
                     stamp + "-" + sequence.ToString("000", CultureInfo.InvariantCulture);

            var now = DateTime.UtcNow;

            var unit = OgUow.For<OBLIGATION>(source);
            unit.PrimaryKey = "OBLIGATION_ID";

            unit.Add(new OBLIGATION
            {
                OBLIGATION_ID = id,
                OBLIGATION_SEQ_NO = sequence,
                OBLIGATION_TYPE = inputs.Kind!.Trim().ToUpperInvariant(),
                JURISDICTION = inputs.Jurisdiction!.Trim(),
                FIELD_ID = string.IsNullOrWhiteSpace(inputs.FieldId) ? null : inputs.FieldId!.Trim(),
                DESCRIPTION = inputs.Description.Trim(),
                WORK_OBLIGATION_DESC = inputs.Description.Trim(),
                CRITICAL_DATE = inputs.DueDate?.Date,
                FULFILLED_IND = "N",
                ACTIVE_IND = "Y",
                SOURCE = "BeepOilGasDesktop",
                EFFECTIVE_DATE = now,
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The obligation was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            var components = OgUow.For<OBLIGATION_COMPONENT>(source);
            components.PrimaryKey = "OBLIGATION_ID";

            var observation = 1m;

            foreach (var name in inputs.Components)
            {
                components.Add(new OBLIGATION_COMPONENT
                {
                    OBLIGATION_ID = id,
                    OBLIGATION_SEQ_NO = sequence,
                    COMPONENT_OBS_NO = observation,
                    DESCRIPTION = name.Trim(),
                    OBLIGATION_COMPONENT_TYPE = "REQUIRED",
                    REQUIRED_IND = "Y",
                    FULFILLED_IND = "N",
                    ACTIVE_IND = "Y",
                    SOURCE = "BeepOilGasDesktop",
                    EFFECTIVE_DATE = now,
                    PPDM_GUID = Guid.NewGuid().ToString("N"),
                    ROW_CREATED_BY = actor.Trim(),
                    ROW_CREATED_DATE = now,
                    ROW_CHANGED_BY = actor.Trim(),
                    ROW_CHANGED_DATE = now,
                });

                observation++;
            }

            var componentsCommitted = await components.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(componentsCommitted))
            {
                return Failure(OgUow.Message(componentsCommitted, "The obligation's components were not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            return await AfterAsync(source, inputs.FieldId, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "creating a compliance obligation", "the obligation was not created", FailureSeverity.Error);

            return Failure(
                "The obligation could not be created" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ComplianceRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgComplianceWorkspace>> FulfilComponentAsync(
        string obligationId,
        decimal seqNo,
        decimal componentObsNo,
        DateTime fulfilledDate,
        string? evidence,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A fulfilment must record who made it.", ComplianceRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the component cannot be fulfilled.",
                ComplianceRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<OBLIGATION_COMPONENT>(source);
            unit.PrimaryKey = "OBLIGATION_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [
                    OgUow.Equal("OBLIGATION_ID", obligationId),
                    OgUow.Equal("COMPONENT_OBS_NO", componentObsNo),
                ],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault(candidate =>
                candidate.OBLIGATION_SEQ_NO == seqNo);

            if (row is null)
            {
                return Failure(
                    $"Component {componentObsNo} is not on obligation {obligationId}.",
                    ComplianceRules.CodeUnknownComponent);
            }

            row.FULFILLED_IND = "Y";
            row.FULFILLED_DATE = fulfilledDate.Date;
            row.FULFILLED_REMARK = string.IsNullOrWhiteSpace(evidence) ? null : evidence.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The component's fulfilment was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The component's fulfilment was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            return await AfterAsync(source, null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"fulfilling a component of {obligationId}", "the component was not fulfilled",
                FailureSeverity.Error);

            return Failure(
                "The component could not be fulfilled" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ComplianceRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgComplianceWorkspace>> FulfilAsync(
        string obligationId,
        decimal seqNo,
        DateTime fulfilledDate,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A fulfilment must record who made it.", ComplianceRules.CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the obligation cannot be fulfilled.",
                ComplianceRules.CodeNoConnection);
        }

        try
        {
            var workspace = await ReadAsync(source, null, DateTime.Today, cancellationToken).ConfigureAwait(false);
            var obligation = workspace.Obligations.FirstOrDefault(candidate =>
                string.Equals(candidate.Id, obligationId, StringComparison.OrdinalIgnoreCase) && candidate.SeqNo == seqNo);

            if (obligation is null)
            {
                return Failure($"Obligation '{obligationId}' is not in this database.",
                    ComplianceRules.CodeUnknownObligation);
            }

            // AC-4: no components, or a required component outstanding, is a refusal that names the reason.
            if (ComplianceRules.CanFulfil(obligation) is { } refusal)
            {
                return Failure(refusal.Message, refusal.Code);
            }

            var unit = OgUow.For<OBLIGATION>(source);
            unit.PrimaryKey = "OBLIGATION_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("OBLIGATION_ID", obligationId)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault(candidate => candidate.OBLIGATION_SEQ_NO == seqNo);

            if (row is null)
            {
                return Failure($"Obligation '{obligationId}' is not in this database.",
                    ComplianceRules.CodeUnknownObligation);
            }

            row.FULFILLED_IND = "Y";
            row.FULFILLED_DATE = fulfilledDate.Date;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The obligation's fulfilment was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The obligation's fulfilment was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            return await AfterAsync(source, null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"fulfilling obligation {obligationId}", "the obligation was not fulfilled",
                FailureSeverity.Error);

            return Failure(
                "The obligation could not be fulfilled" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ComplianceRules.CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgComplianceWorkspace>> WaiveAsync(
        string obligationId,
        decimal seqNo,
        string reason,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Failure("A waiver must record who made it.", ComplianceRules.CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            return Failure("Waiving an obligation needs its reason, which is the record of why it was not discharged.",
                ComplianceRules.CodeReasonRequired);
        }

        var source = _source();

        if (source is null)
        {
            return Failure("No connection is open, so the obligation cannot be waived.",
                ComplianceRules.CodeNoConnection);
        }

        try
        {
            var unit = OgUow.For<OBLIGATION>(source);
            unit.PrimaryKey = "OBLIGATION_ID";

            var rows = await OgUow.ReadAsync(
                unit,
                [OgUow.Equal("OBLIGATION_ID", obligationId)],
                cancellationToken).ConfigureAwait(false);

            var row = rows.FirstOrDefault(candidate => candidate.OBLIGATION_SEQ_NO == seqNo);

            if (row is null)
            {
                return Failure($"Obligation '{obligationId}' is not in this database.",
                    ComplianceRules.CodeUnknownObligation);
            }

            // The reason is its own record; the description is not overwritten with it.
            row.REMARK = ComplianceRules.WaiverPrefix + reason.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return Failure(OgUow.Message(updated, "The waiver was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return Failure(OgUow.Message(committed, "The waiver was not saved."),
                    ComplianceRules.CodeWriteFailed);
            }

            return await AfterAsync(source, null, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, $"waiving obligation {obligationId}", "the obligation was not waived", FailureSeverity.Error);

            return Failure(
                "The obligation could not be waived" +
                (string.IsNullOrWhiteSpace(reference) ? "." : " (reference " + reference + ")."),
                reference ?? ComplianceRules.CodeWriteFailed);
        }
    }

    public Task<OgResult<OgComplianceWorkspace>> RequestReportAsync(
        string obligationId,
        decimal seqNo,
        string actor,
        CancellationToken cancellationToken = default)
    {
        // AC-5: the honest answer. There is no process engine, and there is no regulator channel either.
        _ = obligationId;
        _ = seqNo;
        _ = actor;
        _ = cancellationToken;

        return Task.FromResult(OgResult<OgComplianceWorkspace>.Failed(
            "The internal report process cannot be started: the workflow engine (WFL-01/WFL-02) is not wired in this " +
            "build. No regulator delivery is established either - nothing is filed with a regulator from this screen.",
            ComplianceRules.CodeReportNotWired));
    }

    // ------------------------------------------------------------------ the read model

    private async Task<OgResult<OgComplianceWorkspace>> AfterAsync(
        IDataSource source,
        string? fieldId,
        CancellationToken cancellationToken)
    {
        var workspace = await ReadAsync(source, fieldId, DateTime.Today, cancellationToken).ConfigureAwait(false);

        return OgResult<OgComplianceWorkspace>.Ok(workspace);
    }

    private static async Task<OgComplianceWorkspace> ReadAsync(
        IDataSource source,
        string? fieldId,
        DateTime month,
        CancellationToken cancellationToken)
    {
        var obligations = await OgUow.ReadAllAsync<OBLIGATION>(source).ConfigureAwait(false);
        var components = await OgUow.ReadAllAsync<OBLIGATION_COMPONENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        var today = DateTime.Today;

        var componentsByObligation = components
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .GroupBy(row => (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO))
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(row => row.COMPONENT_OBS_NO)
                    .Select(row => new OgObligationComponent(
                        row.OBLIGATION_SEQ_NO,
                        row.COMPONENT_OBS_NO,
                        row.DESCRIPTION ?? string.Empty,
                        !string.Equals(row.REQUIRED_IND?.Trim(), "N", StringComparison.OrdinalIgnoreCase),
                        row.FULFILLED_DATE,
                        row.FULFILLED_REMARK))
                    .ToList());

        var mapped = obligations
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            .Where(row => string.IsNullOrWhiteSpace(fieldId) ||
                          string.Equals(row.FIELD_ID?.Trim(), fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(row =>
            {
                var key = (row.OBLIGATION_ID, row.OBLIGATION_SEQ_NO);
                var mine = componentsByObligation.TryGetValue(key, out var list) ? list : [];

                var fulfilled = string.Equals(row.FULFILLED_IND?.Trim(), "Y", StringComparison.OrdinalIgnoreCase) ||
                                row.FULFILLED_DATE is not null;

                var waived = row.REMARK?.StartsWith(ComplianceRules.WaiverPrefix, StringComparison.OrdinalIgnoreCase)
                             == true;

                var due = row.CRITICAL_DATE ?? row.EXPIRY_DATE;

                return new OgObligation(
                    row.OBLIGATION_ID,
                    row.OBLIGATION_SEQ_NO,
                    row.OBLIGATION_TYPE ?? string.Empty,
                    row.DESCRIPTION ?? row.WORK_OBLIGATION_DESC ?? string.Empty,
                    row.JURISDICTION ?? string.Empty,
                    due,
                    ComplianceRules.StateOf(fulfilled, waived, due, today),
                    mine);
            })
            .OrderBy(obligation => obligation.DueDate ?? DateTime.MaxValue)
            .ThenBy(obligation => obligation.Id, StringComparer.Ordinal)
            .ToList();

        return new OgComplianceWorkspace(
            fieldId,
            new DateTime(month.Year, month.Month, 1),
            mapped,
            ComplianceRules.CalendarOf(mapped, month, today),
            ComplianceRules.Kinds,
            DateTime.UtcNow);
    }

    private static OgResult<OgComplianceWorkspace> Failure(string message, string code) =>
        OgResult<OgComplianceWorkspace>.Failed(message, code);
}
