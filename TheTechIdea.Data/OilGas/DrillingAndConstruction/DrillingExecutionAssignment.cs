using System.Text.Json;
using System.Text.Json.Serialization;

namespace TheTechIdea.Data.OilGas;

/// <summary>A configured ongoing reporting responsibility; it is not an overdue daily-report schedule.</summary>
public sealed record DrillingExecutionAssignmentOption(
    [property: JsonRequired] string ProcessId,
    [property: JsonRequired] string ProcessName,
    [property: JsonRequired] string Version,
    [property: JsonRequired] string DefinitionRevision,
    [property: JsonRequired] DevelopmentReviewRoleStep Step,
    [property: JsonRequired] DevelopmentReviewRecipientStep Authority)
{
    [JsonIgnore] public bool Ready => Authority.Recipients.Count > 0;

    public void Require()
    {
        if (!LeaseSummaryValidator.Identifier(ProcessId) || string.IsNullOrWhiteSpace(ProcessName) ||
            ProcessName.Length > 256 || ProcessName.Any(char.IsControl) || !LeaseSummaryValidator.Identifier(Version) ||
            !ProcessDefinitionRevision.IsValid(DefinitionRevision) || Step is null || Authority is null)
            throw new InvalidOperationException("An exact configured execution reporting workflow is required.");
        var recipients = Authority.Recipients;
        if (!LeaseSummaryValidator.Identifier(Step.StepId) || string.IsNullOrWhiteSpace(Step.StepName) ||
            Step.StepName.Length > 256 || Step.StepName.Any(char.IsControl) || Step.Sequence < 0 ||
            !LeaseSummaryValidator.Identifier(Step.AssignedRole) || Step.SlaHours < 0 ||
            Authority.StepId != Step.StepId || Authority.AssignedRole != Step.AssignedRole ||
            !ProcessDefinitionRevision.IsValid(Authority.AuthorityRevision) || recipients is null || recipients.Count > 256 ||
            recipients.Any(row => row is null || !LeaseSummaryValidator.Identifier(row.UserId) ||
                !LeaseSummaryValidator.Identifier(row.PersonaCode) || row.PersonaCode.Length > 64) ||
            recipients.Select(row => row.UserId).Distinct(StringComparer.Ordinal).Count() != recipients.Count ||
            Step.PersonaCodes is null || Step.PersonaCodes.Count > 128 ||
            Step.PersonaCodes.Any(code => !LeaseSummaryValidator.Identifier(code) || code.Length > 64) ||
            Step.PersonaCodes.Distinct(StringComparer.Ordinal).Count() != Step.PersonaCodes.Count ||
            !Step.PersonaCodes.Order(StringComparer.Ordinal).SequenceEqual(
                recipients.Select(row => row.PersonaCode).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("The exact configured execution role, personas and recipient authority are required.");
    }
}

/// <summary>Frozen handoff of one recorded execution to configured ongoing reporting responsibility.</summary>
public sealed record DrillingExecutionAssignment(
    [property: JsonRequired] DrillingExecutionStatus Source,
    [property: JsonRequired] DrillingExecutionAssignmentOption Workflow,
    [property: JsonRequired] string SenderRole,
    [property: JsonRequired] string SenderPersonaCode,
    [property: JsonRequired] string SenderAuthorityRevision,
    [property: JsonRequired] Guid IntentId,
    [property: JsonRequired] string Reason)
{
    public const string PayloadKey = "DrillingExecutionAssignment";
    public const string InstancePrefix = "drilling-execution-assignment-";
    public const int MaximumBodyLength = 16 * 1024 * 1024;

    public DevelopmentAssignedReviewScope Scope() => Source.Basis.Scope();
    [JsonIgnore] public string OperationId => Source.Recorded!.DRILLING_OPERATION_ID;

    public void Require()
    {
        if (Source is null || Workflow is null) throw new InvalidOperationException("An exact recorded execution and configured assignment are required.");
        Source.Require(); Workflow.Require();
        if (Source.Recorded is null || !Workflow.Ready || IntentId == Guid.Empty ||
            !LeaseSummaryValidator.Identifier(SenderRole) || !LeaseSummaryValidator.Identifier(SenderPersonaCode) ||
            SenderPersonaCode.Length > 64 || !ProcessDefinitionRevision.IsValid(SenderAuthorityRevision) ||
            string.IsNullOrWhiteSpace(Reason) || Reason != Reason.Trim() || Reason.Length > 1600 ||
            Reason.Any(ch => char.IsControl(ch) && ch is not ('\r' or '\n' or '\t')) ||
            JsonSerializer.SerializeToUtf8Bytes(this).Length > MaximumBodyLength)
            throw new InvalidOperationException("Retain the original recorded execution, current sender role/persona, configured recipients, intent and explicit assignment reason.");
    }

    public string InstanceId()
    {
        Require();
        return InstancePrefix + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { Scope().Company, IntentId }));
    }
    public string Revision() { Require(); return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(this)); }

    public void RequireSource(DrillingExecutionStatus current, bool originalActor = true)
    {
        Require(); current.Require(); Source.Basis.RequireSource(current.Basis, false, originalActor);
        if (current.Recorded is null || JsonSerializer.Serialize(current.Recorded) != JsonSerializer.Serialize(Source.Recorded))
            throw new InvalidOperationException("The exact original execution start, ownership or native proof changed.");
    }

    public static bool HasPayload(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.TryGetProperty("Data", out var data) && data.ValueKind == JsonValueKind.Object && data.TryGetProperty(PayloadKey, out _);
    }
    public static CompanyScope OwnerCompany(string json)
    {
        if (json.Length > MaximumBodyLength + 65536) throw new InvalidOperationException("The original execution assignment ownership exceeds its bound.");
        using var doc = JsonDocument.Parse(json);
        var row = doc.RootElement.GetProperty("Data").GetProperty(PayloadKey).GetProperty("Source").GetProperty("Recorded");
        var company = new CompanyScope(row.GetProperty("OWNER_TENANT_ID").GetString()!, row.GetProperty("OWNER_COMPANY_BA_ID").GetString()!);
        APInvoiceOwnership.RequireScope(company);
        return company;
    }
    public static DrillingExecutionAssignment FromPayload(string json, string instance, string field, string operation)
    {
        if (json.Length > MaximumBodyLength + 65536) throw new InvalidOperationException("The original execution assignment exceeds its bound.");
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.GetProperty("InstanceId").GetString() != instance) throw new InvalidOperationException("The execution assignment envelope changed.");
        var original = doc.RootElement.GetProperty("Data").GetProperty(PayloadKey).Deserialize<DrillingExecutionAssignment>()
            ?? throw new InvalidOperationException("The original execution assignment is unavailable.");
        original.Require();
        if (original.InstanceId() != instance || original.Scope().FieldId != field || original.OperationId != operation)
            throw new InvalidOperationException("The original execution assignment source identity changed.");
        return original;
    }
}

