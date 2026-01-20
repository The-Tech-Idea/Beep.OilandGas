using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for prospect identification operations.
    /// Provides prospect evaluation, ranking, and risk assessment.
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

    /// <summary>
    /// DTO for prospect ranking.
    /// </summary>
    public class ProspectRanking
    {
        public string ProspectId { get; set; } = string.Empty;
        public string ProspectName { get; set; } = string.Empty;
        public int Rank { get; set; }
        public decimal Score { get; set; }
    }
}




