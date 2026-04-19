using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.Models.Data.WellTestAnalysis;

namespace Beep.OilandGas.WellTestAnalysis.Calculations
{
    /// <summary>
    /// Provides pressure derivative calculation for diagnostic plots.
    /// </summary>
    public static class DerivativeAnalysis
    {
        /// <summary>
        /// Calculates pressure derivative for diagnostic analysis.
        /// </summary>
        public static List<PRESSURE_TIME_POINT> CalculateDerivative(List<PRESSURE_TIME_POINT> data, double smoothingFactor = WellTestConstants.DefaultDerivativeSmoothing)
        {
            if (data == null || data.Count < 3)
                return new List<PRESSURE_TIME_POINT>();

            var result = new List<PRESSURE_TIME_POINT>();
            var sortedData = data.OrderBy(p => p.TIME).ToList();

            for (int i = 0; i < sortedData.Count; i++)
            {
                var point = new PRESSURE_TIME_POINT
                {
                    TIME = sortedData[i].TIME,
                    PRESSURE = sortedData[i].PRESSURE
                };

                if (i == 0 || i == sortedData.Count - 1)
                {
                    // End points: use forward/backward difference
                    if (i == 0)
                    {
                        double dt = (double)(((sortedData[i + 1].TIME ?? 0.0) - (sortedData[i].TIME ?? 0.0)));
                        if (Math.Abs(dt) < 1e-12) dt = 1e-12;
                        double dp = (double)(((sortedData[i + 1].PRESSURE ?? 0.0) - (sortedData[i].PRESSURE ?? 0.0)));
                        double deriv = dp / dt * (double)(sortedData[i].TIME ?? 0.0);
                        point.PRESSURE_DERIVATIVE = deriv;
                    }
                    else
                    {
                        double dt = (double)(((sortedData[i].TIME ?? 0.0) - (sortedData[i - 1].TIME ?? 0.0)));
                        if (Math.Abs(dt) < 1e-12) dt = 1e-12;
                        double dp = (double)(((sortedData[i].PRESSURE ?? 0.0) - (sortedData[i - 1].PRESSURE ?? 0.0)));
                        double deriv = (dp / dt) * (double)(sortedData[i].TIME ?? 0.0);
                        point.PRESSURE_DERIVATIVE = deriv;
                    }
                }
                else
                {
                    // Central difference with smoothing
                    double dt1 = (double)(((sortedData[i].TIME ?? 0.0) - (sortedData[i - 1].TIME ?? 0.0)));
                    double dt2 = (double)(((sortedData[i + 1].TIME ?? 0.0) - (sortedData[i].TIME ?? 0.0)));
                    if (Math.Abs(dt1) < 1e-12) dt1 = 1e-12;
                    if (Math.Abs(dt2) < 1e-12) dt2 = 1e-12;
                    double dp1 = (double)(((sortedData[i].PRESSURE ?? 0.0) - (sortedData[i - 1].PRESSURE ?? 0.0)));
                    double dp2 = (double)(((sortedData[i + 1].PRESSURE ?? 0.0) - (sortedData[i].PRESSURE ?? 0.0)));

                    double derivative1 = (dp1 / dt1) * (double)(sortedData[i].TIME ?? 0.0);
                    double derivative2 = (dp2 / dt2) * (double)(sortedData[i].TIME ?? 0.0);

                    // Weighted average for smoothing
                    double smoothed = smoothingFactor * derivative1 + (1 - smoothingFactor) * derivative2;
                    point.PRESSURE_DERIVATIVE = smoothed;
                }

                result.Add(point);
            }

            // Apply additional smoothing if needed
            if (smoothingFactor > 0)
            {
                result = ApplySmoothing(result, smoothingFactor);
            }

            return result;
        }

