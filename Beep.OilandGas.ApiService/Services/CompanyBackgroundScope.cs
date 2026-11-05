using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>A queued request must regain current membership and its captured grants before business execution.</summary>
public static class CompanyBackgroundScope
{
    public static async Task RestoreAsync(RepositoryCompanyContext captured, RepositoryCompanyContextResolver resolver,
        RepositoryTenantContext tenant, CancellationToken token)
    {
        var current = await resolver.ResolveAsync(captured.UserId, captured.Scope, token);
        if (!HasCapturedGrants(captured, current))
            throw new UnauthorizedAccessException("Queued company access is no longer available.");
        tenant.SelectCompany(current!);
    }

    public static bool SameOwner(RepositoryCompanyContext? captured, RepositoryCompanyContext? current) =>
        captured is not null && current is not null && captured.Scope == current.Scope && captured.UserId == current.UserId;

    public static bool HasCapturedGrants(RepositoryCompanyContext captured, RepositoryCompanyContext? current) =>
        SameOwner(captured, current) && !captured.Roles.Except(current!.Roles, StringComparer.Ordinal).Any() &&
        !captured.Permissions.Except(current.Permissions, StringComparer.Ordinal).Any();
}
