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
    /// Provides gas well test analysis methods using pseudo-pressure m(p).
    /// </summary>
    public static class GasWellAnalysis
    {
        /// <summary>
        /// Analyzes a gas well build-up test using Pseudo-Pressure m(p).
        /// </summary>
        /// <param name="data">Well test data.</param>
        /// <param name="gasGravity">Gas specific gravity (Air=1.0).</param>
        /// <param name="reservoirTemperature">Reservoir temperature in Rankin.</param>
        /// <param name="n2">Mole fraction of Nitrogen (optional).</param>
        /// <param name="co2">Mole fraction of CO2 (optional).</param>
        /// <param name="h2s">Mole fraction of H2S (optional).</param>
        /// <returns>Analysis result including permeability and skin.</returns>
        public static WELL_TEST_ANALYSIS_RESULT AnalyzeGasBuildUp(
            WELL_TEST_DATA data,
            double gasGravity,
            double reservoirTemperature,
            double n2 = 0,
            double co2 = 0,
            double h2s = 0)
        {
            WellTestDataValidator.Validate(data);

            if (!Enum.TryParse<WellTestType>(data.TEST_TYPE, out var testType) || testType != WellTestType.BuildUp)
                throw new InvalidWellTestDataException(nameof(data.TEST_TYPE), "Gas build-up analysis requires build-up test data.");

            var result = new WELL_TEST_ANALYSIS_RESULT
            {
                ANALYSIS_METHOD = "Gas Pseudo-Pressure (Horner)"
            };

            // 1. Convert Pressure to Pseudo-Pressure m(p)
            // m(p) = 2 * Integral( p / (mu * z) ) dp
            // We need to calculate m(p) for each pressure point in the test data.
            // Also need m(p) at initial/boundary conditions.

            // Get unique sorted pressures to build an integration table
            var allPressures = data.Pressure.Distinct().OrderBy(p => p).ToList();
            if (allPressures.Count < 2)
                 throw new AnalysisConvergnceException("Insufficient pressure resolution for gas analysis.");
            
            // Build m(p) lookup table
            var mpTable = BuildPseudoPressureTable(allPressures, gasGravity, reservoirTemperature, n2, co2, h2s);
            
            // Map test data pressures to m(p)
            var mpValues = data.Pressure.Select(p => InterpolateMp(p, mpTable)).ToList();
            
            // 2. Perform Horner Analysis using m(p)
            // Plot m(pws) vs log( (tp + dt) / dt )
            // Slope m_slope has units psi^2/cp/log-cycle
            
            double productionTime = (double)data.PRODUCTION_TIME;
            var hornerTime = data.Time.Select(t => (productionTime + t) / t).ToList();
            var logHornerTime = hornerTime.Select(t => Math.Log10(t)).ToArray();
            var mpArray = mpValues.ToArray();

            // Find straight line region (Middle Time)
            int startIndex = FindStraightLineRegion(logHornerTime, mpArray);
            int endIndex = logHornerTime.Length - 1;
            int analysisPoints = Math.Min(15, endIndex - startIndex + 1);
            startIndex = endIndex - analysisPoints + 1;

            var analysisX = logHornerTime.Skip(startIndex).Take(analysisPoints).ToArray();
            var analysisY = mpArray.Skip(startIndex).Take(analysisPoints).ToArray(); // m(p)

            LinearRegression(analysisX, analysisY, out double slope, out double intercept);

            double m = Math.Abs(slope); // Slope of m(p) vs log time

            // 3. Calculate Permeability
            // k = 1637 * T * q_g / (m * h)
            // q_g in Mscf/D
            
            double q_gas = (double)data.FLOW_RATE; // Assumed Mscf/D for gas wells
            double h = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);
            
            // Note: Standard formula for m(p) analysis
            // k = (1637 * T * q) / (m * h)
            // T in Rankin
            // m in psi^2/cp / cycle
            
            double k = (1637 * reservoirTemperature * q_gas) / (m * h);
            result.PERMEABILITY = (decimal)k;

            // 4. Calculate Skin
            // s = 1.151 * [ (m(p1hr) - m(pwf)) / m - log(k / (phi * mu * ct * rw^2)) + 3.23 ]
            // Note: Use properties at P_avg or P_wf for skin calculation terms? Use P_avg typically.
            // Simplified: Use properties at final shut-in pressure.
            
            double mp1hr = intercept + slope * Math.Log10(1.0); // m(p) at Horner time = 1 (Infinite shut-in?) 
            // Wait, Horner plot: time increases to left. t=infinite => (tp+dt)/dt = 1.
            // So Intercept is m(p*) -> extrapolated reservoir pressure.
            
            // We need m(p) at dt=1hr.
            // (tp + 1) / 1 ~= tp ?? No.
            // Horner Time at 1 hr = (tp + 1)/1.
            double time1hr = 1.0;
            double hornerTime1hr = (productionTime + time1hr) / time1hr;
            double logHornerTime1hr = Math.Log10(hornerTime1hr);
            
            double mp_at_1hr = intercept + slope * logHornerTime1hr; 
            
            // m(pwf) is the m(p) at instant of shut-in (dt=0).
            // Usually taken as first data point data.Pressure[0].
            double mp_wf = mpValues[0];

            double phi = (double)data.POROSITY;
            double rw = (double)data.WELLBORE_RADIUS;
            
            // Evaluate viscosity and compressibility at Pwf (flowing pressure)
            double pwf = data.Pressure[0];
            double mu_wf = CalculateViscosity(pwf, reservoirTemperature, gasGravity, n2, co2, h2s);
            double z_wf = CalculateZFactor(pwf, reservoirTemperature, gasGravity, n2, co2, h2s);
            double cg = 1.0 / pwf; // Simplified gas compressibility ~ 1/P
            // Ideally should be 1/P - (1/Z)(dZ/dP).
            double ct = cg * (1 - 0); // Approx
            
            double logTerm = Math.Log10(k / (phi * mu_wf * ct * rw * rw));
            
            // s formula for Gas BuildUp (Horner):
            // s = 1.151 * [ (m(p1hr_shut-in) - m(pwf)) / m - log(k/...) + 3.23 ]
            // Note: m(p1hr_shut-in) refers to the value on the straight line at dt=1hr.
            
            double s = 1.151 * ((mp_at_1hr - mp_wf) / m - logTerm + 3.23);
            result.SKIN_FACTOR = (decimal)s;

            // 5. Reservoir Pressure
            // Extrapolate to (tp+dt)/dt = 1 (Infinite time) -> Intercept
            double mp_res = intercept;
            result.RESERVOIR_PRESSURE = (decimal)InverseInterpolateMp(mp_res, mpTable);
            
            result.R_SQUARED = (decimal)CalculateRSquared(analysisX, analysisY, slope, intercept);

            return result;
        }

        // --- Helper Methods ---

        private static Dictionary<double, double> BuildPseudoPressureTable(
            List<double> pressures, double gravity, double temp, double n2, double co2, double h2s)
        {
            // Create a fine grid from 0 to Max Pressure for accurate integration
            double maxP = pressures.Max();
            int steps = 200;
            double dp = maxP / steps;
            
            var table = new Dictionary<double, double>();
            double mp = 0;
            table.Add(0, 0);

            for(int i=1; i<=steps; i++)
            {
                double p_prev = (i-1)*dp;
                double p_curr = i*dp;
                double p_mid = (p_prev + p_curr) / 2.0;
                
                // Simpson's rule or Trapezoidal
                // Trapezoidal: 0.5 * (val_prev + val_curr) * dp
                
                double mu_prev = CalculateViscosity(Math.Max(0.1, p_prev), temp, gravity, n2, co2, h2s);
                double z_prev = CalculateZFactor(Math.Max(0.1, p_prev), temp, gravity, n2, co2, h2s);
                double val_prev = (2.0 * p_prev) / (mu_prev * z_prev);
                
                double mu_curr = CalculateViscosity(p_curr, temp, gravity, n2, co2, h2s);
                double z_curr = CalculateZFactor(p_curr, temp, gravity, n2, co2, h2s);
                double val_curr = (2.0 * p_curr) / (mu_curr * z_curr);
                
                mp += 0.5 * (val_prev + val_curr) * dp;
                table.Add(p_curr, mp);
            }

            return table;
        }

        private static double InterpolateMp(double p, Dictionary<double, double> table)
        {
            if (table.ContainsKey(p)) return table[p];
            
            var below = table.Where(k => k.Key <= p).OrderByDescending(k => k.Key).FirstOrDefault();
            var above = table.Where(k => k.Key >= p).OrderBy(k => k.Key).FirstOrDefault();
            
            if (below.Key == 0 && below.Value == 0 && above.Key == 0) return 0; // Should not happen with well formed table
            if (above.Key == 0) return below.Value; // P > Max
            
            double ratio = (p - below.Key) / (above.Key - below.Key);
            return below.Value + ratio * (above.Value - below.Value);
        }

        private static double InverseInterpolateMp(double mp, Dictionary<double, double> table)
        {
             // Find P given m(p)
             var below = table.Where(k => k.Value <= mp).OrderByDescending(k => k.Key).FirstOrDefault();
             var above = table.Where(k => k.Value >= mp).OrderBy(k => k.Key).FirstOrDefault();
             
             if (above.Key == 0) return below.Key; // Extrapolating up
             
             double ratio = (mp - below.Value) / (above.Value - below.Value);
             return below.Key + ratio * (above.Key - below.Key);
        }

        // --- PVT Correlations (Simplified Lee-Gonzalez & Hall-Yarborough) ---
        // Note: For rigorous internal use since we can't guarantee external service availability here easily
        
        private static double CalculateZFactor(double P, double T, double G, double n2, double co2, double h2s)
        {
            // P in psi, T in Rankin
            // Hall-Yarborough Simplified
            double P_pr = P / (756.8 - 131.0 * G - 3.6 * co2 + 67.5 * n2); // Pseudo-critical approx
            double T_pr = T / (169.2 + 349.5 * G - 74.0 * co2 + 73.0 * n2);
            
            if (T_pr <= 0) return 0.9;
            
            // Simplified Dranchuk-Abou-Kassem or Hall-Yarborough is complex to implement entirely inline.
            // Use very rough approximation for test purpose if simpler:
            // Papay's Formula
            double z = 1 - 3.52 * P_pr / Math.Pow(10, 0.9813 * T_pr) + 0.274 * Math.Pow(P_pr, 2) / Math.Pow(10, 0.8157 * T_pr);
            return Math.Max(0.1, z);
        }

        private static double CalculateViscosity(double P, double T, double G, double n2, double co2, double h2s)
        {
            // Lee-Gonzalez-Eakin
            double z = CalculateZFactor(P, T, G, n2, co2, h2s);
            double rho_g = (2.827 * G * P) / (z * T); // Density g/cc
            
            double M = 28.96 * G; // Molecular weight
            double X = 3.5 + 986 / T + 0.01 * M;
            double Y = 2.4 - 0.2 * X;
            double K = (9.4 + 0.02 * M) * Math.Pow(T, 1.5) / (209 + 19 * M + T); // Viscosity at 1 atm
            
            double mu = K * 1e-4 * Math.Exp(X * Math.Pow(rho_g, Y));
            return Math.Max(0.01, mu);
        }

        // --- Regression Utils ---
        private static int FindStraightLineRegion(double[] x, double[] y)
        {
             int minPoints = 4;
             if (x.Length < minPoints * 2) return x.Length / 2;
             
             int bestStart = x.Length / 3;
             double bestR2 = 0;
             
             for (int i = x.Length/4; i < x.Length - minPoints; i++)
             {
                  var subX = x.Skip(i).Take(minPoints).ToArray();
                  var subY = y.Skip(i).Take(minPoints).ToArray();
                  LinearRegression(subX, subY, out double s, out double inte);
                  double r2 = CalculateRSquared(subX, subY, s, inte);
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
            slope = Math.Abs(denominator) < 1e-10 ? 0 : (n * sumXY - sumX * sumY) / denominator;
            intercept = (sumY - slope * sumX) / n;
        }

        private static double CalculateRSquared(double[] x, double[] y, double slope, double intercept)
        {
            double yMean = y.Average();
            double ssTotal = y.Sum(yi => Math.Pow(yi - yMean, 2));
            double ssResidual = x.Zip(y, (xi, yi) => Math.Pow(yi - (slope * xi + intercept), 2)).Sum();
            return ssTotal < 1e-10 ? 1.0 : 1.0 - (ssResidual / ssTotal);
        }
    }
}
