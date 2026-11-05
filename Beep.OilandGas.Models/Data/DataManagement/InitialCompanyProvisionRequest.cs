using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.Models.Data.DataManagement;

/// <summary>Creates an access boundary under a new or existing tenant. The business-associate reference is not PPDM source verification.</summary>
public sealed record InitialCompanyProvisionRequest(string TenantId, string TenantName, string BusinessAssociateId, string Reason, string IntentId)
{
    public void Require()
    {
        static bool Valid(string value, int max) => !string.IsNullOrWhiteSpace(value) && value.Length <= max && value == value.Trim() && !value.Any(char.IsControl);
        if (!Valid(TenantId, 64) || !Valid(TenantName, 256) || !Valid(BusinessAssociateId, 128) || !Valid(Reason, 1000) ||
            !Guid.TryParseExact(IntentId, "N", out var intent) || intent == Guid.Empty || intent.ToString("N") != IntentId)
            throw new ArgumentException("Enter a valid tenant ID, tenant name, company business associate ID and reason.");
    }
}

/// <summary>Historical request evidence; not proof of current access or PPDM source readiness.</summary>
public sealed record InitialCompanyProvisionReview(string IntentId, string Status, CompanyScope? Company);
