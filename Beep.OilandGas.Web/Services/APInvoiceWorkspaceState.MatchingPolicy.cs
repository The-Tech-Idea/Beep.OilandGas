using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class APInvoiceWorkspaceState
{
    private CompanyMatchingPolicyCommandReview? policyCommandReview;
    private CompanyMatchingPolicySubmission? policyIntent;
    private CompanyMatchingPolicyResult? policyResult;
    private bool policyConfirmed;
    public event Action? PolicyCommandChanged;
    public bool PolicyCommandBusy {get;private set;}
    public bool HasUnconfirmedPolicyIntent=>policyIntent is not null && !policyConfirmed;
    public CompanyMatchingPolicyCommandReview? PolicyCommandReview=>policyCommandReview is null?null:PurchaseReceiptDecisionProof.Copy(policyCommandReview);
    public CompanyMatchingPolicySubmission? PolicyIntent=>policyIntent is null?null:PurchaseReceiptDecisionProof.Copy(policyIntent);
    public CompanyMatchingPolicyResult? PolicyResult=>policyResult is null?null:PurchaseReceiptDecisionProof.Copy(policyResult);
    public string? SelectedPolicyDraftId {get;private set;}
    public bool CanReconcilePolicy=>!disposed && !PolicyCommandBusy && HasUnconfirmedPolicyIntent && company.Selection is {} selected &&
        selected.Scope==policyIntent!.Command.Company && selected.UserId==policyIntent.Command.ActorUserId;
    public string? PolicyRecoveryRoute=>CanReconcilePolicy?"/accounting/payables":null;
    public CompanyMatchingPolicyProof? VerifiedPolicyProof(string id)
    {
        var row=policies?.Records.SingleOrDefault(item=>item.ACCOUNTING_METHOD_ID==id);if(row is null)return null;
        try{return CompanyMatchingPolicyProof.Require(policies!.Company,PurchaseReceiptDecisionProof.Copy(row));}catch(InvalidOperationException){return null;}
    }
    public IReadOnlyList<string> ReviewablePolicyDraftIds=>policyCommandReview is not {Action:"APPROVE"} review?[]:review.History.Records
        .Where(row=>row.ACTIVE_IND=="Y" && row.MATCHING_POLICY_STATUS=="DRAFT").Where(row=>
        {
            try{var proof=CompanyMatchingPolicyProof.Require(review.History.Company,row);return proof.Command.Action=="DRAFT" && proof.Command.ActorUserId!=review.History.ActorUserId;}
            catch(InvalidOperationException){return false;}
        }).Select(row=>row.ACCOUNTING_METHOD_ID).ToArray();
    private void InvalidatePolicyCommandDisplay()
    {
        policyCommandReview=null;SelectedPolicyDraftId=null;policyResult=null;
        if(policyConfirmed && !PolicyCommandBusy)policyIntent=null;
        policyConfirmed=false;
    }
    public void SelectPolicyDraft(string id)
    {
        if(CorrectionCommandUnsettled || PolicyCommandBusy || HasUnconfirmedPolicyIntent || !ReviewablePolicyDraftIds.Contains(id,StringComparer.Ordinal))throw new InvalidOperationException("Select a current verified draft prepared by another account.");
        SelectedPolicyDraftId=id;
    }
    public async Task LoadPolicyCommandReviewAsync(string action,Func<string,Task<CompanyMatchingPolicyCommandReview>> read)
    {
        if(CorrectionCommandUnsettled || disposed || PolicyCommandBusy || HasUnconfirmedPolicyIntent || VarianceDecisionCommandBusy || HasUnconfirmedVarianceDecisionIntent || VarianceCommandBusy || HasUnconfirmedVarianceIntent)throw new InvalidOperationException("Reconcile the original source intent before reviewing another action.");
        ClearMatchingPolicies();var generation=policyVersion;var version=Version;var original=company.Snapshot;
        if(original.Selection is null)return;
        var review=PurchaseReceiptDecisionProof.Copy(await read(action));
        if(disposed || generation!=policyVersion || version!=Version || original!=company.Snapshot)return;
        review.Require(original.Selection.Scope,original.Selection.UserId,action);policyCommandReview=review;policies=PurchaseReceiptDecisionProof.Copy(review.History);
    }
    public async Task RecordMatchingPolicyAsync(string? policyId,CompanyMatchingPolicyRules? rules,DateTime? effective,DateTime? expiry,string reason,
        Func<CompanyMatchingPolicySubmission,Task<CompanyMatchingPolicyResult>> record,Func<Task<CompanyMatchingPolicyReview>> readHistory)
    {
        if(CorrectionCommandUnsettled || disposed || PolicyCommandBusy || VarianceDecisionCommandBusy || HasUnconfirmedVarianceDecisionIntent || VarianceCommandBusy || HasUnconfirmedVarianceIntent)throw new InvalidOperationException("Wait for or reconcile the current source command outcome.");
        if(!HasUnconfirmedPolicyIntent)
        {
            if(policyCommandReview is not {} review || company.Selection is not {} selected)throw new InvalidOperationException("Review the current company policy action before submission.");
            review.Require(selected.Scope,selected.UserId,review.Action);
            if(review.Action=="APPROVE")
            {
                if(SelectedPolicyDraftId is null || !ReviewablePolicyDraftIds.Contains(SelectedPolicyDraftId,StringComparer.Ordinal))throw new InvalidOperationException("Select an independently prepared verified draft.");
                var row=review.History.Records.Single(item=>item.ACCOUNTING_METHOD_ID==SelectedPolicyDraftId);
                policyId=row.ACCOUNTING_METHOD_ID;rules=CompanyMatchingPolicyReview.Rules(row);effective=row.EFFECTIVE_DATE;expiry=row.EXPIRY_DATE;
            }
            if(string.IsNullOrWhiteSpace(policyId) || rules is null || effective is null)throw new InvalidOperationException("Explicit policy identity, rules and UTC effective instant are required.");
            var command=new CompanyMatchingPolicyCommand(selected.Scope,selected.UserId,policyId,review.Action,rules,effective.Value,expiry,review.History.Revision,
                review.BindingRevision,Guid.NewGuid().ToString("N"),reason);
            review.Require(command);policyIntent=new(command,PurchaseReceiptDecisionProof.Copy(review));policyConfirmed=false;
        }
        if(!CanReconcilePolicy)throw new InvalidOperationException("Return to the original policy company and account to submit or reconcile.");
        var intent=PurchaseReceiptDecisionProof.Copy(policyIntent!);var original=company.Snapshot;var generation=policyVersion;var version=Version;
        bool Current()=>!disposed && generation==policyVersion && version==Version && original==company.Snapshot;
        PolicyCommandBusy=true;policyResult=null;ClearFinancialReview();PolicyCommandChanged?.Invoke();
        try
        {
            var result=PurchaseReceiptDecisionProof.Copy(await record(PurchaseReceiptDecisionProof.Copy(intent)));result.Require(intent);if(!Current())return;
            var history=PurchaseReceiptDecisionProof.Copy(await readHistory());if(!Current())return;
            history.Require(intent.Command.Company,intent.Command.ActorUserId);
            var stored=history.Records.Where(row=>row.ACCOUNTING_METHOD_ID==intent.Command.PolicyId).ToArray();
            if(stored.Length!=1 || JsonSerializer.Serialize(stored[0])!=JsonSerializer.Serialize(result.Policy))throw new InvalidOperationException("The complete policy history does not confirm the original outcome.");
            var proof=CompanyMatchingPolicyProof.Require(intent.Command.Company,result.Policy);
            var originalRows=history.Records.Where(row=>row.ACCOUNTING_METHOD_ID!=intent.Command.PolicyId).Concat(proof.Original is null?[]:new[]{proof.Original}).ToArray();
            intent.Command.Require(CompanyMatchingPolicyReview.Create(intent.Command.Company,intent.Command.ActorUserId,originalRows));
            if(!Current())return;
            policies=history;policyResult=result;policyConfirmed=true;policyCommandReview=null;SelectedPolicyDraftId=null;
        }
        finally{PolicyCommandBusy=false;PolicyCommandChanged?.Invoke();}
    }
}
