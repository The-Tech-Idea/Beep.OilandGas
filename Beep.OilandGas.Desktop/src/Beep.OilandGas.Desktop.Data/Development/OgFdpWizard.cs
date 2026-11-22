using System.Globalization;
using Beep.OilandGas.Models.Data.DevelopmentPlanning;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Development;

/// <summary>One editable field-development-plan draft (DEV-02).</summary>
public sealed record OgFdpDraft(
    string Id,
    string FdpId,
    string FieldId,
    int Version,
    string State,
    int CurrentStep,
    string? Author,
    string? Scope,
    string? Reservoir,
    int WellCount,
    string? Facility,
    decimal CostAmount,
    string? CostCurrency,
    string? Risk,
    string? BaselineId,
    int? ReturnedStep,
    string? ReturnReason)
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Returned = "RETURNED";
    public const string Approved = "APPROVED";
    public const string ApprovedWithConditions = "APPROVED_WITH_CONDITIONS";
    public const string Rejected = "REJECTED";

    public bool IsEditable =>
        string.Equals(State, Draft, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, Returned, StringComparison.OrdinalIgnoreCase);

    public bool IsSubmitted => string.Equals(State, Submitted, StringComparison.OrdinalIgnoreCase);

    public bool IsTerminal =>
        string.Equals(State, Approved, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, ApprovedWithConditions, StringComparison.OrdinalIgnoreCase)
        || string.Equals(State, Rejected, StringComparison.OrdinalIgnoreCase);

    public string StateText => State.ToLowerInvariant();

    public string CostText => CostAmount.ToString("#,0.###", CultureInfo.InvariantCulture) + " " +
                             (string.IsNullOrWhiteSpace(CostCurrency) ? "(currency not recorded)" : CostCurrency!.Trim());

    public string StepText => OgFdpWizardService.StepName(CurrentStep);

    /// <summary>AC-3: the return reason names the step the author must correct.</summary>
    public string? ReturnText => ReturnedStep is { } step
        ? $"Step {step.ToString(CultureInfo.InvariantCulture)} ({OgFdpWizardService.StepName(step)}): {ReturnReason ?? "correct the step"}"
        : null;

    public string Describe() =>
        $"{Id} v{Version.ToString(CultureInfo.InvariantCulture)} · {StateText} · step {CurrentStep.ToString(CultureInfo.InvariantCulture)} " +
        $"({OgFdpWizardService.StepName(CurrentStep)}) · cost {CostText}";
}

/// <summary>One approved baseline (DEV-02). Immutable: a revision appends a new row.</summary>
public sealed record OgFdpBaseline(
    string Id,
    string FdpId,
    string DraftId,
    int Version,
    DateTime? ApprovedAt,
    string? ApprovedBy,
    IReadOnlyList<string> Conditions,
    string? Scope,
    string? Reservoir,
    int WellCount,
    string? Facility,
    decimal CostAmount,
    string? CostCurrency,
    string? Risk)
{
    public string CostText => CostAmount.ToString("#,0.###", CultureInfo.InvariantCulture) + " " +
                             (string.IsNullOrWhiteSpace(CostCurrency) ? "(currency not recorded)" : CostCurrency!.Trim());

    public string Describe() =>
        $"baseline v{Version.ToString(CultureInfo.InvariantCulture)} approved by {ApprovedBy ?? "(unknown)"} " +
        $"on {ApprovedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "unknown"} · {CostText}" +
        (Conditions.Count == 0 ? string.Empty : $" · {string.Join("; ", Conditions)}");
}

/// <summary>One review decision (DEV-02).</summary>
public sealed record OgFdpReview(
    string Id,
    string DraftId,
    string Decision,
    string? Rationale,
    int? ReturnedStep,
    IReadOnlyList<string> Conditions,
    string? Reviewer,
    DateTime? ReviewedAt)
{
    public string Describe() =>
        $"{Decision} by {Reviewer ?? "(unknown)"} " +
        $"{(ReturnedStep is { } step ? $"· correct step {step.ToString(CultureInfo.InvariantCulture)} ({OgFdpWizardService.StepName(step)}) " : string.Empty)}" +
        $"· {Rationale ?? "no rationale"}";
}

/// <summary>One set of wizard values, saved together at the current step.</summary>
public sealed record OgFdpDraftValues(
    string? Scope,
    string? Reservoir,
    int WellCount,
    string? Facility,
    decimal CostAmount,
    string? CostCurrency,
    string? Risk,
    int CurrentStep);

/// <summary>One review decision, as the page submits it.</summary>
public sealed record OgFdpDecision(
    string Decision,
    string? Rationale,
    int? ReturnedStep,
    IReadOnlyList<string> Conditions,
    string Reviewer,
    DateTimeOffset? DecidedAt = null)
{
    public const string Approve = "APPROVE";
    public const string ApproveWithConditions = "APPROVE_WITH_CONDITIONS";
    public const string Return = "RETURN";
    public const string Reject = "REJECT";

    public static IReadOnlyList<string> Decisions { get; } = [Approve, ApproveWithConditions, Return, Reject];

    public string ResultingState => Decision.Trim().ToUpperInvariant() switch
    {
        Approve => OgFdpDraft.Approved,
        ApproveWithConditions => OgFdpDraft.ApprovedWithConditions,
        Return => OgFdpDraft.Returned,
        Reject => OgFdpDraft.Rejected,
        _ => string.Empty,
    };
}

