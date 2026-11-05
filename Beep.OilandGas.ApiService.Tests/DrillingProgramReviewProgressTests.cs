using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingProgramReviewProgressTests
{
    internal static async Task<DrillingProgramReviewProgress> Read(DrillingProgramReviewDecisionNativeTests.Fixture f,string? actor=null)
    {
        var graph=await f.Start.Native.Service.ReadRecordedProgramReviewAsync(f.Start.Original.Scope().Company,f.Start.Original.InstanceId(),f.Guard);var source=DrillingProgramAssignedReviewTests.AsReader(f.Current,actor??f.Command.Scope().ActorUserId);var result=PPDMProcessService.ProgramReviewProgress(graph.Original,graph.Process,graph.Tasks,source,graph.NativeRevision);result.Require(source.Basis.Scope().Company,source.Basis.Scope().ActorUserId,graph.Original.InstanceId());return result;
    }
    [Theory][InlineData("initial")][InlineData("intermediate")][InlineData("approved")][InlineData("rejected-first")][InlineData("rejected-later")]
    public async Task CompleteOriginalOrderedTimelineUsesRecordedDecisionsAndActualPendingTasks(string mode)
    {
        using var f=new DrillingProgramReviewDecisionNativeTests.Fixture();await f.Prepare(3,mode=="rejected-first"?"REJECT":"APPROVE");if(mode!="initial")await f.Decide();if(mode is "approved" or "rejected-later"){f.Command=await f.Create(mode=="rejected-later"?"REJECT":"APPROVE");await f.Decide();}if(mode=="approved"){f.Command=await f.Create();await f.Decide();}var writes=f.Mutations;var result=await Read(f);var scope=result.Source.Basis.Scope();result.Require(scope.Company,scope.ActorUserId,result.InstanceId);Assert.Equal(3,result.Stages.Count);Assert.Equal(mode=="approved"?"APPROVED":mode.StartsWith("rejected")?"REJECTED":"IN_REVIEW",result.State);Assert.Equal(mode is "initial" or "intermediate"?1:0,result.PendingTasks);Assert.Equal(mode=="rejected-first"?2:mode=="rejected-later"?1:0,result.Stages.Count(row=>row.State=="NOT_REACHED"));
        foreach(var row in result.Stages.Where(row=>row.Decision is not null)){Assert.Equal("program-reviewer",row.Reviewer);Assert.Equal("DRILLING_REVIEW",row.ReviewerPersona);Assert.Equal("Independent program design and original funding reviewed",row.Rationale);Assert.Equal(TimeSpan.Zero,row.AtUtc!.Value.Offset);Assert.NotNull(row.ApprovalId);Assert.NotNull(row.HistoryId);}Assert.Equal("DRAFT",result.Source.Program.PROGRAM_STATUS);Assert.Equal("KWD",result.Source.Program.AFE_CURRENCY);Assert.Equal(writes,f.Mutations);
        f.Start.Native.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);var sender=await Read(f,f.Start.Original.Scope().ActorUserId);Assert.Equal(result.State,sender.State);Assert.Equal(f.Start.Original.Scope().ActorUserId,sender.Source.Basis.Scope().ActorUserId);Assert.Equal(writes,f.Mutations);
    }
    [Fact]public async Task MultiplePersonaTasksCountReceivingDestinationsRatherThanRemainingStages()
    {
        using var f=new DrillingProgramReviewStartNativeTests.Fixture();await f.Prepare(3);var stages=f.Original.Workflow.Stages.Select(stage=>stage with{Definition=stage.Definition with{PersonaCodes=["DRILLING_REVIEW","RESERVOIR_REVIEW"]},Authority=stage.Authority with{Recipients=[new("program-reviewer","DRILLING_REVIEW"),new("second-reviewer","RESERVOIR_REVIEW")]}}).ToArray();f.Original=f.Original with{Workflow=f.Original.Workflow with{Stages=stages}};await f.Start();var graph=await f.Native.Service.ReadRecordedProgramReviewAsync(f.Original.Scope().Company,f.Original.InstanceId(),f.Guard);var result=PPDMProcessService.ProgramReviewProgress(graph.Original,graph.Process,graph.Tasks,f.Original.Source,graph.NativeRevision);Assert.Equal(2,result.PendingTasks);Assert.Equal(1,result.Stages.Count(row=>row.State=="PENDING"));Assert.Equal(2,result.Stages.Count(row=>row.State=="WAITING"));Assert.Equal(2,graph.Tasks.Count);
    }
    [Theory][InlineData("pending")][InlineData("outcome")][InlineData("reviewer")][InlineData("audit")][InlineData("order")][InlineData("missing")][InlineData("utc")][InlineData("company")][InlineData("actor")]
    public async Task IncompleteOrSubstitutedTimelineCannotBecomeAnOutcome(string fault)
    {
        using var f=new DrillingProgramReviewDecisionNativeTests.Fixture();await f.Prepare(2);await f.Decide();var result=await Read(f);var scope=result.Source.Basis.Scope();var stages=result.Stages.ToArray();if(fault=="pending")result=result with{PendingTasks=0};if(fault=="outcome")result=result with{State="APPROVED"};if(fault=="reviewer")stages[0]=stages[0] with{Reviewer=f.Start.Original.Scope().ActorUserId};if(fault=="audit")stages[0]=stages[0] with{HistoryId="forged"};if(fault=="utc")stages[0]=stages[0] with{AtUtc=stages[0].AtUtc!.Value.ToOffset(TimeSpan.FromHours(3))};if(fault=="order")Array.Reverse(stages);if(fault=="missing")stages=stages.Take(1).ToArray();result=result with{Stages=stages};result=result with{Revision=result.ContentRevision()};Assert.ThrowsAny<Exception>(()=>result.Require(fault=="company"?new("tenant","foreign"):scope.Company,fault=="actor"?"other":scope.ActorUserId,result.InstanceId));
    }
    [Fact]public async Task SameCompanyViewSourceCannotInventAnotherOriginalParticipant()
    {
        using var f=new DrillingProgramReviewDecisionNativeTests.Fixture();await f.Prepare();await Assert.ThrowsAnyAsync<Exception>(()=>Read(f,"unrelated-viewer"));Assert.Equal(0,f.Mutations);
    }
    [Fact]public async Task ChangedNativeApprovalCannotBeProjectedAsARecordedOutcome()
    {
        using var f=new DrillingProgramReviewDecisionNativeTests.Fixture();await f.Prepare();await f.Decide();var writes=f.Mutations;Assert.Single(f.Start.Native.Native.Rows.OfType<PROCESS_APPROVAL>()).APPROVAL_STATUS="PENDING";await Assert.ThrowsAnyAsync<Exception>(()=>Read(f));Assert.Equal(writes,f.Mutations);
    }
}
#endif
