using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Retains the original native return intent through uncertainty and hides it outside its original company/account/report.</summary>
public sealed class MaintenanceAssetReturnState(CompanyClientContext company):IDisposable
{
    private CompanyWorkOrderCompletionQuery? query,origin;
    private CompanyClientSelection? originalCompany;
    private CompanyMaintenanceAssetReturnCommand? command;
    private Guid? reference;
    private int version;
    private bool subscribed,disposed,confirmed;
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanyMaintenanceAssetReturnReview? Review{get;private set;}
    public CompanyMaintenanceAssetReturnRecovery? Receipt{get;private set;}
    public IReadOnlyList<string> RequiredCapabilities=>Review is {} review?CompanyMaintenanceAssetReturnCapabilities.ForOrder(review.Assets.Order):command is {} retained?CompanyMaintenanceAssetReturnCapabilities.ForOrder(retained.Review.Assets.Order):CompanyMaintenanceAssetReturnCapabilities.Common;
    public bool HasCurrentCapabilities=>company.Selection is {} selected && selected.Roles.Count>0 && RequiredCapabilities.All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    private static bool HasCapabilities(CompanyClientSelection selected,CompanyMaintenanceAssetEvidence? assets=null)=>selected.Roles.Count>0 && (assets is null?CompanyMaintenanceAssetReturnCapabilities.Common:CompanyMaintenanceAssetReturnCapabilities.ForOrder(assets.Order)).All(permission=>selected.Permissions.Contains(permission,StringComparer.Ordinal));
    private bool SameOrigin=>query==origin && originalCompany is {} original && company.Selection is {} current && original.Scope==current.Scope && original.UserId==current.UserId && HasCurrentCapabilities && (command is null || current.Roles.Contains(command.Review.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal));
    public Guid? VisibleReference=>SameOrigin?reference:null;
    public bool NeedsRecovery=>reference is not null && !confirmed;
    public bool TryRetainReference(Guid id)
    {
        if(disposed || id==Guid.Empty || query is null || company.Selection is not {} selected)return false;
        if(reference is {} retained)return retained==id && origin==query && originalCompany is {} original && original.Scope==selected.Scope && original.UserId==selected.UserId;
        reference=id;origin=query;originalCompany=selected;command=null;Review=null;Receipt=null;confirmed=false;version++;Changed?.Invoke();return true;
    }
    public bool CanReview=>!disposed && !Busy && company.Selection is {} selected && HasCapabilities(selected) && (reference is null || confirmed);
    public bool CanRecord=>CanReview && Review is {} review && company.Selection is {} selected && HasCapabilities(selected,review.Assets) && selected.Roles.Contains(review.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal);
    public void Attach(CompanyWorkOrderCompletionQuery? next)
    {if(!subscribed){subscribed=true;company.Changed+=Clear;}next?.Require();if(query==next)return;Clear();query=next;}
    public void Clear()
    {
        version++;query=null;Review=null;Receipt=null;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Changed?.Invoke();
    }
    private CompanyClientSelection RequireCurrent()
    {if(disposed || query is null || company.Selection is not {} selected || !HasCurrentCapabilities)throw new UnauthorizedAccessException("Original closed report and current company capabilities for every affected return target are required.");return selected;}
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(!CanReview)throw new InvalidOperationException("Recover the retained original asset return before another review.");var selected=RequireCurrent();var original=query!;var snapshot=company.Snapshot;var generation=version;
        if(confirmed){command=null;reference=null;origin=null;originalCompany=null;confirmed=false;}Busy=true;Review=null;Receipt=null;Changed?.Invoke();
        try
        {
            var result=await client.ReviewMaintenanceAssetReturnAsync(original);result.Require();
            if(result.Assets.Context.Company!=selected.Scope || result.Assets.Context.ActorUserId!=selected.UserId || result.Assets.Report.Query!=original || !HasCapabilities(selected,result.Assets) || !selected.Roles.Contains(result.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original company/account/report/configured return role or target capabilities differ.");
            if(!disposed && snapshot==company.Snapshot && generation==version)Review=CompanyInventorySourceReview.Copy(result);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public Task RecordAsync(ICompanyMaintenanceHandoffClient client,DateTime activatedAtUtc,IReadOnlyList<CompanyWellReturnActivation> wells,string reason,bool confirmation,Func<Guid,Task> retainBeforeTransport)=>
        RecordAsync(client,activatedAtUtc,wells,reason,confirmation,retainBeforeTransport,null);
    public async Task RecordAsync(ICompanyMaintenanceHandoffClient client,DateTime activatedAtUtc,IReadOnlyList<CompanyWellReturnActivation> wells,string reason,bool confirmation,Func<Guid,Task> retainBeforeTransport,IReadOnlyList<CompanyMaintenanceQualifiedReturnObservation>? qualifiedAssets)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanRecord || Review is null)throw new InvalidOperationException("Review current original asset-return sources before recording.");var selected=RequireCurrent();
        var original=new CompanyMaintenanceAssetReturnCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),activatedAtUtc,CompanyInventorySourceReview.Copy(wells.ToArray()),reason,confirmation)
            {QualifiedAssets=qualifiedAssets is null?null:CompanyInventorySourceReview.Copy(qualifiedAssets.ToArray())};original.Require();
        command=original;reference=original.IntentId;origin=query;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(original.IntentId);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original asset-return context changed before transport.");
            var result=await client.RecordMaintenanceAssetReturnAsync(CompanyInventorySourceReview.Copy(original));result.Require(selected.Scope,selected.UserId,original.Reference());
            if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(original))throw new InvalidOperationException("Recorded asset return differs from the retained original body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainBeforeTransport=null)
    {
        if(disposed || Busy || id==Guid.Empty || reference is {} saved && saved!=id)throw new InvalidOperationException("Use the retained original asset-return reference.");var selected=RequireCurrent();var original=query!;
        if(origin is not null && !SameOrigin)throw new InvalidOperationException("Return to the original asset-return company/account/report before recovery.");
        reference=id;origin=original;originalCompany=selected;Review=null;Receipt=null;var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(id);if(disposed || snapshot!=company.Snapshot || generation!=version)throw new OperationCanceledException("Original asset-return context changed before recovery transport.");
            var requested=new CompanyMaintenanceAssetReturnReference(original,id);var result=await client.RecoverMaintenanceAssetReturnAsync(requested);result.Require(selected.Scope,selected.UserId,requested);
            if(!HasCapabilities(selected,result.Command.Review.Assets) || !selected.Roles.Contains(result.Command.Review.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured return role and target capabilities are required for recovery.");
            if(command is not null && JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recovered return differs from its original retained body.");
            if(!disposed && snapshot==company.Snapshot && generation==version){command=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result);confirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;version++;if(subscribed)company.Changed-=Clear;Changed=null;}
}
