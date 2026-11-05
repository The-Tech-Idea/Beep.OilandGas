namespace TheTechIdea.Data.OilGas;

/// <summary>Shared routing classification; selectors remain subject to API authorization.</summary>
public static class CompanyRequestPolicy
{
    private static bool Under(string path, string prefix) => path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);

    public static bool IsControlPlane(string path) => Under(path, "/api/auth/repository") ||
        (Under(path, "/api/identity") && !Under(path, "/api/identity/security")) || Under(path, "/api/setup") || Under(path, "/api/ppdm39/setup");

    public static bool RequiresCompany(string path) => !IsControlPlane(path) &&
        (Under(path, "/api") || Under(path, "/hubs") || path.Equals("/progressHub", StringComparison.OrdinalIgnoreCase));
}
