using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;

public partial class PeriodClosingService
{
    public async Task<CompanyLedgerPeriodCreation?> GetCompanyPeriodCreationAsync(CompanyScope company,string actor,string book,string currency,string calendar,string periodId,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);_=CompanyLedgerPeriodReview.Create(company,actor,book,currency,calendar,[]);if(!Guid.TryParseExact(periodId,"N",out var identity) || identity==Guid.Empty || identity.ToString("N")!=periodId)throw new InvalidOperationException("The original native period identity is required.");await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound original PRODUCTION period owner is required.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original period recovery binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original period datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original period recovery binding changed.");}
        async Task<COMPANY_LEDGER_PERIOD?> Capture()
        {
            await Guard();RequireCompanyPeriodSchema(source);var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMPANY_LEDGER_PERIOD),connection,CompanyPeriodTable);
            var rows=(await repo.GetAsync([new(){FieldName="COMPANY_LEDGER_PERIOD_ID",Operator="=",FilterValue=periodId}]))?.ToArray();await Guard();
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not COMPANY_LEDGER_PERIOD))throw new InvalidOperationException("The original period outcome is unavailable or ambiguous.");var row=rows.SingleOrDefault() as COMPANY_LEDGER_PERIOD;if(row is null)return null;
            if(row.OWNER_TENANT_ID!=company.TenantId || row.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId || row.BOOK_ID!=book || row.CURRENCY_CODE!=currency || row.CALENDAR_ID!=calendar || row.ROW_CREATED_BY!=actor)throw new UnauthorizedAccessException("Only the original actor can recover the period request in its original company book/currency/calendar.");
            var receipt=CompanyLedgerPeriodAuthority.Require(row);if(receipt.Command.ActorUserId!=actor || receipt.Command.PeriodId!=periodId)throw new UnauthorizedAccessException();return CompanyInventorySourceReview.Copy(row);
        }
        var initial=await Capture();var closing=await Capture();await Guard();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original period outcome changed during recovery.");
        return closing is null?null:CompanyInventorySourceReview.Copy(CompanyLedgerPeriodAuthority.Require(closing).Command);
    }
    public async Task<COMPANY_LEDGER_PERIOD> CreateCompanyPeriodAsync(CompanyLedgerPeriodCreationSubmission submission,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(requireAuthority);
        submission=CompanyInventorySourceReview.Copy(submission);var command=submission.Command;command.Require();await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound original PRODUCTION period owner is required.");
        var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original period creation binding is unavailable.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original period datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original period creation binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);}
        PPDMGenericRepository Repository()=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMPANY_LEDGER_PERIOD),connection,CompanyPeriodTable);
        async Task<COMPANY_LEDGER_PERIOD?> Read(bool owned)
        {
            await Guard(owned);RequireCompanyPeriodSchema(source);var rows=(await Repository().GetAsync([new(){FieldName="COMPANY_LEDGER_PERIOD_ID",Operator="=",FilterValue=command.PeriodId}]))?.ToArray();await Guard(owned);
            if(rows is null || rows.Length>1 || rows.Any(row=>row is not COMPANY_LEDGER_PERIOD))throw new InvalidOperationException("The original period identity is unavailable or ambiguous.");
            var row=rows.SingleOrDefault() as COMPANY_LEDGER_PERIOD;if(row is not null && (row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId || row.BOOK_ID!=command.BookId || row.CURRENCY_CODE!=command.CurrencyCode || row.CALENDAR_ID!=command.CalendarId))throw new InvalidOperationException("The native period identity belongs to another original company book/currency/calendar.");
            if(row is not null)_=CompanyLedgerPeriodAuthority.Require(row);return row is null?null:CompanyInventorySourceReview.Copy(row);
        }
        COMPANY_LEDGER_PERIOD? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>
        {
            await Guard(true);RequireCompanyPeriodSchema(source);var reservations=(await Repository().GetAsync([new(){FieldName="PPDM_GUID",Operator="=",FilterValue=Guid.ParseExact(command.IntentId,"N").ToString()}]))?.ToArray();await Guard(true);
            if(reservations is null || reservations.Length>1 || reservations.Any(row=>row is not COMPANY_LEDGER_PERIOD period || period.COMPANY_LEDGER_PERIOD_ID!=command.PeriodId))throw new InvalidOperationException("The original period intent is unavailable, ambiguous or reserved by another native period.");
            var existing=await Read(true);if((reservations.Length==1)!=(existing is not null))throw new InvalidOperationException("The original period identity and intent reservation are inconsistent.");
            if(existing is not null){if(CompanyLedgerPeriodAuthority.Require(existing).Command!=command)throw new InvalidOperationException("Another original period request owns this identity.");confirmed=existing;await Guard(true);return;}
            if(submission.Review is null)throw new InvalidOperationException("A complete original company/book period review is required before creation.");
            command.Require(submission.Review);var current=await GetCompanyPeriodsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.CalendarId,()=>Guard(true));
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(submission.Review))throw new InvalidOperationException("The complete original period history changed before creation.");command.Require(current);
            var row=command.CreateRow();var writer=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMPANY_LEDGER_PERIOD),connection,CompanyPeriodTable,rowAccess:new CompanyPeriodCreationMutation(command,()=>Guard(true)));
            await writer.InsertAsync(row,command.ActorUserId);var stored=await Read(true);
            if(stored is null || JsonSerializer.Serialize(stored)!=JsonSerializer.Serialize(row) || CompanyLedgerPeriodAuthority.Require(stored).Command!=command)throw new InvalidOperationException("The recorded period differs from the original authorized creation.");
            var closing=await GetCompanyPeriodsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.CalendarId,()=>Guard(true));
            var before=CompanyLedgerPeriodReview.Create(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.CalendarId,closing.Periods.Where(period=>period.COMPANY_LEDGER_PERIOD_ID!=command.PeriodId));
            if(JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("Other recorded periods changed during creation.");confirmed=stored;await Guard(true);
        });
        var durable=await Read(false);if(durable is null || JsonSerializer.Serialize(durable)!=JsonSerializer.Serialize(confirmed) || CompanyLedgerPeriodAuthority.Require(durable).Command!=command)throw new InvalidOperationException("The committed period is unconfirmed. Retain and reconcile the original request.");
        await Guard(false);return CompanyInventorySourceReview.Copy(durable);
    }
    private sealed class CompanyPeriodCreationMutation(CompanyLedgerPeriodCreation command,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor)
        {
            await guard();if(type!=typeof(COMPANY_LEDGER_PERIOD) || table!=CompanyPeriodTable || entity is not COMPANY_LEDGER_PERIOD row || previous is not null || operation!=PPDMRowMutation.Insert || actor!=command.ActorUserId)throw new InvalidOperationException("Only the original explicit company period insertion is authorized.");
            row.AUTHORITY_JSON=CompanyLedgerPeriodAuthority.Create(command,row);_=CompanyLedgerPeriodAuthority.Require(row);
        }
        public Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>throw new InvalidOperationException("A period insertion cannot grant reads.");
        public Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table)=>throw new InvalidOperationException("A period insertion cannot grant reads.");
        public Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows)=>throw new InvalidOperationException("A period insertion cannot grant reads.");
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException("A period insertion cannot grant reads.");
    }
}

