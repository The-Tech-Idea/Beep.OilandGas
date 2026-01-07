using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SkiaSharp;
using Beep.OilandGas.HeatMap.ColorSchemes;
using Beep.OilandGas.HeatMap.Configuration;
using Beep.OilandGas.HeatMap.Interpolation;
using Beep.OilandGas.HeatMap.Visual;
using Beep.OilandGas.HeatMap.Performance;
using Beep.OilandGas.HeatMap.Annotations;
using Beep.OilandGas.HeatMap.Statistics;
using Beep.OilandGas.HeatMap.Clustering;
using Beep.OilandGas.HeatMap.Layers;
using Beep.OilandGas.HeatMap.Filtering;
using Beep.OilandGas.HeatMap.Interaction;
using Beep.OilandGas.HeatMap.Analysis;
using Beep.OilandGas.HeatMap.Animation;
using Beep.OilandGas.HeatMap.Realtime;
using Beep.OilandGas.HeatMap.Export;
using Beep.OilandGas.HeatMap.Visual;
using Beep.OilandGas.HeatMap.Tools;
using Beep.OilandGas.HeatMap.ColorSchemes;
using Beep.OilandGas.HeatMap.Aggregation;
using Beep.OilandGas.HeatMap.Density;
using Beep.OilandGas.HeatMap.Visualization;
using Beep.OilandGas.HeatMap.Contour;

namespace Beep.OilandGas.HeatMap.Rendering
{
    /// <summary>
    /// Enhanced SkiaSharp-based renderer for heat maps with interpolation, color schemes, and visual elements.
    /// </summary>
    public class HeatMapRenderer
    {
        private readonly Configuration.HeatMapConfiguration configuration;
        private readonly List<HeatMapDataPoint> dataPoints;
        private SKColor[] colorScheme;
        private double minValue;
        private double maxValue;
        private double minX;
        private double maxX;
        private double minY;
        private double maxY;
        private double[,] interpolatedGrid;
        private bool gridComputed = false;
        private HeatMapAnnotations annotations;
        private List<(double x1, double y1, double x2, double y2)> contourLines;
        private List<Cluster> clusters;
        private List<HeatMapLayer> layers;
        private List<DataFilter> filters;
        private HeatMapInteraction interaction;
        private List<HeatMapDataPoint> outliers;
        private StatisticalSummary statistics;
        private TimeSeriesAnimation animation;
        private RealtimeDataManager realtimeManager;
        private double zoom = 1.0;
        private SKPoint panOffset = new SKPoint(0, 0);
        private HashSet<HeatMapDataPoint> highlightedPoints;
        private DateTime lastUpdateTime;
        private List<SvgImageOverlay> svgOverlays;
        private List<PieChartOverlay> pieChartOverlays;
        private List<BarChartOverlay> barChartOverlays;
        private BrushSelection brushSelection;
        private MeasurementTools measurementTools;
        private CrosshairReferenceLines crosshairReferenceLines;
        private List<HexbinCell> hexbinCells;
        private List<GridCell> gridCells;
        private double[,] densityGrid;
        private List<HistogramBin> histogramBins;
        private List<(double value, double density)> distributionCurve;
        private Dictionary<double, List<ContourLine>> contourMapLines;
        private List<ContourRegion> filledContourRegions;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeatMapRenderer"/> class.
        /// </summary>
        /// <param name="dataPoints">The data points to render.</param>
        /// <param name="configuration">The heat map configuration.</param>
        public HeatMapRenderer(List<HeatMapDataPoint> dataPoints, Configuration.HeatMapConfiguration configuration = null)
        {
            this.dataPoints = dataPoints ?? throw new ArgumentNullException(nameof(dataPoints));
            this.configuration = configuration ?? new Configuration.HeatMapConfiguration();
            this.annotations = new HeatMapAnnotations();
            this.layers = new List<HeatMapLayer>();
            this.filters = new List<DataFilter>();
            this.interaction = new HeatMapInteraction
            {
                MultiSelectEnabled = configuration.MultiSelectEnabled,
                TooltipsEnabled = configuration.TooltipsEnabled
            };
            this.highlightedPoints = new HashSet<HeatMapDataPoint>();
            this.lastUpdateTime = DateTime.Now;
            this.svgOverlays = new List<SvgImageOverlay>();
            this.pieChartOverlays = new List<PieChartOverlay>();
            this.barChartOverlays = new List<BarChartOverlay>();
            this.brushSelection = new BrushSelection();
            this.measurementTools = new MeasurementTools();
            this.crosshairReferenceLines = new CrosshairReferenceLines();
            
            Initialize();
        }

        /// <summary>
        /// Gets the annotations manager for adding custom annotations.
        /// </summary>
        public HeatMapAnnotations Annotations => annotations;

        /// <summary>
        /// Gets the interaction manager for selection and tooltips.
        /// </summary>
        public HeatMapInteraction Interaction => interaction;

        /// <summary>
        /// Gets the list of layers for multi-layer support.
        /// </summary>
        public List<HeatMapLayer> Layers => layers;

        /// <summary>
        /// Gets the list of filters applied to data points.
        /// </summary>
        public List<DataFilter> Filters => filters;

        /// <summary>
        /// Gets the list of SVG image overlays.
        /// </summary>
        public List<SvgImageOverlay> SvgOverlays => svgOverlays;

        /// <summary>
        /// Gets the list of pie chart overlays.
        /// </summary>
        public List<PieChartOverlay> PieChartOverlays => pieChartOverlays;

        /// <summary>
        /// Gets the list of bar chart overlays.
        /// </summary>
        public List<BarChartOverlay> BarChartOverlays => barChartOverlays;

        /// <summary>
        /// Gets the brush selection tool.
        /// </summary>
        public BrushSelection BrushSelection => brushSelection;

        /// <summary>
        /// Gets the measurement tools.
        /// </summary>
        public MeasurementTools MeasurementTools => measurementTools;

        /// <summary>
        /// Gets the crosshair and reference lines manager.
        /// </summary>
        public CrosshairReferenceLines CrosshairReferenceLines => crosshairReferenceLines;

        /// <summary>
        /// Gets the data points.
        /// </summary>
        public List<HeatMapDataPoint> GetDataPoints() => dataPoints;

        /// <summary>
        /// Gets the bounds of the data (min/max X and Y).
        /// </summary>
        public (double minX, double maxX, double minY, double maxY) GetBounds()
        {
            return (minX, maxX, minY, maxY);
        }

        /// <summary>
        /// Gets or sets the animation for time-series visualization.
        /// </summary>
        public TimeSeriesAnimation Animation
        {
            get => animation;
            set
            {
                animation = value;
                if (animation != null)
                {
                    // Update data points from current animation frame
                    UpdateFromAnimation();
                }
            }
        }

        /// <summary>
        /// Gets or sets the real-time data manager.
        /// </summary>
        public RealtimeDataManager RealtimeManager
        {
            get => realtimeManager;
            set
            {
                if (realtimeManager != null)
                {
                    realtimeManager.PointsAdded -= OnPointsAdded;
                    realtimeManager.PointsUpdated -= OnPointsUpdated;
                }

                realtimeManager = value;

                if (realtimeManager != null)
                {
                    realtimeManager.PointsAdded += OnPointsAdded;
                    realtimeManager.PointsUpdated += OnPointsUpdated;
                }
            }
        }

