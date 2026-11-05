using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Retains the original physical-start intent before transport; uncertainty never creates a replacement.</summary>
public sealed class WorkOrderExecutionStartState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderExecutionPermitReference? permit,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderExecutionStartCommand? command;
    private Guid? reference;
    private long version;
    private bool confirmed,disposed;
    private bool subscribed;
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyWorkOrderExecutorIdentity? Review {get;private set;}
    public CompanyWorkOrderExecutionStartReceipt? Receipt {get;private set;}
    private bool SameOrigin=>permit==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId;
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderExecutionStartCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public bool CanReview=>!Busy && reference is null;
    public bool QualifiedWorkScopeMissing=>Review is {} review && CompanyMaintenanceQualifiedWorkScope.HasTargets(review.Prerequisites.Requirements.Order) && review.QualifiedWorkAssets is null;
    public bool CanStart=>CanReview && Review is not null && !QualifiedWorkScopeMissing && company.Selection?.Roles.Contains(Review.Stage.AssignedRole,StringComparer.Ordinal)==true;
    public void Attach(CompanyWorkOrderExecutionPermitReference? next)
    {
        if(!subscribed){company.Changed+=Invalidate;subscribed=true;}next?.Require();if(permit==next)return;Invalidate();permit=next;
    }
    private void Invalidate(){version++;permit=null;Review=null;Receipt=null;if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();}
    private CompanyClientSelection RequireCurrent(bool inventory)
    {
        if(disposed || permit is null || company.Selection is not {} selected)throw new InvalidOperationException("Return to the original executor/company/permit task.");
        if(!selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal) || inventory && !selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current work-start capabilities are required.");return selected;
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("Recover the retained original work start before another review.");var selected=RequireCurrent(true);var query=permit!;var generation=version;var snapshot=company.Snapshot;Review=null;Receipt=null;Busy=true;Changed?.Invoke();
        try{var result=await client.ReviewWorkOrderExecutionStartAsync(query);result.Require();result.Prerequisites.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured executing role is required.");if(!disposed && generation==version && snapshot==company.Snapshot)Review=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task StartAsync(ICompanyMaintenanceHandoffClient client,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanStart)throw new InvalidOperationException("Review the actual assigned executor once; uncertainty requires original read-only recovery.");
        var selected=RequireCurrent(true);Review!.Require();Review.Prerequisites.Require(selected.Scope,selected.UserId,permit!);
        var original=new CompanyWorkOrderExecutionStartCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),DateTime.UtcNow,reason);original.RequireQualifiedWorkCapture();
        command=original;reference=original.IntentId;origin=permit;originalCompany=selected;Review=null;Receipt=null;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("The original work-start context changed before transport.");
            var result=await client.StartWorkOrderExecutionAsync(CompanyInventorySourceReview.Copy(original));result.Require(original);
            if(!disposed && generation==version && snapshot==company.Snapshot){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the same original work-start intent reference.");var selected=RequireCurrent(false);var query=permit!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original executing company/account/task.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("The work-start context changed before recovery.");
            var referenceQuery=new CompanyWorkOrderExecutionStartReference(query,id);var result=await client.RecoverWorkOrderExecutionStartAsync(referenceQuery);result.Require(selected.Scope,selected.UserId,referenceQuery);
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recorded work start differs from the retained original command.");
            if(!disposed && generation==version && snapshot==company.Snapshot){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=new(selected.Scope,selected.UserId,id,result.Recorded.START_ID,result.Recorded.PERMIT_ID,result.Recorded.STARTED_AT_UTC,result.Recorded.RECORDED_REVISION);Receipt.Require(command);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;if(subscribed)company.Changed-=Invalidate;Invalidate();}
}
