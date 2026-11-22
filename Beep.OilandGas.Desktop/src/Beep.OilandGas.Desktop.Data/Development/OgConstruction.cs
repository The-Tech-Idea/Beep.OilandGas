using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One evidence item against a construction milestone (DEV-08).</summary>
public sealed record OgMilestoneEvidence(
    string Id,
    string MilestoneId,
    string? Document,
    string? Reference,
    DateTime? EvidenceDate,
    string? Reviewer,
    string Outcome,
    string? AddedBy,
    DateTime? AddedAt)
{
    public const string Accepted = "ACCEPTED";
    public const string AcceptedWithNotes = "ACCEPTED_WITH_NOTES";
    public const string Rejected = "REJECTED";

    public static IReadOnlyList<string> Outcomes { get; } = [Accepted, AcceptedWithNotes, Rejected];

    public bool IsRejected => string.Equals(Outcome, Rejected, StringComparison.OrdinalIgnoreCase);

    public string OutcomeText => Outcome.ToLowerInvariant().Replace('_', ' ');

    public string Describe() =>
        $"{Document ?? "(no document)"}" +
        (string.IsNullOrWhiteSpace(Reference) ? string.Empty : $" · {Reference}") +
        $" · {OutcomeText} · reviewed by {Reviewer ?? "(unknown)"} on " +
        $"{EvidenceDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"}";

    public string EvidenceDateText =>
        EvidenceDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)";
}

/// <summary>One construction milestone with its evidence and its computed overdue answer (DEV-08 AC-2).</summary>
public sealed record OgConstructionMilestone(
    string Id,
    string FacilityInvId,
    string? FacilityId,
    string Name,
    int Sequence,
    DateTime? PlannedDate,
    DateTime? ActualDate,
    string State,
    string? Notes,
    IReadOnlyList<OgMilestoneEvidence> Evidence)
{
    public const string NotStarted = "NOT_STARTED";
    public const string InProgress = "IN_PROGRESS";
    public const string Complete = "COMPLETE";

    public bool IsComplete => string.Equals(State, Complete, StringComparison.OrdinalIgnoreCase);

    public string StateText => State.ToLowerInvariant().Replace('_', ' ');

    public string NameText => Name.Replace('_', ' ').ToLowerInvariant();

    /// <summary>The newest evidence item, which is the one the completion and unresolved rules read.</summary>
    public OgMilestoneEvidence? LatestEvidence => Evidence
        .OrderBy(item => item.EvidenceDate ?? DateTime.MinValue)
        .ThenBy(item => item.AddedAt ?? DateTime.MinValue)
        .ThenBy(item => item.Id, StringComparer.Ordinal)
        .LastOrDefault();

    public bool HasEvidence => Evidence.Count > 0;

    /// <summary>AC-2: overdue is computed from the planned date and whether the milestone is complete.</summary>
    public bool IsOverdue(DateTime asOf) => ConstructionRules.IsOverdue(PlannedDate, IsComplete, asOf);

    public int? DaysOverdue(DateTime asOf) => ConstructionRules.DaysOverdue(PlannedDate, IsComplete, asOf);

    public string PlanText(DateTime asOf)
    {
        var planned = PlannedDate is { } date
            ? $"planned {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : "no planned date";

        var actual = ActualDate is { } done
            ? $" · done {done.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : string.Empty;

        var overdue = DaysOverdue(asOf) is { } days
            ? $" · {days.ToString(CultureInfo.InvariantCulture)} d overdue"
            : string.Empty;

        return planned + actual + overdue;
    }

    /// <summary>AC-1: what a milestone still needs before it can complete.</summary>
    public IReadOnlyList<string> CompletionGaps => ConstructionRules.CompletionGaps(this);

    public bool CanComplete => CompletionGaps.Count == 0;

    public string CompletionText => CanComplete
        ? "ready to complete"
        : "cannot complete: " + string.Join("; ", CompletionGaps);

    public string Describe() =>
        $"{NameText} · {StateText} · {PlanText(DateTime.Today)} · " +
        $"{Evidence.Count.ToString(CultureInfo.InvariantCulture)} evidence item(s)";

    public string PlannedText =>
        PlannedDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(none)";

    public string ActualText =>
        ActualDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(not yet)";

    /// <summary>The grid's overdue column; the workspace computes with the as-of date it was loaded for.</summary>
    public string OverdueText => DaysOverdue(DateTime.Today) is { } days
        ? $"{days.ToString(CultureInfo.InvariantCulture)} d"
        : string.Empty;

    public string EvidenceText => Evidence.Count == 0
        ? "none"
        : $"{Evidence.Count.ToString(CultureInfo.InvariantCulture)} · " +
          (LatestEvidence?.OutcomeText ?? "no outcome");
}