public sealed record DrillingExecutionAssignmentReceipt(
    [property: JsonRequired] string IntentRevision,
    [property: JsonRequired] string InstanceId,
    [property: JsonRequired] bool Recorded,
    [property: JsonRequired] string? StepInstanceId,
    [property: JsonRequired] IReadOnlyList<string> TaskIds,
    [property: JsonRequired] string RecordedRevision,
    [property: JsonRequired] DateTimeOffset CheckedAtUtc)
{
    public void Require(DrillingExecutionAssignment original)
    {
        original.Require();
        if (IntentRevision != original.Revision() || InstanceId != original.InstanceId() || TaskIds is null || TaskIds.Count > 128 ||
            TaskIds.Any(id => !LeaseSummaryValidator.Identifier(id)) || TaskIds.Distinct(StringComparer.Ordinal).Count() != TaskIds.Count ||
            !ProcessDefinitionRevision.IsValid(RecordedRevision) || CheckedAtUtc == default || CheckedAtUtc.Offset != TimeSpan.Zero ||
            (Recorded ? StepInstanceId != StepId(InstanceId, original.Workflow.Step.StepId) || !TaskIds.Order(StringComparer.Ordinal).SequenceEqual(original.Workflow.Step.PersonaCodes.Select(persona=>TaskId(InstanceId,StepInstanceId!,persona)).Order(StringComparer.Ordinal)) :
                StepInstanceId is not null || TaskIds.Count != 0))
            throw new InvalidOperationException("The assignment receipt does not confirm this exact original and its configured reporting destinations.");
    }
    public static string TaskId(string instance,string step,string persona)=>"task-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Process=instance,Step=step,Persona=persona}));
    public static string StepId(string instance, string step) => "execution-reporting-step-" + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new { instance, step }));
}
