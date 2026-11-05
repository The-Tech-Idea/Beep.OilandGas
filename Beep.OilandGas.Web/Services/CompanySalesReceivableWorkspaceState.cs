using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Circuit-owned exact draft request, retained through uncertain writes and presentation changes.</summary>
public sealed partial class CompanySalesReceivableWorkspaceState:IDisposable
{
    private readonly CompanyClientContext company;
    private CompanySalesReceivableStatusReview? status;
    private CompanySalesReceivableWorkspaceReview? workspace;
    private CompanySalesReceivableRecovery? recovery;
    private CompanySalesReceivableCommand? pending;
    private object? owner;
    private bool admitted,disposed;
    private long version;
    public CompanySalesReceivableWorkspaceState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public event Action? Changed;
    public bool Busy{get;private set;}
    public bool HasPendingIntent=>pending is not null;
    private bool Visible=>!disposed && owner is not null && admitted && company.Selection is {} selected && selected.Roles.Count>0 && selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal);
    private bool PendingMatches=>Visible && pending is not null && pending.Source.Company==company.Selection!.Scope && pending.Source.ActorUserId==company.Selection.UserId;
    public CompanySalesReceivableStatusReview? Status=>Visible && status is not null?Copy(status):null;
    public CompanySalesReceivableWorkspaceReview? Workspace=>Visible && workspace is not null?Copy(workspace):null;
    public CompanySalesReceivableRecovery? Recovery=>Visible && recovery is not null?Copy(recovery):null;
    public CompanySalesReceivableCommand? Pending=>PendingMatches?Copy(pending!):null;
    public bool CanRetryPending=>PendingMatches && !Busy && company.Selection!.Permissions.Contains("Accounting.PrepareSalesReceivable",StringComparer.Ordinal);
    public bool CanPrepare=>Visible && !Busy && pending is null && pendingReview is null && pendingJournal is null && pendingJournalReview is null && pendingPaymentReview is null && pendingPayment is null && pendingJournalPosting is null && workspace is {Recovery.Retained:null} current && current.Source.CanPrepare && company.Selection!.Permissions.Contains("Accounting.PrepareSalesReceivable",StringComparer.Ordinal);
    private static T Copy<T>(T value)=>CompanyInventorySourceReview.Copy(value);
    public void AttachOwner(object presentation){ArgumentNullException.ThrowIfNull(presentation);if(ReferenceEquals(owner,presentation))return;owner=presentation;Invalidate();}
    public void DetachOwner(object presentation){if(!ReferenceEquals(owner,presentation))return;owner=null;Invalidate();}
    public void Invalidate(){++version;admitted=false;paymentReviewSource=null;paymentReviewRecovery=null;paymentSource=null;paymentResult=null;status=null;workspace=null;recovery=null;journalSource=null;InvalidateReview();Changed?.Invoke();}
    public void Admit(ClaimsPrincipal user)
    {
        var actor=user.FindFirstValue(ClaimTypes.NameIdentifier)??user.FindFirstValue("sub");var subject=user.FindFirstValue("sub");
        if(disposed || owner is null || company.Selection is not {} selected || user.Identity?.IsAuthenticated!=true || actor!=selected.UserId || subject is not null && subject!=selected.UserId ||
            !selected.Roles.Any(user.IsInRole) || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))
        {Invalidate();throw new UnauthorizedAccessException("Current company account, app roles and Accounting.View are required.");}admitted=true;
    }
    private void Ready(bool write=false)
    {if(!Visible || Busy || write && !company.Selection!.Permissions.Contains("Accounting.PrepareSalesReceivable",StringComparer.Ordinal))throw new InvalidOperationException("Review current company/account authority and wait for the active receivable operation.");}
    private void Original(string invoice)
    {
        if(!Guid.TryParseExact(invoice,"N",out var invoiceGuid) || invoiceGuid==Guid.Empty)throw new ArgumentException("The original invoice identity is required.");
        if(pending is not null && (!PendingMatches || pending.Source.InvoiceId!=invoice))throw new InvalidOperationException("Return to the original company, account and invoice to reconcile the retained request.");
    }
    private void Accept(CompanySalesReceivableRecovery loaded)
    {
        if(pendingPaymentReview is not null || pendingPayment is not null || pendingReview is not null || pendingJournal is not null || pendingJournalReview is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Reconcile the original retained reviewer or journal request first.");
        loaded.Require(company.Selection!.Scope,company.Selection.UserId,loaded.InvoiceId);
        var command=loaded.Retained?.Restore(loaded.Company,loaded.ActorUserId);
        if(pending is not null && (command is null || JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(pending)))throw new InvalidOperationException("Native evidence does not confirm the exact original receivable request.");
        recovery=Copy(loaded);
        if(command is not null){if(loaded.Outcome is not null){loaded.Outcome.Require(command);pending=null;}else pending=Copy(command);}
    }
    private async Task ReadAsync(string invoice,Func<Task> read)
    {
        Ready();Original(invoice);InvalidateReview();Busy=true;Changed?.Invoke();try{await read();}finally{Busy=false;Changed?.Invoke();}
    }
    public Task LoadStatusAsync(string invoice,IAccountingServiceClient client)=>ReadAsync(invoice,async()=>
    {
        var snapshot=company.Snapshot;var generation=++version;status=null;workspace=null;recovery=null;
        var loaded=Copy(await client.GetCompanySalesReceivableStatusAsync(invoice));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,invoice);
        if(!disposed && generation==version && snapshot==company.Snapshot && Visible)status=loaded;
    });
    public Task LoadSourceAsync(string invoice,IAccountingServiceClient client)=>ReadAsync(invoice,async()=>
    {
        if(!company.Selection!.Permissions.Contains("Accounting.PrepareSalesReceivable",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current receivable preparation permission is required.");
        var snapshot=company.Snapshot;var generation=++version;workspace=null;recovery=null;
        var loaded=Copy(await client.GetCompanySalesReceivableSourceAsync(invoice));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,invoice);
        if(!disposed && generation==version && snapshot==company.Snapshot && Visible){Accept(loaded.Recovery);workspace=loaded;}
    });
    public Task RecoverAsync(string invoice,IAccountingServiceClient client)=>ReadAsync(invoice,async()=>
    {
        var snapshot=company.Snapshot;var generation=++version;workspace=null;recovery=null;
        var loaded=Copy(await client.GetCompanySalesReceivableRecoveryAsync(invoice));loaded.Require(snapshot.Selection!.Scope,snapshot.Selection.UserId,invoice);
        if(!disposed && generation==version && snapshot==company.Snapshot && Visible)Accept(loaded);
    });
    public async Task PrepareAsync(string number,string reason,IAccountingServiceClient client)
    {
        Ready(true);if(!CanPrepare || workspace is null)throw new InvalidOperationException("Review current approved invoice evidence and reconcile earlier requests first.");
        var command=new CompanySalesReceivableCommand(Guid.NewGuid().ToString("N"),Guid.NewGuid().ToString("N"),Copy(workspace.Source),number,reason);command.Require();pending=command;await SubmitPendingAsync(client);
    }
    public async Task SubmitPendingAsync(IAccountingServiceClient client)
    {
        if(pendingPaymentReview is not null || pendingPayment is not null || pendingReview is not null || pendingJournal is not null || pendingJournalReview is not null || pendingJournalPosting is not null)throw new InvalidOperationException("Reconcile the original retained reviewer or journal request first.");
        Ready(true);if(!PendingMatches)throw new InvalidOperationException("Return to the original company and account to reconcile.");
        var command=Copy(pending!);var snapshot=company.Snapshot;var generation=version;
        bool Current()=>!disposed && generation==version && snapshot==company.Snapshot && Visible;
        workspace=null;recovery=null;status=null;Busy=true;Changed?.Invoke();
        try
        {
            var result=Copy(await client.PrepareCompanySalesReceivableAsync(Copy(command)));result.Require(command);if(!Current())return;
            var loaded=Copy(await client.GetCompanySalesReceivableRecoveryAsync(command.Source.InvoiceId));loaded.Require(command.Source.Company,command.Source.ActorUserId,command.Source.InvoiceId);if(!Current())return;
            if(loaded.Outcome is null || JsonSerializer.Serialize(loaded.Outcome)!=JsonSerializer.Serialize(result))throw new InvalidOperationException("Current native evidence does not confirm the exact original receivable and receipt.");Accept(loaded);
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;owner=null;company.Changed-=Invalidate;Invalidate();}
}
