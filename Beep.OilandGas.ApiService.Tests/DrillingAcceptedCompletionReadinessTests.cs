#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Auth.Tests;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingAcceptedCompletionReadinessTests
{
    internal static DrillingCompletionReviewProgress AsOwnerReader(DrillingCompletionReviewProgress original,string actor)
    {
        var page=original.Source;var basis=page.History.Execution.Basis with{Source=DrillingProgramAssignedReviewTests.AsReader(page.History.Execution.Basis.Source,actor),RecorderRoles=[],PersonaCode=null,RecorderAuthorityRevision=null};basis=basis with{Revision=basis.ContentRevision()};
        var execution=page.History.Execution with{Basis=basis};execution=execution with{Revision=execution.ContentRevision()};var history=page.History with{Execution=execution};history=history with{Revision=history.ContentRevision()};page=page with{History=history};page=page with{Revision=page.ContentRevision()};var result=original with{Source=page};result=result with{Revision=result.ContentRevision()};result.RequireFacts(basis.Scope().Company,actor,result.InstanceId);return result;
    }
    [Fact]public async Task ExplicitOwnerReadAdmitsCurrentManagerFactsWithoutForgingAnAssignedReviewerOrChangingAcceptance()
    {
        var f=new DrillingCompletionAcceptanceNativeTests.Fixture();await f.Prepare();await f.Decide();var progress=AsOwnerReader(await f.Progress(),"reporting-manager");Assert.False(progress.IsParticipant("reporting-manager"));var before=f.Native.Mutations;var callbacks=0;
        var result=await f.Physical.Service.ReadAcceptedCompletionTestsAsync(progress,f.Physical.Guard,()=>{callbacks++;return Task.CompletedTask;});result.Require(progress.Source.History.Execution.Basis.Scope().Company,"reporting-manager",progress.InstanceId);
        Assert.Equal(2,result.Tests.Count);Assert.Equal(progress.Stages.Last().AcceptedDecision!.Acceptance!.Tests.Select(test=>test.Native),result.Tests);Assert.Equal(1,callbacks);Assert.Equal(before,f.Native.Mutations);Assert.Equal(0,f.Physical.Native.Writes);
    }
    [Theory,InlineData("result"),InlineData("unprojected-value"),InlineData("missing"),InlineData("unavailable")]
    public async Task RecordedAcceptanceHistoryCannotSubstituteForCurrentPhysicalTestReadiness(string fault)
    {
        var f=new DrillingCompletionAcceptanceNativeTests.Fixture();await f.Prepare();await f.Decide();var progress=AsOwnerReader(await f.Progress(),"reporting-manager");
        if(fault=="result")f.Physical.Row.TEST_RESULT_CODE="Changed since acceptance";if(fault=="unprojected-value")f.Physical.Row.CASING_PRESSURE=10m;if(fault=="missing")f.Physical.Native.Rows.Remove(f.Physical.Row);if(fault=="unavailable")f.Physical.NullRows=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Physical.Service.ReadAcceptedCompletionTestsAsync(progress,f.Physical.Guard,()=>Task.CompletedTask));Assert.True((await f.Progress()).OriginalCompletionAccepted);Assert.Equal(0,f.Physical.Native.Writes);
    }
    [Theory,InlineData("test"),InlineData("authority")]
    public async Task ClosingSourceChangesAndRevocationRefuseReadiness(string fault)
    {
        var f=new DrillingCompletionAcceptanceNativeTests.Fixture();await f.Prepare();await f.Decide();var progress=AsOwnerReader(await f.Progress(),"reporting-manager");
        Task Confirm(){if(fault=="test")f.Physical.Row.REMARK="Changed during closing program/source proof";else f.Physical.Revoked=true;return Task.CompletedTask;}
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Physical.Service.ReadAcceptedCompletionTestsAsync(progress,f.Physical.Guard,Confirm));Assert.Equal(0,f.Physical.Native.Writes);
    }
    [Fact]public async Task WithdrawnAcceptedClaimIsHistoricalAndCannotBecomeCurrentClosureReadiness()
    {
        var f=new DrillingCompletionAcceptanceNativeTests.Fixture();await f.Prepare();await f.Decide();f.Native.Start.Append(true);f.CurrentHistory();var progress=AsOwnerReader(await f.Progress(),"reporting-manager");Assert.True(progress.OriginalCompletionAccepted);Assert.False(progress.AcceptedClaimIsCurrent);
        var reads=f.Physical.TestReads;await Assert.ThrowsAnyAsync<Exception>(()=>f.Physical.Service.ReadAcceptedCompletionTestsAsync(progress,f.Physical.Guard,()=>Task.CompletedTask));Assert.Equal(reads,f.Physical.TestReads);
    }
}
#endif
