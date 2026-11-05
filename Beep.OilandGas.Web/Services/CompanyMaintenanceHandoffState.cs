using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Retains the original circuit intent before transport; recovery never resubmits it.</summary>
public sealed class CompanyMaintenanceHandoffState : IDisposable
{
    private readonly CompanyClientContext company;
    private ProductionInterventionMaintenanceLink? source;
    private long version;
    private bool disposed,confirmed;
    private Guid? reference;
    private ProductionInterventionMaintenanceLink? referenceSource;
    public event Action? Changed;
    public bool Busy { get; private set; }
    public CompanyMaintenanceProcessStartReview? Review { get; private set; }
    public CompanyMaintenanceProcessStart? Intent { get; private set; }
    public CompanyMaintenanceProcessStartReceipt? Receipt { get; private set; }
    public CompanyMaintenanceStartCatalog? Catalog { get; private set; }
    public Guid? Reference=>Intent?.IntentId??reference;
    public Guid? VisibleReference=>source is not null && referenceSource is not null && source with{RetrievedAtUtc=referenceSource.RetrievedAtUtc}==referenceSource?Reference:null;
    public CompanyMaintenanceHandoffState(CompanyClientContext company)
    {
        this.company=company;company.Changed+=Invalidate;
    }
    public void AttachSource(ProductionInterventionMaintenanceLink? next)
    {
        if(source is not null && next is not null && source with {RetrievedAtUtc=next.RetrievedAtUtc}==next)return;
        Invalidate();source=next is null?null:CompanyInventorySourceReview.Copy(next);
    }
    private void Invalidate()
    {
        version++;source=null;Review=null;Receipt=null;Catalog=null;
        if(confirmed){Intent=null;reference=null;referenceSource=null;confirmed=false;}
        Changed?.Invoke();
    }
    private void RequireCurrent(ProductionInterventionMaintenanceLink current)
    {
        var selected=company.Snapshot.Selection;
        if(disposed || selected is null)throw new InvalidOperationException("Select the original maintenance company/account.");
        current.Require(selected.Scope,selected.UserId,current.FieldId,current.WorkOrderId,current.WellId,current.DecisionId);
    }
    public async Task LoadChoicesAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || Reference is not null)throw new InvalidOperationException("Reconcile the original assignment reference before reviewing another handoff.");
        var original=source??throw new InvalidOperationException("Reload the original intervention source.");RequireCurrent(original);
        var snapshot=company.Snapshot;var generation=version;Catalog=null;Review=null;Busy=true;Changed?.Invoke();
        try
        {
            var query=new CompanyMaintenanceSourceQuery(original.DecisionId,original.WellId,original.WorkOrderId);
            var catalog=await client.GetMaintenanceHandoffChoicesAsync(query);catalog.Require(original.Company,original.Actor,query);
            if(catalog.Source with{RetrievedAtUtc=original.RetrievedAtUtc}!=original)throw new InvalidOperationException("The original intervention source changed during workflow discovery.");
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;
            Catalog=CompanyInventorySourceReview.Copy(catalog);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client,string processId)
    {
        if(Busy || Reference is not null || Catalog is null)throw new InvalidOperationException("Load recorded workflow choices or reconcile the retained original assignment.");
        var choice=Catalog.Choices.SingleOrDefault(value=>value.ProcessId==processId)??throw new InvalidOperationException("Select a recorded receiving workflow from the current choices.");
        var original=source??throw new InvalidOperationException("Reload the original intervention source.");RequireCurrent(original);
        var snapshot=company.Snapshot;var generation=version;Review=null;Receipt=null;Busy=true;Changed?.Invoke();
        try
        {
            var review=await client.ReviewMaintenanceHandoffAsync(new(processId,original.DecisionId,original.WellId,original.WorkOrderId));
            var intent=review.CreateIntent(Guid.NewGuid());intent.RequireSource(original);
            choice.RequireIntent(intent);
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;
            Review=CompanyInventorySourceReview.Copy(review);
        }
        finally {Busy=false;Changed?.Invoke();}
    }
    public async Task SubmitAsync(ICompanyMaintenanceHandoffClient client,Func<Guid,Task>? retainReferenceBeforeTransport=null)
    {
        if(Busy || Reference is not null || Review is null)throw new InvalidOperationException("Review the original assignment once; uncertain submissions require read-only recovery.");
        var original=source??throw new InvalidOperationException("Reload the original intervention source.");RequireCurrent(original);
        var intent=Review.CreateIntent(Guid.NewGuid());intent.RequireSource(original);
        if(intent.Context.Company!=company.Snapshot.Selection!.Scope || intent.Context.ActorUserId!=company.Snapshot.Selection.UserId)
            throw new InvalidOperationException("The assignment review belongs to another company/account.");
        Intent=CompanyInventorySourceReview.Copy(intent);referenceSource=CompanyInventorySourceReview.Copy(original);Review=null;
        if(retainReferenceBeforeTransport is not null)
        {
            Busy=true;Changed?.Invoke();
            try{await retainReferenceBeforeTransport(intent.IntentId);}finally{Busy=false;}
        }
        await ConfirmAsync(client,false);
    }
    public Task RecoverAsync(ICompanyMaintenanceHandoffClient client)=>ConfirmAsync(client,true);
    public async Task RecoverReferenceAsync(ICompanyMaintenanceHandoffClient client,Guid originalReference,Func<Guid,Task>? retainReferenceBeforeTransport=null)
    {
        if(Busy || originalReference==Guid.Empty || Reference is {} retained && retained!=originalReference)throw new InvalidOperationException("The same original maintenance assignment reference is required.");
        var original=source??throw new InvalidOperationException("Reload the original intervention source before reference recovery.");RequireCurrent(original);
        if(referenceSource is {} bound && bound with{RetrievedAtUtc=original.RetrievedAtUtc}!=original)throw new InvalidOperationException("Return to the original company/account/FIELD/source before reference recovery.");
        if(Intent is not null)Intent.RequireSource(original);
        reference=originalReference;referenceSource=CompanyInventorySourceReview.Copy(original);Review=null;Catalog=null;Receipt=null;
        var query=new CompanyMaintenanceRecoveryQuery(originalReference,original.DecisionId,original.FieldId,original.WellId,original.WorkOrderId);
        var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainReferenceBeforeTransport is not null)await retainReferenceBeforeTransport(originalReference);
            var result=await client.RecoverMaintenanceHandoffReferenceAsync(query);result.Require(original.Company,original.Actor,query);result.Intent.RequireSource(original);
            if(Intent is not null && Intent!=result.Intent)throw new InvalidOperationException("The recorded assignment differs from the retained original intent.");
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;
            Intent=CompanyInventorySourceReview.Copy(result.Intent);Receipt=CompanyInventorySourceReview.Copy(result.Receipt);confirmed=true;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    private async Task ConfirmAsync(ICompanyMaintenanceHandoffClient client,bool recovery)
    {
        if(Busy || Intent is null)throw new InvalidOperationException("The retained original assignment intent is required.");
        var intent=CompanyInventorySourceReview.Copy(Intent);
        var original=source??throw new InvalidOperationException("Reload the original source before checking its assignment.");
        RequireCurrent(original);intent.RequireSource(original);
        var snapshot=company.Snapshot;var generation=version;Receipt=null;Busy=true;Changed?.Invoke();
        try
        {
            var receipt=recovery?await client.RecoverMaintenanceHandoffAsync(intent):await client.StartMaintenanceHandoffAsync(intent);
            receipt.Require(intent);
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;
            Receipt=CompanyInventorySourceReview.Copy(receipt);
            confirmed=true;
        }
        finally {Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
