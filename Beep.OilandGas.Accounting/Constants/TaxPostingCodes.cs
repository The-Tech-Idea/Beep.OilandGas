using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Accounting.Constants;

/// <summary>
/// The accounts a tax computation posts to (ACC-08), and the codes that say what a computation is.
/// </summary>
/// <remarks>
/// These are the postings the owner already makes in <c>TaxProvisionService</c> - income tax expense against income tax
/// payable for a current provision, and the deferred balances against income tax expense for the movement - plus the
/// production-tax pair, which the owner's service records as a tax transaction without a posting of its own. Each number is
/// resolved against the configured chart and the command is refused naming the account when it is missing, rather than
/// posting to a number that is not there.
/// </remarks>
public static class TaxPostingCodes
{
    /// <summary>Income tax expense - the debit side of a current provision (the owner's own posting).</summary>
    public const string IncomeTaxExpenseAccount = DefaultGlAccounts.IncomeTaxExpense;

    /// <summary>Income tax payable - the credit side of a current provision (the owner's own posting).</summary>
    public const string IncomeTaxPayableAccount = DefaultGlAccounts.IncomeTaxPayable;

    /// <summary>Deferred tax asset - debited when the asset increases.</summary>
    public const string DeferredTaxAssetAccount = DefaultGlAccounts.DeferredTaxAsset;

    /// <summary>Deferred tax liability - credited when the liability increases.</summary>
    public const string DeferredTaxLiabilityAccount = DefaultGlAccounts.DeferredTaxLiability;

    /// <summary>
    /// Production and severance tax expense. The configured chart has no dedicated production-tax account, so the operating
    /// expense account is used and the choice is stated here rather than left to a caller.
    /// </summary>
    public const string ProductionTaxExpenseAccount = DefaultGlAccounts.OperatingExpense;

    /// <summary>
    /// Amounts owed to the taxing authority for production tax. The chart's payable account is used because a severance tax
    /// is collected by the operator and remitted, which is the same liability shape.
    /// </summary>
    public const string ProductionTaxPayableAccount = DefaultGlAccounts.AccountsPayable;

    /// <summary>The currencies a computation may state.</summary>
    public static IReadOnlyList<string> Currencies { get; } = ["USD", "EUR", "GBP", "SAR", "AED"];
}
