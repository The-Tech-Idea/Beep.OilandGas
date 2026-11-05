using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using System.Text.Json;

namespace Beep.OilandGas.HSE.Models;

/// <summary>The reviewed incident and workflow definition retained in the native process payload.</summary>
public sealed record CompanyHseWorkflowStart(FieldSelectionReview Context, AssetDatabaseScope LifecycleDatabase, Guid IntentId, string IncidentId,
    string IncidentRevision, string DefinitionRevision)
{
    public const string ProcessId = "HSE_INCIDENT_REPORTING";
    public const string EntityType = "HSE_INCIDENT";
    public const string PayloadKey = "CompanyHseWorkflowStart";
    public const string InstancePrefix = "hse-process-";

    public void Require()
    {
        if (Context is null) throw new InvalidOperationException("The original company and FIELD review is required.");
        APInvoiceOwnership.RequireScope(Context.Company);
        FieldSelectionReviewValidator.RequireReview(Context, Context.Company, Context.ActorUserId);
        if (LifecycleDatabase is null || !LeaseSummaryValidator.Identifier(LifecycleDatabase.ConnectionName) ||
            !LeaseSummaryValidator.Identifier(LifecycleDatabase.Fingerprint) || IntentId == Guid.Empty || !LeaseSummaryValidator.Identifier(Context.FieldId) ||
            !LeaseSummaryValidator.Identifier(IncidentId) || !ProcessDefinitionRevision.IsValid(IncidentRevision) ||
            !ProcessDefinitionRevision.IsValid(DefinitionRevision) || JsonSerializer.Serialize(this).Length > 8192)
            throw new InvalidOperationException("The original incident, definition and FIELD review are required.");
    }

    /// <summary>One native process key per incident and company/FIELD target, independent of retry or actor.</summary>
    public string InstanceId()
    {
        Require();
        return InstancePrefix + ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new
        {
            Context.Company, Context.Database, LifecycleDatabase, Context.FieldId, IncidentId
        }));
    }

    public static CompanyHseWorkflowStart FromPayload(string json, string instanceId, string fieldId, string incidentId)
    {
        if (string.IsNullOrWhiteSpace(json) || json.Length > 262144 || !LeaseSummaryValidator.Identifier(instanceId) ||
            !LeaseSummaryValidator.Identifier(fieldId) || !LeaseSummaryValidator.Identifier(incidentId))
            throw new InvalidOperationException("The recorded HSE workflow payload or identity is unavailable.");
        try
        {
            using var document = JsonDocument.Parse(json);
            if (document.RootElement.ValueKind != JsonValueKind.Object || document.RootElement.EnumerateObject().Count(p => p.Name == "Data") != 1 ||
                !document.RootElement.TryGetProperty("Data", out var data) || data.ValueKind != JsonValueKind.Object ||
                data.EnumerateObject().Count(p => p.Name == PayloadKey) != 1 || !data.TryGetProperty(PayloadKey, out var value) || value.GetRawText().Length > 16384)
                throw new InvalidOperationException("The recorded HSE workflow has no original company ownership.");
            var original = value.Deserialize<CompanyHseWorkflowStart>() ?? throw new InvalidOperationException("The original HSE workflow review is unavailable.");
            original.Require();
            if (original.InstanceId() != instanceId || original.Context.FieldId != fieldId || original.IncidentId != incidentId)
                throw new InvalidOperationException("The recorded workflow differs from its original incident or FIELD.");
            return original;
        }
        catch (JsonException invalid) { throw new InvalidOperationException("The recorded HSE workflow payload is invalid.", invalid); }
    }
}

public sealed record CompanyHseWorkflowReceipt(CompanyScope Company, string Actor, string FieldId, Guid IntentId,
    string IncidentId, string InstanceId, string StepInstanceId, IReadOnlyList<string> TaskIds, string GraphRevision)
{
    public void Require(CompanyHseWorkflowStart original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        if (Company != original.Context.Company || Actor != original.Context.ActorUserId || FieldId != original.Context.FieldId ||
            IntentId != original.IntentId || IncidentId != original.IncidentId || InstanceId != original.InstanceId() ||
            !LeaseSummaryValidator.Identifier(StepInstanceId) || TaskIds is null || TaskIds.Count is < 1 or > 128 ||
            TaskIds.Any(id => !LeaseSummaryValidator.Identifier(id)) || TaskIds.Distinct(StringComparer.Ordinal).Count() != TaskIds.Count ||
            !ProcessDefinitionRevision.IsValid(GraphRevision))
            throw new InvalidOperationException("The recorded HSE workflow differs from its retained original.");
    }
}

public sealed record CompanyHseWorkflowReview(FieldSelectionReview Context, AssetDatabaseScope LifecycleDatabase,
    string IncidentId, string IncidentRevision, string DefinitionRevision, string ProcessName, string InitialRole)
{
    public CompanyHseWorkflowStart CreateIntent(Guid intentId)
    {
        var original = new CompanyHseWorkflowStart(Context, LifecycleDatabase, intentId, IncidentId, IncidentRevision, DefinitionRevision);
        original.Require();
        if (string.IsNullOrWhiteSpace(ProcessName) || ProcessName.Length > 512 || ProcessName.Any(char.IsControl) ||
            !LeaseSummaryValidator.Identifier(InitialRole))
            throw new InvalidOperationException("The configured HSE workflow name and initial role are required.");
        return original;
    }
}

/// <summary>The API supplies current company/FIELD authority and the incident owner's recorded revision.</summary>
public interface ICompanyHseWorkflowCommands
{
    Task<CompanyHseClosureReview> ReviewCorrectionReturnAsync(IncidentClosureReview evidence, AssetDatabaseScope lifecycle, string nativeRevision, Func<string, Task> requireRoleAuthority);
    Task<CompanyHseCorrectionReceipt> ReturnForCorrectionAsync(CompanyHseCorrectionCommand original, Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedSource, Func<Task> requireRecordedRejection);
    Task<CompanyHseClosureReview> ReviewClosureStageAsync(IncidentClosureReview evidence, AssetDatabaseScope lifecycle, string nativeRevision, Func<string, Task> requireRoleAuthority);
    Task<CompanyHseClosureReceipt> CompleteClosureStageAsync(CompanyHseClosureCommand original, Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedSource);
    Task<CompanyHseClosureRecovery?> RecoverClosureStageAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle, string incidentId, Func<string, Task> requireRoleAuthority, Func<CompanyHseClosureCommand, Task> requireRecordedSource);
    Task<CompanyHseRcaHandoffReview> ReviewRcaHandoffAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle,
        IncidentRcaRecordedCause cause, Func<string, Task> requireRoleAuthority);
    Task<CompanyHseRcaHandoffReceipt> CompleteRcaHandoffAsync(CompanyHseRcaHandoffCommand original, Func<string, Task> requireRoleAuthority, Func<Task> requireRecordedCause);
    Task<CompanyHseRcaRecoveredHandoff?> RecoverRcaHandoffAsync(FieldSelectionReview context, AssetDatabaseScope lifecycle, string incidentId,
        Func<string, Task> requireRoleAuthority, Func<IncidentRcaRecordedCause, Task> requireRecordedCause);
    Task<CompanyHseWorkflowReceipt> StartHseWorkflowAsync(CompanyHseWorkflowStart original,
        Func<Task> requireCurrentAuthority, Func<Task<string>> readIncidentRevision);
}
