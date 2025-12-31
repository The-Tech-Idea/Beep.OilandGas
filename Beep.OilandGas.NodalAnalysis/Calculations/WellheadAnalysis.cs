using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.NodalAnalysis;
using OperatingPointModel = Beep.OilandGas.Models.NodalAnalysis.OperatingPoint;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides wellhead nodal analysis methods.
    /// </summary>
    public static class WellheadAnalysis
    {
        /// <summary>
        /// Performs wellhead nodal analysis for oil wells.
        /// </summary>
        /// <param name="reservoir">Reservoir properties.</param>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="depth">Well depth in feet.</param>
        /// <param name="bhpCorrelation">BHP correlation method to use.</param>
        /// <param name="flowRateRange">Range of flow rates to analyze.</param>
        /// <returns>Wellhead nodal analysis results.</returns>
        public static WellheadNodalResult AnalyzeWellheadOil(
            ReservoirProperties reservoir,
            decimal wellheadPressure,
            decimal depth,
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal, decimal> bhpCorrelation,
            (decimal min, decimal max) flowRateRange,
            decimal gasLiquidRatio = 500m,
            decimal oilGravity = 35m,
            decimal gasSpecificGravity = 0.65m,
            decimal temperature = 580m)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (depth <= 0)
                throw new ArgumentException("Depth must be greater than zero.", nameof(depth));

            var result = new WellheadNodalResult
            {
                WellheadPressure = wellheadPressure,
                Depth = depth,
                FlowRates = new List<decimal>(),
                BottomHolePressures = new List<decimal>(),
                WellheadPressures = new List<decimal>()
            };

            int numberOfPoints = 50;
            decimal flowRateStep = (flowRateRange.max - flowRateRange.min) / numberOfPoints;

            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal flowRate = flowRateRange.min + i * flowRateStep;

                // Calculate BHP from wellhead using correlation
                decimal bhp = bhpCorrelation(
                    wellheadPressure, depth, flowRate, gasLiquidRatio,
                    oilGravity, gasSpecificGravity, temperature);

                // Calculate IPR pressure at this flow rate
                decimal iprPressure = CalculateIPRPressure(reservoir, flowRate);

                result.FlowRates.Add(flowRate);
                result.BottomHolePressures.Add(bhp);
                result.WellheadPressures.Add(wellheadPressure);

                // Check for operating point (where BHP from VLP equals IPR pressure)
                if (i > 0 && Math.Abs(bhp - iprPressure) < 10m)
                {
                    result.OperatingPoint = new OperatingPointModel
                    {
                        FlowRate = (double)flowRate,
                        BottomholePressure = (double)((bhp + iprPressure) / 2.0m),
                        WellheadPressure = (double)wellheadPressure
                    };
                }
            }

            return result;
        }

        /// <summary>
        /// Performs wellhead nodal analysis for gas wells.
        /// </summary>
        public static WellheadNodalResult AnalyzeWellheadGas(
            ReservoirProperties reservoir,
            decimal wellheadPressure,
            decimal depth,
            Func<decimal, decimal, decimal, decimal, decimal, decimal, decimal> bhpCorrelation,
            (decimal min, decimal max) flowRateRange,
            decimal gasSpecificGravity = 0.65m,
            decimal temperature = 580m,
            decimal wellheadTemperature = 520m)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            var result = new WellheadNodalResult
            {
                WellheadPressure = wellheadPressure,
                Depth = depth,
                FlowRates = new List<decimal>(),
                BottomHolePressures = new List<decimal>(),
                WellheadPressures = new List<decimal>()
            };

            int numberOfPoints = 50;
            decimal flowRateStep = (flowRateRange.max - flowRateRange.min) / numberOfPoints;

            for (int i = 0; i <= numberOfPoints; i++)
            {
                decimal flowRate = flowRateRange.min + i * flowRateStep;

                // Calculate BHP from wellhead using gas correlation
                decimal bhp = bhpCorrelation(
                    wellheadPressure, depth, flowRate, gasSpecificGravity,
                    temperature, wellheadTemperature);

                // Calculate IPR pressure at this flow rate
                decimal iprPressure = CalculateIPRPressure(reservoir, flowRate);

                result.FlowRates.Add(flowRate);
                result.BottomHolePressures.Add(bhp);
                result.WellheadPressures.Add(wellheadPressure);

                // Check for operating point
                if (i > 0 && Math.Abs(bhp - iprPressure) < 10m)
                {
                    result.OperatingPoint = new OperatingPointModel
                    {
                        FlowRate = (double)flowRate,
                        BottomholePressure = (double)((bhp + iprPressure) / 2.0m),
                        WellheadPressure = (double)wellheadPressure
                    };
                }
            }

            return result;
        }

        private static decimal CalculateIPRPressure(ReservoirProperties reservoir, decimal flowRate)
        {
            // Simplified IPR calculation (Vogel equation)
            decimal qMax = (decimal)(reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8);
            if (qMax <= 0)
                return (decimal)reservoir.ReservoirPressure;

            decimal qRatio = flowRate / qMax;
            if (qRatio >= 1.0m)
                return 0m;

            // Vogel: q/q_max = 1 - 0.2*(Pwf/Pr) - 0.8*(Pwf/Pr)²
            // Solve for Pwf
            decimal a = 0.8m;
            decimal b = 0.2m;
            decimal c = qRatio - 1.0m;

            decimal discriminant = b * b - 4m * a * c;
            if (discriminant < 0)
                return (decimal)reservoir.ReservoirPressure;

            decimal pwfRatio = (-b + (decimal)Math.Sqrt((double)discriminant)) / (2m * a);
            return (decimal)reservoir.ReservoirPressure * Math.Max(0m, Math.Min(1m, pwfRatio));
        }
    }

    /// <summary>
    /// Represents wellhead nodal analysis results.
    /// </summary>
    public class WellheadNodalResult
    {
        public decimal WellheadPressure { get; set; }
        public decimal Depth { get; set; }
        public List<decimal> FlowRates { get; set; } = new();
        public List<decimal> BottomHolePressures { get; set; } = new();
        public List<decimal> WellheadPressures { get; set; } = new();
        public OperatingPointModel? OperatingPoint { get; set; }
    }
}

