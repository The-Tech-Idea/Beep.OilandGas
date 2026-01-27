using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.PPDM39.Models;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 WELL entity to NodalAnalysis models (WellboreProperties and ReservoirProperties).
    /// </summary>
    public class NodalAnalysisMapper : IPPDM39Mapper<WELL, WellboreProperties>, IPPDM39Mapper<WELL, ReservoirProperties>
    {
        private readonly Func<WELL, WELL_TUBULAR?, double>? _getTubingDiameter;
        private readonly Func<WELL, WELL_TUBULAR?, double>? _getTubingLength;
        private readonly Func<WELL, WELL_PRESSURE?, double>? _getWellheadPressure;
        private readonly Func<WELL, WELL_PRESSURE?, double>? _getWellheadTemperature;
        private readonly Func<WELL, WELL_PRESSURE?, double>? _getBottomholeTemperature;
        private readonly Func<WELL, double>? _getWaterCut;
        private readonly Func<WELL, double>? _getGasOilRatio;
        private readonly Func<WELL, double>? _getOilGravity;
        private readonly Func<WELL, double>? _getGasSpecificGravity;
        private readonly Func<WELL, WELL_PRESSURE?, double>? _getReservoirPressure;
        private readonly Func<WELL, double>? _getBubblePointPressure;
        private readonly Func<WELL, double>? _getProductivityIndex;
        private readonly Func<WELL, double>? _getFormationVolumeFactor;
        private readonly Func<WELL, double>? _getOilViscosity;

        /// <summary>
        /// Initializes a new instance of NodalAnalysisMapper with default value retrievers.
        /// </summary>
        public NodalAnalysisMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of NodalAnalysisMapper with custom value retrievers.
        /// </summary>
        public NodalAnalysisMapper(
            Func<WELL, WELL_TUBULAR?, double>? getTubingDiameter = null,
            Func<WELL, WELL_TUBULAR?, double>? getTubingLength = null,
            Func<WELL, WELL_PRESSURE?, double>? getWellheadPressure = null,
            Func<WELL, WELL_PRESSURE?, double>? getWellheadTemperature = null,
            Func<WELL, WELL_PRESSURE?, double>? getBottomholeTemperature = null,
            Func<WELL, double>? getWaterCut = null,
            Func<WELL, double>? getGasOilRatio = null,
            Func<WELL, double>? getOilGravity = null,
            Func<WELL, double>? getGasSpecificGravity = null,
            Func<WELL, WELL_PRESSURE?, double>? getReservoirPressure = null,
            Func<WELL, double>? getBubblePointPressure = null,
            Func<WELL, double>? getProductivityIndex = null,
            Func<WELL, double>? getFormationVolumeFactor = null,
            Func<WELL, double>? getOilViscosity = null)
        {
            _getTubingDiameter = getTubingDiameter;
            _getTubingLength = getTubingLength;
            _getWellheadPressure = getWellheadPressure;
            _getWellheadTemperature = getWellheadTemperature;
            _getBottomholeTemperature = getBottomholeTemperature;
            _getWaterCut = getWaterCut;
            _getGasOilRatio = getGasOilRatio;
            _getOilGravity = getOilGravity;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getReservoirPressure = getReservoirPressure;
            _getBubblePointPressure = getBubblePointPressure;
            _getProductivityIndex = getProductivityIndex;
            _getFormationVolumeFactor = getFormationVolumeFactor;
            _getOilViscosity = getOilViscosity;
        }

        /// <summary>
        /// Maps PPDM39 WELL to WellboreProperties for VLP calculations.
        /// </summary>
        /// <param name="ppdm39Well">The PPDM39 WELL entity.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity for tubing properties.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity for pressure and temperature.</param>
        /// <returns>The mapped WellboreProperties.</returns>
        public WellboreProperties MapToDomain(
            WELL ppdm39Well,
            WELL_TUBULAR? tubular = null,
            WELL_PRESSURE? wellPressure = null)
        {
            if (ppdm39Well == null)
                throw new ArgumentNullException(nameof(ppdm39Well));

            var getTubingDiameter = _getTubingDiameter ?? ValueRetrievers.GetTubingDiameter;
            var getTubingLength = _getTubingLength ?? ValueRetrievers.GetTubingLength;
            var getWellheadPressure = _getWellheadPressure ?? ((well, wp) => ValueRetrievers.GetWellheadPressure(well, wp));
            var getWellheadTemperature = _getWellheadTemperature ?? ((well, wp) => ValueRetrievers.GetWellheadTemperature(well, wp));
            var getBottomholeTemperature = _getBottomholeTemperature ?? ((well, wp) => ValueRetrievers.GetBottomholeTemperature(well, wp));
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCut;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatio;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravity;
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravity;

            var wellboreProperties = new WellboreProperties
            {
                TubingDiameter = getTubingDiameter(ppdm39Well, tubular),
                TubingLength = getTubingLength(ppdm39Well, tubular),
                WellheadPressure = getWellheadPressure(ppdm39Well, wellPressure),
                WaterCut = getWaterCut(ppdm39Well),
                GasOilRatio = getGasOilRatio(ppdm39Well),
                OilGravity = getOilGravity(ppdm39Well),
                GasSpecificGravity = getGasSpecificGravity(ppdm39Well),
                WellheadTemperature = getWellheadTemperature(ppdm39Well, wellPressure),
                BottomholeTemperature = getBottomholeTemperature(ppdm39Well, wellPressure),
                TubingRoughness = 0.00015 // feet - industry standard (could use ValueRetrievers.GetDefaultPipelineRoughness if converted to decimal)
            };

            return wellboreProperties;
        }

        /// <summary>
        /// Maps PPDM39 WELL to WellboreProperties (implements interface - uses overload with related entities).
        /// </summary>
        WellboreProperties IPPDM39Mapper<WELL, WellboreProperties>.MapToDomain(WELL ppdm39Well)
        {
            return MapToDomain(ppdm39Well, null, null);
        }

        /// <summary>
        /// Maps PPDM39 WELL to ReservoirProperties for IPR calculations.
        /// </summary>
        /// <param name="ppdm39Well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity for reservoir pressure.</param>
        /// <returns>The mapped ReservoirProperties.</returns>
        public ReservoirProperties MapToReservoirProperties(
            WELL ppdm39Well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (ppdm39Well == null)
                throw new ArgumentNullException(nameof(ppdm39Well));

            var getReservoirPressure = _getReservoirPressure ?? ((well, wp) => ValueRetrievers.GetReservoirPressure(well, wp));
            var getBubblePointPressure = _getBubblePointPressure ?? ValueRetrievers.GetBubblePointPressure;
            var getProductivityIndex = _getProductivityIndex ?? ValueRetrievers.GetProductivityIndex;
            var getWaterCut = _getWaterCut ?? ValueRetrievers.GetWaterCut;
            var getGasOilRatio = _getGasOilRatio ?? ValueRetrievers.GetGasOilRatio;
            var getOilGravity = _getOilGravity ?? ValueRetrievers.GetOilGravity;
            var getFormationVolumeFactor = _getFormationVolumeFactor ?? ValueRetrievers.GetFormationVolumeFactor;
            var getOilViscosity = _getOilViscosity ?? ValueRetrievers.GetOilViscosity;

            var reservoirProperties = new ReservoirProperties
            {
                ReservoirPressure = getReservoirPressure(ppdm39Well, wellPressure),
                BubblePointPressure = getBubblePointPressure(ppdm39Well),
                ProductivityIndex = getProductivityIndex(ppdm39Well),
                WaterCut = getWaterCut(ppdm39Well),
                GasOilRatio = getGasOilRatio(ppdm39Well),
                OilGravity = getOilGravity(ppdm39Well),
                FormationVolumeFactor = getFormationVolumeFactor(ppdm39Well),
                OilViscosity = getOilViscosity(ppdm39Well)
            };

            return reservoirProperties;
        }

        /// <summary>
        /// Maps PPDM39 WELL to ReservoirProperties (implements interface).
        /// </summary>
        ReservoirProperties IPPDM39Mapper<WELL, ReservoirProperties>.MapToDomain(WELL ppdm39Well)
        {
            return MapToReservoirProperties(ppdm39Well, null);
        }


        /// <summary>
        /// Maps WellboreProperties back to PPDM39 WELL (updates existing entity).
        /// </summary>
        public WELL MapToPPDM39(WellboreProperties domainModel, WELL? existingPPDM39Entity = null)
        {
            if (domainModel == null)
                throw new ArgumentNullException(nameof(domainModel));

            // In most cases, we don't map analysis results back to WELL entity directly
            // Analysis results should be stored in ANL_ANALYSIS_REPORT
            // This method is provided for completeness but typically won't be used
            return existingPPDM39Entity ?? new WELL();
        }

        /// <summary>
        /// Maps ReservoirProperties back to PPDM39 WELL (updates existing entity).
        /// </summary>
        WELL IPPDM39Mapper<WELL, ReservoirProperties>.MapToPPDM39(ReservoirProperties domainModel, WELL? existingPPDM39Entity)
        {
            if (domainModel == null)
                throw new ArgumentNullException(nameof(domainModel));

            // Analysis results should be stored in ANL_ANALYSIS_REPORT, not in WELL
            return existingPPDM39Entity ?? new WELL();
        }

        /// <summary>
        /// Maps a collection of PPDM39 WELL entities to WellboreProperties.
        /// </summary>
        public IEnumerable<WellboreProperties> MapToDomain(IEnumerable<WELL> ppdm39Entities)
        {
            return ppdm39Entities?.Select<WELL, WellboreProperties>(w => ((IPPDM39Mapper<WELL, WellboreProperties>)this).MapToDomain(w)) ?? Enumerable.Empty<WellboreProperties>();
        }

        /// <summary>
        /// Maps a collection of PPDM39 WELL entities to ReservoirProperties.
        /// </summary>
        IEnumerable<ReservoirProperties> IPPDM39Mapper<WELL, ReservoirProperties>.MapToDomain(IEnumerable<WELL> ppdm39Entities)
        {
            return ppdm39Entities?.Select(w => ((IPPDM39Mapper<WELL, ReservoirProperties>)this).MapToDomain(w)) 
                ?? Enumerable.Empty<ReservoirProperties>();
        }

        /// <summary>
        /// Maps a collection of WellboreProperties to PPDM39 WELL entities.
        /// </summary>
        public IEnumerable<WELL> MapToPPDM39(IEnumerable<WellboreProperties> domainModels)
        {
            return domainModels?.Select<WellboreProperties, WELL>(w => MapToPPDM39(w)) ?? Enumerable.Empty<WELL>();
        }

        /// <summary>
        /// Maps a collection of ReservoirProperties to PPDM39 WELL entities.
        /// </summary>
        IEnumerable<WELL> IPPDM39Mapper<WELL, ReservoirProperties>.MapToPPDM39(IEnumerable<ReservoirProperties> domainModels)
        {
            return domainModels?.Select(d => ((IPPDM39Mapper<WELL, ReservoirProperties>)this).MapToPPDM39(d)) 
                ?? Enumerable.Empty<WELL>();
        }

    }
}

