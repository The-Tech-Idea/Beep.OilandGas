using Beep.OilandGas.HeatMap.ColorSchemes;
using SkiaSharp;
using System;

namespace Beep.OilandGas.HeatMap.Configuration
{
    /// <summary>
    /// Configuration class for heat map generation and rendering.
    /// </summary>
    public class HEAT_MAP_CONFIGURATION
    {
        /// <summary>
        /// Gets or sets the color scheme type.
        /// </summary>
        public ColorSchemeType ColorSchemeType { get; set; } = ColorSchemeType.Viridis;

        /// <summary>
        /// Gets or sets custom colors for Custom color scheme.
        /// </summary>
        public SKColor[] CustomColors { get; set; }

        /// <summary>
        /// Gets or sets the number of color steps in the gradient.
        /// </summary>
        public int ColorSteps { get; set; } = 256;

        /// <summary>
        /// Gets or sets whether to show the color scale legend.
        /// </summary>
        public bool ShowLegend { get; set; } = true;

        /// <summary>
        /// Gets or sets the legend position (X coordinate).
        /// </summary>
        public float LegendX { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the legend position (Y coordinate).
        /// </summary>
        public float LegendY { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the minimum point radius.
        /// </summary>
        public float MinPointRadius { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the maximum point radius.
        /// </summary>
        public float MaxPointRadius { get; set; } = 20f;

        /// <summary>
        /// Gets or sets whether to show point labels.
        /// </summary>
        public bool ShowLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the label font size.
        /// </summary>
        public float LabelFontSize { get; set; } = 8f;

        /// <summary>
        /// Gets or sets the label color.
        /// </summary>
        public SKColor LabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets whether to use interpolation.
        /// </summary>
        public bool UseInterpolation { get; set; } = false;

        /// <summary>
        /// Gets or sets the interpolation method type.
        /// </summary>
        public Interpolation.InterpolationMethodType InterpolationMethod { get; set; } = Interpolation.InterpolationMethodType.InverseDistanceWeighting;

        /// <summary>
        /// Gets or sets the interpolation grid cell size.
        /// </summary>
        public double InterpolationCellSize { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets the IDW power parameter.
        /// </summary>
        public double IdwPower { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets the maximum distance for IDW interpolation.
        /// </summary>
        public double IdwMaxDistance { get; set; } = double.MaxValue;

        /// <summary>
        /// Gets or sets whether to show grid lines.
        /// </summary>
        public bool ShowGrid { get; set; } = false;

        /// <summary>
        /// Gets or sets the grid line color.
        /// </summary>
        public SKColor GridColor { get; set; } = SKColors.LightGray;

        /// <summary>
        /// Gets or sets the grid line spacing.
        /// </summary>
        public float GridSpacing { get; set; } = 50f;

        /// <summary>
        /// Gets or sets whether to show axis labels.
        /// </summary>
        public bool ShowAxisLabels { get; set; } = false;

        /// <summary>
        /// Gets or sets the X-axis label.
        /// </summary>
        public string XAxisLabel { get; set; } = "X";

        /// <summary>
        /// Gets or sets the Y-axis label.
        /// </summary>
        public string YAxisLabel { get; set; } = "Y";

        /// <summary>
        /// Gets or sets whether to show a scale bar.
        /// </summary>
        public bool ShowScaleBar { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show a north arrow.
        /// </summary>
        public bool ShowNorthArrow { get; set; } = false;

        /// <summary>
        /// Gets or sets the coordinate system name (e.g., "UTM Zone 15N", "WGS84").
        /// </summary>
        public string CoordinateSystem { get; set; }

        /// <summary>
        /// Gets or sets whether interaction features are enabled.
        /// </summary>
        public bool InteractionEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether multi-select is enabled.
        /// </summary>
        public bool MultiSelectEnabled { get; set; } = false;

        /// <summary>
        /// Gets or sets whether tooltips are enabled.
        /// </summary>
        public bool TooltipsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use spatial indexing for performance.
        /// </summary>
        public bool UseSpatialIndex { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use viewport culling for performance.
        /// </summary>
        public bool UseViewportCulling { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to use level-of-detail (LOD) rendering.
        /// </summary>
        public bool UseLOD { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of points to render (for LOD).
        /// </summary>
        public int MaxRenderPoints { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the minimum zoom level for full detail (for LOD).
        /// </summary>
        public double LODMinZoom { get; set; } = 1.0;

        /// <summary>
        /// Gets or sets the maximum point density per unit area (for LOD).
        /// </summary>
        public double MaxDensity { get; set; } = 10.0;

        /// <summary>
        /// Gets or sets whether to show axis tick marks.
        /// </summary>
        public bool ShowAxisTicks { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of axis tick intervals (0 = auto-calculate).
        /// </summary>
        public int AxisTickCount { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether to show contour lines.
        /// </summary>
        public bool ShowContours { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of contour levels.
        /// </summary>
        public int ContourLevels { get; set; } = 5;

        /// <summary>
        /// Gets or sets the contour line color.
        /// </summary>
        public SKColor ContourColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the contour line width.
        /// </summary>
        public float ContourLineWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets whether to show contour labels.
        /// </summary>
        public bool ShowContourLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the font size for contour labels.
        /// </summary>
        public float ContourLabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets whether to use filled contours (contour map).
        /// </summary>
        public bool UseFilledContours { get; set; } = false;

        /// <summary>
        /// Gets or sets custom contour levels (if null, auto-generates based on ContourLevels).
        /// </summary>
        public double[] ContourLevelsArray { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to show value annotations on points.
        /// </summary>
        public bool ShowValueAnnotations { get; set; } = false;

        /// <summary>
        /// Gets or sets the value annotation format string.
        /// </summary>
        public string ValueAnnotationFormat { get; set; } = "F2";

        /// <summary>
        /// Gets or sets whether to use enhanced interpolation methods.
        /// </summary>
        public bool UseEnhancedInterpolation { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of nearest neighbors for optimized IDW (0 = use all).
        /// </summary>
        public int IdwNearestNeighbors { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether to use clustering for data points.
        /// </summary>
        public bool UseClustering { get; set; } = false;

        /// <summary>
        /// Gets or sets the clustering algorithm to use.
        /// </summary>
        public Clustering.ClusteringAlgorithm ClusteringAlgorithm { get; set; } = Clustering.ClusteringAlgorithm.GridBased;

        /// <summary>
        /// Gets or sets the cluster cell size (for grid-based clustering).
        /// </summary>
        public double ClusterCellSize { get; set; } = 50.0;

        /// <summary>
        /// Gets or sets the number of clusters (for K-means).
        /// </summary>
        public int ClusterCount { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to show outliers.
        /// </summary>
        public bool ShowOutliers { get; set; } = false;

        /// <summary>
        /// Gets or sets the outlier factor for IQR method.
        /// </summary>
        public double OutlierFactor { get; set; } = 1.5;

        /// <summary>
        /// Gets or sets whether to show statistical overlays.
        /// </summary>
        public bool ShowStatisticalOverlays { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show clusters.
        /// </summary>
        public bool ShowClusters { get; set; } = false;

        /// <summary>
        /// Gets or sets the cluster rendering style.
        /// </summary>
        public SKColor ClusterColor { get; set; } = SKColors.Red;

        /// <summary>
        /// Gets or sets the outlier color.
        /// </summary>
        public SKColor OutlierColor { get; set; } = SKColors.Orange;

        /// <summary>
        /// Gets or sets whether to use gradient rendering for points.
        /// </summary>
        public bool UsePointGradient { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show animation time indicator.
        /// </summary>
        public bool ShowAnimationTime { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to enable real-time highlighting.
        /// </summary>
        public bool EnableRealtimeHighlighting { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum zoom level.
        /// </summary>
        public double MaxZoom { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets the minimum zoom level.
        /// </summary>
        public double MinZoom { get; set; } = 0.1;

        /// <summary>
        /// Gets or sets whether to show SVG image overlays.
        /// </summary>
        public bool ShowSvgOverlays { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show pie chart overlays.
        /// </summary>
        public bool ShowPieChartOverlays { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show bar chart overlays.
        /// </summary>
        public bool ShowBarChartOverlays { get; set; } = true;

        /// <summary>
        /// Gets or sets the color mapping function.
        /// </summary>
        public ColorMappingFunction ColorMappingFunction { get; set; } = ColorMappingFunction.Linear;

        /// <summary>
        /// Gets or sets the parameter for the color mapping function (e.g., power exponent, log base).
        /// </summary>
        public double ColorMappingParameter { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets a custom color mapping function (used when ColorMappingFunction is Custom).
        /// </summary>
        public Func<double, double> CustomColorMappingFunction { get; set; }

        /// <summary>
        /// Gets or sets whether to use hexbin aggregation.
        /// </summary>
        public bool UseHexbinAggregation { get; set; } = false;

        /// <summary>
        /// Gets or sets the hexbin radius.
        /// </summary>
        public double HexbinRadius { get; set; } = 20.0;

        /// <summary>
        /// Gets or sets the hexbin aggregation method.
        /// </summary>
        public Beep.OilandGas.HeatMap.Aggregation.AggregationMethod HexbinAggregationMethod { get; set; } = Beep.OilandGas.HeatMap.Aggregation.AggregationMethod.Average;

        /// <summary>
        /// Gets or sets whether to use grid aggregation.
        /// </summary>
        public bool UseGridAggregation { get; set; } = false;

        /// <summary>
        /// Gets or sets the grid cell size.
        /// </summary>
        public double GridCellSize { get; set; } = 50.0;

        /// <summary>
        /// Gets or sets the grid aggregation method.
        /// </summary>
        public Beep.OilandGas.HeatMap.Aggregation.AggregationMethod GridAggregationMethod { get; set; } = Beep.OilandGas.HeatMap.Aggregation.AggregationMethod.Average;

        /// <summary>
        /// Gets or sets whether to use kernel density estimation.
        /// </summary>
        public bool UseKernelDensity { get; set; } = false;

        /// <summary>
        /// Gets or sets the kernel density grid width.
        /// </summary>
        public int KernelDensityGridWidth { get; set; } = 100;

        /// <summary>
        /// Gets or sets the kernel density grid height.
        /// </summary>
        public int KernelDensityGridHeight { get; set; } = 100;

        /// <summary>
        /// Gets or sets the kernel density bandwidth (null = auto-calculate).
        /// </summary>
        public double? KernelDensityBandwidth { get; set; } = null;

        /// <summary>
        /// Gets or sets the kernel type for density estimation.
        /// </summary>
        public Density.KernelType KernelDensityType { get; set; } = Density.KernelType.Gaussian;

        /// <summary>
        /// Gets or sets whether to show value distribution (histogram/curve).
        /// </summary>
        public bool ShowValueDistribution { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of histogram bins (null = auto-calculate).
        /// </summary>
        public int? HistogramBins { get; set; } = null;

        /// <summary>
        /// Gets or sets whether to show crosshair.
        /// </summary>
        public bool ShowCrosshair { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to show reference lines.
        /// </summary>
        public bool ShowReferenceLines { get; set; } = false;
        public string map_id { get; internal set; }
        public string config_name { get; internal set; }
        public DateTime? row_created_date { get; internal set; }
        public string HEAT_MAP_ID { get; internal set; }
        public string CONFIGURATION_NAME { get; internal set; }
        public string ACTIVE_IND { get; internal set; }

        /// <summary>
        /// Gets the color scheme based on the configuration.
        /// </summary>
        /// <returns>Array of SKColor values representing the color scheme.</returns>
        public SKColor[] GetColorScheme()
        {
            if (ColorSchemeType == ColorSchemeType.Custom && CustomColors != null && CustomColors.Length >= 2)
            {
                return ColorScheme.CreateCustom(CustomColors, ColorSteps);
            }

            return ColorScheme.GetColorScheme(ColorSchemeType, ColorSteps);
        }
    }
}

