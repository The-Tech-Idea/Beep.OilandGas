using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.DrillingAndConstruction.Models;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Text.Json;
using TheTechIdea.Beep;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.DataManagement.Core.ModuleSetup;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.DrillingAndConstruction.Services;

public partial class DrillingOperationService
{
    private static void RequireDailyReportSchema(IDataSource source)
    {
        var schema = source.GetEntityStructure(new EntityStructure { EntityName = "DRILLING_DAILY_REPORT", DatasourceEntityName = "DRILLING_DAILY_REPORT" }, true);
        var keys = schema?.Fields?.Where(field => field.IsKey).ToArray();
        if (keys is not { Length: 1 } || keys[0].FieldName != "DDR_ID")
            throw new InvalidOperationException("Review the exact DDR_ID key through Beep Migration before reporting drilling progress.");
        void Field(string name, Type type, int width = 0, bool required = false)
        {
            var fields = schema!.Fields.Where(field => field.FieldName == name).ToArray();
            if (fields.Length != 1 || fields[0].Fieldtype != type.FullName || required && fields[0].AllowDBNull ||
                width > 0 && Math.Max(fields[0].Size, fields[0].Size1) < width)
                throw new InvalidOperationException("Review report column " + name + " through Beep Migration before recording progress.");
        }
        foreach (var name in new[] { "DDR_ID", "OWNER_TENANT_ID", "OWNER_COMPANY_BA_ID", "FIELD_ID", "WELL_UWI", "PROGRAM_ID", "DRILLING_OPERATION_ID", "RECORDER_ROLE" }) Field(name, typeof(string), 128, true);
        foreach (var name in new[] { "EXECUTION_START_REVISION", "RECORDER_PERSONA_CODE", "REPORT_REVISION" }) Field(name, typeof(string), 64, true);
        foreach (var name in new[] { "PERIOD_START_UTC", "PERIOD_END_UTC" }) Field(name, typeof(DateTime), required: true);
        foreach (var name in new[] { "DEPTH_START_MD", "DEPTH_END_MD" }) Field(name, typeof(decimal), required: true);
        Field("REPORT_NUMBER", typeof(int), required: true); Field("DEPTH_OUOM", typeof(string), 32, true); Field("REPORT_STATUS", typeof(string), 32, true);
        Field("REPORT_ORIGINAL_JSON", typeof(string), DrillingDailyReportInput.MaximumBodyLength, true);
        foreach (var name in new[] { "REPORT_PROVENANCE", "DAILY_OPERATIONS_SUMMARY" }) Field(name, typeof(string), 1600, true);
        foreach (var name in new[] { "NPT_REASON", "NEXT_PERIOD_PLAN" }) Field(name, typeof(string), 1600);
        foreach (var name in new[] { "ROTATING_HOURS", "NPT_HOURS" }) Field(name, typeof(decimal));
        Field("CORRECTS_REPORT_ID", typeof(string), 128); Field("CORRECTS_REPORT_REVISION", typeof(string), 64);
        Field("CORRECTION_REASON", typeof(string), 1600);
        Field("REPORTED_COMPLETION_JSON", typeof(string), DrillingCompletionReport.MaximumBodyLength);
        Field("REPORTED_COMPLETION_UTC", typeof(DateTime));
        foreach (var name in new[] { "SOURCE", "ACTIVE_IND", "PPDM_GUID", "ROW_CREATED_BY", "ROW_CHANGED_BY" }) Field(name, typeof(string));
        foreach (var name in new[] { "ROW_CREATED_DATE", "ROW_CHANGED_DATE" }) Field(name, typeof(DateTime));
    }

    private const int ReportReadBatchSize = 16;
    private sealed record DailyReportRows(DrillingDailyReportHistory History, IReadOnlyList<DrillingDailyReportObservation> Page,
        DRILLING_DAILY_REPORT? Original, DRILLING_DAILY_REPORT? Competing, DrillingDailyReportHistoryBuilder Proof);

