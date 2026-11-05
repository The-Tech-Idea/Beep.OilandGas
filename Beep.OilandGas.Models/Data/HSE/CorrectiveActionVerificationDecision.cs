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

/// <summary>An independent decision on one exact recorded corrective-action evidence review.</summary>
public sealed record CAVerificationDecision(Guid IntentId, CAVerificationReview Review, string Decision, string Reason)
{
    public void Require()
    {
        if (Review is null) throw new InvalidOperationException("Recorded corrective-action evidence is required.");
        Review.Require();
        if (IntentId == Guid.Empty || !Review.CanReviewIndependently || Decision is not ("VERIFIED" or "REJECTED") ||
            string.IsNullOrWhiteSpace(Reason) || Reason != Reason.Trim() || Reason.Length is < 20 or > 2000 ||
            Reason.Any(c => char.IsControl(c) && c is not ('\r' or '\n')) || JsonSerializer.Serialize(this).Length > 16384)
            throw new InvalidOperationException("An independent reviewer, explicit decision and bounded evidence are required.");
    }
}

public sealed class HSE_CA_VERIFICATION : ModelEntityBase
{
    [Key, Required, MaxLength(32)] public string HSE_CA_VERIFICATION_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_TENANT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string OWNER_COMPANY_BA_ID { get; set; } = "";
    [Required, MaxLength(128)] public string FIELD_ID { get; set; } = "";
    [Required, MaxLength(128)] public string INCIDENT_ID { get; set; } = "";
    [Required, MaxLength(128)] public string PROJECT_ID { get; set; } = "";
    [Required, MaxLength(64)] public string SOURCE_REVISION { get; set; } = "";
    [Required, MaxLength(16384)] public string COMMAND_JSON { get; set; } = "";
    [Required, MaxLength(64)] public string COMMAND_HASH { get; set; } = "";

    public static HSE_CA_VERIFICATION Create(CAVerificationDecision original)
    {
        original.Require(); var review = original.Review; var json = JsonSerializer.Serialize(original);
        return new() { HSE_CA_VERIFICATION_ID = original.IntentId.ToString("N"), OWNER_TENANT_ID = review.Context.Company.TenantId,
            OWNER_COMPANY_BA_ID = review.Context.Company.BusinessAssociateId, FIELD_ID = review.Context.FieldId!, INCIDENT_ID = review.IncidentId,
            PROJECT_ID = review.PlanId, SOURCE_REVISION = review.Revision, COMMAND_JSON = json,
            COMMAND_HASH = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(json))), ACTIVE_IND = "Y" };
    }
    public CAVerificationDecision Restore(CompanyScope company)
    {
        if (company is null || OWNER_TENANT_ID != company.TenantId || OWNER_COMPANY_BA_ID != company.BusinessAssociateId || ACTIVE_IND != "Y" ||
            string.IsNullOrEmpty(COMMAND_JSON) || COMMAND_JSON.Length > 16384 || COMMAND_HASH != Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(COMMAND_JSON))) ||
            ROW_CREATED_DATE is not { Kind: DateTimeKind.Utc } created || ROW_CHANGED_DATE is not { Kind: DateTimeKind.Utc } changed || changed < created || changed > DateTime.UtcNow)
            throw new InvalidOperationException("The recorded verification evidence is unavailable or corrupt.");
        var original = JsonSerializer.Deserialize<CAVerificationDecision>(COMMAND_JSON) ?? throw new InvalidOperationException("The recorded verification is unavailable.");
        original.Require(); var expected = Create(original);
        if (original.Review.Context.Company != company || ROW_CREATED_BY != original.Review.Context.ActorUserId || ROW_CHANGED_BY != ROW_CREATED_BY ||
            HSE_CA_VERIFICATION_ID != expected.HSE_CA_VERIFICATION_ID || FIELD_ID != expected.FIELD_ID || INCIDENT_ID != expected.INCIDENT_ID ||
            PROJECT_ID != expected.PROJECT_ID || SOURCE_REVISION != expected.SOURCE_REVISION)
            throw new InvalidOperationException("The recorded verification does not match its original request.");
        return original;
    }
}
