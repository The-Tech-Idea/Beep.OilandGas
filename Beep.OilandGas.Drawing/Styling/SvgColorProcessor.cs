using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Beep.OilandGas.Drawing.Styling
{
    /// <summary>
    /// Processes SVG files to replace parameter placeholders with actual colors.
    /// Handles USGS-FGDC pattern parameters like param(outline) and param(fill).
    /// </summary>
    public static class SvgColorProcessor
    {
        /// <summary>
        /// Processes an SVG string to replace color parameters with actual colors.
        /// </summary>
        /// <param name="svgContent">The SVG content as a string.</param>
        /// <param name="strokeColor">The stroke color to use (replaces param(outline)).</param>
        /// <param name="fillColor">The fill color to use (replaces param(fill)).</param>
        /// <returns>The processed SVG content with colors applied.</returns>
        public static string ProcessSvgColors(string svgContent, string strokeColor = "#231f20", string fillColor = "none")
        {
            if (string.IsNullOrEmpty(svgContent))
                return svgContent;

            // Replace param(outline) with actual stroke color
            // Pattern: param(outline) #231f20 -> #231f20 (or custom color)
            svgContent = Regex.Replace(
                svgContent,
                @"param\(outline\)\s+#[0-9a-fA-F]{6}",
                strokeColor,
                RegexOptions.IgnoreCase);

            // Replace param(fill) with actual fill color
            // Pattern: param(fill) none -> none (or custom color)
            svgContent = Regex.Replace(
                svgContent,
                @"param\(fill\)\s+\w+",
                fillColor,
                RegexOptions.IgnoreCase);

            // Replace param(stroke-width) if needed (keep default)
            svgContent = Regex.Replace(
                svgContent,
                @"param\(stroke-width\)\s+([\d.]+)",
                "$1",
                RegexOptions.IgnoreCase);

            return svgContent;
        }

        /// <summary>
        /// Converts SKColor to SVG hex color string.
        /// </summary>
        public static string SkColorToHex(SkiaSharp.SKColor color)
        {
            return $"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}";
        }

        /// <summary>
        /// Processes an SVG file and returns the processed content.
        /// </summary>
        public static string ProcessSvgFile(string filePath, SkiaSharp.SKColor strokeColor, SkiaSharp.SKColor? fillColor = null)
        {
            if (!File.Exists(filePath))
                return null;

            string svgContent = File.ReadAllText(filePath);
            string strokeHex = SkColorToHex(strokeColor);
            string fillHex = fillColor.HasValue ? SkColorToHex(fillColor.Value) : "none";

            return ProcessSvgColors(svgContent, strokeHex, fillHex);
        }
    }
}

