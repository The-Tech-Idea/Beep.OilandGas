using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides nodal analysis calculations.
    /// </summary>
    public static class NodalAnalyzer
    {
        /// <summary>
        /// Finds the operating point where IPR and VLP curves intersect.
        /// </summary>
        public static OperatingPoint FindOperatingPoint(List<IPRPoint> iprCurve, List<VLPPoint> vlpCurve)
        {
            if (iprCurve == null || iprCurve.Count == 0)
                throw new ArgumentException("IPR curve cannot be null or empty.", nameof(iprCurve));

            if (vlpCurve == null || vlpCurve.Count == 0)
                throw new ArgumentException("VLP curve cannot be null or empty.", nameof(vlpCurve));

            // Find intersection by checking where IPR pressure >= VLP pressure changes
            var sortedIPR = iprCurve.OrderBy(p => p.FlowRate).ToList();
            var sortedVLP = vlpCurve.OrderBy(p => p.FlowRate).ToList();

            // Find overlapping flow rate range
            double minFlow = Math.Max(sortedIPR[0].FlowRate, sortedVLP[0].FlowRate);
            double maxFlow = Math.Min(sortedIPR.Last().FlowRate, sortedVLP.Last().FlowRate);

            if (minFlow >= maxFlow)
                throw new ArgumentException("IPR and VLP curves do not overlap in flow rate range.");

            // Search for intersection
            double bestFlowRate = minFlow;
            double bestPressure = 0;
            double minDifference = double.MaxValue;

            for (double flowRate = minFlow; flowRate <= maxFlow; flowRate += (maxFlow - minFlow) / 1000.0)
            {
                double iprPressure = InterpolateIPR(sortedIPR, flowRate);
                double vlpPressure = InterpolateVLP(sortedVLP, flowRate);

                double difference = Math.Abs(iprPressure - vlpPressure);

                if (difference < minDifference)
                {
                    minDifference = difference;
                    bestFlowRate = flowRate;
                    bestPressure = (iprPressure + vlpPressure) / 2.0;
                }

                // If curves cross, we found intersection
                if (difference < 1.0) // Within 1 psi
                {
                    return new OperatingPoint(flowRate, bestPressure);
                }
            }

            // Return best match
            return new OperatingPoint(bestFlowRate, bestPressure);
        }

        /// <summary>
        /// Interpolates IPR pressure at a given flow rate.
        /// </summary>
        private static double InterpolateIPR(List<IPRPoint> ipr, double flowRate)
        {
            if (flowRate <= ipr[0].FlowRate)
                return ipr[0].FlowingBottomholePressure;

            if (flowRate >= ipr.Last().FlowRate)
                return ipr.Last().FlowingBottomholePressure;

            for (int i = 0; i < ipr.Count - 1; i++)
            {
                if (flowRate >= ipr[i].FlowRate && flowRate <= ipr[i + 1].FlowRate)
                {
                    double t = (flowRate - ipr[i].FlowRate) / (ipr[i + 1].FlowRate - ipr[i].FlowRate);
                    return ipr[i].FlowingBottomholePressure + 
                        t * (ipr[i + 1].FlowingBottomholePressure - ipr[i].FlowingBottomholePressure);
                }
            }

            return ipr.Last().FlowingBottomholePressure;
        }

        /// <summary>
        /// Interpolates VLP pressure at a given flow rate.
        /// </summary>
        private static double InterpolateVLP(List<VLPPoint> vlp, double flowRate)
        {
            if (flowRate <= vlp[0].FlowRate)
                return (double)vlp[0].RequiredBottomholePressure;

            if (flowRate >= vlp.Last().FlowRate)
                return (double)vlp.Last().RequiredBottomholePressure;

            for (int i = 0; i < vlp.Count - 1; i++)
            {
                if (flowRate >= vlp[i].FlowRate && flowRate <= vlp[i + 1].FlowRate)
                {
                    double t = (flowRate - vlp[i].FlowRate) / (vlp[i + 1].FlowRate - vlp[i].FlowRate);
                    return vlp[i].RequiredBottomholePressure + 
                        t * (vlp[i + 1].RequiredBottomholePressure - vlp[i].RequiredBottomholePressure);
                }
            }

            return vlp.Last().RequiredBottomholePressure;
        }

        /// <summary>
        /// Performs sensitivity analysis on operating point.
        /// </summary>
        public static List<(string parameter, double value, OperatingPoint operatingPoint)> SensitivityAnalysis(
            List<IPRPoint> baseIPR, List<VLPPoint> baseVLP, 
            Dictionary<string, double> parameterVariations)
        {
            var results = new List<(string, double, OperatingPoint)>();

            foreach (var variation in parameterVariations)
            {
                // This is a simplified sensitivity - full implementation would modify IPR/VLP
                // based on parameter changes
                var operatingPoint = FindOperatingPoint(baseIPR, baseVLP);
                results.Add((variation.Key, variation.Value, operatingPoint));
            }

            return results;
        }
    }
}

