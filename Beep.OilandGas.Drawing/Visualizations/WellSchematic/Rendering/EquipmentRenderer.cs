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
        private const float MinimumEquipmentHeight = 10.0f;
        private const float DefaultEquipmentWidth = 10.0f;
        private readonly DepthTransform depthSystem;
        private readonly WellSchematicConfiguration configuration;
        private readonly Dictionary<string, SkiaSharp.Extended.Svg.SKSvg> svgCache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentRenderer"/> class.
        /// </summary>
        /// <param name="depthSystem">The depth transform.</param>
        /// <param name="configuration">The rendering configuration.</param>
        public EquipmentRenderer(DepthTransform depthSystem, WellSchematicConfiguration configuration)
        {
            this.depthSystem = depthSystem ?? throw new ArgumentNullException(nameof(depthSystem));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            svgCache = new Dictionary<string, SkiaSharp.Extended.Svg.SKSvg>(StringComparer.OrdinalIgnoreCase);
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

            foreach (var equipment in equipmentList)
            {
                RenderEquipment(canvas, equipment, wellborePath, canvasHeight);
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

            foreach (var equipment in equipmentList)
            {
                RenderEquipment(canvas, equipment, tubePath, canvasHeight);
            }
        }

        private void RenderEquipment(SKCanvas canvas, WellData_Equip equipment, List<SKPoint> pathPoints, float canvasHeight)
        {
            if (equipment == null || pathPoints == null || pathPoints.Count < 2)
                return;

            float topY = depthSystem.ToScreenY(equipment.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(equipment.BottomDepth, canvasHeight);
            float anchorY = (topY + bottomY) / 2.0f;
            float symbolHeight = Math.Max(MinimumEquipmentHeight, Math.Abs(bottomY - topY));
            float symbolWidth = ResolveEquipmentWidth(equipment, symbolHeight);

            var anchor = PathHelper.GetPointOnPath(pathPoints, anchorY);
            var tangent = PathHelper.GetTangentOnPath(pathPoints, anchorY);
            float tangentLength = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
            if (tangentLength <= 0)
                return;

            float angleDegrees = (float)(Math.Atan2(tangent.Y, tangent.X) * 180.0 / Math.PI) - 90.0f;
            using var fillPaint = CreateEquipmentFillPaint();
            using var strokePaint = CreateEquipmentStrokePaint();

            var svg = LoadSvg(equipment);
            if (svg?.Picture != null)
            {
                DrawSvgEquipment(canvas, svg, anchor, angleDegrees, symbolWidth, symbolHeight);
                return;
            }

            DrawFallbackEquipment(canvas, equipment, anchor, angleDegrees, symbolWidth, symbolHeight, fillPaint, strokePaint);
        }

        private SKPaint CreateEquipmentFillPaint()
        {
            return new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                Color = configuration.Theme.GetColor("Equipment", ColorPalette.WellSchematic.Equipment),
                StrokeWidth = 1
            };
        }

        private SKPaint CreateEquipmentStrokePaint()
        {
            return new SKPaint
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black.WithAlpha(180),
                StrokeWidth = 1.25f
            };
        }

        private static float ResolveEquipmentWidth(WellData_Equip equipment, float symbolHeight)
        {
            if (equipment.Diameter > 0)
                return Math.Max(DefaultEquipmentWidth, equipment.Diameter);

            return Math.Max(DefaultEquipmentWidth, symbolHeight * 0.65f);
        }

        private void DrawSvgEquipment(SKCanvas canvas, SkiaSharp.Extended.Svg.SKSvg svg, SKPoint anchor, float angleDegrees, float width, float height)
        {
            var bounds = svg.Picture.CullRect;
            if (bounds.Width <= 0 || bounds.Height <= 0)
                return;

            canvas.Save();
            canvas.Translate(anchor.X, anchor.Y);
            canvas.RotateDegrees(angleDegrees);
            canvas.Scale(width / bounds.Width, height / bounds.Height);
            canvas.Translate(-bounds.MidX, -bounds.MidY);
            canvas.DrawPicture(svg.Picture);
            canvas.Restore();
        }

        private void DrawFallbackEquipment(SKCanvas canvas, WellData_Equip equipment, SKPoint anchor, float angleDegrees, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            canvas.Save();
            canvas.Translate(anchor.X, anchor.Y);
            canvas.RotateDegrees(angleDegrees);

            switch (ResolveSymbolKind(equipment))
            {
                case EquipmentSymbolKind.Packer:
                    DrawPackerSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                case EquipmentSymbolKind.SafetyValve:
                    DrawSafetyValveSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                case EquipmentSymbolKind.Valve:
                    DrawValveSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                case EquipmentSymbolKind.Gauge:
                    DrawGaugeSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                case EquipmentSymbolKind.Pump:
                    DrawPumpSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                case EquipmentSymbolKind.Screen:
                    DrawScreenSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                case EquipmentSymbolKind.Mandrel:
                    DrawMandrelSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
                default:
                    DrawGenericToolSymbol(canvas, width, height, fillPaint, strokePaint);
                    break;
            }

            canvas.Restore();
        }

        private static EquipmentSymbolKind ResolveSymbolKind(WellData_Equip equipment)
        {
            var descriptor = string.Join(" ", new[]
            {
                equipment.EquipmentType,
                equipment.EquipmentName,
                equipment.EquipmentDescription,
                equipment.ToolTipText
            }.Where(value => !string.IsNullOrWhiteSpace(value))).ToLowerInvariant();

            if (descriptor.Contains("subsurface safety valve") || descriptor.Contains("sssv") || descriptor.Contains("safety valve"))
                return EquipmentSymbolKind.SafetyValve;

            if (descriptor.Contains("packer"))
                return EquipmentSymbolKind.Packer;

            if (descriptor.Contains("sliding sleeve") || descriptor.Contains("control valve") || descriptor.Contains("gas lift valve") || descriptor.Contains("valve") || descriptor.Contains("sleeve"))
                return EquipmentSymbolKind.Valve;

            if (descriptor.Contains("gauge") || descriptor.Contains("sensor") || descriptor.Contains("monitor"))
                return EquipmentSymbolKind.Gauge;

            if (descriptor.Contains("esp") || descriptor.Contains("pump") || descriptor.Contains("pcp"))
                return EquipmentSymbolKind.Pump;

            if (descriptor.Contains("screen") || descriptor.Contains("liner"))
                return EquipmentSymbolKind.Screen;

            if (descriptor.Contains("mandrel") || descriptor.Contains("side pocket"))
                return EquipmentSymbolKind.Mandrel;

            return EquipmentSymbolKind.Generic;
        }

        private static void DrawGenericToolSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            var body = SKRect.Create(-width / 2.0f, -height / 2.0f, width, height);
            canvas.DrawRoundRect(body, Math.Min(width, height) * 0.2f, Math.Min(width, height) * 0.2f, fillPaint);
            canvas.DrawRoundRect(body, Math.Min(width, height) * 0.2f, Math.Min(width, height) * 0.2f, strokePaint);
        }

        private static void DrawPackerSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            using var path = new SKPath();
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;
            path.MoveTo(0, -halfHeight);
            path.LineTo(halfWidth, -halfHeight * 0.2f);
            path.LineTo(halfWidth, halfHeight * 0.2f);
            path.LineTo(0, halfHeight);
            path.LineTo(-halfWidth, halfHeight * 0.2f);
            path.LineTo(-halfWidth, -halfHeight * 0.2f);
            path.Close();
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        private static void DrawValveSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            using var path = new SKPath();
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;
            path.MoveTo(0, -halfHeight);
            path.LineTo(halfWidth, 0);
            path.LineTo(0, halfHeight);
            path.LineTo(-halfWidth, 0);
            path.Close();
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        private static void DrawSafetyValveSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            DrawValveSymbol(canvas, width, height * 0.85f, fillPaint, strokePaint);
            float capWidth = width * 0.7f;
            float capY = -height / 2.0f;
            canvas.DrawLine(-capWidth / 2.0f, capY, capWidth / 2.0f, capY, strokePaint);
        }

        private static void DrawGaugeSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            float radius = Math.Min(width, height) / 2.4f;
            canvas.DrawCircle(0, 0, radius, fillPaint);
            canvas.DrawCircle(0, 0, radius, strokePaint);
            canvas.DrawLine(0, 0, radius * 0.65f, -radius * 0.35f, strokePaint);
        }

        private static void DrawPumpSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            var body = SKRect.Create(-width / 2.8f, -height / 2.0f, width / 1.4f, height);
            canvas.DrawRect(body, fillPaint);
            canvas.DrawRect(body, strokePaint);

            using var head = new SKPath();
            head.MoveTo(0, -height / 2.0f);
            head.LineTo(width / 2.0f, -height / 6.0f);
            head.LineTo(0, height / 6.0f);
            head.Close();
            canvas.DrawPath(head, fillPaint);
            canvas.DrawPath(head, strokePaint);
        }

        private static void DrawScreenSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            var body = SKRect.Create(-width / 2.0f, -height / 2.0f, width, height);
            canvas.DrawRect(body, fillPaint);
            canvas.DrawRect(body, strokePaint);

            using var hatch = new SKPaint
            {
                IsAntialias = strokePaint.IsAntialias,
                Style = strokePaint.Style,
                Color = strokePaint.Color,
                StrokeWidth = 0.8f
            };

            float spacing = Math.Max(3.0f, width / 4.0f);
            for (float x = body.Left + spacing; x < body.Right; x += spacing)
            {
                canvas.DrawLine(x, body.Top, x, body.Bottom, hatch);
            }
        }

        private static void DrawMandrelSymbol(SKCanvas canvas, float width, float height, SKPaint fillPaint, SKPaint strokePaint)
        {
            using var path = new SKPath();
            float halfWidth = width / 2.0f;
            float halfHeight = height / 2.0f;
            path.MoveTo(-halfWidth * 0.5f, -halfHeight);
            path.LineTo(halfWidth * 0.5f, -halfHeight);
            path.LineTo(halfWidth, 0);
            path.LineTo(halfWidth * 0.5f, halfHeight);
            path.LineTo(-halfWidth * 0.5f, halfHeight);
            path.LineTo(-halfWidth, 0);
            path.Close();
            canvas.DrawPath(path, fillPaint);
            canvas.DrawPath(path, strokePaint);
        }

        /// <summary>
        /// Loads an SVG resource.
        /// </summary>
        /// <param name="equipment">The equipment descriptor.</param>
        /// <returns>The SVG, or null if not found.</returns>
        private SkiaSharp.Extended.Svg.SKSvg LoadSvg(WellData_Equip equipment)
        {
            foreach (var candidate in ResolveSvgCandidates(equipment))
            {
                var svg = LoadSvgByName(candidate);
                if (svg?.Picture != null)
                    return svg;
            }

            return null;
        }

        private IEnumerable<string> ResolveSvgCandidates(WellData_Equip equipment)
        {
            if (equipment == null)
                yield break;

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var candidate in ExpandCandidates(equipment.EquipmentSvg, seen))
                yield return candidate;

            foreach (var descriptor in LegacyEquipmentSvgResolver.ResolveAliases(equipment))
            {
                foreach (var candidate in ExpandCandidates(descriptor, seen))
                    yield return candidate;
            }

            foreach (var candidate in ExpandCandidates(GetDefaultSvgForSymbolKind(ResolveSymbolKind(equipment)), seen))
                yield return candidate;
        }

        private IEnumerable<string> ExpandCandidates(string value, HashSet<string> seen)
        {
            if (string.IsNullOrWhiteSpace(value))
                yield break;

            var trimmed = value.Trim();
            if (seen.Add(trimmed))
                yield return trimmed;

            if (!trimmed.EndsWith(".svg", StringComparison.OrdinalIgnoreCase))
            {
                var withExtension = trimmed + ".svg";
                if (seen.Add(withExtension))
                    yield return withExtension;
            }
        }

        private static string GetDefaultSvgForSymbolKind(EquipmentSymbolKind symbolKind)
        {
            return symbolKind switch
            {
                EquipmentSymbolKind.Packer => "packer.svg",
                EquipmentSymbolKind.Valve => "valve.svg",
                EquipmentSymbolKind.SafetyValve => "sub surface safety valve.svg",
                EquipmentSymbolKind.Gauge => "downhole_sensor.svg",
                EquipmentSymbolKind.Pump => "pump.svg",
                EquipmentSymbolKind.Screen => "sand screen.svg",
                EquipmentSymbolKind.Mandrel => "mandrel.svg",
                _ => "tool.svg"
            };
        }

        private SkiaSharp.Extended.Svg.SKSvg LoadSvgByName(string componentName)
        {
            if (string.IsNullOrWhiteSpace(componentName))
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
                    var svgName = componentName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)
                        ? componentName
                        : componentName + ".svg";
                    var resourceName = resourceNames.FirstOrDefault(r =>
                        r.EndsWith(svgName, StringComparison.OrdinalIgnoreCase) ||
                        r.EndsWith(componentName, StringComparison.OrdinalIgnoreCase));

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
                    var candidatePaths = new[]
                    {
                        Path.Combine(configuration.SvgResourcesPath, componentName),
                        Path.Combine(configuration.SvgResourcesPath, componentName.EndsWith(".svg", StringComparison.OrdinalIgnoreCase) ? componentName : componentName + ".svg")
                    };

                    var filePath = candidatePaths.FirstOrDefault(File.Exists);
                    if (!string.IsNullOrWhiteSpace(filePath))
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

        private enum EquipmentSymbolKind
        {
            Generic,
            Packer,
            Valve,
            SafetyValve,
            Gauge,
            Pump,
            Screen,
            Mandrel
        }
    }
}

