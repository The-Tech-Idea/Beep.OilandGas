using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned policy intent retention; presentation invalidation never discards an uncertain write.</summary>
public sealed class CompanyBillingPolicyWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyBillingPolicyReview? history;
    private CompanyBillingPolicyCommandReview? review;
    private CompanyBillingPolicyDraftSubmission? pendingDraft;
    private CompanyBillingPolicyApprovalSubmission? pendingApproval;
    private CompanyBillingPolicyResult? result;
    private object? owner;
    private long version;
    private bool disposed;
    public CompanyBillingPolicyWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public bool HasPendingIntent=>pendingDraft is not null || pendingApproval is not null;
    public string? PendingAction=>!PendingMatchesSelection()?null:pendingDraft is not null?"DRAFT":"APPROVE";
    public string? PendingIntentId=>!PendingMatchesSelection()?null:pendingDraft?.Command.IntentId ??pendingApproval!.Command.IntentId;
    public string? PendingPolicyId=>!PendingMatchesSelection()?null:pendingDraft?.Command.PolicyId ??pendingApproval!.Command.PolicyId;
    public CompanyBillingPolicyDraftSubmission? PendingDraft=>pendingDraft is null || !PendingMatchesSelection()?null:PurchaseReceiptDecisionProof.Copy(pendingDraft);
    public CompanyBillingPolicyApprovalSubmission? PendingApproval=>pendingApproval is null || !PendingMatchesSelection()?null:PurchaseReceiptDecisionProof.Copy(pendingApproval);
    public CompanyBillingPolicyReview? History=>history is null?null:PurchaseReceiptDecisionProof.Copy(history);
    public CompanyBillingPolicyCommandReview? Review=>review is null?null:PurchaseReceiptDecisionProof.Copy(review);
    public CompanyBillingPolicyResult? Result=>result is null?null:PurchaseReceiptDecisionProof.Copy(result);
    public string? SelectedDraftId{get;private set;}
    public bool CanRetryPending=>!disposed && owner is not null && !Busy && PendingMatchesSelection();
    public IReadOnlyList<string> ReviewableDraftIds=>review is not {Action:"APPROVE"}?[]:review.History.Records.Where(row=>
        CompanyBillingPolicyProof.Require(review.History.Company,row).Record is {ApprovedBy:null} prepared && prepared.PreparedBy!=review.History.ActorUserId).Select(row=>row.ACCOUNTING_METHOD_ID).ToArray();
    public void AttachOwner(object presentation)
    {ArgumentNullException.ThrowIfNull(presentation);if(ReferenceEquals(owner,presentation))return;owner=presentation;Invalidate();}
    public void DetachOwner(object presentation)
    {if(!ReferenceEquals(owner,presentation))return;owner=null;Invalidate();}
    public void Invalidate(){++version;history=null;review=null;result=null;SelectedDraftId=null;Changed?.Invoke();}
    private bool PendingMatchesSelection()
    {
        var scope=pendingDraft?.Command.Company ??pendingApproval?.Command.Company;var actor=pendingDraft?.Command.ActorUserId ??pendingApproval?.Command.ActorUserId;
        return !disposed && HasPendingIntent && company.Selection is {} selected && selected.Scope==scope && selected.UserId==actor && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    }
    private static string Permission(string action)=>action=="DRAFT"?"Accounting.PrepareBillingPolicy":action=="APPROVE"?"Accounting.ApproveBillingPolicy":throw new InvalidOperationException("An explicit policy action is required.");
    private void Ready(string? action=null)
    {
        if(disposed || Busy || owner is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) ||
            action is not null && !selected.Permissions.Contains(Permission(action),StringComparer.Ordinal))throw new InvalidOperationException("Return to the original policy company/account with current authority and wait for the active operation.");
    }
    public async Task LoadHistoryAsync(IAccountingServiceClient client)
    {
        Ready();var snapshot=company.Snapshot;var generation=++version;history=null;review=null;result=null;SelectedDraftId=null;Busy=true;Changed?.Invoke();
        try
        {
            var loaded=PurchaseReceiptDecisionProof.Copy(await client.GetCompanyBillingPoliciesAsync() ??throw new InvalidOperationException("Complete billing policy history is unavailable."));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId);
            if(!disposed && generation==version && snapshot==company.Snapshot)history=loaded;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task LoadReviewAsync(string action,IAccountingServiceClient client)
    {
        Ready(action);if(HasPendingIntent)throw new InvalidOperationException("Reconcile the original retained intent before another policy action.");
        var snapshot=company.Snapshot;var generation=++version;review=null;history=null;result=null;SelectedDraftId=null;Busy=true;Changed?.Invoke();
        try
        {
            var loaded=PurchaseReceiptDecisionProof.Copy(await client.GetCompanyBillingPolicyCommandReviewAsync(action) ??throw new InvalidOperationException("The original policy action review is unavailable."));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,action);
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;review=loaded;history=PurchaseReceiptDecisionProof.Copy(loaded.History);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void SelectDraft(string id)
    {Ready("APPROVE");if(HasPendingIntent || !ReviewableDraftIds.Contains(id,StringComparer.Ordinal))throw new InvalidOperationException("Select a current verified draft prepared by another account.");SelectedDraftId=id;Changed?.Invoke();}
    public async Task PrepareAsync(string policyId,CompanyBillingPolicyRules rules,string reason,IAccountingServiceClient client)
    {
        Ready("DRAFT");if(HasPendingIntent || review is not {Action:"DRAFT"} current)throw new InvalidOperationException("Review current preparation authority and reconcile any earlier intent first.");
        var selected=company.Selection!;var command=new CompanyBillingPolicyDraft(Guid.NewGuid().ToString("N"),selected.Scope,selected.UserId,policyId,
            PurchaseReceiptDecisionProof.Copy(rules),reason,current.History.Revision,current.BindingRevision);current.Require(command);
        pendingDraft=new(command,PurchaseReceiptDecisionProof.Copy(current));await SubmitPendingAsync(client);
    }
    public async Task ApproveAsync(string reason,IAccountingServiceClient client)
    {
        Ready("APPROVE");if(HasPendingIntent || review is not {Action:"APPROVE"} current || SelectedDraftId is null || !ReviewableDraftIds.Contains(SelectedDraftId,StringComparer.Ordinal))throw new InvalidOperationException("Select an unchanged independently prepared draft under current approval authority.");
        var selected=company.Selection!;var row=current.History.Records.Single(value=>value.ACCOUNTING_METHOD_ID==SelectedDraftId);var proof=CompanyBillingPolicyProof.Require(selected.Scope,row);
        var command=new CompanyBillingPolicyApproval(Guid.NewGuid().ToString("N"),selected.Scope,selected.UserId,SelectedDraftId,proof.RowRevision,reason,current.History.Revision,current.BindingRevision);current.Require(command);
        pendingApproval=new(command,PurchaseReceiptDecisionProof.Copy(current));await SubmitPendingAsync(client);
    }
    public async Task SubmitPendingAsync(IAccountingServiceClient client)
    {
        if(!PendingMatchesSelection())throw new InvalidOperationException("Return to the original policy company and account to reconcile.");Ready(PendingAction);
        var draft=pendingDraft is null?null:PurchaseReceiptDecisionProof.Copy(pendingDraft);var approval=pendingApproval is null?null:PurchaseReceiptDecisionProof.Copy(pendingApproval);
        var snapshot=company.Snapshot;var generation=version;bool Current()=>!disposed && generation==version && snapshot==company.Snapshot;
        Busy=true;result=null;review=null;history=null;SelectedDraftId=null;Changed?.Invoke();
        try
        {
            var outcome=PurchaseReceiptDecisionProof.Copy(draft is not null?await client.PrepareCompanyBillingPolicyAsync(PurchaseReceiptDecisionProof.Copy(draft)):await client.ApproveCompanyBillingPolicyAsync(PurchaseReceiptDecisionProof.Copy(approval!)));
            if(draft is not null)outcome.Require(draft);else outcome.Require(approval!);if(!Current())return;
            var loaded=PurchaseReceiptDecisionProof.Copy(await client.GetCompanyBillingPoliciesAsync() ??throw new InvalidOperationException("Complete billing policy history is unavailable."));if(!Current())return;loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId);
            var id=draft?.Command.PolicyId ??approval!.Command.PolicyId;var stored=loaded.Records.Where(value=>value.ACCOUNTING_METHOD_ID==id).ToArray();
            if(stored.Length!=1 || JsonSerializer.Serialize(stored[0])!=JsonSerializer.Serialize(outcome.Policy))throw new InvalidOperationException("Complete current company history does not confirm the exact original policy outcome.");
            _=CompanyBillingPolicyProof.Require(loaded.Company,outcome.Policy);if(!Current())return;
            history=loaded;result=outcome;pendingDraft=null;pendingApproval=null;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;owner=null;company.Changed-=Invalidate;Invalidate();}
}
