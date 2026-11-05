#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramReviewDecisionNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingProgramReviewStartNativeTests.Fixture Start=new();internal DrillingProgramReviewDecision Command=null!;internal DrillingProgramReviewSource Current=null!;internal int Mutations,FailAt;internal bool Revoked;
        internal async Task Prepare(int stages=1,string decision="APPROVE")
        {
            await Start.Prepare(stages);await Start.Start();Current=DrillingProgramAssignedReviewTests.AsReader(Start.Original.Source,"program-reviewer");var native=Start.Native.Native;
            ErrorsInfo Write(string table,object value,bool insert)
            {
                native.Writes++;Mutations++;if(Mutations==FailAt)return new(){Flag=Errors.Failed};var copy=JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
                if(insert)native.Rows.Add(copy);else{var key=table switch{"PROCESS_INSTANCE"=>"PROCESS_INSTANCE_ID","PROCESS_STEP_INSTANCE"=>"PROCESS_STEP_INSTANCE_ID","CROSS_PERSONA_TASK"=>"CROSS_TASK_ID",_=>throw new InvalidOperationException(table)};var identity=value.GetType().GetProperty(key)!.GetValue(value);var index=native.Rows.FindIndex(row=>row.GetType()==value.GetType() && Equals(row.GetType().GetProperty(key)!.GetValue(row),identity));if(index<0)throw new InvalidOperationException("Missing native row");native.Rows[index]=copy;}return new(){Flag=Errors.Ok};
            }
            native.Source.Setup(source=>source.InsertEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,true));native.Source.Setup(source=>source.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,false));Command=await Create(decision);
        }
        internal async Task<DrillingProgramReviewDecision> Create(string decision="APPROVE")
        {
            var graph=await Start.Native.Service.ReadRecordedProgramReviewAsync(Start.Original.Scope().Company,Start.Original.InstanceId(),Guard);var step=graph.Process.StepInstances.Single(row=>row.StepId==graph.Process.CurrentStepId);var task=graph.Tasks.Single(row=>row.PROCESS_STEP_INSTANCE_ID==step.StepInstanceId && row.TASK_STATUS=="PENDING");var packet=new DrillingProgramAssignedReview(Start.Original,Current,step.StepInstanceId,step.RequiredRole!,task.TARGET_PERSONA_CODE,task.CROSS_TASK_ID,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(graph.Process)),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(task)),graph.NativeRevision,"",DateTimeOffset.UtcNow);packet=packet with{Revision=packet.ContentRevision()};return new(packet,Guid.NewGuid(),decision,"Independent program design and original funding reviewed");
        }
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Revoked reviewer");return Task.CompletedTask;}
        internal Task<DrillingProgramReviewSource> Read(){Assert.DoesNotContain("begin",Start.Native.Native.Transactions.TakeLast(1));return Task.FromResult(Current);}
        internal Task<DrillingProgramReviewDecisionReceipt> Decide(bool recover=false)=>Start.Native.Service.RecordProgramReviewDecisionAsync(Command,Guard,Read,recover);
        public void Dispose()=>Start.Dispose();
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ExactIndependentDecisionCommitsAuditAndClosesTasksAndRecoveryNeverWrites(string decision)
    {
        using var f=new Fixture();await f.Prepare(1,decision);Assert.False((await f.Decide(true)).Recorded);Assert.Equal(0,f.Mutations);var receipt=await f.Decide();receipt.Require(f.Command);Assert.True(receipt.Recorded);Assert.Equal(decision=="APPROVE"?"REVIEW_APPROVED":"REVIEW_REJECTED",receipt.WorkflowState);Assert.Null(receipt.NextStepInstanceId);Assert.Equal("COMPLETED",Assert.Single(f.Start.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>()).TASK_STATUS);Assert.Single(f.Start.Native.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal(2,f.Start.Native.Native.Rows.OfType<PROCESS_HISTORY>().Count());var writes=f.Mutations;Assert.True((await f.Decide()).Recorded);f.Start.Native.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);Assert.Equal(0,f.Start.Program.Store.Native.Writes);Assert.Equal("DRAFT",f.Current.Program.PROGRAM_STATUS);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task OrderedApprovalRoutesOnlyNextConfiguredStageAndRejectionStopsTheSequence(string second)
    {
        using var f=new Fixture();await f.Prepare(2);var first=f.Command;var receipt=await f.Decide();Assert.Equal("program-2",receipt.WorkflowState);Assert.Equal(DrillingProgramReviewStartReceipt.StepId(first.Review.InstanceId,"program-2"),receipt.NextStepInstanceId);Assert.Single(f.Start.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.TASK_STATUS=="PENDING"));f.Command=await f.Create(second);Assert.Equal(2,f.Command.Review.StageNumber);Assert.True((await f.Decide()).Recorded);Assert.Empty(f.Start.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Where(row=>row.TASK_STATUS=="PENDING"));var writes=f.Mutations;f.Command=first;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);Assert.True((await f.Start.Start(true)).Recorded);Assert.Equal(first.Review.InstanceId,await f.Start.Native.Service.FindRecordedProgramReviewAsync(first.Scope().Company,first.Scope().FieldId,first.Review.Source.Program.PROGRAM_ID,f.Guard));
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)]
    public async Task EveryDecisionMutationRollsBackTheWholeGraphOnFailure(int fail)
    {
        using var f=new Fixture();await f.Prepare(2);var before=JsonSerializer.Serialize(f.Start.Native.Native.Rows);f.FailAt=fail;await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(before,JsonSerializer.Serialize(f.Start.Native.Native.Rows));Assert.Equal("rollback",f.Start.Native.Native.Transactions.Last());
    }
    [Fact]public async Task LostCommitIsRecoveredFromExactOriginalWithoutAnotherMutation()
    {
        using var f=new Fixture();await f.Prepare();f.Start.Native.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());f.Start.Native.Native.FailCommit=false;var writes=f.Mutations;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);f.Command=f.Command with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(writes,f.Mutations);
    }
    [Theory][InlineData("task")][InlineData("revision")][InlineData("source")][InlineData("role")][InlineData("definition")][InlineData("revoked")]
    public async Task ChangedDisplayedEvidenceAuthorityOrDefinitionCannotDecide(string fault)
    {
        using var f=new Fixture();await f.Prepare();var packet=f.Command.Review;if(fault=="task")packet=packet with{TaskRevision=new('F',64)};if(fault=="revision")packet=packet with{NativeRevision=new('F',64)};if(fault=="role")packet=packet with{AssignedRole="ForeignRole"};packet=packet with{Revision=packet.ContentRevision()};f.Command=f.Command with{Review=packet};if(fault=="source"){f.Current.Program.PROGRAM_NAME="Changed saved source";f.Current=f.Current with{Revision=f.Current.ContentRevision()};}if(fault=="definition")f.Start.Native.Definition.VERSION="changed";if(fault=="revoked")f.Revoked=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(0,f.Mutations);
    }
    [Theory][InlineData("approval")][InlineData("history")][InlineData("task")][InlineData("step")][InlineData("process")]
    public async Task ChangedCanonicalDecisionGraphCannotBeRepairedOrAcknowledged(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Decide();var native=f.Start.Native.Native;var writes=f.Mutations;if(fault=="approval")Assert.Single(native.Rows.OfType<PROCESS_APPROVAL>()).APPROVAL_NOTES="Forged";if(fault=="history")native.Rows.RemoveAll(row=>row is PROCESS_HISTORY history && history.EVENT_TYPE!="PROCESS_STARTED");if(fault=="task")Assert.Single(native.Rows.OfType<CROSS_PERSONA_TASK>()).TASK_STATUS="PENDING";if(fault=="step")Assert.Single(native.Rows.OfType<PROCESS_STEP_INSTANCE>()).STEP_NAME="Forged";if(fault=="process")Assert.Single(native.Rows.OfType<PROCESS_INSTANCE>()).STATUS="IN_PROGRESS";await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(writes,f.Mutations);
    }
}
#endif
