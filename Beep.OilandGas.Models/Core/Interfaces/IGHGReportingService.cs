using Beep.OilandGas.Models.Data.Compliance;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IGHGReportingService
{
    Task<GHGEmissionReport> GenerateAnnualReportAsync(
        string fieldId, int year, string jurisdiction, string userId);

    Task<List<EmissionSourceLine>> GetEmissionSourcesAsync(string fieldId, int year);

    Task<double> GetTotalEmissionsAsync(
        string fieldId, int year, string? jurisdiction = null);
}
