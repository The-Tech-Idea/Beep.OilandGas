using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned original maintenance workflow closure intent; uncertainty survives context changes without exposing another actor's work.</summary>
public sealed class MaintenanceClosureState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderCompletionQuery? start,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyMaintenanceClosureCommand? command;
    private Guid? reference;
    private int version;
    private bool subscribed,disposed,confirmed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyMaintenanceClosureReview? Review{get;private set;}
    public CompanyMaintenanceClosureRecovery? Receipt{get;private set;}
    public CompanyMaintenanceClosureStatus? Status{get;private set;}
    private bool SameOrigin=>start==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId &&
        current.Roles.Count>0 && current.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal) && current.Permissions.Contains("Workflow.Execute",StringComparer.Ordinal) && (command is null || current.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal));
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyMaintenanceClosureCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public bool CanReview=>!Busy && (reference is null || confirmed);
    public bool CanRecord=>CanReview && Review is not null && company.Selection?.Roles.Contains(Review.Stage.AssignedRole,StringComparer.Ordinal)==true;
    public void Attach(CompanyWorkOrderCompletionQuery? next)
    {
        if(!subscribed){company.Changed+=Invalidate;subscribed=true;}next?.Require();if(start==next)return;Invalidate();start=next;
    }
    private void Invalidate()
    {
        version++;start=null;Review=null;Receipt=null;Status=null;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();
    }
    private CompanyClientSelection RequireCurrent(bool approve=true)
    {
        if(disposed || start is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal) ||
            approve && !selected.Permissions.Contains("Workflow.Execute",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original recorded work and current standard reviewer roles and return/execution capabilities are required.");
        return selected;
    }
    public async Task ReadStatusAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy)throw new InvalidOperationException("Wait for the current closure operation.");var selected=RequireCurrent(false);var query=start!;var snapshot=company.Snapshot;var generation=version;Busy=true;Status=null;Changed?.Invoke();
        try{var result=await client.ReadMaintenanceClosureStatusAsync(query)??throw new InvalidOperationException("Recorded maintenance closure status is unavailable; an error cannot establish absence.");result.Require();if(result.Query!=query || result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original public closure status company/account/report differs.");if(!disposed && snapshot==company.Snapshot && generation==version)Status=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("An uncertain maintenance workflow closure requires original recovery before another review.");
        var selected=RequireCurrent();var query=start!;var snapshot=company.Snapshot;var generation=version;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}
        Busy=true;Review=null;Receipt=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewMaintenanceClosureAsync(query);result.Require();
            if(result.Stage.Context.Company!=selected.Scope || result.Stage.Context.ActorUserId!=selected.UserId || result.Current.Completion.Report.Query!=query ||
                !selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original reviewer company/account/report/configured role is required.");
            if(!disposed && snapshot==company.Snapshot && generation==version)Review=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecordAsync(ICompanyMaintenanceHandoffClient client,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanRecord)throw new InvalidOperationException("Original recorded work and current configured reviewer/maintenance workflow closure are required; recover uncertain outcomes before another submission.");
        var selected=RequireCurrent();Review!.Require();
        if(selected.Roles.Count==0)throw new UnauthorizedAccessException("The original standard reviewer role is required.");
        var original=new CompanyMaintenanceClosureCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),reason,true);original.Require();
        command=original;reference=original.IntentId;origin=start;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original maintenance closure context changed before transport.");
            var result=await client.CloseMaintenanceAsync(CompanyInventorySourceReview.Copy(original));
            result.Require(selected.Scope,selected.UserId,new(origin!,original.IntentId));
            if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Recorded maintenance closure differs from the retained original body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the retained original maintenance workflow closure reference.");
        var selected=RequireCurrent();var query=start!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original maintenance closure company, account and recorded report.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);
            if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original maintenance closure recovery context changed.");
            var queryReference=new CompanyMaintenanceClosureReference(query,id);var result=await client.RecoverMaintenanceClosureAsync(queryReference);
            result.Require(selected.Scope,selected.UserId,queryReference);
            if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The original configured reviewer role is required for recovery.");
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recovered maintenance closure differs from the original retained body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Invalidate;Changed=null;}
}
