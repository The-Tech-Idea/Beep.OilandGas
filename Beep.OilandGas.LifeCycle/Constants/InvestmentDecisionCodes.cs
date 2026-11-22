using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.LifeCycle.Constants;

/// <summary>
/// The investment review queue's states and decisions (ECO-03), and the rules that keep a decision honest.
/// </summary>
/// <remarks>
/// The proposal is an <c>APPROVAL_WORKFLOW</c> row - the owner's generic approval record, which already holds the entity,
/// the amount, who requested it, who approved it and when - so the queue's state lives in that row's <c>STATUS</c> and the
/// decision itself in <c>DECISION</c>. One row is both the proposal and its decision because the feature makes a decision
/// final: "already decided - read-only", and a concurrent decision's loser is refused rather than creating a second row.
/// </remarks>
public static class InvestmentDecisionCodes
{
    /// <summary>Submitted and waiting for a decision.</summary>
    public const string Queued = "QUEUED";

    /// <summary>A decision has been recorded and the proposal is read-only.</summary>
    public const string Decided = "DECIDED";

    /// <summary>The approved decision has been handed off as an AFE.</summary>
    public const string HandedOff = "HANDED_OFF";

    /// <summary>Every queue state, in order.</summary>
    public static IReadOnlyList<string> States { get; } = [Queued, Decided, HandedOff];

    /// <summary>Approved with no conditions: the money may be committed.</summary>
    public const string Approved = "APPROVED";

    /// <summary>Approved subject to conditions, which travel to the AFE (AC-5).</summary>
    public const string Conditional = "CONDITIONAL";

    /// <summary>Sent back to the author; no money may be committed.</summary>
    public const string Returned = "RETURNED";

    /// <summary>Rejected; no money may be committed.</summary>
    public const string Rejected = "REJECTED";

    /// <summary>Every decision a reviewer can record.</summary>
    public static IReadOnlyList<string> Decisions { get; } = [Approved, Conditional, Returned, Rejected];

    /// <summary>The decisions that authorise money - the only ones a handoff may follow (AC-4).</summary>
    public static IReadOnlyList<string> Authorising { get; } = [Approved, Conditional];

    /// <summary>Whether a decision authorises an AFE.</summary>
    public static bool Authorises(string? decision) =>
        Authorising.Contains(decision?.Trim().ToUpperInvariant() ?? string.Empty, StringComparer.Ordinal);

    /// <summary>Whether a decision needs at least one condition, because it approved subject to them (AC-5).</summary>
    public static bool NeedsConditions(string? decision) =>
        string.Equals(decision?.Trim(), Conditional, StringComparison.OrdinalIgnoreCase);

    /// <summary>A decision's name for a person.</summary>
    public static string Describe(string? decision) => decision?.Trim().ToUpperInvariant() switch
    {
        Approved => "approved",
        Conditional => "approved with conditions",
        Returned => "returned to the author",
        Rejected => "rejected",
        _ => decision ?? "(no decision)",
    };

    /// <summary>
    /// The conditions as they are stored and carried to the AFE: one per line, trimmed, blank lines dropped.
    /// </summary>
    public static IReadOnlyList<string> Split(string? conditions) =>
        string.IsNullOrWhiteSpace(conditions)
            ? []
            : conditions
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList();

    /// <summary>The inverse, so a list can be stored back.</summary>
    public static string? Join(IReadOnlyList<string> conditions) =>
        conditions is null || conditions.Count == 0
            ? null
            : string.Join("\n", conditions.Select(condition => condition.Trim()).Where(condition => condition.Length > 0));
}

/// <summary>The codes a refused investment action reports (ECO-03).</summary>
public static class InvestmentReviewCodes
{
    public const string ActorRequired = "economics.investment-actor-required";
    public const string InputRequired = "economics.investment-input-required";
    public const string InputInvalid = "economics.investment-input-invalid";
    public const string UnknownProposal = "economics.investment-unknown";
    public const string AlreadyDecided = "economics.investment-already-decided";
    public const string AuthorCannotDecide = "economics.investment-author-cannot-decide";
    public const string EconomicsMissing = "economics.investment-economics-missing";
    public const string StaleEconomics = "economics.investment-stale-economics";
    public const string ConditionsRequired = "economics.investment-conditions-required";
    public const string NotAuthorised = "economics.investment-not-authorised";
    public const string AlreadyHandedOff = "economics.investment-already-handed-off";
    public const string NoConnection = "economics.investment-no-connection";
    public const string ReadFailed = "economics.investment-read-failed";
    public const string WriteFailed = "economics.investment-write-failed";
}
