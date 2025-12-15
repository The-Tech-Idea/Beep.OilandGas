using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Routing;

/// <summary>
/// Extension methods for adding login and logout endpoints.
/// Handles the OIDC challenge and sign-out flows.
/// </summary>
internal static class LoginLogoutEndpointRouteBuilderExtensions
{
    // Using the same scheme name as configured in Program.cs
    private const string OIDC_SCHEME = "oidc";

    internal static IEndpointConventionBuilder MapLoginAndLogout(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/authentication");

        // Login endpoint - challenges the OIDC provider
        group.MapGet("/login", (string? returnUrl) => TypedResults.Challenge(
            GetAuthProperties(returnUrl),
            [OIDC_SCHEME]))
            .AllowAnonymous();

        // Logout endpoint - signs out of both Cookie and OIDC handlers
        group.MapPost("/logout", ([FromForm] string? returnUrl) => TypedResults.SignOut(
            GetAuthProperties(returnUrl),
            [CookieAuthenticationDefaults.AuthenticationScheme, OIDC_SCHEME]));

        return group;
    }

    private static AuthenticationProperties GetAuthProperties(string? returnUrl)
    {
        const string pathBase = "/";

        // Prevent open redirects.
        if (string.IsNullOrEmpty(returnUrl))
        {
            returnUrl = pathBase;
        }
        else if (!Uri.IsWellFormedUriString(returnUrl, UriKind.Relative))
        {
            returnUrl = new Uri(returnUrl, UriKind.Absolute).PathAndQuery;
        }
        else if (returnUrl[0] != '/')
        {
            returnUrl = $"{pathBase}{returnUrl}";
        }

        return new AuthenticationProperties { RedirectUri = returnUrl };
    }
}

