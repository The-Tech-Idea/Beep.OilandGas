using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.FlashCalculations.Models;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to FlashCalculations models.
    /// </summary>
    public class FlashCalculationsMapper
    {
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getTemperature;
        private readonly Func<WELL, List<Component>>? _getFeedComposition;

        /// <summary>
        /// Initializes a new instance of FlashCalculationsMapper with default value retrievers.
        /// </summary>
        public FlashCalculationsMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of FlashCalculationsMapper with custom value retrievers.
        /// </summary>
        public FlashCalculationsMapper(
            Func<WELL, WELL_PRESSURE?, decimal>? getPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getTemperature = null,
            Func<WELL, List<Component>>? getFeedComposition = null)
        {
            _getPressure = getPressure;
            _getTemperature = getTemperature;
            _getFeedComposition = getFeedComposition;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to FlashConditions.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped FlashConditions.</returns>
        public FlashConditions MapToFlashConditions(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getPressure = _getPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getTemperature = _getTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;
            var getFeedComposition = _getFeedComposition ?? ((w) => throw new InvalidOperationException("Feed composition not available. Provide getFeedComposition function."));

            return new FlashConditions
            {
                Pressure = getPressure(well, wellPressure),
                Temperature = getTemperature(well, wellPressure),
                FeedComposition = getFeedComposition(well)
            };
        }
    }
}

