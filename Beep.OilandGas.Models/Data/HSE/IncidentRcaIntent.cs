using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.HSE;
using Beep.OilandGas.PPDM39.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HSE;

/// <summary>An exact reviewed root-cause observation, retained for retry and lifecycle handoff.</summary>
public sealed record IncidentRcaIntent(Guid IntentId, IncidentRcaSourceReview Source, IncidentRcaOption Option, string Summary)
{
    public void Require()
    {
        if (Source is null || Option is null) throw new InvalidOperationException("The reviewed incident and cause option are required.");
        Source.Require();
        if (IntentId == Guid.Empty || !Source.Options.Contains(Option) || string.IsNullOrWhiteSpace(Summary) || Summary != Summary.Trim() ||
            Summary.Length is < 20 or > 4000 || Summary.Any(value => char.IsControl(value) && value is not ('\r' or '\n')) ||
            JsonSerializer.Serialize(this).Length > 16384)
            throw new InvalidOperationException("The original root-cause request is invalid or too large.");
    }
}

public sealed record IncidentRcaReceipt(CompanyScope Company, string Actor, string FieldId, Guid IntentId, string IncidentId, int CauseObservation, string IntentRevision)
{
    public void Require(IncidentRcaIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require();
        if (Company != original.Source.Context.Company || Actor != original.Source.Context.ActorUserId || FieldId != original.Source.Context.FieldId ||
            IntentId != original.IntentId || IncidentId != original.Source.IncidentId || CauseObservation != original.Source.NextObservation ||
            IntentRevision != HSE_INCIDENT_RCA_INTENT.Create(original).COMMAND_HASH)
            throw new InvalidOperationException("The recorded root cause differs from the retained original.");
    }
}

/// <summary>Shared verified cause evidence; the author's retained request remains private.</summary>
public sealed record IncidentRcaRecordedCause(CompanyScope Company, AssetDatabaseScope Database, string FieldId,
    string IncidentId, Guid IntentId, int Observation, string CauseCode, string Summary, string RecordedBy, string Revision)
{
    public void Require()
    {
        if (Company is null || Database is null || !LeaseSummaryValidator.Identifier(Company.TenantId) ||
            !LeaseSummaryValidator.Identifier(Company.BusinessAssociateId) || !LeaseSummaryValidator.Identifier(Database.ConnectionName) ||
            !LeaseSummaryValidator.Identifier(Database.Fingerprint) || !LeaseSummaryValidator.Identifier(FieldId) ||
            !LeaseSummaryValidator.Identifier(IncidentId) || IntentId == Guid.Empty || Observation < 1 ||
            !LeaseSummaryValidator.Identifier(CauseCode) || !LeaseSummaryValidator.Identifier(RecordedBy) ||
            string.IsNullOrWhiteSpace(Summary) || Summary != Summary.Trim() || Summary.Length is < 20 or > 4000 ||
            Summary.Any(value => char.IsControl(value) && value is not ('\r' or '\n')) || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("The shared recorded cause is incomplete or invalid.");
    }
}

/// <summary>Private ownership and original request stored beside the PPDM cause.</summary>
public sealed class HSE_INCIDENT_RCA_INTENT : ModelEntityBase
{
    [Key, Required, MaxLength(32)] public string HSE_INCIDENT_RCA_INTENT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_TENANT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = "";
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = "";
    [Required, MaxLength(128)] public string INCIDENT_ID { get; set; } = "";
    public int CAUSE_OBS_NO { get; set; }
    [Required, MaxLength(16384)] public string COMMAND_JSON { get; set; } = "";
    [Required, MaxLength(64)] public string COMMAND_HASH { get; set; } = "";

    public static HSE_INCIDENT_RCA_INTENT Create(IncidentRcaIntent original)
    {
        ArgumentNullException.ThrowIfNull(original); original.Require(); var json = JsonSerializer.Serialize(original);
        return new() { HSE_INCIDENT_RCA_INTENT_ID = original.IntentId.ToString("N"), OWNER_TENANT_ID = original.Source.Context.Company.TenantId,
            OWNER_COMPANY_BA_ID = original.Source.Context.Company.BusinessAssociateId, FIELD_ID = original.Source.Context.FieldId!, INCIDENT_ID = original.Source.IncidentId,
            CAUSE_OBS_NO = original.Source.NextObservation, COMMAND_JSON = json, COMMAND_HASH = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))), ACTIVE_IND = "Y" };
    }

    public IncidentRcaIntent Restore(CompanyScope company, string actor)
    {
        if (company is null || !LeaseSummaryValidator.Identifier(actor) || OWNER_TENANT_ID != company.TenantId || OWNER_COMPANY_BA_ID != company.BusinessAssociateId ||
            ACTIVE_IND != "Y" || ROW_CREATED_BY != actor || ROW_CHANGED_BY != actor || ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created ||
            ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created || string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length > 16384 ||
            COMMAND_HASH != Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(COMMAND_JSON))))
            throw new InvalidOperationException("The retained root-cause ownership or payload is unconfirmed.");
        var original = JsonSerializer.Deserialize<IncidentRcaIntent>(COMMAND_JSON) ?? throw new InvalidOperationException("The retained root-cause request is unavailable.");
        original.Require(); var expected = Create(original);
        if (original.Source.Context.Company != company || original.Source.Context.ActorUserId != actor || HSE_INCIDENT_RCA_INTENT_ID != expected.HSE_INCIDENT_RCA_INTENT_ID ||
            FIELD_ID != expected.FIELD_ID || INCIDENT_ID != expected.INCIDENT_ID || CAUSE_OBS_NO != expected.CAUSE_OBS_NO)
            throw new InvalidOperationException("The retained root-cause request differs from its original identity.");
        return original;
    }
}
