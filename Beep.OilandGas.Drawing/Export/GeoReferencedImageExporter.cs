using System;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Drawing.CoordinateSystems;
using Beep.OilandGas.Drawing.Core;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Export
{
    /// <summary>
    /// Exports map scenes to PNG with world-file and CRS sidecars.
    /// </summary>
    public static class GeoReferencedImageExporter
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        /// <summary>
        /// Exports a drawing engine to PNG and writes companion georeferencing files based on the active scene.
        /// </summary>
        public static void ExportToPng(DrawingEngine engine, string filePath, int quality = 100)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (engine.ActiveScene?.WorldBounds == null)
                throw new InvalidOperationException("The drawing engine must have an active scene with world bounds before georeferenced export.");

            var coordinateReferenceSystem = engine.ActiveScene.CoordinateReferenceSystem
                ?? CoordinateReferenceSystem.CreateProjected("LOCAL:MAP", "Local Map", "m", CoordinateAuthority.Custom);

            ExportToPng(engine, filePath, engine.ActiveScene.WorldBounds.Value, coordinateReferenceSystem, quality);
        }

        /// <summary>
        /// Exports a drawing engine to PNG and writes companion georeferencing files using explicit map metadata.
        /// </summary>
        public static void ExportToPng(
            DrawingEngine engine,
            string filePath,
            SKRect worldBounds,
            CoordinateReferenceSystem coordinateReferenceSystem,
            int quality = 100)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("A target file path is required.", nameof(filePath));
            if (worldBounds.Width <= 0 || worldBounds.Height <= 0)
                throw new ArgumentException("World bounds must have a positive width and height.", nameof(worldBounds));

            coordinateReferenceSystem ??= CoordinateReferenceSystem.CreateProjected("LOCAL:MAP", "Local Map", "m", CoordinateAuthority.Custom);

            ImageExporter.ExportToPng(engine, filePath, quality);
            File.WriteAllText(GetWorldFilePath(filePath), CreateWorldFileContents(worldBounds, engine.Width, engine.Height));
            File.WriteAllText(GetCoordinateReferenceMetadataPath(filePath), JsonSerializer.Serialize(
                CoordinateReferenceSystemExportMetadata.Create(coordinateReferenceSystem),
                SerializerOptions));
        }

        /// <summary>
        /// Gets the companion world-file path for an image.
        /// </summary>
        public static string GetWorldFilePath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("An image path is required.", nameof(imagePath));

            var extension = Path.GetExtension(imagePath).ToLowerInvariant();
            var worldExtension = extension switch
            {
                ".png" => ".pgw",
                ".jpg" or ".jpeg" => ".jgw",
                ".tif" or ".tiff" => ".tfw",
                ".bmp" => ".bpw",
                _ => ".wld"
            };

            return Path.ChangeExtension(imagePath, worldExtension);
        }

        /// <summary>
        /// Gets the companion CRS metadata path for an image.
        /// </summary>
        public static string GetCoordinateReferenceMetadataPath(string imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
                throw new ArgumentException("An image path is required.", nameof(imagePath));

            return imagePath + ".crs.json";
        }

        private static string CreateWorldFileContents(SKRect worldBounds, int width, int height)
        {
            double pixelWidth = worldBounds.Width / Math.Max(1, width);
            double pixelHeight = worldBounds.Height / Math.Max(1, height);
            double upperLeftCenterX = worldBounds.Left + (pixelWidth / 2.0);
            double upperLeftCenterY = worldBounds.Top + (pixelHeight / 2.0);

            return string.Join(
                Environment.NewLine,
                pixelWidth.ToString("G17", CultureInfo.InvariantCulture),
                0d.ToString("G17", CultureInfo.InvariantCulture),
                0d.ToString("G17", CultureInfo.InvariantCulture),
                pixelHeight.ToString("G17", CultureInfo.InvariantCulture),
                upperLeftCenterX.ToString("G17", CultureInfo.InvariantCulture),
                upperLeftCenterY.ToString("G17", CultureInfo.InvariantCulture));
        }
    }
}