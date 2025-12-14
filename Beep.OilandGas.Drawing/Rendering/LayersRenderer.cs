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
    /// Renders geological layers with lithology colors, patterns, and fluid contacts.
    /// </summary>
    public class LayersRenderer
    {
        private readonly List<LayerData> layers;
        private readonly FluidContacts fluidContacts;
        private readonly LayersRendererConfiguration configuration;
        private DepthCoordinateSystem depthSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="LayersRenderer"/> class.
        /// </summary>
        /// <param name="layers">The geological layers to render.</param>
        /// <param name="fluidContacts">Optional fluid contacts.</param>
        /// <param name="configuration">Rendering configuration.</param>
        public LayersRenderer(
            List<LayerData> layers,
            FluidContacts fluidContacts = null,
            LayersRendererConfiguration configuration = null)
        {
            this.layers = layers ?? throw new ArgumentNullException(nameof(layers));
            this.fluidContacts = fluidContacts;
            this.configuration = configuration ?? new LayersRendererConfiguration();

            Initialize();
        }

        /// <summary>
        /// Initializes the renderer with depth system.
        /// </summary>
        private void Initialize()
        {
            if (layers == null || layers.Count == 0)
                return;

            double minDepth = layers.Min(l => Math.Min(l.TopDepth, l.BottomDepth));
            double maxDepth = layers.Max(l => Math.Max(l.TopDepth, l.BottomDepth));

            depthSystem = new DepthCoordinateSystem(minDepth, maxDepth, 1000f); // Default, updated on render
        }

        /// <summary>
        /// Renders the layers.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render on.</param>
        /// <param name="width">Canvas width.</param>
        /// <param name="height">Canvas height.</param>
        /// <param name="xOffset">X offset for positioning.</param>
        /// <param name="yOffset">Y offset for positioning.</param>
        public void Render(SKCanvas canvas, float width, float height, float xOffset = 0, float yOffset = 0)
        {
            if (canvas == null || layers == null || layers.Count == 0)
                return;

            // Update depth system with actual canvas height
            if (depthSystem != null)
            {
                double minDepth = layers.Min(l => Math.Min(l.TopDepth, l.BottomDepth));
                double maxDepth = layers.Max(l => Math.Max(l.TopDepth, l.BottomDepth));
                depthSystem = new DepthCoordinateSystem(minDepth, maxDepth, height);
            }

            // Clear background
            canvas.Clear(configuration.BackgroundColor);

            // Draw grid if enabled
            if (configuration.ShowGrid)
            {
                DrawGrid(canvas, width, height, xOffset, yOffset);
            }

            // Render layers from bottom to top
            var sortedLayers = layers.OrderBy(l => Math.Min(l.TopDepth, l.BottomDepth)).ToList();
            foreach (var layer in sortedLayers)
            {
                RenderLayer(canvas, layer, width, height, xOffset, yOffset);
            }

            // Render fluid contacts if enabled
            if (configuration.ShowFluidContacts && fluidContacts != null)
            {
                RenderFluidContacts(canvas, width, height, xOffset, yOffset);
            }

            // Draw depth scale if enabled
            if (configuration.ShowDepthScale)
            {
                DrawDepthScale(canvas, width, height, xOffset, yOffset);
            }
        }

        /// <summary>
        /// Renders a single layer.
        /// </summary>
        private void RenderLayer(
            SKCanvas canvas,
            LayerData layer,
            float width,
            float height,
            float xOffset,
            float yOffset)
        {
            float topY = depthSystem.ToScreenY(layer.TopDepth, null) + yOffset;
            float bottomY = depthSystem.ToScreenY(layer.BottomDepth, null) + yOffset;
            float layerHeight = Math.Abs(bottomY - topY);

            if (layerHeight < 0.5f)
                return; // Skip very thin layers

            // Get layer color
            SKColor layerColor = GetLayerColor(layer);

            // Get layer pattern
            string patternType = GetLayerPattern(layer);

            // Create layer rectangle
            SKRect layerRect = new SKRect(xOffset, topY, xOffset + width, bottomY);

            // Draw layer fill if enabled
            if (configuration.ShowLayerFills)
            {
                DrawLayerFill(canvas, layerRect, layerColor, patternType, layer);
            }

            // Draw layer border if enabled
            if (configuration.ShowLayerBorders)
            {
                DrawLayerBorder(canvas, layerRect, layer);
            }

            // Draw layer label if enabled
            if (configuration.ShowLayerLabels && layerHeight > 20)
            {
                DrawLayerLabel(canvas, layer, layerRect);
            }
        }

        /// <summary>
        /// Gets the color for a layer.
        /// </summary>
        private SKColor GetLayerColor(LayerData layer)
        {
            // Check custom color mapping
            if (configuration.LayerColors.ContainsKey(layer.LayerName))
            {
                return configuration.LayerColors[layer.LayerName];
            }

            // Check color code
            if (!string.IsNullOrEmpty(layer.ColorCode) && layer.ColorCode.StartsWith("#"))
            {
                try
                {
                    return SKColor.Parse(layer.ColorCode);
                }
                catch
                {
                    // Fall through to lithology palette
                }
            }

            // Use lithology color palette if enabled
            if (configuration.UseLithologyColorPalette && !string.IsNullOrEmpty(layer.Lithology))
            {
                return LithologyColorPalette.GetLithologyColor(layer.Lithology);
            }

            // Default color
            return SKColors.Gray;
        }

        /// <summary>
        /// Gets the pattern type for a layer.
        /// </summary>
        private string GetLayerPattern(LayerData layer)
        {
            if (configuration.LayerPatterns.ContainsKey(layer.LayerName))
            {
                return configuration.LayerPatterns[layer.LayerName];
            }

            if (!string.IsNullOrEmpty(layer.PatternType))
            {
                return layer.PatternType;
            }

            if (configuration.UseLithologyColorPalette && !string.IsNullOrEmpty(layer.Lithology))
            {
                var pattern = LithologyColorPalette.GetLithologyPattern(layer.Lithology);
                return pattern.ToString();
            }

            return "Solid";
        }

        /// <summary>
        /// Draws the layer fill with color and pattern.
        /// </summary>
        private void DrawLayerFill(
            SKCanvas canvas,
            SKRect rect,
            SKColor color,
            string patternType,
            LayerData layer)
        {
            // Highlight pay zones if enabled
            if (configuration.HighlightPayZones && layer.IsPayZone)
            {
                using (var highlightPaint = new SKPaint
                {
                    Color = configuration.PayZoneHighlightColor,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(rect, highlightPaint);
                }
            }

            // Draw pattern if enabled
            if (configuration.ShowLayerPatterns && patternType != "Solid")
            {
                // Try to use SVG pattern first (if lithology is available)
                SKPaint patternPaint = null;
                if (configuration.UseLithologyColorPalette && !string.IsNullOrEmpty(layer.Lithology))
                {
                    // Use SVG lithology patterns from embedded resources
                    patternPaint = LithologyPatternRenderer.CreatePatternPaint(
                        color,
                        LithologyPattern.Solid, // Will be overridden by SVG if found
                        null,
                        4f,
                        layer.Lithology, // Pass lithology name to trigger SVG lookup
                        useSvgPattern: true);
                }
                
                // If SVG pattern not found, fall back to programmatic pattern
                if (patternPaint == null && Enum.TryParse<LithologyPattern>(patternType, true, out var pattern))
                {
                    patternPaint = LithologyPatternRenderer.CreatePatternPaint(
                        color,
                        pattern,
                        null,
                        4f,
                        layer.Lithology, // Still pass lithology name for SVG fallback
                        useSvgPattern: true);
                }
                
                if (patternPaint != null)
                {
                    canvas.DrawRect(rect, patternPaint);
                }
                else
                {
                    // Solid fill as final fallback
                    using (var paint = new SKPaint
                    {
                        Color = color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(rect, paint);
                    }
                }
            }
            else
            {
                // Solid fill
                using (var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(rect, paint);
                }
            }

            // Draw pay zone border if enabled
            if (configuration.HighlightPayZones && layer.IsPayZone)
            {
                using (var borderPaint = new SKPaint
                {
                    Color = configuration.PayZoneBorderColor,
                    StrokeWidth = configuration.PayZoneBorderWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    canvas.DrawRect(rect, borderPaint);
                }
            }
        }

        /// <summary>
        /// Draws the layer border.
        /// </summary>
        private void DrawLayerBorder(SKCanvas canvas, SKRect rect, LayerData layer)
        {
            using (var borderPaint = new SKPaint
            {
                Color = configuration.LayerBorderColor,
                StrokeWidth = configuration.LayerBorderWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            })
            {
                canvas.DrawRect(rect, borderPaint);
            }
        }

        /// <summary>
        /// Draws the layer label.
        /// </summary>
        private void DrawLayerLabel(SKCanvas canvas, LayerData layer, SKRect rect)
        {
            string label = !string.IsNullOrEmpty(layer.LayerName) 
                ? layer.LayerName 
                : layer.Lithology ?? "Layer";

            if (!string.IsNullOrEmpty(layer.Facies))
            {
                label += $" ({layer.Facies})";
            }

            using (var textPaint = new SKPaint
            {
                Color = configuration.LayerLabelColor,
                TextSize = configuration.LayerLabelFontSize,
                IsAntialias = true,
                TextAlign = SKTextAlign.Left
            })
            {
                float labelX = rect.Left + 5;
                float labelY = rect.Top + textPaint.TextSize;
                canvas.DrawText(label, labelX, labelY, textPaint);
            }
        }

        /// <summary>
        /// Renders fluid contacts.
        /// </summary>
        private void RenderFluidContacts(
            SKCanvas canvas,
            float width,
            float height,
            float xOffset,
            float yOffset)
        {
            if (fluidContacts == null)
                return;

            // Free Water Level (FWL)
            if (fluidContacts.FreeWaterLevel.HasValue)
            {
                float fwlY = depthSystem.ToScreenY((float)fluidContacts.FreeWaterLevel.Value, null) + yOffset;
                DrawContactLine(canvas, "FWL", fwlY, configuration.FreeWaterLevelColor, 
                    fluidContacts.FreeWaterLevel.Value, xOffset, width);
            }

            // Oil-Water Contact (OWC)
            if (fluidContacts.OilWaterContact.HasValue)
            {
                float owcY = depthSystem.ToScreenY((float)fluidContacts.OilWaterContact.Value, null) + yOffset;
                DrawContactLine(canvas, "OWC", owcY, configuration.OilWaterContactColor,
                    fluidContacts.OilWaterContact.Value, xOffset, width);
            }

            // Gas-Oil Contact (GOC)
            if (fluidContacts.GasOilContact.HasValue)
            {
                float gocY = depthSystem.ToScreenY((float)fluidContacts.GasOilContact.Value, null) + yOffset;
                DrawContactLine(canvas, "GOC", gocY, configuration.GasOilContactColor,
                    fluidContacts.GasOilContact.Value, xOffset, width);
            }

            // Gas-Water Contact (GWC)
            if (fluidContacts.GasWaterContact.HasValue)
            {
                float gwcY = depthSystem.ToScreenY((float)fluidContacts.GasWaterContact.Value, null) + yOffset;
                DrawContactLine(canvas, "GWC", gwcY, configuration.FreeWaterLevelColor, // Use FWL color for GWC
                    fluidContacts.GasWaterContact.Value, xOffset, width);
            }
        }

        /// <summary>
        /// Draws a fluid contact line.
        /// </summary>
        private void DrawContactLine(
            SKCanvas canvas,
            string label,
            float y,
            SKColor color,
            double depth,
            float xOffset,
            float width)
        {
            using (var linePaint = new SKPaint
            {
                Color = color,
                StrokeWidth = configuration.ContactLineWidth,
                Style = SKPaintStyle.Stroke,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 10, 5 }, 0)
            })
            {
                canvas.DrawLine(xOffset, y, xOffset + width, y, linePaint);
            }

            // Draw label if enabled
            if (configuration.ShowContactLabels)
            {
                using (var textPaint = new SKPaint
                {
                    Color = color,
                    TextSize = configuration.ContactLabelFontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Left
                })
                {
                    string labelText = $"{label} = {depth:F1} m TVDSS";
                    canvas.DrawText(labelText, xOffset + 5, y - 5, textPaint);
                }
            }
        }

        /// <summary>
        /// Draws grid lines.
        /// </summary>
        private void DrawGrid(SKCanvas canvas, float width, float height, float xOffset, float yOffset)
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
                for (float x = xOffset; x < xOffset + width; x += 50)
                {
                    canvas.DrawLine(x, yOffset, x, yOffset + height, paint);
                }

                // Horizontal grid lines (depth-based)
                if (depthSystem != null)
                {
                    double depthRange = depthSystem.MaxValue - depthSystem.MinValue;
                    int gridLines = (int)(depthRange / configuration.DepthInterval);

                    for (int i = 0; i <= gridLines; i++)
                    {
                        double depth = depthSystem.MinValue + (depthRange * i / gridLines);
                        float y = depthSystem.ToScreenY((float)depth, null) + yOffset;
                        canvas.DrawLine(xOffset, y, xOffset + width, y, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Draws depth scale.
        /// </summary>
        private void DrawDepthScale(SKCanvas canvas, float width, float height, float xOffset, float yOffset)
        {
            if (depthSystem == null)
                return;

            double depthRange = depthSystem.MaxValue - depthSystem.MinValue;
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
                    double depth = depthSystem.MinValue + (depthRange * i / scaleCount);
                    float y = depthSystem.ToScreenY((float)depth, null) + yOffset;
                    canvas.DrawText(depth.ToString("F0"), xOffset - 5, y + 5, paint);
                }
            }
        }
    }
}

