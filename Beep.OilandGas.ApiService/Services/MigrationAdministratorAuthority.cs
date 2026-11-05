using System.Security.Claims;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Rechecks the resolved repository administrator immediately before migration dispatch.</summary>
public static class MigrationAdministratorAuthority
{
    public static async Task<string> RequireAsync(ClaimsPrincipal user, IRepositoryAccessService access, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(access);
        var actor = user.ActingUserId();
        var current = await access.GetAccessAsync(actor, token);
        if (current is not { IsActive: true } || !current.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A current repository administrator is required for migration.");
        return actor;
    }
}
