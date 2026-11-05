using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.DrillingAndConstruction.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Beep.OilandGas.DrillingAndConstruction.Models;

/// <summary>An explicit amendment of the exact current observation; its original remains immutable.</summary>
public sealed record DrillingDailyReportCorrection(string ReportId, string NativeRevision, int Number, string Reason)
{
    public void Require(int nextNumber)
    {
        if (!LeaseSummaryValidator.Identifier(ReportId) || !ProcessDefinitionRevision.IsValid(NativeRevision) ||
            Number < 1 || Number >= nextNumber || !DrillingDailyReportInput.Text(Reason, true))
            throw new InvalidOperationException("Choose the exact current report revision and state the correction reason.");
    }
}

public sealed record DrillingDailyReportBasis(
    [property: JsonRequired] DrillingExecutionStatus Execution,
    [property: JsonRequired] int NextNumber,
    [property: JsonRequired] string? PreviousReportId,
    [property: JsonRequired] string? PreviousReportRevision,
    [property: JsonRequired] DateTimeOffset? PreviousPeriodEndUtc,
    [property: JsonRequired] IReadOnlyList<string> RecorderRoles,
    [property: JsonRequired] string? PersonaCode,
    [property: JsonRequired] string? RecorderAuthorityRevision,
    [property: JsonRequired] string Revision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    [JsonIgnore] public bool CanRecord => RecorderRoles.Count > 0;
    public DevelopmentAssignedReviewScope Scope() => Execution.Basis.Scope();
    public string ContentRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
    {
        Execution = Execution.Revision, NextNumber, PreviousReportId, PreviousReportRevision,
        PreviousPeriodEndUtc, RecorderRoles, PersonaCode, RecorderAuthorityRevision
    }));
    public void Require()
    {
        Execution.Require(); RequireFields();
    }
    internal void RequireFields()
    {
        if (Execution.Recorded is null || NextNumber < 1 ||
            (NextNumber == 1 ? PreviousReportId is not null || PreviousReportRevision is not null || PreviousPeriodEndUtc is not null :
                !LeaseSummaryValidator.Identifier(PreviousReportId) || !ProcessDefinitionRevision.IsValid(PreviousReportRevision) ||
                PreviousPeriodEndUtc is null || PreviousPeriodEndUtc == default(DateTimeOffset) || PreviousPeriodEndUtc.Value.Offset != TimeSpan.Zero) ||
            RecorderRoles is null || RecorderRoles.Count > 256 || RecorderRoles.Any(role => !LeaseSummaryValidator.Identifier(role)) ||
            RecorderRoles.Distinct(StringComparer.Ordinal).Count() != RecorderRoles.Count ||
            (CanRecord ? !LeaseSummaryValidator.Identifier(PersonaCode) || PersonaCode!.Length > 64 || !ProcessDefinitionRevision.IsValid(RecorderAuthorityRevision) : PersonaCode is not null || RecorderAuthorityRevision is not null) ||
            CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero || PreviousPeriodEndUtc > CheckedAtUtc || Revision != ContentRevision())
            throw new InvalidOperationException("The exact execution, current report head and configured recorder are required.");
    }
    public void RequireExecution(DrillingExecutionStatus current, bool sameActor = true)
    {
        Require(); current.Require();
        Execution.Basis.RequireSource(current.Basis, false, sameActor);
        if (current.Recorded is null || current.Recorded.EXECUTION_START_REVISION != Execution.Recorded!.EXECUTION_START_REVISION ||
            current.Recorded.DRILLING_OPERATION_ID != Execution.Recorded.DRILLING_OPERATION_ID)
            throw new InvalidOperationException("The original drilling execution changed.");
    }
}

