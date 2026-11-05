using System.Security.Claims;
using Beep.OilandGas.Repository;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>Restores an immutable handshake scope only after fresh application membership resolution.</summary>
public sealed class CompanyHubScope(RepositoryTenantContext tenant, RepositoryCompanyContextResolver companies,
    Func<ClaimsPrincipal, string?> resolveAccount)
{
    public async Task<RepositoryCompanyContext?> RestoreAsync(ClaimsPrincipal? principal, RepositoryCompanyContext? handshake,
        CancellationToken token = default)
    {
        if (principal?.Identity?.IsAuthenticated != true || handshake is null || resolveAccount(principal) != handshake.UserId) return null;
        var current = await companies.ResolveAsync(handshake.UserId, handshake.Scope, token);
        if (current is null) return null;
        tenant.SelectCompany(current);
        return current;
    }

    public async Task<bool> CanDeliverAsync(ClaimsPrincipal? principal, RepositoryCompanyContext? handshake, string group,
        string? requiredRole, Func<CompanyHubChannel, RepositoryCompanyContext, Task<bool>> resourceAccess,
        CancellationToken token = default)
    {
        var channel = CompanyHubGroups.Parse(group);
        if (handshake is null || channel is null || channel.Scope != handshake.Scope) return false;
        var current = await RestoreAsync(principal, handshake, token);
        if (current is null || (requiredRole is not null && !current.Roles.Contains(requiredRole, StringComparer.Ordinal))) return false;
        if (channel.Kind == CompanyChannelKind.User) return channel.Resource == current.UserId;
        return await resourceAccess(channel, current);
    }
}
