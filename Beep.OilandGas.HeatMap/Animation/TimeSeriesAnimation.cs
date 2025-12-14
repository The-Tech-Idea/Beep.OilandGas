using System;
using System.Collections.Generic;
using System.Linq;

namespace Beep.OilandGas.HeatMap.Animation
{
    /// <summary>
    /// Represents a time-stamped data point for animation.
    /// </summary>
    public class TimeSeriesDataPoint : HeatMapDataPoint
    {
        /// <summary>
        /// Gets or sets the timestamp for this data point.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the time index (0-based) for animation.
        /// </summary>
        public int TimeIndex { get; set; }

        public TimeSeriesDataPoint() : base()
        {
        }

        public TimeSeriesDataPoint(double x, double y, double value, string label, DateTime timestamp)
            : base(x, y, value, label)
        {
            Timestamp = timestamp;
        }
    }

    /// <summary>
    /// Manages time-series animation for heat maps.
    /// </summary>
    public class TimeSeriesAnimation
    {
        /// <summary>
        /// Gets the time-stamped data points organized by time index.
        /// </summary>
        public Dictionary<int, List<TimeSeriesDataPoint>> TimeFrames { get; }

        /// <summary>
        /// Gets or sets the current time index.
        /// </summary>
        public int CurrentTimeIndex { get; set; }

        /// <summary>
        /// Gets the minimum time index.
        /// </summary>
        public int MinTimeIndex => TimeFrames.Keys.Any() ? TimeFrames.Keys.Min() : 0;

        /// <summary>
        /// Gets the maximum time index.
        /// </summary>
        public int MaxTimeIndex => TimeFrames.Keys.Any() ? TimeFrames.Keys.Max() : 0;

        /// <summary>
        /// Gets or sets whether the animation is playing.
        /// </summary>
        public bool IsPlaying { get; set; }

        /// <summary>
        /// Gets or sets the animation speed (frames per second).
        /// </summary>
        public double AnimationSpeed { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets whether to loop the animation.
        /// </summary>
        public bool Loop { get; set; } = true;

        /// <summary>
        /// Gets or sets the interpolation mode for transitions.
        /// </summary>
        public InterpolationMode InterpolationMode { get; set; } = InterpolationMode.Linear;

        /// <summary>
        /// Event raised when the current time index changes.
        /// </summary>
        public event EventHandler<int> TimeIndexChanged;

        /// <summary>
        /// Event raised when animation starts.
        /// </summary>
        public event EventHandler AnimationStarted;

        /// <summary>
        /// Event raised when animation stops.
        /// </summary>
        public event EventHandler AnimationStopped;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSeriesAnimation"/> class.
        /// </summary>
        public TimeSeriesAnimation()
        {
            TimeFrames = new Dictionary<int, List<TimeSeriesDataPoint>>();
            CurrentTimeIndex = 0;
        }

        /// <summary>
        /// Adds a time-stamped data point to the animation.
        /// </summary>
        /// <param name="point">The time-stamped data point.</param>
        public void AddDataPoint(TimeSeriesDataPoint point)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            if (!TimeFrames.ContainsKey(point.TimeIndex))
            {
                TimeFrames[point.TimeIndex] = new List<TimeSeriesDataPoint>();
            }

            TimeFrames[point.TimeIndex].Add(point);
        }

        /// <summary>
        /// Gets the data points for the current time index.
        /// </summary>
        /// <returns>List of data points for the current time.</returns>
        public List<HeatMapDataPoint> GetCurrentFrame()
        {
            if (TimeFrames.ContainsKey(CurrentTimeIndex))
            {
                return TimeFrames[CurrentTimeIndex].Cast<HeatMapDataPoint>().ToList();
            }

            return new List<HeatMapDataPoint>();
        }

