using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class WorkOrderExecutionPermitDecisionState
{
    private CompanyWorkOrderDocumentAcceptanceReference? documentAcceptanceReference;
    private CompanyWorkOrderDocumentAcceptanceCommand? documentAcceptanceCommand;
    private CompanyWorkOrderExecutionPermitReference? documentAcceptanceOrigin;
    private CompanyClientSelection? documentAcceptanceOwner;
    private Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? documentAcceptanceReceipt;
    private bool documentAcceptanceConfirmed;
    private bool SameDocumentAcceptanceOrigin=>proposal==documentAcceptanceOrigin && documentAcceptanceOwner is {} original && company.Selection is {} current && current.Scope==original.Scope && current.UserId==original.UserId;
    public CompanyWorkOrderDocumentAcceptanceCommand? VisibleDocumentAcceptanceCommand=>SameDocumentAcceptanceOrigin && documentAcceptanceCommand is not null?CompanyInventorySourceReview.Copy(documentAcceptanceCommand):null;
    public Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? DocumentAcceptanceReceipt=>SameDocumentAcceptanceOrigin && documentAcceptanceReceipt is not null?CompanyInventorySourceReview.Copy(documentAcceptanceReceipt):null;
    public CompanyWorkOrderDocumentAcceptanceReference? VisibleDocumentAcceptanceReference=>SameDocumentAcceptanceOrigin?documentAcceptanceReference:null;
    public bool CanAcceptDocument=>!Busy && documentAcceptanceCommand is null && documentAcceptanceReference is null;
    public void BeginNextDocumentAcceptance()
    {
        if(Busy || !SameDocumentAcceptanceOrigin || !documentAcceptanceConfirmed || documentAcceptanceCommand is null || documentAcceptanceReceipt is null)throw new InvalidOperationException("Confirm or recover the original document acceptance before beginning another document requirement.");RequireDocumentAcceptanceCurrent();documentAcceptanceCommand.RequireRecorded(documentAcceptanceReceipt);documentAcceptanceReceipt=null;documentAcceptanceReference=null;documentAcceptanceCommand=null;documentAcceptanceOrigin=null;documentAcceptanceOwner=null;documentAcceptanceConfirmed=false;Changed?.Invoke();
    }
    private void ClearDocumentAcceptanceDisplay(){documentAcceptanceReceipt=null;if(documentAcceptanceConfirmed){documentAcceptanceReference=null;documentAcceptanceCommand=null;documentAcceptanceOrigin=null;documentAcceptanceOwner=null;documentAcceptanceConfirmed=false;}}
    private CompanyClientSelection RequireDocumentAcceptanceCurrent()
    {
        return RequireCurrent();
    }
    public async Task AcceptDocumentAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderPermitDocumentReview review,string inspectionId,string reason,bool contentReviewed,bool requirementVerified,Func<CompanyWorkOrderDocumentAcceptanceCommand,Task> retainBeforeTransport,CompanyWorkOrderDocumentContributorReview? contributors=null)
    {
        if(!CanAcceptDocument)throw new InvalidOperationException("Recover the original retained document acceptance before replacing any intent.");ArgumentNullException.ThrowIfNull(retainBeforeTransport);var selected=RequireDocumentAcceptanceCurrent();if(review.Source.Query.Permit!=proposal)throw new InvalidOperationException("Use the original attached permit task for document acceptance.");review.Require(selected.Scope,selected.UserId,review.Source.Query);if(!selected.Roles.Contains(review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured assigned company role is required.");
        var original=new CompanyWorkOrderDocumentAcceptanceCommand(CompanyInventorySourceReview.Copy(review),inspectionId,Guid.NewGuid(),reason,contentReviewed,requirementVerified){ContributorReview=contributors is null?null:CompanyInventorySourceReview.Copy(contributors)};original.RequireCurrentAcceptance();documentAcceptanceCommand=original;documentAcceptanceReference=new(original.Review.Source.Query.Permit,original.InspectionId,original.IntentId);documentAcceptanceOrigin=proposal;documentAcceptanceOwner=selected;documentAcceptanceReceipt=null;documentAcceptanceConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(CompanyInventorySourceReview.Copy(original));if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original document acceptance context changed before transport.");
            var result=await client.RecordWorkOrderDocumentAcceptanceAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){documentAcceptanceReceipt=CompanyInventorySourceReview.Copy(result);documentAcceptanceConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverDocumentAcceptanceAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || documentAcceptanceCommand is null || !SameDocumentAcceptanceOrigin)throw new InvalidOperationException("Return to the same original document reviewer/company/permit task for recovery.");RequireDocumentAcceptanceCurrent();var original=CompanyInventorySourceReview.Copy(documentAcceptanceCommand);var generation=version;var snapshot=company.Snapshot;documentAcceptanceReceipt=null;Busy=true;Changed?.Invoke();
        try{var result=await client.RecoverWorkOrderDocumentAcceptanceAsync(original);original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){documentAcceptanceReceipt=CompanyInventorySourceReview.Copy(result);documentAcceptanceConfirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverDocumentAcceptanceReferenceAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderDocumentAcceptanceReference reference,Func<CompanyWorkOrderDocumentAcceptanceReference,Task>? retainBeforeTransport=null)
    {
        reference.Require();if(Busy || documentAcceptanceReference is {} retained && retained!=reference)throw new InvalidOperationException("Recover the same retained original document acceptance reference.");var selected=RequireDocumentAcceptanceCurrent();if(proposal!=reference.Permit || documentAcceptanceOrigin is not null && !SameDocumentAcceptanceOrigin)throw new InvalidOperationException("Return to the original document reviewer/company/permit task.");
        documentAcceptanceReference=reference;documentAcceptanceOrigin=proposal;documentAcceptanceOwner=selected;documentAcceptanceReceipt=null;documentAcceptanceConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original private document acceptance reference changed before transport.");
            var result=await client.RecoverWorkOrderDocumentAcceptanceReferenceAsync(reference);result.Require(selected.Scope,selected.UserId,reference);if(documentAcceptanceCommand is not null && System.Text.Json.JsonSerializer.Serialize(documentAcceptanceCommand)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained document acceptance body changed during reference recovery.");if(!selected.Roles.Contains(result.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assigned company role is required.");
            if(!disposed && generation==version && snapshot==company.Snapshot){documentAcceptanceCommand=CompanyInventorySourceReview.Copy(result.Command);documentAcceptanceReceipt=CompanyInventorySourceReview.Copy(result.Recorded);documentAcceptanceConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
}
