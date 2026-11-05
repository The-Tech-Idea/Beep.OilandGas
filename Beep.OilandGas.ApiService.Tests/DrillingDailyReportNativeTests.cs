#if WORKFLOW_INBOX_FOCUSED
using System.Data;
using System.Text.Json;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.Core;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;
public sealed class DrillingDailyReportNativeTests
{
    internal static bool Matches(object row, List<TheTechIdea.Beep.Report.AppFilter> filters) => filters.All(filter =>
    {
        var value = row.GetType().GetProperty(filter.FieldName)?.GetValue(row);
        if (filter.Operator == "IS NULL") return value is null;
        if (filter.Operator == "=") return value?.ToString() == filter.FilterValue;
        if (value is not int number || !int.TryParse(filter.FilterValue, out var bound)) throw new InvalidOperationException("Unsupported fixture report filter.");
        return filter.Operator switch { ">=" => number >= bound, "<=" => number <= bound, ">" => number > bound, _ => throw new InvalidOperationException("Unsupported fixture numeric operator.") };
    });
    internal sealed class Fixture : IDisposable
    {
        internal DrillingExecutionStartNativeTests.Fixture Start = new();
        internal DrillingDailyReportBasis Basis = null!;
        internal DrillingDailyReportInput Command = null!;
        internal DrillingExecutionStatus Execution = null!;
        internal Action<EntityStructure>? Schema;
        internal bool FailInsert, Corrupt, Revoked;
        internal Action? OnInsert;
        internal DrillingProgramDraftNativeTests.Fixture Store => Start.Store;
        internal async Task Prepare()
        {
            await Start.Prepare(); await Start.Save();
            Execution = await Store.Service.ReadExecutionStatusAsync(Start.Current, Start.Guard);
            var native = Store.Store.Native;
            native.Source.Setup(source => source.GetEntityStructure(It.IsAny<EntityStructure>(), true)).Returns((EntityStructure requested, bool refresh) =>
            {
                var value = new ClassCreator(native.Editor.Object).ConvertToEntityStructure(requested.EntityName == "DRILLING_DAILY_REPORT" ? typeof(DRILLING_DAILY_REPORT) : typeof(DRILLING_OPERATION));
                if (requested.EntityName == "DRILLING_DAILY_REPORT") Schema?.Invoke(value);
                return value;
            });
            native.Source.Setup(source => source.InsertEntity("DRILLING_DAILY_REPORT", It.IsAny<object>())).Returns((string table, object value) =>
            {
                native.Writes++; if (FailInsert) return new ErrorsInfo { Flag = Errors.Failed };
                var row = CompanyInventorySourceReview.Copy((DRILLING_DAILY_REPORT)value);
                if (Corrupt) row.DEPTH_END_MD = 999999;
                native.Rows.Add(row); OnInsert?.Invoke(); return new ErrorsInfo { Flag = Errors.Ok };
            });
            native.Source.Setup(source => source.GetEntityAsync("DRILLING_DAILY_REPORT", It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync((string table, List<TheTechIdea.Beep.Report.AppFilter> filters) =>
                Start.NullRows ? null! : native.Rows.Where(row => row is DRILLING_DAILY_REPORT && Matches(row, filters)).Select(row => CompanyInventorySourceReview.Copy((DRILLING_DAILY_REPORT)row)).ToArray());
            await Refresh();
        }
        internal async Task Refresh()
        {
            var history = await Store.Service.ReadDailyReportsAsync(Execution, Guard); var head = history.Head;
            Basis = new(Execution, history.Count + 1, head?.ReportId, head?.NativeRevision, head?.LatestPeriodEndUtc,
                ["ConfiguredExecutionRecorder"], "DRILLING_RECORDER", new('B', 64), "", DateTimeOffset.UtcNow);
            Basis = Basis with { Revision = Basis.ContentRevision() }; Basis.Require();
            Command = new(Basis, Guid.NewGuid(), "ConfiguredExecutionRecorder", head is null ? Start.Command.SpudAtUtc : Basis.PreviousPeriodEndUtc!.Value,
                DateTimeOffset.UtcNow, 100, 125, Execution.Basis.Source.Program.DEPTH_OUOM, null, null, null,
                "Original site log / observed measurement 471", "Observed hole drilling progress", "Continue only after site clearance");
            Command.Require();
        }
        internal Task Guard() { if (Revoked) throw new UnauthorizedAccessException("Current report authority revoked"); return Task.CompletedTask; }
        internal Task<DrillingExecutionStatus> Read() => Task.FromResult(Execution);
        internal Task<DrillingDailyReportReceipt> Save(bool recover = false) => Store.Service.RecordDailyReportAsync(Command, Guard, Read, recover);
        public void Dispose() => Start.Dispose();
    }

    [Fact] public async Task ObservedReportHasExactProvenanceUnknownHoursAndHistoryWithoutChangingStartOrFinance()
    {
        using var f = new Fixture(); await f.Prepare();
        var before = JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>());
        Assert.Null((await f.Save(true)).Recorded); var result = await f.Save(); result.Require(f.Command);
        var row = Assert.IsType<DRILLING_DAILY_REPORT>(result.Recorded);
        Assert.Equal(125, row.DEPTH_END_MD); Assert.Null(row.ROTATING_HOURS); Assert.Null(row.NPT_HOURS);
        Assert.Equal(f.Command.Provenance, row.REPORT_PROVENANCE); Assert.Equal("execution-recorder", row.ROW_CREATED_BY);
        Assert.Equal("RECORDED", row.REPORT_STATUS); Assert.NotNull((await f.Save()).Recorded);
        Assert.Equal(2, f.Store.Store.Native.Writes);
        var history = await f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard); history.Require(); Assert.Equal(1, history.Count);
        Assert.Equal(before, JsonSerializer.Serialize(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>()));
        result.Recorded!.REPORT_PROVENANCE = "consumer mutation";
        Assert.Equal(f.Command.Provenance, (await f.Save(true)).Recorded!.REPORT_PROVENANCE);
    }

