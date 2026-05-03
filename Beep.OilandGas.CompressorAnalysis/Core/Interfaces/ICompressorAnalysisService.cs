using System.Threading.Tasks;
using Beep.OilandGas.CompressorAnalysis.Data;

namespace Beep.OilandGas.CompressorAnalysis.Core.Interfaces
{
    /// <summary>
    /// Canonical compressor analysis API for centrifugal/reciprocating power and pressure-flow estimates.
    /// Table-shaped inputs/outputs live in namespace <c>Beep.OilandGas.CompressorAnalysis.Data</c>; wire DTOs stay in <c>Beep.OilandGas.Models.Data.Calculations</c>.
    /// </summary>
    public interface ICompressorAnalysisService
    {
        /// <summary>
        /// Polytropic/adiabatic power and discharge temperature for a centrifugal train (single or staged inputs on properties).
        /// </summary>
        Task<COMPRESSOR_POWER_RESULT> CalculateCentrifugalPowerAsync(CENTRIFUGAL_COMPRESSOR_PROPERTIES properties);

        /// <summary>
        /// Power and discharge conditions for reciprocating geometry and operating conditions on properties.
        /// </summary>
        Task<COMPRESSOR_POWER_RESULT> CalculateReciprocatingPowerAsync(RECIPROCATING_COMPRESSOR_PROPERTIES properties);

        /// <summary>
        /// Iterative required discharge pressure / power within a horsepower cap (same physics as <c>CompressorPressureCalculator.CalculateRequiredPressure</c>).
        /// </summary>
        /// <param name="maxPower">Maximum driver power (HP) allowed in the search loop.</param>
        Task<COMPRESSOR_PRESSURE_RESULT> CalculateRequiredPressureAsync(
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions,
            decimal requiredFlowRate,
            decimal maxPower = 1000m,
            decimal compressorEfficiency = 0.75m);
    }
}
