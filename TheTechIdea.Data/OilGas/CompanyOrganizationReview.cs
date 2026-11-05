namespace TheTechIdea.Data.OilGas;

/// <summary>A bounded observation of the selected company's PPDM business-associate record.</summary>
public sealed record CompanyOrganizationReview(CompanyScope Company, string BindingRevision, string Status);
