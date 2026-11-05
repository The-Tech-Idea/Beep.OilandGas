using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>Frozen independent review of one immutable completion claim, not completion acceptance.</summary>
public sealed record DrillingCompletionReviewStart(
    [property: JsonRequired] DrillingDailyReportHistory Source,
    [property: JsonRequired] DrillingCompletionReviewOption Workflow,
    [property: JsonRequired] string SenderRole,
    [property: JsonRequired] string SenderPersonaCode,
    [property: JsonRequired] string SenderAuthorityRevision,
    [property: JsonRequired] Guid IntentId,
    [property: JsonRequired] string Reason)
{
    public const string PayloadKey = "DrillingCompletionReviewStart";
    public const string InstancePrefix = "drilling-completion-review-";
    public const int MaximumBodyLength = 16 * 1024 * 1024;
    public DevelopmentAssignedReviewScope Scope() => Source.Execution.Basis.Scope();
    [JsonIgnore] public string ClaimId => Source.ReportedCompletion!.ReportId;
    [JsonIgnore] public string Description => "Completion claim " + Source.ReportedCompletion!.Number + " / " + Source.Execution.Recorded!.WELL_UWI;

    public void Require()
    {
        if (Source is null || Workflow is null) throw new InvalidOperationException("An exact reported completion and configured independent review are required.");
        Source.Require(); Workflow.Require(Source);
        if (Source.ReportedCompletion is null || !Workflow.Ready || IntentId == Guid.Empty ||
            !LeaseSummaryValidator.Identifier(SenderRole) || !LeaseSummaryValidator.Identifier(SenderPersonaCode) || SenderPersonaCode.Length > 64 ||
            !ProcessDefinitionRevision.IsValid(SenderAuthorityRevision) || string.IsNullOrWhiteSpace(Reason) || Reason != Reason.Trim() || Reason.Length > 1600 ||
            Reason.Any(ch => char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) || JsonSerializer.SerializeToUtf8Bytes(this).Length > MaximumBodyLength)
            throw new InvalidOperationException("Retain the exact current completion claim, configured independent stages, sender authority, intent and explicit reason.");
    }
    public string InstanceId() { Require(); return InstancePrefix + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Scope().Company, IntentId })); }
    public string Revision() { Require(); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this)); }

    /// <summary>Recovery permits later progress but requires the exact immutable claim in a fully qualified current page.</summary>
    public void RequireSource(DrillingDailyReportPage current, bool exact = true, bool originalActor = true)
    {
        Require(); current.Require(); Source.Execution.Basis.RequireSource(current.History.Execution.Basis, false, originalActor);
        if (JsonSerializer.Serialize(Source.Execution.Recorded) != JsonSerializer.Serialize(current.History.Execution.Recorded))
            throw new InvalidOperationException("The original completion execution start or native source changed.");
        var claim = Source.ReportedCompletion!;
        var rows = current.Reports.Where(row => row.DDR_ID == claim.ReportId && row.REPORT_NUMBER == claim.Number).ToArray();
        if (rows.Length != 1 || rows[0].REPORT_REVISION != claim.NativeRevision || rows[0].ROW_CREATED_BY != claim.RecorderUserId ||
            rows[0].RECORDER_ROLE != claim.RecorderRole || rows[0].RECORDER_PERSONA_CODE != claim.RecorderPersonaCode ||
            rows[0].ROW_CREATED_DATE != claim.RecordedAtUtc.UtcDateTime || JsonSerializer.Serialize(rows[0].Completion) != JsonSerializer.Serialize(claim.Report) ||
            exact && (current.History.Revision != Source.Revision || current.History.ReportedCompletion?.ReportId != ClaimId || rows[0].SupersededByReportId is not null))
            throw new InvalidOperationException("The exact immutable completion claim or its current submission history changed.");
    }
    public static bool HasPayload(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("Data", out var data) && data.ValueKind == JsonValueKind.Object && data.TryGetProperty(PayloadKey, out _);
    }
    public static CompanyScope OwnerCompany(string json)
    {
        if (json.Length > MaximumBodyLength + 65536) throw new InvalidOperationException("Completion review ownership exceeds its bound.");
        using var doc = JsonDocument.Parse(json);
        var row = doc.RootElement.GetProperty("Data").GetProperty(PayloadKey).GetProperty("Source").GetProperty("Execution").GetProperty("Recorded");
        var company = new CompanyScope(row.GetProperty("OWNER_TENANT_ID").GetString()!, row.GetProperty("OWNER_COMPANY_BA_ID").GetString()!);
        APInvoiceOwnership.RequireScope(company); return company;
    }
    public static DrillingCompletionReviewStart FromPayload(string json, string instance, string field, string claim)
    {
        if (json.Length > MaximumBodyLength + 65536) throw new InvalidOperationException("The original completion review exceeds its bound.");
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.GetProperty("InstanceId").GetString() != instance) throw new InvalidOperationException("The completion review envelope changed.");
        var original = doc.RootElement.GetProperty("Data").GetProperty(PayloadKey).Deserialize<DrillingCompletionReviewStart>()
            ?? throw new InvalidOperationException("The retained completion review is unavailable.");
        original.Require();
        if (original.InstanceId() != instance || original.Scope().FieldId != field || original.ClaimId != claim)
            throw new InvalidOperationException("The original completion review identity, FIELD or claim changed.");
        return original;
    }
}

public sealed record DrillingCompletionReviewStartReceipt(
    [property: JsonRequired] string IntentRevision,
    [property: JsonRequired] string InstanceId,
    [property: JsonRequired] bool Recorded,
    [property: JsonRequired] string? StepInstanceId,
    [property: JsonRequired] IReadOnlyList<string> TaskIds,
    [property: JsonRequired] string RecordedRevision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingCompletionReviewStart original)
    {
        original.Require(); var first = original.Workflow.Stages[0].Definition;
        if (IntentRevision != original.Revision() || InstanceId != original.InstanceId() || TaskIds is null || TaskIds.Count > 128 ||
            TaskIds.Any(id => !LeaseSummaryValidator.Identifier(id)) || TaskIds.Distinct(StringComparer.Ordinal).Count() != TaskIds.Count ||
            !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero ||
            (Recorded ? StepInstanceId != StepId(InstanceId, first.StepId) ||
                !TaskIds.Order(StringComparer.Ordinal).SequenceEqual(first.PersonaCodes.Select(persona => TaskId(InstanceId, StepInstanceId!, persona)).Order(StringComparer.Ordinal)) :
                StepInstanceId is not null || TaskIds.Count != 0))
            throw new InvalidOperationException("The completion review receipt must confirm the exact retained intent and complete first-stage destination keys.");
    }
    public static string StepId(string instance, string step) => "completion-review-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { instance, step }));
    public static string TaskId(string instance, string step, string persona) => "task-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Process = instance, Step = step, Persona = persona }));
}
