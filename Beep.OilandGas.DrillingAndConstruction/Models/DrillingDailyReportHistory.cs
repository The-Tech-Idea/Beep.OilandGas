using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

public sealed record DrillingDailyReportHead(string ReportId, int Number, string NativeRevision, DateTimeOffset LatestPeriodEndUtc);

/// <summary>Complete validated history identity and head, without transporting every retained original.</summary>
public sealed record DrillingDailyReportHistory(
    [property: JsonRequired] DrillingExecutionStatus Execution,
    [property: JsonRequired] int Count,
    [property: JsonRequired] int ObservedPeriodCount,
    [property: JsonRequired] DrillingDailyReportHead? Head,
    [property: JsonRequired] DrillingDailyReportHead? LatestObservedReport,
    [property: JsonRequired] DrillingReportedCompletion? ReportedCompletion,
    [property: JsonRequired] string ChainRevision,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore] public string ProgressState => ReportedCompletion is not null ? "COMPLETION_REPORTED" : Count > 0 ? "PROGRESS_REPORTED" : "STARTED";
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Execution = Execution.Revision, Count, ObservedPeriodCount, Head, LatestObservedReport, ReportedCompletion, ChainRevision }));
    public void Require()
    {
        Execution.Require();
        if (Execution.Recorded is null || Count < 0 || ObservedPeriodCount < 0 || ObservedPeriodCount > Count || (Count == 0) != (ObservedPeriodCount == 0) || !ProcessDefinitionRevision.IsValid(ChainRevision) ||
            (Count == 0 ? Head is not null : Head is null || Head.Number != Count || !LeaseSummaryValidator.Identifier(Head.ReportId) ||
                !ProcessDefinitionRevision.IsValid(Head.NativeRevision) || Head.LatestPeriodEndUtc == default || Head.LatestPeriodEndUtc.Offset != TimeSpan.Zero || Head.LatestPeriodEndUtc > CheckedAtUtc) ||
            CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero || Revision != ContentRevision())
            throw new InvalidOperationException("The complete observed-report history and head are unconfirmed.");
        if (Count == 0 ? LatestObservedReport is not null || ReportedCompletion is not null :
            LatestObservedReport is null || !LeaseSummaryValidator.Identifier(LatestObservedReport.ReportId) ||
            !ProcessDefinitionRevision.IsValid(LatestObservedReport.NativeRevision) || LatestObservedReport.Number < 1 ||
            LatestObservedReport.Number > Count || LatestObservedReport.LatestPeriodEndUtc != Head!.LatestPeriodEndUtc)
            throw new InvalidOperationException("The exact current latest observed period and reported completion state are unconfirmed.");
        ReportedCompletion?.Require(this);
    }
}

