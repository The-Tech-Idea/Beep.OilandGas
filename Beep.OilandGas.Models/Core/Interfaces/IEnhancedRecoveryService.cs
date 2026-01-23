using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for enhanced oil recovery (EOR) operations.
    /// Provides EOR method selection, injection management, and recovery analysis.
    /// </summary>
    public interface IEnhancedRecoveryService
    {
        /// <summary>
        /// Analyzes EOR potential for a field or well.
        /// </summary>
        /// <param name="fieldId">Field identifier</param>
        /// <param name="eorMethod">EOR method to analyze</param>
        /// <returns>EOR analysis result</returns>
        Task<EnhancedRecoveryOperation> AnalyzeEORPotentialAsync(string fieldId, string eorMethod);

        /// <summary>
        /// Calculates recovery factor for EOR project.
        /// </summary>
        /// <param name="projectId">EOR project identifier</param>
        /// <returns>Recovery factor calculation result</returns>
        Task<EnhancedRecoveryOperation> CalculateRecoveryFactorAsync(string projectId);

        /// <summary>
        /// Manages injection well operations.
        /// </summary>
        /// <param name="injectionWellId">Injection well identifier</param>
        /// <param name="injectionRate">Injection rate</param>
        /// <returns>Injection operation result</returns>
        Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate);
    }
}





