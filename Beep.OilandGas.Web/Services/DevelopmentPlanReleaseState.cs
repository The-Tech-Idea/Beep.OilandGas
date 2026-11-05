using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed class DevelopmentPlanReleaseState : IDisposable
{
    private readonly CompanyClientContext company;
    private readonly IPersonaContextService personas;
    private readonly IDevelopmentServiceClient client;
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly Func<Exception,string,string,string> explain;
    private DevelopmentReviewProgress? displayed;
    private DevelopmentPlanRelease? prepared,pending;
    private DevelopmentPlanBaselineReview? result;
    private bool journalReady,disposed;
    private long generation;
    public bool Busy{get;private set;}
    public string? Error{get;private set;}
    public string? Confirmation{get;private set;}
    public DevelopmentPlanRelease? Prepared=>prepared is null?null:Copy(prepared);
    public DevelopmentPlanRelease? Pending=>pending?.Review.InstanceId==displayed?.InstanceId?Copy(pending):null;
    public string? RetainedProcessId=>pending?.Review.InstanceId;
    public DevelopmentPlanBaselineReview? Result=>result is null?null:Copy(result);
    public bool CanView=>company.Selection is {} selected && new[]{"DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanInspect=>!Busy && displayed is not null && CanView && pending is null;
    public bool CanPrepare=>CanInspect && journalReady && result is {Baseline:null} && displayed!.WorkflowState=="REVIEW_APPROVED" &&
        displayed.Scope.ActorUserId!=displayed.RequesterUserId && displayed.Scope.ActorUserId!=displayed.Plan.ROW_CREATED_BY &&
        company.Selection!.Roles.Contains(displayed.Stages[^1].AssignedRole,StringComparer.Ordinal) &&
        new[]{"DevelopmentPlanning.Approve","Workflow.Approve"}.All(permission=>company.Selection.Permissions.Contains(permission,StringComparer.Ordinal));
    public bool CanRecover=>!Busy && CanView && pending is not null && pending.Review.InstanceId==displayed?.InstanceId && company.Selection?.Scope==pending.Review.Scope.Company &&
        company.Selection.UserId==pending.Review.Scope.ActorUserId && company.Selection.Roles.Contains(pending.Review.Stages[^1].AssignedRole,StringComparer.Ordinal);
    public event Action? Changed;
    private static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))!;
    public DevelopmentPlanReleaseState(CompanyClientContext company,IPersonaContextService personas,IDevelopmentServiceClient client,IDevelopmentDraftIntentStore journal,Func<Exception,string,string,string> explain)
    {this.company=company;this.personas=personas;this.client=client;this.journal=journal;this.explain=explain;company.Changed+=Invalidate;personas.Changed+=Invalidate;}
    private void Invalidate(){++generation;displayed=null;prepared=null;pending=null;result=null;journalReady=false;Error=null;Confirmation=null;Changed?.Invoke();}
    public async Task LoadAsync(DevelopmentReviewProgress review)
    {
        if(disposed)return;Invalidate();var epoch=generation;var selected=company.Snapshot;
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        if(selected.Selection is null || !CanView || Busy)return;
        try
        {
            review.Require(selected.Selection.Scope,selected.Selection.UserId,review.InstanceId);displayed=Copy(review);
            var original=await journal.ReadReleaseAsync(selected.Selection.Scope,selected.Selection.UserId);if(!Active())return;
            if(original is not null){original.Require();if(original.Review.Scope.Company!=selected.Selection.Scope || original.Review.Scope.ActorUserId!=selected.Selection.UserId)throw new InvalidOperationException("The retained baseline belongs to another account or company.");pending=Copy(original);}
            journalReady=true;
        }
        catch(Exception error){var text=explain(error,"loading original FDP baseline input","The original baseline recovery input could not be confirmed");if(Active())Error=text;}
        finally{if(Active())Changed?.Invoke();}
    }
    public async Task InspectAsync()
    {
        if(!CanInspect)return;var review=Copy(displayed!);var epoch=generation;var selected=company.Snapshot;Busy=true;result=null;prepared=null;Error=null;Confirmation=null;Changed?.Invoke();
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        try{var found=await client.GetDraftBaselineAsync(review.InstanceId,review.Revision);found.Require(review);if(Active())result=Copy(found);}
        catch(Exception error){var text=explain(error,"inspecting approved FDP baseline","The approved baseline or its absence could not be confirmed");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Prepare(string rationale)
    {
        if(!CanPrepare)return;prepared=null;Error=null;Confirmation=null;
        try{var original=new DevelopmentPlanRelease(Copy(displayed!),Guid.NewGuid(),rationale.Trim());original.Require();prepared=original;}
        catch(Exception error){Error=explain(error,"reviewing FDP baseline approval","Record an explicit baseline approval rationale before confirming");}Changed?.Invoke();
    }
    public void Cancel(){if(Busy || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitAsync()=>RecordAsync(false);
    public Task RecoverAsync()=>RecordAsync(true);
    private async Task RecordAsync(bool recover)
    {
        if(recover?!CanRecover:!CanPrepare || prepared is null)return;
        var original=Copy(recover?pending!:prepared!);var epoch=generation;var selected=company.Snapshot;Busy=true;Error=null;Confirmation=null;result=null;
        if(!recover){pending=original;prepared=null;}Changed?.Invoke();
        bool Active()=>!disposed && epoch==generation && selected==company.Snapshot;
        try
        {
            if(!recover){await journal.RetainReleaseAsync(original);if(!Active())return;}
            var receipt=recover?await client.RecoverDraftBaselineAsync(original):await client.ApproveDraftBaselineAsync(original);receipt.Require(original);
            if(!recover && receipt.Result.Baseline is null)throw new InvalidOperationException("The FDP baseline approval was not confirmed recorded.");
            if(!Active())return;await journal.ClearReleaseAsync(original);
            if(Active())
            {
                pending=null;prepared=null;journalReady=true;result=receipt.Result.Baseline is null?null:Copy(receipt.Result);
                Confirmation=receipt.Result.Baseline is not null?(recover?"The original approved FDP baseline was confirmed. No new baseline was written.":"The approved FDP baseline was recorded and confirmed. AFE and investment release remain separate."):
                    "The original baseline approval was confirmed absent. Inspect its current review and baseline before preparing another approval.";
            }
        }
        catch(Exception error){var text=explain(error,recover?"recovering original FDP baseline":"approving original FDP baseline","The original baseline outcome is unconfirmed. Its exact recovery input was preserved");if(Active())Error=text;}
        finally{Busy=false;if(!disposed)Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;personas.Changed-=Invalidate;Invalidate();}
}
