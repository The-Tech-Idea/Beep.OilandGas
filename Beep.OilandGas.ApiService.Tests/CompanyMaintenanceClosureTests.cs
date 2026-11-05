#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyMaintenanceClosureTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyWorkOrderReturnAcceptanceTests.Fixture Flow=new();
        internal CompanyWorkOrderReturnRecovery Accepted=null!;
        internal CompanyMaintenanceClosureCommand Command=null!;
        internal async Task Ready(bool realAudit=false,bool assetReturn=false,bool multipleWells=false)
        {
            await Flow.Ready(realAudit,assetReturn:assetReturn,multipleWells:multipleWells);var service=Flow.Flow.Flow.Flow.Flow.Owner.Service;Accepted=await service.RecordReturnAcceptanceAsync(Flow.Command(),Flow.Authority,Flow.Read);
            var stage=await service.ReviewMaintenanceClosureStageAsync(Flow.Review.Assignment.Stage.Context,Accepted.Command.Reference().Query,Flow.Authority);Flow.Review=Flow.Review with{Assignment=stage.Assignment};var current=(await Flow.Read(false))!;
            var review=new CompanyMaintenanceClosureReview(stage.Assignment.Stage,stage.Acceptance,current);review.Require();Command=new(review,Guid.NewGuid(),"Final return duty complete with recorded physical acceptance",true);
        }
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task TerminalClosureCompletesExactlyExistingTasksAndStepAndPreservesEveryEarlierReceipt(bool realAudit)
    {
        var f=new Fixture();await f.Ready(realAudit);var service=f.Flow.Flow.Flow.Flow.Flow.Owner.Service;var native=f.Flow.Flow.Flow.Flow.Flow.Owner.Native;var stage=f.Command.Review.Stage;var writes=native.Writes;var report=JsonSerializer.Serialize(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single());var approvals=JsonSerializer.Serialize(native.Rows.OfType<PROCESS_APPROVAL>().ToArray());var taskCount=native.Rows.OfType<CROSS_PERSONA_TASK>().Count();
        Assert.Null(await service.ReadMaintenanceClosureFactsAsync(stage.Context,f.Command.Reference().Query,()=>Task.CompletedTask));var result=await service.CloseMaintenanceAsync(f.Command,f.Flow.Authority,f.Flow.Read);result.Require(stage.Context.Company,stage.Context.ActorUserId,f.Command.Reference());Assert.Equal(writes+result.Receipt.ClosedTaskIds.Count+3,native.Writes);writes=native.Writes;
        var process=native.Rows.OfType<PROCESS_INSTANCE>().Single();Assert.Equal("COMPLETED",process.STATUS);Assert.Equal("COMPLETED",process.CURRENT_STATE);Assert.Equal(stage.Context.ActorUserId,process.COMPLETED_BY);Assert.Equal(result.Receipt.RecordedAtUtc,process.COMPLETED_DATE);Assert.Equal(taskCount,native.Rows.OfType<CROSS_PERSONA_TASK>().Count());Assert.All(native.Rows.OfType<CROSS_PERSONA_TASK>(),row=>Assert.Equal("COMPLETED",row.TASK_STATUS));Assert.All(native.Rows.OfType<PROCESS_STEP_INSTANCE>(),row=>Assert.Equal("COMPLETED",row.STATUS));Assert.Equal(report,JsonSerializer.Serialize(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));Assert.Equal(approvals,JsonSerializer.Serialize(native.Rows.OfType<PROCESS_APPROVAL>().ToArray()));
        Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await service.CloseMaintenanceAsync(f.Command,f.Flow.Authority,f.Flow.Read)));Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(await service.RecoverMaintenanceClosureAsync(stage.Context,f.Command.Reference(),f.Flow.Authority)));
        Assert.Equal(JsonSerializer.Serialize(f.Accepted),JsonSerializer.Serialize(await service.RecoverReturnAcceptanceAsync(stage.Context,f.Accepted.Command.Reference(),f.Flow.Authority)));
        var handoff=f.Flow.Handoff;Assert.Equal(JsonSerializer.Serialize(handoff),JsonSerializer.Serialize(await service.RecoverReturnHandoffAsync(handoff.Command.Review.Stage.Context,handoff.Command.Reference(),f.Flow.Flow.Flow.Authority)));
        var decision=f.Flow.Flow.Decision;Assert.Equal(JsonSerializer.Serialize(decision),JsonSerializer.Serialize(await service.RecoverCompletionDecisionAsync(decision.Command.Review.Stage.Context,decision.Command.Reference(),f.Flow.Flow.Flow.Authority)));
        var execution=f.Flow.Flow.Flow.Handoff;Assert.Equal(JsonSerializer.Serialize(execution),JsonSerializer.Serialize(await service.RecoverExecutionHandoffAsync(execution.Command.Review.Stage.Context,execution.Command.Reference(),f.Flow.Flow.Flow.Flow.Authority)));
        var facts=await service.ReadMaintenanceClosureFactsAsync(stage.Context with{ActorUserId="authorized-reader"},f.Command.Reference().Query,()=>Task.CompletedTask);facts!.Require();Assert.Equal(result.Receipt.ClosedTaskIds,facts.ClosedTaskIds);Assert.DoesNotContain(f.Command.Reason,JsonSerializer.Serialize(facts));Assert.DoesNotContain(f.Command.IntentId.ToString(),JsonSerializer.Serialize(facts));Assert.Equal(writes,native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>service.CloseMaintenanceAsync(f.Command with{IntentId=Guid.NewGuid()},f.Flow.Authority,f.Flow.Read));await Assert.ThrowsAsync<InvalidOperationException>(()=>service.ReviewMaintenanceAssignedStageAsync(stage.Context,stage.Task,f.Flow.Authority));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("source")][InlineData("owned-source")][InlineData("revoked")][InlineData("unconfirmed")][InlineData("changed-acceptance")][InlineData("remaining-step")]
    public async Task ChangedCurrentSourceAuthorityPhysicalAcceptanceOrNonterminalDefinitionCannotClose(string fault)
    {
        var f=new Fixture();await f.Ready();var service=f.Flow.Flow.Flow.Flow.Flow.Owner.Service;var native=f.Flow.Flow.Flow.Flow.Flow.Owner.Native;var command=f.Command;if(fault=="source")f.Flow.Changed=true;if(fault=="owned-source")f.Flow.OwnedChanged=true;if(fault=="revoked")f.Flow.Revoked=true;if(fault=="unconfirmed")command=command with{Confirmed=false};if(fault=="changed-acceptance")command=command with{Review=command.Review with{Acceptance=command.Review.Acceptance with{PhysicalSubjectRevision=new string('F',64)}}};
        if(fault=="remaining-step")native.Rows.OfType<PROCESS_STEP_INSTANCE>().First(row=>row.PROCESS_STEP_INSTANCE_ID!=command.Review.Stage.StepInstanceId).STATUS="PENDING";var writes=native.Writes;await Assert.ThrowsAnyAsync<Exception>(()=>service.CloseMaintenanceAsync(command,f.Flow.Authority,f.Flow.Read));Assert.Equal(writes,native.Writes);
    }
    [Theory][InlineData("process")][InlineData("step")][InlineData("history")][InlineData("task")][InlineData("delta")][InlineData("foreign-actor")][InlineData("foreign-intent")][InlineData("uncertain")]
    public async Task RecoveryChecksExactCompleteGraphAndNeverReplaysAnUncertainClosure(string fault)
    {
        var f=new Fixture();await f.Ready();var service=f.Flow.Flow.Flow.Flow.Flow.Owner.Service;var native=f.Flow.Flow.Flow.Flow.Flow.Owner.Native;native.FailCommit=fault=="uncertain";var error=await Record.ExceptionAsync(()=>service.CloseMaintenanceAsync(f.Command,f.Flow.Authority,f.Flow.Read));if(fault=="uncertain")Assert.NotNull(error);else Assert.Null(error);native.FailCommit=false;var writes=native.Writes;var context=f.Command.Review.Stage.Context;var reference=f.Command.Reference();
        if(fault=="process")native.Rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STATE="OPERATING";if(fault=="step")native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.PROCESS_STEP_INSTANCE_ID==f.Command.Review.Stage.StepInstanceId).NOTES="changed";if(fault=="history")native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="MAINTENANCE_CLOSED").USER_ID="other";if(fault=="task")native.Rows.OfType<CROSS_PERSONA_TASK>().First(row=>row.ASSIGNED_ROLE=="FacilityRole").TASK_STATUS="CANCELLED";
        if(fault=="delta"){var process=native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;payload["Data"]!["CompanyMaintenanceClosure"]!["Delta"]!["BeforeRevision"]=new string('F',64);process.INSTANCE_DATA_JSON=payload.ToJsonString();}if(fault=="foreign-actor")context=context with{ActorUserId="another-closer"};if(fault=="foreign-intent")reference=reference with{IntentId=Guid.NewGuid()};
        if(fault=="uncertain"){var result=await service.RecoverMaintenanceClosureAsync(context,reference,f.Flow.Authority);result.Receipt.Require(f.Command);}else await Assert.ThrowsAnyAsync<Exception>(()=>service.RecoverMaintenanceClosureAsync(context,reference,f.Flow.Authority));Assert.Equal(writes,native.Writes);
    }
}
#endif
