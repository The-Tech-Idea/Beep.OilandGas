using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.DTOs;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Interface for calculation services
    /// </summary>
    public interface ICalculationService
    {
        /// <summary>
        /// Performs Decline Curve Analysis
        /// </summary>
        Task<DCAResult> PerformDCAAnalysisAsync(DCARequest request);

        /// <summary>
        /// Performs Economic Analysis
        /// </summary>
        Task<EconomicAnalysisResult> PerformEconomicAnalysisAsync(EconomicAnalysisRequest request);

        /// <summary>
        /// Performs Nodal Analysis
        /// </summary>
        Task<NodalAnalysisResult> PerformNodalAnalysisAsync(NodalAnalysisRequest request);

        /// <summary>
        /// Performs Well Test Analysis
        /// </summary>
        Task<WellTestAnalysisResult> PerformWellTestAnalysisAsync(WellTestAnalysisCalculationRequest request);

        /// <summary>
        /// Performs Flash Calculation
        /// </summary>
        Task<FlashCalculationResult> PerformFlashCalculationAsync(FlashCalculationRequest request);

        /// <summary>
        /// Gets calculation results by ID
        /// </summary>
        Task<object?> GetCalculationResultAsync(string calculationId, string calculationType);

        /// <summary>
        /// Gets all calculation results for a well, pool, or field
        /// </summary>
        Task<List<object>> GetCalculationResultsAsync(string? wellId = null, string? poolId = null, string? fieldId = null, string? calculationType = null);
    }
}

