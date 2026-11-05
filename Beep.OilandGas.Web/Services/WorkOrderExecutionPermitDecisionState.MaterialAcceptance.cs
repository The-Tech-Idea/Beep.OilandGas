using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class WorkOrderExecutionPermitDecisionState
{
    private CompanyWorkOrderMaterialAcceptanceReference? materialReference;
    private CompanyWorkOrderMaterialAcceptanceCommand? materialCommand;
    private CompanyWorkOrderExecutionPermitReference? materialOrigin;
    private CompanyClientSelection? materialOwner;
    private Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? materialReceipt;
    private bool materialConfirmed;
    private bool SameMaterialOrigin=>proposal==materialOrigin && materialOwner is {} original && company.Selection is {} current && current.Scope==original.Scope && current.UserId==original.UserId;
    public CompanyWorkOrderMaterialAcceptanceCommand? VisibleMaterialCommand=>SameMaterialOrigin && materialCommand is not null?CompanyInventorySourceReview.Copy(materialCommand):null;
    public Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? MaterialReceipt=>SameMaterialOrigin && materialReceipt is not null?CompanyInventorySourceReview.Copy(materialReceipt):null;
    public CompanyWorkOrderMaterialAcceptanceReference? VisibleMaterialReference=>SameMaterialOrigin?materialReference:null;
    public bool CanAcceptMaterial=>!Busy && materialCommand is null && materialReference is null;
    private void ClearMaterialDisplay(){materialReceipt=null;if(materialConfirmed){materialReference=null;materialCommand=null;materialOrigin=null;materialOwner=null;materialConfirmed=false;}}
    private CompanyClientSelection RequireMaterialCurrent()
    {
        var selected=RequireCurrent();if(!selected.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current material inventory view permission is required.");return selected;
    }
    public async Task AcceptMaterialAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderMaterialReadinessReview review,string inspectionId,string reason,Func<CompanyWorkOrderMaterialAcceptanceCommand,Task> retainBeforeTransport)
    {
        if(!CanAcceptMaterial)throw new InvalidOperationException("Recover the original retained material acceptance before replacing any intent.");ArgumentNullException.ThrowIfNull(retainBeforeTransport);var selected=RequireMaterialCurrent();if(review.Query.Permit!=proposal)throw new InvalidOperationException("Use the original attached permit task for material acceptance.");review.Require(selected.Scope,selected.UserId,review.Query);if(!selected.Roles.Contains(review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("The current configured assigned company role is required.");
        var original=new CompanyWorkOrderMaterialAcceptanceCommand(CompanyInventorySourceReview.Copy(review),inspectionId,Guid.NewGuid(),reason);original.Require();materialCommand=original;materialReference=new(original.Review.Query.Permit,original.InspectionId,original.IntentId);materialOrigin=proposal;materialOwner=selected;materialReceipt=null;materialConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            await retainBeforeTransport(CompanyInventorySourceReview.Copy(original));if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original material context changed before transport.");
            var result=await client.RecordWorkOrderMaterialAcceptanceAsync(CompanyInventorySourceReview.Copy(original));original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){materialReceipt=CompanyInventorySourceReview.Copy(result);materialConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverMaterialAsync(ICompanyMaintenanceHandoffClient client)
    {
        if(Busy || materialCommand is null || !SameMaterialOrigin)throw new InvalidOperationException("Return to the same original material reviewer/company/permit task for recovery.");RequireMaterialCurrent();var original=CompanyInventorySourceReview.Copy(materialCommand);var generation=version;var snapshot=company.Snapshot;materialReceipt=null;Busy=true;Changed?.Invoke();
        try{var result=await client.RecoverWorkOrderMaterialAcceptanceAsync(original);original.RequireRecorded(result);if(!disposed && generation==version && snapshot==company.Snapshot){materialReceipt=CompanyInventorySourceReview.Copy(result);materialConfirmed=true;}}
        finally{Busy=false;Changed?.Invoke();}
    }
    public async Task RecoverMaterialReferenceAsync(ICompanyMaintenanceHandoffClient client,CompanyWorkOrderMaterialAcceptanceReference reference,Func<CompanyWorkOrderMaterialAcceptanceReference,Task>? retainBeforeTransport=null)
    {
        reference.Require();if(Busy || materialReference is {} retained && retained!=reference)throw new InvalidOperationException("Recover the same retained original material reference.");var selected=RequireMaterialCurrent();if(proposal!=reference.Permit || materialOrigin is not null && !SameMaterialOrigin)throw new InvalidOperationException("Return to the original material reviewer/company/permit task.");
        materialReference=reference;materialOrigin=proposal;materialOwner=selected;materialReceipt=null;materialConfirmed=false;var generation=version;var snapshot=company.Snapshot;Busy=true;Changed?.Invoke();
        try
        {
            if(retainBeforeTransport is not null)await retainBeforeTransport(reference);if(disposed || generation!=version || snapshot!=company.Snapshot)throw new OperationCanceledException("Original private material reference changed before transport.");
            var result=await client.RecoverWorkOrderMaterialAcceptanceReferenceAsync(reference);result.Require(selected.Scope,selected.UserId,reference);if(materialCommand is not null && System.Text.Json.JsonSerializer.Serialize(materialCommand)!=System.Text.Json.JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Original retained material acceptance body changed during reference recovery.");if(!selected.Roles.Contains(result.Command.Review.PermitReview.CurrentSource.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original assigned company role is required.");
            if(!disposed && generation==version && snapshot==company.Snapshot){materialCommand=CompanyInventorySourceReview.Copy(result.Command);materialReceipt=CompanyInventorySourceReview.Copy(result.Recorded);materialConfirmed=true;}
        }
        finally{Busy=false;Changed?.Invoke();}
    }
}
