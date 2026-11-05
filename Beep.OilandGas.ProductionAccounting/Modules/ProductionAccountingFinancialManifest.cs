using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.ProductionAccounting.Modules;

/// <summary>Native financial types contributed to the sole PRODUCTION migration owner.</summary>
public static class ProductionAccountingFinancialManifest
{
    public static IReadOnlyList<Type> EntityTypes {get;}=Array.AsReadOnly(new[]{
        typeof(GL_ACCOUNT),typeof(GLAccountMapping),typeof(GL_ENTRY),typeof(JOURNAL_ENTRY),typeof(JOURNAL_ENTRY_LINE),typeof(AR_INVOICE),typeof(AR_PAYMENT),typeof(AR_CREDIT_MEMO),typeof(Beep.OilandGas.Models.Data.Accounting.COMPANY_LEDGER_PERIOD),typeof(Beep.OilandGas.Models.Data.Accounting.COMPANY_LEDGER_BOOK),typeof(FX_RATE),typeof(Beep.OilandGas.Accounting.Models.SALES_PAYMENT_REVIEW_INTENT),typeof(Beep.OilandGas.Models.Data.Accounting.SALES_JOURNAL_REVIEW_INTENT),typeof(Beep.OilandGas.Models.Data.Accounting.SALES_JOURNAL_POSTING_INTENT),typeof(Beep.OilandGas.Accounting.Models.SALES_JOURNAL_POSTING_OUTCOME)});
}
