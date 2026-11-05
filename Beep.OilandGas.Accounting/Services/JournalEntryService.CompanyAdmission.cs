using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    internal static void RequireUnownedGLEntry(GL_ENTRY row)
    {
        if(row.OWNER_TENANT_ID is not null || row.OWNER_COMPANY_BA_ID is not null || row.BOOK_ID is not null || row.FIELD_ID is not null || row.CURRENCY_CODE is not null || row.SOURCE_AR_INVOICE_ID is not null || row.COMPANY_LEDGER_PERIOD_ID is not null || row.JOURNAL_ENTRY_LINE_ID is not null || row.SALES_JOURNAL_POSTING_INTENT_ID is not null || row.MAPPING_KEY is not null)
            throw new InvalidOperationException("Company-owned GL evidence requires its original verified posting workflow; companyless legacy reads or balances are unavailable.");
    }
    private static void RequireUnownedJournal(JOURNAL_ENTRY row)
    {
        if(row.OWNER_TENANT_ID is not null || row.OWNER_COMPANY_BA_ID is not null || row.BOOK_ID is not null || row.FIELD_ID is not null || row.CURRENCY_CODE is not null || row.SOURCE_AR_INVOICE_ID is not null || row.COMPANY_LEDGER_PERIOD_ID is not null || row.PREPARATION_WORKFLOW_JSON is not null || row.REVIEW_WORKFLOW_JSON is not null || row.POSTING_WORKFLOW_JSON is not null || row.SALES_JOURNAL_POSTING_INTENT_ID is not null)
            throw new InvalidOperationException("Company-owned journal evidence requires its original source workflow; a companyless legacy read or mutation is unavailable.");
    }
    private static void RequireUnownedJournalLine(JOURNAL_ENTRY_LINE row)
    {
        if(row.OWNER_TENANT_ID is not null || row.OWNER_COMPANY_BA_ID is not null || row.BOOK_ID is not null || row.FIELD_ID is not null || row.SOURCE_AR_INVOICE_ID is not null || row.COMPANY_LEDGER_PERIOD_ID is not null || row.MAPPING_KEY is not null)
            throw new InvalidOperationException("Company-owned journal line evidence requires its original source workflow; a companyless legacy read or mutation is unavailable.");
    }
}
