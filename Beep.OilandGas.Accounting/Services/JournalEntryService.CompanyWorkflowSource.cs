using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesJournalWorkflowSource> GetCompanySalesJournalWorkflowSourceAsync(CompanyScope company,string actor,string bindingRevision,string id,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)
    {
        using var validation=CompanySalesInvoiceValidation.Enter();PurchaseOrderOwnership.RequireScope(company);if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original journal workflow identity is required.");ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound journal workflow source owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original journal workflow connection is unavailable.");var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original journal workflow datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original journal workflow binding changed.");RequireCompanyJournalSchema(source);}
        async Task<CompanySalesJournalWorkflowSource> Capture()
        {
            await Guard();var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(JOURNAL_ENTRY),connection,"JOURNAL_ENTRY");var rows=(await repo.GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();
            if(rows is not {Length:1} || rows[0] is not JOURNAL_ENTRY header || header.OWNER_TENANT_ID!=company.TenantId || header.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || string.IsNullOrWhiteSpace(header.FIELD_ID) || string.IsNullOrWhiteSpace(header.BOOK_ID) || string.IsNullOrWhiteSpace(header.CURRENCY_CODE))throw new InvalidOperationException("The exact owned journal and its recorded book/currency/FIELD context are unavailable.");
            await requireFieldRead(header.FIELD_ID);await Guard();var journal=await GetCompanySalesJournalForReviewAsync(company,actor,header.BOOK_ID,header.CURRENCY_CODE,id,Guard,requireFieldRead)??throw new InvalidOperationException("The complete native journal workflow source disappeared.");if(JsonSerializer.Serialize(header)!=JsonSerializer.Serialize(journal.Header))throw new InvalidOperationException("The original journal workflow context changed during source read.");
            var result=CompanySalesJournalWorkflowSource.Create(company,actor,bindingRevision,journal);await requireFieldRead(header.FIELD_ID);await Guard();return result;
        }
        var first=await Capture();var closing=await Capture();first.Require(company,actor,id);closing.Require(company,actor,id);if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete native journal workflow source changed during capture.");await Guard();return closing;
    }
}
