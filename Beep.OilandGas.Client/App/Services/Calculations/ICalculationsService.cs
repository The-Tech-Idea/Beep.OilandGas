using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.EconomicAnalysis;
namespace Beep.OilandGas.Client.App.Services.Calculations
{
    /// <summary>
    /// Service interface for all Calculation operations
    /// </summary>
    public interface ICalculationsService
    {
        #region Flash Calculation

        Task<FlashResult> PerformIsothermalFlashAsync(FLASH_CONDITIONS request, CancellationToken cancellationToken = default);
        Task<List<FlashResult>> PerformMultiStageFlashAsync(FLASH_CONDITIONS request, CancellationToken cancellationToken = default);
        Task<FLASH_CALCULATION_RESULT> SaveFlashResultAsync(FLASH_CALCULATION_RESULT result, string? userId = null, CancellationToken cancellationToken = default);
        Task<List<FLASH_CALCULATION_RESULT>> GetFlashHistoryAsync(string compositionId, CancellationToken cancellationToken = default);

        #endregion

        #region Nodal Analysis

        Task<OperatingPoint> PerformNodalAnalysisAsync(ReservoirProperties request, CancellationToken cancellationToken = default);
        Task<NODAL_ANALYSIS_RESULT> OptimizeNodalAnalysisAsync(NODAL_RESERVOIR_PROPERTIES request, CancellationToken cancellationToken = default);
        Task<NODAL_ANALYSIS_RESULT> SaveNodalAnalysisResultAsync(NODAL_ANALYSIS_RESULT result, string? userId = null, CancellationToken cancellationToken = default);
        Task<List<NODAL_ANALYSIS_RESULT>> GetNodalAnalysisHistoryAsync(string wellId, CancellationToken cancellationToken = default);

        #endregion

        #region Economic Analysis

        Task<decimal> CalculateNPVAsync(List<CashFlow> request, CancellationToken cancellationToken = default);
        Task<decimal> CalculateIRRAsync(List<CashFlow> request, CancellationToken cancellationToken = default);
        Task<EconomicResult> PerformEconomicAnalysisAsync(List<CashFlow> request, CancellationToken cancellationToken = default);
        Task<List<NPV_PROFILE_POINT>> GenerateNPVProfileAsync(List<ECONOMIC_CASH_FLOW> request, CancellationToken cancellationToken = default);
        Task<ECONOMIC_ANALYSIS_RESULT> SaveEconomicResultAsync(ECONOMIC_ANALYSIS_RESULT result, string? userId = null, CancellationToken cancellationToken = default);
        Task<ECONOMIC_ANALYSIS_RESULT> GetEconomicResultAsync(string resultId, CancellationToken cancellationToken = default);

        #endregion
    }
}
