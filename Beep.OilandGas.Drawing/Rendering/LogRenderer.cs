using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;

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
        private DepthCoordinateSystem depthSystem;
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

            // Create depth coordinate system
            depthSystem = new DepthCoordinateSystem(
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

            // Update depth system with actual canvas height (recreate if needed)
            if (depthSystem == null || Math.Abs(depthSystem.MaxValue - logData.EndDepth) > 0.1)
            {
                depthSystem = new DepthCoordinateSystem(logData.StartDepth, logData.EndDepth, height);
            }

            // Recalculate path with updated depth system
            CalculateWellborePath();

            // Draw wellbore path if enabled
            if (configuration.ShowWellborePath)
            {
                DrawWellborePath(canvas, xOffset, yOffset);
            }

            // Draw depth markers if enabled
            if (configuration.ShowDepthMarkers)
            {
                DrawDepthMarkers(canvas, xOffset, yOffset, width);
            }

            // Draw log tracks
            float currentX = xOffset;
            foreach (var curveName in logData.Curves.Keys)
            {
                DrawLogTrack(canvas, curveName, currentX, yOffset, width, height);
                currentX += configuration.TrackWidth + configuration.TrackSpacing;
            }

            // Draw depth scale if enabled
            if (configuration.ShowDepthScale)
            {
                DrawDepthScale(canvas, xOffset, yOffset, height);
            }
        }

        /// <summary>
        /// Draws a single log track.
        /// </summary>
        private void DrawLogTrack(SKCanvas canvas, string curveName, float x, float y, float width, float height)
        {
            if (!logData.Curves.ContainsKey(curveName))
                return;

            var curveValues = logData.Curves[curveName];
            if (curveValues == null || curveValues.Count == 0 || logData.Depths == null || logData.Depths.Count == 0)
                return;

            // Get curve metadata
            var metadata = GetMetadata(curveName);

            // Get curve color
            SKColor curveColor = configuration.DefaultCurveColors.ContainsKey(curveName)
                ? configuration.DefaultCurveColors[curveName]
                : SKColors.Black;

            // Get min/max values
            double minValue = configuration.MinValues.ContainsKey(curveName) && configuration.MinValues[curveName].HasValue
                ? configuration.MinValues[curveName].Value
                : curveValues.Min();
            double maxValue = configuration.MaxValues.ContainsKey(curveName) && configuration.MaxValues[curveName].HasValue
                ? configuration.MaxValues[curveName].Value
                : curveValues.Max();

            // Handle logarithmic scale for resistivity
            if (configuration.UseLogScaleForResistivity && 
                (curveName.ToUpper().Contains("RES") || curveName.ToUpper().Contains("RT") || curveName.ToUpper().Contains("RXO")))
            {
                minValue = Math.Log10(Math.Max(minValue, 0.01));
                maxValue = Math.Log10(Math.Max(maxValue, 0.01));
            }

            // Draw track background
            using (var bgPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill
            })
            {
                canvas.DrawRect(x, y, configuration.TrackWidth, height, bgPaint);
            }

            // Draw grid if enabled
            if (configuration.ShowGrid)
            {
                DrawTrackGrid(canvas, x, y, height);
            }

            // Draw curve
            DrawCurve(canvas, curveName, curveValues, logData.Depths, x, y, height, 
                minValue, maxValue, curveColor);

            // Draw curve name if enabled
            if (configuration.ShowCurveNames)
            {
                DrawCurveName(canvas, curveName, metadata, x, y);
            }
        }

        /// <summary>
        /// Draws a log curve along the wellbore path.
        /// </summary>
        private void DrawCurve(
            SKCanvas canvas,
            string curveName,
            List<double> values,
            List<double> depths,
            float trackX,
            float trackY,
            float trackHeight,
            double minValue,
            double maxValue,
            SKColor color)
        {
            if (values.Count != depths.Count || values.Count == 0)
                return;

            var path = new SKPath();
            bool firstPoint = true;

            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i];
                double depth = depths[i];

                // Handle null values
                var curveMetadata = logData.CurveMetadata.ContainsKey(curveName) 
                    ? logData.CurveMetadata[curveName] 
                    : null;
                if (curveMetadata != null && curveMetadata.NullValue.HasValue && 
                    Math.Abs(value - curveMetadata.NullValue.Value) < 0.001)
                    continue;

                // Normalize value to track width
                double normalizedValue = (value - minValue) / (maxValue - minValue);
                if (double.IsNaN(normalizedValue) || double.IsInfinity(normalizedValue))
                    normalizedValue = 0.5;

                // Handle logarithmic scale
                if (configuration.UseLogScaleForResistivity && 
                    (curveName.ToUpper().Contains("RES") || curveName.ToUpper().Contains("RT") || curveName.ToUpper().Contains("RXO")))
                {
                    value = Math.Log10(Math.Max(value, 0.01));
                    normalizedValue = (value - minValue) / (maxValue - minValue);
                }

                // Get position along wellbore path
                SKPoint pathPoint = GetPointAtDepth(depth);
                if (pathPoint == SKPoint.Empty)
                    continue;

                // Calculate curve position (perpendicular to wellbore)
                float curveX = trackX + (float)(normalizedValue * configuration.TrackWidth);
                float curveY = pathPoint.Y;

                if (firstPoint)
                {
                    path.MoveTo(curveX, curveY);
                    firstPoint = false;
                }
                else
                {
                    path.LineTo(curveX, curveY);
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
                    fillPath.LineTo(trackX + configuration.TrackWidth, lastPoint.Y);
                    fillPath.LineTo(trackX + configuration.TrackWidth, firstPoint2.Y);
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
        private void DrawTrackGrid(SKCanvas canvas, float x, float y, float height)
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
                    float gridX = x + (configuration.TrackWidth * i / gridLines);
                    canvas.DrawLine(gridX, y, gridX, y + height, gridPaint);
                }

                // Horizontal grid lines (depth scale)
                double depthRange = logData.EndDepth - logData.StartDepth;
                int depthLines = (int)(depthRange / configuration.DepthInterval);
                for (int i = 0; i <= depthLines; i++)
                {
                    double depth = logData.StartDepth + (depthRange * i / depthLines);
                    float gridY = depthSystem.ToScreenY((float)depth, null);
                    canvas.DrawLine(x, gridY, x + configuration.TrackWidth, gridY, gridPaint);
                }
            }
        }

        /// <summary>
        /// Draws the curve name label.
        /// </summary>
        private void DrawCurveName(SKCanvas canvas, string curveName, LogCurveMetadata metadata, float x, float y)
        {
            string displayName = metadata?.DisplayName ?? curveName;
            string unit = metadata?.Unit != null ? $" ({metadata.Unit})" : "";

            using (var textPaint = new SKPaint
            {
                Color = configuration.CurveNameColor,
                TextSize = configuration.CurveNameFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                canvas.DrawText(displayName + unit, x + configuration.TrackWidth / 2, y + 15, textPaint);
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
                int markerCount = (int)(depthRange / configuration.DepthInterval);

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
                int scaleCount = (int)(depthRange / configuration.DepthInterval);

                for (int i = 0; i <= scaleCount; i++)
                {
                    double depth = logData.StartDepth + (depthRange * i / scaleCount);
                    float y = depthSystem.ToScreenY((float)depth, null) + yOffset;
                    canvas.DrawText(depth.ToString("F0"), xOffset - 5, y + 5, scalePaint);
                }
            }
        }

        private LogCurveMetadata metadata => logData.CurveMetadata.ContainsKey("") ? null : null; // Placeholder
    }
}

