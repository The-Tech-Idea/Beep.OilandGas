using SkiaSharp;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Configuration for well rendering with multiple boreholes.
    /// </summary>
    public class WellRendererConfiguration
    {
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the well spacing (horizontal distance between wells).
        /// </summary>
        public float WellSpacing { get; set; } = 200f;

        /// <summary>
        /// Gets or sets the borehole spacing (horizontal distance between boreholes in same well).
        /// </summary>
        public float BoreholeSpacing { get; set; } = 50f;

        /// <summary>
        /// Gets or sets the wellbore path color.
        /// </summary>
        public SKColor WellborePathColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the wellbore path width.
        /// </summary>
        public float WellborePathWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets the casing color.
        /// </summary>
        public SKColor CasingColor { get; set; } = SKColors.Gray;

        /// <summary>
        /// Gets or sets the casing width.
        /// </summary>
        public float CasingWidth { get; set; } = 8f;

        /// <summary>
        /// Gets or sets the tubing color.
        /// </summary>
        public SKColor TubingColor { get; set; } = SKColors.Blue;

        /// <summary>
        /// Gets or sets the tubing width.
        /// </summary>
        public float TubingWidth { get; set; } = 4f;

        /// <summary>
        /// Gets or sets whether to show casing.
        /// </summary>
        public bool ShowCasing { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show tubing.
        /// </summary>
        public bool ShowTubing { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show equipment.
        /// </summary>
        public bool ShowEquipment { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show perforations.
        /// </summary>
        public bool ShowPerforations { get; set; } = true;

        /// <summary>
        /// Gets or sets the perforation color.
        /// </summary>
        public SKColor PerforationColor { get; set; } = SKColors.Red;

        /// <summary>
        /// Gets or sets the perforation size.
        /// </summary>
        public float PerforationSize { get; set; } = 6f;

        /// <summary>
        /// Gets or sets whether to show depth scale.
        /// </summary>
        public bool ShowDepthScale { get; set; } = true;

        /// <summary>
        /// Gets or sets the depth scale color.
        /// </summary>
        public SKColor DepthScaleColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the depth scale font size.
        /// </summary>
        public float DepthScaleFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the depth interval for depth markers.
        /// </summary>
        public double DepthInterval { get; set; } = 100.0;

        /// <summary>
        /// Gets or sets whether to show well labels.
        /// </summary>
        public bool ShowWellLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the well label font size.
        /// </summary>
        public float WellLabelFontSize { get; set; } = 12f;

        /// <summary>
        /// Gets or sets the well label color.
        /// </summary>
        public SKColor WellLabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets whether to show borehole labels.
        /// </summary>
        public bool ShowBoreholeLabels { get; set; } = false;

        /// <summary>
        /// Gets or sets the borehole label font size.
        /// </summary>
        public float BoreholeLabelFontSize { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the view mode (Vertical, Horizontal, 3D).
        /// </summary>
        public WellViewMode ViewMode { get; set; } = WellViewMode.Vertical;

        /// <summary>
        /// Gets or sets the horizontal stretch factor for horizontal wells.
        /// </summary>
        public float HorizontalStretchFactor { get; set; } = 10f;

        /// <summary>
        /// Gets or sets the vertical alignment for horizontal sections.
        /// </summary>
        public HorizontalAlignment VerticalAlignment { get; set; } = Rendering.HorizontalAlignment.SplitUp;

        /// <summary>
        /// Gets or sets the horizontal alignment for horizontal sections.
        /// </summary>
        public HorizontalSectionAlignment HorizontalAlignment { get; set; } = HorizontalSectionAlignment.LaterallyCompressed;

        /// <summary>
        /// Gets or sets whether to show grid.
        /// </summary>
        public bool ShowGrid { get; set; } = false;

        /// <summary>
        /// Gets or sets the grid color.
        /// </summary>
        public SKColor GridColor { get; set; } = new SKColor(200, 200, 200);

        /// <summary>
        /// Gets or sets the grid line width.
        /// </summary>
        public float GridLineWidth { get; set; } = 0.5f;

        /// <summary>
        /// Gets or sets the well colors (well identifier to color mapping).
        /// </summary>
        public Dictionary<string, SKColor> WellColors { get; set; } = new Dictionary<string, SKColor>();

        /// <summary>
        /// Gets or sets the scale factor for depth (pixels per unit depth).
        /// </summary>
        public float DepthScaleFactor { get; set; } = 1f;

        /// <summary>
        /// Gets or sets the left margin for rendering.
        /// </summary>
        public float LeftMargin { get; set; } = 60f;
    }

    /// <summary>
    /// Well view mode options.
    /// </summary>
    public enum WellViewMode
    {
        /// <summary>
        /// Vertical view (traditional).
        /// </summary>
        Vertical,

        /// <summary>
        /// Horizontal view (stretched horizontal sections).
        /// </summary>
        Horizontal,

        /// <summary>
        /// 3D view (perspective projection).
        /// </summary>
        View3D
    }

    /// <summary>
    /// Horizontal alignment options for horizontal sections.
    /// </summary>
    public enum HorizontalAlignment
    {
        /// <summary>
        /// Split up (vertical section on left, horizontal on right).
        /// </summary>
        SplitUp,

        /// <summary>
        /// Overlay (horizontal section overlays vertical).
        /// </summary>
        Overlay,

        /// <summary>
        /// Separate (completely separate views).
        /// </summary>
        Separate
    }

    /// <summary>
    /// Horizontal section alignment options.
    /// </summary>
    public enum HorizontalSectionAlignment
    {
        /// <summary>
        /// Laterally compressed (horizontal section compressed horizontally).
        /// </summary>
        LaterallyCompressed,

        /// <summary>
        /// True scale (horizontal section at true scale).
        /// </summary>
        TrueScale,

        /// <summary>
        /// Stretched (horizontal section stretched for visibility).
        /// </summary>
        Stretched
    }
}

