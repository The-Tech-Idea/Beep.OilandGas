using System.Collections.Generic;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Defines the semantic role of a log track.
    /// </summary>
    public enum LogTrackKind
    {
        Curve,
        Depth,
        Lithology,
        Zonation
    }

    /// <summary>
    /// Defines the scaling behavior for a log track curve.
    /// </summary>
    public enum LogTrackScaleType
    {
        Linear,
        Logarithmic
    }

    /// <summary>
    /// Defines a log track and the curves rendered inside it.
    /// </summary>
    public sealed class LogTrackDefinition
    {
        /// <summary>
        /// Gets or sets the kind of track.
        /// </summary>
        public LogTrackKind Kind { get; set; } = LogTrackKind.Curve;

        /// <summary>
        /// Gets or sets the display name for the track header.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the width of the track. If null, the renderer default is used.
        /// </summary>
        public float? Width { get; set; }

        /// <summary>
        /// Gets or sets the optional background color for the track.
        /// </summary>
        public SKColor? BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the major interval used for depth labels and ticks.
        /// </summary>
        public double? MajorInterval { get; set; }

        /// <summary>
        /// Gets or sets the number of minor subdivisions between major ticks.
        /// </summary>
        public int MinorSubdivisionCount { get; set; } = 4;

        /// <summary>
        /// Gets or sets the depth label format string.
        /// </summary>
        public string LabelFormat { get; set; } = "F0";

        /// <summary>
        /// Gets or sets the curves rendered in the track.
        /// </summary>
        public List<LogTrackCurveDefinition> Curves { get; set; } = new List<LogTrackCurveDefinition>();

        /// <summary>
        /// Gets or sets the depth intervals rendered in the track.
        /// </summary>
        public List<LogIntervalData> Intervals { get; set; } = new List<LogIntervalData>();
    }

    /// <summary>
    /// Defines how a single curve is rendered inside a log track.
    /// </summary>
    public sealed class LogTrackCurveDefinition
    {
        /// <summary>
        /// Gets or sets the source curve mnemonic or name.
        /// </summary>
        public string CurveName { get; set; }

        /// <summary>
        /// Gets or sets the optional display label used in headers or legends.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the optional curve color override.
        /// </summary>
        public SKColor? Color { get; set; }

        /// <summary>
        /// Gets or sets the optional minimum scale value.
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// Gets or sets the optional maximum scale value.
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the optional scale type override.
        /// </summary>
        public LogTrackScaleType? ScaleType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the displayed scale should run in reverse.
        /// </summary>
        public bool InvertScale { get; set; }

        /// <summary>
        /// Gets or sets the numeric format used for scale annotations.
        /// </summary>
        public string ValueFormat { get; set; }
    }
}