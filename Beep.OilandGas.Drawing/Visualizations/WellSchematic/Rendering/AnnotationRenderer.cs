using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    /// <summary>
    /// Renders annotations, depth scale, and grid for well schematics.
    /// </summary>
    public class AnnotationRenderer
    {
        private readonly CoordinateSystem depthSystem;
        private readonly WellSchematicConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnnotationRenderer"/> class.
        /// </summary>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="configuration">The rendering configuration.</param>
        public AnnotationRenderer(CoordinateSystem depthSystem, WellSchematicConfiguration configuration)
        {
            this.depthSystem = depthSystem ?? throw new ArgumentNullException(nameof(depthSystem));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Renders the depth scale.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="canvasWidth">The canvas width.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        public void RenderDepthScale(SKCanvas canvas, float canvasWidth, float canvasHeight, CoordinateSystem depthSystem)
        {
            float scaleWidth = configuration.DepthScaleWidth;
            float x = canvasWidth - scaleWidth;

            var paint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 1,
                IsAntialias = true
            };

            var textPaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = 12,
                IsAntialias = true,
                TextAlign = SKTextAlign.Right
            };

            // Draw scale line
            canvas.DrawLine(x, 0, x, canvasHeight, paint);

            // Draw tick marks and labels
            float minDepth = (float)depthSystem.MinValue;
            float maxDepth = (float)depthSystem.MaxValue;
            float depthRange = maxDepth - minDepth;
            int numTicks = 10;

            for (int i = 0; i <= numTicks; i++)
            {
                float depth = minDepth + (depthRange * i / numTicks);
                float y = depthSystem.ToScreenY(depth, canvasHeight);

                // Draw tick mark
                canvas.DrawLine(x - 5, y, x, y, paint);

                // Draw label
                string label = depth.ToString("F0");
                canvas.DrawText(label, x - 10, y + 5, textPaint);
            }
        }

        /// <summary>
        /// Renders the grid.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="canvasWidth">The canvas width.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="gridColor">The grid color.</param>
        public void RenderGrid(SKCanvas canvas, float canvasWidth, float canvasHeight, CoordinateSystem depthSystem, SKColor gridColor)
        {
            var paint = new SKPaint
            {
                Color = gridColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 0.5f,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            };

            float minDepth = (float)depthSystem.MinValue;
            float maxDepth = (float)depthSystem.MaxValue;
            float depthRange = maxDepth - minDepth;
            int numLines = 20;

            // Draw horizontal grid lines
            for (int i = 0; i <= numLines; i++)
            {
                float depth = minDepth + (depthRange * i / numLines);
                float y = depthSystem.ToScreenY(depth, canvasHeight);
                canvas.DrawLine(0, y, canvasWidth, y, paint);
            }

            // Draw vertical grid lines
            int numVerticalLines = 10;
            for (int i = 0; i <= numVerticalLines; i++)
            {
                float x = canvasWidth * i / numVerticalLines;
                canvas.DrawLine(x, 0, x, canvasHeight, paint);
            }
        }

        /// <summary>
        /// Renders borehole labels.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="borehole">The borehole data.</param>
        /// <param name="index">The borehole index.</param>
        /// <param name="centerX">The center X coordinate.</param>
        /// <param name="depthSystem">The depth coordinate system.</param>
        public void RenderBoreholeLabels(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, CoordinateSystem depthSystem)
        {
            var textPaint = new SKPaint
            {
                Color = configuration.Theme.ForegroundColor,
                TextSize = 14,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                FakeBoldText = true
            };

            // Render borehole name/label if available
            // Use UBHI or generate label from index
            string boreholeLabel = !string.IsNullOrEmpty(borehole.UBHI) ? borehole.UBHI : $"Borehole {index + 1}";
            float labelY = depthSystem.ToScreenY(borehole.TopDepth, canvas.DeviceClipBounds.Height) - 10;
            canvas.DrawText(boreholeLabel, centerX, labelY, textPaint);
        }
    }
}

