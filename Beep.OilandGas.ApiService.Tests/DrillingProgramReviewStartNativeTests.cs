using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Processes;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramReviewStartNativeTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DrillingProgramDraftNativeTests.Fixture Program=new();internal CompanyProcessStartTests.Fixture Native=new("ConfiguredProgramReviewer");internal DrillingProgramReviewStart Original=null!;internal DrillingProgramReviewSource Current=null!;
        internal async Task Prepare(int stages=1)
        {
            await Program.Prepare();Native.Definition.PROCESS_TYPE="DRILLING";Native.Definition.ENTITY_TYPE="DRILLING_PROGRAM";Native.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=Enumerable.Range(1,stages).Select(index=>new ProcessStepDefinition{StepId="program-"+index,StepName="Independent program stage "+index,SequenceNumber=index,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["ConfiguredProgramReviewer"]}).ToArray()});
            var definition=(await Native.Service.GetProcessDefinitionAsync("definition"))!;Current=DrillingProgramReviewTests.Review(Program).Source;var options=definition.Steps.Select(step=>new DrillingProgramReviewStage(new(step.StepId,step.StepName,step.SequenceNumber,"ConfiguredProgramReviewer",["DRILLING_REVIEW"],step.SlaHours),new(step.StepId,"ConfiguredProgramReviewer",[new("program-reviewer","DRILLING_REVIEW")],new('A',64)))).ToArray();Original=new(Current,new(definition.ProcessId,definition.ProcessName,definition.Version,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(definition)),options),Guid.NewGuid());Original.Require();
        }
        internal Task Guard()=>Native.Guard();internal Task<DrillingProgramReviewSource> Read(){Assert.DoesNotContain("begin",Native.Native.Transactions.TakeLast(1));return Task.FromResult(Current);}
        internal Task<DrillingProgramReviewStartReceipt> Start(bool recover=false)=>Native.Service.RecordProgramReviewStartAsync(Original,Guard,Read,recover);
        public void Dispose()=>Program.Dispose();
    }
    [Theory][InlineData(1)][InlineData(2)]public async Task FrozenProgramSourceOrderedStepsAuditAndFirstReceivingTasksCommitOnce(int stages)
    {
        using var f=new Fixture();await f.Prepare(stages);var receipt=await f.Start();receipt.Require(f.Original);Assert.True(receipt.Recorded);Assert.Single(receipt.TaskIds);Assert.Equal(stages+3,f.Native.Native.Writes);Assert.Equal(new[]{"begin","commit"},f.Native.Native.Transactions);Assert.Equal("DRILLING_PROGRAM",Assert.Single(f.Native.Native.Rows.OfType<PROCESS_INSTANCE>()).ENTITY_TYPE);Assert.Equal("DRILLING_REVIEW",Assert.Single(f.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>()).TARGET_PERSONA_CODE);Assert.Empty(f.Native.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal(0,f.Program.Store.Native.Writes);
        Assert.True((await f.Start()).Recorded);Assert.True((await f.Start(true)).Recorded);Assert.Equal(receipt.InstanceId,await f.Native.Service.FindRecordedProgramReviewAsync(f.Original.Scope().Company,f.Original.Scope().FieldId,f.Original.Source.Program.PROGRAM_ID,f.Guard));Assert.Equal(stages+3,f.Native.Native.Writes);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]public async Task AnyProcessStepHistoryOrTaskFailureRollsBackTheCompleteNativeAssignment(int fail)
    {using var f=new Fixture();await f.Prepare();f.Native.FailWrite=fail;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Single(f.Native.Native.Rows);Assert.Equal("rollback",f.Native.Native.Transactions.Last());}
    [Fact]public async Task ReadOnlyAbsenceIsNotPartialEvidenceAndLostCommitRecoversWithoutAnotherWrite()
    {
        using var f=new Fixture();await f.Prepare();Assert.False((await f.Start(true)).Recorded);Assert.Equal(0,f.Native.Native.Writes);f.Native.Native.Rows.Add(new PROCESS_STEP_INSTANCE{PROCESS_STEP_INSTANCE_ID="orphan",PROCESS_INSTANCE_ID=f.Original.InstanceId()});await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));f.Native.Native.Rows.RemoveAll(row=>row is PROCESS_STEP_INSTANCE);
        f.Native.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());f.Native.Native.FailCommit=false;Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Native.Native.Writes);
    }
    [Fact]public async Task OriginalRecoveryDoesNotNeedLiveDefinitionAndNewIntentCannotDuplicateExistingProgramAssignment()
    {
        using var f=new Fixture();await f.Prepare();await f.Start();var original=f.Original;f.Original=original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Native.Native.Writes);f.Original=original;f.Native.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Native.Native.Writes);
    }
    [Theory][InlineData("missing")][InlineData("context")][InlineData("route")][InlineData("role")][InlineData("approval")][InlineData("retag")]
    public async Task MissingOrRetaggedNativeGraphCannotBeAcknowledgedOrRepaired(string fault)
    {
        using var f=new Fixture();await f.Prepare();await f.Start();var task=Assert.Single(f.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>());if(fault=="missing")f.Native.Native.Rows.Remove(task);if(fault=="context")task.TASK_CONTEXT_JSON="{}";if(fault=="route")task.ROUTE="/other";if(fault=="role")task.ASSIGNED_ROLE="ForeignRole";if(fault=="approval")f.Native.Native.Rows.Add(new PROCESS_APPROVAL{PROCESS_APPROVAL_ID="unqualified",PROCESS_INSTANCE_ID=f.Original.InstanceId()});if(fault=="retag")Assert.Single(f.Native.Native.Rows.OfType<PROCESS_INSTANCE>()).ENTITY_TYPE="WELL";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.FindRecordedProgramReviewAsync(f.Original.Scope().Company,f.Original.Scope().FieldId,f.Original.Source.Program.PROGRAM_ID,f.Guard));Assert.Equal(4,f.Native.Native.Writes);
    }
    [Theory][InlineData("borrowed")][InlineData("isolation")][InlineData("revoked")][InlineData("definition")][InlineData("source")]
    public async Task UnsupportedAuthoritySourceDefinitionOrTransactionCannotCreateAssignment(string fault)
    {
        using var f=new Fixture();await f.Prepare();if(fault=="borrowed")f.Native.Native.Borrowed=true;if(fault=="isolation")f.Native.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="revoked")f.Native.RevokeAt=1;if(fault=="definition")f.Native.ChangeDefinition=true;if(fault=="source"){f.Current.Program.PROGRAM_NAME="Unrecorded edit";f.Current=f.Current with{Revision=f.Current.ContentRevision()};}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Single(f.Native.Native.Rows);
    }
    [Fact]public async Task GenericProcessCommandsCannotMutateSourceBoundProgramReview()
    {
        using var f=new Fixture();await f.Prepare();await f.Start();var writes=f.Native.Native.Writes;var id=f.Original.InstanceId();await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.StartProcessAsync("definition",f.Original.Source.Program.PROGRAM_ID,"DRILLING_PROGRAM",f.Original.Scope().FieldId,f.Original.Scope().ActorUserId));await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.CancelProcessAsync(id,"Unqualified",f.Original.Scope().ActorUserId));await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.SkipStepAsync(id,f.Original.Workflow.Stages[0].Definition.StepId,"Unqualified",f.Original.Scope().ActorUserId));await Assert.ThrowsAnyAsync<Exception>(()=>f.Native.Service.AddHistoryEntryAsync(id,new(){Action="UNQUALIFIED"}));Assert.Equal(writes,f.Native.Native.Writes);
    }
}
#endif
