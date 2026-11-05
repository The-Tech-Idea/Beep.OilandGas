#if WORKFLOW_INBOX_FOCUSED || RECORDED_APPROVAL_HTTP_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Row=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
using Detail=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
using Rules=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
using Item=actualOwners::Beep.OilandGas.Models.Data.ProductionAccounting.INVENTORY_ITEM;
namespace Beep.OilandGas.ApiService.Tests;
internal sealed class WorkOrderMaterialAcceptanceFixture
{
    internal readonly Row Pending;
    internal readonly AssetDatabaseScope InventoryDatabase;
    internal readonly WORK_ORDER_MATERIAL_ALLOCATION Allocation;
    internal readonly Item Inventory=new(){INVENTORY_ITEM_ID="item-الكويت",ITEM_NAME="Recorded spare",OWNER_TENANT_ID="tenant",OWNER_COMPANY_BA_ID="company",ACTIVE_IND="Y",WAREHOUSE_ID="warehouse",LOCATION_ID="bin",UNIT_OF_MEASURE="EA",QUANTITY_ON_HAND=5m};
    internal readonly CompanyWorkOrderExecutionPermitIntent Permit;
    internal readonly actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT StoredPermit;
    internal readonly CompanyMaintenanceInspectionReview IssuedSource;
    internal readonly CompanyMaintenanceAssignedStageReview ReviewerStage;
    internal readonly CompanyWorkOrderMaterialAcceptanceCommand Command;
    internal WorkOrderMaterialAcceptanceFixture(string workOrderType="",bool requirementProof=false,AssetDatabaseScope? inventoryDatabase=null,AssetDatabaseScope? coreDatabase=null,string fieldId="field",CompanyMaintenanceReceivingQuery? taskOverride=null)
    {
        InventoryDatabase=inventoryDatabase??new("production","production-fingerprint");var utc=DateTime.UtcNow.AddMinutes(-2);var company=new CompanyScope("tenant","company");var core=coreDatabase??new AssetDatabaseScope("lifecycle","core-fingerprint");var context=new FieldSelectionReview(company,core,"issuer","issue-authority",fieldId);var task=taskOverride??new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),fieldId,"order-الكويت");
        var stage=new CompanyMaintenanceAssignedStageReview(context,task,"step","execute","Recorded duty","ConfiguredRole","receiver","history","approval",new string('A',64),new string('B',64));var order=new Detail{WoSubType=workOrderType,InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",State="",EvidenceUnavailableReason="No execution state",Components=[new(task.WorkOrderId,"component",task.FieldId,null,null)]};var initial=new CompanyMaintenanceInspectionReview(stage,core,order,[],CompanyMaintenanceInspectionReview.Revision(stage,core,order,[]),new DateTimeOffset(utc));
        var query=new CompanyWorkOrderMaterialSourceQuery(task,Inventory.INVENTORY_ITEM_ID,2m,"EA");var sourceInventory=CompanyInventorySourceReview.Create(company,"issuer",Inventory,[]);var source=new CompanyWorkOrderMaterialSourceReview(context with{Database=InventoryDatabase},query,initial,sourceInventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,sourceInventory));var review=CompanyWorkOrderMaterialAllocationReview.Create(source,[],utc);var reservation=new CompanyWorkOrderMaterialAllocationCommand(review,Guid.NewGuid(),utc,utc.AddHours(3),"Private original logistics rationale");Allocation=reservation.Prepare(utc);
        Pending=Rules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=1,CondType="PRE_START",RequiredText="Review exact materials",Jurisdiction="Reported jurisdiction",RegulatoryRef=Allocation.ALLOCATION_ID,PermitPrerequisiteKind="MATERIAL"},task.WorkOrderId,"issuer","inspection",utc);var rows=new[]{requirementProof?Rules.ProjectWithRequirementProof(Pending):Rules.Project(Pending)};IssuedSource=initial with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,core,order,rows)};
        Permit=new(context,task,Guid.NewGuid(),IssuedSource.SnapshotRevision,"ElectricalIsolation","Reported jurisdiction","Original work","isolation","risk","personnel",Allocation.ALLOCATION_ID,utc.AddMinutes(1),utc.AddHours(2)){RequirementEvidenceVersion=requirementProof?1:0};StoredPermit=Permit.Prepare(utc,IssuedSource);ReviewerStage=stage with{Context=context with{ActorUserId="independent",Revision="review-authority"}};var ready=Current();Command=new(ready,Pending.INSPECTION_ID,Guid.NewGuid(),"Explicit independent material acceptance");Command.Require();
    }
    internal CompanyWorkOrderMaterialReadinessReview Current(Row? accepted=null)
    {
        var utc=DateTime.UtcNow;var inspections=new[]{Permit.RequirementEvidenceVersion==1?Rules.ProjectWithRequirementProof(accepted??Pending):Rules.Project(accepted??Pending)};var source=IssuedSource with{Stage=ReviewerStage,Inspections=inspections,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(ReviewerStage,IssuedSource.InspectionDatabase,IssuedSource.Order,inspections),ReviewedAtUtc=new DateTimeOffset(utc)};var permit=new CompanyWorkOrderExecutionPermitApprovalReview(ReviewerStage.Context,Permit,IssuedSource,source,ProcessDefinitionRevision.FromJson(System.Text.Json.JsonSerializer.Serialize(StoredPermit)),utc,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(Permit,IssuedSource,source,utc));
        var reservation=CompanyWorkOrderMaterialReservationFacts.From(Allocation);var inventory=CompanyInventorySourceReview.Create(Permit.Context.Company,"independent",Inventory,[]);var query=new CompanyWorkOrderMaterialSourceQuery(Permit.Task,reservation.ItemId,2m,"EA");var materialSource=new CompanyWorkOrderMaterialSourceReview(ReviewerStage.Context with{Database=InventoryDatabase},query,source,inventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,inventory));var material=CompanyWorkOrderMaterialAllocationReview.Create(materialSource,[Allocation],utc);var request=new CompanyWorkOrderMaterialReadinessQuery(new(Permit.Task,Permit.IntentId),reservation.IntentId);return new(request,permit,reservation,material,utc,CompanyWorkOrderMaterialReadinessReview.GetBlockers(permit,reservation,material,utc));
    }
}
#endif
