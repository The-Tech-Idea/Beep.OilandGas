using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class WorkOrderExecutionPermitDecisionState
{
    private CompanyWorkOrderPersonnelAcceptanceReference? personnelAcceptanceReference;
    private CompanyWorkOrderPersonnelAcceptanceCommand? personnelAcceptanceCommand;
    private CompanyWorkOrderExecutionPermitReference? personnelAcceptanceOrigin;
    private CompanyClientSelection? personnelAcceptanceOwner;
    private Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? personnelAcceptanceReceipt;
    private bool personnelAcceptanceConfirmed;
    private bool SamePersonnelAcceptanceOrigin=>proposal==personnelAcceptanceOrigin && personnelAcceptanceOwner is {} original && company.Selection is {} current && current.Scope==original.Scope && current.UserId==original.UserId;
    public CompanyWorkOrderPersonnelAcceptanceCommand? VisiblePersonnelAcceptanceCommand=>SamePersonnelAcceptanceOrigin && personnelAcceptanceCommand is not null?CompanyInventorySourceReview.Copy(personnelAcceptanceCommand):null;
    public Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? PersonnelAcceptanceReceipt=>SamePersonnelAcceptanceOrigin && personnelAcceptanceReceipt is not null?CompanyInventorySourceReview.Copy(personnelAcceptanceReceipt):null;
    public CompanyWorkOrderPersonnelAcceptanceReference? VisiblePersonnelAcceptanceReference=>SamePersonnelAcceptanceOrigin?personnelAcceptanceReference:null;
    public bool CanAcceptPersonnel=>!Busy && personnelAcceptanceCommand is null && personnelAcceptanceReference is null;
    private void ClearPersonnelAcceptanceDisplay(){personnelAcceptanceReceipt=null;if(personnelAcceptanceConfirmed){personnelAcceptanceReference=null;personnelAcceptanceCommand=null;personnelAcceptanceOrigin=null;personnelAcceptanceOwner=null;personnelAcceptanceConfirmed=false;}}
    private CompanyClientSelection RequirePersonnelAcceptanceCurrent()
    {
        return RequireCurrent();
    }
    public async Task AcceptPersonnelAsync(ICompanyMaintenanceHandoffClient client,CompanyPersonnelPolicyEvidenceReview review,string inspectionId,string reason,Func<CompanyWorkOrderPersonnelAcceptanceCommand,Task> retainBeforeTransport)
    {
        if(!CanAcceptPersonnel)throw new InvalidOperationException("Recover the original retained personnel acceptance before replacing any intent.");ArgumentNullException.ThrowIfNull(retainBeforeTransport);var selected=RequirePersonnelAcceptanceCurrent();if(review.Assignment.Query.Permit!=proposal)throw new InvalidOperationException("Use the original attached permit task for personnel acceptance.");review.Require(selected.Scope,selected.UserId,review.Assignment.Query);if(!selected.Roles.Contains(review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured assigned company role is required.");
        if(review.EligibilityEvidenceVersion!=1)throw new InvalidOperationException("Obtain a current explicit employment eligibility review before retaining a new personnel acceptance intent.");
        var original=new CompanyWorkOrderPersonnelAcceptanceCommand(CompanyInventorySourceReview.Copy(review),inspectionId,Guid.NewGuid(),reason){RequirementReferenceVersion=review.Assignment.CurrentPersonnel.PermitReview.Proposal.RequirementEvidenceVersion is 1 or 2?1:0};original.Require();personnelAcceptanceCommand=original;personnelAcceptanceReference=new(original.Review.Assignment.Query.Permit,original.InspectionId,original.IntentId);personnelAcceptanceOrigin=proposal;personnelAcceptanceOwner=selected;personnelAcceptanceReceipt=null;personnelAcceptanceConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(CompanyInventorySourceReview.Copy(original));if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original personnel acceptance context changed before transport.");
            var result=await client.RecordWorkOrderPersonnelAcceptanceAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){personnelAcceptanceReceipt=CompanyInventorySourceReview.Copy(result);personnelAcceptanceConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverPersonnelAcceptanceAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || personnelAcceptanceCommand is null || !SamePersonnelAcceptanceOrigin)throw new InvalidOperationException("Return to the same original personnel reviewer/company/permit task for recovery.");RequirePersonnelAcceptanceCurrent();var original=CompanyInventorySourceReview.Copy(personnelAcceptanceCommand);var generation=version;var snapshot=company.Snapshot;personnelAcceptanceReceipt=null;Busy=true;Changed?.Invoke();
        try{var result=await client.RecoverWorkOrderPersonnelAcceptanceAsync(original);original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){personnelAcceptanceReceipt=CompanyInventorySourceReview.Copy(result);personnelAcceptanceConfirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverPersonnelAcceptanceReferenceAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderPersonnelAcceptanceReference reference,Func<CompanyWorkOrderPersonnelAcceptanceReference,Task>? retainBeforeTransport=null)
    {
        reference.Require();if(Busy || personnelAcceptanceReference is {} retained && retained!=reference)throw new InvalidOperationException("Recover the same retained original personnel acceptance reference.");var selected=RequirePersonnelAcceptanceCurrent();if(proposal!=reference.Permit || personnelAcceptanceOrigin is not null && !SamePersonnelAcceptanceOrigin)throw new InvalidOperationException("Return to the original personnel reviewer/company/permit task.");
        personnelAcceptanceReference=reference;personnelAcceptanceOrigin=proposal;personnelAcceptanceOwner=selected;personnelAcceptanceReceipt=null;personnelAcceptanceConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original private personnel acceptance reference changed before transport.");
            var result=await client.RecoverWorkOrderPersonnelAcceptanceReferenceAsync(reference);result.Require(selected.Scope,selected.UserId,reference);if(personnelAcceptanceCommand is not null && System.Text.Json.JsonSerializer.Serialize(personnelAcceptanceCommand)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained personnel acceptance body changed during reference recovery.");if(!selected.Roles.Contains(result.Command.Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assigned company role is required.");
            if(!disposed && generation==version && snapshot==company.Snapshot){personnelAcceptanceCommand=CompanyInventorySourceReview.Copy(result.Command);personnelAcceptanceReceipt=CompanyInventorySourceReview.Copy(result.Recorded);personnelAcceptanceConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
}
