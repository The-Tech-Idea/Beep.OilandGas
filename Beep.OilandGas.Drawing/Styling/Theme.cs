using SkiaSharp;
using System.Collections.Generic;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Represents a visual theme for oil and gas drawings.
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public SKColor BackgroundColor { get; set; } = SKColors.White;

        /// <summary>
        /// Gets or sets the foreground/text color.
        /// </summary>
        public SKColor ForegroundColor { get; set; } = SKColors.Black;

        /// <summary>
        /// Gets or sets the grid color.
        /// </summary>
        public SKColor GridColor { get; set; } = SKColors.LightGray;

        /// <summary>
        /// Gets or sets the selection color.
        /// </summary>
        public SKColor SelectionColor { get; set; } = SKColors.Yellow;

        /// <summary>
        /// Gets or sets the highlight color.
        /// </summary>
        public SKColor HighlightColor { get; set; } = SKColors.Orange;

        /// <summary>
        /// Gets or sets custom color mappings.
        /// </summary>
        public Dictionary<string, SKColor> CustomColors { get; set; } = new Dictionary<string, SKColor>();

        /// <summary>
        /// Gets the standard theme.
        /// </summary>
        public static Theme Standard => new Theme
        {
            BackgroundColor = SKColors.White,
            ForegroundColor = SKColors.Black,
            GridColor = SKColors.LightGray,
            SelectionColor = SKColors.Yellow,
            HighlightColor = SKColors.Orange
        };

        /// <summary>
        /// Gets a dark theme.
        /// </summary>
        public static Theme Dark => new Theme
        {
            BackgroundColor = SKColors.Black,
            ForegroundColor = SKColors.White,
            GridColor = SKColors.DarkGray,
            SelectionColor = SKColors.Yellow,
            HighlightColor = SKColors.Orange
        };

        /// <summary>
        /// Gets a high contrast theme.
        /// </summary>
        public static Theme HighContrast => new Theme
        {
            BackgroundColor = SKColors.White,
            ForegroundColor = SKColors.Black,
            GridColor = SKColors.Black,
            SelectionColor = SKColors.Yellow,
            HighlightColor = SKColors.Orange
        };

        /// <summary>
        /// Gets a color by key, checking custom colors first, then standard palettes.
        /// </summary>
        /// <param name="key">The color key.</param>
        /// <param name="defaultColor">The default color if not found.</param>
        /// <returns>The color.</returns>
        public SKColor GetColor(string key, SKColor defaultColor = default)
        {
            if (CustomColors != null && CustomColors.ContainsKey(key))
                return CustomColors[key];

            // Try standard palettes
            var color = ColorPalette.GetColor("WellSchematic", key);
            if (color != SKColors.White || defaultColor == default)
                return color;

            return defaultColor;
        }
    }
}

