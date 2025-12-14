using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.DCA.Constants;
using Beep.OilandGas.DCA.Exceptions;
using Beep.OilandGas.DCA.Validation;

namespace Beep.OilandGas.DCA.MultiSegment
{
    /// <summary>
    /// Represents a single segment in a multi-segment decline curve.
    /// </summary>
    public class DeclineSegment
    {
        /// <summary>
        /// Gets or sets the start time of this segment.
        /// </summary>
        public double StartTime { get; set; }

        /// <summary>
        /// Gets or sets the end time of this segment.
        /// </summary>
        public double EndTime { get; set; }

        /// <summary>
        /// Gets or sets the initial production rate at the start of this segment.
        /// </summary>
        public double InitialProductionRate { get; set; }

        /// <summary>
        /// Gets or sets the decline rate for this segment.
        /// </summary>
        public double DeclineRate { get; set; }

        /// <summary>
        /// Gets or sets the decline exponent (b) for hyperbolic decline, or 0 for exponential, or 1 for harmonic.
        /// </summary>
        public double DeclineExponent { get; set; }

        /// <summary>
        /// Gets or sets the decline type for this segment.
        /// </summary>
        public DeclineType DeclineType { get; set; }
    }

    /// <summary>
    /// Enumeration of decline curve types.
    /// </summary>
    public enum DeclineType
    {
        /// <summary>
        /// Exponential decline (b = 0).
        /// </summary>
        Exponential,

        /// <summary>
        /// Harmonic decline (b = 1).
        /// </summary>
        Harmonic,

        /// <summary>
        /// Hyperbolic decline (0 &lt; b &lt; 1).
        /// </summary>
        Hyperbolic
    }

    /// <summary>
    /// Provides methods for multi-segment decline curve analysis.
    /// Allows modeling production with different decline characteristics in different time periods.
    /// </summary>
    public static class MultiSegmentDecline
    {
        /// <summary>
        /// Calculates production rate at a given time using multi-segment decline.
        /// </summary>
        /// <param name="segments">List of decline segments in chronological order.</param>
        /// <param name="t">Time since start of production.</param>
        /// <returns>Production rate at time t.</returns>
        /// <exception cref="ArgumentNullException">Thrown when segments is null.</exception>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when segments are invalid.</exception>
        public static double CalculateProductionRate(List<DeclineSegment> segments, double t)
        {
            if (segments == null)
                throw new ArgumentNullException(nameof(segments));

            if (segments.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one segment is required.");
            }

            if (t < 0)
            {
                throw new Exceptions.InvalidDataException($"Time must be non-negative. Provided: {t}.");
            }

            // Find the segment that contains time t
            DeclineSegment activeSegment = null;
            double segmentStartTime = 0.0;

            foreach (var segment in segments.OrderBy(s => s.StartTime))
            {
                if (t >= segment.StartTime && t <= segment.EndTime)
                {
                    activeSegment = segment;
                    segmentStartTime = segment.StartTime;
                    break;
                }
            }

            if (activeSegment == null)
            {
                // Time is beyond all segments, use the last segment
                activeSegment = segments.OrderBy(s => s.EndTime).Last();
                segmentStartTime = activeSegment.StartTime;
            }

            // Calculate production rate using the appropriate decline method
            double relativeTime = t - segmentStartTime;
            double qi = activeSegment.InitialProductionRate;
            double di = activeSegment.DeclineRate;
            double b = activeSegment.DeclineExponent;

            switch (activeSegment.DeclineType)
            {
                case DeclineType.Exponential:
                    return DCAGenerator.ExponentialDecline(qi, di, relativeTime);

                case DeclineType.Harmonic:
                    return DCAGenerator.HarmonicDecline(qi, di, relativeTime);

                case DeclineType.Hyperbolic:
                    return DCAGenerator.HyperbolicDecline(qi, di, relativeTime, b);

                default:
                    throw new Exceptions.InvalidDataException($"Unknown decline type: {activeSegment.DeclineType}");
            }
        }

        /// <summary>
        /// Validates that segments are properly ordered and non-overlapping.
        /// </summary>
        /// <param name="segments">List of segments to validate.</param>
        /// <exception cref="Exceptions.InvalidDataException">Thrown when segments are invalid.</exception>
        public static void ValidateSegments(List<DeclineSegment> segments)
        {
            if (segments == null || segments.Count == 0)
            {
                throw new Exceptions.InvalidDataException("At least one segment is required.");
            }

            var orderedSegments = segments.OrderBy(s => s.StartTime).ToList();

            for (int i = 0; i < orderedSegments.Count; i++)
            {
                var segment = orderedSegments[i];

                if (segment.StartTime < 0)
                {
                    throw new Exceptions.InvalidDataException(
                        $"Segment {i} has negative start time: {segment.StartTime}.");
                }

                if (segment.EndTime <= segment.StartTime)
                {
                    throw new Exceptions.InvalidDataException(
                        $"Segment {i} has invalid time range: start={segment.StartTime}, end={segment.EndTime}.");
                }

                if (segment.InitialProductionRate <= 0)
                {
                    throw new Exceptions.InvalidDataException(
                        $"Segment {i} has invalid initial production rate: {segment.InitialProductionRate}.");
                }

                if (segment.DeclineRate <= 0)
                {
                    throw new Exceptions.InvalidDataException(
                        $"Segment {i} has invalid decline rate: {segment.DeclineRate}.");
                }

                // Check for overlaps with next segment
                if (i < orderedSegments.Count - 1)
                {
                    var nextSegment = orderedSegments[i + 1];
                    if (segment.EndTime > nextSegment.StartTime)
                    {
                        throw new Exceptions.InvalidDataException(
                            $"Segment {i} overlaps with segment {i + 1}.");
                    }
                }
            }
        }
    }
}

