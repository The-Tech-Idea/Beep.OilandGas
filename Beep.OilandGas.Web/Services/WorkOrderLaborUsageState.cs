using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned original labor report; uncertainty survives context changes without exposing another actor's work.</summary>
public sealed class WorkOrderLaborUsageState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderExecutionStartReference? start,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderLaborUsageCommand? command;
    private Guid? reference;
    private int version;
    private bool subscribed,disposed,confirmed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyWorkOrderLaborUsageReview? Review{get;private set;}
    public CompanyWorkOrderLaborUsageRecovery? Receipt{get;private set;}
    private bool SameOrigin=>start==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId &&
        current.Roles.Count>0 && current.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) &&
        (command is null || current.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal));
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderLaborUsageCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public bool CanReview=>!Busy && (reference is null || confirmed);
    public bool CanRecord=>CanReview && Review is not null;
    public void Attach(CompanyWorkOrderExecutionStartReference? next)
    {
        if(!subscribed){company.Changed+=Invalidate;subscribed=true;}next?.Require();if(start==next)return;Invalidate();start=next;
    }
    private void Invalidate()
    {
        version++;start=null;Review=null;Receipt=null;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();
    }
    private CompanyClientSelection RequireCurrent()
    {
        if(disposed || start is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Original start and current execution authority are required.");
        return selected;
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("An uncertain labor issue requires original recovery before another review.");
        var selected=RequireCurrent();var query=start!;var snapshot=company.Snapshot;var generation=version;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}
        Busy=true;Review=null;Receipt=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewWorkOrderLaborUsageAsync(query);result.Require();
            if(result.Execution.Start.Company!=selected.Scope || result.Execution.Start.ExecutorUserId!=selected.UserId || result.Execution.Start.Permit!=query.Permit || result.Execution.Start.IntentId!=query.IntentId ||
                !selected.Roles.Contains(result.Execution.Start.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original executing company/account/start/role is required.");
            if(!disposed && snapshot==company.Snapshot && generation==version)Review=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecordAsync(ICompanyMaintenanceHandoffClient client,DateTime fromUtc,DateTime toUtc,decimal quantity,WorkOrderLaborTimeUnit unit,string workPerformed,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanRecord)throw new InvalidOperationException("Current original start and duty evidence is required; uncertain issues cannot be replaced.");
        var selected=RequireCurrent();Review!.Require();
        if(!selected.Roles.Contains(Review.Execution.Start.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The original configured execution role is required.");
        var original=new CompanyWorkOrderLaborUsageCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),fromUtc,toUtc,quantity,unit,workPerformed,reason);original.Require();
        command=original;reference=original.IntentId;origin=start;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original labor use context changed before transport.");
            var result=await client.RecordWorkOrderLaborUsageAsync(CompanyInventorySourceReview.Copy(original));
            result.Require(selected.Scope,selected.UserId,new(origin!,original.IntentId));
            if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Recorded stock use differs from the retained original body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the retained original labor issue reference.");
        var selected=RequireCurrent();var query=start!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original labor use company/account/start.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);
            if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original labor recovery context changed.");
            var queryReference=new CompanyWorkOrderLaborUsageReference(query,id);var result=await client.RecoverWorkOrderLaborUsageAsync(queryReference);
            result.Require(selected.Scope,selected.UserId,queryReference);
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recovered labor usage differs from the original retained body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Invalidate;Changed=null;}
}
