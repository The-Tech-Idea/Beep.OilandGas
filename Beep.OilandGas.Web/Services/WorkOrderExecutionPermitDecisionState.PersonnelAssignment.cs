using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class WorkOrderExecutionPermitDecisionState
{
    private CompanyWorkOrderPersonnelAssignmentCommand? personnelCommand;
    private CompanyWorkOrderPersonnelAssignmentReference? personnelReference;
    private CompanyWorkOrderExecutionPermitReference? personnelOrigin;
    private CompanyClientSelection? personnelOwner;
    private WORK_ORDER_PERSONNEL_ASSIGNMENT? personnelReceipt;
    private bool personnelConfirmed;
    private bool SamePersonnelOrigin=>proposal==personnelOrigin && personnelOwner is {} original && company.Selection is {} current && current.Scope==original.Scope && current.UserId==original.UserId;
    public CompanyWorkOrderPersonnelAssignmentReference? VisiblePersonnelReference=>SamePersonnelOrigin?personnelReference:null;
    public CompanyWorkOrderPersonnelAssignmentCommand? VisiblePersonnelCommand=>SamePersonnelOrigin && personnelCommand is not null?CompanyInventorySourceReview.Copy(personnelCommand):null;
    public WORK_ORDER_PERSONNEL_ASSIGNMENT? PersonnelReceipt=>SamePersonnelOrigin && personnelReceipt is not null?CompanyInventorySourceReview.Copy(personnelReceipt):null;
    public bool CanAssignPersonnel=>!Busy && personnelCommand is null && personnelReference is null;
    private void ClearPersonnelDisplay(){personnelReceipt=null;if(personnelConfirmed){personnelCommand=null;personnelReference=null;personnelOrigin=null;personnelOwner=null;personnelConfirmed=false;}}
    private CompanyClientSelection RequirePersonnelCurrent()
    {
        var selected=RequireCurrent();if(!selected.Permissions.Contains("Maintenance.AssignPersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current personnel assignment permission is required.");return selected;
    }
    private CompanyClientSelection RequirePersonnelReferenceCurrent()
    {
        var selected=RequireCurrent();if(!selected.Permissions.Contains("Maintenance.AssignPersonnel",StringComparer.Ordinal) && !selected.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assignment or cancellation reference permission is required.");return selected;
    }
    public async Task AssignPersonnelAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderPersonnelSourceReview review,string duty,DateTime from,DateTime to,string reason,Func<CompanyWorkOrderPersonnelAssignmentReference,Task> retainBeforeTransport)
    {
        ArgumentNullException.ThrowIfNull(retainBeforeTransport);if(!CanAssignPersonnel)throw new InvalidOperationException("Recover the original retained personnel assignment before replacing any intent.");
        var selected=RequirePersonnelCurrent();review.Require(selected.Scope,selected.UserId,review.Personnel.Query);
        if(review.Personnel.Query.Permit!=proposal || !selected.Roles.Contains(review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original permit task and configured company role are required.");
        var original=new CompanyWorkOrderPersonnelAssignmentCommand(CompanyInventorySourceReview.Copy(review),Guid.NewGuid(),duty,from,to,reason);original.Require();
        personnelCommand=original;personnelReference=new(review.Personnel.Query.Permit,original.IntentId);personnelOrigin=proposal;personnelOwner=selected;personnelReceipt=null;personnelConfirmed=false;
        var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(personnelReference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original personnel context changed before transport.");
            var result=await client.RecordWorkOrderPersonnelAssignmentAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);
            if(!disposed && generation==version && snapshot==company.Snapshot){personnelReceipt=CompanyInventorySourceReview.Copy(result);personnelConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverPersonnelReferenceAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderPersonnelAssignmentReference reference,Func<CompanyWorkOrderPersonnelAssignmentReference,Task>? retainBeforeTransport=null)
    {
        reference.Require();if(Busy || personnelReference is {} retained && retained!=reference)throw new InvalidOperationException("Use the same retained original personnel assignment reference.");
        var selected=RequirePersonnelReferenceCurrent();if(proposal!=reference.Permit || personnelOrigin is not null && !SamePersonnelOrigin)throw new InvalidOperationException("Return to the original personnel maker/company/permit task.");
        personnelReference=reference;personnelOrigin=proposal;personnelOwner=selected;personnelReceipt=null;personnelConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original personnel reference context changed before transport.");
            var result=await client.RecoverWorkOrderPersonnelAssignmentReferenceAsync(reference);result.Require(selected.Scope,selected.UserId,reference);
            if(personnelCommand is not null && System.Text.Json.JsonSerializer.Serialize(personnelCommand)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained personnel assignment body changed.");
            if(!selected.Roles.Contains(result.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original configured company role is required.");
            if(!disposed && generation==version && snapshot==company.Snapshot){personnelCommand=CompanyInventorySourceReview.Copy(result.Command);personnelReceipt=CompanyInventorySourceReview.Copy(result.Recorded);personnelConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
}
