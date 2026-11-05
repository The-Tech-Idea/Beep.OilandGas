using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned original completion decision intent; uncertainty survives context changes without exposing another actor's work.</summary>
public sealed class WorkOrderCompletionDecisionState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderCompletionQuery? start,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderCompletionDecisionCommand? command;
    private Guid? reference;
    private int version;
    private bool subscribed,disposed,confirmed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyWorkOrderCompletionDecisionReview? Review{get;private set;}
    public CompanyWorkOrderCompletionDecisionRecovery? Receipt{get;private set;}
    private bool SameOrigin=>start==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId &&
        current.Roles.Count>0 && current.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal) && current.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal) && (command is null || current.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal));
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderCompletionDecisionCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public bool CanReview=>!Busy && (reference is null || confirmed);
    public bool CanRecord=>CanReview && Review is not null && company.Selection?.Roles.Contains(Review.Stage.AssignedRole,StringComparer.Ordinal)==true;
    public void Attach(CompanyWorkOrderCompletionQuery? next)
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
        if(disposed || start is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal) ||
            !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original recorded work and current standard reviewer roles and completion/approval capabilities are required.");
        return selected;
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("An uncertain completion decision requires original recovery before another review.");
        var selected=RequireCurrent();var query=start!;var snapshot=company.Snapshot;var generation=version;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}
        Busy=true;Review=null;Receipt=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewCompletionDecisionAsync(query);result.Require();
            if(result.Stage.Context.Company!=selected.Scope || result.Stage.Context.ActorUserId!=selected.UserId || result.Completion.Report.Query!=query ||
                !selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original reviewer company/account/report/configured role is required.");
            if(!disposed && snapshot==company.Snapshot && generation==version)Review=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecordAsync(ICompanyMaintenanceHandoffClient client,string decision,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanRecord)throw new InvalidOperationException("Original recorded work and current configured independent reviewer assignment are required; recover uncertain outcomes before another decision.");
        var selected=RequireCurrent();Review!.Require();
        if(selected.Roles.Count==0)throw new UnauthorizedAccessException("The original standard reviewer role is required.");
        var original=new CompanyWorkOrderCompletionDecisionCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),decision,reason);original.Require();
        command=original;reference=original.IntentId;origin=start;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original completion decision context changed before transport.");
            var result=await client.RecordCompletionDecisionAsync(CompanyInventorySourceReview.Copy(original));
            result.Require(selected.Scope,selected.UserId,new(origin!,original.IntentId));
            if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Recorded completion decision differs from the retained original body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the retained original completion decision reference.");
        var selected=RequireCurrent();var query=start!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original handoff company, account and recorded report.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);
            if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original completion decision recovery context changed.");
            var queryReference=new CompanyWorkOrderCompletionDecisionReference(query,id);var result=await client.RecoverCompletionDecisionAsync(queryReference);
            result.Require(selected.Scope,selected.UserId,queryReference);
            if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The original configured reviewer role is required for recovery.");
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recovered handoff differs from the original retained body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Invalidate;Changed=null;}
}