    [Fact] public async Task OrderedSecondReportPreservesFirstAndLostAcknowledgementRecoversWithoutNewWrite()
    {
        using var f = new Fixture(); await f.Prepare(); var first = await f.Save(); await f.Refresh();
        Assert.Equal(2, f.Command.Basis.NextNumber); f.Store.Store.Native.FailCommit = true;
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); f.Store.Store.Native.FailCommit = false;
        Assert.NotNull((await f.Save(true)).Recorded); Assert.Equal(3, f.Store.Store.Native.Writes);
        var history = await f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard);
        Assert.Equal(2, history.Count); Assert.Equal(first.Recorded!.REPORT_REVISION, f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single(row => row.REPORT_NUMBER == 1).REPORT_REVISION); Assert.Equal(first.Recorded.DDR_ID, DrillingDailyReportInput.FromRecord(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single(row => row.REPORT_NUMBER == 2)).Basis.PreviousReportId);
    }

    [Fact] public async Task AnotherOriginalCannotOverwriteOrDuplicateAnAdvancedReportHead()
    {
        using var f = new Fixture(); await f.Prepare(); var competing = f.Command with { IntentId = Guid.NewGuid(), Summary = "Competing original observation" };
        await f.Save(); f.Command = competing; await Assert.ThrowsAnyAsync<Exception>(() => f.Save());
        Assert.Null((await f.Save(true)).Recorded); Assert.Single(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>());
        await f.Refresh(); f.Command = f.Command with { PeriodStartUtc = f.Basis.PreviousPeriodEndUtc!.Value.AddTicks(-1) };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Equal(2, f.Store.Store.Native.Writes);
    }

    [Theory] [InlineData("period-offset")] [InlineData("before-spud")] [InlineData("future")] [InlineData("depth")] [InlineData("unit")] [InlineData("hours")] [InlineData("npt-reason")] [InlineData("provenance")] [InlineData("summary")] [InlineData("role")]
    public async Task ObservedInputsCannotUseGuessedOrInvalidValues(string fault)
    {
        using var f = new Fixture(); await f.Prepare();
        f.Command = fault switch
        {
            "period-offset" => f.Command with { PeriodEndUtc = f.Command.PeriodEndUtc.ToOffset(TimeSpan.FromHours(3)) },
            "before-spud" => f.Command with { PeriodStartUtc = f.Start.Command.SpudAtUtc.AddSeconds(-1) },
            "future" => f.Command with { PeriodEndUtc = DateTimeOffset.UtcNow.AddHours(1) },
            "depth" => f.Command with { DepthEndMd = -1 }, "unit" => f.Command with { DepthUnit = "unqualified-unit" },
            "hours" => f.Command with { RotatingHours = 25 }, "npt-reason" => f.Command with { NptHours = 0.000000001m },
            "provenance" => f.Command with { Provenance = " " }, "summary" => f.Command with { Summary = "" },
            _ => f.Command with { RecorderRole = "OtherRole" }
        };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Empty(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>());
    }

    [Theory] [InlineData("insert")] [InlineData("corrupt")] [InlineData("revoked")]
    public async Task ReportFailureRollsBackItsEntireObservedRecord(string fault)
    {
        using var f = new Fixture(); await f.Prepare(); f.FailInsert = fault == "insert"; f.Corrupt = fault == "corrupt";
        f.OnInsert = () => { if (fault == "revoked") f.Revoked = true; };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Empty(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>());
        Assert.Single(f.Store.Store.Native.Rows.OfType<DRILLING_OPERATION>()); Assert.Equal("rollback", f.Store.Store.Native.Transactions.Last());
    }

    [Theory] [InlineData("key")] [InlineData("owner")] [InlineData("proof-width")] [InlineData("missing")]
    public async Task MissingReportSchemaIsUnavailableRatherThanConfirmedAbsence(string fault)
    {
        using var f = new Fixture(); await f.Prepare(); f.Schema = schema =>
        {
            if (fault == "key") schema.Fields.ForEach(field => field.IsKey = false);
            if (fault == "owner") schema.Fields.Single(field => field.FieldName == "OWNER_TENANT_ID").AllowDBNull = true;
            if (fault == "proof-width") { var field = schema.Fields.Single(row => row.FieldName == "REPORT_ORIGINAL_JSON"); field.Size = 1; field.Size1 = 1; }
            if (fault == "missing") schema.Fields.RemoveAll(field => field.FieldName == "REPORT_PROVENANCE");
        };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save(true)); await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard));
        Assert.Empty(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>());
    }

    [Theory] [InlineData("DRILLING_DAILY_REPORT")] [InlineData("alias")]
    public async Task GenericCrudCannotBypassTheObservedReportOwner(string table)
    {
        using var f = new Fixture(); await f.Prepare();
        var repository = new PPDMGenericRepository(f.Store.Store.Native.Editor.Object, Mock.Of<ICommonColumnHandler>(), Mock.Of<IPPDM39DefaultsRepository>(),
            Mock.Of<Beep.OilandGas.PPDM39.Core.Metadata.IPPDMMetadataRepository>(), typeof(DRILLING_DAILY_REPORT), f.Execution.Basis.Source.Basis.ExecutionDatabase.ConnectionName, table);
        await Assert.ThrowsAnyAsync<Exception>(() => repository.InsertAsync(f.Command.ToRecord(DateTime.UtcNow), "execution-recorder"));
        await Assert.ThrowsAnyAsync<Exception>(() => repository.UpdateAsync(f.Command.ToRecord(DateTime.UtcNow), "execution-recorder"));
        Assert.Empty(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>());
    }

    [Fact] public async Task ObservedZeroRemainsZeroAndCurrentReaderIsSeparateFromOriginalRecorder()
    {
        using var f = new Fixture(); await f.Prepare(); f.Command = f.Command with { RotatingHours = 0, NptHours = 0, DepthStartMd = 0, DepthEndMd = 0 };
        var recorded = (await f.Save()).Recorded!; Assert.Equal(0, recorded.NPT_HOURS); Assert.Equal(0, recorded.ROTATING_HOURS); Assert.Equal(0, recorded.DEPTH_END_MD);
        var source = DrillingProgramAssignedReviewTests.AsReader(f.Execution.Basis.Source, "another-report-reader");
        var basis = f.Execution.Basis with { Source = source, RecorderRoles = [], PersonaCode = null, RecorderAuthorityRevision = null };
        basis = basis with { Revision = basis.ContentRevision() };
        var execution = f.Execution with { Basis = basis }; execution = execution with { Revision = execution.ContentRevision() };
        var history = await f.Store.Service.ReadDailyReportsAsync(execution, f.Guard); history.Require();
        Assert.Equal("another-report-reader", history.Execution.Basis.Scope().ActorUserId); Assert.Equal("execution-recorder", f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single().ROW_CREATED_BY);
        Assert.Equal(2, f.Store.Store.Native.Writes);
    }

    [Theory] [InlineData("unavailable")] [InlineData("borrowed")] [InlineData("isolation")] [InlineData("no-execution")] [InlineData("duplicate")]
    public async Task UnavailableOrAmbiguousReportEvidenceCannotBecomeEmptyHistory(string fault)
    {
        using var f = new Fixture(); await f.Prepare();
        if (fault == "unavailable") f.Start.NullRows = true;
        if (fault == "borrowed") f.Store.Store.Native.Borrowed = true;
        if (fault == "isolation") f.Store.Store.Native.Isolation = IsolationLevel.ReadCommitted;
        if (fault == "no-execution") f.Store.Store.Native.Rows.Clear();
        if (fault == "duplicate") { await f.Save(); f.Store.Store.Native.Rows.Add(CompanyInventorySourceReview.Copy(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>().Single())); }
        await Assert.ThrowsAnyAsync<Exception>(() => f.Store.Service.ReadDailyReportsAsync(f.Execution, f.Guard));
    }

    [Fact] public async Task BrokenPreviousReportProofCannotBeAcknowledgedAsOrderedHistory()
    {
        using var f = new Fixture(); await f.Prepare(); await f.Save(); await f.Refresh();
        var bad = f.Command.Basis with { PreviousReportRevision = new('C', 64) }; bad = bad with { Revision = bad.ContentRevision() };
        f.Command = f.Command with { Basis = bad };
        await Assert.ThrowsAnyAsync<Exception>(() => f.Save()); Assert.Single(f.Store.Store.Native.Rows.OfType<DRILLING_DAILY_REPORT>());
        Assert.Equal(2, f.Store.Store.Native.Writes);
    }
}
#endif
