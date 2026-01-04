using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.WellTestAnalysis.Exceptions;
using Beep.OilandGas.Models.WellTestAnalysis;
using Beep.OilandGas.WellTestAnalysis.Validation;

namespace Beep.OilandGas.WellTestAnalysis.Calculations
{
    /// <summary>
    /// Provides build-up test analysis methods.
    /// </summary>
    public static class BuildUpAnalysis
    {
        /// <summary>
        /// Performs Horner analysis on build-up test data.
        /// </summary>
        public static WellTestAnalysisResult AnalyzeHorner(WellTestData data)
        {
            WellTestDataValidator.Validate(data);

            if (data.TestType != WellTestType.BuildUp)
                throw new InvalidWellTestDataException(nameof(data.TestType), "Horner analysis requires build-up test data.");

            if (data.ProductionTime <= 0)
                throw new InvalidWellTestDataException(nameof(data.ProductionTime), "Production time must be positive for Horner analysis.");

            var result = new WellTestAnalysisResult
            {
                AnalysisMethod = "Horner"
            };

            // Calculate Horner time: (tp + Δt) / Δt
            var hornerTime = data.Time.Select(t => (data.ProductionTime + t) / t).ToList();
            var pressures = data.Pressure.ToList();

            // Find the middle time region (semi-log straight line)
            int startIndex = FindMiddleTimeRegion(hornerTime, pressures);

            if (startIndex < 0 || startIndex >= hornerTime.Count - 2)
                throw new AnalysisConvergenceException("Could not identify middle time region for Horner analysis.");

            // Use last portion of middle time region for analysis
            int endIndex = hornerTime.Count - 1;
            int analysisPoints = Math.Min(10, endIndex - startIndex + 1);
            startIndex = endIndex - analysisPoints + 1;

            // Linear regression on semi-log plot: P vs log((tp+Δt)/Δt)
            var logHornerTime = hornerTime.Skip(startIndex).Take(analysisPoints)
                .Select(t => Math.Log10(t)).ToArray();
            var analysisPressures = pressures.Skip(startIndex).Take(analysisPoints).ToArray();

            LinearRegression(logHornerTime, analysisPressures, out double slope, out double intercept);

            // Calculate permeability: k = (162.6 * q * B * μ) / (m * h)
            // where m is the slope in psi/log cycle
            double m = Math.Abs(slope);
            if (m < WellTestConstants.Epsilon)
                throw new AnalysisConvergenceException("Invalid slope from Horner analysis.");

            result.Permeability = (162.6 * data.FlowRate * data.OilFormationVolumeFactor * data.OilViscosity) /
                                 (m * data.FormationThickness);

            // Calculate skin factor: s = 1.151 * [(P1hr - Pwf) / m - log(k / (φ * μ * ct * rw²)) - 3.23]
            // Extrapolate to Horner time = 1 (Δt = tp)
            double p1hr = intercept + slope * Math.Log10(1.0);
            double pws = pressures[0]; // Initial shut-in pressure

            double logTerm = Math.Log10(result.Permeability / 
                (data.Porosity * data.OilViscosity * data.TotalCompressibility * 
                 Math.Pow(data.WellboreRadius, 2)));

            result.SkinFactor = 1.151 * ((p1hr - pws) / m - logTerm - 3.23);

            // Extrapolate to infinite time (Horner time = 1) for reservoir pressure
            result.ReservoirPressure = intercept;

            // Calculate productivity index
            result.ProductivityIndex = data.FlowRate / (result.ReservoirPressure - pws);

            // Calculate flow efficiency
            double deltaPs = result.SkinFactor * m / 1.151;
            result.FlowEfficiency = (result.ReservoirPressure - pws - deltaPs) / (result.ReservoirPressure - pws);

            // Calculate damage ratio
            result.DamageRatio = 1.0 / result.FlowEfficiency;

            // Calculate radius of investigation
            double lastTime = data.Time.Last();
            result.RadiusOfInvestigation = Math.Sqrt(
                result.Permeability * lastTime / 
                (948 * data.Porosity * data.OilViscosity * data.TotalCompressibility));

            // Calculate R-squared
            result.RSquared = CalculateRSquared(logHornerTime, analysisPressures, slope, intercept);

            return result;
        }

