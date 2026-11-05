using System.Security.Claims;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed class CompanySalesInvoiceReviewWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private CompanySalesInvoiceReviewSource? source;
    private CompanySalesInvoiceDecisionCommand? pending;
    private object? owner;
    private bool admitted,disposed;
    private long version;
    public CompanySalesInvoiceReviewWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public CompanySalesInvoiceReviewSource? Source=>!disposed && admitted && owner is not null && source is not null?CompanyInventorySourceReview.Copy(source):null;
    public CompanySalesInvoiceDecisionCommand? Pending=>!disposed && admitted && owner is not null && pending is not null && company.Selection is {} selected && pending.Company==selected.Scope && pending.ActorUserId==selected.UserId?CompanyInventorySourceReview.Copy(pending):null;
    public bool CanDecide=>!Busy && Pending is null && pending is null && Source?.CanReview==true && company.Selection!.Permissions.Contains("Accounting.RecordSalesInvoiceDecision",StringComparer.Ordinal);
    public void AttachOwner(object value){ArgumentNullException.ThrowIfNull(value);if(ReferenceEquals(owner,value))return;owner=value;Invalidate();}
    public void DetachOwner(object value){if(!ReferenceEquals(owner,value))return;owner=null;Invalidate();}
    public void Invalidate(){++version;source=null;admitted=false;Changed?.Invoke();}
    public void Admit(ClaimsPrincipal user)
    {
        var actor=user.FindFirstValue(ClaimTypes.NameIdentifier) ??user.FindFirstValue("sub");
        if(disposed || owner is null || company.Selection is not {} selected || user.Identity?.IsAuthenticated!=true || actor!=selected.UserId || !selected.Roles.Any(user.IsInRole) ||
            !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Accounting.ReviewSalesInvoice",StringComparer.Ordinal))
        {Invalidate();throw new UnauthorizedAccessException("Select the original company and account with current invoice review authority.");}admitted=true;
    }
    public async Task LoadAsync(string invoice,IAccountingServiceClient client,string? preparationRevision=null,string? assignedReviewRevision=null)
    {
        if(disposed || Busy || owner is null || !admitted)throw new InvalidOperationException("Review current company/account authority before loading invoice evidence.");
        var snapshot=company.Snapshot;var generation=++version;source=null;Busy=true;Changed?.Invoke();
        try
        {
            var loaded=CompanyInventorySourceReview.Copy(await client.GetCompanySalesInvoiceReviewAsync(invoice));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,invoice);
            if(preparationRevision is not null && CompanyAccountingSourceTypes.InvoicePreparationRevision(loaded.Original)!=preparationRevision)throw new InvalidOperationException("The assigned original invoice preparation changed.");
            if(assignedReviewRevision is not null && loaded.Revision!=assignedReviewRevision)throw new InvalidOperationException("The displayed assigned invoice evidence changed. Reload the process before reviewing this invoice.");
            if(!Current())return;
            var retained=await client.GetCompanySalesInvoiceReviewIntentAsync(invoice);if(!Current())return;
            if(retained is not null)
            {
                var original=retained.Restore(snapshot.Selection.Scope,snapshot.Selection.UserId);
                if(original.InvoiceId!=invoice || pending is not null && System.Text.Json.JsonSerializer.Serialize(pending)!=System.Text.Json.JsonSerializer.Serialize(original))throw new InvalidOperationException("A different original reviewer command remains retained. Reconcile the original invoice first.");
                pending=original;
            }
            if(loaded.Original.Invoice.REVIEW_WORKFLOW_JSON is not null)
            {
                var proof=CompanySalesInvoiceDecisionProof.Require(snapshot.Selection.Scope,loaded.Original.Invoice);
                if(pending is not null)
                {
                    if(System.Text.Json.JsonSerializer.Serialize(proof.Intent.Restore(snapshot.Selection.Scope,pending.ActorUserId))!=System.Text.Json.JsonSerializer.Serialize(pending))throw new InvalidOperationException("The recorded native disposition differs from the original retained command.");pending=null;
                }
            }
            else if(pending is not null && pending.InvoiceId!=invoice)throw new InvalidOperationException("Reconcile the original retained reviewer command before another invoice.");
            if(Current())source=loaded;
        }
        finally{Busy=false;Changed?.Invoke();}
        bool Current()=>!disposed && admitted && owner is not null && generation==version && snapshot==company.Snapshot;
    }
    public async Task RecordAsync(string decision,string taxDisposition,string taxEvidence,string chargesDisposition,string chargesEvidence,string entitlementDisposition,string entitlementEvidence,string reason,IAccountingServiceClient client)
    {
        if(!CanDecide)throw new InvalidOperationException("Review the current original invoice and independent disposition authority first.");
        var command=new CompanySalesInvoiceDecisionCommand(Guid.NewGuid().ToString("N"),Source!,decision,taxDisposition,taxEvidence,chargesDisposition,chargesEvidence,entitlementDisposition,entitlementEvidence,reason);command.Require();pending=CompanyInventorySourceReview.Copy(command);await Send(client);
    }
    public Task RetryAsync(IAccountingServiceClient client)=>Send(client);
    private async Task Send(IAccountingServiceClient client)
    {
        if(disposed || Busy || !admitted || owner is null || Pending is not {} original || company.Selection is not {} selected || !selected.Permissions.Contains("Accounting.RecordSalesInvoiceDecision",StringComparer.Ordinal))throw new UnauthorizedAccessException("Return to the original reviewer company/account with current disposition authority to retry the retained command.");
        var snapshot=company.Snapshot;var generation=++version;source=null;Busy=true;Changed?.Invoke();
        bool Current()=>!disposed && admitted && owner is not null && generation==version && snapshot==company.Snapshot;
        try
        {
            var result=await client.RecordCompanySalesInvoiceDecisionAsync(CompanyInventorySourceReview.Copy(original));result.Require(original.Review.Original.Intent.Restore(original.Company,original.Review.Original.Intent.ROW_CREATED_BY));
            if(System.Text.Json.JsonSerializer.Serialize(CompanySalesInvoiceDecisionProof.Require(original.Company,result.Invoice).Intent.Restore(original.Company,original.ActorUserId))!=System.Text.Json.JsonSerializer.Serialize(original))throw new InvalidOperationException("The native disposition acknowledgement differs from the retained original command.");
            if(!Current())return;
            var closing=await client.GetCompanySalesInvoiceReviewAsync(original.InvoiceId);closing.Require(original.Company,original.ActorUserId,original.InvoiceId);
            if(System.Text.Json.JsonSerializer.Serialize(closing.Original)!=System.Text.Json.JsonSerializer.Serialize(result))throw new InvalidOperationException("The complete current invoice disposition confirmation is unavailable.");
            if(Current()){pending=null;source=CompanyInventorySourceReview.Copy(closing);}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;owner=null;company.Changed-=Invalidate;Invalidate();}
}
