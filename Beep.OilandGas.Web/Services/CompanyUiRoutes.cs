namespace Beep.OilandGas.Web.Services;

/// <summary>The existing repository/account setup surfaces retain repository roles; business pages use company roles.</summary>
public static class CompanyUiRoutes
{
    public static bool IsRepositorySurface(string path)
    {
        path = path.TrimEnd('/');
        return Under(path, "/account") || Under(path, "/admin") ||
        Under(path, "/setup") || Under(path, "/ppdm39/setup") || Under(path, "/ppdm39/create-database") ||
        path.Equals("/ppdm39/data-management/schema", StringComparison.OrdinalIgnoreCase) ||
        path.Equals("/ppdm39/database-management", StringComparison.OrdinalIgnoreCase);
    }

    private static bool Under(string path, string prefix) => path.Equals(prefix, StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase);
}
