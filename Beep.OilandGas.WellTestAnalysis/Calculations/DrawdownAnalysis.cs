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
    /// Provides drawdown test analysis methods.
    /// </summary>
    public static class DrawdownAnalysis
    {
        /// <summary>
        /// Analyzes a constant-rate drawdown test.
        /// Uses the semi-log plot of Pwf vs log(t).
        /// </summary>
        /// <param name="data">Well test data.</param>
        /// <returns>Analysis result including permeability and skin.</returns>
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeDrawdown(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);

            if (!Enum.TryParse<WellTestType>(data.TEST_TYPE, out var testType) || testType != WellTestType.Drawdown)
                throw new InvalidWellTestDataException(nameof(data.TEST_TYPE), "Drawdown analysis requires drawdown test data.");

            var result = new WELL_TEST_ANALYSIS_RESULT
            {
                ANALYSIS_METHOD = "Drawdown (Semi-Log)"
            };

            // Drawdown Plot: Pwf vs log(t)
            // Equation: Pwf = Pi - m * [log(t) + log(k/(phi*mu*ct*rw^2)) - 3.23 + 0.87*s]
            // Slope m = 162.6 * q * B * mu / (k * h)

            var time = data.Time.Where(t => t > 0).ToList();
            var pressure = data.Pressure.Take(time.Count).ToList(); // Ensure matching length

            if (time.Count < 5)
                throw new AnalysisConvergnceException("Insufficient data points for drawdown analysis.");

            var logTime = time.Select(t => Math.Log10(t)).ToArray();
            var pressures = pressure.ToArray();

            // Find straight line portion (Infinite Acting Radial Flow)
            // Typically after wellbore storage effects have ended.
            // Simplified: Use the middle-to-late region, similar to MDH.
            
            int startIndex = FindStraightLineRegion(logTime, pressures);
             if (startIndex < 0 || startIndex >= logTime.Length - 2)
                throw new AnalysisConvergnceException("Could not identify straight line region for drawdown analysis.");

            int endIndex = logTime.Length - 1;
            int analysisPoints = Math.Min(15, endIndex - startIndex + 1); // Use more points if available
            startIndex = endIndex - analysisPoints + 1; // Analyze the end portion

             // Linear regression Pwf vs log(t)
            // Pwf = Intercept + Slope * log(t)
             var analysisLogTime = logTime.Skip(startIndex).Take(analysisPoints).ToArray();
            var analysisPressures = pressures.Skip(startIndex).Take(analysisPoints).ToArray();

            LinearRegression(analysisLogTime, analysisPressures, out double slope, out double intercept);

            // Slope m is negative for drawdown (pressure decreases with time)
            double m = Math.Abs(slope);

            if (m < WellTestConstants.Epsilon)
                 throw new AnalysisConvergnceException("Invalid slope (zero) from drawdown analysis.");

            // Calculate Permeability k
            // k = 162.6 * q * B * mu / (m * h)
             double flowRate = (double)data.FLOW_RATE;
            double bFactor = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double mu = (double)(data.OIL_VISCOSITY == 0m ? 1m : data.OIL_VISCOSITY);
            double h = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);

            double k = (162.6 * flowRate * bFactor * mu) / (m * h);
            result.PERMEABILITY = (decimal)k;

            // Calculate Skin Factor s
            // s = 1.151 * [ (Pi - P1hr)/m - log(k / (phi * mu * ct * rw^2)) + 3.23 ]
            // Note: P1hr is Pwf at t=1hr from the straight line.
                        
            double p1hr = intercept + slope * Math.Log10(1.0); // Predict P at log(t)=0 i.e. t=1
            // Note: For drawdown, P1hr is typically LESS than Pi.
            // If the test started from Pi, we need Pi.
            // Assumption: data.INITIAL_RESERVOIR_PRESSURE is provided or inferred.
            // If not provided, we can't calculate Skin exactly without Pi.
            // However, usually Pi is the pressure at t=0 (not log(t)=0).
            // Let's assume the first data point at t=0 is Pi, or use a provided Pi.
            
            double Pi = (double)(data.INITIAL_RESERVOIR_PRESSURE > 0 ? data.INITIAL_RESERVOIR_PRESSURE : (decimal)data.Pressure.First());

             double phi = (double)data.POROSITY;
            double ct = (double)(data.TOTAL_COMPRESSIBILITY == 0m ? 1e-6m : data.TOTAL_COMPRESSIBILITY); // Default if 0
            double rw = (double)(data.WELLBORE_RADIUS == 0m ? 0.25m : data.WELLBORE_RADIUS);

            double logTerm = Math.Log10(k / (phi * mu * ct * rw * rw));
            
            // Formula rearrangement:
            // P1hr = Pi - m * [log(k/...) - 3.23 + 0.87*s]
            // (Pi - P1hr)/m = log(k/...) - 3.23 + 0.87*s
            // 0.87*s = (Pi - P1hr)/m - log(k/...) + 3.23
            // s = 1.151 * [ (Pi - P1hr)/m - log(k/...) + 3.23 ]
            
            double s = 1.151 * ( (Pi - p1hr)/m - logTerm + 3.23 );
            result.SKIN_FACTOR = (decimal)s;
            
            result.RESERVOIR_PRESSURE = (decimal)Pi; // Reported for consistency
            result.R_SQUARED = (decimal)CalculateRSquared(analysisLogTime, analysisPressures, slope, intercept);

            // Radius of Investigation
            double t_last = time.Last();
            double rin = Math.Sqrt( (k * t_last) / (948 * phi * mu * ct) );
            result.RADIUS_OF_INVESTIGATION = (decimal)rin;

            // Flow Efficiency
            // FE = (Pi - Pwf - DeltaP_skin) / (Pi - Pwf)
            // Simplified: FE = J_actual / J_ideal
             double deltaP_skin = 141.2 * flowRate * bFactor * mu * s / (k * h); // 0.87 * m * s
             // Using Pwf at end of test
             double Pwf_end = pressures.Last();
             double drawdown = Pi - Pwf_end;
             
             if (drawdown > 0)
             {
                 double fe = (drawdown - deltaP_skin) / drawdown;
                 result.FLOW_EFFICIENCY = (decimal)fe;
                 result.DAMAGE_RATIO = (decimal)(fe != 0 ? 1.0/fe : 0);
             }

            return result;
        }

        private static int FindStraightLineRegion(double[] logTime, double[] pressure)
        {
             // Similar logic to MDH/Horner - find best linear fit in middle/late time
             // Simple search for best R2
             int minPoints = 5;
             if (logTime.Length < minPoints * 2) return logTime.Length / 2;
             
             int bestStart = logTime.Length / 3;
             double bestR2 = 0;
             
             for (int i = logTime.Length/4; i < logTime.Length - minPoints; i++)
             {
                  var x = logTime.Skip(i).Take(minPoints).ToArray();
                  var y = pressure.Skip(i).Take(minPoints).ToArray();
                  LinearRegression(x, y, out double s, out double inte);
                  double r2 = CalculateRSquared(x, y, s, inte);
                  if (r2 > bestR2)
                  {
                      bestR2 = r2;
                      bestStart = i;
                  }
             }
             return bestStart;
        }
        
        private static void LinearRegression(double[] x, double[] y, out double slope, out double intercept)
        {
            int n = x.Length;
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (a, b) => a * b).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            double denominator = n * sumX2 - sumX * sumX;
            if (Math.Abs(denominator) < 1e-10)
            {
                slope = 0;
                intercept = sumY / n;
                return;
            }

            slope = (n * sumXY - sumX * sumY) / denominator;
            intercept = (sumY - slope * sumX) / n;
        }

        private static double CalculateRSquared(double[] x, double[] y, double slope, double intercept)
        {
            double yMean = y.Average();
            double ssTotal = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssResidual = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();

            if (ssTotal < 1e-10) return 1.0;
            return 1.0 - (ssResidual / ssTotal);
        }
    }
}
