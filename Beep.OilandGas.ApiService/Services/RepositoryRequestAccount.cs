using System.Security.Claims;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Http;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>The auth owner's resolved account, with fresh app-owned authority for account operations.</summary>
public sealed class RepositoryRequestAccount(IHttpContextAccessor http, Func<ClaimsPrincipal, string?> resolveAccount,
    IRepositoryAccessService access, RepositoryRoleAssignmentService assignments)
{
    public async Task<string> RequireAsync(CancellationToken token = default)
    {
        var principal = http.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
            throw RefusalException.Forbidden("An authenticated OilGas account is required.");
        var actor = resolveAccount(principal);
        if (string.IsNullOrWhiteSpace(actor) || !await access.IsAccountActiveAsync(actor, token))
            throw RefusalException.Forbidden("An active resolved OilGas account is required.");
        return actor;
    }

    public async Task<string> RequireAdministratorAsync(CancellationToken token = default)
    {
        var actor = await RequireAsync(token);
        await assignments.RequireRepositoryAdministratorAsync(actor, token);
        return actor;
    }

    public async Task<string> RequireSelfOrAdministratorAsync(string subject, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);
        var actor = await RequireAsync(token);
        if (!string.Equals(actor, subject, StringComparison.Ordinal))
            await assignments.RequireRepositoryAdministratorAsync(actor, token);
        return actor;
    }

    public async Task<string> RequireSelfAsync(string subject, CancellationToken token = default)
    {
        var actor = await RequireAsync(token);
        if (!string.Equals(actor, subject, StringComparison.Ordinal))
            throw RefusalException.Forbidden("This operation belongs to the signed-in account.");
        return actor;
    }
}
