#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.Report;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DrillingDailyReportConflictNativeTests
{
    [Theory, InlineData(1), InlineData(2)]
    public async Task ExactPositionAndCompleteHistoryAreReadOnlyAndDefensivelyCopied(int number)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare();
        if (number == 2) { await f.Save(); await f.Refresh(); }
        var original = f.Command with { IntentId = Guid.NewGuid(), Summary = "My unrecorded observations" };
        var recorded = await f.Save(); var before = JsonSerializer.Serialize(f.Store.Store.Native.Rows);
        var proof = await f.Store.Service.ReadDailyReportConflictAsync(original, f.Guard, f.Read); proof.Require(original);
        Assert.True(proof.OriginalConfirmedAbsent); Assert.Equal(number, proof.Competing.REPORT_NUMBER);
        Assert.Equal(recorded.Recorded!.DDR_ID, proof.Competing.DDR_ID); Assert.Equal(number, proof.ReportCount);
        Assert.Equal(before, JsonSerializer.Serialize(f.Store.Store.Native.Rows)); Assert.Equal(number + 1, f.Store.Store.Native.Writes);
        proof.Competing.REPORT_PROVENANCE = "Consumer mutation";
        var again = await f.Store.Service.ReadDailyReportConflictAsync(original, f.Guard, f.Read);
        Assert.Equal(f.Command.Provenance, again.Competing.REPORT_PROVENANCE);
    }
    [Fact]
    public async Task DifferentOriginalRecorderUsesCurrentOriginalReaderWithoutWriteAuthority()
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); await f.Save();
        var source = DrillingProgramAssignedReviewTests.AsReader(f.Execution.Basis.Source, "other-recorder");
        var executionBasis = f.Execution.Basis with { Source = source }; executionBasis = executionBasis with { Revision = executionBasis.ContentRevision() };
        f.Execution = f.Execution with { Basis = executionBasis }; f.Execution = f.Execution with { Revision = f.Execution.ContentRevision() };
        var basis = f.Command.Basis with { Execution = f.Execution }; basis = basis with { Revision = basis.ContentRevision() };
        var original = f.Command with { Basis = basis, IntentId = Guid.NewGuid() };
        executionBasis = executionBasis with { RecorderRoles = [], PersonaCode = null, RecorderAuthorityRevision = null };
        executionBasis = executionBasis with { Revision = executionBasis.ContentRevision() };
        f.Execution = f.Execution with { Basis = executionBasis }; f.Execution = f.Execution with { Revision = f.Execution.ContentRevision() };
        var proof = await f.Store.Service.ReadDailyReportConflictAsync(original, f.Guard, f.Read);
        Assert.Equal("execution-recorder", proof.Competing.ROW_CREATED_BY); Assert.Equal("other-recorder", original.Basis.Scope().ActorUserId);
        Assert.Equal(2, f.Store.Store.Native.Writes);
    }
    [Theory, InlineData(false), InlineData(true)]
    public async Task NoCompetitorOrRecordedOriginalNeverBecomesClearable(bool recorded)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); if (recorded) await f.Save();
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportConflictAsync(f.Command, f.Guard, f.Read));
        Assert.Equal(recorded ? 2 : 1, f.Store.Store.Native.Writes);
    }
    [Theory, InlineData("key"), InlineData("intent"), InlineData("duplicate"), InlineData("corrupt"), InlineData("unavailable"),
        InlineData("borrowed"), InlineData("isolation"), InlineData("schema"), InlineData("revoked"), InlineData("predecessor")]
    public async Task PartialConflictingOrUnavailableEvidenceCannotProveAbsence(string fault)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); await f.Save();
        var original = f.Command with { IntentId = Guid.NewGuid() }; var native = f.Store.Store.Native;
        var row = native.Rows.OfType<DRILLING_DAILY_REPORT>().Single();
        if (fault is "key" or "intent" or "duplicate")
        {
            var copy = CompanyInventorySourceReview.Copy(row);
            if (fault == "key") { copy.DDR_ID = original.ReportId(); copy.DRILLING_OPERATION_ID = "another-operation"; }
            if (fault == "intent") { copy.DDR_ID = "other-report"; copy.DRILLING_OPERATION_ID = "another-operation"; copy.PPDM_GUID = original.IntentId.ToString("D"); }
            native.Rows.Add(copy);
        }
        if (fault == "corrupt") row.DEPTH_END_MD = 9999;
        if (fault == "unavailable") f.Start.NullRows = true;
        if (fault == "borrowed") native.Borrowed = true;
        if (fault == "isolation") native.Isolation = IsolationLevel.ReadCommitted;
        if (fault == "schema") f.Schema = schema => schema.Fields.RemoveAll(field => field.FieldName == "REPORT_ORIGINAL_JSON");
        if (fault == "revoked") f.Revoked = true;
        if (fault == "predecessor")
        {
            await f.Refresh(); original = f.Command with { IntentId = Guid.NewGuid() }; await f.Save();
            var bad = original.Basis with { PreviousReportRevision = new('C', 64) }; bad = bad with { Revision = bad.ContentRevision() };
            original = original with { Basis = bad };
        }
        var writes = native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportConflictAsync(original, f.Guard, f.Read));
        Assert.Equal(writes, native.Writes);
    }
    [Theory, InlineData("key"), InlineData("intent"), InlineData("population")]
    public async Task ProviderFailedFlagCannotBecomeAbsenceOrQualifiedHistory(string query)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); await f.Save();
        var original = f.Command with { IntentId = Guid.NewGuid() }; var native = f.Store.Store.Native;
        var error = new ErrorsInfo { Flag = Errors.Ok }; native.Source.SetupGet(s => s.ErrorObject).Returns(error);
        native.Source.Setup(s => s.GetEntityAsync("DRILLING_DAILY_REPORT", It.IsAny<List<AppFilter>>())).ReturnsAsync((string table, List<AppFilter> filters) =>
        {
            var fail = query == "key" ? filters.Any(f => f.FieldName == "DDR_ID") : query == "intent" ?
                filters.Any(f => f.FieldName == "PPDM_GUID") : filters.Any(f => f.FieldName == "DRILLING_OPERATION_ID");
            error.Flag = fail ? Errors.Failed : Errors.Ok;
            return native.Rows.Where(row => row.GetType().Name == table && DrillingDailyReportNativeTests.Matches(row, filters))
                .Select(row => JsonSerializer.Deserialize(JsonSerializer.Serialize(row), row.GetType())!).ToArray();
        });
        var failure = await Assert.ThrowsAsync<InvalidOperationException>(() => f.Store.Service.ReadDailyReportConflictAsync(original, f.Guard, f.Read));
        Assert.Equal("The Beep provider reported a failed repository operation.", failure.Message); Assert.Equal(2, native.Writes);
    }
    [Theory, InlineData("original"), InlineData("competing"), InlineData("source"), InlineData("append")]
    public async Task ClosingExecutionCallbackCannotHideChangedReportEvidence(string change)
    {
        using var f = new DrillingDailyReportNativeTests.Fixture(); await f.Prepare(); await f.Save();
        var original = f.Command with { IntentId = Guid.NewGuid() }; var reads = 0;
        await f.Refresh(); var later = f.Command.ToRecord(DateTime.UtcNow);
        Task<DrillingExecutionStatus> Read()
        {
            Assert.DoesNotContain("begin", f.Store.Store.Native.Transactions.TakeLast(1));
            if (++reads == 2)
            {
                if (change == "original") f.Store.Store.Native.Rows.Add(original.ToRecord(DateTime.UtcNow));
                if (change == "competing") f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single().REPORT_PROVENANCE = "Changed";
                if (change == "append") f.Store.Store.Native.Rows.Add(later);
                if (change == "source") f.Execution = f.Execution with { Recorded = null };
            }
            return f.Read();
        }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportConflictAsync(original, f.Guard, Read));
        Assert.Equal(2, reads); Assert.Equal(2, f.Store.Store.Native.Writes);
    }
}
#endif
