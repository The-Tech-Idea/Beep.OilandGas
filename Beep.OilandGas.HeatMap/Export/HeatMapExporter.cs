using Beep.OilandGas.Models.HeatMap;
using SkiaSharp;
using System;
using System.IO;

namespace Beep.OilandGas.HeatMap.Export
{
    /// <summary>
    /// Provides export functionality for heat maps to various image formats.
    /// </summary>
    public static class HeatMapExporter
    {
        /// <summary>
        /// Exports the heat map to a PNG file.
        /// </summary>
        /// <param name="surface">The SkiaSharp surface containing the rendered heat map.</param>
        /// <param name="filePath">Path to the output PNG file.</param>
        /// <param name="quality">Image quality (0-100, default: 100).</param>
        public static void ExportToPng(SKSurface surface, string filePath, int quality = 100)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, quality))
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }

        /// <summary>
        /// Exports the heat map to a JPEG file.
        /// </summary>
        /// <param name="surface">The SkiaSharp surface containing the rendered heat map.</param>
        /// <param name="filePath">Path to the output JPEG file.</param>
        /// <param name="quality">Image quality (0-100, default: 90).</param>
        public static void ExportToJpeg(SKSurface surface, string filePath, int quality = 90)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            quality = Math.Max(0, Math.Min(100, quality)); // Clamp to [0, 100]

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Jpeg, quality))
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }

        /// <summary>
        /// Exports the heat map to a WebP file.
        /// </summary>
        /// <param name="surface">The SkiaSharp surface containing the rendered heat map.</param>
        /// <param name="filePath">Path to the output WebP file.</param>
        /// <param name="quality">Image quality (0-100, default: 90).</param>
        public static void ExportToWebP(SKSurface surface, string filePath, int quality = 90)
        {
            if (surface == null)
                throw new ArgumentNullException(nameof(surface));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            quality = Math.Max(0, Math.Min(100, quality)); // Clamp to [0, 100]

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Webp, quality))
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }

        /// <summary>
        /// Exports the heat map to an SVG file (basic implementation).
        /// Note: This is a simplified SVG export. For full SVG support, consider using a dedicated SVG library.
        /// </summary>
        /// <param name="dataPoints">The data points to export.</param>
        /// <param name="filePath">Path to the output SVG file.</param>
        /// <param name="width">Width of the SVG canvas.</param>
        /// <param name="height">Height of the SVG canvas.</param>
        public static void ExportToSvg(
            System.Collections.Generic.List<HEAT_MAP_DATA_POINT> dataPoints,
            string filePath,
            double width,
            double height)
        {
            if (dataPoints == null)
                throw new ArgumentNullException(nameof(dataPoints));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new StreamWriter(filePath))
            {
                writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                writer.WriteLine($"<svg width=\"{width}\" height=\"{height}\" xmlns=\"http://www.w3.org/2000/svg\">");

                // Draw background
                writer.WriteLine($"  <rect width=\"{width}\" height=\"{height}\" fill=\"white\"/>");

                // Draw data points as circles
                foreach (var point in dataPoints)
                {
                    // Convert value to color (simplified - using grayscale)
                    int intensity = (int)(point.Value * 255);
                    string color = $"rgb({intensity},{intensity},{intensity})";

                    // Calculate radius based on value
                    double radius = 5 + (point.Value * 15);

                    writer.WriteLine($"  <circle cx=\"{point.X}\" cy=\"{point.Y}\" r=\"{radius}\" fill=\"{color}\" opacity=\"0.7\"/>");

                    // Add label if present
                    if (!string.IsNullOrEmpty(point.Label))
                    {
                        writer.WriteLine($"  <text x=\"{point.X}\" y=\"{point.Y - radius - 5}\" font-size=\"10\" text-anchor=\"middle\">{point.Label}</text>");
                    }
                }

                writer.WriteLine("</svg>");
            }
        }

        /// <summary>
        /// Exports data points with coordinates to a CSV file.
        /// </summary>
        /// <param name="dataPoints">The data points to export.</param>
        /// <param name="filePath">Path to the output CSV file.</param>
        /// <param name="includeOriginalCoordinates">Whether to include original UTM coordinates.</param>
        public static void ExportDataToCsv(
            System.Collections.Generic.List<HEAT_MAP_DATA_POINT> dataPoints,
            string filePath,
            bool includeOriginalCoordinates = true)
        {
            if (dataPoints == null)
                throw new ArgumentNullException(nameof(dataPoints));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            using (var writer = new StreamWriter(filePath))
            {
                // Write header
                if (includeOriginalCoordinates)
                {
                    writer.WriteLine("X,Y,Value,Label,OriginalX,OriginalY");
                }
                else
                {
                    writer.WriteLine("X,Y,Value,Label");
                }

                // Write data
                foreach (var point in dataPoints)
                {
                    if (includeOriginalCoordinates)
                    {
                        writer.WriteLine($"{point.X},{point.Y},{point.Value},{point.Label ?? ""},{point.OriginalY},{point.OriginalY}");
                    }
                    else
                    {
                        writer.WriteLine($"{point.X},{point.Y},{point.Value},{point.Label ?? ""}");
                    }
                }
            }
        }
    }
}

