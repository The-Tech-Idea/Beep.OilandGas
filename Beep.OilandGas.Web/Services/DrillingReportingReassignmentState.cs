using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.PermitsAndApplications.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.Common;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

public sealed class DrillingReportingReassignmentState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDrillingServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception,string,string,string> explain;
    private readonly RetainedOriginalFeedback feedback=new();
    private bool disposed,reading,writing,ready;
    private long generation,request;
    private string? assignmentId,workflowId,managerRole;
    private DrillingReportingReassignmentCatalog? catalog;
    private DrillingReportingReassignmentReview? review;
    private DrillingReportingReassignment? prepared,pending;
    private DrillingReportingReassignmentReceipt? receipt;
    public event Action? Changed;
    public bool Busy=>reading||writing;
    public string? Error{get;private set;}
    public string? Confirmation{get;private set;}
    public string? BlockedClosureId{get;private set;}
    public bool NeedsReload=>CanView && !Busy && !ready && Error is null;
    public DrillingReportingReassignmentCatalog? Catalog=>Copy(catalog);
    public DrillingReportingReassignmentReview? Review=>Copy(review);
    public DrillingReportingReassignment? Prepared=>Copy(prepared);
    public DrillingReportingReassignment? Pending=>pending?.Review.Responsibility.Root.InstanceId()==assignmentId?Copy(pending):null;
    public DrillingReportingReassignment? Retained=>Copy(pending);
    public DrillingReportingReassignmentReceipt? Receipt=>Copy(receipt);
    public string PendingTitle=>feedback.Title("reporting transfer");
    public string? PendingExplanation=>pending is null?null:feedback.Explanation;
    public bool CanView=>company.Selection is {} selected && new[]{"Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool WriteRights=>CanView && new[]{"Drilling.Edit","Drilling.ManageWellProgram","Workflow.Start"}.All(permission=>company.Selection!.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanReview=>!Busy && ready && WriteRights && catalog is not null && pending is null && receipt is null && BlockedClosureId is null &&
        catalog.Options.Any(row=>row.ProcessId==workflowId && row.Ready &&
            (!DrillingReportingReassignmentReview.SameDestination(catalog.Responsibility.Workflow,row) || catalog.Scope().ActorUserId!=catalog.Responsibility.ManagerUserId || managerRole!=catalog.Responsibility.ManagerRole)) &&
        catalog.ManagerRoles.Contains(managerRole??"",StringComparer.Ordinal) && company.Selection!.Roles.Contains(managerRole??"",StringComparer.Ordinal);
    private bool ProfileMatches=>review is not null && company.Selection is {} selected && personas.CurrentProfile?.UserId==selected.UserId &&
        personas.CurrentProfile.PersonaCode==review.ManagerPersona && personas.CurrentPersona?.Code==review.ManagerPersona && selected.Roles.Contains(review.ManagerRole,StringComparer.Ordinal);
    public bool CanPrepare=>!Busy && ready && WriteRights && ProfileMatches && review is not null && pending is null && receipt is null && BlockedClosureId is null;
    public bool CanRecover=>!Busy && ready && CanView && Pending is not null && pending!.Scope().Company==company.Selection!.Scope && pending.Scope().ActorUserId==company.Selection.UserId;
    public DrillingReportingReassignmentState(CompanyClientContext company,IPersonaContextService personas,IDrillingServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private static T? Copy<T>(T? value) where T:class=>value is null?null:CompanyInventorySourceReview.Copy(value);
    private bool Active((long Version,CompanyClientSelection? Selection) selected,long epoch)=>!disposed && company.Snapshot==selected && generation==epoch;
    private void Invalidate()
    {
        ++generation;reading=false;ready=false;catalog=null;review=null;prepared=null;pending=null;receipt=null;workflowId=null;managerRole=null;BlockedClosureId=null;Error=null;Confirmation=null;feedback.Reset();if(!disposed)Changed?.Invoke();
    }
    public async Task LoadAsync(string? id)
    {
        if(disposed)return;var sequence=++request;assignmentId=id;Invalidate();var selected=company.Snapshot;if(!CanView || selected.Selection is null)return;
        var epoch=generation;bool Current()=>sequence==request && Active(selected,epoch);reading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(id) || !id.StartsWith(DrillingExecutionAssignment.InstancePrefix,StringComparison.Ordinal))throw new InvalidOperationException("Open an exact company reporting responsibility.");
            var retained=await journal.ReadReportingReassignmentAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Current())return;
            if(retained is not null)
            {
                retained.Require();if(retained.Scope().Company!=selected.Selection.Scope || retained.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained transfer belongs to another company/account.");
                pending=Copy(retained);feedback.ReadFromTab();ready=true;return;
            }
            var closure=await journal.ReadReportingClosureAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Current())return;
            if(closure is not null){closure.Require();if(closure.Scope().Company!=selected.Selection.Scope || closure.Scope().ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained closure belongs to another company/account.");BlockedClosureId=closure.Review.Responsibility.Root.InstanceId();ready=true;return;}
            if(!WriteRights){ready=true;return;}
            var found=await client.GetReportingReassignmentCatalogAsync(id);if(!Current())return;
            found.Require(selected.Selection.Scope,selected.Selection.UserId,id);catalog=Copy(found);ready=true;
        }
        catch(Exception error){var text=explain(error,"loading reporting transfer authority","The current reporting generation and configured receiving teams could not be confirmed");if(Current())Error=text;}
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public void Select(string? processId,string? role)
    {if(Busy || pending is not null || receipt is not null)return;workflowId=processId;managerRole=role;review=null;prepared=null;Error=null;Changed?.Invoke();}
    public async Task ReviewAsync()
    {
        if(!CanReview)return;var selected=company.Snapshot;var sequence=++request;var epoch=generation;var source=Copy(catalog)!;
        var choice=new DrillingReportingReassignmentSelection(source.Responsibility.Root.InstanceId(),source.Revision,workflowId!,managerRole!);
        bool Current()=>sequence==request && Active(selected,epoch);reading=true;review=null;prepared=null;Error=null;Changed?.Invoke();
        try
        {
            await personas.EnsureLoadedAsync();if(disposed || selected!=company.Snapshot || sequence!=request)return;epoch=generation;reading=true;Changed?.Invoke();
            var found=await client.ReviewReportingReassignmentAsync(choice);if(!Current())return;found.Require();
            if(found.Scope().Company!=selected.Selection!.Scope || found.Scope().ActorUserId!=selected.Selection.UserId || found.Responsibility.ContentRevision()!=source.Responsibility.ContentRevision() ||
                found.Execution.Revision!=source.Execution.Revision || found.NativeRevision!=source.NativeRevision || found.ManagerRole!=choice.ManagerRole || found.ManagerPersona!=source.ManagerPersona ||
                found.AuthorityRevision!=source.AuthorityRevision || found.Destination.ProcessId!=choice.ProcessId || !source.Options.Any(row=>System.Text.Json.JsonSerializer.Serialize(row)==System.Text.Json.JsonSerializer.Serialize(found.Destination)))
                throw new InvalidOperationException("The displayed reporting generation, receiving team or manager authority changed. Reload before transfer.");
            if(personas.CurrentProfile?.UserId!=selected.Selection.UserId || personas.CurrentProfile.PersonaCode!=found.ManagerPersona || personas.CurrentPersona?.Code!=found.ManagerPersona)
                throw new InvalidOperationException("Use the current configured management workspace before confirming a reporting transfer.");
            catalog=source;workflowId=choice.ProcessId;managerRole=choice.ManagerRole;review=Copy(found);ready=true;
        }
        catch(Exception error){var text=explain(error,"reviewing reporting transfer","The current generation, configured receiving team and management authority could not be confirmed");if(Current())Error=text;}
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public void Prepare(string? actualUtc,string? reason,bool confirmed)
    {
        if(!CanPrepare)return;prepared=null;Error=null;
        try
        {
            if(!ExplicitUtcInput.TryParse(actualUtc,out var utc))throw new InvalidOperationException("Enter an explicit actual transfer UTC ending in Z or +00:00.");
            var value=new DrillingReportingReassignment(Copy(review)!,Guid.NewGuid(),new DateTimeOffset(utc),reason?.Trim()??"",confirmed);value.Require();prepared=value;
        }
        catch(Exception error){Error=explain(error,"preparing reporting transfer","Enter actual UTC after review, a reason and explicit confirmation");}Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>Record(false);
    public Task RecoverAsync()=>Record(true);
    private async Task Record(bool recover)
    {
        if(disposed || (recover?!CanRecover:!CanPrepare || prepared is null))return;
        var original=Copy(recover?pending:prepared)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        writing=true;Error=null;Confirmation=null;if(!recover){pending=original;prepared=null;receipt=null;feedback.BeginSaving();}var fallback=feedback.FailureExplanation;Changed?.Invoke();
        try
        {
            if(!recover)
            {
                if(!Current())return;await journal.RetainReportingReassignmentAsync(original);if(!Current())return;
                feedback.ConfirmSaved();feedback.AttemptSubmission();fallback=feedback.FailureExplanation;Changed?.Invoke();if(!Current())return;
            }
            var result=recover?await client.RecoverReportingReassignmentAsync(original):await client.ReassignReportingAsync(original);result.Require(original);
            if(!recover && !result.Recorded)throw new InvalidOperationException("The original reporting transfer is unconfirmed.");
            if(!Current())return;receipt=Copy(result);catalog=null;review=null;feedback.ConfirmReceiptAndBeginClear(result.Recorded);fallback=feedback.FailureExplanation;Changed?.Invoke();
            if(!Current())return;await journal.ClearReportingReassignmentAsync(original);if(!Current())return;pending=null;feedback.Reset();
            Confirmation=result.Recorded?"The reporting transfer was confirmed. Only the former generation ended; the receiving generation remains ongoing.":"This original reporting transfer was confirmed absent. Reload current responsibility before preparing another.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original reporting transfer":"transferring reporting responsibility",fallback);if(Current()){feedback.Failed();Error=text;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
