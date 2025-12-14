using SkiaSharp;

namespace Beep.OilandGas.ProductionAccounting.Rendering
{
    /// <summary>
    /// Configuration for production chart rendering with SkiaSharp.
    /// </summary>
    public class ProductionChartRendererConfiguration
    {
        public SKColor BackgroundColor { get; set; } = SKColors.White;
        public SKColor PlotAreaBackgroundColor { get; set; } = SKColors.White;
        public bool ShowGrid { get; set; } = true;
        public SKColor GridColor { get; set; } = new SKColor(224, 224, 224);
        public float GridLineWidth { get; set; } = 1f;

        // Chart colors
        public SKColor ProductionColor { get; set; } = new SKColor(33, 150, 243); // Blue
        public SKColor RevenueColor { get; set; } = new SKColor(76, 175, 80); // Green
        public SKColor CostColor { get; set; } = new SKColor(244, 67, 54); // Red
        public SKColor InventoryColor { get; set; } = new SKColor(156, 39, 176); // Purple
        public SKColor AllocationColor { get; set; } = new SKColor(255, 152, 0); // Orange

        // Typography
        public float FontSize { get; set; } = 12f;
        public float TitleFontSize { get; set; } = 18f;
        public float AxisLabelFontSize { get; set; } = 14f;
        public float TickLabelFontSize { get; set; } = 10f;
        public SKColor TextColor { get; set; } = SKColors.Black;
        public SKColor TitleColor { get; set; } = SKColors.Black;

        // Margins
        public float LeftMargin { get; set; } = 80f;
        public float RightMargin { get; set; } = 40f;
        public float TopMargin { get; set; } = 60f;
        public float BottomMargin { get; set; } = 80f;

        // Axis
        public int XAxisTickCount { get; set; } = 10;
        public int YAxisTickCount { get; set; } = 10;
        public string XAxisLabel { get; set; } = "Date";
        public string YAxisLabel { get; set; } = "Volume (Barrels)";
        public string Title { get; set; } = "Production Chart";

        public ProductionChartType ChartType { get; set; } = ProductionChartType.ProductionTrend;
        public bool ShowLegend { get; set; } = true;
        public double MinZoom { get; set; } = 0.1;
        public double MaxZoom { get; set; } = 10.0;
        public float ExportDPI { get; set; } = 300f;
    }

    /// <summary>
    /// Production chart type enumeration.
    /// </summary>
    public enum ProductionChartType
    {
        /// <summary>
        /// Production trend over time.
        /// </summary>
        ProductionTrend,

        /// <summary>
        /// Revenue trend.
        /// </summary>
        RevenueTrend,

        /// <summary>
        /// Cost trend.
        /// </summary>
        CostTrend,

        /// <summary>
        /// Inventory levels.
        /// </summary>
        Inventory,

        /// <summary>
        /// Allocation breakdown.
        /// </summary>
        Allocation
    }
}

