#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
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
using TheTechIdea.Data.OilGas;
using Xunit;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
using Rules=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderPersonnelAcceptanceTests
{
    [Theory][InlineData("record")][InlineData("schema")][InlineData("authority")][InlineData("source-before")][InlineData("core-after")][InlineData("write")][InlineData("readback")][InlineData("unknown")][InlineData("borrowed")][InlineData("isolation")][InlineData("generic-pass")][InlineData("historical")][InlineData("requirement-proof")][InlineData("original-reference")][InlineData("parent-before")][InlineData("parent-after")]
    public async Task NativePersonnelAcceptanceOwnsOneUpdateClosesProofAndRetainsExactRecovery(string mode)
    {
        var proof=new WorkOrderPersonnelAcceptanceFixture(mode=="historical",mode is "requirement-proof" or "original-reference",mode=="original-reference");var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();f.Rows.Add(CompanyInventorySourceReview.Copy(proof.Pending));f.Rows.Add(CompanyInventorySourceReview.Copy(proof.StoredPermit));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(Row).FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);
        metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_PERMIT",PrimaryKeyColumn="PERMIT_ID",EntityTypeName=proof.StoredPermit.GetType().FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(["PERMIT_ID"]);
        var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(true);
        f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(typeof(Row));if(mode=="schema")shape.Fields.RemoveAll(field=>field.FieldName=="PERSONNEL_ACCEPTANCE_DATA_JSON");return shape;});
        f.Source.Setup(s=>s.UpdateEntity("WORK_ORDER_INSPECTION",It.IsAny<object>())).Returns((string _,object value)=>{f.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((Row)value);if(mode=="readback")row.RESULT_NOTES="damaged";f.Rows.RemoveAll(value=>value is Row);f.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};});
        Task Authority()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var context=proof.Evidence.Assignment.CurrentPersonnel.PermitReview.Context;var parentLevels=new List<string>();
        Task Parent(string _,string field,string order,string level)
        {
            parentLevels.Add(level);
            if(mode=="parent-before" || mode=="parent-after" && f.Source.Invocations.Any(call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WORK_ORDER_INSPECTION"))
                throw new UnauthorizedAccessException("Original WRITE parent authority changed");
            return Task.CompletedTask;
        }
        var owner=new InspectionService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<InspectionService>.Instance,Parent,_=>Authority());
        if(mode=="generic-pass"){await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>owner.RecordResultAsync("order-الكويت",proof.Pending.COND_SEQ,new(){FieldId="field",InspectionId=proof.Pending.INSPECTION_ID,ExpectedSourceRevision=Rules.Revision(proof.Pending),Result="PASS",Notes="Unreviewed generic shortcut"},context.ActorUserId));Assert.Equal(0,f.Writes);Assert.Equal("",Assert.Single(f.Rows.OfType<Row>()).RESULT_STATUS);return;}
        if(mode=="unknown")f.FailCommit=true;if(mode=="borrowed")f.Borrowed=true;if(mode=="isolation")f.Isolation=System.Data.IsolationLevel.ReadCommitted;
        Task Current(Row? own){if(mode=="source-before")throw new InvalidOperationException("Source drift");proof.Command.RequireCurrent(proof.Current(own),own);return Task.CompletedTask;}
        Task Core(Row? own){if(mode=="core-after" && own is not null)throw new InvalidOperationException("Local credential drift");proof.Command.RequireCurrentCore(proof.Current(own).Assignment.CurrentPersonnel.PermitReview.CurrentSource,own);return Task.CompletedTask;}
        if(mode=="historical")
        {
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelAcceptanceAsync(proof.Command,Authority,Current,Core));Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);
            var historical=proof.Command.Prepare(proof.Pending,DateTime.UtcNow);var before=JsonSerializer.Serialize(historical);Assert.DoesNotContain("EligibilityEvidenceVersion",historical.PERSONNEL_ACCEPTANCE_DATA_JSON);Assert.DoesNotContain("AcceptedEmployment",historical.PERSONNEL_ACCEPTANCE_DATA_JSON);f.Rows.RemoveAll(value=>value is Row);f.Rows.Add(historical);
            var recovered=await owner.RecordPersonnelAcceptanceAsync(proof.Command,Authority,_=>throw new InvalidOperationException("Historical recovery cannot requalify"),_=>throw new InvalidOperationException("Historical recovery cannot rewrite"),true);Assert.Equal(before,JsonSerializer.Serialize(recovered));
            var reference=new CompanyWorkOrderPersonnelAcceptanceReference(proof.Command.Review.Assignment.Query.Permit,proof.Command.InspectionId,proof.Command.IntentId);var envelope=await owner.RecoverPersonnelAcceptanceReferenceAsync(context,reference,Authority);Assert.Equal(before,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(0,f.Writes);Assert.Empty(f.Transactions);return;
        }
        var error=await Record.ExceptionAsync(async()=>{var row=await owner.RecordPersonnelAcceptanceAsync(proof.Command,Authority,Current,Core);proof.Command.RequireRecorded(row);});if(mode is "record" or "requirement-proof" or "original-reference")Assert.Null(error);else Assert.NotNull(error);
        if(mode=="parent-before")Assert.DoesNotContain(f.Source.Invocations,call=>call.Method.Name=="GetEntityAsync" && call.Arguments[0] as string=="WORK_ORDER_INSPECTION");
        if(mode is "record" or "requirement-proof" or "original-reference")Assert.All(parentLevels,level=>Assert.Equal("WRITE",level));
        if(mode is "record" or "requirement-proof" or "original-reference" or "unknown")
        {
            Assert.Equal(1,f.Writes);var before=JsonSerializer.Serialize(f.Rows.OfType<Row>().Single());var projected=Rules.Project(f.Rows.OfType<Row>().Single());Assert.Equal(proof.Command.IntentId,projected.PersonnelAcceptance!.IntentId);Assert.DoesNotContain(proof.Command.Reason,JsonSerializer.Serialize(projected));
            var result=await owner.RecordPersonnelAcceptanceAsync(proof.Command,()=>Task.CompletedTask,_=>throw new InvalidOperationException("No current writes during recovery"),_=>throw new InvalidOperationException("No core writes during recovery"),true);Assert.Equal(before,JsonSerializer.Serialize(result));
            var reference=new CompanyWorkOrderPersonnelAcceptanceReference(proof.Command.Review.Assignment.Query.Permit,proof.Command.InspectionId,proof.Command.IntentId);var recovered=await owner.RecoverPersonnelAcceptanceReferenceAsync(context,reference,()=>Task.CompletedTask);recovered.Require(context.Company,context.ActorUserId,reference);Assert.Equal(before,JsonSerializer.Serialize(recovered.Recorded));
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelAcceptanceAsync(proof.Command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask,Current,Core));Assert.Equal(1,f.Writes);
        }
        else{Assert.Equal("",Assert.Single(f.Rows.OfType<Row>()).RESULT_STATUS);if(mode is "core-after" or "write" or "readback" or "isolation")Assert.Contains("rollback",f.Transactions);}
    }
    [Fact] public void HistoricalProjectionStaysPrivateAndFullAuditCannotBeAdoptedAfterMutation()
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();Assert.DoesNotContain("PERSONNEL_ACCEPTANCE",JsonSerializer.Serialize(f.Pending));var saved=f.Command.Prepare(f.Pending,DateTime.UtcNow);f.Command.RequireRecorded(saved);saved.ROW_CREATED_BY="other";saved.PERSONNEL_ACCEPTANCE_REVISION=CompanyWorkOrderPersonnelAcceptanceCommand.Digest(saved);Assert.Throws<InvalidOperationException>(()=>f.Command.RequireRecorded(saved));
    }
    [Theory][InlineData("foreign-reviewer")][InlineData("foreign-company")]
    public async Task PrivateRecoveryChecksRecordedReviewerAndNativeCompanyBeforeMalformedCommand(string mode)
    {
        var proof=new WorkOrderPersonnelAcceptanceFixture(mode=="historical");var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();var row=proof.Command.Prepare(proof.Pending,DateTime.UtcNow);row.INSPECTOR="other";row.PERSONNEL_ACCEPTANCE_DATA_JSON="malformed private original body";f.Rows.Add(row);var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_INSPECTION",PrimaryKeyColumn="INSPECTION_ID",EntityTypeName=typeof(Row).FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_INSPECTION")).ReturnsAsync(["INSPECTION_ID"]);var creator=new ClassCreator(f.Editor.Object);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_INSPECTION")).Returns(true);f.Source.Setup(s=>s.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(true);metadata.Setup(m=>m.GetTableMetadataAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_PERMIT",PrimaryKeyColumn="PERMIT_ID",EntityTypeName=proof.StoredPermit.GetType().FullName!});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(["PERMIT_ID"]);f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(Row)));
        var company=mode=="foreign-company"?new CompanyScope("tenant","other"):proof.Evidence.Assignment.Roster.Context.Company;var owner=new InspectionService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((company,"another-reviewer",proof.Evidence.Assignment.Roster.Context.Database)),NullLogger<InspectionService>.Instance,(_,_,_,_)=>Task.CompletedTask,_=>Task.CompletedTask);
        var error=await Record.ExceptionAsync(()=>owner.RecordPersonnelAcceptanceAsync(proof.Command,()=>Task.CompletedTask,_=>Task.CompletedTask,_=>Task.CompletedTask,true));Assert.True(error is UnauthorizedAccessException or Beep.OilandGas.Models.Core.Refusals.RefusalException,error?.ToString());Assert.Equal(0,f.Writes);
    }
}
#endif
