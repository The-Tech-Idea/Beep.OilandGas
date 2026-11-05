using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned original budget intent; uncertainty survives context changes without exposing another actor's work.</summary>
public sealed class WorkOrderBudgetState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderBudgetQuery? start,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyWorkOrderBudgetCommand? command;
    private Guid? reference;
    private int version;
    private bool subscribed,disposed,confirmed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyWorkOrderBudgetReview? Review{get;private set;}
    public CompanyWorkOrderBudgetRecovery? Receipt{get;private set;}
    private bool SameOrigin=>start==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId &&
        current.Roles.Count>0 && current.Permissions.Contains("Accounting.PrepareWorkOrderBudget",StringComparer.Ordinal) && current.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public CompanyWorkOrderBudgetCommand? VisibleCommand=>SameOrigin && command is not null?CompanyInventorySourceReview.Copy(command):null;
    public bool CanReview=>!Busy && (reference is null || confirmed);
    public bool CanRecord=>CanReview && Review is not null && Review.Budgets.Count==0;
    public void Attach(CompanyWorkOrderBudgetQuery? next)
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
        if(disposed || start is null || company.Selection is not {} selected || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.PrepareWorkOrderBudget",StringComparer.Ordinal) ||
            !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original order/book and current standard financial roles and budget capability are required.");
        return selected;
    }
    public async Task ReviewAsync(IWorkOrderServiceClient client)
    {
        if(!CanReview)throw new InvalidOperationException("An uncertain budget issue requires original recovery before another review.");
        var selected=RequireCurrent();var query=start!;var snapshot=company.Snapshot;var generation=version;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}
        Busy=true;Review=null;Receipt=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewBudgetAsync(query);result.Require();
            if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query ||
                selected.Roles.Count==0)throw new UnauthorizedAccessException("Original financial company/account/order/book/role is required.");
            if(!disposed && snapshot==company.Snapshot && generation==version)Review=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task CreateAsync(IWorkOrderServiceClient client,string number,string name,decimal amount,DateTime effectiveUtc,string reason,Func<Guid,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanRecord)throw new InvalidOperationException("Current native order/book and an empty budget population are required; uncertain issues cannot be replaced.");
        var selected=RequireCurrent();Review!.Require();
        if(selected.Roles.Count==0)throw new UnauthorizedAccessException("The original standard financial role is required.");
        var original=new CompanyWorkOrderBudgetCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),number,name,amount,effectiveUtc,reason);original.Require();
        command=original;reference=original.IntentId;origin=start;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original budget context changed before transport.");
            var result=await client.CreateBudgetAsync(CompanyInventorySourceReview.Copy(original));
            result.Require(selected.Scope,selected.UserId,new(origin!,original.IntentId));
            if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Recorded budget differs from the retained original body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(IWorkOrderServiceClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the retained original budget issue reference.");
        var selected=RequireCurrent();var query=start!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original budget company/account/order/book.");
        reference=id;origin=query;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);
            if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original budget recovery context changed.");
            var queryReference=new CompanyWorkOrderBudgetReference(query,id);var result=await client.RecoverBudgetAsync(queryReference);
            result.Require(selected.Scope,selected.UserId,queryReference);
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recovered budget usage differs from the original retained body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Invalidate;Changed=null;}
}
