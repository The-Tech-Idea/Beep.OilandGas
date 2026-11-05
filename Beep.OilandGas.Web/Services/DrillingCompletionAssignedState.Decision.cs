using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class DrillingCompletionAssignedState
{
    private readonly IDevelopmentDraftIntentStore journal;
    private readonly RetainedOriginalFeedback feedback=new();
    private DrillingCompletionReviewDecision? preparedDecision,pendingDecision;
    private DrillingCompletionReviewDecisionReceipt? decisionReceipt;
    public string? RequestedProcessId{get;private set;}
    public string? DecisionConfirmation{get;private set;}
    public DrillingCompletionReviewDecision? PreparedDecision=>preparedDecision is null?null:CompanyInventorySourceReview.Copy(preparedDecision);
    public DrillingCompletionReviewDecision? PendingDecision=>pendingDecision?.Review.InstanceId==RequestedProcessId?CompanyInventorySourceReview.Copy(pendingDecision):null;
    public string? RetainedDecisionProcessId=>pendingDecision?.Review.InstanceId;
    public DrillingCompletionReviewDecisionReceipt? DecisionReceipt=>decisionReceipt is null?null:CompanyInventorySourceReview.Copy(decisionReceipt);
    public string PendingTitle=>feedback.Title("completion review decision");
    public string PendingExplanation=>feedback.Explanation;
    public bool TabCopyConfirmed=>pendingDecision is not null && feedback.TabCopyConfirmed;
    public bool CanRead=>company.Selection is {} selected && new[]{"Drilling.View","Drilling.ViewDailyReports","Accounting.View","DevelopmentPlanning.View","Workflow.View"}.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    private bool CanAct(DrillingCompletionAssignedReview review)=>CanRead && company.Selection!.Permissions.Contains("Drilling.Approve",StringComparer.Ordinal) && company.Selection.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal) &&
        company.Selection.Roles.Contains(review.AssignedRole,StringComparer.Ordinal) && personas.CurrentProfile?.UserId==company.Selection.UserId && personas.CurrentProfile.PersonaCode==review.PersonaCode && personas.CurrentPersona?.Code==review.PersonaCode;
    public bool CanPrepareDecision=>!Busy && current is not null && CanAct(current) && preparedDecision is null && pendingDecision is null && decisionReceipt is null;
    public bool CanSubmitDecision=>!Busy && preparedDecision is not null && pendingDecision is null && CanAct(preparedDecision.Review);
    public bool CanRecoverDecision=>!Busy && CanRead && PendingDecision is {} original && original.Scope().Company==company.Selection!.Scope && original.Scope().ActorUserId==company.Selection.UserId;
    private void ResetDecision(){preparedDecision=null;pendingDecision=null;decisionReceipt=null;feedback.Reset();DecisionConfirmation=null;}
    public void PrepareDecision(string decision,string? rationale)
    {
        if(!CanPrepareDecision)return;Error=null;
        try{var original=new DrillingCompletionReviewDecision(CompanyInventorySourceReview.Copy(current!),Guid.NewGuid(),decision,rationale?.Trim()??"");original.Require();preparedDecision=original;ResetTestInspection();ResetAcceptanceDraft();}
        catch(Exception error){Error=explain(error,"preparing independent completion review","Choose an explicit stage decision and provide a reason against this exact claim");}
        Changed?.Invoke();
    }
    public void CancelDecision(){if(Busy || pendingDecision is not null)return;preparedDecision=null;Changed?.Invoke();}
    public Task SubmitDecisionAsync()=>RecordDecision(false);
    public Task RecoverDecisionAsync()=>RecordDecision(true);
    private async Task RecordDecision(bool recover)
    {
        if(disposed || (recover?!CanRecoverDecision:!CanSubmitDecision))return;
        var original=CompanyInventorySourceReview.Copy(recover?pendingDecision!:preparedDecision!);var captured=company.Snapshot;var epoch=generation;
        bool Active()=>!disposed && company.Snapshot==captured && generation==epoch;
        writing=true;Error=null;DecisionConfirmation=null;
        if(!recover){pendingDecision=original;preparedDecision=null;current=null;decisionReceipt=null;feedback.BeginSaving();}
        var fallback=feedback.FailureExplanation;Changed?.Invoke();
        try
        {
            if(!recover)
            {
                await journal.RetainCompletionDecisionAsync(original);if(!Active())return;
                feedback.ConfirmSaved();feedback.AttemptSubmission();fallback=feedback.FailureExplanation;Changed?.Invoke();if(!Active())return;
            }
            var receipt=recover?await client.RecoverCompletionReviewDecisionAsync(original):await client.DecideCompletionReviewAsync(original);receipt.Require(original);
            if(!recover && !receipt.Recorded)throw new InvalidOperationException("The original completion review decision was not confirmed recorded.");if(!Active())return;
            decisionReceipt=CompanyInventorySourceReview.Copy(receipt);feedback.ConfirmReceiptAndBeginClear(receipt.Recorded);fallback=feedback.FailureExplanation;Changed?.Invoke();if(!Active())return;
            await journal.ClearCompletionDecisionAsync(original);if(!Active())return;
            pendingDecision=null;feedback.Reset();current=null;
            DecisionConfirmation=receipt.CompletionAccepted?"The original independent physical completion acceptance, checked test signoffs and final review decision were confirmed recorded. Ongoing reporting closure, WELL/financial updates and production release remain separate.":receipt.Recorded?"The exact independent stage decision and its original task routing were confirmed recorded. Physical test acceptance, reporting closure and production release remain separate.":
                "This exact decision was confirmed absent. Reload the current assigned claim before preparing another decision.";
        }
        catch(Exception error){var message=explain(error,recover?"recovering independent completion decision":"recording independent completion decision",fallback);if(Active()){feedback.Failed();Error=message;}}
        finally{writing=false;if(!disposed)Changed?.Invoke();}
    }
}