/// <summary>Shared streaming proof validator used by the native owner; it retains identities, never all original bodies.</summary>
public sealed class DrillingDailyReportHistoryBuilder
{
    private sealed record Entry(int PeriodIndex, int Number, string NativeRevision);
    private sealed record Period(string CurrentId, DateTimeOffset Start, DateTimeOffset End, string ObservationRevision, string? CompletionRevision);
    private readonly DrillingExecutionStatus execution;
    private readonly DateTimeOffset checkedAt;
    private readonly Dictionary<string, Entry> entries = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> superseded = new(StringComparer.Ordinal);
    private readonly List<Period> periods = [];
    private readonly HashSet<string> sources = new(StringComparer.Ordinal);
    private readonly DrillingDailyReportRecordValidator validator = new();
    private DrillingDailyReportHead? head;
    private DrillingReportedCompletion? reportedCompletion;
    private static string? CompletionRevision(DrillingCompletionReport? report) => report is null ? null : ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(report));
    private string chain = ProcessDefinitionRevision.FromJson("drilling-report-history-v2");
    public int Count { get; private set; }
    public DrillingDailyReportHistoryBuilder(DrillingExecutionStatus execution, DateTimeOffset checkedAt)
    {
        execution.Require();
        if (execution.Recorded is null || checkedAt == default || checkedAt.Offset != TimeSpan.Zero)
            throw new InvalidOperationException("A recorded execution and actual UTC check are required.");
        this.execution = CompanyInventorySourceReview.Copy(execution); this.checkedAt = checkedAt;
    }
    private int CheckNext(DrillingDailyReportInput original)
    {
        if (Count == int.MaxValue || original.Basis.NextNumber != Count + 1 || entries.ContainsKey(original.ReportId()) ||
            original.Basis.PreviousReportId != head?.ReportId || original.Basis.PreviousReportRevision != head?.NativeRevision ||
            original.Basis.PreviousPeriodEndUtc != head?.LatestPeriodEndUtc)
            throw new InvalidOperationException("The complete report sequence, predecessor or latest observed period changed.");
        if (original.Correction is not {} correction) return periods.Count;
        if (!entries.TryGetValue(correction.ReportId, out var target) || target.Number != correction.Number ||
            target.NativeRevision != correction.NativeRevision || periods[target.PeriodIndex].CurrentId != correction.ReportId)
            throw new InvalidOperationException("The correction target is missing, changed, belongs to another execution or has already been superseded.");
        var index = target.PeriodIndex;
        if (original.Completion is not null && index != periods.Count - 1 && CompletionRevision(original.Completion) != periods[index].CompletionRevision)
            throw new InvalidOperationException("Completion must reference the current latest observed period, not an earlier reporting period.");
        if (periods[index].CompletionRevision is not null && original.Completion is null && !original.WithdrawCompletion ||
            periods[index].CompletionRevision is null && original.WithdrawCompletion)
            throw new InvalidOperationException("Preserve or amend the current completion evidence, or explicitly withdraw an existing completion report with its reason.");
        if (index > 0 && original.PeriodStartUtc < periods[index - 1].End ||
            index + 1 < periods.Count && original.PeriodEndUtc > periods[index + 1].Start ||
            original.ObservationRevision() == periods[index].ObservationRevision)
            throw new InvalidOperationException("A correction must change observed values and fit between the current preceding and following reporting periods.");
        return index;
    }
    /// <summary>Validate a new append before any write, against the complete current qualified population.</summary>
    public void RequireAppend(DrillingDailyReportInput original)
    {
        original.Require(); original.Basis.RequireExecution(execution, false); CheckNext(original);
    }
    public DrillingDailyReportObservation Add(DRILLING_DAILY_REPORT row)
    {
        var original = validator.Read(row);
        var key = DrillingDailyReportRecordValidator.ExecutionKey(original.Basis.Execution);
        if (!sources.Contains(key)) { original.Basis.RequireExecution(execution, false); sources.Add(key); }
        if (row.ROW_CHANGED_DATE > checkedAt.UtcDateTime)
            throw new InvalidOperationException("The observed report audit changed after this history check.");
        var index = CheckNext(original);
        var completion = original.Completion is {} reported ? new DrillingReportedCompletion(row.DDR_ID, row.REPORT_NUMBER, row.REPORT_REVISION,
            reported, row.ROW_CREATED_BY, row.RECORDER_ROLE, row.RECORDER_PERSONA_CODE,
            new(DateTime.SpecifyKind(row.ROW_CREATED_DATE!.Value, DateTimeKind.Utc))) : null;
        var period = new Period(row.DDR_ID, original.PeriodStartUtc, original.PeriodEndUtc, original.ObservationRevision(), CompletionRevision(original.Completion));
        if (original.Correction is {} correction) { periods[index] = period; superseded.Add(correction.ReportId, row.DDR_ID); }
        else periods.Add(period);
        if (index == periods.Count - 1) reportedCompletion = completion;
        entries.Add(row.DDR_ID, new(index, row.REPORT_NUMBER, row.REPORT_REVISION));
        chain = ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Previous = chain, Number = row.REPORT_NUMBER, NativeRevision = row.REPORT_REVISION }));
        head = new(row.DDR_ID, row.REPORT_NUMBER, row.REPORT_REVISION, periods[^1].End); Count++;
        return DrillingDailyReportObservation.FromValidatedRecord(row, original);
    }
    public DrillingDailyReportObservation QualifyDisplay(DrillingDailyReportObservation row) => row with
    {
        SupersededByReportId = superseded.GetValueOrDefault(row.DDR_ID)
    };
    public DrillingDailyReportHistory Build()
    {
        var latest = periods.Count == 0 ? null : periods[^1];
        var identity = latest is null ? null : entries[latest.CurrentId];
        var observed = latest is null ? null : new DrillingDailyReportHead(latest.CurrentId, identity!.Number, identity.NativeRevision, latest.End);
        var result = new DrillingDailyReportHistory(CompanyInventorySourceReview.Copy(execution), Count, periods.Count, head, observed, reportedCompletion, chain, "", checkedAt);
        result = result with { Revision = result.ContentRevision() }; result.Require(); return result;
    }
}

