#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingDailyReportPageNativeTests
{
    internal static List<DRILLING_DAILY_REPORT> Series(DrillingDailyReportInput seed, int count)
    {
        var rows = new List<DRILLING_DAILY_REPORT>(); var first = seed.Basis.NextNumber;
        DrillingDailyReportHead? head = seed.Basis.PreviousReportId is null ? null : new(seed.Basis.PreviousReportId, first - 1, seed.Basis.PreviousReportRevision!, seed.Basis.PreviousPeriodEndUtc!.Value);
        var span = seed.PeriodEndUtc - seed.PeriodStartUtc;
        for (var index = 0; index < count; index++)
        {
            var basis = seed.Basis with { NextNumber = first + index, CheckedAtUtc = DateTimeOffset.UtcNow, PreviousReportId = head?.ReportId,
                PreviousReportRevision = head?.NativeRevision, PreviousPeriodEndUtc = head?.LatestPeriodEndUtc };
            basis = basis with { Revision = basis.ContentRevision() };
            var input = seed with { Basis = basis, IntentId = Guid.NewGuid(),
                PeriodStartUtc = seed.PeriodStartUtc.AddTicks(span.Ticks * index / count),
                PeriodEndUtc = seed.PeriodStartUtc.AddTicks(span.Ticks * (index + 1) / count),
                Provenance = "Qualified observed site log " + (index + 1) };
            var row = input.ToRecord(DateTime.UtcNow); rows.Add(row);
            head = new(row.DDR_ID, row.REPORT_NUMBER, row.REPORT_REVISION, input.PeriodEndUtc);
        }
        return rows;
    }
    internal static DrillingDailyReportPage Page(DrillingExecutionStatus execution, IReadOnlyList<DRILLING_DAILY_REPORT> rows, int after = 0, int size = 25)
    {
        var builder = new DrillingDailyReportHistoryBuilder(execution, DateTimeOffset.UtcNow); var observations = new List<DrillingDailyReportObservation>();
        foreach (var row in rows) { var observed = builder.Add(row); if (row.REPORT_NUMBER > after && row.REPORT_NUMBER <= (long)after + size) observations.Add(observed); }
        var result = new DrillingDailyReportPage(builder.Build(), after, size, observations.Select(builder.QualifyDisplay).ToArray(), "");
        result = result with { Revision = result.ContentRevision() }; result.Require(); return result;
    }
    [Fact]
    public async Task HistoryBeyond512UsesBoundedNativeRangesAndCompactExactPagesWithoutWrites()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare();
        var rows = Series(f.Command, 537); f.Store.Store.Native.Rows.AddRange(rows);
        var largest = 0; var reads = 0;
        f.Store.Store.Native.Source.Setup(s => s.GetEntityAsync("DRILLING_DAILY_REPORT", It.IsAny<List<AppFilter>>())).ReturnsAsync((string table, List<AppFilter> filters) =>
        {
            Assert.Contains(filters, x => x.FieldName == "OWNER_TENANT_ID"); Assert.Contains(filters, x => x.FieldName == "OWNER_COMPANY_BA_ID");
            Assert.Contains(filters, x => x.FieldName == "DRILLING_OPERATION_ID"); Assert.Contains(filters, x => x.FieldName == "REPORT_NUMBER");
            var selected = rows.Where(row => DrillingDailyReportNativeTests.Matches(row, filters)).Select(row => CompanyInventorySourceReview.Copy(row)).ToArray();
            largest = Math.Max(largest, selected.Length); reads++; return selected;
        });
        var first = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        Assert.Equal(537, first.History.Count); Assert.Equal(537, first.History.Head!.Number); Assert.Equal(25, first.Reports.Count);
        Assert.Equal(1, first.Reports[0].REPORT_NUMBER); Assert.Equal(25, first.Reports[^1].REPORT_NUMBER); Assert.True(first.HasNext);
        var last = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 525, 25, first.History.Revision, f.Guard, () => Task.CompletedTask);
        Assert.Equal(12, last.Reports.Count); Assert.Equal(526, last.Reports[0].REPORT_NUMBER); Assert.Equal(537, last.Reports[^1].REPORT_NUMBER);
        Assert.False(last.HasNext); Assert.True(last.HasPrevious); Assert.Equal(first.History.Revision, last.History.Revision);
        Assert.InRange(largest, 1, 16); Assert.True(reads > 64); Assert.Equal(1, f.Store.Store.Native.Writes);
        Assert.DoesNotContain("REPORT_ORIGINAL_JSON", JsonSerializer.Serialize(first));
        Assert.True(JsonSerializer.Serialize(first).Length < JsonSerializer.Serialize(rows).Length / 5);
    }
    [Theory, InlineData("gap"), InlineData("distant-gap"), InlineData("duplicate-key"), InlineData("corrupt-outside-page"), InlineData("zero"), InlineData("execution-proof"), InlineData("basis-proof"), InlineData("measurement-proof")]
    public async Task HiddenOrCorruptRowsOutsideVisiblePageCannotBecomeAQualifiedHistory(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); var rows = Series(f.Command, 29);
        if (fault == "gap") rows.RemoveAt(16);
        if (fault == "distant-gap") rows.RemoveRange(1, 27);
        if (fault == "zero") rows[^1].REPORT_NUMBER = 0;
        if (fault == "corrupt-outside-page") rows[^1].REPORT_PROVENANCE = "Changed outside requested page";
        if (fault == "duplicate-key")
        {
            var original = DrillingDailyReportInput.FromRecord(rows[^1]) with { IntentId = DrillingDailyReportInput.FromRecord(rows[0]).IntentId };
            rows[^1] = original.ToRecord(DateTime.UtcNow);
        }
        if (fault is "execution-proof" or "basis-proof" or "measurement-proof")
        {
            var input = DrillingDailyReportInput.FromRecord(rows[^1]);
            if (fault == "execution-proof") input = input with { Basis = input.Basis with { Execution = input.Basis.Execution with { CheckedAtUtc = input.Basis.Execution.CheckedAtUtc.ToOffset(TimeSpan.FromHours(3)) } } };
            if (fault == "basis-proof") input = input with { Basis = input.Basis with { CheckedAtUtc = input.Basis.CheckedAtUtc.ToOffset(TimeSpan.FromHours(3)) } };
            if (fault == "measurement-proof") { input = input with { Summary = "" }; rows[^1].DAILY_OPERATIONS_SUMMARY = ""; }
            rows[^1].REPORT_ORIGINAL_JSON = JsonSerializer.Serialize(input); rows[^1].REPORT_REVISION = DrillingDailyReportInput.Digest(rows[^1]);
        }
        f.Store.Store.Native.Rows.AddRange(rows);
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 1, null, f.Guard, () => Task.CompletedTask));
        Assert.Equal(1, f.Store.Store.Native.Writes);
    }
    [Theory, InlineData(false), InlineData(true)]
    public async Task AppendingOutsideVisiblePageInvalidatesNavigationOrClosingSourceCheck(bool closing)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); var rows = Series(f.Command, 29);
        f.Store.Store.Native.Rows.AddRange(rows.Take(28));
        var first = await f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask);
        if (!closing) f.Store.Store.Native.Rows.Add(rows[^1]);
        Task Confirm() { Assert.DoesNotContain("begin", f.Store.Store.Native.Transactions.TakeLast(1)); if (closing) f.Store.Store.Native.Rows.Add(rows[^1]); return Task.CompletedTask; }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportPageAsync(f.Execution, closing ? 0 : 25, 25, first.History.Revision, f.Guard, Confirm));
        Assert.Equal(1, f.Store.Store.Native.Writes);
    }
    [Theory, InlineData(-1, 25, null), InlineData(1, 25, null), InlineData(0, 0, null), InlineData(0, 51, null), InlineData(0, 25, "unconfirmed")]
    public async Task InvalidOrUnboundPageRequestsRefuseBeforeOpeningNativeTransaction(int after, int size, string? expected)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); var before = f.Store.Store.Native.Transactions.Count;
        await Assert.ThrowsAsync<InvalidOperationException>(() => f.Store.Service.ReadDailyReportPageAsync(f.Execution, after, size, expected, f.Guard, () => Task.CompletedTask));
        Assert.Equal(before, f.Store.Store.Native.Transactions.Count);
    }
    [Theory, InlineData("provider"), InlineData("null-number"), InlineData("revoked")]
    public async Task FailureOrRevocationDuringBoundedReadNeverBecomesEmptyHistory(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); var rows = Series(f.Command, 29); f.Store.Store.Native.Rows.AddRange(rows);
        var error = new ErrorsInfo { Flag = Errors.Ok }; f.Store.Store.Native.Source.SetupGet(s => s.ErrorObject).Returns(error);
        f.Store.Store.Native.Source.Setup(s => s.GetEntityAsync("DRILLING_DAILY_REPORT", It.IsAny<List<AppFilter>>())).ReturnsAsync((string table, List<AppFilter> filters) =>
        {
            if (fault == "null-number" && filters.Any(x => x.Operator == "IS NULL")) return new object[] { rows[0] };
            if (filters.Any(x => x.Operator == ">=")) { if (fault == "provider") error.Flag = Errors.Failed; if (fault == "revoked") f.Revoked = true; }
            return rows.Where(row => DrillingDailyReportNativeTests.Matches(row, filters)).ToArray();
        });
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportPageAsync(f.Execution, 0, 25, null, f.Guard, () => Task.CompletedTask));
        Assert.Equal(1, f.Store.Store.Native.Writes);
    }
}
#endif
