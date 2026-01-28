using System;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.PPDM39.Models;


namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to SuckerRodPumping models.
    /// </summary>
    public class SuckerRodPumpingMapper
    {
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getWellDepth;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getTubingDiameter;
        private readonly Func<WELL, decimal>? _getRodDiameter;
        private readonly Func<WELL, decimal>? _getPumpDiameter;
        private readonly Func<WELL, decimal>? _getStrokeLength;
        private readonly Func<WELL, decimal>? _getStrokesPerMinute;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getWellheadPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getBottomHolePressure;
        private readonly Func<WELL, decimal>? _getOilGravity;
        private readonly Func<WELL, decimal>? _getWaterCut;
        private readonly Func<WELL, decimal>? _getGasOilRatio;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;

        /// <summary>
        /// Initializes a new instance of SuckerRodPumpingMapper with default value retrievers.
        /// </summary>
        public SuckerRodPumpingMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of SuckerRodPumpingMapper with custom value retrievers.
        /// </summary>
        public SuckerRodPumpingMapper(
            Func<WELL, WELL_TUBULAR?, decimal>? getWellDepth = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getTubingDiameter = null,
            Func<WELL, decimal>? getRodDiameter = null,
            Func<WELL, decimal>? getPumpDiameter = null,
            Func<WELL, decimal>? getStrokeLength = null,
            Func<WELL, decimal>? getStrokesPerMinute = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getWellheadPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getBottomHolePressure = null,
            Func<WELL, decimal>? getOilGravity = null,
            Func<WELL, decimal>? getWaterCut = null,
            Func<WELL, decimal>? getGasOilRatio = null,
            Func<WELL, decimal>? getGasSpecificGravity = null)
        {
            _getWellDepth = getWellDepth;
            _getTubingDiameter = getTubingDiameter;
            _getRodDiameter = getRodDiameter;
            _getPumpDiameter = getPumpDiameter;
            _getStrokeLength = getStrokeLength;
            _getStrokesPerMinute = getStrokesPerMinute;
            _getWellheadPressure = getWellheadPressure;
            _getBottomHolePressure = getBottomHolePressure;
            _getOilGravity = getOilGravity;
            _getWaterCut = getWaterCut;
            _getGasOilRatio = getGasOilRatio;
            _getGasSpecificGravity = getGasSpecificGravity;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to SUCKER_ROD_SYSTEM_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped SUCKER_ROD_SYSTEM_PROPERTIES.</returns>
        public SUCKER_ROD_SYSTEM_PROPERTIES MapToSuckerRodSystemProperties(
            WELL well,
            WELL_TUBULAR? tubular = null,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getWellDepth = _getWellDepth ?? ValueRetrievers.GetWellDepth;
            var getTubingDiameter = _getTubingDiameter ?? ValueRetrievers.GetTubingDiameterDecimal;
            var getRodDiameter = _getRodDiameter ?? ((w) => throw new InvalidOperationException("Rod diameter not available. Provide getRodDiameter function."));
            var getPumpDiameter = _getPumpDiameter ?? ((w) => throw new InvalidOperationException("Pump diameter not available. Provide getPumpDiameter function."));
            var getStrokeLength = _getStrokeLength ?? ((w) => throw new InvalidOperationException("Stroke length not available. Provide getStrokeLength function."));
            var getStrokesPerMinute = _getStrokesPerMinute ?? ((w) => throw new InvalidOperationException("Strokes per minute not available. Provide getStrokesPerMinute function."));
            var getWellheadPressure = _getWellheadPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getBottomHolePressure = _getBottomHolePressure ?? ValueRetrievers.GetReservoirPressureDecimal;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravityDecimal;
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCutDecimal;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatioDecimal;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;

            return new SUCKER_ROD_SYSTEM_PROPERTIES
            {
                WellDepth = getWellDepth(well, tubular),
                TubingDiameter = getTubingDiameter(well, tubular),
                RodDiameter = getRodDiameter(well),
                PumpDiameter = getPumpDiameter(well),
                StrokeLength = getStrokeLength(well),
                StrokesPerMinute = getStrokesPerMinute(well),
                WellheadPressure = getWellheadPressure(well, wellPressure),
                BottomHolePressure = getBottomHolePressure(well, wellPressure),
                OilGravity = getOilGravity(well),
                WaterCut = getWaterCut(well),
                GasOilRatio = getGasOilRatio(well),
                GasSpecificGravity = getGasSpecificGravity(well)
            };
        }
    }
}

