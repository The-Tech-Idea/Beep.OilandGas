#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Beep.OilandGas.Web.Auth.Tests;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingCompletionReviewDecisionNativeTests
{
    internal sealed class Fixture
    {
        internal DrillingCompletionReviewStartNativeTests.Fixture Start=new();
        internal DrillingCompletionReviewDecision Command=null!;internal DrillingDailyReportPage Current=null!;
        internal int Mutations,FailAt;internal bool Revoked;internal Action<int>? OnRead;internal int Reads;
        internal async Task Prepare(int stages=1,string decision="APPROVE",int destinations=1,bool acceptanceReady=false)
        {
            await Start.Prepare(stages,destinations,acceptanceReady);await Start.Start();var native=Start.Lifecycle.Native;
            ErrorsInfo Write(string table,object value,bool insert)
            {
                native.Writes++;Mutations++;if(Mutations==FailAt)return new(){Flag=Errors.Failed};var copy=JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
                if(insert)native.Rows.Add(copy);else
                {
                    var key=table switch{"PROCESS_INSTANCE"=>"PROCESS_INSTANCE_ID","PROCESS_STEP_INSTANCE"=>"PROCESS_STEP_INSTANCE_ID","CROSS_PERSONA_TASK"=>"CROSS_TASK_ID",_=>throw new InvalidOperationException(table)};
                    var identity=value.GetType().GetProperty(key)!.GetValue(value);var index=native.Rows.FindIndex(row=>row.GetType()==value.GetType() && Equals(row.GetType().GetProperty(key)!.GetValue(row),identity));
                    if(index<0)throw new InvalidOperationException("Missing native row");native.Rows[index]=copy;
                }
                return new(){Flag=Errors.Ok};
            }
            native.Source.Setup(source=>source.InsertEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,true));
            native.Source.Setup(source=>source.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,false));
            Command=await Create(decision);
        }
        internal async Task<DrillingCompletionReviewDecision> Create(string decision="APPROVE")
        {var packet=await DrillingCompletionAssignedStateTests.Packet(Start);Current=packet.Source;var result=new DrillingCompletionReviewDecision(packet,Guid.NewGuid(),decision,"Independent review of this original reported claim; physical test acceptance and reporting closure remain separate");result.Require();return result;}
        internal Task Guard(){if(Revoked)throw new UnauthorizedAccessException("Reviewer revoked");return Task.CompletedTask;}
        internal Task<DrillingDailyReportPage> Read(){Assert.DoesNotContain("begin",Start.Lifecycle.Native.Transactions.TakeLast(1));OnRead?.Invoke(++Reads);return Task.FromResult(CompanyInventorySourceReview.Copy(Current));}
        internal Task<DrillingCompletionReviewDecisionReceipt> Decide(bool recover=false)=>Start.Lifecycle.Service.RecordCompletionReviewDecisionAsync(Command,Guard,Read,recover);
    }
    [Theory,InlineData("APPROVE"),InlineData("REJECT")]
    public async Task ExactIndependentDecisionClosesOnlyCompletionReviewTasksAndRecoveryNeverWrites(string decision)
    {
        var f=new Fixture();await f.Prepare(decision:decision);var reports=JsonSerializer.Serialize(f.Start.Rows);Assert.False((await f.Decide(true)).Recorded);Assert.Equal(0,f.Mutations);
        var receipt=await f.Decide();receipt.Require(f.Command);Assert.True(receipt.Recorded);Assert.Equal(decision=="APPROVE"?"REVIEW_APPROVED":"REVIEW_REJECTED",receipt.WorkflowState);
        Assert.All((await f.Start.Recorded()).Tasks,task=>Assert.Equal("COMPLETED",task.TASK_STATUS));var writes=f.Mutations;
        Assert.True((await f.Decide()).Recorded);f.Start.Lifecycle.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);Assert.True((await f.Decide(true)).Recorded);Assert.True((await f.Start.Start(true)).Recorded);
        Assert.Equal(writes,f.Mutations);Assert.Equal(reports,JsonSerializer.Serialize(f.Start.Rows));Assert.Equal("STARTED",f.Current.History.Execution.Recorded!.STATUS);
        var graph=await f.Start.Recorded();var progress=PPDMProcessService.CompletionReviewProgress(graph.Original,graph.Process,graph.Tasks,f.Current,graph.NativeRevision);
        Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",progress.State);Assert.Equal(0,progress.PendingTasks);Assert.Equal("COMPLETION_REPORTED",progress.Source.History.ProgressState);
    }
    [Theory,InlineData("APPROVE"),InlineData("REJECT")]
    public async Task OrderedReviewRoutesOnlyNextConfiguredStageAndPreservesEarlierDecisionRecovery(string last)
    {
        var f=new Fixture();await f.Prepare(2,destinations:2);var first=f.Command;var receipt=await f.Decide();Assert.Equal(2,(await f.Start.Recorded()).Tasks.Count(task=>task.TASK_STATUS=="PENDING"));
        Assert.Equal(DrillingCompletionReviewStartReceipt.StepId(first.Review.InstanceId,first.Review.Original.Workflow.Stages[1].Definition.StepId),receipt.NextStepInstanceId);
        f.Command=await f.Create(last);Assert.Equal(2,f.Command.Review.StageNumber);Assert.True((await f.Decide()).Recorded);var writes=f.Mutations;
        f.Command=first;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);Assert.True((await f.Start.Start(true)).Recorded);Assert.Equal(first.Review.InstanceId,await f.Start.Find());
        Assert.DoesNotContain((await f.Start.Recorded()).Tasks,task=>task.TASK_STATUS=="PENDING");
    }
    [Theory,InlineData(1),InlineData(2),InlineData(3),InlineData(4),InlineData(5),InlineData(6),InlineData(7),InlineData(8)]
    public async Task EveryDecisionMutationFailureRollsBackCompleteNativeGraph(int fail)
    {
        var f=new Fixture();await f.Prepare(2);var before=JsonSerializer.Serialize(f.Start.Lifecycle.Native.Rows);f.FailAt=fail;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(before,JsonSerializer.Serialize(f.Start.Lifecycle.Native.Rows));Assert.Equal("rollback",f.Start.Lifecycle.Native.Transactions.Last());
    }
    [Fact]
    public async Task LostCommitRecoversExactOriginalWithoutLiveDefinitionAndCannotBecomeAnotherIntent()
    {
        var f=new Fixture();await f.Prepare();f.Start.Lifecycle.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());f.Start.Lifecycle.Native.FailCommit=false;
        f.Start.Lifecycle.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);var writes=f.Mutations;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);
        f.Command=f.Command with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(writes,f.Mutations);
    }
    [Theory,InlineData("revision"),InlineData("task"),InlineData("role"),InlineData("definition"),InlineData("revoked"),InlineData("borrowed"),InlineData("isolation")]
    public async Task ChangedDisplayedProofAuthorityDefinitionOrTransactionCannotDecide(string fault)
    {
        var f=new Fixture();await f.Prepare();var packet=f.Command.Review;
        if(fault=="revision")packet=packet with{NativeRevision=new('F',64)};if(fault=="task")packet=packet with{TaskRevision=new('F',64)};if(fault=="role")packet=packet with{AssignedRole="ForeignRole"};
        packet=packet with{Revision=packet.ContentRevision()};f.Command=f.Command with{Review=packet};if(fault=="definition")f.Start.Lifecycle.Definition.VERSION="changed";if(fault=="revoked")f.Revoked=true;
        if(fault=="borrowed")f.Start.Lifecycle.Native.Borrowed=true;if(fault=="isolation")f.Start.Lifecycle.Native.Isolation=IsolationLevel.ReadCommitted;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(0,f.Mutations);
    }
    [Theory,InlineData("approval"),InlineData("history"),InlineData("task"),InlineData("step"),InlineData("process"),InlineData("start-audit")]
    public async Task AlteredCanonicalDecisionGraphCannotBeAcknowledgedOrRepaired(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.Decide();var rows=f.Start.Lifecycle.Native.Rows;var writes=f.Mutations;
        if(fault=="approval")Assert.Single(rows.OfType<PROCESS_APPROVAL>()).APPROVAL_NOTES="Altered";
        if(fault=="history")rows.RemoveAll(row=>row is PROCESS_HISTORY h && h.EVENT_TYPE!="PROCESS_STARTED");
        if(fault=="task")Assert.Single(rows.OfType<CROSS_PERSONA_TASK>()).TASK_STATUS="PENDING";
        if(fault=="step")Assert.Single(rows.OfType<PROCESS_STEP_INSTANCE>()).NOTES="Altered";
        if(fault=="process")Assert.Single(rows.OfType<PROCESS_INSTANCE>()).STATUS="IN_PROGRESS";
        if(fault=="start-audit")rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="PROCESS_STARTED").DETAILS="Altered";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(writes,f.Mutations);
    }
    [Theory,InlineData(false),InlineData(true)]
    public async Task LaterHistoryRequiresFreshDecisionAndWithdrawnClaimCanOnlyBeRejected(bool withdraw)
    {
        var f=new Fixture();await f.Prepare();f.Start.Append(withdraw);f.Current=(await DrillingCompletionAssignedStateTests.Packet(f.Start)).Source;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(0,f.Mutations);
        if(withdraw)await Assert.ThrowsAnyAsync<Exception>(async()=>{f.Command=await f.Create();});
        f.Command=await f.Create("REJECT");Assert.True((await f.Decide()).Recorded);Assert.True((await f.Decide(true)).Recorded);
    }
    [Theory,InlineData("approval"),InlineData("history")]
    public async Task RetaggedOriginalDecisionKeyCannotBecomeConfirmedAbsence(string kind)
    {
        var f=new Fixture();await f.Prepare();f.Start.Lifecycle.Native.Rows.Add(kind=="approval"?new PROCESS_APPROVAL{PROCESS_APPROVAL_ID=f.Command.AuditId("approval"),PROCESS_INSTANCE_ID="retagged"}:new PROCESS_HISTORY{PROCESS_HISTORY_ID=f.Command.AuditId("decision"),PROCESS_INSTANCE_ID="retagged"});
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(0,f.Mutations);
    }
    [Fact]
    public async Task ClosingSourceCallbackCannotHideNewNativeOrphanAfterDecisionCommit()
    {
        var f=new Fixture();await f.Prepare();var orphan=new PROCESS_HISTORY{PROCESS_HISTORY_ID="late-completion-orphan",PROCESS_INSTANCE_ID=f.Command.Review.InstanceId,EVENT_TYPE="UNQUALIFIED"};
        f.OnRead=count=>{if(count==2)f.Start.Lifecycle.Native.Rows.Add(orphan);};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Single(f.Start.Lifecycle.Native.Rows.OfType<PROCESS_APPROVAL>());var writes=f.Mutations;
        f.Start.Lifecycle.Native.Rows.Remove(orphan);f.OnRead=null;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);
    }
    [Fact]
    public async Task ReviewDecisionPreservesOngoingReportingResponsibilityAndItsWholePendingGraph()
    {
        var f=new Fixture();await f.Prepare();var reportingTemplate=new DrillingExecutionAssignmentNativeTests.Fixture();await reportingTemplate.Prepare();
        var definition=CompanyInventorySourceReview.Copy(reportingTemplate.Lifecycle.Definition);definition.PROCESS_DEFINITION_ID="completion-decision-ongoing-reporting";f.Start.Lifecycle.Native.Rows.Add(definition);
        var configured=(await f.Start.Lifecycle.Service.GetProcessDefinitionAsync(definition.PROCESS_DEFINITION_ID))!;
        var option=reportingTemplate.Original.Workflow with{ProcessId=configured.ProcessId,DefinitionRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured))};
        var reporting=reportingTemplate.Original with{Source=CompanyInventorySourceReview.Copy(f.Start.Current.History.Execution),Workflow=option,IntentId=Guid.NewGuid()};
        Assert.True((await f.Start.Lifecycle.Service.RecordExecutionAssignmentAsync(reporting,f.Guard,()=>Task.FromResult(f.Start.Current.History.Execution))).Recorded);
        string OtherGraph()=>JsonSerializer.Serialize(f.Start.Lifecycle.Native.Rows.Where(row=>row.GetType().GetProperty("PROCESS_INSTANCE_ID")?.GetValue(row)?.ToString()==reporting.InstanceId()).ToArray());
        var before=OtherGraph();Assert.True((await f.Decide()).Recorded);Assert.True((await f.Decide(true)).Recorded);Assert.Equal(before,OtherGraph());
        Assert.Equal("PENDING",f.Start.Lifecycle.Native.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.PROCESS_INSTANCE_ID==reporting.InstanceId()).TASK_STATUS);
    }
}
#endif
