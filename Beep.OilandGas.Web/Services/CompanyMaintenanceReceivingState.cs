using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;

/// <summary>Retains one circuit's original receiving command; only read-only recovery reconciles uncertainty.</summary>
public sealed class CompanyMaintenanceReceivingState : IDisposable
{
    private readonly CompanyClientContext company;
    private CompanyMaintenanceReceivingQuery? task,origin;
    private CompanyClientSelection? originalCompany;
    private string? sourceRevision;
    private Guid? reference;
    private long version;
    private bool confirmed,disposed;
    public event Action? Changed;
    public bool Busy {get;private set;}
    public CompanyMaintenanceAcceptanceReview? Review {get;private set;}
    public CompanyMaintenanceAcceptanceCommand? Intent {get;private set;}
    public CompanyMaintenanceAcceptanceReceipt? Receipt {get;private set;}
    public Guid? Reference=>Intent?.IntentId??reference;
    public Guid? VisibleReference=>task==origin && company.Snapshot.Selection is {} current && originalCompany is {} selected && current.Scope==selected.Scope && current.UserId==selected.UserId?Reference:null;
    public CompanyMaintenanceReceivingState(CompanyClientContext company){this.company=company;company.Changed+=Invalidate;}
    public void Attach(CompanyMaintenanceReceivingQuery? next,string? revision)
    {
        next?.Require();if(task==next && sourceRevision==revision)return;Invalidate();task=next;sourceRevision=revision;
    }
    private void Invalidate()
    {
        version++;task=null;sourceRevision=null;Review=null;Receipt=null;
        if(confirmed){Intent=null;reference=null;origin=null;originalCompany=null;confirmed=false;}
        Changed?.Invoke();
    }
    private CompanyClientSelection RequireCurrent()
    {
        var selected=company.Snapshot.Selection;
        if(disposed || selected is null || task is null || string.IsNullOrWhiteSpace(sourceRevision))throw new InvalidOperationException("Reload the original company/FIELD/order receiving task.");
        if(!selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current app receiving approval permission is required.");return selected;
    }
    private static void RequireReview(CompanyMaintenanceAcceptanceReview review,CompanyClientSelection selected,CompanyMaintenanceReceivingQuery current)
    {
        review.Require();
        if(review.Context.Company!=selected.Scope || review.Context.ActorUserId!=selected.UserId || review.InstanceId!=current.InstanceId || review.Assignment.FieldId!=current.FieldId || review.Assignment.WorkOrderId!=current.WorkOrderId || !selected.Roles.Contains(review.AssignedRole,StringComparer.Ordinal))
            throw new InvalidOperationException("The original receiving review differs from the selected company/account/task/role.");
    }
    public async Task ReviewAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || Reference is not null)throw new InvalidOperationException("Reconcile the original acceptance before reviewing another command.");var selected=RequireCurrent();var current=task!;var snapshot=company.Snapshot;var generation=version;
        Review=null;Receipt=null;Busy=true;Changed?.Invoke();
        try{var review=await client.ReviewMaintenanceReceivingTaskAsync(current);RequireReview(review,selected,current);if(disposed || generation!=version || snapshot!=company.Snapshot)return;Review=CompanyInventorySourceReview.Copy(review);}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task AcceptAsync(ICompanyMaintenanceHandoffClient client,string notes,Func<Guid,Task> retainReferenceBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainReferenceBeforeTransport);
        if(Busy || Reference is not null || Review is null)throw new InvalidOperationException("Review the original task once; uncertain acceptance requires read-only recovery.");
        var selected=RequireCurrent();RequireReview(Review,selected,task!);
        var command=new CompanyMaintenanceAcceptanceCommand(CompanyInventorySourceReview.Copy(Review),Guid.NewGuid(),notes);command.Require();
        Intent=command;reference=command.IntentId;origin=task;originalCompany=selected;Review=null;Receipt=null;
        var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            await retainReferenceBeforeTransport(command.IntentId);
            if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("The original receiving context changed before transport.");
            var receipt=await client.AcceptMaintenanceAsync(CompanyInventorySourceReview.Copy(command));receipt.Require(command);
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;Receipt=CompanyInventorySourceReview.Copy(receipt);confirmed=true;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverAsync(ICompanyMaintenanceHandoffClient client,Guid id,Func<Guid,Task>? retainReferenceBeforeTransport=null)
    {
        if(Busy || id==Guid.Empty || Reference is {} retained && retained!=id)throw new InvalidOperationException("Use the same original acceptance reference.");var selected=RequireCurrent();var current=task!;
        if(origin is not null && (origin!=current || originalCompany!.Scope!=selected.Scope || originalCompany.UserId!=selected.UserId))throw new InvalidOperationException("Return to the original receiving company/account/task before recovery.");
        reference=id;origin=current;originalCompany=selected;Review=null;Receipt=null;var query=new CompanyMaintenanceAcceptanceReferenceQuery(current,id);var snapshot=company.Snapshot;var generation=version;Busy=true;Changed?.Invoke();
        try
        {
            if(retainReferenceBeforeTransport is not null)await retainReferenceBeforeTransport(id);
            if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("The original receiving context changed before recovery.");
            var result=await client.RecoverMaintenanceAcceptanceReferenceAsync(query);result.Require(selected.Scope,selected.UserId,query);RequireReview(result.Command.Review,selected,current);
            if(Intent is not null && Intent!=result.Command)throw new InvalidOperationException("The recorded acceptance differs from the original retained command.");
            if(disposed || generation!=version || snapshot!=company.Snapshot)return;Intent=CompanyInventorySourceReview.Copy(result.Command);Receipt=CompanyInventorySourceReview.Copy(result.Receipt);confirmed=true;
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void Dispose(){if(disposed)return;disposed=true;company.Changed-=Invalidate;Invalidate();}
}
