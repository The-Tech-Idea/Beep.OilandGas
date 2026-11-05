using System.Text.Json;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DevelopmentDraftWorkspaceState
{
    private DevelopmentReviewStart? preparedHandoff,pendingHandoff;
    private DevelopmentReviewStartReceipt? handoffReceipt;
    private bool handoffJournalUnavailable;
    public DevelopmentReviewStart? PreparedHandoff=>preparedHandoff is null?null:Copy(preparedHandoff);
    public DevelopmentReviewStart? PendingHandoff=>pendingHandoff?.Source.PlanId==RequestedPlanId?Copy(pendingHandoff):null;
    public string? RetainedReviewPlanId=>pendingHandoff?.Source.PlanId;
    public DevelopmentReviewStartReceipt? HandoffReceipt=>handoffReceipt is null?null:Copy(handoffReceipt);
    public string? HandoffError{get;private set;}
    public string? HandoffConfirmation{get;private set;}
    public bool CanRequestReview=>CanInspectWorkflows && CanCreate && company.Selection?.Permissions.Contains("Workflow.Start",StringComparer.Ordinal)==true &&
        !operation && !Loading && pendingHandoff is null && !handoffJournalUnavailable && handoffReceipt?.Recorded!=true && reviewStatus?.Review is null;
    public bool CanRecoverHandoff=>!operation && Loaded && CanCreate && company.Selection?.Permissions.Contains("Workflow.View",StringComparer.Ordinal)==true &&
        pendingHandoff is not null && pendingHandoff.Source.PlanId==RequestedPlanId && Context is not null &&
        pendingHandoff.Source.Context.Company==Context.Company && pendingHandoff.Source.Context.ActorUserId==Context.ActorUserId &&
        pendingHandoff.Source.Context.Database==Context.Database && pendingHandoff.Source.Context.FieldId==Context.FieldId && pendingHandoff.Source.PlanningDatabase==PlanningDatabase;
    private void ResetHandoff(){preparedHandoff=null;pendingHandoff=null;handoffReceipt=null;HandoffError=null;HandoffConfirmation=null;handoffJournalUnavailable=false;}
    public void CancelPreparedHandoff(){if(operation || pendingHandoff is not null)return;preparedHandoff=null;Changed?.Invoke();}
    public async Task PrepareHandoffAsync(string processId)
    {
        if(!CanRequestReview || workflows is null || sourceReview is null || loadedScope is null)return;
        var choice=workflows.Choices.SingleOrDefault(row=>row.ProcessId==processId);if(choice is null)return;
        var original=Copy(sourceReview);var catalog=Copy(workflows);var captured=loadedScope.Value;var version=generation;
        operation=true;preparedHandoff=null;HandoffError=null;HandoffConfirmation=null;Changed?.Invoke();
        bool Current()=>!disposed && version==generation && company.Snapshot==captured;
        try
        {
            await RequireCurrent(original.Context,captured,true);
            var result=Copy(await client.PrepareDraftReviewAsync(original.PlanId,new(original.Revision,catalog.Revision,processId)));if(!Current())return;result.Require();result.RequireSource(original);
            if(result.Source.Context!=original.Context || result.LifecycleDatabase!=catalog.LifecycleDatabase ||
                JsonSerializer.Serialize(result.Workflow with{Steps=choice.Steps})!=JsonSerializer.Serialize(choice) || result.Workflow.Steps.Count!=choice.Steps.Count ||
                result.Workflow.Steps.Where((step,index)=>JsonSerializer.Serialize(step with{PersonaCodes=choice.Steps[index].PersonaCodes})!=JsonSerializer.Serialize(choice.Steps[index])).Any())
                throw new InvalidOperationException("The prepared handoff differs from the displayed source or selected workflow.");
            await RequireCurrent(original.Context,captured,true);if(Current())preparedHandoff=result;
        }
        catch(Exception error){var text=explain(error,"preparing independent development review","The development review handoff could not be prepared");if(Current())HandoffError=text;}
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task SubmitHandoffAsync()
    {
        if(!CanRequestReview || preparedHandoff is null || loadedScope is null)return;
        var original=Copy(preparedHandoff);var captured=loadedScope.Value;var version=generation;operation=true;ResetProgress();HandoffError=null;HandoffConfirmation=null;Changed?.Invoke();
        bool Current()=>!disposed && version==generation && company.Snapshot==captured;
        try
        {
            await RequireCurrent(original.Source.Context,captured,true);if(!Current())return;pendingHandoff=original;preparedHandoff=null;Changed?.Invoke();
            await journal.RetainReviewAsync(original);await RequireCurrent(original.Source.Context,captured,true);
            if(!Current())return;
            var receipt=await client.SubmitDraftReviewAsync(original);receipt.Require(original);if(!receipt.Recorded)throw new InvalidOperationException("The original review submission was not confirmed recorded.");
            await RequireCurrent(original.Source.Context,captured,true);await journal.ClearReviewAsync(original);
            if(Current()){pendingHandoff=null;handoffReceipt=Copy(receipt);HandoffConfirmation="Development review requested and its initial assignments confirmed. Independent approval is still required.";}
        }
        catch(Exception error){var text=explain(error,"submitting the original development review","The original review outcome is unconfirmed. Recover it before requesting another review");if(Current())HandoffError=text;}
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverHandoffAsync()
    {
        if(!CanRecoverHandoff || pendingHandoff is null || loadedScope is null)return;
        var original=Copy(pendingHandoff);var captured=loadedScope.Value;var version=generation;operation=true;ResetProgress();HandoffError=null;HandoffConfirmation=null;Changed?.Invoke();
        bool Current()=>!disposed && version==generation && company.Snapshot==captured;
        try
        {
            await RequireCurrent(original.Source.Context,captured,false);
            var receipt=await client.RecoverDraftReviewAsync(original);receipt.Require(original);
            await RequireCurrent(original.Source.Context,captured,false);await journal.ClearReviewAsync(original);
            if(Current()){pendingHandoff=null;preparedHandoff=null;handoffReceipt=Copy(receipt);HandoffConfirmation=receipt.Recorded?
                "The original development review handoff was found and confirmed. No new assignment was written.":"The original development review handoff was confirmed absent. Inspect current workflows before preparing another request.";if(!receipt.Recorded)workflows=null;}
        }
        catch(Exception error){var text=explain(error,"recovering the original development review","The original review outcome is still unconfirmed. Its recovery input was preserved");if(Current())HandoffError=text;}
        finally{operation=false;if(!disposed)Changed?.Invoke();}
    }
}
