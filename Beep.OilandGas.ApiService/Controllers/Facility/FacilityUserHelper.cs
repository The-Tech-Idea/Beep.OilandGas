using System.Security.Claims;

namespace Beep.OilandGas.ApiService.Controllers.Facility;

/// <summary>
/// Resolves API user id for PPDM row audit columns (JWT sub / name identifier).
/// </summary>
internal static class FacilityUserHelper
{
    public static string ResolveUserId(ClaimsPrincipal user) =>
        user.FindFirst("sub")?.Value
        ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? "SYSTEM";
}
