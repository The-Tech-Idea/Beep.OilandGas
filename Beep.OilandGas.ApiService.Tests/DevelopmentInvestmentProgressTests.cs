#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DevelopmentInvestmentProgressTests
{
    internal static async Task<DevelopmentInvestmentProgress> Read(DevelopmentInvestmentStageNativeTests.Fixture f)
    {
        var owner=f.Handoff.Original;var recorded=await f.Handoff.Native.Service.ReadRecordedInvestmentHandoffAsync(owner.Scope().Company,owner.Scope().FieldId,owner.Review.Source.AfeId,f.Guard);
        return PPDMProcessService.InvestmentProgress(recorded!.Value.Original,recorded.Value.Process,recorded.Value.Tasks,f.Current);
    }
    [Fact]public async Task OriginalOrderedStagesAndRealPendingTasksReplaceTheHistoricalDestinationTotal()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(3);var initial=await Read(f);Assert.Equal("IN_REVIEW",initial.State);Assert.Equal(1,initial.PendingTasks);Assert.Equal(new[]{"PENDING","WAITING","WAITING"},initial.Stages.Select(s=>s.Status));
        await f.Reserve();var pending=await Read(f);Assert.Equal("RESERVED",pending.State);Assert.Equal(f.Command.Scope().ActorUserId,pending.ReservedBy);Assert.Null(pending.Stages[0].Reviewer);
        await f.Complete();var progress=await Read(f);Assert.Equal("IN_REVIEW",progress.State);Assert.Equal(1,progress.PendingTasks);Assert.Equal(2,f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().Count());
        Assert.Equal(new[]{"APPROVED","PENDING","WAITING"},progress.Stages.Select(s=>s.Status));Assert.Equal(f.Command.Rationale,progress.Stages[0].Rationale);Assert.Equal(f.Command.Scope().ActorUserId,progress.Stages[0].Reviewer);Assert.NotNull(progress.Stages[0].ApprovalId);Assert.NotNull(progress.Stages[0].HistoryId);Assert.Null(f.Current.Recorded);
        var writes=f.Mutations;var rows=JsonSerializer.Serialize(f.Handoff.Native.Native.Rows);var again=await Read(f);Assert.Equal(progress.Revision,again.Revision);Assert.Equal(writes,f.Mutations);Assert.Equal(rows,JsonSerializer.Serialize(f.Handoff.Native.Native.Rows));
    }
    [Fact]public async Task OriginalTimelineSurvivesLiveDefinitionRemovalAndConsumerMutation()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(2);await f.Reserve();await f.Complete();f.Handoff.Native.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);
        var before=await Read(f);var revision=before.Revision;((List<DevelopmentInvestmentStageProgress>)before.Stages).Clear();var after=await Read(f);Assert.Equal(2,after.Stages.Count);Assert.Equal(revision,after.Revision);Assert.Equal("APPROVED",after.Stages[0].Status);
    }
    [Theory][InlineData("APPROVE")][InlineData("REJECT")]
    public async Task ExactFinancialOnlyOutcomeIsDistinctFromConfirmedNativeCompletion(string decision)
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(1,decision);await f.Reserve();f.Financial();var financial=await Read(f);Assert.Equal("FINANCIAL_RECORDED",financial.State);Assert.Equal(1,financial.PendingTasks);Assert.Equal("RESERVED",financial.Stages[0].Status);Assert.Null(financial.Stages[0].Reviewer);
        await f.Complete();var complete=await Read(f);Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",complete.State);Assert.Equal(0,complete.PendingTasks);Assert.Null(complete.ReservedBy);Assert.Equal(f.Current.Recorded!.RecordedRevision,complete.FinancialRevision);Assert.Equal(decision,complete.Stages[0].Decision);
        f.Current=f.Current with{Recorded=null};await Assert.ThrowsAsync<InvalidOperationException>(()=>Read(f));
    }
    [Fact]public async Task RejectionLeavesLaterStagesNotReachedAndNoPendingTasks()
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(3,"REJECT");await f.Reserve();f.Financial();await f.Complete();var result=await Read(f);Assert.Equal("REJECTED",result.State);Assert.Equal(0,result.PendingTasks);Assert.Equal(new[]{"REJECTED","WAITING","WAITING"},result.Stages.Select(s=>s.Status));
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task AnUnrelatedValidFinancialDecisionCannotBecomeThisWorkflowOutcome(bool reserved)
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare();if(reserved)await f.Reserve();var other=f.Command with{IntentId=Guid.NewGuid(),Rationale="Another valid original financial intent"};f.Current=f.Current with{Recorded=DevelopmentInvestmentCommand.Facts(other.FinancialCommand().ToRecord(DateTime.UtcNow))};
        var writes=f.Mutations;var result=await Read(f);Assert.Equal("RECONCILIATION_REQUIRED",result.State);Assert.Equal(1,result.PendingTasks);Assert.Null(result.Stages[0].Reviewer);Assert.Equal(writes,f.Mutations);
    }
    [Theory][InlineData("task")][InlineData("history")][InlineData("approval")]
    public async Task ChangedCanonicalRowsCannotBePublishedAsAProgressTimeline(string fault)
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(2);await f.Reserve();await f.Complete();
        if(fault=="task")f.Handoff.Native.Native.Rows.OfType<CROSS_PERSONA_TASK>().First().TASK_STATUS="PENDING";
        else if(fault=="history")f.Handoff.Native.Native.Rows.OfType<PROCESS_HISTORY>().Last().DETAILS="Changed original audit";
        else f.Handoff.Native.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES="Changed original rationale";
        var writes=f.Mutations;await Assert.ThrowsAnyAsync<Exception>(()=>Read(f));Assert.Equal(writes,f.Mutations);
    }
    [Theory][InlineData("pending")][InlineData("order")][InlineData("financial")][InlineData("reviewer")][InlineData("audit")][InlineData("revision")]
    public async Task BrokenTransportProjectionIsRejectedEvenAfterItsEnvelopeHashIsRecomputed(string fault)
    {
        using var f=new DevelopmentInvestmentStageNativeTests.Fixture();await f.Prepare(2);await f.Reserve();await f.Complete();var result=await Read(f);var stages=result.Stages.ToArray();
        result=fault switch{"pending"=>result with{PendingTasks=0},"order"=>result with{Stages=stages.Reverse().ToArray()},"financial"=>result with{State="APPROVED"},"reviewer"=>result with{Stages=[stages[0] with{Reviewer=f.Handoff.Original.Scope().ActorUserId},stages[1]]},"audit"=>result with{Stages=[stages[0] with{ApprovalId=null},stages[1]]},_=>result with{NativeRevision="broken"}};
        result=result with{Revision=result.ContentRevision()};await Assert.ThrowsAsync<InvalidOperationException>(()=>Task.Run(()=>result.Require(f.Current,f.Handoff.Original.InstanceId(),f.Handoff.Original.Scope().ActorUserId,new DateTimeOffset(DateTime.SpecifyKind(f.Handoff.Original.Review.Source.Afe().CreatedAtUtc.UtcDateTime,DateTimeKind.Utc)))));
    }
}
#endif
