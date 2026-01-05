using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Beep.OilandGas.Models.OilProperties;
using Beep.OilandGas.Models.GasProperties;
using Beep.OilandGas.Models.HeatMap;
using Beep.OilandGas.Models.Data.OilProperties;
using Beep.OilandGas.Models.Data.GasProperties;
using Beep.OilandGas.Models.Data.HeatMap;
using Beep.OilandGas.Models.Data.Common;

namespace Beep.OilandGas.Client.App.Services.Properties
{
    /// <summary>
    /// Service interface for all Properties operations (Oil, Gas, HeatMap)
    /// </summary>
    public interface IPropertiesService
    {
        #region Oil Properties

        Task<decimal> CalculateOilFormationVolumeFactorAsync(OilPropertyConditions request, CancellationToken cancellationToken = default);
        Task<decimal> CalculateOilDensityAsync(OilPropertyConditions request, CancellationToken cancellationToken = default);
        Task<decimal> CalculateOilViscosityAsync(OilPropertyConditions request, CancellationToken cancellationToken = default);
        Task<OilPropertyResult> CalculateOilPropertiesAsync(OilPropertyConditions request, CancellationToken cancellationToken = default);
        Task<OIL_COMPOSITION> SaveOilCompositionAsync(OIL_COMPOSITION composition, string? userId = null, CancellationToken cancellationToken = default);
        Task<OIL_COMPOSITION> GetOilCompositionAsync(string compositionId, CancellationToken cancellationToken = default);
        Task<List<OIL_PROPERTY_RESULT>> GetOilPropertyHistoryAsync(string compositionId, CancellationToken cancellationToken = default);
        Task<OIL_PROPERTY_RESULT> SaveOilResultAsync(OIL_PROPERTY_RESULT result, string? userId = null, CancellationToken cancellationToken = default);

        #endregion

        #region Gas Properties

        Task<decimal> CalculateGasZFactorAsync(GasComposition request, CancellationToken cancellationToken = default);
        Task<decimal> CalculateGasDensityAsync(GasComposition request, CancellationToken cancellationToken = default);
        Task<decimal> CalculateGasFormationVolumeFactorAsync(GasComposition request, CancellationToken cancellationToken = default);
        Task<GAS_COMPOSITION> SaveGasCompositionAsync(GAS_COMPOSITION composition, string? userId = null, CancellationToken cancellationToken = default);
        Task<GAS_COMPOSITION> GetGasCompositionAsync(string compositionId, CancellationToken cancellationToken = default);

        #endregion

        #region Heat Map

        Task<List<HeatMapDataPoint>> GenerateHeatMapAsync(HEAT_MAP_CONFIGURATION request, CancellationToken cancellationToken = default);
        Task<HEAT_MAP_CONFIGURATION> SaveHeatMapConfigurationAsync(HEAT_MAP_CONFIGURATION configuration, string? userId = null, CancellationToken cancellationToken = default);
        Task<HEAT_MAP_CONFIGURATION> GetHeatMapConfigurationAsync(string heatMapId, CancellationToken cancellationToken = default);
        Task<List<HEAT_MAP_DATA_POINT>> GenerateProductionHeatMapAsync(HEAT_MAP_CONFIGURATION request, CancellationToken cancellationToken = default);

        #endregion
    }
}
