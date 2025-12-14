using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Styling;
using Beep.OilandGas.Drawing.CoordinateSystems;
using DepthCoordinateSystem = Beep.OilandGas.Drawing.CoordinateSystems.DepthCoordinateSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Layer for rendering reservoir layers with lithology colors and patterns.
    /// </summary>
    public class ReservoirLayer : LayerBase
    {
        private List<LayerData> layers;
        private FluidContacts fluidContacts;
        private ReservoirLayerConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReservoirLayer"/> class.
        /// </summary>
        /// <param name="layers">The layer data to render.</param>
        /// <param name="fluidContacts">Optional fluid contacts.</param>
        /// <param name="configuration">Optional configuration.</param>
        public ReservoirLayer(
            List<LayerData> layers,
            FluidContacts fluidContacts = null,
            ReservoirLayerConfiguration configuration = null)
            : base("Reservoir Layer")
        {
            this.layers = layers ?? new List<LayerData>();
            this.fluidContacts = fluidContacts;
            this.configuration = configuration ?? new ReservoirLayerConfiguration();
        }

        /// <summary>
        /// Renders the reservoir layer content.
        /// </summary>
        protected override void RenderContent(SKCanvas canvas, Viewport viewport)
        {
            if (layers == null || layers.Count == 0)
                return;

            // Create depth coordinate system
            double minDepth = layers.Min(l => l.TopDepth);
            double maxDepth = layers.Max(l => l.BottomDepth);
            
            // Use viewport's coordinate system or create a simple depth-based one
            var depthSystem = new DepthCoordinateSystem(minDepth, maxDepth, canvas.DeviceClipBounds.Height);

            // Render layers from bottom to top
            foreach (var layer in layers.OrderBy(l => l.TopDepth))
            {
                RenderLayer(canvas, layer, depthSystem, viewport);
            }

            // Render fluid contacts
            if (fluidContacts != null && configuration.ShowFluidContacts)
            {
                RenderFluidContacts(canvas, fluidContacts, depthSystem, viewport);
            }
        }

        /// <summary>
        /// Renders a single layer.
        /// </summary>
        private void RenderLayer(SKCanvas canvas, LayerData layer, CoordinateSystem depthSystem, Viewport viewport)
        {
            float topY = depthSystem.ToScreenY(layer.TopDepth, canvas.DeviceClipBounds.Height);
            float bottomY = depthSystem.ToScreenY(layer.BottomDepth, canvas.DeviceClipBounds.Height);
            float layerHeight = Math.Abs(bottomY - topY);

            if (layerHeight < 0.5f)
                return; // Skip very thin layers

            // Get color and pattern
            SKColor layerColor;
            LithologyPattern pattern;

            if (!string.IsNullOrEmpty(layer.ColorCode) && layer.ColorCode.StartsWith("#"))
            {
                // Parse hex color
                layerColor = SKColor.Parse(layer.ColorCode);
            }
            else
            {
                // Use lithology color palette
                layerColor = LithologyColorPalette.GetLithologyColor(layer.Lithology);
            }

            if (Enum.TryParse<LithologyPattern>(layer.PatternType, out var parsedPattern))
            {
                pattern = parsedPattern;
            }
            else
            {
                pattern = LithologyColorPalette.GetLithologyPattern(layer.Lithology);
            }

            // Create paint with pattern - use SVG patterns if lithology is available
            var paint = LithologyPatternRenderer.CreatePatternPaint(
                layerColor,
                pattern,
                null,
                configuration.PatternSize,
                layer.Lithology, // Pass lithology name to trigger SVG pattern lookup
                useSvgPattern: true);

            // Adjust opacity for non-pay zones
            if (!layer.IsPayZone && configuration.DimNonPayZones)
            {
                paint.Color = new SKColor(
                    paint.Color.Red,
                    paint.Color.Green,
                    paint.Color.Blue,
                    (byte)(paint.Color.Alpha * 0.5f));
            }

            // Draw layer rectangle
            float leftX = configuration.LeftMargin;
            float rightX = canvas.DeviceClipBounds.Width - configuration.RightMargin;
            var rect = new SKRect(leftX, topY, rightX, bottomY);
            canvas.DrawRect(rect, paint);

            // Draw border if configured
            if (configuration.ShowLayerBorders)
            {
                var borderPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 0.5f,
                    IsAntialias = true
                };
                canvas.DrawRect(rect, borderPaint);
            }

            // Draw label if configured
            if (configuration.ShowLayerLabels && layerHeight > 20) // Only if layer is tall enough
            {
                DrawLayerLabel(canvas, layer, topY, leftX, rightX);
            }
        }

        /// <summary>
        /// Draws a layer label.
        /// </summary>
        private void DrawLayerLabel(SKCanvas canvas, LayerData layer, float topY, float leftX, float rightX)
        {
            var textPaint = new SKPaint
            {
                Color = configuration.LabelColor,
                TextSize = configuration.LabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            };

            string label = !string.IsNullOrEmpty(layer.Lithology) ? layer.Lithology : layer.LayerName;
            if (!string.IsNullOrEmpty(layer.Facies))
                label += $" ({layer.Facies})";

            float labelX = leftX + 5;
            float labelY = topY + textPaint.TextSize;

            canvas.DrawText(label, labelX, labelY, textPaint);
        }

        /// <summary>
        /// Renders fluid contacts (FWL, OWC, GOC, GWC).
        /// </summary>
        private void RenderFluidContacts(SKCanvas canvas, FluidContacts contacts, CoordinateSystem depthSystem, Viewport viewport)
        {
            float leftX = configuration.LeftMargin;
            float rightX = canvas.DeviceClipBounds.Width - configuration.RightMargin;

            // Free Water Level (FWL)
            if (contacts.FreeWaterLevel.HasValue)
            {
                float fwlY = depthSystem.ToScreenY(contacts.FreeWaterLevel.Value, canvas.DeviceClipBounds.Height);
                var fwlPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.FreeWaterLevel,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0),
                    IsAntialias = true
                };
                canvas.DrawLine(leftX, fwlY, rightX, fwlY, fwlPaint);

                // Label
                var labelPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.FreeWaterLevel,
                    TextSize = 12,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                canvas.DrawText($"FWL = {contacts.FreeWaterLevel.Value:F1} m TVDSS", leftX + 5, fwlY - 5, labelPaint);
            }

            // Oil-Water Contact (OWC)
            if (contacts.OilWaterContact.HasValue)
            {
                float owcY = depthSystem.ToScreenY(contacts.OilWaterContact.Value, canvas.DeviceClipBounds.Height);
                var owcPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.OilWaterContact,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0),
                    IsAntialias = true
                };
                canvas.DrawLine(leftX, owcY, rightX, owcY, owcPaint);

                // Label
                var labelPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.OilWaterContact,
                    TextSize = 12,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                canvas.DrawText($"OWC = {contacts.OilWaterContact.Value:F1} m TVDSS", leftX + 5, owcY - 5, labelPaint);
            }

            // Gas-Oil Contact (GOC)
            if (contacts.GasOilContact.HasValue)
            {
                float gocY = depthSystem.ToScreenY(contacts.GasOilContact.Value, canvas.DeviceClipBounds.Height);
                var gocPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.GasOilContact,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0),
                    IsAntialias = true
                };
                canvas.DrawLine(leftX, gocY, rightX, gocY, gocPaint);

                // Label
                var labelPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.GasOilContact,
                    TextSize = 12,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                canvas.DrawText($"GOC = {contacts.GasOilContact.Value:F1} m TVDSS", leftX + 5, gocY - 5, labelPaint);
            }

            // Gas-Water Contact (GWC)
            if (contacts.GasWaterContact.HasValue)
            {
                float gwcY = depthSystem.ToScreenY(contacts.GasWaterContact.Value, canvas.DeviceClipBounds.Height);
                var gwcPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.GasWaterContact,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 2.0f,
                    PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0),
                    IsAntialias = true
                };
                canvas.DrawLine(leftX, gwcY, rightX, gwcY, gwcPaint);

                // Label
                var labelPaint = new SKPaint
                {
                    Color = LithologyColorPalette.FluidColors.GasWaterContact,
                    TextSize = 12,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                };
                canvas.DrawText($"GWC = {contacts.GasWaterContact.Value:F1} m TVDSS", leftX + 5, gwcY - 5, labelPaint);
            }
        }

        /// <summary>
        /// Gets the bounding rectangle of the layer content.
        /// </summary>
        public override SKRect GetBounds()
        {
            if (layers == null || layers.Count == 0)
                return SKRect.Empty;

            double minDepth = layers.Min(l => l.TopDepth);
            double maxDepth = layers.Max(l => l.BottomDepth);

            // Return a default bounding rect (actual bounds depend on canvas size)
            return new SKRect(
                configuration.LeftMargin,
                0,
                800 - configuration.RightMargin, // Default width
                (float)(maxDepth - minDepth)); // Approximate height
        }
    }

    /// <summary>
    /// Configuration for reservoir layer rendering.
    /// </summary>
    public class ReservoirLayerConfiguration
    {
        /// <summary>
        /// Gets or sets the left margin.
        /// </summary>
        public float LeftMargin { get; set; } = 50.0f;

        /// <summary>
        /// Gets or sets the right margin.
        /// </summary>
        public float RightMargin { get; set; } = 50.0f;

        /// <summary>
        /// Gets or sets whether to show layer borders.
        /// </summary>
        public bool ShowLayerBorders { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show layer labels.
        /// </summary>
        public bool ShowLayerLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show fluid contacts.
        /// </summary>
        public bool ShowFluidContacts { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to dim non-pay zones.
        /// </summary>
        public bool DimNonPayZones { get; set; } = true;

        /// <summary>
        /// Gets or sets the pattern size.
        /// </summary>
        public float PatternSize { get; set; } = 4.0f;

        /// <summary>
        /// Gets or sets the label color.
        /// </summary>
        public SKColor LabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 10.0f;
    }
}

