using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class JournalEntryService
{
    public Task<CompanySalesJournalPreparationOutcome?> GetCompanySalesJournalPreparationAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)=>ReadCompanyJournalAsync(company,actor,book,currency,id,requireAuthority,requireFieldRead,true);
    public Task<CompanySalesJournalPreparationOutcome?> GetCompanySalesJournalForReviewAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority,Func<string,Task> requireFieldRead)=>ReadCompanyJournalAsync(company,actor,book,currency,id,requireAuthority,requireFieldRead,false);
    private async Task<CompanySalesJournalPreparationOutcome?> ReadCompanyJournalAsync(CompanyScope company,string actor,string book,string currency,string id,Func<Task> requireAuthority,Func<string,Task> requireFieldRead,bool producerOnly)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);ArgumentNullException.ThrowIfNull(requireFieldRead);_=CompanyGLAccountReview.Create(company,actor,book,currency,[]);
        if(!Guid.TryParseExact(id,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=id)throw new ArgumentException("The original native journal identity is required.");
        await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original request-bound PRODUCTION recovery owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original preparation recovery binding is unavailable.");var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original preparation datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original preparation recovery binding changed.");RequireCompanyJournalSchema(source);}
        async Task<CompanySalesJournalPreparationOutcome?> Capture()
        {
            await Guard();var repository=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(JOURNAL_ENTRY),connection,"JOURNAL_ENTRY");
            var raw=(await repository.GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();
            if(raw is null || raw.Length>1 || raw.Any(row=>row is not JOURNAL_ENTRY))throw new InvalidOperationException("The original journal outcome is unavailable or ambiguous.");if(raw.SingleOrDefault() is not JOURNAL_ENTRY header)return null;
            if(header.OWNER_TENANT_ID!=company.TenantId || header.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || header.BOOK_ID!=book || header.CURRENCY_CODE!=currency || producerOnly && header.ROW_CREATED_BY!=actor)throw new UnauthorizedAccessException("The original journal company/book/currency or producer recovery scope is unavailable.");
            if(string.IsNullOrWhiteSpace(header.FIELD_ID))throw new InvalidOperationException("The original preparation FIELD is unavailable.");await requireFieldRead(header.FIELD_ID);await Guard();await RequireActualJournalPostingAsync(header,connection,source,Guard,requireFieldRead);await RequireActualJournalReviewerAsync(header,connection,source,Guard);var proof=CompanySalesJournalPreparationProof.Require(header);
            if(proof.Command.Source.Company!=company || producerOnly && proof.Command.Source.ActorUserId!=actor || proof.Command.JournalId!=id)throw new UnauthorizedAccessException();
            var lineRepo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(JOURNAL_ENTRY_LINE),connection,"JOURNAL_ENTRY_LINE");var rows=(await lineRepo.GetAsync([new(){FieldName="JOURNAL_ENTRY_ID",Operator="=",FilterValue=id}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>4 || rows.Any(row=>row is not JOURNAL_ENTRY_LINE))throw new InvalidOperationException("The complete original journal line outcome is unavailable.");
            var result=new CompanySalesJournalPreparationOutcome(header,rows.Cast<JOURNAL_ENTRY_LINE>().OrderBy(row=>row.LINE_NUMBER).ToArray());result.Require(proof.Command);await requireFieldRead(header.FIELD_ID);await Guard();return CompanyInventorySourceReview.Copy(result);
        }
        var initial=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The complete original journal preparation changed during recovery.");return closing;
    }
}
