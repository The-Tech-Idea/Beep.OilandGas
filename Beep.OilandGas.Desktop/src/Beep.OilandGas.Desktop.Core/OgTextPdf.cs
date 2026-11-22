namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>
/// Turns pages of plain text into a PDF (RPT-01's export). Defined here so the reporting service and the drawing layer can meet without
/// either owning the other: the service hands over the pages the preview shows, and the implementation draws exactly those.
/// </summary>
public interface IOgTextPdf
{
    /// <summary>A PDF with one page per entry of <paramref name="pages"/>, each drawn line for line.</summary>
    byte[] Render(IReadOnlyList<IReadOnlyList<string>> pages, string title);
}
