using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;
using Beep.OilandGas.Drawing.Styling;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering
{
    /// <summary>
    /// Renders equipment on wellbores and tubing.
    /// </summary>
    public class EquipmentRenderer
    {
        private readonly CoordinateSystem depthSystem;
        private readonly WellSchematicConfiguration configuration;
        private readonly Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> svgCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentRenderer"/> class.
        /// </summary>
        /// <param name="depthSystem">The depth coordinate system.</param>
        /// <param name="configuration">The rendering configuration.</param>
        public EquipmentRenderer(CoordinateSystem depthSystem, WellSchematicConfiguration configuration)
        {
            this.depthSystem = depthSystem ?? throw new ArgumentNullException(nameof(depthSystem));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            svgCache = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>();
        }

        /// <summary>
        /// Renders equipment on a borehole.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="equipmentList">The equipment list.</param>
        /// <param name="wellborePath">The wellbore path points.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <param name="onEquipmentClicked">Callback for equipment clicks.</param>
        public void RenderBoreholeEquipment(SKCanvas canvas, List<WellData_Equip> equipmentList, List<SKPoint> wellborePath, float canvasHeight, Action<WellData_Equip> onEquipmentClicked)
        {
            if (equipmentList == null || equipmentList.Count == 0)
                return;

            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = configuration.Theme.GetColor("Equipment", ColorPalette.WellSchematic.Equipment),
                StrokeWidth = 1
            };

            foreach (var equipment in equipmentList)
            {
                float topY = depthSystem.ToScreenY(equipment.TopDepth, canvasHeight);
                float bottomY = depthSystem.ToScreenY(equipment.BottomDepth, canvasHeight);

                SKPoint topPosition = PathHelper.GetPointOnPath(wellborePath, topY);
                SKPoint bottomPosition = PathHelper.GetPointOnPath(wellborePath, bottomY);

                float height = Math.Abs(bottomY - topY);

                // Load SVG
                var svg = LoadSvg(equipment.EquipmentSvg);
                if (svg != null && svg.Picture != null)
                {
                    equipment.Diameter = svg.Picture.CullRect.Width;
                    float width = equipment.Diameter * configuration.DiameterScale;

                    // Draw SVG
                    SKRect equipmentRect = new SKRect(
                        topPosition.X - width / 2,
                        topPosition.Y,
                        topPosition.X + width / 2,
                        topPosition.Y + height);

                    SKMatrix matrix = SKMatrix.CreateScale(
                        width / svg.CanvasSize.Width,
                        height / svg.CanvasSize.Height);
                    matrix.PostConcat(SKMatrix.CreateTranslation(equipmentRect.Left, equipmentRect.Top));

                    canvas.DrawPicture(svg.Picture, ref matrix, paint);

                    // Store for interaction (if needed)
                    // TODO: Implement interaction tracking
                }
            }
        }

        /// <summary>
        /// Renders equipment on tubing.
        /// </summary>
        /// <param name="canvas">The canvas to render to.</param>
        /// <param name="equipmentList">The equipment list.</param>
        /// <param name="tubePath">The tubing path points.</param>
        /// <param name="tubeIndex">The tube index.</param>
        /// <param name="canvasHeight">The canvas height.</param>
        /// <param name="onEquipmentClicked">Callback for equipment clicks.</param>
        public void RenderTubingEquipment(SKCanvas canvas, List<WellData_Equip> equipmentList, List<SKPoint> tubePath, int tubeIndex, float canvasHeight, Action<WellData_Equip> onEquipmentClicked)
        {
            if (equipmentList == null || equipmentList.Count == 0)
                return;

            var paint = new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = configuration.Theme.GetColor("Equipment", ColorPalette.WellSchematic.Equipment),
                StrokeWidth = 1
            };

            foreach (var equipment in equipmentList)
            {
                float topY = depthSystem.ToScreenY(equipment.TopDepth, canvasHeight);
                float bottomY = depthSystem.ToScreenY(equipment.BottomDepth, canvasHeight);

                var topPoint = PathHelper.GetPointOnPath(tubePath, topY);
                var bottomPoint = PathHelper.GetPointOnPath(tubePath, bottomY);

                // Load SVG
                var svg = LoadSvg(equipment.EquipmentSvg);
                if (svg != null && svg.Picture != null)
                {
                    var equipmentBounds = svg.Picture.CullRect;

                    // Calculate scale factors
                    float scaleX = Math.Abs(bottomPoint.X - topPoint.X) / equipmentBounds.Width;
                    float scaleY = Math.Abs(bottomPoint.Y - topPoint.Y) / equipmentBounds.Height;

                    // Create transformation matrix
                    SKMatrix matrix = SKMatrix.CreateScale(scaleX, scaleY);
                    matrix = matrix.PreConcat(SKMatrix.CreateTranslation(topPoint.X, topPoint.Y));

                    // Draw SVG
                    canvas.Save();
                    canvas.SetMatrix(matrix);
                    canvas.DrawPicture(svg.Picture, ref matrix, paint);
                    canvas.Restore();
                }
            }
        }

        /// <summary>
        /// Loads an SVG resource.
        /// </summary>
        /// <param name="componentName">The component name.</param>
        /// <returns>The SVG, or null if not found.</returns>
        private SkiaSharp.Extended.Svg.SKSvg LoadSvg(string componentName)
        {
            if (string.IsNullOrEmpty(componentName))
                return null;

            // Check cache
            if (svgCache.ContainsKey(componentName))
                return svgCache[componentName];

            SkiaSharp.Extended.Svg.SKSvg svg = null;

            try
            {
                if (configuration.UseEmbeddedSvg)
                {
                    // Load from embedded resources
                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceNames = assembly.GetManifestResourceNames();
                    var resourceName = resourceNames.FirstOrDefault(r => r.EndsWith(componentName + ".svg") || r.EndsWith(componentName));

                    if (resourceName != null)
                    {
                        using (var stream = assembly.GetManifestResourceStream(resourceName))
                        {
                            if (stream != null)
                            {
                                svg = new SkiaSharp.Extended.Svg.SKSvg();
                                svg.Load(stream);
                            }
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(configuration.SvgResourcesPath))
                {
                    // Load from file
                    string filePath = Path.Combine(configuration.SvgResourcesPath, componentName);
                    if (File.Exists(filePath))
                    {
                        svg = new SkiaSharp.Extended.Svg.SKSvg();
                        svg.Load(filePath);
                    }
                }
            }
            catch (Exception)
            {
                // Log error if needed
                svg = null;
            }

            // Cache the result (even if null)
            svgCache[componentName] = svg;
            return svg;
        }
    }
}

