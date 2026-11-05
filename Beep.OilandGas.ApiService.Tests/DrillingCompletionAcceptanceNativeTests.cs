#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingCompletionAcceptanceNativeTests
{
    internal sealed class Fixture
    {
        internal DrillingCompletionReviewDecisionNativeTests.Fixture Native=new();internal DrillingCompletionTestInspectionTests.Fixture Physical=new();internal int Confirmations;
        internal async Task Prepare(int stages=1,bool ready=true,bool advance=true)
        {await Native.Prepare(stages,acceptanceReady:ready);if(stages>1 && advance){await Native.Decide();Native.Command=await Native.Create();}await AddEvidence();}
        internal async Task AddEvidence()
        {
            await Physical.Prepare(Native.Command.Review);var tests=Native.Command.Review.Original.Source.ReportedCompletion!.Report.Tests;
            for(int index=1;index<tests.Count;index++){var row=CompanyInventorySourceReview.Copy(Physical.Row);row.TEST_NUM="test-"+(index+1);row.REPORT_TEST_NUM=tests[index].Reference;row.TEST_DATE=DateTime.SpecifyKind(tests[index].TestedAtUtc.UtcDateTime,DateTimeKind.Unspecified);Physical.Native.Rows.Add(row);}
            var accepted=new List<DrillingCompletionAcceptedTest>();
            for(int index=0;index<tests.Count;index++){var inspected=await Inspect(new(Native.Command.Review,index,Physical.Row.SOURCE,Physical.Row.TEST_TYPE,Physical.Row.RUN_NUM,"test-"+(index+1)));accepted.Add(Sign(inspected));}
            Native.Command=Native.Command with{Acceptance=new(Native.Command.Review.Revision,DateTimeOffset.UtcNow,accepted,true)};
        }
        internal static DrillingCompletionAcceptedTest Sign(DrillingCompletionTestInspection inspected,string dateResolution="")=>new(inspected.Selection.ReportedTestIndex,inspected.Native,inspected.Revision,inspected.CheckedAtUtc,"Approved acceptance criteria / specification 42","Signed UTC certificate / reference "+inspected.Reported.Reference,dateResolution,"Independent checked certificate meets the referenced acceptance criteria",true,true);
        internal Task<DrillingCompletionTestInspection> Inspect(DrillingCompletionTestSelection selection)=>Physical.Service.InspectCompletionTestAsync(selection,Physical.Guard,()=>Task.CompletedTask);
        internal async Task Confirm(DrillingCompletionAcceptance acceptance)
        {
            Assert.DoesNotContain("begin",Native.Start.Lifecycle.Native.Transactions.TakeLast(1));Confirmations++;
            foreach(var test in acceptance.Tests){var current=await Inspect(test.Selection(Native.Command.Review));Assert.Equal(test.InspectionRevision,current.Revision);Assert.Equal(test.Native,current.Native);}
        }
        internal Task<DrillingCompletionReviewDecisionReceipt> Decide(bool recover=false,bool reader=true)=>Native.Start.Lifecycle.Service.RecordCompletionReviewDecisionAsync(Native.Command,Native.Guard,Native.Read,recover,reader?Confirm:null);
        internal async Task<DrillingCompletionReviewProgress> Progress()
        {var graph=await Native.Start.Recorded();return PPDMProcessService.CompletionReviewProgress(graph.Original,graph.Process,graph.Tasks,Native.Current,graph.NativeRevision);}
        internal void CurrentHistory()
        {
            var page=Native.Start.Current;var execution=page.History.Execution with{Basis=Native.Current.History.Execution.Basis};execution=execution with{Revision=execution.ContentRevision()};var history=page.History with{Execution=execution};history=history with{Revision=history.ContentRevision()};Native.Current=page with{History=history};Native.Current=Native.Current with{Revision=Native.Current.ContentRevision()};Native.Current.Require();
        }
    }
    [Fact]public async Task ExactFinalAcceptanceStoresEveryIndependentTestWithCanonicalAuditAndNeverClosesExecution()
    {
        var f=new Fixture();await f.Prepare();var rows=JsonSerializer.Serialize(f.Physical.Native.Rows);var reports=JsonSerializer.Serialize(f.Native.Start.Rows);
        var receipt=await f.Decide();receipt.Require(f.Native.Command);Assert.True(receipt.CompletionAccepted);Assert.Equal(2,f.Confirmations);Assert.Equal(2,f.Native.Command.Acceptance!.Tests.Count);
        var approval=Assert.Single(f.Native.Start.Lifecycle.Native.Rows.OfType<PROCESS_APPROVAL>());Assert.Equal("COMPLETION_ACCEPTANCE",approval.APPROVAL_TYPE);
        var history=f.Native.Start.Lifecycle.Native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="COMPLETION_ACCEPTANCE_APPROVED");Assert.Contains(f.Native.Command.Revision(),history.EVENT_DATA_JSON);
        var progress=await f.Progress();Assert.True(progress.OriginalCompletionAccepted);Assert.True(progress.AcceptedClaimIsCurrent);Assert.Equal("APPROVED",progress.State);Assert.Equal("STARTED",progress.Source.History.Execution.Recorded!.STATUS);Assert.Equal(rows,JsonSerializer.Serialize(f.Physical.Native.Rows));Assert.Equal(reports,JsonSerializer.Serialize(f.Native.Start.Rows));
    }
    [Theory,InlineData(1),InlineData(2),InlineData(3),InlineData(4),InlineData(5),InlineData(6)]
    public async Task EveryAcceptanceMutationFailureRollsBackWholeNativeReview(int failure)
    {var f=new Fixture();await f.Prepare();var before=JsonSerializer.Serialize(f.Native.Start.Lifecycle.Native.Rows);f.Native.FailAt=failure;await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(before,JsonSerializer.Serialize(f.Native.Start.Lifecycle.Native.Rows));}
    [Theory,InlineData("outstanding"),InlineData("not-final"),InlineData("missing-test"),InlineData("duplicate-test"),InlineData("criteria"),InlineData("utc-evidence"),InlineData("confirmation"),InlineData("actual-offset"),InlineData("future"),InlineData("before-inspection"),InlineData("self-edited"),InlineData("source-reader")]
    public async Task IncompleteNonfinalUnconfirmedOrUnqualifiedAcceptanceNeverWrites(string fault)
    {
        var f=new Fixture();await f.Prepare(fault=="not-final"?2:1,fault!="outstanding",false);var acceptance=f.Native.Command.Acceptance!;var tests=acceptance.Tests.ToArray();
        if(fault=="missing-test")acceptance=acceptance with{Tests=[]};if(fault=="duplicate-test")acceptance=acceptance with{Tests=tests.Select(_=>tests[0]).ToArray()};
        if(fault is "criteria" or "utc-evidence" or "self-edited")
        {
            if(fault=="self-edited"){f.Physical.Row.ROW_CHANGED_BY=f.Native.Command.Scope().ActorUserId;tests[0]=Fixture.Sign(await f.Inspect(tests[0].Selection(f.Native.Command.Review)));}
            else tests[0]=fault=="criteria"?tests[0] with{CriteriaReference=""}:tests[0] with{ConfirmedReportedUtc=false};acceptance=acceptance with{Tests=tests,AcceptedAtUtc=DateTimeOffset.UtcNow};
        }
        if(fault=="confirmation")acceptance=acceptance with{ConfirmedCompletion=false};if(fault=="actual-offset")acceptance=acceptance with{AcceptedAtUtc=acceptance.AcceptedAtUtc.ToOffset(TimeSpan.FromHours(3))};if(fault=="future")acceptance=acceptance with{AcceptedAtUtc=DateTimeOffset.UtcNow.AddDays(1)};if(fault=="before-inspection")acceptance=acceptance with{AcceptedAtUtc=tests[0].InspectedAtUtc.AddSeconds(-1)};
        f.Native.Command=f.Native.Command with{Acceptance=acceptance};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(reader:fault!="source-reader"));Assert.Equal(0,f.Native.Mutations);
    }
    [Fact]public async Task NativeCalendarDifferenceRequiresExplicitReviewerResolutionAndNeverInfersTimezone()
    {
        var f=new Fixture();await f.Prepare();f.Physical.Row.TEST_DATE=f.Physical.Row.TEST_DATE!.Value.AddDays(1);var checkedTest=await f.Inspect(f.Native.Command.Acceptance!.Tests[0].Selection(f.Native.Command.Review));Assert.False(checkedTest.CalendarDateMatches);
        var tests=f.Native.Command.Acceptance.Tests.ToArray();tests[0]=Fixture.Sign(checkedTest);f.Native.Command=f.Native.Command with{Acceptance=f.Native.Command.Acceptance with{Tests=tests,AcceptedAtUtc=DateTimeOffset.UtcNow}};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(0,f.Native.Mutations);
        tests[0]=Fixture.Sign(checkedTest,"Native record uses the local calendar day; the signed certificate explicitly confirms the reported UTC instant");f.Native.Command=f.Native.Command with{Acceptance=f.Native.Command.Acceptance with{Tests=tests,AcceptedAtUtc=DateTimeOffset.UtcNow}};Assert.True((await f.Decide()).CompletionAccepted);
    }
    [Fact]public async Task ChangedNativeTestRefusesBeforeWriteAndLostCommitRecoveryNeverRequiresCurrentTestSource()
    {
        var f=new Fixture();await f.Prepare();f.Physical.Row.TEST_RESULT_CODE="changed";await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(0,f.Native.Mutations);f.Physical.Row.TEST_RESULT_CODE=f.Native.Command.Acceptance!.Tests[0].Native.ResultCode;
        f.Native.Start.Lifecycle.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());f.Native.Start.Lifecycle.Native.FailCommit=false;var writes=f.Native.Mutations;var calls=f.Confirmations;f.Physical.NullRows=true;f.Native.Start.Lifecycle.Native.Rows.RemoveAll(row=>row is PROCESS_DEFINITION);
        Assert.True((await f.Decide(true,false)).CompletionAccepted);Assert.Equal(writes,f.Native.Mutations);Assert.Equal(calls,f.Confirmations);
    }
    [Fact]public async Task AcceptedEvidenceTamperingCannotEscapeItsCanonicalHistoryDigest()
    {
        var f=new Fixture();await f.Prepare();await f.Decide();var process=Assert.Single(f.Native.Start.Lifecycle.Native.Rows.OfType<PROCESS_INSTANCE>());var node=JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;
        node["Data"]!["DrillingCompletionDecisions"]![0]!["Command"]!["Acceptance"]!["Tests"]![0]!["CriteriaReference"]="Another criterion with unchanged native audit";process.INSTANCE_DATA_JSON=node.ToJsonString();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Progress());await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));
    }
    [Fact]public async Task ClosingTestChangeRefusesAcknowledgementButRecoveryPreservesTheCommittedOriginal()
    {
        var f=new Fixture();await f.Prepare();f.Native.OnRead=count=>{if(count==2)f.Physical.Row.TEST_RESULT_CODE="Changed after the owned acceptance commit";};
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Single(f.Native.Start.Lifecycle.Native.Rows.OfType<PROCESS_APPROVAL>(),row=>row.APPROVAL_TYPE=="COMPLETION_ACCEPTANCE");var writes=f.Native.Mutations;
        f.Native.OnRead=null;Assert.True((await f.Decide(true,false)).CompletionAccepted);Assert.Equal(writes,f.Native.Mutations);Assert.True((await f.Progress()).OriginalCompletionAccepted);
    }
    [Fact]public async Task OrderedAcceptanceAndLaterWithdrawalPreserveOriginalSignoffAndReadOnlyRecovery()
    {
        var f=new Fixture();await f.Prepare(2);Assert.True((await f.Decide()).CompletionAccepted);var writes=f.Native.Mutations;f.Native.Start.Append(true);f.CurrentHistory();
        var progress=await f.Progress();Assert.True(progress.OriginalCompletionAccepted);Assert.False(progress.AcceptedClaimIsCurrent);Assert.True((await f.Decide(true,false)).CompletionAccepted);Assert.Equal(writes,f.Native.Mutations);Assert.Equal("STARTED",progress.Source.History.Execution.Recorded!.STATUS);
    }
}
#endif
