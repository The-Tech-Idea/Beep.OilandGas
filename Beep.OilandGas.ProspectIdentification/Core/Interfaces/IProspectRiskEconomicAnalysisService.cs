// Physical location: Beep.OilandGas.ProspectIdentification — namespace matches Models.Core.Interfaces.
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Prospect-level risk and economic screening (deterministic helpers).
    /// Implemented by <see cref="Beep.OilandGas.ProspectIdentification.Services.ProspectIdentificationService"/>.
    /// </summary>
    public interface IProspectRiskEconomicAnalysisService
    {
        Task<ProspectRiskAnalysisResult> PerformRiskAssessmentAsync(
            string prospectId,
            string assessedBy,
            Dictionary<string, decimal> riskScores);

        Task<EconomicViabilityAnalysis> AnalyzeEconomicViabilityAsync(
            string prospectId,
            decimal estimatedOil,
            decimal estimatedGas,
            decimal capitalCost,
            decimal operatingCost,
            decimal oilPrice,
            decimal gasPrice);
    }
}
