using SkiaSharp;

namespace Beep.OilandGas.EconomicAnalysis.Rendering
{
    public class EconomicRendererConfiguration
    {
        public SKColor BackgroundColor { get; set; } = SKColors.White;
        public SKColor PlotAreaBackgroundColor { get; set; } = SKColors.White;
        public bool ShowGrid { get; set; } = true;
        public SKColor GridColor { get; set; } = new SKColor(224, 224, 224);
        public float GridLineWidth { get; set; } = 1f;

        // Cash Flow Chart
        public SKColor PositiveCashFlowColor { get; set; } = new SKColor(76, 175, 80); // Green
        public SKColor NegativeCashFlowColor { get; set; } = new SKColor(244, 67, 54); // Red
        public float CashFlowBarWidth { get; set; } = 0.8f;

        // NPV Profile
        public SKColor NPVCurveColor { get; set; } = new SKColor(33, 150, 243); // Blue
        public float NPVCurveLineWidth { get; set; } = 2.5f;
        public SKColor ZeroLineColor { get; set; } = new SKColor(158, 158, 158); // Gray

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
        public string YAxisLabel { get; set; } = "Cash Flow ($)";
        public string Title { get; set; } = "Economic Analysis";

        public EconomicPlotType PlotType { get; set; } = EconomicPlotType.CashFlow;
        public double MinZoom { get; set; } = 0.1;
        public double MaxZoom { get; set; } = 10.0;
        public float ExportDPI { get; set; } = 300f;
    }

    public enum EconomicPlotType
    {
        CashFlow,
        NPVProfile,
        CumulativeCashFlow
    }
}

