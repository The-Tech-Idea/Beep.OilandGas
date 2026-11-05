using Beep.OilandGas.Accounting.Models;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.ProductionAccounting.Modules;

/// <summary>Native financial types contributed to the sole PRODUCTION migration owner.</summary>
public static class ProductionAccountingFinancialManifest
{
    public static IReadOnlyList<Type> EntityTypes {get;}=Array.AsReadOnly(new[]{
        typeof(GL_ACCOUNT),typeof(GLAccountMapping),typeof(GL_ENTRY),typeof(JOURNAL_ENTRY),typeof(JOURNAL_ENTRY_LINE),typeof(AR_INVOICE),typeof(AR_PAYMENT),typeof(AR_CREDIT_MEMO),typeof(TheTechIdea.Data.OilGas.COMPANY_LEDGER_PERIOD),typeof(TheTechIdea.Data.OilGas.COMPANY_LEDGER_BOOK),typeof(FX_RATE),typeof(TheTechIdea.Data.OilGas.SALES_PAYMENT_REVIEW_INTENT),typeof(TheTechIdea.Data.OilGas.SALES_JOURNAL_REVIEW_INTENT),typeof(TheTechIdea.Data.OilGas.SALES_JOURNAL_POSTING_INTENT),typeof(TheTechIdea.Data.OilGas.SALES_JOURNAL_POSTING_OUTCOME)});
}
