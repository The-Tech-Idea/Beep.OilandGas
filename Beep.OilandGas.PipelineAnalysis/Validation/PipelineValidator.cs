using System;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
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
        public static void ValidatePipelineProperties(PIPELINE_PROPERTIES pipeline)
        {
            if (pipeline == null)
                throw new ArgumentNullException(nameof(pipeline));

            if (pipeline.DIAMETER <= 0)
                throw new InvalidPipelinePropertiesException("Pipeline diameter must be greater than zero.");

            if (pipeline.LENGTH <= 0)
                throw new InvalidPipelinePropertiesException("Pipeline length must be greater than zero.");

            if (pipeline.ROUGHNESS < 0)
                throw new InvalidPipelinePropertiesException("Pipeline roughness cannot be negative.");

            if (pipeline.INLET_PRESSURE <= 0)
                throw new InvalidPipelinePropertiesException("Inlet pressure must be greater than zero.");

            if (pipeline.OUTLET_PRESSURE < 0)
                throw new InvalidPipelinePropertiesException("Outlet pressure cannot be negative.");

            if (pipeline.OUTLET_PRESSURE >= pipeline.INLET_PRESSURE)
                throw new InvalidPipelinePropertiesException("Outlet pressure must be less than inlet pressure for flow.");

            if (pipeline.AVERAGE_TEMPERATURE <= 0)
                throw new InvalidPipelinePropertiesException("Average temperature must be greater than zero.");
        }

        /// <summary>
        /// Validates gas pipeline flow properties.
        /// </summary>
        public static void ValidateGasFlowProperties(GAS_PIPELINE_FLOW_PROPERTIES flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            ValidatePipelineProperties(flowProperties.PIPELINE);

            if (flowProperties.GAS_FLOW_RATE < 0)
                throw new InvalidFlowPropertiesException("Gas flow rate cannot be negative.");

            if (flowProperties.GAS_SPECIFIC_GRAVITY <= 0)
                throw new InvalidFlowPropertiesException("Gas specific gravity must be greater than zero.");

            if (flowProperties.BASE_PRESSURE <= 0)
                throw new InvalidFlowPropertiesException("Base pressure must be greater than zero.");

            if (flowProperties.BASE_TEMPERATURE <= 0)
                throw new InvalidFlowPropertiesException("Base temperature must be greater than zero.");
        }

        /// <summary>
        /// Validates liquid pipeline flow properties.
        /// </summary>
        public static void ValidateLiquidFlowProperties(LIQUID_PIPELINE_FLOW_PROPERTIES flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            ValidatePipelineProperties(flowProperties.PIPELINE);

            if (flowProperties.LIQUID_FLOW_RATE < 0)
                throw new InvalidFlowPropertiesException("Liquid flow rate cannot be negative.");

            if (flowProperties.LIQUID_SPECIFIC_GRAVITY <= 0)
                throw new InvalidFlowPropertiesException("Liquid specific gravity must be greater than zero.");

            if (flowProperties.LIQUID_VISCOSITY <= 0)
                throw new InvalidFlowPropertiesException("Liquid viscosity must be greater than zero.");
        }
    }
}

