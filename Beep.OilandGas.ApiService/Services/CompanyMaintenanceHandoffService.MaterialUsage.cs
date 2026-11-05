using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;

public sealed partial class CompanyMaintenanceHandoffService
{
    private async Task<CompanyWorkOrderMaterialUseExecutionProof?> ReadMaterialUseProofAsync(ClaimsPrincipal user,
        CompanyWorkOrderExecutionStartReference reference,string productionConnection,bool ownedProduction)
    {
        reference.Require();var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        if(executionPermits is null)throw new InvalidOperationException("The native original work-start owner is unavailable.");
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)
                throw new UnauthorizedAccessException("Original executor/company/FIELD/order authority changed around physical material use.");
        }
        return await executionPermits.ReadMaterialUseExecutionProofAsync(context,reference,productionConnection,ownedProduction,Guard,
            owned=>ReadNativeExecutionRequirementsCurrentAsync(context,reference.Permit,owned,Guard));
    }

    private static void RequireMaterialUseProof(CompanyWorkOrderMaterialUsageReview review,CompanyWorkOrderMaterialUseExecutionProof proof)
    {
        var source=review.Material.Source;var reference=new CompanyWorkOrderExecutionStartReference(review.Start.Permit,review.Start.IntentId);
        proof.Require(review.Start.Company,review.Start.ExecutorUserId,reference);
        if(proof.Inspections.Start!=review.Start || proof.Reservation!=review.Reservation || proof.Inspections.Current.Approval.ValidToUtc!=review.ValidToUtc ||
            CompanyWorkOrderExecutionRequirements.OrderRevision(proof.Inspections.Current.Order)!=CompanyWorkOrderExecutionRequirements.OrderRevision(source.WorkOrder.Order) ||
            JsonSerializer.Serialize(proof.Inspections.Current.Requirements.OrderBy(row=>row.CondSeq))!=JsonSerializer.Serialize(source.WorkOrder.Inspections.OrderBy(row=>row.CondSeq)))
            throw new InvalidOperationException("Original approved start/reservation/order/current requirement population changed before physical material use.");
    }

    public async Task<CompanyWorkOrderMaterialUsageReview> ReviewMaterialUsageAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionStartReference reference)
    {
        reference.Require();
        var boundary=await Boundary(user,reference.Permit.Task.WorkOrderId,reference.Permit.Task.FieldId);
        _=await PermitAuthority(user,boundary.Preference,reference.Permit.Task.WorkOrderId,"Maintenance.ExecuteWork");
        var connection=await modules.ResolveAsync("PRODUCTION");
        var proof=await ReadMaterialUseProofAsync(user,reference,connection,false)??throw new InvalidOperationException("Current original execution proof is unavailable.");
        if(inventory is null)throw new InvalidOperationException("The existing native inventory owner is unavailable.");
        var stage=await ReviewAssignedStageAsync(user,reference.Permit.Task);
        if(!user.IsInRole(proof.Inspections.Start.AssignedRole) || stage.AssignedRole!=proof.Inspections.Start.AssignedRole || stage.StepId!=proof.Inspections.Start.StepId)
            throw new UnauthorizedAccessException("Physical material use requires the actual executor's original current configured duty.");
        var current=proof.Inspections.Current;
        var order=new CompanyMaintenanceInspectionReview(stage,current.Context.Database,current.Order,current.Requirements,
            CompanyMaintenanceInspectionReview.Revision(stage,current.Context.Database,current.Order,current.Requirements),DateTimeOffset.UtcNow);
        var reservation=proof.Reservation;
        var source=await ReviewMaterialSourceCoreAsync(user,new(reference.Permit.Task,reservation.ItemId,reservation.Quantity,reservation.Unit),true,order);
        async Task Authority()=>await RequireMaterialAllocationAuthority(user,source,"Maintenance.ExecuteWork");
        await Authority();var material=await inventory.ReviewWorkOrderMaterialAllocationsAsync(source,Authority);
        var result=new CompanyWorkOrderMaterialUsageReview(proof.Inspections.Start,reservation,material,current.Approval.ValidToUtc,DateTime.UtcNow);
        result.Require();RequireMaterialUseProof(result,proof);
        var closing=await ReadMaterialUseProofAsync(user,reference,connection,false)??throw new InvalidOperationException("Original closing execution proof is unavailable.");
        RequireMaterialUseProof(result,closing);var closingStage=await ReviewAssignedStageAsync(user,reference.Permit.Task);
        if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(closingStage))throw new InvalidOperationException("Original assigned duty changed around material usage review.");
        await Authority();return CompanyInventorySourceReview.Copy(result);
    }

    public async Task<CompanyWorkOrderMaterialUsageRecovery> RecordMaterialUsageAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialUsageCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();
        var review=command.Review;var source=review.Material.Source;var reference=new CompanyWorkOrderExecutionStartReference(review.Start.Permit,review.Start.IntentId);
        async Task Authority()
        {
            await RequireMaterialAllocationAuthority(user,source,"Maintenance.ExecuteWork");
            if(!user.IsInRole(review.Start.AssignedRole))throw new UnauthorizedAccessException("The original configured execution role is required for material use.");
        }
        async Task Execution(bool owned)
        {
            await Authority();
            var proof=await ReadMaterialUseProofAsync(user,reference,source.Context.Database.ConnectionName,owned);
            if(proof is not null)RequireMaterialUseProof(review,proof);
            else if(!owned)throw new InvalidOperationException("Current original execution proof is unavailable.");
            // Separate lifecycle/CORE owners close outside PRODUCTION. A later change leaves the original intent recoverable.
            if(!owned)
            {
                var stage=await ReviewAssignedStageAsync(user,reference.Permit.Task);
                if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(source.WorkOrder.Stage))
                    throw new InvalidOperationException("Original assigned duty changed around physical stock use.");
            }
            await Authority();
        }
        await Authority();if(inventory is null)throw new InvalidOperationException("The existing native physical stock owner is unavailable.");
        return await inventory.RecordWorkOrderMaterialUsageAsync(command,Authority,Execution);
    }

    public async Task<CompanyWorkOrderMaterialUsageRecovery> RecoverMaterialUsageAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialUsageReference reference)
    {
        reference.Require();
        var task=reference.Start.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var core=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        var connection=await modules.ResolveAsync("PRODUCTION");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection));
        var context=core with{Database=database};
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=core)
                throw new UnauthorizedAccessException("Current original usage recovery authority changed.");
            var access=await company.RefreshCompanyAsync();
            if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal))
                throw new UnauthorizedAccessException("Original company/account and inventory evidence authority is required for private usage recovery.");
            var current=await modules.ResolveAsync("PRODUCTION");
            if(new AssetDatabaseScope(current,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],current))!=database)
                throw new InvalidOperationException("Original usage recovery PRODUCTION binding changed.");
        }
        await Guard();if(inventory is null)throw new InvalidOperationException("The existing native usage recovery owner is unavailable.");
        var result=await inventory.RecoverWorkOrderMaterialUsageAsync(context,reference,Guard);
        result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
}
