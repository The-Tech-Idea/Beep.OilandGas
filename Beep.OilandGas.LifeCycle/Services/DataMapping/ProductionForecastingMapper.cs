using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.ProductionForecasting;
using Beep.OilandGas.PPDM39.Models;


namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to ProductionForecasting models.
    /// </summary>
    public class ProductionForecastingMapper : IPPDM39Mapper<WELL, RESERVOIR_FORECAST_PROPERTIES>
    {
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getInitialPressure;
        private readonly Func<WELL, decimal>? _getPermeability;
        private readonly Func<WELL, decimal>? _getThickness;
        private readonly Func<WELL, decimal>? _getDrainageRadius;
        private readonly Func<WELL, WELL_TUBULAR?, decimal>? _getWellboreRadius;
        private readonly Func<WELL, decimal>? _getFormationVolumeFactor;
        private readonly Func<WELL, decimal>? _getOilViscosity;
        private readonly Func<WELL, decimal>? _getTotalCompressibility;
        private readonly Func<WELL, decimal>? _getPorosity;
        private readonly Func<WELL, decimal>? _getSkinFactor;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getTemperature;

        /// <summary>
        /// Initializes a new instance of ProductionForecastingMapper with default value retrievers.
        /// </summary>
        public ProductionForecastingMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of ProductionForecastingMapper with custom value retrievers.
        /// </summary>
        public ProductionForecastingMapper(
            Func<WELL, WELL_PRESSURE?, decimal>? getInitialPressure = null,
            Func<WELL, decimal>? getPermeability = null,
            Func<WELL, decimal>? getThickness = null,
            Func<WELL, decimal>? getDrainageRadius = null,
            Func<WELL, WELL_TUBULAR?, decimal>? getWellboreRadius = null,
            Func<WELL, decimal>? getFormationVolumeFactor = null,
            Func<WELL, decimal>? getOilViscosity = null,
            Func<WELL, decimal>? getTotalCompressibility = null,
            Func<WELL, decimal>? getPorosity = null,
            Func<WELL, decimal>? getSkinFactor = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getTemperature = null)
        {
            _getInitialPressure = getInitialPressure;
            _getPermeability = getPermeability;
            _getThickness = getThickness;
            _getDrainageRadius = getDrainageRadius;
            _getWellboreRadius = getWellboreRadius;
            _getFormationVolumeFactor = getFormationVolumeFactor;
            _getOilViscosity = getOilViscosity;
            _getTotalCompressibility = getTotalCompressibility;
            _getPorosity = getPorosity;
            _getSkinFactor = getSkinFactor;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getTemperature = getTemperature;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to RESERVOIR_FORECAST_PROPERTIES.
        /// </summary>
        /// <param name="ppdm39Well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity for pressure data.</param>
        /// <param name="tubular">Optional WELL_TUBULAR entity for wellbore radius.</param>
        /// <returns>The mapped RESERVOIR_FORECAST_PROPERTIES.</returns>
        public RESERVOIR_FORECAST_PROPERTIES MapToDomain(
            WELL ppdm39Well,
            WELL_PRESSURE? wellPressure = null,
            WELL_TUBULAR? tubular = null)
        {
            if (ppdm39Well == null)
                throw new ArgumentNullException(nameof(ppdm39Well));

            var getInitialPressure = _getInitialPressure ?? ValueRetrievers.GetReservoirPressureDecimal;
            var getPermeability = _getPermeability ?? ((well) => throw new InvalidOperationException("Permeability not available. Provide getPermeability function."));
            var getThickness = _getThickness ?? ((well) => throw new InvalidOperationException("Thickness not available. Provide getThickness function."));
            var getDrainageRadius = _getDrainageRadius ?? ((well) => throw new InvalidOperationException("Drainage radius not available. Provide getDrainageRadius function."));
            var getWellboreRadius = _getWellboreRadius ?? ValueRetrievers.GetWellboreRadiusDecimal;
            var getFormationVolumeFactor = _getFormationVolumeFactor ?? ValueRetrievers.GetFormationVolumeFactorDecimal;
            var getOilViscosity = _getOilViscosity ?? ValueRetrievers.GetOilViscosityDecimal;
            var getTotalCompressibility = _getTotalCompressibility ?? ((well) => throw new InvalidOperationException("Total compressibility not available. Provide getTotalCompressibility function."));
            var getPorosity = _getPorosity ?? ((well) => throw new InvalidOperationException("Porosity not available. Provide getPorosity function."));
            var getSkinFactor = _getSkinFactor ?? ((well) => throw new InvalidOperationException("Skin factor not available. Provide getSkinFactor function."));
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getTemperature = _getTemperature ?? ValueRetrievers.GetReservoirTemperature;

            var forecastProperties = new RESERVOIR_FORECAST_PROPERTIES
            {
                INITIAL_PRESSURE = getInitialPressure(ppdm39Well, wellPressure),
                PERMEABILITY = getPermeability(ppdm39Well),
                THICKNESS = getThickness(ppdm39Well),
                DRAINAGE_RADIUS = getDrainageRadius(ppdm39Well),
                WELLBORE_RADIUS = getWellboreRadius(ppdm39Well, tubular),
                FORMATION_VOLUME_FACTOR = getFormationVolumeFactor(ppdm39Well),
                OIL_VISCOSITY = getOilViscosity(ppdm39Well),
                TOTAL_COMPRESSIBILITY = getTotalCompressibility(ppdm39Well),
                POROSITY = getPorosity(ppdm39Well),
                SKIN_FACTOR = getSkinFactor(ppdm39Well),
                GAS_SPECIFIC_GRAVITY = getGasSpecificGravity(ppdm39Well),
                TEMPERATURE = getTemperature(ppdm39Well, wellPressure)
            };

            return forecastProperties;
        }

        /// <summary>
        /// Maps PPDM39 WELL to RESERVOIR_FORECAST_PROPERTIES (implements interface).
        /// </summary>
        RESERVOIR_FORECAST_PROPERTIES IPPDM39Mapper<WELL, RESERVOIR_FORECAST_PROPERTIES>.MapToDomain(WELL ppdm39Well)
        {
            return MapToDomain(ppdm39Well, null, null);
        }

        /// <summary>
        /// Maps RESERVOIR_FORECAST_PROPERTIES back to PPDM39 (typically not used - results stored in ANL_ANALYSIS_REPORT).
        /// </summary>
        public WELL MapToPPDM39(RESERVOIR_FORECAST_PROPERTIES domainModel, WELL? existingPPDM39Entity = null)
        {
            // Forecast results should be stored in ANL_ANALYSIS_REPORT, not in WELL
            return existingPPDM39Entity ?? new WELL();
        }

        /// <summary>
        /// Maps a collection of PPDM39 WELL entities to RESERVOIR_FORECAST_PROPERTIES.
        /// </summary>
        public IEnumerable<RESERVOIR_FORECAST_PROPERTIES> MapToDomain(IEnumerable<WELL> ppdm39Entities)
        {
            return ppdm39Entities?.Select<WELL, RESERVOIR_FORECAST_PROPERTIES>(w => ((IPPDM39Mapper<WELL, RESERVOIR_FORECAST_PROPERTIES>)this).MapToDomain(w)) ?? Enumerable.Empty<RESERVOIR_FORECAST_PROPERTIES>();
        }

        /// <summary>
        /// Maps a collection of RESERVOIR_FORECAST_PROPERTIES to PPDM39 WELL entities.
        /// </summary>
        public IEnumerable<WELL> MapToPPDM39(IEnumerable<RESERVOIR_FORECAST_PROPERTIES> domainModels)
        {
            return domainModels?.Select<RESERVOIR_FORECAST_PROPERTIES, WELL>(d => ((IPPDM39Mapper<WELL, RESERVOIR_FORECAST_PROPERTIES>)this).MapToPPDM39(d)) ?? Enumerable.Empty<WELL>();
        }
    }
}

