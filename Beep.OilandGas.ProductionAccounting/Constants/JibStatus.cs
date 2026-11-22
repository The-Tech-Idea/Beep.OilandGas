using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Beep.OilandGas.ProductionAccounting.Constants;

/// <summary>
/// The joint-interest billing states (ACC-07), which the owner had no vocabulary for.
/// </summary>
/// <remarks>
/// <c>JOINT_INTEREST_BILL.BILL_STATUS</c> exists as a column but nothing in the owner states its values, and
/// <c>JOINT_INTEREST_STATEMENT</c> has no status at all. The four states below are the ones this feature's commands need -
/// raised, independently approved, posted, or returned with a reason - and they are stated here so the screen and the
/// service cannot disagree.
/// </remarks>
public static class JibStatus
{
    /// <summary>Built and not yet approved.</summary>
    public const string Draft = "DRAFT";

    /// <summary>Approved by someone other than the author; it may now be posted.</summary>
    public const string Approved = "APPROVED";

    /// <summary>Posted: the receivable and the cost recovery are in the ledger.</summary>
    public const string Posted = "POSTED";

    /// <summary>Sent back to the author with a reason.</summary>
    public const string Returned = "RETURNED";

    /// <summary>Every JIB state.</summary>
    public static readonly string[] States = [Draft, Approved, Posted, Returned];
}

/// <summary>
/// The royalty dispute states (ACC-07), as the reference says - the dispute reference is what blocks the payment.
/// </summary>
/// <remarks>
/// The owner's <c>ROYALTY_DISPUTE</c> record is keyed to <c>ROYALTY_STATEMENT</c>, a table this app never populates, so the
/// dispute is recorded against the <c>ROYALTY_CALCULATION</c> that is actually paid - with the reference AC-3 asks for. The
/// two states below are its lifecycle.
/// </remarks>
public static class RoyaltyDisputeStatus
{
    /// <summary>Open: the royalty cannot be paid while it is in this state.</summary>
    public const string Open = "OPEN";

    /// <summary>Resolved: the payment may proceed.</summary>
    public const string Resolved = "RESOLVED";

    /// <summary>Every dispute state.</summary>
    public static readonly string[] States = [Open, Resolved];
}
