using System.Security.Claims;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;

namespace Beep.OilandGas.ApiService.Services;

public sealed class RepositoryClaimsTransformation(
    IRepositoryAccessService access,
    ILogger<RepositoryClaimsTransformation> logger) : IClaimsTransformation
{
    private ClaimsPrincipal? _source;
    private ClaimsPrincipal? _resolved;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (ReferenceEquals(principal, _source) || ReferenceEquals(principal, _resolved))
            return _resolved!;
        if (principal.Identity?.IsAuthenticated != true) return principal;

        var issuer = principal.FindFirstValue("iss");
        var subject = principal.FindFirstValue("sub");
        if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(subject))
            return new ClaimsPrincipal(new ClaimsIdentity());

        // External tokens authenticate only. Never carry their authorization claims forward.
        var claims = principal.Claims.Where(claim => claim.Type != "role" && claim.Type != "roles"
            && claim.Type != ClaimTypes.Role && claim.Type != "permission"
            && claim.Type != "permissions" && claim.Type != "elevated_permissions"
            && claim.Type != ClaimTypes.NameIdentifier && claim.Type != "oilgas:roles-resolved");
        var identity = new ClaimsIdentity(claims, principal.Identity.AuthenticationType, "name", ClaimTypes.Role);
        try
        {
            var user = await access.GetAccessAsync(issuer, subject);
            if (user is { IsActive: false }) return new ClaimsPrincipal(new ClaimsIdentity());
            if (user is not null)
            {
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId));
                foreach (var role in user.Roles) identity.AddClaim(new Claim(ClaimTypes.Role, role));
                foreach (var permission in user.Permissions) identity.AddClaim(new Claim("permission", permission));
            }
            identity.AddClaim(new Claim("oilgas:roles-resolved", "true", ClaimValueTypes.Boolean, "OilGas"));
            _source = principal;
            return _resolved = new ClaimsPrincipal(identity);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Repository role resolution failed; denying authentication");
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
    }
}
