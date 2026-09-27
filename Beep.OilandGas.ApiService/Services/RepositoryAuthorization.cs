using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Microsoft.AspNetCore.Authorization;

namespace Beep.OilandGas.ApiService.Services;

public static class RepositoryAuthorization
{
    /// <summary>
    /// A signed-in caller, whatever the state of their OilGas account — for <c>/api/auth/repository/me</c>, which answers
    /// that state (an account that is off is told so, not refused as if signed out).
    /// </summary>
    public const string SignedIn = "Repository.SignedIn";

    public static void Configure(AuthorizationOptions options)
    {
        // Every endpoint, unless it says otherwise: a caller whose OilGas account this API resolved (party_id, issued here
        // by the identity server's client library) and which is active (RepositoryRolesClaimsTransformation).
        var activeAccount = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireAssertion(context => IsActiveAccount(context.User))
            .Build();
        options.DefaultPolicy = activeAccount;
        options.FallbackPolicy = activeAccount;
        options.AddPolicy(SignedIn, policy => policy.RequireAuthenticatedUser());
        options.AddPolicy("Admin.ManageUsers", policy => policy.Combine(activeAccount).RequireRole("Administrator"));
        options.AddPolicy("Admin.AssignRoles", policy => policy.Combine(activeAccount).RequireRole("Administrator"));
    }

    /// <summary>Whether <paramref name="user"/> is an active OilGas account — both claims issued by this API, never a token.</summary>
    public static bool IsActiveAccount(ClaimsPrincipal user) =>
        PartyIdClaims.Find(user) is not null
        && user.HasClaim(claim => claim.Type == RepositoryRolesClaimsTransformation.ActiveAccount && PartyIdClaims.IsIssuedHere(claim));
}
