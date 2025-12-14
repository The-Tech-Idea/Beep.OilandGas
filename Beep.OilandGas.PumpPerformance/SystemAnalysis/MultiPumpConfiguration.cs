using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.PumpPerformance.Calculations;
using Beep.OilandGas.PumpPerformance.Exceptions;
using Beep.OilandGas.PumpPerformance.Validation;

namespace Beep.OilandGas.PumpPerformance.SystemAnalysis
{
    /// <summary>
    /// Represents a pump configuration result.
    /// </summary>
    public class PumpConfigurationResult
    {
        /// <summary>
        /// Gets or sets the total flow rate in GPM.
        /// </summary>
        public double TotalFlowRate { get; set; }

        /// <summary>
        /// Gets or sets the total head in feet.
        /// </summary>
        public double TotalHead { get; set; }

        /// <summary>
        /// Gets or sets the total power requirement in horsepower.
        /// </summary>
        public double TotalPower { get; set; }

        /// <summary>
        /// Gets or sets the overall efficiency.
        /// </summary>
        public double OverallEfficiency { get; set; }

        /// <summary>
        /// Gets or sets individual pump operating points.
        /// </summary>
        public List<PumpOperatingPoint> PumpOperatingPoints { get; set; }

        public PumpConfigurationResult()
        {
            PumpOperatingPoints = new List<PumpOperatingPoint>();
        }
    }

    /// <summary>
    /// Represents an operating point for a single pump.
    /// </summary>
    public class PumpOperatingPoint
    {
        /// <summary>
        /// Gets or sets the pump identifier.
        /// </summary>
        public string PumpId { get; set; }

        /// <summary>
        /// Gets or sets the flow rate in GPM.
        /// </summary>
        public double FlowRate { get; set; }

        /// <summary>
        /// Gets or sets the head in feet.
        /// </summary>
        public double Head { get; set; }

        /// <summary>
        /// Gets or sets the power in horsepower.
        /// </summary>
        public double Power { get; set; }

        /// <summary>
        /// Gets or sets the efficiency.
        /// </summary>
        public double Efficiency { get; set; }
    }

    /// <summary>
    /// Provides calculations for multiple pump configurations (series and parallel).
    /// </summary>
    public static class MultiPumpConfiguration
    {
        /// <summary>
        /// Calculates performance for pumps in series.
        /// In series: Flow rate is constant, head is additive.
        /// </summary>
        /// <param name="pumpCurves">List of H-Q curves for each pump (same flow rates).</param>
        /// <param name="systemCurve">System resistance curve.</param>
        /// <returns>Configuration result with combined performance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
        /// <exception cref="InvalidInputException">Thrown when input is invalid.</exception>
        public static PumpConfigurationResult CalculateSeriesConfiguration(
            List<List<HeadQuantityPoint>> pumpCurves,
            List<SystemCurvePoint> systemCurve)
        {
            if (pumpCurves == null || pumpCurves.Count == 0)
                throw new ArgumentNullException(nameof(pumpCurves), 
                    "Pump curves list cannot be null or empty.");

            if (systemCurve == null || systemCurve.Count == 0)
                throw new ArgumentNullException(nameof(systemCurve), 
                    "System curve cannot be null or empty.");

            // Combine pump curves in series (add heads at same flow rates)
            var combinedCurve = CombinePumpCurvesSeries(pumpCurves);

            // Find operating point
            var operatingPoint = SystemCurveCalculations.FindOperatingPoint(combinedCurve, systemCurve);

            if (!operatingPoint.HasValue)
                throw new InvalidInputException("System", 
                    "No operating point found for series pump configuration.");

            var result = new PumpConfigurationResult
            {
                TotalFlowRate = operatingPoint.Value.flowRate,
                TotalHead = operatingPoint.Value.head
            };

            // Calculate individual pump operating points
            foreach (var pumpCurve in pumpCurves)
            {
                double individualHead = HeadQuantityCalculations.InterpolateHead(
                    pumpCurve, operatingPoint.Value.flowRate);

                var point = new PumpOperatingPoint
                {
                    PumpId = $"Pump {pumpCurves.IndexOf(pumpCurve) + 1}",
                    FlowRate = operatingPoint.Value.flowRate,
                    Head = individualHead
                };

                // Find power and efficiency from curve if available
                var curvePoint = pumpCurve.FirstOrDefault(p => 
                    Math.Abs(p.FlowRate - operatingPoint.Value.flowRate) < 0.1);
                if (curvePoint != null)
                {
                    point.Power = curvePoint.Power;
                    point.Efficiency = curvePoint.Efficiency;
                    result.TotalPower += curvePoint.Power;
                }

                result.PumpOperatingPoints.Add(point);
            }

            // Calculate overall efficiency
            if (result.TotalPower > 0)
            {
                double hydraulicPower = (result.TotalFlowRate * result.TotalHead) / 
                                       Constants.PumpConstants.HorsepowerConversionFactor;
                result.OverallEfficiency = hydraulicPower / result.TotalPower;
            }

            return result;
        }

