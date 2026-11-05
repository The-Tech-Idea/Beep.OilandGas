using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Client.App;
namespace Beep.OilandGas.Web.Services;
public sealed partial class WorkOrderExecutionPermitDecisionState
{
    private CompanyWorkOrderPersonnelReleaseCommand? personnelReleaseCommand;
    private CompanyWorkOrderPersonnelReleaseReference? personnelReleaseReference;
    private CompanyWorkOrderExecutionPermitReference? personnelReleaseOrigin;
    private CompanyClientSelection? personnelReleaseOwner;
    private WORK_ORDER_PERSONNEL_RELEASE? personnelReleaseReceipt;
    private bool personnelReleaseConfirmed;
    private bool SamePersonnelReleaseOrigin=>proposal==personnelReleaseOrigin && personnelReleaseOwner is {} original && company.Selection is {} current && current.Scope==original.Scope && current.UserId==original.UserId;
    public CompanyWorkOrderPersonnelReleaseReference? VisiblePersonnelReleaseReference=>SamePersonnelReleaseOrigin?personnelReleaseReference:null;
    public CompanyWorkOrderPersonnelReleaseCommand? VisiblePersonnelReleaseCommand=>SamePersonnelReleaseOrigin && personnelReleaseCommand is not null?CompanyInventorySourceReview.Copy(personnelReleaseCommand):null;
    public WORK_ORDER_PERSONNEL_RELEASE? PersonnelReleaseReceipt=>SamePersonnelReleaseOrigin && personnelReleaseReceipt is not null?CompanyInventorySourceReview.Copy(personnelReleaseReceipt):null;
    public bool CanReleasePersonnel=>!Busy && personnelReleaseCommand is null && personnelReleaseReference is null && PersonnelReceipt is not null && VisiblePersonnelCommand is not null;
    private void ClearPersonnelReleaseDisplay(){personnelReleaseReceipt=null;if(personnelReleaseConfirmed){personnelReleaseCommand=null;personnelReleaseReference=null;personnelReleaseOrigin=null;personnelReleaseOwner=null;personnelReleaseConfirmed=false;}}
    private CompanyClientSelection RequirePersonnelReleaseCurrent(){var selected=RequireCurrent();if(!selected.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel cancellation permission is required.");return selected;}
    public async Task ReleasePersonnelAsync(ICompanyMaintenanceHandoffClient client,string reason,Func<CompanyWorkOrderPersonnelReleaseReference,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanReleasePersonnel)throw new InvalidOperationException("Recover the original personnel assignment and reconcile retained cancellation before replacing any intent.");
        var selected=RequirePersonnelReleaseCurrent();var assignment=VisiblePersonnelCommand!;var recorded=PersonnelReceipt!;
        var original=new CompanyWorkOrderPersonnelReleaseCommand(new(assignment,recorded),Guid.NewGuid(),reason);original.Require();
        var reference=new CompanyWorkOrderPersonnelReleaseReference(new(assignment.Review.Personnel.Query.Permit,assignment.IntentId),original.IntentId);
        personnelReleaseCommand=original;personnelReleaseReference=reference;personnelReleaseOrigin=proposal;personnelReleaseOwner=selected;personnelReleaseReceipt=null;personnelReleaseConfirmed=false;
        var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original personnel cancellation context changed before transport.");
            var result=await client.RecordWorkOrderPersonnelReleaseAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);
            if(!disposed && generation==version && snapshot==company.Snapshot){personnelReleaseReceipt=CompanyInventorySourceReview.Copy(result);personnelReleaseConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverPersonnelReleaseReferenceAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderPersonnelReleaseReference reference,Func<CompanyWorkOrderPersonnelReleaseReference,Task>? retainBeforeTransport=null)
    {
        reference.Require();if(Busy || personnelReleaseReference is {} retained && retained!=reference)throw new InvalidOperationException("Use the same original retained personnel cancellation reference.");
        var selected=RequirePersonnelReleaseCurrent();if(proposal!=reference.Assignment.Permit || personnelReleaseOrigin is not null && !SamePersonnelReleaseOrigin)throw new InvalidOperationException("Return to the same original cancellation maker/company/permit task.");
        personnelReleaseReference=reference;personnelReleaseOrigin=proposal;personnelReleaseOwner=selected;personnelReleaseReceipt=null;personnelReleaseConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original personnel cancellation reference context changed before transport.");
            var result=await client.RecoverWorkOrderPersonnelReleaseReferenceAsync(reference);result.Require(selected.Scope,selected.UserId,reference);
            if(personnelReleaseCommand is not null && System.Text.Json.JsonSerializer.Serialize(personnelReleaseCommand)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained personnel cancellation body changed.");
            if(!selected.Roles.Contains(result.Command.Original.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel configured role is required.");
            if(!disposed && generation==version && snapshot==company.Snapshot){personnelReleaseCommand=CompanyInventorySourceReview.Copy(result.Command);personnelReleaseReceipt=CompanyInventorySourceReview.Copy(result.Recorded);personnelReleaseConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public void BeginNewPersonnelDuty()
    {
        if(Busy || !SamePersonnelReleaseOrigin || !personnelReleaseConfirmed || personnelReleaseCommand is null || personnelReleaseReceipt is null)throw new InvalidOperationException("Confirm the exact original cancellation before beginning another duty.");
        RequirePersonnelReleaseCurrent();personnelReleaseCommand.RequireRecorded(personnelReleaseReceipt);
        personnelCommand=null;personnelReference=null;personnelOrigin=null;personnelOwner=null;personnelReceipt=null;personnelConfirmed=false;
        personnelReleaseCommand=null;personnelReleaseReference=null;personnelReleaseOrigin=null;personnelReleaseOwner=null;personnelReleaseReceipt=null;personnelReleaseConfirmed=false;version++;Changed?.Invoke();
    }
}
