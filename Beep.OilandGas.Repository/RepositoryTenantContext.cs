namespace Beep.OilandGas.Repository;

/// <summary>One tenant selection for one DI scope. API/worker boundaries validate authority before selecting it.</summary>
public sealed class RepositoryTenantContext
{
    public string? TenantId { get; private set; }
    public RepositoryCompanyContext? Company { get; private set; }

    public void SelectCompany(RepositoryCompanyContext company)
    {
        ArgumentNullException.ThrowIfNull(company);
        if (Company is not null && (Company.Scope != company.Scope || Company.UserId != company.UserId))
            throw new InvalidOperationException("A request cannot change its company or account.");
        SelectTenant(company.Scope.TenantId);
        Company = company;
    }

    public void SelectTenant(string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        if (TenantId is not null && !string.Equals(TenantId, tenantId, StringComparison.Ordinal))
            throw new InvalidOperationException("A request cannot change its tenant.");
        TenantId = tenantId;
    }
}
