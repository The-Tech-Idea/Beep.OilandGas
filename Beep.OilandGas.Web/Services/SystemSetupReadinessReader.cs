using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Reads installation setup evidence through the API for the first-run guide.</summary>
public static class SystemSetupReadinessReader
{
    public static async Task<string> ReadAsync(ApiClient api)
    {
        var status = await api.GetAsync<SystemSetupRepositoryStatus>("/api/setup/system-repository/status");
        if (status?.Status != "SchemaReady") return status?.Status switch
        {
            "Unconfigured" => "Configure the system Beep connection.",
            "MigrationRequired" => "Review and apply the system setup migration.",
            _ => "System repository unavailable; check its connection."
        };
        return await HasDefaultsAsync(api)
            ? "Shared tool defaults are present; review each company environment."
            : "System schema ready; create the shared tool defaults.";
    }

    public static async Task<bool> HasDefaultsAsync(ApiClient api)
    {
        var defaults = await api.GetAsync<SystemSetupReadResult>("/api/setup/system-repository/records/TOOLS/DEFAULT");
        if (defaults is null || defaults.Category != "TOOLS" || defaults.Key != "DEFAULT")
            throw new InvalidOperationException("The system defaults response is unconfirmed.");
        if (defaults.Configuration is null) return false;
        SystemSetupIdentity.RequireValid(defaults.Configuration, "TOOLS", "DEFAULT");
        return true;
    }
}
