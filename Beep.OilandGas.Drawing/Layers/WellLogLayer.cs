using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Rendering;

namespace Beep.OilandGas.Drawing.Layers
{
    /// <summary>
    /// Layer that renders petrophysical well logs using <see cref="LogRenderer"/>.
    /// World Y axis = depth (feet or metres); world X axis matches pixel X 1:1 with the rendered log width.
    /// </summary>
    public sealed class WellLogLayer : LayerBase, IInteractiveLayer
    {
        private readonly LogData _logData;
        private readonly DeviationSurvey _deviationSurvey;
        private readonly LogRendererConfiguration _configuration;
        private readonly LogRenderer _renderer;

        private float _lastCanvasWidth = 600f;
        private float _lastCanvasHeight = 1200f;

        /// <summary>
        /// Initializes a new instance of <see cref="WellLogLayer"/>.
        /// </summary>
        public WellLogLayer(
            LogData logData,
            DeviationSurvey deviationSurvey,
            LogRendererConfiguration configuration)
            : base("Well Log")
        {
            _logData = logData ?? throw new ArgumentNullException(nameof(logData));
            _deviationSurvey = deviationSurvey;
            _configuration = configuration ?? new LogRendererConfiguration();
            _renderer = new LogRenderer(_logData, _deviationSurvey, _configuration);
        }

        /// <inheritdoc/>
        public override SKRect GetBounds()
        {
            float totalWidth = ComputeTotalWidth();
            return new SKRect(0f, (float)_logData.StartDepth, totalWidth, (float)_logData.EndDepth);
        }

        /// <inheritdoc/>
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            var clip = canvas.DeviceClipBounds;
            _lastCanvasWidth = clip.Width;
            _lastCanvasHeight = clip.Height;

            // LogRenderer renders directly in pixel space — do NOT apply the viewport matrix.
            _renderer.Render(canvas, _lastCanvasWidth, _lastCanvasHeight);
        }