        /// <summary>
        /// Applies smoothing to derivative values.
        /// </summary>
        private static List<PRESSURE_TIME_POINT> ApplySmoothing(List<PRESSURE_TIME_POINT> data, double smoothingFactor)
        {
            if (data.Count < 3)
                return data;

            var smoothed = new List<PRESSURE_TIME_POINT>();

            for (int i = 0; i < data.Count; i++)
            {
                var point = new PRESSURE_TIME_POINT
                {
                    TIME = data[i].TIME,
                    PRESSURE = data[i].PRESSURE
                };

                if (i == 0)
                {
                    point.PRESSURE_DERIVATIVE = data[i].PRESSURE_DERIVATIVE;
                }
                else if (i == data.Count - 1)
                {
                    point.PRESSURE_DERIVATIVE = data[i].PRESSURE_DERIVATIVE;
                }
                else
                {
                    // Moving average smoothing
                    double avg = (data[i - 1].PRESSURE_DERIVATIVE ?? 0.0) * 0.25 +
                                (data[i].PRESSURE_DERIVATIVE ?? 0.0) * 0.5 +
                                (data[i + 1].PRESSURE_DERIVATIVE ?? 0.0) * 0.25;
                    point.PRESSURE_DERIVATIVE = avg;
                }

                smoothed.Add(point);
            }

            return smoothed;
        }

        /// <summary>
        /// Identifies reservoir model from derivative signature.
        /// </summary>
        public static ReservoirModel IdentifyModel(List<PRESSURE_TIME_POINT> derivativeData)
        {
            if (derivativeData == null || derivativeData.Count < 5)
                return ReservoirModel.InfiniteActing;

            var sorted = derivativeData.OrderBy(p => p.TIME).ToList();
            
            // Analyze derivative behavior
            // Early time: wellbore storage
            // Middle time: infinite acting (slope = 0)
            // Late time: boundary effects

            int middleStart = sorted.Count / 3;
            int middleEnd = sorted.Count * 2 / 3;

            // Check middle time region for infinite acting (constant derivative)
            var middleDerivatives = sorted.Skip(middleStart).Take(middleEnd - middleStart)
                .Where(p => p.PRESSURE_DERIVATIVE.HasValue)
                .Select(p => p.PRESSURE_DERIVATIVE.Value)
                .ToList();

            if (middleDerivatives.Count < 3)
                return ReservoirModel.InfiniteActing;

            double avgDerivative = (double)middleDerivatives.Average();
            double stdDev = Math.Sqrt(middleDerivatives.Sum(d => Math.Pow((double)d - avgDerivative, 2)) / middleDerivatives.Count);

            // If derivative is relatively constant, infinite acting
            if (stdDev / avgDerivative < 0.1)
            {
                // Check late time for boundaries
                int lateStart = sorted.Count * 2 / 3;
                var lateDerivatives = sorted.Skip(lateStart)
                    .Where(p => p.PRESSURE_DERIVATIVE.HasValue)
                    .Select(p => p.PRESSURE_DERIVATIVE.Value)
                    .ToList();

                if (lateDerivatives.Count >= 3)
                {
                    // Check if derivative is increasing (closed boundary) or decreasing (constant pressure)
                    double lateAvg = (double)lateDerivatives.Average();
                    double lateTrend = (double)lateDerivatives.Last() - (double)lateDerivatives.First();

                    if (lateTrend > avgDerivative * 0.2)
                        return ReservoirModel.ClosedBoundary;
                    else if (lateTrend < -avgDerivative * 0.2)
                        return ReservoirModel.ConstantPressureBoundary;
                }

                return ReservoirModel.InfiniteActing;
            }

            // Check for dual porosity signature (dip in derivative)
            var allDerivatives = sorted.Where(p => p.PRESSURE_DERIVATIVE.HasValue)
                .Select(p => p.PRESSURE_DERIVATIVE.Value)
                .ToList();

            if (allDerivatives.Count >= 5)
            {
                int minIndex = allDerivatives.IndexOf(allDerivatives.Min());
                if (minIndex > sorted.Count / 4 && minIndex < sorted.Count * 3 / 4)
                {
                    // Check if there's a recovery after the dip
                    if (minIndex < allDerivatives.Count - 2)
                    {
                        double recovery = (double)allDerivatives.Skip(minIndex + 1).Take(3).Average() - (double)allDerivatives[minIndex];
                        if (recovery > (double)allDerivatives[minIndex] * 0.3)
                            return ReservoirModel.DualPorosity;
                    }
                }
            }

            return ReservoirModel.InfiniteActing;
        }

