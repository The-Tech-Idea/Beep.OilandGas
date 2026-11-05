using Beep.OilandGas.PPDM39.Core.Security;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;
public partial class ARService
{
    internal static async Task ConfirmNativePostingReceivableAsync(CompanyScope company,AR_INVOICE expected,IDataSource source,Func<Type,string,PPDMGenericRepository> repository,Func<Task> guard,Func<string,Task> fieldRead)
    {
        await guard();ReceivableSchema(source,false);ReceivableSchema(source,true);ReceivableReceiptSchema(source);ReceivableReviewSchema(source);
        async Task<T> One<T>(string table,string key,string value)
        {
            await guard();var rows=(await repository(typeof(T),table).GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName=key,Operator="=",FilterValue=value}]))?.ToArray();await guard();if(rows is not {Length:1} || rows[0] is not T row)throw new InvalidOperationException("The actual original native AR posting artifact is missing or ambiguous.");return row;
        }
        var current=await One<AR_INVOICE>("AR_INVOICE","AR_INVOICE_ID",expected.AR_INVOICE_ID);await fieldRead(current.FIELD_ID!);await guard();if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The exact native AR posting row changed.");
        var retained=await One<SALES_RECEIVABLE_INTENT>("SALES_RECEIVABLE_INTENT","SOURCE_INVOICE_ID",current.SOURCE_INVOICE_ID!);var receipt=await One<SALES_RECEIVABLE_OUTCOME>("SALES_RECEIVABLE_OUTCOME","SOURCE_INVOICE_ID",current.SOURCE_INVOICE_ID!);var review=await One<SALES_RECEIVABLE_REVIEW_INTENT>("SALES_RECEIVABLE_REVIEW_INTENT","AR_INVOICE_ID",current.AR_INVOICE_ID);
        var command=retained.Restore(company,retained.ROW_CREATED_BY);new CompanySalesReceivableOutcome(retained,current,receipt).Require(command);if(JsonSerializer.Serialize(CompanySalesReceivableDecisionProof.Require(company,current).Intent)!=JsonSerializer.Serialize(review))throw new InvalidOperationException("The actual native AR reviewer artifact differs from its frozen approval.");await fieldRead(current.FIELD_ID!);await guard();
    }
    internal async Task<SALES_JOURNAL_POSTING_OUTCOME?> ReadCompanyJournalPostingCorrelationAsync(CompanyScope company,string book,string currency,string receivable,string connection,IDataSource source,Func<Task> guard)
    {
        await guard();await RequireJournalSourceBindingAsync(connection,source);JournalEntryService.RequireCompanyPostingRecordSchema(source,"SALES_JOURNAL_POSTING_OUTCOME",true);
        var rows=(await OwnedReceivableRepo<SALES_JOURNAL_POSTING_OUTCOME>(connection,"SALES_JOURNAL_POSTING_OUTCOME").GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="BOOK_ID",Operator="=",FilterValue=book},new(){FieldName="CURRENCY_CODE",Operator="=",FilterValue=currency},new(){FieldName="SOURCE_AR_INVOICE_ID",Operator="=",FilterValue=receivable}]))?.ToArray();await guard();await RequireJournalSourceBindingAsync(connection,source);
        if(rows is null || rows.Length>1 || rows.Any(value=>value is not SALES_JOURNAL_POSTING_OUTCOME))throw new InvalidOperationException("The unique native AR posting correlation is unavailable or ambiguous.");if(rows.SingleOrDefault() is not SALES_JOURNAL_POSTING_OUTCOME row)return null;var outcome=row.Restore();var original=outcome.Intent.Restore().Source.Current;
        if(original.Company!=company || original.BookId!=book || original.CurrencyCode!=currency || original.ReceivableId!=receivable)throw new InvalidOperationException("The native AR posting correlation belongs to another source context.");return CompanyInventorySourceReview.Copy(row);
    }
    internal async Task RecordCompanyJournalPostingCorrelationAsync(CompanySalesJournalPostingOutcome outcome,string connection,IDataSource source,Func<Task> requireAuthority,Func<string,Task> requireFieldAccess)
    {
        var command=outcome.Intent.Restore();outcome.Require(command);var current=command.Source.Current;
        async Task Guard(){await requireAuthority();PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);await RequireJournalSourceBindingAsync(connection,source);await requireFieldAccess(current.FieldId);JournalEntryService.RequireCompanyPostingRecordSchema(source,"SALES_JOURNAL_POSTING_OUTCOME",true);}
        await Guard();if(await ReadCompanyJournalPostingCorrelationAsync(current.Company,current.BookId,current.CurrencyCode,current.ReceivableId,connection,source,Guard) is not null)throw new InvalidOperationException("Another native posting correlation already owns this AR/book/currency.");
        var original=await GetCompanySalesReceivableForReviewAsync(current.Company,current.ReceivableId,Guard,requireFieldAccess);if(JsonSerializer.Serialize(original.Receivable)!=JsonSerializer.Serialize(outcome.Receivable))throw new InvalidOperationException("The original exact native AR row changed before posting correlation.");
        var repo=OwnedReceivableRepo<SALES_JOURNAL_POSTING_OUTCOME>(connection,"SALES_JOURNAL_POSTING_OUTCOME");foreach(var (name,value) in new[]{("SALES_JOURNAL_POSTING_OUTCOME_ID",command.IntentId),("JOURNAL_ENTRY_ID",command.Source.Reviewed.Header.JOURNAL_ENTRY_ID),("PPDM_GUID",Guid.ParseExact(command.IntentId,"N").ToString())}){var reserved=(await repo.GetAsync([new(){FieldName=name,Operator="=",FilterValue=value}]))?.ToArray();await Guard();if(reserved is null || reserved.Length!=0)throw new InvalidOperationException("The original native AR posting outcome/journal/global intent reservation is unavailable or occupied.");}
        var row=SALES_JOURNAL_POSTING_OUTCOME.Create(outcome);await new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_JOURNAL_POSTING_OUTCOME),connection,"SALES_JOURNAL_POSTING_OUTCOME",rowAccess:new ARPostingCorrelationMutation(outcome,Guard)).InsertAsync(row,command.Source.ActorUserId);
        var stored=await ReadCompanyJournalPostingCorrelationAsync(current.Company,current.BookId,current.CurrencyCode,current.ReceivableId,connection,source,Guard);if(stored is null || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row))throw new InvalidOperationException("The complete native AR posting correlation insertion is unconfirmed.");
        var closing=await GetCompanySalesReceivableForReviewAsync(current.Company,current.ReceivableId,Guard,requireFieldAccess);if(JsonSerializer.Serialize(closing.Receivable)!=JsonSerializer.Serialize(outcome.Receivable))throw new InvalidOperationException("The original native AR row changed during posting correlation.");await Guard();
    }
    private sealed class ARPostingCorrelationMutation(CompanySalesJournalPostingOutcome expected,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor){await guard();if(type!=typeof(SALES_JOURNAL_POSTING_OUTCOME) || table!="SALES_JOURNAL_POSTING_OUTCOME" || entity is not SALES_JOURNAL_POSTING_OUTCOME row || previous is not null || operation!=PPDMRowMutation.Insert || actor!=expected.Intent.Restore().Source.ActorUserId || JsonSerializer.Serialize(row.Restore())!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("Only the exact original native AR posting correlation insertion is authorized.");}
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("Posting correlation grants no general reads.");
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("Posting correlation grants no general reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("Posting correlation grants no general reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("Posting correlation grants no general reads.");
    }
}
