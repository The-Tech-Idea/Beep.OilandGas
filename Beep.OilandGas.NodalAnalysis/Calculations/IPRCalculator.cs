using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides IPR (Inflow Performance Relationship) calculation methods.
    /// </summary>
    public static class IPRCalculator
    {
        /// <summary>
        /// Generates IPR curve using Vogel method for solution gas drive reservoirs.
        /// </summary>
        public static List<IPRPoint> GenerateVogelIPR(ReservoirProperties reservoir, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (reservoir.ReservoirPressure <= 0)
                throw new ArgumentException("Reservoir pressure must be positive.", nameof(reservoir));

            var ipr = new List<IPRPoint>();

            // Vogel equation: q/q_max = 1 - 0.2 * (Pwf/Pr) - 0.8 * (Pwf/Pr)²
            // Rearranged: Pwf = Pr * [1 - 0.2 * (q/q_max) - 0.8 * (q/q_max)²] / [1 - 0.2 - 0.8]
            // Actually: q = q_max * [1 - 0.2 * (Pwf/Pr) - 0.8 * (Pwf/Pr)²]

            // Calculate maximum flow rate (at Pwf = 0)
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;

            for (int i = 0; i <= points; i++)
            {
                double qRatio = (double)i / points;
                double flowRate = qRatio * maxFlowRate;

                if (flowRate > qMax)
                    flowRate = qMax;

                // Solve Vogel equation for Pwf
                double pwfRatio = flowRate / qMax;
                double pwf = reservoir.ReservoirPressure * 
                    (1.0 - 0.2 * pwfRatio - 0.8 * pwfRatio * pwfRatio);

                if (pwf < 0)
                    pwf = 0;

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates IPR curve using Fetkovich method for multi-point IPR.
        /// </summary>
        public static List<IPRPoint> GenerateFetkovichIPR(ReservoirProperties reservoir, 
            List<(double flowRate, double pressure)> testPoints, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            if (testPoints == null || testPoints.Count < 2)
                throw new ArgumentException("At least 2 test points are required for Fetkovich method.", nameof(testPoints));

            var ipr = new List<IPRPoint>();

            // Fetkovich: q = C * (Pr² - Pwf²)^n
            // Fit to test points to find C and n
            var sortedPoints = testPoints.OrderByDescending(p => p.pressure).ToList();

            // Use two points to estimate C and n
            var p1 = sortedPoints[0];
            var p2 = sortedPoints[sortedPoints.Count - 1];

            double pr2 = reservoir.ReservoirPressure * reservoir.ReservoirPressure;
            double pwf1_2 = p1.pressure * p1.pressure;
            double pwf2_2 = p2.pressure * p2.pressure;

            double term1 = Math.Log(p1.flowRate / p2.flowRate);
            double term2 = Math.Log((pr2 - pwf1_2) / (pr2 - pwf2_2));

            if (Math.Abs(term2) < 1e-10)
                throw new ArgumentException("Invalid test points for Fetkovich method.", nameof(testPoints));

            double n = term1 / term2;
            double c = p1.flowRate / Math.Pow(pr2 - pwf1_2, n);

            // Generate IPR curve
            for (int i = 0; i <= points; i++)
            {
                double flowRate = (double)i / points * maxFlowRate;
                double pwf2 = pr2 - Math.Pow(flowRate / c, 1.0 / n);
                double pwf = Math.Sqrt(Math.Max(0, pwf2));

                if (pwf > reservoir.ReservoirPressure)
                    pwf = reservoir.ReservoirPressure;

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates IPR curve using Wiggins method for three-phase flow.
        /// </summary>
        public static List<IPRPoint> GenerateWigginsIPR(ReservoirProperties reservoir, double maxFlowRate = 5000, int points = 50)
        {
            if (reservoir == null)
                throw new ArgumentNullException(nameof(reservoir));

            var ipr = new List<IPRPoint>();

            // Wiggins method accounts for water cut
            double waterCut = reservoir.WaterCut;
            double oilCut = 1.0 - waterCut;

            // Modified Vogel for three-phase
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;

            for (int i = 0; i <= points; i++)
            {
                double qRatio = (double)i / points;
                double flowRate = qRatio * maxFlowRate;

                if (flowRate > qMax)
                    flowRate = qMax;

                // Wiggins: accounts for water cut in the IPR
                double pwfRatio = flowRate / qMax;
                double pwf = reservoir.ReservoirPressure *
                    (1.0 - 0.2 * pwfRatio - 0.8 * pwfRatio * pwfRatio);

                // Adjust for water cut
                pwf = pwf * (oilCut + waterCut * 0.8); // Water has different flow characteristics

                if (pwf < 0)
                    pwf = 0;

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates gas well IPR using backpressure equation.
        /// </summary>
        public static List<IPRPoint> GenerateGasWellIPR(double reservoirPressure, double aof, 
            double backpressureExponent = 0.5, double maxFlowRate = 10000, int points = 50)
        {
            var ipr = new List<IPRPoint>();

            // Backpressure equation: q = C * (Pr² - Pwf²)^n
            // AOF is flow rate at Pwf = 0
            double c = aof / Math.Pow(reservoirPressure * reservoirPressure, backpressureExponent);

            for (int i = 0; i <= points; i++)
            {
                double flowRate = (double)i / points * maxFlowRate;

                if (flowRate > aof)
                    flowRate = aof;

                double pr2 = reservoirPressure * reservoirPressure;
                double pwf2 = pr2 - Math.Pow(flowRate / c, 1.0 / backpressureExponent);
                double pwf = Math.Sqrt(Math.Max(0, pwf2));

                ipr.Add(new IPRPoint(flowRate, pwf));
            }

            return ipr;
        }

        /// <summary>
        /// Generates IPR curve with specified parameters (wrapper for DataFlowService).
        /// </summary>
        public static List<IPRPoint> GenerateIPRCurve(
            decimal reservoirPressure,
            decimal bubblePointPressure,
            decimal productivityIndex,
            decimal waterCut,
            decimal gasOilRatio,
            decimal oilGravity,
            decimal formationVolumeFactor,
            decimal oilViscosity,
            int points = 20)
        {
            // Create reservoir properties from parameters
            var reservoir = new ReservoirProperties
            {
                ReservoirPressure = (double)reservoirPressure,
                BubblePointPressure = (double)bubblePointPressure,
                ProductivityIndex = (double)productivityIndex,
                WaterCut = (double)waterCut,
                GasOilRatio = (double)gasOilRatio,
                OilGravity = (double)oilGravity,
                FormationVolumeFactor = (double)formationVolumeFactor,
                OilViscosity = (double)oilViscosity
            };

            // Use Vogel method as default
            return GenerateVogelIPR(reservoir, maxFlowRate: 5000, points: points);
        }
    }
}