        /// <summary>
        /// Calculates performance for pumps in parallel.
        /// In parallel: Head is constant, flow rate is additive.
        /// </summary>
        /// <param name="pumpCurves">List of H-Q curves for each pump.</param>
        /// <param name="systemCurve">System resistance curve.</param>
        /// <returns>Configuration result with combined performance.</returns>
        /// <exception cref="ArgumentNullException">Thrown when input is null.</exception>
        /// <exception cref="InvalidInputException">Thrown when input is invalid.</exception>
        public static PumpConfigurationResult CalculateParallelConfiguration(
            List<List<HeadQuantityPoint>> pumpCurves,
            List<SystemCurvePoint> systemCurve)
        {
            if (pumpCurves == null || pumpCurves.Count == 0)
                throw new ArgumentNullException(nameof(pumpCurves), 
                    "Pump curves list cannot be null or empty.");

            if (systemCurve == null || systemCurve.Count == 0)
                throw new ArgumentNullException(nameof(systemCurve), 
                    "System curve cannot be null or empty.");

            // Combine pump curves in parallel (add flow rates at same heads)
            var combinedCurve = CombinePumpCurvesParallel(pumpCurves);

            // Find operating point
            var operatingPoint = SystemCurveCalculations.FindOperatingPoint(combinedCurve, systemCurve);

            if (!operatingPoint.HasValue)
                throw new InvalidInputException("System", 
                    "No operating point found for parallel pump configuration.");

            var result = new PumpConfigurationResult
            {
                TotalFlowRate = operatingPoint.Value.flowRate,
                TotalHead = operatingPoint.Value.head
            };

            // Calculate individual pump operating points
            double flowPerPump = operatingPoint.Value.flowRate / pumpCurves.Count;

            foreach (var pumpCurve in pumpCurves)
            {
                double individualFlow = flowPerPump;
                double individualHead = HeadQuantityCalculations.InterpolateHead(
                    pumpCurve, individualFlow);

                var point = new PumpOperatingPoint
                {
                    PumpId = $"Pump {pumpCurves.IndexOf(pumpCurve) + 1}",
                    FlowRate = individualFlow,
                    Head = individualHead
                };

                // Find power and efficiency from curve if available
                var curvePoint = pumpCurve.FirstOrDefault(p => 
                    Math.Abs(p.FlowRate - individualFlow) < 0.1);
                if (curvePoint != null)
                {
                    point.Power = curvePoint.Power;
                    point.Efficiency = curvePoint.Efficiency;
                    result.TotalPower += curvePoint.Power;
                }

                result.PumpOperatingPoints.Add(point);
            }

            // Calculate overall efficiency
            if (result.TotalPower > 0)
            {
                double hydraulicPower = (result.TotalFlowRate * result.TotalHead) / 
                                       Constants.PumpConstants.HorsepowerConversionFactor;
                result.OverallEfficiency = hydraulicPower / result.TotalPower;
            }

            return result;
        }

