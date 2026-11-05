using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Compliance;

namespace Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;

/// <summary>Reporting is unavailable until company-scoped activity and reviewed emission factors are configured.</summary>
public sealed class GHGReportingService : IGHGReportingService
{
    private static NotSupportedException Unavailable() => new("GHG reporting requires verified company-scoped activity data and emission factors.");
    public Task<GHGEmissionReport> GenerateAnnualReportAsync(string fieldId, int year, string jurisdiction, string userId) => throw Unavailable();
    public Task<List<EmissionSourceLine>> GetEmissionSourcesAsync(string fieldId, int year) => throw Unavailable();
    public Task<double> GetTotalEmissionsAsync(string fieldId, int year, string? jurisdiction) => throw Unavailable();
}