        /// <summary>
        /// Performs Miller-Dyes-Hutchinson (MDH) analysis on build-up test data.
        /// </summary>
        public static WellTestAnalysisResult AnalyzeMDH(WellTestData data)
        {
            WellTestDataValidator.Validate(data);

            if (data.TestType != WellTestType.BuildUp)
                throw new InvalidWellTestDataException(nameof(data.TestType), "MDH analysis requires build-up test data.");

            var result = new WellTestAnalysisResult
            {
                AnalysisMethod = "MDH"
            };

            // MDH uses log(Δt) vs P
            var logTime = data.Time.Select(t => Math.Log10(t)).ToList();
            var pressures = data.Pressure.ToList();

            // Find middle time region
            int startIndex = FindMiddleTimeRegionMDH(data.Time, pressures);

            if (startIndex < 0 || startIndex >= logTime.Count - 2)
                throw new AnalysisConvergenceException("Could not identify middle time region for MDH analysis.");

            int endIndex = logTime.Count - 1;
            int analysisPoints = Math.Min(10, endIndex - startIndex + 1);
            startIndex = endIndex - analysisPoints + 1;

            var analysisLogTime = logTime.Skip(startIndex).Take(analysisPoints).ToArray();
            var analysisPressures = pressures.Skip(startIndex).Take(analysisPoints).ToArray();

            LinearRegression(analysisLogTime, analysisPressures, out double slope, out double intercept);

            double m = Math.Abs(slope);
            if (m < WellTestConstants.Epsilon)
                throw new AnalysisConvergenceException("Invalid slope from MDH analysis.");

            // Calculate permeability
            result.Permeability = (162.6 * data.FlowRate * data.OilFormationVolumeFactor * data.OilViscosity) /
                                 (m * data.FormationThickness);

            // Calculate skin factor
            double p1hr = intercept + slope * Math.Log10(1.0);
            double pws = pressures[0];

            double logTerm = Math.Log10(result.Permeability /
                (data.Porosity * data.OilViscosity * data.TotalCompressibility *
                 Math.Pow(data.WellboreRadius, 2)));

            result.SkinFactor = 1.151 * ((p1hr - pws) / m - logTerm - 3.23);

            // For MDH, reservoir pressure is extrapolated from the straight line
            // This is less accurate than Horner for short production times
            result.ReservoirPressure = intercept + slope * Math.Log10(data.ProductionTime);

            result.ProductivityIndex = data.FlowRate / (result.ReservoirPressure - pws);
            double deltaPs = result.SkinFactor * m / 1.151;
            result.FlowEfficiency = (result.ReservoirPressure - pws - deltaPs) / (result.ReservoirPressure - pws);
            result.DamageRatio = 1.0 / result.FlowEfficiency;

            double lastTime = data.Time.Last();
            result.RadiusOfInvestigation = Math.Sqrt(
                result.Permeability * lastTime /
                (948 * data.Porosity * data.OilViscosity * data.TotalCompressibility));

            result.RSquared = CalculateRSquared(analysisLogTime, analysisPressures, slope, intercept);

            return result;
        }

        /// <summary>
        /// Finds the middle time region for Horner analysis.
        /// </summary>
        private static int FindMiddleTimeRegion(List<double> hornerTime, List<double> pressure)
        {
            // Middle time region typically starts after early time effects
            // Look for region with relatively constant slope
            int minPoints = 5;
            if (hornerTime.Count < minPoints * 2)
                return hornerTime.Count / 2;

            // Start from middle of data
            int start = hornerTime.Count / 3;
            int end = hornerTime.Count - 1;

            // Find region with most linear behavior
            double bestR2 = 0;
            int bestStart = start;

            for (int i = start; i <= end - minPoints; i++)
            {
                var x = hornerTime.Skip(i).Take(minPoints).Select(t => Math.Log10(t)).ToArray();
                var y = pressure.Skip(i).Take(minPoints).ToArray();

                LinearRegression(x, y, out double s, out double inter);
                double r2 = CalculateRSquared(x, y, s, inter);

                if (r2 > bestR2)
                {
                    bestR2 = r2;
                    bestStart = i;
                }
            }

            return bestStart;
        }

        /// <summary>
        /// Finds the middle time region for MDH analysis.
        /// </summary>
        private static int FindMiddleTimeRegionMDH(List<double> time, List<double> pressure)
        {
            int minPoints = 5;
            if (time.Count < minPoints * 2)
                return time.Count / 2;

            int start = time.Count / 3;
            int end = time.Count - 1;

            double bestR2 = 0;
            int bestStart = start;

            for (int i = start; i <= end - minPoints; i++)
            {
                var x = time.Skip(i).Take(minPoints).Select(t => Math.Log10(t)).ToArray();
                var y = pressure.Skip(i).Take(minPoints).ToArray();

                LinearRegression(x, y, out double s, out double inter);
                double r2 = CalculateRSquared(x, y, s, inter);

                if (r2 > bestR2)
                {
                    bestR2 = r2;
                    bestStart = i;
                }
            }

            return bestStart;
        }

        /// <summary>
        /// Performs linear regression.
        /// </summary>
        private static void LinearRegression(double[] x, double[] y, out double slope, out double intercept)
        {
            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (a, b) => a * b).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < WellTestConstants.Epsilon)
            {
                slope = 0;
                intercept = sumY / n;
                return;
            }

            slope = (n * sumXY - sumX * sumY) / denominator;
            intercept = (sumY - slope * sumX) / n;
        }

        /// <summary>
        /// Calculates R-squared value.
        /// </summary>
        private static double CalculateRSquared(double[] x, double[] y, double slope, double intercept)
        {
            double yMean = y.Average();
            double ssTotal = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssResidual = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();

            if (ssTotal < WellTestConstants.Epsilon)
                return 1.0;

            return 1.0 - (ssResidual / ssTotal);
        }
    }
}

