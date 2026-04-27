// Physical location: Beep.OilandGas.ProspectIdentification (assembly) — namespace matches Models.Core.Interfaces
// so ApiService and other hosts resolve the contract alongside other cross-cutting service interfaces.
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Live prospect identification API contract (evaluate, list, create, rank).
    /// Implemented by <see cref="Beep.OilandGas.ProspectIdentification.Services.ProspectIdentificationService"/> together with
    /// <see cref="IProspectTechnicalMaturationService"/>, <see cref="IProspectRiskEconomicAnalysisService"/>, and
    /// <see cref="IProspectPortfolioOptimizationService"/> (same instance in DI).
    /// </summary>
    public interface IProspectIdentificationService
    {
        /// <summary>
        /// Evaluates a prospect for drilling potential.
        /// </summary>
        /// <param name="prospectId">Prospect identifier</param>
        /// <returns>Prospect evaluation result</returns>
        Task<ProspectEvaluation> EvaluateProspectAsync(string prospectId);

        /// <summary>
        /// Gets all prospects with optional filters.
        /// </summary>
        /// <param name="filters">Optional filters</param>
        /// <returns>List of prospects</returns>
        Task<List<Prospect>> GetProspectsAsync(Dictionary<string, string>? filters = null);

        /// <summary>
        /// Creates a new prospect.
        /// </summary>
        /// <param name="prospect">Prospect data</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Created prospect identifier</returns>
        Task<string> CreateProspectAsync(Prospect prospect, string userId);

        /// <summary>
        /// Ranks prospects based on evaluation criteria.
        /// </summary>
        /// <param name="prospectIds">List of prospect identifiers</param>
        /// <param name="rankingCriteria">Ranking criteria</param>
        /// <returns>Ranked list of prospects</returns>
        Task<List<ProspectRanking>> RankProspectsAsync(List<string> prospectIds, Dictionary<string, decimal> rankingCriteria);
    }
}

