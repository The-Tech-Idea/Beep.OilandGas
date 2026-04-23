using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Beep.OilandGas.Drawing.Layers;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.DataLoaders.Models;
using Beep.OilandGas.Drawing.Exceptions;
using Beep.OilandGas.Drawing.Interaction;
using Beep.OilandGas.Drawing.Styling;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Models;
using Beep.OilandGas.Drawing.Validation;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic
{
    /// <summary>
    /// Enhanced well schematic renderer with full support for vertical and deviated wellbores,
    /// casing, tubing, equipment, perforations, and annotations.
    /// </summary>
    public class EnhancedWellSchematicRenderer : LayerBase, IInteractiveLayer
    {
        private const float DefaultLayoutHeight = 1000.0f;
        private const float MinimumLayoutHeight = 400.0f;
        private const float HorizontalLayoutPadding = 40.0f;
        private const float RightOverlayPadding = 20.0f;
        private const float SymbolPadding = 30.0f;
        private readonly WellData wellData;
        private readonly WellSchematicConfiguration configuration;
        private readonly DepthTransform depthSystem;
        private readonly Dictionary<string, DeviationSurvey> deviationSurveys;
        private readonly WellborePathCalculator pathCalculator;
        private readonly EquipmentRenderer equipmentRenderer;
        private readonly AnnotationRenderer annotationRenderer;
        private readonly Dictionary<int, List<SKPoint>> outerWellborePaths;
        private readonly Dictionary<int, List<SKPoint>> innerWellborePaths;
        private readonly Dictionary<(int BoreholeIndex, int TubeIndex), List<SKPoint>> tubingPaths;

        /// <summary>
        /// Gets or sets whether to show annotations.
        /// </summary>
        public bool ShowAnnotations { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show depth scale.
        /// </summary>
        public bool ShowDepthScale { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show grid.
        /// </summary>
        public bool ShowGrid { get; set; } = false;

        /// <summary>
        /// Gets the well data being rendered.
        /// </summary>
        public WellData WellData => wellData;

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public WellSchematicConfiguration Configuration => configuration;

        /// <summary>
        /// Event raised when equipment is clicked.
        /// </summary>
        public event EventHandler<EquipmentClickedEventArgs> EquipmentClicked;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnhancedWellSchematicRenderer"/> class.
        /// </summary>
        /// <param name="wellData">The well data to render.</param>
        /// <param name="configuration">The rendering configuration.</param>
        /// <param name="deviationSurveys">Optional deviation surveys keyed by borehole identifier.</param>
        public EnhancedWellSchematicRenderer(
            WellData wellData,
            WellSchematicConfiguration configuration = null,
            IDictionary<string, DeviationSurvey> deviationSurveys = null)
            : base("Enhanced Well Schematic")
        {
            if (wellData == null)
                throw new ArgumentNullException(nameof(wellData));

            this.wellData = wellData;
            this.configuration = configuration ?? WellSchematicConfiguration.Default;
            this.deviationSurveys = BuildDeviationSurveyLookup(deviationSurveys);

            // Validate well data
            WellDataValidator.ValidateWellData(wellData);

            // Create depth transform
            if (wellData.BoreHoles != null && wellData.BoreHoles.Count > 0)
            {
                float minDepth = wellData.BoreHoles.Min(b => b.TopDepth);
                float maxDepth = wellData.BoreHoles.Max(b => b.BottomDepth);
                depthSystem = new DepthTransform(minDepth, maxDepth, unitCode: "ft");
            }

            // Initialize renderers
            pathCalculator = new WellborePathCalculator(depthSystem, this.configuration);
            equipmentRenderer = new EquipmentRenderer(depthSystem, this.configuration);
            annotationRenderer = new AnnotationRenderer(depthSystem, this.configuration);

            // Initialize path storage
            outerWellborePaths = new Dictionary<int, List<SKPoint>>();
            innerWellborePaths = new Dictionary<int, List<SKPoint>>();
            tubingPaths = new Dictionary<(int BoreholeIndex, int TubeIndex), List<SKPoint>>();
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

            var layout = CreateLayout();
            PrepareGeometry(layout);

            // Apply viewport transformation
            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                // Render depth scale if enabled
                if (ShowDepthScale)
                {
                    annotationRenderer.RenderDepthScale(canvas, layout.Width, layout.Height, depthSystem);
                }

                // Render grid if enabled
                if (ShowGrid)
                {
                    annotationRenderer.RenderGrid(canvas, layout.Width, layout.Height, depthSystem, configuration.Theme.GridColor);
                }

                // Render each borehole
                for (int i = 0; i < wellData.BoreHoles.Count; i++)
                {
                    var borehole = wellData.BoreHoles[i];
                    RenderBorehole(canvas, borehole, i, layout.CenterX, layout.Height, layout.CalloutStartX);
                }

                // Render annotations if enabled
                if (ShowAnnotations)
                {
                    for (int i = 0; i < wellData.BoreHoles.Count; i++)
                    {
                        var borehole = wellData.BoreHoles[i];
                        annotationRenderer.RenderBoreholeLabels(canvas, borehole, i, layout.CenterX, depthSystem);
                    }
                }
            }
            finally
            {
                canvas.Restore();
            }
        }

        /// <inheritdoc />
        public LayerHitResult HitTest(SKPoint worldPoint, float worldTolerance)
        {
            if (wellData.BoreHoles == null || wellData.BoreHoles.Count == 0)
                return null;

            var layout = CreateLayout();
            PrepareGeometry(layout);

            var equipmentHit = HitTestEquipment(worldPoint, worldTolerance, layout);
            if (equipmentHit != null)
                return equipmentHit;

            return HitTestPerforations(worldPoint, worldTolerance, layout);
        }

        /// <summary>
        /// Renders a complete borehole with all components.
        /// </summary>
        private void RenderBorehole(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, float canvasHeight, float calloutStartX)
        {
            var deviationSurvey = ResolveDeviationSurvey(borehole);

            // Calculate wellbore path
            var wellborePath = pathCalculator.CalculateWellborePath(borehole, deviationSurvey, index, centerX, canvasHeight);
            outerWellborePaths[index] = wellborePath.OuterPath;
            innerWellborePaths[index] = wellborePath.CenterLine;

            // Render wellbore
            RenderWellbore(canvas, borehole, wellborePath);

            // Render casing
            if (borehole.Casing != null && borehole.Casing.Count > 0)
            {
                RenderCasing(canvas, borehole, index, centerX, canvasHeight);
            }

            // Render tubing
            if (borehole.Tubing != null && borehole.Tubing.Count > 0)
            {
                RenderTubing(canvas, borehole, index, centerX, canvasHeight);
            }

            // Render equipment on borehole
            if (borehole.Equip != null && borehole.Equip.Count > 0)
            {
                RenderBoreholeEquipment(canvas, borehole, index, canvasHeight);
            }

            // Render equipment on tubing
            if (borehole.Tubing != null)
            {
                for (int tubeIndex = 0; tubeIndex < borehole.Tubing.Count; tubeIndex++)
                {
                    var tubing = borehole.Tubing[tubeIndex];
                    if (tubing.Equip != null && tubing.Equip.Count > 0)
                    {
                        RenderTubingEquipment(canvas, borehole, index, tubeIndex, canvasHeight);
                    }
                }
            }

            // Render perforations
            if (borehole.Perforation != null && borehole.Perforation.Count > 0)
            {
                RenderPerforations(canvas, borehole, index, centerX, canvasHeight);
            }

            if (ShowAnnotations && borehole.Perforation != null && borehole.Perforation.Count > 0)
            {
                annotationRenderer.RenderPerforationCallouts(canvas, borehole.Perforation, wellborePath.CenterLine, calloutStartX, canvasHeight);
            }

            if (ShowAnnotations)
            {
                var equipmentCalloutTargets = BuildEquipmentCalloutTargets(borehole, index, wellborePath.CenterLine);
                annotationRenderer.RenderEquipmentCallouts(canvas, equipmentCalloutTargets, calloutStartX, canvasHeight);
            }

            if (ShowAnnotations && configuration.ShouldRenderSurveyMarkers() && deviationSurvey?.SurveyPoints?.Count > 1)
            {
                annotationRenderer.RenderSurveyMarkers(canvas, deviationSurvey, wellborePath.CenterLine, calloutStartX);
            }
        }

        /// <summary>
        /// Renders the wellbore.
        /// </summary>
        private void RenderWellbore(SKCanvas canvas, WellData_Borehole borehole, WellborePathResult pathResult)
        {
            var paint = new SKPaint
            {
                Color = configuration.Theme.GetColor("Wellbore", ColorPalette.WellSchematic.Wellbore),
                Style = SKPaintStyle.Stroke,
                StrokeWidth = configuration.WellboreStrokeWidth,
                IsAntialias = true
            };

            // Draw outer wellbore path
            var outerPath = new SKPath();
            if (pathResult.OuterPath.Count > 0)
            {
                outerPath.MoveTo(pathResult.OuterPath[0]);
                for (int i = 1; i < pathResult.OuterPath.Count; i++)
                {
                    outerPath.LineTo(pathResult.OuterPath[i]);
                }
                outerPath.Close();
            }
            canvas.DrawPath(outerPath, paint);

            // Draw inner wellbore path (if different)
            if (pathResult.InnerPath.Count > 0 && pathResult.InnerPath != pathResult.OuterPath)
            {
                var innerPath = new SKPath();
                innerPath.MoveTo(pathResult.InnerPath[0]);
                for (int i = 1; i < pathResult.InnerPath.Count; i++)
                {
                    innerPath.LineTo(pathResult.InnerPath[i]);
                }
                innerPath.Close();
                canvas.DrawPath(innerPath, paint);
            }
        }

        /// <summary>
        /// Renders casing.
        /// </summary>
        private void RenderCasing(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, float canvasHeight)
        {
            if (!innerWellborePaths.TryGetValue(index, out var centerLine) || centerLine.Count < 2)
                return;

            var casingList = borehole.Casing.OrderByDescending(c => c.BottomDepth).ToList();
            int offset = 1;

            foreach (var casing in casingList)
            {
                float topY = depthSystem.ToScreenY(casing.TopDepth, canvasHeight);
                float bottomY = depthSystem.ToScreenY(casing.BottomDepth, canvasHeight);
                float casingOffset = borehole.OuterDiameterOffset + (configuration.CasingSpacing * offset);
                offset++;
                var centerSegment = PathHelper.GetPathSegment(centerLine, topY, bottomY);
                if (centerSegment.Count < 2)
                    continue;

                var leftCasingPath = PathHelper.CreateParallelPath(centerSegment, -casingOffset);
                var rightCasingPath = PathHelper.CreateParallelPath(centerSegment, casingOffset);

                casing.outerCasingPoints = new Dictionary<int, List<(float x, float y)>>
                {
                    [0] = ToTuplePoints(leftCasingPath)
                };
                casing.innerCasingPoints = new Dictionary<int, List<(float x, float y)>>
                {
                    [1] = ToTuplePoints(rightCasingPath)
                };

                var paint = new SKPaint
                {
                    Color = configuration.Theme.GetColor("Casing", ColorPalette.WellSchematic.Casing),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = configuration.CasingStrokeWidth,
                    IsAntialias = true
                };

                DrawPolyline(canvas, leftCasingPath, paint);
                DrawPolyline(canvas, rightCasingPath, paint);

                if (leftCasingPath.Count > 0 && rightCasingPath.Count > 0)
                {
                    canvas.DrawLine(leftCasingPath[0], rightCasingPath[0], paint);
                    canvas.DrawLine(leftCasingPath[^1], rightCasingPath[^1], paint);
                }
            }
        }

        /// <summary>
        /// Renders tubing.
        /// </summary>
        private void RenderTubing(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, float canvasHeight)
        {
            var tubes = borehole.Tubing;
            if (tubes == null || tubes.Count == 0)
                return;

            if (!innerWellborePaths.TryGetValue(index, out var centerLine) || centerLine.Count < 2)
                return;

            float totalPadding = (tubes.Count - 1) * configuration.PaddingBetweenTubes + 2 * configuration.PaddingToSide;
            float wellboreWidth = borehole.OuterDiameterOffset * 2;
            float tubeSpace = (wellboreWidth - totalPadding) / tubes.Count;

            for (int tubeIndex = 0; tubeIndex < tubes.Count; tubeIndex++)
            {
                var tube = tubes[tubeIndex];
                float tubeLeftOffset = -borehole.OuterDiameterOffset + configuration.PaddingToSide + tubeIndex * (tubeSpace + configuration.PaddingBetweenTubes);
                float tubeCenterOffset = tubeLeftOffset + tubeSpace / 2.0f;
                var tubePath = pathCalculator.CalculateTubingPath(borehole, tubeIndex, tubeCenterOffset, centerLine, canvasHeight);
                tubingPaths[(index, tubeIndex)] = tubePath;
                tube.StoredPoints = ToTuplePoints(tubePath);
                float tubeStrokeWidth = Math.Max(configuration.TubingStrokeWidth, tube.Diameter * configuration.DiameterScale);

                var paint = new SKPaint
                {
                    Color = configuration.Theme.GetColor("Tubing", ColorPalette.WellSchematic.Tubing),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = tubeStrokeWidth,
                    StrokeCap = SKStrokeCap.Round,
                    StrokeJoin = SKStrokeJoin.Round,
                    IsAntialias = true
                };

                DrawPolyline(canvas, tubePath, paint);
            }
        }

        /// <summary>
        /// Renders equipment on the borehole.
        /// </summary>
        private void RenderBoreholeEquipment(SKCanvas canvas, WellData_Borehole borehole, int index, float canvasHeight)
        {
            if (innerWellborePaths.ContainsKey(index))
            {
                var innerPath = innerWellborePaths[index];
                equipmentRenderer.RenderBoreholeEquipment(canvas, borehole.Equip, innerPath, canvasHeight, OnEquipmentClicked);
            }
        }

        /// <summary>
        /// Renders equipment on tubing.
        /// </summary>
        private void RenderTubingEquipment(SKCanvas canvas, WellData_Borehole borehole, int index, int tubeIndex, float canvasHeight)
        {
            if (tubingPaths.TryGetValue((index, tubeIndex), out var tubePath))
            {
                var tubing = borehole.Tubing[tubeIndex];
                equipmentRenderer.RenderTubingEquipment(canvas, tubing.Equip, tubePath, tubeIndex, canvasHeight, OnEquipmentClicked);
            }
        }

        /// <summary>
        /// Renders perforations.
        /// </summary>
        private void RenderPerforations(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, float canvasHeight)
        {
            var paint = new SKPaint
            {
                Color = configuration.Theme.GetColor("Perforation", ColorPalette.WellSchematic.Perforation),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            foreach (var perf in borehole.Perforation)
            {
                float topY = depthSystem.ToScreenY(perf.TopDepth, canvasHeight);
                float bottomY = depthSystem.ToScreenY(perf.BottomDepth, canvasHeight);
                float markWidth = configuration.PerforationMarkWidth;

                // Draw perforation marks (triangles pointing outward)
                if (innerWellborePaths.ContainsKey(index))
                {
                    var innerPath = innerWellborePaths[index];
                    var perfPath = PathHelper.GetPathSegment(innerPath, topY, bottomY);
                    if (perfPath.Count < 2)
                        continue;

                    var middlePoint = PathHelper.GetPointOnPathNormalized(perfPath, 0.5f);
                    var tangent = PathHelper.GetTangentOnPath(perfPath, middlePoint.Y);
                    float tangentLength = (float)Math.Sqrt(tangent.X * tangent.X + tangent.Y * tangent.Y);
                    if (tangentLength <= 0)
                        continue;

                    var tangentUnit = new SKPoint(tangent.X / tangentLength, tangent.Y / tangentLength);
                    var normal = PathHelper.GetNormalOnPath(perfPath, middlePoint.Y);
                    if ((middlePoint.X < centerX && normal.X > 0) || (middlePoint.X >= centerX && normal.X < 0))
                    {
                        normal = new SKPoint(-normal.X, -normal.Y);
                    }

                    float height = Math.Abs(bottomY - topY);

                    // Draw triangle aligned to the local wellbore tangent and outward normal.
                    var baseHalf = new SKPoint(tangentUnit.X * (height / 2.0f), tangentUnit.Y * (height / 2.0f));
                    var trianglePoints = new SKPoint[3];
                    trianglePoints[0] = new SKPoint(middlePoint.X - baseHalf.X, middlePoint.Y - baseHalf.Y);
                    trianglePoints[1] = new SKPoint(middlePoint.X + normal.X * markWidth, middlePoint.Y + normal.Y * markWidth);
                    trianglePoints[2] = new SKPoint(middlePoint.X + baseHalf.X, middlePoint.Y + baseHalf.Y);

                    canvas.DrawVertices(SKVertexMode.Triangles, trianglePoints, null, null, paint);
                }
            }
        }

        /// <summary>
        /// Handles equipment click events.
        /// </summary>
        private void OnEquipmentClicked(WellData_Equip equipment)
        {
            EquipmentClicked?.Invoke(this, new EquipmentClickedEventArgs(equipment));
        }

        private static void DrawPolyline(SKCanvas canvas, IReadOnlyList<SKPoint> points, SKPaint paint)
        {
            if (points == null || points.Count < 2)
                return;

            using var path = new SKPath();
            path.MoveTo(points[0]);
            for (int index = 1; index < points.Count; index++)
            {
                path.LineTo(points[index]);
            }

            canvas.DrawPath(path, paint);
        }

        /// <summary>
        /// Gets the bounding rectangle of the well schematic.
        /// </summary>
        /// <returns>The bounding rectangle.</returns>
        public override SKRect GetBounds()
        {
            if (wellData.BoreHoles == null || wellData.BoreHoles.Count == 0)
                return new SKRect(0, 0, 0, 0);

            return CreateLayout().Bounds;
        }

        private void PrepareGeometry(SchematicLayout layout)
        {
            outerWellborePaths.Clear();
            innerWellborePaths.Clear();
            tubingPaths.Clear();

            for (int index = 0; index < wellData.BoreHoles.Count; index++)
            {
                var borehole = wellData.BoreHoles[index];
                var deviationSurvey = ResolveDeviationSurvey(borehole);
                var wellborePath = pathCalculator.CalculateWellborePath(borehole, deviationSurvey, index, layout.CenterX, layout.Height);
                outerWellborePaths[index] = wellborePath.OuterPath;
                innerWellborePaths[index] = wellborePath.CenterLine;

                if (borehole.Tubing == null || borehole.Tubing.Count == 0)
                    continue;

                float totalPadding = (borehole.Tubing.Count - 1) * configuration.PaddingBetweenTubes + 2 * configuration.PaddingToSide;
                float wellboreWidth = borehole.OuterDiameterOffset * 2;
                float tubeSpace = (wellboreWidth - totalPadding) / borehole.Tubing.Count;

                for (int tubeIndex = 0; tubeIndex < borehole.Tubing.Count; tubeIndex++)
                {
                    float tubeLeftOffset = -borehole.OuterDiameterOffset + configuration.PaddingToSide + tubeIndex * (tubeSpace + configuration.PaddingBetweenTubes);
                    float tubeCenterOffset = tubeLeftOffset + tubeSpace / 2.0f;
                    tubingPaths[(index, tubeIndex)] = pathCalculator.CalculateTubingPath(borehole, tubeIndex, tubeCenterOffset, wellborePath.CenterLine, layout.Height);
                }
            }
        }

        private LayerHitResult HitTestEquipment(SKPoint worldPoint, float worldTolerance, SchematicLayout layout)
        {
            LayerHitResult closestHit = null;

            for (int index = 0; index < wellData.BoreHoles.Count; index++)
            {
                var borehole = wellData.BoreHoles[index];
                if (!innerWellborePaths.TryGetValue(index, out var centerLine) || centerLine.Count < 2)
                    continue;

                foreach (var target in BuildEquipmentCalloutTargets(borehole, index, centerLine))
                {
                    var candidate = TryHitEquipmentTarget(target, worldPoint, worldTolerance, layout.Height);
                    if (candidate != null && (closestHit == null || candidate.Distance < closestHit.Distance))
                        closestHit = candidate;
                }

                var labelLayouts = annotationRenderer.PrepareEquipmentCallouts(BuildEquipmentCalloutTargets(borehole, index, centerLine), layout.CalloutStartX, layout.Height);
                foreach (var calloutLayout in labelLayouts)
                {
                    var candidate = TryHitEquipmentCallout(calloutLayout, worldPoint, worldTolerance);
                    if (candidate != null && (closestHit == null || candidate.Distance < closestHit.Distance))
                        closestHit = candidate;
                }
            }

            return closestHit;
        }

        private LayerHitResult HitTestPerforations(SKPoint worldPoint, float worldTolerance, SchematicLayout layout)
        {
            LayerHitResult closestHit = null;

            for (int index = 0; index < wellData.BoreHoles.Count; index++)
            {
                var borehole = wellData.BoreHoles[index];
                if (borehole.Perforation == null || borehole.Perforation.Count == 0)
                    continue;

                if (!innerWellborePaths.TryGetValue(index, out var centerLine) || centerLine.Count < 2)
                    continue;

                foreach (var perforation in borehole.Perforation)
                {
                    var candidate = TryHitPerforation(perforation, centerLine, worldPoint, worldTolerance, layout.Height, index);
                    if (candidate != null && (closestHit == null || candidate.Distance < closestHit.Distance))
                        closestHit = candidate;
                }

                var labelLayouts = annotationRenderer.PreparePerforationCallouts(borehole.Perforation, centerLine, layout.CalloutStartX, layout.Height);
                foreach (var calloutLayout in labelLayouts)
                {
                    var candidate = TryHitPerforationCallout(calloutLayout, worldPoint, worldTolerance, index);
                    if (candidate != null && (closestHit == null || candidate.Distance < closestHit.Distance))
                        closestHit = candidate;
                }
            }

            return closestHit;
        }

        private LayerHitResult TryHitEquipmentTarget(EquipmentCalloutTarget target, SKPoint worldPoint, float worldTolerance, float canvasHeight)
        {
            if (target?.Equipment == null || target.Path == null || target.Path.Count < 2)
                return null;

            float topY = depthSystem.ToScreenY(target.Equipment.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(target.Equipment.BottomDepth, canvasHeight);
            float anchorY = (topY + bottomY) / 2.0f;
            float symbolHeight = Math.Max(10.0f, Math.Abs(bottomY - topY));
            float symbolWidth = target.Equipment.Diameter > 0
                ? Math.Max(10.0f, target.Equipment.Diameter)
                : Math.Max(10.0f, symbolHeight * 0.65f);

            var anchor = PathHelper.GetPointOnPath(target.Path, anchorY);
            float distance = SKPoint.Distance(worldPoint, anchor);
            float hitRadius = Math.Max(symbolWidth, symbolHeight) * 0.5f + worldTolerance;
            if (distance > hitRadius)
                return null;

            return new LayerHitResult(
                layerName: Name,
                featureId: ResolveFeatureId(target.Equipment.GuidID, target.Equipment.ID),
                featureLabel: ResolveEquipmentLabel(target),
                featureKind: "Equipment",
                worldAnchor: anchor,
                distance: distance,
                metadata: BuildEquipmentMetadata(target));
        }

        private LayerHitResult TryHitEquipmentCallout(AnnotationRenderer.EquipmentCalloutLayout layout, SKPoint worldPoint, float worldTolerance)
        {
            if (layout == null)
                return null;

            float distance = DistanceToRect(worldPoint, layout.LabelBounds);
            if (distance > worldTolerance)
                return null;

            return new LayerHitResult(
                layerName: Name,
                featureId: ResolveFeatureId(layout.Callout.Target.Equipment.GuidID, layout.Callout.Target.Equipment.ID),
                featureLabel: ResolveEquipmentLabel(layout.Callout.Target),
                featureKind: "Equipment",
                worldAnchor: layout.Callout.Anchor,
                distance: distance,
                metadata: BuildEquipmentMetadata(layout.Callout.Target));
        }

        private LayerHitResult TryHitPerforation(WellData_Perf perforation, List<SKPoint> centerLine, SKPoint worldPoint, float worldTolerance, float canvasHeight, int boreholeIndex)
        {
            if (perforation == null)
                return null;

            float topY = depthSystem.ToScreenY(perforation.TopDepth, canvasHeight);
            float bottomY = depthSystem.ToScreenY(perforation.BottomDepth, canvasHeight);
            var perfPath = PathHelper.GetPathSegment(centerLine, topY, bottomY);
            if (perfPath.Count < 2)
                return null;

            float distance = HitTestGeometry.DistanceToPolyline(worldPoint, perfPath);
            if (distance > configuration.PerforationMarkWidth + worldTolerance)
                return null;

            return new LayerHitResult(
                layerName: Name,
                featureId: ResolveFeatureId(perforation.GuidID, perforation.ID),
                featureLabel: BuildPerforationLabel(perforation),
                featureKind: "Perforation",
                worldAnchor: PathHelper.GetPointOnPathNormalized(perfPath, 0.5f),
                distance: distance,
                metadata: new Dictionary<string, string>
                {
                    ["BoreholeIndex"] = boreholeIndex.ToString(),
                    ["TopDepth"] = perforation.TopDepth.ToString("0.###"),
                    ["BottomDepth"] = perforation.BottomDepth.ToString("0.###"),
                    ["PerfType"] = perforation.PerfType ?? string.Empty,
                    ["CompletionCode"] = perforation.CompletionCode ?? string.Empty
                });
        }

        private LayerHitResult TryHitPerforationCallout(AnnotationRenderer.PerforationCalloutLayout layout, SKPoint worldPoint, float worldTolerance, int boreholeIndex)
        {
            if (layout == null)
                return null;

            float distance = DistanceToRect(worldPoint, layout.LabelBounds);
            if (distance > worldTolerance)
                return null;

            var perforation = layout.Callout.Perforation;
            return new LayerHitResult(
                layerName: Name,
                featureId: ResolveFeatureId(perforation.GuidID, perforation.ID),
                featureLabel: BuildPerforationLabel(perforation),
                featureKind: "Perforation",
                worldAnchor: layout.Callout.Anchor,
                distance: distance,
                metadata: new Dictionary<string, string>
                {
                    ["BoreholeIndex"] = boreholeIndex.ToString(),
                    ["TopDepth"] = perforation.TopDepth.ToString("0.###"),
                    ["BottomDepth"] = perforation.BottomDepth.ToString("0.###"),
                    ["PerfType"] = perforation.PerfType ?? string.Empty,
                    ["CompletionCode"] = perforation.CompletionCode ?? string.Empty
                });
        }

        private SchematicLayout CreateLayout()
        {
            float layoutHeight = Math.Max(MinimumLayoutHeight, DefaultLayoutHeight * Math.Max(0.1f, configuration.DepthScale));
            var extent = MeasureContentExtent(layoutHeight);
            float leftPadding = HorizontalLayoutPadding - Math.Min(0, extent.MinX);
            float centerX = leftPadding;
            float calloutWidth = ShowAnnotations ? configuration.GetEffectiveCalloutWidth() : 0;
            float rightPadding = HorizontalLayoutPadding + calloutWidth + (ShowDepthScale ? configuration.DepthScaleWidth + RightOverlayPadding : 0);
            float contentRight = centerX + extent.MaxX;
            float layoutWidth = Math.Max(300.0f, contentRight + rightPadding);
            float calloutStartX = layoutWidth - (ShowDepthScale ? configuration.DepthScaleWidth + RightOverlayPadding : HorizontalLayoutPadding) - calloutWidth;

            return new SchematicLayout(layoutWidth, layoutHeight, centerX, calloutStartX);
        }

        private ContentExtent MeasureContentExtent(float layoutHeight)
        {
            float minX = 0;
            float maxX = 0;

            for (int index = 0; index < wellData.BoreHoles.Count; index++)
            {
                var borehole = wellData.BoreHoles[index];
                var deviationSurvey = ResolveDeviationSurvey(borehole);
                var previewPath = pathCalculator.CalculateWellborePath(borehole, deviationSurvey, index, 0, layoutHeight);

                IncludePoints(previewPath.OuterPath, ref minX, ref maxX);
                IncludePoints(previewPath.CenterLine, ref minX, ref maxX);

                float casingOffset = borehole.Casing != null && borehole.Casing.Count > 0
                    ? borehole.OuterDiameterOffset + (configuration.CasingSpacing * borehole.Casing.Count)
                    : borehole.OuterDiameterOffset;

                float tubingOffset = borehole.Tubing != null && borehole.Tubing.Count > 0
                    ? borehole.OuterDiameterOffset + configuration.PaddingToSide
                    : 0;

                float halfWidthMargin = Math.Max(casingOffset, tubingOffset) + configuration.PerforationMarkWidth + SymbolPadding;
                minX = Math.Min(minX, previewPath.CenterLine.Min(point => point.X) - halfWidthMargin);
                maxX = Math.Max(maxX, previewPath.CenterLine.Max(point => point.X) + halfWidthMargin);
            }

            return new ContentExtent(minX, maxX);
        }

        private static void IncludePoints(IEnumerable<SKPoint> points, ref float minX, ref float maxX)
        {
            foreach (var point in points)
            {
                minX = Math.Min(minX, point.X);
                maxX = Math.Max(maxX, point.X);
            }
        }

        private Dictionary<string, DeviationSurvey> BuildDeviationSurveyLookup(IDictionary<string, DeviationSurvey> surveys)
        {
            var lookup = new Dictionary<string, DeviationSurvey>(StringComparer.OrdinalIgnoreCase);
            if (surveys == null)
                return lookup;

            foreach (var entry in surveys)
            {
                var survey = entry.Value;
                if (survey == null)
                    continue;

                var report = DataNormalizationValidator.ValidateDeviationSurvey(survey);
                if (report.HasErrors)
                    throw new DrawingValidationException($"Deviation survey validation failed: {report.BuildSummary()}", report);

                RegisterDeviationSurveyKeys(lookup, entry.Key, survey);
            }

            return lookup;
        }

        private DeviationSurvey ResolveDeviationSurvey(WellData_Borehole borehole)
        {
            if (borehole == null || deviationSurveys.Count == 0)
                return null;

            var candidates = new[]
            {
                borehole.UBHI,
                borehole.GuidID,
                !string.IsNullOrWhiteSpace(wellData?.UWI) && !string.IsNullOrWhiteSpace(borehole.UBHI) ? $"{wellData.UWI}_{borehole.UBHI}" : null,
                !string.IsNullOrWhiteSpace(wellData?.UWI) && !string.IsNullOrWhiteSpace(borehole.GuidID) ? $"{wellData.UWI}_{borehole.GuidID}" : null
            };

            foreach (var candidate in candidates)
            {
                if (!string.IsNullOrWhiteSpace(candidate) && deviationSurveys.TryGetValue(candidate, out var survey))
                    return survey;
            }

            return deviationSurveys.Values.FirstOrDefault(survey =>
                string.Equals(survey.BoreholeIdentifier, borehole.UBHI, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(survey.BoreholeIdentifier, borehole.GuidID, StringComparison.OrdinalIgnoreCase));
        }

        private static void RegisterDeviationSurveyKeys(Dictionary<string, DeviationSurvey> lookup, string explicitKey, DeviationSurvey survey)
        {
            if (!string.IsNullOrWhiteSpace(explicitKey))
                lookup[explicitKey] = survey;

            if (!string.IsNullOrWhiteSpace(survey.BoreholeIdentifier))
                lookup[survey.BoreholeIdentifier] = survey;

            if (!string.IsNullOrWhiteSpace(survey.WellIdentifier) && !string.IsNullOrWhiteSpace(survey.BoreholeIdentifier))
                lookup[$"{survey.WellIdentifier}_{survey.BoreholeIdentifier}"] = survey;
        }

        private List<EquipmentCalloutTarget> BuildEquipmentCalloutTargets(WellData_Borehole borehole, int index, List<SKPoint> centerLine)
        {
            var targets = new List<EquipmentCalloutTarget>();

            if (borehole.Equip != null)
            {
                targets.AddRange(borehole.Equip
                    .Where(equipment => equipment != null)
                    .Select(equipment => new EquipmentCalloutTarget(equipment, centerLine)));
            }

            if (borehole.Tubing != null)
            {
                for (int tubeIndex = 0; tubeIndex < borehole.Tubing.Count; tubeIndex++)
                {
                    var tubing = borehole.Tubing[tubeIndex];
                    if (tubing?.Equip == null || tubing.Equip.Count == 0)
                        continue;

                    if (!tubingPaths.TryGetValue((index, tubeIndex), out var tubePath) || tubePath.Count < 2)
                        continue;

                    string prefix = $"T{tubeIndex + 1}";
                    targets.AddRange(tubing.Equip
                        .Where(equipment => equipment != null)
                        .Select(equipment => new EquipmentCalloutTarget(equipment, tubePath, prefix)));
                }
            }

            return targets;
        }

        private static string ResolveFeatureId(string guidId, int numericId)
        {
            if (!string.IsNullOrWhiteSpace(guidId))
                return guidId;

            return numericId > 0 ? numericId.ToString() : Guid.NewGuid().ToString("N");
        }

        private static string ResolveEquipmentLabel(EquipmentCalloutTarget target)
        {
            var equipment = target.Equipment;
            string label = FirstNonEmpty(equipment.EquipmentName, equipment.EquipmentType, equipment.EquipmentDescription, equipment.ToolTipText, "Equipment");
            return string.IsNullOrWhiteSpace(target.Prefix) ? label : target.Prefix + ": " + label;
        }

        private static IReadOnlyDictionary<string, string> BuildEquipmentMetadata(EquipmentCalloutTarget target)
        {
            var equipment = target.Equipment;
            var metadata = new Dictionary<string, string>
            {
                ["EquipmentType"] = equipment.EquipmentType ?? string.Empty,
                ["EquipmentName"] = equipment.EquipmentName ?? string.Empty,
                ["EquipmentDescription"] = equipment.EquipmentDescription ?? string.Empty,
                ["EquipmentStatus"] = equipment.EquipmentStatus ?? string.Empty,
                ["TopDepth"] = equipment.TopDepth.ToString("0.###"),
                ["BottomDepth"] = equipment.BottomDepth.ToString("0.###")
            };

            if (!string.IsNullOrWhiteSpace(target.Prefix))
                metadata["Tube"] = target.Prefix;

            return metadata;
        }

        private static string BuildPerforationLabel(WellData_Perf perforation)
        {
            string interval = $"{perforation.TopDepth:0}-{perforation.BottomDepth:0} MD";

            if (!string.IsNullOrWhiteSpace(perforation.CompletionCode))
                return $"Perf {interval} [{perforation.CompletionCode}]";

            if (!string.IsNullOrWhiteSpace(perforation.PerfType))
                return $"Perf {interval} {perforation.PerfType}";

            return $"Perf {interval}";
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                    return value.Trim();
            }

            return string.Empty;
        }

        private static float DistanceToRect(SKPoint point, SKRect rect)
        {
            float deltaX = 0f;
            if (point.X < rect.Left)
                deltaX = rect.Left - point.X;
            else if (point.X > rect.Right)
                deltaX = point.X - rect.Right;

            float deltaY = 0f;
            if (point.Y < rect.Top)
                deltaY = rect.Top - point.Y;
            else if (point.Y > rect.Bottom)
                deltaY = point.Y - rect.Bottom;

            return (float)Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));
        }

        private static List<(float x, float y)> ToTuplePoints(IReadOnlyList<SKPoint> points)
        {
            var tuples = new List<(float x, float y)>(points.Count);

            for (int index = 0; index < points.Count; index++)
            {
                var point = points[index];
                tuples.Add((point.X, point.Y));
            }

            return tuples;
        }

        private readonly record struct SchematicLayout(float Width, float Height, float CenterX, float CalloutStartX)
        {
            public SKRect Bounds => new SKRect(0, 0, Width, Height);
        }

        private readonly record struct ContentExtent(float MinX, float MaxX);
    }

    /// <summary>
    /// Event arguments for equipment click events.
    /// </summary>
    public class EquipmentClickedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the clicked equipment.
        /// </summary>
        public WellData_Equip Equipment { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EquipmentClickedEventArgs"/> class.
        /// </summary>
        /// <param name="equipment">The clicked equipment.</param>
        public EquipmentClickedEventArgs(WellData_Equip equipment)
        {
            Equipment = equipment ?? throw new ArgumentNullException(nameof(equipment));
        }
    }
}

