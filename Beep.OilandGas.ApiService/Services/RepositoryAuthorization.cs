using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Beep.OilandGas.ApiService.Services;

public static class RepositoryAuthorization
{
    public const string ExternalAccount = "Repository.ExternalAccount";

    public static void Configure(AuthorizationOptions options)
    {
        // NameIdentifier is supplied only by the repository claims transformation.
        var registeredAccount = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .RequireClaim(ClaimTypes.NameIdentifier)
            .Build();
        options.DefaultPolicy = registeredAccount;
        options.FallbackPolicy = registeredAccount;
        options.AddPolicy(ExternalAccount, policy => policy
            .RequireAuthenticatedUser().RequireClaim("iss").RequireClaim("sub"));
        options.AddPolicy("Admin.ManageUsers", policy => policy
            .RequireAuthenticatedUser().RequireClaim(ClaimTypes.NameIdentifier).RequireRole("Administrator"));
        options.AddPolicy("Admin.AssignRoles", policy => policy
            .RequireAuthenticatedUser().RequireClaim(ClaimTypes.NameIdentifier).RequireRole("Administrator"));
    }
}
