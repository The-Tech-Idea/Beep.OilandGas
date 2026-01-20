using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.GasLift;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to GasLift models.
    /// </summary>
    public class GasLiftMapper
    {
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getWellDepth;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getWellheadPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getBottomHolePressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getWellheadTemperature;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getBottomHoleTemperature;
        private readonly Func<WELL, decimal>? _getOilGravity;
        private readonly Func<WELL, decimal>? _getWaterCut;
        private readonly Func<WELL, decimal>? _getGasOilRatio;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, decimal>? _getDesiredProductionRate;

        /// <summary>
        /// Initializes a new instance of GasLiftMapper with default value retrievers.
        /// </summary>
        public GasLiftMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of GasLiftMapper with custom value retrievers.
        /// </summary>
        public GasLiftMapper(
            Func<WELL, WELL_TUBULAR?, decimal>? getWellDepth = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getWellheadPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getBottomHolePressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getWellheadTemperature = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getBottomHoleTemperature = null,
            Func<WELL, decimal>? getOilGravity = null,
            Func<WELL, decimal>? getWaterCut = null,
            Func<WELL, decimal>? getGasOilRatio = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, decimal>? getDesiredProductionRate = null)
        {
            _getWellDepth = getWellDepth;
            _getWellheadPressure = getWellheadPressure;
            _getBottomHolePressure = getBottomHolePressure;
            _getWellheadTemperature = getWellheadTemperature;
            _getBottomHoleTemperature = getBottomHoleTemperature;
            _getOilGravity = getOilGravity;
            _getWaterCut = getWaterCut;
            _getGasOilRatio = getGasOilRatio;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getDesiredProductionRate = getDesiredProductionRate;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to GasLiftWellProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped GasLiftWellProperties.</returns>
        public GasLiftWellProperties MapToGasLiftWellProperties(
            WELL well,
            WELL_TUBULAR? tubular = null,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getWellDepth = _getWellDepth ?? ValueRetrievers.GetWellDepth;
            var getWellheadPressure = _getWellheadPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getBottomHolePressure = _getBottomHolePressure ?? ValueRetrievers.GetReservoirPressureDecimal;
            var getWellheadTemperature = _getWellheadTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;
            var getBottomHoleTemperature = _getBottomHoleTemperature ?? ValueRetrievers.GetBottomholeTemperatureInRankine;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravityDecimal;
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCutDecimal;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatioDecimal;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getDesiredProductionRate = _getDesiredProductionRate ?? ((w) => throw new InvalidOperationException("Desired production rate not available. Provide getDesiredProductionRate function."));

            return new GasLiftWellProperties
            {
                WellDepth = getWellDepth(well, tubular),
                WellheadPressure = getWellheadPressure(well, wellPressure),
                BottomHolePressure = getBottomHolePressure(well, wellPressure),
                WellheadTemperature = getWellheadTemperature(well, wellPressure),
                BottomHoleTemperature = getBottomHoleTemperature(well, wellPressure),
                OilGravity = getOilGravity(well),
                WaterCut = getWaterCut(well),
                GasOilRatio = getGasOilRatio(well),
                GasSpecificGravity = getGasSpecificGravity(well),
                DesiredProductionRate = getDesiredProductionRate(well)
            };
        }
    }
}

