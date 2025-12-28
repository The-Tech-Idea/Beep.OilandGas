using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PlungerLift.Models;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to PlungerLift models.
    /// </summary>
    public class PlungerLiftMapper
    {
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getWellDepth;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getTubingDiameter;
        private readonly Func<WELL, decimal>? _getPlungerDiameter;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getWellheadPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getCasingPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getBottomHolePressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getWellheadTemperature;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getBottomHoleTemperature;
        private readonly Func<WELL, decimal>? _getOilGravity;
        private readonly Func<WELL, decimal>? _getWaterCut;
        private readonly Func<WELL, decimal>? _getGasOilRatio;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, decimal>? _getLiquidProductionRate;

        /// <summary>
        /// Initializes a new instance of PlungerLiftMapper with default value retrievers.
        /// </summary>
        public PlungerLiftMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of PlungerLiftMapper with custom value retrievers.
        /// </summary>
        public PlungerLiftMapper(
            Func<WELL, WELL_TUBULAR?, decimal>? getWellDepth = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getTubingDiameter = null,
            Func<WELL, decimal>? getPlungerDiameter = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getWellheadPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getCasingPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getBottomHolePressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getWellheadTemperature = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getBottomHoleTemperature = null,
            Func<WELL, decimal>? getOilGravity = null,
            Func<WELL, decimal>? getWaterCut = null,
            Func<WELL, decimal>? getGasOilRatio = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, decimal>? getLiquidProductionRate = null)
        {
            _getWellDepth = getWellDepth;
            _getTubingDiameter = getTubingDiameter;
            _getPlungerDiameter = getPlungerDiameter;
            _getWellheadPressure = getWellheadPressure;
            _getCasingPressure = getCasingPressure;
            _getBottomHolePressure = getBottomHolePressure;
            _getWellheadTemperature = getWellheadTemperature;
            _getBottomHoleTemperature = getBottomHoleTemperature;
            _getOilGravity = getOilGravity;
            _getWaterCut = getWaterCut;
            _getGasOilRatio = getGasOilRatio;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getLiquidProductionRate = getLiquidProductionRate;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to PlungerLiftWellProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped PlungerLiftWellProperties.</returns>
        public PlungerLiftWellProperties MapToPlungerLiftWellProperties(
            WELL well,
            WELL_TUBULAR? tubular = null,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getWellDepth = _getWellDepth ?? ValueRetrievers.GetWellDepth;
            var getTubingDiameter = _getTubingDiameter ?? ValueRetrievers.GetTubingDiameterDecimal;
            var getPlungerDiameter = _getPlungerDiameter ?? ((w) => throw new InvalidOperationException("Plunger diameter not available. Provide getPlungerDiameter function."));
            var getWellheadPressure = _getWellheadPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getCasingPressure = _getCasingPressure ?? ValueRetrievers.GetCasingPressure;
            var getBottomHolePressure = _getBottomHolePressure ?? ValueRetrievers.GetReservoirPressureDecimal;
            var getWellheadTemperature = _getWellheadTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;
            var getBottomHoleTemperature = _getBottomHoleTemperature ?? ValueRetrievers.GetBottomholeTemperatureInRankine;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravityDecimal;
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCutDecimal;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatioDecimal;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getLiquidProductionRate = _getLiquidProductionRate ?? ((w) => throw new InvalidOperationException("Liquid production rate not available. Provide getLiquidProductionRate function."));

            return new PlungerLiftWellProperties
            {
                WellDepth = getWellDepth(well, tubular),
                TubingDiameter = getTubingDiameter(well, tubular),
                PlungerDiameter = getPlungerDiameter(well),
                WellheadPressure = getWellheadPressure(well, wellPressure),
                CasingPressure = getCasingPressure(well, wellPressure),
                BottomHolePressure = getBottomHolePressure(well, wellPressure),
                WellheadTemperature = getWellheadTemperature(well, wellPressure),
                BottomHoleTemperature = getBottomHoleTemperature(well, wellPressure),
                OilGravity = getOilGravity(well),
                WaterCut = getWaterCut(well),
                GasOilRatio = getGasOilRatio(well),
                GasSpecificGravity = getGasSpecificGravity(well),
                LiquidProductionRate = getLiquidProductionRate(well)
            };
        }
    }
}

