using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Controls composition and styling for reservoir map views with contour, fault, and well overlays.
    /// </summary>
    public sealed class ReservoirMapConfiguration
    {
        /// <summary>
        /// Gets or sets whether the contour base should be rendered.
        /// </summary>
        public bool ShowContours { get; set; } = true;

        /// <summary>
        /// Gets or sets the contour configuration for the base map.
        /// </summary>
        public ReservoirContourConfiguration ContourConfiguration { get; set; } = new ReservoirContourConfiguration();

        /// <summary>
        /// Gets or sets whether fault traces should be rendered.
        /// </summary>
        public bool ShowFaults { get; set; } = true;

        /// <summary>
        /// Gets or sets whether fault labels should be rendered.
        /// </summary>
        public bool ShowFaultLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the fault stroke color.
        /// </summary>
        public SKColor FaultColor { get; set; } = new SKColor(132, 35, 27);

        /// <summary>
        /// Gets or sets the fault stroke width.
        /// </summary>
        public float FaultLineWidth { get; set; } = 2.4f;

        /// <summary>
        /// Gets or sets the fault label font size.
        /// </summary>
        public float FaultLabelFontSize { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the fault label color.
        /// </summary>
        public SKColor FaultLabelColor { get; set; } = new SKColor(132, 35, 27);

        /// <summary>
        /// Gets or sets whether wells should be rendered.
        /// </summary>
        public bool ShowWells { get; set; } = true;

        /// <summary>
        /// Gets or sets whether well labels should be rendered.
        /// </summary>
        public bool ShowWellLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether trajectory polylines should be rendered when provided.
        /// </summary>
        public bool ShowWellTrajectories { get; set; } = true;

        /// <summary>
        /// Gets or sets the default well marker color.
        /// </summary>
        public SKColor WellColor { get; set; } = new SKColor(18, 87, 163);

        /// <summary>
        /// Gets or sets the well marker outline color.
        /// </summary>
        public SKColor WellOutlineColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the well trajectory color.
        /// </summary>
        public SKColor WellTrajectoryColor { get; set; } = new SKColor(18, 87, 163, 170);

        /// <summary>
        /// Gets or sets the well marker radius.
        /// </summary>
        public float WellMarkerRadius { get; set; } = 6f;

        /// <summary>
        /// Gets or sets the well trajectory line width.
        /// </summary>
        public float WellTrajectoryLineWidth { get; set; } = 1.4f;

        /// <summary>
        /// Gets or sets the well label font size.
        /// </summary>
        public float WellLabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the well label color.
        /// </summary>
        public SKColor WellLabelColor { get; set; } = SKColors.Black;
    }
}