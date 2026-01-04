using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.WellTestAnalysis.Constants;
using Beep.OilandGas.Models.WellTestAnalysis;

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
        public static List<PressureTimePoint> CalculateDerivative(List<PressureTimePoint> data, double smoothingFactor = WellTestConstants.DefaultDerivativeSmoothing)
        {
            if (data == null || data.Count < 3)
                return new List<PressureTimePoint>();

            var result = new List<PressureTimePoint>();
            var sortedData = data.OrderBy(p => p.Time).ToList();

            for (int i = 0; i < sortedData.Count; i++)
            {
                var point = new PressureTimePoint
                {
                    Time = sortedData[i].Time,
                    Pressure = sortedData[i].Pressure
                };

                if (i == 0 || i == sortedData.Count - 1)
                {
                    // End points: use forward/backward difference
                    if (i == 0)
                    {
                        double dt = sortedData[i + 1].Time - sortedData[i].Time;
                        double dp = sortedData[i + 1].Pressure - sortedData[i].Pressure;
                        point.PressureDerivative = (dp / dt) * sortedData[i].Time;
                    }
                    else
                    {
                        double dt = sortedData[i].Time - sortedData[i - 1].Time;
                        double dp = sortedData[i].Pressure - sortedData[i - 1].Pressure;
                        point.PressureDerivative = (dp / dt) * sortedData[i].Time;
                    }
                }
                else
                {
                    // Central difference with smoothing
                    double dt1 = sortedData[i].Time - sortedData[i - 1].Time;
                    double dt2 = sortedData[i + 1].Time - sortedData[i].Time;
                    double dp1 = sortedData[i].Pressure - sortedData[i - 1].Pressure;
                    double dp2 = sortedData[i + 1].Pressure - sortedData[i].Pressure;

                    double derivative1 = (dp1 / dt1) * sortedData[i].Time;
                    double derivative2 = (dp2 / dt2) * sortedData[i].Time;

                    // Weighted average for smoothing
                    point.PressureDerivative = smoothingFactor * derivative1 + (1 - smoothingFactor) * derivative2;
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
        private static List<PressureTimePoint> ApplySmoothing(List<PressureTimePoint> data, double smoothingFactor)
        {
            if (data.Count < 3)
                return data;

            var smoothed = new List<PressureTimePoint>();

            for (int i = 0; i < data.Count; i++)
            {
                var point = new PressureTimePoint
                {
                    Time = data[i].Time,
                    Pressure = data[i].Pressure
                };

                if (i == 0)
                {
                    point.PressureDerivative = data[i].PressureDerivative;
                }
                else if (i == data.Count - 1)
                {
                    point.PressureDerivative = data[i].PressureDerivative;
                }
                else
                {
                    // Moving average smoothing
                    double avg = (data[i - 1].PressureDerivative ?? 0) * 0.25 +
                                (data[i].PressureDerivative ?? 0) * 0.5 +
                                (data[i + 1].PressureDerivative ?? 0) * 0.25;
                    point.PressureDerivative = avg;
                }

                smoothed.Add(point);
            }

            return smoothed;
        }

        /// <summary>
        /// Identifies reservoir model from derivative signature.
        /// </summary>
        public static ReservoirModel IdentifyModel(List<PressureTimePoint> derivativeData)
        {
            if (derivativeData == null || derivativeData.Count < 5)
                return ReservoirModel.InfiniteActing;

            var sorted = derivativeData.OrderBy(p => p.Time).ToList();
            
            // Analyze derivative behavior
            // Early time: wellbore storage
            // Middle time: infinite acting (slope = 0)
            // Late time: boundary effects

            int middleStart = sorted.Count / 3;
            int middleEnd = sorted.Count * 2 / 3;

            // Check middle time region for infinite acting (constant derivative)
            var middleDerivatives = sorted.Skip(middleStart).Take(middleEnd - middleStart)
                .Where(p => p.PressureDerivative.HasValue)
                .Select(p => p.PressureDerivative.Value)
                .ToList();

            if (middleDerivatives.Count < 3)
                return ReservoirModel.InfiniteActing;

            double avgDerivative = middleDerivatives.Average();
            double stdDev = Math.Sqrt(middleDerivatives.Sum(d => Math.Pow(d - avgDerivative, 2)) / middleDerivatives.Count);

            // If derivative is relatively constant, infinite acting
            if (stdDev / avgDerivative < 0.1)
            {
                // Check late time for boundaries
                int lateStart = sorted.Count * 2 / 3;
                var lateDerivatives = sorted.Skip(lateStart)
                    .Where(p => p.PressureDerivative.HasValue)
                    .Select(p => p.PressureDerivative.Value)
                    .ToList();

                if (lateDerivatives.Count >= 3)
                {
                    // Check if derivative is increasing (closed boundary) or decreasing (constant pressure)
                    double lateAvg = lateDerivatives.Average();
                    double lateTrend = lateDerivatives.Last() - lateDerivatives.First();

                    if (lateTrend > avgDerivative * 0.2)
                        return ReservoirModel.ClosedBoundary;
                    else if (lateTrend < -avgDerivative * 0.2)
                        return ReservoirModel.ConstantPressureBoundary;
                }

                return ReservoirModel.InfiniteActing;
            }

            // Check for dual porosity signature (dip in derivative)
            var allDerivatives = sorted.Where(p => p.PressureDerivative.HasValue)
                .Select(p => p.PressureDerivative.Value)
                .ToList();

            if (allDerivatives.Count >= 5)
            {
                int minIndex = allDerivatives.IndexOf(allDerivatives.Min());
                if (minIndex > sorted.Count / 4 && minIndex < sorted.Count * 3 / 4)
                {
                    // Check if there's a recovery after the dip
                    if (minIndex < allDerivatives.Count - 2)
                    {
                        double recovery = allDerivatives.Skip(minIndex + 1).Take(3).Average() - allDerivatives[minIndex];
                        if (recovery > allDerivatives[minIndex] * 0.3)
                            return ReservoirModel.DualPorosity;
                    }
                }
            }

            return ReservoirModel.InfiniteActing;
        }
    }
}

