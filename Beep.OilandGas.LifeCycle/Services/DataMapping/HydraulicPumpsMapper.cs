using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.HydraulicPumps;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to HydraulicPumps models.
    /// </summary>
    public class HydraulicPumpsMapper
    {
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getWellDepth;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getTubingDiameter;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getCasingDiameter;
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
        /// Initializes a new instance of HydraulicPumpsMapper with default value retrievers.
        /// </summary>
        public HydraulicPumpsMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of HydraulicPumpsMapper with custom value retrievers.
        /// </summary>
        public HydraulicPumpsMapper(
            Func<WELL, WELL_TUBULAR?, decimal>? getWellDepth = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getTubingDiameter = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getCasingDiameter = null,
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
            _getTubingDiameter = getTubingDiameter;
            _getCasingDiameter = getCasingDiameter;
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
        /// Maps PPDM39 WELL and related entities to HYDRAULIC_PUMP_WELL_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped HYDRAULIC_PUMP_WELL_PROPERTIES.</returns>
        public HYDRAULIC_PUMP_WELL_PROPERTIES MapToHydraulicPumpWellProperties(
            WELL well,
            WELL_TUBULAR? tubular = null,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getWellDepth = _getWellDepth ?? ValueRetrievers.GetWellDepth;
            var getTubingDiameter = _getTubingDiameter ?? ValueRetrievers.GetTubingDiameterDecimal;
            var getCasingDiameter = _getCasingDiameter ?? ValueRetrievers.GetCasingDiameter;
            var getWellheadPressure = _getWellheadPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getBottomHolePressure = _getBottomHolePressure ?? ValueRetrievers.GetReservoirPressureDecimal;
            var getWellheadTemperature = _getWellheadTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;
            var getBottomHoleTemperature = _getBottomHoleTemperature ?? ValueRetrievers.GetBottomholeTemperatureInRankine;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravityDecimal;
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCutDecimal;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatioDecimal;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getDesiredProductionRate = _getDesiredProductionRate ?? ((w) => throw new InvalidOperationException("Desired production rate not available. Provide getDesiredProductionRate function."));

            return new HYDRAULIC_PUMP_WELL_PROPERTIES
            {
                WELL_DEPTH = getWellDepth(well, tubular),
                TUBING_DIAMETER = getTubingDiameter(well, tubular),
                CASING_DIAMETER = getCasingDiameter(well, tubular),
                WELLHEAD_PRESSURE = getWellheadPressure(well, wellPressure),
                BOTTOM_HOLE_PRESSURE = getBottomHolePressure(well, wellPressure),
                WELLHEAD_TEMPERATURE = getWellheadTemperature(well, wellPressure),
                BOTTOM_HOLE_TEMPERATURE = getBottomHoleTemperature(well, wellPressure),
                OIL_GRAVITY = getOilGravity(well),
                WATER_CUT = getWaterCut(well),
                GAS_OIL_RATIO = (int)getGasOilRatio(well),
                GAS_SPECIFIC_GRAVITY = (int)getGasSpecificGravity(well),
                DESIRED_PRODUCTION_RATE = (int)getDesiredProductionRate(well)
            };
        }
    }
}

