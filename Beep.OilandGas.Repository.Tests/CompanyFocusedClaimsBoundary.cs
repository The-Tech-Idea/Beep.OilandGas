// Focused test-only resolved-account seam. Production uses the shared identity library;
// this does not certify token validation, PartyIdClaims or the OIDC role bridge.
namespace Beep.OilandGas.ApiService.Services;
internal static class CompanyFocusedClaimsBoundary
{
    public static string? FindActingUserId(this System.Security.Claims.ClaimsPrincipal user)=>user.FindFirst("party_id")?.Value;
    public static string ActingUserId(this System.Security.Claims.ClaimsPrincipal user)=>
        user.FindFirst("party_id")?.Value ?? throw new UnauthorizedAccessException("A resolved fixture account is required.");
}
