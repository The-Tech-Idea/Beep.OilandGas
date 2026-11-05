using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public async Task<CompanySalesJournalPostingRecovery> GetCompanySalesJournalPostingRecoveryAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original posting recovery owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original posting recovery connection is unavailable.");var source=_editor.GetDataSource(connection)??throw new InvalidOperationException("The original posting recovery datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original posting recovery binding changed.");RequireCompanyJournalPostingSchema(source);}
        async Task<CompanySalesJournalPostingRecovery> Capture()
        {
            await Guard();var journal=await GetCompanySalesJournalForReviewAsync(company,actor,book,currency,id,Guard,requireFieldRead)??throw new InvalidOperationException("The original posting journal is unavailable.");
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_JOURNAL_POSTING_INTENT),connection,"SALES_JOURNAL_POSTING_INTENT");var rows=(await repo.GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();if(rows is null || rows.Length>1 || rows.Any(value=>value is not SALES_JOURNAL_POSTING_INTENT))throw new InvalidOperationException("The unique retained posting recovery is unavailable or ambiguous.");var intent=rows.SingleOrDefault() as SALES_JOURNAL_POSTING_INTENT;
            if(intent is not null && (intent.OWNER_TENANT_ID!=company.TenantId || intent.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || intent.BOOK_ID!=book || intent.CURRENCY_CODE!=currency || intent.ROW_CREATED_BY!=actor))throw new UnauthorizedAccessException("Only the original posting producer can recover the request in its company/book/currency.");
            var outcome=journal.Header.STATUS=="POSTED"?await ConfirmNativeJournalPostingAsync(journal.Header,connection,source,Guard,requireFieldRead):null;var result=new CompanySalesJournalPostingRecovery(company,actor,id,intent,outcome);result.Require();await Guard();return CompanyInventorySourceReview.Copy(result);
        }
        var first=await Capture();var closing=await Capture();if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete native posting recovery changed.");await Guard();return closing;
    }
}
