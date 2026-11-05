namespace TheTechIdea.Data.OilGas;

/// <summary>Selectors only. The API checks current membership; these headers grant no authority.</summary>
public static class CompanyRequestHeaders
{
    public const string Tenant = "X-OilGas-Tenant";
    public const string Company = "X-OilGas-Company";
}