/// <summary>One recorded equipment item of the facility (DEV-08 AC-5).</summary>
public sealed record OgEquipmentRecord(
    string EquipmentId,
    bool Active,
    DateTime? InstallDate,
    DateTime? RemoveDate,
    string? Use,
    string? Remark)
{
    public string StateText => Active ? "active" : "not active";

    public string Describe() =>
        $"{EquipmentId} · {StateText}" +
        (InstallDate is { } installed
            ? $" · installed {installed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : " · no install date") +
        (RemoveDate is { } removed
            ? $" · removed {removed.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}"
            : string.Empty) +
        (string.IsNullOrWhiteSpace(Use) ? string.Empty : $" · {Use}");

    public string InstallText =>
        InstallDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(no date)";

    public string RemoveText =>
        RemoveDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "(not removed)";
}

/// <summary>The handover of a facility to operations (DEV-08 AC-4), matching the plan's <c>OgFacilityHandover</c>.</summary>
public sealed record OgFacilityHandover(
    string Id,
    string FacilityInvId,
    string? FacilityId,
    string HandedOverTo,
    DateTime? HandedOverAt,
    IReadOnlyList<string> Conditions,
    string? Signature,
    string? SignedBy,
    DateTime? SignedAt)
{
    /// <summary>An acceptance record: the row exists only because the asset was accepted.</summary>
    public bool Accepted => true;

    public string ConditionsText => Conditions.Count == 0
        ? "no conditions"
        : string.Join("; ", Conditions);

    public string Describe() =>
        $"handed to {HandedOverTo} on " +
        $"{HandedOverAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} · " +
        $"{ConditionsText} · signed by {SignedBy ?? "(unknown)"} · " +
        $"signature {Signature ?? "(none)"}";
}

/// <summary>One facility with its milestones, evidence, equipment and handover (DEV-08).</summary>
public sealed record OgConstructionFacility(
    string Id,
    string? FacilityId,
    string? FieldId,
    string Name,
    string? Kind,
    string? Phase,
    string? PlanId,
    int PlanVersion,
    decimal? Capacity,
    string? CapacityUnit,
    decimal? Budget,
    decimal? Actual,
    string? Currency,
    DateTime? StartDate,
    DateTime? CompletionDate,
    IReadOnlyList<OgConstructionMilestone> Milestones,
    OgFacilityHandover? Handover,
    IReadOnlyList<OgEquipmentRecord> Equipment)
{
    public bool IsHandedOver => Handover is not null;

    public int CompletedMilestones => Milestones.Count(milestone => milestone.IsComplete);

    public int TotalMilestones => Milestones.Count;

    public string CompletionText => Milestones.Count == 0
        ? "no milestone is tracked"
        : $"{CompletedMilestones.ToString(CultureInfo.InvariantCulture)} of " +
          $"{TotalMilestones.ToString(CultureInfo.InvariantCulture)} milestone(s) complete";

    /// <summary>The state the facility list shows: handed over, or the first unfinished milestone.</summary>
    public string StateText
    {
        get
        {
            if (IsHandedOver)
            {
                return "handed over";
            }

            var next = Milestones
                .Where(milestone => !milestone.IsComplete)
                .OrderBy(milestone => milestone.Sequence)
                .FirstOrDefault();

            return next is null
                ? Milestones.Count == 0
                    ? "not tracked"
                    : "ready for handover"
                : next.NameText;
        }
    }

    public int OverdueCount(DateTime asOf) => Milestones.Count(milestone => milestone.IsOverdue(asOf));

    public string CapacityText => Capacity is { } capacity
        ? $"{capacity.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(CapacityUnit) ? "(unit not recorded)" : CapacityUnit!.Trim())
        : "no capacity recorded";

    public string BudgetText => Budget is { } budget
        ? $"{budget.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no budget recorded";

    /// <summary>AC-2 of DEV-04's sibling rule: a missing actual makes the spend explicitly partial.</summary>
    public string SpendText => Actual is { } actual
        ? $"{actual.ToString("#,0.###", CultureInfo.InvariantCulture)} " +
          (string.IsNullOrWhiteSpace(Currency) ? "(currency not recorded)" : Currency!.Trim())
        : "no actual spend recorded";

    public string VarianceText => Budget is { } budget && Actual is { } actual
        ? (actual - budget) switch
        {
            var variance when variance > 0m =>
                $"{variance.ToString("#,0.###", CultureInfo.InvariantCulture)} over budget",
            var variance when variance < 0m =>
                $"${(-variance).ToString("#,0.###", CultureInfo.InvariantCulture)} under budget",
            _ => "on budget",
        }
        : "variance not computable";

    /// <summary>AC-3: an unresolved prior submission hides the create and edit controls, and names itself.</summary>
    public string? UnresolvedReason => ConstructionRules.UnresolvedReason(Milestones);

    public bool CanEdit => !IsHandedOver && UnresolvedReason is null;

    /// <summary>Handover needs every milestone complete and nothing unresolved.</summary>
    public IReadOnlyList<string> HandoverGaps => ConstructionRules.HandoverGaps(this);

    public bool CanHandover => HandoverGaps.Count == 0;

    public string HandoverText => Handover is { } handover
        ? handover.Describe()
        : CanHandover
            ? "no handover recorded; the facility can be handed over"
            : "no handover recorded; " + string.Join("; ", HandoverGaps);

    /// <summary>AC-5: what recorded equipment does and does not prove.</summary>
    public string EquipmentText => Equipment.Count == 0
        ? "No equipment is recorded for this facility. " + ConstructionRules.EquipmentSentence
        : $"{Equipment.Count.ToString(CultureInfo.InvariantCulture)} equipment item(s) recorded. " +
          ConstructionRules.EquipmentSentence;

    public string Describe() =>
        $"{Name} · {Kind ?? "(no type)"} · {StateText} · {CompletionText} · " +
        $"{OverdueCount(DateTime.Today).ToString(CultureInfo.InvariantCulture)} overdue · " +
        $"budget {BudgetText} · spend {SpendText}";
}

/// <summary>
/// A well completion that DEV-07 accepted and handed to production, shown as the upstream record of this handover
/// (DEV-08's input). It is read only; nothing here is copied onto the facility handover.
/// </summary>
public sealed record OgUpstreamHandover(
    string WellId,
    string? AcceptedBy,
    DateTime? AcceptedAt,
    IReadOnlyList<string> Conditions)
{
    public string Describe() =>
        $"{WellId} accepted by {AcceptedBy ?? "(unknown)"} on " +
        $"{AcceptedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"}" +
        (Conditions.Count == 0 ? string.Empty : $" · {string.Join("; ", Conditions)}");
}

