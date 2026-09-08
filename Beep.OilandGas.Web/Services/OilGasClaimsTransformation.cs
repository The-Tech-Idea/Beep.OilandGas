using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace Beep.OilandGas.Web.Services;

public sealed class OilGasClaimsTransformation(
    RepositoryAccountClient client,
    TokenProvider tokens,
    IHttpContextAccessor accessor,
    ILogger<OilGasClaimsTransformation> logger) : IClaimsTransformation
{
    private readonly object _requestCacheKey = new();

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true) return principal;
        var context = accessor.HttpContext;
        if (context?.Items[_requestCacheKey] is ClaimsPrincipal cached) return cached;
        var subject = principal.FindFirstValue("sub");
        var token = context?.Items["OilGas.AccessToken"] as string
            ?? (subject is null ? null : tokens.GetUserToken(subject));
        if (string.IsNullOrWhiteSpace(token)) return new ClaimsPrincipal(new ClaimsIdentity());
        try
        {
            var access = await client.GetAccessAsync(token, context?.RequestAborted ?? default);
            if (!access.IsActive) return new ClaimsPrincipal(new ClaimsIdentity());
            var claims = principal.Claims.Where(claim => claim.Type != "role" && claim.Type != "roles"
                && claim.Type != ClaimTypes.Role && claim.Type != "permission"
                && claim.Type != "permissions" && claim.Type != "elevated_permissions"
                && claim.Type != ClaimTypes.NameIdentifier && claim.Type != "oilgas:roles-resolved");
            var identity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType, "name", ClaimTypes.Role);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, access.UserId));
            foreach (var role in access.Roles) identity.AddClaim(new Claim(ClaimTypes.Role, role));
            foreach (var permission in access.Permissions) identity.AddClaim(new Claim("permission", permission));
            identity.AddClaim(new Claim("oilgas:roles-resolved", "true", ClaimValueTypes.Boolean, "OilGas"));
            var result = new ClaimsPrincipal(identity);
            if (context is not null) context.Items[_requestCacheKey] = result;
            return result;
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "OilGas role resolution failed; denying access");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
