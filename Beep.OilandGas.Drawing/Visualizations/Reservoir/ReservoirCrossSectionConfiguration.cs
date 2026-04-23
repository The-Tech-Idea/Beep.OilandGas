using SkiaSharp;

namespace Beep.OilandGas.Drawing.Visualizations.Reservoir
{
    /// <summary>
    /// Controls extraction density and rendering for reservoir cross-sections.
    /// </summary>
    public sealed class ReservoirCrossSectionConfiguration
    {
        /// <summary>
        /// Gets or sets the number of section samples to extract per surface.
        /// </summary>
        public int SampleCount { get; set; } = 160;

        /// <summary>
        /// Gets or sets the number of neighboring surface points used during interpolation.
        /// </summary>
        public int MaxInfluencePoints { get; set; } = 8;

        /// <summary>
        /// Gets or sets the inverse-distance interpolation power.
        /// </summary>
        public double InterpolationPower { get; set; } = 2.0;

        /// <summary>
        /// Gets or sets whether surface labels should be rendered.
        /// </summary>
        public bool ShowSurfaceLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets whether wells should be rendered.
        /// </summary>
        public bool ShowWellMarkers { get; set; } = true;

        /// <summary>
        /// Gets or sets whether fluid contacts should be rendered.
        /// </summary>
        public bool ShowFluidContacts { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum map-space offset allowed when projecting wells onto the section.
        /// </summary>
        public double MaximumWellOffsetFromSection { get; set; } = 250;

        /// <summary>
        /// Gets or sets the profile line width.
        /// </summary>
        public float SurfaceLineWidth { get; set; } = 2.0f;

        /// <summary>
        /// Gets or sets the surface label font size.
        /// </summary>
        public float SurfaceLabelFontSize { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the well marker radius.
        /// </summary>
        public float WellMarkerRadius { get; set; } = 5f;

        /// <summary>
        /// Gets or sets the well label font size.
        /// </summary>
        public float WellLabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the fluid contact stroke width.
        /// </summary>
        public float ContactLineWidth { get; set; } = 1.5f;

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the default well color.
        /// </summary>
        public SKColor WellColor { get; set; } = new SKColor(18, 87, 163);

        /// <summary>
        /// Gets or sets the default contact color.
        /// </summary>
        public SKColor ContactColor { get; set; } = new SKColor(0, 110, 184);
    }
}