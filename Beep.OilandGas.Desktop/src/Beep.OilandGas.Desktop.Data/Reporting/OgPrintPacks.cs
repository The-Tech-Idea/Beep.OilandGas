using System.Globalization;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Wells;
using TheTechIdea.OilGas.WellSchematic;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;

/// <summary>What a pack contains (RPT-02's <c>OgPrintPack.ContentType</c>).</summary>
public sealed record OgPackContentType(string Key, string Name, string Description, string? ReportKey)
{
    /// <summary>The RPT-01 reports a pack can print one of per well - the ones whose parameters are a well.</summary>
    public bool IsReport => ReportKey is not null;

    public string Describe() => Name + " · " + Description;
}

/// <summary>Which wells a pack covers (RPT-02's <c>Scope</c>).</summary>
/// <param name="Wells">The wells named, when the scope is a selection or a saved list; empty for all of the field.</param>
public sealed record OgPackScope(string Kind, string? Field, IReadOnlyList<string> Wells)
{
    public const string All = "all";
    public const string FieldScope = "field";
    public const string Selected = "selected";

    public string Describe() => Kind switch
    {
        FieldScope => "the wells of field '" + (Field ?? string.Empty) + "'",
        Selected => Wells.Count.ToString(CultureInfo.InvariantCulture) + " selected well(s)",
        _ => "every well",
    };
}

/// <summary>The print options a pack applies to every document (RPT-02's <c>Options</c>, AC-5).</summary>
/// <param name="ScaleRatio">1:500 is 500; 0 fits the whole well on one page.</param>
public sealed record OgPackOptions(
    string Paper,
    float ScaleRatio,
    bool IncludeHeaderBlock,
    bool IncludeLegend,
    string? Title)
{
    public string Describe() => Paper + " · " + (ScaleRatio > 0
                                ? "1:" + ScaleRatio.ToString("0", CultureInfo.InvariantCulture)
                                : "fitted to one page") +
                                " · header " + (IncludeHeaderBlock ? "on" : "off") +
                                " · legend " + (IncludeLegend ? "on" : "off") +
                                (string.IsNullOrWhiteSpace(Title) ? string.Empty : " · title '" + Title + "'");
}

/// <summary>The pack as the page defines it (RPT-02's <c>OgPrintPack</c>).</summary>
public sealed record OgPrintPack(string ContentType, OgPackScope Scope, OgPackOptions Options)
{
    public string Describe() => ContentType + " · " + Scope.Describe() + " · " + Options.Describe();
}

