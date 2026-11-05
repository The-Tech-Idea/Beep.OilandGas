using System.Collections.ObjectModel;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

/// <summary>A validated, immutable access snapshot for one account and company selection.</summary>
public sealed class RepositoryCompanyContext
{
    internal RepositoryCompanyContext(string userId, RepositoryCompanyAccess access)
    {
        UserId = userId;
        Scope = access.Scope;
        Roles = Array.AsReadOnly(access.Roles.ToArray());
        Permissions = Array.AsReadOnly(access.Permissions.ToArray());
    }

    public string UserId { get; }
    public CompanyScope Scope { get; }
    public ReadOnlyCollection<string> Roles { get; }
    public ReadOnlyCollection<string> Permissions { get; }

    public RepositoryCompanyAccess ToAccess() => new(Scope, Roles.ToArray(), Permissions.ToArray());
}

/// <summary>No selection or claims are cached between requests. Every resolution checks current membership and roles.</summary>
public sealed class RepositoryCompanyContextResolver(IRepositoryAccessService access)
{
    public async Task<RepositoryCompanyContext?> ResolveAsync(string userId, CompanyScope selection,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentNullException.ThrowIfNull(selection);
        var result = await access.GetCompanyAccessAsync(userId, selection, cancellationToken);
        // A gateway must never substitute another company when the requested selection is unavailable.
        return result is null || result.Scope != selection ? null : new(userId, result);
    }
}
