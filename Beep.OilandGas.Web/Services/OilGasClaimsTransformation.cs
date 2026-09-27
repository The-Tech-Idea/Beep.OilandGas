using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;
using Microsoft.AspNetCore.Authentication;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Puts the OilGas repository's own roles and permissions on each request, as the API reports them for the signed-in
/// person, with the account's id as <c>party_id</c> — the claims <c>[Authorize(Roles = …)]</c> and
/// <c>&lt;AuthorizeView&gt;</c> read. The identity server authenticates only: a role, permission or account claim its
/// tokens carry counts for nothing and is removed from every identity.
/// </summary>
/// <remarks>
/// <para>
/// The Web holds no user store, so it asks the API (<see cref="RepositoryAccountClient"/>), with the person's own access
/// token from the identity server's client library. It read that token from a dictionary the library no longer has.
/// </para>
/// <para>
/// Fails closed: an account the API refuses or could not answer about holds no role and no key for this request.
/// Whether the person may stay signed in is <see cref="OilGasAccountAdmission"/>'s question, asked by the library.
/// </para>
/// </remarks>
public sealed class OilGasClaimsTransformation(RepositoryAccountClient repository) : IClaimsTransformation
{
    /// <summary>Marks a principal whose roles were resolved in this request.</summary>
    public const string ResolvedMarker = "oilgas:roles-resolved";

    private static readonly HashSet<string> AuthorizationClaimTypes =
    [
        "role", "roles", ClaimTypes.Role, "permission", "permissions", "elevated_permissions",
        ClaimTypes.NameIdentifier, PartyIdClaimsTransformation<string>.ClaimType, ResolvedMarker
    ];

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated
            || identity.HasClaim(claim => claim.Type == ResolvedMarker && PartyIdClaims.IsIssuedHere(claim)))
        {
            return principal;
        }

        var granted = new List<Claim>();
        var answer = await repository.GetAccountAsync(principal);
        if (answer is { State: RepositoryAccountState.Active, Access: { } access })
        {
            granted.Add(new Claim(PartyIdClaimsTransformation<string>.ClaimType, access.UserId));
            granted.AddRange(access.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            granted.AddRange(access.Permissions.Select(permission => new Claim("permission", permission)));
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
