using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.AspNetCore.Components.Web;
using System.Security.Claims;

namespace Beep.OilandGas.Web.Components.Account;

/// <summary>
/// AuthenticationStateProvider that works with OIDC cookies.
/// Revalidates authentication state periodically to keep it in sync with the cookie.
/// </summary>
public class OidcAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly ILogger<OidcAuthenticationStateProvider> _logger;

    public OidcAuthenticationStateProvider(
        ILoggerFactory loggerFactory)
        : base(loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<OidcAuthenticationStateProvider>();
    }

    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(5);

    protected override Task<bool> ValidateAuthenticationStateAsync(
        AuthenticationState authenticationState,
        CancellationToken cancellationToken)
    {
        // For OIDC, we trust the cookie - if it exists and is valid, the user is authenticated
        var user = authenticationState.User;
        var isAuthenticated = user?.Identity?.IsAuthenticated ?? false;
        
        if (isAuthenticated)
        {
            _logger.LogDebug("Authentication state validated for user: {UserId}", 
                user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown");
        }
        
        return Task.FromResult(isAuthenticated);
    }
}