    private async Task<DailyReportRows> ReadDailyReportRowsAsync(DrillingExecutionStatus execution, Func<Task> check,
        DrillingDailyReportInput? original = null, int after = 0, int size = 0)
    {
        execution.Require(); await check();
        if (execution.Recorded is null) throw new InvalidOperationException("A recorded original execution is required before reading drilling reports.");
        var actual = await ReadExecutionRowAsync(execution.Basis);
        if (actual is null || actual.EXECUTION_START_REVISION != execution.Recorded.EXECUTION_START_REVISION)
            throw new InvalidOperationException("The original drilling execution changed before report readback.");
        var scope = execution.Basis.Scope(); var target = execution.Basis.Source.Basis.ExecutionDatabase;
        var repository = new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata, typeof(DRILLING_DAILY_REPORT), target.ConnectionName, "DRILLING_DAILY_REPORT");
        AppFilter Eq(string field, string value) => new() { FieldName = field, FilterValue = value, Operator = "=" };
        AppFilter Number(string op, int value) => new() { FieldName = "REPORT_NUMBER", FilterValue = value.ToString(System.Globalization.CultureInfo.InvariantCulture), Operator = op, valueType = typeof(int).FullName, FieldType = typeof(int) };
        List<AppFilter> ScopeFilters() => [Eq("OWNER_TENANT_ID", scope.Company.TenantId), Eq("OWNER_COMPANY_BA_ID", scope.Company.BusinessAssociateId), Eq("DRILLING_OPERATION_ID", actual.DRILLING_OPERATION_ID)];
        async Task<object[]> Rows(List<AppFilter> filters)
        {
            await check();
            return (await repository.GetAsync(filters))?.ToArray() ?? throw new InvalidOperationException("The report population is unavailable; absence was not confirmed.");
        }
        var invalid = ScopeFilters(); invalid.Add(Number("<=", 0));
        if ((await Rows(invalid)).Length != 0) throw new InvalidOperationException("A report has an invalid sequence number.");
        var nullNumber = ScopeFilters(); nullNumber.Add(new() { FieldName = "REPORT_NUMBER", Operator = "IS NULL", FilterValue = null! });
        if ((await Rows(nullNumber)).Length != 0) throw new InvalidOperationException("A report sequence number is missing.");
        var builder = new DrillingDailyReportHistoryBuilder(execution, DateTimeOffset.UtcNow);
        var page = new List<DrillingDailyReportObservation>(); DRILLING_DAILY_REPORT? owned = null, competing = null;
        while (builder.Count < int.MaxValue)
        {
            var first = builder.Count + 1; var last = (int)Math.Min(int.MaxValue, (long)builder.Count + ReportReadBatchSize);
            var filters = ScopeFilters(); filters.Add(Number(">=", first)); filters.Add(Number("<=", last));
            var rows = await Rows(filters);
            if (rows.Length > ReportReadBatchSize || rows.Any(row => row is not DRILLING_DAILY_REPORT))
                throw new InvalidOperationException("The bounded native report range is incomplete, unsupported or ambiguous.");
            foreach (var row in rows.Cast<DRILLING_DAILY_REPORT>().OrderBy(row => row.REPORT_NUMBER))
            {
                if (row.OWNER_TENANT_ID != scope.Company.TenantId || row.OWNER_COMPANY_BA_ID != scope.Company.BusinessAssociateId ||
                    row.DRILLING_OPERATION_ID != actual.DRILLING_OPERATION_ID || row.REPORT_NUMBER < first || row.REPORT_NUMBER > last)
                    throw new UnauthorizedAccessException("A report belongs to another original company/execution/range.");
                var observation = builder.Add(row);
                if (row.REPORT_NUMBER > after && (long)row.REPORT_NUMBER <= (long)after + size) page.Add(observation);
                if (original is not null && row.DDR_ID == original.ReportId()) owned = CompanyInventorySourceReview.Copy(row);
                if (original is not null && row.REPORT_NUMBER == original.Basis.NextNumber) competing = CompanyInventorySourceReview.Copy(row);
            }
            if (rows.Length < ReportReadBatchSize) break;
        }
        // An empty/short range must not hide a gap with later records.
        var tail = ScopeFilters(); tail.Add(Number(">", builder.Count));
        if ((await Rows(tail)).Length != 0) throw new InvalidOperationException("The complete report sequence has a missing position.");
        if (original is not null)
        {
            var byId = await Rows([Eq("DDR_ID", original.ReportId())]);
            var byIntent = await Rows([Eq("OWNER_TENANT_ID", scope.Company.TenantId), Eq("OWNER_COMPANY_BA_ID", scope.Company.BusinessAssociateId), Eq("PPDM_GUID", original.IntentId.ToString("D"))]);
            var expected = owned is null ? Array.Empty<DRILLING_DAILY_REPORT>() : new[] { owned };
            if (byId.Length > 1 || byIntent.Length > 1 || byId.Any(row => row is not DRILLING_DAILY_REPORT) || byIntent.Any(row => row is not DRILLING_DAILY_REPORT) ||
                JsonSerializer.Serialize(byId) != JsonSerializer.Serialize(byIntent) || JsonSerializer.Serialize(byId) != JsonSerializer.Serialize(expected))
                throw new InvalidOperationException("The original report key/intent/execution is conflicting or ambiguous.");
            if (owned is not null && DrillingDailyReportInput.FromRecord(owned).Revision() != original.Revision())
                throw new InvalidOperationException("The report differs from the retained original intent.");
        }
        await check(); return new(builder.Build(), page.Select(builder.QualifyDisplay).ToArray(), owned, competing, builder);
    }

    private Func<bool, Task> DailyReportGuard(IDataSource source, DrillingExecutionStatus execution, Func<Task> authority)
    {
        var target = execution.Basis.Source.Basis.ExecutionDatabase;
        return async owned =>
        {
            if (!ReferenceEquals(source, _editor.GetDataSource(target.ConnectionName))) throw new InvalidOperationException("The original execution source changed.");
            MigrationConnectionTarget.Validate(_editor, source, target.ConnectionName);
            void Transaction() { if (owned) PPDMDataSourceTransaction.RequireOwned(source, IsolationLevel.Serializable); else PPDMDataSourceTransaction.RequireIdle(source); }
            Transaction(); await authority(); Transaction();
        };
    }

    public async Task<DrillingDailyReportHistory> ReadDailyReportsAsync(DrillingExecutionStatus execution, Func<Task> authority)
    {
        execution.Require(); var source = _editor.GetDataSource(execution.Basis.Source.Basis.ExecutionDatabase.ConnectionName) ?? throw new InvalidOperationException("The original execution source is unavailable.");
        var guard = DailyReportGuard(source, execution, authority); DrillingDailyReportHistory? found = null;
        await guard(false);
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await guard(true); RequireExecutionSchema(source); RequireDailyReportSchema(source);
            found = (await ReadDailyReportRowsAsync(execution, () => guard(true))).History; await guard(true);
            if (found.Revision != (await ReadDailyReportRowsAsync(execution, () => guard(true))).History.Revision) throw new InvalidOperationException("The report history changed during readback.");
            await guard(true);
        });
        await guard(false); RequireExecutionSchema(source); RequireDailyReportSchema(source);
        if (found!.Revision != (await ReadDailyReportRowsAsync(execution, () => guard(false))).History.Revision) throw new InvalidOperationException("The report history changed before its outcome was confirmed.");
        await guard(false); return found;
    }

    public async Task<DrillingDailyReportReceipt> RecordDailyReportAsync(DrillingDailyReportInput input, Func<Task> authority,
        Func<Task<DrillingExecutionStatus>> readExecution, bool recover = false)
    {
        input.Require(); var original = CompanyInventorySourceReview.Copy(input); var basis = original.Basis;
        var source = _editor.GetDataSource(basis.Execution.Basis.Source.Basis.ExecutionDatabase.ConnectionName) ?? throw new InvalidOperationException("The original execution source is unavailable.");
        var guard = DailyReportGuard(source, basis.Execution, authority);
        async Task CheckExecution() { await guard(false); basis.RequireExecution(await readExecution()); await guard(false); }
        await CheckExecution(); DRILLING_DAILY_REPORT? recorded = null;
        await PPDMDataSourceTransaction.ExecuteAsync(source, async () =>
        {
            await guard(true); RequireExecutionSchema(source); RequireDailyReportSchema(source);
            var scan = await ReadDailyReportRowsAsync(basis.Execution, () => guard(true), original); var history = scan.History; recorded = scan.Original; await guard(true);
            if (recorded is null && !recover)
            {
                scan.Proof.RequireAppend(original);
                var head = history.Head;
                if ((long)basis.NextNumber != (long)history.Count + 1 || basis.PreviousReportId != head?.ReportId || basis.PreviousReportRevision != head?.NativeRevision)
                    throw new InvalidOperationException("Another report advanced the execution. Recover the original before reviewing its current history.");
                var row = original.ToRecord(DateTime.UtcNow); _commonColumnHandler.PrepareForInsert(row, basis.Scope().ActorUserId);
                row.REPORT_REVISION = DrillingDailyReportInput.Digest(row); DrillingDailyReportInput.FromRecord(row); await guard(true);
                await PPDMDataSourceTransaction.WithSourceAsync(source, () =>
                {
                    BeepOperationResult.EnsureSuccess(source.InsertEntity("DRILLING_DAILY_REPORT", row), "Insert", "DRILLING_DAILY_REPORT"); return Task.FromResult(true);
                });
                await guard(true); recorded = (await ReadDailyReportRowsAsync(basis.Execution, () => guard(true), original)).Original;
                if (recorded is null) throw new InvalidOperationException("The original report was not confirmed recorded.");
            }
            await guard(true);
        });
        await guard(false); RequireExecutionSchema(source); RequireDailyReportSchema(source);
        var closing = (await ReadDailyReportRowsAsync(basis.Execution, () => guard(false), original)).Original;
        if (JsonSerializer.Serialize(recorded) != JsonSerializer.Serialize(closing)) throw new InvalidOperationException("The original report commit is unconfirmed. Recover its retained original.");
        await CheckExecution(); var result = new DrillingDailyReportReceipt(original.Revision(), original.ReportId(), closing, DateTimeOffset.UtcNow); result.Require(original); return result;
    }
}