/// <summary>One plan and everything recorded against it.</summary>
public sealed record OgFdpPlan(
    string Id,
    string Name,
    string? PlanStatus,
    IReadOnlyList<OgFdpDraft> Drafts,
    IReadOnlyList<OgFdpBaseline> Baselines,
    IReadOnlyList<OgFdpReview> Reviews)
{
    public OgFdpDraft? Current => Drafts.OrderByDescending(draft => draft.Version).FirstOrDefault();

    public IReadOnlyList<OgFdpReview> ReviewsFor(string draftId) =>
        Reviews.Where(review => string.Equals(review.DraftId, draftId, StringComparison.OrdinalIgnoreCase)).ToList();

    public string Describe() =>
        $"{Name} · {PlanStatus ?? "no plan status"} · {Drafts.Count.ToString(CultureInfo.InvariantCulture)} draft(s) · " +
        $"{Baselines.Count.ToString(CultureInfo.InvariantCulture)} baseline(s)";
}

/// <summary>The plans for one field.</summary>
public sealed record OgFdpWorkspace(
    string FieldName,
    IReadOnlyList<OgFdpPlan> Plans)
{
    public bool IsEmpty => Plans.Count == 0;

    public int BaselineCount => Plans.Sum(plan => plan.Baselines.Count);
}