        /// <inheritdoc/>
        public LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance)
        {
            var layouts = _renderer.GetInteractionLayouts(_lastCanvasHeight);
            if (layouts == null || layouts.Count == 0)
                return null;

            float headerHeight = _configuration.ShowTrackHeaders ? _configuration.TrackHeaderHeight : 0f;
            float trackBodyY = headerHeight;
            float trackBodyHeight = MathF.Max(1f, _lastCanvasHeight - headerHeight);
            float depthRange = (float)(_logData.EndDepth - _logData.StartDepth);

            if (depthRange <= 0f)
                return null;

            // Convert world point (Y = depth) to pixel space.
            var depthTransform = new DepthTransform(_logData.StartDepth, _logData.EndDepth, trackBodyHeight);
            float pixelX = worldPoint.X;
            float pixelY = trackBodyY + depthTransform.ToScreenY(worldPoint.Y);
            var pixelPoint = new SKPoint(pixelX, pixelY);

            // Pixel tolerance scaled from world (depth-unit) tolerance.
            float pixelTolerance = MathF.Max(1f, worldTolerance * trackBodyHeight / depthRange);

            foreach (var layout in layouts)
            {
                if (!layout.BodyBounds.Contains(pixelPoint))
                    continue;

                // 1. Crossovers (highest priority — must be inside crossover polygon bounds).
                foreach (var crossover in layout.Crossovers)
                {
                    if (crossover.Bounds.Contains(pixelPoint))
                        return BuildCrossoverHit(crossover, worldPoint);
                }

                // 2. Curve polyline proximity.
                var curveHit = FindClosestCurveHit(layout.Curves, pixelPoint, pixelTolerance, worldPoint, depthRange, trackBodyHeight);
                if (curveHit != null)
                    return curveHit;

                // 3. Interval area hit.
                foreach (var interval in layout.Intervals)
                {
                    if (interval.Bounds.Contains(pixelPoint))
                        return BuildIntervalHit(interval, worldPoint);
                }

                // 4. Track area fallback.
                return BuildTrackAreaHit(layout, worldPoint, depthTransform, depthRange);
            }

            return null;
        }

        // ── Private helpers ──────────────────────────────────────────────────────

        private LayerHitResult BuildCrossoverHit(LogCrossoverInteractionLayout crossover, SKPoint worldAnchor)
        {
            string densityName = crossover.DensityMetadata?.DisplayName ?? crossover.DensityCurve?.CurveName ?? "Density";
            string neutronName = crossover.NeutronMetadata?.DisplayName ?? crossover.NeutronCurve?.CurveName ?? "Neutron";

            return new LayerHitResult(
                layerName: Name,
                featureId: $"crossover:{crossover.DensityCurve?.CurveName}:{crossover.NeutronCurve?.CurveName}",
                featureLabel: $"{densityName} / {neutronName}",
                featureKind: "Density-Neutron Crossover",
                worldAnchor: worldAnchor,
                distance: 0f,
                metadata: new Dictionary<string, string>
                {
                    ["DensityCurveName"] = crossover.DensityCurve?.CurveName ?? string.Empty,
                    ["NeutronCurveName"] = crossover.NeutronCurve?.CurveName ?? string.Empty,
                    ["TopDepth"] = crossover.TopDepth.ToString("G6", CultureInfo.InvariantCulture),
                    ["BottomDepth"] = crossover.BottomDepth.ToString("G6", CultureInfo.InvariantCulture)
                });
        }

        private LayerHitResult FindClosestCurveHit(
            IReadOnlyList<LogCurveInteractionLayout> curves,
            SKPoint pixelPoint,
            float pixelTolerance,
            SKPoint worldAnchor,
            float depthRange,
            float trackBodyHeight)
        {
            LayerHitResult best = null;

            foreach (var curve in curves)
            {
                if (curve.Points == null || curve.Points.Count == 0)
                    continue;

                float minDist = float.MaxValue;
                int closestIndex = -1;
                for (int i = 0; i < curve.Points.Count; i++)
                {
                    float d = SKPoint.Distance(pixelPoint, curve.Points[i]);
                    if (d < minDist)
                    {
                        minDist = d;
                        closestIndex = i;
                    }
                }

                if (minDist > pixelTolerance || closestIndex < 0)
                    continue;

                string curveName = curve.Definition?.CurveName ?? string.Empty;
                string displayName = curve.Metadata?.DisplayName ?? curveName;

                string valueStr = string.Empty;
                if (!string.IsNullOrEmpty(curveName) &&
                    _logData.Curves.TryGetValue(curveName, out var vals) &&
                    closestIndex < vals.Count)
                {
                    valueStr = vals[closestIndex].ToString("G6", CultureInfo.InvariantCulture);
                }

                // Convert pixel distance back to a world-space distance for comparison.
                float worldDist = minDist * depthRange / trackBodyHeight;

                var hit = new LayerHitResult(
                    layerName: Name,
                    featureId: $"curve:{curveName}",
                    featureLabel: displayName,
                    featureKind: "Log Curve",
                    worldAnchor: worldAnchor,
                    distance: worldDist,
                    metadata: new Dictionary<string, string>
                    {
                        ["CurveName"] = curveName,
                        ["Value"] = valueStr
                    });

                if (best == null || hit.Distance < best.Distance)
                    best = hit;
            }

            return best;
        }

        private LayerHitResult BuildIntervalHit(LogIntervalInteractionLayout interval, SKPoint worldAnchor)
        {
            string label = interval.Interval?.Lithology ?? interval.Interval?.Label ?? interval.Label ?? "Unknown";

            return new LayerHitResult(
                layerName: Name,
                featureId: $"interval:{interval.Interval?.IntervalId}",
                featureLabel: label,
                featureKind: "Lithology Interval",
                worldAnchor: worldAnchor,
                distance: 0f,
                metadata: new Dictionary<string, string>
                {
                    ["IntervalId"] = interval.Interval?.IntervalId ?? string.Empty,
                    ["Lithology"] = interval.Interval?.Lithology ?? string.Empty
                });
        }

        private LayerHitResult BuildTrackAreaHit(
            LogTrackInteractionLayout layout,
            SKPoint worldAnchor,
            DepthTransform depthTransform,
            float depthRange)
        {
            string trackKind = layout.Track?.Kind switch
            {
                LogTrackKind.Depth => "Depth",
                LogTrackKind.Lithology => "Lithology",
                LogTrackKind.Zonation => "Zonation",
                _ => "Curve"
            };

            string trackName = layout.Track?.Name ?? "Track";
            string depthUnit = _logData.DepthUnit ?? string.Empty;

            var metadata = new Dictionary<string, string>
            {
                ["TrackKind"] = trackKind,
                ["DepthUnit"] = depthUnit
            };

            // Add interpolated curve values for curve tracks.
            if (layout.Curves != null && (layout.Track?.Kind ?? LogTrackKind.Curve) == LogTrackKind.Curve)
            {
                double hitDepth = worldAnchor.Y;
                foreach (var curve in layout.Curves)
                {
                    string curveName = curve.Definition?.CurveName ?? string.Empty;
                    if (string.IsNullOrEmpty(curveName))
                        continue;

                    if (!_logData.Curves.TryGetValue(curveName, out var vals) ||
                        vals == null || vals.Count == 0 ||
                        _logData.Depths == null || _logData.Depths.Count == 0)
                        continue;

                    double interpolated = InterpolateAtDepth(_logData.Depths, vals, hitDepth);
                    metadata[$"Value:{curveName}"] = interpolated.ToString("G6", CultureInfo.InvariantCulture);
                }
            }

            return new LayerHitResult(
                layerName: Name,
                featureId: $"track:{trackName}",
                featureLabel: trackName,
                featureKind: "Log Track",
                worldAnchor: worldAnchor,
                distance: 0f,
                metadata: metadata);
        }

        private static double InterpolateAtDepth(List<double> depths, List<double> values, double depth)
        {
            if (depths.Count == 1)
                return values[0];

            // Binary search for the bounding samples.
            for (int i = 0; i < depths.Count - 1; i++)
            {
                if (depth >= depths[i] && depth <= depths[i + 1])
                {
                    double t = (depth - depths[i]) / (depths[i + 1] - depths[i]);
                    return values[i] + t * (values[i + 1] - values[i]);
                }
            }

            // Clamp to endpoints.
            return depth < depths[0] ? values[0] : values[depths.Count - 1];
        }

        private float ComputeTotalWidth()
        {
            var tracks = _configuration.Tracks;

            if (tracks == null || tracks.Count == 0)
                return _configuration.TrackWidth;

            bool hasDepthTrack = false;
            float total = 0f;
            for (int i = 0; i < tracks.Count; i++)
            {
                if (tracks[i].Kind == LogTrackKind.Depth)
                    hasDepthTrack = true;

                float w = tracks[i].Width
                    ?? (tracks[i].Kind == LogTrackKind.Depth
                        ? _configuration.DepthTrackWidth
                        : _configuration.TrackWidth);
                total += w;
                if (i < tracks.Count - 1)
                    total += _configuration.TrackSpacing;
            }

            // If the renderer will auto-insert a depth track, account for its width.
            if (_configuration.ShowDepthScale &&
                _configuration.RenderDepthScaleAsTrack &&
                !hasDepthTrack)
            {
                total += _configuration.DepthTrackWidth + _configuration.TrackSpacing;
            }

            return total;
        }
    }
}
