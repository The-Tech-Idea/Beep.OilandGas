using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.PumpPerformance;
using Beep.OilandGas.PPDM39.Models;


namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to PumpPerformance models.
    /// </summary>
    public class PumpPerformanceMapper
    {
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getWellDepth;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getCasingDiameter;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getTubingDiameter;
        private readonly Func<WELL, decimal>? _getOilGravity;
        private readonly Func<WELL, decimal>? _getWaterCut;
        private readonly Func<WELL, decimal>? _getGasOilRatio;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getWellheadPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getBottomHoleTemperature;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getPumpSettingDepth;

        /// <summary>
        /// Initializes a new instance of PumpPerformanceMapper with default value retrievers.
        /// </summary>
        public PumpPerformanceMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of PumpPerformanceMapper with custom value retrievers.
        /// </summary>
        public PumpPerformanceMapper(
            Func<WELL, WELL_TUBULAR?, decimal>? getWellDepth = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getCasingDiameter = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getTubingDiameter = null,
            Func<WELL, decimal>? getOilGravity = null,
            Func<WELL, decimal>? getWaterCut = null,
            Func<WELL, decimal>? getGasOilRatio = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getWellheadPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getBottomHoleTemperature = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getPumpSettingDepth = null)
        {
            _getWellDepth = getWellDepth;
            _getCasingDiameter = getCasingDiameter;
            _getTubingDiameter = getTubingDiameter;
            _getOilGravity = getOilGravity;
            _getWaterCut = getWaterCut;
            _getGasOilRatio = getGasOilRatio;
            _getWellheadPressure = getWellheadPressure;
            _getBottomHoleTemperature = getBottomHoleTemperature;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getPumpSettingDepth = getPumpSettingDepth;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to ESP_DESIGN_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <param name="desiredFlowRate">Desired flow rate in bbl/day.</param>
        /// <param name="totalDynamicHead">Total dynamic head in feet.</param>
        /// <returns>The mapped ESP_DESIGN_PROPERTIES.</returns>
        public ESP_DESIGN_PROPERTIES MapToESPDesignProperties(
            WELL well,
            WELL_TUBULAR? tubular = null,
            WELL_PRESSURE? wellPressure = null,
            decimal desiredFlowRate = 0,
            decimal totalDynamicHead = 0)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getWellDepth = _getWellDepth ?? ValueRetrievers.GetWellDepth;
            var getCasingDiameter = _getCasingDiameter ?? ValueRetrievers.GetCasingDiameter;
            var getTubingDiameter = _getTubingDiameter ?? ValueRetrievers.GetTubingDiameterDecimal;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravityDecimal;
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCutDecimal;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatioDecimal;
            var getWellheadPressure = _getWellheadPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getBottomHoleTemperature = _getBottomHoleTemperature ?? ValueRetrievers.GetBottomholeTemperatureInRankine;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getPumpSettingDepth = _getPumpSettingDepth ?? ValueRetrievers.GetPumpSettingDepth;

            return new ESP_DESIGN_PROPERTIES
            {
                DESIRED_FLOW_RATE = desiredFlowRate,
                TOTAL_DYNAMIC_HEAD = totalDynamicHead,
                WELL_DEPTH = getWellDepth(well, tubular),
                CASING_DIAMETER = getCasingDiameter(well, tubular),
                TUBING_DIAMETER = getTubingDiameter(well, tubular),
                OIL_GRAVITY = getOilGravity(well),
                WATER_CUT = getWaterCut(well),
                GAS_OIL_RATIO = getGasOilRatio(well),
                WELLHEAD_PRESSURE = getWellheadPressure(well, wellPressure),
                BOTTOM_HOLE_TEMPERATURE = getBottomHoleTemperature(well, wellPressure),
                GAS_SPECIFIC_GRAVITY = getGasSpecificGravity(well),
                PUMP_SETTING_DEPTH = getPumpSettingDepth(well, tubular)
            };
        }
    }
}

