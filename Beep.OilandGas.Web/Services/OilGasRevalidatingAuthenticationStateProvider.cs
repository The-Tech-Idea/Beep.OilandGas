using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace Beep.OilandGas.Web.Services;

public class OilGasRevalidatingAuthenticationStateProvider(
    ILoggerFactory loggerFactory, RepositoryAccountClient repository, TokenProvider tokens)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<OilGasRevalidatingAuthenticationStateProvider>();
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(1);

    protected override async Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        var user = authenticationState.User;
        var subject = user.FindFirstValue("sub");
        var localId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (user.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(subject)
            || string.IsNullOrWhiteSpace(localId)) return false;
        // Circuit revalidation uses its captured subject, never a later request's HttpContext.
        var token = tokens.GetUserToken(subject);
        if (string.IsNullOrWhiteSpace(token)) return false;
        try
        {
            var access = await repository.GetAccessAsync(token, cancellationToken);
            return access.IsActive && string.Equals(access.UserId, localId, StringComparison.Ordinal)
                && user.FindAll(ClaimTypes.Role).Select(c => c.Value).ToHashSet(StringComparer.Ordinal).SetEquals(access.Roles)
                && user.FindAll("permission").Select(c => c.Value).ToHashSet(StringComparer.Ordinal).SetEquals(access.Permissions);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Repository access revalidation failed; invalidating circuit authentication");
            return false;
        }
    }
}
