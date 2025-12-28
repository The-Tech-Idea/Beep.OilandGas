using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.CompressorAnalysis.Models;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to CompressorAnalysis models.
    /// </summary>
    public class CompressorAnalysisMapper
    {
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getSuctionPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getDischargePressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getSuctionTemperature;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getDischargeTemperature;
        private readonly Func<WELL, decimal>? _getGasFlowRate;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, decimal>? _getGasMolecularWeight;
        private readonly Func<WELL, decimal>? _getCompressorEfficiency;
        private readonly Func<WELL, decimal>? _getMechanicalEfficiency;

        /// <summary>
        /// Initializes a new instance of CompressorAnalysisMapper with default value retrievers.
        /// </summary>
        public CompressorAnalysisMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of CompressorAnalysisMapper with custom value retrievers.
        /// </summary>
        public CompressorAnalysisMapper(
            Func<WELL, WELL_PRESSURE?, decimal>? getSuctionPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getDischargePressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getSuctionTemperature = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getDischargeTemperature = null,
            Func<WELL, decimal>? getGasFlowRate = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, decimal>? getGasMolecularWeight = null,
            Func<WELL, decimal>? getCompressorEfficiency = null,
            Func<WELL, decimal>? getMechanicalEfficiency = null)
        {
            _getSuctionPressure = getSuctionPressure;
            _getDischargePressure = getDischargePressure;
            _getSuctionTemperature = getSuctionTemperature;
            _getDischargeTemperature = getDischargeTemperature;
            _getGasFlowRate = getGasFlowRate;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getGasMolecularWeight = getGasMolecularWeight;
            _getCompressorEfficiency = getCompressorEfficiency;
            _getMechanicalEfficiency = getMechanicalEfficiency;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to CompressorOperatingConditions.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped CompressorOperatingConditions.</returns>
        public CompressorOperatingConditions MapToCompressorOperatingConditions(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getSuctionPressure = _getSuctionPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getDischargePressure = _getDischargePressure ?? ValueRetrievers.GetDischargePressure2x;
            var getSuctionTemperature = _getSuctionTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;
            var getDischargeTemperature = _getDischargeTemperature ?? ValueRetrievers.GetDischargeTemperaturePlus50F;
            var getGasFlowRate = _getGasFlowRate ?? ((w) => throw new InvalidOperationException("Gas flow rate not available. Provide getGasFlowRate function."));
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getGasMolecularWeight = _getGasMolecularWeight ?? ValueRetrievers.GetGasMolecularWeight;
            var getCompressorEfficiency = _getCompressorEfficiency ?? ValueRetrievers.GetDefaultCompressorEfficiency;
            var getMechanicalEfficiency = _getMechanicalEfficiency ?? ValueRetrievers.GetDefaultMechanicalEfficiency;

            return new CompressorOperatingConditions
            {
                SuctionPressure = getSuctionPressure(well, wellPressure),
                DischargePressure = getDischargePressure(well, wellPressure),
                SuctionTemperature = getSuctionTemperature(well, wellPressure),
                DischargeTemperature = getDischargeTemperature(well, wellPressure),
                GasFlowRate = getGasFlowRate(well),
                GasSpecificGravity = getGasSpecificGravity(well),
                GasMolecularWeight = getGasMolecularWeight(well),
                CompressorEfficiency = getCompressorEfficiency(well),
                MechanicalEfficiency = getMechanicalEfficiency(well)
            };
        }
    }
}