        /// <summary>
        /// Combines pump curves in series (adds heads at same flow rates).
        /// </summary>
        private static List<HeadQuantityPoint> CombinePumpCurvesSeries(
            List<List<HeadQuantityPoint>> pumpCurves)
        {
            if (pumpCurves.Count == 1)
                return pumpCurves[0];

            // Use flow rates from first curve
            var firstCurve = pumpCurves[0].OrderBy(p => p.FlowRate).ToList();
            var combinedCurve = new List<HeadQuantityPoint>();

            foreach (var point in firstCurve)
            {
                double combinedHead = 0;
                double combinedPower = 0;

                foreach (var curve in pumpCurves)
                {
                    double head = HeadQuantityCalculations.InterpolateHead(curve, point.FlowRate);
                    combinedHead += head;

                    var curvePoint = curve.FirstOrDefault(p => 
                        Math.Abs(p.FlowRate - point.FlowRate) < 0.1);
                    if (curvePoint != null)
                    {
                        combinedPower += curvePoint.Power;
                    }
                }

                combinedCurve.Add(new HeadQuantityPoint(
                    point.FlowRate, combinedHead, point.Efficiency, combinedPower));
            }

            return combinedCurve;
        }

        /// <summary>
        /// Combines pump curves in parallel (adds flow rates at same heads).
        /// </summary>
        private static List<HeadQuantityPoint> CombinePumpCurvesParallel(
            List<List<HeadQuantityPoint>> pumpCurves)
        {
            if (pumpCurves.Count == 1)
                return pumpCurves[0];

            // Collect all unique heads from all curves
            var allHeads = new HashSet<double>();
            foreach (var curve in pumpCurves)
            {
                foreach (var point in curve)
                {
                    allHeads.Add(Math.Round(point.Head, 1)); // Round to avoid duplicates
                }
            }

            var sortedHeads = allHeads.OrderBy(h => h).ToList();
            var combinedCurve = new List<HeadQuantityPoint>();

            foreach (var head in sortedHeads)
            {
                double combinedFlow = 0;
                double combinedPower = 0;

                foreach (var curve in pumpCurves)
                {
                    // Find flow rate at this head by interpolation
                    var sortedCurve = curve.OrderBy(p => p.Head).ToList();
                    
                    // Find surrounding points
                    for (int i = 0; i < sortedCurve.Count - 1; i++)
                    {
                        if (head >= sortedCurve[i].Head && head <= sortedCurve[i + 1].Head)
                        {
                            double t = (head - sortedCurve[i].Head) / 
                                      (sortedCurve[i + 1].Head - sortedCurve[i].Head);
                            double flow = sortedCurve[i].FlowRate + 
                                         t * (sortedCurve[i + 1].FlowRate - sortedCurve[i].FlowRate);
                            combinedFlow += flow;

                            // Estimate power (simplified)
                            if (sortedCurve[i].Power > 0)
                            {
                                combinedPower += sortedCurve[i].Power;
                            }
                            break;
                        }
                    }
                }

                combinedCurve.Add(new HeadQuantityPoint(
                    combinedFlow, head, 0, combinedPower));
            }

            return combinedCurve;
        }

        /// <summary>
        /// Compares series vs parallel configuration for given pumps and system.
        /// </summary>
        /// <param name="pumpCurves">List of H-Q curves for each pump.</param>
        /// <param name="systemCurve">System resistance curve.</param>
        /// <returns>Tuple with (series result, parallel result).</returns>
        public static (PumpConfigurationResult series, PumpConfigurationResult parallel) 
            CompareConfigurations(
            List<List<HeadQuantityPoint>> pumpCurves,
            List<SystemCurvePoint> systemCurve)
        {
            var seriesResult = CalculateSeriesConfiguration(pumpCurves, systemCurve);
            var parallelResult = CalculateParallelConfiguration(pumpCurves, systemCurve);

            return (seriesResult, parallelResult);
        }
    }
}