        /// <summary>
        /// Identifies all flow regimes present in a pressure-transient test using the
        /// Bourdet derivative diagnostic plot approach. Each regime is returned as a
        /// <see cref="FlowRegimeIndicator"/> with start/end times, characteristic slope,
        /// goodness-of-fit, and a plain-English interpretation.
        ///
        /// Regimes identified (in temporal order):
        /// <list type="number">
        ///   <item>Wellbore Storage — unit slope on log-log (slope ≈ 1)</item>
        ///   <item>Transition — hump between storage and IARF</item>
        ///   <item>Radial Flow (IARF) — flat derivative (slope ≈ 0)</item>
        ///   <item>Linear Flow — half-slope on derivative (slope ≈ 0.5), e.g. hydraulic fracture</item>
        ///   <item>Bilinear Flow — quarter-slope on derivative (slope ≈ 0.25)</item>
        ///   <item>Pseudo-Steady State — unit slope on late-time derivative (closed boundary)</item>
        ///   <item>Steady State — derivative declines to zero (constant pressure boundary)</item>
        /// </list>
        /// Reference: Bourdet, D. et al. (1983) SPE-12777; Hurst &amp; van Everdingen (1949).
        /// </summary>
        /// <param name="derivativeData">
        ///   Bourdet pressure derivative data — as returned by
        ///   <see cref="CalculateDerivative"/>. TIME and PRESSURE_DERIVATIVE must be populated.
        /// </param>
        /// <returns>List of identified flow regimes, ordered by start time.</returns>
        public static List<FlowRegimeIndicator> IdentifyFlowRegimes(List<PRESSURE_TIME_POINT> derivativeData)
        {
            var regimes = new List<FlowRegimeIndicator>();

            if (derivativeData == null || derivativeData.Count < 5)
                return regimes;

            var sorted = derivativeData
                .Where(p => p.TIME > 0 && p.PRESSURE_DERIVATIVE.HasValue)
                .OrderBy(p => p.TIME)
                .ToList();

            if (sorted.Count < 5)
                return regimes;

            // Build log-log arrays for slope detection
            var logT = sorted.Select(p => Math.Log10((double)p.TIME)).ToArray();
            var logD = sorted.Select(p => Math.Log10(Math.Max(Math.Abs((double)p.PRESSURE_DERIVATIVE.Value), 1e-10))).ToArray();
            int n = logT.Length;

            int seq = 0;

            // ── 1. Detect unit-slope region (wellbore storage) ──────────────
            int storageEnd = FindRegimeEnd(logT, logD, targetSlope: 1.0, tolerance: 0.15, minPoints: 3);
            if (storageEnd > 0)
            {
                LinearRegression1D(logT, logD, 0, storageEnd, out double s1, out double i1);
                regimes.Add(new FlowRegimeIndicator
                {
                    Sequence = ++seq,
                    RegimeType = "Wellbore Storage",
                    StartTime = (double)sorted[0].TIME,
                    EndTime = (double)sorted[storageEnd].TIME,
                    Slope = s1,
                    Intercept = i1,
                    RSquared = CalcR2LogLog(logT, logD, 0, storageEnd, s1, i1),
                    ConfidenceLevel = "High",
                    Interpretation = "Unit-slope wellbore storage; well closed off from formation."
                });
            }

            // ── 2. Detect flat region (IARF) — slope ≈ 0 ───────────────────
            int iarfStart = storageEnd + 1;
            int iarfEnd = FindRegimeEnd(logT, logD, targetSlope: 0.0, tolerance: 0.1,
                minPoints: 3, searchStart: iarfStart);
            if (iarfEnd > iarfStart)
            {
                double avgDeriv = sorted.Skip(iarfStart).Take(iarfEnd - iarfStart + 1)
                    .Average(p => Math.Abs((double)p.PRESSURE_DERIVATIVE.Value));
                LinearRegression1D(logT, logD, iarfStart, iarfEnd, out double s2, out double i2);
                regimes.Add(new FlowRegimeIndicator
                {
                    Sequence = ++seq,
                    RegimeType = "Infinite Acting Radial Flow",
                    StartTime = (double)sorted[iarfStart].TIME,
                    EndTime = (double)sorted[iarfEnd].TIME,
                    Slope = s2,
                    Intercept = i2,
                    RSquared = CalcR2LogLog(logT, logD, iarfStart, iarfEnd, s2, i2),
                    ConfidenceLevel = Math.Abs(s2) < 0.05 ? "High" : "Medium",
                    Interpretation = $"Infinite Acting Radial Flow. Derivative ≈ {avgDeriv:F1} psi. " +
                                     "Use semi-log straight line to compute permeability and skin."
                });
            }

            // ── 3. Detect half-slope region (linear flow from fracture) ─────
            int linStart = iarfEnd + 1;
            int linEnd = FindRegimeEnd(logT, logD, targetSlope: 0.5, tolerance: 0.12,
                minPoints: 3, searchStart: linStart);
            if (linEnd > linStart)
            {
                LinearRegression1D(logT, logD, linStart, linEnd, out double s3, out double i3);
                regimes.Add(new FlowRegimeIndicator
                {
                    Sequence = ++seq,
                    RegimeType = "Linear Flow",
                    StartTime = (double)sorted[linStart].TIME,
                    EndTime = (double)sorted[linEnd].TIME,
                    Slope = s3,
                    Intercept = i3,
                    RSquared = CalcR2LogLog(logT, logD, linStart, linEnd, s3, i3),
                    ConfidenceLevel = "Medium",
                    Interpretation = "Half-slope linear flow — indicates a hydraulic fracture or linear boundary."
                });
            }

            // ── 4. Detect quarter-slope region (bilinear flow) ───────────────
            int bilStart = iarfEnd + 1;
            if (linEnd <= linStart) bilStart = iarfEnd + 1;  // only if no linear flow found
            int bilEnd = FindRegimeEnd(logT, logD, targetSlope: 0.25, tolerance: 0.08,
                minPoints: 3, searchStart: bilStart);
            if (bilEnd > bilStart)
            {
                LinearRegression1D(logT, logD, bilStart, bilEnd, out double s4, out double i4);
                regimes.Add(new FlowRegimeIndicator
                {
                    Sequence = ++seq,
                    RegimeType = "Bilinear Flow",
                    StartTime = (double)sorted[bilStart].TIME,
                    EndTime = (double)sorted[bilEnd].TIME,
                    Slope = s4,
                    Intercept = i4,
                    RSquared = CalcR2LogLog(logT, logD, bilStart, bilEnd, s4, i4),
                    ConfidenceLevel = "Medium",
                    Interpretation = "Quarter-slope bilinear flow — finite-conductivity fracture."
                });
            }

            // ── 5. Detect late unit-slope (pseudo-steady state) ──────────────
            int pssStart = Math.Max(iarfEnd + 1, n * 2 / 3);
            int pssEnd = FindRegimeEnd(logT, logD, targetSlope: 1.0, tolerance: 0.15,
                minPoints: 3, searchStart: pssStart);
            if (pssEnd > pssStart)
            {
                LinearRegression1D(logT, logD, pssStart, pssEnd, out double s5, out double i5);
                regimes.Add(new FlowRegimeIndicator
                {
                    Sequence = ++seq,
                    RegimeType = "Pseudo-Steady State",
                    StartTime = (double)sorted[pssStart].TIME,
                    EndTime = (double)sorted[pssEnd].TIME,
                    Slope = s5,
                    Intercept = i5,
                    RSquared = CalcR2LogLog(logT, logD, pssStart, pssEnd, s5, i5),
                    ConfidenceLevel = "High",
                    Interpretation = "Late unit-slope — closed boundary pseudo-steady state."
                });
            }
            else
            {
                // Check for steady state (derivative declining to zero)
                if (n >= 3)
                {
                    double lateTrend = logD[n - 1] - logD[n - 3];
                    if (lateTrend < -0.3)
                    {
                        regimes.Add(new FlowRegimeIndicator
                        {
                            Sequence = ++seq,
                            RegimeType = "Steady State",
                            StartTime = (double)sorted[n * 2 / 3].TIME,
                            EndTime = (double)sorted[n - 1].TIME,
                            Slope = -1.0,
                            ConfidenceLevel = "Low",
                            Interpretation = "Derivative declining — indicates constant pressure boundary (steady state)."
                        });
                    }
                }
            }

            return regimes.OrderBy(r => r.StartTime).ToList();
        }

