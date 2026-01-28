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
    }
}