/// <summary>The construction workspace of one field.</summary>
public sealed record OgConstructionWorkspace(
    string FieldName,
    IReadOnlyList<OgConstructionFacility> Facilities,
    OgConstructionFacility? Selected,
    IReadOnlyList<OgUpstreamHandover> UpstreamHandovers,
    DateTime AsOf)
{
    public bool IsEmpty => Facilities.Count == 0;

    public int OverdueFacilities => Facilities.Count(facility => facility.OverdueCount(AsOf) > 0);

    public string Summary =>
        $"{Facilities.Count.ToString(CultureInfo.InvariantCulture)} facility/facilities · " +
        $"{Facilities.Count(facility => facility.IsHandedOver).ToString(CultureInfo.InvariantCulture)} handed over · " +
        $"{OverdueFacilities.ToString(CultureInfo.InvariantCulture)} with an overdue milestone";

    public string UpstreamText => UpstreamHandovers.Count == 0
        ? "No well completion has been accepted for this field yet, so no upstream handover is shown."
        : string.Join(" · ", UpstreamHandovers.Select(handover => handover.Describe()));
}

/// <summary>One milestone, as the page creates it.</summary>
public sealed record OgMilestoneValues(string? Name, DateTime? PlannedDate);

/// <summary>The date and note a milestone carries after an update.</summary>
public sealed record OgMilestoneUpdate(DateTime? PlannedDate, string? Notes);

/// <summary>One evidence item, as the page records it.</summary>
public sealed record OgMilestoneEvidenceValues(
    string? Document,
    string? Reference,
    DateTime? EvidenceDate,
    string? Reviewer,
    string? Outcome);

/// <summary>The handover, as the page records it.</summary>
public sealed record OgFacilityHandoverValues(
    string? HandedOverTo,
    DateTime? HandedOverAt,
    IReadOnlyList<string> Conditions,
    string? Signature,
    string? SignedBy);

/// <summary>The shared construction rules, stated once so the page, the service and the tests agree.</summary>
public static class ConstructionRules
{
    public const string CodeMilestoneNameRequired = "construction-milestone-name-required";
    public const string CodeMilestoneNameInvalid = "construction-milestone-name-invalid";
    public const string CodeEvidenceRequired = "construction-evidence-required";
    public const string CodeOutcomeInvalid = "construction-evidence-outcome-invalid";
    public const string CodeReviewerRequired = "construction-evidence-reviewer-required";
    public const string CodeNoEvidence = "construction-milestone-no-evidence";
    public const string CodeUnresolved = "construction-unresolved";
    public const string CodeRecipientRequired = "construction-handover-recipient-required";
    public const string CodeSignatureRequired = "construction-handover-signature-required";
    public const string CodeHandoverDateRequired = "construction-handover-date-required";
    public const string CodeMilestonesIncomplete = "construction-handover-milestones-incomplete";

    /// <summary>The tracker order: design, procurement, fabrication, installation, commissioning, handover.</summary>
    public static IReadOnlyList<string> MilestoneNames { get; } =
        ["DESIGN", "PROCUREMENT", "FABRICATION", "INSTALLATION", "COMMISSIONING", "HANDOVER"];

    /// <summary>
    /// AC-5: the sentence the feature document tells this screen to keep. Recorded equipment is a record of what is
    /// installed, not proof that it was installed, tested or accepted.
    /// </summary>
    public const string EquipmentSentence =
        "Recorded equipment shows what is listed for the facility; it is not evidence that the equipment was installed, " +
        "tested or accepted.";

    public static int SequenceOf(string name)
    {
        var index = MilestoneNames.ToList().FindIndex(candidate =>
            string.Equals(candidate, (name ?? string.Empty).Trim(), StringComparison.OrdinalIgnoreCase));

        return index < 0 ? 0 : index + 1;
    }

    /// <summary>AC-2: a past planned date on an incomplete milestone is overdue, and the days are computed.</summary>
    public static bool IsOverdue(DateTime? planned, bool isComplete, DateTime asOf) =>
        planned is { } date && !isComplete && date.Date < asOf.Date;

    public static int? DaysOverdue(DateTime? planned, bool isComplete, DateTime asOf) =>
        IsOverdue(planned, isComplete, asOf) ? (asOf.Date - planned!.Value.Date).Days : null;