public sealed record DrillingDailyReportInput(
    [property: JsonRequired] DrillingDailyReportBasis Basis,
    [property: JsonRequired] Guid IntentId,
    [property: JsonRequired] string RecorderRole,
    [property: JsonRequired] DateTimeOffset PeriodStartUtc,
    [property: JsonRequired] DateTimeOffset PeriodEndUtc,
    [property: JsonRequired] decimal DepthStartMd,
    [property: JsonRequired] decimal DepthEndMd,
    [property: JsonRequired] string DepthUnit,
    [property: JsonRequired] decimal? RotatingHours,
    [property: JsonRequired] decimal? NptHours,
    [property: JsonRequired] string? NptReason,
    [property: JsonRequired] string Provenance,
    [property: JsonRequired] string Summary,
    [property: JsonRequired] string? NextPlan)
{
    public const int MaximumBodyLength = 16 * 1024 * 1024;
    // Absence is the canonical representation of an observation without a correction.
    // Adding optional correction metadata must not rewrite its immutable original/digest.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DrillingDailyReportCorrection? Correction { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] public DrillingCompletionReport? Completion { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)] public bool WithdrawCompletion { get; init; }
    public static bool Text(string? text, bool required = false) => text is null ? !required :
        text == text.Trim() && (!required || text.Length > 0) && text.Length <= 1600 &&
        !text.Any(c => char.IsControl(c) && c is not ('\r' or '\n' or '\t'));
    public void Require()
    {
        Basis.Require(); RequireFields();
    }
    internal void RequireFields()
    {
        Correction?.Require(Basis.NextNumber);
        var spud = new DateTimeOffset(DateTime.SpecifyKind(Basis.Execution.Recorded!.SPUD_DATE!.Value, DateTimeKind.Utc));
        Completion?.Require(spud, PeriodEndUtc);
        if (Completion is not null && Correction is null)
            throw new InvalidOperationException("Report completion against the exact current latest observation, preserving its original measurements and audit.");
        if (WithdrawCompletion && (Correction is null || Completion is not null))
            throw new InvalidOperationException("An explicit withdrawal must correct an existing completion report with a reason and without replacement completion claims.");
        var duration = PeriodEndUtc - PeriodStartUtc;
        var hours = (decimal)duration.TotalHours;
        if (!Basis.CanRecord || IntentId == Guid.Empty || !Basis.RecorderRoles.Contains(RecorderRole, StringComparer.Ordinal) ||
            PeriodStartUtc == default || PeriodStartUtc.Offset != TimeSpan.Zero || PeriodEndUtc.Offset != TimeSpan.Zero ||
            duration <= TimeSpan.Zero || duration > TimeSpan.FromHours(24) || PeriodStartUtc < spud ||
            Correction is null && Basis.PreviousPeriodEndUtc is {} end && PeriodStartUtc < end ||
            DepthStartMd < 0 || DepthEndMd < DepthStartMd || DepthEndMd > 1000000 ||
            string.IsNullOrWhiteSpace(DepthUnit) || DepthUnit.Length > 32 || DepthUnit != Basis.Execution.Basis.Source.Program.DEPTH_OUOM ||
            RotatingHours < 0 || NptHours < 0 || RotatingHours > hours || NptHours > hours || (RotatingHours ?? 0) + (NptHours ?? 0) > hours ||
            !Text(NptReason, NptHours > 0) || !Text(Provenance, true) || !Text(Summary, true) || !Text(NextPlan) ||
            JsonSerializer.SerializeToUtf8Bytes(this).Length > MaximumBodyLength)
            throw new InvalidOperationException("Report completed UTC periods (up to 24 hours), observed hole MD in the program unit, source provenance and summary. New periods follow the current history; corrections must fit their original neighbours. Unknown hours stay empty; NPT requires its reason.");
    }
    public string ReportId() => "drilling-report-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Basis.Scope().Company, IntentId }));
    public string Revision() { Require(); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this)); }
    internal string ObservationRevision() => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
    {
        PeriodStartUtc, PeriodEndUtc, DepthStartMd, DepthEndMd, DepthUnit, RotatingHours, NptHours, NptReason, Provenance, Summary, NextPlan, Completion
    }));
    public DRILLING_DAILY_REPORT ToRecord(DateTime utc)
    {
        Require(); return ToValidatedRecord(utc);
    }
    internal DRILLING_DAILY_REPORT ToValidatedRecord(DateTime utc)
    {
        if (utc.Kind != DateTimeKind.Utc || utc < Basis.CheckedAtUtc.UtcDateTime || utc < PeriodEndUtc.UtcDateTime)
            throw new InvalidOperationException("A completed observed reporting period and actual producer UTC are required.");
        Completion?.Require(new DateTimeOffset(DateTime.SpecifyKind(Basis.Execution.Recorded!.SPUD_DATE!.Value, DateTimeKind.Utc)), PeriodEndUtc, new(utc));
        var scope = Basis.Scope(); var execution = Basis.Execution.Recorded!;
        var row = new DRILLING_DAILY_REPORT
        {
            DDR_ID = ReportId(), OWNER_TENANT_ID = scope.Company.TenantId, OWNER_COMPANY_BA_ID = scope.Company.BusinessAssociateId,
            FIELD_ID = scope.FieldId, WELL_UWI = execution.WELL_UWI, PROGRAM_ID = execution.PROGRAM_ID,
            DRILLING_OPERATION_ID = execution.DRILLING_OPERATION_ID, EXECUTION_START_REVISION = execution.EXECUTION_START_REVISION,
            REPORT_NUMBER = Basis.NextNumber, PERIOD_START_UTC = PeriodStartUtc.UtcDateTime, PERIOD_END_UTC = PeriodEndUtc.UtcDateTime,
            DEPTH_START_MD = DepthStartMd, DEPTH_END_MD = DepthEndMd, DEPTH_OUOM = DepthUnit,
            ROTATING_HOURS = RotatingHours, NPT_HOURS = NptHours, NPT_REASON = NptReason,
            REPORT_PROVENANCE = Provenance, DAILY_OPERATIONS_SUMMARY = Summary, NEXT_PERIOD_PLAN = NextPlan,
            CORRECTS_REPORT_ID = Correction?.ReportId, CORRECTS_REPORT_REVISION = Correction?.NativeRevision, CORRECTION_REASON = Correction?.Reason,
            REPORTED_COMPLETION_JSON = Completion is null ? null : JsonSerializer.Serialize(Completion), REPORTED_COMPLETION_UTC = Completion?.CompletedAtUtc.UtcDateTime,
            RECORDER_ROLE = RecorderRole, RECORDER_PERSONA_CODE = Basis.PersonaCode!, REPORT_STATUS = "RECORDED",
            REPORT_ORIGINAL_JSON = JsonSerializer.Serialize(this), PPDM_GUID = IntentId.ToString("D"), SOURCE = "BEEP_OILGAS", ACTIVE_IND = "Y",
            ROW_CREATED_BY = scope.ActorUserId, ROW_CHANGED_BY = scope.ActorUserId, ROW_CREATED_DATE = utc, ROW_CHANGED_DATE = utc
        };
        row.REPORT_REVISION = Digest(row); return row;
    }
    public static string Digest(DRILLING_DAILY_REPORT row)
    {
        var copy = CompanyInventorySourceReview.Copy(row); copy.REPORT_REVISION = "";
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));
    }
    public static DrillingDailyReportInput FromRecord(DRILLING_DAILY_REPORT row) => new DrillingDailyReportRecordValidator().Read(row);

}

