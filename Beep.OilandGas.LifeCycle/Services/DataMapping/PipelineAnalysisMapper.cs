using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.LifeCycle.Services.DataMapping
{
    /// <summary>
    /// Maps PPDM39 entities to PipelineAnalysis models.
    /// </summary>
    public class PipelineAnalysisMapper
    {
        private readonly Func<WELL, decimal>? _getPipelineDiameter;
        private readonly Func<WELL, decimal>? _getPipelineLength;
        private readonly Func<WELL, decimal>? _getPipelineRoughness;
        private readonly Func<WELL, decimal>? _getElevationChange;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getInletPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getOutletPressure;
        private readonly Func<WELL, WELL_PRESSURE?, decimal>? _getAverageTemperature;
        private readonly Func<WELL, decimal>? _getGasFlowRate;
        private readonly Func<WELL, decimal>? _getLiquidFlowRate;
        private readonly Func<WELL, decimal>? _getGasSpecificGravity;
        private readonly Func<WELL, decimal>? _getLiquidSpecificGravity;
        private readonly Func<WELL, decimal>? _getLiquidViscosity;

        /// <summary>
        /// Initializes a new instance of PipelineAnalysisMapper with default value retrievers.
        /// </summary>
        public PipelineAnalysisMapper()
        {
        }

        /// <summary>
        /// Initializes a new instance of PipelineAnalysisMapper with custom value retrievers.
        /// </summary>
        public PipelineAnalysisMapper(
            Func<WELL, decimal>? getPipelineDiameter = null,
            Func<WELL, decimal>? getPipelineLength = null,
            Func<WELL, decimal>? getPipelineRoughness = null,
            Func<WELL, decimal>? getElevationChange = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getInletPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getOutletPressure = null,
            Func<WELL, WELL_PRESSURE?, decimal>? getAverageTemperature = null,
            Func<WELL, decimal>? getGasFlowRate = null,
            Func<WELL, decimal>? getLiquidFlowRate = null,
            Func<WELL, decimal>? getGasSpecificGravity = null,
            Func<WELL, decimal>? getLiquidSpecificGravity = null,
            Func<WELL, decimal>? getLiquidViscosity = null)
        {
            _getPipelineDiameter = getPipelineDiameter;
            _getPipelineLength = getPipelineLength;
            _getPipelineRoughness = getPipelineRoughness;
            _getElevationChange = getElevationChange;
            _getInletPressure = getInletPressure;
            _getOutletPressure = getOutletPressure;
            _getAverageTemperature = getAverageTemperature;
            _getGasFlowRate = getGasFlowRate;
            _getLiquidFlowRate = getLiquidFlowRate;
            _getGasSpecificGravity = getGasSpecificGravity;
            _getLiquidSpecificGravity = getLiquidSpecificGravity;
            _getLiquidViscosity = getLiquidViscosity;
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to PIPELINE_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped PIPELINE_PROPERTIES.</returns>
        public PIPELINE_PROPERTIES MapToPipelineProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var getPipelineDiameter = _getPipelineDiameter ?? ((w) => throw new InvalidOperationException("Pipeline diameter not available. Provide getPipelineDiameter function."));
            var getPipelineLength = _getPipelineLength ?? ((w) => throw new InvalidOperationException("Pipeline length not available. Provide getPipelineLength function."));
            var getPipelineRoughness = _getPipelineRoughness ?? ValueRetrievers.GetDefaultPipelineRoughness;
            var getElevationChange = _getElevationChange ?? ValueRetrievers.GetDefaultElevationChange;
            var getInletPressure = _getInletPressure ?? ValueRetrievers.GetWellheadPressureDecimal;
            var getOutletPressure = _getOutletPressure ?? ValueRetrievers.GetDownstreamPressure90Percent;
            var getAverageTemperature = _getAverageTemperature ?? ValueRetrievers.GetWellheadTemperatureInRankine;

            return new PIPELINE_PROPERTIES
            {
                Diameter = getPipelineDiameter(well),
                Length = getPipelineLength(well),
                Roughness = getPipelineRoughness(well),
                ElevationChange = getElevationChange(well),
                InletPressure = getInletPressure(well, wellPressure),
                OutletPressure = getOutletPressure(well, wellPressure),
                AverageTemperature = getAverageTemperature(well, wellPressure)
            };
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to GAS_PIPELINE_FLOW_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped GAS_PIPELINE_FLOW_PROPERTIES.</returns>
        public GAS_PIPELINE_FLOW_PROPERTIES MapToGasPipelineFlowProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var pipeline = MapToPipelineProperties(well, wellPressure);
            var getGasFlowRate = _getGasFlowRate ?? ((w) => throw new InvalidOperationException("Gas flow rate not available. Provide getGasFlowRate function."));
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getGasMolecularWeight = ValueRetrievers.GetGasMolecularWeight;

            return new GAS_PIPELINE_FLOW_PROPERTIES
            {
                Pipeline = pipeline,
                GasFlowRate = getGasFlowRate(well),
                GasSpecificGravity = getGasSpecificGravity(well),
                GasMolecularWeight = getGasMolecularWeight(well)
            };
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to LIQUID_PIPELINE_FLOW_PROPERTIES.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped LIQUID_PIPELINE_FLOW_PROPERTIES.</returns>
        public LIQUID_PIPELINE_FLOW_PROPERTIES MapToLiquidPipelineFlowProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var pipeline = MapToPipelineProperties(well, wellPressure);
            var getLiquidFlowRate = _getLiquidFlowRate ?? ((w) => throw new InvalidOperationException("Liquid flow rate not available. Provide getLiquidFlowRate function."));
            var getLiquidSpecificGravity = _getLiquidSpecificGravity ?? ValueRetrievers.GetLiquidSpecificGravityFromAPIGravity;
            var getLiquidViscosity = _getLiquidViscosity ?? ValueRetrievers.GetOilViscosityDecimal;

            return new LIQUID_PIPELINE_FLOW_PROPERTIES
            {
                Pipeline = pipeline,
                LiquidFlowRate = getLiquidFlowRate(well),
                LiquidSpecificGravity = getLiquidSpecificGravity(well),
                LiquidViscosity = getLiquidViscosity(well)
            };
        }
    }
}

