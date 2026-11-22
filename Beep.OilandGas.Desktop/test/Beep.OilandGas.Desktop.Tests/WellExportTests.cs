using SkiaSharp;
using Svg.Skia;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Export;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-09: a PDF with pages, an SVG that opens in Svg.Skia, a PNG at the requested DPI, no partial file on a
/// cancelled export, and a plan whose depth range and scale are the ones the export will use.
/// </summary>
public sealed class WellExportTests
{
    [Fact]
    public void A_pdf_is_written_with_pages_and_the_pdf_signature()
    {
        UseDefaultEnvironment();

        var path = Destination("plot.pdf");
        var service = Service();

        var exported = service.Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Pdf, path, ScaleRatio: 50f));

        Assert.True(exported.Success, $"{exported.Message} (reference {exported.Reference})");
        Assert.True(exported.Value!.Pages >= 1);
        Assert.True(File.Exists(path));

        var header = new byte[5];
        using (var stream = File.OpenRead(path))
        {
            Assert.Equal(5, stream.Read(header, 0, 5));
        }

        // AC-1: a real PDF, paginated by the tools' planner.
        Assert.Equal("%PDF-", System.Text.Encoding.ASCII.GetString(header));
        Assert.True(exported.Value.Bytes > 1_000, $"Only {exported.Value.Bytes} bytes.");
    }

    [Fact]
    public void A_pdf_gets_more_pages_as_the_scale_grows_finer()
    {
        UseDefaultEnvironment();

        var document = OgDocumentMapper.TripleComboDemo();
        var service = Service();

        var fit = service.Plan(document, new OgExportRequest(OgExportFormat.Pdf, Destination("a.pdf")));
        var fine = service.Plan(document, new OgExportRequest(OgExportFormat.Pdf, Destination("b.pdf"), ScaleRatio: 20f));

        Assert.True(fit.Success && fine.Success);
        Assert.True(fine.Value!.Pages >= fit.Value!.Pages);
        Assert.Equal(20f, fine.Value.ScaleRatio);

        // AC-1/AC-5: the page count the plan promised is the page count the exporter wrote.
        Assert.True(fine.Value.Pages > 1, $"A 1:20 export of {document.Axis.Bottom - document.Axis.Top:0} ft fitted on one page.");

        var written = service.Export(
            document,
            new OgExportRequest(OgExportFormat.Pdf, Destination("fine.pdf"), ScaleRatio: 20f));

        Assert.True(written.Success, written.Message);
        Assert.Equal(fine.Value.Pages, written.Value!.Pages);
    }

    [Fact]
    public void An_svg_opens_in_Svg_Skia()
    {
        UseDefaultEnvironment();

        var path = Destination("plot.svg");
        var service = Service();

        var exported = service.Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Svg, path));

        Assert.True(exported.Success, exported.Message);
        Assert.True(File.Exists(path));

        var text = File.ReadAllText(path);
        Assert.Contains("<svg", text, StringComparison.OrdinalIgnoreCase);

        // AC-2: parsed by the same renderer the desktop uses for SVG (ADR-001's probe), not just "it is text".
        var svg = new SKSvg();
        svg.Load(path);
        Assert.NotNull(svg.Picture);
        Assert.True(svg.Picture!.CullRect.Width > 0);
    }

    [Fact]
    public void A_png_matches_the_requested_dpi()
    {
        UseDefaultEnvironment();

        var lowPath = Destination("low.png");
        var highPath = Destination("high.png");
        var service = Service();

        Assert.True(service.Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Png, lowPath, Dpi: 96f)).Success);

        Assert.True(service.Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Png, highPath, Dpi: 192f)).Success);

        using var low = SKBitmap.Decode(lowPath);
        using var high = SKBitmap.Decode(highPath);

        Assert.NotNull(low);
        Assert.NotNull(high);

        // AC-3: twice the dpi is twice the pixels, within rounding.
        Assert.InRange(high!.Height, (2 * low!.Height) - 2, (2 * low.Height) + 2);
        Assert.InRange(high.Width, (2 * low.Width) - 2, (2 * low.Width) + 2);
    }

    [Fact]
    public void A_cancelled_export_leaves_no_file_at_all()
    {
        UseDefaultEnvironment();

        var path = Destination("cancelled.pdf");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var exported = Service().Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Pdf, path),
            cancellation.Token);

        // AC-4: no destination and no .partial.
        Assert.False(exported.Success);
        Assert.Equal(OgExportService.CodeCancelled, exported.Reference);
        Assert.False(File.Exists(path));
        Assert.False(File.Exists(path + ".partial"));
    }

    [Fact]
    public void The_plan_reports_the_depth_range_and_scale_the_export_uses()
    {
        UseDefaultEnvironment();

        var document = OgDocumentMapper.Create(OgTrackTemplate.TripleComboDemo);
        var service = Service();

        var whole = service.Plan(document, new OgExportRequest(OgExportFormat.Pdf, Destination("w.pdf"), ScaleRatio: 100f));
        var part = service.Plan(document, new OgExportRequest(
            OgExportFormat.Pdf,
            Destination("p.pdf"),
            DepthTop: 8200f,
            DepthBottom: 8250f,
            ScaleRatio: 100f));

        Assert.True(whole.Success && part.Success);
        Assert.Equal(8200f, part.Value!.Top, 1);
        Assert.Equal(8250f, part.Value.Bottom, 1);
        Assert.Equal(100f, part.Value.ScaleRatio);

        // AC-5: the narrower range at the same scale needs no more pages.
        Assert.True(part.Value.Pages <= whole.Value!.Pages);
    }

    [Fact]
    public void An_export_without_a_destination_is_refused()
    {
        UseDefaultEnvironment();

        var exported = Service().Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Pdf, string.Empty));

        Assert.False(exported.Success);
        Assert.Equal(OgExportService.CodeDestinationMissing, exported.Reference);
    }

    [Fact]
    public void A_high_dpi_png_export_warns_before_it_starts()
    {
        UseDefaultEnvironment();

        var path = Destination("huge.png");
        var exported = Service().Export(
            OgDocumentMapper.TripleComboDemo(),
            new OgExportRequest(OgExportFormat.Png, path, Dpi: 600f));

        Assert.True(exported.Success, exported.Message);
        Assert.Contains(exported.Value!.Warnings, warning =>
            warning.Contains("memory", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void The_export_folder_is_under_the_per_user_data_root()
    {
        Assert.True(OgPaths.IsWritableLocation(OgPaths.ExportsDirectory));
    }

    private static OgExportService Service() => new(new QuietFailureReporter());

    private static string Destination(string fileName)
    {
        var directory = Path.Combine(OgPaths.DataRoot, "exports-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);
        return Path.Combine(directory, fileName);
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }

    private sealed class QuietFailureReporter : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) =>
            FailureReferenceFormat.New();
    }
}
