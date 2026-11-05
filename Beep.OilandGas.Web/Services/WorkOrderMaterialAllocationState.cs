using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
/// <summary>Circuit-owned original reservation; unknown outcomes block replacement.</summary>
public sealed class WorkOrderMaterialAllocationState : IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyWorkOrderMaterialAllocationCommand? command;
    private CompanyMaintenanceReceivingQuery? task,origin;
    private CompanyClientSelection? owner;
    private Guid? releaseReference;
    private CompanyWorkOrderMaterialReleaseCommand? releaseCommand;
    private Guid? reference;
    private long version;
    private bool disposed;
    public bool Busy {get;private set;}
    public CompanyWorkOrderMaterialAllocationReview? Review {get;private set;}
    public WORK_ORDER_MATERIAL_ALLOCATION? Receipt {get;private set;}
    public event Action? Changed;
    private bool SameOrigin=>task==origin && company.Selection is {} current && owner is {} original && current.Scope==original.Scope && current.UserId==original.UserId;
    public CompanyWorkOrderMaterialAllocationCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public WORK_ORDER_MATERIAL_RELEASE? ReleaseReceipt {get;private set;}
    public Guid? VisibleReleaseReference=>SameOrigin?releaseReference:null;
    public bool CanRelease=>!Busy && releaseReference is null && Receipt is not null && VisibleCommand is not null;
    public bool CanReserve=>!Busy && reference is null && Review is not null;
    public WorkOrderMaterialAllocationState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public void Attach(CompanyMaintenanceReceivingQuery? value){value?.Require();if(value==task)return;Invalidate();task=value;}
    private void Invalidate(){version++;task=null;Review=null;Receipt=null;ReleaseReceipt=null;Changed?.Invoke();}
    private CompanyClientSelection RequireCurrent()
    {
        if(disposed || task is null || company.Selection is not {} selected)throw new InvalidOperationException("Reload the original company/work-order task.");
        if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current inventory view and reservation permissions are required.");return selected;
    }
    public void DiscardSourceReview()
    {
        if(Busy || reference is not null)throw new InvalidOperationException("Recover the original reservation before replacing its reviewed requirement.");
        version++;Review=null;Changed?.Invoke();
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderMaterialSourceQuery query)
    {
        if(Busy || reference is not null)throw new InvalidOperationException("Recover the original reservation before replacing its source.");var selected=RequireCurrent();if(query.Task!=task)throw new InvalidOperationException("Use the original assigned task.");var scope=company.Snapshot;var generation=version;Review=null;Busy=true;Changed?.Invoke();
        try{var result=await client.ReviewWorkOrderMaterialAllocationAsync(query);result.Require();result.Source.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Source.WorkOrder.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current assigned role is required.");if(!disposed && generation==version && scope==company.Snapshot)Review=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReserveAsync(ICompanyMaintenanceHandoffClient client,DateTime from,DateTime to,string reason,Func<CompanyWorkOrderMaterialAllocationCommand,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanReserve)throw new InvalidOperationException("Review once; an original uncertain reservation blocks another command.");var selected=RequireCurrent();var original=new CompanyWorkOrderMaterialAllocationCommand(CompanyInventorySourceReview.Copy(Review!),Guid.NewGuid(),from,to,reason);original.Require();
        command=original;reference=original.IntentId;origin=task;owner=selected;Review=null;Receipt=null;var generation=version;var scope=company.Snapshot;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(CompanyInventorySourceReview.Copy(original));if(disposed || generation!=version || scope!=company.Snapshot)throw new OperationCanceledException("Original reservation context changed before transport.");var row=await client.RecordWorkOrderMaterialAllocationAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(row);if(!disposed && generation==version && scope==company.Snapshot)Receipt=CompanyInventorySourceReview.Copy(row);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || command is null || !SameOrigin)throw new InvalidOperationException("Return to the original reservation company/account/task for read-only recovery.");RequireCurrent();var original=CompanyInventorySourceReview.Copy(command);var scope=company.Snapshot;var generation=version;Receipt=null;Busy=true;Changed?.Invoke();
        try{var row=await client.RecoverWorkOrderMaterialAllocationAsync(original);original.RequireRecorded(row);if(!disposed && generation==version && scope==company.Snapshot)Receipt=CompanyInventorySourceReview.Copy(row);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverReferenceAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} retained && retained!=id)throw new InvalidOperationException("Use the same original reservation reference.");var selected=RequireCurrent();if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original reservation account/task.");
        reference=id;origin=task;owner=selected;Review=null;Receipt=null;var generation=version;var scope=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || generation!=version || scope!=company.Snapshot)throw new OperationCanceledException("Original recovery context changed before transport.");
            var query=new CompanyWorkOrderMaterialAllocationReferenceQuery(task!,id);var result=await client.RecoverWorkOrderMaterialAllocationReferenceAsync(query);result.Require(selected.Scope,selected.UserId,query);
            if(command is not null && System.Text.Json.JsonSerializer.Serialize(command)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained reservation body changed during reference recovery.");
            if(!disposed && generation==version && scope==company.Snapshot){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result.Recorded);}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReleaseAsync(ICompanyMaintenanceHandoffClient client,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        if(!CanRelease)throw new InvalidOperationException("Recover the original reservation before one release; reconcile any original uncertain release.");var selected=RequireCurrent();if(!selected.Permissions.Contains("Inventory.ReleaseWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current release permission is required.");ArgumentNullException.ThrowIfNull(retainBeforeTransport);
        var original=new CompanyWorkOrderMaterialReleaseCommand(new(CompanyInventorySourceReview.Copy(command!),CompanyInventorySourceReview.Copy(Receipt!)),Guid.NewGuid(),reason);original.Require();releaseCommand=original;releaseReference=original.IntentId;ReleaseReceipt=null;var generation=version;var scope=company.Snapshot;Busy=true;Changed?.Invoke();
        try{await retainBeforeTransport(original.IntentId);if(disposed || generation!=version || scope!=company.Snapshot)throw new OperationCanceledException("Original release scope changed before transport.");var row=await client.ReleaseWorkOrderMaterialAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(row);if(!disposed && generation==version && scope==company.Snapshot)ReleaseReceipt=CompanyInventorySourceReview.Copy(row);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverReleaseReferenceAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is null || !SameOrigin || releaseReference is {} retained && retained!=id)throw new InvalidOperationException("Return to the same original reservation/release references.");var selected=RequireCurrent();if(!selected.Permissions.Contains("Inventory.ReleaseWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current release permission is required.");releaseReference=id;ReleaseReceipt=null;var generation=version;var scope=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || generation!=version || scope!=company.Snapshot)throw new OperationCanceledException("Original release context changed before recovery.");var query=new CompanyWorkOrderMaterialReleaseReferenceQuery(new(task!,reference.Value),id);var result=await client.RecoverWorkOrderMaterialReleaseReferenceAsync(query);result.Require(selected.Scope,selected.UserId,query);
            if(releaseCommand is not null && System.Text.Json.JsonSerializer.Serialize(releaseCommand)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Retained original release body changed.");if(!disposed && generation==version && scope==company.Snapshot){releaseCommand=CompanyInventorySourceReview.Copy(result.Command);ReleaseReceipt=CompanyInventorySourceReview.Copy(result.Recorded);}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void BeginNewRequirement()
    {
        if(Busy || !SameOrigin || ReleaseReceipt is null || releaseCommand is null)throw new InvalidOperationException("Confirm the original full release before reviewing a replacement requirement.");RequireCurrent();releaseCommand.RequireRecorded(ReleaseReceipt);version++;command=null;reference=null;releaseCommand=null;releaseReference=null;Receipt=null;ReleaseReceipt=null;Review=null;origin=null;owner=null;Changed?.Invoke();
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;command=null;owner=null;origin=null;Invalidate();}
}