        /// <summary>
        /// Gets interpolated data points between two time frames.
        /// </summary>
        /// <param name="timeIndex1">First time index.</param>
        /// <param name="timeIndex2">Second time index.</param>
        /// <param name="t">Interpolation factor (0.0 to 1.0).</param>
        /// <returns>List of interpolated data points.</returns>
        public List<HeatMapDataPoint> GetInterpolatedFrame(int timeIndex1, int timeIndex2, double t)
        {
            t = Math.Max(0.0, Math.Min(1.0, t)); // Clamp to [0, 1]

            var frame1 = TimeFrames.ContainsKey(timeIndex1) 
                ? TimeFrames[timeIndex1] 
                : new List<TimeSeriesDataPoint>();
            var frame2 = TimeFrames.ContainsKey(timeIndex2) 
                ? TimeFrames[timeIndex2] 
                : new List<TimeSeriesDataPoint>();

            var interpolated = new List<HeatMapDataPoint>();

            // Create a map of points by their base coordinates
            var pointMap = new Dictionary<(double, double), (TimeSeriesDataPoint p1, TimeSeriesDataPoint p2)>();

            foreach (var p1 in frame1)
            {
                var key = (p1.X, p1.Y);
                if (!pointMap.ContainsKey(key))
                {
                    pointMap[key] = (p1, null);
                }
            }

            foreach (var p2 in frame2)
            {
                var key = (p2.X, p2.Y);
                if (pointMap.ContainsKey(key))
                {
                    var existing = pointMap[key];
                    pointMap[key] = (existing.p1, p2);
                }
                else
                {
                    pointMap[key] = (null, p2);
                }
            }

            // Interpolate values
            foreach (var kvp in pointMap)
            {
                var (p1, p2) = kvp.Value;
                double interpolatedValue;

                if (p1 != null && p2 != null)
                {
                    // Interpolate between both points
                    interpolatedValue = InterpolateValue(p1.Value, p2.Value, t);
                }
                else if (p1 != null)
                {
                    // Only point 1 exists (fade out)
                    interpolatedValue = p1.Value * (1 - t);
                }
                else if (p2 != null)
                {
                    // Only point 2 exists (fade in)
                    interpolatedValue = p2.Value * t;
                }
                else
                {
                    continue;
                }

                interpolated.Add(new HeatMapDataPoint
                {
                    X = kvp.Key.Item1,
                    Y = kvp.Key.Item2,
                    Value = interpolatedValue,
                    Label = p1?.Label ?? p2?.Label
                });
            }

            return interpolated;
        }

        /// <summary>
        /// Advances to the next time frame.
        /// </summary>
        /// <returns>True if advanced, false if at the end.</returns>
        public bool NextFrame()
        {
            if (CurrentTimeIndex < MaxTimeIndex)
            {
                CurrentTimeIndex++;
                TimeIndexChanged?.Invoke(this, CurrentTimeIndex);
                return true;
            }
            else if (Loop)
            {
                CurrentTimeIndex = MinTimeIndex;
                TimeIndexChanged?.Invoke(this, CurrentTimeIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Goes to the previous time frame.
        /// </summary>
        /// <returns>True if moved back, false if at the beginning.</returns>
        public bool PreviousFrame()
        {
            if (CurrentTimeIndex > MinTimeIndex)
            {
                CurrentTimeIndex--;
                TimeIndexChanged?.Invoke(this, CurrentTimeIndex);
                return true;
            }
            else if (Loop)
            {
                CurrentTimeIndex = MaxTimeIndex;
                TimeIndexChanged?.Invoke(this, CurrentTimeIndex);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Jumps to a specific time index.
        /// </summary>
        /// <param name="timeIndex">The time index to jump to.</param>
        public void GoToFrame(int timeIndex)
        {
            CurrentTimeIndex = Math.Max(MinTimeIndex, Math.Min(MaxTimeIndex, timeIndex));
            TimeIndexChanged?.Invoke(this, CurrentTimeIndex);
        }

        /// <summary>
        /// Starts the animation.
        /// </summary>
        public void Play()
        {
            IsPlaying = true;
            AnimationStarted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        public void Stop()
        {
            IsPlaying = false;
            AnimationStopped?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        public void Pause()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// Resets the animation to the first frame.
        /// </summary>
        public void Reset()
        {
            CurrentTimeIndex = MinTimeIndex;
            IsPlaying = false;
            TimeIndexChanged?.Invoke(this, CurrentTimeIndex);
        }

        /// <summary>
        /// Interpolates a value between two values based on the interpolation mode.
        /// </summary>
        private double InterpolateValue(double value1, double value2, double t)
        {
            return InterpolationMode switch
            {
                InterpolationMode.Linear => value1 + (value2 - value1) * t,
                InterpolationMode.EaseIn => value1 + (value2 - value1) * (t * t),
                InterpolationMode.EaseOut => value1 + (value2 - value1) * (1 - Math.Pow(1 - t, 2)),
                InterpolationMode.EaseInOut => value1 + (value2 - value1) * (t < 0.5 
                    ? 2 * t * t 
                    : 1 - Math.Pow(-2 * t + 2, 2) / 2),
                _ => value1 + (value2 - value1) * t
            };
        }
    }

    /// <summary>
    /// Enumeration of interpolation modes for animation transitions.
    /// </summary>
    public enum InterpolationMode
    {
        /// <summary>
        /// Linear interpolation.
        /// </summary>
        Linear,

        /// <summary>
        /// Ease-in interpolation (slow start).
        /// </summary>
        EaseIn,

        /// <summary>
        /// Ease-out interpolation (slow end).
        /// </summary>
        EaseOut,

        /// <summary>
        /// Ease-in-out interpolation (slow start and end).
        /// </summary>
        EaseInOut
    }
}