public sealed record DrillingDailyReportReceipt(
    [property: JsonRequired] string IntentRevision,
    [property: JsonRequired] string ReportId,
    [property: JsonRequired] DRILLING_DAILY_REPORT? Recorded,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingDailyReportInput original)
    {
        original.Require();
        if (IntentRevision != original.Revision() || ReportId != original.ReportId() ||
            Recorded is not null && DrillingDailyReportInput.FromRecord(Recorded).Revision() != original.Revision() ||
            CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero)
            throw new InvalidOperationException("The exact original drilling report outcome is unconfirmed.");
    }
}

/// <summary>Per-scan immutable-proof validation. Keys hash the complete serialized execution, never its claimed revision or live authority.</summary>
public sealed class DrillingDailyReportRecordValidator
{
    private readonly HashSet<string> executions = new(StringComparer.Ordinal);
    public static string ExecutionKey(DrillingExecutionStatus execution) => ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(execution));
    private void Validate(DrillingDailyReportInput original)
    {
        var key = ExecutionKey(original.Basis.Execution);
        if (!executions.Contains(key)) { original.Basis.Execution.Require(); executions.Add(key); }
        original.Basis.RequireFields(); original.RequireFields();
    }
    public DrillingDailyReportInput Read(DRILLING_DAILY_REPORT row)
    {
        if (row?.REPORT_ORIGINAL_JSON is not { Length: > 0 and <= DrillingDailyReportInput.MaximumBodyLength } json)
            throw new InvalidOperationException("The original drilling report proof is unavailable.");
        var original = JsonSerializer.Deserialize<DrillingDailyReportInput>(json) ?? throw new InvalidOperationException("The drilling report proof is invalid.");
        Validate(original); var actual = CompanyInventorySourceReview.Copy(row);
        DateTime Utc(DateTime? value) => value is {} utc && utc != default && utc.Kind != DateTimeKind.Local ?
            DateTime.SpecifyKind(utc, DateTimeKind.Utc) : throw new InvalidOperationException("The report UTC/audit proof is invalid.");
        actual.ROW_CREATED_DATE = Utc(actual.ROW_CREATED_DATE); actual.ROW_CHANGED_DATE = Utc(actual.ROW_CHANGED_DATE);
        actual.PERIOD_START_UTC = Utc(actual.PERIOD_START_UTC); actual.PERIOD_END_UTC = Utc(actual.PERIOD_END_UTC);
        if (actual.REPORTED_COMPLETION_UTC is not null) actual.REPORTED_COMPLETION_UTC = Utc(actual.REPORTED_COMPLETION_UTC);
        var expected = original.ToValidatedRecord(actual.ROW_CREATED_DATE.Value);
        // The shared common-column producer stamps creation and change separately on insert.
        expected.ROW_CHANGED_DATE = actual.ROW_CHANGED_DATE; expected.REPORT_REVISION = DrillingDailyReportInput.Digest(expected);
        if (actual.ROW_CHANGED_DATE < actual.ROW_CREATED_DATE || row.REPORT_REVISION != expected.REPORT_REVISION ||
            JsonSerializer.Serialize(actual) != JsonSerializer.Serialize(expected))
            throw new InvalidOperationException("The report differs from its exact immutable original execution/measurement/provenance/audit record.");
        return original;
    }
}
