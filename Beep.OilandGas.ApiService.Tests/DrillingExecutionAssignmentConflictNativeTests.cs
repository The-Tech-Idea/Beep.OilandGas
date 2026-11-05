#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingExecutionAssignmentConflictNativeTests
{
    [Theory,InlineData(1),InlineData(2)]public async Task DifferentRecordedResponsibilityIsQualifiedWithoutChangingAnyGraph(int destinations)
    {
        var f=new DrillingExecutionAssignmentNativeTests.Fixture();await f.Prepare(destinations);await f.Start();var original=f.Original with{IntentId=Guid.NewGuid(),Reason="Different unrecorded original"};
        var before=JsonSerializer.Serialize(f.Lifecycle.Native.Rows);var writes=f.Lifecycle.Native.Writes;
        var result=await f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(original,f.Guard,f.Read);result.Require(original);
        Assert.Equal(f.Original.InstanceId(),result.Competing.InstanceId());Assert.Equal(destinations,result.Outcome.TaskIds.Count);Assert.True(result.OriginalConfirmedAbsent);
        Assert.Equal(before,JsonSerializer.Serialize(f.Lifecycle.Native.Rows));Assert.Equal(writes,f.Lifecycle.Native.Writes);
        result.Competing.Source.Recorded!.STATUS="Consumer mutation";
        Assert.Equal("STARTED",(await f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(original,f.Guard,f.Read)).Competing.Source.Recorded!.STATUS);
    }
    [Fact]public async Task AnotherOriginalSenderWithCurrentReadOnlySourceCanReviewTheRecordedSender()
    {
        var f=new DrillingExecutionAssignmentNativeTests.Fixture();await f.Prepare();await f.Start();
        var basis=f.Current.Basis with{Source=DrillingProgramAssignedReviewTests.AsReader(f.Current.Basis.Source,"other-coordinator"),RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};
        basis=basis with{Revision=basis.ContentRevision()};f.Current=f.Current with{Basis=basis};f.Current=f.Current with{Revision=f.Current.ContentRevision()};
        var original=f.Original with{Source=CompanyInventorySourceReview.Copy(f.Current),IntentId=Guid.NewGuid()};var writes=f.Lifecycle.Native.Writes;
        var result=await f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(original,f.Guard,f.Read);result.Require(original);
        Assert.Equal("other-coordinator",original.Scope().ActorUserId);Assert.NotEqual(original.Scope().ActorUserId,result.Competing.Scope().ActorUserId);Assert.Equal(writes,f.Lifecycle.Native.Writes);
    }
    [Theory,InlineData(false),InlineData(true)]public async Task NoCompetitorOrRecordedOriginalCannotBecomeClearable(bool recorded)
    {
        var f=new DrillingExecutionAssignmentNativeTests.Fixture();await f.Prepare();if(recorded)await f.Start();var writes=f.Lifecycle.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(f.Original,f.Guard,f.Read));Assert.Equal(writes,f.Lifecycle.Native.Writes);
    }
    [Theory,InlineData("parent"),InlineData("step-parent"),InlineData("step-key"),InlineData("history-key"),InlineData("approval-step"),InlineData("task-key"),InlineData("task-step")]
    public async Task OriginalOrRetaggedChildEvidenceNeverBecomesAbsence(string fault)
    {
        var f=new DrillingExecutionAssignmentNativeTests.Fixture();await f.Prepare();await f.Start();var original=f.Original with{IntentId=Guid.NewGuid()};var id=original.InstanceId();var step=DrillingExecutionAssignmentReceipt.StepId(id,original.Workflow.Step.StepId);
        object row=fault switch{
            "parent"=>new PROCESS_INSTANCE{PROCESS_INSTANCE_ID=id,ENTITY_ID="retagged"},
            "step-parent"=>new PROCESS_STEP_INSTANCE{PROCESS_STEP_INSTANCE_ID="orphan-step",PROCESS_INSTANCE_ID=id},
            "step-key"=>new PROCESS_STEP_INSTANCE{PROCESS_STEP_INSTANCE_ID=step,PROCESS_INSTANCE_ID="retagged"},
            "history-key"=>new PROCESS_HISTORY{PROCESS_HISTORY_ID="execution-assignment-start-"+ProcessDefinitionRevision.FromJson(id),PROCESS_INSTANCE_ID="retagged"},
            "approval-step"=>new PROCESS_APPROVAL{PROCESS_APPROVAL_ID="orphan-approval",PROCESS_INSTANCE_ID="retagged",PROCESS_STEP_INSTANCE_ID=step},
            "task-key"=>new CROSS_PERSONA_TASK{CROSS_TASK_ID=DrillingExecutionAssignmentReceipt.TaskId(id,step,original.Workflow.Step.PersonaCodes[0]),PROCESS_INSTANCE_ID="retagged",PROCESS_STEP_INSTANCE_ID="retagged",ENTITY_ID="retagged"},
            _=>new CROSS_PERSONA_TASK{CROSS_TASK_ID="retagged-task",PROCESS_INSTANCE_ID="retagged",PROCESS_STEP_INSTANCE_ID=step,ENTITY_ID="retagged"}};
        f.Lifecycle.Native.Rows.Add(row);var writes=f.Lifecycle.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(original,f.Guard,f.Read));Assert.Equal(writes,f.Lifecycle.Native.Writes);
    }
    [Theory,InlineData("duplicate"),InlineData("task"),InlineData("priority"),InlineData("isolation"),InlineData("borrowed"),InlineData("provider"),InlineData("revoked")]
    public async Task AmbiguousCorruptUnavailableOrUnauthorizedEvidenceRefusesInspection(string fault)
    {
        var f=new DrillingExecutionAssignmentNativeTests.Fixture();await f.Prepare();await f.Start();var original=f.Original with{IntentId=Guid.NewGuid()};var n=f.Lifecycle.Native;
        if(fault=="duplicate")n.Rows.Add(CompanyInventorySourceReview.Copy(n.Rows.OfType<PROCESS_INSTANCE>().Single()));
        if(fault=="task")n.Rows.OfType<CROSS_PERSONA_TASK>().Single().TASK_STATUS="COMPLETED";
        if(fault=="priority")n.Rows.OfType<CROSS_PERSONA_TASK>().Single().PRIORITY=1;
        if(fault=="isolation")n.Isolation=IsolationLevel.ReadCommitted;if(fault=="borrowed")n.Borrowed=true;
        if(fault=="provider")n.Source.SetupGet(s=>s.ErrorObject).Returns(new TheTechIdea.Beep.ConfigUtil.ErrorsInfo{Flag=TheTechIdea.Beep.ConfigUtil.Errors.Failed});
        Task Guard(){if(fault=="revoked")throw new UnauthorizedAccessException();return f.Guard();}var writes=n.Writes;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(original,Guard,f.Read));Assert.Equal(writes,n.Writes);
    }
    [Fact]public async Task ClosingSourceCannotHideNewOriginalEvidence()
    {
        var f=new DrillingExecutionAssignmentNativeTests.Fixture();await f.Prepare();await f.Start();var original=f.Original with{IntentId=Guid.NewGuid()};var count=0;
        Task<DrillingExecutionStatus> Read(){if(++count==2)f.Lifecycle.Native.Rows.Add(new PROCESS_HISTORY{PROCESS_HISTORY_ID="late-original",PROCESS_INSTANCE_ID=original.InstanceId()});return f.Read();}
        var writes=f.Lifecycle.Native.Writes;await Assert.ThrowsAnyAsync<Exception>(()=>f.Lifecycle.Service.ReadExecutionAssignmentConflictAsync(original,f.Guard,Read));Assert.Equal(2,count);Assert.Equal(writes,f.Lifecycle.Native.Writes);
    }
}
#endif
