using System;
using Beep.OilandGas.Drawing.Core;
using Beep.OilandGas.Drawing.Exceptions;
using SkiaSharp;

namespace Beep.OilandGas.Drawing.Export
{
    /// <summary>
    /// Provides PDF export capabilities for drawings.
    /// </summary>
    public static class PdfExporter
    {
        /// <summary>
        /// Exports a drawing engine to a PDF file.
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
                using var document = SKDocument.CreatePdf(filePath);
                using var canvas = document.BeginPage(engine.Width, engine.Height);
                engine.RenderToCanvas(canvas);
                document.EndPage();
                document.Close();
            }
            catch (Exception ex)
            {
                throw new RenderingException("ExportToPdf",
                    $"Failed to export to PDF: {ex.Message}", ex);
            }
        }
    }
}