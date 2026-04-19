using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.EnhancedRecovery;

namespace Beep.OilandGas.EnhancedRecovery.Services
{
    public partial class EnhancedRecoveryService
    {
        // Explicit implementations of Models.Core.Interfaces.IEnhancedRecoveryService
        Task<EnhancedRecoveryOperation> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.AnalyzeEORPotentialAsync(
            string fieldId, string eorMethod)
        {
            return Task.FromResult(new EnhancedRecoveryOperation());
        }

        Task<EnhancedRecoveryOperation> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.CalculateRecoveryFactorAsync(
            string projectId)
        {
            return Task.FromResult(new EnhancedRecoveryOperation());
        }

        Task<InjectionOperation> Beep.OilandGas.Models.Core.Interfaces.IEnhancedRecoveryService.ManageInjectionAsync(
            string injectionWellId, decimal injectionRate)
        {
            return Task.FromResult(new InjectionOperation());
        }
    }
}
