using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned original proposal; uncertainty is reconciled only by the recorded reference.</summary>
public sealed class WorkOrderExecutionPermitState : IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyMaintenanceReceivingQuery? task,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderExecutionPermitIntent? intent;
    private string? sourceRevision;
    private Guid? reference;
    private long version;
    private bool confirmed,disposed;
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyWorkOrderExecutionPermitIssueReview? Review {get;private set;}
    public CompanyWorkOrderExecutionPermitReceipt? Receipt {get;private set;}
    private bool SameOrigin=>task==origin && originalCompany is {} original && company.Selection is {} current && current.Scope==original.Scope && current.UserId==original.UserId;
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderExecutionPermitIntent? VisibleIntent=>SameOrigin && intent is not null?CompanyInventorySourceReview.Copy(intent):null;
    public bool CanIssue=>!Busy && reference is null && Review is not null;
    public WorkOrderExecutionPermitState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public void Attach(CompanyMaintenanceReceivingQuery? next,string? revision)
    {next?.Require();if(task==next && sourceRevision==revision)return;Invalidate();task=next;sourceRevision=revision;}
    private void Invalidate(){version++;task=null;sourceRevision=null;Review=null;Receipt=null;if(confirmed){intent=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();}
    private CompanyClientSelection RequireCurrent()
    {
        if(disposed || task is null || string.IsNullOrWhiteSpace(sourceRevision) || company.Selection is not {} selected)throw new InvalidOperationException("Reload the original permit task/company/FIELD.");
        if(!selected.Permissions.Contains("HSE.IssuePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current HSE.IssuePermit is required.");return selected;
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || reference is not null)throw new InvalidOperationException("Reconcile the original pending permit before another issue review.");var selected=RequireCurrent();var current=task!;var generation=version;var scope=company.Snapshot;Review=null;Receipt=null;Busy=true;Changed?.Invoke();
        try{var result=await client.ReviewWorkOrderExecutionPermitIssueAsync(current);result.Require(selected.Scope,selected.UserId,current);if(!selected.Roles.Contains(result.Source.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current assigned maintenance role is required.");if(!disposed && generation==version && scope==company.Snapshot)Review=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task IssueAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderExecutionPermitIntent proposal,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanIssue)throw new InvalidOperationException("Review once; pending/uncertain issue requires original read-only recovery.");
        var selected=RequireCurrent();Review!.Require(selected.Scope,selected.UserId,task!);proposal=CompanyInventorySourceReview.Copy(proposal);
        if(proposal.Context!=Review.Context || proposal.Task!=task || proposal.InspectionRevision!=Review.Source.SnapshotRevision || proposal.RequirementEvidenceVersion!=Review.RequirementEvidenceVersion || proposal.IntentId!=Guid.Empty)throw new InvalidOperationException("Use the original reviewed source and a new retained proposal identity.");
        proposal=proposal with{IntentId=Guid.NewGuid()};proposal.Require();intent=proposal;reference=proposal.IntentId;origin=task;originalCompany=selected;Review=null;Receipt=null;var generation=version;var scope=company.Snapshot;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(proposal.IntentId);if(disposed || generation!=version || scope!=company.Snapshot)throw new OperationCanceledException("The original issue context changed before transport.");var receipt=await client.IssueWorkOrderExecutionPermitAsync(CompanyInventorySourceReview.Copy(proposal));receipt.Require(proposal);if(!disposed && generation==version && scope==company.Snapshot){Receipt=CompanyInventorySourceReview.Copy(receipt);confirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} retained && retained!=id)throw new InvalidOperationException("Use the same original pending permit reference.");var selected=RequireCurrent();var current=task!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original issuer/company/task before recovery.");
        reference=id;origin=current;originalCompany=selected;Review=null;Receipt=null;var generation=version;var scope=company.Snapshot;Busy=true;Changed?.Invoke();
        try{if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || generation!=version || scope!=company.Snapshot)throw new OperationCanceledException("The original issue context changed before recovery.");var query=new CompanyWorkOrderExecutionPermitReference(current,id);var result=await client.RecoverWorkOrderExecutionPermitReferenceAsync(query);result.Require(selected.Scope,selected.UserId,query);if(intent is not null && intent!=result.Intent)throw new InvalidOperationException("The recorded pending proposal differs from the retained original intent.");if(!disposed && generation==version && scope==company.Snapshot){intent=CompanyInventorySourceReview.Copy(result.Intent);Receipt=CompanyInventorySourceReview.Copy(result.Receipt);confirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
