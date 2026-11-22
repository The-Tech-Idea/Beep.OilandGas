using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.Accounting.Constants;

/// <summary>
/// The accounts a royalty and a joint-interest bill post to (ACC-07, SG-10-7), and the methods an allocation may state.
/// </summary>
/// <remarks>
/// "Post to the configured accounts" is read as the accounts this app is configured with - the owner's
/// <see cref="DefaultGlAccounts"/> chart - and the service resolves each number against the chart of accounts and
/// <b>refuses naming the account</b> when it is missing, rather than posting to a number that is not there. The owner already
/// carries <see cref="DefaultGlAccounts.RoyaltyExpense"/> and <see cref="DefaultGlAccounts.AccruedRoyalties"/> for exactly
/// this purpose; the JIB sides use the receivable and the cost it recovers.
/// </remarks>
public static class RoyaltyJibCodes
{
    /// <summary>Royalty expense - the debit side of an accrued royalty.</summary>
    public const string RoyaltyExpenseAccount = DefaultGlAccounts.RoyaltyExpense;

    /// <summary>Accrued royalties - the liability an unpaid royalty sits in until it is paid.</summary>
    public const string AccruedRoyaltyAccount = DefaultGlAccounts.AccruedRoyalties;

    /// <summary>Cash - the credit side of a royalty payment.</summary>
    public const string CashAccount = DefaultGlAccounts.Cash;

    /// <summary>Accounts receivable - the debit side of a joint-interest bill to a partner.</summary>
    public const string JibReceivableAccount = DefaultGlAccounts.AccountsReceivable;

    /// <summary>Operating expense - the cost a joint-interest bill recovers from the partners.</summary>
    public const string JibCostAccount = DefaultGlAccounts.OperatingExpense;

    /// <summary>
    /// The allocation methods an operator may state, taken from the owner's own <c>AllocationMethod</c> enumeration so the
    /// screen cannot invent a method the model does not carry.
    /// </summary>
    public static IReadOnlyList<string> AllocationMethods { get; } =
        Enum.GetNames<AllocationMethod>();

    /// <summary>What a cost allocation target may be.</summary>
    public static IReadOnlyList<string> TargetTypes { get; } = ["PROPERTY", "WELL", "COST_CENTRE"];
}
