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
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Rendering;
using Beep.OilandGas.Drawing.Visualizations.WellSchematic.Configuration;

namespace Beep.OilandGas.Drawing.Visualizations.WellSchematic
{
    /// <summary>
    /// Enhanced well schematic renderer with full support for vertical and deviated wellbores,
    /// casing, tubing, equipment, perforations, and annotations.
    /// </summary>
    public class EnhancedWellSchematicRenderer : LayerBase
    {
        private readonly WellData wellData;
        private readonly WellSchematicConfiguration configuration;
        private readonly CoordinateSystem depthSystem;
        private readonly WellborePathCalculator pathCalculator;
        private readonly EquipmentRenderer equipmentRenderer;
        private readonly AnnotationRenderer annotationRenderer;
        private readonly Dictionary<int, List<SKPoint>> outerWellborePaths;
        private readonly Dictionary<int, List<SKPoint>> innerWellborePaths;
        private readonly Dictionary<int, List<SKPoint>> tubingPaths;

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
        public EnhancedWellSchematicRenderer(WellData wellData, WellSchematicConfiguration configuration = null)
            : base("Enhanced Well Schematic")
        {
            if (wellData == null)
                throw new ArgumentNullException(nameof(wellData));

            this.wellData = wellData;
            this.configuration = configuration ?? WellSchematicConfiguration.Default;

            // Validate well data
            WellDataValidator.ValidateWellData(wellData);

            // Create depth coordinate system
            if (wellData.BoreHoles != null && wellData.BoreHoles.Count > 0)
            {
                float minDepth = wellData.BoreHoles.Min(b => b.TopDepth);
                float maxDepth = wellData.BoreHoles.Max(b => b.BottomDepth);
                depthSystem = CoordinateSystem.CreateDepthSystem(minDepth, maxDepth, "feet");
            }

            // Initialize renderers
            pathCalculator = new WellborePathCalculator(depthSystem, this.configuration);
            equipmentRenderer = new EquipmentRenderer(depthSystem, this.configuration);
            annotationRenderer = new AnnotationRenderer(depthSystem, this.configuration);

            // Initialize path storage
            outerWellborePaths = new Dictionary<int, List<SKPoint>>();
            innerWellborePaths = new Dictionary<int, List<SKPoint>>();
            tubingPaths = new Dictionary<int, List<SKPoint>>();
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

            // Calculate canvas dimensions
            float canvasWidth = canvas.DeviceClipBounds.Width;
            float canvasHeight = canvas.DeviceClipBounds.Height;
            float centerX = canvasWidth / 2.0f;

            // Apply viewport transformation
            canvas.Save();
            canvas.SetMatrix(viewport.GetMatrix());

            try
            {
                // Render depth scale if enabled
                if (ShowDepthScale)
                {
                    annotationRenderer.RenderDepthScale(canvas, canvasWidth, canvasHeight, depthSystem);
                }

                // Render grid if enabled
                if (ShowGrid)
                {
                    annotationRenderer.RenderGrid(canvas, canvasWidth, canvasHeight, depthSystem, configuration.Theme.GridColor);
                }

                // Render each borehole
                for (int i = 0; i < wellData.BoreHoles.Count; i++)
                {
                    var borehole = wellData.BoreHoles[i];
                    RenderBorehole(canvas, borehole, i, centerX, canvasWidth, canvasHeight);
                }

                // Render annotations if enabled
                if (ShowAnnotations)
                {
                    for (int i = 0; i < wellData.BoreHoles.Count; i++)
                    {
                        var borehole = wellData.BoreHoles[i];
                        annotationRenderer.RenderBoreholeLabels(canvas, borehole, i, centerX, depthSystem);
                    }
                }
            }
            finally
            {
                canvas.Restore();
            }
        }

