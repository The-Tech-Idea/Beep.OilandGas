#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Models.Processes;
using Moq;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingReportingClosureNativeTests
{
    internal sealed class Fixture
    {
        internal DrillingCompletionAcceptanceNativeTests.Fixture Accepted=new();
        internal DrillingExecutionAssignment Original=null!;
        internal DrillingReportingClosure Command=null!;
        internal DrillingExecutionStatus Execution=null!;
        internal int EvidenceReads;
        internal Action<int>? OnEvidence;
        internal Action? OnGuard;
        internal bool ReadingEvidence;
        internal PPDMProcessService Service=>Accepted.Native.Start.Lifecycle.Service;
        internal PersonaTaskRoutingTests.Fixture Store=>Accepted.Native.Start.Lifecycle.Native;
        internal async Task Prepare(int destinations=1)
        {
            await Accepted.Prepare();await Accepted.Decide();
            var reporting=new DrillingExecutionAssignmentNativeTests.Fixture();await reporting.Prepare(destinations);
            var definition=CompanyInventorySourceReview.Copy(reporting.Lifecycle.Definition);definition.PROCESS_DEFINITION_ID="reporting-definition";Store.Rows.Add(definition);
            var configured=(await Service.GetProcessDefinitionAsync(definition.PROCESS_DEFINITION_ID))!;
            var progress=DrillingAcceptedCompletionReadinessTests.AsOwnerReader(await Accepted.Progress(),reporting.Original.Scope().ActorUserId);
            Execution=progress.Source.History.Execution;
            Original=reporting.Original with{Source=Execution,Workflow=reporting.Original.Workflow with{ProcessId=configured.ProcessId,DefinitionRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(configured))}};
            await Service.RecordExecutionAssignmentAsync(Original,Guard,()=>Task.FromResult(Execution));
            var graph=await Recorded();var evidence=await Evidence();
            var review=new DrillingReportingClosureReview(PPDMProcessService.CurrentReportingGeneration(graph.Process),evidence,PPDMProcessService.ReportingProcessRevision(graph.Process),graph.NativeRevision,Original.SenderRole,Original.SenderPersonaCode,new('A',64),"",DateTimeOffset.UtcNow);
            review=review with{Revision=review.ContentRevision()};Command=new(review,Guid.NewGuid(),DateTimeOffset.UtcNow,"Accepted work is complete; close the original ongoing reporting responsibility",true);Command.Require();
            Accepted.Native.Mutations=0;EvidenceReads=0;
        }
        internal Task Guard(){OnGuard?.Invoke();return Accepted.Native.Guard();}
        internal async Task<DrillingAcceptedCompletionTests> Evidence()
        {
            Assert.DoesNotContain("begin",Store.Transactions.TakeLast(1));EvidenceReads++;OnEvidence?.Invoke(EvidenceReads);
            ReadingEvidence=true;
            try
            {
                var progress=DrillingAcceptedCompletionReadinessTests.AsOwnerReader(await Accepted.Progress(),Original.Scope().ActorUserId);
                return await Accepted.Physical.Service.ReadAcceptedCompletionTestsAsync(progress,Guard,()=>Task.CompletedTask);
            }
            finally{ReadingEvidence=false;}
        }
        internal Task<DrillingReportingClosureReceipt> Close(bool recover=false)=>Service.RecordReportingClosureAsync(Command,Guard,Evidence,recover);
        internal Task<(DrillingExecutionAssignment Original,ProcessInstance Process,IReadOnlyList<CROSS_PERSONA_TASK> Tasks,string NativeRevision)> Recorded()=>Service.ReadRecordedExecutionAssignmentAsync(Original.Scope().Company,Original.InstanceId(),Guard);
    }

    [Theory,InlineData(1),InlineData(2)]
    public async Task ClosureCompletesOnlyOriginalReportingTasksAndPreservesAcceptanceAndAssignmentRecovery(int destinations)
    {
        var f=new Fixture();await f.Prepare(destinations);var accepted=(await f.Accepted.Progress()).NativeRevision;var physical=JsonSerializer.Serialize(f.Accepted.Physical.Native.Rows);
        var result=await f.Close();result.Require(f.Command);Assert.True(result.Recorded);Assert.Equal(2,f.EvidenceReads);
        var graph=await f.Recorded();Assert.Equal(ProcessStatus.COMPLETED,graph.Process.Status);Assert.Equal("REPORTING_CLOSED",graph.Process.CurrentState);Assert.All(graph.Tasks,task=>Assert.Equal("COMPLETED",task.TASK_STATUS));
        Assert.Equal(f.Command.Review.Responsibility.TaskIds().Order(),graph.Tasks.Select(task=>task.CROSS_TASK_ID).Order());Assert.Equal(destinations+3,f.Accepted.Native.Mutations);
        Assert.Equal(accepted,(await f.Accepted.Progress()).NativeRevision);Assert.Equal(physical,JsonSerializer.Serialize(f.Accepted.Physical.Native.Rows));Assert.Equal("STARTED",f.Execution.Recorded!.STATUS);
        var writes=f.Accepted.Native.Mutations;Assert.True((await f.Close()).Recorded);Assert.True((await f.Close(true)).Recorded);
        Assert.True((await f.Service.RecordExecutionAssignmentAsync(f.Original,f.Guard,()=>Task.FromResult(f.Execution))).Recorded);
        Assert.True((await f.Service.RecordExecutionAssignmentAsync(f.Original,f.Guard,()=>Task.FromResult(f.Execution),true)).Recorded);Assert.Equal(writes,f.Accepted.Native.Mutations);
    }

    [Fact]
    public async Task ClosureAfterTransferClosesOnlyCurrentGenerationAndEarlierTransferStillRecovers()
    {
        var f=new Fixture();await f.Prepare();var original=f.Original;
        var destination=original.Workflow with { Step=original.Workflow.Step with { PersonaCodes=["NEW-REPORTER"] },
            Authority=original.Workflow.Authority with { Recipients=[new("new-reporting-account","NEW-REPORTER")],AuthorityRevision=new('D',64) } };
        var ready=await f.Service.ReviewReportingReassignmentAsync(original.Scope().Company,original.InstanceId(),destination,original.SenderRole,original.SenderPersonaCode,new('A',64),f.Guard,()=>Task.FromResult(f.Execution),()=>Task.FromResult(destination));
        var transfer=new DrillingReportingReassignment(ready,Guid.NewGuid(),DateTimeOffset.UtcNow,"Transfer continuing reporting to the replacement crew",true);
        await f.Service.RecordReportingReassignmentAsync(transfer,f.Guard,()=>Task.FromResult(f.Execution),()=>Task.FromResult(destination));
        var graph=await f.Recorded();var current=PPDMProcessService.CurrentReportingGeneration(graph.Process);Assert.Equal(2,current.Number);
        var archived=JsonSerializer.Serialize(graph.Tasks.Where(row=>row.PROCESS_STEP_INSTANCE_ID==transfer.Review.Responsibility.StepInstanceId));
        var accepted=(await f.Accepted.Progress()).NativeRevision;var evidence=await f.Evidence();
        var review=new DrillingReportingClosureReview(current,evidence,PPDMProcessService.ReportingProcessRevision(graph.Process),graph.NativeRevision,current.ManagerRole,current.ManagerPersona,new('A',64),"",DateTimeOffset.UtcNow);
        review=review with { Revision=review.ContentRevision() };f.Command=new(review,Guid.NewGuid(),DateTimeOffset.UtcNow,"Current accepted work permits closing the replacement reporting generation",true);
        var result=await f.Close();result.Require(f.Command);Assert.Single(result.TaskIds);
        graph=await f.Recorded();Assert.All(graph.Tasks,row=>Assert.Equal("COMPLETED",row.TASK_STATUS));
        Assert.Equal(archived,JsonSerializer.Serialize(graph.Tasks.Where(row=>row.PROCESS_STEP_INSTANCE_ID==transfer.Review.Responsibility.StepInstanceId)));
        Assert.Equal(accepted,(await f.Accepted.Progress()).NativeRevision);var writes=f.Accepted.Native.Mutations;
        f.Accepted.Physical.NullRows=true;f.Store.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);
        Assert.True((await f.Close(true)).Recorded);
        Assert.True((await f.Service.RecordReportingReassignmentAsync(transfer,f.Guard,()=>throw new InvalidOperationException("No source read during original transfer recovery"),()=>throw new InvalidOperationException("No destination read during original transfer recovery"),true)).Recorded);
        Assert.True((await f.Service.RecordExecutionAssignmentAsync(original,f.Guard,()=>Task.FromResult(f.Execution),true)).Recorded);Assert.Equal(writes,f.Accepted.Native.Mutations);
    }
    [Theory,InlineData(1),InlineData(2),InlineData(3),InlineData(4),InlineData(5)]
    public async Task EveryOwnedTaskProcessStepOrHistoryFailureRollsBackTheWholeClosure(int failure)
    {
        var f=new Fixture();await f.Prepare(2);var before=JsonSerializer.Serialize(f.Store.Rows);f.Accepted.Native.FailAt=failure;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Close());Assert.Equal(before,JsonSerializer.Serialize(f.Store.Rows));Assert.Equal("rollback",f.Store.Transactions.Last());
    }
    [Fact]public async Task LostAcknowledgementRecoversReadOnlyWithoutTestsOrDefinitionAndAbsentRecoveryNeverWrites()
    {
        var f=new Fixture();await f.Prepare();Assert.False((await f.Close(true)).Recorded);Assert.Equal(0,f.Accepted.Native.Mutations);Assert.Equal(0,f.EvidenceReads);
        bool armed=false;
        f.OnGuard=()=>{if(!armed && !f.ReadingEvidence && f.EvidenceReads>0 && f.Store.Transactions.LastOrDefault()=="begin"){armed=true;f.Store.FailCommit=true;}};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Close());f.Store.FailCommit=false;f.OnGuard=null;Assert.True(armed);
        Assert.Single(f.Store.Rows.OfType<PROCESS_HISTORY>(),row=>row.EVENT_TYPE=="REPORTING_CLOSED");Assert.Equal(4,f.Accepted.Native.Mutations);
        var writes=f.Accepted.Native.Mutations;var reads=f.EvidenceReads;
        f.Accepted.Physical.NullRows=true;f.Store.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);Assert.True((await f.Close(true)).Recorded);Assert.Equal(writes,f.Accepted.Native.Mutations);Assert.Equal(reads,f.EvidenceReads);
        f.Command=f.Command with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Close(true));Assert.Equal(writes,f.Accepted.Native.Mutations);
    }
    [Theory,InlineData("test"),InlineData("withdrawn"),InlineData("accepted-history"),InlineData("duplicate-accepted"),InlineData("pending-task"),InlineData("revoked")]
    public async Task ChangedCurrentAcceptanceSourceResponsibilityOrAuthorityCannotCloseReporting(string fault)
    {
        var f=new Fixture();await f.Prepare();
        if(fault=="test")f.Accepted.Physical.Row.REMARK="Changed since acceptance";
        if(fault=="withdrawn"){f.Accepted.Native.Start.Append(true);f.Accepted.CurrentHistory();}
        if(fault=="accepted-history")f.Store.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="COMPLETION_ACCEPTANCE_APPROVED").DETAILS="Changed";
        if(fault=="duplicate-accepted"){var copy=CompanyInventorySourceReview.Copy(f.Store.Rows.OfType<PROCESS_INSTANCE>().Single(row=>row.PROCESS_INSTANCE_ID==f.Command.Review.Evidence.Acceptance.InstanceId));copy.PROCESS_INSTANCE_ID="duplicate-accepted";f.Store.Rows.Add(copy);}
        if(fault=="pending-task")f.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.PROCESS_INSTANCE_ID==f.Original.InstanceId()).ASSIGNED_ROLE="ForeignRole";
        if(fault=="revoked")f.Accepted.Native.Revoked=true;
        var before=JsonSerializer.Serialize(f.Store.Rows);await Assert.ThrowsAnyAsync<Exception>(()=>f.Close());Assert.Equal(before,JsonSerializer.Serialize(f.Store.Rows));Assert.Equal(0,f.Accepted.Native.Mutations);
    }
    [Theory,InlineData("task"),InlineData("history"),InlineData("proof"),InlineData("process-audit")]
    public async Task ChangedCanonicalClosureCannotBeReadAcknowledgedOrRepaired(string fault)
    {
        var f=new Fixture();await f.Prepare();await f.Close();var writes=f.Accepted.Native.Mutations;
        var process=f.Store.Rows.OfType<PROCESS_INSTANCE>().Single(row=>row.PROCESS_INSTANCE_ID==f.Original.InstanceId());
        if(fault=="task")f.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.PROCESS_INSTANCE_ID==f.Original.InstanceId()).TASK_STATUS="PENDING";
        if(fault=="history")f.Store.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="REPORTING_CLOSED").EVENT_DATA_JSON="{}";
        if(fault=="process-audit")process.ROW_CHANGED_BY="another";
        if(fault=="proof"){var node=JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;node["Data"]![DrillingReportingClosure.PayloadKey]!["Command"]!["Reason"]="Changed rationale";process.INSTANCE_DATA_JSON=node.ToJsonString();}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Recorded());await Assert.ThrowsAnyAsync<Exception>(()=>f.Close(true));Assert.Equal(writes,f.Accepted.Native.Mutations);
    }
    [Fact]public async Task ClosingEvidenceChangeRefusesAcknowledgementAndRetainsRecoverableOriginal()
    {
        var f=new Fixture();await f.Prepare();f.OnEvidence=count=>{if(count==2)f.Accepted.Physical.Row.REMARK="Changed after commit";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Close());Assert.Single(f.Store.Rows.OfType<PROCESS_HISTORY>(),row=>row.EVENT_TYPE=="REPORTING_CLOSED");var writes=f.Accepted.Native.Mutations;
        Assert.True((await f.Close(true)).Recorded);Assert.Equal(writes,f.Accepted.Native.Mutations);
    }
    [Fact]public async Task ClosingCallbackCannotMutateReportingTasksAfterCurrentAcceptedEvidenceWasRead()
    {
        var f=new Fixture();await f.Prepare();f.OnEvidence=count=>{if(count==2)f.Store.Rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.PROCESS_INSTANCE_ID==f.Original.InstanceId()).TASK_STATUS="PENDING";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Close());Assert.Single(f.Store.Rows.OfType<PROCESS_HISTORY>(),row=>row.EVENT_TYPE=="REPORTING_CLOSED");
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Close(true));
    }
    [Theory,InlineData(false),InlineData(true)]public async Task RetaggedClosureHistoryKeyCannotBecomeConfirmedAbsence(bool recover)
    {
        var f=new Fixture();await f.Prepare();f.Store.Rows.Add(new PROCESS_HISTORY{PROCESS_HISTORY_ID=f.Command.HistoryId(),PROCESS_INSTANCE_ID="another-process",EVENT_TYPE="REPORTING_CLOSED"});var before=JsonSerializer.Serialize(f.Store.Rows);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Close(recover));Assert.Equal(before,JsonSerializer.Serialize(f.Store.Rows));Assert.Equal(0,f.Accepted.Native.Mutations);
    }
    [Fact]public async Task AuthorityCallbackCannotRebindTheOwnedLifecycleSource()
    {
        var f=new Fixture();await f.Prepare();var before=JsonSerializer.Serialize(f.Store.Rows);var foreign=new Mock<TheTechIdea.Beep.IDataSource>(MockBehavior.Strict);bool changed=false;
        f.OnGuard=()=>
        {
            if(!changed && !f.ReadingEvidence && f.EvidenceReads>0 && f.Store.Transactions.LastOrDefault()=="begin")
            {changed=true;f.Store.Editor.Setup(editor=>editor.GetDataSource("lifecycle")).Returns(foreign.Object);}
        };
        var error=await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Close());Assert.Equal("The reporting closure target changed.",error.Message);Assert.True(changed);
        Assert.Equal(before,JsonSerializer.Serialize(f.Store.Rows));Assert.Equal(0,f.Accepted.Native.Mutations);Assert.Equal("rollback",f.Store.Transactions.Last());foreign.VerifyNoOtherCalls();
    }
}
#endif
