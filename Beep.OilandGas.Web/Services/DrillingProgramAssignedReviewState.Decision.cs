using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed partial class DrillingProgramAssignedReviewState
{
    private DrillingProgramReviewDecision? preparedDecision,pendingDecision;
    private DrillingProgramReviewDecisionReceipt? decisionReceipt;
    private bool deciding;
    public bool Busy=>deciding;
    public string? RequestedProcessId{get;private set;}
    public string? DecisionError{get;private set;}
    public string? DecisionConfirmation{get;private set;}
    public DrillingProgramReviewDecision? PreparedDecision=>preparedDecision is null?null:CompanyInventorySourceReview.Copy(preparedDecision);
    public DrillingProgramReviewDecision? PendingDecision=>pendingDecision?.Review.InstanceId==RequestedProcessId?CompanyInventorySourceReview.Copy(pendingDecision):null;
    public string? RetainedDecisionProcessId=>pendingDecision?.Review.InstanceId;
    public DrillingProgramReviewDecisionReceipt? DecisionReceipt=>decisionReceipt is null?null:CompanyInventorySourceReview.Copy(decisionReceipt);
    public bool CanPrepareDecision=>!deciding && !Loading && CanView && current is not null && pendingDecision is null && decisionReceipt is null;
    public bool CanRecoverDecision=>!deciding && !Loading && CanRead && pendingDecision is not null && RequestedProcessId==pendingDecision.Review.InstanceId &&
        company.Selection?.Scope==pendingDecision.Scope().Company && company.Selection.UserId==pendingDecision.Scope().ActorUserId;
    private void ResetDecision(){preparedDecision=null;pendingDecision=null;decisionReceipt=null;DecisionError=null;DecisionConfirmation=null;}
    public void PrepareDecision(string decision,string notes)
    {
        if(!CanPrepareDecision)return;preparedDecision=null;DecisionError=null;DecisionConfirmation=null;
        try{var command=new DrillingProgramReviewDecision(CompanyInventorySourceReview.Copy(current!),Guid.NewGuid(),decision,notes.Trim());command.Require();preparedDecision=command;}
        catch(Exception error){DecisionError=explain(error,"reviewing the independent stage decision","Choose approve or reject and provide the recorded review rationale");}
        Changed?.Invoke();
    }
    public void CancelDecision(){if(deciding || pendingDecision is not null)return;preparedDecision=null;Changed?.Invoke();}
    private bool SameDecisionContext(DrillingProgramReviewDecision original,(long Version,CompanyClientSelection? Selection) captured,long epoch)=>
        !disposed && company.Snapshot==captured && generation==epoch;
    public async Task SubmitDecisionAsync()
    {
        if(!CanPrepareDecision || preparedDecision is null)return;
        var original=CompanyInventorySourceReview.Copy(preparedDecision);var captured=company.Snapshot;var epoch=generation;deciding=true;DecisionError=null;DecisionConfirmation=null;
        pendingDecision=original;preparedDecision=null;Changed?.Invoke();
        bool Current()=>SameDecisionContext(original,captured,epoch);
        try
        {
            await journal.RetainProgramDecisionAsync(original);if(!Current())return;
            var receipt=await client.DecideProgramReviewAsync(original);receipt.Require(original);if(!receipt.Recorded)throw new InvalidOperationException("The original review decision was not confirmed recorded.");
            if(!Current())return;await journal.ClearProgramDecisionAsync(original);
            if(Current()){pendingDecision=null;current=null;progress=null;decisionReceipt=CompanyInventorySourceReview.Copy(receipt);DecisionConfirmation=original.Decision=="APPROVE"?"This review stage was approved and its configured routing was confirmed. Drilling execution remains separate.":"This review stage was rejected. Later review work was not approved or released.";}
        }
        catch(Exception error){var text=explain(error,"recording the independent program review","The original decision outcome is unconfirmed. Recover it before another decision");if(Current())DecisionError=text;}
        finally{deciding=false;if(!disposed)Changed?.Invoke();}
    }
    public async Task RecoverDecisionAsync()
    {
        if(!CanRecoverDecision || pendingDecision is null)return;
        var original=CompanyInventorySourceReview.Copy(pendingDecision);var captured=company.Snapshot;var epoch=generation;deciding=true;DecisionError=null;DecisionConfirmation=null;Changed?.Invoke();
        bool Current()=>SameDecisionContext(original,captured,epoch);
        try
        {
            var receipt=await client.RecoverProgramReviewDecisionAsync(original);receipt.Require(original);if(!Current())return;
            await journal.ClearProgramDecisionAsync(original);
            if(Current()){pendingDecision=null;current=null;progress=null;decisionReceipt=CompanyInventorySourceReview.Copy(receipt);DecisionConfirmation=receipt.Recorded?"The original independent stage decision was found and confirmed. No new decision or task was written.":"The original decision was confirmed absent. Reload the current assigned source before preparing another decision.";}
        }
        catch(Exception error){var text=explain(error,"recovering the original program review decision","The original decision is still unconfirmed. Its exact recovery input was preserved");if(Current())DecisionError=text;}
        finally{deciding=false;if(!disposed)Changed?.Invoke();}
    }
}
