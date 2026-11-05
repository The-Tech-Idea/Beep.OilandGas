using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>One circuit's retained handoff intent; visible evidence is invalidated on company/source changes.</summary>
public sealed class CompanyAccountingHandoffState : IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyAccountingProcessStartCatalogQuery? source;
    private CancellationTokenSource changes=new();
    private long version;
    private bool confirmed,disposed;
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyAccountingProcessStartCatalog? Catalog {get;private set;}
    public CompanyAccountingProcessStartReview? Review {get;private set;}
    public CompanyAccountingProcessStart? Intent {get;private set;}
    public CompanyAccountingProcessStartReceipt? Receipt {get;private set;}
    public CompanyNativeDecisionHandoffReview? Discovery {get;private set;}
    public bool HasUnconfirmedIntent=>Intent is not null && !confirmed;
    public CompanyAccountingHandoffState(CompanyClientContext company){this.company=company;company.Changed+=ClearVisible;}
    public void AttachSource(string type,string? id,string? revision)
    {
        var next=id is null || revision is null?null:new CompanyAccountingProcessStartCatalogQuery(type,id,revision);
        next?.Require();
        if(source==next)return;
        ClearVisible();source=next;
    }
    private void ClearVisible()
    {
        version++;var previous=changes;changes=new();previous.Cancel();previous.Dispose();
        Catalog=null;Review=null;Receipt=null;Discovery=null;
        if(confirmed){Intent=null;confirmed=false;}
        Changed?.Invoke();
    }
    public async Task LoadCatalogAsync(IBusinessProcessServiceClient client)
    {
        if(disposed || Busy)throw new InvalidOperationException("Wait for the current handoff review or submission.");
        var selection=company.Snapshot;var query=source ?? throw new InvalidOperationException("Reload the original source before reviewing workflow choices.");
        if(selection.Selection is null)throw new InvalidOperationException("Select the original company before reviewing workflow choices.");
        Catalog=null;Review=null;Discovery=null;if(!confirmed)Receipt=null;var generation=++version;var token=changes.Token;Busy=true;Changed?.Invoke();
        try
        {
            CompanyNativeDecisionHandoffReview? discovered=null;
            if(CompanyAccountingSourceTypes.IsNativeDecision(query.EntityType) && query.EntityType!="SALES_CONTRACT")
            {
                if(HasUnconfirmedIntent && (Intent!.Company!=selection.Selection.Scope || Intent.ActorUserId!=selection.Selection.UserId || Intent.EntityType!=query.EntityType || Intent.EntityId!=query.EntityId || Intent.SourceRevision!=query.ExpectedSourceRevision))
                    throw new InvalidOperationException("Reconcile the retained original handoff in its original company/account/preparation.");
                discovered=await client.DiscoverNativeDecisionHandoffAsync(query,token);discovered.Require(selection.Selection.Scope,selection.Selection.UserId,query);
                if(disposed || generation!=version || selection!=company.Snapshot || query!=source)return;
                if(discovered.Start is {} existing)
                {
                    if(Intent is not null && JsonSerializer.Serialize(existing)!=JsonSerializer.Serialize(Intent))throw new InvalidOperationException("A different handoff is recorded. Retain the original intent for reconciliation.");
                    Discovery=CompanyInventorySourceReview.Copy(discovered);
                    if(Intent is not null)confirmed=true;
                    return;
                }
                if(confirmed)throw new InvalidOperationException("The previously confirmed original handoff is unavailable. Reconcile its recorded process.");
            }
            var catalog=await client.GetCompanyAccountingStartCatalogAsync(query,token);catalog.Require(selection.Selection.Scope,selection.Selection.UserId,query);
            if(disposed || generation!=version || selection!=company.Snapshot || query!=source)return;
            if(discovered is not null)
            {
                var closing=await client.DiscoverNativeDecisionHandoffAsync(query,token);closing.Require(selection.Selection.Scope,selection.Selection.UserId,query);
                if(disposed || generation!=version || selection!=company.Snapshot || query!=source)return;
                if(JsonSerializer.Serialize(closing)!=JsonSerializer.Serialize(discovered))throw new InvalidOperationException("The original handoff candidate set changed while reviewing choices.");
                Discovery=CompanyInventorySourceReview.Copy(closing);
            }
            Catalog=JsonSerializer.Deserialize<CompanyAccountingProcessStartCatalog>(JsonSerializer.Serialize(catalog))!;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Select(string processId)
    {
        if(Busy || Catalog is null)throw new InvalidOperationException("Load recorded workflow choices first.");
        var review=Catalog.Choices.SingleOrDefault(choice=>choice.ProcessId==processId) ?? throw new InvalidOperationException("Select a recorded workflow from the current choices.");
        if(Intent is not null)review.RequireIntent(Intent);
        Review=null;if(!confirmed)Receipt=null;
        Review=review;Changed?.Invoke();
    }
    public async Task SubmitAsync(IBusinessProcessServiceClient client)
    {
        if(disposed || Busy || Review is null || source is null)throw new InvalidOperationException("Review a recorded workflow before submitting.");
        if(confirmed)throw new InvalidOperationException("This handoff is already confirmed. Open its recorded process to continue.");
        var selected=company.Snapshot;var review=Review;
        if(selected.Selection is null || selected.Selection.Scope!=review.Company || selected.Selection.UserId!=review.ActorUserId ||
            source.EntityType!=review.EntityType || source.EntityId!=review.EntityId || source.ExpectedSourceRevision!=review.SourceRevision)
            throw new InvalidOperationException("Return to the original company and displayed source before submitting.");
        if(Intent is null)Intent=review.CreateIntent(Guid.NewGuid().ToString("N"));
        review.RequireIntent(Intent);
        var intent=Intent;var generation=version;var token=changes.Token;Receipt=null;Busy=true;Changed?.Invoke();
        try
        {
            var receipt=await client.StartCompanyAccountingAsync(intent,token);receipt.Require(intent);
            if(disposed || generation!=version || selected!=company.Snapshot)return;
            Receipt=JsonSerializer.Deserialize<CompanyAccountingProcessStartReceipt>(JsonSerializer.Serialize(receipt))!;confirmed=true;
        }
        catch
        {
            Review=null;Catalog=null;throw;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=ClearVisible;changes.Cancel();changes.Dispose();}
}
