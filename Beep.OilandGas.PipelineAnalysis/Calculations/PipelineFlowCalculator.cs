using System;
using Beep.OilandGas.Models.Data.PipelineAnalysis;
using Beep.OilandGas.GasProperties.Calculations;

namespace Beep.OilandGas.PipelineAnalysis.Calculations
{
    /// <summary>
    /// Provides pipeline flow rate and pressure drop calculations.
    /// </summary>
    public static class PipelineFlowCalculator
    {
        /// <summary>
        /// Calculates gas pipeline flow rate for given pressure drop.
        /// </summary>
        /// <param name="flowProperties">Gas pipeline flow properties.</param>
        /// <returns>Pipeline flow analysis results.</returns>
        public static PipelineFlowAnalysisResult CalculateGasFlow(
            GasPipelineFlowProperties flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            if (flowProperties.Pipeline == null)
                throw new ArgumentNullException(nameof(flowProperties.Pipeline));

            var result = new PipelineFlowAnalysisResult();

            var pipeline = flowProperties.Pipeline;

            // Calculate average pressure
            decimal averagePressure = (pipeline.InletPressure + pipeline.OutletPressure) / 2m;

            // Calculate Z-factor
            decimal zFactor = ZFactorCalculator.CalculateBrillBeggs(
                averagePressure, pipeline.AverageTemperature, flowProperties.GasSpecificGravity);

            // Calculate flow rate using Weymouth equation
            decimal diameterFt = pipeline.Diameter / 12m;
            decimal pressureDifferenceSquared = pipeline.InletPressure * pipeline.InletPressure -
                                               pipeline.OutletPressure * pipeline.OutletPressure;

            if (pressureDifferenceSquared <= 0)
            {
                result.FlowRate = 0m;
                result.PressureDrop = 0m;
                return result;
            }

            // Initial friction factor estimate
            decimal frictionFactor = 0.02m; // Initial estimate

            // Iterate to find correct friction factor
            for (int i = 0; i < 10; i++)
            {
                // Calculate flow rate
                decimal constant = 433.5m * flowProperties.BaseTemperature / flowProperties.BasePressure;
                decimal diameterTerm = (decimal)Math.Pow((double)diameterFt, 2.667);
                decimal pressureTerm = (decimal)Math.Sqrt((double)(pressureDifferenceSquared /
                                                                  (pipeline.Length * pipeline.AverageTemperature * zFactor * flowProperties.GasSpecificGravity)));

                decimal flowRate = constant * diameterTerm * pressureTerm / (decimal)Math.Sqrt((double)frictionFactor);

                // Update friction factor based on flow rate
                decimal reynoldsNumber = CalculateReynoldsNumber(
                    flowProperties.GasSpecificGravity, flowRate, pipeline.Diameter,
                    averagePressure, pipeline.AverageTemperature, zFactor);

                decimal newFrictionFactor = CalculateFrictionFactorFromReynolds(
                    reynoldsNumber, pipeline.Roughness, pipeline.Diameter);

                if (Math.Abs(newFrictionFactor - frictionFactor) < 0.001m)
                    break;

                frictionFactor = newFrictionFactor;
            }

            // Final flow rate calculation
            decimal constantFinal = 433.5m * flowProperties.BaseTemperature / flowProperties.BasePressure;
            decimal diameterTermFinal = (decimal)Math.Pow((double)diameterFt, 2.667);
            decimal pressureTermFinal = (decimal)Math.Sqrt((double)(pressureDifferenceSquared /
                                                                    (pipeline.Length * pipeline.AverageTemperature * zFactor * flowProperties.GasSpecificGravity)));

            decimal flowRateFinal = constantFinal * diameterTermFinal * pressureTermFinal / (decimal)Math.Sqrt((double)frictionFactor);

            result.FlowRate = flowRateFinal / 1000m; // Mscf/day
            result.PressureDrop = pipeline.InletPressure - pipeline.OutletPressure;

            // Calculate flow velocity
            decimal pipelineArea = (decimal)Math.PI * diameterFt * diameterFt / 4m;
            decimal gasDensity = (averagePressure * flowProperties.GasSpecificGravity * 28.9645m) /
                                (zFactor * 10.7316m * pipeline.AverageTemperature);
            decimal flowRateFt3PerSec = flowRateFinal * 1000m / 86400m * 379.0m / 1000m; // Simplified
            result.FlowVelocity = flowRateFt3PerSec / pipelineArea;

            // Calculate Reynolds number
            result.ReynoldsNumber = CalculateReynoldsNumber(
                flowProperties.GasSpecificGravity, result.FlowVelocity, pipeline.Diameter,
                averagePressure, pipeline.AverageTemperature, zFactor);

            result.FrictionFactor = frictionFactor;
            result.PressureGradient = result.PressureDrop / pipeline.Length;
            result.OutletPressure = pipeline.OutletPressure;

            // Determine flow regime
            if (result.ReynoldsNumber < 2000m)
                result.FlowRegime = "Laminar";
            else if (result.ReynoldsNumber < 4000m)
                result.FlowRegime = "Transitional";
            else
                result.FlowRegime = "Turbulent";

            return result;
        }