/// <summary>Qualified display projection. Original command/source JSON stays with its canonical native record.</summary>
public sealed record DrillingDailyReportObservation(
    string DDR_ID, int REPORT_NUMBER, string DRILLING_OPERATION_ID,
    string OWNER_TENANT_ID, string OWNER_COMPANY_BA_ID, string FIELD_ID, string WELL_UWI,
    DateTime PERIOD_START_UTC, DateTime PERIOD_END_UTC, decimal DEPTH_START_MD, decimal DEPTH_END_MD, string DEPTH_OUOM,
    decimal? ROTATING_HOURS, decimal? NPT_HOURS, string? NPT_REASON,
    string REPORT_PROVENANCE, string DAILY_OPERATIONS_SUMMARY, string? NEXT_PERIOD_PLAN,
    string ROW_CREATED_BY, string RECORDER_ROLE, string RECORDER_PERSONA_CODE,
    DateTime ROW_CREATED_DATE, DateTime ROW_CHANGED_DATE, string REPORT_REVISION,
    DrillingDailyReportCorrection? Correction, DrillingCompletionReport? Completion, bool CompletionWithdrawn, string? SupersededByReportId)
{
    public bool MatchesObservations(DrillingDailyReportInput input) => PERIOD_START_UTC == input.PeriodStartUtc.UtcDateTime &&
        PERIOD_END_UTC == input.PeriodEndUtc.UtcDateTime && DEPTH_START_MD == input.DepthStartMd && DEPTH_END_MD == input.DepthEndMd &&
        DEPTH_OUOM == input.DepthUnit && ROTATING_HOURS == input.RotatingHours && NPT_HOURS == input.NptHours &&
        NPT_REASON == input.NptReason && REPORT_PROVENANCE == input.Provenance && DAILY_OPERATIONS_SUMMARY == input.Summary && NEXT_PERIOD_PLAN == input.NextPlan &&
        JsonSerializer.Serialize(Completion) == JsonSerializer.Serialize(input.Completion);
    public static DrillingDailyReportObservation FromRecord(DRILLING_DAILY_REPORT row)
    {
        var original = DrillingDailyReportInput.FromRecord(row); return FromValidatedRecord(row, original);
    }
    internal static DrillingDailyReportObservation FromValidatedRecord(DRILLING_DAILY_REPORT row, DrillingDailyReportInput? original = null)
    {
        DateTime Utc(DateTime? value) => DateTime.SpecifyKind(value!.Value, DateTimeKind.Utc);
        return new(row.DDR_ID, row.REPORT_NUMBER, row.DRILLING_OPERATION_ID, row.OWNER_TENANT_ID, row.OWNER_COMPANY_BA_ID,
            row.FIELD_ID, row.WELL_UWI, Utc(row.PERIOD_START_UTC), Utc(row.PERIOD_END_UTC), row.DEPTH_START_MD!.Value, row.DEPTH_END_MD!.Value,
            row.DEPTH_OUOM, row.ROTATING_HOURS, row.NPT_HOURS, row.NPT_REASON, row.REPORT_PROVENANCE, row.DAILY_OPERATIONS_SUMMARY,
            row.NEXT_PERIOD_PLAN, row.ROW_CREATED_BY, row.RECORDER_ROLE, row.RECORDER_PERSONA_CODE, Utc(row.ROW_CREATED_DATE), Utc(row.ROW_CHANGED_DATE), row.REPORT_REVISION,
            row.CORRECTS_REPORT_ID is null ? null : new(row.CORRECTS_REPORT_ID, row.CORRECTS_REPORT_REVISION!,
                (original ?? DrillingDailyReportInput.FromRecord(row)).Correction!.Number, row.CORRECTION_REASON!),
            (original ?? DrillingDailyReportInput.FromRecord(row)).Completion, (original ?? DrillingDailyReportInput.FromRecord(row)).WithdrawCompletion, null);
    }
    public void Require(DrillingDailyReportHistory history)
    {
        Correction?.Require(REPORT_NUMBER);
        Completion?.Require(new DateTimeOffset(DateTime.SpecifyKind(history.Execution.Recorded!.SPUD_DATE!.Value, DateTimeKind.Utc)),
            new(DateTime.SpecifyKind(PERIOD_END_UTC, DateTimeKind.Utc)), new(DateTime.SpecifyKind(ROW_CREATED_DATE, DateTimeKind.Utc)));
        if (Completion is not null && Correction is null || CompletionWithdrawn && (Completion is not null || Correction is null))
            throw new InvalidOperationException("Completion reporting/withdrawal must preserve its exact original observation amendment.");
        var scope = history.Execution.Basis.Scope(); var execution = history.Execution.Recorded!;
        var duration = PERIOD_END_UTC - PERIOD_START_UTC; var hours = (decimal)duration.TotalHours;
        if (SupersededByReportId is not null && (!LeaseSummaryValidator.Identifier(SupersededByReportId) || SupersededByReportId == DDR_ID) ||
            !LeaseSummaryValidator.Identifier(DDR_ID) || REPORT_NUMBER < 1 || REPORT_NUMBER > history.Count ||
            DRILLING_OPERATION_ID != execution.DRILLING_OPERATION_ID || OWNER_TENANT_ID != scope.Company.TenantId || OWNER_COMPANY_BA_ID != scope.Company.BusinessAssociateId ||
            FIELD_ID != scope.FieldId || WELL_UWI != execution.WELL_UWI || PERIOD_START_UTC == default || PERIOD_START_UTC.Kind != DateTimeKind.Utc ||
            PERIOD_END_UTC.Kind != DateTimeKind.Utc || duration <= TimeSpan.Zero || duration > TimeSpan.FromHours(24) || PERIOD_START_UTC < execution.SPUD_DATE ||
            DEPTH_START_MD < 0 || DEPTH_END_MD < DEPTH_START_MD || DEPTH_END_MD > 1000000 || DEPTH_OUOM != history.Execution.Basis.Source.Program.DEPTH_OUOM ||
            ROTATING_HOURS < 0 || NPT_HOURS < 0 || (ROTATING_HOURS ?? 0) + (NPT_HOURS ?? 0) > hours ||
            !DrillingDailyReportInput.Text(NPT_REASON, NPT_HOURS > 0) || !DrillingDailyReportInput.Text(REPORT_PROVENANCE, true) ||
            !DrillingDailyReportInput.Text(DAILY_OPERATIONS_SUMMARY, true) || !DrillingDailyReportInput.Text(NEXT_PERIOD_PLAN) ||
            !LeaseSummaryValidator.Identifier(ROW_CREATED_BY) || !LeaseSummaryValidator.Identifier(RECORDER_ROLE) || !LeaseSummaryValidator.Identifier(RECORDER_PERSONA_CODE) || RECORDER_PERSONA_CODE.Length > 64 ||
            ROW_CREATED_DATE == default || ROW_CREATED_DATE.Kind != DateTimeKind.Utc || ROW_CHANGED_DATE.Kind != DateTimeKind.Utc ||
            ROW_CREATED_DATE < PERIOD_END_UTC || ROW_CHANGED_DATE < ROW_CREATED_DATE || ROW_CHANGED_DATE > history.CheckedAtUtc.UtcDateTime || !ProcessDefinitionRevision.IsValid(REPORT_REVISION))
            throw new InvalidOperationException("The qualified report observation belongs to another source or has invalid measurements/audit.");
    }
}

