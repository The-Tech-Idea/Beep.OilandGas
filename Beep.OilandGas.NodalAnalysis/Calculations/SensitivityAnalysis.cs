using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.NodalAnalysis.Calculations
{
    /// <summary>
    /// Provides sensitivity analysis for nodal analysis operating points.
    /// Supports parameter variation sweeps, tornado chart data generation,
    /// and statistical P10/P50/P90 uncertainty analysis.
    /// </summary>
    public static class SensitivityAnalysis
    {
        // ─────────────────────────────────────────────────────────────────
        //  Parameter variation sweep
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs a single-parameter sensitivity sweep by varying one reservoir property
        /// over a specified range and recording the resulting operating-point flow rate.
        /// All other parameters are held at their base-case values.
        /// </summary>
        /// <param name="baseReservoir">Base-case reservoir properties.</param>
        /// <param name="baseWellbore">Base-case wellbore properties.</param>
        /// <param name="parameterName">Name of the reservoir property to vary (e.g. "ReservoirPressure").</param>
        /// <param name="lowValue">Low end of the range (−P10 case).</param>
        /// <param name="highValue">High end of the range (+P90 case).</param>
        /// <param name="steps">Number of steps between low and high (minimum 2).</param>
        /// <returns>List of <see cref="SensitivityCurvePoint"/> with one entry per step.</returns>
        public static List<SensitivityCurvePoint> SweepParameter(
            ReservoirProperties baseReservoir,
            WellboreProperties baseWellbore,
            string parameterName,
            double lowValue,
            double highValue,
            int steps = 10)
        {
            if (baseReservoir == null) throw new ArgumentNullException(nameof(baseReservoir));
            if (baseWellbore == null) throw new ArgumentNullException(nameof(baseWellbore));
            if (string.IsNullOrWhiteSpace(parameterName)) throw new ArgumentNullException(nameof(parameterName));
            if (steps < 2) steps = 2;

            var results = new List<SensitivityCurvePoint>();

            // Compute base-case flow rate for percentage change reference
            double baseRate = ComputeVogelOperatingFlowRate(baseReservoir, baseWellbore);

            double range = highValue - lowValue;
            for (int i = 0; i <= steps; i++)
            {
                double paramValue = lowValue + (range * i / steps);

                // Clone base reservoir and override the target parameter
                var modified = CloneReservoir(baseReservoir);
                SetReservoirProperty(modified, parameterName, paramValue);

                double flowRate = ComputeVogelOperatingFlowRate(modified, baseWellbore);
                double pctChange = (baseRate > 0) ? (flowRate - baseRate) / baseRate * 100.0 : 0.0;

                results.Add(new SensitivityCurvePoint
                {
                    ParameterValue = paramValue,
                    FlowRate = flowRate,
                    PercentageChange = pctChange,
                    Sequence = i
                });
            }

            return results;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Tornado chart analysis
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Performs a tornado-chart sensitivity analysis by varying each parameter in
        /// <paramref name="parameterRanges"/> independently between its low and high values,
        /// recording the resulting swing in operating flow rate.
        /// Parameters are ranked by the magnitude of the swing (descending), which is the
        /// standard tornado-chart ordering.
        /// </summary>
        /// <param name="baseReservoir">Base-case reservoir properties.</param>
        /// <param name="baseWellbore">Base-case wellbore properties.</param>
        /// <param name="parameterRanges">
        ///   Dictionary of parameter names to (lowValue, highValue) tuples.
        ///   Keys must match property names on <see cref="ReservoirProperties"/>.
        /// </param>
        /// <returns>
        ///   A <see cref="TornadoAnalysisResult"/> with parameters ranked by impact,
        ///   plus the base-case operating point.
        /// </returns>
        public static TornadoAnalysisResult ComputeTornadoChart(
            ReservoirProperties baseReservoir,
            WellboreProperties baseWellbore,
            Dictionary<string, (double low, double high)> parameterRanges)
        {
            if (baseReservoir == null) throw new ArgumentNullException(nameof(baseReservoir));
            if (baseWellbore == null) throw new ArgumentNullException(nameof(baseWellbore));
            if (parameterRanges == null || parameterRanges.Count == 0)
                throw new ArgumentException("At least one parameter range is required.", nameof(parameterRanges));

            double baseRate = ComputeVogelOperatingFlowRate(baseReservoir, baseWellbore);
            double baseBhp = ComputeVogelOperatingBhp(baseReservoir, baseWellbore);

            var rankings = new List<ParameterRanking>();

            foreach (var kvp in parameterRanges)
            {
                string name = kvp.Key;
                double lowVal = kvp.Value.low;
                double highVal = kvp.Value.high;

                // Low-case flow rate
                var modLow = CloneReservoir(baseReservoir);
                SetReservoirProperty(modLow, name, lowVal);
                double rateLow = ComputeVogelOperatingFlowRate(modLow, baseWellbore);

                // High-case flow rate
                var modHigh = CloneReservoir(baseReservoir);
                SetReservoirProperty(modHigh, name, highVal);
                double rateHigh = ComputeVogelOperatingFlowRate(modHigh, baseWellbore);

                double swing = Math.Abs(rateHigh - rateLow);
                double flowImpact = rateHigh - rateLow;   // signed: +ve means higher high = more production
                double variation = highVal - lowVal;

                rankings.Add(new ParameterRanking
                {
                    ParameterName = name,
                    Variation = variation,
                    FlowRateImpact = flowImpact,
                    PressureImpact = 0.0  // pressure impact calculation not required for tornado
                });

                // Attach curve points for the tornado bar extents
                // (stored as a transient list — no database persistence for sub-objects here)
            }

            // Sort by absolute swing descending (standard tornado ordering)
            rankings.Sort((a, b) => Math.Abs(b.FlowRateImpact).CompareTo(Math.Abs(a.FlowRateImpact)));

            var result = new TornadoAnalysisResult
            {
                BaseFlowRate = baseRate,
                BasePressure = baseBhp,
                ParameterRankings = rankings,
                MostInfluentialParameter = rankings.Count > 0 ? rankings[0].ParameterName : string.Empty,
                LeastInfluentialParameter = rankings.Count > 0 ? rankings[rankings.Count - 1].ParameterName : string.Empty
            };

            return result;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Statistical P10 / P50 / P90 analysis
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates P10, P50, and P90 flow rates via Monte-Carlo sampling of reservoir
        /// parameters described by normal distributions.  Each parameter in
        /// <paramref name="parameterDistributions"/> is sampled independently; the resulting
        /// operating flow rates are aggregated and sorted to extract percentiles.
        /// </summary>
        /// <param name="baseReservoir">Base-case reservoir properties (used for parameters not in the dictionary).</param>
        /// <param name="baseWellbore">Base-case wellbore properties.</param>
        /// <param name="parameterDistributions">
        ///   Dictionary of parameter names to (mean, standardDeviation) normal-distribution parameters.
        /// </param>
        /// <param name="iterations">Number of Monte-Carlo iterations (default 1000).</param>
        /// <param name="seed">Random seed for reproducibility (null = system random).</param>
        /// <returns>A <see cref="MonteCarloResults"/> object with P10/P50/P90 flow-rate percentiles.</returns>
        public static MonteCarloResults ComputeStatisticalPercentiles(
            ReservoirProperties baseReservoir,
            WellboreProperties baseWellbore,
            Dictionary<string, (double mean, double stdDev)> parameterDistributions,
            int iterations = 1000,
            int? seed = null)
        {
            if (baseReservoir == null) throw new ArgumentNullException(nameof(baseReservoir));
            if (baseWellbore == null) throw new ArgumentNullException(nameof(baseWellbore));
            if (parameterDistributions == null || parameterDistributions.Count == 0)
                throw new ArgumentException("At least one parameter distribution is required.", nameof(parameterDistributions));
            if (iterations < 10) iterations = 10;

            var rng = seed.HasValue ? new Random(seed.Value) : new Random();
            var flowRates = new List<double>(iterations);

            for (int iter = 0; iter < iterations; iter++)
            {
                var sample = CloneReservoir(baseReservoir);

                foreach (var kvp in parameterDistributions)
                {
                    double mean = kvp.Value.mean;
                    double stdDev = kvp.Value.stdDev;
                    double sampledValue = SampleNormal(rng, mean, stdDev);
                    SetReservoirProperty(sample, kvp.Key, sampledValue);
                }

                double rate = ComputeVogelOperatingFlowRate(sample, baseWellbore);
                if (rate > 0) flowRates.Add(rate);
            }

            flowRates.Sort();
            int n = flowRates.Count;

            double p10 = n > 0 ? flowRates[(int)(0.10 * (n - 1))] : 0;
            double p50 = n > 0 ? flowRates[(int)(0.50 * (n - 1))] : 0;
            double p90 = n > 0 ? flowRates[(int)(0.90 * (n - 1))] : 0;
            double mean2 = n > 0 ? flowRates.Average() : 0;
            double stdDev2 = n > 1
                ? Math.Sqrt(flowRates.Sum(r => (r - mean2) * (r - mean2)) / (n - 1))
                : 0;

            return new MonteCarloResults
            {
                P10 = p10,
                P50 = p50,
                P90 = p90,
                Mean = mean2,
                StandardDeviation = stdDev2,
                Iterations = n
            };
        }

        /// <summary>
        /// Computes a ranked sensitivity list showing which parameters have the greatest
        /// percentage impact on operating flow rate, given a uniform ±percentVariation around
        /// each parameter's base value.
        /// </summary>
        /// <param name="baseReservoir">Base-case reservoir properties.</param>
        /// <param name="baseWellbore">Base-case wellbore properties.</param>
        /// <param name="parameterNames">Names of reservoir properties to vary.</param>
        /// <param name="percentVariation">Fractional variation around base (e.g. 0.20 = ±20%).</param>
        /// <returns>Parameter rankings sorted by descending absolute impact on flow rate.</returns>
        public static List<ParameterRanking> RankParameterSensitivity(
            ReservoirProperties baseReservoir,
            WellboreProperties baseWellbore,
            IEnumerable<string> parameterNames,
            double percentVariation = 0.20)
        {
            if (baseReservoir == null) throw new ArgumentNullException(nameof(baseReservoir));
            if (baseWellbore == null) throw new ArgumentNullException(nameof(baseWellbore));
            if (parameterNames == null) throw new ArgumentNullException(nameof(parameterNames));

            double baseRate = ComputeVogelOperatingFlowRate(baseReservoir, baseWellbore);
            var rankings = new List<ParameterRanking>();

            foreach (string name in parameterNames)
            {
                double baseValue = GetReservoirProperty(baseReservoir, name);
                if (baseValue == 0) continue;  // Cannot vary a zero-value parameter

                double lowVal = baseValue * (1.0 - percentVariation);
                double highVal = baseValue * (1.0 + percentVariation);

                var modLow = CloneReservoir(baseReservoir);
                SetReservoirProperty(modLow, name, Math.Max(0, lowVal));
                double rateLow = ComputeVogelOperatingFlowRate(modLow, baseWellbore);

                var modHigh = CloneReservoir(baseReservoir);
                SetReservoirProperty(modHigh, name, highVal);
                double rateHigh = ComputeVogelOperatingFlowRate(modHigh, baseWellbore);

                double impact = rateHigh - rateLow;
                double pressureImpact = ComputeVogelOperatingBhp(modHigh, baseWellbore)
                                      - ComputeVogelOperatingBhp(modLow, baseWellbore);

                rankings.Add(new ParameterRanking
                {
                    ParameterName = name,
                    Variation = percentVariation * 100.0,
                    FlowRateImpact = impact,
                    PressureImpact = pressureImpact
                });
            }

            rankings.Sort((a, b) => Math.Abs(b.FlowRateImpact).CompareTo(Math.Abs(a.FlowRateImpact)));
            return rankings;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Estimates operating flow rate using the Vogel IPR / simple Darcy VLP intersection.
        /// For sensitivity purposes a simplified intersection is sufficient; for full accuracy
        /// use <see cref="OperatingPointCalculator"/>.
        /// </summary>
        private static double ComputeVogelOperatingFlowRate(
            ReservoirProperties reservoir, WellboreProperties wellbore)
        {
            const int pts = 40;
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;
            if (qMax <= 0) return 0;

            double bestQ = 0;
            double minDiff = double.MaxValue;

            for (int i = 1; i <= pts; i++)
            {
                double q = qMax * i / pts;
                double pwfIPR = reservoir.ReservoirPressure * (1.0 - 0.2 * (q / qMax) - 0.8 * Math.Pow(q / qMax, 2));
                double pwfVLP = wellbore.WellheadPressure + (wellbore.TubingLength > 0 ? 0.433 * wellbore.TubingLength : 500);

                double diff = Math.Abs(pwfIPR - pwfVLP);
                if (diff < minDiff)
                {
                    minDiff = diff;
                    bestQ = q;
                }
                if (pwfIPR < pwfVLP) break;  // IPR below VLP — operating point was the previous step
            }

            return bestQ;
        }

        private static double ComputeVogelOperatingBhp(
            ReservoirProperties reservoir, WellboreProperties wellbore)
        {
            double q = ComputeVogelOperatingFlowRate(reservoir, wellbore);
            double qMax = reservoir.ProductivityIndex * reservoir.ReservoirPressure / 1.8;
            if (qMax <= 0) return reservoir.ReservoirPressure;
            double ratio = q / qMax;
            return reservoir.ReservoirPressure * (1.0 - 0.2 * ratio - 0.8 * ratio * ratio);
        }

        private static ReservoirProperties CloneReservoir(ReservoirProperties src)
        {
            return new ReservoirProperties
            {
                ReservoirPressure = src.ReservoirPressure,
                BubblePointPressure = src.BubblePointPressure,
                ProductivityIndex = src.ProductivityIndex,
                WaterCut = src.WaterCut,
                GasOilRatio = src.GasOilRatio,
                OilGravity = src.OilGravity,
                FormationVolumeFactor = src.FormationVolumeFactor,
                OilViscosity = src.OilViscosity
            };
        }

        private static void SetReservoirProperty(ReservoirProperties r, string name, double value)
        {
            switch (name)
            {
                case nameof(ReservoirProperties.ReservoirPressure):    r.ReservoirPressure = value;    break;
                case nameof(ReservoirProperties.BubblePointPressure):  r.BubblePointPressure = value;  break;
                case nameof(ReservoirProperties.ProductivityIndex):    r.ProductivityIndex = value;    break;
                case nameof(ReservoirProperties.WaterCut):             r.WaterCut = value;             break;
                case nameof(ReservoirProperties.GasOilRatio):          r.GasOilRatio = value;          break;
                case nameof(ReservoirProperties.OilGravity):           r.OilGravity = value;           break;
                case nameof(ReservoirProperties.FormationVolumeFactor):r.FormationVolumeFactor = value;break;
                case nameof(ReservoirProperties.OilViscosity):         r.OilViscosity = value;         break;
                default:
                    throw new ArgumentException($"Unknown ReservoirProperties parameter: '{name}'", nameof(name));
            }
        }

        private static double GetReservoirProperty(ReservoirProperties r, string name)
        {
            return name switch
            {
                nameof(ReservoirProperties.ReservoirPressure)    => r.ReservoirPressure,
                nameof(ReservoirProperties.BubblePointPressure)  => r.BubblePointPressure,
                nameof(ReservoirProperties.ProductivityIndex)    => r.ProductivityIndex,
                nameof(ReservoirProperties.WaterCut)             => r.WaterCut,
                nameof(ReservoirProperties.GasOilRatio)          => r.GasOilRatio,
                nameof(ReservoirProperties.OilGravity)           => r.OilGravity,
                nameof(ReservoirProperties.FormationVolumeFactor)=> r.FormationVolumeFactor,
                nameof(ReservoirProperties.OilViscosity)         => r.OilViscosity,
                _ => throw new ArgumentException($"Unknown ReservoirProperties parameter: '{name}'", nameof(name))
            };
        }

        /// <summary>
        /// Box-Muller transform for normal-distribution sampling.
        /// </summary>
        private static double SampleNormal(Random rng, double mean, double stdDev)
        {
            // Box-Muller
            double u1 = 1.0 - rng.NextDouble();
            double u2 = 1.0 - rng.NextDouble();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
            return mean + stdDev * z;
        }
    }
}