        /// <summary>
        /// Calculates the Bourdet derivative using the recommended log-spaced smoothing
        /// (L-factor method). The Bourdet derivative is defined as:
        ///
        ///   p'(t) = t · dp/dln(t)
        ///
        /// L is the smoothing window length in log-time space. A value of L = 0.1–0.2
        /// is standard. Larger L gives more smoothing at the cost of resolution.
        /// Reference: Bourdet, D. et al. (1989) SPE Formation Evaluation.
        /// </summary>
        /// <param name="data">Pressure-time data (sorted or unsorted; sorted internally).</param>
        /// <param name="L">Log-time smoothing window (default 0.1).</param>
        /// <returns>List of <see cref="PRESSURE_TIME_POINT"/> with PRESSURE_DERIVATIVE set to t·dp/dlnt.</returns>
        public static List<PRESSURE_TIME_POINT> CalculateBourdetDerivative(
            List<PRESSURE_TIME_POINT> data, double L = 0.1)
        {
            if (data == null || data.Count < 3)
                return new List<PRESSURE_TIME_POINT>();

            var sorted = data
                .Where(p => p.TIME > 0)
                .OrderBy(p => p.TIME)
                .ToList();

            var result = new List<PRESSURE_TIME_POINT>();

            for (int i = 0; i < sorted.Count; i++)
            {
                double t0 = (double)sorted[i].TIME.Value;
                double p0 = (double)sorted[i].PRESSURE.Value;
                double lnT0 = Math.Log(t0);

                // Left window: find point where ln(t) ≤ ln(t0) - L
                int iLeft = i - 1;
                while (iLeft > 0 && Math.Log((double)sorted[iLeft].TIME.Value) > lnT0 - L)
                    iLeft--;

                // Right window: find point where ln(t) ≥ ln(t0) + L
                int iRight = i + 1;
                while (iRight < sorted.Count - 1 && Math.Log((double)sorted[iRight].TIME.Value) < lnT0 + L)
                    iRight++;

                double deriv;
                bool hasLeft = iLeft >= 0 && iLeft < i;
                bool hasRight = iRight > i && iRight < sorted.Count;

                if (hasLeft && hasRight)
                {
                    double tL = (double)sorted[iLeft].TIME.Value;
                    double pL = (double)sorted[iLeft].PRESSURE.Value;
                    double tR = (double)sorted[iRight].TIME.Value;
                    double pR = (double)sorted[iRight].PRESSURE.Value;

                    double lnTL = Math.Log(tL);
                    double lnTR = Math.Log(tR);

                    // Weighted Bourdet: interpolate left and right differences
                    double dLeft = (p0 - pL) / (lnT0 - lnTL);
                    double dRight = (pR - p0) / (lnTR - lnT0);

                    double wLeft = lnTR - lnT0;
                    double wRight = lnT0 - lnTL;
                    double wTotal = lnTR - lnTL;
                    deriv = wTotal > 1e-12
                        ? (dLeft * wLeft + dRight * wRight) / wTotal
                        : (dLeft + dRight) / 2.0;
                }
                else if (hasLeft)
                {
                    double tL = (double)sorted[iLeft].TIME.Value;
                    double pL = (double)sorted[iLeft].PRESSURE.Value;
                    double lnTL = Math.Log(tL);
                    deriv = Math.Abs(lnT0 - lnTL) > 1e-12
                        ? (p0 - pL) / (lnT0 - lnTL)
                        : 0;
                }
                else if (hasRight)
                {
                    double tR = (double)sorted[iRight].TIME.Value;
                    double pR = (double)sorted[iRight].PRESSURE.Value;
                    double lnTR = Math.Log(tR);
                    deriv = Math.Abs(lnTR - lnT0) > 1e-12
                        ? (pR - p0) / (lnTR - lnT0)
                        : 0;
                }
                else
                {
                    deriv = 0;
                }

                result.Add(new PRESSURE_TIME_POINT
                {
                    TIME = sorted[i].TIME,
                    PRESSURE = sorted[i].PRESSURE,
                    PRESSURE_DERIVATIVE = deriv
                });
            }

            return result;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Finds the end index of a regime segment characterised by <paramref name="targetSlope"/>
        /// on the log-log derivative plot.  Returns -1 if no qualifying segment found.
        /// </summary>
        private static int FindRegimeEnd(
            double[] logT, double[] logD,
            double targetSlope, double tolerance,
            int minPoints, int searchStart = 0)
        {
            int regimeEnd = -1;
            int consecutiveHits = 0;

            for (int i = Math.Max(1, searchStart); i < logT.Length; i++)
            {
                double dt = logT[i] - logT[i - 1];
                if (Math.Abs(dt) < 1e-10) continue;
                double localSlope = (logD[i] - logD[i - 1]) / dt;

                if (Math.Abs(localSlope - targetSlope) <= tolerance)
                {
                    consecutiveHits++;
                    if (consecutiveHits >= minPoints)
                        regimeEnd = i;
                }
                else
                {
                    if (consecutiveHits >= minPoints) break;  // regime ended
                    consecutiveHits = 0;
                }
            }

            return regimeEnd;
        }

        private static void LinearRegression1D(
            double[] x, double[] y, int start, int end,
            out double slope, out double intercept)
        {
            int n = end - start + 1;
            if (n < 2) { slope = 0; intercept = y[start]; return; }

            double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
            for (int i = start; i <= end; i++)
            {
                sumX += x[i]; sumY += y[i];
                sumXY += x[i] * y[i]; sumX2 += x[i] * x[i];
            }

            double denom = n * sumX2 - sumX * sumX;
            if (Math.Abs(denom) < 1e-12) { slope = 0; intercept = sumY / n; return; }

            slope = (n * sumXY - sumX * sumY) / denom;
            intercept = (sumY - slope * sumX) / n;
        }

        private static double CalcR2LogLog(
            double[] logT, double[] logD, int start, int end,
            double slope, double intercept)
        {
            int n = end - start + 1;
            if (n < 2) return 0;
            double yMean = 0;
            for (int i = start; i <= end; i++) yMean += logD[i];
            yMean /= n;

            double ssTotal = 0, ssRes = 0;
            for (int i = start; i <= end; i++)
            {
                ssTotal += Math.Pow(logD[i] - yMean, 2);
                ssRes += Math.Pow(logD[i] - (slope * logT[i] + intercept), 2);
            }

            return ssTotal < 1e-12 ? 1.0 : 1.0 - ssRes / ssTotal;
        }

        private static int FindMiddleTimeRegion(double[] logTime, double[] pressures)
        {
            return 0;  // placeholder; actual MDH logic is in BuildUpAnalysis
        }
    }
}