        /// <summary>
        /// Gets or sets the current zoom level.
        /// </summary>
        public double Zoom
        {
            get => zoom;
            set => zoom = Math.Max(0.1, Math.Min(100.0, value));
        }

        /// <summary>
        /// Gets or sets the current pan offset.
        /// </summary>
        public SKPoint PanOffset
        {
            get => panOffset;
            set => panOffset = value;
        }

        /// <summary>
        /// Initializes the renderer with data point analysis and color scheme.
        /// </summary>
        private void Initialize()
        {
            if (dataPoints.Count == 0)
                return;

            // Calculate value range
            minValue = dataPoints.Min(p => p.Value);
            maxValue = dataPoints.Max(p => p.Value);

            // Calculate spatial bounds
            minX = dataPoints.Min(p => p.X);
            maxX = dataPoints.Max(p => p.X);
            minY = dataPoints.Min(p => p.Y);
            maxY = dataPoints.Max(p => p.Y);

            // Initialize color scheme
            colorScheme = configuration.GetColorScheme();

            // Apply filters if any
            var filteredPoints = ApplyFilters(dataPoints);

            // Compute statistics
            statistics = DataAnalysis.CalculateSummary(filteredPoints);

            // Identify outliers if needed
            if (configuration.ShowOutliers)
            {
                outliers = StatisticalOverlays.IdentifyOutliers(filteredPoints);
            }

            // Compute clustering if enabled
            if (configuration.UseClustering)
            {
                ComputeClusters(filteredPoints);
            }

            // Compute interpolation grid if enabled
            if (configuration.UseInterpolation && configuration.InterpolationMethod != InterpolationMethodType.None)
            {
                ComputeInterpolationGrid();
                
                // Generate contour lines if enabled
                if (configuration.ShowContours && gridComputed)
                {
                    GenerateContourLines();
                }
            }

            // Compute aggregations if enabled
            if (configuration.UseHexbinAggregation)
            {
                ComputeHexbinAggregation(configuration.HexbinRadius, configuration.HexbinAggregationMethod);
            }

            if (configuration.UseGridAggregation)
            {
                ComputeGridAggregation(configuration.GridCellSize, configuration.GridAggregationMethod);
            }

            // Compute kernel density if enabled
            if (configuration.UseKernelDensity)
            {
                ComputeKernelDensity(configuration.KernelDensityGridWidth, 
                    configuration.KernelDensityGridHeight, 
                    configuration.KernelDensityBandwidth, 
                    configuration.KernelDensityType);
            }

            // Compute value distribution if enabled
            if (configuration.ShowValueDistribution)
            {
                ComputeValueDistribution(configuration.HistogramBins);
            }

            // Setup crosshair/reference lines
            crosshairReferenceLines.ShowCrosshair = configuration.ShowCrosshair;
            crosshairReferenceLines.ShowVerticalReference = configuration.ShowReferenceLines;
            crosshairReferenceLines.ShowHorizontalReference = configuration.ShowReferenceLines;
        }

        /// <summary>
        /// Computes the interpolation grid for smooth heat map rendering.
        /// </summary>
        private void ComputeInterpolationGrid()
        {
            if (dataPoints.Count == 0)
                return;

            int gridWidth = (int)((maxX - minX) / configuration.InterpolationCellSize) + 1;
            int gridHeight = (int)((maxY - minY) / configuration.InterpolationCellSize) + 1;

            interpolatedGrid = new double[gridWidth, gridHeight];

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    double targetX = minX + x * configuration.InterpolationCellSize;
                    double targetY = minY + y * configuration.InterpolationCellSize;

                    // Use unified interpolation method that handles both standard and enhanced methods
                    double value = InterpolationMethod.Interpolate(
                        configuration.InterpolationMethod,
                        dataPoints,
                        targetX,
                        targetY,
                        configuration.IdwPower,
                        configuration.IdwMaxDistance,
                        configuration.IdwNearestNeighbors > 0 ? configuration.IdwNearestNeighbors : 0);

                    interpolatedGrid[x, y] = value;
                }
            }

