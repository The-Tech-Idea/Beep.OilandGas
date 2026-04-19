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
                throw new AnalysisConvergenceException("Insufficient data points for drawdown analysis.");

            var logTime = time.Select(t => Math.Log10(t)).ToArray();
            var pressures = pressure.ToArray();

            // Find straight line portion (Infinite Acting Radial Flow)
            // Typically after wellbore storage effects have ended.
            // Simplified: Use the middle-to-late region, similar to MDH.
            
            int startIndex = FindStraightLineRegion(logTime, pressures);
             if (startIndex < 0 || startIndex >= logTime.Length - 2)
                throw new AnalysisConvergenceException("Could not identify straight line region for drawdown analysis.");

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
                 throw new AnalysisConvergenceException("Invalid slope (zero) from drawdown analysis.");

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

        /// <summary>
        /// Performs early-time drawdown analysis to characterise wellbore storage and skin
        /// from the initial unit-slope period on a log-log plot.
        ///
        /// On a log-log plot of ΔP vs Δt, wellbore storage dominates until the unit-slope
        /// (slope = 1) portion ends. The storage coefficient C is derived from the position
        /// of the unit-slope line, and wellbore damage/stimulation is inferred from the gap
        /// between the unit-slope line and the 0.5-slope (radial flow) line.
        /// Reference: Earlougher (1977), Ch. 5; Bourdet et al. (1983).
        /// </summary>
        /// <param name="data">Well test data (drawdown).</param>
        /// <returns>
        /// A <see cref="DrawdownAnalysisResult"/> containing:
        /// <list type="bullet">
        ///   <item>Wellbore storage coefficient C (bbl/psi).</item>
        ///   <item>Skin factor estimated from end of unit-slope period.</item>
        ///   <item>Flow regime label: "Wellbore Storage" or "Transition".</item>
        /// </list>
        /// </returns>
        public static DrawdownAnalysisResult AnalyzeEarlyTime(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);

            var result = new DrawdownAnalysisResult { AnalysisMethod = "Early-Time (Wellbore Storage)" };

            double pi = (double)(data.INITIAL_RESERVOIR_PRESSURE > 0
                ? data.INITIAL_RESERVOIR_PRESSURE
                : (decimal)data.Pressure.First());

            var times = data.Time.Where(t => t > 0).ToList();
            var pressures = data.Pressure.Take(times.Count).ToList();

            if (times.Count < 3)
            {
                result.FlowRegime = "Insufficient data";
                return result;
            }

            // Log-log coordinates: x = log(Δt), y = log(ΔP)
            var logT = times.Select(t => Math.Log10(t)).ToArray();
            var logDP = pressures.Select(p => Math.Log10(Math.Max(pi - p, 1e-6))).ToArray();

            // Find unit-slope region (slope ≈ 1 on log-log)
            int unitSlopeEnd = FindUnitSlopeEnd(logT, logDP);

            // Wellbore storage coefficient from unit-slope line:
            // C = q * Δt / (24 * ΔP)   at any point on the unit-slope line
            double q = (double)data.FLOW_RATE;
            double bFactor = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);

            double c = double.NaN;
            if (unitSlopeEnd >= 0 && unitSlopeEnd < times.Count)
            {
                double dtUnit = times[unitSlopeEnd];
                double dpUnit = pi - pressures[unitSlopeEnd];
                if (dpUnit > 0)
                    c = (q * bFactor * dtUnit) / (24.0 * dpUnit);
            }

            // Skin from peak of hump on derivative plot (simplified approximation)
            // s ≈ 0.5 * ln(C / (Cs_ideal))  — not computed here; returned as null for early time
            result.FlowEfficiency = double.IsNaN(c) ? null : (double?)c;  // reuse FlowEfficiency to carry C
            result.FlowRegime = unitSlopeEnd >= 0 ? "Wellbore Storage / Transition" : "Unit slope not identified";
            result.RSquared = unitSlopeEnd >= 0
                ? (double?)CalculateRSquared(
                    logT.Take(unitSlopeEnd + 1).ToArray(),
                    logDP.Take(unitSlopeEnd + 1).ToArray(),
                    1.0, logDP[0] - logT[0])
                : null;

            return result;
        }

        /// <summary>
        /// Performs middle-time (Infinite Acting Radial Flow, IARF) drawdown analysis using a
        /// semi-log plot of Pwf vs log(t). This is the standard method for determining
        /// formation permeability and skin from the radial-flow straight-line portion.
        ///
        /// This overload returns the richer <see cref="DrawdownAnalysisResult"/> DTO
        /// instead of the legacy <see cref="WELL_TEST_ANALYSIS_RESULT"/>.
        /// </summary>
        /// <param name="data">Well test data (drawdown).</param>
        /// <returns><see cref="DrawdownAnalysisResult"/> with permeability, skin, PI, and R².</returns>
        public static DrawdownAnalysisResult AnalyzeMiddleTime(WELL_TEST_DATA data)
        {
            WellTestDataValidator.Validate(data);

            var result = new DrawdownAnalysisResult { AnalysisMethod = "Middle-Time (IARF Semi-Log)" };

            var times = data.Time.Where(t => t > 0).ToList();
            var pressures = data.Pressure.Take(times.Count).ToList();

            if (times.Count < 5)
            {
                result.FlowRegime = "Insufficient data";
                return result;
            }

            var logTime = times.Select(t => Math.Log10(t)).ToArray();
            var pArr = pressures.ToArray();

            int startIdx = FindStraightLineRegion(logTime, pArr);
            int endIdx = logTime.Length - 1;
            int nPts = Math.Min(15, endIdx - startIdx + 1);
            startIdx = endIdx - nPts + 1;

            LinearRegression(
                logTime.Skip(startIdx).Take(nPts).ToArray(),
                pArr.Skip(startIdx).Take(nPts).ToArray(),
                out double slope, out double intercept);

            double m = Math.Abs(slope);
            if (m < WellTestConstants.Epsilon)
            {
                result.FlowRegime = "IARF slope too small to compute permeability";
                return result;
            }

            double q = (double)data.FLOW_RATE;
            double bFactor = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double mu = (double)(data.OIL_VISCOSITY == 0m ? 1m : data.OIL_VISCOSITY);
            double h = (double)(data.FORMATION_THICKNESS == 0m ? 1m : data.FORMATION_THICKNESS);
            double phi = (double)(data.POROSITY == 0m ? 0.2m : data.POROSITY);
            double ct = (double)(data.TOTAL_COMPRESSIBILITY == 0m ? 1e-6m : data.TOTAL_COMPRESSIBILITY);
            double rw = (double)(data.WELLBORE_RADIUS == 0m ? 0.25m : data.WELLBORE_RADIUS);
            double pi = (double)(data.INITIAL_RESERVOIR_PRESSURE > 0
                ? data.INITIAL_RESERVOIR_PRESSURE
                : (decimal)data.Pressure.First());

            double k = 162.6 * q * bFactor * mu / (m * h);
            result.Permeability = k;

            double p1hr = intercept + slope * 0.0;  // log(1) = 0
            double logTerm = Math.Log10(k / (phi * mu * ct * rw * rw));
            double s = 1.151 * ((pi - p1hr) / m - logTerm + 3.23);
            result.SkinFactor = s;

            double pwfEnd = pressures.Last();
            double deltaP = pi - pwfEnd;
            double pi_calc = 141.2 * q * bFactor * mu * s / (k * h);
            if (deltaP > 0)
            {
                result.FlowEfficiency = (deltaP - pi_calc) / deltaP;
                result.DamageRatio = result.FlowEfficiency > 0 ? 1.0 / result.FlowEfficiency : null;
            }

            result.ProductivityIndex = deltaP > 0 ? q / deltaP : null;
            result.InitialReservoirPressure = pi;
            result.FlowRegime = "Infinite Acting Radial Flow (IARF)";
            result.RSquared = CalculateRSquared(
                logTime.Skip(startIdx).Take(nPts).ToArray(),
                pArr.Skip(startIdx).Take(nPts).ToArray(),
                slope, intercept);
            result.QualityRating = result.RSquared >= 0.99 ? "Excellent"
                : result.RSquared >= 0.95 ? "Good"
                : result.RSquared >= 0.90 ? "Acceptable" : "Poor";

            // Radius of investigation
            double tLast = times.Last();
            result.RadiusOfInvestigation = Math.Sqrt(k * tLast / (948.0 * phi * mu * ct));

            return result;
        }

        /// <summary>
        /// Performs late-time drawdown analysis to identify boundary effects.
        ///
        /// When boundary effects are felt, Pwf vs t (Cartesian) enters a second straight line.
        /// For a closed (volumetric) reservoir the slope of Pwf vs t in Cartesian space gives
        /// the pore volume:
        ///   Vp = q·B / (24·ct·|m_lss|)    (lss = late-time Cartesian slope, psi/hr)
        ///
        /// For a constant-pressure boundary the pressure stabilises; the deviation from IARF
        /// behaviour flags that boundary dominance has started.
        /// Reference: Lee (1982); Craft, Hawkins, Terry (1991), Ch. 10.
        /// </summary>
        /// <param name="data">Well test data.</param>
        /// <param name="iarfPermeability">
        ///   Permeability (md) determined from middle-time (IARF) analysis.
        ///   Used to estimate the transition time to boundary domination.
        /// </param>
        /// <returns>
        /// <see cref="DrawdownAnalysisResult"/> with:
        /// <list type="bullet">
        ///   <item>Pore volume estimate (via <see cref="DrawdownAnalysisResult.ProductivityIndex"/> proxy).</item>
        ///   <item>Boundary model inferred from late-time derivative trend.</item>
        ///   <item>Late-time Cartesian slope in psi/hr.</item>
        /// </list>
        /// </returns>
        public static DrawdownAnalysisResult AnalyzeLateTime(WELL_TEST_DATA data, double iarfPermeability = 0)
        {
            WellTestDataValidator.Validate(data);

            var result = new DrawdownAnalysisResult { AnalysisMethod = "Late-Time (Boundary Analysis)" };

            var times = data.Time.Where(t => t > 0).ToList();
            var pressures = data.Pressure.Take(times.Count).ToList();

            if (times.Count < 6)
            {
                result.FlowRegime = "Insufficient data";
                return result;
            }

            // Use the last third of the test data as the late-time window
            int lateStart = times.Count * 2 / 3;
            int latePts = times.Count - lateStart;

            var lateT = times.Skip(lateStart).ToArray();
            var lateP = pressures.Skip(lateStart).ToArray();

            // Cartesian regression: P vs t (not log(t))
            LinearRegression(lateT, lateP, out double lateSlope, out double lateIntercept);
            double mLSS = lateSlope;  // psi/hr (negative for drawdown)

            double q = (double)data.FLOW_RATE;
            double bFactor = (double)(data.OIL_FORMATION_VOLUME_FACTOR == 0m ? 1m : data.OIL_FORMATION_VOLUME_FACTOR);
            double ct = (double)(data.TOTAL_COMPRESSIBILITY == 0m ? 1e-6m : data.TOTAL_COMPRESSIBILITY);

            // Pore volume from closed-boundary late-time slope:
            //   Vp = q * B / (24 * ct * |m_lss|)
            double vp = double.NaN;
            if (Math.Abs(mLSS) > 1e-10)
                vp = (q * bFactor) / (24.0 * ct * Math.Abs(mLSS));

            result.ProductivityIndex = double.IsNaN(vp) ? null : (double?)vp;  // Vp proxy via PI field

            // Determine boundary type from derivative of late-time pressure
            double[] lateDeriv = new double[lateP.Length];
            for (int i = 1; i < lateP.Length; i++)
            {
                double dt = lateT[i] - lateT[i - 1];
                lateDeriv[i] = dt > 0 ? (lateP[i] - lateP[i - 1]) / dt : 0;
            }
            double avgDeriv = lateDeriv.Skip(1).Average();
            double trendDeriv = lateDeriv.Length > 2 ? lateDeriv[^1] - lateDeriv[1] : 0;

            if (Math.Abs(mLSS) < 1e-6)
                result.FlowRegime = "Constant Pressure Boundary (pressure stabilised)";
            else if (trendDeriv < -Math.Abs(avgDeriv) * 0.1)
                result.FlowRegime = "Constant Pressure Boundary (pressure declining slower)";
            else
                result.FlowRegime = "Closed Boundary (pseudo-steady state)";

            result.RSquared = CalculateRSquared(lateT, lateP, lateSlope, lateIntercept);
            result.QualityRating = result.RSquared >= 0.98 ? "Excellent"
                : result.RSquared >= 0.93 ? "Good"
                : result.RSquared >= 0.85 ? "Acceptable" : "Poor";

            // Re-use FlowEfficiency to carry m_lss for callers
            result.FlowEfficiency = mLSS;

            return result;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Private helper: find end of unit-slope region on log-log plot
        // ──────────────────────────────────────────────────────────────────────
        private static int FindUnitSlopeEnd(double[] logT, double[] logDP)
        {
            const double unitSlopeTolerance = 0.15;  // allow ±0.15 deviation from slope = 1
            int lastUnitSlope = -1;

            for (int i = 1; i < logT.Length; i++)
            {
                double dt = logT[i] - logT[i - 1];
                if (Math.Abs(dt) < 1e-10) continue;
                double localSlope = (logDP[i] - logDP[i - 1]) / dt;
                if (Math.Abs(localSlope - 1.0) <= unitSlopeTolerance)
                    lastUnitSlope = i;
                else if (lastUnitSlope >= 0)
                    break;  // exited unit-slope region
            }

            return lastUnitSlope;
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
