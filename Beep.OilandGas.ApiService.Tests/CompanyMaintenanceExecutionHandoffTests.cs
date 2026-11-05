using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep;
using TheTechIdea.Beep.Tools;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceExecutionHandoffTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyMaintenanceAcceptanceTests.Fixture Flow=new();
        internal FieldSelectionReview Executor=null!;
        internal CompanyWorkOrderSubmissionFacts Report=null!;
        internal CompanyMaintenanceAssignedStageReview Stage=null!;
        internal WorkOrderExecutionPermitService Reports=null!;
        internal bool Revoked,ChangedOwnedReport;
        internal async Task Ready(bool completionVerification=false,string? returnRole=null,bool returnAcceptance=false,bool assetReturn=false,bool multipleWells=false)
        {
            Flow.Owner.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=new[]{new ProcessStepDefinition{StepId="receive",StepName="Independent receiving",StepType="APPROVAL",SequenceNumber=1,RequiredRoles=["MaintenanceRole"],RequiresApproval=true},new ProcessStepDefinition{StepId="execute",StepName="Actual executor work",StepType="ACTION",SequenceNumber=2,RequiredRoles=["SupervisorRole"]},new ProcessStepDefinition{StepId="verify",StepName="Independent completion review",StepType="APPROVAL",SequenceNumber=3,RequiredRoles=["VerifierRole"],RequiresApproval=true}}});
            if(returnRole is not null){var steps=JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject>(Flow.Owner.Definition.PROCESS_CONFIG_JSON)!;steps["Steps"]!.AsArray().Add(JsonSerializer.SerializeToNode(new ProcessStepDefinition{StepId="return",StepName="Responsible return approval",StepType="APPROVAL",SequenceNumber=4,RequiredRoles=[returnRole],RequiresApproval=true}));Flow.Owner.Definition.PROCESS_CONFIG_JSON=steps.ToJsonString();}
            Flow.Owner.Native.Personas.Setup(value=>value.GetActivePersonasAsync("VerifierRole")).ReturnsAsync(["HSE","SUPERVISOR"]);
            if(returnRole is not null && returnRole is not ("VerifierRole" or "SupervisorRole"))Flow.Owner.Native.Personas.Setup(value=>value.GetActivePersonasAsync(returnRole)).ReturnsAsync(["FACILITY","PRODUCTION"]);
            await Flow.Ready();await Flow.Accept(new(await Flow.Review(),Guid.NewGuid(),"Original receiving acceptance"));
            Executor=Flow.Receiver with{ActorUserId="executor",Revision="execution-handoff-authority"};var task=new CompanyMaintenanceReceivingQuery(Flow.Start.InstanceId(),"field",Flow.Link.WorkOrderId);
            Stage=await Flow.Owner.Service.ReviewMaintenanceAssignedStageAsync(Executor,task,Authority);
            WorkOrderDetailModel? assetOrder=assetReturn?new(){InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",State="",EvidenceUnavailableReason="No header execution state",WoSubType="CORRECTIVE",Components=[new(task.WorkOrderId,"component",task.FieldId,"well",""){NativeRevision=new string('A',64)},new(task.WorkOrderId,"isolation-component",task.FieldId,"",""){InformationItemId="ISOLATION-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('B',64)},new(task.WorkOrderId,"risk-component",task.FieldId,"",""){InformationItemId="RISK-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('C',64)}]}:null;
            if(multipleWells && assetOrder is not null)assetOrder.Components=assetOrder.Components.Append(new(task.WorkOrderId,"second-well-component",task.FieldId,"well-2",""){NativeRevision=new string('D',64)}).ToList();
            var original=WorkOrderExecutionStartFixture.Submission(new("core","core-binding"),actor:"executor",role:"SupervisorRole",originalOrder:assetOrder,taskOverride:task,completionVerification:completionVerification,returnAcceptance:returnAcceptance);var row=original.Command.Prepare(DateTime.UtcNow);Flow.Owner.Native.Rows.Add(row);
            var creator=new ClassCreator(Flow.Owner.Native.Editor.Object);Flow.Owner.Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);Flow.Owner.Native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(true);
            Flow.Owner.Native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION)));
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
            Reports=new(Flow.Owner.Native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
            Report=await Reports.ReadSubmissionFactsAsync(original.Command.Review.Inspections.Current.Context,new(task,row.SUBMISSION_ID),()=>Task.CompletedTask);
        }
        internal Task Authority(string? role){if(Revoked)throw new UnauthorizedAccessException("Revoked executor");if(role is not null)Assert.Equal("SupervisorRole",role);return Task.CompletedTask;}
        internal async Task<CompanyWorkOrderSubmissionFacts?> Read(bool owned)
        {
            if(owned){PPDMDataSourceTransaction.RequireOwned(Flow.Owner.Native.Source.Object,IsolationLevel.Serializable);if(ChangedOwnedReport)return Report with{WorkPerformed="Changed actual report"};}
            var context=new FieldSelectionReview(Report.Company,Report.Database,Report.ExecutorUserId,"original-core-authority","field");return await Reports.ReadSubmissionFactsAsync(context,Report.Query,()=>Task.CompletedTask,owned);
        }
    }
    [Fact] public async Task RecordedReportHandoffClosesExecutorTasksAndOpensConfiguredIndependentReviewWithoutPhysicalMutation()
    {
        var f=new Fixture();await f.Ready();var before=f.Flow.Owner.Native.Writes;var report=JsonSerializer.Serialize(f.Flow.Owner.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single());
        var review=await f.Flow.Owner.Service.ReviewExecutionHandoffAsync(f.Stage,f.Report,f.Authority);Assert.Equal("VerifierRole",review.NextRole);
        var command=new CompanyMaintenanceExecutionHandoffCommand(review,Guid.NewGuid(),"Reported work ready for independent review");var result=await f.Flow.Owner.Service.RecordExecutionHandoffAsync(command,f.Authority,f.Read);result.Require(f.Executor.Company,f.Executor.ActorUserId,command.Reference());
        Assert.Single(result.Receipt.ClosedTaskIds);Assert.Equal(2,result.Receipt.ReviewerTaskIds.Count);Assert.True(f.Flow.Owner.Native.Writes>before);var writes=f.Flow.Owner.Native.Writes;
        Assert.Equal("COMPLETED",f.Flow.Owner.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.CROSS_TASK_ID==result.Receipt.ClosedTaskIds[0]).TASK_STATUS);
        Assert.Equal(report,JsonSerializer.Serialize(f.Flow.Owner.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));Assert.DoesNotContain(f.Flow.Owner.Native.Rows,row=>row.GetType().Name is "PROJECT" or "FINANCE" or "ACCOUNTING_COST" or "INVENTORY_TRANSACTION");
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await f.Flow.Owner.Service.RecordExecutionHandoffAsync(command,f.Authority,f.Read)));
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await f.Flow.Owner.Service.RecoverExecutionHandoffAsync(f.Executor,command.Reference(),f.Authority)));Assert.Equal(writes,f.Flow.Owner.Native.Writes);
        var reviewer=f.Executor with{ActorUserId="independent-reviewer",Revision="review-role-authority"};Task Reviewer(string? role){if(role is not null)Assert.Equal("VerifierRole",role);return Task.CompletedTask;}
        var stage=await f.Flow.Owner.Service.ReviewMaintenanceAssignedStageAsync(reviewer,f.Stage.Task,Reviewer);Assert.Equal("verify",stage.StepId);Assert.Equal(result.Receipt.ReviewerStepInstanceId,stage.StepInstanceId);Assert.Equal("VerifierRole",stage.AssignedRole);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Flow.Owner.Service.ReviewMaintenanceAssignedStageAsync(f.Executor,f.Stage.Task,Reviewer));Assert.Equal(writes,f.Flow.Owner.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Owner.Service.RecordExecutionHandoffAsync(command with{IntentId=Guid.NewGuid()},f.Authority,f.Read));Assert.Equal(writes,f.Flow.Owner.Native.Writes);
    }
    [Theory][InlineData("source")][InlineData("role")][InlineData("definition")][InlineData("unknown")][InlineData("wrong-reference")]
    public async Task HandoffRetainsOriginalAndCannotReplaceChangedSourceRoleOrUncertainOutcome(string fault)
    {
        var f=new Fixture();await f.Ready();var review=await f.Flow.Owner.Service.ReviewExecutionHandoffAsync(f.Stage,f.Report,f.Authority);var command=new CompanyMaintenanceExecutionHandoffCommand(review,Guid.NewGuid(),"Original executor report handoff");var writes=f.Flow.Owner.Native.Writes;
        if(fault=="source")f.ChangedOwnedReport=true;if(fault=="role")f.Revoked=true;if(fault=="definition")f.Flow.Owner.Definition.PROCESS_NAME="Changed configured definition";if(fault=="unknown")f.Flow.Owner.Native.FailCommit=true;
        if(fault=="wrong-reference")
        {
            await f.Flow.Owner.Service.RecordExecutionHandoffAsync(command,f.Authority,f.Read);writes=f.Flow.Owner.Native.Writes;
            await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Owner.Service.RecoverExecutionHandoffAsync(f.Executor,command.Reference() with{IntentId=Guid.NewGuid()},f.Authority));Assert.Equal(writes,f.Flow.Owner.Native.Writes);return;
        }
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Owner.Service.RecordExecutionHandoffAsync(command,f.Authority,f.Read));
        if(fault=="unknown"){var result=await f.Flow.Owner.Service.RecoverExecutionHandoffAsync(f.Executor,command.Reference(),f.Authority);result.Require(f.Executor.Company,f.Executor.ActorUserId,command.Reference());Assert.True(f.Flow.Owner.Native.Writes>writes);}
        else{Assert.Equal(writes,f.Flow.Owner.Native.Writes);Assert.DoesNotContain(f.Flow.Owner.Native.Rows.OfType<PROCESS_HISTORY>(),row=>row.EVENT_TYPE=="WORK_REPORTED");}
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task SeparateCoreSourceIsNeverReadInsideOwnedLifecycleCommand(bool revoked)
    {
        var f=new Fixture();await f.Ready();var core=new Mock<IDataSource>();f.Flow.Owner.Native.Editor.Setup(value=>value.GetDataSource("separate-core")).Returns(core.Object);
        var context=new FieldSelectionReview(f.Report.Company,f.Report.Database with{ConnectionName="separate-core"},f.Report.ExecutorUserId,"separate-core-authority",f.Report.Query.Task.FieldId);var guards=0;var writes=f.Flow.Owner.Native.Writes;
        Task Guard(){guards++;if(revoked)throw new UnauthorizedAccessException("Revoked separate source authority");return Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Flow.Owner.Native.Source.Object,async()=>Assert.Null(await f.Reports.ReadSubmissionHandoffProofAsync(context,f.Report.Query,f.Executor.Database.ConnectionName,true,Guard))));
        if(revoked)Assert.IsType<UnauthorizedAccessException>(error);else Assert.Null(error);Assert.Equal(1,guards);Assert.Empty(core.Invocations);Assert.Equal(writes,f.Flow.Owner.Native.Writes);
    }
}
#endif