        /// <summary>
        /// Renders a complete borehole with all components.
        /// </summary>
        private void RenderBorehole(SKCanvas canvas, WellData_Borehole borehole, int index, float centerX, float canvasWidth, float canvasHeight)
        {
            // Calculate wellbore path
            var wellborePath = pathCalculator.CalculateWellborePath(borehole, index, centerX, canvasHeight);
            outerWellborePaths[index] = wellborePath.OuterPath;
            innerWellborePaths[index] = wellborePath.InnerPath;

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
            var casingList = borehole.Casing.OrderByDescending(c => c.BottomDepth).ToList();
            int offset = 1;

            foreach (var casing in casingList)
            {
                float topY = depthSystem.ToScreenY(casing.TopDepth, canvasHeight);
                float bottomY = depthSystem.ToScreenY(casing.BottomDepth, canvasHeight);
                float casingOffset = borehole.OuterDiameterOffset + (configuration.CasingSpacing * offset);
                offset++;

                var paint = new SKPaint
                {
                    Color = configuration.Theme.GetColor("Casing", ColorPalette.WellSchematic.Casing),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = configuration.CasingStrokeWidth,
                    IsAntialias = true
                };

                // Draw left and right casing lines
                SKPoint topLeft = new SKPoint(centerX - casingOffset, topY);
                SKPoint bottomLeft = new SKPoint(centerX - casingOffset, bottomY);
                SKPoint topRight = new SKPoint(centerX + casingOffset, topY);
                SKPoint bottomRight = new SKPoint(centerX + casingOffset, bottomY);

                canvas.DrawLine(topLeft, bottomLeft, paint);
                canvas.DrawLine(topRight, bottomRight, paint);

                // Draw top and bottom caps
                canvas.DrawLine(topLeft, topRight, paint);
                canvas.DrawLine(bottomLeft, bottomRight, paint);
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

            float totalPadding = (tubes.Count - 1) * configuration.PaddingBetweenTubes + 2 * configuration.PaddingToSide;
            float wellboreWidth = borehole.OuterDiameterOffset * 2;
            float tubeSpace = (wellboreWidth - totalPadding) / tubes.Count;

            for (int tubeIndex = 0; tubeIndex < tubes.Count; tubeIndex++)
            {
                var tube = tubes[tubeIndex];
                var tubePath = pathCalculator.CalculateTubingPath(borehole, index, tubeIndex, tubeSpace, centerX, canvasHeight);
                tubingPaths[tubeIndex] = tubePath;

                var paint = new SKPaint
                {
                    Color = configuration.Theme.GetColor("Tubing", ColorPalette.WellSchematic.Tubing),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = configuration.TubingStrokeWidth,
                    IsAntialias = true
                };

                var path = new SKPath();
                if (tubePath.Count > 0)
                {
                    path.MoveTo(tubePath[0]);
                    for (int i = 1; i < tubePath.Count; i++)
                    {
                        path.LineTo(tubePath[i]);
                    }
                    path.Close();
                }
                canvas.DrawPath(path, paint);
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
            if (tubingPaths.ContainsKey(tubeIndex))
            {
                var tubePath = tubingPaths[tubeIndex];
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
                    var topPoint = PathHelper.GetPointOnPath(innerPath, topY);
                    var bottomPoint = PathHelper.GetPointOnPath(innerPath, bottomY);
                    var middlePoint = new SKPoint((topPoint.X + bottomPoint.X) / 2, (topPoint.Y + bottomPoint.Y) / 2);

                    // Calculate direction (outward from wellbore)
                    float directionX = middlePoint.X < centerX ? -1 : 1;
                    float height = Math.Abs(bottomY - topY);

                    // Draw triangle pointing outward
                    var trianglePoints = new SKPoint[3];
                    trianglePoints[0] = new SKPoint(middlePoint.X, middlePoint.Y - height / 2);
                    trianglePoints[1] = new SKPoint(middlePoint.X + directionX * markWidth, middlePoint.Y);
                    trianglePoints[2] = new SKPoint(middlePoint.X, middlePoint.Y + height / 2);

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

            // Account for casing offsets
            float maxCasingOffset = 0;
            foreach (var borehole in wellData.BoreHoles)
            {
                if (borehole.Casing != null && borehole.Casing.Count > 0)
                {
                    float casingOffset = borehole.OuterDiameterOffset + (configuration.CasingSpacing * borehole.Casing.Count);
                    maxCasingOffset = Math.Max(maxCasingOffset, casingOffset);
                }
            }

            return new SKRect(
                -(maxDiameter / 2.0f + maxCasingOffset),
                minDepth,
                maxDiameter / 2.0f + maxCasingOffset,
                maxDepth);
        }
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