        /// <summary>
        /// Calculates liquid pipeline flow rate for given pressure drop.
        /// </summary>
        public static PipelineFlowAnalysisResult CalculateLiquidFlow(
            LiquidPipelineFlowProperties flowProperties)
        {
            if (flowProperties == null)
                throw new ArgumentNullException(nameof(flowProperties));

            if (flowProperties.Pipeline == null)
                throw new ArgumentNullException(nameof(flowProperties.Pipeline));

            var result = new PipelineFlowAnalysisResult();

            var pipeline = flowProperties.Pipeline;

            // Calculate pressure head
            decimal liquidDensity = flowProperties.LiquidSpecificGravity * 62.4m;
            decimal pressureHead = (pipeline.InletPressure - pipeline.OutletPressure) * 144m / liquidDensity;

            if (pressureHead <= 0)
            {
                result.FlowRate = 0m;
                result.PressureDrop = 0m;
                return result;
            }

            // Calculate flow using Darcy-Weisbach equation
            decimal diameterFt = pipeline.Diameter / 12m;
            decimal pipelineArea = (decimal)Math.PI * diameterFt * diameterFt / 4m;

            // Initial friction factor estimate
            decimal frictionFactor = 0.02m;

            // Iterate to find correct friction factor
            for (int i = 0; i < 10; i++)
            {
                // Calculate flow velocity
                decimal flowVelocity = (decimal)Math.Sqrt((double)(2m * 32.174m * diameterFt * pressureHead / (frictionFactor * pipeline.Length)));

                // Calculate Reynolds number
                decimal reynoldsNumber = CalculateLiquidReynoldsNumber(
                    flowProperties.LiquidSpecificGravity, flowVelocity, pipeline.Diameter, flowProperties.LiquidViscosity);

                // Update friction factor
                decimal newFrictionFactor = CalculateFrictionFactorFromReynolds(
                    reynoldsNumber, pipeline.Roughness, pipeline.Diameter);

                if (Math.Abs(newFrictionFactor - frictionFactor) < 0.001m)
                    break;

                frictionFactor = newFrictionFactor;
            }

            // Final flow velocity
            decimal flowVelocityFinal = (decimal)Math.Sqrt((double)(2m * 32.174m * diameterFt * pressureHead / (frictionFactor * pipeline.Length)));

            // Calculate flow rate
            decimal flowRateFt3PerSec = flowVelocityFinal * pipelineArea;
            decimal flowRateBblPerDay = flowRateFt3PerSec * 86400m / 5.615m;

            result.FlowRate = flowRateBblPerDay;
            result.PressureDrop = pipeline.InletPressure - pipeline.OutletPressure;
            result.FlowVelocity = flowVelocityFinal;

            // Calculate Reynolds number
            result.ReynoldsNumber = CalculateLiquidReynoldsNumber(
                flowProperties.LiquidSpecificGravity, flowVelocityFinal, pipeline.Diameter, flowProperties.LiquidViscosity);

            result.FrictionFactor = frictionFactor;
            result.PressureGradient = result.PressureDrop / pipeline.Length;
            result.OutletPressure = pipeline.OutletPressure;

            // Determine flow regime
            if (result.ReynoldsNumber < 2000m)
                result.FlowRegime = "Laminar";
            else if (result.ReynoldsNumber < 4000m)
                result.FlowRegime = "Transitional";
            else
                result.FlowRegime = "Turbulent";

            return result;
        }

        /// <summary>
        /// Calculates pressure drop for given gas flow rate.
        /// </summary>
        public static decimal CalculateGasPressureDrop(
            GasPipelineFlowProperties flowProperties)
        {
            var flowResult = CalculateGasFlow(flowProperties);
            return flowResult.PressureDrop;
        }

        /// <summary>
        /// Calculates pressure drop for given liquid flow rate.
        /// </summary>
        public static decimal CalculateLiquidPressureDrop(
            LiquidPipelineFlowProperties flowProperties)
        {
            var flowResult = CalculateLiquidFlow(flowProperties);
            return flowResult.PressureDrop;
        }

        // Helper methods

        private static decimal CalculateReynoldsNumber(
            decimal gasSpecificGravity,
            decimal velocity,
            decimal diameter,
            decimal pressure,
            decimal temperature,
            decimal zFactor)
        {
            decimal gasDensity = (pressure * gasSpecificGravity * 28.9645m) / (zFactor * 10.7316m * temperature);
            decimal gasViscosity = 0.02m * 0.000672m; // lb/(ft-s)

            if (velocity <= 0m)
                velocity = 10m; // Estimate

            return gasDensity * velocity * (diameter / 12m) / gasViscosity;
        }

        private static decimal CalculateLiquidReynoldsNumber(
            decimal specificGravity,
            decimal velocity,
            decimal diameter,
            decimal viscosity)
        {
            decimal liquidDensity = specificGravity * 62.4m;
            decimal viscosityLbPerFtS = viscosity * 0.000672m;

            return liquidDensity * velocity * (diameter / 12m) / viscosityLbPerFtS;
        }

        private static decimal CalculateFrictionFactorFromReynolds(
            decimal reynoldsNumber,
            decimal roughness,
            decimal diameter)
        {
            if (reynoldsNumber < 2000m)
                return 64m / reynoldsNumber;

            decimal relativeRoughness = roughness / (diameter / 12m);
            decimal logArg = relativeRoughness / 3.7m + 5.74m / (decimal)Math.Pow((double)reynoldsNumber, 0.9);
            decimal frictionFactor = 0.25m / (decimal)Math.Pow((double)Math.Log10((double)logArg), 2.0);

            return Math.Max(0.01m, Math.Min(0.1m, frictionFactor));
        }
    }
}

