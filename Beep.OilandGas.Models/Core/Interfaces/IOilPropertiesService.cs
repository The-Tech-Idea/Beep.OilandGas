using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.OilProperties;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for oil property calculations.
    /// Provides oil property calculations, composition management, and property correlation selection.
    /// </summary>
    public interface IOilPropertiesService
    {
        /// <summary>
        /// Calculates oil formation volume factor.
        /// </summary>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <param name="gasOilRatio">Gas-oil ratio (scf/stb)</param>
        /// <param name="oilGravity">Oil gravity (API)</param>
        /// <param name="correlation">Correlation method to use</param>
        /// <returns>Formation volume factor (rb/stb)</returns>
        decimal CalculateFormationVolumeFactor(decimal pressure, decimal temperature, decimal gasOilRatio, decimal oilGravity, string correlation = "Standing");

        /// <summary>
        /// Calculates oil density.
        /// </summary>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <param name="oilGravity">Oil gravity (API)</param>
        /// <param name="gasOilRatio">Gas-oil ratio (scf/stb)</param>
        /// <returns>Oil density (lb/ft³)</returns>
        decimal CalculateOilDensity(decimal pressure, decimal temperature, decimal oilGravity, decimal gasOilRatio);

        /// <summary>
        /// Calculates oil viscosity.
        /// </summary>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <param name="oilGravity">Oil gravity (API)</param>
        /// <param name="gasOilRatio">Gas-oil ratio (scf/stb)</param>
        /// <returns>Oil viscosity (cp)</returns>
        decimal CalculateOilViscosity(decimal pressure, decimal temperature, decimal oilGravity, decimal gasOilRatio);

        /// <summary>
        /// Calculates comprehensive oil properties for a composition.
        /// </summary>
        /// <param name="composition">Oil composition data</param>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <returns>Oil property calculation result</returns>
        Task<OilPropertyResult> CalculateOilPropertiesAsync(OilComposition composition, decimal pressure, decimal temperature);

        /// <summary>
        /// Saves oil composition to database.
        /// </summary>
        /// <param name="composition">Oil composition data</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveOilCompositionAsync(OilComposition composition, string userId);

        /// <summary>
        /// Gets oil composition from database.
        /// </summary>
        /// <param name="compositionId">Composition identifier</param>
        /// <returns>Oil composition data</returns>
        Task<OilComposition?> GetOilCompositionAsync(string compositionId);

        /// <summary>
        /// Gets oil property calculation history for a composition.
        /// </summary>
        /// <param name="compositionId">Composition identifier</param>
        /// <returns>List of oil property calculation results</returns>
        Task<List<OilPropertyResult>> GetOilPropertyHistoryAsync(string compositionId);

        /// <summary>
        /// Saves oil property calculation result to database.
        /// </summary>
        /// <param name="result">Property calculation result</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveOilPropertyResultAsync(OilPropertyResult result, string userId);
    }
}





