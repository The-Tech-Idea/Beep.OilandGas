using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Styling;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Renders well logs along wellbore paths (supports vertical and directional/deviated wells).
    /// </summary>
    public class LogRenderer
    {
        private readonly LogData logData;
        private readonly DeviationSurvey deviationSurvey;
        private readonly LogRendererConfiguration configuration;
        private DepthTransform depthSystem;
        private List<SKPoint> wellborePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogRenderer"/> class.
        /// </summary>
        /// <param name="logData">The log data to render.</param>
        /// <param name="deviationSurvey">The deviation survey for directional wells (optional).</param>
        /// <param name="configuration">Rendering configuration.</param>
        public LogRenderer(
            LogData logData,
            DeviationSurvey deviationSurvey = null,
            LogRendererConfiguration configuration = null)
        {
            this.logData = logData ?? throw new ArgumentNullException(nameof(logData));
            this.deviationSurvey = deviationSurvey;
            this.configuration = configuration ?? new LogRendererConfiguration();

            // Create depth transform
            depthSystem = new DepthTransform(
                logData.StartDepth,
                logData.EndDepth,
                1000f); // Default canvas height, will be updated on render

            // Calculate wellbore path
            CalculateWellborePath();
        }

        /// <summary>
        /// Calculates the wellbore path from deviation survey or creates vertical path.
        /// </summary>
        private void CalculateWellborePath()
        {
            if (deviationSurvey != null && deviationSurvey.SurveyPoints != null && deviationSurvey.SurveyPoints.Count > 0)
            {
                // Directional/deviated well
                wellborePath = WellborePathCalculator.CalculatePath(
                    deviationSurvey,
                    depthSystem,
                    configuration.HorizontalStretchFactor);
            }
            else
            {
                // Vertical well
                wellborePath = WellborePathCalculator.CalculateVerticalPath(
                    logData.StartDepth,
                    logData.EndDepth,
                    depthSystem);
            }
        }

        /// <summary>
        /// Renders the log tracks along the wellbore path.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render on.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        /// <param name="xOffset">X offset for positioning.</param>
        /// <param name="yOffset">Y offset for positioning.</param>
        public void Render(SKCanvas canvas, float width, float height, float xOffset = 0, float yOffset = 0)
        {
            if (canvas == null || logData == null || wellborePath == null || wellborePath.Count == 0)
                return;

            var renderContext = PrepareTrackRenderContext(height, yOffset);
            float trackBodyY = renderContext.TrackBodyY;
            float trackBodyHeight = renderContext.TrackBodyHeight;
            var tracks = renderContext.Tracks;
            bool hasDepthTrack = tracks.Any(track => track.Kind == LogTrackKind.Depth);

            // Draw wellbore path if enabled
            if (configuration.ShowWellborePath)
            {
                DrawWellborePath(canvas, xOffset, trackBodyY);
            }

            // Draw depth markers if enabled
            if (configuration.ShowDepthMarkers)
            {
                DrawDepthMarkers(canvas, xOffset, trackBodyY, width);
            }

            // Draw log tracks
            float currentX = xOffset;
            foreach (var track in tracks)
            {
                float trackWidth = GetTrackWidth(track);
                DrawLogTrack(canvas, track, currentX, trackBodyY, trackBodyHeight);
                currentX += trackWidth + configuration.TrackSpacing;
            }

            // Draw depth scale if enabled
            if (configuration.ShowDepthScale && !hasDepthTrack)
            {
                DrawDepthScale(canvas, xOffset, trackBodyY, trackBodyHeight);
            }
        }

        internal IReadOnlyList<LogTrackInteractionLayout> GetInteractionLayouts(float height, float xOffset = 0, float yOffset = 0)
        {
            if (logData == null || wellborePath == null || wellborePath.Count == 0)
                return Array.Empty<LogTrackInteractionLayout>();

            var renderContext = PrepareTrackRenderContext(height, yOffset);
            var layouts = new List<LogTrackInteractionLayout>(renderContext.Tracks.Count);
            float currentX = xOffset;

            foreach (var track in renderContext.Tracks)
            {
                float trackWidth = GetTrackWidth(track);
                var scaleAnnotations = BuildTrackScaleAnnotations(track);
                float annotationBandHeight = configuration.ShowTrackScaleAnnotations && scaleAnnotations.Count > 0
                    ? (configuration.TrackScaleAnnotationPadding * 2) + (scaleAnnotations.Count * configuration.TrackScaleAnnotationRowHeight)
                    : 0;
                float plotY = renderContext.TrackBodyY + annotationBandHeight;
                var bodyBounds = new SKRect(currentX, renderContext.TrackBodyY, currentX + trackWidth, renderContext.TrackBodyY + renderContext.TrackBodyHeight);
                var bounds = new SKRect(currentX, yOffset, currentX + trackWidth, renderContext.TrackBodyY + renderContext.TrackBodyHeight);
                var curveStates = track.Kind == LogTrackKind.Curve && track.Curves != null && track.Curves.Count > 0
                    ? BuildCurveRenderStates(track, currentX, plotY, trackWidth)
                    : new List<CurveRenderState>();
                var curves = curveStates.Count > 0
                    ? curveStates
                        .Select(state => new LogCurveInteractionLayout(
                            state.Definition,
                            state.Metadata,
                            state.Color,
                            state.Samples.Select(sample => sample.Point).ToList()))
                        .ToList()
                    : new List<LogCurveInteractionLayout>();
                var crossovers = curveStates.Count > 0 && configuration.ShowDensityNeutronCrossoverShading
                    ? BuildDensityNeutronCrossoverLayouts(curveStates)
                    : new List<LogCrossoverInteractionLayout>();
                var intervals = track.Kind == LogTrackKind.Lithology || track.Kind == LogTrackKind.Zonation
                    ? BuildIntervalLayouts(track, currentX, renderContext.TrackBodyY, trackWidth)
                    : new List<LogIntervalInteractionLayout>();

                layouts.Add(new LogTrackInteractionLayout(track, bounds, bodyBounds, curves, crossovers, intervals));
                currentX += trackWidth + configuration.TrackSpacing;
            }

            return layouts;
        }

        /// <summary>
        /// Draws a single log track.
        /// </summary>
        private void DrawLogTrack(SKCanvas canvas, LogTrackDefinition track, float x, float y, float height)
        {
            if (track == null)
                return;

            float trackWidth = GetTrackWidth(track);
            var scaleAnnotations = BuildTrackScaleAnnotations(track);
            float annotationBandHeight = configuration.ShowTrackScaleAnnotations && scaleAnnotations.Count > 0
                ? (configuration.TrackScaleAnnotationPadding * 2) + (scaleAnnotations.Count * configuration.TrackScaleAnnotationRowHeight)
                : 0;
            float plotY = y + annotationBandHeight;
            float plotHeight = Math.Max(1.0f, height - annotationBandHeight);

            if (configuration.ShowTrackHeaders)
            {
                DrawTrackHeader(canvas, track, x, y - configuration.TrackHeaderHeight, trackWidth);
            }

            // Draw track background
            using (var bgPaint = new SKPaint
            {
                Color = track.BackgroundColor ?? configuration.BackgroundColor,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRect(x, y, trackWidth, height, bgPaint);
            }

            if (track.Kind == LogTrackKind.Depth)
            {
                DrawDepthTrack(canvas, track, x, y, height, trackWidth);
                return;
            }

            if (track.Kind == LogTrackKind.Lithology || track.Kind == LogTrackKind.Zonation)
            {
                DrawIntervalTrack(canvas, track, x, y, height, trackWidth);
                return;
            }

            if (track.Curves == null || track.Curves.Count == 0)
                return;

            var curveStates = BuildCurveRenderStates(track, x, plotY, trackWidth);
            if (curveStates.Count == 0)
                return;

            if (annotationBandHeight > 0)
            {
                DrawTrackScaleAnnotations(canvas, x, y, trackWidth, scaleAnnotations);
            }

            // Draw grid if enabled
            if (configuration.ShowGrid)
            {
                DrawTrackGrid(canvas, track, curveStates, x, plotY, plotHeight, trackWidth);
            }

            if (configuration.ShowDensityNeutronCrossoverShading && TryGetDensityNeutronCrossoverPair(curveStates, out var densityCurve, out var neutronCurve))
            {
                DrawDensityNeutronCrossoverShading(canvas, densityCurve, neutronCurve);
            }

            for (int curveIndex = 0; curveIndex < curveStates.Count; curveIndex++)
            {
                var curveState = curveStates[curveIndex];

                DrawCurve(canvas, curveState.Samples, x, plotY, plotHeight, trackWidth, curveState.Color);

                if (configuration.ShowCurveNames && !configuration.ShowTrackHeaders)
                {
                    DrawCurveName(canvas, curveState.Definition, curveState.Metadata, x, plotY, trackWidth, curveIndex);
                }
            }
        }

        /// <summary>
        /// Draws the dedicated depth track.
        /// </summary>
        private void DrawDepthTrack(SKCanvas canvas, LogTrackDefinition track, float x, float y, float height, float trackWidth)
        {
            double depthRange = logData.EndDepth - logData.StartDepth;
            if (depthRange <= 0)
                return;

            double majorInterval = track.MajorInterval.GetValueOrDefault(configuration.DepthInterval);
            if (majorInterval <= 0)
                majorInterval = configuration.DepthInterval > 0 ? configuration.DepthInterval : 100.0;

            int majorCount = Math.Max(1, (int)Math.Ceiling(depthRange / majorInterval));
            int minorSubdivisionCount = Math.Max(0, track.MinorSubdivisionCount);
            string labelFormat = string.IsNullOrWhiteSpace(track.LabelFormat) ? "F0" : track.LabelFormat;

            using var linePaint = new SKPaint
            {
                Color = configuration.DepthScaleColor,
                StrokeWidth = 1.0f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            using var gridPaint = new SKPaint
            {
                Color = configuration.GridColor,
                StrokeWidth = configuration.GridLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            using var textPaint = new SKPaint
            {
                Color = configuration.DepthScaleColor,
                TextSize = configuration.DepthScaleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            };

            float axisX = x + trackWidth - 1.0f;
            canvas.DrawLine(axisX, y, axisX, y + height, linePaint);

            for (int index = 0; index <= majorCount; index++)
            {
                double depth = logData.StartDepth + (majorInterval * index);
                if (depth > logData.EndDepth)
                    depth = logData.EndDepth;

                float tickY = y + depthSystem.ToScreenY((float)depth, null);

                if (configuration.ShowGrid)
                {
                    canvas.DrawLine(x, tickY, axisX, tickY, gridPaint);
                }

                canvas.DrawLine(axisX - configuration.DepthTrackMajorTickLength, tickY, axisX, tickY, linePaint);
                canvas.DrawText(depth.ToString(labelFormat), axisX - configuration.DepthTrackMajorTickLength - 4.0f, tickY + 4.0f, textPaint);

                if (minorSubdivisionCount == 0 || depth >= logData.EndDepth)
                    continue;

                double nextDepth = Math.Min(logData.EndDepth, depth + majorInterval);
                double minorStep = (nextDepth - depth) / (minorSubdivisionCount + 1);

                for (int minorIndex = 1; minorIndex <= minorSubdivisionCount; minorIndex++)
                {
                    double minorDepth = depth + (minorStep * minorIndex);
                    if (minorDepth >= nextDepth)
                        break;

                    float minorY = y + depthSystem.ToScreenY((float)minorDepth, null);
                    canvas.DrawLine(axisX - configuration.DepthTrackMinorTickLength, minorY, axisX, minorY, linePaint);
                }
            }
        }

        /// <summary>
        /// Draws a log curve along the wellbore path.
        /// </summary>
        private void DrawCurve(
            SKCanvas canvas,
            IReadOnlyList<CurveSample> samples,
            float trackX,
            float trackY,
            float trackHeight,
            float trackWidth,
            SKColor color)
        {
            if (samples == null || samples.Count == 0)
                return;

            var path = new SKPath();
            bool firstPoint = true;

            for (int i = 0; i < samples.Count; i++)
            {
                var point = samples[i].Point;

                if (firstPoint)
                {
                    path.MoveTo(point);
                    firstPoint = false;
                }
                else
                {
                    path.LineTo(point);
                }
            }

            // Draw filled curve if enabled
            if (configuration.FillCurves && path.PointCount > 0)
            {
                using (var fillPath = new SKPath(path))
                {
                    // Close path for fill
                    SKPoint firstPoint2 = path.GetPoint(0);
                    SKPoint lastPoint = path.GetPoint(path.PointCount - 1);
                    fillPath.LineTo(trackX + trackWidth, lastPoint.Y);
                    fillPath.LineTo(trackX + trackWidth, firstPoint2.Y);
                    fillPath.LineTo(trackX, firstPoint2.Y);
                    fillPath.Close();

                    using (var fillPaint = new SKPaint
                    {
                        Color = configuration.FillColor,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawPath(fillPath, fillPaint);
                    }
                }
            }

            // Draw curve line
            using (var curvePaint = new SKPaint
            {
                Color = color,
                StrokeWidth = configuration.CurveLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                canvas.DrawPath(path, curvePaint);
            }
        }

        /// <summary>
        /// Gets a point on the wellbore path at a specific depth.
        /// </summary>
        private SKPoint GetPointAtDepth(double depth)
        {
            if (wellborePath == null || wellborePath.Count == 0)
                return SKPoint.Empty;

            // For vertical wells, simple conversion
            if (deviationSurvey == null || deviationSurvey.SurveyPoints == null || deviationSurvey.SurveyPoints.Count == 0)
            {
                float y = depthSystem.ToScreenY((float)depth, null);
                return new SKPoint(0, y);
            }

            // For directional wells, interpolate along path
            // Find closest depth in log depths
            int closestIndex = 0;
            double minDiff = double.MaxValue;

            if (logData.Depths != null)
            {
                for (int i = 0; i < logData.Depths.Count; i++)
                {
                    double diff = Math.Abs(logData.Depths[i] - depth);
                    if (diff < minDiff)
                    {
                        minDiff = diff;
                        closestIndex = i;
                    }
                }
            }

            // Map to wellbore path point
            if (closestIndex < wellborePath.Count)
            {
                return wellborePath[closestIndex];
            }

            // Interpolate between path points
            if (wellborePath.Count >= 2)
            {
                // Find surrounding path points based on depth
                var surveyPoint = deviationSurvey.GetInterpolatedPointAtDepth(depth);
                if (surveyPoint != null)
                {
                    // Calculate position from survey point
                    // This is simplified - full implementation would calculate 3D position
                    float y = depthSystem.ToScreenY((float)depth, null);
                    return new SKPoint(0, y); // Simplified - would need full 3D calculation
                }
            }

            return wellborePath.LastOrDefault();
        }

        /// <summary>
        /// Draws grid lines in a log track.
        /// </summary>
        private void DrawTrackGrid(SKCanvas canvas, LogTrackDefinition track, IReadOnlyList<CurveRenderState> curveStates, float x, float y, float height, float trackWidth)
        {
            if (configuration.ShowLogDecadeGridLines && TryGetLogGridRange(curveStates, out var logMinimumValue, out var logMaximumValue))
            {
                DrawLogTrackGrid(canvas, x, y, height, trackWidth, logMinimumValue, logMaximumValue);
            }
            else
            {
                using (var gridPaint = new SKPaint
                {
                    Color = configuration.GridColor,
                    StrokeWidth = configuration.GridLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    // Vertical grid lines (value scale)
                    int gridLines = 5;
                    for (int i = 0; i <= gridLines; i++)
                    {
                        float gridX = x + (trackWidth * i / gridLines);
                        canvas.DrawLine(gridX, y, gridX, y + height, gridPaint);
                    }
                }
            }

            using (var horizontalPaint = new SKPaint
            {
                Color = configuration.GridColor,
                StrokeWidth = configuration.GridLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                double depthRange = logData.EndDepth - logData.StartDepth;
                int depthLines = Math.Max(1, (int)Math.Ceiling(depthRange / configuration.DepthInterval));
                for (int i = 0; i <= depthLines; i++)
                {
                    double depth = logData.StartDepth + (depthRange * i / depthLines);
                    float gridY = depthSystem.ToScreenY((float)depth, null);
                    canvas.DrawLine(x, y + gridY, x + trackWidth, y + gridY, horizontalPaint);
                }
            }
        }

        /// <summary>
        /// Draws the curve name label.
        /// </summary>
        private void DrawCurveName(SKCanvas canvas, LogTrackCurveDefinition curveDefinition, LogCurveMetadata metadata, float x, float y, float trackWidth, int lineIndex)
        {
            string displayName = curveDefinition.DisplayName ?? metadata?.DisplayName ?? curveDefinition.CurveName;
            string unit = metadata?.Unit != null ? $" ({metadata.Unit})" : "";

            using (var textPaint = new SKPaint
            {
                Color = configuration.CurveNameColor,
                TextSize = configuration.CurveNameFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                canvas.DrawText(displayName + unit, x + trackWidth / 2, y + 15 + (lineIndex * (configuration.CurveNameFontSize + 2)), textPaint);
            }
        }

        /// <summary>
        /// Gets metadata for a curve.
        /// </summary>
        private LogCurveMetadata GetMetadata(string curveName)
        {
            return logData.CurveMetadata.ContainsKey(curveName) 
                ? logData.CurveMetadata[curveName] 
                : null;
        }

        /// <summary>
        /// Draws the wellbore path.
        /// </summary>
        private void DrawWellborePath(SKCanvas canvas, float xOffset, float yOffset)
        {
            if (wellborePath == null || wellborePath.Count < 2)
                return;

            using (var path = new SKPath())
            {
                path.MoveTo(wellborePath[0].X + xOffset, wellborePath[0].Y + yOffset);
                for (int i = 1; i < wellborePath.Count; i++)
                {
                    path.LineTo(wellborePath[i].X + xOffset, wellborePath[i].Y + yOffset);
                }

                using (var paint = new SKPaint
                {
                    Color = configuration.WellborePathColor,
                    StrokeWidth = configuration.WellborePathWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(path, paint);
                }
            }
        }

        /// <summary>
        /// Draws depth markers along the wellbore path.
        /// </summary>
        private void DrawDepthMarkers(SKCanvas canvas, float xOffset, float yOffset, float width)
        {
            if (wellborePath == null || wellborePath.Count == 0)
                return;

            using (var markerPaint = new SKPaint
            {
                Color = configuration.DepthMarkerColor,
                StrokeWidth = configuration.DepthMarkerLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            using (var textPaint = new SKPaint
            {
                Color = configuration.DepthMarkerColor,
                TextSize = configuration.DepthScaleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            })
            {
                double depthRange = logData.EndDepth - logData.StartDepth;
                int markerCount = Math.Max(1, (int)Math.Ceiling(depthRange / configuration.DepthInterval));

                for (int i = 0; i <= markerCount; i++)
                {
                    double depth = logData.StartDepth + (depthRange * i / markerCount);
                    SKPoint pathPoint = GetPointAtDepth(depth);

                    if (pathPoint != SKPoint.Empty)
                    {
                        // Draw marker line perpendicular to wellbore
                        float markerX = pathPoint.X + xOffset;
                        float markerY = pathPoint.Y + yOffset;

                        // Draw line
                        canvas.DrawLine(markerX - 10, markerY, markerX + 10, markerY, markerPaint);

                        // Draw depth label
                        canvas.DrawText(depth.ToString("F0"), markerX - 15, markerY + 5, textPaint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws the depth scale.
        /// </summary>
        private void DrawDepthScale(SKCanvas canvas, float xOffset, float yOffset, float height)
        {
            using (var scalePaint = new SKPaint
            {
                Color = configuration.DepthScaleColor,
                TextSize = configuration.DepthScaleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            })
            {
                double depthRange = logData.EndDepth - logData.StartDepth;
                int scaleCount = Math.Max(1, (int)Math.Ceiling(depthRange / configuration.DepthInterval));

                for (int i = 0; i <= scaleCount; i++)
                {
                    double depth = logData.StartDepth + (depthRange * i / scaleCount);
                    float y = depthSystem.ToScreenY((float)depth, null) + yOffset;
                    canvas.DrawText(depth.ToString("F0"), xOffset - 5, y + 5, scalePaint);
                }
            }
        }

        private IReadOnlyList<LogTrackDefinition> GetTracksToRender()
        {
            if (configuration.Tracks != null && configuration.Tracks.Count > 0)
            {
                var configuredTracks = configuration.Tracks
                    .Select(CloneTrackWithAvailableCurves)
                    .Where(track => track != null && (track.Kind == LogTrackKind.Depth || track.Curves.Count > 0 || track.Intervals.Count > 0))
                    .ToList();

                if (configuration.ShowDepthScale && configuration.RenderDepthScaleAsTrack && configuredTracks.All(track => track.Kind != LogTrackKind.Depth))
                {
                    configuredTracks.Insert(0, CreateDefaultDepthTrack());
                }

                if (configuredTracks.Count > 0)
                    return configuredTracks;
            }

            var defaultTracks = configuration.UseStandardTrackTemplates
                ? LogTrackTemplates.CreateStandardPetrophysicalTracks(logData)
                : logData.Curves.Keys
                    .Select(curveName => new LogTrackDefinition
                    {
                        Name = GetDefaultTrackName(curveName),
                        Curves = new List<LogTrackCurveDefinition>
                        {
                            new LogTrackCurveDefinition { CurveName = curveName }
                        }
                    })
                    .ToList();

            if (configuration.ShowDepthScale && configuration.RenderDepthScaleAsTrack)
            {
                defaultTracks.Insert(0, CreateDefaultDepthTrack());
            }

            return defaultTracks;
        }

        private LogTrackDefinition CloneTrackWithAvailableCurves(LogTrackDefinition track)
        {
            if (track == null)
                return null;

            if (track.Kind == LogTrackKind.Depth)
            {
                return new LogTrackDefinition
                {
                    Kind = LogTrackKind.Depth,
                    Name = string.IsNullOrWhiteSpace(track.Name) ? "Depth" : track.Name,
                    Width = track.Width,
                    BackgroundColor = track.BackgroundColor,
                    MajorInterval = track.MajorInterval,
                    MinorSubdivisionCount = track.MinorSubdivisionCount,
                    LabelFormat = track.LabelFormat
                };
            }

            if (track.Kind == LogTrackKind.Lithology || track.Kind == LogTrackKind.Zonation)
            {
                var availableIntervals = (track.Intervals ?? new List<LogIntervalData>())
                    .Where(interval => interval != null && interval.BottomDepth > interval.TopDepth)
                    .OrderBy(interval => interval.TopDepth)
                    .ToList();

                return new LogTrackDefinition
                {
                    Kind = track.Kind,
                    Name = string.IsNullOrWhiteSpace(track.Name)
                        ? (track.Kind == LogTrackKind.Lithology ? "Lithology" : "Zones")
                        : track.Name,
                    Width = track.Width,
                    BackgroundColor = track.BackgroundColor,
                    MajorInterval = track.MajorInterval,
                    MinorSubdivisionCount = track.MinorSubdivisionCount,
                    LabelFormat = track.LabelFormat,
                    Intervals = availableIntervals
                };
            }

            var availableCurves = (track?.Curves ?? new List<LogTrackCurveDefinition>())
                .Where(curve => !string.IsNullOrWhiteSpace(curve?.CurveName) && logData.Curves.ContainsKey(curve.CurveName))
                .Select(curve => new LogTrackCurveDefinition
                {
                    CurveName = curve.CurveName,
                    DisplayName = curve.DisplayName,
                    Color = curve.Color,
                    MinValue = curve.MinValue,
                    MaxValue = curve.MaxValue,
                    ScaleType = curve.ScaleType,
                    InvertScale = curve.InvertScale,
                    ValueFormat = curve.ValueFormat
                })
                .ToList();

            return new LogTrackDefinition
            {
                Kind = LogTrackKind.Curve,
                Name = string.IsNullOrWhiteSpace(track?.Name)
                    ? availableCurves.Select(curve => GetCurveHeaderLabel(curve)).FirstOrDefault() ?? string.Empty
                    : track.Name,
                Width = track?.Width,
                BackgroundColor = track?.BackgroundColor,
                MajorInterval = track?.MajorInterval,
                MinorSubdivisionCount = track?.MinorSubdivisionCount ?? 4,
                LabelFormat = track?.LabelFormat,
                Curves = availableCurves
            };
        }

        private void DrawTrackHeader(SKCanvas canvas, LogTrackDefinition track, float x, float y, float trackWidth)
        {
            using var backgroundPaint = new SKPaint
            {
                Color = configuration.TrackHeaderBackgroundColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            using var borderPaint = new SKPaint
            {
                Color = configuration.TrackHeaderBorderColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1.0f,
                IsAntialias = true
            };

            using var titlePaint = new SKPaint
            {
                Color = configuration.CurveNameColor,
                TextSize = configuration.TrackHeaderFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            using var detailPaint = new SKPaint
            {
                Color = configuration.CurveNameColor,
                TextSize = configuration.TrackHeaderDetailFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            var headerRect = new SKRect(x, y, x + trackWidth, y + configuration.TrackHeaderHeight);
            canvas.DrawRect(headerRect, backgroundPaint);
            canvas.DrawRect(headerRect, borderPaint);

            string title = string.IsNullOrWhiteSpace(track.Name)
                ? track.Curves.Select(GetCurveHeaderLabel).FirstOrDefault() ?? string.Empty
                : track.Name;
            string detail = track.Kind == LogTrackKind.Depth
                ? $"MD ({logData.DepthUnit})"
                : track.Kind == LogTrackKind.Lithology || track.Kind == LogTrackKind.Zonation
                    ? $"{track.Intervals.Count} intervals"
                : string.Join(" | ", track.Curves.Select(GetCurveHeaderLabel));

            canvas.DrawText(title, x + trackWidth / 2.0f, y + 13.0f, titlePaint);
            canvas.DrawText(detail, x + trackWidth / 2.0f, y + configuration.TrackHeaderHeight - 6.0f, detailPaint);
        }

        private SKColor ResolveCurveColor(LogTrackCurveDefinition curveDefinition)
        {
            if (curveDefinition.Color.HasValue)
                return curveDefinition.Color.Value;

            if (configuration.DefaultCurveColors.TryGetValue(curveDefinition.CurveName, out var configuredColor))
                return configuredColor;

            return SKColors.Black;
        }

        private List<TrackScaleAnnotation> BuildTrackScaleAnnotations(LogTrackDefinition track)
        {
            var annotations = new List<TrackScaleAnnotation>();

            if (!configuration.ShowTrackScaleAnnotations || track.Kind != LogTrackKind.Curve || track.Curves == null)
                return annotations;

            foreach (var curveDefinition in track.Curves)
            {
                if (!logData.Curves.TryGetValue(curveDefinition.CurveName, out var curveValues) || curveValues == null || curveValues.Count == 0)
                    continue;

                var metadata = GetMetadata(curveDefinition.CurveName);
                bool useLogScale = ResolveScaleType(curveDefinition, metadata, curveDefinition.CurveName) == LogTrackScaleType.Logarithmic;
                double displayMin = ResolveDisplayMinimumValue(curveDefinition, curveDefinition.CurveName, curveValues);
                double displayMax = ResolveDisplayMaximumValue(curveDefinition, curveDefinition.CurveName, curveValues);
                string format = ResolveValueFormat(curveDefinition, useLogScale);
                string unit = metadata?.Unit ?? string.Empty;
                string label = ResolveScaleLabel(curveDefinition, metadata, useLogScale);
                var color = ResolveCurveColor(curveDefinition);

                var existing = annotations.FirstOrDefault(annotation => annotation.Matches(displayMin, displayMax, curveDefinition.InvertScale, useLogScale, unit, format));
                if (existing != null)
                {
                    existing.AppendLabel(GetShortScaleLabel(curveDefinition, metadata));
                    continue;
                }

                annotations.Add(new TrackScaleAnnotation(label, unit, format, displayMin, displayMax, curveDefinition.InvertScale, useLogScale, color));
            }

            return annotations;
        }

        private void DrawIntervalTrack(SKCanvas canvas, LogTrackDefinition track, float x, float y, float height, float trackWidth)
        {
            var intervals = BuildIntervalLayouts(track, x, y, trackWidth);

            if (intervals.Count == 0)
                return;

            using var borderPaint = new SKPaint
            {
                Color = configuration.IntervalBorderColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = configuration.IntervalBorderWidth,
                IsAntialias = true
            };

            foreach (var interval in intervals)
            {
                using var fillPaint = CreateIntervalPaint(track.Kind, interval.Interval);
                canvas.DrawRect(interval.Bounds, fillPaint);
                canvas.DrawRect(interval.Bounds, borderPaint);

                if (configuration.ShowIntervalLabels && interval.Bounds.Height >= configuration.MinimumIntervalLabelHeight)
                {
                    DrawIntervalLabel(canvas, track.Kind, interval.Interval, interval.Bounds);
                }
            }
        }

        private void DrawTrackScaleAnnotations(SKCanvas canvas, float x, float y, float trackWidth, IReadOnlyList<TrackScaleAnnotation> annotations)
        {
            if (annotations == null || annotations.Count == 0)
                return;

            using var separatorPaint = new SKPaint
            {
                Color = configuration.TrackScaleAnnotationSeparatorColor,
                StrokeWidth = 1.0f,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            for (int index = 0; index < annotations.Count; index++)
            {
                var annotation = annotations[index];
                float baseline = y + configuration.TrackScaleAnnotationPadding + ((index + 1) * configuration.TrackScaleAnnotationRowHeight) - 2.0f;

                using var leftPaint = new SKPaint
                {
                    Color = annotation.Color,
                    TextSize = configuration.TrackScaleAnnotationFontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };

                using var rightPaint = new SKPaint
                {
                    Color = annotation.Color,
                    TextSize = configuration.TrackScaleAnnotationFontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Right
                };

                using var centerPaint = new SKPaint
                {
                    Color = annotation.Color,
                    TextSize = configuration.TrackScaleAnnotationFontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                };

                canvas.DrawText(annotation.GetLeftValue(), x + 2.0f, baseline, leftPaint);
                canvas.DrawText(annotation.BuildCenterLabel(), x + trackWidth / 2.0f, baseline, centerPaint);
                canvas.DrawText(annotation.GetRightValue(), x + trackWidth - 2.0f, baseline, rightPaint);
            }

            float separatorY = y + (configuration.TrackScaleAnnotationPadding * 2) + (annotations.Count * configuration.TrackScaleAnnotationRowHeight);
            canvas.DrawLine(x, separatorY, x + trackWidth, separatorY, separatorPaint);
        }

        private List<CurveRenderState> BuildCurveRenderStates(LogTrackDefinition track, float trackX, float trackY, float trackWidth)
        {
            var states = new List<CurveRenderState>();

            foreach (var curveDefinition in track.Curves)
            {
                if (!logData.Curves.TryGetValue(curveDefinition.CurveName, out var curveValues) || curveValues == null || curveValues.Count == 0)
                    continue;

                if (logData.Depths == null || logData.Depths.Count == 0)
                    continue;

                var metadata = GetMetadata(curveDefinition.CurveName);
                var color = ResolveCurveColor(curveDefinition);
                bool useLogScale = ResolveScaleType(curveDefinition, metadata, curveDefinition.CurveName) == LogTrackScaleType.Logarithmic;
                double displayMinimumValue = ResolveDisplayMinimumValue(curveDefinition, curveDefinition.CurveName, curveValues);
                double displayMaximumValue = ResolveDisplayMaximumValue(curveDefinition, curveDefinition.CurveName, curveValues);
                double plottingMinimumValue = ResolveMinimumValue(curveDefinition, curveDefinition.CurveName, curveValues, useLogScale);
                double plottingMaximumValue = ResolveMaximumValue(curveDefinition, curveDefinition.CurveName, curveValues, useLogScale);
                var samples = BuildCurveSamples(curveDefinition.CurveName, curveValues, logData.Depths, trackX, trackY, trackWidth, plottingMinimumValue, plottingMaximumValue, useLogScale);

                if (samples.Count == 0)
                    continue;

                states.Add(new CurveRenderState(
                    curveDefinition,
                    metadata,
                    color,
                    useLogScale,
                    displayMinimumValue,
                    displayMaximumValue,
                    samples));
            }

            return states;
        }

        private List<LogIntervalInteractionLayout> BuildIntervalLayouts(LogTrackDefinition track, float x, float y, float trackWidth)
        {
            var intervals = (track.Intervals ?? new List<LogIntervalData>())
                .Where(interval => interval != null && interval.BottomDepth > interval.TopDepth)
                .OrderBy(interval => interval.TopDepth)
                .ToList();

            var layouts = new List<LogIntervalInteractionLayout>(intervals.Count);
            foreach (var interval in intervals)
            {
                float topY = y + depthSystem.ToScreenY(interval.TopDepth, null);
                float bottomY = y + depthSystem.ToScreenY(interval.BottomDepth, null);
                float intervalTop = Math.Min(topY, bottomY);
                float intervalBottom = Math.Max(topY, bottomY);

                if (intervalBottom - intervalTop < 0.5f)
                    continue;

                var rect = new SKRect(x, intervalTop, x + trackWidth, intervalBottom);
                string label = track.Kind == LogTrackKind.Lithology
                    ? BuildLithologyIntervalLabel(interval)
                    : BuildZoneIntervalLabel(interval);
                layouts.Add(new LogIntervalInteractionLayout(interval, rect, label));
            }

            return layouts;
        }

        private List<CurveSample> BuildCurveSamples(
            string curveName,
            List<double> values,
            List<double> depths,
            float trackX,
            float trackY,
            float trackWidth,
            double minimumValue,
            double maximumValue,
            bool useLogScale)
        {
            var samples = new List<CurveSample>();
            var curveMetadata = logData.CurveMetadata.ContainsKey(curveName)
                ? logData.CurveMetadata[curveName]
                : null;

            for (int index = 0; index < values.Count && index < depths.Count; index++)
            {
                double value = values[index];
                double depth = depths[index];

                if (curveMetadata != null && curveMetadata.NullValue.HasValue && Math.Abs(value - curveMetadata.NullValue.Value) < 0.001)
                    continue;

                if (useLogScale)
                {
                    value = Math.Log10(Math.Max(value, 0.01));
                }

                double normalizedValue = (value - minimumValue) / (maximumValue - minimumValue);
                if (double.IsNaN(normalizedValue) || double.IsInfinity(normalizedValue))
                    normalizedValue = 0.5;

                SKPoint pathPoint = GetPointAtDepth(depth);
                if (pathPoint == SKPoint.Empty)
                    continue;

                float curveX = trackX + (float)(normalizedValue * trackWidth);
                float curveY = trackY + pathPoint.Y;
                samples.Add(new CurveSample(index, new SKPoint(curveX, curveY)));
            }

            return samples;
        }

        private bool TryGetLogGridRange(IReadOnlyList<CurveRenderState> curveStates, out double minimumValue, out double maximumValue)
        {
            var logState = curveStates.FirstOrDefault(state => state.UseLogScale && state.DisplayMinimumValue > 0 && state.DisplayMaximumValue > state.DisplayMinimumValue);
            if (logState == null)
            {
                minimumValue = 0;
                maximumValue = 0;
                return false;
            }

            minimumValue = logState.DisplayMinimumValue;
            maximumValue = logState.DisplayMaximumValue;
            return true;
        }

        private void DrawLogTrackGrid(SKCanvas canvas, float x, float y, float height, float trackWidth, double minimumValue, double maximumValue)
        {
            double logMinimum = Math.Log10(Math.Max(minimumValue, 0.01));
            double logMaximum = Math.Log10(Math.Max(maximumValue, 0.01));
            if (logMaximum <= logMinimum)
                return;

            using var majorPaint = new SKPaint
            {
                Color = configuration.GridColor,
                StrokeWidth = Math.Max(configuration.GridLineWidth, 0.8f),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            using var minorPaint = new SKPaint
            {
                Color = configuration.GridColor.WithAlpha(110),
                StrokeWidth = Math.Max(configuration.GridLineWidth * 0.7f, 0.35f),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };

            int firstDecade = (int)Math.Floor(logMinimum);
            int lastDecade = (int)Math.Ceiling(logMaximum);

            for (int decade = firstDecade; decade <= lastDecade; decade++)
            {
                for (int multiplier = 1; multiplier <= 9; multiplier++)
                {
                    double value = multiplier * Math.Pow(10, decade);
                    if (value < minimumValue || value > maximumValue)
                        continue;

                    double normalized = (Math.Log10(value) - logMinimum) / (logMaximum - logMinimum);
                    float gridX = x + (float)(normalized * trackWidth);
                    canvas.DrawLine(gridX, y, gridX, y + height, multiplier == 1 ? majorPaint : minorPaint);
                }
            }
        }

        private bool TryGetDensityNeutronCrossoverPair(IReadOnlyList<CurveRenderState> curveStates, out CurveRenderState densityCurve, out CurveRenderState neutronCurve)
        {
            neutronCurve = curveStates.FirstOrDefault(state => MatchesCurveIdentity(state, "Neutron", "NPHI", "TNPH", "NPOR"));
            densityCurve = curveStates.FirstOrDefault(state => MatchesCurveIdentity(state, "DensityPorosity", "DPHI", "BulkDensity", "RHOB", "RHOZ", "DENSITY"));

            if (neutronCurve == null || densityCurve == null || ReferenceEquals(neutronCurve, densityCurve))
            {
                densityCurve = null;
                neutronCurve = null;
                return false;
            }

            return true;
        }

        private bool MatchesCurveIdentity(CurveRenderState state, params string[] fragments)
        {
            string candidate = string.Join(" ", new[]
            {
                state.Definition.CurveName,
                state.Definition.DisplayName,
                state.Metadata?.Mnemonic,
                state.Metadata?.DisplayName,
                state.Metadata?.Description
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

            return fragments.Any(fragment => candidate.IndexOf(fragment, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void DrawDensityNeutronCrossoverShading(SKCanvas canvas, CurveRenderState densityCurve, CurveRenderState neutronCurve)
        {
            using var fillPaint = new SKPaint
            {
                Color = configuration.DensityNeutronCrossoverFillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            foreach (var segment in BuildDensityNeutronCrossoverSegments(densityCurve, neutronCurve))
            {
                using var fillPath = new SKPath();
                fillPath.MoveTo(segment.Polygon[0]);
                for (int index = 1; index < segment.Polygon.Count; index++)
                {
                    fillPath.LineTo(segment.Polygon[index]);
                }

                fillPath.Close();
                canvas.DrawPath(fillPath, fillPaint);
            }
        }

        private List<LogCrossoverInteractionLayout> BuildDensityNeutronCrossoverLayouts(IReadOnlyList<CurveRenderState> curveStates)
        {
            if (!TryGetDensityNeutronCrossoverPair(curveStates, out var densityCurve, out var neutronCurve))
                return new List<LogCrossoverInteractionLayout>();

            return BuildDensityNeutronCrossoverSegments(densityCurve, neutronCurve)
                .Select(segment => new LogCrossoverInteractionLayout(
                    densityCurve.Definition,
                    densityCurve.Metadata,
                    neutronCurve.Definition,
                    neutronCurve.Metadata,
                    ComputeBounds(segment.Polygon),
                    segment.Polygon,
                    segment.TopDepth,
                    segment.BottomDepth))
                .ToList();
        }

        private List<DensityNeutronCrossoverSegment> BuildDensityNeutronCrossoverSegments(CurveRenderState densityCurve, CurveRenderState neutronCurve)
        {
            var segments = new List<DensityNeutronCrossoverSegment>();
            var densitySamples = densityCurve.Samples.ToDictionary(sample => sample.Index, sample => sample);
            var neutronSamples = neutronCurve.Samples.ToDictionary(sample => sample.Index, sample => sample);
            var densitySegment = new List<CurveSample>();
            var neutronSegment = new List<CurveSample>();

            void FlushSegment()
            {
                if (densitySegment.Count < 2 || neutronSegment.Count < 2)
                {
                    densitySegment.Clear();
                    neutronSegment.Clear();
                    return;
                }

                var polygon = densitySegment
                    .Select(sample => sample.Point)
                    .Concat(neutronSegment.AsEnumerable().Reverse().Select(sample => sample.Point))
                    .ToList();

                int minimumIndex = Math.Min(densitySegment[0].Index, neutronSegment[0].Index);
                int maximumIndex = Math.Max(densitySegment[^1].Index, neutronSegment[^1].Index);
                segments.Add(new DensityNeutronCrossoverSegment(
                    polygon,
                    ResolveDepthAtIndex(minimumIndex),
                    ResolveDepthAtIndex(maximumIndex)));

                densitySegment.Clear();
                neutronSegment.Clear();
            }

            for (int index = 0; index < logData.Depths.Count; index++)
            {
                if (densitySamples.TryGetValue(index, out var densitySample) && neutronSamples.TryGetValue(index, out var neutronSample))
                {
                    densitySegment.Add(densitySample);
                    neutronSegment.Add(neutronSample);
                }
                else
                {
                    FlushSegment();
                }
            }

            FlushSegment();
            return segments;
        }

        private double ResolveDepthAtIndex(int index)
        {
            if (logData.Depths == null || logData.Depths.Count == 0)
                return 0;

            int safeIndex = Math.Clamp(index, 0, logData.Depths.Count - 1);
            return logData.Depths[safeIndex];
        }

        private static SKRect ComputeBounds(IReadOnlyList<SKPoint> points)
        {
            if (points == null || points.Count == 0)
                return SKRect.Empty;

            float left = points.Min(point => point.X);
            float top = points.Min(point => point.Y);
            float right = points.Max(point => point.X);
            float bottom = points.Max(point => point.Y);
            return new SKRect(left, top, right, bottom);
        }

        private double ResolveMinimumValue(LogTrackCurveDefinition curveDefinition, string curveName, List<double> values, bool useLogScale)
        {
            double minValue = ResolveDisplayMinimumValue(curveDefinition, curveName, values);

            return useLogScale ? Math.Log10(Math.Max(minValue, 0.01)) : minValue;
        }

        private double ResolveMaximumValue(LogTrackCurveDefinition curveDefinition, string curveName, List<double> values, bool useLogScale)
        {
            double maxValue = ResolveDisplayMaximumValue(curveDefinition, curveName, values);

            return useLogScale ? Math.Log10(Math.Max(maxValue, 0.01)) : maxValue;
        }

        private double ResolveDisplayMinimumValue(LogTrackCurveDefinition curveDefinition, string curveName, List<double> values)
        {
            return curveDefinition.MinValue
                ?? (configuration.MinValues.ContainsKey(curveName) && configuration.MinValues[curveName].HasValue
                    ? configuration.MinValues[curveName].Value
                    : values.Min());
        }

        private double ResolveDisplayMaximumValue(LogTrackCurveDefinition curveDefinition, string curveName, List<double> values)
        {
            return curveDefinition.MaxValue
                ?? (configuration.MaxValues.ContainsKey(curveName) && configuration.MaxValues[curveName].HasValue
                    ? configuration.MaxValues[curveName].Value
                    : values.Max());
        }

        private LogTrackScaleType ResolveScaleType(LogTrackCurveDefinition curveDefinition, LogCurveMetadata metadata, string curveName)
        {
            if (curveDefinition.ScaleType.HasValue)
                return curveDefinition.ScaleType.Value;

            return ShouldUseLogScale(curveName)
                ? LogTrackScaleType.Logarithmic
                : LogTrackScaleType.Linear;
        }

        private bool ShouldUseLogScale(string curveName)
        {
            return configuration.UseLogScaleForResistivity &&
                (curveName.ToUpper().Contains("RES") || curveName.ToUpper().Contains("RT") || curveName.ToUpper().Contains("RXO"));
        }

        private float GetTrackWidth(LogTrackDefinition track)
        {
            if (track.Width.HasValue)
                return track.Width.Value;

            return track.Kind == LogTrackKind.Depth
                ? configuration.DepthTrackWidth
                : configuration.TrackWidth;
        }

        private LogTrackDefinition CreateDefaultDepthTrack()
        {
            return new LogTrackDefinition
            {
                Kind = LogTrackKind.Depth,
                Name = "Depth",
                Width = configuration.DepthTrackWidth,
                MajorInterval = configuration.DepthInterval,
                MinorSubdivisionCount = 4,
                LabelFormat = "F0"
            };
        }

        private string GetDefaultTrackName(string curveName)
        {
            var metadata = GetMetadata(curveName);
            return metadata?.DisplayName ?? curveName;
        }

        private string GetCurveHeaderLabel(LogTrackCurveDefinition curveDefinition)
        {
            var metadata = GetMetadata(curveDefinition.CurveName);
            string label = curveDefinition.DisplayName ?? metadata?.DisplayName ?? curveDefinition.CurveName;
            return metadata?.Unit is { Length: > 0 }
                ? label + " (" + metadata.Unit + ")"
                : label;
        }

        private string ResolveValueFormat(LogTrackCurveDefinition curveDefinition, bool useLogScale)
        {
            if (!string.IsNullOrWhiteSpace(curveDefinition.ValueFormat))
                return curveDefinition.ValueFormat;

            return useLogScale ? "0.###" : "0.##";
        }

        private string ResolveScaleLabel(LogTrackCurveDefinition curveDefinition, LogCurveMetadata metadata, bool useLogScale)
        {
            string label = GetShortScaleLabel(curveDefinition, metadata);
            string unit = metadata?.Unit;
            string suffix = useLogScale ? " log" : string.Empty;

            if (!string.IsNullOrWhiteSpace(unit))
                return label + " (" + unit + ")" + suffix;

            return label + suffix;
        }

        private string GetShortScaleLabel(LogTrackCurveDefinition curveDefinition, LogCurveMetadata metadata)
        {
            return metadata?.Mnemonic
                ?? curveDefinition.DisplayName
                ?? metadata?.DisplayName
                ?? curveDefinition.CurveName;
        }

        private SKPaint CreateIntervalPaint(LogTrackKind trackKind, LogIntervalData interval)
        {
            SKColor baseColor = ResolveIntervalColor(trackKind, interval);

            if (trackKind == LogTrackKind.Lithology)
            {
                LithologyPattern pattern = ResolveIntervalPattern(interval);
                var paint = LithologyPatternRenderer.CreatePatternPaint(
                    baseColor,
                    pattern,
                    null,
                    configuration.IntervalPatternSize,
                    interval.Lithology,
                    useSvgPattern: true);

                if (!interval.IsPayZone && configuration.DimNonPayIntervals)
                {
                    paint.Color = new SKColor(paint.Color.Red, paint.Color.Green, paint.Color.Blue, (byte)(paint.Color.Alpha * 0.55f));
                }

                return paint;
            }

            var fillColor = interval.IsPayZone || !configuration.DimNonPayIntervals
                ? baseColor
                : new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, (byte)(baseColor.Alpha * 0.55f));

            return new SKPaint
            {
                Color = fillColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private SKColor ResolveIntervalColor(LogTrackKind trackKind, LogIntervalData interval)
        {
            if (!string.IsNullOrWhiteSpace(interval.ColorCode) && interval.ColorCode.StartsWith("#", StringComparison.Ordinal))
            {
                try
                {
                    return SKColor.Parse(interval.ColorCode);
                }
                catch
                {
                }
            }

            if (trackKind == LogTrackKind.Lithology)
                return LithologyColorPalette.GetLithologyColor(interval.Lithology);

            return interval.IsPayZone
                ? new SKColor(202, 224, 255)
                : new SKColor(232, 232, 232);
        }

        private LithologyPattern ResolveIntervalPattern(LogIntervalData interval)
        {
            if (!string.IsNullOrWhiteSpace(interval.PatternType) && Enum.TryParse<LithologyPattern>(interval.PatternType, out var explicitPattern))
                return explicitPattern;

            return LithologyColorPalette.GetLithologyPattern(interval.Lithology);
        }

        private void DrawIntervalLabel(SKCanvas canvas, LogTrackKind trackKind, LogIntervalData interval, SKRect rect)
        {
            string label = trackKind == LogTrackKind.Lithology
                ? BuildLithologyIntervalLabel(interval)
                : BuildZoneIntervalLabel(interval);

            if (string.IsNullOrWhiteSpace(label))
                return;

            using var textPaint = new SKPaint
            {
                Color = configuration.IntervalLabelColor,
                TextSize = configuration.IntervalLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            float centerY = rect.MidY + (configuration.IntervalLabelFontSize * 0.35f);
            canvas.Save();
            canvas.ClipRect(rect);
            canvas.DrawText(label, rect.MidX, centerY, textPaint);
            canvas.Restore();
        }

        private static string BuildLithologyIntervalLabel(LogIntervalData interval)
        {
            if (!string.IsNullOrWhiteSpace(interval.Lithology) && !string.IsNullOrWhiteSpace(interval.Facies))
                return interval.Lithology + " / " + interval.Facies;

            return interval.Lithology
                ?? interval.Facies
                ?? interval.Label;
        }

        private static string BuildZoneIntervalLabel(LogIntervalData interval)
        {
            return interval.Label
                ?? interval.Lithology
                ?? interval.Facies;
        }

        private TrackRenderContext PrepareTrackRenderContext(float height, float yOffset)
        {
            float headerHeight = configuration.ShowTrackHeaders ? configuration.TrackHeaderHeight : 0;
            float trackBodyY = yOffset + headerHeight;
            float trackBodyHeight = Math.Max(1.0f, height - headerHeight);
            var tracks = GetTracksToRender();

            depthSystem = new DepthTransform(logData.StartDepth, logData.EndDepth, trackBodyHeight);
            CalculateWellborePath();
            return new TrackRenderContext(tracks, trackBodyY, trackBodyHeight);
        }

        private sealed record CurveSample(int Index, SKPoint Point);

        private sealed record CurveRenderState(
            LogTrackCurveDefinition Definition,
            LogCurveMetadata Metadata,
            SKColor Color,
            bool UseLogScale,
            double DisplayMinimumValue,
            double DisplayMaximumValue,
            IReadOnlyList<CurveSample> Samples);

        private sealed record DensityNeutronCrossoverSegment(
            IReadOnlyList<SKPoint> Polygon,
            double TopDepth,
            double BottomDepth);

        private sealed record TrackRenderContext(
            IReadOnlyList<LogTrackDefinition> Tracks,
            float TrackBodyY,
            float TrackBodyHeight);

        private sealed class TrackScaleAnnotation
        {
            private readonly List<string> labels = new List<string>();

            public TrackScaleAnnotation(string label, string unit, string valueFormat, double minimumValue, double maximumValue, bool invertScale, bool useLogScale, SKColor color)
            {
                labels.Add(label);
                Unit = unit;
                ValueFormat = valueFormat;
                MinimumValue = minimumValue;
                MaximumValue = maximumValue;
                InvertScale = invertScale;
                UseLogScale = useLogScale;
                Color = color;
            }

            public string Unit { get; }
            public string ValueFormat { get; }
            public double MinimumValue { get; }
            public double MaximumValue { get; }
            public bool InvertScale { get; }
            public bool UseLogScale { get; }
            public SKColor Color { get; }

            public bool Matches(double minimumValue, double maximumValue, bool invertScale, bool useLogScale, string unit, string valueFormat)
            {
                return MinimumValue.Equals(minimumValue)
                    && MaximumValue.Equals(maximumValue)
                    && InvertScale == invertScale
                    && UseLogScale == useLogScale
                    && string.Equals(Unit, unit, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(ValueFormat, valueFormat, StringComparison.OrdinalIgnoreCase);
            }

            public void AppendLabel(string label)
            {
                if (!labels.Contains(label))
                    labels.Add(label);
            }

            public string BuildCenterLabel() => string.Join("/", labels);

            public string GetLeftValue() => FormatValue(InvertScale ? MaximumValue : MinimumValue);

            public string GetRightValue() => FormatValue(InvertScale ? MinimumValue : MaximumValue);

            private string FormatValue(double value) => value.ToString(ValueFormat);
        }
    }
}