public sealed record DrillingDailyReportPage(
    [property: JsonRequired] DrillingDailyReportHistory History,
    [property: JsonRequired] int AfterNumber,
    [property: JsonRequired] int PageSize,
    [property: JsonRequired] IReadOnlyList<DrillingDailyReportObservation> Reports,
    [property: JsonRequired] string Revision)
{
    public const int DefaultPageSize = 25, MaximumPageSize = 50;
    [JsonIgnore] public bool HasPrevious => AfterNumber > 0;
    [JsonIgnore] public bool HasNext => (long)AfterNumber + Reports.Count < History.Count;
    public static void RequireRequest(int after, int size, string? expected)
    {
        if (after < 0 || size < 1 || size > MaximumPageSize || (expected is null ? after != 0 : !ProcessDefinitionRevision.IsValid(expected)))
            throw new InvalidOperationException("Open the first history page or navigate with its exact checked revision and a bounded page size.");
    }
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { History = History.Revision, AfterNumber, PageSize, Reports }));
    public void Require()
    {
        History.Require(); RequireRequest(AfterNumber, PageSize, History.Revision);
        if (AfterNumber > History.Count || Reports is null || Reports.Count != Math.Min(PageSize, History.Count - AfterNumber) ||
            Reports.Any(row => row is null) || Revision != ContentRevision() || Reports.Select(row => row.DDR_ID).Distinct(StringComparer.Ordinal).Count() != Reports.Count)
            throw new InvalidOperationException("The exact checked report page is missing, changed or incomplete.");
        for (var i = 0; i < Reports.Count; i++)
        {
            Reports[i].Require(History);
            if (Reports[i].REPORT_NUMBER != (long)AfterNumber + i + 1) throw new InvalidOperationException("The report page is not in its exact checked sequence.");
        }
    }
}
