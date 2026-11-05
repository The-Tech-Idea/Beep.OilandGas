using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderInspectionSchemaTests
{
    [Fact]
    public void AssetReturnIntentUsesCanonicalCoreManifestAndCannotPartiallyRegisterOnCollision()
    {
        var type=typeof(TheTechIdea.Data.OilGas.WORK_ORDER_ASSET_RETURN);
        Assert.Contains(type,WorkOrderSchema.EntityTypes);
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));
        var row=metadata["WORK_ORDER_ASSET_RETURN"];
        Assert.Equal(type.FullName,row.EntityTypeName);Assert.Equal("PPDM_CORE",row.Module);Assert.Equal("RETURN_ID",row.PrimaryKeyColumn);
        Assert.Equal("RETURN_ID",Assert.Single(type.GetProperties().Where(property=>Attribute.IsDefined(property,typeof(System.ComponentModel.DataAnnotations.KeyAttribute)))).Name);
        foreach(var column in row.CommonColumns)Assert.NotNull(type.GetProperty(column));
        Assert.Contains(row.ForeignKeys,key=>key.ForeignKeyColumn=="SUBMISSION_ID" && key.ReferencedTable=="WORK_ORDER_EXECUTION_SUBMISSION");
        foreach(var (name,capacity) in new[]{("COMMAND_JSON",1024*1024),("BEFORE_JSON",2*1024*1024),("AFTER_JSON",2*1024*1024)})
            Assert.Equal(capacity,Assert.Single(type.GetProperty(name)!.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute),true).Cast<System.ComponentModel.DataAnnotations.MaxLengthAttribute>()).Length);
        foreach(var table in new[]{"WORK_ORDER_ASSET_RETURN","WORK_ORDER_INSPECTION","WORK_ORDER_EXECUTION_PERMIT","WORK_ORDER_LABOR_USAGE"})
        {
            var conflict=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{table.ToLowerInvariant(),new(){PrimaryKeyColumn="foreign"}}};
            Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(conflict));Assert.Single(conflict);Assert.Equal("foreign",conflict[table].PrimaryKeyColumn);
        }
    }
    [Fact] public void LaborManifestHasNativeIdentityParentKeysAndCollisionCannotPartiallyRegisterCore()
    {
        var type=typeof(TheTechIdea.Data.OilGas.WORK_ORDER_LABOR_USAGE);Assert.Equal("TheTechIdea.Data",type.Assembly.GetName().Name);
        Assert.Equal("LABOR_ID",Assert.Single(type.GetProperties().Where(property=>property.IsDefined(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true))).Name);
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var row=metadata["WORK_ORDER_LABOR_USAGE"];
        Assert.Equal("PPDM_CORE",row.Module);Assert.Equal(type.FullName,row.EntityTypeName);Assert.Equal("LABOR_ID",row.PrimaryKeyColumn);
        Assert.Contains(row.ForeignKeys,key=>key.ForeignKeyColumn=="START_ID" && key.ReferencedTable=="WORK_ORDER_EXECUTION_START");
        Assert.Contains(row.ForeignKeys,key=>key.ForeignKeyColumn=="ASSIGNMENT_ID" && key.ReferencedTable=="WORK_ORDER_PERSONNEL_ASSIGNMENT");
        var conflict=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_labor_usage",new(){PrimaryKeyColumn="foreign"}}};
        Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(conflict));Assert.Single(conflict);
        Assert.Equal(typeof(decimal),type.GetProperty("QUANTITY")!.PropertyType);Assert.Equal(typeof(DateTime),type.GetProperty("LABOR_FROM_UTC")!.PropertyType);
    }
    [Fact] public void WorkSubmissionUsesOneExplicitBeepOwnerAndRefusesMetadataCollision()
    {
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var entry=metadata["WORK_ORDER_EXECUTION_SUBMISSION"];var type=typeof(TheTechIdea.Data.OilGas.WORK_ORDER_EXECUTION_SUBMISSION);
        Assert.Equal(type.FullName,entry.EntityTypeName);Assert.Equal("SUBMISSION_ID",entry.PrimaryKeyColumn);Assert.Equal("PPDM_CORE",entry.Module);Assert.Contains(type,WorkOrderSchema.EntityTypes);
        Assert.Contains(entry.ForeignKeys,row=>row.ForeignKeyColumn=="START_ID" && row.ReferencedTable=="WORK_ORDER_EXECUTION_START");Assert.Contains(entry.ForeignKeys,row=>row.ForeignKeyColumn=="PERMIT_ID" && row.ReferencedTable=="WORK_ORDER_EXECUTION_PERMIT");
        foreach(var column in entry.CommonColumns)Assert.NotNull(type.GetProperty(column));
        var collision=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_execution_submission",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(collision));Assert.Single(collision);
    }
    [Fact] public void ExecutionStartHasOneExplicitBeepOwnerAndCollisionCannotPartiallyRegisterOtherTables()
    {
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var entry=metadata["WORK_ORDER_EXECUTION_START"];var type=typeof(TheTechIdea.Data.OilGas.WORK_ORDER_EXECUTION_START);
        Assert.Equal(type.FullName,entry.EntityTypeName);Assert.Equal("START_ID",entry.PrimaryKeyColumn);Assert.Equal("PPDM_CORE",entry.Module);Assert.Contains(type,WorkOrderSchema.EntityTypes);
        Assert.Contains(entry.ForeignKeys,row=>row.ForeignKeyColumn=="PERMIT_ID" && row.ReferencedTable=="WORK_ORDER_EXECUTION_PERMIT");Assert.Contains(entry.ForeignKeys,row=>row.ForeignKeyColumn=="EMPLOYEE_BA_ID" && row.ReferencedTable=="BUSINESS_ASSOCIATE");
        foreach(var column in entry.CommonColumns)Assert.NotNull(type.GetProperty(column));
        var collision=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_execution_start",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(collision));Assert.Single(collision);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public void DocumentCompositeRelationshipCompletionIsRepeatableAndConflictsCannotPartiallyMutate(bool conflict)
    {
        var parent=new PPDMTableMetadata{PrimaryKeyColumn="INFO_ITEM_SUBTYPE,INFORMATION_ITEM_ID"};
        PPDMTableMetadata Child()=>new(){ForeignKeys=[new(){ForeignKeyColumn="INFO_ITEM_SUBTYPE",ReferencedTable="RM_INFORMATION_ITEM",ReferencedPrimaryKey="INFO_ITEM_SUBTYPE"}]};
        var component=Child();var file=Child();if(conflict)file.ForeignKeys.Add(new(){ForeignKeyColumn="INFORMATION_ITEM_ID",ReferencedTable="other",ReferencedPrimaryKey="other"});var metadata=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"RM_INFORMATION_ITEM",parent},{"WORK_ORDER_COMPONENT",component},{"RM_FILE_CONTENT",file}};
        if(conflict){Assert.Throws<InvalidOperationException>(()=>WorkOrderDocumentSourceMetadata.Complete(metadata));Assert.Single(component.ForeignKeys);return;}
        WorkOrderDocumentSourceMetadata.Complete(metadata);WorkOrderDocumentSourceMetadata.Complete(metadata);Assert.Equal(2,component.ForeignKeys.Count);Assert.Equal(2,file.ForeignKeys.Count);Assert.Contains(component.ForeignKeys,value=>value.ForeignKeyColumn=="INFORMATION_ITEM_ID" && value.ReferencedTable=="RM_INFORMATION_ITEM" && value.ReferencedPrimaryKey=="INFORMATION_ITEM_ID");
    }
    [Fact] public void PersonnelPolicyHasOneCanonicalBeepCoreOwnerAndNoPartialMetadataRegistrationOnCollision()
    {
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var entry=metadata["WORK_ORDER_PERSONNEL_POLICY"];Assert.Equal(typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_POLICY).FullName,entry.EntityTypeName);Assert.Equal("POLICY_RECORD_ID",entry.PrimaryKeyColumn);Assert.Equal("PPDM_CORE",entry.Module);Assert.Contains(typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_POLICY),WorkOrderSchema.EntityTypes);
        var collision=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_personnel_policy",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(collision));Assert.Single(collision);Assert.False(collision.ContainsKey("WORK_ORDER_INSPECTION"));Assert.False(collision.ContainsKey("WORK_ORDER_PERSONNEL_RELEASE"));
    }
    [Fact] public void PersonnelReleaseUsesCanonicalCoreManifestAndCollisionCannotPartiallyRegisterOtherEntities()
    {
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var entry=metadata["WORK_ORDER_PERSONNEL_RELEASE"];var type=typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_RELEASE);
        Assert.Equal(type.FullName,entry.EntityTypeName);Assert.Equal("RELEASE_ID",entry.PrimaryKeyColumn);Assert.Equal("PPDM_CORE",entry.Module);Assert.Contains(type,WorkOrderSchema.EntityTypes);Assert.Contains(entry.ForeignKeys,row=>row.ForeignKeyColumn=="ASSIGNMENT_ID" && row.ReferencedTable=="WORK_ORDER_PERSONNEL_ASSIGNMENT");foreach(var column in entry.CommonColumns)Assert.NotNull(type.GetProperty(column));
        var conflict=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_personnel_release",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(conflict));Assert.Single(conflict);Assert.False(conflict.ContainsKey("WORK_ORDER_PERSONNEL_ASSIGNMENT"));Assert.False(conflict.ContainsKey("WORK_ORDER_INSPECTION"));
    }
    [Fact] public void NativePersonnelAssignmentHasOneCoreMetadataOwnerAndCollisionCannotPartiallyRegister()
    {
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var row=metadata["WORK_ORDER_PERSONNEL_ASSIGNMENT"];var type=typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_ASSIGNMENT);
        Assert.Equal(type.FullName,row.EntityTypeName);Assert.Equal("ASSIGNMENT_ID",row.PrimaryKeyColumn);Assert.Equal("PPDM_CORE",row.Module);Assert.Contains(type,WorkOrderSchema.EntityTypes);
        foreach(var column in row.CommonColumns)Assert.NotNull(type.GetProperty(column));
        Assert.Contains(row.ForeignKeys,key=>key.ForeignKeyColumn=="EMPLOYEE_BA_ID" && key.ReferencedTable=="BUSINESS_ASSOCIATE");
        var conflict=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_personnel_assignment",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(conflict));Assert.Single(conflict);Assert.False(conflict.ContainsKey("WORK_ORDER_INSPECTION"));Assert.False(conflict.ContainsKey("WORK_ORDER_EXECUTION_PERMIT"));
    }
    [Theory][InlineData("ISOLATION")][InlineData("RISK")][InlineData("PERSONNEL")][InlineData("MATERIAL")]
    public void ExplicitPrerequisiteKindIsRetainedInRequirementsResultsAndRevision(string kind)
    {
        var request=Request();request.PermitPrerequisiteKind=kind;request.RegulatoryRef="original-source-reference";var row=WorkOrderInspectionRules.PrepareRequirement(request,"order","planner","inspection",DateTime.UtcNow);var original=WorkOrderInspectionRules.Project(row);Assert.Equal(kind,original.PermitPrerequisiteKind);
        var result=WorkOrderInspectionRules.PrepareResult(row,new(){FieldId="field",InspectionId="inspection",ExpectedSourceRevision=original.SourceRevision,Result="PASS",Notes="Recorded independent evidence"},"independent",DateTime.UtcNow);var saved=WorkOrderInspectionRules.Project(result);Assert.Equal(kind,saved.PermitPrerequisiteKind);Assert.NotEqual(original.SourceRevision,saved.SourceRevision);WorkOrderInspectionRules.RequireReadback(result,result);
        var changed=WorkOrderInspectionRules.PrepareResult(row,new(){FieldId="field",InspectionId="inspection",ExpectedSourceRevision=original.SourceRevision,Result="PASS",Notes="Recorded independent evidence"},"independent",DateTime.UtcNow);changed.PERMIT_PREREQUISITE_KIND=kind=="RISK"?"ISOLATION":"RISK";Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.RequireReadback(result,changed));
    }
    [Theory][InlineData("unknown")][InlineData("reference")][InlineData("category")]
    public void ImplicitOrUnreferencedPrerequisiteCannotBePrepared(string fault)
    {var request=Request();request.PermitPrerequisiteKind=fault=="unknown"?"OTHER":"ISOLATION";request.RegulatoryRef=fault=="reference"?"":"reference";if(fault=="category")request.CondType=ConditionType.CloseOut;Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.PrepareRequirement(request,"order","planner","inspection",DateTime.UtcNow));}
    [Fact] public void GeneralInspectionKeepsHistoricalRevisionAndJsonWhenKindIsAbsent()
    {
        var row=Row();var body=System.Text.Json.JsonSerializer.Serialize(new{row.INSPECTION_ID,row.FIELD_ID,row.WORK_ORDER_ID,row.COND_SEQ,row.COND_TYPE,row.REQUIRED_TEXT,row.JURISDICTION,row.REGULATORY_REF,row.REQUIREMENT_ACTOR,row.RESULT_STATUS,row.RESULT_NOTES,row.INSPECTOR,row.INSPECTED_AT_UTC,row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE,row.SOURCE,row.PPDM_GUID});Assert.Equal(TheTechIdea.Data.OilGas.ProcessDefinitionRevision.FromJson(body),WorkOrderInspectionRules.Revision(row));Assert.DoesNotContain("PermitPrerequisiteKind",System.Text.Json.JsonSerializer.Serialize(WorkOrderInspectionRules.Project(row)));Assert.DoesNotContain("PERMIT_PREREQUISITE_KIND",System.Text.Json.JsonSerializer.Serialize(row));
    }
    private static AddWorkOrderInspectionRequirementRequest Request()=>new(){FieldId="field",CondSeq=1,CondType=ConditionType.PreStart,RequiredText="Recorded isolation requirement",Jurisdiction="Recorded authority",RegulatoryRef="Reviewed reference"};
    [Fact] public void DocumentAcceptanceProofExtendsExistingBeepInspectionWithNullableBoundedColumnsAndPreservesOldJson()
    {
        var type=typeof(WORK_ORDER_INSPECTION);foreach(var (name,size) in new[]{("DOCUMENT_ACCEPTANCE_DATA_JSON",8*1024*1024),("DOCUMENT_ACCEPTANCE_REVISION",64)}){var property=type.GetProperty(name)!;Assert.Equal(typeof(string),property.PropertyType);Assert.Equal(size,((System.ComponentModel.DataAnnotations.MaxLengthAttribute)property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.MaxLengthAttribute),true).Single()).Length);Assert.Equal(System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,((System.Text.Json.Serialization.JsonIgnoreAttribute)property.GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute),true).Single()).Condition);}
        Assert.DoesNotContain("DOCUMENT_ACCEPTANCE",System.Text.Json.JsonSerializer.Serialize(Row()));Assert.Contains(type,WorkOrderSchema.EntityTypes);
    }
    private static WORK_ORDER_INSPECTION Row()=>WorkOrderInspectionRules.PrepareRequirement(Request(),"order","planner","inspection",DateTime.UtcNow);
    [Fact] public void ExplicitBusinessManifestIsDataOwnedAndAllowedInBeepModuleScope(){Assert.Equal(new[]{typeof(WORK_ORDER_INSPECTION),typeof(WORK_ORDER_EXECUTION_PERMIT),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_ASSIGNMENT),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_RELEASE),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_POLICY),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_MAINTENANCE_RETURN_POLICY),typeof(TheTechIdea.Data.OilGas.COMPANY_PERSONNEL_IDENTITY),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_EXECUTION_START),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_EXECUTION_SUBMISSION),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_LABOR_USAGE),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_ASSET_RETURN)},WorkOrderSchema.EntityTypes);Assert.All(WorkOrderSchema.EntityTypes,type=>Assert.Equal("TheTechIdea.Data",type.Assembly.GetName().Name));ModuleSchemaBoundary.Validate(WorkOrderSchema.EntityTypes);Assert.Equal("INSPECTION_ID",Assert.Single(typeof(WORK_ORDER_INSPECTION).GetProperties().Where(p=>p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true).Length>0)).Name);Assert.Equal("PERMIT_ID",Assert.Single(typeof(WORK_ORDER_EXECUTION_PERMIT).GetProperties().Where(p=>p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute),true).Length>0)).Name);}
    [Fact] public void MetadataMatchesRealEntityAndRecordedParentKeys(){var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var entry=metadata["WORK_ORDER_INSPECTION"];Assert.Equal(typeof(WORK_ORDER_INSPECTION).FullName,entry.EntityTypeName);Assert.Equal("INSPECTION_ID",entry.PrimaryKeyColumn);Assert.Equal("PPDM_CORE",entry.Module);Assert.Contains(entry.ForeignKeys,x=>x.ForeignKeyColumn=="WORK_ORDER_ID" && x.ReferencedTable=="WORK_ORDER");Assert.Contains(entry.ForeignKeys,x=>x.ForeignKeyColumn=="FIELD_ID" && x.ReferencedTable=="FIELD");foreach(var column in entry.CommonColumns)Assert.NotNull(typeof(WORK_ORDER_INSPECTION).GetProperty(column));}
    [Fact] public void AnotherMetadataOwnerCannotSilentlyOverrideInspection(){var metadata=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_inspection",new(){PrimaryKeyColumn="other"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(metadata));Assert.Equal("other",metadata["WORK_ORDER_INSPECTION"].PrimaryKeyColumn);}
    [Fact] public void ExecutionPermitMetadataIsCoreOwnedAndCollisionCannotPartiallyRegisterInspection()
    {
        var metadata=WorkOrderInspectionMetadata.AddTo(new(StringComparer.OrdinalIgnoreCase));var permit=metadata["WORK_ORDER_EXECUTION_PERMIT"];Assert.Equal("PERMIT_ID",permit.PrimaryKeyColumn);Assert.Equal(typeof(WORK_ORDER_EXECUTION_PERMIT).FullName,permit.EntityTypeName);Assert.Equal("PPDM_CORE",permit.Module);Assert.Contains(permit.ForeignKeys,key=>key.ForeignKeyColumn=="WORK_ORDER_ID" && key.ReferencedTable=="WORK_ORDER");Assert.Contains(permit.ForeignKeys,key=>key.ForeignKeyColumn=="FIELD_ID" && key.ReferencedTable=="FIELD");
        var conflict=new Dictionary<string,PPDMTableMetadata>(StringComparer.OrdinalIgnoreCase){{"work_order_execution_permit",new(){PrimaryKeyColumn="foreign"}}};Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionMetadata.AddTo(conflict));Assert.Single(conflict);Assert.False(conflict.ContainsKey("WORK_ORDER_INSPECTION"));
    }
    [Fact] public void NewRequirementHasNoInventedInspectorOrPass(){var row=Row();Assert.Equal("planner",row.REQUIREMENT_ACTOR);Assert.Equal("Recorded authority",row.JURISDICTION);Assert.Equal("",row.RESULT_STATUS);Assert.Equal("",row.INSPECTOR);Assert.Null(row.INSPECTED_AT_UTC);WorkOrderInspectionRules.Require("field","order",[row]);Assert.Equal(64,WorkOrderInspectionRules.Revision(row).Length);}
    [Theory][InlineData("field")][InlineData("sequence")][InlineData("type")][InlineData("text")][InlineData("jurisdiction")][InlineData("actor")][InlineData("order")][InlineData("identity")][InlineData("date")][InlineData("zone")]
    public void IncompleteRequirementCannotBePrepared(string fault){var r=Request();var actor="planner";var order="order";var id="inspection";var utc=DateTime.UtcNow;switch(fault){case "field":r.FieldId="";break;case "sequence":r.CondSeq=0;break;case "type":r.CondType="OTHER";break;case "text":r.RequiredText="";break;case "jurisdiction":r.Jurisdiction="";break;case "actor":actor="";break;case "order":order="";break;case "identity":id="";break;case "date":utc=default;break;default:utc=DateTime.SpecifyKind(utc,DateTimeKind.Unspecified);break;}Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.PrepareRequirement(r,order,actor,id,utc));}
    [Theory][InlineData("field")][InlineData("order")][InlineData("inactive")][InlineData("identity")][InlineData("duplicate")][InlineData("sequence")][InlineData("status")][InlineData("inspector")][InlineData("date")][InlineData("notes")][InlineData("unrecorded")]
    public void UnconfirmedOrFabricatedInspectionEvidenceIsRefused(string fault){var row=Row();row.RESULT_STATUS="PASS";row.INSPECTOR="technician";row.RESULT_NOTES="Recorded isolation evidence";row.INSPECTED_AT_UTC=DateTime.UtcNow;switch(fault){case "field":row.FIELD_ID="other";break;case "order":row.WORK_ORDER_ID="other";break;case "inactive":row.ACTIVE_IND="N";break;case "identity":row.INSPECTION_ID="";break;case "sequence":row.COND_SEQ=0;break;case "status":row.RESULT_STATUS="COMPLETED";break;case "inspector":row.INSPECTOR="";break;case "date":row.INSPECTED_AT_UTC=null;break;case "notes":row.RESULT_NOTES="";break;case "unrecorded":row.RESULT_STATUS="";break;}IReadOnlyList<WORK_ORDER_INSPECTION> rows=fault=="duplicate"?[row,row]:[row];Assert.Throws<InvalidOperationException>(()=>WorkOrderInspectionRules.Require("field","order",rows));}
    [Fact] public void ResultAndRequirementChangesInvalidateOriginalRevision(){var row=Row();var original=WorkOrderInspectionRules.Revision(row);row.REQUIRED_TEXT="Changed requirement";Assert.NotEqual(original,WorkOrderInspectionRules.Revision(row));var requirement=WorkOrderInspectionRules.Revision(row);row.RESULT_STATUS="FAIL";row.RESULT_NOTES="Actual failed isolation check";row.INSPECTOR="technician";row.INSPECTED_AT_UTC=DateTime.UtcNow;Assert.NotEqual(requirement,WorkOrderInspectionRules.Revision(row));}
    [Fact] public void ActualEmptyChecklistIsNotEvidenceOfPassing(){WorkOrderInspectionRules.Require("field","order",[]);Assert.False(WorkOrderWorkflow.RecordedConditionsPassed([], [ConditionType.PreStart]));}
}
