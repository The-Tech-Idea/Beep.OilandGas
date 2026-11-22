using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.ProductionAccounting.Constants;

/// <summary>
/// The AFE lifecycle: every state an authorisation for expenditure can be in, and every transition between them
/// (ECO-02).
/// </summary>
/// <remarks>
/// The feature's stated risk is that the AFE has five partials and an unclear state machine, and the extraction confirms
/// it: <see cref="Services.AfeService"/> only ever writes two of them - <c>CreateAfeAsync</c> sets
/// <see cref="DocumentWorkflowStatusCodes.Draft"/> and <c>ApproveAfeAsync</c> sets
/// <see cref="ApprovalWorkflowStatusCodes.Approved"/> - and those two are also the only rows in the <c>AFE_STATUS</c>
/// reference seed. There is no submit, no rejection, no supplement and no closure anywhere in the project. The full set is
/// therefore stated here, once, so the screen cannot invent a transition the owner does not have a name for; the two the
/// owner already uses are taken from <see cref="AfeStatusCodes"/> rather than respelled.
///
/// A supplement is not a transition of the approved row: it is a <b>new revision</b> of the same AFE number, and the
/// approved row is never written again (ECO-02 AC-1, SG-09-4). <see cref="Transition"/> therefore lists the four moves a
/// single row can make, and <see cref="StartsRevision"/> names the one action that creates another row.
/// </remarks>
public static class AfeLifecycle
{
    /// <summary>Raised but not yet submitted for approval.</summary>
    public const string Draft = AfeStatusCodes.Draft;

    /// <summary>Submitted, waiting for an approver.</summary>
    public const string Submitted = "SUBMITTED";

    /// <summary>Approved, and therefore the effective authorisation for its revision.</summary>
    public const string Approved = AfeStatusCodes.Approved;

    /// <summary>Sent back by the approver; the author may revise and submit it again.</summary>
    public const string Returned = "RETURNED";

    /// <summary>Closed: the work is done and the AFE is read-only.</summary>
    public const string Closed = "CLOSED";

    /// <summary>Every state, in lifecycle order.</summary>
    public static IReadOnlyList<string> All { get; } = [Draft, Submitted, Approved, Returned, Closed];

    /// <summary>One legal move: from, to, and the action that makes it.</summary>
    /// <param name="From">The state the row is in.</param>
    /// <param name="To">The state it moves to.</param>
    /// <param name="Action">The command that performs it.</param>
    /// <param name="Actor">Who may perform it.</param>
    public sealed record AfeTransition(string From, string To, string Action, string Actor);

    /// <summary>
    /// Every move a single AFE row can make.
    /// </summary>
    /// <remarks>
    /// A supplement is deliberately absent: it creates a new revision instead of moving the approved row, which is what
    /// AC-1 requires. <see cref="StartsRevision"/> is that action.
    /// </remarks>
    public static IReadOnlyList<AfeTransition> Transition { get; } =
    [
        new(Draft, Submitted, "submit", "the author"),
        new(Submitted, Approved, "approve", "an approver who is not the author"),
        new(Submitted, Returned, "return", "an approver"),
        new(Returned, Draft, "revise", "the author"),
        new(Approved, Closed, "close", "the author"),
    ];

    /// <summary>The action that raises a new revision of an approved AFE rather than moving it.</summary>
    public const string StartsRevision = "supplement";

    /// <summary>The states a row may be written from at all - every state but <see cref="Closed"/>.</summary>
    public static bool IsOpen(string? status) =>
        !string.Equals(status?.Trim(), Closed, StringComparison.OrdinalIgnoreCase);

    /// <summary>Whether a move is legal, and if not, which move would be.</summary>
    public static bool CanTransition(string? from, string? to) =>
        Transition.Any(transition =>
            string.Equals(transition.From, from?.Trim(), StringComparison.OrdinalIgnoreCase)
            && string.Equals(transition.To, to?.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>The moves available from a state, for the sentence a refused command shows.</summary>
    public static IReadOnlyList<string> AvailableFrom(string? from) =>
        Transition
            .Where(transition => string.Equals(transition.From, from?.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(transition => transition.Action + " (" + transition.To + ")")
            .ToList();

    /// <summary>A state's name for a person.</summary>
    public static string Describe(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        Draft => "draft",
        Submitted => "submitted for approval",
        Approved => "approved and effective",
        Returned => "returned to the author",
        Closed => "closed and read-only",
        _ => status ?? "(no status)",
    };

    /// <summary>
    /// Whether a revision is the effective authorisation: only an approved revision is, which is what makes a supplement
    /// inert until it is itself approved (AC-6).
    /// </summary>
    public static bool IsEffective(string? status) =>
        string.Equals(status?.Trim(), Approved, StringComparison.OrdinalIgnoreCase);
}