    public static (string Code, string Message)? ValidateMilestone(OgMilestoneValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.Name))
        {
            return (CodeMilestoneNameRequired,
                $"A milestone needs a name; use {string.Join(", ", MilestoneNames)}.");
        }

        return SequenceOf(values.Name) == 0
            ? (CodeMilestoneNameInvalid,
                $"'{values.Name}' is not a construction milestone; use {string.Join(", ", MilestoneNames)}.")
            : null;
    }

    public static (string Code, string Message)? ValidateEvidence(OgMilestoneEvidenceValues values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(values.Document) && string.IsNullOrWhiteSpace(values.Reference))
        {
            return (CodeEvidenceRequired, "Evidence needs a document or a reference.");
        }

        var outcome = (values.Outcome ?? string.Empty).Trim().ToUpperInvariant();

        if (!OgMilestoneEvidence.Outcomes.Contains(outcome, StringComparer.Ordinal))
        {
            return (CodeOutcomeInvalid,
                $"'{values.Outcome}' is not an evidence outcome; use {string.Join(", ", OgMilestoneEvidence.Outcomes)}.");
        }

        return string.IsNullOrWhiteSpace(values.Reviewer)
            ? (CodeReviewerRequired, "Evidence must record who reviewed it.")
            : null;
    }

    /// <summary>AC-1: a milestone completes on evidence; the newest outcome must not be a rejection.</summary>
    public static IReadOnlyList<string> CompletionGaps(OgConstructionMilestone milestone)
    {
        ArgumentNullException.ThrowIfNull(milestone);

        if (milestone.Evidence.Count == 0)
        {
            return ["no evidence is recorded, so the milestone cannot complete"];
        }

        return milestone.LatestEvidence is { IsRejected: true } latest
            ? [$"the latest evidence ({latest.Describe()}) was rejected, so the milestone cannot complete"]
            : [];
    }

    /// <summary>
    /// AC-3: the facility has an unresolved prior submission when any milestone's newest evidence was rejected. The
    /// feature document states the rule ("an unresolved original hides create and edit") without defining the original,
    /// so this screen treats the rejected evidence item as the unresolved submission and names it.
    /// </summary>
    public static string? UnresolvedReason(IReadOnlyList<OgConstructionMilestone> milestones)
    {
        ArgumentNullException.ThrowIfNull(milestones);

        var rejected = milestones
            .Select(milestone => milestone.LatestEvidence)
            .Where(latest => latest is { IsRejected: true })
            .Select(latest => latest!)
            .OrderBy(latest => latest.EvidenceDate ?? DateTime.MinValue)
            .ThenBy(latest => latest.Id, StringComparer.Ordinal)
            .FirstOrDefault();

        return rejected is null
            ? null
            : $"An evidence submission is unresolved: {rejected.Describe()}. Resolve it before changing the facility.";
    }

    public static IReadOnlyList<string> HandoverGaps(OgConstructionFacility facility)
    {
        ArgumentNullException.ThrowIfNull(facility);

        if (facility.IsHandedOver)
        {
            return ["the facility was already handed over"];
        }

        if (facility.Milestones.Count == 0)
        {
            return ["no milestone is tracked, so there is nothing to hand over"];
        }

        var unfinished = facility.Milestones
            .Where(milestone => !milestone.IsComplete)
            .OrderBy(milestone => milestone.Sequence)
            .Select(milestone => milestone.NameText)
            .ToList();

        if (unfinished.Count > 0)
        {
            return ["these milestones are not complete: " + string.Join(", ", unfinished)];
        }

        return facility.UnresolvedReason is { } unresolved ? [unresolved] : [];
    }

    /// <summary>
    /// AC-6: the version the caller saw must still be the facility's version. A null expectation is not checked, so a
    /// caller that does not hold a snapshot is not forced to invent one.
    /// </summary>
    public static string? VersionRefusal(int currentVersion, int? expectedVersion) =>
        expectedVersion is { } expected && expected != currentVersion
            ? $"the facility moved from plan version {expected.ToString(CultureInfo.InvariantCulture)} to " +
              $"{currentVersion.ToString(CultureInfo.InvariantCulture)} since this screen read it"
            : null;

    public static (string Code, string Message)? ValidateHandover(
        OgConstructionFacility facility,
        OgFacilityHandoverValues values)
    {
        ArgumentNullException.ThrowIfNull(facility);
        ArgumentNullException.ThrowIfNull(values);

        if (facility.HandoverGaps.Count > 0)
        {
            return facility.IsHandedOver
                ? ("construction-handover-already-recorded", "This facility already has a handover record.")
                : facility.Milestones.All(milestone => milestone.IsComplete) && facility.Milestones.Count > 0
                    ? (CodeUnresolved, string.Join("; ", facility.HandoverGaps))
                    : (CodeMilestonesIncomplete, string.Join("; ", facility.HandoverGaps));
        }

        if (string.IsNullOrWhiteSpace(values.HandedOverTo))
        {
            return (CodeRecipientRequired, "A handover records who the asset is handed to.");
        }

        if (values.HandedOverAt is null)
        {
            return (CodeHandoverDateRequired, "A handover records the date it happened.");
        }

        return string.IsNullOrWhiteSpace(values.Signature)
            ? (CodeSignatureRequired, "A handover records the acceptance signature it was signed under.")
            : null;
    }
}

