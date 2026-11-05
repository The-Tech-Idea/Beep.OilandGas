using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanyLedgerAccountState
{
    private CompanyLedgerPeriodConfigurationReview? periodTransitionReview;
    private CompanyLedgerPeriodTransitionSubmission? pendingPeriodTransition;
    private CompanyLedgerPeriodTransitionOutcome? periodTransitionResult;
    public bool HasPendingPeriodTransition=>pendingPeriodTransition is not null;
    private bool PeriodTransitionContext=>!disposed && owner is not null && company.Selection is {} selected && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    private bool PendingPeriodTransitionMatches=>PeriodTransitionContext && pendingPeriodTransition is {} pending && pending.Command.Company==company.Selection!.Scope && pending.Command.ActorUserId==company.Selection.UserId;
    public CompanyLedgerPeriodConfigurationReview? PeriodTransitionReview=>PeriodTransitionContext && periodTransitionReview is not null?CompanyInventorySourceReview.Copy(periodTransitionReview):null;
    public CompanyLedgerPeriodTransitionSubmission? PendingPeriodTransition=>PendingPeriodTransitionMatches?CompanyInventorySourceReview.Copy(pendingPeriodTransition!):null;
    public CompanyLedgerPeriodTransitionOutcome? PeriodTransitionResult=>PeriodTransitionContext && periodTransitionResult is not null?CompanyInventorySourceReview.Copy(periodTransitionResult):null;
    private void PeriodTransitionReady(bool write=true,bool reopen=false){if(!PeriodTransitionContext || Busy || write && (!company.Selection!.Permissions.Contains("Accounting.ManagePeriods",StringComparer.Ordinal) || reopen && !company.Selection.Permissions.Contains("Accounting.ReopenPeriods",StringComparer.Ordinal)))throw new UnauthorizedAccessException("Current owned period management and separate reopening authority are required.");}
    public async Task LoadPeriodTransitionReviewAsync(string book,string currency,string calendar,IAccountingServiceClient client)
    {
        PeriodTransitionReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the retained original ledger request first.");var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var result=await client.GetCompanyPeriodTransitionReviewAsync(book,currency,calendar);result.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,currency,calendar);if(!disposed && generation==version && snapshot==company.Snapshot)periodTransitionReview=CompanyInventorySourceReview.Copy(result);}finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task ChangePeriodStateAsync(string id,string target,string disposition,string evidence,string reason,IAccountingServiceClient client)
    {
        PeriodTransitionReady(reopen:target=="OPEN");if(HasPendingLedgerRequest || periodTransitionReview is not {} displayed)throw new InvalidOperationException("Review the original native period state first.");var history=displayed.History;displayed.Require(company.Selection!.Scope,company.Selection.UserId,history.BookId,history.CurrencyCode,history.CalendarId);var period=history.Periods.SingleOrDefault(row=>row.COMPANY_LEDGER_PERIOD_ID==id) ??throw new InvalidOperationException("Select an original reviewed period.");
        var command=new CompanyLedgerPeriodTransitionCommand(Guid.NewGuid().ToString("N"),history.Company,history.ActorUserId,history.BookId,history.CurrencyCode,history.CalendarId,id,period.STATUS,target,disposition,evidence,reason,history.Revision,displayed.BindingRevision);command.Require(history);pendingPeriodTransition=new(command,CompanyInventorySourceReview.Copy(history));await SubmitPeriodTransitionAsync(client);
    }
    public async Task SubmitPeriodTransitionAsync(IAccountingServiceClient client)
    {
        PeriodTransitionReady(reopen:pendingPeriodTransition?.Command.ToState=="OPEN");if(!PendingPeriodTransitionMatches)throw new InvalidOperationException("Return to the original period transition company/account.");var pending=CompanyInventorySourceReview.Copy(pendingPeriodTransition!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var result=await client.TransitionCompanyPeriodAsync(pending);result.Require(pending.Command);if(!Current())return;var recovery=await client.GetCompanyPeriodTransitionAsync(pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId,pending.Command.PeriodId,pending.Command.IntentId);if(recovery is null || JsonSerializer.Serialize(recovery)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("The original durable period transition recovery is unconfirmed.");recovery.Require(pending.Command);if(!Current())return;
            var publicSource=await client.GetCompanyLedgerPeriodsAsync(pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId);publicSource.Require(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId);var expected=CompanyLedgerPeriodSource.FromNative(CompanyLedgerPeriodReview.Create(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId,[result.Period])).Periods.Single();
            if(publicSource.Periods.SingleOrDefault(period=>period.PeriodId==pending.Command.PeriodId)!=expected)throw new InvalidOperationException("The complete public period source does not confirm this original state outcome.");if(!Current())return;periods=CompanyInventorySourceReview.Copy(publicSource);periodTransitionResult=CompanyInventorySourceReview.Copy(result);pendingPeriodTransition=null;
        }finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverPeriodTransitionAsync(string book,string currency,string calendar,string id,string intent,IAccountingServiceClient client)
    {
        PeriodTransitionReady(false);if(HasPendingFx || HasPendingBook || HasPendingConfiguration || HasPendingMappingConfiguration || HasPendingMappingTransition || HasPendingPeriod || HasPendingPeriodTransition && (!PendingPeriodTransitionMatches || pendingPeriodTransition!.Command.PeriodId!=id || pendingPeriodTransition.Command.IntentId!=intent || pendingPeriodTransition.Command.BookId!=book || pendingPeriodTransition.Command.CurrencyCode!=currency || pendingPeriodTransition.Command.CalendarId!=calendar))throw new InvalidOperationException("Reconcile the original retained ledger request in its own context.");var retained=pendingPeriodTransition;var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var result=await client.GetCompanyPeriodTransitionAsync(book,currency,calendar,id,intent) ??throw new InvalidOperationException("The original period transition is unavailable.");result.Require(retained?.Command ?? result.Command);if(result.Command.Company!=snapshot.Selection!.Scope || result.Command.ActorUserId!=snapshot.Selection.UserId)throw new InvalidOperationException("The private period transition belongs to another context.");if(!disposed && generation==version && snapshot==company.Snapshot){periodTransitionResult=CompanyInventorySourceReview.Copy(result);pendingPeriodTransition=null;}}finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
