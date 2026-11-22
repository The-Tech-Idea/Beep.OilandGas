using System.Globalization;
using SkiaSharp;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;

/// <summary>
/// RPT-01's PDF: the pages the report preview shows, drawn one PDF page each (AC-5).
/// </summary>
/// <remarks>
/// The report is text, so the PDF is the same text on the same page boundaries rather than a second rendering that could drift: the
/// report service hands over its paginated lines and this draws exactly them, with the same fixed-width font the preview uses. It is
/// deliberately plain - a heading, a rule, the lines - because a report's value is its numbers, and the tools' own PDF exporters are for
/// drawing surfaces.
/// </remarks>
public sealed class OgReportPdf : IOgTextPdf
{
    /// <summary>A4 at 72 points per inch, which is the unit a PDF page is measured in.</summary>
    private const float PageWidth = 595f;

    private const float PageHeight = 842f;

    private const float Margin = 42f;

    private const float LineHeight = 13f;

    public byte[] Render(IReadOnlyList<IReadOnlyList<string>> pages, string title)
    {
        ArgumentNullException.ThrowIfNull(pages);

        using var stream = new System.IO.MemoryStream();
        using var document = SKDocument.CreatePdf(stream);

        var culture = CultureInfo.InvariantCulture;

        using var font = new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Normal), 8.5f);
        using var heading = new SKFont(SKTypeface.FromFamilyName("Consolas", SKFontStyle.Bold), 10.5f);
        using var paint = new SKPaint { IsAntialias = true, Color = SKColors.Black };

        // A page is drawn even when the report produced none, so the file is never empty.
        var sheets = pages.Count == 0 ? [new List<string>()] : pages;

        for (var index = 0; index < sheets.Count; index++)
        {
            var canvas = document.BeginPage(PageWidth, PageHeight);
            var y = Margin;

            canvas.DrawText(title ?? string.Empty, Margin, y, SKTextAlign.Left, heading, paint);
            y += LineHeight * 1.6f;

            canvas.DrawText(
                "page " + (index + 1).ToString(culture) + " of " + sheets.Count.ToString(culture),
                Margin, y, SKTextAlign.Left, font, paint);
            y += LineHeight * 1.6f;

            using (var rule = new SKPaint { IsAntialias = false, Color = SKColors.Black, StrokeWidth = 0.6f })
            {
                canvas.DrawLine(Margin, y, PageWidth - Margin, y, rule);
            }

            y += LineHeight;

            foreach (var line in sheets[index])
            {
                if (y > PageHeight - Margin)
                {
                    // The report service paginates to fit; a line beyond the frame here would be a pagination bug, so it is drawn at
                    // the margin rather than silently dropped.
                    y = PageHeight - Margin;
                }

                canvas.DrawText(line ?? string.Empty, Margin, y, SKTextAlign.Left, font, paint);
                y += LineHeight;
            }

            document.EndPage();
        }

        document.Close();

        return stream.ToArray();
    }
}
