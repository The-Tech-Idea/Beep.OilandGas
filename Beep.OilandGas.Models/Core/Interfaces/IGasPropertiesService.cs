using System.Collections.Generic;
using System.Threading.Tasks;
using Beep.OilandGas.Models.Data.GasProperties;

namespace Beep.OilandGas.Models.Core.Interfaces
{
    /// <summary>
    /// Service interface for gas property calculations.
    /// Provides gas property calculations, composition management, and property correlation selection.
    /// </summary>
    public interface IGasPropertiesService
    {
        /// <summary>
        /// Calculates Z-factor for gas.
        /// </summary>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <param name="specificGravity">Gas specific gravity</param>
        /// <param name="correlation">Correlation method to use</param>
        /// <returns>Z-factor value</returns>
        decimal CalculateZFactor(decimal pressure, decimal temperature, decimal specificGravity, string correlation = "Standing-Katz");

        /// <summary>
        /// Calculates gas density.
        /// </summary>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <param name="zFactor">Z-factor</param>
        /// <param name="molecularWeight">Molecular weight</param>
        /// <returns>Gas density (lb/ft³)</returns>
        decimal CalculateGasDensity(decimal pressure, decimal temperature, decimal zFactor, decimal molecularWeight);

        /// <summary>
        /// Calculates gas formation volume factor.
        /// </summary>
        /// <param name="pressure">Pressure (psia)</param>
        /// <param name="temperature">Temperature (Rankine)</param>
        /// <param name="zFactor">Z-factor</param>
        /// <returns>Formation volume factor (ft³/scf)</returns>
        decimal CalculateFormationVolumeFactor(decimal pressure, decimal temperature, decimal zFactor);

        /// <summary>
        /// Saves gas composition to database.
        /// </summary>
        /// <param name="composition">Gas composition data</param>
        /// <param name="userId">User ID for audit</param>
        /// <returns>Task</returns>
        Task SaveGasCompositionAsync(GasComposition composition, string userId);

        /// <summary>
        /// Gets gas composition from database.
        /// </summary>
        /// <param name="compositionId">Composition identifier</param>
        /// <returns>Gas composition data</returns>
        Task<GasComposition?> GetGasCompositionAsync(string compositionId);
    }
}





