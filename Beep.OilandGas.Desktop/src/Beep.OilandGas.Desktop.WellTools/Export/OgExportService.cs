using System.IO;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Export;

/// <summary>The formats a well visual can leave the application in.</summary>
public enum OgExportFormat
{
    Pdf,
    Svg,
    Png,
}

/// <summary>What to export, and how. Kept free of the tools' option type so a page can build it.</summary>
public sealed record OgExportRequest(
    OgExportFormat Format,
    string Destination,
    float? DepthTop = null,
    float? DepthBottom = null,
    float ScaleRatio = 0f,
    float Dpi = 96f,
    string? Title = null);

/// <summary>What an export produced, before or after writing it.</summary>
public sealed record OgExportPlan(int Pages, float Top, float Bottom, float ScaleRatio, string Format);

public sealed record OgExportResult(string Path, int Pages, long Bytes, IReadOnlyList<string> Warnings);

/// <summary>
/// VSB-09: exports a <see cref="WellboreDocument"/> as PDF, SVG or PNG through the tools' own
/// <see cref="WellboreExport"/> (the standard chosen in PLAN 09: the tools path for well visuals).
/// </summary>
/// <remarks>
/// <para>
/// <b>Planning before writing.</b> <see cref="Plan"/> paginates through the same options the export will
/// use, so a page can show "N pages" before a person commits to a 300-page PDF.
/// </para>
/// <para>
/// <b>No partial file.</b> The export writes to <c>&lt;destination&gt;.partial</c> and moves it into place
/// only after the exporter succeeds. A refusal, a cancellation or an exception deletes the partial, so a
/// half-written PDF is never left where someone will open it.
/// </para>
/// <para>
/// A destination that cannot be written is refused before any rendering starts, because the alternative is
/// rendering a large PNG and then discovering there is nowhere to put it.
/// </para>
/// </remarks>
public sealed class OgExportService
{
    public const string CodeDestinationUnwritable = "export-destination-unwritable";
    public const string CodeCancelled = "export-cancelled";
    public const string CodeDestinationMissing = "export-destination-missing";
    public const string CodeUnsupported = "export-format-unsupported";

    private readonly IFailureReporter _failures;

    public OgExportService(IFailureReporter failures)
    {
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
    }

    /// <summary>Paginates without writing anything. A page shows the page count from this.</summary>
    public OgResult<OgExportPlan> Plan(WellboreDocument document, OgExportRequest request)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            var options = OptionsFor(document, request);
            var plan = WellboreExport.Paginate(document, options);

            var top = document.Axis.Top;
            var bottom = document.Axis.Bottom;

            if (plan.Pages.Count > 0)
            {
                top = plan.Pages[0].Top;
                bottom = plan.Pages[0].Bottom;
            }

            return OgResult<OgExportPlan>.Ok(new OgExportPlan(
                plan.Pages.Count,
                top,
                bottom,
                options.ScaleRatio,
                request.Format.ToString().ToLowerInvariant()));        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"planning a {request.Format} export",
                "no page count was produced",
                FailureSeverity.Error);

            return OgResult<OgExportPlan>.Failed("The export could not be planned.", reference);
        }
    }

    /// <summary>Writes the export to <see cref="OgExportRequest.Destination"/>.</summary>
    public OgResult<OgExportResult> Export(
        WellboreDocument document,
        OgExportRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Destination))
        {
            return OgResult<OgExportResult>.Failed("An export needs a destination path.", CodeDestinationMissing);
        }

        var destination = Path.GetFullPath(request.Destination);
        var directory = Path.GetDirectoryName(destination);

        if (string.IsNullOrWhiteSpace(directory))
        {
            return OgResult<OgExportResult>.Failed($"'{destination}' has no directory to write to.", CodeDestinationUnwritable);
        }

        var partial = destination + ".partial";

        try
        {
            Directory.CreateDirectory(directory);

            if (cancellationToken.IsCancellationRequested)
            {
                return OgResult<OgExportResult>.Failed("The export was cancelled.", CodeCancelled);
            }

            var options = OptionsFor(document, request);
            var pages = 0;

            using (var stream = new FileStream(partial, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                switch (request.Format)
                {
                    case OgExportFormat.Pdf:
                        pages = WellboreExport.ToPdf(document, stream, options);
                        break;

                    case OgExportFormat.Svg:
                        WellboreExport.ToSvg(document, stream, options);
                        pages = 1;
                        break;

                    case OgExportFormat.Png:
                        WellboreExport.ToPng(document, stream, options);
                        pages = 1;
                        break;
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                // The file was written before the caller changed their mind: it must not survive.
                DeletePartial(partial);
                return OgResult<OgExportResult>.Failed("The export was cancelled.", CodeCancelled);
            }

            File.Move(partial, destination, overwrite: true);

            var bytes = new FileInfo(destination).Length;

            return OgResult<OgExportResult>.Ok(new OgExportResult(
                destination,
                pages,
                bytes,
                WarningsFor(request)));
        }
        catch (Exception ex)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"exporting the well view to {request.Format}",
                "no file was produced",
                FailureSeverity.Error);

            // Reported first, so the failure survives even if the partial cannot be removed.
            DeletePartial(partial);

            if (ex is UnauthorizedAccessException or DirectoryNotFoundException)
            {
                return OgResult<OgExportResult>.Failed(
                    $"The destination folder cannot be written: {directory}  (reference {reference})",
                    CodeDestinationUnwritable);
            }

            return OgResult<OgExportResult>.Failed(
                $"The export failed. Reference: {reference}",
                reference);
        }
    }

    private static WellborePrintOptions OptionsFor(WellboreDocument document, OgExportRequest request)
    {
        var options = new WellborePrintOptions
        {
            ScaleRatio = request.ScaleRatio,
            Dpi = request.Dpi,
            Title = request.Title ?? document.Name,
        };

        if (request.DepthTop is { } top && request.DepthBottom is { } bottom)
        {
            options.DepthRange = (Math.Min(top, bottom), Math.Max(top, bottom));
        }

        return options;
    }

    private static IReadOnlyList<string> WarningsFor(OgExportRequest request)
    {
        var warnings = new List<string>();

        if (request.Format == OgExportFormat.Png && request.Dpi > 300f)
        {
            warnings.Add($"A {request.Dpi:0} dpi PNG can need a lot of memory; export it at 300 dpi or use PDF if the machine struggles.");
        }

        return warnings;
    }

    /// <summary>
    /// Removes the partial file. Deliberately no catch: a delete that fails is a real problem the caller's
    /// handler reports, and swallowing it here would hide why a stale <c>.partial</c> is left behind.
    /// </summary>
    private static void DeletePartial(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
