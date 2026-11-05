namespace Beep.OilandGas.PPDM39.DataManagement.Models;

public interface ICompanyFieldEligibility
{
    // Canonical PPDM data entitlement caps the app grant; null means no eligible field access.
    Task<string?> GetMaximumLevelAsync(string fieldId);
}

// Deployment-owned interpretation of each company's existing PPDM reference vocabulary and date zone.
// Empty configuration grants nothing; never infer codes from display names or a financial interest percentage.
public sealed class PPDMFieldEligibilityConfiguration
{
    public List<CompanyFieldEligibilityRules> Companies { get; set; } = [];
}

public sealed class CompanyFieldEligibilityRules
{
    public string TenantId { get; set; } = "";
    public string BusinessAssociateId { get; set; } = "";
    public string TimeZoneId { get; set; } = "";
    public string EntitlementType { get; set; } = "";
    public string ComponentType { get; set; } = "";
    public string ComponentAccessConditionCode { get; set; } = "";
    public string InterestReviewComponentType { get; set; } = "";
    public string InterestReviewSetType { get; set; } = "";
    public string LeaseLandRightSubtype { get; set; } = "";
    public string LeaseCompanyComponentType { get; set; } = "";
    public string LeaseStatusType { get; set; } = "";
    public List<HseCaseRestrictionReadRule> HseCaseReadRestrictions { get; set; } = [];
    public Dictionary<string, string> AccessTypes { get; set; } = [];
}

// Explicit deployment-owned review of a recorded definition, qualified by the selected physical database.
// Permissions are resolved from the existing app RBAC owner; contacts and token roles confer no access.
public sealed class HseCaseRestrictionReadRule
{
    public string DatabaseFingerprint { get; set; } = "";
    public string RestrictionId { get; set; } = "";
    public decimal RestrictionVersion { get; set; }
    public string DefinitionFingerprint { get; set; } = "";
    public string RequiredPermission { get; set; } = "";
}