/// <summary>DEV-08: facility construction progress and the handover to operations.</summary>
public interface IOgConstruction
{
    Task<OgResult<OgConstructionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? facilityInvId = null,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates one milestone of the facility's tracker. The plan version the caller saw is revalidated before the write
    /// (AC-6): a facility that moved on is refused rather than written to.
    /// </summary>
    Task<OgResult<OgConstructionWorkspace>> CreateMilestoneAsync(
        string facilityInvId,
        OgMilestoneValues values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>Updates a milestone's planned date and note.</summary>
    Task<OgResult<OgConstructionWorkspace>> UpdateMilestoneAsync(
        string milestoneId,
        OgMilestoneUpdate values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgConstructionWorkspace>> AddEvidenceAsync(
        string milestoneId,
        OgMilestoneEvidenceValues values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>AC-1: completes a milestone on evidence, never without it.</summary>
    Task<OgResult<OgConstructionWorkspace>> CompleteMilestoneAsync(
        string milestoneId,
        DateTime? actualDate,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default);

    /// <summary>AC-4: records the handover - recipient, date, conditions and signature.</summary>
    Task<OgResult<OgConstructionWorkspace>> HandoverAsync(
        string facilityInvId,
        OgFacilityHandoverValues values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Construction over the DevelopmentPlanning-owned <c>FACILITY_INVESTMENT</c>, <c>FACILITY_MILESTONE</c>,
/// <c>FACILITY_MILESTONE_EVIDENCE</c> and <c>FACILITY_HANDOVER</c>, the PPDM <c>FACILITY</c> and
/// <c>FACILITY_EQUIPMENT</c> rows the facility refers to, and the DEV-07 accepted completions of the field, all through
/// their entity models and a unit of work.
/// </summary>
/// <remarks>
/// Every write re-reads the facility before it starts and again after it commits, and refuses if the facility it was
/// given is not the facility it would write to (AC-6). Recorded equipment is read and shown, and the screen states what
/// it does not prove (AC-5).
/// </remarks>
public sealed class OgConstructionService : IOgConstruction
{
    public const string CodeNoConnection = "construction-no-connection";
    public const string CodeReadFailed = "construction-read-failed";
    public const string CodeWriteFailed = "construction-write-failed";
    public const string CodeActorRequired = "construction-actor-required";
    public const string CodeFacilityRequired = "construction-facility-required";
    public const string CodeFacilityUnknown = "construction-facility-unknown";
    public const string CodeFacilityForeign = "construction-facility-foreign";
    public const string CodeFacilityChanged = "construction-facility-changed";
    public const string CodeMilestoneUnknown = "construction-milestone-unknown";
    public const string CodeMilestoneExists = "construction-milestone-exists";
    public const string CodeAlreadyComplete = "construction-milestone-already-complete";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgConstructionService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgConstructionWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        string? facilityInvId = null,
        DateTime? asOf = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "No connection is open, so construction progress cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var date = (asOf ?? DateTime.Today).Date;

            var facilities = model.Investments
                .Where(row => FieldMatches(row.FIELD_ID, fieldId, fieldName))
                .OrderBy(row => row.FACILITY_NAME, StringComparer.OrdinalIgnoreCase)
                .ThenBy(row => row.FACILITY_INV_ID, StringComparer.Ordinal)
                .Select(row => Build(row, model, date))
                .ToList();

            var selected = string.IsNullOrWhiteSpace(facilityInvId)
                ? facilities.FirstOrDefault()
                : facilities.FirstOrDefault(facility =>
                    string.Equals(facility.Id, facilityInvId.Trim(), StringComparison.OrdinalIgnoreCase));

            return OgResult<OgConstructionWorkspace>.Ok(new OgConstructionWorkspace(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                facilities,
                selected,
                Upstream(model),
                date));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the facility construction progress",
                "construction progress is not shown",
                FailureSeverity.Error);

            return OgResult<OgConstructionWorkspace>.Failed(
                "The construction progress could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgConstructionWorkspace>> CreateMilestoneAsync(
        string facilityInvId,
        OgMilestoneValues values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "A milestone change must record who made it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(facilityInvId))
        {
            return OgResult<OgConstructionWorkspace>.Failed("Choose the facility first.", CodeFacilityRequired);
        }

        if (ConstructionRules.ValidateMilestone(values) is { } refusal)
        {
            return OgResult<OgConstructionWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "No connection is open, so the milestone cannot be created.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var facility = model.Investments.FirstOrDefault(row =>
                string.Equals(row.FACILITY_INV_ID, facilityInvId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (facility is null)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"Facility '{facilityInvId}' is not in this database.", CodeFacilityUnknown);
            }

            // AC-6 (before the write): the facility the caller named must still be the facility it saw.
            if (ConstructionRules.VersionRefusal(facility.PLAN_VERSION, expectedPlanVersion) is { } moved)
            {
                return Moved(facilityInvId, moved);
            }

            var name = values.Name!.Trim().ToUpperInvariant();
            var sequence = ConstructionRules.SequenceOf(name);

            if (model.Milestones.Any(row =>
                    string.Equals(row.FACILITY_INV_ID, facility.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(row.MILESTONE_NAME, name, StringComparison.OrdinalIgnoreCase)))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"Facility '{facility.FACILITY_NAME}' already has a {name.ToLowerInvariant()} milestone.",
                    OgConstructionService.CodeMilestoneExists);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<FACILITY_MILESTONE>(source);
            unit.PrimaryKey = "MILESTONE_ID";

            unit.Add(new FACILITY_MILESTONE
            {
                MILESTONE_ID = Guid.NewGuid().ToString("N"),
                FACILITY_INV_ID = facility.FACILITY_INV_ID!,
                FACILITY_ID = facility.FACILITY_ID,
                FDP_ID = facility.FDP_ID,
                FIELD_ID = facility.FIELD_ID,
                MILESTONE_NAME = name,
                MILESTONE_SEQ = sequence,
                PLANNED_DATE = values.PlannedDate?.Date,
                STATE = OgConstructionMilestone.NotStarted,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(committed, "The milestone was not created."), CodeWriteFailed);
            }

            return await ReloadAsync(source, facility, null, actor, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating a milestone on facility {facilityInvId}",
                "the milestone was not created",
                FailureSeverity.Error);

            return OgResult<OgConstructionWorkspace>.Failed(
                "The milestone could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgConstructionWorkspace>> UpdateMilestoneAsync(
        string milestoneId,
        OgMilestoneUpdate values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "A milestone change must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "No connection is open, so the milestone cannot be saved.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var milestone = model.Milestones.FirstOrDefault(row =>
                string.Equals(row.MILESTONE_ID, milestoneId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (milestone is null)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"Milestone '{milestoneId}' is not in this database.", CodeMilestoneUnknown);
            }

            var facility = model.Investments.FirstOrDefault(row =>
                string.Equals(row.FACILITY_INV_ID, milestone.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase));

            if (facility is null)
            {
                return Changed(milestone.FACILITY_INV_ID);
            }

            if (ConstructionRules.VersionRefusal(facility.PLAN_VERSION, expectedPlanVersion) is { } moved)
            {
                return Moved(facility.FACILITY_INV_ID!, moved);
            }

            if (IsHandedOver(model, facility.FACILITY_INV_ID!))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    "This facility was handed over; its milestones are closed.",
                    "construction-handover-already-recorded");
            }

            var unit = OgUow.For<FACILITY_MILESTONE>(source);
            unit.PrimaryKey = "MILESTONE_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("MILESTONE_ID", milestone.MILESTONE_ID)],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return Changed(milestone.FACILITY_INV_ID);
            }

            row.PLANNED_DATE = values.PlannedDate?.Date ?? row.PLANNED_DATE;
            row.NOTES = Blank(values.Notes) ?? row.NOTES;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(updated, "The milestone was not saved."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(committed, "The milestone was not saved."), CodeWriteFailed);
            }

            return await ReloadAsync(source, facility, null, actor, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving milestone {milestoneId}",
                "the milestone was not saved",
                FailureSeverity.Error);

            return OgResult<OgConstructionWorkspace>.Failed(
                "The milestone could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgConstructionWorkspace>> AddEvidenceAsync(
        string milestoneId,
        OgMilestoneEvidenceValues values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "Evidence must record who added it.", CodeActorRequired);
        }

        if (ConstructionRules.ValidateEvidence(values) is { } refusal)
        {
            return OgResult<OgConstructionWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "No connection is open, so the evidence cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var milestone = model.Milestones.FirstOrDefault(row =>
                string.Equals(row.MILESTONE_ID, milestoneId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (milestone is null)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"Milestone '{milestoneId}' is not in this database.", CodeMilestoneUnknown);
            }

            var facility = model.Investments.FirstOrDefault(row =>
                string.Equals(row.FACILITY_INV_ID, milestone.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase));

            if (facility is null)
            {
                return Changed(milestone.FACILITY_INV_ID);
            }

            if (ConstructionRules.VersionRefusal(facility.PLAN_VERSION, expectedPlanVersion) is { } moved)
            {
                return Moved(facility.FACILITY_INV_ID!, moved);
            }

            if (IsHandedOver(model, facility.FACILITY_INV_ID!))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    "This facility was handed over; its evidence is closed.",
                    "construction-handover-already-recorded");
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<FACILITY_MILESTONE_EVIDENCE>(source);
            unit.PrimaryKey = "EVIDENCE_ID";

