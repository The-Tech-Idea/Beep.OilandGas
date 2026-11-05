#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderPersonnelReleaseTests
{
    [Theory][InlineData("record")][InlineData("write")][InlineData("readback")][InlineData("assignment-after")][InlineData("unknown")][InlineData("authority")][InlineData("schema")][InlineData("borrowed")][InlineData("isolation")][InlineData("different-intent")]
    public async Task NativeCancellationPreservesOriginalAssignmentAndRetainsExactlyOneAppendAndRecovery(string mode)
    {
        var assignment=WorkOrderPersonnelReviewFixture.Original();var stored=assignment.Prepare(DateTime.UtcNow);var command=new CompanyWorkOrderPersonnelReleaseCommand(new(assignment,stored),Guid.NewGuid(),"Explicit original personnel cancellation");var context=assignment.Review.Personnel.Context;
        var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();f.Rows.Add(CompanyInventorySourceReview.Copy(stored));var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(f.Editor.Object);
        foreach(var (table,type,key) in new[]{("WORK_ORDER_PERSONNEL_ASSIGNMENT",typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),"ASSIGNMENT_ID"),("WORK_ORDER_PERSONNEL_RELEASE",typeof(WORK_ORDER_PERSONNEL_RELEASE),"RELEASE_ID")})
        {
            f.Source.Setup(source=>source.CheckEntityExist(table)).Returns(true);
            metadata.Setup(m=>m.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=key});metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
            f.Source.Setup(source=>source.GetEntityStructure(It.Is<EntityStructure>(request=>request.EntityName==table),true)).Returns(()=>{var shape=creator.ConvertToEntityStructure(type);if(mode=="schema" && table=="WORK_ORDER_PERSONNEL_RELEASE")shape.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");return shape;});
        }
        if(mode=="different-intent")f.Rows.Add((command with{IntentId=Guid.NewGuid()}).Prepare(DateTime.UtcNow));
        f.Source.Setup(source=>source.InsertEntity("WORK_ORDER_PERSONNEL_RELEASE",It.IsAny<object>())).Returns((string _,object value)=>
        {
            f.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_PERSONNEL_RELEASE)value);if(mode=="readback")row.SOURCE="changed";f.Rows.Add(row);
            if(mode=="assignment-after")f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Single().DUTY_CODE="changed";return new ErrorsInfo{Flag=Errors.Ok};
        });
        if(mode=="unknown")f.FailCommit=true;if(mode=="borrowed")f.Borrowed=true;if(mode=="isolation")f.Isolation=System.Data.IsolationLevel.ReadCommitted;
        var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        var error=await Record.ExceptionAsync(async()=>{var row=await owner.RecordPersonnelReleaseAsync(command,Guard);command.RequireRecorded(row);});
        if(mode=="record")Assert.Null(error);else Assert.NotNull(error);
        Assert.Equal(JsonSerializer.Serialize(stored),JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Single()));
        if(mode is "record" or "unknown")
        {
            Assert.Equal(1,f.Writes);var saved=JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_PERSONNEL_RELEASE>().Single());
            var recovered=await owner.RecordPersonnelReleaseAsync(command,()=>Task.CompletedTask,true);Assert.Equal(saved,JsonSerializer.Serialize(recovered));Assert.Equal(1,f.Writes);
            var reference=new CompanyWorkOrderPersonnelReleaseReference(new(assignment.Review.Personnel.Query.Permit,assignment.IntentId),command.IntentId);var envelope=await owner.RecoverPersonnelReleaseReferenceAsync(context,reference,()=>Task.CompletedTask);envelope.Require(context.Company,context.ActorUserId,reference);Assert.Equal(saved,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(1,f.Writes);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelReleaseAsync(command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask));Assert.Equal(1,f.Writes);
            var roster=await owner.ReadPersonnelAssignmentsAsync(context,reference.Assignment.Permit,()=>Task.CompletedTask);roster.Require(context.Company,context.ActorUserId,reference.Assignment.Permit);Assert.Single(roster.Releases!);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(roster));Assert.DoesNotContain(command.IntentId.ToString("D"),JsonSerializer.Serialize(roster));
            var raw=f.Rows.OfType<WORK_ORDER_PERSONNEL_RELEASE>().Single();raw.ROW_CREATED_BY="other";raw.COMMAND_JSON="malformed private cancellation";await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecordPersonnelReleaseAsync(command,()=>Task.CompletedTask,true));Assert.Equal(1,f.Writes);
        }
        else
        {
            Assert.Equal(mode=="different-intent"?1:0,f.Rows.OfType<WORK_ORDER_PERSONNEL_RELEASE>().Count());
            if(mode is "write" or "readback" or "assignment-after" or "isolation")Assert.Contains("rollback",f.Transactions);
            if(mode is "authority" or "schema" or "borrowed" or "different-intent")Assert.Empty(f.Transactions);
        }
    }
}
#endif
