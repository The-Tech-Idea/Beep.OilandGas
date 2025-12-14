using SkiaSharp;
using System;
using System.IO;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Exceptions;

namespace Beep.OilandGas.Drawing.Export
{
    /// <summary>
    /// Provides image export capabilities for drawings.
    /// </summary>
    public static class ImageExporter
    {
        /// <summary>
        /// Exports a drawing engine to a PNG file.
        /// </summary>
        /// <param name="engine">The drawing engine to export.</param>
        /// <param name="filePath">The output file path.</param>
        /// <param name="quality">Image quality (0-100, default: 100).</param>
        /// <exception cref="ArgumentNullException">Thrown when engine or filePath is null.</exception>
        /// <exception cref="RenderingException">Thrown when export fails.</exception>
        public static void ExportToPng(DrawingEngine engine, string filePath, int quality = 100)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                using (var surface = engine.Render())
                {
                    using (var image = surface.Snapshot())
                    {
                        using (var data = image.Encode(SKEncodedImageFormat.Png, quality))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RenderingException("ExportToPng", 
                    $"Failed to export to PNG: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exports a drawing engine to a JPEG file.
        /// </summary>
        /// <param name="engine">The drawing engine to export.</param>
        /// <param name="filePath">The output file path.</param>
        /// <param name="quality">Image quality (0-100, default: 90).</param>
        /// <exception cref="ArgumentNullException">Thrown when engine or filePath is null.</exception>
        /// <exception cref="RenderingException">Thrown when export fails.</exception>
        public static void ExportToJpeg(DrawingEngine engine, string filePath, int quality = 90)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            quality = Math.Max(0, Math.Min(100, quality));

            try
            {
                using (var surface = engine.Render())
                {
                    using (var image = surface.Snapshot())
                    {
                        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, quality))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RenderingException("ExportToJpeg", 
                    $"Failed to export to JPEG: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exports a drawing engine to a WebP file.
        /// </summary>
        /// <param name="engine">The drawing engine to export.</param>
        /// <param name="filePath">The output file path.</param>
        /// <param name="quality">Image quality (0-100, default: 90).</param>
        /// <exception cref="ArgumentNullException">Thrown when engine or filePath is null.</exception>
        /// <exception cref="RenderingException">Thrown when export fails.</exception>
        public static void ExportToWebP(DrawingEngine engine, string filePath, int quality = 90)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            quality = Math.Max(0, Math.Min(100, quality));

            try
            {
                using (var surface = engine.Render())
                {
                    using (var image = surface.Snapshot())
                    {
                        using (var data = image.Encode(SKEncodedImageFormat.Webp, quality))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RenderingException("ExportToWebP", 
                    $"Failed to export to WebP: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exports a drawing engine to an image with specified format.
        /// </summary>
        /// <param name="engine">The drawing engine to export.</param>
        /// <param name="filePath">The output file path.</param>
        /// <param name="format">The image format.</param>
        /// <param name="quality">Image quality (0-100).</param>
        /// <exception cref="ArgumentNullException">Thrown when engine or filePath is null.</exception>
        /// <exception cref="RenderingException">Thrown when export fails.</exception>
        public static void Export(DrawingEngine engine, string filePath, SKEncodedImageFormat format, int quality = 90)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            quality = Math.Max(0, Math.Min(100, quality));

            try
            {
                using (var surface = engine.Render())
                {
                    using (var image = surface.Snapshot())
                    {
                        using (var data = image.Encode(format, quality))
                        {
                            using (var stream = File.Create(filePath))
                            {
                                data.SaveTo(stream);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new RenderingException("Export", 
                    $"Failed to export to {format}: {ex.Message}", ex);
            }
        }
    }
}