/// <summary>DEV-02: the field development plan wizard, review and baseline history.</summary>
public interface IOgFdpWizard
{
    Task<OgResult<OgFdpWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFdpPlan>> CreateDraftAsync(
        string fieldId,
        string fieldName,
        string name,
        string author,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFdpPlan>> SaveDraftAsync(
        string draftId,
        OgFdpDraftValues values,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFdpPlan>> SubmitAsync(
        string draftId,
        string actor,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFdpPlan>> ReviewAsync(
        string draftId,
        OgFdpDecision decision,
        CancellationToken cancellationToken = default);

    Task<OgResult<OgFdpPlan>> ReviseAsync(
        string draftId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The submit that was not acknowledged, or null.</summary>
    OgFdpSubmit? Pending { get; }

    Task<OgResult<OgFdpPlan>> RetrySubmitAsync(CancellationToken cancellationToken = default);
}

/// <summary>One submit, retained when the acknowledgement was lost.</summary>
public sealed record OgFdpSubmit(string DraftId, string Actor);

/// <summary>
/// The wizard over <c>FIELD_DEVELOPMENT_PLAN</c> and the DevelopmentPlanning-owned <c>FDP_DRAFT</c>,
/// <c>FDP_BASELINE</c> and <c>FDP_REVIEW</c> tables, read and written through their entity models and a unit of work.
/// </summary>
public sealed class OgFdpWizardService : IOgFdpWizard
{
    public const string CodeNoConnection = "fdp-no-connection";
    public const string CodeReadFailed = "fdp-read-failed";
    public const string CodeWriteFailed = "fdp-write-failed";
    public const string CodeUnknown = "fdp-unknown";
    public const string CodeActorRequired = "fdp-actor-required";
    public const string CodeNameRequired = "fdp-name-required";
    public const string CodeLocked = "fdp-draft-locked";
    public const string CodeAlreadySubmitted = "fdp-already-submitted";
    public const string CodeTerminal = "fdp-terminal";
    public const string CodeStepsMissing = "fdp-steps-missing";
    public const string CodeNotSubmitted = "fdp-not-submitted";
    public const string CodeDecisionRequired = "fdp-decision-required";
    public const string CodeRationaleRequired = "fdp-rationale-required";
    public const string CodeReturnStepRequired = "fdp-return-step-required";
    public const string CodeConditionsRequired = "fdp-conditions-required";
    public const string CodeAuthorCannotApprove = "fdp-author-cannot-approve";
    public const string CodeCostInvalid = "fdp-cost-invalid";
    public const string CodeCurrencyRequired = "fdp-currency-required";
    public const string CodeRevisionNeedsApproved = "fdp-revision-needs-approved";
    public const string CodeNothingPending = "fdp-nothing-pending";

    /// <summary>The wizard's steps, in order (DEV-02 section 3).</summary>
    public static IReadOnlyList<string> Steps { get; } =
    [
        "Scope",
        "Reservoirs and well count",
        "Well schedule",
        "Facilities and capacity",
        "Costs and schedule",
        "Risk and mitigations",
        "Review and submit",
    ];

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgFdpWizardService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public OgFdpSubmit? Pending { get; private set; }

    /// <summary>The step's display name; an unknown step is named rather than hidden.</summary>
    public static string StepName(int step) =>
        step >= 1 && step <= Steps.Count ? Steps[step - 1] : $"Step {step.ToString(CultureInfo.InvariantCulture)}";

    /// <summary>AC-1/AC-3: the steps that still have no value, in order.</summary>
    public static IReadOnlyList<int> MissingSteps(OgFdpDraft draft, bool hasSchedule)
    {
        ArgumentNullException.ThrowIfNull(draft);

        var missing = new List<int>();

        if (string.IsNullOrWhiteSpace(draft.Scope))
        {
            missing.Add(1);
        }

        if (string.IsNullOrWhiteSpace(draft.Reservoir) || draft.WellCount <= 0)
        {
            missing.Add(2);
        }

        if (!hasSchedule)
        {
            missing.Add(3);
        }

        if (string.IsNullOrWhiteSpace(draft.Facility))
        {
            missing.Add(4);
        }

        if (draft.CostAmount <= 0 || string.IsNullOrWhiteSpace(draft.CostCurrency))
        {
            missing.Add(5);
        }

        if (string.IsNullOrWhiteSpace(draft.Risk))
        {
            missing.Add(6);
        }

        return missing;
    }

    public async Task<OgResult<OgFdpWorkspace>> LoadAsync(
        string? fieldId,
        string fieldName,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgFdpWorkspace>.Failed(
                "No connection is open, so the development plans cannot be read.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var plans = model.Plans
                .Where(plan => FieldMatches(plan.FIELD_ID, fieldId, fieldName))
                .Select(plan => BuildPlan(plan, model))
                .OrderByDescending(plan => plan.Current?.Version ?? 0)
                .ThenBy(plan => plan.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return OgResult<OgFdpWorkspace>.Ok(new OgFdpWorkspace(
                string.IsNullOrWhiteSpace(fieldName) ? "(no field selected)" : fieldName.Trim(),
                plans));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the development plans",
                "the plans are not shown",
                FailureSeverity.Error);

            return OgResult<OgFdpWorkspace>.Failed(
                "The development plans could not be read.", reference ?? CodeReadFailed);
        }
    }

    public async Task<OgResult<OgFdpPlan>> CreateDraftAsync(
        string fieldId,
        string fieldName,
        string name,
        string author,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(author))
        {
            return OgResult<OgFdpPlan>.Failed("A draft must record who started it.", CodeActorRequired);
        }

        if (string.IsNullOrWhiteSpace(fieldId) || string.IsNullOrWhiteSpace(name))
        {
            return OgResult<OgFdpPlan>.Failed(
                "A development plan needs a field and a name.", CodeNameRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFdpPlan>.Failed(
                "No connection is open, so the draft cannot be created.", CodeNoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var fdpId = Guid.NewGuid().ToString("N");
            var draftId = Guid.NewGuid().ToString("N");

            var planUnit = OgUow.For<FIELD_DEVELOPMENT_PLAN>(source);
            planUnit.Add(new FIELD_DEVELOPMENT_PLAN
            {
                TENANT_ID = "desktop",
                BUSINESS_ASSOCIATE_ID = "desktop",
                DRAFT_INTENT_ID = draftId,
                DRAFT_REQUEST_REVISION = "1",
                FDP_ID = fdpId,
                FIELD_ID = fieldId.Trim(),
                FDP_NAME = name.Trim(),
                FDP_VERSION = 1,
                FDP_STATUS = OgFdpDraft.Draft,
                TOTAL_WELLS_PLANNED = 0,
                TOTAL_CAPEX_MM = 0,
                CAPEX_CURRENCY = null,
                ROW_CREATED_BY = author.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = author.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committedPlan = await planUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedPlan))
            {
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(committedPlan, "The plan was not created."), CodeWriteFailed);
            }

            var draftUnit = OgUow.For<FDP_DRAFT>(source);
            draftUnit.Add(new FDP_DRAFT
            {
                DRAFT_ID = draftId,
                FDP_ID = fdpId,
                FIELD_ID = fieldId.Trim(),
                VERSION = 1,
                STATE = OgFdpDraft.Draft,
                CURRENT_STEP = 1,
                AUTHOR = author.Trim(),
                ROW_CREATED_BY = author.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = author.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committedDraft = await draftUnit.Commit().ConfigureAwait(false);

            return OgUow.IsOk(committedDraft)
                ? await ReloadPlanAsync(source, fdpId, cancellationToken).ConfigureAwait(false)
                : OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(committedDraft, "The draft was not created."), CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"creating a development plan for {fieldName}",
                "the draft was not created",
                FailureSeverity.Error);

            return OgResult<OgFdpPlan>.Failed(
                "The draft could not be created.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFdpPlan>> SaveDraftAsync(
        string draftId,
        OgFdpDraftValues values,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgFdpPlan>.Failed("A draft change must record who made it.", CodeActorRequired);
        }

        if (values.CostAmount < 0m)
        {
            return OgResult<OgFdpPlan>.Failed("A cost cannot be negative.", CodeCostInvalid);
        }

        // AC-5: a cost value needs its currency, just like a money value elsewhere.
        if (values.CostAmount > 0m && string.IsNullOrWhiteSpace(values.CostCurrency))
        {
            return OgResult<OgFdpPlan>.Failed("A cost needs the currency it is in.", CodeCurrencyRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFdpPlan>.Failed(
                "No connection is open, so the draft cannot be saved.", CodeNoConnection);
        }

        try
        {
            var draftUnit = OgUow.For<FDP_DRAFT>(source);
            var rows = await OgUow.ReadAsync(draftUnit, [OgUow.Equal("DRAFT_ID", draftId.Trim())], cancellationToken)
                .ConfigureAwait(false);
            var draft = rows.FirstOrDefault();

            if (draft is null)
            {
                return OgResult<OgFdpPlan>.Failed($"Draft '{draftId}' is not in this database.", CodeUnknown);
            }

            if (!string.Equals(draft.STATE, OgFdpDraft.Draft, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(draft.STATE, OgFdpDraft.Returned, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgFdpPlan>.Failed(
                    $"This draft is {draft.STATE}; only a draft or a returned draft can be edited.",
                    CodeLocked);
            }

            var now = DateTime.UtcNow;

            draft.SCOPE_TEXT = Blank(values.Scope);
            draft.RESERVOIR_TEXT = Blank(values.Reservoir);
            draft.WELL_COUNT = Math.Max(0, values.WellCount);
            draft.FACILITY_TEXT = Blank(values.Facility);
            draft.COST_AMOUNT = values.CostAmount;
            draft.COST_CURRENCY = Blank(values.CostCurrency)?.ToUpperInvariant();
            draft.RISK_TEXT = Blank(values.Risk);
            draft.CURRENT_STEP = Math.Clamp(values.CurrentStep, 1, Steps.Count);
            draft.ROW_CHANGED_BY = actor.Trim();
            draft.ROW_CHANGED_DATE = now;

            var updated = draftUnit.Update(draft);

            if (!OgUow.IsOk(updated))
            {
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(updated, "The draft was not saved."), CodeWriteFailed);
            }

            var committed = await draftUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(committed, "The draft was not saved."), CodeWriteFailed);
            }

            await SyncPlanAsync(source, draft.FDP_ID, ToDraft(draft), actor.Trim(), now, OgFdpDraft.Draft, cancellationToken)
                .ConfigureAwait(false);

            return await ReloadPlanAsync(source, draft.FDP_ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"saving the development plan draft {draftId}",
                "the draft was not saved",
                FailureSeverity.Error);

            return OgResult<OgFdpPlan>.Failed(
                "The draft could not be saved.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFdpPlan>> SubmitAsync(
        string draftId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        var submitted = await SubmitCoreAsync(draftId, actor, cancellationToken).ConfigureAwait(false);

        if (submitted.Success)
        {
            Pending = null;
        }

        return submitted;
    }

    public async Task<OgResult<OgFdpPlan>> ReviewAsync(
        string draftId,
        OgFdpDecision decision,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(decision);

        if (string.IsNullOrWhiteSpace(decision.Reviewer))
        {
            return OgResult<OgFdpPlan>.Failed("A review must record who decided.", CodeActorRequired);
        }

        var normalised = decision.Decision?.Trim().ToUpperInvariant() ?? string.Empty;

        if (!OgFdpDecision.Decisions.Contains(normalised, StringComparer.Ordinal))
        {
            return OgResult<OgFdpPlan>.Failed(
                $"'{decision.Decision}' is not a review decision; use {string.Join(", ", OgFdpDecision.Decisions)}.",
                CodeDecisionRequired);
        }

        if (string.IsNullOrWhiteSpace(decision.Rationale))
        {
            return OgResult<OgFdpPlan>.Failed("A review decision needs a rationale.", CodeRationaleRequired);
        }

        var conditions = (decision.Conditions ?? [])
            .Where(condition => !string.IsNullOrWhiteSpace(condition))
            .Select(condition => condition.Trim())
            .ToList();

        if (normalised == OgFdpDecision.ApproveWithConditions && conditions.Count == 0)
        {
            return OgResult<OgFdpPlan>.Failed(
                "An approval with conditions needs at least one condition.", CodeConditionsRequired);
        }

        if (normalised == OgFdpDecision.Return
            && (decision.ReturnedStep is not { } step || step < 1 || step > Steps.Count - 1))
        {
            return OgResult<OgFdpPlan>.Failed(
                "A return must name the wizard step that needs correcting (1 to 6).", CodeReturnStepRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFdpPlan>.Failed(
                "No connection is open, so the review cannot be recorded.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var draft = model.Drafts.FirstOrDefault(row =>
                string.Equals(row.DRAFT_ID, draftId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (draft is null)
            {
                return OgResult<OgFdpPlan>.Failed($"Draft '{draftId}' is not in this database.", CodeUnknown);
            }

            if (!string.Equals(draft.STATE, OgFdpDraft.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                // A retry after a partially applied review must not append a second baseline.
                if (model.Baselines.Any(baseline =>
                        string.Equals(baseline.DRAFT_ID, draft.DRAFT_ID, StringComparison.OrdinalIgnoreCase)))
                {
                    return await ReloadPlanAsync(source, draft.FDP_ID, cancellationToken).ConfigureAwait(false);
                }

                return OgResult<OgFdpPlan>.Failed(
                    $"This draft is {draft.STATE}; only a submitted draft can be reviewed.", CodeNotSubmitted);
            }

            // AC-4: the author cannot approve their own plan.
            if (!string.IsNullOrWhiteSpace(draft.AUTHOR)
                && string.Equals(draft.AUTHOR.Trim(), decision.Reviewer.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgFdpPlan>.Failed(
                    $"{decision.Reviewer.Trim()} submitted this plan, so they cannot review it. Another reviewer must.",
                    CodeAuthorCannotApprove);
            }

            var decidedAt = decision.DecidedAt ?? DateTimeOffset.UtcNow;
            var state = decision.ResultingState;
            var approved = normalised is OgFdpDecision.Approve or OgFdpDecision.ApproveWithConditions;
            string? baselineId = null;

            if (approved)
            {
                baselineId = Guid.NewGuid().ToString("N");
                var baselineUnit = OgUow.For<FDP_BASELINE>(source);

                baselineUnit.Add(new FDP_BASELINE
                {
                    BASELINE_ID = baselineId,
                    FDP_ID = draft.FDP_ID,
                    DRAFT_ID = draft.DRAFT_ID,
                    VERSION = model.Baselines
                        .Where(baseline => string.Equals(baseline.FDP_ID, draft.FDP_ID, StringComparison.OrdinalIgnoreCase))
                        .Select(baseline => baseline.VERSION)
                        .DefaultIfEmpty(draft.VERSION - 1)
                        .Max() + 1,
                    APPROVED_AT = decidedAt.UtcDateTime,
                    APPROVED_BY = decision.Reviewer.Trim(),
                    CONDITIONS_TEXT = conditions.Count == 0 ? null : string.Join("\n", conditions),
                    SCOPE_TEXT = draft.SCOPE_TEXT,
                    RESERVOIR_TEXT = draft.RESERVOIR_TEXT,
                    WELL_COUNT = draft.WELL_COUNT,
                    FACILITY_TEXT = draft.FACILITY_TEXT,
                    COST_AMOUNT = draft.COST_AMOUNT,
                    COST_CURRENCY = draft.COST_CURRENCY,
                    RISK_TEXT = draft.RISK_TEXT,
                    ROW_CREATED_BY = decision.Reviewer.Trim(),
                    ROW_CREATED_DATE = decidedAt.UtcDateTime,
                    ROW_CHANGED_BY = decision.Reviewer.Trim(),
                    ROW_CHANGED_DATE = decidedAt.UtcDateTime,
                });

                var committedBaseline = await baselineUnit.Commit().ConfigureAwait(false);

                if (!OgUow.IsOk(committedBaseline))
                {
                    return OgResult<OgFdpPlan>.Failed(
                        OgUow.Message(committedBaseline, "The baseline was not recorded."), CodeWriteFailed);
                }
            }

            var draftUnit = OgUow.For<FDP_DRAFT>(source);
            var drafts = await OgUow.ReadAsync(draftUnit, [OgUow.Equal("DRAFT_ID", draft.DRAFT_ID)], cancellationToken)
                .ConfigureAwait(false);
            var current = drafts.FirstOrDefault();

            if (current is null)
            {
                return OgResult<OgFdpPlan>.Failed($"Draft '{draftId}' is not in this database.", CodeUnknown);
            }

            current.STATE = state;
            current.BASELINE_ID = baselineId;
            current.RETURNED_STEP = normalised == OgFdpDecision.Return ? decision.ReturnedStep : null;
            current.RETURN_REASON = normalised == OgFdpDecision.Return ? decision.Rationale.Trim() : null;
            current.ROW_CHANGED_BY = decision.Reviewer.Trim();
            current.ROW_CHANGED_DATE = decidedAt.UtcDateTime;

            var updatedDraft = draftUnit.Update(current);

            if (!OgUow.IsOk(updatedDraft))
            {
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(updatedDraft, "The review was not recorded."), CodeWriteFailed);
            }

            var committedDraft = await draftUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedDraft))
            {
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(committedDraft, "The review was not recorded."), CodeWriteFailed);
            }

            var reviewUnit = OgUow.For<FDP_REVIEW>(source);
            reviewUnit.Add(new FDP_REVIEW
            {
                REVIEW_ID = Guid.NewGuid().ToString("N"),
                DRAFT_ID = current.DRAFT_ID,
                FDP_ID = current.FDP_ID,
                DECISION = normalised,
                RATIONALE = decision.Rationale.Trim(),
                RETURNED_STEP = normalised == OgFdpDecision.Return ? decision.ReturnedStep : null,
                CONDITIONS_TEXT = conditions.Count == 0 ? null : string.Join("\n", conditions),
                REVIEWER = decision.Reviewer.Trim(),
                REVIEWED_AT = decidedAt.UtcDateTime,
                ROW_CREATED_BY = decision.Reviewer.Trim(),
                ROW_CREATED_DATE = decidedAt.UtcDateTime,
                ROW_CHANGED_BY = decision.Reviewer.Trim(),
                ROW_CHANGED_DATE = decidedAt.UtcDateTime,
            });

            var committedReview = await reviewUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committedReview))
            {
                _failures.ReportHandled(
                    new InvalidOperationException(
                        $"The review row for {draftId} was not written: " +
                        OgUow.Message(committedReview, "the row was not inserted")),
                    $"recording the review of {draftId}",
                    "the decision stands but its history row is missing",
                    FailureSeverity.Degraded);
            }

            await SyncPlanAsync(
                source,
                current.FDP_ID,
                ToDraft(current),
                decision.Reviewer.Trim(),
                decidedAt.UtcDateTime,
                state,
                cancellationToken).ConfigureAwait(false);

            return await ReloadPlanAsync(source, current.FDP_ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording the review of {draftId}",
                "the review was not applied",
                FailureSeverity.Error);

            return OgResult<OgFdpPlan>.Failed(
                "The review could not be recorded.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFdpPlan>> ReviseAsync(
        string draftId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgFdpPlan>.Failed("A revision must record who made it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFdpPlan>.Failed(
                "No connection is open, so the plan cannot be revised.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var previous = model.Drafts.FirstOrDefault(row =>
                string.Equals(row.DRAFT_ID, draftId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (previous is null)
            {
                return OgResult<OgFdpPlan>.Failed($"Draft '{draftId}' is not in this database.", CodeUnknown);
            }

            if (!string.Equals(previous.STATE, OgFdpDraft.Approved, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(previous.STATE, OgFdpDraft.ApprovedWithConditions, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(previous.STATE, OgFdpDraft.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgFdpPlan>.Failed(
                    "A revision starts from an approved or rejected plan; a working draft is edited in place.",
                    CodeRevisionNeedsApproved);
            }

            var baseline = model.Baselines
                .Where(row => string.Equals(row.FDP_ID, previous.FDP_ID, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(row => row.VERSION)
                .FirstOrDefault();

            var version = model.Drafts
                .Where(row => string.Equals(row.FDP_ID, previous.FDP_ID, StringComparison.OrdinalIgnoreCase))
                .Select(row => row.VERSION)
                .DefaultIfEmpty(0)
                .Max() + 1;

            var now = DateTime.UtcNow;
            var draftIdNew = Guid.NewGuid().ToString("N");

            var draftUnit = OgUow.For<FDP_DRAFT>(source);
            draftUnit.Add(new FDP_DRAFT
            {
                DRAFT_ID = draftIdNew,
                FDP_ID = previous.FDP_ID,
                FIELD_ID = previous.FIELD_ID,
                VERSION = version,
                STATE = OgFdpDraft.Draft,
                CURRENT_STEP = 1,
                AUTHOR = actor.Trim(),
                SCOPE_TEXT = baseline?.SCOPE_TEXT ?? previous.SCOPE_TEXT,
                RESERVOIR_TEXT = baseline?.RESERVOIR_TEXT ?? previous.RESERVOIR_TEXT,
                WELL_COUNT = baseline?.WELL_COUNT ?? previous.WELL_COUNT,
                FACILITY_TEXT = baseline?.FACILITY_TEXT ?? previous.FACILITY_TEXT,
                COST_AMOUNT = baseline?.COST_AMOUNT ?? previous.COST_AMOUNT,
                COST_CURRENCY = baseline?.COST_CURRENCY ?? previous.COST_CURRENCY,
                RISK_TEXT = baseline?.RISK_TEXT ?? previous.RISK_TEXT,
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await draftUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(committed, "The revision was not created."), CodeWriteFailed);
            }

            await SyncPlanAsync(
                source,
                previous.FDP_ID,
                new OgFdpDraft(
                    draftIdNew,
                    previous.FDP_ID,
                    previous.FIELD_ID,
                    version,
                    OgFdpDraft.Draft,
                    1,
                    actor.Trim(),
                    baseline?.SCOPE_TEXT ?? previous.SCOPE_TEXT,
                    baseline?.RESERVOIR_TEXT ?? previous.RESERVOIR_TEXT,
                    baseline?.WELL_COUNT ?? previous.WELL_COUNT,
                    baseline?.FACILITY_TEXT ?? previous.FACILITY_TEXT,
                    baseline?.COST_AMOUNT ?? previous.COST_AMOUNT,
                    baseline?.COST_CURRENCY ?? previous.COST_CURRENCY,
                    baseline?.RISK_TEXT ?? previous.RISK_TEXT,
                    null,
                    null,
                    null),
                actor.Trim(),
                now,
                OgFdpDraft.Draft,
                cancellationToken).ConfigureAwait(false);

            return await ReloadPlanAsync(source, previous.FDP_ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"revising development plan draft {draftId}",
                "the revision was not created",
                FailureSeverity.Error);

            return OgResult<OgFdpPlan>.Failed(
                "The plan could not be revised.", reference ?? CodeWriteFailed);
        }
    }

    public async Task<OgResult<OgFdpPlan>> RetrySubmitAsync(CancellationToken cancellationToken = default)
    {
        if (Pending is not { } pending)
        {
            return OgResult<OgFdpPlan>.Failed(
                "There is no submit waiting to be retried.", CodeNothingPending);
        }

        var retried = await SubmitCoreAsync(pending.DraftId, pending.Actor, cancellationToken).ConfigureAwait(false);

        if (retried.Success)
        {
            Pending = null;
        }

        return retried;
    }

    // ------------------------------------------------------------------ reads

    private static async Task<FdpModel> ReadAsync(IDataSource source, CancellationToken cancellationToken)
    {
        var plans = await OgUow.ReadAllAsync<FIELD_DEVELOPMENT_PLAN>(source).ConfigureAwait(false);
        var drafts = await OgUow.ReadAllAsync<FDP_DRAFT>(source).ConfigureAwait(false);
        var baselines = await OgUow.ReadAllAsync<FDP_BASELINE>(source).ConfigureAwait(false);
        var reviews = await OgUow.ReadAllAsync<FDP_REVIEW>(source).ConfigureAwait(false);
        var schedules = await OgUow.ReadAllAsync<DEVELOPMENT_WELL_SCHEDULE>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return new FdpModel(plans, drafts, baselines, reviews, schedules);
    }

    private static OgFdpPlan BuildPlan(FIELD_DEVELOPMENT_PLAN plan, FdpModel model) =>
        new(
            plan.FDP_ID ?? string.Empty,
            string.IsNullOrWhiteSpace(plan.FDP_NAME) ? plan.FDP_ID ?? "(unnamed plan)" : plan.FDP_NAME,
            plan.FDP_STATUS,
            model.Drafts
                .Where(draft => string.Equals(draft.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(draft => draft.VERSION)
                .Select(ToDraft)
                .ToList(),
            model.Baselines
                .Where(baseline => string.Equals(baseline.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(baseline => baseline.VERSION)
                .Select(ToBaseline)
                .ToList(),
            model.Reviews
                .Where(review => string.Equals(review.FDP_ID, plan.FDP_ID, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(review => review.REVIEWED_AT)
                .Select(ToReview)
                .ToList());

    private static OgFdpDraft ToDraft(FDP_DRAFT draft) =>
        new(
            draft.DRAFT_ID ?? string.Empty,
            draft.FDP_ID ?? string.Empty,
            draft.FIELD_ID ?? string.Empty,
            draft.VERSION,
            string.IsNullOrWhiteSpace(draft.STATE) ? OgFdpDraft.Draft : draft.STATE.Trim().ToUpperInvariant(),
            draft.CURRENT_STEP,
            Blank(draft.AUTHOR),
            Blank(draft.SCOPE_TEXT),
            Blank(draft.RESERVOIR_TEXT),
            draft.WELL_COUNT,
            Blank(draft.FACILITY_TEXT),
            draft.COST_AMOUNT,
            Blank(draft.COST_CURRENCY),
            Blank(draft.RISK_TEXT),
            Blank(draft.BASELINE_ID),
            draft.RETURNED_STEP,
            Blank(draft.RETURN_REASON));

    private static OgFdpBaseline ToBaseline(FDP_BASELINE baseline) =>
        new(
            baseline.BASELINE_ID ?? string.Empty,
            baseline.FDP_ID ?? string.Empty,
            baseline.DRAFT_ID ?? string.Empty,
            baseline.VERSION,
            baseline.APPROVED_AT,
            Blank(baseline.APPROVED_BY),
            Split(baseline.CONDITIONS_TEXT),
            Blank(baseline.SCOPE_TEXT),
            Blank(baseline.RESERVOIR_TEXT),
            baseline.WELL_COUNT,
            Blank(baseline.FACILITY_TEXT),
            baseline.COST_AMOUNT,
            Blank(baseline.COST_CURRENCY),
            Blank(baseline.RISK_TEXT));

    private static OgFdpReview ToReview(FDP_REVIEW review) =>
        new(
            review.REVIEW_ID ?? string.Empty,
            review.DRAFT_ID ?? string.Empty,
            Blank(review.DECISION) ?? string.Empty,
            Blank(review.RATIONALE),
            review.RETURNED_STEP,
            Split(review.CONDITIONS_TEXT),
            Blank(review.REVIEWER),
            review.REVIEWED_AT);

    private async Task<OgResult<OgFdpPlan>> SubmitCoreAsync(
        string draftId,
        string actor,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgFdpPlan>.Failed("A submit must record who sent it.", CodeActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgFdpPlan>.Failed(
                "No connection is open, so the plan cannot be submitted.", CodeNoConnection);
        }

        try
        {
            var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
            var draft = model.Drafts.FirstOrDefault(row =>
                string.Equals(row.DRAFT_ID, draftId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (draft is null)
            {
                return OgResult<OgFdpPlan>.Failed($"Draft '{draftId}' is not in this database.", CodeUnknown);
            }

            if (string.Equals(draft.STATE, OgFdpDraft.Submitted, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgFdpPlan>.Failed("This plan is already submitted.", CodeAlreadySubmitted);
            }

            if (string.Equals(draft.STATE, OgFdpDraft.Approved, StringComparison.OrdinalIgnoreCase)
                || string.Equals(draft.STATE, OgFdpDraft.ApprovedWithConditions, StringComparison.OrdinalIgnoreCase)
                || string.Equals(draft.STATE, OgFdpDraft.Rejected, StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgFdpPlan>.Failed(
                    "This plan is closed; start a revision instead of submitting it again.", CodeTerminal);
            }

            var hasSchedule = model.Schedules.Any(schedule =>
                string.Equals(schedule.FDP_ID, draft.FDP_ID, StringComparison.OrdinalIgnoreCase));

            var missing = MissingSteps(ToDraft(draft), hasSchedule);

            if (missing.Count > 0)
            {
                draft.CURRENT_STEP = missing[0];
                var unit = OgUow.For<FDP_DRAFT>(source);
                var rows = await OgUow.ReadAsync(unit, [OgUow.Equal("DRAFT_ID", draft.DRAFT_ID)], cancellationToken)
                    .ConfigureAwait(false);
                var tracked = rows.FirstOrDefault();

                if (tracked is not null)
                {
                    tracked.CURRENT_STEP = missing[0];
                    tracked.ROW_CHANGED_BY = actor.Trim();
                    tracked.ROW_CHANGED_DATE = DateTime.UtcNow;
                    unit.Update(tracked);
                    await unit.Commit().ConfigureAwait(false);
                }

                return OgResult<OgFdpPlan>.Failed(
                    "The plan cannot be submitted; complete: " +
                    string.Join(", ", missing.Select(step =>
                        $"{step.ToString(CultureInfo.InvariantCulture)} ({StepName(step)})")) + ".",
                    CodeStepsMissing);
            }

            var now = DateTime.UtcNow;
            var draftUnit = OgUow.For<FDP_DRAFT>(source);
            var drafts = await OgUow.ReadAsync(draftUnit, [OgUow.Equal("DRAFT_ID", draft.DRAFT_ID)], cancellationToken)
                .ConfigureAwait(false);
            var current = drafts.FirstOrDefault();

            if (current is null)
            {
                return OgResult<OgFdpPlan>.Failed($"Draft '{draftId}' is not in this database.", CodeUnknown);
            }

            current.STATE = OgFdpDraft.Submitted;
            current.RETURNED_STEP = null;
            current.RETURN_REASON = null;
            current.ROW_CHANGED_BY = actor.Trim();
            current.ROW_CHANGED_DATE = now;

            var updated = draftUnit.Update(current);

            if (!OgUow.IsOk(updated))
            {
                Pending = new OgFdpSubmit(current.DRAFT_ID, actor.Trim());
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(updated, "The submit was not acknowledged; retry sends the same submit."),
                    CodeWriteFailed);
            }

            var committed = await draftUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                Pending = new OgFdpSubmit(current.DRAFT_ID, actor.Trim());
                return OgResult<OgFdpPlan>.Failed(
                    OgUow.Message(committed, "The submit was not acknowledged; retry sends the same submit."),
                    CodeWriteFailed);
            }

            await SyncPlanAsync(
                source,
                current.FDP_ID,
                ToDraft(current),
                actor.Trim(),
                now,
                OgFdpDraft.Submitted,
                cancellationToken).ConfigureAwait(false);

            return await ReloadPlanAsync(source, current.FDP_ID, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            Pending = new OgFdpSubmit(draftId.Trim(), actor.Trim());
            var reference = _failures.ReportHandled(
                ex,
                $"submitting development plan draft {draftId}",
                "the submit was not acknowledged; the original is retained for retry",
                FailureSeverity.Error);

            return OgResult<OgFdpPlan>.Failed(
                "The plan could not be submitted; retry sends the same submit.", reference ?? CodeWriteFailed);
        }
    }

    private async Task SyncPlanAsync(
        IDataSource source,
        string fdpId,
        OgFdpDraft values,
        string actor,
        DateTime now,
        string state,
        CancellationToken cancellationToken)
    {
        var planUnit = OgUow.For<FIELD_DEVELOPMENT_PLAN>(source);
        var rows = await OgUow.ReadAsync(planUnit, [OgUow.Equal("FDP_ID", fdpId)], cancellationToken)
            .ConfigureAwait(false);
        var plan = rows.FirstOrDefault();

        if (plan is null)
        {
            return;
        }

        plan.FDP_STATUS = state;
        plan.FDP_VERSION = Math.Max(plan.FDP_VERSION, values.Version);
        plan.TOTAL_WELLS_PLANNED = values.WellCount;
        plan.TOTAL_CAPEX_MM = values.CostAmount;
        plan.CAPEX_CURRENCY = values.CostCurrency;
        plan.DEVELOPMENT_CONCEPT_DESC = values.Scope;
        plan.ROW_CHANGED_BY = actor;
        plan.ROW_CHANGED_DATE = now;

        var updated = planUnit.Update(plan);

        if (!OgUow.IsOk(updated))
        {
            _failures.ReportHandled(
                new InvalidOperationException(
                    $"The plan row for {fdpId} was not updated: {OgUow.Message(updated, "the row was not updated")}"),
                $"applying the draft to plan {fdpId}",
                "the draft is recorded but the plan row still shows the previous values",
                FailureSeverity.Degraded);

            return;
        }

        var committed = await planUnit.Commit().ConfigureAwait(false);

        if (!OgUow.IsOk(committed))
        {
            _failures.ReportHandled(
                new InvalidOperationException(
                    $"The plan row for {fdpId} was not committed: {OgUow.Message(committed, "the row was not updated")}"),
                $"applying the draft to plan {fdpId}",
                "the draft is recorded but the plan row still shows the previous values",
                FailureSeverity.Degraded);
        }
    }

    private static async Task<OgResult<OgFdpPlan>> ReloadPlanAsync(
        IDataSource source,
        string fdpId,
        CancellationToken cancellationToken)
    {
        var model = await ReadAsync(source, cancellationToken).ConfigureAwait(false);
        var plan = model.Plans.FirstOrDefault(row =>
            string.Equals(row.FDP_ID, fdpId, StringComparison.OrdinalIgnoreCase));

        return plan is null
            ? OgResult<OgFdpPlan>.Failed($"Plan '{fdpId}' is not in this database.", CodeUnknown)
            : OgResult<OgFdpPlan>.Ok(BuildPlan(plan, model));
    }

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

    private sealed record FdpModel(
        IReadOnlyList<FIELD_DEVELOPMENT_PLAN> Plans,
        IReadOnlyList<FDP_DRAFT> Drafts,
        IReadOnlyList<FDP_BASELINE> Baselines,
        IReadOnlyList<FDP_REVIEW> Reviews,
        IReadOnlyList<DEVELOPMENT_WELL_SCHEDULE> Schedules);
}
