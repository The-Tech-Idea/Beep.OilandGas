using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.ChokeAnalysis;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
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
        /// Maps PPDM39 WELL and related entities to CHOKE_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped CHOKE_PROPERTIES.</returns>
        public CHOKE_PROPERTIES MapToChokeProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getChokeDiameter = _getChokeDiameter ?? ((w) => throw new InvalidOperationException("Choke diameter not available. Provide getChokeDiameter function."));
            var getChokeType = _getChokeType ?? ((w) => ChokeType.Bean); // Default to Bean
            var getDischargeCoefficient = _getDischargeCoefficient ?? ValueRetrievers.GetDefaultDischargeCoefficient;

            return new CHOKE_PROPERTIES
            {
                CHOKE_DIAMETER = getChokeDiameter(well),
                CHOKE_TYPE = getChokeType(well).ToString(),
                DISCHARGE_COEFFICIENT = getDischargeCoefficient(well)
            };
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to GAS_CHOKE_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped GAS_CHOKE_PROPERTIES.</returns>
        public GAS_CHOKE_PROPERTIES MapToGasChokeProperties(
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

            return new GAS_CHOKE_PROPERTIES
            {
                UPSTREAM_PRESSURE = getUpstreamPressure(well, wellPressure),
                DOWNSTREAM_PRESSURE = getDownstreamPressure(well, wellPressure),
                TEMPERATURE = getTemperature(well, wellPressure),
                GAS_SPECIFIC_GRAVITY = getGasSpecificGravity(well),
                Z_FACTOR = getZFactor(well),
                FLOW_RATE = getGasFlowRate(well)
            };
        }
    }
}

