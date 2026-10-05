using Beep.OilandGas.Models.Core.Refusals;
using System.Security.Claims;
using Beep.Foundation.IdentityServer.Shared.Identity;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>
/// Who an API request acts for: the OilGas account this API resolved for the caller's token — the application's own key
/// (<c>party_id</c>), never the identity server's <c>sub</c>, and never anything the request itself names.
/// </summary>
/// <remarks>
/// <para>
/// Every recorded action is attributed through here. Controllers took a <c>userId</c> from the query string and preferred it
/// to the principal (<c>userId ?? "system"</c>), so any signed-in caller could record an action as anybody, and an action
/// with no user was recorded against a fabricated <c>"system"</c>; others read <c>sub</c> first, the identity server's
/// subject rather than the account (S3-06 §2).
/// </para>
/// <para>
/// The API's fallback policy admits only a request whose account was resolved, so an action reaching here without one is
/// a defect in its own authorization; it is refused (<see cref="RefusalKind.Forbidden"/>, answered 403 by the API's
/// refusal handler) rather than attributed to anybody. Work that genuinely acts for nobody —
/// a scheduled job, a seed — names itself from server code (<see cref="System"/>), never from a request.
/// </para>
/// </remarks>
public static class ActingUser
{
    /// <summary>The non-person actor for work the server does on its own account — never for a request.</summary>
    public const string System = "system";

    /// <summary>The acting account's id, or a <see cref="RefusalKind.Forbidden"/> refusal when the request has none.</summary>
    public static string ActingUserId(this ClaimsPrincipal? user) =>
        PartyIdClaims.Find(user)
        ?? throw RefusalException.Forbidden("The request is not signed in to an OilGas account.");

    /// <summary>The acting account's id when there is one — for code that serves a signed-out caller differently.</summary>
    public static string? FindActingUserId(this ClaimsPrincipal? user) => PartyIdClaims.Find(user);
}