            unit.Add(new FACILITY_MILESTONE_EVIDENCE
            {
                EVIDENCE_ID = Guid.NewGuid().ToString("N"),
                MILESTONE_ID = milestone.MILESTONE_ID!,
                FACILITY_INV_ID = milestone.FACILITY_INV_ID!,
                DOCUMENT = Blank(values.Document),
                REFERENCE = Blank(values.Reference),
                EVIDENCE_DATE = values.EvidenceDate?.Date ?? now.Date,
                REVIEWER = (values.Reviewer ?? string.Empty).Trim(),
                OUTCOME = values.Outcome!.Trim().ToUpperInvariant(),
                ADDED_BY = actor.Trim(),
                ADDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(committed, "The evidence was not recorded."), CodeWriteFailed);
            }

            return await ReloadAsync(source, facility, null, actor, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording evidence on milestone {milestoneId}",
                "the evidence was not recorded",
                FailureSeverity.Error);

            return OgResult<OgConstructionWorkspace>.Failed(
                "The evidence could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgConstructionWorkspace>> CompleteMilestoneAsync(
        string milestoneId,
        DateTime? actualDate,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "Completing a milestone must record who did it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "No connection is open, so the milestone cannot be completed.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var milestoneRow = model.Milestones.FirstOrDefault(row =>
                string.Equals(row.MILESTONE_ID, milestoneId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (milestoneRow is null)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"Milestone '{milestoneId}' is not in this database.", CodeMilestoneUnknown);
            }

            var facility = model.Investments.FirstOrDefault(row =>
                string.Equals(row.FACILITY_INV_ID, milestoneRow.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase));

            if (facility is null)
            {
                return Changed(milestoneRow.FACILITY_INV_ID);
            }

            if (ConstructionRules.VersionRefusal(facility.PLAN_VERSION, expectedPlanVersion) is { } moved)
            {
                return Moved(facility.FACILITY_INV_ID!, moved);
            }

            if (IsHandedOver(model, facility.FACILITY_INV_ID!))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    "This facility was handed over; its milestones are closed.",
                    "construction-handover-already-recorded");
            }

            var milestone = ToMilestone(milestoneRow, model);

            if (milestone.IsComplete)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"The {milestone.NameText} milestone is already complete.", CodeAlreadyComplete);
            }

            // AC-1: no evidence, or a rejected newest outcome, means it cannot complete.
            var gaps = milestone.CompletionGaps;

            if (gaps.Count > 0)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"The {milestone.NameText} milestone cannot complete: {string.Join("; ", gaps)}.",
                    milestone.HasEvidence ? ConstructionRules.CodeUnresolved : ConstructionRules.CodeNoEvidence);
            }

            var now = DateTime.UtcNow;
            var unit = OgUow.For<FACILITY_MILESTONE>(source);
            unit.PrimaryKey = "MILESTONE_ID";

            var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("MILESTONE_ID", milestoneRow.MILESTONE_ID)],
                cancellationToken).ConfigureAwait(false);
            var row = rows.FirstOrDefault();

            if (row is null)
            {
                return Changed(milestoneRow.FACILITY_INV_ID);
            }

            row.STATE = OgConstructionMilestone.Complete;
            row.ACTUAL_DATE = (actualDate ?? now).Date;
            row.ACTOR = actor.Trim();
            row.RECORDED_AT = now;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var updated = unit.Update(row);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(updated, "The milestone was not completed."), CodeWriteFailed);
            }

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(committed, "The milestone was not completed."), CodeWriteFailed);
            }

            return await ReloadAsync(source, facility, null, actor, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"completing milestone {milestoneId}",
                "the milestone was not completed",
                FailureSeverity.Error);

            return OgResult<OgConstructionWorkspace>.Failed(
                "The milestone could not be completed.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgConstructionWorkspace>> HandoverAsync(
        string facilityInvId,
        OgFacilityHandoverValues values,
        string actor,
        int? expectedPlanVersion = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "A handover must record who recorded it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(facilityInvId))
        {
            return OgResult<OgConstructionWorkspace>.Failed("Choose the facility first.", CodeFacilityRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgConstructionWorkspace>.Failed(
                "No connection is open, so the handover cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var facilityRow = model.Investments.FirstOrDefault(row =>
                string.Equals(row.FACILITY_INV_ID, facilityInvId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (facilityRow is null)
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    $"Facility '{facilityInvId}' is not in this database.", CodeFacilityUnknown);
            }

            // AC-6 (before the write): the facility must be the plan version the caller saw.
            if (ConstructionRules.VersionRefusal(facilityRow.PLAN_VERSION, expectedPlanVersion) is { } moved)
            {
                return Moved(facilityRow.FACILITY_INV_ID!, moved);
            }

            var facility = Build(facilityRow, model, DateTime.Today);

            if (ConstructionRules.ValidateHandover(facility, values) is { } refusal)
            {
                return OgResult<OgConstructionWorkspace>.Failed(refusal.Message, refusal.Code);
            }

            var conditions = (values.Conditions ?? [])
                .Where(condition => !string.IsNullOrWhiteSpace(condition))
                .Select(condition => condition.Trim())
                .ToList();

            var now = DateTime.UtcNow;
            var unit = OgUow.For<FACILITY_HANDOVER>(source);
            unit.PrimaryKey = "HANDOVER_ID";

            unit.Add(new FACILITY_HANDOVER
            {
                HANDOVER_ID = Guid.NewGuid().ToString("N"),
                FACILITY_INV_ID = facilityRow.FACILITY_INV_ID!,
                FACILITY_ID = facilityRow.FACILITY_ID,
                FDP_ID = facilityRow.FDP_ID,
                FIELD_ID = facilityRow.FIELD_ID,
                HANDED_OVER_TO = values.HandedOverTo!.Trim(),
                HANDED_OVER_AT = values.HandedOverAt!.Value.Date,
                CONDITIONS_TEXT = conditions.Count == 0 ? null : string.Join("\n", conditions),
                SIGNATURE = Blank(values.Signature),
                SIGNED_BY = Blank(values.SignedBy)?.Trim() ?? actor.Trim(),
                SIGNED_AT = now,
                ACTOR = actor.Trim(),
                RECORDED_AT = now,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgConstructionWorkspace>.Failed(
                    OgUow.Message(committed, "The handover was not recorded."), CodeWriteFailed);
            }

            return await ReloadAsync(source, facilityRow, null, actor, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"handing over facility {facilityInvId}",
                "the handover was not recorded",
                FailureSeverity.Error);

            return OgResult<OgConstructionWorkspace>.Failed(
                "The handover could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ reads

    private static async Task<ConstructionModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var plans = await OgUow.ReadAllAsync<FIELD_DEVELOPMENT_PLAN>(source).ConfigureAwait(false);
        var investments = await OgUow.ReadAllAsync<FACILITY_INVESTMENT>(source).ConfigureAwait(false);
        var milestones = await OgUow.ReadAllAsync<FACILITY_MILESTONE>(source).ConfigureAwait(false);
        var evidence = await OgUow.ReadAllAsync<FACILITY_MILESTONE_EVIDENCE>(source).ConfigureAwait(false);
        var handovers = await OgUow.ReadAllAsync<FACILITY_HANDOVER>(source).ConfigureAwait(false);
        var completions = await OgUow.ReadAllAsync<DEVELOPMENT_COMPLETION_SUBMISSION>(source)
            .ConfigureAwait(false);
        var equipment = await OgUow.ReadAllAsync<FACILITY_EQUIPMENT>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new ConstructionModel(
            plans, investments, milestones, evidence, handovers, completions, equipment);
    }

    /// <summary>After a write: the facility must still be the same facility, or the result is not reported as success.</summary>
    private async Task<OgResult<OgConstructionWorkspace>> ReloadAsync(
        IDataSource source,
        FACILITY_INVESTMENT written,
        string? selectedMilestoneId,
        string actor,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);

        var again = model.Investments.FirstOrDefault(row =>
            string.Equals(row.FACILITY_INV_ID, written.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase));

        // AC-6 (after the write): the row must still exist and still belong to the same plan and field.
        if (again is null
            || !string.Equals(again.FDP_ID, written.FDP_ID, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(again.FIELD_ID, written.FIELD_ID, StringComparison.OrdinalIgnoreCase))
        {
            _failures.ReportHandled(
                new InvalidOperationException(
                    $"Facility {written.FACILITY_INV_ID} changed while a construction write was being committed " +
                    $"({actor})."),
                "revalidating the facility after a construction write",
                "the write may have landed on a facility that has since changed; the result is not reported as success",
                FailureSeverity.Error);

            return Changed(written.FACILITY_INV_ID ?? string.Empty);
        }

        var facility = Build(again, model, DateTime.Today);
        _ = selectedMilestoneId;

        return OgResult<OgConstructionWorkspace>.Ok(new OgConstructionWorkspace(
            string.IsNullOrWhiteSpace(again.FIELD_ID) ? "(no field)" : again.FIELD_ID!,
            [facility],
            facility,
            Upstream(model),
            DateTime.Today));
    }

    private static OgResult<OgConstructionWorkspace> Changed(string facilityInvId) =>
        OgResult<OgConstructionWorkspace>.Failed(
            $"Facility '{facilityInvId}' changed while this screen held it; reopen the page and try again.",
            CodeFacilityChanged);

    private static OgResult<OgConstructionWorkspace> Moved(string facilityInvId, string reason) =>
        OgResult<OgConstructionWorkspace>.Failed(
            $"Facility '{facilityInvId}' was not written to: {reason}. Reopen the page and try again.",
            CodeFacilityChanged);

    private static OgConstructionFacility Build(
        FACILITY_INVESTMENT investment,
        ConstructionModel model,
        DateTime asOf)
    {
        var milestones = model.Milestones
            .Where(row => string.Equals(row.FACILITY_INV_ID, investment.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.MILESTONE_SEQ)
            .ThenBy(row => row.MILESTONE_NAME, StringComparer.OrdinalIgnoreCase)
            .Select(row => ToMilestone(row, model))
            .ToList();

        var handover = model.Handovers
            .Where(row => string.Equals(row.FACILITY_INV_ID, investment.FACILITY_INV_ID, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(row => row.HANDED_OVER_AT)
            .Select(row => new OgFacilityHandover(
                row.HANDOVER_ID ?? string.Empty,
                row.FACILITY_INV_ID ?? string.Empty,
                Blank(row.FACILITY_ID),
                Blank(row.HANDED_OVER_TO) ?? "(no recipient)",
                row.HANDED_OVER_AT,
                Split(row.CONDITIONS_TEXT),
                Blank(row.SIGNATURE),
                Blank(row.SIGNED_BY),
                row.SIGNED_AT))
            .FirstOrDefault();

        var equipment = string.IsNullOrWhiteSpace(investment.FACILITY_ID)
            ? []
            : model.Equipment
                .Where(row => string.Equals(row.FACILITY_ID, investment.FACILITY_ID, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => string.Equals(row.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase))
                .ThenBy(row => row.EQUIPMENT_ID, StringComparer.OrdinalIgnoreCase)
                .Select(row => new OgEquipmentRecord(
                    Blank(row.EQUIPMENT_ID) ?? "(no equipment id)",
                    string.Equals(row.ACTIVE_IND, "Y", StringComparison.OrdinalIgnoreCase),
                    row.INSTALL_DATE,
                    row.REMOVE_DATE,
                    Blank(row.USE_DESCRIPTION),
                    Blank(row.REMARK)))
                .ToList();

        _ = asOf;

        return new OgConstructionFacility(
            investment.FACILITY_INV_ID ?? string.Empty,
            Blank(investment.FACILITY_ID),
            Blank(investment.FIELD_ID),
            Blank(investment.FACILITY_NAME) ?? "(unnamed facility)",
            Blank(investment.FACILITY_TYPE),
            Blank(investment.INVESTMENT_PHASE),
            Blank(investment.FDP_ID),
            investment.PLAN_VERSION,
            investment.CAPACITY_PLANNED_BOPD is > 0m ? investment.CAPACITY_PLANNED_BOPD : null,
            Blank(investment.CAPACITY_OUOM),
            investment.CAPEX_PLANNED_MM is > 0m ? investment.CAPEX_PLANNED_MM : null,
            investment.CAPEX_ACTUAL_MM is > 0m ? investment.CAPEX_ACTUAL_MM : null,
            Blank(investment.COST_CURRENCY),
            investment.PLANNED_START_DATE,
            investment.PLANNED_COMPLETION_DATE,
            milestones,
            handover,
            equipment);
    }

    private static OgConstructionMilestone ToMilestone(FACILITY_MILESTONE row, ConstructionModel model)
    {
        var evidence = model.Evidence
            .Where(item => string.Equals(item.MILESTONE_ID, row.MILESTONE_ID, StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.EVIDENCE_DATE)
            .ThenBy(item => item.ADDED_AT)
            .Select(item => new OgMilestoneEvidence(
                item.EVIDENCE_ID ?? string.Empty,
                item.MILESTONE_ID ?? string.Empty,
                Blank(item.DOCUMENT),
                Blank(item.REFERENCE),
                item.EVIDENCE_DATE,
                Blank(item.REVIEWER),
                Blank(item.OUTCOME) ?? string.Empty,
                Blank(item.ADDED_BY),
                item.ADDED_AT))
            .ToList();

        return new OgConstructionMilestone(
            row.MILESTONE_ID ?? string.Empty,
            row.FACILITY_INV_ID ?? string.Empty,
            Blank(row.FACILITY_ID),
            Blank(row.MILESTONE_NAME) ?? "(unnamed milestone)",
            row.MILESTONE_SEQ,
            row.PLANNED_DATE,
            row.ACTUAL_DATE,
            Blank(row.STATE) ?? OgConstructionMilestone.NotStarted,
            Blank(row.NOTES),
            evidence);
    }

    /// <summary>DEV-07's accepted completions, read as this screen's upstream record.</summary>
    private static IReadOnlyList<OgUpstreamHandover> Upstream(ConstructionModel model) =>
        model.Completions
            .Where(row => !string.IsNullOrWhiteSpace(row.HANDOVER_TO))
            .Where(row => row.STATE is not null
                          && (string.Equals(row.STATE, "ACCEPTED", StringComparison.OrdinalIgnoreCase)
                              || string.Equals(row.STATE, "ACCEPTED_WITH_CONDITIONS", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(row => row.WELL_ID, StringComparer.OrdinalIgnoreCase)
            .Select(row => new OgUpstreamHandover(
                row.WELL_ID,
                Blank(row.ACCEPTED_BY),
                row.ACCEPTED_AT,
                Split(row.CONDITIONS_TEXT)))
            .ToList();

    private static bool IsHandedOver(ConstructionModel model, string facilityInvId) =>
        model.Handovers.Any(row =>
            string.Equals(row.FACILITY_INV_ID, facilityInvId, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<string> Split(string? text) =>
        string.IsNullOrWhiteSpace(text)
            ? []
            : text.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool FieldMatches(string? value, string? fieldId, string? fieldName) =>
        (string.IsNullOrWhiteSpace(fieldId) && string.IsNullOrWhiteSpace(fieldName))
        || (!string.IsNullOrWhiteSpace(fieldId)
            && string.Equals(value ?? string.Empty, fieldId.Trim(), StringComparison.OrdinalIgnoreCase))
        || (!string.IsNullOrWhiteSpace(fieldName)
            && string.Equals(value ?? string.Empty, fieldName.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string? Blank(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record ConstructionModel(
        IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Plans,
        IReadOnlyList<FACILITY_INVESTMENT> Investments,
        IReadOnlyList<FACILITY_MILESTONE> Milestones,
        IReadOnlyList<FACILITY_MILESTONE_EVIDENCE> Evidence,
        IReadOnlyList<FACILITY_HANDOVER> Handovers,
        IReadOnlyList<DEVELOPMENT_COMPLETION_SUBMISSION> Completions,
        IReadOnlyList<FACILITY_EQUIPMENT> Equipment);
}
