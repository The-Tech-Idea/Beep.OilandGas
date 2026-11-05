#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderPersonnelAssignmentTests
{
    [Theory][InlineData("record")][InlineData("write")][InlineData("readback")][InlineData("membership-after")][InlineData("permit-after")][InlineData("authority")][InlineData("schema")][InlineData("unknown")][InlineData("overlap")][InlineData("released-overlap")][InlineData("released-overlap-drift")][InlineData("borrowed")][InlineData("isolation")][InlineData("requirement-proof")]
    public async Task NativeAssignmentRetainsOriginalProofAndOwnsItsRollbackAndRecovery(string mode)
    {
        var proof=new WorkOrderMaterialAcceptanceFixture(requirementProof:mode=="requirement-proof");var f=new PersonaTaskRoutingTests.Fixture();f.Rows.Clear();
        var utc=DateTime.UtcNow;var context=proof.Command.Review.PermitReview.Context;
        var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID="company",EMPLOYEE_BA_ID="employee",EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",EMPLOYEE_POSITION="Recorded operator",STATUS="ACTIVE",EFFECTIVE_DATE=utc.AddDays(-2),EXPIRY_DATE=utc.AddDays(2)};
        var license=new BA_LICENSE{LICENSEE_BA_ID="employee",LICENSE_ID=proof.Permit.PersonnelReference,ACTIVE_IND="Y",LICENSE_TYPE="Recorded certificate",GRANTED_BY_BA_ID="reported-authority",GRANTED_DATE=utc.AddDays(-1),EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        f.Rows.Add(employee);f.Rows.Add(license);f.Rows.Add(CompanyInventorySourceReview.Copy(proof.StoredPermit));
        var query=new CompanyWorkOrderPersonnelSourceQuery(proof.Command.Review.Query.Permit,"employee",license.LICENSE_ID);
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(f.Editor.Object);
        var types=new Dictionary<string,Type>{{"BA_EMPLOYEE",typeof(BA_EMPLOYEE)},{"BA_LICENSE",typeof(BA_LICENSE)},{"WORK_ORDER_EXECUTION_PERMIT",proof.StoredPermit.GetType()},{"WORK_ORDER_PERSONNEL_ASSIGNMENT",typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT)},{"WORK_ORDER_PERSONNEL_RELEASE",typeof(WORK_ORDER_PERSONNEL_RELEASE)}};
        foreach(var (table,type) in types)
        {
            var keys=table=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}:table=="BA_LICENSE"?new[]{"LICENSEE_BA_ID","LICENSE_ID"}:table=="WORK_ORDER_EXECUTION_PERMIT"?new[]{"PERMIT_ID"}:table=="WORK_ORDER_PERSONNEL_RELEASE"?new[]{"RELEASE_ID"}:new[]{"ASSIGNMENT_ID"};
            metadata.Setup(m=>m.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)});
            metadata.Setup(m=>m.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);f.Source.Setup(s=>s.CheckEntityExist(table)).Returns(true);
        }
        f.Source.Setup(s=>s.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>
        {
            var shape=creator.ConvertToEntityStructure(types[request.EntityName]);
            foreach(var field in shape.Fields)if(request.EntityName=="BA_EMPLOYEE" || request.EntityName=="BA_LICENSE"){field.IsKey=request.EntityName=="BA_EMPLOYEE"?new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}.Contains(field.FieldName):new[]{"LICENSEE_BA_ID","LICENSE_ID"}.Contains(field.FieldName);if(field.IsKey)field.AllowDBNull=false;}
            if(mode=="schema" && request.EntityName=="WORK_ORDER_PERSONNEL_ASSIGNMENT")shape.Fields.RemoveAll(field=>field.FieldName=="COMMAND_JSON");return shape;
        });
        var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((context.Company,context.ActorUserId,context.Database)),NullLogger<ContractorManagementService>.Instance);
        var facts=await owner.ReadPersonnelSourceAsync(context,query,()=>Task.CompletedTask);
        var review=new CompanyWorkOrderPersonnelSourceReview(proof.Command.Review.PermitReview,facts,CompanyWorkOrderPersonnelSourceReview.GetBlockers(proof.Permit,facts));
        var command=new CompanyWorkOrderPersonnelAssignmentCommand(review,Guid.NewGuid(),"RecordedDuty",proof.Permit.ValidFromUtc,proof.Permit.ValidToUtc,"Explicit assignment rationale");
        command.Require();
        if(mode is "overlap" or "released-overlap" or "released-overlap-drift")
        {
            var prior=command with{IntentId=Guid.NewGuid()};var priorRow=prior.Prepare(DateTime.UtcNow);f.Rows.Add(priorRow);
            if(mode is "released-overlap" or "released-overlap-drift")f.Rows.Add(new CompanyWorkOrderPersonnelReleaseCommand(new(prior,priorRow),Guid.NewGuid(),"Explicit earlier cancellation").Prepare(DateTime.UtcNow));
        }
        f.Source.Setup(s=>s.InsertEntity("WORK_ORDER_PERSONNEL_ASSIGNMENT",It.IsAny<object>())).Returns((string _,object value)=>
        {
            f.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_PERSONNEL_ASSIGNMENT)value);if(mode=="readback")row.DUTY_CODE="changed";f.Rows.Add(row);
            if(mode=="released-overlap-drift")f.Rows.OfType<WORK_ORDER_PERSONNEL_RELEASE>().Single().SOURCE="changed during replacement";
            if(mode=="membership-after")f.Rows.OfType<BA_EMPLOYEE>().Single().EMPLOYER_BA_ID="other";
            if(mode=="permit-after")f.Rows.Single(value=>value.GetType()==proof.StoredPermit.GetType()).GetType().GetProperty("STATUS")!.SetValue(f.Rows.Single(value=>value.GetType()==proof.StoredPermit.GetType()),"REJECTED");
            return new ErrorsInfo{Flag=Errors.Ok};
        });
        Task Authority()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException()):Task.CompletedTask;
        if(mode=="unknown")f.FailCommit=true;if(mode=="borrowed")f.Borrowed=true;if(mode=="isolation")f.Isolation=System.Data.IsolationLevel.ReadCommitted;
        var error=await Record.ExceptionAsync(async()=>{var row=await owner.RecordPersonnelAssignmentAsync(command,Authority,()=>Task.CompletedTask);command.RequireRecorded(row);});
        if(mode is "record" or "released-overlap" or "requirement-proof")Assert.Null(error);else Assert.NotNull(error);
        if(mode=="released-overlap"){Assert.Equal(1,f.Writes);Assert.Equal(2,f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Count());Assert.Single(f.Rows.OfType<WORK_ORDER_PERSONNEL_RELEASE>());}
        else if(mode is "record" or "unknown" or "requirement-proof")
        {
            Assert.Equal(1,f.Writes);if(mode=="requirement-proof"){Assert.Equal(1,command.Review.PermitReview.Proposal.RequirementEvidenceVersion);Assert.All(command.Review.PermitReview.CurrentSource.Inspections,row=>Assert.True(ProcessDefinitionRevision.IsValid(row.RequirementRevision)));}var saved=JsonSerializer.Serialize(f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Single());
            var recovered=await owner.RecordPersonnelAssignmentAsync(command,()=>Task.CompletedTask,()=>throw new InvalidOperationException("Historical recovery cannot assert current qualification"),true);
            Assert.Equal(saved,JsonSerializer.Serialize(recovered));Assert.Equal(1,f.Writes);
            var reference=new CompanyWorkOrderPersonnelAssignmentReference(query.Permit,command.IntentId);var envelope=await owner.RecoverPersonnelAssignmentReferenceAsync(context,reference,()=>Task.CompletedTask);envelope.Require(context.Company,context.ActorUserId,reference);Assert.Equal(saved,JsonSerializer.Serialize(envelope.Recorded));Assert.Equal(1,f.Writes);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.RecordPersonnelAssignmentAsync(command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask,()=>Task.CompletedTask));Assert.Equal(1,f.Writes);
            var raw=f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Single();raw.ROW_CREATED_BY="other";raw.COMMAND_JSON="malformed private evidence";
            await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecordPersonnelAssignmentAsync(command,()=>Task.CompletedTask,()=>Task.CompletedTask,true));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecoverPersonnelAssignmentReferenceAsync(context,reference,()=>Task.CompletedTask));Assert.Equal(1,f.Writes);
        }
        else
        {
            Assert.Equal(mode is "overlap" or "released-overlap-drift"?1:0,f.Rows.OfType<WORK_ORDER_PERSONNEL_ASSIGNMENT>().Count());
            if(mode is "write" or "readback" or "membership-after" or "permit-after" or "overlap" or "released-overlap-drift" or "isolation")Assert.Contains("rollback",f.Transactions);
            if(mode is "authority" or "schema" or "borrowed")Assert.Empty(f.Transactions);
        }
    }
    [Fact] public async Task LegacyAssignmentShortcutsRefuseBeforeProjectReadsOrWrites()
    {
        var f=new PersonaTaskRoutingTests.Fixture();var owner=new ContractorManagementService(f.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),Mock.Of<IPPDMMetadataRepository>(),()=>Task.FromResult((new CompanyScope("tenant","company"),"maker",new AssetDatabaseScope("lifecycle","fingerprint"))),NullLogger<ContractorManagementService>.Instance);
        await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>owner.AssignContractorAsync("order","step","employee","duty","maker"));
        await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>owner.RemoveContractorAsync("order","step","employee","maker"));
        await Assert.ThrowsAsync<Beep.OilandGas.Models.Core.Refusals.RefusalException>(()=>owner.GetAssignmentsAsync("order"));
        Assert.Equal(0,f.Writes);f.Source.Verify(s=>s.GetEntityAsync("PROJECT_STEP_BA",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>()),Times.Never);
    }
}
#endif
