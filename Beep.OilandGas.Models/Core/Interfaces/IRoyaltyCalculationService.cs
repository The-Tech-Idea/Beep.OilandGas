using Beep.OilandGas.Models.Data.Compliance;

namespace Beep.OilandGas.Models.Core.Interfaces;

public interface IRoyaltyCalculationService
{
    Task<RoyaltySummary> CalculateUSARoyaltyAsync(
        string fieldId, int year, int month, string userId);

    Task<RoyaltySummary> CalculateAlbertaCrownRoyaltyAsync(
        string fieldId, int year, int quarter, string userId);

    Task<List<RoyaltyVariance>> GetVarianceHistoryAsync(string fieldId, int year);
}
