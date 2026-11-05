using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

public sealed class DrillingReportingClosureState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDrillingServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception,string,string,string> explain;
    private readonly RetainedOriginalFeedback feedback=new();
    private bool disposed,reading,writing,ready;
    private long generation,request;
    private string? assignmentId,completionId;
    private DrillingReportingClosureStatus? status;
    private DrillingReportingClosureReview? review;
    private DrillingReportingClosure? prepared,pending;
    private DrillingReportingClosureReceipt? receipt;
    public event Action? Changed;
    public bool Busy=>reading||writing;
    public string? Error{get;private set;}
    public string? Confirmation{get;private set;}
    public string? BlockedTransferId{get;private set;}
    public bool NeedsReload=>CanView && !Busy && !ready && Error is null;
    public DrillingReportingClosureStatus? Status=>Copy(status);
    public DrillingReportingClosureReview? Review=>Copy(review);
    public DrillingReportingClosure? Prepared=>Copy(prepared);
    public DrillingReportingClosure? Pending=>pending?.Review.Responsibility.Root.InstanceId()==assignmentId?Copy(pending):null;
    public DrillingReportingClosure? Retained=>Copy(pending);
    public DrillingReportingClosureReceipt? Receipt=>Copy(receipt);
    public string PendingTitle=>feedback.Title("reporting closure");
    public string? PendingExplanation=>pending is null?null:feedback.Explanation;
    public bool CanView=>company.Selection is {} selected && new[]{"Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    private bool WriteRights=>CanView && new[]{"Drilling.Edit","Drilling.ManageWellProgram","Workflow.Start"}.All(permission=>company.Selection!.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanReviewTransfer=>!Busy && ready && WriteRights && pending is null && receipt is null && BlockedTransferId is null && status is {Closure:null};
    public bool CanInspect=>!Busy && ready && WriteRights && pending is null && prepared is null && receipt is null && status is {Closure:null} &&
        BlockedTransferId is null && status.Responsibility.ManagerUserId==company.Selection!.UserId && company.Selection.Roles.Contains(status.Responsibility.ManagerRole,StringComparer.Ordinal);
    private bool ProfileMatches=>review is not null && company.Selection is {} selected && personas.CurrentProfile?.UserId==selected.UserId &&
        personas.CurrentProfile.PersonaCode==review.ManagerPersona && personas.CurrentPersona?.Code==review.ManagerPersona && selected.Roles.Contains(review.ManagerRole,StringComparer.Ordinal);
    public bool CanPrepare=>!Busy && ready && WriteRights && ProfileMatches && review is not null && pending is null && receipt is null;
    public bool CanRecover=>!Busy && ready && CanView && Pending is not null && pending!.Scope().Company==company.Selection!.Scope && pending.Scope().ActorUserId==company.Selection.UserId;

    public DrillingReportingClosureState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {
        this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;
        company.Changed+=Invalidate;personas.Changed+=Invalidate;
    }
    private static T? Copy<T>(T? value) where T:class=>value is null?null:CompanyInventorySourceReview.Copy(value);
    private bool Active((long Version,CompanyClientSelection? Selection) selected,long epoch)=>!disposed && company.Snapshot==selected && generation==epoch;
    private void Invalidate()
    {
        ++generation;reading=false;ready=false;status=null;review=null;prepared=null;pending=null;receipt=null;BlockedTransferId=null;Error=null;Confirmation=null;feedback.Reset();
        if(!disposed)Changed?.Invoke();
    }
    public async Task LoadAsync(string? id,string? acceptedReview=null)
    {
        if(disposed)return;var sequence=++request;assignmentId=id;completionId=acceptedReview;Invalidate();var selected=company.Snapshot;
        if(!CanView || selected.Selection is null)return;
        var epoch=generation;bool Current()=>sequence==request && Active(selected,epoch);reading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(id) || !id.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open the original company reporting responsibility.");
            var retained=await journal.ReadReportingClosureAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Current())return;
            if(retained is not null)
            {
                retained.Require();if(retained.Scope().Company!=selected.Selection.Scope || retained.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained closure belongs to another company/account.");
                pending=Copy(retained);feedback.ReadFromTab();ready=true;return;
            }
            var transfer=await journal.ReadReportingReassignmentAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Current())return;
            if(transfer is not null){transfer.Require();if(transfer.Scope().Company!=selected.Selection.Scope || transfer.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained transfer belongs to another company/account.");BlockedTransferId=transfer.Review.Responsibility.Root.InstanceId();ready=true;return;}
            var found=await client.GetReportingResponsibilityAsync(id);if(!Current())return;
            found.Require(selected.Selection.Scope,selected.Selection.UserId,id);status=Copy(found);ready=true;
        }
        catch(Exception error){var text=explain(error,"loading original reporting responsibility","The original reporting responsibility could not be confirmed");if(Current())Error=text;}
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public async Task InspectAsync()
    {
        if(!CanInspect)return;var selected=company.Snapshot;var sequence=++request;var epoch=generation;var original=Copy(status)!;
        bool Current()=>sequence==request && Active(selected,epoch);reading=true;review=null;Error=null;Changed?.Invoke();
        try
        {
            await personas.EnsureLoadedAsync();if(disposed || selected!=company.Snapshot || sequence!=request)return;epoch=generation;reading=true;Changed?.Invoke();
            var accepted=completionId;
            if(string.IsNullOrEmpty(accepted))
            {
                var routing=await client.GetCompletionRoutingAsync(original.Execution.Basis.ReviewInstanceId);if(!Current())return;
                routing.Require(selected.Selection!.Scope,selected.Selection.UserId,original.Execution.Basis.ReviewInstanceId);
                original.Responsibility.Root.RequireSource(routing.Source.Execution,false);
                accepted=routing.ExistingReviewInstanceId??throw new InvalidOperationException("No independent completion review is recorded for the current reported claim.");
            }
            var found=await client.ReviewReportingClosureAsync(original.Responsibility.Root.InstanceId(),accepted);if(!Current())return;found.Require();
            if(found.Scope().Company!=selected.Selection!.Scope || found.Scope().ActorUserId!=selected.Selection.UserId || found.Responsibility.ContentRevision()!=original.Responsibility.ContentRevision())
                throw new InvalidOperationException("The closure review belongs to another original responsibility/account.");
            review=Copy(found);status=original;ready=true;completionId=accepted;
        }
        catch(Exception error){var text=explain(error,"checking current reporting closure readiness","Current accepted completion, test evidence and management authority could not be confirmed");if(Current())Error=text;}
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public void Prepare(string? actualUtc,string? reason,bool confirmed)
    {
        if(!CanPrepare)return;prepared=null;Error=null;
        try
        {
            if(!ExplicitUtcInput.TryParse(actualUtc,out var utc))throw new InvalidOperationException("Enter an explicit actual UTC ending in Z or +00:00.");
            var value=new DrillingReportingClosure(Copy(review)!,Guid.NewGuid(),new DateTimeOffset(utc),reason?.Trim()??"",confirmed);value.Require();prepared=value;
        }
        catch(Exception error){Error=explain(error,"preparing reporting closure","Enter actual UTC after the readiness check, a rationale and explicit confirmation");}
        Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>Record(false);
    public Task RecoverAsync()=>Record(true);
    private async Task Record(bool recover)
    {
        if(disposed || (recover?!CanRecover:!CanPrepare || prepared is null))return;
        var original=Copy(recover?pending:prepared)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        writing=true;Error=null;Confirmation=null;if(!recover){pending=original;prepared=null;receipt=null;feedback.BeginSaving();}
        var fallback=feedback.FailureExplanation;Changed?.Invoke();
        try
        {
            if(!recover)
            {
                if(!Current())return;await journal.RetainReportingClosureAsync(original);if(!Current())return;
                feedback.ConfirmSaved();feedback.AttemptSubmission();fallback=feedback.FailureExplanation;Changed?.Invoke();if(!Current())return;
            }
            var result=recover?await client.RecoverReportingClosureAsync(original):await client.CloseReportingAsync(original);result.Require(original);
            if(!recover && !result.Recorded)throw new InvalidOperationException("The original reporting closure is unconfirmed.");
            if(!Current())return;receipt=Copy(result);status=null;review=null;feedback.ConfirmReceiptAndBeginClear(result.Recorded);fallback=feedback.FailureExplanation;Changed?.Invoke();
            if(!Current())return;await journal.ClearReportingClosureAsync(original);if(!Current())return;pending=null;feedback.Reset();
            Confirmation=result.Recorded?"The original ongoing reporting responsibility and its owned tasks were confirmed closed. Physical release remains a separate workflow.":
                "This original reporting closure was confirmed absent. Reload and check current evidence before preparing another.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original reporting closure":"closing original reporting responsibility",fallback);if(Current()){feedback.Failed();Error=text;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
