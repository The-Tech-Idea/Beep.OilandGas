using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.WellTestAnalysis.Exceptions;
using Beep.OilandGas.Models.Data.WellTestAnalysis;
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
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeHorner(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);

            // TEST_TYPE is stored as string in WELL_TEST_DATA; parse to enum
            if (!Enum.TryParse<WellTestType>(data.TEST_TYPE, out var testType) || testType != WellTestType.BuildUp)
                throw new InvalidWellTestDataException(nameof(data.TEST_TYPE), "Horner analysis requires build-up test data.");

            double productionTime = (double)data.PRODUCTION_TIME;
            if (productionTime <= 0)
                throw new InvalidWellTestDataException(nameof(data.PRODUCTION_TIME), "Production time must be positive for Horner analysis.");

            var result = new WELL_TEST_ANALYSIS_RESULT
            {
                ANALYSIS_METHOD = "Horner"
            };

            // Calculate Horner time: (tp + Î”t) / Î”t
            // Use local doubles for arithmetic to avoid decimal/double mixing
            var hornerTime = data.Time.Select(t => (productionTime + t) / t).ToList();
            var pressures = data.Pressure.ToList();

            // Find the middle time region (semi-log straight line)
            int startIndex = FindMiddleTimeRegion(hornerTime, pressures);

            if (startIndex < 0 || startIndex >= hornerTime.Count - 2)
                throw new AnalysisConvergenceException("Could not identify middle time region for Horner analysis.");

            // Use last portion of middle time region for analysis
            int endIndex = hornerTime.Count - 1;
            int analysisPoints = Math.Min(10, endIndex - startIndex + 1);
            startIndex = endIndex - analysisPoints + 1;

            // Linear regression on semi-log plot: P vs log((tp+Î”t)/Î”t)
            var logHornerTime = hornerTime.Skip(startIndex).Take(analysisPoints)
                .Select(t => Math.Log10(t)).ToArray();
            var analysisPressures = pressures.Skip(startIndex).Take(analysisPoints).ToArray();

            LinearRegression(logHornerTime, analysisPressures, out double slope, out double intercept);

            // Calculate permeability: k = (162.6 * q * B * Î¼) / (m * h)
            // where m is the slope in psi/log cycle
            double m = Math.Abs(slope);
            if (m < WellTestConstants.Epsilon)
                throw new AnalysisConvergenceException("Invalid slope from Horner analysis.");

            // Convert data fields to double for calculation
            double flowRate = (double)data.FLOW_RATE;
            double bFactor = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double mu = (double)(data.OIL_VISCOSITY == 0m ? 1m : data.OIL_VISCOSITY);
            double formationThickness = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);

            double permeability = (162.6 * flowRate * bFactor * mu) / (m * formationThickness);
            result.PERMEABILITY = (decimal)permeability;

            // Calculate skin factor: s = 1.151 * [(P1hr - Pwf) / m - log(k / (Ï† * Î¼ * ct * rwÂ²)) - 3.23]
            // Extrapolate to Horner time = 1 (Î”t = tp)
            double p1hr = intercept + slope * Math.Log10(1.0);
            double pws = pressures[0]; // Initial shut-in pressure

            double porosity = (double)data.POROSITY;
            double totalCompressibility = (double)(data.TOTAL_COMPRESSIBILITY == 0m ? 1m : data.TOTAL_COMPRESSIBILITY);
            double rw = (double)(data.WELLBORE_RADIUS == 0m ? 1m : data.WELLBORE_RADIUS);

            double logTerm = Math.Log10(permeability / (porosity * mu * totalCompressibility * Math.Pow(rw, 2)));
            double skin = 1.151 * ((p1hr - pws) / m - logTerm - 3.23);
            result.SKIN_FACTOR = (decimal)skin;

            // Extrapolate to infinite time (Horner time = 1) for reservoir pressure
            result.RESERVOIR_PRESSURE = (decimal)intercept;

            // Calculate productivity index
            double reservoirPressure = (double)result.RESERVOIR_PRESSURE;
            double pj = flowRate / (reservoirPressure - pws);
            result.PRODUCTIVITY_INDEX = (decimal)pj;

            // Calculate flow efficiency
            double deltaPs = skin * m / 1.151;
            double flowEfficiency = (reservoirPressure - pws - deltaPs) / (reservoirPressure - pws);
            result.FLOW_EFFICIENCY = (decimal)flowEfficiency;

            // Calculate damage ratio
            result.DAMAGE_RATIO = (decimal)(1.0 / flowEfficiency);

            // Calculate radius of investigation
            double lastTime = data.Time.Last();
            double radius = Math.Sqrt(permeability * lastTime / (948 * porosity * mu * totalCompressibility));
            result.RADIUS_OF_INVESTIGATION = (decimal)radius;

            // Calculate R-squared
            result.R_SQUARED = (decimal)CalculateRSquared(logHornerTime, analysisPressures, slope, intercept);

            return result;
        }

        /// <summary>
        /// Performs Miller-Dyes-Hutchinson (MDH) analysis on build-up test data.
        /// </summary>
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeMDH(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);

            // TEST_TYPE stored as string; parse
            if (!Enum.TryParse<WellTestType>(data.TEST_TYPE, out var mdhTestType) || mdhTestType != WellTestType.BuildUp)
                throw new InvalidWellTestDataException(nameof(data.TEST_TYPE), "MDH analysis requires build-up test data.");

            var result = new WELL_TEST_ANALYSIS_RESULT
            {
                ANALYSIS_METHOD = "MDH"
            };

            // MDH uses log(Î”t) vs P
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

            // Convert inputs to double
            double flowRate2 = (double)data.FLOW_RATE;
            double bFactor2 = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double mu2 = (double)(data.OIL_VISCOSITY == 0m ? 1m : data.OIL_VISCOSITY);
            double formationThickness2 = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);

            double permeability2 = (162.6 * flowRate2 * bFactor2 * mu2) / (m * formationThickness2);
            result.PERMEABILITY = (decimal)permeability2;

            double p1hr2 = intercept + slope * Math.Log10(1.0);
            double pws2 = pressures[0];

            double porosity2 = (double)data.POROSITY;
            double totalCompressibility2 = (double)(data.TOTAL_COMPRESSIBILITY == 0m ? 1m : data.TOTAL_COMPRESSIBILITY);
            double rw2 = (double)(data.WELLBORE_RADIUS == 0m ? 1m : data.WELLBORE_RADIUS);

            double logTerm2 = Math.Log10(permeability2 / (porosity2 * mu2 * totalCompressibility2 * Math.Pow(rw2, 2)));
            double skin2 = 1.151 * ((p1hr2 - pws2) / m - logTerm2 - 3.23);
            result.SKIN_FACTOR = (decimal)skin2;

            // Reservoir pressure extrapolation
            double prodTime2 = (double)data.PRODUCTION_TIME;
            result.RESERVOIR_PRESSURE = (decimal)(intercept + slope * Math.Log10(prodTime2));

            double reservoirPressure2 = (double)result.RESERVOIR_PRESSURE;
            double pj2 = flowRate2 / (reservoirPressure2 - pws2);
            result.PRODUCTIVITY_INDEX = (decimal)pj2;

            double deltaPs2 = skin2 * m / 1.151;
            double flowEfficiency2 = (reservoirPressure2 - pws2 - deltaPs2) / (reservoirPressure2 - pws2);
            result.FLOW_EFFICIENCY = (decimal)flowEfficiency2;
            result.DAMAGE_RATIO = (decimal)(1.0 / flowEfficiency2);

            double lastTime2 = data.Time.Last();
            double radius2 = Math.Sqrt(permeability2 * lastTime2 / (948 * porosity2 * mu2 * totalCompressibility2));
            result.RADIUS_OF_INVESTIGATION = (decimal)radius2;

            result.R_SQUARED = (decimal)CalculateRSquared(analysisLogTime, analysisPressures, slope, intercept);

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

