using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.ProductionOperations.Models;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.LifeCycle.Services.Production;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Processes;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Coordinates the existing production, app preference/grant and fixed lifecycle owners.</summary>
public sealed partial class CompanyMaintenanceHandoffService(CompanyModuleConnection company,ModuleConnectionResolver modules,
    ICompanyFieldSelectionService selection,ICompanyFieldSelectionAuthority preferences,IUserAssetAccessStore grants,
    IProcessService processes,PPDMProductionService production,IFieldOrchestrator field,IWorkOrderService? workOrders=null,IInspectionService? inspections=null,
    Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder.WorkOrderExecutionPermitService? executionPermits=null,
    Beep.OilandGas.Accounting.Services.InventoryService? inventory=null,IContractorManagementService? contractors=null,ICompanyMaintenanceWellEvidenceOwner? maintenanceWells=null,ICompanyMaintenanceQualifiedAssetEvidenceOwner? qualifiedAssets=null):ICompanyMaterialReservationQueueReader
{
    internal static readonly System.Diagnostics.ActivitySource PermitProofActivities=new("Beep.OilandGas.WorkOrderPermitDecision");
    public async Task<CompanyWorkOrderSubmissionReview> ReviewSubmissionAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionStartReference reference)
    {
        var inspection=await ReviewSubmissionInspectionsAsync(user,reference);var stage=await ReviewAssignedStageAsync(user,reference.Permit.Task);
        var result=new CompanyWorkOrderSubmissionReview(inspection,stage);result.Require();
        var closing=await ReviewAssignedStageAsync(user,reference.Permit.Task);if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original assigned stage changed around submission review.");return result;
    }
    public async Task<CompanyWorkOrderSubmissionReceipt> SubmitWorkAsync(ClaimsPrincipal user,CompanyWorkOrderSubmissionCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var comparison=command.Review.Inspections;var task=comparison.Start.Permit.Task;
        var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        if(context!=comparison.Current.Context || !user.IsInRole(command.Review.Stage.AssignedRole))throw new UnauthorizedAccessException("Original current executor/company/FIELD/order/configured role is required for submission.");
        if(executionPermits is null)throw new InvalidOperationException("Native work-submission owner is unavailable.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context || !user.IsInRole(command.Review.Stage.AssignedRole))throw new UnauthorizedAccessException("Original execution submission authority changed.");}
        return await executionPermits.SubmitWorkAsync(command,Guard,owned=>ReadExecutionPermitRequirementsAsync(user,comparison.Start.Permit,"Maintenance.ExecuteWork",owned),()=>ReviewAssignedStageAsync(user,task));
    }
    public async Task<CompanyWorkOrderSubmissionRecovery> RecoverSubmissionAsync(ClaimsPrincipal user,CompanyWorkOrderSubmissionReference reference)
    {
        reference.Require();var task=reference.Start.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        if(executionPermits is null)throw new InvalidOperationException("Original native submission recovery owner is unavailable.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)throw new UnauthorizedAccessException("Current original submission recovery authority changed.");}
        var result=await executionPermits.RecoverSubmissionAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
    public async Task<CompanyWorkOrderSubmissionInspectionReview> ReviewSubmissionInspectionsAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionStartReference reference)
    {
        reference.Require();var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        if(executionPermits is null)throw new InvalidOperationException("Original native work-start owner is unavailable.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)throw new UnauthorizedAccessException("Current original executor/company/FIELD/order inspection authority changed.");}
        var stage=await ReviewAssignedStageAsync(user,task);
        var result=await executionPermits.ReviewSubmissionInspectionsAsync(context,reference,Guard,()=>ReadExecutionPermitRequirementsAsync(user,reference.Permit,"Maintenance.ExecuteWork"));
        var closing=await ReviewAssignedStageAsync(user,task);
        if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(closing) || result.Start.AssignedRole!=stage.AssignedRole || result.Start.StepId!=stage.StepId)throw new InvalidOperationException("Original execution stage/role changed during submission inspection review.");
        result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
    public async Task<CompanyWorkOrderRecordedStart?> ReadRecordedExecutionStartAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();var task=query.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ViewApprovedPermit");
        if(executionPermits is null)throw new InvalidOperationException("The original recorded work-start owner is unavailable.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ViewApprovedPermit")!=context)throw new UnauthorizedAccessException("Current company/persona/FIELD/order recorded-work view authority changed.");}
        var result=await executionPermits.ReadRecordedStartAsync(context,query,Guard);result?.Require(context.Company,query);await Guard();return result;
    }
    public async Task<CompanyWorkOrderExecutionPrerequisites> ReadExecutionPrerequisitesAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
        =>(await ReadOrStartExecutionAsync(user,query,null)).Review!;
    public async Task<CompanyWorkOrderExecutionStartReceipt> StartExecutionAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionStartCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();
        return(await ReadOrStartExecutionAsync(user,command.Review.Prerequisites.Requirements.Approval.Permit,command)).Receipt!;
    }
    public async Task<CompanyWorkOrderExecutorIdentity> ReviewExecutionStartAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
    {
        var proof=await ReadExecutionPrerequisitesAsync(user,query);var context=proof.Requirements.Context;
        if(contractors is not ICompanyWorkOrderExecutorIdentityOwner owner)throw new InvalidOperationException("The native actual-executor identity owner is unavailable.");
        var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.ExecuteWork")!=context)throw new UnauthorizedAccessException("The actual executor/company/FIELD/order authority changed.");}
        var stage=await ReviewAssignedStageAsync(user,query.Task);var result=await owner.ReadExecutorIdentityAsync(proof,stage,Guard);var closingStage=await ReviewAssignedStageAsync(user,query.Task);
        if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(closingStage))throw new InvalidOperationException("Current assigned lifecycle step/role changed around executor identity review.");
        if(CompanyMaintenanceQualifiedWorkScope.HasTargets(proof.Requirements.Order))
        {
            if(qualifiedAssets is not ICompanyMaintenanceQualifiedWorkAssetOwner workAssets)throw new InvalidOperationException("The native original qualified work-scope owner is unavailable.");
            var assets=await workAssets.ReadMaintenanceQualifiedWorkAssetsAsync(context,proof.Requirements.Order,Guard,false);
            result=result with{QualifiedWorkAssets=assets,ComparedAtUtc=DateTime.UtcNow,Revision=""};result=result with{Revision=CompanyWorkOrderExecutorIdentity.Digest(result)};
            var finalStage=await ReviewAssignedStageAsync(user,query.Task);if(JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(finalStage))throw new InvalidOperationException("Original assigned executor role changed around qualified work-scope review.");
        }
        result.Require();await Guard();return result;
    }
    public async Task<CompanyWorkOrderExecutionStartRecovery> RecoverExecutionStartAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionStartReference reference)
    {
        reference.Require();var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        if(executionPermits is null)throw new InvalidOperationException("The native original work-start owner is unavailable.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)throw new UnauthorizedAccessException("Current original executor/company/FIELD/order recovery authority changed.");}
        var result=await executionPermits.RecoverExecutionStartAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
    private async Task<(CompanyWorkOrderExecutionPrerequisites? Review,CompanyWorkOrderExecutionStartReceipt? Receipt)> ReadOrStartExecutionAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query,CompanyWorkOrderExecutionStartCommand? start)
    {
        query.Require();
        var task=query.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork");
        var initial=await company.RefreshCompanyAsync();if(!initial.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("The current executor also requires Inventory.View.");
        if(executionPermits is null || workOrders is null || inventory is null || inspections is not ICompanyWorkOrderRequirementSource requirements || inspections is not ICompanyWorkOrderDocumentContributorCore documents || contractors is not ICompanyPersonnelAcceptanceCoreProof personnel || contractors is not ICompanyRecordedReviewerIdentityCore identities)
            throw new InvalidOperationException("Complete existing native execution prerequisite owners are required.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ExecuteWork")!=context)throw new UnauthorizedAccessException("Current executor/company/standard role/FIELD/order/core authority changed.");}
        if(start is not null && start.Review.Prerequisites.Requirements.Context!=context)throw new UnauthorizedAccessException("Return to the original actual executor and reviewed work-start authority.");
        var connection=await modules.ResolveAsync("PRODUCTION");var stockDatabase=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection));
        var stockContext=context with{Database=stockDatabase,Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{context,Stock=stockDatabase,Permission="Inventory.View"}))};
        async Task StockAuthority()
        {
            await Guard();var access=await company.RefreshCompanyAsync();
            if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("The current executor also requires Inventory.View.");
            var current=await modules.ResolveAsync("PRODUCTION");if(new AssetDatabaseScope(current,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],current))!=stockDatabase)throw new InvalidOperationException("Original execution stock binding changed.");await Guard();
        }
        async Task<(Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel Order,IReadOnlyList<Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition> Requirements)> Read(bool owned)
        {
            await Guard();var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original current execution order is unavailable.");
            var rows=owned?await requirements.ReadRequirementProofCoreAsync(context,task.WorkOrderId,Guard):await requirements.ReadRequirementProofAsync(context,task.WorkOrderId,Guard);
            var closing=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original execution order changed around requirements.");await Guard();
            if(CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=CompanyWorkOrderExecutionRequirements.OrderRevision(closing))throw new InvalidOperationException("Original execution order scope/components changed.");return(closing,rows);
        }
        await StockAuthority();
        if(start is not null)
        {
            if(contractors is not ICompanyWorkOrderExecutorIdentityOwner executorIdentities)throw new InvalidOperationException("The native executor/assigned-person identity owner is unavailable.");
            var receipt=await executionPermits.StartExecutionAsync(start,Guard,Read,personnel,documents,identities,executorIdentities,
                review=>inventory.RequireExecutionMaterialCurrentProofAsync(stockContext,review.Proposal,review.CurrentMaterialAcceptance!,StockAuthority),
                review=>inventory.RequireExecutionMaterialOwnedCoreProofAsync(stockContext,review.Proposal,review.CurrentMaterialAcceptance!,StockAuthority,context.Database.ConnectionName),()=>ReviewAssignedStageAsync(user,query.Task),
                async owned=>
                {
                    if(qualifiedAssets is not ICompanyMaintenanceQualifiedWorkAssetOwner workAssets)throw new InvalidOperationException("The native original qualified work-scope owner is unavailable.");
                    return await workAssets.ReadMaintenanceQualifiedWorkAssetsAsync(context,start.Review.Prerequisites.Requirements.Order,Guard,owned);
                });
            receipt.Require(start);await StockAuthority();return(null,receipt);
        }
        var result=await executionPermits.ReadExecutionPrerequisitesAsync(context,query,Guard,Read,personnel,documents,identities,
            review=>inventory.RequireExecutionMaterialCurrentProofAsync(stockContext,review.Proposal,review.CurrentMaterialAcceptance!,StockAuthority),
            review=>inventory.RequireExecutionMaterialOwnedCoreProofAsync(stockContext,review.Proposal,review.CurrentMaterialAcceptance!,StockAuthority,context.Database.ConnectionName));
        result.Require(context.Company,context.ActorUserId,query);await StockAuthority();return(result,null);
    }
    public async Task<CompanyWorkOrderRecordedApproval> ReadRecordedExecutionPermitApprovalAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();if(executionPermits is null)throw new InvalidOperationException("The recorded permit owner is unavailable.");var task=query.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ViewApprovedPermit");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ViewApprovedPermit")!=context)throw new UnauthorizedAccessException("Current company actor, configured roles, FIELD/order and approved-permit capability changed.");}
        await Guard();var result=await executionPermits.ReadRecordedApprovalAsync(context,query,Guard);result.Require(context.Company,query);await Guard();return result;
    }
    public Task<CompanyWorkOrderExecutionRequirements> ReadExecutionPermitRequirementsAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
        =>ReadExecutionPermitRequirementsAsync(user,query,"Maintenance.ViewApprovedPermit");
    private async Task<CompanyWorkOrderExecutionRequirements> ReadExecutionPermitRequirementsAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query,string permission)
        =>await ReadExecutionPermitRequirementsAsync(user,query,permission,false);
    private async Task<CompanyWorkOrderExecutionRequirements> ReadExecutionPermitRequirementsAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query,string permission,bool owned)
    {
        query.Require();if(executionPermits is null || workOrders is null || inspections is not ICompanyWorkOrderRequirementSource requirements)throw new InvalidOperationException("Existing approved permit/order/immutable requirement owners are required.");
        var task=query.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,permission);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,permission)!=context)throw new UnauthorizedAccessException("Current original execution-view company/actor/role/FIELD/order/database authority changed.");}
        async Task<(Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel Order,IReadOnlyList<Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition> Requirements)> ReadCurrent()
            =>await ReadNativeExecutionRequirementsCurrentAsync(context,query,owned,Guard);
        await Guard();return owned?await executionPermits.ReadExecutionRequirementsCoreAsync(context,query,Guard,ReadCurrent):await executionPermits.ReadExecutionRequirementsAsync(context,query,Guard,ReadCurrent);
    }
    private async Task<(Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel Order,IReadOnlyList<Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition> Requirements)> ReadNativeExecutionRequirementsCurrentAsync(
        FieldSelectionReview context,CompanyWorkOrderExecutionPermitReference query,bool owned,Func<Task> guard)
    {
        if(workOrders is null || inspections is not ICompanyWorkOrderRequirementSource requirements)
            throw new InvalidOperationException("Existing native order and immutable requirement owners are required.");
        var task=query.Task;await guard();var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Current original native order is unavailable.");
        var rows=owned?await requirements.ReadRequirementProofCoreAsync(context,task.WorkOrderId,guard):await requirements.ReadRequirementProofAsync(context,task.WorkOrderId,guard);
        var closing=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Current original native order changed.");await guard();
        if(CompanyWorkOrderExecutionRequirements.OrderRevision(order)!=CompanyWorkOrderExecutionRequirements.OrderRevision(closing))
            throw new InvalidOperationException("Complete original native order changed around requirement reads.");return(closing,rows);
    }
    public async Task<CompanyWorkOrderDocumentContributorReview> ReviewDocumentContributorsAsync(ClaimsPrincipal user,CompanyWorkOrderPermitDocumentQuery query)
    {
        query.Require();if(inspections is not ICompanyWorkOrderDocumentContributorOwner owner || contractors is not ICompanyPersonnelIdentityOwner identities)throw new InvalidOperationException("Existing native contributor/person identity owners are unavailable.");var permit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);var context=permit.Context;
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original document contributor reviewer authority changed.");}
        var identityContext=new CompanyPersonnelPolicyContext(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{context.Revision,Permission="HSE.ApprovePermit",context.Database})));
        var first=await identities.ReadPersonnelIdentitiesAsync(identityContext,Guard);var links=first.Versions.Where(row=>row.UserId==context.ActorUserId && row.EffectiveFromUtc<=DateTime.UtcNow && row.EffectiveToUtc>DateTime.UtcNow).ToArray();CompanyNativePersonFacts? person=null;if(links.Length==1)person=await identities.ReadIdentityPersonAsync(identityContext,links[0].PersonBaId,Guard);
        var evidence=await owner.ReadDocumentContributorsAsync(context,query,query.Kind=="ISOLATION"?permit.Proposal.IsolationReference:permit.Proposal.RiskReference,Guard);var closing=await identities.ReadPersonnelIdentitiesAsync(identityContext,Guard);CompanyNativePersonFacts? closingPerson=null;if(links.Length==1)closingPerson=await identities.ReadIdentityPersonAsync(identityContext,links[0].PersonBaId,Guard);var closingPermit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);await Guard();
        if(first.Revision!=closing.Revision || person!=closingPerson || permit.Context!=closingPermit.Context || permit.RecordedRevision!=closingPermit.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closingPermit.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Complete original document/identity mapping/person/permit/order proof changed.");
        var document=new CompanyWorkOrderPermitDocumentReview(closingPermit,evidence.Source,CompanyWorkOrderPermitDocumentReview.GetBlockers(closingPermit.Proposal,evidence.Source));var utc=DateTime.UtcNow;var result=new CompanyWorkOrderDocumentContributorReview(document,evidence,closing,closingPerson,utc,CompanyWorkOrderDocumentContributorReview.GetBlockers(document,evidence,closing,closingPerson,utc));result.Require();return result;
    }
    public async Task<CompanyWorkOrderPermitDocumentContent> ReadPermitDocumentContentAsync(ClaimsPrincipal user,CompanyWorkOrderPermitDocumentReview original)
    {
        var review=CompanyInventorySourceReview.Copy(original);if(review is null)throw new InvalidOperationException("Original document review is unavailable.");review.Require();var context=review.PermitReview.Context;review.Require(context.Company,context.ActorUserId,review.Source.Query);
        if(inspections is not ICompanyWorkOrderPermitDocumentSourceOwner owner)throw new InvalidOperationException("Existing native document owner is unavailable.");
        var boundary=await Boundary(user,review.Source.Query.Permit.Task.WorkOrderId,review.Source.Query.Permit.Task.FieldId);
        async Task Guard(){if(user.ActingUserId()!=context.ActorUserId || await PermitAuthority(user,boundary.Preference,review.Source.Query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original document reviewer authority changed.");}
        await Guard();var first=await ReviewPermitDocumentSourceAsync(user,review.Source.Query);first.Require(context.Company,context.ActorUserId,review.Source.Query);
        if(CompanyWorkOrderPermitDocumentFacts.CurrentProof(first.Source)!=CompanyWorkOrderPermitDocumentFacts.CurrentProof(review.Source) || first.PermitReview.RecordedRevision!=review.PermitReview.RecordedRevision || first.PermitReview.CurrentSource.SnapshotRevision!=review.PermitReview.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Original permit/document evidence changed before content delivery.");
        var content=await owner.ReadPermitDocumentContentAsync(first,Guard);var closing=await ReviewPermitDocumentSourceAsync(user,review.Source.Query);await Guard();var result=content with{Review=closing};result.Require(review);return result;
    }
    public async Task<CompanyWorkOrderDocumentCatalog> ReviewPermitDocumentCatalogAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();if(inspections is not ICompanyWorkOrderPermitDocumentSourceOwner owner)throw new InvalidOperationException("Existing native work-order document source owner is unavailable.");var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var permit=await ReviewExecutionPermitApprovalCoreAsync(user,query);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"HSE.ApprovePermit")!=permit.Context)throw new UnauthorizedAccessException("Current independent document reviewer authority changed.");}
        var catalog=await owner.ReadPermitDocumentCatalogAsync(permit.Context,query,Guard);var closing=await ReviewExecutionPermitApprovalCoreAsync(user,query);await Guard();
        if(permit.Context!=closing.Context || permit.RecordedRevision!=closing.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closing.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Original permit/order/inspection facts changed during document catalog review.");catalog.Require(permit.Context.Company,permit.Context.ActorUserId,query);return catalog;
    }
    public async Task<CompanyWorkOrderPermitDocumentReview> ReviewPermitDocumentSourceAsync(ClaimsPrincipal user,CompanyWorkOrderPermitDocumentQuery query)
    {
        query.Require();if(inspections is not ICompanyWorkOrderPermitDocumentSourceOwner owner)throw new InvalidOperationException("Existing native work-order document source owner is unavailable.");var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);var permit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=permit.Context)throw new UnauthorizedAccessException("Current independent document reviewer authority changed.");}
        var reference=query.Kind=="ISOLATION"?permit.Proposal.IsolationReference:permit.Proposal.RiskReference;var closing=await owner.ReadPermitDocumentSourceAsync(permit.Context,query,reference,Guard);var closingPermit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);await Guard();
        if(permit.Context!=closingPermit.Context || permit.RecordedRevision!=closingPermit.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closingPermit.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Complete original permit/order/document membership/version/content proof changed during review.");
        var result=new CompanyWorkOrderPermitDocumentReview(closingPermit,closing,CompanyWorkOrderPermitDocumentReview.GetBlockers(closingPermit.Proposal,closing));result.Require(permit.Context.Company,permit.Context.ActorUserId,query);return result;
    }
    public async Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecordDocumentAcceptanceAsync(ClaimsPrincipal user,CompanyWorkOrderDocumentAcceptanceCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();if(inspections is not ICompanyWorkOrderDocumentAcceptanceOwner owner)throw new InvalidOperationException("Existing native document inspection/core proof owners are unavailable.");
        var context=command.Review.PermitReview.Context;var query=command.Review.Source.Query;var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);
        async Task Guard(){if(user.ActingUserId()!=context.ActorUserId || await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original independent document reviewer/company/task/native authority changed.");}
        async Task Proof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? own){using var proofActivity=PermitProofActivities.StartActivity("permit.document.acceptance.current-permit");await Guard();command.RequireCurrentPermit(await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit),own);await Guard();}
        async Task CoreProof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? own)
        {
            using var proofActivity=PermitProofActivities.StartActivity("permit.document.acceptance.owned-source-identity");
            await Guard();if(workOrders is null)throw new InvalidOperationException("Existing local work-order owner is unavailable.");var task=query.Permit.Task;var source=command.Review.PermitReview.CurrentSource;
            var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original local order is unavailable.");var rows=source.Inspections.Any(row=>row.RequirementRevision is not null)?await (inspections as ICompanyWorkOrderRequirementSource??throw new InvalidOperationException("Owned immutable requirement source is unavailable.")).ReadRequirementProofCoreAsync(context,task.WorkOrderId,Guard):await inspections.GetChecklistAsync(task.WorkOrderId,task.FieldId);
            var current=new CompanyMaintenanceInspectionReview(source.Stage,source.InspectionDatabase,order,rows,CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,order,rows),DateTimeOffset.UtcNow);command.RequireCurrentCore(current,own);
            if(command.ContributorReview is {} contributorReview){if(contractors is not ICompanyDocumentReviewerIdentityCore identityCore)throw new InvalidOperationException("Existing owned reviewer identity proof owner is unavailable.");await identityCore.RequireDocumentReviewerIdentityCoreAsync(contributorReview,Guard);}await Guard();
        }
        await Guard();return await owner.RecordDocumentAcceptanceAsync(command,Guard,Proof,CoreProof,readOnly);
    }
    public async Task<CompanyWorkOrderDocumentAcceptanceRecovery> RecoverDocumentAcceptanceReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderDocumentAcceptanceReference reference)
    {
        reference.Require();if(inspections is not ICompanyWorkOrderDocumentAcceptanceOwner owner)throw new InvalidOperationException("Existing native document inspection owner is unavailable.");
        var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original document acceptance reference authority changed.");}
        var result=await owner.RecoverDocumentAcceptanceReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
    public async Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecordPersonnelAcceptanceAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelAcceptanceCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();if(inspections is not ICompanyWorkOrderPersonnelAcceptanceOwner owner || contractors is not ICompanyPersonnelAcceptanceCoreProof core)throw new InvalidOperationException("Existing native personnel inspection/core proof owners are unavailable.");
        var context=command.Review.Assignment.CurrentPersonnel.PermitReview.Context;var query=command.Review.Assignment.Query;var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);
        async Task Guard(){if(user.ActingUserId()!=context.ActorUserId || await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original independent personnel reviewer/company/task/native authority changed.");}
        async Task Proof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? own){await Guard();command.RequireCurrent(await ReviewPersonnelPolicyEvidenceAsync(user,query),own);await Guard();}
        async Task CoreProof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? own)
        {
            await Guard();if(workOrders is null)throw new InvalidOperationException("Existing local work-order owner is unavailable.");var task=query.Permit.Task;var source=command.Review.Assignment.CurrentPersonnel.PermitReview.CurrentSource;
            var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original local order is unavailable.");var rows=source.Inspections.Any(row=>row.RequirementRevision is not null)?await (inspections as ICompanyWorkOrderRequirementSource??throw new InvalidOperationException("Owned immutable requirement source is unavailable.")).ReadRequirementProofCoreAsync(context,task.WorkOrderId,Guard):await inspections.GetChecklistAsync(task.WorkOrderId,task.FieldId);
            var current=new CompanyMaintenanceInspectionReview(source.Stage,source.InspectionDatabase,order,rows,CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,order,rows),DateTimeOffset.UtcNow);command.RequireCurrentCore(current,own);
            await core.RequirePersonnelAcceptanceCoreAsync(command.Review,Guard);await Guard();
        }
        await Guard();return await owner.RecordPersonnelAcceptanceAsync(command,Guard,Proof,CoreProof,readOnly);
    }
    public async Task<CompanyWorkOrderPersonnelAcceptanceRecovery> RecoverPersonnelAcceptanceReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelAcceptanceReference reference)
    {
        reference.Require();if(inspections is not ICompanyWorkOrderPersonnelAcceptanceOwner owner)throw new InvalidOperationException("Existing native personnel inspection owner is unavailable.");
        var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original personnel acceptance reference authority changed.");}
        var result=await owner.RecoverPersonnelAcceptanceReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await Guard();return result;
    }
    public Task<CompanyPersonnelPolicyEvidenceReview> ReviewPersonnelPolicyEvidenceAsync(ClaimsPrincipal user,CompanyWorkOrderAssignedPersonnelQuery query)=>ReviewPersonnelPolicyEvidenceCoreAsync(user,query);
    private async Task<CompanyPersonnelPolicyEvidenceReview> ReviewPersonnelPolicyEvidenceCoreAsync(ClaimsPrincipal user,CompanyWorkOrderAssignedPersonnelQuery query,CompanyWorkOrderExecutionPermitDecision? recordedDecision=null,CompanyWorkOrderExecutionPermitApprovalReview? reviewedPermit=null)
    {
        query.Require();if(contractors is not ICompanyPersonnelPolicyOwner policies || contractors is not ICompanyPersonnelCredentialOwner credentials || contractors is not ICompanyWorkOrderPersonnelAssignmentReads assignments)throw new InvalidOperationException("Existing company personnel roster/policy/credential owners are unavailable.");
        var task=query.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var permit=reviewedPermit is null?await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit,recordedDecision):CompanyInventorySourceReview.Copy(reviewedPermit);permit.Require(boundary.Preference.Company,user.ActingUserId(),query.Permit);var context=permit.Context;
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original personnel policy/source HSE authority changed.");}
        var policyContext=new CompanyPersonnelPolicyContext(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{context.Revision,Permission="HSE.ApprovePermit",context.Database})));
        await Guard();var roster=await assignments.ReadPersonnelAssignmentsAsync(context,query.Permit,Guard);
        var selected=roster.Assignments.SingleOrDefault(row=>row.AssignmentId==query.AssignmentId)??throw new InvalidOperationException("The exact selected assignment is not in the complete current permit roster.");
        var sourceQuery=new CompanyWorkOrderPersonnelSourceQuery(query.Permit,selected.EmployeeBaId,selected.LicenseId);
        var catalog=await policies.ReadPersonnelPoliciesAsync(policyContext,Guard);var first=await credentials.ReadPersonnelCredentialsAsync(context,sourceQuery,Guard);
        var closingRoster=await assignments.ReadPersonnelAssignmentsAsync(context,query.Permit,Guard);var closingCatalog=await policies.ReadPersonnelPoliciesAsync(policyContext,Guard);var closing=await credentials.ReadPersonnelCredentialsAsync(context,sourceQuery,Guard);var closingPermit=reviewedPermit is null?await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit,recordedDecision):permit;await Guard();
        if(roster.Context!=closingRoster.Context || JsonSerializer.Serialize(roster.Assignments)!=JsonSerializer.Serialize(closingRoster.Assignments) || JsonSerializer.Serialize(roster.Releases)!=JsonSerializer.Serialize(closingRoster.Releases) ||
            JsonSerializer.Serialize(catalog.Versions)!=JsonSerializer.Serialize(closingCatalog.Versions) || first.Personnel.Employment!=closing.Personnel.Employment || first.Personnel.License!=closing.Personnel.License || JsonSerializer.Serialize(first.Statuses)!=JsonSerializer.Serialize(closing.Statuses) || JsonSerializer.Serialize(first.Conditions)!=JsonSerializer.Serialize(closing.Conditions) ||
            permit.Context!=closingPermit.Context || permit.RecordedRevision!=closingPermit.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closingPermit.CurrentSource.SnapshotRevision)
            throw new InvalidOperationException("Complete original assignment/policy/member/licence/status/condition/permit/order/binding/authority source changed during policy comparison.");
        var personnel=new CompanyWorkOrderPersonnelSourceReview(closingPermit,closing.Personnel,CompanyWorkOrderPersonnelSourceReview.GetBlockers(closingPermit.Proposal,closing.Personnel));
        var assignment=new CompanyWorkOrderAssignedPersonnelReview(query,closingRoster,personnel,CompanyWorkOrderAssignedPersonnelReview.GetBlockers(selected,personnel,closingRoster.Releases));
        var result=new CompanyPersonnelPolicyEvidenceReview(assignment,closingCatalog,closing,CompanyPersonnelPolicyEvidenceReview.GetBlockers(assignment,closingCatalog,closing,1)){EligibilityEvidenceVersion=1};result.Require(context.Company,context.ActorUserId,query);await Guard();return result;
    }
    public async Task<WORK_ORDER_PERSONNEL_RELEASE> RecordPersonnelReleaseAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelReleaseCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();if(contractors is not ICompanyWorkOrderPersonnelReleaseOwner owner)throw new InvalidOperationException("Existing native personnel cancellation owner is unavailable.");
        var context=command.Original.Command.Review.Personnel.Context;var task=command.Original.Command.Review.Personnel.Query.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        async Task Guard()
        {
            var current=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit");
            var access=await company.RefreshCompanyAsync();
            if(user.ActingUserId()!=context.ActorUserId || current.Company!=context.Company || current.ActorUserId!=context.ActorUserId || current.Database!=context.Database || access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel maker/company/native binding/task and release authority are required.");
        }
        await Guard();return await owner.RecordPersonnelReleaseAsync(command,Guard,readOnly);
    }
    public async Task<CompanyWorkOrderPersonnelReleaseRecovery> RecoverPersonnelReleaseReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelReleaseReference reference)
    {
        reference.Require();if(contractors is not ICompanyWorkOrderPersonnelReleaseOwner owner)throw new InvalidOperationException("Existing native personnel cancellation owner is unavailable.");
        var task=reference.Assignment.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original personnel release-reference authority changed.");
            var access=await company.RefreshCompanyAsync();if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel cancellation permission and standard roles are required.");
        }
        var result=await owner.RecoverPersonnelReleaseReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);await RecordPersonnelReleaseAsync(user,result.Command,true);await Guard();return result;
    }
    public async Task<CompanyWorkOrderPersonnelAssignmentRosterReview> ReviewPersonnelAssignmentsAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();if(contractors is not ICompanyWorkOrderPersonnelAssignmentReads owner)throw new InvalidOperationException("Existing native personnel roster owner is unavailable.");
        var task=query.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var permit=await ReviewExecutionPermitApprovalCoreAsync(user,query);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=permit.Context)throw new UnauthorizedAccessException("Original personnel roster reviewer/company/task authority changed.");}
        var first=await owner.ReadPersonnelAssignmentsAsync(permit.Context,query,Guard);var closingPermit=await ReviewExecutionPermitApprovalCoreAsync(user,query);var closing=await owner.ReadPersonnelAssignmentsAsync(permit.Context,query,Guard);await Guard();
        if(permit.Context!=closingPermit.Context || permit.RecordedRevision!=closingPermit.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closingPermit.CurrentSource.SnapshotRevision ||
            JsonSerializer.Serialize(first.Assignments)!=JsonSerializer.Serialize(closing.Assignments) || JsonSerializer.Serialize(first.Releases)!=JsonSerializer.Serialize(closing.Releases))throw new InvalidOperationException("Complete original permit/order/inspection/assignment roster changed during review.");
        var result=new CompanyWorkOrderPersonnelAssignmentRosterReview(closingPermit,closing);result.Require(permit.Context.Company,permit.Context.ActorUserId,query);return result;
    }
    public async Task<CompanyWorkOrderAssignedPersonnelReview> ReviewAssignedPersonnelAsync(ClaimsPrincipal user,CompanyWorkOrderAssignedPersonnelQuery query)
    {
        query.Require();if(contractors is not ICompanyWorkOrderPersonnelAssignmentReads owner)throw new InvalidOperationException("Existing native personnel roster owner is unavailable.");
        var roster=await ReviewPersonnelAssignmentsAsync(user,query.Permit);
        var assignment=roster.Roster.Assignments.SingleOrDefault(row=>row.AssignmentId==query.AssignmentId)??throw new InvalidOperationException("The selected original personnel assignment is not in this complete current permit roster.");
        var current=await ReviewPersonnelSourceAsync(user,new(query.Permit,assignment.EmployeeBaId,assignment.LicenseId));
        var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=current.PermitReview.Context)throw new UnauthorizedAccessException("Original current assigned-personnel reviewer authority changed.");}
        var closing=await owner.ReadPersonnelAssignmentsAsync(current.Personnel.Context,query.Permit,Guard);await Guard();
        var closingPersonnel=await ReviewPersonnelSourceAsync(user,current.Personnel.Query);await Guard();
        if(current.PermitReview.Context!=roster.PermitReview.Context || current.PermitReview.RecordedRevision!=roster.PermitReview.RecordedRevision ||
            current.PermitReview.CurrentSource.SnapshotRevision!=roster.PermitReview.CurrentSource.SnapshotRevision || JsonSerializer.Serialize(roster.Roster.Assignments)!=JsonSerializer.Serialize(closing.Assignments) || JsonSerializer.Serialize(roster.Roster.Releases)!=JsonSerializer.Serialize(closing.Releases) ||
            current.Personnel.Context!=closingPersonnel.Personnel.Context || current.Personnel.Employment!=closingPersonnel.Personnel.Employment || current.Personnel.License!=closingPersonnel.Personnel.License ||
            current.PermitReview.RecordedRevision!=closingPersonnel.PermitReview.RecordedRevision || current.PermitReview.CurrentSource.SnapshotRevision!=closingPersonnel.PermitReview.CurrentSource.SnapshotRevision)
            throw new InvalidOperationException("Complete original assignment roster/permit/order/personnel source or authority changed during current comparison.");
        var result=new CompanyWorkOrderAssignedPersonnelReview(query,closing,closingPersonnel,CompanyWorkOrderAssignedPersonnelReview.GetBlockers(assignment,closingPersonnel,closing.Releases));
        result.Require(current.Personnel.Context.Company,current.Personnel.Context.ActorUserId,query);return result;
    }
    public async Task<CompanyWorkOrderPersonnelAssignmentRecovery> RecoverPersonnelAssignmentReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelAssignmentReference reference)
    {
        reference.Require();if(contractors is not ICompanyWorkOrderPersonnelAssignmentOwner owner)throw new InvalidOperationException("Existing native personnel assignment owner is unavailable.");
        var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard()
        {
            if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original personnel reference actor/company/task changed.");
            var access=await company.RefreshCompanyAsync();if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Maintenance.AssignPersonnel",StringComparer.Ordinal) && !access.Permissions.Contains("Maintenance.ReleasePersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original personnel assignment or cancellation permission and standard roles are required.");
        }
        await Guard();var result=await owner.RecoverPersonnelAssignmentReferenceAsync(context,reference,Guard);result.Require(context.Company,context.ActorUserId,reference);
        await Guard();return result;
    }
    public async Task<WORK_ORDER_PERSONNEL_ASSIGNMENT> RecordPersonnelAssignmentAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelAssignmentCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();
        if(contractors is not ICompanyWorkOrderPersonnelAssignmentOwner owner)throw new InvalidOperationException("Existing native personnel assignment owner is unavailable.");
        var context=command.Review.Personnel.Context;var query=command.Review.Personnel.Query;var task=query.Permit.Task;
        var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        async Task Guard()
        {
            if(user.ActingUserId()!=context.ActorUserId || await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=command.Review.PermitReview.Context)throw new UnauthorizedAccessException("Original personnel assignment actor/company/task authority changed.");
            var access=await company.RefreshCompanyAsync();
            if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Maintenance.AssignPersonnel",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard app roles and Maintenance.AssignPersonnel are required.");
        }
        async Task Proof(){await Guard();command.RequireCurrent(await ReviewPersonnelSourceAsync(user,query));await Guard();}
        async Task CoreProof()
        {
            await Guard();if(workOrders is null || inspections is null)throw new InvalidOperationException("Existing local work-order/inspection owners are unavailable.");
            var source=command.Review.PermitReview.CurrentSource;
            var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original local work order is unavailable.");
            var rows=command.Review.PermitReview.Proposal.RequirementEvidenceVersion is 1 or 2?await (inspections as ICompanyWorkOrderRequirementSource??throw new InvalidOperationException("Existing owned immutable requirement source is unavailable.")).ReadRequirementProofCoreAsync(context,task.WorkOrderId,Guard):await inspections.GetChecklistAsync(task.WorkOrderId,task.FieldId);
            if(CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,order,rows)!=source.SnapshotRevision)throw new InvalidOperationException("Original complete local work-order/inspection source changed inside assignment.");
            await Guard();
        }
        await Guard();return await owner.RecordPersonnelAssignmentAsync(command,Guard,Proof,readOnly,CoreProof);
    }
    public async Task<CompanyWorkOrderPersonnelSourceReview> ReviewPersonnelSourceAsync(ClaimsPrincipal user,CompanyWorkOrderPersonnelSourceQuery query)
    {
        query.Require();if(contractors is not ICompanyWorkOrderPersonnelSourceOwner owner)throw new InvalidOperationException("Existing company personnel source owner is unavailable.");var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);var permit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=permit.Context)throw new UnauthorizedAccessException("Original independent personnel reviewer authority changed.");}
        await Guard();var first=await owner.ReadPersonnelSourceAsync(permit.Context,query,Guard);var closing=await owner.ReadPersonnelSourceAsync(permit.Context,query,Guard);var closingPermit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);await Guard();
        if(first.Employment!=closing.Employment || first.License!=closing.License || permit.Context!=closingPermit.Context || permit.RecordedRevision!=closingPermit.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closingPermit.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Complete original company employment/licence/permit/order evidence changed during personnel review.");var result=new CompanyWorkOrderPersonnelSourceReview(closingPermit,closing,CompanyWorkOrderPersonnelSourceReview.GetBlockers(closingPermit.Proposal,closing));result.Require(permit.Context.Company,permit.Context.ActorUserId,query);return result;
    }
    public async Task<CompanyWorkOrderMaterialAcceptanceRecovery> RecoverMaterialAcceptanceReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialAcceptanceReference reference)
    {
        reference.Require();if(inspections is not ICompanyWorkOrderMaterialAcceptanceOwner owner)throw new InvalidOperationException("Existing native material inspection owner is unavailable.");var task=reference.Permit.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("Original material reference reviewer authority changed.");var access=await company.RefreshCompanyAsync();if(access.Scope!=context.Company || access.UserId!=context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original reviewer standard roles and Inventory.View are required.");}
        await Guard();var result=await owner.RecoverMaterialAcceptanceReferenceAsync(context,reference,Guard);await Guard();result.Require(context.Company,context.ActorUserId,reference);await RecordMaterialAcceptanceAsync(user,result.Command,true);await Guard();return result;
    }
    public async Task<Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION> RecordMaterialAcceptanceAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialAcceptanceCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();if(inspections is not ICompanyWorkOrderMaterialAcceptanceOwner owner)throw new InvalidOperationException("The existing native material inspection owner is unavailable.");var query=command.Review.Query;var originalContext=command.Review.PermitReview.Context;var boundary=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);
        async Task AuthorityGuard()
        {
            if(user.ActingUserId()!=originalContext.ActorUserId || await PermitAuthority(user,boundary.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=originalContext)throw new UnauthorizedAccessException("Original independent material reviewer authority changed.");var access=await company.RefreshCompanyAsync();if(access.Scope!=originalContext.Company || access.UserId!=originalContext.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original reviewer account/standard roles and Inventory.View are required.");
            var source=command.Review.CurrentMaterial.Source.Context.Database;var connection=await modules.ResolveAsync("PRODUCTION");if(source!=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection)))throw new InvalidOperationException("Original material acceptance PRODUCTION binding changed.");
        }
        async Task Proof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? own){await AuthorityGuard();var current=await ReviewPermitMaterialReadinessAsync(user,query);command.RequireCurrent(current,own);await AuthorityGuard();}
        async Task CoreProof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION? own)
        {
            await AuthorityGuard();if(workOrders is null)throw new InvalidOperationException("Existing core work-order owner is unavailable.");var task=query.Permit.Task;var source=command.Review.PermitReview.CurrentSource;var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original current core order is unavailable.");var rows=source.Inspections.Any(row=>row.RequirementRevision is not null)?await (inspections as ICompanyWorkOrderRequirementSource??throw new InvalidOperationException("Owned immutable requirement source is unavailable.")).ReadRequirementProofCoreAsync(originalContext,task.WorkOrderId,AuthorityGuard):await inspections.GetChecklistAsync(task.WorkOrderId,task.FieldId);
            var current=new CompanyMaintenanceInspectionReview(source.Stage,source.InspectionDatabase,order,rows,CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,order,rows),DateTimeOffset.UtcNow);command.RequireCurrentCore(current,own);await AuthorityGuard();
        }
        await AuthorityGuard();return await owner.RecordMaterialAcceptanceAsync(command,AuthorityGuard,Proof,readOnly,CoreProof);
    }
    public async Task<CompanyWorkOrderMaterialCatalog> ReviewMaterialCatalogAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery query)
    {
        query.Require();if(inventory is null)throw new InvalidOperationException("Existing native inventory owner is unavailable.");
        async Task<CompanyMaintenanceAssignedStageReview> Capture()
        {
            var stage=await ReviewAssignedStageAsync(user,query);var access=await company.RefreshCompanyAsync();
            if(access.Scope!=stage.Context.Company || access.UserId!=stage.Context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !access.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current assigned company account/standard roles and inventory permissions are required.");
            var connection=await modules.ResolveAsync("PRODUCTION");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection));return stage with{Context=stage.Context with{Database=database,Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Original=stage.Context,Inventory=database,Permission="Inventory.View",Reservation="Inventory.ReserveWorkOrderMaterial"}))}};
        }
        var original=await Capture();var reads=new CompanyInventoryReads(inventory,company);var first=await reads.GetQueueAsync(user);var closing=await reads.GetQueueAsync(user);
        if(await Capture()!=original || JsonSerializer.Serialize(first.OrderBy(row=>row.INVENTORY_ITEM_ID,StringComparer.Ordinal))!=JsonSerializer.Serialize(closing.OrderBy(row=>row.INVENTORY_ITEM_ID,StringComparer.Ordinal)))throw new InvalidOperationException("Original assigned task/company/PRODUCTION inventory choices changed during catalog review.");
        var result=CompanyWorkOrderMaterialCatalog.Create(original,closing,DateTime.UtcNow);result.Require(original.Context.Company,original.Context.ActorUserId,query);return result;
    }
    public async Task<CompanyMaterialReservationQueue> ReadMaterialReservationQueueAsync(System.Security.Claims.ClaimsPrincipal user)
    {
        if(inventory is null)throw new InvalidOperationException("Existing native inventory reservation owner is unavailable.");var current=await selection.GetCurrentFieldSelectionAsync();var access=await company.RefreshCompanyAsync();var actor=user.ActingUserId();FieldSelectionReviewValidator.RequireCurrent(current,access.Scope,actor);if(!current.SelectionAvailable || current.SelectionContext is null)throw new InvalidOperationException("Select a current authorized FIELD for reservation discovery.");var preference=current.SelectionContext;
        async Task<(FieldSelectionReview Context,HashSet<string> Orders)> Capture()
        {
            var live=await company.RefreshCompanyAsync();if(user.Identity?.IsAuthenticated!=true || actor!=live.UserId || live.Scope!=preference.Company || !live.Roles.Any(user.IsInRole) || !live.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || field.CurrentFieldId!=preference.FieldId)throw new UnauthorizedAccessException("Current company account, standard roles, Inventory.View and original FIELD are required.");await preferences.RequireStoredSelectionAsync(preference);
            var rows=await grants.ReadAsync(actor,preference.Database.Fingerprint);var usable=rows.Where(row=>row.UserId==actor && row.IsActive && row.OrganizationId==preference.Company.BusinessAssociateId && row.DatabaseScope==preference.Database.Fingerprint).ToArray();var fieldRows=usable.Where(row=>row.AssetType=="FIELD" && row.AssetId==preference.FieldId).ToArray();if(fieldRows.Length!=1 || !AssetAccessLevelChecks.Meets(fieldRows[0].AccessLevel,"READ"))throw new UnauthorizedAccessException("Current direct FIELD READ rights are required.");
            var orders=usable.Where(row=>row.AssetType=="WORK_ORDER").GroupBy(row=>row.AssetId,StringComparer.Ordinal).Where(group=>group.Count()==1 && AssetAccessLevelChecks.Meets(group.Single().AccessLevel,"READ")).Select(group=>group.Key).ToHashSet(StringComparer.Ordinal);var connection=await modules.ResolveAsync("PRODUCTION");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection));var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preference=preference,Company=live.Scope,Roles=live.Roles.Order(StringComparer.Ordinal),Permissions=live.Permissions.Order(StringComparer.Ordinal),Grants=rows.OrderBy(row=>row.Id,StringComparer.Ordinal),Database=database}));await preferences.RequireStoredSelectionAsync(preference);if(field.CurrentFieldId!=preference.FieldId)throw new InvalidOperationException("Selected queue FIELD changed.");return(preference with{Database=database,Revision=revision},orders);
        }
        var original=await Capture();async Task Guard(){var now=await Capture();if(now.Context!=original.Context || !now.Orders.SetEquals(original.Orders))throw new UnauthorizedAccessException("Original queue authority, order rights or binding changed.");}
        var result=await inventory.ReadMaterialReservationQueueAsync(original.Context,original.Orders.Contains,Guard);await Guard();result.Require(preference.Company,actor,preference.FieldId!);return result;
    }
    public async Task<CompanyWorkOrderMaterialReadinessReview> ReviewPermitMaterialReadinessAsync(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialReadinessQuery query)
    {
        query.Require();if(inventory is null)throw new InvalidOperationException("Existing native inventory owner is unavailable.");var captured=await Boundary(user,query.Permit.Task.WorkOrderId,query.Permit.Task.FieldId);var permit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);var connection=await modules.ResolveAsync("PRODUCTION");var fingerprint=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection);
        async Task Guard()
        {
            if(await PermitAuthority(user,captured.Preference,query.Permit.Task.WorkOrderId,"HSE.ApprovePermit")!=permit.Context)throw new UnauthorizedAccessException("Original independent permit reviewer authority changed.");var access=await company.RefreshCompanyAsync();if(!access.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !access.Roles.Any(user.IsInRole))throw new UnauthorizedAccessException("Current company Inventory.View and standard roles are required for material readiness.");if(await modules.ResolveAsync("PRODUCTION")!=connection || await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection)!=fingerprint)throw new InvalidOperationException("Original readiness PRODUCTION target changed.");
        }
        await Guard();var reservation=await inventory.ReadWorkOrderMaterialReservationFactsAsync(permit.Context.Company,query.Permit.Task,query.AllocationIntentId,Guard);var materialQuery=new CompanyWorkOrderMaterialSourceQuery(query.Permit.Task,reservation.ItemId,reservation.Quantity,reservation.Unit);
        var source=await ReviewMaterialSourceCoreAsync(user,materialQuery,permit.Proposal.RequirementEvidenceVersion is 1 or 2,permit.CurrentSource);var material=await inventory.ReviewWorkOrderMaterialAllocationsAsync(source,Guard);var closingReservation=await inventory.ReadWorkOrderMaterialReservationFactsAsync(permit.Context.Company,query.Permit.Task,query.AllocationIntentId,Guard);var closingPermit=await ReviewExecutionPermitApprovalCoreAsync(user,query.Permit);var closingSource=await ReviewMaterialSourceCoreAsync(user,materialQuery,permit.Proposal.RequirementEvidenceVersion is 1 or 2,closingPermit.CurrentSource);await Guard();
        if(reservation!=closingReservation || permit.Context!=closingPermit.Context || permit.RecordedRevision!=closingPermit.RecordedRevision || permit.CurrentSource.SnapshotRevision!=closingPermit.CurrentSource.SnapshotRevision || source.Context!=closingSource.Context || source.WorkOrder.SnapshotRevision!=closingSource.WorkOrder.SnapshotRevision || source.Inventory.Revision!=closingSource.Inventory.Revision)throw new InvalidOperationException("Complete original permit/order/material/reservation evidence changed during readiness comparison.");
        var utc=DateTime.UtcNow;var result=new CompanyWorkOrderMaterialReadinessReview(query,permit,reservation,material,utc,CompanyWorkOrderMaterialReadinessReview.GetBlockers(permit,reservation,material,utc));result.Require(permit.Context.Company,permit.Context.ActorUserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialReleaseRecovery> RecoverMaterialReleaseReferenceAsync(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialReleaseReferenceQuery query)
    {
        query.Require();if(inventory is null)throw new InvalidOperationException("Existing inventory owner is unavailable.");var captured=await Boundary(user,query.Allocation.Task.WorkOrderId,query.Allocation.Task.FieldId);var connection=await modules.ResolveAsync("PRODUCTION");var fingerprint=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection);
        async Task Guard(){await Boundary(user,query.Allocation.Task.WorkOrderId,query.Allocation.Task.FieldId);var access=await company.RefreshCompanyAsync();if(access.Scope!=captured.Preference.Company || access.UserId!=captured.Preference.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !access.Permissions.Contains("Inventory.ReleaseWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Original release account/role/permission is required.");await ReviewAssignedStageAsync(user,query.Allocation.Task);if(await modules.ResolveAsync("PRODUCTION")!=connection || await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection)!=fingerprint)throw new InvalidOperationException("Original release binding changed.");}
        await Guard();var result=await inventory.RecoverWorkOrderMaterialReleaseReferenceAsync(captured.Preference.Company,captured.Preference.ActorUserId,query,Guard);await Guard();result.Require(captured.Preference.Company,captured.Preference.ActorUserId,query);return result;
    }
    public async Task<WORK_ORDER_MATERIAL_RELEASE> ReleaseMaterialAllocationAsync(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialReleaseCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();if(inventory is null)throw new InvalidOperationException("Existing inventory owner is unavailable.");var source=command.Original.Command.Review.Source;
        async Task Authority()=>await RequireMaterialAllocationAuthority(user,source,"Inventory.ReleaseWorkOrderMaterial");
        async Task CurrentTask(){await Authority();await ReviewAssignedStageAsync(user,source.Query.Task);}
        await Authority();return await inventory.ReleaseWorkOrderMaterialAsync(command,Authority,CurrentTask,readOnly);
    }
    public async Task<CompanyWorkOrderMaterialAllocationRecovery> RecoverMaterialAllocationReferenceAsync(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialAllocationReferenceQuery query)
    {
        query.Require();if(inventory is null)throw new InvalidOperationException("Existing inventory reservation owner is unavailable.");var captured=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var connection=await modules.ResolveAsync("PRODUCTION");var fingerprint=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection);
        async Task Guard()
        {
            await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var access=await company.RefreshCompanyAsync();if(access.Scope!=captured.Preference.Company || access.UserId!=captured.Preference.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !access.Permissions.Contains("Inventory.ReserveWorkOrderMaterial",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original reservation account/roles/permissions are required.");
            await ReviewAssignedStageAsync(user,query.Task);if(await modules.ResolveAsync("PRODUCTION")!=connection || await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection)!=fingerprint)throw new InvalidOperationException("Original recovery PRODUCTION target changed.");
        }
        await Guard();var result=await inventory.RecoverWorkOrderMaterialAllocationReferenceAsync(captured.Preference.Company,captured.Preference.ActorUserId,query,Guard);await Guard();result.Require(captured.Preference.Company,captured.Preference.ActorUserId,query);return result;
    }
    private async Task RequireMaterialAllocationAuthority(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialSourceReview original,string permission="Inventory.ReserveWorkOrderMaterial")
    {
        var captured=await Boundary(user,original.Query.Task.WorkOrderId,original.Query.Task.FieldId);var access=await company.RefreshCompanyAsync();
        if(access.Scope!=original.Context.Company || access.UserId!=original.Context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains(permission,StringComparer.Ordinal) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original company standard roles and the required inventory permission are required.");
        var connection=await modules.ResolveAsync("PRODUCTION");var fingerprint=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection);
        if(original.Context.Database!=new AssetDatabaseScope(connection,fingerprint))throw new InvalidOperationException("Original PRODUCTION allocation binding changed.");
    }
    public async Task<CompanyWorkOrderMaterialAllocationReview> ReviewMaterialAllocationAsync(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialSourceQuery query)
    {
        var source=await ReviewMaterialSourceAsync(user,query);await RequireMaterialAllocationAuthority(user,source);
        var result=await inventory!.ReviewWorkOrderMaterialAllocationsAsync(source,()=>RequireMaterialAllocationAuthority(user,source));
        var closing=await ReviewMaterialSourceAsync(user,query);if(closing.Context!=source.Context || closing.WorkOrder.SnapshotRevision!=source.WorkOrder.SnapshotRevision || closing.Inventory.Revision!=source.Inventory.Revision)throw new InvalidOperationException("Original order/inventory source changed during allocation review.");result.Require();return result;
    }
    public async Task<WORK_ORDER_MATERIAL_ALLOCATION> RecordMaterialAllocationAsync(System.Security.Claims.ClaimsPrincipal user,CompanyWorkOrderMaterialAllocationCommand original,bool readOnly=false)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var source=command.Review.Source;
        if(inventory is null)throw new InvalidOperationException("Existing company inventory owner is unavailable.");
        async Task Authority()=>await RequireMaterialAllocationAuthority(user,source);
        async Task WorkOrder(){await Authority();var current=await ReviewAssignedInspectionsAsync(user,source.Query.Task);if(current.SnapshotRevision!=source.WorkOrder.SnapshotRevision)throw new InvalidOperationException("Original assigned order/inspection source changed.");}
        await Authority();return await inventory.RecordWorkOrderMaterialAllocationAsync(command,Authority,WorkOrder,readOnly);
    }
    public Task<CompanyWorkOrderMaterialSourceReview> ReviewMaterialSourceAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialSourceQuery query)=>ReviewMaterialSourceCoreAsync(user,query,false);
    private async Task<CompanyWorkOrderMaterialSourceReview> ReviewMaterialSourceCoreAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialSourceQuery query,bool requirementProof,CompanyMaintenanceInspectionReview? reviewedOrders=null)
    {
        query.Require();if(inventory is null)throw new InvalidOperationException("The existing company inventory source owner is unavailable.");var captured=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);
        async Task<FieldSelectionReview> AuthorityCapture()
        {
            var authority=await Authority(user,captured.Preference,query.Task.WorkOrderId);var access=await company.RefreshCompanyAsync();
            if(!access.Permissions.Contains("Inventory.View",StringComparer.Ordinal) || !access.Roles.Any(user.IsInRole))throw new UnauthorizedAccessException("Current standard app roles and Inventory.View are required.");
            var connection=await modules.ResolveAsync("PRODUCTION");var database=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection));
            if(await Authority(user,captured.Preference,query.Task.WorkOrderId)!=authority)throw new UnauthorizedAccessException("The original material source authority changed.");return captured.Preference with{Database=database,Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Authority=authority,Permission="Inventory.View",Database=database}))};
        }
        var context=await AuthorityCapture();if(reviewedOrders is not null){reviewedOrders.Require(context.Company,context.ActorUserId,query.Task);if(requirementProof && reviewedOrders.Inspections.Any(row=>!ProcessDefinitionRevision.IsValid(row.RequirementRevision)))throw new InvalidOperationException("Previously reviewed versioned permit order source is incomplete.");}Task<CompanyMaintenanceInspectionReview> ReadOrders()=>reviewedOrders is not null?Task.FromResult(CompanyInventorySourceReview.Copy(reviewedOrders)):requirementProof?ReviewAssignedRequirementInspectionsAsync(user,query.Task,"Inventory.View"):ReviewAssignedInspectionsAsync(user,query.Task);var orders=await ReadOrders();var reads=new CompanyInventoryReads(inventory,company);var initial=await reads.GetSourceAsync(user,query.ItemId)??throw new InvalidOperationException("The original owned inventory item is unavailable.");
        var closingOrders=await ReadOrders();var closing=await reads.GetSourceAsync(user,query.ItemId)??throw new InvalidOperationException("The original owned inventory item changed.");
        if(await AuthorityCapture()!=context || orders.SnapshotRevision!=closingOrders.SnapshotRevision || initial.Revision!=closing.Revision)throw new InvalidOperationException("The complete original order/inspection/inventory/PRODUCTION evidence changed during material review.");
        var result=new CompanyWorkOrderMaterialSourceReview(context,query,closingOrders,closing,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,closing));result.Require(context.Company,context.ActorUserId,query);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderExecutionPermitDecisionReceipt> DecideExecutionPermitAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitDecision original,bool readOnly=false)
    {
        ArgumentNullException.ThrowIfNull(original);var decision=CompanyInventorySourceReview.Copy(original);decision.Require();if(executionPermits is null)throw new InvalidOperationException("The native independent permit decision owner is unavailable.");
        var task=decision.Review.Proposal.Task;var captured=await Boundary(user,task.WorkOrderId,task.FieldId);
        async Task Guard(){if(await PermitAuthority(user,captured.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=decision.Review.Context)throw new UnauthorizedAccessException("Return to the original independent reviewer/company/FIELD/authority.");}
        async Task MaterialAuthority()
        {
            await Guard();var material=decision.Review.CurrentMaterialAcceptance??throw new InvalidOperationException("Complete original accepted material proof is unavailable.");var access=await company.RefreshCompanyAsync();
            if(access.Scope!=decision.Review.Context.Company || access.UserId!=decision.Review.Context.ActorUserId || !access.Roles.Any(user.IsInRole) || !access.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current original approval reviewer and Inventory.View are required.");
            var connection=await modules.ResolveAsync("PRODUCTION");var binding=new AssetDatabaseScope(connection,await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection));
            if(binding!=material.Material.Source.Context.Database)throw new InvalidOperationException("Original current material binding changed during approval.");await Guard();
        }
        async Task CurrentPrerequisites()
        {
            await Guard();decision.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
            // Current core personnel/document/identity populations are proven at both sides of the owned write below.
            // Outside that transaction close the original lifecycle/order/inspection and potentially separate stock source.
            var current=decision.Review.Proposal.RequirementEvidenceVersion is 1 or 2?await ReviewAssignedRequirementInspectionsAsync(user,task,"HSE.ApprovePermit"):await ReviewAssignedInspectionsAsync(user,task);
            current.Require(decision.Review.Context.Company,decision.Review.Context.ActorUserId,task);
            if(current.SnapshotRevision!=decision.Review.CurrentSource.SnapshotRevision || current.InspectionDatabase!=decision.Review.Context.Database)throw new InvalidOperationException("Complete original current order/inspection/assigned-stage proof changed before approval.");
            if(inventory is null)throw new InvalidOperationException("The explicit current stock proof owner is unavailable.");
            await inventory.RequireAcceptedMaterialCurrentProofAsync(decision.Review.Proposal,decision.Review.CurrentMaterialAcceptance!,MaterialAuthority);
            await Guard();
        }
        async Task CorePrerequisites()
        {
            await Guard();decision.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
            if(workOrders is null || inspections is not ICompanyWorkOrderRequirementSource requirements || inspections is not ICompanyWorkOrderDocumentContributorCore documents || contractors is not ICompanyPersonnelAcceptanceCoreProof personnel || contractors is not ICompanyRecordedReviewerIdentityCore identities || inventory is null)throw new InvalidOperationException("Complete owned native prerequisite proof owners are unavailable.");
            var originalSource=decision.Review.CurrentSource;
            var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("The original owned current order is unavailable.");
            var rows=await requirements.ReadRequirementProofCoreAsync(decision.Review.Context,task.WorkOrderId,Guard);
            var currentRevision=CompanyMaintenanceInspectionReview.Revision(originalSource.Stage,originalSource.InspectionDatabase,order,rows);
            if(currentRevision!=originalSource.SnapshotRevision)throw new InvalidOperationException("Complete original owned order/requirement/acceptance evidence changed inside approval.");
            using(PermitProofActivities.StartActivity("permit.owned.personnel"))
                await personnel.RequirePersonnelAcceptanceCoreAsync(decision.Review.CurrentPersonnelAcceptance!.Evidence,Guard);
            foreach(var accepted in decision.Review.CurrentDocumentAcceptance!)
            {
                using var documentActivity=PermitProofActivities.StartActivity(accepted.Acceptance.Kind=="ISOLATION"?"permit.owned.isolation":"permit.owned.risk");
                await documents.RequireApprovalDocumentCoreAsync(decision.Review.Context,accepted,Guard);
                await identities.RequireRecordedReviewerIdentityCoreAsync(accepted.CurrentIdentities!,accepted.RecordedReviewerPerson!,accepted.Acceptance.Reviewer,decision.Review.Proposal.ValidFromUtc,decision.Review.Proposal.ValidToUtc,Guard);
            }
            var material=decision.Review.CurrentMaterialAcceptance!;
            // The transaction owner forbids cross-provider reads. Separate stock closes in CurrentPrerequisites before/after this command.
            using(PermitProofActivities.StartActivity("permit.owned.material"))
            {await MaterialAuthority();await inventory.RequireAcceptedMaterialOwnedCoreProofAsync(decision.Review.Proposal,material,MaterialAuthority,decision.Review.Context.Database.ConnectionName);}
            await MaterialAuthority();await Guard();
        }
        async Task CommittedPrerequisites()
        {
            // Core requirements/personnel/documents/reviewer identity closed in the owned serializable write.
            // Separate stock cannot join that transaction and must close again after commit.
            await Guard();decision.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
            if(inventory is null)throw new InvalidOperationException("The explicit committed material proof owner is unavailable.");
            await inventory.RequireAcceptedMaterialCurrentProofAsync(decision.Review.Proposal,decision.Review.CurrentMaterialAcceptance!,MaterialAuthority);
            await Guard();
        }
        await Guard();return await executionPermits.DecideAsync(decision,Guard,()=>decision.Review.Proposal.RequirementEvidenceVersion is 1 or 2?ReviewAssignedRequirementInspectionsAsync(user,task,"HSE.ApprovePermit"):ReviewAssignedInspectionsAsync(user,task),readOnly,
            decision.Outcome=="Approve"?CurrentPrerequisites:null,decision.Outcome=="Approve"?CorePrerequisites:null,decision.Outcome=="Approve"?CommittedPrerequisites:null);
    }
    public async Task<CompanyWorkOrderExecutionPermitDecisionRecovery> RecoverExecutionPermitDecisionReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitDecisionReference query)
    {
        query.Require();if(executionPermits is null)throw new InvalidOperationException("The original permit decision reference owner is unavailable.");var task=query.Proposal.Task;var captured=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,captured.Preference,task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard(){if(await PermitAuthority(user,captured.Preference,task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("The original independent permit decision authority changed.");}
        var decision=await executionPermits.ReadOriginalDecisionAsync(context,query,Guard);var receipt=await DecideExecutionPermitAsync(user,decision,true);var result=new CompanyWorkOrderExecutionPermitDecisionRecovery(decision,receipt);result.Require(context.Company,context.ActorUserId,query);return result;
    }
    public Task<CompanyWorkOrderExecutionPermitApprovalReview> ReviewExecutionPermitApprovalAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)=>ReviewExecutionPermitPrerequisiteProofAsync(user,query);
    private async Task<CompanyWorkOrderExecutionPermitApprovalReview> ReviewExecutionPermitPrerequisiteProofAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query,CompanyWorkOrderExecutionPermitDecision? recordedDecision=null)
    {
        using var compositionActivity=PermitProofActivities.StartActivity("permit.composition");
        var original=await ReviewExecutionPermitApprovalCoreAsync(user,query,recordedDecision);CompanyWorkOrderAcceptedMaterialReview? material=null;
        var candidates=original.CurrentSource.Inspections.Where(row=>row.PermitPrerequisiteKind=="MATERIAL" && row.RegulatoryRef==original.Proposal.MaterialReference && row.Jurisdiction==original.Proposal.Jurisdiction).ToArray();var access=await company.RefreshCompanyAsync();
        if(inventory is not null && access.Permissions.Contains("Inventory.View",StringComparer.Ordinal) && candidates.Length==1 && candidates[0].MaterialAcceptance is {AllocationIntentId:var allocationIntent} acceptance && allocationIntent!=Guid.Empty)
        {
            using var materialActivity=PermitProofActivities.StartActivity("permit.composition.material");
            var preference=(await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId)).Preference;var connection=await modules.ResolveAsync("PRODUCTION");var fingerprint=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection);
            async Task Guard()
            {
                if(await PermitAuthority(user,preference,query.Task.WorkOrderId,"HSE.ApprovePermit")!=original.Context)throw new UnauthorizedAccessException("Original accepted material permit authority changed.");var current=await company.RefreshCompanyAsync();if(!current.Roles.Any(user.IsInRole) || !current.Permissions.Contains("Inventory.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current accepted material Inventory.View and standard roles are required.");if(await modules.ResolveAsync("PRODUCTION")!=connection || await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection)!=fingerprint)throw new InvalidOperationException("Original accepted material PRODUCTION binding changed.");
            }
            await Guard();var reservation=await inventory.ReadWorkOrderMaterialReservationFactsAsync(original.Context.Company,query.Task,allocationIntent,Guard);var source=await ReviewMaterialSourceCoreAsync(user,new(query.Task,reservation.ItemId,reservation.Quantity,reservation.Unit),original.Proposal.RequirementEvidenceVersion is 1 or 2,original.CurrentSource);var allocations=await inventory.ReviewWorkOrderMaterialAllocationsAsync(source,Guard);var closingReservation=await inventory.ReadWorkOrderMaterialReservationFactsAsync(original.Context.Company,query.Task,allocationIntent,Guard);var closingSource=await ReviewMaterialSourceCoreAsync(user,source.Query,original.Proposal.RequirementEvidenceVersion is 1 or 2,original.CurrentSource);await Guard();
            if(reservation!=closingReservation || source.Inventory.Revision!=closingSource.Inventory.Revision || source.WorkOrder.SnapshotRevision!=closingSource.WorkOrder.SnapshotRevision)throw new InvalidOperationException("Complete original accepted material/permit/order/inventory evidence changed during current review.");
            var compared=DateTime.UtcNow;material=new(acceptance,reservation,allocations,compared,CompanyWorkOrderAcceptedMaterialReview.GetBlockers(original.Proposal,acceptance,reservation,allocations,compared));material.Require(original.Proposal,original.CurrentSource);
        }
        CompanyWorkOrderAcceptedPersonnelReview? personnel=null;
        var acceptedPersonnel=original.CurrentSource.Inspections.Where(row=>row.PermitPrerequisiteKind=="PERSONNEL" && row.Jurisdiction==original.Proposal.Jurisdiction && row.PersonnelAcceptance is not null).ToArray();
        if(acceptedPersonnel.Length==1 && acceptedPersonnel[0].PersonnelAcceptance is {} personnelFacts)
        {
            using var personnelActivity=PermitProofActivities.StartActivity("permit.composition.personnel");
            var closingEvidence=await ReviewPersonnelPolicyEvidenceCoreAsync(user,new(query,personnelFacts.AssignmentId),recordedDecision,original);
            var compared=DateTime.UtcNow;personnel=new(personnelFacts,closingEvidence,compared,CompanyWorkOrderAcceptedPersonnelReview.GetBlockers(original.Proposal,personnelFacts,closingEvidence,compared));personnel.Require(original.Proposal,original.CurrentSource);
        }
        var documents=new List<CompanyWorkOrderAcceptedDocumentReview>();
        var documentRows=original.CurrentSource.Inspections.Where(row=>(row.PermitPrerequisiteKind is "ISOLATION" or "RISK") && row.Jurisdiction==original.Proposal.Jurisdiction && row.DocumentAcceptance is not null).ToArray();
        if(documentRows.Length>0)
        {
            if(inspections is not ICompanyWorkOrderPermitDocumentSourceOwner documentOwner)throw new InvalidOperationException("Existing native accepted-document source owner is unavailable.");var preference=(await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId)).Preference;
            async Task Guard(){if(await PermitAuthority(user,preference,query.Task.WorkOrderId,"HSE.ApprovePermit")!=original.Context)throw new UnauthorizedAccessException("Current accepted-document reviewer authority changed.");}
            foreach(var kind in new[]{"ISOLATION","RISK"})
            {
                using var documentActivity=PermitProofActivities.StartActivity(kind=="ISOLATION"?"permit.composition.isolation":"permit.composition.risk");
                var accepted=documentRows.Where(row=>row.PermitPrerequisiteKind==kind).ToArray();if(accepted.Length>1)throw new InvalidOperationException("Recorded document acceptance population is ambiguous.");if(accepted.Length==0)continue;var facts=accepted[0].DocumentAcceptance!;
                var documentQuery=new CompanyWorkOrderPermitDocumentQuery(query,kind,facts.ComponentId);var expectedDocument=kind=="ISOLATION"?original.Proposal.IsolationReference:original.Proposal.RiskReference;
                CompanyPersonnelIdentityCatalog? identities=null;CompanyNativePersonFacts? recordedReviewer=null;CompanyWorkOrderDocumentContributors? contributors=null;
                CompanyWorkOrderPermitDocumentFacts source;
                if(facts.ContributorEvidenceVersion==1)
                {
                    if(inspections is not ICompanyWorkOrderDocumentContributorOwner contributorOwner || contractors is not ICompanyPersonnelIdentityOwner identityOwner)throw new InvalidOperationException("Existing accepted-document contributor and recorded-reviewer identity owners are unavailable.");
                    var identityContext=new CompanyPersonnelPolicyContext(original.Context.Company,original.Context.Database,original.Context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{original.Context.Revision,Permission="HSE.ApprovePermit",original.Context.Database})));
                    var firstIdentities=await identityOwner.ReadPersonnelIdentitiesAsync(identityContext,Guard);var identityUtc=DateTime.UtcNow;var links=firstIdentities.Versions.Where(row=>row.UserId==facts.Reviewer && row.EffectiveFromUtc<=identityUtc && row.EffectiveToUtc>identityUtc).ToArray();CompanyNativePersonFacts? firstPerson=null;if(links.Length==1)firstPerson=await identityOwner.ReadIdentityPersonAsync(identityContext,links[0].PersonBaId,Guard);
                    // This native reader already closes complete document/content/contributor/person populations twice.
                    contributors=await contributorOwner.ReadDocumentContributorsAsync(original.Context,documentQuery,expectedDocument,Guard);source=contributors.Source;
                    identities=await identityOwner.ReadPersonnelIdentitiesAsync(identityContext,Guard);if(links.Length==1)recordedReviewer=await identityOwner.ReadIdentityPersonAsync(identityContext,links[0].PersonBaId,Guard);await Guard();
                    if(firstIdentities.Revision!=identities.Revision || firstPerson!=recordedReviewer)throw new InvalidOperationException("Complete current recorded-reviewer identity/person proof changed around document contributor comparison.");
                }
                else source=await documentOwner.ReadPermitDocumentSourceAsync(original.Context,documentQuery,expectedDocument,Guard);
                await Guard();var evidence=new CompanyWorkOrderPermitDocumentReview(original,source,CompanyWorkOrderPermitDocumentReview.GetBlockers(original.Proposal,source));
                var compared=DateTime.UtcNow;var current=new CompanyWorkOrderAcceptedDocumentReview(facts,evidence,compared,CompanyWorkOrderAcceptedDocumentReview.GetBlockers(original.Proposal,facts,evidence,compared,1,contributors,identities,recordedReviewer)){ContributorComparisonVersion=1,CurrentContributors=contributors,CurrentIdentities=identities,RecordedReviewerPerson=recordedReviewer};current.Require(original.Proposal,original.CurrentSource);documents.Add(current);
            }
        }
        var closed=await ReviewExecutionPermitApprovalCoreAsync(user,query,recordedDecision);
        if(closed.Context!=original.Context || closed.RecordedRevision!=original.RecordedRevision || closed.CurrentSource.SnapshotRevision!=original.CurrentSource.SnapshotRevision)throw new InvalidOperationException("Complete original permit/order/inspection/assigned-stage proof changed during prerequisite assembly.");
        var utc=DateTime.UtcNow;var policy=original.Proposal.RequirementEvidenceVersion switch{2=>7,1=>6,0=>5,_=>throw new InvalidOperationException("Unknown original requirement policy.")};var result=original with{ComparedAtUtc=utc,PrerequisiteEvidenceVersion=policy,CurrentMaterialAcceptance=material,CurrentPersonnelAcceptance=personnel,CurrentDocumentAcceptance=documents.ToArray(),Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(original.Proposal,original.OriginalSource,original.CurrentSource,utc,policy,material,personnel,documents)};result.Require(original.Context.Company,original.Context.ActorUserId,query);return result;
    }
    private async Task<CompanyWorkOrderExecutionPermitApprovalReview> ReviewExecutionPermitApprovalCoreAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query,CompanyWorkOrderExecutionPermitDecision? recordedDecision=null)
    {
        query.Require();if(executionPermits is null)throw new InvalidOperationException("The native independent work-permit review owner is unavailable.");
        var captured=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var context=await PermitAuthority(user,captured.Preference,query.Task.WorkOrderId,"HSE.ApprovePermit");
        async Task Guard(){if(await PermitAuthority(user,captured.Preference,query.Task.WorkOrderId,"HSE.ApprovePermit")!=context)throw new UnauthorizedAccessException("The independent permit review authority changed.");}
        var result=await executionPermits.ReviewApprovalAsync(context,query,Guard,()=>ReviewAssignedInspectionsAsync(user,query.Task),()=>ReviewAssignedRequirementInspectionsAsync(user,query.Task,"HSE.ApprovePermit"),recordedDecision);await Guard();result.Require(context.Company,context.ActorUserId,query);return result;
    }
    public async Task<CompanyWorkOrderExecutionPermitRecovery> RecoverExecutionPermitReferenceAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitReference query)
    {
        query.Require();if(executionPermits is null)throw new InvalidOperationException("The native work-permit reference owner is unavailable.");var captured=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var context=await PermitIssueAuthority(user,captured.Preference,query.Task.WorkOrderId);
        async Task Guard(){if(await PermitIssueAuthority(user,captured.Preference,query.Task.WorkOrderId)!=context)throw new UnauthorizedAccessException("The original permit issue authority changed.");}
        var intent=await executionPermits.ReadOriginalAsync(context,query,Guard);var receipt=await IssueExecutionPermitAsync(user,intent,true);var result=new CompanyWorkOrderExecutionPermitRecovery(intent,receipt);result.Require(context.Company,context.ActorUserId,query);return result;
    }
    private async Task<FieldSelectionReview> PermitIssueAuthority(ClaimsPrincipal user,FieldSelectionReview preference,string order)
        =>await PermitAuthority(user,preference,order,"HSE.IssuePermit");
    private async Task<FieldSelectionReview> PermitAuthority(ClaimsPrincipal user,FieldSelectionReview preference,string order,string permission)
        =>(await PermitAuthorities(user,preference,order,permission)).Core;
    private async Task<(FieldSelectionReview Lifecycle,FieldSelectionReview Core)> PermitAuthorities(ClaimsPrincipal user,FieldSelectionReview preference,string order,string permission,string? role=null,string? additionalPermission=null)
    {
        // Bracket the complete paired snapshot once; neither capture crosses a native-data read.
        await ReadAuthorityPartAsync("maintenance.authority.preference",()=>preferences.RequireStoredSelectionAsync(preference));
        var authority=await AuthoritySnapshot(user,preference,order,"WRITE",permission,role,additionalPermission);
        var core=await ReadAuthorityPartAsync("maintenance.authority.core.resolve",()=>modules.ResolveAsync("PPDM_CORE"));var database=new AssetDatabaseScope(core,await ReadAuthorityPartAsync("maintenance.authority.core.binding",()=>modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],core)));
        if(await AuthoritySnapshot(user,preference,order,"WRITE",permission,role,additionalPermission)!=authority)throw new UnauthorizedAccessException("Current permit issue authority changed.");
        await ReadAuthorityPartAsync("maintenance.authority.preference",()=>preferences.RequireStoredSelectionAsync(preference));
        if(field.CurrentFieldId!=preference.FieldId)throw new InvalidOperationException("The original FIELD changed during permit authority review.");
        return (authority,preference with{Database=database,Revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Authority=authority,Permission=permission,Database=database}))});
    }
    public async Task<CompanyWorkOrderExecutionPermitIssueReview> ReviewExecutionPermitIssueAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task)
    {
        task.Require();var captured=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitIssueAuthority(user,captured.Preference,task.WorkOrderId);
        var source=await ReviewAssignedRequirementInspectionsAsync(user,task);var result=new CompanyWorkOrderExecutionPermitIssueReview(context,source){RequirementEvidenceVersion=source.Inspections.Any(row=>row.ApplicableStage is not null)?2:1};result.Require(context.Company,context.ActorUserId,task);
        if(await PermitIssueAuthority(user,captured.Preference,task.WorkOrderId)!=context)throw new UnauthorizedAccessException("The original permit issue role/source authority changed.");return result;
    }
    public async Task<CompanyWorkOrderExecutionPermitReceipt> IssueExecutionPermitAsync(ClaimsPrincipal user,CompanyWorkOrderExecutionPermitIntent intent,bool readOnly=false)
    {
        ArgumentNullException.ThrowIfNull(intent);intent=CompanyInventorySourceReview.Copy(intent);intent.Require();
        if(executionPermits is null)throw new InvalidOperationException("The native work-order execution permit owner is unavailable.");
        var captured=await Boundary(user,intent.Task.WorkOrderId,intent.Task.FieldId);
        async Task Guard(){if(await PermitIssueAuthority(user,captured.Preference,intent.Task.WorkOrderId)!=intent.Context)throw new UnauthorizedAccessException("Return to the original permit issuer/company/FIELD/core authority.");}
        Task<CompanyMaintenanceInspectionReview> Read()=>intent.RequirementEvidenceVersion is 1 or 2?ReviewAssignedRequirementInspectionsAsync(user,intent.Task):ReviewAssignedInspectionsAsync(user,intent.Task);
        await Guard();return readOnly?await executionPermits.RecoverAsync(intent,Guard,Read):await executionPermits.IssueAsync(intent,Guard,Read);
    }
    private async Task<CompanyMaintenanceInspectionReview> ReviewAssignedRequirementInspectionsAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task,string permission="HSE.IssuePermit")
    {
        task.Require();if(workOrders is null || inspections is not ICompanyWorkOrderRequirementSource owner)throw new InvalidOperationException("Existing order and immutable native requirement source owners are unavailable.");var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,permission);
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,permission)!=context)throw new UnauthorizedAccessException("Original versioned requirement authority changed.");}
        var stage=await ReviewAssignedStageAsync(user,task);var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original versioned order is unavailable.");var requirements=await owner.ReadRequirementProofAsync(context,task.WorkOrderId,Guard);await Guard();
        var closingOrder=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original versioned order changed.");var closingStage=await ReviewAssignedStageAsync(user,task);await Guard();if(System.Text.Json.JsonSerializer.Serialize(order)!=System.Text.Json.JsonSerializer.Serialize(closingOrder) || System.Text.Json.JsonSerializer.Serialize(stage)!=System.Text.Json.JsonSerializer.Serialize(closingStage))throw new InvalidOperationException("Complete original order/assigned-stage changed during versioned requirement review.");
        var result=new CompanyMaintenanceInspectionReview(closingStage,context.Database,closingOrder,requirements,CompanyMaintenanceInspectionReview.Revision(closingStage,context.Database,closingOrder,requirements),DateTimeOffset.UtcNow);result.Require(context.Company,context.ActorUserId,task);return result;
    }
    public async Task<CompanyMaintenanceInspectionReview> ReviewAssignedInspectionsAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task)
    {
        ArgumentNullException.ThrowIfNull(task);task.Require();
        if(workOrders is null || inspections is null)throw new InvalidOperationException("The existing recorded work-order and inspection read owners are required.");
        async Task<AssetDatabaseScope> Core()
        {
            var connection=await modules.ResolveAsync("PPDM_CORE");return new(connection,await modules.GetMigrationBindingFingerprintAsync(["PPDM_CORE"],connection));
        }
        var stage=await ReviewAssignedStageAsync(user,task);var database=await Core();
        var order=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("The original recorded work-order header is unavailable.");
        var rows=await inspections.GetChecklistAsync(task.WorkOrderId,task.FieldId);
        var revision=CompanyMaintenanceInspectionReview.Revision(stage,database,order,rows);
        var result=CompanyInventorySourceReview.Copy(new CompanyMaintenanceInspectionReview(stage,database,order,rows,revision,DateTimeOffset.UtcNow));result.Require(stage.Context.Company,stage.Context.ActorUserId,task);
        var closingOrder=await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("The original recorded order changed during inspection review.");
        var closingRows=await inspections.GetChecklistAsync(task.WorkOrderId,task.FieldId);
        var closingStage=await ReviewAssignedStageAsync(user,task);var closingDatabase=await Core();
        if(revision!=CompanyMaintenanceInspectionReview.Revision(closingStage,closingDatabase,closingOrder,closingRows))throw new InvalidOperationException("The complete assigned role/order/inspection/database evidence changed during review.");
        result.Require(stage.Context.Company,stage.Context.ActorUserId,task);return result;
    }
    public async Task<CompanyMaintenanceAssignedStageReview> ReviewAssignedStageAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task)
    {
        ArgumentNullException.ThrowIfNull(task);task.Require();var captured=await Boundary(user,task.WorkOrderId,task.FieldId);
        if(captured.Process is not ICompanyMaintenanceAssignedStageReads owner)throw new InvalidOperationException("The native next-role maintenance read owner is unavailable.");
        async Task Guard(string? role)
        {
            if(await Authority(user,captured.Preference,task.WorkOrderId)!=captured.Context)throw new UnauthorizedAccessException("The next-role maintenance authority changed.");
            var access=await company.RefreshCompanyAsync();
            if(role is not null && (!user.IsInRole(role) || !access.Roles.Contains(role,StringComparer.Ordinal)))throw new UnauthorizedAccessException("The current configured next maintenance role is required.");
            if(await Authority(user,captured.Preference,task.WorkOrderId)!=captured.Context)throw new UnauthorizedAccessException("The next-role maintenance authority changed during role review.");
        }
        var result=await owner.ReviewMaintenanceAssignedStageAsync(captured.Context,task,Guard);await Guard(result.AssignedRole);result.Require(captured.Context.Company,captured.Context.ActorUserId,task);return result;
    }
    private async Task<FieldSelectionReview> Authority(ClaimsPrincipal user,FieldSelectionReview original,string order,string requiredAccess="WRITE")
    {
        await preferences.RequireStoredSelectionAsync(original);
        var result=await AuthoritySnapshot(user,original,order,requiredAccess);
        await preferences.RequireStoredSelectionAsync(original);
        if(field.CurrentFieldId!=original.FieldId)throw new InvalidOperationException("The original FIELD changed during maintenance authority review.");
        return result;
    }
    private static async Task<T> ReadAuthorityPartAsync<T>(string operation,Func<Task<T>> read)
    {using var activity=PermitProofActivities.StartActivity(operation);return await read();}
    private static async Task ReadAuthorityPartAsync(string operation,Func<Task> read)
    {using var activity=PermitProofActivities.StartActivity(operation);await read();}
    private async Task<FieldSelectionReview> AuthoritySnapshot(ClaimsPrincipal user,FieldSelectionReview original,string order,string requiredAccess,string? requiredPermission=null,string? requiredRole=null,string? additionalPermission=null)
    {
        var access=await ReadAuthorityPartAsync("maintenance.authority.app",()=>company.RefreshCompanyAsync());var actor=user.ActingUserId();
        if(user.Identity?.IsAuthenticated!=true || actor!=original.ActorUserId || access.UserId!=actor || access.Scope!=original.Company ||
            !access.Roles.Any(user.IsInRole) || field.CurrentFieldId!=original.FieldId)throw new UnauthorizedAccessException("Current standard company roles and the original FIELD/account are required.");
        if(requiredPermission is not null && !access.Permissions.Contains(requiredPermission,StringComparer.Ordinal) || additionalPermission is not null && !access.Permissions.Contains(additionalPermission,StringComparer.Ordinal) || requiredRole is not null && (!user.IsInRole(requiredRole) || !access.Roles.Contains(requiredRole,StringComparer.Ordinal)))throw new UnauthorizedAccessException("Current configured company role and required work-order capabilities are required.");
        var lifecycle=await ReadAuthorityPartAsync("maintenance.authority.lifecycle.resolve",()=>modules.ResolveAsync("LIFECYCLE"));var fingerprint=await ReadAuthorityPartAsync("maintenance.authority.lifecycle.binding",()=>modules.GetMigrationBindingFingerprintAsync(["LIFECYCLE"],lifecycle));
        var rows=await ReadAuthorityPartAsync("maintenance.authority.grants",()=>grants.ReadAsync(actor,original.Database.Fingerprint));
        foreach(var resource in new[]{("FIELD",original.FieldId!), ("WORK_ORDER",order)})
        {
            var matching=rows.Where(row=>row.UserId==actor && row.IsActive && row.OrganizationId==original.Company.BusinessAssociateId && row.DatabaseScope==original.Database.Fingerprint && row.AssetType==resource.Item1 && row.AssetId==resource.Item2).ToArray();
            if(matching.Length!=1 || !AssetAccessLevelChecks.Meets(matching[0].AccessLevel,requiredAccess))throw new UnauthorizedAccessException("Current direct FIELD and recorded order access are required.");
        }
        var revision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Preference=original,Company=access.Scope,Roles=access.Roles.Order(StringComparer.Ordinal),Permissions=access.Permissions.Order(StringComparer.Ordinal),Grants=rows.OrderBy(row=>row.Id,StringComparer.Ordinal)}));
        if(field.CurrentFieldId!=original.FieldId)throw new InvalidOperationException("The original FIELD changed during maintenance authority review.");
        return new(access.Scope,new(lifecycle,fingerprint),actor,revision,original.FieldId);
    }
    private async Task<(CompanyMaintenanceProcessStartReview Review,FieldSelectionReview Preference,IProcessService Process)> Capture(ClaimsPrincipal user,CompanyMaintenanceProcessStartQuery query)
    {
        using var requestActivity=PermitProofActivities.StartActivity("maintenance.handoff.review");
        ArgumentNullException.ThrowIfNull(query);query.Require();var current=await selection.GetCurrentFieldSelectionAsync();var access=await company.RefreshCompanyAsync();var actor=user.ActingUserId();
        FieldSelectionReviewValidator.RequireCurrent(current,access.Scope,actor);
        if(!current.SelectionAvailable || current.FieldId!=field.CurrentFieldId)throw new UnauthorizedAccessException("A current reviewed FIELD is required for maintenance handoff.");
        var original=current.SelectionContext!;var initial=await Authority(user,original,query.WorkOrderId);
        if(processes is not BoundProcessService bound)throw new InvalidOperationException("The fixed-target lifecycle process owner is required.");
        var process=await bound.CreateBoundServiceAsync(initial.Database.ConnectionName);
        var definition=await process.GetProcessDefinitionAsync(query.ProcessId)??throw new InvalidOperationException("The configured receiving workflow is unavailable.");
        RequireDefinition(definition);
        var first=definition.Steps.OrderBy(step=>step.SequenceNumber).First();
        if(!first.RequiresApproval || first.StepType!="APPROVAL" || first.RequiredRoles is not {Count:1})throw new InvalidOperationException("An explicit initial receiving review role is required.");
        ProductionInterventionMaintenanceLink source;
        using(PermitProofActivities.StartActivity("maintenance.handoff.production_link"))source=await production.ReadInterventionMaintenanceLinkAsync(query.WellId,query.DecisionId,query.WorkOrderId,actor,access.Scope);
        var closing=await Authority(user,original,query.WorkOrderId);
        var closedDefinition=await process.GetProcessDefinitionAsync(query.ProcessId);
        if(initial!=closing || JsonSerializer.Serialize(definition)!=JsonSerializer.Serialize(closedDefinition))throw new InvalidOperationException("Maintenance authority or configured definition changed during review.");
        return(new(closing,query.ProcessId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(closedDefinition)),source,definition.ProcessName,first.RequiredRoles[0]),original,process);
    }
    public async Task<CompanyMaintenanceProcessStartReview> ReviewAsync(ClaimsPrincipal user,CompanyMaintenanceProcessStartQuery query)
        =>(await Capture(user,query)).Review;
    private async Task<FieldSelectionReview> ReceivingAuthority(ClaimsPrincipal user,FieldSelectionReview preference,string order,string? role)
    {
        var context=await Authority(user,preference,order);var access=await company.RefreshCompanyAsync();
        if(!access.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal) || role is not null && (!access.Roles.Contains(role,StringComparer.Ordinal) || !user.IsInRole(role)))
            throw new UnauthorizedAccessException("The current configured app receiving role and Workflow.Approve permission are required.");
        if(await Authority(user,preference,order)!=context)throw new UnauthorizedAccessException("The receiving app authority changed during review.");return context;
    }
    public async Task<CompanyMaintenanceAcceptanceReview> ReviewAcceptanceAsync(ClaimsPrincipal user,CompanyMaintenanceRecoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var captured=await Boundary(user,query.WorkOrderId,query.FieldId);
        if(await ReceivingAuthority(user,captured.Preference,query.WorkOrderId,null)!=captured.Context)throw new UnauthorizedAccessException("The original receiving authority changed.");
        if(captured.Process is not ICompanyMaintenanceAcceptanceCommands commands)throw new InvalidOperationException("The native independent receiving owner is unavailable.");
        async Task Guard(string? role){if(await ReceivingAuthority(user,captured.Preference,query.WorkOrderId,role)!=captured.Context)throw new UnauthorizedAccessException("The original receiving authority or binding changed.");}
        var source=await production.ReadInterventionMaintenanceLinkAsync(query.WellId,query.DecisionId,query.WorkOrderId,captured.Context.ActorUserId,captured.Context.Company);
        var review=await commands.ReviewMaintenanceAcceptanceAsync(captured.Context,query,source,Guard);
        await Guard(review.AssignedRole);
        var closing=await production.ReadInterventionMaintenanceLinkAsync(query.WellId,query.DecisionId,query.WorkOrderId,captured.Context.ActorUserId,captured.Context.Company);
        if(source with{RetrievedAtUtc=closing.RetrievedAtUtc}!=closing)throw new InvalidOperationException("The approved receiving source changed during role review.");
        await Guard(review.AssignedRole);review.Require();return review;
    }
    public async Task<CompanyMaintenanceAcceptanceReview> ReviewReceivingTaskAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task)
    {
        ArgumentNullException.ThrowIfNull(task);task.Require();var captured=await Boundary(user,task.WorkOrderId,task.FieldId);
        if(captured.Process is not ICompanyMaintenanceAcceptanceCommands commands)throw new InvalidOperationException("The native independent receiving owner is unavailable.");
        async Task Guard(string? role){if(await ReceivingAuthority(user,captured.Preference,task.WorkOrderId,role)!=captured.Context)throw new UnauthorizedAccessException("The original receiving task authority changed.");}
        var assignment=await commands.DiscoverMaintenanceReceivingAssignmentAsync(captured.Context,task,Guard);
        var review=await ReviewAcceptanceAsync(user,assignment);
        if(review.InstanceId!=task.InstanceId || review.Context!=captured.Context)throw new InvalidOperationException("The original receiving task changed during review.");return review;
    }
    public async Task<CompanyMaintenanceAcceptanceRecovery> RecoverAcceptanceReferenceAsync(ClaimsPrincipal user,CompanyMaintenanceAcceptanceReferenceQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var captured=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);
        if(captured.Process is not ICompanyMaintenanceAcceptanceCommands commands)throw new InvalidOperationException("The native independent receiving owner is unavailable.");
        async Task Guard(string? role){if(await ReceivingAuthority(user,captured.Preference,query.Task.WorkOrderId,role)!=captured.Context)throw new UnauthorizedAccessException("The original acceptance-reference authority changed.");}
        var command=await commands.ReadOriginalMaintenanceAcceptanceAsync(captured.Context,query,Guard);
        var receipt=await RecoverAcceptanceAsync(user,command);var result=new CompanyMaintenanceAcceptanceRecovery(command,receipt);result.Require(captured.Context.Company,captured.Context.ActorUserId,query);return result;
    }
    public Task<CompanyMaintenanceAcceptanceReceipt> AcceptAsync(ClaimsPrincipal user,CompanyMaintenanceAcceptanceCommand command)
        =>ReadOrAcceptAsync(user,command,false);
    public Task<CompanyMaintenanceAcceptanceReceipt> RecoverAcceptanceAsync(ClaimsPrincipal user,CompanyMaintenanceAcceptanceCommand command)
        =>ReadOrAcceptAsync(user,command,true);
    private async Task<CompanyMaintenanceAcceptanceReceipt> ReadOrAcceptAsync(ClaimsPrincipal user,CompanyMaintenanceAcceptanceCommand command,bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(command);command=JsonSerializer.Deserialize<CompanyMaintenanceAcceptanceCommand>(JsonSerializer.Serialize(command))!;command.Require();
        var review=command.Review;var captured=await Boundary(user,review.Assignment.WorkOrderId,review.Assignment.FieldId);
        if(await ReceivingAuthority(user,captured.Preference,review.Assignment.WorkOrderId,review.AssignedRole)!=review.Context || captured.Context!=review.Context)
            throw new UnauthorizedAccessException("The original independent receiving app authority is required.");
        if(captured.Process is not ICompanyMaintenanceAcceptanceCommands commands)throw new InvalidOperationException("The native independent receiving owner is unavailable.");
        async Task Guard(string? role){if(await ReceivingAuthority(user,captured.Preference,review.Assignment.WorkOrderId,role)!=review.Context)throw new UnauthorizedAccessException("The original receiving app authority or binding changed.");}
        Task<ProductionInterventionMaintenanceLink> Read()=>production.ReadInterventionMaintenanceLinkAsync(review.Assignment.WellId,review.Assignment.DecisionId,review.Assignment.WorkOrderId,review.Context.ActorUserId,review.Context.Company);
        return readOnly?await commands.RecoverMaintenanceAcceptanceAsync(command,Guard,Read):await commands.AcceptMaintenanceAsync(command,Guard,Read);
    }
    private static void RequireDefinition(ProcessDefinition definition)
    {
        if(!definition.IsActive || definition.ProcessType!="WORK_ORDER" || definition.EntityType!="WORK_ORDER" || !LeaseSummaryValidator.Identifier(definition.ProcessId) || string.IsNullOrWhiteSpace(definition.ProcessName) || definition.Steps is not {Count:>0 and <=64} || definition.Steps.Any(step=>step is null))
            throw new InvalidOperationException("An active bounded recorded WORK_ORDER receiving workflow is required.");
        var steps=definition.Steps.OrderBy(step=>step.SequenceNumber).ToArray();
        if(steps.Any(step=>!LeaseSummaryValidator.Identifier(step.StepId) || step.SequenceNumber<0 || step.SlaHours<0 || step.RequiredRoles is not {Count:1} || !LeaseSummaryValidator.Identifier(step.RequiredRoles[0])) || steps.Select(step=>step.StepId).Distinct(StringComparer.Ordinal).Count()!=steps.Length || steps.Select(step=>step.SequenceNumber).Distinct().Count()!=steps.Length || !steps[0].RequiresApproval || steps[0].StepType!="APPROVAL")
            throw new InvalidOperationException("Explicit unique maintenance role steps and an initial receiving approval are required.");
    }
    private async Task<(FieldSelectionReview Preference,FieldSelectionReview Context,IProcessService Process)> Boundary(ClaimsPrincipal user,string order,string? requiredField=null,string requiredAccess="WRITE")
    {
        var current=await selection.GetCurrentFieldSelectionAsync();var access=await company.RefreshCompanyAsync();var actor=user.ActingUserId();
        FieldSelectionReviewValidator.RequireCurrent(current,access.Scope,actor);
        if(!current.SelectionAvailable || current.FieldId!=field.CurrentFieldId || requiredField is not null && current.FieldId!=requiredField)
            throw new UnauthorizedAccessException("The original selected maintenance FIELD/account is required.");
        var preference=current.SelectionContext!;var context=await Authority(user,preference,order,requiredAccess);
        if(processes is not BoundProcessService bound)throw new InvalidOperationException("The fixed-target lifecycle process owner is required.");
        return(preference,context,await bound.CreateBoundServiceAsync(context.Database.ConnectionName));
    }
    public async Task<CompanyMaintenanceStartCatalog> CatalogAsync(ClaimsPrincipal user,CompanyMaintenanceSourceQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var captured=await Boundary(user,query.WorkOrderId);
        var definitions=await captured.Process.GetProcessDefinitionsByTypeAsync("WORK_ORDER");
        if(definitions is null || definitions.Count>64 || definitions.Any(value=>value is null) || definitions.Select(value=>value.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)
            throw new InvalidOperationException("The configured maintenance workflow list is unavailable or ambiguous.");
        var snapshot=JsonSerializer.Serialize(definitions);var source=await production.ReadInterventionMaintenanceLinkAsync(query.WellId,query.DecisionId,query.WorkOrderId,captured.Context.ActorUserId,captured.Context.Company);
        var choices=new List<CompanyMaintenanceProcessStartReview>();
        foreach(var definition in definitions.Where(value=>value.IsActive && value.EntityType=="WORK_ORDER"))
        {
            RequireDefinition(definition);var first=definition.Steps.OrderBy(step=>step.SequenceNumber).First();
            choices.Add(new(captured.Context,definition.ProcessId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),source,definition.ProcessName,first.RequiredRoles[0]));
        }
        if(await Authority(user,captured.Preference,query.WorkOrderId)!=captured.Context || JsonSerializer.Serialize(await captured.Process.GetProcessDefinitionsByTypeAsync("WORK_ORDER"))!=snapshot)
            throw new InvalidOperationException("Maintenance authority or configured workflow choices changed during review.");
        var catalog=new CompanyMaintenanceStartCatalog(captured.Context,source,choices.AsReadOnly());catalog.Require(captured.Context.Company,captured.Context.ActorUserId,query);return catalog;
    }
    public async Task<CompanyMaintenanceRecovery> RecoverReferenceAsync(ClaimsPrincipal user,CompanyMaintenanceRecoveryQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);query.Require();var captured=await Boundary(user,query.WorkOrderId,query.FieldId);
        if(captured.Process is not ICompanyMaintenanceProcessCommands commands)throw new InvalidOperationException("The original maintenance command owner is unavailable.");
        async Task Guard(){if(await Authority(user,captured.Preference,query.WorkOrderId)!=captured.Context)throw new UnauthorizedAccessException("The original maintenance app authority or lifecycle binding changed.");}
        var intent=await commands.ReadOriginalMaintenanceIntentAsync(captured.Context,query,Guard);
        var receipt=await RecoverAsync(user,intent);var result=new CompanyMaintenanceRecovery(intent,receipt);
        result.Require(captured.Context.Company,captured.Context.ActorUserId,query);return result;
    }
    public async Task<CompanyMaintenanceProcessStartReceipt> StartAsync(ClaimsPrincipal user,CompanyMaintenanceProcessStart intent)
        => await ReadOrStartAsync(user,intent,false);
    public async Task<CompanyMaintenanceProcessStartReceipt> RecoverAsync(ClaimsPrincipal user,CompanyMaintenanceProcessStart intent)
        => await ReadOrStartAsync(user,intent,true);
    private async Task<CompanyMaintenanceProcessStartReceipt> ReadOrStartAsync(ClaimsPrincipal user,CompanyMaintenanceProcessStart intent,bool readOnly)
    {
        ArgumentNullException.ThrowIfNull(intent);intent=JsonSerializer.Deserialize<CompanyMaintenanceProcessStart>(JsonSerializer.Serialize(intent))!;intent.Require();
        var captured=await Boundary(user,intent.Source.WorkOrderId,intent.Context.FieldId);
        if(captured.Context!=intent.Context)throw new UnauthorizedAccessException("The original maintenance company/account/FIELD/lifecycle authority is required.");
        if(captured.Process is not ICompanyMaintenanceProcessCommands commands)throw new InvalidOperationException("The original native maintenance command owner is unavailable.");
        async Task Guard(){if(await Authority(user,captured.Preference,intent.Source.WorkOrderId)!=intent.Context)throw new UnauthorizedAccessException("The original maintenance app authority or lifecycle binding changed.");}
        Task<ProductionInterventionMaintenanceLink> Read()=>production.ReadInterventionMaintenanceLinkAsync(intent.Source.WellId,intent.Source.DecisionId,intent.Source.WorkOrderId,intent.Context.ActorUserId,intent.Context.Company);
        return readOnly ? await commands.RecoverMaintenanceProcessAsync(intent,Guard,Read) : await commands.StartMaintenanceProcessAsync(intent,Guard,Read);
    }
}
