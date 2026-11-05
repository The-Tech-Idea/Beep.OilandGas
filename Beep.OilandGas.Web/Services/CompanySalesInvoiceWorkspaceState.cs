using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned original invoice intent; presentation changes never discard an uncertain write.</summary>
public sealed class CompanySalesInvoiceWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private CompanySalesInvoiceWorkspaceReview? workspace;
    private CompanySalesInvoiceDraftCommand? pending;
    private CompanySalesInvoiceDraftOutcome? result;
    private object? owner;
    private bool admitted,disposed;
    private long version;
    public CompanySalesInvoiceWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public bool HasPendingIntent=>pending is not null;
    private bool Visible=>!disposed && owner is not null && admitted && company.Selection is {} selected && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    private bool PendingMatches=>Visible && pending is not null && company.Selection!.Scope==pending.Review.Company && company.Selection.UserId==pending.Review.ActorUserId;
    public CompanySalesInvoiceWorkspaceReview? Workspace=>!Visible || workspace is null?null:Copy(workspace);
    public CompanySalesInvoiceDraftOutcome? Result=>!Visible || result is null?null:Copy(result);
    public CompanySalesInvoiceDraftCommand? Pending=>!PendingMatches?null:Copy(pending!);
    public bool CanRetryPending=>PendingMatches && !Busy && company.Selection!.Permissions.Contains("Accounting.PrepareSalesInvoice",StringComparer.Ordinal);
    public bool HasCurrencyPolicy=>Visible && workspace is {} current && current.Source.Policies.Records.Any(row=>
        CompanyBillingPolicyProof.Require(current.Source.Company,row).Record is {ApprovedBy:not null} record && record.Rules.CurrencyCode==current.Source.Delivery.ReviewBillingBasis().CurrencyCode);
    public bool CanPrepare=>Visible && !Busy && pending is null && workspace is {Retained:null} current && current.Source.Invoices.Count==0 &&
        current.Source.Delivery.ReviewBillingBasis().HasCompatiblePriceBasis && HasCurrencyPolicy && company.Selection!.Permissions.Contains("Accounting.PrepareSalesInvoice",StringComparer.Ordinal);
    private static T Copy<T>(T value)=>CompanyInventorySourceReview.Copy(value);
    public void AttachOwner(object presentation){ArgumentNullException.ThrowIfNull(presentation);if(ReferenceEquals(owner,presentation))return;owner=presentation;Invalidate();}
    public void DetachOwner(object presentation){if(!ReferenceEquals(owner,presentation))return;owner=null;Invalidate();}
    public void Invalidate(){++version;admitted=false;workspace=null;result=null;Changed?.Invoke();}
    public void Admit(ClaimsPrincipal user)
    {
        var actor=user.FindFirstValue(ClaimTypes.NameIdentifier) ??user.FindFirstValue("sub");
        if(disposed || owner is null || company.Selection is not {} selected || user.Identity?.IsAuthenticated!=true || actor!=selected.UserId ||
            !selected.Roles.Any(user.IsInRole) || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))
        {Invalidate();throw new UnauthorizedAccessException("Current company accounting role and account are required.");}
        admitted=true;
    }
    private void Ready(bool write=false)
    {if(!Visible || Busy || write && !company.Selection!.Permissions.Contains("Accounting.PrepareSalesInvoice",StringComparer.Ordinal))throw new InvalidOperationException("Review current company/account authority and wait for the active invoice operation.");}
    private void Accept(CompanySalesInvoiceWorkspaceReview loaded)
    {
        loaded.Require(company.Selection!.Scope,company.Selection.UserId,loaded.Source.TicketId);
        var original=loaded.Retained?.Restore(loaded.Source.Company,loaded.Source.ActorUserId);
        if(pending is not null && (original is null || JsonSerializer.Serialize(original)!=JsonSerializer.Serialize(pending)))
            throw new InvalidOperationException("Current native evidence does not confirm the exact original invoice request.");
        workspace=Copy(loaded);result=null;
        if(original is null)return;
        if(loaded.Outcome is {} outcome){outcome.Require(original);result=Copy(outcome);pending=null;}
        else pending=Copy(original);
    }
    public async Task LoadAsync(string ticket,IAccountingServiceClient client)
    {
        Ready();if(string.IsNullOrWhiteSpace(ticket) || ticket.Length>128 || ticket.Any(char.IsControl))throw new InvalidOperationException("The bounded original delivery identity is required.");
        if(pending is not null && (!PendingMatches || pending.Review.TicketId!=ticket))throw new InvalidOperationException("Return to the original company, account and delivery to reconcile the retained request.");
        var snapshot=company.Snapshot;var generation=++version;workspace=null;result=null;Busy=true;Changed?.Invoke();
        try
        {
            var loaded=Copy(await client.GetCompanySalesInvoiceWorkspaceAsync(ticket));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,ticket);
            if(!disposed && generation==version && snapshot==company.Snapshot && Visible)Accept(loaded);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task PrepareAsync(CompanySalesInvoiceFacts facts,IAccountingServiceClient client)
    {
        Ready(true);if(!CanPrepare || workspace is null)throw new InvalidOperationException("Review current original source and reconcile any earlier invoice request first.");
        var command=new CompanySalesInvoiceDraftCommand(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),Copy(workspace.Source),Copy(facts));command.Require();
        pending=command;await SubmitPendingAsync(client);
    }
    public async Task SubmitPendingAsync(IAccountingServiceClient client)
    {
        Ready(true);if(!PendingMatches)throw new InvalidOperationException("Return to the original invoice company and account to reconcile.");
        var command=Copy(pending!);var snapshot=company.Snapshot;var generation=version;
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && Visible;
        workspace=null;result=null;Busy=true;Changed?.Invoke();
        try
        {
            var outcome=Copy(await client.PrepareCompanySalesInvoiceAsync(Copy(command)));outcome.Require(command);if(!Current())return;
            var loaded=Copy(await client.GetCompanySalesInvoiceWorkspaceAsync(command.Review.TicketId));loaded.Require(command.Review.Company,command.Review.ActorUserId,command.Review.TicketId);if(!Current())return;
            if(loaded.Outcome is null || JsonSerializer.Serialize(loaded.Outcome)!=JsonSerializer.Serialize(outcome))throw new InvalidOperationException("Complete current native evidence does not confirm the exact invoice and outcome receipt.");
            Accept(loaded);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;owner=null;company.Changed-=Invalidate;Invalidate();}
}
