using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Renders multiple wells with multiple boreholes (supports vertical and directional/deviated wells).
    /// </summary>
    public class WellRenderer
    {
        private readonly List<WellData> wells;
        private readonly Dictionary<string, DeviationSurvey> deviationSurveys; // Well UWI + Borehole ID -> Survey
        private readonly WellRendererConfiguration configuration;
        private readonly Dictionary<string, DepthCoordinateSystem> depthSystems;
        private readonly Dictionary<string, List<SKPoint>> wellborePaths;

        /// <summary>
        /// Initializes a new instance of the <see cref="WellRenderer"/> class.
        /// </summary>
        /// <param name="wells">List of wells to render.</param>
        /// <param name="deviationSurveys">Dictionary mapping well+borehole identifiers to deviation surveys.</param>
        /// <param name="configuration">Rendering configuration.</param>
        public WellRenderer(
            List<WellData> wells,
            Dictionary<string, DeviationSurvey> deviationSurveys = null,
            WellRendererConfiguration configuration = null)
        {
            this.wells = wells ?? throw new ArgumentNullException(nameof(wells));
            this.deviationSurveys = deviationSurveys ?? new Dictionary<string, DeviationSurvey>();
            this.configuration = configuration ?? new WellRendererConfiguration();

            depthSystems = new Dictionary<string, DepthCoordinateSystem>();
            wellborePaths = new Dictionary<string, List<SKPoint>>();

            Initialize();
        }

        /// <summary>
        /// Initializes depth systems and wellbore paths.
        /// </summary>
        private void Initialize()
        {
            // Calculate overall depth range
            double minDepth = double.MaxValue;
            double maxDepth = double.MinValue;

            foreach (var well in wells)
            {
                if (well.BoreHoles != null)
                {
                    foreach (var borehole in well.BoreHoles)
                    {
                        minDepth = Math.Min(minDepth, borehole.TopDepth);
                        maxDepth = Math.Max(maxDepth, borehole.BottomDepth);
                    }
                }
            }

            if (minDepth >= maxDepth)
            {
                minDepth = 0;
                maxDepth = 10000;
            }

            // Create depth systems and paths for each well/borehole
            foreach (var well in wells)
            {
                if (well.BoreHoles == null)
                    continue;

                foreach (var borehole in well.BoreHoles)
                {
                    string key = GetWellBoreholeKey(well.UWI, borehole.UBHI ?? borehole.GuidID);
                    
                    // Create depth system for this borehole
                    var depthSystem = new DepthCoordinateSystem(
                        Math.Min(borehole.TopDepth, minDepth),
                        Math.Max(borehole.BottomDepth, maxDepth),
                        1000f); // Default, will be updated on render
                    depthSystems[key] = depthSystem;

                    // Calculate wellbore path
                    string surveyKey = $"{well.UWI}_{borehole.UBHI ?? borehole.GuidID}";
                    DeviationSurvey survey = deviationSurveys.ContainsKey(surveyKey) 
                        ? deviationSurveys[surveyKey] 
                        : null;

                    if (survey != null && survey.SurveyPoints != null && survey.SurveyPoints.Count > 0)
                    {
                        // Directional/deviated well
                        wellborePaths[key] = WellborePathCalculator.CalculatePath(
                            survey,
                            depthSystem,
                            configuration.HorizontalStretchFactor);
                    }
                    else if (borehole.IsVertical)
                    {
                        // Vertical well
                        wellborePaths[key] = WellborePathCalculator.CalculateVerticalPath(
                            borehole.TopDepth,
                            borehole.BottomDepth,
                            depthSystem);
                    }
                    else
                    {
                        // Assume horizontal section exists
                        double verticalDepth = borehole.TopDepth;
                        double horizontalLength = borehole.BottomDepth - borehole.TopDepth; // Simplified
                        wellborePaths[key] = WellborePathCalculator.CalculateVerticalHorizontalPath(
                            borehole.TopDepth,
                            verticalDepth,
                            horizontalLength,
                            depthSystem,
                            0,
                            configuration.HorizontalStretchFactor,
                            configuration.HorizontalAlignment);
                    }
                }
            }
        }

        /// <summary>
        /// Gets a unique key for well+borehole combination.
        /// </summary>
        private string GetWellBoreholeKey(string wellUWI, string boreholeID)
        {
            return $"{wellUWI}_{boreholeID}";
        }

        /// <summary>
        /// Renders all wells and boreholes.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render on.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        public void Render(SKCanvas canvas, float width, float height)
        {
            if (canvas == null || wells == null || wells.Count == 0)
                return;

            // Clear background
            canvas.Clear(configuration.BackgroundColor);

            // Draw grid if enabled
            if (configuration.ShowGrid)
            {
                DrawGrid(canvas, width, height);
            }

            // Calculate positions for wells
            float currentX = configuration.LeftMargin;
            float wellSpacing = configuration.WellSpacing;

            foreach (var well in wells)
            {
                if (well.BoreHoles == null || well.BoreHoles.Count == 0)
                    continue;

                // Render all boreholes for this well
                float boreholeX = currentX;
                foreach (var borehole in well.BoreHoles)
                {
                    RenderBorehole(canvas, well, borehole, boreholeX, height);
                    
                    // Move to next borehole position
                    boreholeX += configuration.BoreholeSpacing;
                }

                // Draw well label if enabled
                if (configuration.ShowWellLabels)
                {
                    DrawWellLabel(canvas, well, currentX, height);
                }

                // Move to next well position
                currentX += wellSpacing;
            }

            // Draw depth scale if enabled
            if (configuration.ShowDepthScale)
            {
                DrawDepthScale(canvas, width, height);
            }
        }

        /// <summary>
        /// Renders a single borehole.
        /// </summary>
        private void RenderBorehole(
            SKCanvas canvas,
            WellData well,
            WellData_Borehole borehole,
            float xPosition,
            float canvasHeight)
        {
            string key = GetWellBoreholeKey(well.UWI, borehole.UBHI ?? borehole.GuidID);

            if (!wellborePaths.ContainsKey(key) || !depthSystems.ContainsKey(key))
                return;

            var path = wellborePaths[key];
            var depthSystem = depthSystems[key];

            // Update depth system with actual canvas height
            depthSystem = new DepthCoordinateSystem(
                depthSystem.MinValue,
                depthSystem.MaxValue,
                canvasHeight);

            // Recalculate path if needed
            string surveyKey = $"{well.UWI}_{borehole.UBHI ?? borehole.GuidID}";
            DeviationSurvey survey = deviationSurveys.ContainsKey(surveyKey) 
                ? deviationSurveys[surveyKey] 
                : null;

            if (survey != null && survey.SurveyPoints != null && survey.SurveyPoints.Count > 0)
            {
                path = WellborePathCalculator.CalculatePath(survey, depthSystem, configuration.HorizontalStretchFactor);
            }

            // Draw wellbore path
            DrawWellborePath(canvas, path, xPosition, well, borehole);

            // Draw casing if enabled
            if (configuration.ShowCasing && borehole.Casing != null)
            {
                DrawCasing(canvas, borehole, path, xPosition, depthSystem);
            }

            // Draw tubing if enabled
            if (configuration.ShowTubing && borehole.Tubing != null)
            {
                DrawTubing(canvas, borehole, path, xPosition, depthSystem);
            }

            // Draw perforations if enabled
            if (configuration.ShowPerforations && borehole.Perforation != null)
            {
                DrawPerforations(canvas, borehole, path, xPosition, depthSystem);
            }

            // Draw equipment if enabled
            if (configuration.ShowEquipment && borehole.Equip != null)
            {
                DrawEquipment(canvas, borehole, path, xPosition, depthSystem);
            }

            // Draw borehole label if enabled
            if (configuration.ShowBoreholeLabels)
            {
                DrawBoreholeLabel(canvas, borehole, xPosition, depthSystem, canvasHeight);
            }
        }

        /// <summary>
        /// Draws the wellbore path.
        /// </summary>
        private void DrawWellborePath(
            SKCanvas canvas,
            List<SKPoint> path,
            float xOffset,
            WellData well,
            WellData_Borehole borehole)
        {
            if (path == null || path.Count < 2)
                return;

            // Get well color
            SKColor pathColor = configuration.WellColors.ContainsKey(well.UWI)
                ? configuration.WellColors[well.UWI]
                : configuration.WellborePathColor;

            using (var pathObj = new SKPath())
            {
                pathObj.MoveTo(path[0].X + xOffset, path[0].Y);
                for (int i = 1; i < path.Count; i++)
                {
                    pathObj.LineTo(path[i].X + xOffset, path[i].Y);
                }

                using (var paint = new SKPaint
                {
                    Color = pathColor,
                    StrokeWidth = configuration.WellborePathWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawPath(pathObj, paint);
                }
            }
        }

        /// <summary>
        /// Draws casing strings.
        /// </summary>
        private void DrawCasing(
            SKCanvas canvas,
            WellData_Borehole borehole,
            List<SKPoint> wellborePath,
            float xOffset,
            DepthCoordinateSystem depthSystem)
        {
            if (borehole.Casing == null)
                return;

            foreach (var casing in borehole.Casing)
            {
                // Find path points for casing depth range
                float topY = depthSystem.ToScreenY(casing.TopDepth, null);
                float bottomY = depthSystem.ToScreenY(casing.BottomDepth, null);

                // Draw casing as rectangle around wellbore path
                float casingWidth = configuration.CasingWidth;
                float halfWidth = casingWidth / 2;

                using (var paint = new SKPaint
                {
                    Color = configuration.CasingColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    // For vertical sections, draw rectangle
                    if (wellborePath.Count == 2) // Simple vertical path
                    {
                        canvas.DrawRect(
                            xOffset - halfWidth,
                            topY,
                            casingWidth,
                            bottomY - topY,
                            paint);
                    }
                    else
                    {
                        // For directional paths, draw along path
                        DrawCasingAlongPath(canvas, wellborePath, casing, xOffset, halfWidth, depthSystem);
                    }
                }
            }
        }

        /// <summary>
        /// Draws casing along a directional wellbore path.
        /// </summary>
        private void DrawCasingAlongPath(
            SKCanvas canvas,
            List<SKPoint> path,
            WellData_Casing casing,
            float xOffset,
            float halfWidth,
            DepthCoordinateSystem depthSystem)
        {
            // Find path segment for casing depth range
            float topY = depthSystem.ToScreenY(casing.TopDepth, null);
            float bottomY = depthSystem.ToScreenY(casing.BottomDepth, null);

            // Draw casing as parallel lines along path
            using (var paint = new SKPaint
            {
                Color = configuration.CasingColor,
                StrokeWidth = configuration.CasingWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                // Simplified - draw along path segments within depth range
                for (int i = 0; i < path.Count - 1; i++)
                {
                    SKPoint p1 = path[i];
                    SKPoint p2 = path[i + 1];

                    // Check if segment is within casing depth range
                    if ((p1.Y >= topY && p1.Y <= bottomY) || (p2.Y >= topY && p2.Y <= bottomY))
                    {
                        canvas.DrawLine(
                            p1.X + xOffset - halfWidth, p1.Y,
                            p2.X + xOffset - halfWidth, p2.Y,
                            paint);
                        canvas.DrawLine(
                            p1.X + xOffset + halfWidth, p1.Y,
                            p2.X + xOffset + halfWidth, p2.Y,
                            paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws tubing strings.
        /// </summary>
        private void DrawTubing(
            SKCanvas canvas,
            WellData_Borehole borehole,
            List<SKPoint> wellborePath,
            float xOffset,
            DepthCoordinateSystem depthSystem)
        {
            if (borehole.Tubing == null)
                return;

            foreach (var tubing in borehole.Tubing)
            {
                float topY = depthSystem.ToScreenY(tubing.TopDepth, null);
                float bottomY = depthSystem.ToScreenY(tubing.BottomDepth, null);

                using (var paint = new SKPaint
                {
                    Color = configuration.TubingColor,
                    StrokeWidth = configuration.TubingWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    if (wellborePath.Count == 2) // Vertical
                    {
                        canvas.DrawLine(xOffset, topY, xOffset, bottomY, paint);
                    }
                    else
                    {
                        // Draw along path
                        DrawTubingAlongPath(canvas, wellborePath, tubing, xOffset, depthSystem);
                    }
                }
            }
        }

        /// <summary>
        /// Draws tubing along a directional wellbore path.
        /// </summary>
        private void DrawTubingAlongPath(
            SKCanvas canvas,
            List<SKPoint> path,
            WellData_Tubing tubing,
            float xOffset,
            DepthCoordinateSystem depthSystem)
        {
            float topY = depthSystem.ToScreenY(tubing.TopDepth, null);
            float bottomY = depthSystem.ToScreenY(tubing.BottomDepth, null);

            using (var paint = new SKPaint
            {
                Color = configuration.TubingColor,
                StrokeWidth = configuration.TubingWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    SKPoint p1 = path[i];
                    SKPoint p2 = path[i + 1];

                    if ((p1.Y >= topY && p1.Y <= bottomY) || (p2.Y >= topY && p2.Y <= bottomY))
                    {
                        canvas.DrawLine(
                            p1.X + xOffset, p1.Y,
                            p2.X + xOffset, p2.Y,
                            paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws perforations.
        /// </summary>
        private void DrawPerforations(
            SKCanvas canvas,
            WellData_Borehole borehole,
            List<SKPoint> wellborePath,
            float xOffset,
            DepthCoordinateSystem depthSystem)
        {
            if (borehole.Perforation == null)
                return;

            foreach (var perf in borehole.Perforation)
            {
                float perfY = depthSystem.ToScreenY(perf.TopDepth, null);

                // Find point on path at this depth
                SKPoint perfPoint = GetPointAtDepth(wellborePath, perf.TopDepth, depthSystem);
                if (perfPoint == SKPoint.Empty)
                    continue;

                using (var paint = new SKPaint
                {
                    Color = configuration.PerforationColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawCircle(perfPoint.X + xOffset, perfPoint.Y, configuration.PerforationSize, paint);
                }
            }
        }

        /// <summary>
        /// Draws equipment.
        /// </summary>
        private void DrawEquipment(
            SKCanvas canvas,
            WellData_Borehole borehole,
            List<SKPoint> wellborePath,
            float xOffset,
            DepthCoordinateSystem depthSystem)
        {
            if (borehole.Equip == null)
                return;

            foreach (var equip in borehole.Equip)
            {
                float equipY = depthSystem.ToScreenY(equip.TopDepth, null);
                SKPoint equipPoint = GetPointAtDepth(wellborePath, equip.TopDepth, depthSystem);
                if (equipPoint == SKPoint.Empty)
                    continue;

                // Draw equipment as rectangle
                float equipHeight = depthSystem.ToScreenY(equip.BottomDepth, null) - equipY;
                float equipWidth = 20f;

                using (var paint = new SKPaint
                {
                    Color = SKColors.Gray,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(
                        equipPoint.X + xOffset - equipWidth / 2,
                        equipY,
                        equipWidth,
                        equipHeight,
                        paint);
                }
            }
        }

        /// <summary>
        /// Gets a point on the wellbore path at a specific depth.
        /// </summary>
        private SKPoint GetPointAtDepth(List<SKPoint> path, double depth, DepthCoordinateSystem depthSystem)
        {
            if (path == null || path.Count == 0)
                return SKPoint.Empty;

            float targetY = depthSystem.ToScreenY((float)depth, null);

            // Find closest point
            SKPoint closest = path[0];
            float minDist = Math.Abs(path[0].Y - targetY);

            foreach (var point in path)
            {
                float dist = Math.Abs(point.Y - targetY);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = point;
                }
            }

            return closest;
        }

        /// <summary>
        /// Draws well label.
        /// </summary>
        private void DrawWellLabel(SKCanvas canvas, WellData well, float x, float height)
        {
            using (var paint = new SKPaint
            {
                Color = configuration.WellLabelColor,
                TextSize = configuration.WellLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                string label = !string.IsNullOrEmpty(well.UWI) ? well.UWI : well.GuidID;
                canvas.DrawText(label, x, height - 10, paint);
            }
        }

        /// <summary>
        /// Draws borehole label.
        /// </summary>
        private void DrawBoreholeLabel(
            SKCanvas canvas,
            WellData_Borehole borehole,
            float x,
            DepthCoordinateSystem depthSystem,
            float height)
        {
            using (var paint = new SKPaint
            {
                Color = configuration.WellLabelColor,
                TextSize = configuration.BoreholeLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            })
            {
                string label = !string.IsNullOrEmpty(borehole.UBHI) ? borehole.UBHI : $"BH{borehole.BoreHoleIndex}";
                float labelY = depthSystem.ToScreenY(borehole.TopDepth, null);
                canvas.DrawText(label, x, labelY - 5, paint);
            }
        }

        /// <summary>
        /// Draws grid lines.
        /// </summary>
        private void DrawGrid(SKCanvas canvas, float width, float height)
        {
            using (var paint = new SKPaint
            {
                Color = configuration.GridColor,
                StrokeWidth = configuration.GridLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                // Vertical grid lines
                for (float x = configuration.LeftMargin; x < width; x += 50)
                {
                    canvas.DrawLine(x, 0, x, height, paint);
                }

                // Horizontal grid lines (depth-based)
                if (depthSystems.Count > 0)
                {
                    var firstSystem = depthSystems.Values.First();
                    double depthRange = firstSystem.MaxValue - firstSystem.MinValue;
                    int gridLines = (int)(depthRange / configuration.DepthInterval);

                    for (int i = 0; i <= gridLines; i++)
                    {
                        double depth = firstSystem.MinValue + (depthRange * i / gridLines);
                        float y = firstSystem.ToScreenY((float)depth, null);
                        canvas.DrawLine(0, y, width, y, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws depth scale.
        /// </summary>
        private void DrawDepthScale(SKCanvas canvas, float width, float height)
        {
            if (depthSystems.Count == 0)
                return;

            var firstSystem = depthSystems.Values.First();
            double depthRange = firstSystem.MaxValue - firstSystem.MinValue;
            int scaleCount = (int)(depthRange / configuration.DepthInterval);

            using (var paint = new SKPaint
            {
                Color = configuration.DepthScaleColor,
                TextSize = configuration.DepthScaleFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            })
            {
                for (int i = 0; i <= scaleCount; i++)
                {
                    double depth = firstSystem.MinValue + (depthRange * i / scaleCount);
                    float y = firstSystem.ToScreenY((float)depth, null);
                    canvas.DrawText(depth.ToString("F0"), configuration.LeftMargin - 5, y + 5, paint);
                }
            }
        }
    }
}

