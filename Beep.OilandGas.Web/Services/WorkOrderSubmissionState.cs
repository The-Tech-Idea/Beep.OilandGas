using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Retains an own-executor original work report; uncertainty permits only its exact recovery.</summary>
public sealed class WorkOrderSubmissionState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderExecutionStartReference? start,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderSubmissionCommand? command;
    private Guid? reference;
    private long version;
    private bool confirmed,disposed,subscribed;
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyWorkOrderSubmissionReview? Review {get;private set;}
    public CompanyWorkOrderSubmissionReceipt? Receipt {get;private set;}
    private bool SameOrigin=>start==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId;
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderSubmissionCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public bool CanReview=>!Busy && reference is null;
    public bool CanSubmit=>CanReview && Review is not null && Review.Inspections.Blockers.Count==0 && company.Selection?.Roles.Contains(Review.Stage.AssignedRole,StringComparer.Ordinal)==true;
    public void Attach(CompanyWorkOrderExecutionStartReference? next)
    {
        if(!subscribed){company.Changed+=Invalidate;subscribed=true;}next?.Require();if(start==next)return;Invalidate();start=next;
    }
    private void Invalidate(){version++;start=null;Review=null;Receipt=null;if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();}
    private CompanyClientSelection RequireCurrent()
    {if(disposed || start is null || company.Selection is not {} selected || !selected.Permissions.Contains("Maintenance.ExecuteWork",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original start and current executing company/account/capability are required.");return selected;}
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("An uncertain report requires original recovery, not another submission review.");var selected=RequireCurrent();var query=start!;var snapshot=company.Snapshot;var generation=version;Busy=true;Review=null;Receipt=null;Changed?.Invoke();
        try{var result=await client.ReviewWorkOrderSubmissionAsync(query);result.Require();result.Inspections.Require(selected.Scope,selected.UserId,query);if(!selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The configured original execution role is required.");if(!disposed && snapshot==company.Snapshot && generation==version)Review=CompanyInventorySourceReview.Copy(result);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task SubmitAsync(ICompanyMaintenanceHandoffClient client,DateTime completedUtc,string work,IReadOnlyList<string> defects,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanSubmit)throw new InvalidOperationException("Accepted original submission inspection review is required; retained uncertainty cannot create replacement work.");
        var selected=RequireCurrent();Review!.Require();Review.Inspections.Require(selected.Scope,selected.UserId,start!);
        var original=new CompanyWorkOrderSubmissionCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),completedUtc,work,defects.ToArray(),reason);original.Require();
        command=original;reference=original.IntentId;origin=start;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original report context changed before transport.");
            var result=await client.SubmitWorkOrderAsync(CompanyInventorySourceReview.Copy(original));result.Require(original);
            if(!disposed && snapshot==company.Snapshot && generation==version){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the same original work-submission reference.");var selected=RequireCurrent();var query=start!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original submitting company/account/start.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original submission context changed before recovery.");
            var referenceQuery=new CompanyWorkOrderSubmissionReference(query,id);var result=await client.RecoverWorkOrderSubmissionAsync(referenceQuery);result.Require(selected.Scope,selected.UserId,referenceQuery);
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recorded report differs from the retained original command.");
            if(!disposed && snapshot==company.Snapshot && generation==version){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=new(selected.Scope,selected.UserId,id,result.Recorded.SUBMISSION_ID,result.Recorded.START_ID,result.Recorded.COMPLETED_AT_UTC,result.Recorded.RECORDED_REVISION);Receipt.Require(command);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;if(subscribed)company.Changed-=Invalidate;Invalidate();}
}
