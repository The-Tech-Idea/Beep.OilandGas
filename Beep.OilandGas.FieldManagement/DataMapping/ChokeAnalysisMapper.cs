using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.ChokeAnalysis.Models;

namespace Beep.OilandGas.FieldManagement.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to ChokeAnalysis models.
    /// </summary>
    public class ChokeAnalysisMapper
    {
        private readonly Func<WELL, decimal>? _getChokeDiameter;
        private readonly Func<WELL, ChokeType>? _getChokeType;
        private readonly Func<WELL, decimal>? _getDischargeCoefficient;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getUpstreamPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getDownstreamPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getTemperature;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, decimal>? _getZFactor;
        private readonly Func<WELL, decimal>? _getGasFlowRate;

        /// <summary>
        /// Initializes a new instance of ChokeAnalysisMapper with default value retrievers.
        /// </summary>
        public ChokeAnalysisMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of ChokeAnalysisMapper with custom value retrievers.
        /// </summary>
        public ChokeAnalysisMapper(
            Func<WELL, decimal>? getChokeDiameter = null,
            Func<WELL, ChokeType>? getChokeType = null,
            Func<WELL, decimal>? getDischargeCoefficient = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getUpstreamPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getDownstreamPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getTemperature = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, decimal>? getZFactor = null,
            Func<WELL, decimal>? getGasFlowRate = null)
        {
            _getChokeDiameter = getChokeDiameter;
            _getChokeType = getChokeType;
            _getDischargeCoefficient = getDischargeCoefficient;
            _getUpstreamPressure = getUpstreamPressure;
            _getDownstreamPressure = getDownstreamPressure;
            _getTemperature = getTemperature;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getZFactor = getZFactor;
            _getGasFlowRate = getGasFlowRate;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to ChokeProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped ChokeProperties.</returns>
        public ChokeProperties MapToChokeProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getChokeDiameter = _getChokeDiameter ?? ((w) => throw new InvalidOperationException("Choke diameter not available. Provide getChokeDiameter function."));
            var getChokeType = _getChokeType ?? ((w) => ChokeType.Bean); // Default to Bean
            var getDischargeCoefficient = _getDischargeCoefficient ?? ValueRetrievers.GetDefaultDischargeCoefficient;

            return new ChokeProperties
            {
                ChokeDiameter = getChokeDiameter(well),
                ChokeType = getChokeType(well),
                DischargeCoefficient = getDischargeCoefficient(well)
            };
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to GasChokeProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped GasChokeProperties.</returns>
        public GasChokeProperties MapToGasChokeProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getUpstreamPressure = _getUpstreamPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getDownstreamPressure = _getDownstreamPressure ?? ValueRetrievers.GetDownstreamPressure80Percent;
            var getTemperature = _getTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getZFactor = _getZFactor ?? ((w) => throw new InvalidOperationException("Z-factor not available. Provide getZFactor function."));
            var getGasFlowRate = _getGasFlowRate ?? ((w) => throw new InvalidOperationException("Gas flow rate not available. Provide getGasFlowRate function."));

            return new GasChokeProperties
            {
                UpstreamPressure = getUpstreamPressure(well, wellPressure),
                DownstreamPressure = getDownstreamPressure(well, wellPressure),
                Temperature = getTemperature(well, wellPressure),
                GasSpecificGravity = getGasSpecificGravity(well),
                ZFactor = getZFactor(well),
                FlowRate = getGasFlowRate(well)
            };
        }
    }
}

