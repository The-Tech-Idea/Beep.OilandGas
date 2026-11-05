using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesPostedLedgerReview> GetCompanySalesPostedLedgerAsync(CompanyScope company,string actor,string bindingRevision,string book,string currency,string receivableId,ARService ar,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)
        =>await ReadCompanySalesPostedLedgerAsync(company,actor,bindingRevision,book,currency,receivableId,ar,requireAuthority,requireFieldRead,true);
    // Payment's enclosing capture compares the entire ledger/AR/payment evidence twice.
    // Keep fresh native reads and authority checks, without nesting another whole-source pair.
    internal Task<CompanySalesPostedLedgerReview> CaptureCompanySalesPostedLedgerAsync(CompanyScope company,string actor,string bindingRevision,string book,string currency,string receivableId,ARService ar,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)
        =>ReadCompanySalesPostedLedgerAsync(company,actor,bindingRevision,book,currency,receivableId,ar,requireAuthority,requireFieldRead,false);
    private async Task<CompanySalesPostedLedgerReview> ReadCompanySalesPostedLedgerAsync(CompanyScope company,string actor,string bindingRevision,string book,string currency,string receivableId,ARService ar,Func<Task> requireAuthority,Func<string,Task> requireFieldRead,bool compareCaptures)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();ArgumentNullException.ThrowIfNull(ar);ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("The request-bound native company ledger reader is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original ledger connection is unavailable.");var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original ledger datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original native ledger binding changed.");await ar.RequireJournalSourceBindingAsync(connection,source);RequireCompanyJournalPostingSchema(source);}
        async Task<CompanySalesPostedLedgerReview> Capture()
        {
            await Guard();var arRow=await ar.GetCompanySalesReceivableForReviewAsync(company,receivableId,Guard,requireFieldRead);await requireFieldRead(arRow.Receivable.FIELD_ID!);await Guard();var field=arRow.Receivable.FIELD_ID!;
            var correlation=await ar.ReadCompanyJournalPostingCorrelationAsync(company,book,currency,receivableId,connection,source,Guard);
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(JOURNAL_ENTRY),connection,"JOURNAL_ENTRY");var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="BOOK_ID",Operator="=",FilterValue=book},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=currency},new(){FieldName="SOURCE_AR_INVOICE_ID",Operator="=",FilterValue=receivableId}]))?.ToArray();await Guard();
            if(raw is null || raw.Length>1 || raw.Any(row=>row is not JOURNAL_ENTRY))throw new InvalidOperationException("The unique original AR/book/currency journal reservation is unavailable or ambiguous.");var header=raw.SingleOrDefault() as JOURNAL_ENTRY;CompanySalesJournalPostingOutcome? outcome=null;
            if(header is not null)
            {
                if(header.FIELD_ID!=field)throw new InvalidOperationException("The original native journal and AR FIELD differ.");await requireFieldRead(field);var journal=await GetCompanySalesJournalForReviewAsync(company,actor,book,currency,header.JOURNAL_ENTRY_ID,Guard,requireFieldRead)??throw new InvalidOperationException("The complete original ledger journal disappeared.");
                if(journal.Header.STATUS=="POSTED"){if(correlation is null)throw new InvalidOperationException("The native posted journal lacks its AR posting correlation.");outcome=await ConfirmNativeJournalPostingAsync(journal.Header,connection,source,Guard,requireFieldRead);if(JsonSerializer.Serialize(correlation.Restore())!=JsonSerializer.Serialize(outcome))throw new InvalidOperationException("The native AR correlation disagrees with the complete posted journal/GL outcome.");}
                else if(correlation is not null)throw new InvalidOperationException("An AR posting correlation cannot borrow an unposted native journal.");
            }
            else if(correlation is not null)throw new InvalidOperationException("The native AR posting correlation lacks its original journal.");
            var result=CompanySalesPostedLedgerReview.Create(company,actor,bindingRevision,book,currency,receivableId,field,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(arRow.Receivable)),outcome);await requireFieldRead(field);await Guard();return result;
        }
        var first=await Capture();await Guard();if(!compareCaptures)return first;var closing=await Capture();await Guard();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete public native ledger projection changed during read-back.");return closing;
    }
}
