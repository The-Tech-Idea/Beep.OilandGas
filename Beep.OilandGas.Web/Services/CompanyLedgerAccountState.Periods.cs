using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class CompanyLedgerAccountState
{
    private CompanyLedgerPeriodSource? periods;
    private CompanyLedgerPeriodConfigurationReview? periodConfigurationReview;
    private CompanyLedgerPeriodCreationSubmission? pendingPeriod;
    private COMPANY_LEDGER_PERIOD? periodResult;
    public CompanyLedgerPeriodSource? Periods=>periods is null?null:CompanyInventorySourceReview.Copy(periods);
    public CompanyLedgerPeriodConfigurationReview? PeriodConfigurationReview=>periodConfigurationReview is null?null:CompanyInventorySourceReview.Copy(periodConfigurationReview);
    public bool HasPendingPeriod=>pendingPeriod is not null;
    private bool PendingPeriodMatches()=>!disposed && pendingPeriod is {} pending && company.Selection is {} selected && selected.Scope==pending.Command.Company && selected.UserId==pending.Command.ActorUserId && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) && selected.Permissions.Contains("Accounting.ConfigureLedger",StringComparer.Ordinal);
    public CompanyLedgerPeriodCreationSubmission? PendingPeriod=>PendingPeriodMatches()?CompanyInventorySourceReview.Copy(pendingPeriod):null;
    public COMPANY_LEDGER_PERIOD? PeriodResult=>periodResult is null?null:CompanyInventorySourceReview.Copy(periodResult);
    public async Task LoadPeriodsAsync(string book,string currency,string calendar,IAccountingServiceClient client)
    {
        if(disposed || Busy || owner is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException();
        _=CompanyLedgerPeriodReview.Create(selected.Scope,selected.UserId,book,currency,calendar,[]);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanyLedgerPeriodsAsync(book,currency,calendar);loaded.Require(selected.Scope,selected.UserId,book,currency,calendar);if(!disposed && generation==version && snapshot==company.Snapshot)periods=CompanyInventorySourceReview.Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task LoadPeriodConfigurationReviewAsync(string book,string currency,string calendar,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the retained original ledger request first.");var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var loaded=await client.GetCompanyLedgerPeriodConfigurationReviewAsync(book,currency,calendar);loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,book,currency,calendar);if(!disposed && generation==version && snapshot==company.Snapshot)periodConfigurationReview=CompanyInventorySourceReview.Copy(loaded);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task CreatePeriodAsync(string code,DateTime from,DateTime through,string reason,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest || periodConfigurationReview is not {} displayed)throw new InvalidOperationException("Review the complete original period configuration first.");var history=displayed.History;displayed.Require(company.Selection!.Scope,company.Selection.UserId,history.BookId,history.CurrencyCode,history.CalendarId);
        var command=new CompanyLedgerPeriodCreation(history.Company,history.ActorUserId,history.BookId,history.CurrencyCode,history.CalendarId,Guid.NewGuid().ToString("N"),code,from,through,Guid.NewGuid().ToString("N"),reason,history.Revision,displayed.BindingRevision);
        command.Require(history);pendingPeriod=new(command,CompanyInventorySourceReview.Copy(history));await SubmitPeriodAsync(client);
    }
    public async Task RecoverPeriodAsync(string book,string currency,string calendar,string id,IAccountingServiceClient client)
    {
        ConfigurationReady();if(HasPendingLedgerRequest)throw new InvalidOperationException("Reconcile the retained original ledger request first.");var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        try{var command=await client.GetCompanyLedgerPeriodCreationRequestAsync(book,currency,calendar,id) ??throw new InvalidOperationException("The committed original period request is unavailable; no new period is authorized.");command.Require();
            if(command.Company!=snapshot.Selection!.Scope || command.ActorUserId!=snapshot.Selection.UserId || command.BookId!=book || command.CurrencyCode!=currency || command.CalendarId!=calendar || command.PeriodId!=id)throw new InvalidOperationException("The original period request is unconfirmed.");
            if(!disposed && generation==version && snapshot==company.Snapshot)pendingPeriod=new(CompanyInventorySourceReview.Copy(command),null);}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task SubmitPeriodAsync(IAccountingServiceClient client)
    {
        ConfigurationReady();if(!PendingPeriodMatches())throw new InvalidOperationException("Return to the original period company/account to reconcile.");var pending=CompanyInventorySourceReview.Copy(pendingPeriod!);var snapshot=company.Snapshot;Invalidate();var generation=version;Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        try{var result=await client.CreateCompanyLedgerPeriodAsync(CompanyInventorySourceReview.Copy(pending));if(CompanyLedgerPeriodAuthority.Require(result).Command!=pending.Command)throw new InvalidOperationException("The original period outcome is unconfirmed.");if(!Current())return;
            var source=await client.GetCompanyLedgerPeriodsAsync(pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId);source.Require(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId);
            var expected=CompanyLedgerPeriodSource.FromNative(CompanyLedgerPeriodReview.Create(pending.Command.Company,pending.Command.ActorUserId,pending.Command.BookId,pending.Command.CurrencyCode,pending.Command.CalendarId,[result])).Periods.Single();
            if(source.Periods.SingleOrDefault(row=>row.PeriodId==pending.Command.PeriodId)!=expected)throw new InvalidOperationException("The complete public period source does not confirm the original configuration.");
            if(!Current())return;periods=CompanyInventorySourceReview.Copy(source);periodResult=CompanyInventorySourceReview.Copy(result);pendingPeriod=null;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
}