            gridComputed = true;
        }

        /// <summary>
        /// Renders the heat map to a SkiaSharp canvas.
        /// </summary>
        /// <param name="canvas">The SkiaSharp canvas to render to.</param>
        /// <param name="width">Canvas width in pixels.</param>
        /// <param name="height">Canvas height in pixels.</param>
        public void Render(SKCanvas canvas, float width, float height)
        {
            if (dataPoints.Count == 0)
                return;

            // Clear background
            canvas.Clear(configuration.BackgroundColor);

            // Save canvas state
            canvas.Save();

            try
            {
                // Calculate coordinate transformation
                float scaleX = width / (float)(maxX - minX);
                float scaleY = height / (float)(maxY - minY);
                float offsetX = (float)-minX * scaleX;
                float offsetY = (float)-minY * scaleY;

                // Apply transformation
                canvas.Translate(offsetX, offsetY);
                canvas.Scale(scaleX, scaleY);

                // Render interpolation grid if enabled
                if (configuration.UseInterpolation && gridComputed)
                {
                    RenderInterpolatedGrid(canvas, width, height);
                }

                // Render data points
                RenderDataPoints(canvas);

                // Restore transformation for UI elements
                canvas.Restore();

                // Render filled contours first (if enabled) - behind other elements
                if (configuration.UseFilledContours && filledContourRegions != null && filledContourRegions.Count > 0)
                {
                    RenderFilledContours(canvas, width, height);
                }

                // Render contour lines if enabled
                if (configuration.ShowContours && (contourMapLines != null || contourLines != null))
                {
                    RenderContourLines(canvas, width, height);
                }

                // Render hexbin aggregation if enabled
                if (configuration.UseHexbinAggregation && hexbinCells != null && hexbinCells.Count > 0)
                {
                    RenderHexbinAggregation(canvas, width, height);
                }

                // Render grid aggregation if enabled
                if (configuration.UseGridAggregation && gridCells != null && gridCells.Count > 0)
                {
                    RenderGridAggregation(canvas, width, height);
                }

                // Render kernel density if enabled
                if (configuration.UseKernelDensity && densityGrid != null)
                {
                    RenderKernelDensity(canvas, width, height);
                }

                // Render visual elements (grid, labels, legend, etc.)
                RenderVisualElements(canvas, width, height);

                // Render statistical overlays if enabled
                if (configuration.ShowStatisticalOverlays)
                {
                    RenderStatisticalOverlays(canvas, width, height);
                }

                // Render SVG image overlays
                if (configuration.ShowSvgOverlays && svgOverlays.Count > 0)
                {
                    RenderSvgOverlays(canvas, width, height);
                }

                // Render pie chart overlays
                if (configuration.ShowPieChartOverlays && pieChartOverlays.Count > 0)
                {
                    RenderPieChartOverlays(canvas, width, height);
                }

                // Render bar chart overlays
                if (configuration.ShowBarChartOverlays && barChartOverlays.Count > 0)
                {
                    RenderBarChartOverlays(canvas, width, height);
                }

                // Render interaction elements (selection, tooltips)
                if (configuration.InteractionEnabled)
                {
                    RenderInteraction(canvas, width, height);
                }

                // Render animation time indicator
                RenderAnimationTime(canvas, width, height);

                // Render brush selection
                if (brushSelection.IsActive)
                {
                    brushSelection.Render(canvas);
                }

                // Render measurement tools
                if (measurementTools.IsActive)
                {
                    measurementTools.Render(canvas);
                }

                // Render annotations (title, text, callouts, values)
                annotations.Render(canvas, width, height, minX, maxX, minY, maxY);
            }
            finally
            {
                canvas.Restore();
            }
        }

        /// <summary>
        /// Renders the interpolated grid as a smooth heat map.
        /// </summary>
        private void RenderInterpolatedGrid(SKCanvas canvas, float canvasWidth, float canvasHeight)
        {
            if (interpolatedGrid == null)
                return;

            int gridWidth = interpolatedGrid.GetLength(0);
            int gridHeight = interpolatedGrid.GetLength(1);

            float cellWidth = canvasWidth / gridWidth;
            float cellHeight = canvasHeight / gridHeight;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    double normalizedValue = NormalizeValue(interpolatedGrid[x, y]);
                    SKColor color = GetColorFromScheme(normalizedValue);

                    float rectX = x * cellWidth;
                    float rectY = y * cellHeight;

                    using (var paint = new SKPaint
                    {
                        Color = color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(rectX, rectY, cellWidth, cellHeight, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Renders individual data points.
        /// </summary>
        private void RenderDataPoints(SKCanvas canvas)
        {
            var pointsToRender = dataPoints;

            // Apply LOD if enabled (simplified - using all points for now)
            if (configuration.UseLOD && pointsToRender.Count > configuration.MaxRenderPoints)
            {
                double viewportArea = (maxX - minX) * (maxY - minY);
                pointsToRender = LevelOfDetail.ApplyAdaptiveLOD(
                    pointsToRender, 
                    1.0, // zoom level (could be passed as parameter)
                    viewportArea,
                    configuration.MaxRenderPoints, 
                    configuration.MaxDensity);
            }

            foreach (var point in pointsToRender)
            {
                RenderDataPoint(canvas, point);
            }
        }

        /// <summary>
        /// Renders a single data point.
        /// </summary>
        private void RenderDataPoint(SKCanvas canvas, HeatMapDataPoint point)
        {
            double normalizedValue = NormalizeValue(point.Value);
            SKColor color = GetColorFromScheme(normalizedValue);

            // Calculate point radius based on value
            float radius = (float)(configuration.MinPointRadius + 
                normalizedValue * (configuration.MaxPointRadius - configuration.MinPointRadius));

            using (var paint = new SKPaint
            {
                Color = color,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            })
            {
                canvas.DrawCircle((float)point.X, (float)point.Y, radius, paint);
            }

            // Draw label if enabled
            if (configuration.ShowLabels && !string.IsNullOrEmpty(point.Label))
            {
                using (var textPaint = new SKPaint
                {
                    Color = configuration.LabelColor,
                    TextSize = configuration.LabelFontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                })
                {
                    canvas.DrawText(
                        point.Label, 
                        (float)point.X, 
                        (float)point.Y - radius - textPaint.TextSize, 
                        textPaint);
                }
            }

            // Draw value annotation if enabled
            if (configuration.ShowValueAnnotations)
            {
                var valueAnnotation = new ValueAnnotation
                {
                    X = point.X,
                    Y = point.Y,
                    Value = point.Value,
                    Format = configuration.ValueAnnotationFormat,
                    FontSize = configuration.LabelFontSize * 0.9f,
                    Color = configuration.LabelColor,
                    Offset = new SKPoint(0, -(radius + configuration.LabelFontSize + 5))
                };
                annotations.AddValueAnnotation(valueAnnotation);
            }
        }

        /// <summary>
        /// Renders visual elements (grid, axis labels, legend, etc.).
        /// </summary>
        private void RenderVisualElements(SKCanvas canvas, float width, float height)
        {
            // Render grid
            if (configuration.ShowGrid)
            {
                VisualElements.DrawGrid(
                    canvas, 
                    width, 
                    height, 
                    configuration.GridSpacing, 
                    configuration.GridColor);
            }

            // Render axis labels
            if (configuration.ShowAxisLabels)
            {
                VisualElements.DrawAxisLabels(
                    canvas, 
                    width, 
                    height, 
                    configuration.XAxisLabel, 
                    configuration.YAxisLabel);
            }

            // Render axis ticks
            if (configuration.ShowAxisTicks)
            {
                var ticksConfig = new AxisTicksConfiguration
                {
                    TickCount = configuration.AxisTickCount,
                    ShowTicks = true,
                    ShowTickLabels = true
                };

                AxisTicks.RenderXAxisTicks(canvas, width, height, minX, maxX, ticksConfig);
                AxisTicks.RenderYAxisTicks(canvas, width, height, minY, maxY, ticksConfig);
            }

            // Render color scale legend
            if (configuration.ShowLegend)
            {
                var legend = new ColorScaleLegend(colorScheme, minValue, maxValue)
                {
                    X = configuration.LegendX,
                    Y = configuration.LegendY
                };
                legend.Draw(canvas);
            }

            // Render scale bar
            if (configuration.ShowScaleBar)
            {
                // Calculate appropriate scale bar length (10% of width)
                float scaleBarLength = width * 0.1f;
                double realWorldDistance = (maxX - minX) * 0.1; // 10% of data range
                VisualElements.DrawScaleBar(
                    canvas, 
                    width - scaleBarLength - 20, 
                    height - 40, 
                    scaleBarLength, 
                    realWorldDistance);
            }

            // Render north arrow
            if (configuration.ShowNorthArrow)
            {
                VisualElements.DrawNorthArrow(canvas, width - 30, 30);
            }

            // Render coordinate system info
            if (!string.IsNullOrEmpty(configuration.CoordinateSystem))
            {
                VisualElements.DrawCoordinateSystem(
                    canvas, 
                    10, 
                    height - 20, 
                    configuration.CoordinateSystem);
            }
        }

        /// <summary>
        /// Normalizes a value to the range [0, 1].
        /// </summary>
        private double NormalizeValue(double value)
        {
            if (maxValue == minValue)
                return 0.5;

            // First normalize to [0, 1]
            double normalized = Math.Max(0, Math.Min(1, (value - minValue) / (maxValue - minValue)));

            // Apply color mapping function
            if (configuration.ColorMappingFunction == ColorMappingFunction.Custom && 
                configuration.CustomColorMappingFunction != null)
            {
                return ColorMapping.ApplyCustomMapping(normalized, configuration.CustomColorMappingFunction);
            }
            else
            {
                return ColorMapping.ApplyMapping(normalized, configuration.ColorMappingFunction, 
                    configuration.ColorMappingParameter);
            }
        }

        /// <summary>
        /// Gets a color from the color scheme based on a normalized value [0, 1].
        /// </summary>
        private SKColor GetColorFromScheme(double normalizedValue)
        {
            if (colorScheme == null || colorScheme.Length == 0)
                return SKColors.Gray;

            int index = (int)(normalizedValue * (colorScheme.Length - 1));
            index = Math.Max(0, Math.Min(colorScheme.Length - 1, index));
            return colorScheme[index];
        }

        /// <summary>
        /// Updates the data points and reinitializes the renderer.
        /// </summary>
        public void UpdateDataPoints(List<HeatMapDataPoint> newDataPoints)
        {
            if (newDataPoints == null)
                throw new ArgumentNullException(nameof(newDataPoints));

            dataPoints.Clear();
            dataPoints.AddRange(newDataPoints);
            gridComputed = false;
            Initialize();
        }

        /// <summary>
        /// Updates the configuration and reinitializes the renderer.
        /// </summary>
        public void UpdateConfiguration(Configuration.HeatMapConfiguration newConfiguration)
        {
            if (newConfiguration == null)
                throw new ArgumentNullException(nameof(newConfiguration));

            // Update configuration properties
            var configType = typeof(Configuration.HeatMapConfiguration);
            foreach (var prop in configType.GetProperties())
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(configuration, prop.GetValue(newConfiguration));
                }
            }

            gridComputed = false;
            Initialize();
        }

        /// <summary>
        /// Gets the current value range.
        /// </summary>
        public (double Min, double Max) GetValueRange() => (minValue, maxValue);

        /// <summary>
        /// Gets the current spatial bounds.
        /// </summary>
        public (double MinX, double MaxX, double MinY, double MaxY) GetSpatialBounds() => 
            (minX, maxX, minY, maxY);

        /// <summary>
        /// Gets interpolated value using enhanced interpolation methods.
        /// </summary>
        private double GetEnhancedInterpolationValue(double targetX, double targetY)
        {
            // Use the unified Interpolate method from InterpolationMethod
            return InterpolationMethod.Interpolate(
                configuration.InterpolationMethod,
                dataPoints,
                targetX,
                targetY,
                configuration.IdwPower,
                configuration.IdwMaxDistance,
                configuration.IdwNearestNeighbors);
        }

        /// <summary>
        /// Generates contour lines from the interpolated grid.
        /// </summary>
        private void GenerateContourLines()
        {
            if (interpolatedGrid == null)
                return;

            // Calculate contour levels
            double[] contourLevels;
            if (configuration.ContourLevelsArray != null && configuration.ContourLevelsArray.Length > 0)
            {
                contourLevels = configuration.ContourLevelsArray;
            }
            else
            {
                contourLevels = ContourMap.GenerateContourLevels(minValue, maxValue, configuration.ContourLevels);
            }

            // Generate enhanced contour lines using ContourMap
            contourMapLines = ContourMap.GenerateContourLines(
                interpolatedGrid, 
                contourLevels, 
                minX, maxX, minY, maxY);

            // Also generate filled contours if enabled
            if (configuration.UseFilledContours)
            {
                filledContourRegions = ContourMap.GenerateFilledContours(
                    interpolatedGrid,
                    contourLevels,
                    minX, maxX, minY, maxY,
                    colorScheme);
            }

            // Keep legacy format for backward compatibility
            contourLines = new List<(double, double, double, double)>();
            foreach (var levelGroup in contourMapLines)
            {
                foreach (var contour in levelGroup.Value)
                {
                    for (int i = 0; i < contour.Points.Count - 1; i++)
                    {
                        contourLines.Add((
                            contour.Points[i].X,
                            contour.Points[i].Y,
                            contour.Points[i + 1].X,
                            contour.Points[i + 1].Y));
                    }
                }
            }
        }

        /// <summary>
        /// Renders contour lines on the canvas.
        /// </summary>
        private void RenderContourLines(SKCanvas canvas, float canvasWidth, float canvasHeight)
        {
            // Render filled contours first (if enabled)
            if (configuration.UseFilledContours && filledContourRegions != null && filledContourRegions.Count > 0)
            {
                float scaleX = canvasWidth / (float)(maxX - minX);
                float scaleY = canvasHeight / (float)(maxY - minY);
                float offsetX = (float)-minX * scaleX;
                float offsetY = (float)-minY * scaleY;

                ContourMap.RenderFilledContours(canvas, filledContourRegions, scaleX, scaleY, offsetX, offsetY);
            }

            // Render contour lines
            if (contourMapLines != null && contourMapLines.Count > 0)
            {
                float scaleX = canvasWidth / (float)(maxX - minX);
                float scaleY = canvasHeight / (float)(maxY - minY);
                float offsetX = (float)-minX * scaleX;
                float offsetY = (float)-minY * scaleY;

                ContourMap.RenderContourLines(
                    canvas,
                    contourMapLines,
                    scaleX, scaleY, offsetX, offsetY,
                    configuration.ContourColor,
                    configuration.ContourLineWidth,
                    configuration.ShowContourLabels,
                    configuration.ContourLabelFontSize);
            }
            else if (contourLines != null && contourLines.Count > 0)
            {
                // Fallback to legacy rendering
                // Calculate coordinate transformation
                float scaleX = canvasWidth / (float)(maxX - minX);
                float scaleY = canvasHeight / (float)(maxY - minY);
                float offsetX = (float)-minX * scaleX;
                float offsetY = (float)-minY * scaleY;

                using (var contourPaint = new SKPaint
                {
                    Color = configuration.ContourColor,
                    StrokeWidth = configuration.ContourLineWidth,
                    Style = SKPaintStyle.Stroke,
                    IsAntialias = true
                })
                {
                    // Draw contour lines
                    foreach (var (x1, y1, x2, y2) in contourLines)
                    {
                        float screenX1 = (float)(x1 * scaleX + offsetX);
                        float screenY1 = (float)(y1 * scaleY + offsetY);
                        float screenX2 = (float)(x2 * scaleX + offsetX);
                        float screenY2 = (float)(y2 * scaleY + offsetY);

                        canvas.DrawLine(screenX1, screenY1, screenX2, screenY2, contourPaint);
                    }
                }

                // Draw contour labels if enabled
                if (configuration.ShowContourLabels)
                {
                    float scaleX2 = canvasWidth / (float)(maxX - minX);
                    float scaleY2 = canvasHeight / (float)(maxY - minY);
                    float offsetX2 = (float)-minX * scaleX2;
                    float offsetY2 = (float)-minY * scaleY2;
                    RenderContourLabels(canvas, canvasWidth, canvasHeight, scaleX2, scaleY2, offsetX2, offsetY2);
                }
            }
        }

        /// <summary>
        /// Renders labels on contour lines.
        /// </summary>
        private void RenderContourLabels(SKCanvas canvas, float width, float height,
            float scaleX, float scaleY, float offsetX, float offsetY)
        {
            if (contourLines == null || contourLines.Count == 0)
                return;

            var labelPaint = new SKPaint
            {
                Color = configuration.ContourColor,
                TextSize = 10f,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true
            };

            // Sample some contour lines for labeling (avoid overcrowding)
            int labelInterval = Math.Max(1, contourLines.Count / 20);
            for (int i = 0; i < contourLines.Count; i += labelInterval)
            {
                var (x1, y1, x2, y2) = contourLines[i];
                float midX = (float)((x1 + x2) / 2 * scaleX + offsetX);
                float midY = (float)((y1 + y2) / 2 * scaleY + offsetY);

                // Calculate approximate contour value (simplified)
                double contourValue = minValue + (maxValue - minValue) * i / contourLines.Count;
                string label = contourValue.ToString("F1");

                // Draw background for label
                var bgPaint = new SKPaint
                {
                    Color = SKColors.White,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };

                var textBounds = new SKRect();
                labelPaint.MeasureText(label, ref textBounds);
                textBounds.Offset(midX, midY);
                textBounds.Inflate(4, 2);

                canvas.DrawRect(textBounds, bgPaint);
                canvas.DrawText(label, midX, midY, labelPaint);
            }
        }

        /// <summary>
        /// Applies all filters to data points.
        /// </summary>
        private List<HeatMapDataPoint> ApplyFilters(List<HeatMapDataPoint> points)
        {
            if (filters.Count == 0)
                return points;

            var filtered = points;
            foreach (var filter in filters)
            {
                filtered = filter.Apply(filtered);
            }
            return filtered;
        }

        /// <summary>
        /// Computes clusters from data points.
        /// </summary>
        private void ComputeClusters(List<HeatMapDataPoint> points)
        {
            if (points == null || points.Count == 0)
                return;

            clusters = configuration.ClusteringAlgorithm switch
            {
                ClusteringAlgorithm.GridBased =>
                    PointClustering.GridBasedClustering(points, configuration.ClusterCellSize),
                ClusteringAlgorithm.DistanceBased =>
                    PointClustering.DistanceBasedClustering(points, configuration.ClusterCellSize),
                ClusteringAlgorithm.KMeans =>
                    PointClustering.KMeansClustering(points, configuration.ClusterCount),
                _ => new List<Cluster>()
            };
        }

        /// <summary>
        /// Renders clusters on the canvas.
        /// </summary>
        private void RenderClusters(SKCanvas canvas, float width, float height)
        {
            if (clusters == null || clusters.Count == 0)
                return;

            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            var clusterPaint = new SKPaint
            {
                Color = configuration.ClusterColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            };

            var fillPaint = new SKPaint
            {
                Color = new SKColor(configuration.ClusterColor.Red, configuration.ClusterColor.Green, configuration.ClusterColor.Blue, 50),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            foreach (var cluster in clusters)
            {
                float centerX = (float)(cluster.CenterX * scaleX + offsetX);
                float centerY = (float)(cluster.CenterY * scaleY + offsetY);
                float radius = (float)(cluster.Radius * Math.Max(scaleX, scaleY));

                // Draw cluster circle
                canvas.DrawCircle(centerX, centerY, radius, fillPaint);
                canvas.DrawCircle(centerX, centerY, radius, clusterPaint);

                // Draw cluster label
                var labelPaint = new SKPaint
                {
                    Color = configuration.ClusterColor,
                    TextSize = 12f,
                    TextAlign = SKTextAlign.Center,
                    IsAntialias = true
                };
                canvas.DrawText($"C{cluster.Count}", centerX, centerY - radius - 5, labelPaint);
            }
        }

        /// <summary>
        /// Renders outliers on the canvas.
        /// </summary>
        private void RenderOutliers(SKCanvas canvas, float width, float height)
        {
            if (outliers == null || outliers.Count == 0)
                return;

            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            var outlierPaint = new SKPaint
            {
                Color = configuration.OutlierColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var strokePaint = new SKPaint
            {
                Color = configuration.OutlierColor,
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2f,
                IsAntialias = true
            };

            foreach (var outlier in outliers)
            {
                float x = (float)(outlier.X * scaleX + offsetX);
                float y = (float)(outlier.Y * scaleY + offsetY);
                float radius = 8f;

                // Draw outlier marker (X shape)
                canvas.DrawCircle(x, y, radius, outlierPaint);
                canvas.DrawLine(x - radius, y - radius, x + radius, y + radius, strokePaint);
                canvas.DrawLine(x - radius, y + radius, x + radius, y - radius, strokePaint);
            }
        }

        /// <summary>
        /// Renders multiple layers.
        /// </summary>
        private void RenderLayers(SKCanvas canvas, float width, float height)
        {
            // Sort layers by z-order
            var sortedLayers = layers.OrderBy(l => l.ZOrder).ToList();

            foreach (var layer in sortedLayers.Where(l => l.IsVisible))
            {
                canvas.Save();
                
                // Apply layer opacity using paint
                var layerPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, (byte)(255 * layer.Opacity))
                };
                canvas.SaveLayer(layerPaint);

                // Render layer points
                foreach (var point in layer.DataPoints)
                {
                    RenderDataPoint(canvas, point, layer);
                }

                canvas.Restore();
            }
        }

        /// <summary>
        /// Updates data points from animation current frame.
        /// </summary>
        private void UpdateFromAnimation()
        {
            if (animation == null)
                return;

            var currentFrame = animation.GetCurrentFrame();
            if (currentFrame != null && currentFrame.Count > 0)
            {
                dataPoints.Clear();
                dataPoints.AddRange(currentFrame);
                gridComputed = false;
                Initialize();
            }
        }

        /// <summary>
        /// Handles real-time points added event.
        /// </summary>
        private void OnPointsAdded(object sender, List<HeatMapDataPoint> points)
        {
            if (realtimeManager != null && realtimeManager.HighlightNewPoints)
            {
                foreach (var point in points)
                {
                    highlightedPoints.Add(point);
                }
                // Remove highlights after duration
                var removeTime = DateTime.Now.AddSeconds(realtimeManager.HighlightDurationSeconds);
                Task.Delay(TimeSpan.FromSeconds(realtimeManager.HighlightDurationSeconds))
                    .ContinueWith(_ => RemoveHighlights(points));
            }
        }

        /// <summary>
        /// Handles real-time points updated event.
        /// </summary>
        private void OnPointsUpdated(object sender, List<HeatMapDataPoint> points)
        {
            if (realtimeManager != null && realtimeManager.HighlightUpdatedPoints)
            {
                foreach (var point in points)
                {
                    highlightedPoints.Add(point);
                }
                Task.Delay(TimeSpan.FromSeconds(realtimeManager.HighlightDurationSeconds))
                    .ContinueWith(_ => RemoveHighlights(points));
            }
        }

        /// <summary>
        /// Removes highlights from points.
        /// </summary>
        private void RemoveHighlights(List<HeatMapDataPoint> points)
        {
            foreach (var point in points)
            {
                highlightedPoints.Remove(point);
            }
        }

        /// <summary>
        /// Renders a data point with layer-specific styling.
        /// </summary>
        private void RenderDataPoint(SKCanvas canvas, HeatMapDataPoint point, HeatMapLayer layer = null)
        {
            double normalizedValue = NormalizeValue(point.Value);
            
            // Use layer color scheme if available
            SKColor[] layerColorScheme = null;
            if (layer != null)
            {
                if (layer.CustomColors != null && layer.CustomColors.Length >= 2)
                {
                    layerColorScheme = ColorScheme.CreateCustom(layer.CustomColors, configuration.ColorSteps);
                }
                else
                {
                    layerColorScheme = ColorScheme.GetColorScheme(layer.ColorSchemeType, configuration.ColorSteps);
                }
            }

            SKColor color = layerColorScheme != null
                ? GetColorFromScheme(normalizedValue, layerColorScheme)
                : GetColorFromScheme(normalizedValue);

            float minRadius = layer?.MinPointRadius ?? configuration.MinPointRadius;
            float maxRadius = layer?.MaxPointRadius ?? configuration.MaxPointRadius;
            float radius = (float)(minRadius + normalizedValue * (maxRadius - minRadius));

            // Check if point should be highlighted (real-time updates)
            bool isHighlighted = highlightedPoints.Contains(point);

            // Draw shadow/glow for highlighted points
            if (isHighlighted && realtimeManager != null)
            {
                var glowPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 0, 100), // Yellow glow
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, radius * 0.5f)
                };
                canvas.DrawCircle((float)point.X, (float)point.Y, radius * 1.5f, glowPaint);
            }

            // Draw point with gradient for better visual appeal
            if (configuration.UsePointGradient)
            {
                RenderPointWithGradient(canvas, point, color, radius);
            }
            else
            {
                using (var paint = new SKPaint
                {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                })
                {
                    canvas.DrawCircle((float)point.X, (float)point.Y, radius, paint);
                }
            }

            // Draw highlight border for real-time points
            if (isHighlighted)
            {
                var highlightPaint = new SKPaint
                {
                    Color = SKColors.Yellow,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3f,
                    IsAntialias = true
                };
                canvas.DrawCircle((float)point.X, (float)point.Y, radius + 2, highlightPaint);
            }

            // Draw label if enabled
            if (configuration.ShowLabels && !string.IsNullOrEmpty(point.Label))
            {
                using (var textPaint = new SKPaint
                {
                    Color = configuration.LabelColor,
                    TextSize = configuration.LabelFontSize,
                    IsAntialias = true,
                    TextAlign = SKTextAlign.Center
                })
                {
                    canvas.DrawText(
                        point.Label,
                        (float)point.X,
                        (float)point.Y - radius - textPaint.TextSize,
                        textPaint);
                }
            }

            // Draw value annotation if enabled
            if (configuration.ShowValueAnnotations)
            {
                var valueAnnotation = new ValueAnnotation
                {
                    X = point.X,
                    Y = point.Y,
                    Value = point.Value,
                    Format = configuration.ValueAnnotationFormat,
                    FontSize = configuration.LabelFontSize * 0.9f,
                    Color = configuration.LabelColor,
                    Offset = new SKPoint(0, -(radius + configuration.LabelFontSize + 5))
                };
                annotations.AddValueAnnotation(valueAnnotation);
            }
        }

        /// <summary>
        /// Gets a color from a color scheme (with optional custom scheme).
        /// </summary>
        private SKColor GetColorFromScheme(double normalizedValue, SKColor[] customScheme = null)
        {
            var scheme = customScheme ?? colorScheme;
            if (scheme == null || scheme.Length == 0)
                return SKColors.Gray;

            int index = (int)(normalizedValue * (scheme.Length - 1));
            index = Math.Max(0, Math.Min(scheme.Length - 1, index));
            return scheme[index];
        }

        /// <summary>
        /// Renders statistical overlays.
        /// </summary>
        private void RenderStatisticalOverlays(SKCanvas canvas, float width, float height)
        {
            if (statistics == null)
                return;

            var statsPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 11f,
                TextAlign = SKTextAlign.Left,
                IsAntialias = true
            };

            float y = 50;
            float x = width - 200;

            // Draw statistics box
            var bgPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 220),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var statsRect = new SKRect(x - 10, y - 5, width - 10, y + 120);
            canvas.DrawRect(statsRect, bgPaint);

            canvas.DrawText($"Count: {statistics.Count}", x, y, statsPaint);
            y += 15;
            canvas.DrawText($"Mean: {statistics.MeanValue:F2}", x, y, statsPaint);
            y += 15;
            canvas.DrawText($"Median: {statistics.MedianValue:F2}", x, y, statsPaint);
            y += 15;
            canvas.DrawText($"Std Dev: {statistics.StandardDeviation:F2}", x, y, statsPaint);
            y += 15;
            canvas.DrawText($"Min: {statistics.MinValue:F2}", x, y, statsPaint);
            y += 15;
            canvas.DrawText($"Max: {statistics.MaxValue:F2}", x, y, statsPaint);
            y += 15;
            canvas.DrawText($"Range: {statistics.MaxValue - statistics.MinValue:F2}", x, y, statsPaint);
        }

        /// <summary>
        /// Renders interaction elements (selection, tooltips).
        /// </summary>
        private void RenderInteraction(SKCanvas canvas, float width, float height)
        {
            // Render selected points
            if (interaction.SelectedPoints.Count > 0)
            {
                var selectionPaint = new SKPaint
                {
                    Color = SKColors.Blue,
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = 3f,
                    IsAntialias = true
                };

                float scaleX = width / (float)(maxX - minX);
                float scaleY = height / (float)(maxY - minY);
                float offsetX = (float)-minX * scaleX;
                float offsetY = (float)-minY * scaleY;

                foreach (var selected in interaction.SelectedPoints)
                {
                    float x = (float)(selected.DataPoint.X * scaleX + offsetX);
                    float y = (float)(selected.DataPoint.Y * scaleY + offsetY);
                    canvas.DrawCircle(x, y, 15f, selectionPaint);
                }
            }

            // Render tooltip
            if (interaction.TooltipsEnabled && interaction.HoveredPoint != null)
            {
                RenderTooltip(canvas, interaction.HoveredPoint, interaction.HoverPosition);
            }
        }

        /// <summary>
        /// Renders a tooltip for a hovered point.
        /// </summary>
        private void RenderTooltip(SKCanvas canvas, HeatMapDataPoint point, SKPoint position)
        {
            string tooltipText = $"X: {point.X:F2}\nY: {point.Y:F2}\nValue: {point.Value:F2}";
            if (!string.IsNullOrEmpty(point.Label))
            {
                tooltipText = $"{point.Label}\n{tooltipText}";
            }

            var textPaint = new SKPaint
            {
                Color = interaction.TooltipTextColor,
                TextSize = interaction.TooltipFontSize,
                IsAntialias = true
            };

            var lines = tooltipText.Split('\n');
            float lineHeight = interaction.TooltipFontSize + 4;
            float maxWidth = 0;

            foreach (var line in lines)
            {
                float width = textPaint.MeasureText(line);
                if (width > maxWidth) maxWidth = width;
            }

            float padding = 8f;
            float tooltipWidth = maxWidth + padding * 2;
            float tooltipHeight = lines.Length * lineHeight + padding * 2;

            float x = position.X + interaction.TooltipOffset.X;
            float y = position.Y + interaction.TooltipOffset.Y;

            // Draw background
            var bgPaint = new SKPaint
            {
                Color = interaction.TooltipBackgroundColor,
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            var tooltipRect = new SKRect(x, y, x + tooltipWidth, y + tooltipHeight);
            canvas.DrawRect(tooltipRect, bgPaint);

            // Draw text
            float textY = y + padding + interaction.TooltipFontSize;
            foreach (var line in lines)
            {
                canvas.DrawText(line, x + padding, textY, textPaint);
                textY += lineHeight;
            }
        }

        /// <summary>
        /// Renders a point with gradient effect.
        /// </summary>
        private void RenderPointWithGradient(SKCanvas canvas, HeatMapDataPoint point, SKColor baseColor, float radius)
        {
            var centerX = (float)point.X;
            var centerY = (float)point.Y;

            // Create radial gradient
            var colors = new SKColor[]
            {
                new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, 255),
                new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, 180),
                new SKColor(baseColor.Red, baseColor.Green, baseColor.Blue, 0)
            };

            var colorPos = new float[] { 0f, 0.7f, 1f };

            using (var shader = SKShader.CreateRadialGradient(
                new SKPoint(centerX, centerY),
                radius,
                colors,
                colorPos,
                SKShaderTileMode.Clamp))
            {
                var paint = new SKPaint
                {
                    Shader = shader,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true
                };
                canvas.DrawCircle(centerX, centerY, radius, paint);
            }
        }

        /// <summary>
        /// Renders animation time indicator.
        /// </summary>
        private void RenderAnimationTime(SKCanvas canvas, float width, float height)
        {
            if (animation == null || !configuration.ShowAnimationTime)
                return;

            var timePaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = 14f,
                TextAlign = SKTextAlign.Center,
                IsAntialias = true,
                FakeBoldText = true
            };

            var bgPaint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, 220),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };

            string timeText = $"Frame: {animation.CurrentTimeIndex} / {animation.MaxTimeIndex}";
            if (animation.TimeFrames.ContainsKey(animation.CurrentTimeIndex) && 
                animation.TimeFrames[animation.CurrentTimeIndex].Count > 0)
            {
                var firstPoint = animation.TimeFrames[animation.CurrentTimeIndex][0];
                if (firstPoint is TimeSeriesDataPoint tsPoint)
                {
                    timeText += $" | {tsPoint.Timestamp:yyyy-MM-dd HH:mm}";
                }
            }

            var textBounds = new SKRect();
            timePaint.MeasureText(timeText, ref textBounds);
            textBounds.Offset(width / 2, 30);
            textBounds.Inflate(10, 5);

            canvas.DrawRect(textBounds, bgPaint);
            canvas.DrawText(timeText, width / 2, 30 + timePaint.TextSize / 3, timePaint);
        }

        /// <summary>
        /// Exports the heat map to a PNG file.
        /// </summary>
        public void ExportToPng(string filePath, int quality = 100)
        {
            var imageInfo = new SKImageInfo((int)GetCurrentWidth(), (int)GetCurrentHeight());
            using (var surface = SKSurface.Create(imageInfo))
            {
                Render(surface.Canvas, imageInfo.Width, imageInfo.Height);
                HeatMapExporter.ExportToPng(surface, filePath, quality);
            }
        }

        /// <summary>
        /// Exports the heat map to a JPEG file.
        /// </summary>
        public void ExportToJpeg(string filePath, int quality = 90)
        {
            var imageInfo = new SKImageInfo((int)GetCurrentWidth(), (int)GetCurrentHeight());
            using (var surface = SKSurface.Create(imageInfo))
            {
                Render(surface.Canvas, imageInfo.Width, imageInfo.Height);
                HeatMapExporter.ExportToJpeg(surface, filePath, quality);
            }
        }

        /// <summary>
        /// Exports the heat map to a WebP file.
        /// </summary>
        public void ExportToWebP(string filePath, int quality = 90)
        {
            var imageInfo = new SKImageInfo((int)GetCurrentWidth(), (int)GetCurrentHeight());
            using (var surface = SKSurface.Create(imageInfo))
            {
                Render(surface.Canvas, imageInfo.Width, imageInfo.Height);
                HeatMapExporter.ExportToWebP(surface, filePath, quality);
            }
        }

        /// <summary>
        /// Exports the heat map to an SVG file.
        /// </summary>
        public void ExportToSvg(string filePath, double width, double height)
        {
            HeatMapExporter.ExportToSvg(dataPoints, filePath, width, height);
        }

        /// <summary>
        /// Zooms to fit all data points in the viewport.
        /// </summary>
        public void ZoomToFit(float canvasWidth, float canvasHeight, float padding = 20f)
        {
            var result = ZoomToSelection.CalculateZoomToFit(dataPoints, canvasWidth, canvasHeight, padding);
            if (result.HasValue)
            {
                zoom = result.Value.zoom;
                panOffset = result.Value.panOffset;
            }
        }

        /// <summary>
        /// Zooms to fit selected points in the viewport.
        /// </summary>
        public void ZoomToSelectedPoints(float canvasWidth, float canvasHeight, float padding = 20f)
        {
            if (interaction.SelectedPoints.Count == 0)
                return;

            var result = ZoomToSelection.CalculateZoomToSelection(
                interaction.SelectedPoints, canvasWidth, canvasHeight, padding, configuration.MaxZoom);
            if (result.HasValue)
            {
                zoom = result.Value.zoom;
                panOffset = result.Value.panOffset;
            }
        }

        /// <summary>
        /// Zooms to a specific bounding box.
        /// </summary>
        public void ZoomToBounds(double minX, double minY, double maxX, double maxY, 
            float canvasWidth, float canvasHeight, float padding = 20f)
        {
            var result = ZoomToSelection.CalculateZoomToBounds(
                minX, minY, maxX, maxY, canvasWidth, canvasHeight, padding, configuration.MaxZoom);
            if (result.HasValue)
            {
                zoom = result.Value.zoom;
                panOffset = result.Value.panOffset;
            }
        }

        /// <summary>
        /// Gets the current canvas width (for export).
        /// </summary>
        private float GetCurrentWidth()
        {
            return (float)((maxX - minX) * zoom);
        }

        /// <summary>
        /// Gets the current canvas height (for export).
        /// </summary>
        private float GetCurrentHeight()
        {
            return (float)((maxY - minY) * zoom);
        }

        /// <summary>
        /// Updates the animation to the next frame.
        /// </summary>
        public void NextAnimationFrame()
        {
            if (animation != null)
            {
                animation.NextFrame();
                UpdateFromAnimation();
            }
        }

        /// <summary>
        /// Updates the animation to the previous frame.
        /// </summary>
        public void PreviousAnimationFrame()
        {
            if (animation != null)
            {
                animation.PreviousFrame();
                UpdateFromAnimation();
            }
        }

        /// <summary>
        /// Jumps to a specific animation frame.
        /// </summary>
        public void GoToAnimationFrame(int frameIndex)
        {
            if (animation != null)
            {
                animation.GoToFrame(frameIndex);
                UpdateFromAnimation();
            }
        }

        /// <summary>
        /// Renders SVG image overlays.
        /// </summary>
        private void RenderSvgOverlays(SKCanvas canvas, float width, float height)
        {
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            foreach (var overlay in svgOverlays.Where(o => o.IsVisible))
            {
                PointOverlayRenderer.RenderSvgOverlay(canvas, overlay, scaleX, scaleY, offsetX, offsetY);
            }
        }

        /// <summary>
        /// Renders pie chart overlays.
        /// </summary>
        private void RenderPieChartOverlays(SKCanvas canvas, float width, float height)
        {
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            foreach (var chart in pieChartOverlays.Where(c => c.IsVisible))
            {
                PointOverlayRenderer.RenderPieChart(canvas, chart, scaleX, scaleY, offsetX, offsetY);
            }
        }

        /// <summary>
        /// Renders bar chart overlays.
        /// </summary>
        private void RenderBarChartOverlays(SKCanvas canvas, float width, float height)
        {
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            foreach (var chart in barChartOverlays.Where(c => c.IsVisible))
            {
                PointOverlayRenderer.RenderBarChart(canvas, chart, scaleX, scaleY, offsetX, offsetY);
            }
        }

        /// <summary>
        /// Sets up coordinate transforms for crosshair/reference lines.
        /// </summary>
        private void SetupCrosshairTransforms(float width, float height)
        {
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            crosshairReferenceLines.ScreenToDataTransform = (screenX, screenY) =>
            {
                double dataX = (screenX - offsetX) / scaleX;
                double dataY = (screenY - offsetY) / scaleY;
                return (dataX, dataY);
            };

            crosshairReferenceLines.DataToScreenTransform = (dataX, dataY) =>
            {
                float screenX = (float)(dataX * scaleX + offsetX);
                float screenY = (float)(dataY * scaleY + offsetY);
                return (screenX, screenY);
            };
        }

        /// <summary>
        /// Renders hexbin aggregation.
        /// </summary>
        private void RenderHexbinAggregation(SKCanvas canvas, float width, float height)
        {
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            foreach (var cell in hexbinCells)
            {
                double normalizedValue = NormalizeValue(cell.Value);
                SKColor color = GetColorFromScheme(normalizedValue);
                DataAggregation.RenderHexbinCell(canvas, cell, color, scaleX, scaleY, offsetX, offsetY);
            }
        }

        /// <summary>
        /// Renders grid aggregation.
        /// </summary>
        private void RenderGridAggregation(SKCanvas canvas, float width, float height)
        {
            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            foreach (var cell in gridCells)
            {
                double normalizedValue = NormalizeValue(cell.Value);
                SKColor color = GetColorFromScheme(normalizedValue);
                DataAggregation.RenderGridCell(canvas, cell, color, scaleX, scaleY, offsetX, offsetY);
            }
        }

        /// <summary>
        /// Renders kernel density estimation.
        /// </summary>
        private void RenderKernelDensity(SKCanvas canvas, float width, float height)
        {
            if (densityGrid == null)
                return;

            int gridWidth = densityGrid.GetLength(0);
            int gridHeight = densityGrid.GetLength(1);
            float cellWidth = width / gridWidth;
            float cellHeight = height / gridHeight;

            // Find max density for normalization
            double maxDensity = 0;
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (densityGrid[x, y] > maxDensity)
                        maxDensity = densityGrid[x, y];
                }
            }

            if (maxDensity <= 0)
                return;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    double normalizedDensity = densityGrid[x, y] / maxDensity;
                    SKColor color = GetColorFromScheme(normalizedDensity);

                    float rectX = x * cellWidth;
                    float rectY = y * cellHeight;

                    using (var paint = new SKPaint
                    {
                        Color = color,
                        Style = SKPaintStyle.Fill,
                        IsAntialias = true
                    })
                    {
                        canvas.DrawRect(rectX, rectY, cellWidth, cellHeight, paint);
                    }
                }
            }
        }

        /// <summary>
        /// Renders value distribution (histogram and curve).
        /// </summary>
        private void RenderValueDistribution(SKCanvas canvas, float width, float height)
        {
            float distWidth = width * 0.3f;
            float distHeight = height * 0.2f;
            float distX = width - distWidth - 10;
            float distY = 10;

            // Render histogram
            if (histogramBins != null && histogramBins.Count > 0)
            {
                ValueDistribution.RenderHistogram(canvas, histogramBins, distX, distY, 
                    distWidth, distHeight, new SKColor(100, 150, 255, 200), SKColors.Blue, true);
            }

            // Render distribution curve
            if (distributionCurve != null && distributionCurve.Count > 0)
            {
                float curveY = distY + distHeight + 20;
                ValueDistribution.RenderDistributionCurve(canvas, distributionCurve, 
                    distX, curveY, distWidth, distHeight, SKColors.Red, 2f, true);
            }
        }

        /// <summary>
        /// Computes hexbin aggregation.
        /// </summary>
        public void ComputeHexbinAggregation(double radius, Beep.OilandGas.HeatMap.Aggregation.AggregationMethod method = Beep.OilandGas.HeatMap.Aggregation.AggregationMethod.Average)
        {
            hexbinCells = DataAggregation.CreateHexbin(dataPoints, radius, method);
        }

        /// <summary>
        /// Computes grid aggregation.
        /// </summary>
        public void ComputeGridAggregation(double cellSize, Beep.OilandGas.HeatMap.Aggregation.AggregationMethod method = Beep.OilandGas.HeatMap.Aggregation.AggregationMethod.Average)
        {
            gridCells = DataAggregation.CreateGrid(dataPoints, cellSize, method, minX, maxX, minY, maxY);
        }

        /// <summary>
        /// Computes kernel density estimation.
        /// </summary>
        public void ComputeKernelDensity(int gridWidth, int gridHeight, double? bandwidth = null, KernelType kernelType = KernelType.Gaussian)
        {
            densityGrid = KernelDensityEstimation.CalculateDensity(
                dataPoints, gridWidth, gridHeight, minX, maxX, minY, maxY, bandwidth, kernelType);
        }

        /// <summary>
        /// Computes value distribution visualization.
        /// </summary>
        public void ComputeValueDistribution(int? numBins = null)
        {
            histogramBins = ValueDistribution.CreateHistogram(dataPoints, numBins);
            distributionCurve = ValueDistribution.CreateDistributionCurve(dataPoints);
        }

        /// <summary>
        /// Renders filled contour regions.
        /// </summary>
        private void RenderFilledContours(SKCanvas canvas, float width, float height)
        {
            if (filledContourRegions == null || filledContourRegions.Count == 0)
                return;

            float scaleX = width / (float)(maxX - minX);
            float scaleY = height / (float)(maxY - minY);
            float offsetX = (float)-minX * scaleX;
            float offsetY = (float)-minY * scaleY;

            ContourMap.RenderFilledContours(canvas, filledContourRegions, scaleX, scaleY, offsetX, offsetY);
        }

        /// <summary>
        /// Generates contour map with custom levels.
        /// </summary>
        public void GenerateContourMap(double[] customLevels = null)
        {
            if (interpolatedGrid == null)
                ComputeInterpolationGrid();

            if (interpolatedGrid == null)
                return;

            double[] contourLevels = customLevels ?? 
                ContourMap.GenerateContourLevels(minValue, maxValue, configuration.ContourLevels);

            contourMapLines = ContourMap.GenerateContourLines(
                interpolatedGrid,
                contourLevels,
                minX, maxX, minY, maxY);

            if (configuration.UseFilledContours)
            {
                filledContourRegions = ContourMap.GenerateFilledContours(
                    interpolatedGrid,
                    contourLevels,
                    minX, maxX, minY, maxY,
                    colorScheme);
            }
        }
    }
}

