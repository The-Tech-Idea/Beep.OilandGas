using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingExecutionAssignmentState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDrillingServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception,string,string,string> explain;
    private readonly RetainedOriginalFeedback feedback = new();
    private bool disposed, reading, writing, ready;
    private long generation, request;
    private string? reviewId;
    private DrillingExecutionAssignmentReview? review;
    private DrillingExecutionAssignment? prepared, pending;
    private DrillingExecutionAssignmentReceipt? receipt;
    public event Action? Changed;
    public bool Busy => reading || writing;
    public string? Error { get; private set; }
    public string? Confirmation { get; private set; }
    public DrillingExecutionAssignmentReview? Review => Copy(review);
    public DrillingExecutionAssignment? Prepared => Copy(prepared);
    public DrillingExecutionAssignment? Pending => pending?.Source.Basis.ReviewInstanceId == reviewId ? Copy(pending) : null;
    public string? RetainedReviewId => pending?.Source.Basis.ReviewInstanceId;
    public DrillingExecutionAssignmentReceipt? Receipt => Copy(receipt);
    public string PendingTitle => feedback.Title("reporting assignment");
    public string? PendingExplanation => pending is null ? null : ConflictAcknowledged && conflict is not null ? ConflictCleanupExplanation : feedback.Explanation;
    public bool PendingTabCopyConfirmed => pending is not null && feedback.TabCopyConfirmed;
    public bool CanView => company.Selection is {} selected && new[] { "Drilling.View", "Drilling.ViewDailyReports", "Accounting.View", "DevelopmentPlanning.View", "Workflow.View" }
        .All(permission => selected.Permissions.Contains(permission, StringComparer.Ordinal));
    private bool WriteRights => CanView && new[] { "Drilling.Edit", "Drilling.ManageWellProgram", "Workflow.Start" }
        .All(permission => company.Selection!.Permissions.Contains(permission, StringComparer.Ordinal));
    private bool ProfileMatches => review is not null && review.SenderRoles.Count > 0 &&
        personas.CurrentProfile?.UserId == company.Selection?.UserId && personas.CurrentProfile?.PersonaCode == review.SenderPersonaCode &&
        personas.CurrentPersona?.Code == review.SenderPersonaCode && review.SenderRoles.All(role => company.Selection!.Roles.Contains(role, StringComparer.Ordinal));
    public bool CanPrepare => !Busy && ready && WriteRights && ProfileMatches && review!.CanAssign && pending is null && receipt is null;
    public bool CanRecover => !Busy && ready && CanView && Pending is not null &&
        pending!.Scope().Company == company.Selection!.Scope && pending.Scope().ActorUserId == company.Selection.UserId;
    public DrillingExecutionAssignmentState(CompanyClientContext company, IPersonaContextService personas, IDrillingServiceClient client,
        IDevelopmentDraftIntentStore journal, Func<Exception,string,string,string> explain)
    {
        this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;
        company.Changed+=Invalidate;personas.Changed+=Invalidate;
    }
    private static T? Copy<T>(T? value) where T:class => value is null ? null : CompanyInventorySourceReview.Copy(value);
    private void Invalidate()
    {
        ++generation;reading=false;ready=false;review=null;prepared=null;pending=null;receipt=null;conflict=null;ConflictAcknowledged=false;feedback.Reset();Error=null;Confirmation=null;
        if(!disposed)Changed?.Invoke();
    }
    private bool Active((long Version,CompanyClientSelection? Selection) selected,long epoch) => !disposed && company.Snapshot==selected && generation==epoch;
    public async Task LoadAsync(string? requestedReview)
    {
        if(disposed)return;var sequence=++request;reviewId=requestedReview;Invalidate();var selected=company.Snapshot;
        if(!CanView || selected.Selection is null)return;
        var epoch=generation;bool Current()=>sequence==request && Active(selected,epoch);reading=true;Changed?.Invoke();
        try
        {
            if(!LeaseSummaryValidator.Identifier(reviewId) || !reviewId.StartsWith(DrillingProgramReviewStart.InstancePrefix,StringComparison.Ordinal))
                throw new InvalidOperationException("Open reporting assignment from an exact recorded execution.");
            var original=await journal.ReadExecutionAssignmentAsync(selected.Selection.Scope,selected.Selection.UserId);
            if(!Current())return;
            if(original is not null)
            {
                original.Require();if(original.Scope().Company!=selected.Selection.Scope || original.Scope().ActorUserId!=selected.Selection.UserId)
                    throw new InvalidOperationException("The retained reporting assignment belongs to another company/account.");
                pending=Copy(original);feedback.ReadFromTab();ready=true;return;
            }
            var found=await client.GetExecutionAssignmentAsync(reviewId);if(!Current())return;
            found.Require(selected.Selection.Scope,selected.Selection.UserId,reviewId);
            if(WriteRights && found.SenderRoles.Count>0)
            {
                await personas.EnsureLoadedAsync();if(disposed || selected!=company.Snapshot || sequence!=request)return;epoch=generation;
            }
            if(Current()){review=Copy(found);ready=true;}
        }
        catch(Exception error)
        {
            var text=explain(error,"loading original execution reporting responsibility","The recorded execution and configured reporting assignment could not be confirmed");
            if(Current()){review=null;Error=text;}
        }
        finally{if(Current()){reading=false;Changed?.Invoke();}}
    }
    public void Prepare(string? processId,string? role,string? reason)
    {
        if(!CanPrepare)return;Error=null;prepared=null;
        try
        {
            var option=review!.Options.SingleOrDefault(row=>row.ProcessId==processId)??throw new InvalidOperationException("Choose an actual configured reporting workflow.");
            if(!review.SenderRoles.Contains(role??"",StringComparer.Ordinal))throw new InvalidOperationException("Choose your current configured sender role.");
            var original=new DrillingExecutionAssignment(Copy(review.Source)!,Copy(option)!,role!,review.SenderPersonaCode!,review.SenderAuthorityRevision!,Guid.NewGuid(),reason?.Trim()??"");
            original.Require();prepared=original;
        }
        catch(Exception error){Error=explain(error,"preparing original reporting assignment","Choose the configured reporting role/recipients and enter an explicit assignment reason");}
        Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>Record(false);
    public Task RecoverAsync()=>Record(true);
    private async Task Record(bool recover)
    {
        if(disposed || (recover?!CanRecover:!CanPrepare || prepared is null))return;
        var original=Copy(recover?pending:prepared)!;var selected=company.Snapshot;var epoch=generation;bool Current()=>Active(selected,epoch);
        writing=true;Error=null;Confirmation=null;conflict=null;ConflictAcknowledged=false;
        if(!recover){receipt=null;pending=original;prepared=null;feedback.BeginSaving();}
        var fallback=feedback.FailureExplanation;Changed?.Invoke();
        try
        {
            if(!recover)
            {
                if(!Current())return;await journal.RetainExecutionAssignmentAsync(original);if(!Current())return;
                feedback.ConfirmSaved();feedback.AttemptSubmission();fallback=feedback.FailureExplanation;Changed?.Invoke();if(!Current())return;
            }
            var result=recover?await client.RecoverExecutionAssignmentAsync(original):await client.AssignExecutionAsync(original);
            result.Require(original);if(!recover && !result.Recorded)throw new InvalidOperationException("The original reporting assignment is unconfirmed.");
            if(!Current())return;receipt=Copy(result);review=null;feedback.ConfirmReceiptAndBeginClear(result.Recorded);fallback=feedback.FailureExplanation;Changed?.Invoke();
            if(!Current())return;await journal.ClearExecutionAssignmentAsync(original);if(!Current())return;pending=null;feedback.Reset();
            Confirmation=result.Recorded?"The original reporting responsibility and configured persona tasks were confirmed recorded. This does not record a daily report, completion or handover.":
                "This original reporting assignment was confirmed absent. Reload current source and recipients before preparing another.";
        }
        catch(Exception error){var text=explain(error,recover?"recovering original reporting assignment":"recording reporting responsibility",fallback);if(Current()){feedback.Failed();Error=text;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;++request;Invalidate();}
}
