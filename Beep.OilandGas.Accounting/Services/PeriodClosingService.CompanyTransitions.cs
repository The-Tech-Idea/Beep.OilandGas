using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class PeriodClosingService
{
    private static void RequirePeriodTransitionSchema(IDataSource source)
    {
        RequireCompanyPeriodSchema(source);var schema=source.GetEntityStructure(new EntityStructure{EntityName=CompanyPeriodTable,DatasourceEntityName=CompanyPeriodTable},true);var fields=schema?.Fields?.Where(field=>field.FieldName=="STATE_HISTORY_JSON").ToArray();
        if(fields is not {Length:1} || fields[0].Fieldtype!=typeof(string).FullName || !fields[0].AllowDBNull || Math.Max(fields[0].Size,fields[0].Size1)<CompanyLedgerPeriodStateHistory.MaximumLength)throw new InvalidOperationException("Review the bounded native period history through the PRODUCTION Beep migration.");
    }
    public async Task<CompanyLedgerPeriodTransitionOutcome> TransitionCompanyPeriodAsync(CompanyLedgerPeriodTransitionSubmission submission,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(submission);ArgumentNullException.ThrowIfNull(submission.Command);ArgumentNullException.ThrowIfNull(requireAuthority);submission=CompanyInventorySourceReview.Copy(submission);var command=submission.Command;command.Require();await requireAuthority();if(_resolveConnection is null)throw new InvalidOperationException("The original period resolver is unavailable.");var connection=await _resolveConnection();if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The original period binding is unavailable.");var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The original period datasource is unavailable.");
        async Task Guard(bool owned){await requireAuthority();if(await _resolveConnection()!=connection || !ReferenceEquals(_editor.GetDataSource(connection),source))throw new InvalidOperationException("The original period transition binding changed.");if(owned)PPDMDataSourceTransaction.RequireOwned(source,IsolationLevel.Serializable);RequirePeriodTransitionSchema(source);}
        PPDMGenericRepository Repo(IPPDMRowAccessPolicy? policy=null)=>new(_editor,_commonColumnHandler,_defaults,_metadata,typeof(COMPANY_LEDGER_PERIOD),connection,CompanyPeriodTable,rowAccess:policy);
        async Task<COMPANY_LEDGER_PERIOD> Row(bool owned)
        {
            await Guard(owned);var rows=(await Repo().GetAsync([new(){FieldName="COMPANY_LEDGER_PERIOD_ID",Operator="=",FilterValue=command.PeriodId}]))?.ToArray();await Guard(owned);
            if(rows is not {Length:1} || rows[0] is not COMPANY_LEDGER_PERIOD row || row.OWNER_TENANT_ID!=command.Company.TenantId || row.OWNER_COMPANY_BA_ID!=command.Company.BusinessAssociateId || row.BOOK_ID!=command.BookId || row.CURRENCY_CODE!=command.CurrencyCode || row.CALENDAR_ID!=command.CalendarId)throw new InvalidOperationException("The exact original owned native period is unavailable.");_=CompanyLedgerPeriodAuthority.Require(row);return CompanyInventorySourceReview.Copy(row);
        }
        CompanyLedgerPeriodTransitionOutcome? Recover(COMPANY_LEDGER_PERIOD row)
        {
            if(row.STATE_HISTORY_JSON is null)return null;var history=CompanyLedgerPeriodStateHistory.Require(row);var matches=history.Entries.Where(entry=>entry.Command.IntentId==command.IntentId).ToArray();if(matches.Length==0)return null;if(matches.Length!=1 || matches[0].Command!=command)throw new InvalidOperationException("The retained period intent belongs to a different original command.");var result=new CompanyLedgerPeriodTransitionOutcome(command,history.At(command.IntentId));result.Require(command);return result;
        }
        CompanyLedgerPeriodTransitionOutcome? confirmed=null;
        await PPDMDataSourceTransaction.ExecuteAsync(source,async()=>{
            await Guard(true);var global=(await Repo().GetAsync([]))?.ToArray();await Guard(true);if(global is null || global.Length>4096 || global.Any(row=>row is not COMPANY_LEDGER_PERIOD))throw new InvalidOperationException("The complete bounded period intent reservations are unavailable.");
            foreach(var row in global.Cast<COMPANY_LEDGER_PERIOD>()){if(row.PPDM_GUID==Guid.ParseExact(command.IntentId,"N").ToString())throw new InvalidOperationException("The transition intent is already a period creation identity.");if(row.STATE_HISTORY_JSON is not null){var history=CompanyLedgerPeriodStateHistory.Require(row);if(history.Entries.Any(entry=>entry.Command.IntentId==command.IntentId && (row.COMPANY_LEDGER_PERIOD_ID!=command.PeriodId || entry.Command!=command)))throw new InvalidOperationException("Another period transition reserves this original intent.");}}
            var original=await Row(true);var known=Recover(original);if(known is not null){confirmed=known;return;}
            if(submission.Review is null)throw new InvalidOperationException("The complete original period review is required for a new transition.");command.Require(submission.Review);var current=await GetCompanyPeriodsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.CalendarId,()=>Guard(true));
            if(JsonSerializer.Serialize(current)!=JsonSerializer.Serialize(submission.Review) || JsonSerializer.Serialize(command.Require(current))!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The complete original period state/source changed before transition.");
            var changed=CompanyInventorySourceReview.Copy(original);changed.STATUS=command.ToState;
            await Repo(new CompanyPeriodStateMutation(command,original,()=>Guard(true))).UpdateAsync(changed,command.ActorUserId);var stored=await Row(true);var result=Recover(stored) ??throw new InvalidOperationException("The exact native period transition outcome is unavailable.");
            var closing=await GetCompanyPeriodsAsync(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.CalendarId,()=>Guard(true));var before=CompanyLedgerPeriodReview.Create(command.Company,command.ActorUserId,command.BookId,command.CurrencyCode,command.CalendarId,closing.Periods.Select(row=>row.COMPANY_LEDGER_PERIOD_ID==command.PeriodId?original:row));
            if(JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(current))throw new InvalidOperationException("Other period facts changed during transition.");confirmed=result;await Guard(true);
        });
        var initial=Recover(await Row(false));var closingOutcome=Recover(await Row(false));await Guard(false);if(initial is null || closingOutcome is null || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(confirmed) || JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closingOutcome))throw new InvalidOperationException("The committed period state is unconfirmed. Reconcile its exact original command.");return CompanyInventorySourceReview.Copy(closingOutcome);
    }
    public async Task<CompanyLedgerPeriodTransitionOutcome?> GetCompanyPeriodTransitionAsync(CompanyScope company,string actor,string book,string currency,string calendar,string periodId,string intentId,Func<Task> requireAuthority)
    {
        if(!Guid.TryParseExact(periodId,"N",out var period) || period==Guid.Empty || period.ToString("N")!=periodId || !Guid.TryParseExact(intentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=intentId)throw new ArgumentException("The original period and transition identities are required.");
        async Task<CompanyLedgerPeriodTransitionOutcome?> Capture(){var review=await GetCompanyPeriodsAsync(company,actor,book,currency,calendar,requireAuthority);var row=review.Periods.SingleOrDefault(row=>row.COMPANY_LEDGER_PERIOD_ID==periodId);if(row?.STATE_HISTORY_JSON is null)return null;var history=CompanyLedgerPeriodStateHistory.Require(row);var entry=history.Entries.SingleOrDefault(entry=>entry.Command.IntentId==intentId);if(entry is null)return null;if(entry.Command.ActorUserId!=actor)throw new UnauthorizedAccessException("Only the original state-transition producer can recover its private command.");var result=new CompanyLedgerPeriodTransitionOutcome(entry.Command,history.At(intentId));result.Require(entry.Command);await requireAuthority();return result;}
        var initial=await Capture();var closing=await Capture();await requireAuthority();if(JsonSerializer.Serialize(initial)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("The original period transition changed during recovery.");return closing;
    }
    private sealed class CompanyPeriodStateMutation(CompanyLedgerPeriodTransitionCommand command,COMPANY_LEDGER_PERIOD original,Func<Task> guard):IPPDMRowAccessPolicy
    {
        public async Task ValidateMutationAsync(Type type,string table,object entity,object? previous,PPDMRowMutation operation,string? actor){await guard();if(type!=typeof(COMPANY_LEDGER_PERIOD) || table!=CompanyPeriodTable || entity is not COMPANY_LEDGER_PERIOD row || previous is not COMPANY_LEDGER_PERIOD before || operation!=PPDMRowMutation.Update || actor!=command.ActorUserId || JsonSerializer.Serialize(before)!=JsonSerializer.Serialize(original) || row.ROW_CHANGED_BY!=actor || row.ROW_CHANGED_DATE is not {} changed)throw new InvalidOperationException("Only the exact original period status and native transition audit update are authorized.");var history=CompanyLedgerPeriodStateHistory.Append(original,command,changed);row.STATE_HISTORY_JSON=history.Serialize();_=CompanyLedgerPeriodStateHistory.Require(row);}
        public async Task<IReadOnlyList<IReadOnlyList<AppFilter>>> GetReadConjunctionsAsync(Type type,string table)=>[await GetReadConstraintsAsync(type,table)];
        public async Task<IReadOnlyList<AppFilter>> GetReadConstraintsAsync(Type type,string table){await guard();if(type!=typeof(COMPANY_LEDGER_PERIOD) || table!=CompanyPeriodTable)throw new InvalidOperationException();return [new(){FieldName="COMPANY_LEDGER_PERIOD_ID",Operator="=",FilterValue=command.PeriodId},new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=command.Company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=command.Company.BusinessAssociateId}];}
        public async Task ValidateRowsAsync(Type type,string table,IReadOnlyList<object> rows){await guard();if(type!=typeof(COMPANY_LEDGER_PERIOD) || table!=CompanyPeriodTable || rows.Count!=1 || JsonSerializer.Serialize(rows[0])!=JsonSerializer.Serialize(original))throw new InvalidOperationException("The exact original period predecessor is required.");}
        public Task<string?> GetEffectiveAccessLevelAsync(Type type,string table,object row,bool inherited=true)=>throw new InvalidOperationException();
    }
}
