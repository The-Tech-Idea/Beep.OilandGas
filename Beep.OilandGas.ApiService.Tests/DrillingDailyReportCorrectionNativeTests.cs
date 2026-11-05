using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingDailyReportCorrectionNativeTests
{
    internal static async Task<List<DRILLING_DAILY_REPORT>> Seed(DrillingDailyReportNativeTests.Fixture f)
    {
        await f.Prepare(); var rows = DrillingDailyReportPageNativeTests.Series(f.Command, 3);
        f.Store.Store.Native.Rows.AddRange(rows); await f.Refresh(); return rows;
    }
    internal static DrillingDailyReportInput Correct(DrillingDailyReportNativeTests.Fixture f, DRILLING_DAILY_REPORT target) =>
        DrillingDailyReportInput.FromRecord(target) with
        {
            Basis = f.Basis, IntentId = Guid.NewGuid(), Summary = "Corrected site observation supported by the signed original log",
            Correction = new(target.DDR_ID, target.REPORT_REVISION, target.REPORT_NUMBER, "The site log clarified the observed operation")
        };
    [Fact]
    public async Task UnamendedObservationKeepsItsExactOriginalAndDigestWithoutEmptyCorrectionMetadata()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare();
        var json = JsonSerializer.Serialize(f.Command); Assert.DoesNotContain("Correction", json);
        var row = f.Command.ToRecord(DateTime.UtcNow); var serialized = JsonSerializer.Serialize(row);
        Assert.DoesNotContain("CORRECTS_REPORT", serialized); Assert.DoesNotContain("CORRECTION_REASON", serialized);
        Assert.Equal(json, row.REPORT_ORIGINAL_JSON); Assert.Equal(f.Command.Revision(), DrillingDailyReportInput.FromRecord(row).Revision());
        var saved = (await f.Save()).Recorded!; Assert.Equal(row.REPORT_ORIGINAL_JSON, saved.REPORT_ORIGINAL_JSON);
        var schema = new TheTechIdea.Beep.Tools.ClassCreator(f.Store.Store.Native.Editor.Object).ConvertToEntityStructure(typeof(DRILLING_DAILY_REPORT));
        Assert.Contains(schema.Fields, x => x.FieldName == "CORRECTS_REPORT_ID"); Assert.Contains(schema.Fields, x => x.FieldName == "CORRECTION_REASON");
    }
    [Fact]
    public async Task EarlierPeriodCorrectionAppendsWithoutReplacingOriginalsOrMovingLatestPeriodBackwards()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f);
        var originals = JsonSerializer.Serialize(rows); var execution = JsonSerializer.Serialize(f.Execution.Recorded);
        f.Command = Correct(f, rows[0]); var saved = await f.Save(); saved.Require(f.Command);
        Assert.Equal(originals, JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Where(r => r.REPORT_NUMBER <= 3)));
        var page = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        Assert.Equal(4, page.History.Count); Assert.Equal(3, page.History.ObservedPeriodCount);
        Assert.Equal(rows[2].PERIOD_END_UTC, page.History.Head!.LatestPeriodEndUtc.UtcDateTime);
        Assert.Equal(saved.ReportId, page.Reports[0].SupersededByReportId); Assert.Null(page.Reports[3].SupersededByReportId);
        Assert.Equal(rows[0].DDR_ID, page.Reports[3].Correction!.ReportId); Assert.Equal(f.Command.Correction!.Reason, page.Reports[3].Correction!.Reason);
        Assert.DoesNotContain("REPORT_ORIGINAL_JSON", JsonSerializer.Serialize(page));
        await f.Refresh(); Assert.Equal(5, f.Basis.NextNumber); Assert.Equal(rows[2].PERIOD_END_UTC, f.Command.PeriodStartUtc.UtcDateTime);
        await f.Save(); Assert.Equal(execution, JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>().Single()));
    }
    [Fact]
    public async Task LatestPeriodAndItsCorrectionCanBeCorrectedAgainUsingTheExactCurrentTarget()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f);
        f.Command = Correct(f, rows[2]) with { PeriodEndUtc = new DateTimeOffset(DateTime.SpecifyKind(rows[2].PERIOD_END_UTC!.Value.AddTicks(-1), DateTimeKind.Utc)) };
        var first = (await f.Save()).Recorded!; await f.Refresh();
        Assert.Equal(first.PERIOD_END_UTC, f.Basis.PreviousPeriodEndUtc!.Value.UtcDateTime);
        f.Command = Correct(f, first) with { Summary = "Second correction reconciled against the measurement reference" };
        var second = (await f.Save()).Recorded!;
        var page = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        Assert.Equal(5, page.History.Count); Assert.Equal(3, page.History.ObservedPeriodCount);
        Assert.Equal(first.DDR_ID, page.Reports[2].SupersededByReportId); Assert.Equal(second.DDR_ID, page.Reports[3].SupersededByReportId);
        Assert.Null(page.Reports[4].SupersededByReportId); Assert.Equal(first.REPORT_REVISION, second.CORRECTS_REPORT_REVISION);
    }
    [Theory]
    [InlineData("reason")] [InlineData("no-change")] [InlineData("missing")] [InlineData("revision")]
    [InlineData("number")] [InlineData("superseded")] [InlineData("overlap-before")] [InlineData("overlap-after")]
    public async Task InvalidOrStaleCorrectionNeverWritesOrMutatesTheQualifiedHistory(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f);
        if (fault == "superseded") { f.Command = Correct(f, rows[1]); await f.Save(); await f.Refresh(); }
        var command = Correct(f, rows[1]); var correction = command.Correction!;
        f.Command = fault switch
        {
            "reason" => command with { Correction = correction with { Reason = " " } },
            "no-change" => command with { Summary = DrillingDailyReportInput.FromRecord(rows[1]).Summary },
            "missing" => command with { Correction = correction with { ReportId = "missing-original-report" } },
            "revision" => command with { Correction = correction with { NativeRevision = new('F', 64) } },
            "number" => command with { Correction = correction with { Number = 1 } },
            "overlap-before" => command with { PeriodStartUtc = command.PeriodStartUtc.AddTicks(-1) },
            "overlap-after" => command with { PeriodEndUtc = command.PeriodEndUtc.AddTicks(1) },
            _ => command
        };
        var before = JsonSerializer.Serialize(f.Store.Store.Native.Rows); var writes = f.Store.Store.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(writes, f.Store.Store.Native.Writes);
        Assert.Equal(before, JsonSerializer.Serialize(f.Store.Store.Native.Rows));
    }
    [Theory, InlineData("insert"), InlineData("corrupt"), InlineData("revoked")]
    public async Task CorrectionWriteFailureRollsBackAndReadOnlyRecoveryConfirmsAbsence(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f); f.Command = Correct(f, rows[1]);
        var before = JsonSerializer.Serialize(f.Store.Store.Native.Rows); var writes = f.Store.Store.Native.Writes;
        f.FailInsert = fault == "insert"; f.Corrupt = fault == "corrupt"; if (fault == "revoked") f.OnInsert = () => f.Revoked = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(before, JsonSerializer.Serialize(f.Store.Store.Native.Rows));
        f.FailInsert = f.Corrupt = f.Revoked = false; f.OnInsert = null;
        Assert.Null((await f.Save(true)).Recorded); Assert.Equal(writes + 1, f.Store.Store.Native.Writes);
    }
    [Fact]
    public async Task LostCorrectionCommitIsRecoveredExactlyWithoutAnotherWriteOrReplacingItsTarget()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f); f.Command = Correct(f, rows[1]);
        f.Store.Store.Native.FailCommit = true; await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); f.Store.Store.Native.FailCommit = false;
        var writes = f.Store.Store.Native.Writes; var receipt = await f.Save(true); receipt.Require(f.Command);
        Assert.NotNull(receipt.Recorded); Assert.Equal(writes, f.Store.Store.Native.Writes);
        Assert.Equal(rows[1].REPORT_REVISION, f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single(r => r.DDR_ID == rows[1].DDR_ID).REPORT_REVISION);
    }
    [Fact]
    public async Task CorrectionFrozenBeforeAnotherNormalReportCannotAdvanceTheHistoryTwice()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f);
        var frozen = Correct(f, rows[1]); await f.Save(); f.Command = frozen;
        var before = JsonSerializer.Serialize(f.Store.Store.Native.Rows); var writes = f.Store.Store.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(writes, f.Store.Store.Native.Writes);
        Assert.Equal(before, JsonSerializer.Serialize(f.Store.Store.Native.Rows)); Assert.Null((await f.Save(true)).Recorded);
    }
    [Theory, InlineData("reason"), InlineData("target"), InlineData("target-revision")]
    public async Task ChangedCorrectionColumnsCannotBeQualifiedByRehashingTheNativeRow(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f); f.Command = Correct(f, rows[1]); await f.Save();
        var native = f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single(r => r.REPORT_NUMBER == 4);
        if (fault == "reason") native.CORRECTION_REASON = "Substituted reason";
        if (fault == "target") native.CORRECTS_REPORT_ID = rows[0].DDR_ID;
        if (fault == "target-revision") native.CORRECTS_REPORT_REVISION = new('F', 64);
        native.REPORT_REVISION = DrillingDailyReportInput.Digest(native);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard));
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save(true));
    }
    [Theory, InlineData("missing"), InlineData("width"), InlineData("reason-width")]
    public async Task CorrectionMetadataMustBePresentBeforeReadingOrWriting(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); var rows = await Seed(f); f.Command = Correct(f, rows[0]);
        f.Schema = schema => { var field = schema.Fields.Single(x => x.FieldName == (fault == "reason-width" ? "CORRECTION_REASON" : "CORRECTS_REPORT_ID")); if (fault == "missing") schema.Fields.Remove(field); else { field.Size = 1; field.Size1 = 1; } };
        var writes = f.Store.Store.Native.Writes; await Assert.ThrowsAnyAsync<Exception>(() => f.Save());
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard)); Assert.Equal(writes, f.Store.Store.Native.Writes);
    }
}
#endif
