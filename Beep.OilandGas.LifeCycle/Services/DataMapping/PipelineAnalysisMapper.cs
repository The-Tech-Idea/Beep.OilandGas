using System;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.PipelineAnalysis.Models;

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
        /// Maps PPDM39 WELL and related entities to PipelineProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped PipelineProperties.</returns>
        public PipelineProperties MapToPipelineProperties(
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

            return new PipelineProperties
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
        /// Maps PPDM39 WELL and related entities to GasPipelineFlowProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped GasPipelineFlowProperties.</returns>
        public GasPipelineFlowProperties MapToGasPipelineFlowProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var pipeline = MapToPipelineProperties(well, wellPressure);
            var getGasFlowRate = _getGasFlowRate ?? ((w) => throw new InvalidOperationException("Gas flow rate not available. Provide getGasFlowRate function."));
            var getGasSpecificGravity = _getGasSpecificGravity ?? ValueRetrievers.GetGasSpecificGravityDecimal;
            var getGasMolecularWeight = ValueRetrievers.GetGasMolecularWeight;

            return new GasPipelineFlowProperties
            {
                Pipeline = pipeline,
                GasFlowRate = getGasFlowRate(well),
                GasSpecificGravity = getGasSpecificGravity(well),
                GasMolecularWeight = getGasMolecularWeight(well)
            };
        }

        /// <summary>
        /// Maps PPDM39 WELL and related entities to LiquidPipelineFlowProperties.
        /// </summary>
        /// <param name="well">The PPDM39 WELL entity.</param>
        /// <param name="wellPressure">Optional WELL_PRESSURE entity.</param>
        /// <returns>The mapped LiquidPipelineFlowProperties.</returns>
        public LiquidPipelineFlowProperties MapToLiquidPipelineFlowProperties(
            WELL well,
            WELL_PRESSURE? wellPressure = null)
        {
            if (well == null)
                throw new ArgumentNullException(nameof(well));

            var pipeline = MapToPipelineProperties(well, wellPressure);
            var getLiquidFlowRate = _getLiquidFlowRate ?? ((w) => throw new InvalidOperationException("Liquid flow rate not available. Provide getLiquidFlowRate function."));
            var getLiquidSpecificGravity = _getLiquidSpecificGravity ?? ValueRetrievers.GetLiquidSpecificGravityFromAPIGravity;
            var getLiquidViscosity = _getLiquidViscosity ?? ValueRetrievers.GetOilViscosityDecimal;

            return new LiquidPipelineFlowProperties
            {
                Pipeline = pipeline,
                LiquidFlowRate = getLiquidFlowRate(well),
                LiquidSpecificGravity = getLiquidSpecificGravity(well),
                LiquidViscosity = getLiquidViscosity(well)
            };
        }
    }
}

