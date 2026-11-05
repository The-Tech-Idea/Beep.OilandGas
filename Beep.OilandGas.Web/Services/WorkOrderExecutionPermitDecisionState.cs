using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned independent decision; uncertainty never becomes another submission.</summary>
public sealed partial class WorkOrderExecutionPermitDecisionState : IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyWorkOrderExecutionPermitReference? proposal,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderExecutionPermitDecision? decision;
    private string? sourceRevision;
    private Guid? reference;
    private long version;
    private bool confirmed,disposed;
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyWorkOrderExecutionPermitApprovalReview? Review {get;private set;}
    public CompanyWorkOrderExecutionPermitDecisionReceipt? Receipt {get;private set;}
    private bool SameOrigin=>proposal==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId;
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderExecutionPermitDecision? VisibleDecision=>SameOrigin && decision is not null?CompanyInventorySourceReview.Copy(decision):null;
    public bool CanReview=>!Busy && reference is null;
    public bool CanReject=>CanReview && Review is not null;
    public bool CanApprove
    {
        get
        {
            if(!CanReview || Review is null || company.Selection?.Permissions.Contains("Inventory.View",StringComparer.Ordinal)!=true)return false;
            try{Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);return true;}catch(InvalidOperationException){return false;}
        }
    }
    public WorkOrderExecutionPermitDecisionState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public void Attach(CompanyWorkOrderExecutionPermitReference? next,string? revision)
    {next?.Require();if(proposal==next && sourceRevision==revision)return;Invalidate();proposal=next;sourceRevision=revision;}
    private void Invalidate(){version++;ClearMaterialDisplay();ClearPersonnelDisplay();ClearPersonnelReleaseDisplay();ClearPersonnelAcceptanceDisplay();ClearDocumentAcceptanceDisplay();proposal=null;sourceRevision=null;Review=null;Receipt=null;if(confirmed){decision=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();}
    private CompanyClientSelection RequireCurrent()
    {
        if(disposed || proposal is null || string.IsNullOrWhiteSpace(sourceRevision) || company.Selection is not {} selected)throw new InvalidOperationException("Return to the original independent reviewer/company/permit task.");
        if(!selected.Permissions.Contains("HSE.ApprovePermit",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current independent permit decision authority is required.");return selected;
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("Reconcile the original retained decision before another review.");var selected=RequireCurrent();var query=proposal!;var generation=version;var snapshot=company.Snapshot;Review=null;Receipt=null;Busy=true;Changed?.Invoke();
        try{var result=await client.ReviewWorkOrderExecutionPermitApprovalAsync(query);result.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured maintenance role is required.");if(!disposed && generation==version && snapshot==company.Snapshot)Review=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public Task RejectAsync(ICompanyMaintenanceHandoffClient client,string rationale,Func<Guid,Task> retainBeforeTransport)=>DecideAsync(client,"Reject",rationale,retainBeforeTransport);
    public Task ApproveAsync(ICompanyMaintenanceHandoffClient client,string rationale,Func<Guid,Task> retainBeforeTransport)=>DecideAsync(client,"Approve",rationale,retainBeforeTransport);
    private async Task DecideAsync(ICompanyMaintenanceHandoffClient client,string outcome,string rationale,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(outcome=="Approve"?!CanApprove:!CanReject)throw new InvalidOperationException("Review the original complete prerequisites once; retained uncertainty requires original read-only recovery.");
        var selected=RequireCurrent();Review!.Require(selected.Scope,selected.UserId,proposal!);var original=new CompanyWorkOrderExecutionPermitDecision(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),outcome,rationale);original.Require();decision=original;reference=original.IntentId;origin=proposal;originalCompany=selected;Review=null;Receipt=null;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(original.IntentId);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("The original independent decision context changed before transport.");var result=await client.DecideWorkOrderExecutionPermitAsync(CompanyInventorySourceReview.Copy(original));result.Require(original);if(!disposed && generation==version && snapshot==company.Snapshot){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} retained && retained!=id)throw new InvalidOperationException("Use the same original independent decision reference.");var selected=RequireCurrent();var query=proposal!;if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original independent reviewer/company/permit task before recovery.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try{if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("The decision context changed before recovery.");var referenceQuery=new CompanyWorkOrderExecutionPermitDecisionReference(query,id);var result=await client.RecoverWorkOrderExecutionPermitDecisionReferenceAsync(referenceQuery);result.Require(selected.Scope,selected.UserId,referenceQuery);if(decision is not null && JsonSerializer.Serialize(decision)!=JsonSerializer.Serialize(result.Decision))throw new InvalidOperationException("The recorded decision differs from the retained original body.");if(!disposed && generation==version && snapshot==company.Snapshot){decision=CompanyInventorySourceReview.Copy(result.Decision);Receipt=CompanyInventorySourceReview.Copy(result.Receipt);confirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
