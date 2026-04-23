using System;
using System.IO;
using System.Text;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Exceptions;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Export
{
    /// <summary>
    /// Provides SVG export capabilities for drawings.
    /// </summary>
    public static class SvgExporter
    {
        /// <summary>
        /// Exports a drawing engine to an SVG file.
        /// </summary>
        /// <param name="engine">The drawing engine to export.</param>
        /// <param name="filePath">The output file path.</param>
        /// <exception cref="ArgumentNullException">Thrown when engine or filePath is null.</exception>
        /// <exception cref="RenderingException">Thrown when export fails.</exception>
        public static void Export(DrawingEngine engine, string filePath)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));

            try
            {
                var svg = ExportToString(engine);
                File.WriteAllText(filePath, svg, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            }
            catch (RenderingException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new RenderingException("ExportToSvg",
                    $"Failed to export to SVG: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Exports a drawing engine to an SVG string.
        /// </summary>
        /// <param name="engine">The drawing engine to export.</param>
        /// <returns>The SVG document content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when engine is null.</exception>
        /// <exception cref="RenderingException">Thrown when export fails.</exception>
        public static string ExportToString(DrawingEngine engine)
        {
            if (engine == null)
                throw new ArgumentNullException(nameof(engine));

            try
            {
                using var stream = new MemoryStream();
                using (var managedStream = new SKManagedWStream(stream))
                {
                    using var canvas = SKSvgCanvas.Create(new SKRect(0, 0, engine.Width, engine.Height), managedStream);
                    engine.RenderToCanvas(canvas);
                    canvas.Flush();
                }

                return Encoding.UTF8.GetString(stream.ToArray());
            }
            catch (Exception ex)
            {
                throw new RenderingException("ExportSvgToString",
                    $"Failed to export SVG content: {ex.Message}", ex);
            }
        }
    }
}