/// <summary>One document of a pack, or why there is none (RPT-02's manifest item).</summary>
/// <param name="Status">written, skipped or failed.</param>
public sealed record OgPackItem(
    string WellId,
    string? File,
    int Pages,
    long Bytes,
    string Status,
    string? Reason,
    IReadOnlyList<string> Warnings)
{
    public bool Written => string.Equals(Status, OgPackRules.Written, StringComparison.Ordinal);

    public string Describe() => WellId + " · " + Status + (File is null ? string.Empty : " · " + File) +
                                (Pages > 0 ? " · " + Pages.ToString(CultureInfo.InvariantCulture) + " page(s)" : string.Empty) +
                                (Bytes > 0 ? " · " + (Bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB" : string.Empty) +
                                (Reason is null ? string.Empty : " · " + Reason) +
                                (Warnings.Count == 0 ? string.Empty : " · " + string.Join("; ", Warnings));
}

/// <summary>What a pack produced (RPT-02's <c>OgPackManifest</c>).</summary>
public sealed record OgPackManifest(
    string Folder,
    string ContentType,
    string Scope,
    string Options,
    IReadOnlyList<OgPackItem> Items,
    int Pages,
    long Bytes,
    bool Cancelled,
    bool StoppedOnError,
    string? ManifestPath,
    string? CsvPath,
    DateTime GeneratedAt)
{
    public int Written => Items.Count(item => item.Written);

    public int Skipped => Items.Count(item => string.Equals(item.Status, OgPackRules.Skipped, StringComparison.Ordinal));

    public int Failed => Items.Count(item => string.Equals(item.Status, OgPackRules.Failed, StringComparison.Ordinal));

    public string SummaryText =>
        Written.ToString(CultureInfo.InvariantCulture) + " document(s) · " +
        Skipped.ToString(CultureInfo.InvariantCulture) + " skipped · " +
        Failed.ToString(CultureInfo.InvariantCulture) + " failed · " +
        Pages.ToString(CultureInfo.InvariantCulture) + " page(s) · " +
        (Bytes / 1024d / 1024d).ToString("0.##", CultureInfo.InvariantCulture) + " MB" +
        (Cancelled ? " · cancelled, and the manifest holds what completed" : string.Empty) +
        (StoppedOnError ? " · stopped on the first failure" : string.Empty) +
        " · " + Folder;

    public string ItemsText => Items.Count == 0
        ? "The pack produced no item."
        : string.Join(Environment.NewLine, Items.Select(item => "· " + item.Describe()));

    public string ItemsCsv()
    {
        var text = new StringBuilder("well,file,pages,bytes,status,reason,warnings").AppendLine();

        foreach (var item in Items)
        {
            text.AppendLine(string.Join(",",
                Quote(item.WellId),
                Quote(item.File ?? string.Empty),
                item.Pages.ToString(CultureInfo.InvariantCulture),
                item.Bytes.ToString(CultureInfo.InvariantCulture),
                Quote(item.Status),
                Quote(item.Reason ?? string.Empty),
                Quote(string.Join("; ", item.Warnings))));
        }

        return text.ToString();
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>What a run would produce, before anything is written (RPT-02's preview).</summary>
public sealed record OgPackEstimate(
    int Files,
    IReadOnlyList<string> Wells,
    IReadOnlyList<string> Skipped,
    int SamplePages,
    long SampleBytes,
    IReadOnlyList<string> NameSample,
    IReadOnlyList<string> Warnings)
{
    public string Describe() => Files.ToString(CultureInfo.InvariantCulture) + " file(s) for " +
                                Wells.Count.ToString(CultureInfo.InvariantCulture) + " well(s)" +
                                (Skipped.Count == 0 ? string.Empty
                                    : " · " + Skipped.Count.ToString(CultureInfo.InvariantCulture) + " would be skipped") +
                                (SamplePages > 0
                                    ? " · the first document is " + SamplePages.ToString(CultureInfo.InvariantCulture) +
                                      " page(s), " + (SampleBytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB"
                                    : string.Empty);

    public string NamesText => NameSample.Count == 0
        ? "No file name to show."
        : string.Join(Environment.NewLine, NameSample);

    public string SkippedText => Skipped.Count == 0
        ? "Every well in scope is expected to produce a document."
        : string.Join(Environment.NewLine, Skipped);
}

/// <summary>How far a run has got (RPT-02's progress).</summary>
public sealed record OgPackProgress(int Done, int Total, string WellId)
{
    public string Describe() => Done.ToString(CultureInfo.InvariantCulture) + " of " +
                                Total.ToString(CultureInfo.InvariantCulture) + " · " + WellId;
}

/// <summary>The print-pack rules RPT-02 states once.</summary>
public static class OgPackRules
{
    public const string Written = "written";
    public const string Skipped = "skipped";
    public const string Failed = "failed";

    public const string CodeNoConnection = "pack-no-connection";
    public const string CodeNotPermitted = "pack-not-permitted";
    public const string CodeContentType = "pack-content-type";
    public const string CodeNoWells = "pack-no-wells";
    public const string CodeNoFolder = "pack-no-folder";
    public const string CodeWriteFailed = "pack-write-failed";
    public const string CodeReadFailed = "pack-read-failed";
    public const string CodeReportKey = "pack-report-key";

    /// <summary>The content types a per-well pack can hold, and why the others are not here.</summary>
    public static IReadOnlyList<OgPackContentType> ContentTypes { get; } =
    [
        new("wellview", "Well view", "the well's schematic and its tracks, drawn by the well view's own exporter", null),
        new("report", "Report", "an RPT-01 report whose parameters are a well, printed through the report service",
            OgReportsService.FormationPicks),
    ];

    /// <summary>The RPT-01 reports a pack may pick, because they take a well and print one document for it.</summary>
    public static IReadOnlyList<string> ReportKeys { get; } =
        [OgReportsService.Wellbores, OgReportsService.FormationPicks, OgReportsService.SurveyStations, OgReportsService.DailyReports];

    /// <summary>
    /// The paper sizes the owner defines. <c>PaperSize</c> is a readonly struct with static properties, not an enum, so the list is these
    /// four names and nothing else - offering a size the owner cannot make would be a choice that fails when it is used.
    /// </summary>
    public static IReadOnlyList<string> Papers { get; } = ["A4", "A3", "Letter", "Tabloid"];

    /// <summary>
    /// AC-4: a deterministic, unique file name. The index orders the pack, and the well's own UWI is made safe - so the same scope in the
    /// same order always gives the same names, and two wells whose names clean to the same text are still told apart by their index.
    /// </summary>
    public static string FileName(int index, string contentType, string wellId, string extension = "pdf")
    {
        var safe = new string((wellId ?? string.Empty)
            .Select(character => char.IsLetterOrDigit(character) || character is '-' ? character : '-')
            .ToArray()).Trim('-');

        // An identifier with nothing a file name can keep - or one that is only separators - still gets a usable name rather than a
        // file called '---'.
        if (safe.Length == 0)
        {
            safe = "well";
        }

        return (index + 1).ToString("000", CultureInfo.InvariantCulture) + "-" + contentType + "-" + safe + "." + extension;
    }

    /// <summary>AC-6: the pages a PDF holds, read out of the file's own page objects.</summary>
    public static int PageCount(byte[] pdf)
    {
        ArgumentNullException.ThrowIfNull(pdf);

        var text = Encoding.Latin1.GetString(pdf);

        var pages = CountOf(text, "/Type /Page") - CountOf(text, "/Type /Pages");

        // A PDF whose page objects this reader does not recognise reports zero rather than a guess.
        return Math.Max(0, pages);
    }

    /// <summary>The print options the page's choices become, on the owners' own type.</summary>
    public static WellborePrintOptions PrintOptions(OgPackOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var print = new WellborePrintOptions
        {
            ScaleRatio = options.ScaleRatio,
            IncludeHeaderBlock = options.IncludeHeaderBlock,
            IncludeLegend = options.IncludeLegend,
            Title = string.IsNullOrWhiteSpace(options.Title) ? null : options.Title!.Trim(),
        };

        // A name the owner does not define leaves the environment's default, which is the same paper the rest of the application prints on.
        print.Paper = (options.Paper ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "A4" => PaperSize.A4,
            "A3" => PaperSize.A3,
            "LETTER" => PaperSize.Letter,
            "TABLOID" => PaperSize.Tabloid,
            _ => print.Paper,
        };

        return print;
    }

    /// <summary>The two permissions a run needs: the pack's own and the export's.</summary>
    public static bool Permitted(OgPermissions permissions) =>
        permissions.IsSystemAdmin ||
        (permissions.PermissionSourceAvailable &&
         permissions.HasPermission("reporting.pack.run") &&
         permissions.HasPermission("data.export"));

    private static int CountOf(string text, string needle)
    {
        var count = 0;
        var at = 0;

        while ((at = text.IndexOf(needle, at, StringComparison.Ordinal)) >= 0)
        {
            count++;
            at += needle.Length;
        }

        return count;
    }
}

/// <summary>RPT-02: the print packs - a pack defined, previewed, run and manifested.</summary>
public interface IOgPrintPacks
{
    /// <summary>AC-1 and the feature's preview: how many documents a pack would write, and the first one, without writing anything.</summary>
    Task<OgResult<OgPackEstimate>> EstimateAsync(
        OgPrintPack pack, OgPermissions permissions, CancellationToken cancellationToken = default);

    /// <summary>AC-1..AC-6: writes one document per well and one manifest, cancellable at well granularity.</summary>
    Task<OgResult<OgPackManifest>> RunAsync(
        OgPrintPack pack,
        string folder,
        OgPermissions permissions,
        bool stopOnError,
        IProgress<OgPackProgress>? progress,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// RPT-02 over the well view's own exporter (<see cref="WellboreExport.ToPdf"/>, <see cref="WellborePrintOptions"/>) and the RPT-01 report
/// service, with the well models from <see cref="OgWellModelReader"/>.
/// </summary>
/// <remarks>
/// <para>
/// A pack is one document per well plus a manifest, and the document is the owners': a well view printed by <see cref="WellboreExport"/>
/// with the pack's paper, scale, header and legend, or an RPT-01 report printed through its own export. Nothing here draws a page.
/// </para>
/// <para>
/// A well that cannot produce a document is <b>listed as skipped with the reason</b> rather than dropped; a cancellation keeps what was
/// written and writes the manifest for it; and the manifest's page count is read back out of each file, so it cannot disagree with the
/// documents it describes.
/// </para>
/// </remarks>
public sealed class OgPrintPacksService : IOgPrintPacks
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private readonly IOgReports _reports;

    public OgPrintPacksService(Func<IDataSource?> source, IFailureReporter failures, IOgReports reports)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _reports = reports ?? throw new ArgumentNullException(nameof(reports));
    }

    public async Task<OgResult<OgPackEstimate>> EstimateAsync(
        OgPrintPack pack, OgPermissions permissions, CancellationToken cancellationToken = default)
    {
        var checkedPack = await CheckAsync(pack, permissions, cancellationToken).ConfigureAwait(false);

        if (!checkedPack.Success)
        {
            return OgResult<OgPackEstimate>.Failed(checkedPack.Message!, checkedPack.Reference);
        }

        try
        {
            var source = _source()!;

            var (models, wells) = await ScopeAsync(pack, source, cancellationToken).ConfigureAwait(false);

            var skipped = new List<string>();
            var usable = new List<WellModel>();
            var warnings = new List<string>();

            foreach (var model in models)
            {
                var problem = ProblemOf(pack, model);

                if (problem is null)
                {
                    usable.Add(model);
                }
                else
                {
                    skipped.Add(model.Header.Uwi + " · " + problem);
                }
            }

            var names = usable.Take(5)
                .Select((model, index) => OgPackRules.FileName(index, pack.ContentType, model.Header.Uwi ?? string.Empty))
                .ToList();

            var samplePages = 0;
            var sampleBytes = 0L;

            if (usable.Count > 0)
            {
                // The first document is rendered into memory, so the preview's page count and size are measured rather than guessed.
                var (bytes, _, _) = await RenderAsync(pack, usable[0], warnings, cancellationToken).ConfigureAwait(false);

                samplePages = OgPackRules.PageCount(bytes);
                sampleBytes = bytes.Length;
            }

            return OgResult<OgPackEstimate>.Ok(new OgPackEstimate(
                usable.Count, wells, skipped, samplePages, sampleBytes, names, warnings));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "estimating the print pack", "the pack is not estimated", FailureSeverity.Error);

            return OgResult<OgPackEstimate>.Failed(
                "The pack could not be estimated (reference " + reference + ").",
                reference ?? OgPackRules.CodeReadFailed);
        }
    }

    public async Task<OgResult<OgPackManifest>> RunAsync(
        OgPrintPack pack,
        string folder,
        OgPermissions permissions,
        bool stopOnError,
        IProgress<OgPackProgress>? progress,
        CancellationToken cancellationToken = default)
    {
        // The folder is the cheapest thing to be wrong about, so it is answered first rather than after a read.
        if (string.IsNullOrWhiteSpace(folder))
        {
            return OgResult<OgPackManifest>.Failed(
                "A pack writes into a folder; none was given.", OgPackRules.CodeNoFolder);
        }

        var checkedPack = await CheckAsync(pack, permissions, cancellationToken).ConfigureAwait(false);

        if (!checkedPack.Success)
        {
            return OgResult<OgPackManifest>.Failed(checkedPack.Message!, checkedPack.Reference);
        }

        try
        {
            var source = _source()!;

            var (models, _) = await ScopeAsync(pack, source, cancellationToken).ConfigureAwait(false);

            Directory.CreateDirectory(folder);

            var items = new List<OgPackItem>();
            var cancelling = false;
            var stopped = false;
            var index = 0;

            for (var position = 0; position < models.Count; position++)
            {
                var model = models[position];
                var well = model.Header.Uwi ?? string.Empty;

                // AC-3: a cancellation is taken at a well boundary, so what has been written is complete.
                if (cancellationToken.IsCancellationRequested)
                {
                    cancelling = true;
                    break;
                }

                progress?.Report(new OgPackProgress(position, models.Count, well));

                var problem = ProblemOf(pack, model);

                if (problem is not null)
                {
                    items.Add(new OgPackItem(well, null, 0, 0, OgPackRules.Skipped, problem, []));
                    continue;
                }

                var name = OgPackRules.FileName(index, pack.ContentType, well);
                var path = Path.Combine(folder, name);

                try
                {
                    var (bytes, warnings, _) = await RenderAsync(pack, model, [], cancellationToken).ConfigureAwait(false);

                    await File.WriteAllBytesAsync(path, bytes, cancellationToken).ConfigureAwait(false);

                    // AC-6: the pages are read back from the file that was written.
                    items.Add(new OgPackItem(well, name, OgPackRules.PageCount(bytes), bytes.Length, OgPackRules.Written, null, warnings));
                    index++;
                }
                catch (OperationCanceledException)
                {
                    // AC-3: a cancellation is an ordinary answer, and it is reported so a shortened pack is visible in the record.
                    _failures.ReportHandled(
                        new OperationCanceledException("the pack was cancelled"), "running the print pack",
                        "the pack stops at the well it had reached", FailureSeverity.Degraded);

                    cancelling = true;
                    break;
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    var reference = _failures.ReportHandled(
                        ex, "writing the pack document for " + well, "that document is not written", FailureSeverity.Error);

                    items.Add(new OgPackItem(well, name, 0, 0, OgPackRules.Failed,
                        "the file could not be written (reference " + reference + ")", []));

                    // The feature's disk-full state: the run stops and the manifest records what completed.
                    stopped = true;
                    break;
                }
            }

            progress?.Report(new OgPackProgress(items.Count(item => item.Written), models.Count, "(finished)"));

            var manifest = new OgPackManifest(
                folder,
                pack.ContentType,
                pack.Scope.Describe(),
                pack.Options.Describe(),
                items,
                items.Sum(item => item.Pages),
                items.Sum(item => item.Bytes),
                cancelling,
                stopped,
                null,
                null,
                DateTime.UtcNow);

            var (json, csv) = WriteManifest(folder, manifest);

            return OgResult<OgPackManifest>.Ok(manifest with { ManifestPath = json, CsvPath = csv });
        }
        catch (OperationCanceledException)
        {
            _failures.ReportHandled(
                new OperationCanceledException("the pack was cancelled before it started"), "running the print pack",
                "no document was written", FailureSeverity.Degraded);

            return OgResult<OgPackManifest>.Failed(
                "The pack was cancelled before any document was written, so there is no manifest.",
                OgPackRules.CodeWriteFailed);
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "running the print pack", "the pack is not run", FailureSeverity.Error);

            return OgResult<OgPackManifest>.Failed(
                "The pack could not be run (reference " + reference + ").",
                reference ?? OgPackRules.CodeWriteFailed);
        }
    }

    // ------------------------------------------------------------------ the pack's parts

    private async Task<OgResult> CheckAsync(
        OgPrintPack pack, OgPermissions permissions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(pack);
        ArgumentNullException.ThrowIfNull(permissions);

        if (!OgPackRules.Permitted(permissions))
        {
            return OgResult.Failed(
                "Running a pack needs reporting.pack.run and data.export" +
                (permissions.PermissionSourceAvailable
                    ? ", and this person does not hold both."
                    : ", and the permission source could not be read - so access is refused rather than assumed."),
                OgPackRules.CodeNotPermitted);
        }

        if (OgPackRules.ContentTypes.FirstOrDefault(type =>
                string.Equals(type.Key, pack.ContentType, StringComparison.OrdinalIgnoreCase)) is not { } contentType)
        {
            return OgResult.Failed(
                "'" + pack.ContentType + "' is not a pack content type; use " +
                string.Join(", ", OgPackRules.ContentTypes.Select(type => type.Key)) + ".",
                OgPackRules.CodeContentType);
        }

        if (contentType.IsReport &&
            !OgPackRules.ReportKeys.Contains(pack.Options.Title ?? string.Empty, StringComparer.OrdinalIgnoreCase) &&
            !string.Equals(pack.Options.Title, contentType.ReportKey, StringComparison.OrdinalIgnoreCase))
        {
            // The report to print is carried in the options' title only when the pack type is a report; a report key outside the list is
            // refused here rather than producing a pack of empty documents.
            var wanted = pack.Options.Title;

            if (!string.IsNullOrWhiteSpace(wanted) &&
                !OgPackRules.ReportKeys.Contains(wanted.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                return OgResult.Failed(
                    "'" + wanted + "' is not a report a pack can print per well; use " +
                    string.Join(", ", OgPackRules.ReportKeys) + ".", OgPackRules.CodeReportKey);
            }
        }

        if (_source() is null)
        {
            return OgResult.Failed("No connection is open, so the pack cannot be read.", OgPackRules.CodeNoConnection);
        }

        // AC-…: nothing in scope is a refusal with the reason, not an empty pack.
        var source = _source()!;
        var (models, _) = await ScopeAsync(pack, source, cancellationToken).ConfigureAwait(false);

        return models.Count == 0
            ? OgResult.Failed(
                "No well is in scope: " + pack.Scope.Describe() +
                (string.IsNullOrWhiteSpace(pack.Scope.Field) && pack.Scope.Wells.Count == 0
                    ? " - the WELL table holds no active well."
                    : " - no active well matches it."),
                OgPackRules.CodeNoWells)
            : OgResult.Ok();
    }

    /// <summary>The wells in scope, as models, and their UWIs in the order the pack will use.</summary>
    private async Task<(List<WellModel> Models, IReadOnlyList<string> Wells)> ScopeAsync(
        OgPrintPack pack, IDataSource source, CancellationToken cancellationToken)
    {
        var field = string.Equals(pack.Scope.Kind, OgPackScope.FieldScope, StringComparison.OrdinalIgnoreCase)
            ? pack.Scope.Field
            : null;

        var models = await OgWellModelReader.ReadAsync(source, field, 0, null, cancellationToken).ConfigureAwait(false);

        if (string.Equals(pack.Scope.Kind, OgPackScope.Selected, StringComparison.OrdinalIgnoreCase) &&
            pack.Scope.Wells.Count > 0)
        {
            var wanted = pack.Scope.Wells.ToHashSet(StringComparer.OrdinalIgnoreCase);

            models = models
                .Where(model => wanted.Contains(model.Header.Uwi ?? string.Empty))
                .ToList();
        }

        return (models, models.Select(model => model.Header.Uwi ?? string.Empty).ToList());
    }

    /// <summary>
    /// AC-2: why a well would produce no document - the reason the manifest and the preview both print. Null when it would.
    /// </summary>
    private static string? ProblemOf(OgPrintPack pack, WellModel model)
    {
        var uwi = model.Header.Uwi ?? string.Empty;

        if (string.IsNullOrWhiteSpace(uwi))
        {
            return "the well has no identifier";
        }

        if (model.Survey.Count < 2)
        {
            return "no survey (2+ stations), so the well view cannot be drawn to depth";
        }

        if (!string.Equals(pack.ContentType, "report", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return model.Tops.Count == 0 && string.Equals(ReportKeyOf(pack), OgReportsService.FormationPicks, StringComparison.OrdinalIgnoreCase)
            ? "no formation pick is recorded, so the picks report would be empty"
            : null;
    }

    private static string ReportKeyOf(OgPrintPack pack) =>
        string.IsNullOrWhiteSpace(pack.Options.Title) ? OgReportsService.FormationPicks : pack.Options.Title!.Trim();

    /// <summary>One document, rendered into memory by the owner's exporter - the pages, so the manifest can count them.</summary>
    private async Task<(byte[] Bytes, IReadOnlyList<string> Warnings, int Pages)> RenderAsync(
        OgPrintPack pack, WellModel model, List<string> warnings, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.Equals(pack.ContentType, "report", StringComparison.OrdinalIgnoreCase))
        {
            return await RenderReportAsync(pack, model, cancellationToken).ConfigureAwait(false);
        }

        var options = new WellModelConversionOptions();

        if (string.Equals(model.Header.DepthDatum, "KB", StringComparison.OrdinalIgnoreCase))
        {
            options.DepthUnit = DepthUnit.Meters;
        }

        var document = WellModelConverter.ToWellboreView(model, options, out var conversionWarnings);

        warnings.AddRange(conversionWarnings);

        var print = OgPackRules.PrintOptions(pack.Options);

        print.Title ??= document.Name + " · " + model.Header.Uwi;

        using var stream = new MemoryStream();

        WellboreExport.ToPdf(document, stream, print);

        var bytes = stream.ToArray();

        return (bytes, warnings, OgPackRules.PageCount(bytes));
    }

    /// <summary>A pack of reports: each document is the RPT-01 report printed through its own service.</summary>
    private async Task<(byte[] Bytes, IReadOnlyList<string> Warnings, int Pages)> RenderReportAsync(
        OgPrintPack pack, WellModel model, CancellationToken cancellationToken)
    {
        var key = ReportKeyOf(pack);
        var uwi = model.Header.Uwi ?? string.Empty;

        // The report service gates itself by the area's permission; the pack already checked its own two, so the report is run with the
        // permissions the person holds. A refusal is the well's skip reason, not a silent empty document.
        var ran = await _reports.RunAsync(
            key,
            new Dictionary<string, string> { ["well"] = uwi },
            new OgPermissions(["pack"], [PermissionOf(key)], permissionSourceAvailable: true),
            null,
            cancellationToken).ConfigureAwait(false);

        if (!ran.Success || ran.Value is null)
        {
            throw new InvalidOperationException(ran.Message ?? "the report was refused");
        }

        // The report service writes to a path, so the pack exports to a scratch file and reads it back - the bytes are the service's
        // own document, not a second rendering.
        var scratch = Path.Combine(Path.GetTempPath(),
            "og-pack-" + Guid.NewGuid().ToString("n")[..8] + "-" + ran.Value.Key + ".pdf");

        try
        {
            var exported = await _reports.ExportAsync(ran.Value, "pdf", scratch).ConfigureAwait(false);

            if (!exported.Success)
            {
                throw new InvalidOperationException(exported.Message ?? "the report could not be written");
            }

            var bytes = await File.ReadAllBytesAsync(scratch, cancellationToken).ConfigureAwait(false);

            return (bytes, ran.Value.Warnings, OgPackRules.PageCount(bytes));
        }
        finally
        {
            // A scratch file the process cannot delete is reported and left: losing a temporary file is not losing the pack.
            try { File.Delete(scratch); }
            catch (IOException ex)
            {
                _failures.ReportHandled(
                    ex, "removing the pack scratch file " + scratch, "the temporary file is left behind",
                    FailureSeverity.Degraded);
            }
        }
    }

    private static string PermissionOf(string reportKey) => reportKey switch
    {
        OgReportsService.Wellbores or OgReportsService.DailyReports => "drilling.read",
        OgReportsService.FormationPicks or OgReportsService.SurveyStations => "stratigraphy.read",
        _ => "wells.read",
    };

    /// <summary>AC-1 and AC-3: the manifest, as JSON and as CSV, written where the documents are.</summary>
    private (string? Json, string? Csv) WriteManifest(string folder, OgPackManifest manifest)
    {
        try
        {
            var json = Path.Combine(folder, "manifest.json");
            var csv = Path.Combine(folder, "manifest.csv");

            var stored = new
            {
                generatedAt = manifest.GeneratedAt,
                contentType = manifest.ContentType,
                scope = manifest.Scope,
                options = manifest.Options,
                pages = manifest.Pages,
                bytes = manifest.Bytes,
                cancelled = manifest.Cancelled,
                stoppedOnError = manifest.StoppedOnError,
                items = manifest.Items.Select(item => new
                {
                    wellId = item.WellId,
                    file = item.File,
                    pages = item.Pages,
                    bytes = item.Bytes,
                    status = item.Status,
                    reason = item.Reason,
                    warnings = item.Warnings,
                }),
            };

            File.WriteAllText(json, JsonSerializer.Serialize(stored, new JsonSerializerOptions { WriteIndented = true }),
                new UTF8Encoding(false));
            File.WriteAllText(csv, manifest.ItemsCsv(), new UTF8Encoding(false));

            return (json, csv);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // The documents were written; the manifest could not be. That is reported, and the run still returns its items.
            _failures.ReportHandled(
                ex, "writing the pack manifest in " + folder, "the manifest is not written", FailureSeverity.Error);

            return (null, null);
        }
    }
}
