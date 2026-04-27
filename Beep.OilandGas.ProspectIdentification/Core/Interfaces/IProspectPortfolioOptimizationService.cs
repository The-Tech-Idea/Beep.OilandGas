// Physical location: Beep.OilandGas.ProspectIdentification — namespace matches Models.Core.Interfaces.
using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.ProspectIdentification;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Portfolio optimization on ranked prospects (works alongside <see cref="IProspectIdentificationService.RankProspectsAsync"/>).
    /// Implemented by <see cref="Beep.OilandGas.ProspectIdentification.Services.ProspectIdentificationService"/>.
    /// </summary>
    public interface IProspectPortfolioOptimizationService
    {
        Task<PortfolioOptimizationResult> OptimizePortfolioAsync(
            List<ProspectRanking> rankedProspects,
            decimal riskTolerance,
            decimal capitalBudget);
    }
}
