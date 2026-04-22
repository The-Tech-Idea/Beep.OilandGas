using System.Threading.Tasks;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using Beep.OilandGas.Models.Data.Operations;

namespace Beep.OilandGas.Web.Services
{
    public interface IEnhancedRecoveryServiceClient
    {
        Task<EnhancedRecoveryOperation> AnalyzeEORPotentialAsync(string fieldId, string eorMethod);
        Task<EnhancedRecoveryOperation> CalculateRecoveryFactorAsync(string operationId);
        Task<EOREconomicAnalysis> AnalyzeEconomicsAsync(AnalyzeEOREconomicsRequest request);
        Task<List<InjectionOperation>> GetInjectionOperationsAsync(string? wellUWI = null);
        Task<InjectionOperation> ManageInjectionAsync(string injectionWellId, decimal injectionRate);
    }
}