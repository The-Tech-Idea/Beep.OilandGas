#if WORKFLOW_INBOX_FOCUSED
// Explicit resolved-account/sign-in URL boundary for the focused landing composition.
// This does not certify shared IdentityServer claim provenance or the live sign-in flow.
namespace Beep.Foundation.IdentityServer.Shared.Identity
{
    public static class PartyIdClaims
    {
        public static string? Find(System.Security.Claims.ClaimsPrincipal user) => user.FindFirst("party_id")?.Value;
    }
}
namespace Beep.OilandGas.Web.Services
{
    public static class BeepClientEndpoints
    {
        public static string LoginWithReturn(string path) => "/account/login?returnUrl="+System.Uri.EscapeDataString(path);
        public static string RegisterWithReturn(string path) => "/account/register?returnUrl="+System.Uri.EscapeDataString(path);
    }
}
#endif
