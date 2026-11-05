using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
namespace TheTechIdea.Data.OilGas;

public sealed record IncidentClosureStoredProof(IncidentClosedReceipt Receipt, string StatusJson, string ResponseRevision)
{
    public void Require() { Receipt.Require(); if (string.IsNullOrWhiteSpace(StatusJson) || StatusJson.Length > 16384 || !ProcessDefinitionRevision.IsValid(ResponseRevision)) throw new InvalidOperationException("The native closure proof is invalid."); }
}
public sealed class HSE_INCIDENT_CLOSURE : ModelEntityBase
{
    [Key, Required, MaxLength(32)] public string HSE_INCIDENT_CLOSURE_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_TENANT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = "";
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = "";
    [Required, MaxLength(128)] public string INCIDENT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string PROJECT_ID { get; set; } = "";
    [Required, MaxLength(65536)] public string COMMAND_JSON { get; set; } = "";
    [Required, MaxLength(64)] public string COMMAND_HASH { get; set; } = "";
    public static HSE_INCIDENT_CLOSURE Create(IncidentClosureStoredProof proof)
    {
        proof.Require(); var r = proof.Receipt.Original.Review; var json = JsonSerializer.Serialize(proof);
        if (json.Length > 65536) throw new InvalidOperationException("The native closure proof is too large.");
        return new() { HSE_INCIDENT_CLOSURE_ID = proof.Receipt.Original.IntentId.ToString("N"), OWNER_TENANT_ID = r.Context.Company.TenantId, OWNER_COMPANY_BA_ID = r.Context.Company.BusinessAssociateId,
            FIELD_ID = r.Context.FieldId!, INCIDENT_ID = r.Evidence.Actions.IncidentId, PROJECT_ID = r.Evidence.Actions.PlanId, COMMAND_JSON = json, COMMAND_HASH = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))), ACTIVE_IND = "Y" };
    }
    public IncidentClosureStoredProof Restore(CompanyScope company)
    {
        if (OWNER_TENANT_ID != company.TenantId || OWNER_COMPANY_BA_ID != company.BusinessAssociateId || ACTIVE_IND != "Y" || string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length > 65536 ||
            COMMAND_HASH != Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(COMMAND_JSON))) || ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created || changed > DateTime.UtcNow)
            throw new InvalidOperationException("Recorded native closure is unavailable or corrupt.");
        var proof = JsonSerializer.Deserialize<IncidentClosureStoredProof>(COMMAND_JSON) ?? throw new InvalidOperationException("Recorded native closure is invalid."); proof.Require(); var expected = Create(proof);
        if (proof.Receipt.Original.Review.Context.Company != company || HSE_INCIDENT_CLOSURE_ID != expected.HSE_INCIDENT_CLOSURE_ID || FIELD_ID != expected.FIELD_ID || INCIDENT_ID != expected.INCIDENT_ID || PROJECT_ID != expected.PROJECT_ID ||
            ROW_CREATED_BY != proof.Receipt.Original.Review.Context.ActorUserId || ROW_CHANGED_BY != ROW_CREATED_BY) throw new InvalidOperationException("The recorded closure ownership or audit changed.");
        return proof;
    }
}
