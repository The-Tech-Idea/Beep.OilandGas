using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Authentication;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Authentication;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>
/// Puts the OilGas repository's own roles and permissions on each request, for the account the identity server's client
/// library resolved (<c>party_id</c>), and says whether that account is active. The identity server authenticates only: a
/// role, permission or account claim any token carries counts for nothing and is removed from every identity.
/// </summary>
/// <remarks>
/// <para>
/// Chained after the library's <c>party_id</c> transformation. It replaced a transformation that resolved the account
/// from the token's issuer and subject itself — a second copy of the resolution, without the reseed repair — and turned a
/// deactivated account into an anonymous principal, so the Web could not tell "off" from "not signed in" (S3-06 §6).
/// </para>
/// <para>
/// Fails closed: an unresolved person, a machine, an account that is gone or off, and a failed read hold no role and are
/// not an active account, which the API's fallback policy requires.
/// </para>
/// </remarks>
public sealed class RepositoryRolesClaimsTransformation(
    IRepositoryAccessService access,
    ILogger<RepositoryRolesClaimsTransformation> logger) : IClaimsTransformation
{
    /// <summary>Marks a principal whose roles were resolved in this request.</summary>
    public const string ResolvedMarker = "oilgas:roles-resolved";

    /// <summary>Present, issued here, only on an active OilGas account's principal.</summary>
    public const string ActiveAccount = "oilgas:active-account";

    /// <summary>A permission granted through one of the account's roles.</summary>
    public const string Permission = "permission";

    private static readonly HashSet<string> AuthorizationClaimTypes =
    [
        "role", "roles", ClaimTypes.Role, Permission, "permissions", "elevated_permissions",
        ClaimTypes.NameIdentifier, ActiveAccount, ResolvedMarker
    ];

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated
            || identity.HasClaim(claim => claim.Type == ResolvedMarker && PartyIdClaims.IsIssuedHere(claim)))
        {
            return principal;
        }

        var granted = new List<Claim>();
        var userId = PartyIdClaims.Find(principal);

        if (userId is not null && !principal.IsMachine())
        {
            try
            {
                if (await access.GetAccessAsync(userId) is { IsActive: true } account)
                {
                    granted.Add(new Claim(ActiveAccount, "true"));
                    granted.AddRange(account.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
                    granted.AddRange(account.Permissions.Select(permission => new Claim(Permission, permission)));
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Broad on purpose: the repository's database provider decides what it throws. The request holds no role
                // and is not an active account, so the fallback policy refuses it; nothing is admitted on a guess.
                logger.LogError(exception, "The roles of OilGas account {UserId} could not be read; the request holds none.", userId);
            }
        }

        granted.Add(new Claim(ResolvedMarker, "true"));

        var primary = new ClaimsIdentity(
            identity.Claims.Where(claim => !AuthorizationClaimTypes.Contains(claim.Type)).Concat(granted),
            identity.AuthenticationType,
            "name",
            ClaimTypes.Role);

        var rebuilt = new ClaimsPrincipal(primary);
        foreach (var other in principal.Identities.Where(other => !ReferenceEquals(other, identity)))
        {
            rebuilt.AddIdentity(new ClaimsIdentity(
                other.Claims.Where(claim => !AuthorizationClaimTypes.Contains(claim.Type)),
                other.AuthenticationType,
                other.NameClaimType,
                other.RoleClaimType));
        }

        return rebuilt;
    }
}
