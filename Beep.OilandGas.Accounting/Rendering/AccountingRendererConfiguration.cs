using SkiaSharp;

namespace Beep.OilandGas.Accounting.Rendering
{
    /// <summary>
    /// Configuration for accounting visualization rendering with SkiaSharp.
    /// </summary>
    public class AccountingRendererConfiguration
    {
        public SKColor BackgroundColor { get; set; } = SKColors.White;
        public SKColor PlotAreaBackgroundColor { get; set; } = SKColors.White;
        public bool ShowGrid { get; set; } = true;
        public SKColor GridColor { get; set; } = new SKColor(224, 224, 224);
        public float GridLineWidth { get; set; } = 1f;

        // Cost colors
        public SKColor AcquisitionCostColor { get; set; } = new SKColor(33, 150, 243); // Blue
        public SKColor ExplorationCostColor { get; set; } = new SKColor(255, 87, 34); // Orange
        public SKColor DevelopmentCostColor { get; set; } = new SKColor(76, 175, 80); // Green
        public SKColor ProductionCostColor { get; set; } = new SKColor(156, 39, 176); // Purple
        public SKColor AmortizationColor { get; set; } = new SKColor(244, 67, 54); // Red

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
        public string XAxisLabel { get; set; } = "Period";
        public string YAxisLabel { get; set; } = "Amount ($)";
        public string Title { get; set; } = "Oil and Gas Accounting";

        public AccountingPlotType PlotType { get; set; } = AccountingPlotType.CostTrend;
        public bool ShowLegend { get; set; } = true;
        public double MinZoom { get; set; } = 0.1;
        public double MaxZoom { get; set; } = 10.0;
        public float ExportDPI { get; set; } = 300f;
    }

    /// <summary>
    /// Accounting plot type enumeration.
    /// </summary>
    public enum AccountingPlotType
    {
        /// <summary>
        /// Cost trend over time.
        /// </summary>
        CostTrend,

        /// <summary>
        /// Amortization schedule.
        /// </summary>
        AmortizationSchedule,

        /// <summary>
        /// Cost breakdown pie chart.
        /// </summary>
        CostBreakdown,

        /// <summary>
        /// Reserve vs cost chart.
        /// </summary>
        ReserveVsCost
    }
}

