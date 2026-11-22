using System.Globalization;
using Beep.OilandGas.LifeCycle.Constants;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Economics;

/// <summary>One investment proposal in the review queue (ECO-03).</summary>
public sealed record OgInvestmentProposal(
    string Id,
    string Title,
    string? EntityId,
    decimal Amount,
    string Currency,
    string Class,
    string? SubmittedBy,
    DateTime? SubmittedAt,
    DateTime? DecisionDue,
    string State,
    string? Decision,
    string? Rationale,
    IReadOnlyList<string> Conditions,
    string? DecidedBy,
    DateTime? DecidedAt,
    decimal? DecisionAmount,
    string? EconomicsId,
    decimal? EconomicsNpv,
    decimal? ReviewedAmount,
    DateTime? ReviewedAt,
    string? Risks,
    string? AfeId,
    DateTime? HandoffAt,
    string? HandoffBy)
{
    public bool IsQueued => string.Equals(State, InvestmentDecisionCodes.Queued, StringComparison.OrdinalIgnoreCase);

    public bool IsDecided => !IsQueued;

    public bool IsHandedOff => string.Equals(State, InvestmentDecisionCodes.HandedOff,
        StringComparison.OrdinalIgnoreCase);

    /// <summary>The state table's "economics missing - decision disabled with a link to ECO-01".</summary>
    public bool HasEconomics => string.IsNullOrWhiteSpace(EconomicsId) || EconomicsNpv is null
        ? false
        : true;

    /// <summary>AC-2: the amount now differs from the amount that was reviewed, so a decision would be stale.</summary>
    public bool EconomicsStale => ReviewedAmount is { } reviewed && reviewed != Amount;

    public bool Authorises => InvestmentDecisionCodes.Authorises(Decision);

    /// <summary>Days in the queue, measured to the decision due date the proposal states.</summary>
    public string AgeText
    {
        get
        {
            if (SubmittedAt is not { } submitted)
            {
                return "no submission date";
            }

            var days = (DateTime.UtcNow.Date - submitted.Date).Days;

            return days <= 0
                ? "today"
                : days.ToString(CultureInfo.InvariantCulture) + " day(s)";
        }
    }

    public string DueText => DecisionDue is { } due
        ? due.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) +
          (due.Date < DateTime.UtcNow.Date && IsQueued ? " (overdue)" : string.Empty)
        : "no due date";

    public string AmountText => MoneyText.Of(Amount) + " " + Currency;

    public string DecisionText => IsQueued
        ? "awaiting a decision"
        : InvestmentDecisionCodes.Describe(Decision) + " by " + (DecidedBy ?? "(unknown)") + " on " +
          (DecidedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date") +
          " on " + MoneyText.Of(DecisionAmount ?? Amount) + " " + Currency;

    public string RationaleText => string.IsNullOrWhiteSpace(Rationale)
        ? "no rationale recorded"
        : Rationale!;

    public string ConditionsText => Conditions.Count == 0
        ? "no conditions"
        : string.Join(Environment.NewLine, Conditions);

    public string EconomicsText => !HasEconomics
        ? "no economics: the decision is disabled until an evaluation is linked (ECO-01)"
        : "economics " + (EconomicsId ?? string.Empty) + " · NPV " + MoneyText.Of(EconomicsNpv!.Value) +
          (EconomicsStale
              ? " · the amount has changed since it was reviewed (" + MoneyText.Of(ReviewedAmount!.Value) +
                " reviewed, " + MoneyText.Of(Amount) + " now), so a re-review is required"
              : " · reviewed on " +
                (ReviewedAt?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "an unrecorded date"));

    public string HandoffText => IsHandedOff
        ? "handed off as AFE " + (AfeId ?? "(unknown)") + " by " + (HandoffBy ?? "(unknown)") + " on " +
          (HandoffAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date")
        : Authorises && IsDecided
            ? "ready to hand off - no AFE yet"
            : "no handoff";

    public string Describe() =>
        $"{Title} · {EntityId ?? "(no entity)"} · {AmountText} · {Class} · {State} · due {DueText} · " +
        (IsQueued ? "queued " + AgeText : InvestmentDecisionCodes.Describe(Decision));
}

/// <summary>The fields a proposal is submitted from (ECO-03).</summary>
public sealed record OgInvestmentEntry(
    string? Title,
    string? EntityId,
    decimal? Amount,
    string? Currency,
    string? Class,
    DateTime? DecisionDue,
    string? EconomicsId,
    decimal? EconomicsNpv,
    string? Risks);

/// <summary>The review workspace (ECO-03).</summary>
public sealed record OgInvestmentWorkspace(
    IReadOnlyList<OgInvestmentProposal> Queue,
    OgInvestmentProposal? Selected,
    IReadOnlyList<string> Classes,
    IReadOnlyList<string> Decisions,
    IReadOnlyList<string> Currencies,
    bool HistoryFailed,
    string? HistoryReason)
{
    public string QueueText => Queue.Count == 0
        ? "The investment queue is empty."
        : string.Join(Environment.NewLine, Queue.Select(proposal => proposal.Describe()));

    public string HistoryStateText => HistoryFailed
        ? "the queue could not be read; decisions cannot be recorded"
        : $"{Queue.Count.ToString(CultureInfo.InvariantCulture)} proposal(s)";

    public int AwaitingCount => Queue.Count(proposal => proposal.IsQueued);

    public int ReadyToHandOff => Queue.Count(proposal => proposal.IsDecided && proposal.Authorises && !proposal.IsHandedOff);

    public int StaleCount => Queue.Count(proposal => proposal.IsQueued && proposal.EconomicsStale);

    public string SummaryText =>
        $"{Queue.Count.ToString(CultureInfo.InvariantCulture)} proposal(s) · " +
        $"{AwaitingCount.ToString(CultureInfo.InvariantCulture)} awaiting a decision · " +
        $"{StaleCount.ToString(CultureInfo.InvariantCulture)} with stale economics · " +
        $"{ReadyToHandOff.ToString(CultureInfo.InvariantCulture)} approved and not yet handed off";

    public string ReferenceText =>
        $"states {string.Join(", ", InvestmentDecisionCodes.States)} · decisions " +
        $"{string.Join(", ", InvestmentDecisionCodes.Decisions)} · a decision needs rationale and a non-author, and a " +
        "handoff creates exactly one AFE";
}

/// <summary>The investment review rules, stated once so the page, the service and the tests agree.</summary>
public static class InvestmentReviewRules
{
    public static IReadOnlyList<string> Classes { get; } = ["DRILL", "FACILITY", "WORKOVER", "OTHER"];

    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];

    public static IReadOnlyList<string> Decisions => InvestmentDecisionCodes.Decisions;

    public static IReadOnlyList<string> Missing(OgInvestmentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var missing = new List<string>();

        if (string.IsNullOrWhiteSpace(entry.Title))
        {
            missing.Add("title");
        }

        if (string.IsNullOrWhiteSpace(entry.EntityId))
        {
            missing.Add("well or entity");
        }

        if (entry.Amount is null)
        {
            missing.Add("amount");
        }

        if (string.IsNullOrWhiteSpace(entry.Currency))
        {
            missing.Add("currency");
        }

        if (string.IsNullOrWhiteSpace(entry.Class))
        {
            missing.Add("class");
        }

        return missing;
    }

    public static (string Code, string Message)? Validate(OgInvestmentEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var missing = Missing(entry);

        if (missing.Count > 0)
        {
            return (InvestmentReviewCodes.InputRequired, $"The proposal needs {string.Join(", ", missing)}.");
        }

        if (!Classes.Contains(entry.Class!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (InvestmentReviewCodes.InputInvalid,
                $"'{entry.Class}' is not an investment class; use {string.Join(", ", Classes)}.");
        }

        if (!Currencies.Contains(entry.Currency!.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (InvestmentReviewCodes.InputInvalid,
                $"'{entry.Currency}' is not a currency this queue decides in; use {string.Join(", ", Currencies)}.");
        }

        return entry.Amount <= 0m
            ? (InvestmentReviewCodes.InputInvalid, "The proposal amount must be above zero.")
            : null;
    }

    /// <summary>
    /// AC-1: a decision needs an actor, a rationale and - for a conditional approval - at least one condition. The
    /// decision itself must be one the owner names.
    /// </summary>
    public static (string Code, string Message)? ValidateDecision(
        string? decision,
        string? rationale,
        IReadOnlyList<string> conditions)
    {
        if (string.IsNullOrWhiteSpace(decision)
            || !Decisions.Contains(decision.Trim().ToUpperInvariant(), StringComparer.Ordinal))
        {
            return (InvestmentReviewCodes.InputInvalid,
                $"'{decision}' is not a decision; use {string.Join(", ", Decisions)}.");
        }

        if (string.IsNullOrWhiteSpace(rationale))
        {
            return (InvestmentReviewCodes.InputRequired,
                "A decision must record its rationale, or nobody can see why the money was approved.");
        }

        if (InvestmentDecisionCodes.NeedsConditions(decision) && conditions.Count == 0)
        {
            return (InvestmentReviewCodes.ConditionsRequired,
                "A conditional approval must state at least one condition, because those conditions travel to the AFE.");
        }

        return null;
    }
}

/// <summary>ECO-03: the investment review queue and the handoff to an AFE.</summary>
public interface IOgInvestmentReview
{
    /// <summary>Submits a proposal to the queue.</summary>
    Task<OgResult<OgInvestmentWorkspace>> SubmitAsync(
        OgInvestmentEntry entry,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Records a decision. Refuses the author (AC-3), stale economics (AC-2) and a second decision.</summary>
    Task<OgResult<OgInvestmentWorkspace>> DecideAsync(
        string proposalId,
        string decision,
        string rationale,
        string conditions,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Changes the amount, which makes the existing review stale until it is reviewed again (AC-2).</summary>
    Task<OgResult<OgInvestmentWorkspace>> AmendAsync(
        string proposalId,
        decimal amount,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Re-reviews the proposal at its current amount, clearing the staleness.</summary>
    Task<OgResult<OgInvestmentWorkspace>> ReviewAsync(
        string proposalId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>Creates the one AFE the decision authorises, carrying its conditions (AC-4, AC-5, AC-6).</summary>
    Task<OgResult<OgInvestmentWorkspace>> HandoffAsync(
        string proposalId,
        string actor,
        CancellationToken cancellationToken = default);

    /// <summary>The queue; an unavailable one is a state.</summary>
    Task<OgResult<OgInvestmentWorkspace>> LoadAsync(
        string? proposalId = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// ECO-03 over LifeCycle's <c>APPROVAL_WORKFLOW</c> row (the owner's approval record, extended additively) and
/// ProductionAccounting's <c>AFE</c>, through units of work.
/// </summary>
/// <remarks>
/// One row is the proposal and its decision, because a decision is final in this feature. The handoff is idempotent in
/// two directions: the proposal remembers the AFE it created, and the AFE remembers the proposal it came from - so a lost
/// acknowledgement is recovered by asking the AFEs which proposal they belong to rather than by creating a second one
/// (AC-6).
/// </remarks>
public sealed class OgInvestmentReviewService : IOgInvestmentReview
{
    private const string InvestmentEntityName = "INVESTMENT";

    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;

    public OgInvestmentReviewService(Func<IDataSource?> source, IFailureReporter failures)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    public async Task<OgResult<OgInvestmentWorkspace>> SubmitAsync(
        OgInvestmentEntry entry,
        string actor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entry);

        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "A proposal must record who submitted it.", InvestmentReviewCodes.ActorRequired);
        }

        if (InvestmentReviewRules.Validate(entry) is { } refusal)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "No connection is open, so the proposal cannot be submitted.", InvestmentReviewCodes.NoConnection);
        }

        try
        {
            var now = DateTime.UtcNow;
            var id = Guid.NewGuid().ToString("N");

            var unit = OgUow.For<APPROVAL_WORKFLOW>(source);
            unit.PrimaryKey = "APPROVAL_WORKFLOW_ID";

            unit.Add(new APPROVAL_WORKFLOW
            {
                APPROVAL_WORKFLOW_ID = id,
                ENTITY_NAME = InvestmentEntityName,
                ENTITY_ID = entry.EntityId!.Trim(),
                AMOUNT = entry.Amount!.Value,
                CURRENCY_CODE = entry.Currency!.Trim().ToUpperInvariant(),
                STATUS = InvestmentDecisionCodes.Queued,
                REQUESTED_BY = actor.Trim(),
                REQUESTED_DATE = now,
                PROPOSAL_TITLE = entry.Title!.Trim(),
                INVESTMENT_CLASS = entry.Class!.Trim().ToUpperInvariant(),
                DECISION_DUE = entry.DecisionDue,
                ECONOMICS_ID = string.IsNullOrWhiteSpace(entry.EconomicsId) ? null : entry.EconomicsId.Trim(),
                ECONOMICS_NPV = entry.EconomicsNpv,
                // A fresh proposal is reviewed at its own amount; that is what a later change is compared with (AC-2).
                REVIEWED_AMOUNT = entry.Amount.Value,
                REVIEWED_AT = now,
                RISKS_TEXT = entry.Risks,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await unit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    OgUow.Message(committed, "The proposal was not submitted."),
                    InvestmentReviewCodes.WriteFailed);
            }

            var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInvestmentWorkspace>.Ok(Workspace(
                queue,
                queue.FirstOrDefault(proposal => proposal.Id == id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "submitting an investment proposal",
                "the proposal was not submitted",
                FailureSeverity.Error);

            return OgResult<OgInvestmentWorkspace>.Failed(
                "The proposal could not be submitted.", reference ?? InvestmentReviewCodes.WriteFailed);
        }
    }

    public async Task<OgResult<OgInvestmentWorkspace>> DecideAsync(
        string proposalId,
        string decision,
        string rationale,
        string conditions,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "A decision must record who made it.", InvestmentReviewCodes.ActorRequired);
        }

        var conditionList = InvestmentDecisionCodes.Split(conditions);

        if (InvestmentReviewRules.ValidateDecision(decision, rationale, conditionList) is { } refusal)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(refusal.Message, refusal.Code);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "No connection is open, so the decision cannot be recorded.", InvestmentReviewCodes.NoConnection);
        }

        try
        {
            var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);
            var proposal = Find(queue, proposalId);

            if (proposal is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            // The state table's "already decided - read-only", and its "concurrent decision - loser refused, winner named".
            if (!proposal.IsQueued)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} was already decided by {proposal.DecidedBy ?? "(unknown)"} on " +
                    $"{proposal.DecidedAt?.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) ?? "an unrecorded date"} " +
                    $"({InvestmentDecisionCodes.Describe(proposal.Decision)}); a decided proposal is read-only.",
                    InvestmentReviewCodes.AlreadyDecided);
            }

            // AC-3: the person who submitted the proposal cannot decide it.
            if (!string.IsNullOrWhiteSpace(proposal.SubmittedBy)
                && string.Equals(proposal.SubmittedBy.Trim(), actor.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} was submitted by {proposal.SubmittedBy}, who therefore cannot decide it. " +
                    "A different reviewer must decide.",
                    InvestmentReviewCodes.AuthorCannotDecide);
            }

            // The state table's "economics missing - decision disabled with a link to ECO-01".
            if (!proposal.HasEconomics)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} has no economics linked, so there is nothing to decide on. Run an evaluation " +
                    "(ECO-01) and link it to the proposal first.",
                    InvestmentReviewCodes.EconomicsMissing);
            }

            // AC-2: the amount changed after the review, so the reviewer would be deciding on stale numbers.
            if (proposal.EconomicsStale)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} is now {proposal.AmountText} but was reviewed at " +
                    $"{MoneyText.Of(proposal.ReviewedAmount!.Value)} {proposal.Currency}; review it again before " +
                    "deciding.",
                    InvestmentReviewCodes.StaleEconomics);
            }

            var row = await ReadRowAsync(source, proposal.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            var now = DateTime.UtcNow;

            row.DECISION = decision.Trim().ToUpperInvariant();
            row.RATIONALE = rationale.Trim();
            row.CONDITIONS_TEXT = InvestmentDecisionCodes.Join(conditionList);
            row.DECISION_AMOUNT = row.AMOUNT;
            row.STATUS = InvestmentDecisionCodes.Decided;
            row.APPROVED_BY = actor.Trim();
            row.APPROVAL_DATE = now;
            row.COMMENTS = rationale.Trim();
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = now;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(written, InvestmentReviewCodes.WriteFailed);
            }

            queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInvestmentWorkspace>.Ok(Workspace(
                queue,
                Find(queue, proposal.Id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"recording an investment decision on {proposalId}",
                "the decision was not recorded",
                FailureSeverity.Error);

            return OgResult<OgInvestmentWorkspace>.Failed(
                "The decision could not be recorded.", reference ?? InvestmentReviewCodes.WriteFailed);
        }
    }

    public async Task<OgResult<OgInvestmentWorkspace>> AmendAsync(
        string proposalId,
        decimal amount,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "An amendment must record who made it.", InvestmentReviewCodes.ActorRequired);
        }

        if (amount <= 0m)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "The amount must be above zero.", InvestmentReviewCodes.InputInvalid);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "No connection is open, so the proposal cannot be amended.", InvestmentReviewCodes.NoConnection);
        }

        try
        {
            var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);
            var proposal = Find(queue, proposalId);

            if (proposal is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            if (!proposal.IsQueued)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} is already decided, so its amount cannot change.",
                    InvestmentReviewCodes.AlreadyDecided);
            }

            var row = await ReadRowAsync(source, proposal.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            // The reviewed amount is deliberately left alone: that is what makes the review stale (AC-2).
            row.AMOUNT = amount;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(written, InvestmentReviewCodes.WriteFailed);
            }

            queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInvestmentWorkspace>.Ok(Workspace(queue, Find(queue, proposal.Id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"amending investment proposal {proposalId}",
                "the proposal was not amended",
                FailureSeverity.Error);

            return OgResult<OgInvestmentWorkspace>.Failed(
                "The proposal could not be amended.", reference ?? InvestmentReviewCodes.WriteFailed);
        }
    }

    public async Task<OgResult<OgInvestmentWorkspace>> ReviewAsync(
        string proposalId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "A review must record who made it.", InvestmentReviewCodes.ActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "No connection is open, so the proposal cannot be reviewed.", InvestmentReviewCodes.NoConnection);
        }

        try
        {
            var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);
            var proposal = Find(queue, proposalId);

            if (proposal is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            var row = await ReadRowAsync(source, proposal.Id, cancellationToken).ConfigureAwait(false);

            if (row is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            row.REVIEWED_AMOUNT = row.AMOUNT;
            row.REVIEWED_AT = DateTime.UtcNow;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(written, InvestmentReviewCodes.WriteFailed);
            }

            queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);

            return OgResult<OgInvestmentWorkspace>.Ok(Workspace(queue, Find(queue, proposal.Id)));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"reviewing investment proposal {proposalId}",
                "the review was not recorded",
                FailureSeverity.Error);

            return OgResult<OgInvestmentWorkspace>.Failed(
                "The review could not be recorded.", reference ?? InvestmentReviewCodes.WriteFailed);
        }
    }

    public async Task<OgResult<OgInvestmentWorkspace>> HandoffAsync(
        string proposalId,
        string actor,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "A handoff must record who made it.", InvestmentReviewCodes.ActorRequired);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "No connection is open, so the decision cannot be handed off.", InvestmentReviewCodes.NoConnection);
        }

        try
        {
            var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);
            var proposal = Find(queue, proposalId);

            if (proposal is null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"Proposal '{proposalId}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
            }

            // AC-4: only an authorising decision can become money.
            if (!proposal.IsDecided || !proposal.Authorises)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} is {InvestmentDecisionCodes.Describe(proposal.Decision)}; only an approved or " +
                    "conditionally approved proposal can be handed off.",
                    InvestmentReviewCodes.NotAuthorised);
            }

            var afes = await OgUow.ReadAllAsync<AFE>(source).ConfigureAwait(false);

            // AC-6, the second direction: if the AFE exists but the proposal does not know it yet, adopt it rather than
            // creating a second one. This is the lost-acknowledgement path.
            var existing = afes.FirstOrDefault(afe =>
                string.Equals(afe.SOURCE_PROPOSAL_ID, proposal.Id, StringComparison.OrdinalIgnoreCase));

            if (existing is not null)
            {
                return await RecordHandoffAsync(source, proposal, existing, actor, cancellationToken)
                    .ConfigureAwait(false);
            }

            // AC-4, the first direction: the proposal already knows its AFE, so there is nothing to create.
            if (proposal.IsHandedOff && !string.IsNullOrWhiteSpace(proposal.AfeId))
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    $"{proposal.Title} was already handed off as AFE {proposal.AfeId}; a decision creates exactly one " +
                    "AFE, so no second one is raised.",
                    InvestmentReviewCodes.AlreadyHandedOff);
            }

            var now = DateTime.UtcNow;
            var number = AfeRules.NextNumber(
                afes.Select(afe => new OgAfe(
                    afe.AFE_ID,
                    afe.AFE_NUMBER ?? string.Empty,
                    afe.AFE_NAME ?? string.Empty,
                    afe.AFE_KIND ?? string.Empty,
                    afe.ENTITY_ID,
                    0m, 0m, 0m, 0m,
                    afe.CURRENCY_CODE ?? string.Empty,
                    afe.STATUS ?? string.Empty,
                    afe.AUTHOR,
                    afe.APPROVER,
                    afe.APPROVAL_DATE,
                    afe.CLOSED_AT,
                    afe.CLOSED_BY,
                    afe.CLOSE_NOTE,
                    afe.REVISION_NO,
                    afe.SUPERSEDES_AFE_ID,
                    afe.ROOT_AFE_ID,
                    afe.REVISION_REASON,
                    [],
                    [],
                    afe.DESCRIPTION)).ToList(),
                now.Year);

            var afeId = Guid.NewGuid().ToString("N");

            var afeUnit = OgUow.For<AFE>(source);
            afeUnit.PrimaryKey = "AFE_ID";

            afeUnit.Add(new AFE
            {
                AFE_ID = afeId,
                AFE_NUMBER = number,
                AFE_NAME = proposal.Title,
                AFE_KIND = string.IsNullOrWhiteSpace(proposal.Class) ? "OTHER" : proposal.Class,
                ENTITY_ID = proposal.EntityId,
                PROPERTY_ID = proposal.EntityId,
                FIELD_ID = proposal.EntityId,
                // AC-4 and the feature's "exact reviewed amount and currency".
                ESTIMATED_COST = proposal.Amount,
                APPROVED_AMOUNT = proposal.Amount,
                ACTUAL_COST = 0m,
                CURRENCY_CODE = proposal.Currency,
                STATUS = "APPROVED",
                APPROVAL_DATE = now,
                APPROVER = proposal.DecidedBy,
                AUTHOR = proposal.SubmittedBy,
                DESCRIPTION = "Handed off from investment proposal " + proposal.Title,
                SOURCE_PROPOSAL_ID = proposal.Id,
                // AC-5: the conditions travel with the money.
                CONDITIONS_TEXT = InvestmentDecisionCodes.Join(proposal.Conditions),
                REVISION_NO = 1,
                ROOT_AFE_ID = afeId,
                ACTIVE_IND = "Y",
                PPDM_GUID = Guid.NewGuid().ToString("N"),
                ROW_CREATED_BY = actor.Trim(),
                ROW_CREATED_DATE = now,
                ROW_CHANGED_BY = actor.Trim(),
                ROW_CHANGED_DATE = now,
            });

            var committed = await afeUnit.Commit().ConfigureAwait(false);

            if (!OgUow.IsOk(committed))
            {
                return OgResult<OgInvestmentWorkspace>.Failed(
                    OgUow.Message(committed, "The AFE was not created; nothing was handed off."),
                    InvestmentReviewCodes.WriteFailed);
            }

            return await RecordHandoffAsync(
                source,
                proposal,
                new AFE { AFE_ID = afeId, AFE_NUMBER = number },
                actor,
                cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"handing off investment proposal {proposalId}",
                "the decision was not handed off",
                FailureSeverity.Error);

            return OgResult<OgInvestmentWorkspace>.Failed(
                "The decision could not be handed off.", reference ?? InvestmentReviewCodes.WriteFailed);
        }
    }

    public async Task<OgResult<OgInvestmentWorkspace>> LoadAsync(
        string? proposalId = null,
        CancellationToken cancellationToken = default)
    {
        var source = _source();

        if (source is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                "No connection is open, so the queue cannot be read.", InvestmentReviewCodes.NoConnection);
        }

        try
        {
            var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);

            var selected = string.IsNullOrWhiteSpace(proposalId)
                ? queue.FirstOrDefault(proposal => proposal.IsQueued) ?? queue.FirstOrDefault()
                : Find(queue, proposalId) ?? queue.FirstOrDefault();

            return OgResult<OgInvestmentWorkspace>.Ok(Workspace(queue, selected));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the investment queue",
                "the queue is unavailable; no decision can be recorded",
                FailureSeverity.Degraded);

            return OgResult<OgInvestmentWorkspace>.Ok(Workspace(
                [],
                null,
                true,
                $"the queue could not be read (reference {reference})"));
        }
    }

    // ------------------------------------------------------------------ the writes

    /// <summary>Writes the handoff onto the proposal, then returns the queue with the AFE in view.</summary>
    private async Task<OgResult<OgInvestmentWorkspace>> RecordHandoffAsync(
        IDataSource source,
        OgInvestmentProposal proposal,
        AFE afe,
        string actor,
        CancellationToken cancellationToken)
    {
        var row = await ReadRowAsync(source, proposal.Id, cancellationToken).ConfigureAwait(false);

        if (row is null)
        {
            return OgResult<OgInvestmentWorkspace>.Failed(
                $"Proposal '{proposal.Id}' is not in this database.", InvestmentReviewCodes.UnknownProposal);
        }

        // Idempotent: recording the same AFE twice is not an error, and it is what makes a retry after a lost
        // acknowledgement safe.
        if (!string.Equals(row.AFE_ID, afe.AFE_ID, StringComparison.OrdinalIgnoreCase))
        {
            row.AFE_ID = afe.AFE_ID;
            row.HANDOFF_AT = DateTime.UtcNow;
            row.HANDOFF_BY = actor.Trim();
            row.STATUS = InvestmentDecisionCodes.HandedOff;
            row.ROW_CHANGED_BY = actor.Trim();
            row.ROW_CHANGED_DATE = DateTime.UtcNow;

            var written = await UpdateAsync(source, row).ConfigureAwait(false);

            if (written is not null)
            {
                return OgResult<OgInvestmentWorkspace>.Failed(written, InvestmentReviewCodes.WriteFailed);
            }
        }

        var queue = await ReadQueueAsync(source, cancellationToken).ConfigureAwait(false);

        return OgResult<OgInvestmentWorkspace>.Ok(Workspace(queue, Find(queue, proposal.Id)));
    }

    private static async Task<string?> UpdateAsync(IDataSource source, APPROVAL_WORKFLOW row)
    {
        var unit = OgUow.For<APPROVAL_WORKFLOW>(source);
        unit.PrimaryKey = "APPROVAL_WORKFLOW_ID";

        var updated = unit.Update(row);

        if (!OgUow.IsOk(updated))
        {
            return OgUow.Message(updated, "The proposal was not saved.");
        }

        var committed = await unit.Commit().ConfigureAwait(false);

        return OgUow.IsOk(committed) ? null : OgUow.Message(committed, "The proposal was not saved.");
    }

    private static async Task<APPROVAL_WORKFLOW?> ReadRowAsync(
        IDataSource source,
        string proposalId,
        CancellationToken cancellationToken)
    {
        var unit = OgUow.For<APPROVAL_WORKFLOW>(source);
        unit.PrimaryKey = "APPROVAL_WORKFLOW_ID";

        var rows = await OgUow.ReadAsync(
            unit,
            [OgUow.Equal("APPROVAL_WORKFLOW_ID", proposalId)],
            cancellationToken).ConfigureAwait(false);

        return rows.FirstOrDefault();
    }

    // ------------------------------------------------------------------ the read

    private static async Task<IReadOnlyList<OgInvestmentProposal>> ReadQueueAsync(
        IDataSource source,
        CancellationToken cancellationToken)
    {
        var rows = await OgUow.ReadAllAsync<APPROVAL_WORKFLOW>(source).ConfigureAwait(false);

        cancellationToken.ThrowIfCancellationRequested();

        return rows
            .Where(row => !string.IsNullOrWhiteSpace(row.APPROVAL_WORKFLOW_ID))
            .Where(row => !string.Equals(row.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
            // Only the investment approvals belong in this queue; the same table holds other workflows.
            .Where(row => string.Equals(row.ENTITY_NAME, InvestmentEntityName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(row => row.DECISION_DUE ?? DateTime.MaxValue)
            .ThenBy(row => row.REQUESTED_DATE ?? DateTime.MinValue)
            .Select(row => new OgInvestmentProposal(
                row.APPROVAL_WORKFLOW_ID,
                row.PROPOSAL_TITLE ?? row.COMMENTS ?? "(untitled)",
                row.ENTITY_ID,
                row.AMOUNT,
                row.CURRENCY_CODE ?? string.Empty,
                row.INVESTMENT_CLASS ?? string.Empty,
                row.REQUESTED_BY,
                row.REQUESTED_DATE,
                row.DECISION_DUE,
                row.STATUS ?? InvestmentDecisionCodes.Queued,
                row.DECISION,
                row.RATIONALE,
                InvestmentDecisionCodes.Split(row.CONDITIONS_TEXT),
                row.APPROVED_BY,
                row.APPROVAL_DATE,
                row.DECISION_AMOUNT,
                row.ECONOMICS_ID,
                row.ECONOMICS_NPV,
                row.REVIEWED_AMOUNT,
                row.REVIEWED_AT,
                row.RISKS_TEXT,
                row.AFE_ID,
                row.HANDOFF_AT,
                row.HANDOFF_BY))
            .Take(200)
            .ToList();
    }

    private static OgInvestmentProposal? Find(IReadOnlyList<OgInvestmentProposal> queue, string? proposalId) =>
        string.IsNullOrWhiteSpace(proposalId)
            ? null
            : queue.FirstOrDefault(proposal =>
                string.Equals(proposal.Id, proposalId.Trim(), StringComparison.OrdinalIgnoreCase));

    private static OgInvestmentWorkspace Workspace(
        IReadOnlyList<OgInvestmentProposal> queue,
        OgInvestmentProposal? selected,
        bool historyFailed = false,
        string? historyReason = null) =>
        new(
            queue,
            selected,
            InvestmentReviewRules.Classes,
            InvestmentDecisionCodes.Decisions,
            InvestmentReviewRules.Currencies,
            historyFailed,
            historyReason);
}
