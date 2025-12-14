using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Styling;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.Validation;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic
{
    /// <summary>
    /// Layer for rendering well schematics.
    /// </summary>
    public class WellSchematicLayer : LayerBase
    {
        private readonly WellData wellData;
        private readonly Theme theme;
        private readonly CoordinateSystem depthSystem;

        /// <summary>
        /// Gets or sets the wellbore stroke width.
        /// </summary>
        public float WellboreStrokeWidth { get; set; } = 2.0f;

        /// <summary>
        /// Gets or sets the casing stroke width.
        /// </summary>
        public float CasingStrokeWidth { get; set; } = 1.5f;

        /// <summary>
        /// Gets or sets the tubing stroke width.
        /// </summary>
        public float TubingStrokeWidth { get; set; } = 1.0f;

        /// <summary>
        /// Initializes a new instance of the <see cref="WellSchematicLayer"/> class.
        /// </summary>
        /// <param name="wellData">The well data to render.</param>
        /// <param name="theme">The theme to use for rendering.</param>
        public WellSchematicLayer(WellData wellData, Theme theme = null)
            : base("Well Schematic")
        {
            if (wellData == null)
                throw new ArgumentNullException(nameof(wellData));

            this.wellData = wellData;
            this.theme = theme ?? Theme.Standard;

            // Validate well data
            WellDataValidator.ValidateWellData(wellData);

            // Create depth coordinate system
            if (wellData.BoreHoles != null && wellData.BoreHoles.Count > 0)
            {
                float minDepth = wellData.BoreHoles.Min(b => b.TopDepth);
                float maxDepth = wellData.BoreHoles.Max(b => b.BottomDepth);
                depthSystem = CoordinateSystem.CreateDepthSystem(minDepth, maxDepth, "feet");
            }
        }

        /// <summary>
        /// Renders the well schematic content.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="viewport">The viewport for transformations.</param>
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            if (wellData.BoreHoles == null || wellData.BoreHoles.Count == 0)
                return;

            // Apply viewport transformation
            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                // Render each borehole
                for (int i = 0; i < wellData.BoreHoles.Count; i++)
                {
                    RenderBorehole(canvas, wellData.BoreHoles[i], i);
                }
            }
            finally
            {
                canvas.Restore();
            }
        }

        /// <summary>
        /// Renders a single borehole.
        /// </summary>
        private void RenderBorehole(SKCanvas canvas, WellData_Borehole borehole, int index)
        {
            float centerX = canvas.DeviceClipBounds.Width / 2.0f;
            float topY = depthSystem.ToScreenY(borehole.TopDepth, canvas.DeviceClipBounds.Height);
            float bottomY = depthSystem.ToScreenY(borehole.BottomDepth, canvas.DeviceClipBounds.Height);
            float radius = borehole.Diameter / 2.0f;

            // Render wellbore
            var wellborePaint = new SKPaint
            {
                Color = ColorPalette.WellSchematic.Wellbore,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = WellboreStrokeWidth,
                IsAntialias = true
            };

            if (borehole.IsVertical)
            {
                // Vertical wellbore
                var path = new SKPath();
                path.AddRect(new SKRect(
                    centerX - radius,
                    topY,
                    centerX + radius,
                    bottomY));
                canvas.DrawPath(path, wellborePaint);
            }
            else
            {
                // Curved wellbore - simplified for now
                // TODO: Implement curved wellbore rendering
                var path = new SKPath();
                path.AddRect(new SKRect(
                    centerX - radius,
                    topY,
                    centerX + radius,
                    bottomY));
                canvas.DrawPath(path, wellborePaint);
            }

            // Render casing
            if (borehole.Casing != null)
            {
                foreach (var casing in borehole.Casing)
                {
                    RenderCasing(canvas, casing, centerX);
                }
            }

            // Render tubing
            if (borehole.Tubing != null)
            {
                foreach (var tubing in borehole.Tubing)
                {
                    RenderTubing(canvas, tubing, centerX);
                }
            }

            // Render equipment
            if (borehole.Equip != null)
            {
                foreach (var equipment in borehole.Equip)
                {
                    RenderEquipment(canvas, equipment, centerX);
                }
            }

            // Render perforations
            if (borehole.Perforation != null)
            {
                foreach (var perf in borehole.Perforation)
                {
                    RenderPerforation(canvas, perf, centerX);
                }
            }
        }

        /// <summary>
        /// Renders casing.
        /// </summary>
        private void RenderCasing(SKCanvas canvas, WellData_Casing casing, float centerX)
        {
            float topY = depthSystem.ToScreenY(casing.TopDepth, canvas.DeviceClipBounds.Height);
            float bottomY = depthSystem.ToScreenY(casing.BottomDepth, canvas.DeviceClipBounds.Height);
            float radius = casing.OUTER_DIAMETER / 2.0f;

            var paint = new SKPaint
            {
                Color = ColorPalette.WellSchematic.Casing,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = CasingStrokeWidth,
                IsAntialias = true
            };

            var path = new SKPath();
            path.AddRect(new SKRect(
                centerX - radius,
                topY,
                centerX + radius,
                bottomY));
            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Renders tubing.
        /// </summary>
        private void RenderTubing(SKCanvas canvas, WellData_Tubing tubing, float centerX)
        {
            float topY = depthSystem.ToScreenY(tubing.TopDepth, canvas.DeviceClipBounds.Height);
            float bottomY = depthSystem.ToScreenY(tubing.BottomDepth, canvas.DeviceClipBounds.Height);
            float radius = tubing.Diameter / 2.0f;

            var paint = new SKPaint
            {
                Color = ColorPalette.WellSchematic.Tubing,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = TubingStrokeWidth,
                IsAntialias = true
            };

            var path = new SKPath();
            path.AddRect(new SKRect(
                centerX - radius,
                topY,
                centerX + radius,
                bottomY));
            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Renders equipment.
        /// </summary>
        private void RenderEquipment(SKCanvas canvas, WellData_Equip equipment, float centerX)
        {
            float topY = depthSystem.ToScreenY(equipment.TopDepth, canvas.DeviceClipBounds.Height);
            float bottomY = depthSystem.ToScreenY(equipment.BottomDepth, canvas.DeviceClipBounds.Height);
            float radius = equipment.Diameter / 2.0f;

            var paint = new SKPaint
            {
                Color = ColorPalette.WellSchematic.Equipment,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var rect = new SKRect(
                centerX - radius,
                topY,
                centerX + radius,
                bottomY);
            canvas.DrawRect(rect, paint);
        }

        /// <summary>
        /// Renders perforations.
        /// </summary>
        private void RenderPerforation(SKCanvas canvas, WellData_Perf perforation, float centerX)
        {
            float topY = depthSystem.ToScreenY(perforation.TopDepth, canvas.DeviceClipBounds.Height);
            float bottomY = depthSystem.ToScreenY(perforation.BottomDepth, canvas.DeviceClipBounds.Height);

            var paint = new SKPaint
            {
                Color = ColorPalette.WellSchematic.Perforation,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            // Draw perforation marks
            float markWidth = 5.0f;
            var rect = new SKRect(
                centerX - markWidth,
                topY,
                centerX + markWidth,
                bottomY);
            canvas.DrawRect(rect, paint);
        }

        /// <summary>
        /// Gets the bounding rectangle of the well schematic.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        public override SKRect GetBounds()
        {
            if (wellData.BoreHoles == null || wellData.BoreHoles.Count == 0)
                return new SKRect(0, 0, 0, 0);

            float minDepth = wellData.BoreHoles.Min(b => b.TopDepth);
            float maxDepth = wellData.BoreHoles.Max(b => b.BottomDepth);
            float maxDiameter = wellData.BoreHoles.Max(b => b.Diameter);

            return new SKRect(
                -maxDiameter / 2.0f,
                minDepth,
                maxDiameter / 2.0f,
                maxDepth);
        }
    }
}

