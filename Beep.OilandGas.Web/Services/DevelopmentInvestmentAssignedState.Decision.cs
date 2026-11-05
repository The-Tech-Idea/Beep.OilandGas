using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;

namespace Beep.OilandGas.Web.Services;
public sealed partial class DevelopmentInvestmentAssignedState
{
    private DevelopmentInvestmentStageDecision? prepared,pending,lastOriginal;
    private DevelopmentInvestmentStageReceipt? receipt;
    private bool journalReady,deciding;
    public string? RequestedInstanceId{get;private set;}
    public bool Busy=>deciding;
    public string? DecisionError{get;private set;}
    public string? DecisionConfirmation{get;private set;}
    public DevelopmentInvestmentStageDecision? Prepared=>prepared is null?null:CompanyInventorySourceReview.Copy(prepared);
    public DevelopmentInvestmentStageDecision? Pending=>pending?.Review.InstanceId==RequestedInstanceId?CompanyInventorySourceReview.Copy(pending):null;
    public string? RetainedInstanceId=>pending?.Review.InstanceId;
    public DevelopmentInvestmentStageDecision? AcknowledgedOriginal=>lastOriginal is null?null:CompanyInventorySourceReview.Copy(lastOriginal);
    public DevelopmentInvestmentStageReceipt? Receipt=>receipt is null?null:CompanyInventorySourceReview.Copy(receipt);
    public bool CanPrepare=>!deciding && !Loading && CanReview && journalReady && pending is null && receipt is null && current is {ReservedDecisionRevision:null} && current.Source.Recorded is null && current.Source.Blockers.Count==0;
    public bool CanRecover=>!deciding && !Loading && CanView && Pending is not null;
    public bool CanContinue=>CanRecover && CanReview && receipt?.Status is "RESERVED" or "FINANCIAL_RECORDED" && pending is not null &&
        personas.CurrentProfile?.UserId==pending.Scope().ActorUserId && personas.CurrentPersona?.Code==pending.Review.PersonaCode && company.Selection!.Roles.Contains(pending.Review.AssignedRole,StringComparer.Ordinal);
    private void ResetDecision(){prepared=null;pending=null;lastOriginal=null;receipt=null;journalReady=false;DecisionError=null;DecisionConfirmation=null;}
    public bool CanRestoreOriginal=>!deciding && !Loading && CanView && journalReady && pending is null && receipt is null && LeaseSummaryValidator.Identifier(RequestedInstanceId) && RequestedInstanceId!.StartsWith(DevelopmentInvestmentHandoff.InstancePrefix,StringComparison.Ordinal);
    public async Task RestoreOriginalAsync()
    {
        if(!CanRestoreOriginal)return;var selected=company.Snapshot;var epoch=generation;var instance=RequestedInstanceId!;bool Active()=>!disposed && selected==company.Snapshot && epoch==generation;
        deciding=true;DecisionError=null;DecisionConfirmation=null;prepared=null;Changed?.Invoke();
        try
        {
            var found=await client.ReadOwnInvestmentStageOriginalAsync(instance);found.Require(selected.Selection!.Scope,selected.Selection.UserId,instance);if(!Active())return;
            if(found.Original is {} original)
            {
                pending=CompanyInventorySourceReview.Copy(original);current=null;await journal.RetainInvestmentStageAsync(original);if(!Active())return;
                DecisionConfirmation="The exact original decision was retained from the recorded assignment. Recover its outcome before continuing.";
            }
            else DecisionConfirmation="No original stage decision for your account was found in the verified assignment. Reload its current source before preparing a decision.";
        }
        catch(Exception error){var text=explain(error,"restoring original investment stage input","The original stage recovery input is unconfirmed");if(Active())DecisionError=text;}
        finally{deciding=false;if(!disposed)Changed?.Invoke();}
    }
    public void PrepareDecision(string? decision,string? rationale)
    {
        if(!CanPrepare)return;prepared=null;DecisionError=null;DecisionConfirmation=null;
        try{var value=new DevelopmentInvestmentStageDecision(CompanyInventorySourceReview.Copy(current!),Guid.NewGuid(),decision??"",rationale?.Trim()??"");value.Require();prepared=value;}
        catch(Exception error){DecisionError=explain(error,"preparing independent investment stage","Choose approve or reject and provide the original review rationale");}
        Changed?.Invoke();
    }
    public void CancelDecision(){if(deciding || pending is not null)return;prepared=null;Changed?.Invoke();}
    public Task SubmitDecisionAsync()=>RecordDecision(false,false);
    public Task RecoverDecisionAsync()=>RecordDecision(true,false);
    public Task ContinueDecisionAsync()=>RecordDecision(false,true);
    private async Task RecordDecision(bool recover,bool resume)
    {
        if(recover?!CanRecover:resume?!CanContinue:!CanPrepare || prepared is null)return;
        var original=CompanyInventorySourceReview.Copy(recover || resume?pending!:prepared!);var selected=company.Snapshot;var epoch=generation;
        bool Active()=>!disposed && selected==company.Snapshot && epoch==generation;
        deciding=true;DecisionError=null;DecisionConfirmation=null;if(!recover && !resume){pending=original;prepared=null;}Changed?.Invoke();
        try
        {
            if(!recover){await journal.RetainInvestmentStageAsync(original);if(!Active())return;}
            var observed=recover?await client.RecoverAssignedInvestmentStageAsync(original):await client.DecideAssignedInvestmentStageAsync(original);observed.Require(original);
            if(!recover && observed.Status!="RECORDED")throw new InvalidOperationException("The original investment stage is not confirmed complete. Recover its retained input.");
            if(!Active())return;
            if(observed.Status is "RECORDED" or "ABSENT")
            {
                await journal.ClearInvestmentStageAsync(original);if(!Active())return;pending=null;prepared=null;current=null;
            }
            receipt=CompanyInventorySourceReview.Copy(observed);lastOriginal=original;
            DecisionConfirmation=observed.Status switch{
                "RECORDED"=>observed.Financial is null?"The configured stage decision and next assignment were recorded. No final financial outcome was created.":"The final financial outcome and native stage/task closure were confirmed recorded. Commitment and expenditure remain separate.",
                "ABSENT"=>"The original decision was confirmed absent. Reload the current assigned source before preparing another decision.",
                "FINANCIAL_RECORDED"=>"The original financial outcome is recorded; assignment completion is still pending. Continue the same retained decision after confirming current review authority.",
                _=>"The original stage is reserved; its decision is not complete. Continue the same retained decision after confirming current review authority."};
        }
        catch(Exception error){var message=explain(error,recover?"recovering original investment stage":"recording original investment stage","The original stage outcome is unconfirmed. Its exact recovery input was preserved");if(Active())DecisionError=message;}
        finally{deciding=false;if(!disposed)Changed?.Invoke();}
    }
}
