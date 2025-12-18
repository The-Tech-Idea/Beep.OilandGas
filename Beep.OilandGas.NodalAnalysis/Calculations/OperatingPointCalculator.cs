using System;
using System.Collections.Generic;
using Beep.OilandGas.NodalAnalysis.Models;
using Beep.OilandGas.GasProperties.Calculations;
using System.Linq;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides operating point calculations for nodal analysis.
    /// </summary>
    public static class OperatingPointCalculator
    {
        /// <summary>
        /// Calculates operating point (intersection of IPR and VLP curves).
        /// </summary>
        /// <param name="reservoirPressure">Reservoir pressure in psia.</param>
        /// <param name="wellheadPressure">Wellhead pressure in psia.</param>
        /// <param name="depth">Well depth in feet.</param>
        /// <param name="tubingDiameter">Tubing diameter in inches.</param>
        /// <param name="oilGravity">Oil API gravity.</param>
        /// <param name="gasSpecificGravity">Gas specific gravity (relative to air).</param>
        /// <param name="temperature">Average temperature in Rankine.</param>
        /// <param name="gasLiquidRatio">Gas-liquid ratio in scf/bbl.</param>
        /// <param name="productivityIndex">Productivity index in bbl/day/psi.</param>
        /// <returns>Operating point (flow rate and bottom hole pressure).</returns>
        public static Models.OperatingPoint CalculateOperatingPoint(
            decimal reservoirPressure,
            decimal wellheadPressure,
            decimal depth,
            decimal tubingDiameter,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal gasLiquidRatio,
            decimal productivityIndex)
        {
            if (reservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be greater than zero.", nameof(reservoirPressure));

            if (wellheadPressure <= 0)
                throw new ArgumentException("Wellhead pressure must be greater than zero.", nameof(wellheadPressure));

            if (productivityIndex <= 0)
                throw new ArgumentException("Productivity index must be greater than zero.", nameof(productivityIndex));

            // Generate IPR curve
            var iprCurve = GenerateIPRCurve(reservoirPressure, productivityIndex);

            // Generate VLP curve
            var vlpCurve = GenerateVLPCurve(
                reservoirPressure, wellheadPressure, depth, tubingDiameter,
                oilGravity, gasSpecificGravity, temperature, gasLiquidRatio);

            // Find intersection point
            var operatingPoint = FindIntersection(iprCurve, vlpCurve);

            return operatingPoint;
        }

        /// <summary>
        /// Generates IPR (Inflow Performance Relationship) curve.
        /// </summary>
        private static List<(double FlowRate, double BottomHolePressure)> GenerateIPRCurve(
            decimal reservoirPressure,
            decimal productivityIndex)
        {
            var curve = new List<(double, double)>();

            // Generate points from 0 to maximum flow rate
            int numberOfPoints = 50;
            double maxFlowRate = (double)(productivityIndex * reservoirPressure); // Maximum theoretical flow rate

            for (int i = 0; i <= numberOfPoints; i++)
            {
                double flowRate = maxFlowRate * i / numberOfPoints;
                double bottomHolePressure = (double)reservoirPressure - (flowRate / (double)productivityIndex);

                if (bottomHolePressure > 0)
                {
                    curve.Add((flowRate, bottomHolePressure));
                }
            }

            return curve;
        }

        /// <summary>
        /// Generates VLP (Vertical Lift Performance) curve.
        /// </summary>
        private static List<(double FlowRate, double BottomHolePressure)> GenerateVLPCurve(
            decimal reservoirPressure,
            decimal wellheadPressure,
            decimal depth,
            decimal tubingDiameter,
            decimal oilGravity,
            decimal gasSpecificGravity,
            decimal temperature,
            decimal gasLiquidRatio)
        {
            var curve = new List<(double, double)>();

            // Generate points for different flow rates
            int numberOfPoints = 50;
            double maxFlowRate = 5000.0; // bbl/day (typical maximum)

            for (int i = 0; i <= numberOfPoints; i++)
            {
                double flowRate = maxFlowRate * i / numberOfPoints;

                if (flowRate <= 0)
                    continue;

                // Calculate BHP for this flow rate using Hagedorn-Brown
                decimal bottomHolePressure = BHPCorrelations.CalculateHagedornBrown(
                    wellheadPressure, depth, (decimal)flowRate, gasLiquidRatio,
                    oilGravity, gasSpecificGravity, temperature);

                if (bottomHolePressure > 0 && bottomHolePressure <= reservoirPressure * 1.5m)
                {
                    curve.Add((flowRate, (double)bottomHolePressure));
                }
            }

            return curve;
        }

        /// <summary>
        /// Finds intersection point between IPR and VLP curves.
        /// </summary>
        private static Models.OperatingPoint FindIntersection(
            List<(double FlowRate, double BottomHolePressure)> iprCurve,
            List<(double FlowRate, double BottomHolePressure)> vlpCurve)
        {
            if (iprCurve.Count == 0 || vlpCurve.Count == 0)
                throw new InvalidOperationException("Curves cannot be empty.");

            // Find closest points between curves
            double minDistance = double.MaxValue;
            double operatingFlowRate = 0.0;
            double operatingBHP = 0.0;

            foreach (var iprPoint in iprCurve)
            {
                foreach (var vlpPoint in vlpCurve)
                {
                    // Calculate distance in (flow rate, BHP) space
                    double flowRateDiff = Math.Abs(iprPoint.FlowRate - vlpPoint.FlowRate);
                    double bhpDiff = Math.Abs(iprPoint.BottomHolePressure - vlpPoint.BottomHolePressure);

                    // Weighted distance (normalize by typical values)
                    double distance = Math.Sqrt(flowRateDiff * flowRateDiff / 1000000.0 + 
                                               bhpDiff * bhpDiff / 10000.0);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        // Use average of the two points
                        operatingFlowRate = (iprPoint.FlowRate + vlpPoint.FlowRate) / 2.0;
                        operatingBHP = (iprPoint.BottomHolePressure + vlpPoint.BottomHolePressure) / 2.0;
                    }
                }
            }

            return new Models.OperatingPoint
            {
                FlowRate = (double)operatingFlowRate,
                BottomholePressure = (double)operatingBHP,
                WellheadPressure = 0.0, // Will be calculated separately
            };
        }

        /// <summary>
        /// Calculates multiple operating points for sensitivity analysis.
        /// </summary>
        /// <param name="baseConditions">Base operating conditions.</param>
        /// <param name="sensitivityParameters">Parameters to vary for sensitivity analysis.</param>
        /// <returns>List of operating points for different conditions.</returns>
        public static List<Models.OperatingPoint> CalculateOperatingPointSensitivity(
            OperatingConditions baseConditions,
            SensitivityParameters sensitivityParameters)
        {
            var operatingPoints = new List<Models.OperatingPoint>();

            // Vary wellhead pressure
            if (sensitivityParameters.VaryWellheadPressure)
            {
                decimal minPressure = baseConditions.WellheadPressure * (1m - sensitivityParameters.PressureVariation);
                decimal maxPressure = baseConditions.WellheadPressure * (1m + sensitivityParameters.PressureVariation);
                int steps = sensitivityParameters.NumberOfSteps;

                for (int i = 0; i <= steps; i++)
                {
                    decimal wellheadPressure = minPressure + (maxPressure - minPressure) * i / steps;

                    var operatingPoint = CalculateOperatingPoint(
                        baseConditions.ReservoirPressure,
                        wellheadPressure,
                        baseConditions.Depth,
                        baseConditions.TubingDiameter,
                        baseConditions.OilGravity,
                        baseConditions.GasSpecificGravity,
                        baseConditions.Temperature,
                        baseConditions.GasLiquidRatio,
                        baseConditions.ProductivityIndex);

                    operatingPoints.Add(operatingPoint);
                }
            }

            // Vary GLR
            if (sensitivityParameters.VaryGLR)
            {
                decimal minGLR = baseConditions.GasLiquidRatio * (1m - sensitivityParameters.GLRVariation);
                decimal maxGLR = baseConditions.GasLiquidRatio * (1m + sensitivityParameters.GLRVariation);
                int steps = sensitivityParameters.NumberOfSteps;

                for (int i = 0; i <= steps; i++)
                {
                    decimal glr = minGLR + (maxGLR - minGLR) * i / steps;

                    var operatingPoint = CalculateOperatingPoint(
                        baseConditions.ReservoirPressure,
                        baseConditions.WellheadPressure,
                        baseConditions.Depth,
                        baseConditions.TubingDiameter,
                        baseConditions.OilGravity,
                        baseConditions.GasSpecificGravity,
                        baseConditions.Temperature,
                        glr,
                        baseConditions.ProductivityIndex);

                    operatingPoints.Add(operatingPoint);
                }
            }

            return operatingPoints;
        }

        /// <summary>
        /// Finds operating point from IPR and VLP curves (wrapper for DataFlowService).
        /// </summary>
        public static OperatingPoint FindOperatingPoint(List<IPRPoint> iprCurve, List<VLPPoint> vlpCurve)
        {
            if (iprCurve == null || iprCurve.Count == 0)
                throw new ArgumentException("IPR curve cannot be null or empty.", nameof(iprCurve));

            if (vlpCurve == null || vlpCurve.Count == 0)
                throw new ArgumentException("VLP curve cannot be null or empty.", nameof(vlpCurve));

            // Convert to tuples for internal method
            var iprTuples = iprCurve.Select(p => (p.FlowRate, p.FlowingBottomholePressure)).ToList();
            var vlpTuples = vlpCurve.Select(p => (p.FlowRate, p.RequiredBottomholePressure)).ToList();

            // Use internal FindIntersection method
            var internalResult = FindIntersection(iprTuples, vlpTuples);

            return new OperatingPoint
            {
                FlowRate = internalResult.FlowRate,
                BottomholePressure = internalResult.BottomholePressure,
                WellheadPressure = 0.0 // Not calculated in this simplified version
            };
        }
    }

    /// <summary>
    /// Represents operating conditions.
    /// </summary>
    public class OperatingConditions
    {
        public decimal ReservoirPressure { get; set; }
        public decimal WellheadPressure { get; set; }
        public decimal Depth { get; set; }
        public decimal TubingDiameter { get; set; }
        public decimal OilGravity { get; set; }
        public decimal GasSpecificGravity { get; set; }
        public decimal Temperature { get; set; }
        public decimal GasLiquidRatio { get; set; }
        public decimal ProductivityIndex { get; set; }
    }

    /// <summary>
    /// Represents sensitivity analysis parameters.
    /// </summary>
    public class SensitivityParameters
    {
        public bool VaryWellheadPressure { get; set; } = true;
        public bool VaryGLR { get; set; } = false;
        public decimal PressureVariation { get; set; } = 0.2m; // 20% variation
        public decimal GLRVariation { get; set; } = 0.3m; // 30% variation
        public int NumberOfSteps { get; set; } = 10;
    }
}

