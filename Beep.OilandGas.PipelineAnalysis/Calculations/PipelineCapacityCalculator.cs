using System;

using Beep.OilandGas.GasProperties.Calculations;
using Beep.OilandGas.Models.Data.PipelineAnalysis;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Provides pipeline capacity calculations.
    /// </summary>
    public static class PipelineCapacityCalculator
    {
        /// <summary>
        /// Calculates gas capacity using Weymouth (diameter in inches, length in miles,
        /// absolute pressures in psia and temperatures in Rankine). Capacity is Mscf/day;
        /// velocity is ft/s and gradient psi/ft. Standard-condition compressibility is 1.
        /// </summary>
        /// <param name="flowProperties">Gas pipeline flow properties.</param>
        /// <returns>Pipeline capacity results.</returns>
        public static PIPELINE_CAPACITY_RESULT CalculateGasPipelineCapacity(
            GAS_PIPELINE_FLOW_PROPERTIES flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            if (flowProperties.PIPELINE == null)
                throw new ArgumentNullException(nameof(flowProperties.PIPELINE));

            var result = new PIPELINE_CAPACITY_RESULT();

            var pipeline = flowProperties.PIPELINE;
            if (pipeline.DIAMETER <= 0 || pipeline.LENGTH <= 0 || pipeline.AVERAGE_TEMPERATURE <= 0 ||
                pipeline.INLET_PRESSURE <= 0 || pipeline.OUTLET_PRESSURE < 0 || pipeline.ROUGHNESS < 0 ||
                flowProperties.GAS_SPECIFIC_GRAVITY <= 0 || flowProperties.BASE_PRESSURE <= 0 || flowProperties.BASE_TEMPERATURE <= 0)
                throw new ArgumentException("Explicit positive pipe dimensions, absolute conditions, gas gravity and standard conditions are required.", nameof(flowProperties));

            // Calculate average pressure
            decimal averagePressure = (pipeline.INLET_PRESSURE + pipeline.OUTLET_PRESSURE) / 2m;

            // Calculate Z-factor at average conditions
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, pipeline.AVERAGE_TEMPERATURE, flowProperties.GAS_SPECIFIC_GRAVITY);

            // Weymouth's 433.5 coefficient uses inches and miles; its diameter exponent
            // already embodies the empirical friction relation. Do not apply another 1/sqrt(f).
            decimal diameterFt = pipeline.DIAMETER / 12m; // feet
            decimal pressureDifferenceSquared = pipeline.INLET_PRESSURE * pipeline.INLET_PRESSURE -
                                               pipeline.OUTLET_PRESSURE * pipeline.OUTLET_PRESSURE;

            if (pressureDifferenceSquared <= 0)
            {
                result.MAXIMUM_FLOW_RATE = 0m;
                result.PRESSURE_DROP = 0m;
                return result;
            }

            // Weymouth equation
            decimal constant = 433.5m * flowProperties.BASE_TEMPERATURE / flowProperties.BASE_PRESSURE;
            decimal diameterTerm = (decimal)Math.Pow((double)pipeline.DIAMETER, 2.667);
            decimal pressureTerm = (decimal)Math.Sqrt((double)(pressureDifferenceSquared / 
                                                               (pipeline.LENGTH * pipeline.AVERAGE_TEMPERATURE * zFactor * flowProperties.GAS_SPECIFIC_GRAVITY)));

            decimal maximumFlowRate = constant * diameterTerm * pressureTerm;

            // Convert to Mscf/day
            result.MAXIMUM_FLOW_RATE = maximumFlowRate / 1000m; // Mscf/day

            // Calculate pressure drop
            result.PRESSURE_DROP = pipeline.INLET_PRESSURE - pipeline.OUTLET_PRESSURE;

            // Calculate flow velocity
            decimal pipelineArea = (decimal)Math.PI * diameterFt * diameterFt / 4m; // square feet
            decimal flowRateFt3PerSec = maximumFlowRate / 86400m *
                (flowProperties.BASE_PRESSURE / averagePressure) *
                (pipeline.AVERAGE_TEMPERATURE / flowProperties.BASE_TEMPERATURE) * zFactor;
            result.FLOW_VELOCITY = flowRateFt3PerSec / pipelineArea; // ft/s

            // Calculate Reynolds number
            result.REYNOLDS_NUMBER = CalculateReynoldsNumber(
                flowProperties.GAS_SPECIFIC_GRAVITY, result.FLOW_VELOCITY, pipeline.DIAMETER,
                averagePressure, pipeline.AVERAGE_TEMPERATURE, zFactor);

            // Report the existing hydraulic helper at the calculated Reynolds number.
            // This diagnostic is not an extra multiplier in the Weymouth capacity relation.
            result.FRICTION_FACTOR = PipelineCalculator.CalculateFrictionFactor(result.REYNOLDS_NUMBER, pipeline.ROUGHNESS, diameterFt);

            // Pressure gradient
            result.PRESSURE_GRADIENT = result.PRESSURE_DROP / (pipeline.LENGTH * 5280m); // psi/ft from miles

            result.OUTLET_PRESSURE = pipeline.OUTLET_PRESSURE;

            return result;
        }

        /// <summary>
        /// Calculates liquid pipeline capacity.
        /// </summary>
        /// <param name="flowProperties">Liquid pipeline flow properties.</param>
        /// <returns>Pipeline capacity results.</returns>
        public static PIPELINE_CAPACITY_RESULT CalculateLiquidPipelineCapacity(
            LIQUID_PIPELINE_FLOW_PROPERTIES flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            if (flowProperties.PIPELINE == null)
                throw new ArgumentNullException(nameof(flowProperties.PIPELINE));

            var result = new PIPELINE_CAPACITY_RESULT();

            var pipeline = flowProperties.PIPELINE;

            // Calculate pipeline capacity using Darcy-Weisbach equation
            // Simplified: Q = sqrt((2 * g * D * h) / (f * L))
            // Where h = pressure head difference

            decimal diameterFt = pipeline.DIAMETER / 12m; // feet
            decimal pipelineArea = (decimal)Math.PI * diameterFt * diameterFt / 4m; // square feet

            // Calculate pressure head
            decimal liquidDensity = flowProperties.LIQUID_SPECIFIC_GRAVITY * 62.4m; // lb/ftÂ³
            decimal pressureHead = (pipeline.INLET_PRESSURE - pipeline.OUTLET_PRESSURE) * 144m / liquidDensity; // feet

            if (pressureHead <= 0)
            {
                result.MAXIMUM_FLOW_RATE = 0m;
                result.PRESSURE_DROP = 0m;
                return result;
            }

            // Calculate friction factor (iterative)
            decimal frictionFactor = CalculateLiquidFrictionFactor(
                pipeline.DIAMETER, pipeline.ROUGHNESS, flowProperties.LIQUID_FLOW_RATE,
                flowProperties.LIQUID_SPECIFIC_GRAVITY, flowProperties.LIQUID_VISCOSITY);

            // Calculate flow velocity
            decimal flowVelocity = (decimal)Math.Sqrt((double)(2m * 32.174m * diameterFt * pressureHead / (frictionFactor * pipeline.LENGTH)));

            // Calculate flow rate
            decimal flowRateFt3PerSec = flowVelocity * pipelineArea; // ftÂ³/s
            decimal flowRateBblPerDay = flowRateFt3PerSec * 86400m / 5.615m; // bbl/day

            result.MAXIMUM_FLOW_RATE = flowRateBblPerDay;
            result.PRESSURE_DROP = pipeline.INLET_PRESSURE  - pipeline.OUTLET_PRESSURE;
            result.FLOW_VELOCITY = flowVelocity;

            // Calculate Reynolds number
            result.REYNOLDS_NUMBER = CalculateLiquidReynoldsNumber(
                flowProperties.LIQUID_SPECIFIC_GRAVITY, flowVelocity, pipeline.DIAMETER, flowProperties.LIQUID_VISCOSITY);

            result.FRICTION_FACTOR = frictionFactor;
            result.PRESSURE_GRADIENT = result.PRESSURE_DROP / pipeline.LENGTH;
            result.OUTLET_PRESSURE = pipeline.OUTLET_PRESSURE;

            return result;
        }

        /// <summary>
        /// Calculates friction factor for liquid flow.
        /// </summary>
        private static decimal CalculateLiquidFrictionFactor(
            decimal diameter,
            decimal roughness,
            decimal flowRate,
            decimal specificGravity,
            decimal viscosity)
        {
            // Calculate Reynolds number
            decimal reynoldsNumber = CalculateLiquidReynoldsNumber(specificGravity, 0m, diameter, viscosity);

            if (reynoldsNumber < 2000m)
            {
                // Laminar flow
                return 64m / reynoldsNumber;
            }
            else
            {
                // Turbulent flow - Swamee-Jain approximation
                decimal relativeRoughness = roughness / (diameter / 12m);
                decimal logArg = relativeRoughness / 3.7m + 5.74m / (decimal)Math.Pow((double)reynoldsNumber, 0.9);
                decimal frictionFactor = 0.25m / (decimal)Math.Pow((double)Math.Log10((double)logArg), 2.0);

                return Math.Max(0.01m, Math.Min(0.1m, frictionFactor));
            }
        }

        /// <summary>
        /// Calculates Reynolds number for gas flow.
        /// </summary>
        private static decimal CalculateReynoldsNumber(
            decimal gasSpecificGravity,
            decimal velocity,
            decimal diameter,
            decimal pressure,
            decimal temperature,
            decimal zFactor)
        {
            // Re = (Ï * v * D) / Î¼
            decimal gasDensity = (pressure * gasSpecificGravity * 28.9645m) / (zFactor * 10.7316m * temperature);
            decimal gasViscosity = 0.02m; // cp (simplified)
            decimal gasViscosityLbPerFtS = gasViscosity * 0.000672m; // lb/(ft-s)

            if (velocity <= 0m)
            {
                // Estimate velocity for Reynolds calculation
                decimal diameterFt = diameter / 12m;
                decimal area = (decimal)Math.PI * diameterFt * diameterFt / 4m;
                velocity = 10m; // ft/s (typical estimate)
            }

            decimal reynoldsNumber = gasDensity * velocity * (diameter / 12m) / gasViscosityLbPerFtS;

            return Math.Max(1m, reynoldsNumber);
        }

        /// <summary>
        /// Calculates Reynolds number for liquid flow.
        /// </summary>
        private static decimal CalculateLiquidReynoldsNumber(
            decimal specificGravity,
            decimal velocity,
            decimal diameter,
            decimal viscosity)
        {
            // Re = (Ï * v * D) / Î¼
            decimal liquidDensity = specificGravity * 62.4m; // lb/ftÂ³
            decimal viscosityLbPerFtS = viscosity * 0.000672m; // lb/(ft-s)

            if (velocity <= 0m)
            {
                // Estimate velocity for Reynolds calculation
                decimal diameterFt = diameter / 12m;
                decimal area = (decimal)Math.PI * diameterFt * diameterFt / 4m;
                velocity = 5m; // ft/s (typical estimate)
            }

            decimal reynoldsNumber = liquidDensity * velocity * (diameter / 12m) / viscosityLbPerFtS;

            return Math.Max(1m, reynoldsNumber);
        }
    }
}

