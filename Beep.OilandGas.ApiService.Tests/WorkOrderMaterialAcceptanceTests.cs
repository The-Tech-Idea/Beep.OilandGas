#if WORKFLOW_INBOX_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using Xunit;
using Row=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
using Rules=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderMaterialAcceptanceTests
{
    [Theory][InlineData("record")][InlineData("schema")][InlineData("authority")][InlineData("stock-before")][InlineData("stock-after")][InlineData("write")][InlineData("readback")][InlineData("unknown-commit")][InlineData("permit-after")][InlineData("versioned")]
    public async Task NativeMaterialAcceptanceRetainsOriginalProofUsesOneOwnedUpdateAndRollsBackDrift(string mode)
    {
        var proof=new WorkOrderMaterialAcceptanceFixture(requirementProof:mode=="versioned");var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();f.Rows.Add(CompanyInventorySourceReview.Copy(proof.Pending));f.Rows.Add(CompanyInventorySourceReview.Copy(proof.StoredPermit));var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(Row).FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(true);metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_PERMIT",PrimaryKeyColumn="PERMIT_ID",EntityTypeName=proof.StoredPermit.GetType().FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(["PERMIT_ID"]);f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(Row));if(mode=="schema")shape.Fields.RemoveAll(field=>field.FieldName=="MATERIAL_ACCEPTANCE_DATA_JSON");return shape;});
        f.Source.Setup(s=>s.UpdateEntity("WORK_ORDER_INSPECTION",It.IsAny<object>())).Returns((string _,object value)=>{f.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((Row)value);if(mode=="readback")row.RESULT_NOTES="damaged";f.Rows.RemoveAll(value=>value is Row);f.Rows.Add(row);if(mode=="stock-after")proof.Inventory.QUANTITY_ON_HAND=4m;if(mode=="permit-after")((actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT)f.Rows.Single(value=>value is actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT)).STATUS="REJECTED";return new ErrorsInfo{Flag=Errors.Ok};});
        Task Authority()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;var owner=new InspectionService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((proof.Permit.Context.Company,"independent",proof.IssuedSource.InspectionDatabase)),NullLogger<InspectionService>.Instance,(_,_,_,_)=>Task.CompletedTask,_=>Authority());
        if(mode=="stock-before")proof.Inventory.QUANTITY_ON_HAND=4m;if(mode=="unknown-commit")f.FailCommit=true;
        Task Current(Row? own){proof.Command.RequireCurrent(proof.Current(own),own);return Task.CompletedTask;}
        var error=await Record.ExceptionAsync(async()=>{var saved=await owner.RecordMaterialAcceptanceAsync(proof.Command,Authority,Current);proof.Command.RequireRecorded(saved);Assert.Equal(proof.Command.IntentId,Rules.Project(saved).MaterialAcceptance!.IntentId);});
        if(mode is "record" or "versioned")Assert.Null(error);else Assert.NotNull(error);
        if(mode is "record" or "versioned" or "unknown-commit")
        {
            Assert.Equal(1,f.Writes);var original=JsonSerializer.Serialize(f.Rows.OfType<Row>().Single());var recorded=f.Rows.OfType<Row>().Single();Assert.DoesNotContain("Private original logistics rationale",JsonSerializer.Serialize(Rules.Project(recorded)));var recovered=await owner.RecordMaterialAcceptanceAsync(proof.Command,()=>Task.CompletedTask,_=>throw new InvalidOperationException("Recovery must not turn historical proof into current acceptance"),true);Assert.Equal(original,JsonSerializer.Serialize(recovered));Assert.Equal(1,f.Writes);var reference=new CompanyWorkOrderMaterialAcceptanceReference(proof.Command.Review.Query.Permit,proof.Command.InspectionId,proof.Command.IntentId);var envelope=await owner.RecoverMaterialAcceptanceReferenceAsync(proof.Command.Review.PermitReview.Context,reference,()=>Task.CompletedTask);envelope.Require(proof.Permit.Context.Company,"independent",reference);Assert.Equal(original,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(1,f.Writes);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordMaterialAcceptanceAsync(proof.Command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask,Current));Assert.Equal(1,f.Writes);
        }
        else{Assert.Equal("",Assert.Single(f.Rows.OfType<Row>()).RESULT_STATUS);if(mode is "stock-after" or "write" or "readback" or "permit-after")Assert.Contains("rollback",f.Transactions);}
    }
    [Fact] public void OriginalNativeAuditAndPublicAcceptanceProofCannotBeAdoptedAfterMutation()
    {
        var f=new WorkOrderMaterialAcceptanceFixture();var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);f.Command.RequireRecorded(row);Assert.Null(Rules.Project(f.Pending).MaterialAcceptance);Assert.DoesNotContain("MATERIAL_ACCEPTANCE",JsonSerializer.Serialize(f.Pending));row.ROW_CREATED_BY="other";Assert.Throws<InvalidOperationException>(()=>f.Command.RequireRecorded(row));
    }
    [Fact] public void VersionedMaterialAcceptanceRejectsChangedImmutableAuditEvenWithRecomputedAcceptanceDigest()
    {
        var f=new WorkOrderMaterialAcceptanceFixture(requirementProof:true);var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);f.Command.RequireRecorded(row);Assert.NotNull(f.Command.OriginalInspection.RequirementRevision);row.ROW_CREATED_BY="changed requirement author";row.MATERIAL_ACCEPTANCE_REVISION=CompanyWorkOrderMaterialAcceptanceCommand.Digest(row);Assert.Throws<InvalidOperationException>(()=>f.Command.RequireRecorded(row));
    }
    [Theory][InlineData("foreign-reviewer")][InlineData("foreign-company")]
    public async Task PrivateRecoveryChecksRecordedReviewerAndNativeCompanyBeforeMalformedCommand(string mode)
    {
        var proof=new WorkOrderMaterialAcceptanceFixture();var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var row=proof.Command.Prepare(proof.Pending,DateTime.UtcNow);row.INSPECTOR="other";row.MATERIAL_ACCEPTANCE_DATA_JSON="malformed private original body";f.Rows.Add(row);var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(Row).FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(true);metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_PERMIT",PrimaryKeyColumn="PERMIT_ID",EntityTypeName=proof.StoredPermit.GetType().FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(["PERMIT_ID"]);f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(Row)));
        var company=mode=="foreign-company"?new CompanyScope("tenant","other"):proof.Permit.Context.Company;var owner=new InspectionService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((company,"independent",proof.IssuedSource.InspectionDatabase)),NullLogger<InspectionService>.Instance,(_,_,_,_)=>Task.CompletedTask,_=>Task.CompletedTask);
        var error=await Record.ExceptionAsync(()=>owner.RecordMaterialAcceptanceAsync(proof.Command,()=>Task.CompletedTask,_=>Task.CompletedTask,true));Assert.True(error is UnauthorizedAccessException or Beep.OilandGas.Models.Core.Refusals.RefusalException,error?.ToString());Assert.Equal(0,f.Writes);
    }
}
#endif
