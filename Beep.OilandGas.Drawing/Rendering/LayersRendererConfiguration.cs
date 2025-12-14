using SkiaSharp;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Rendering
{
    /// <summary>
    /// Configuration for geological layers rendering.
    /// </summary>
    public class LayersRendererConfiguration
    {
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets whether to show layer fills.
        /// </summary>
        public bool ShowLayerFills { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show layer borders.
        /// </summary>
        public bool ShowLayerBorders { get; set; } = true;

        /// <summary>
        /// Gets or sets the layer border color.
        /// </summary>
        public SKColor LayerBorderColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the layer border width.
        /// </summary>
        public float LayerBorderWidth { get; set; } = 1f;

        /// <summary>
        /// Gets or sets whether to show layer patterns.
        /// </summary>
        public bool ShowLayerPatterns { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to show layer labels.
        /// </summary>
        public bool ShowLayerLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the layer label font size.
        /// </summary>
        public float LayerLabelFontSize { get; set; } = 11f;

        /// <summary>
        /// Gets or sets the layer label color.
        /// </summary>
        public SKColor LayerLabelColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets whether to show fluid contacts.
        /// </summary>
        public bool ShowFluidContacts { get; set; } = true;

        /// <summary>
        /// Gets or sets the oil-water contact color.
        /// </summary>
        public SKColor OilWaterContactColor { get; set; } = SKColors.Blue;

        /// <summary>
        /// Gets or sets the gas-oil contact color.
        /// </summary>
        public SKColor GasOilContactColor { get; set; } = SKColors.Red;

        /// <summary>
        /// Gets or sets the free water level color.
        /// </summary>
        public SKColor FreeWaterLevelColor { get; set; } = SKColors.DarkBlue;

        /// <summary>
        /// Gets or sets the contact line width.
        /// </summary>
        public float ContactLineWidth { get; set; } = 2f;

        /// <summary>
        /// Gets or sets whether to show contact labels.
        /// </summary>
        public bool ShowContactLabels { get; set; } = true;

        /// <summary>
        /// Gets or sets the contact label font size.
        /// </summary>
        public float ContactLabelFontSize { get; set; } = 10f;

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
        /// Gets or sets the layer colors (layer name to color mapping).
        /// </summary>
        public Dictionary<string, SKColor> LayerColors { get; set; } = new Dictionary<string, SKColor>();

        /// <summary>
        /// Gets or sets the layer patterns (layer name to pattern type mapping).
        /// </summary>
        public Dictionary<string, string> LayerPatterns { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Gets or sets whether to use lithology color palette.
        /// </summary>
        public bool UseLithologyColorPalette { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to highlight pay zones.
        /// </summary>
        public bool HighlightPayZones { get; set; } = true;

        /// <summary>
        /// Gets or sets the pay zone highlight color.
        /// </summary>
        public SKColor PayZoneHighlightColor { get; set; } = new SKColor(255, 255, 0, 64);

        /// <summary>
        /// Gets or sets the pay zone border color.
        /// </summary>
        public SKColor PayZoneBorderColor { get; set; } = SKColors.Orange;

        /// <summary>
        /// Gets or sets the pay zone border width.
        /// </summary>
        public float PayZoneBorderWidth { get; set; } = 2f;
    }
}

