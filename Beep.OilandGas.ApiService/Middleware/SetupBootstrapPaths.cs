using Microsoft.AspNetCore.Http;

namespace Beep.OilandGas.ApiService.Middleware;

/// <summary>Only endpoints that independently require the temporary setup key.</summary>
public static class SetupBootstrapPaths
{
    public static bool IsKeyProtected(PathString path) =>
        path.StartsWithSegments("/api/setup/bootstrap/drivers", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWithSegments("/api/setup/bootstrap/repository", StringComparison.OrdinalIgnoreCase);
}
