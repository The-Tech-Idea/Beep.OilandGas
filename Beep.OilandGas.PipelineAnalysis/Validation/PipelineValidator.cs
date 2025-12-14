using System;
using Beep.OilandGas.PipelineAnalysis.Models;
using Beep.OilandGas.PipelineAnalysis.Exceptions;

namespace Beep.OilandGas.PipelineAnalysis.Validation
{
    /// <summary>
    /// Provides validation for pipeline calculations.
    /// </summary>
    public static class PipelineValidator
    {
        /// <summary>
        /// Validates pipeline properties.
        /// </summary>
        public static void ValidatePipelineProperties(PipelineProperties pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            if (pipeline.Diameter <= 0)
                throw new InvalidPipelinePropertiesException("Pipeline diameter must be greater than zero.");

            if (pipeline.Length <= 0)
                throw new InvalidPipelinePropertiesException("Pipeline length must be greater than zero.");

            if (pipeline.Roughness < 0)
                throw new InvalidPipelinePropertiesException("Pipeline roughness cannot be negative.");

            if (pipeline.InletPressure <= 0)
                throw new InvalidPipelinePropertiesException("Inlet pressure must be greater than zero.");

            if (pipeline.OutletPressure < 0)
                throw new InvalidPipelinePropertiesException("Outlet pressure cannot be negative.");

            if (pipeline.OutletPressure >= pipeline.InletPressure)
                throw new InvalidPipelinePropertiesException("Outlet pressure must be less than inlet pressure for flow.");

            if (pipeline.AverageTemperature <= 0)
                throw new InvalidPipelinePropertiesException("Average temperature must be greater than zero.");
        }

        /// <summary>
        /// Validates gas pipeline flow properties.
        /// </summary>
        public static void ValidateGasFlowProperties(GasPipelineFlowProperties flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            ValidatePipelineProperties(flowProperties.Pipeline);

            if (flowProperties.GasFlowRate < 0)
                throw new InvalidFlowPropertiesException("Gas flow rate cannot be negative.");

            if (flowProperties.GasSpecificGravity <= 0)
                throw new InvalidFlowPropertiesException("Gas specific gravity must be greater than zero.");

            if (flowProperties.BasePressure <= 0)
                throw new InvalidFlowPropertiesException("Base pressure must be greater than zero.");

            if (flowProperties.BaseTemperature <= 0)
                throw new InvalidFlowPropertiesException("Base temperature must be greater than zero.");
        }

        /// <summary>
        /// Validates liquid pipeline flow properties.
        /// </summary>
        public static void ValidateLiquidFlowProperties(LiquidPipelineFlowProperties flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            ValidatePipelineProperties(flowProperties.Pipeline);

            if (flowProperties.LiquidFlowRate < 0)
                throw new InvalidFlowPropertiesException("Liquid flow rate cannot be negative.");

            if (flowProperties.LiquidSpecificGravity <= 0)
                throw new InvalidFlowPropertiesException("Liquid specific gravity must be greater than zero.");

            if (flowProperties.LiquidViscosity <= 0)
                throw new InvalidFlowPropertiesException("Liquid viscosity must be greater than zero.");
        }
    }
}

