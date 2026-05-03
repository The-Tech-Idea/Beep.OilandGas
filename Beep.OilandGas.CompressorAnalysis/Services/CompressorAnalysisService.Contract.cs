using System;
using System.Threading.Tasks;
using Beep.OilandGas.CompressorAnalysis.Calculations;
using Beep.OilandGas.CompressorAnalysis.Validation;
using Beep.OilandGas.CompressorAnalysis.Data;

namespace Beep.OilandGas.CompressorAnalysis.Services
{
    /// <summary>
    /// <see cref="ICompressorAnalysisService"/> — thin wrappers over static calculators + validation (stable DI surface).
    /// </summary>
    public partial class CompressorAnalysisService
    {
        /// <inheritdoc />
        public Task<COMPRESSOR_POWER_RESULT> CalculateCentrifugalPowerAsync(CENTRIFUGAL_COMPRESSOR_PROPERTIES properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            CompressorValidator.ValidateCentrifugalCompressorProperties(properties);
            var result = CentrifugalCompressorCalculator.CalculatePower(properties);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<COMPRESSOR_POWER_RESULT> CalculateReciprocatingPowerAsync(RECIPROCATING_COMPRESSOR_PROPERTIES properties)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            CompressorValidator.ValidateReciprocatingCompressorProperties(properties);
            var result = ReciprocatingCompressorCalculator.CalculatePower(properties);
            return Task.FromResult(result);
        }

        /// <inheritdoc />
        public Task<COMPRESSOR_PRESSURE_RESULT> CalculateRequiredPressureAsync(
            COMPRESSOR_OPERATING_CONDITIONS operatingConditions,
            decimal requiredFlowRate,
            decimal maxPower = 1000m,
            decimal compressorEfficiency = 0.75m)
        {
            if (operatingConditions == null)
                throw new ArgumentNullException(nameof(operatingConditions));
            CompressorValidator.ValidateOperatingConditions(operatingConditions);
            var result = CompressorPressureCalculator.CalculateRequiredPressure(
                operatingConditions,
                requiredFlowRate,
                maxPower,
                compressorEfficiency);
            return Task.FromResult(result);
        }
    }
}
