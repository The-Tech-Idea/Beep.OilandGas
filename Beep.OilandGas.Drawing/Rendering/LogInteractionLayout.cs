using System.Collections.Generic;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Rendering
{
    internal sealed record LogTrackInteractionLayout(
        LogTrackDefinition Track,
        SKRect Bounds,
        SKRect BodyBounds,
        IReadOnlyList<LogCurveInteractionLayout> Curves,
        IReadOnlyList<LogCrossoverInteractionLayout> Crossovers,
        IReadOnlyList<LogIntervalInteractionLayout> Intervals);

    internal sealed record LogCurveInteractionLayout(
        LogTrackCurveDefinition Definition,
        LogCurveMetadata Metadata,
        SKColor Color,
        IReadOnlyList<SKPoint> Points);

    internal sealed record LogCrossoverInteractionLayout(
        LogTrackCurveDefinition DensityCurve,
        LogCurveMetadata DensityMetadata,
        LogTrackCurveDefinition NeutronCurve,
        LogCurveMetadata NeutronMetadata,
        SKRect Bounds,
        IReadOnlyList<SKPoint> Polygon,
        double TopDepth,
        double BottomDepth);

    internal sealed record LogIntervalInteractionLayout(
        LogIntervalData Interval,
        SKRect Bounds,
        string Label);
}