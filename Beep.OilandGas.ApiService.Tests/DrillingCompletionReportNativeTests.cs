using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingCompletionReportNativeTests
{
    internal static DrillingCompletionReport Evidence() => new(DateTimeOffset.UtcNow, "Drilling work reported complete against the signed site report",
        [new("Recorded pressure test", "Signed test certificate / site reference 42", DateTimeOffset.UtcNow, "REPORTED_PASS", "Inspector-issued certificate referenced by the recorder", "Recorder reports the stated test result; independent acceptance is pending")],
        ["Independent inspection acceptance remains outstanding"], true);
    internal static DrillingDailyReportInput Complete(DrillingDailyReportNativeTests.Fixture f, DRILLING_DAILY_REPORT target) =>
        DrillingDailyReportInput.FromRecord(target) with { Basis = f.Basis, IntentId = Guid.NewGuid(),
            Correction = new(target.DDR_ID, target.REPORT_REVISION, target.REPORT_NUMBER, "Report actual completion and supporting references against the final observation"), Completion = Evidence() };
    [Fact]
    public async Task CompletionAppendsAttributableReportedEvidenceWithoutAcceptingTestsOrChangingTheExecutionStart()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f);
        var originals = JsonSerializer.Serialize(f.Store.Store.Native.Rows); f.Command = Complete(f, rows[2]); var saved = await f.Save();
        Assert.Equal(originals, JsonSerializer.Serialize(f.Store.Store.Native.Rows.Where(row => row is not DRILLING_DAILY_REPORT report || report.REPORT_NUMBER < 4)));
        var page = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        Assert.Equal(4, page.History.Count); Assert.Equal(3, page.History.ObservedPeriodCount); Assert.Equal("COMPLETION_REPORTED", page.History.ProgressState);
        var completion = page.History.ReportedCompletion!; Assert.Equal(saved.ReportId, completion.ReportId); Assert.Equal(saved.ReportId, page.History.LatestObservedReport!.ReportId);
        Assert.Equal(f.Basis.Scope().ActorUserId, completion.RecorderUserId); Assert.Equal(f.Command.RecorderRole, completion.RecorderRole);
        Assert.Equal("REPORTED_PASS", completion.Report.Tests.Single().ReportedResult); Assert.NotEmpty(completion.Report.OutstandingItems);
        Assert.Equal("STARTED", page.History.Execution.Recorded!.STATUS); Assert.True(completion.Report.CompletedAtUtc >= new DateTimeOffset(DateTime.SpecifyKind(rows[2].PERIOD_END_UTC!.Value, DateTimeKind.Utc)));
        Assert.Equal(saved.ReportId, page.Reports[2].SupersededByReportId); Assert.NotNull(page.Reports[3].Completion);
    }
    [Theory, InlineData("REPORTED_FAIL"), InlineData("INCONCLUSIVE")]
    public async Task FailedOrInconclusiveReportedTestsRemainUnacceptedWithOutstandingWork(string result)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); var command = Complete(f, rows[2]);
        f.Command = command with { Completion = command.Completion! with { Tests = [command.Completion.Tests.Single() with { ReportedResult = result, TestedAtUtc = f.Start.Command.SpudAtUtc.AddHours(-1) }] } };
        await f.Save(); var history = await f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard);
        Assert.Equal(result, history.ReportedCompletion!.Report.Tests.Single().ReportedResult); Assert.NotEmpty(history.ReportedCompletion.Report.OutstandingItems);
        Assert.Equal("COMPLETION_REPORTED", history.ProgressState); Assert.Equal("STARTED", history.Execution.Recorded!.STATUS);
    }
    [Fact]
    public async Task CompletionCannotBeClaimedAgainstAnEarlierObservedPeriod()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[0]);
        var writes = f.Store.Store.Native.Writes; await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(writes, f.Store.Store.Native.Writes);
    }
    [Theory]
    [InlineData("work")] [InlineData("tests")] [InlineData("result")] [InlineData("test-offset")] [InlineData("test-missing-utc")]
    [InlineData("test-future")] [InlineData("completion-before-period")] [InlineData("completion-future")]
    [InlineData("unreviewed")] [InlineData("reference")] [InlineData("duplicate")]
    public async Task ActualUtcDistinctReferencesAndExplicitOutstandingReviewAreRequired(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); var original = Complete(f, rows[2]); var report = original.Completion!; var test = report.Tests.Single();
        report = fault switch
        {
            "work" => report with { WorkCompleted = " " }, "tests" => report with { Tests = [] },
            "result" => report with { Tests = [test with { ReportedResult = "ACCEPTED" }] },
            "test-offset" => report with { Tests = [test with { TestedAtUtc = test.TestedAtUtc.ToOffset(TimeSpan.FromHours(3)) }] },
            "test-missing-utc" => report with { Tests = [test with { TestedAtUtc = default }] },
            "test-future" => report with { Tests = [test with { TestedAtUtc = DateTimeOffset.UtcNow.AddHours(1) }] },
            "completion-before-period" => report with { CompletedAtUtc = new DateTimeOffset(DateTime.SpecifyKind(rows[2].PERIOD_END_UTC!.Value.AddTicks(-1), DateTimeKind.Utc)) },
            "completion-future" => report with { CompletedAtUtc = DateTimeOffset.UtcNow.AddHours(1) },
            "unreviewed" => report with { OutstandingItemsReviewed = false }, "reference" => report with { Tests = [test with { Reference = "" }] },
            _ => report with { Tests = [test, test] }
        };
        f.Command = original with { Completion = report }; var writes = f.Store.Store.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(writes, f.Store.Store.Native.Writes); Assert.Equal(3, f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Count());
    }
    [Fact]
    public async Task LaterObservedProgressMakesTheEarlierCompletionClaimHistorical()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[2]); var completion = (await f.Save()).Recorded!;
        await f.Refresh(); await f.Save(); var page = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        Assert.Null(page.History.ReportedCompletion); Assert.Equal("PROGRESS_REPORTED", page.History.ProgressState); Assert.Equal(5, page.History.LatestObservedReport!.Number);
        Assert.NotNull(page.Reports.Single(row => row.DDR_ID == completion.DDR_ID).Completion); Assert.Equal(4, page.History.ObservedPeriodCount);
    }
    [Fact]
    public async Task CorrectionMustPreserveAmendOrExplicitlyWithdrawTheExistingCompletionClaim()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[2]); var recorded = (await f.Save()).Recorded!; await f.Refresh();
        var correction = DrillingDailyReportCorrectionNativeTests.Correct(f, recorded); var writes = f.Store.Store.Native.Writes;
        f.Command = correction with { Completion = null }; await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(writes, f.Store.Store.Native.Writes);
        f.Command = correction with { Completion = null, WithdrawCompletion = true }; var withdrawn = (await f.Save()).Recorded!;
        var page = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        Assert.Null(page.History.ReportedCompletion); Assert.Equal("PROGRESS_REPORTED", page.History.ProgressState);
        Assert.True(page.Reports[^1].CompletionWithdrawn); Assert.Null(withdrawn.REPORTED_COMPLETION_JSON); Assert.Null(withdrawn.REPORTED_COMPLETION_UTC);
        Assert.NotNull(page.Reports.Single(row => row.DDR_ID == recorded.DDR_ID).Completion);
    }
    [Theory, InlineData("insert"), InlineData("corrupt"), InlineData("revoked")]
    public async Task FailedCompletionWriteRollsBackAndOriginalRecoveryNeverFabricatesACompletion(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[2]);
        var before = JsonSerializer.Serialize(f.Store.Store.Native.Rows); f.FailInsert = fault == "insert"; f.Corrupt = fault == "corrupt"; if (fault == "revoked") f.OnInsert = () => f.Revoked = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(before, JsonSerializer.Serialize(f.Store.Store.Native.Rows));
        f.FailInsert = f.Corrupt = f.Revoked = false; f.OnInsert = null; Assert.Null((await f.Save(true)).Recorded);
        Assert.Null((await f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard)).ReportedCompletion);
    }
    [Fact]
    public async Task LostCompletionAcknowledgementRecoversTheExactOriginalWithoutAnotherInsert()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[2]);
        f.Store.Store.Native.FailCommit = true; await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); f.Store.Store.Native.FailCommit = false;
        var writes = f.Store.Store.Native.Writes; var recovered = await f.Save(true); recovered.Require(f.Command); Assert.NotNull(recovered.Recorded);
        Assert.Equal(writes, f.Store.Store.Native.Writes); Assert.Equal(f.Command.Completion!.CompletedAtUtc.UtcDateTime, recovered.Recorded!.REPORTED_COMPLETION_UTC);
    }
    [Theory, InlineData("payload"), InlineData("date")]
    public async Task RehashingChangedCompletionColumnsDoesNotReplaceTheRetainedOriginalProof(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[2]); await f.Save();
        var row = f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single(row => row.REPORT_NUMBER == 4);
        if (fault == "payload") row.REPORTED_COMPLETION_JSON = JsonSerializer.Serialize(f.Command.Completion! with { WorkCompleted = "Substituted completion claim" });
        else row.REPORTED_COMPLETION_UTC = row.REPORTED_COMPLETION_UTC!.Value.AddSeconds(1);
        row.REPORT_REVISION = DrillingDailyReportInput.Digest(row);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard)); await Assert.ThrowsAnyAsync<Exception>(() => f.Save(true));
    }
    [Theory, InlineData("missing"), InlineData("width"), InlineData("date-type")]
    public async Task CompletionColumnsMustBeQualifiedThroughCanonicalBeepMetadata(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await DrillingDailyReportCorrectionNativeTests.Seed(f); f.Command = Complete(f, rows[2]);
        f.Schema = schema => { var field = schema.Fields.Single(row => row.FieldName == (fault == "date-type" ? "REPORTED_COMPLETION_UTC" : "REPORTED_COMPLETION_JSON"));
            if (fault == "missing") schema.Fields.Remove(field); else if (fault == "width") { field.Size = 1; field.Size1 = 1; } else field.Fieldtype = typeof(string).FullName; };
        var writes = f.Store.Store.Native.Writes; await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(writes, f.Store.Store.Native.Writes);
    }
    [Fact]
    public async Task OrdinaryReportsOmitAllAbsentCompletionMetadataAndKeepTheirExactOriginalJson()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); var original = JsonSerializer.Serialize(f.Command); var row = (await f.Save()).Recorded!;
        Assert.DoesNotContain("\"Completion\"", original); Assert.DoesNotContain("WithdrawCompletion", original); Assert.DoesNotContain("REPORTED_COMPLETION", JsonSerializer.Serialize(row));
        Assert.Equal(original, row.REPORT_ORIGINAL_JSON); Assert.Equal(f.Command.Revision(), DrillingDailyReportInput.FromRecord(row).Revision());
    }
}
#endif
