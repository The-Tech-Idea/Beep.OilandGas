using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data.DrillingAndConstruction;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Editor.UOW;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;

/// <summary>One parameter of a report (RPT-01's <c>OgReportDefinition.Parameters</c>).</summary>
/// <param name="Allowed">The values the form offers; empty means the key is free text.</param>
public sealed record OgReportParameter(
    string Key,
    string Label,
    bool Required,
    string? Hint,
    string? DefaultValue,
    IReadOnlyList<string> Allowed)
{
    public bool IsDate => string.Equals(Hint, "date", StringComparison.OrdinalIgnoreCase);

    public string Describe() => Label + " (" + Key + ")" + (Required ? " · required" : " · optional") +
                                (Allowed.Count == 0 ? string.Empty : " · one of " + string.Join(", ", Allowed)) +
                                (string.IsNullOrWhiteSpace(DefaultValue) ? string.Empty : " · default " + DefaultValue);
}

/// <summary>A report the catalogue offers (RPT-01).</summary>
/// <param name="Permission">The read permission of the area the report draws on - AC-4's gate.</param>
public sealed record OgReportDefinition(
    string Key,
    string Name,
    string Area,
    string Description,
    string Permission,
    IReadOnlyList<OgReportParameter> Parameters,
    bool TemplateRequired)
{
    public string Describe() => Name + " · " + Area + " · " + Description +
                                (TemplateRequired ? " · a template is needed" : string.Empty);

    public string ParametersText => Parameters.Count == 0
        ? "No parameters."
        : string.Join(Environment.NewLine, Parameters.Select(parameter => "· " + parameter.Describe()));
}

/// <summary>A column of a report's table.</summary>
public sealed record OgReportColumn(string Header, string Key, bool Numeric);

/// <summary>The table a report produced, already as text so the preview, the export and the PDF cannot differ.</summary>
public sealed record OgReportTable(
    string Key,
    string Title,
    IReadOnlyList<OgReportColumn> Columns,
    IReadOnlyList<IReadOnlyList<string>> Rows)
{
    public int Count => Rows.Count;

    /// <summary>The table as fixed-width text, which is what a page of the preview holds.</summary>
    public IReadOnlyList<string> Lines()
    {
        if (Columns.Count == 0)
        {
            return [];
        }

        var widths = Columns.Select((column, index) => Math.Max(
            column.Header.Length,
            Rows.Count == 0 ? 0 : Rows.Max(row => index < row.Count ? row[index].Length : 0))).ToList();

        string Line(IReadOnlyList<string> cells) => string.Join("  ",
            Columns.Select((column, index) =>
            {
                var cell = index < cells.Count ? cells[index] : string.Empty;

                return column.Numeric ? cell.PadLeft(widths[index]) : cell.PadRight(widths[index]);
            })).TrimEnd();

        var lines = new List<string> { Line(Columns.Select(column => column.Header).ToList()) };

        lines.Add(string.Join("  ", widths.Select(width => new string('-', width))));
        lines.AddRange(Rows.Select(Line));

        return lines;
    }
}

/// <summary>One page of the previewed document (RPT-01, AC-1).</summary>
public sealed record OgReportPage(int Number, IReadOnlyList<string> Lines, string Text);

/// <summary>What running a report gave (RPT-01's <c>OgReportResult</c>).</summary>
public sealed record OgReportResult(
    string Key,
    string Title,
    string Area,
    IReadOnlyDictionary<string, string> Parameters,
    OgReportTable Table,
    IReadOnlyList<OgReportPage> Pages,
    IReadOnlyList<string> Tokens,
    string TemplateName,
    bool UsedFallbackLayout,
    int TemplateVersion,
    bool Empty,
    IReadOnlyList<string> Warnings,
    DateTime GeneratedAt)
{
    public int PageCount => Pages.Count;

    public string TitleText => Title + (UsedFallbackLayout ? " (generated layout)" : " · template " + TemplateName);

    public string SummaryText =>
        Title + " · " + Table.Count.ToString(CultureInfo.InvariantCulture) + " row(s) · " +
        Pages.Count.ToString(CultureInfo.InvariantCulture) + " page(s) · " +
        (Empty ? "no data for these parameters" : "generated") + " · " + TitleText +
        (Parameters.Count == 0
            ? string.Empty
            : " · " + string.Join(", ", Parameters.Select(pair => pair.Key + "=" + pair.Value)));

    public string PageText => Pages.Count == 0
        ? "The report produced no page."
        : Pages[0].Text;

    public string WarningsText => Warnings.Count == 0
        ? "The report raised no warning."
        : string.Join(Environment.NewLine, Warnings.Select(warning => "! " + warning));

    public string TokensText => Tokens.Count == 0
        ? "No token is available to the template."
        : string.Join(Environment.NewLine, Tokens.Select(token => "{{" + token + "}}"));

    public string GeneratedText =>
        "generated " + GeneratedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) +
        " · template " + TemplateName + " v" + TemplateVersion.ToString(CultureInfo.InvariantCulture) +
        (UsedFallbackLayout ? " (generated layout - no template was chosen)" : string.Empty);
}

/// <summary>A stored template (RPT-01, AC-6: versioned, and the default is chosen explicitly).</summary>
public sealed record OgReportTemplate(
    string Name,
    string Body,
    bool IsDefault,
    int Version,
    DateTime UpdatedAt)
{
    public string Describe() => Name + " v" + Version.ToString(CultureInfo.InvariantCulture) +
                                (IsDefault ? " · default" : string.Empty) + " · " +
                                UpdatedAt.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
}

/// <summary>What an export wrote (RPT-01).</summary>
public sealed record OgReportExport(string Format, string Path, long Bytes, string Message)
{
    public string Summary => Format.ToUpperInvariant() + " · " +
                             (Bytes / 1024d).ToString("0.#", CultureInfo.InvariantCulture) + " KB · " + Path;
}

/// <summary>The reporting rules RPT-01 states once.</summary>
public static class OgReportRules
{
    public const string CodeNoConnection = "report-no-connection";
    public const string CodeUnknownReport = "report-unknown";
    public const string CodeMissingParameter = "report-parameter-missing";
    public const string CodePermission = "report-permission";
    public const string CodeUnknownToken = "report-token-unknown";
    public const string CodeTemplateInvalid = "report-template-invalid";
    public const string CodeTemplateName = "report-template-name";
    public const string CodeActorRequired = "report-actor-required";
    public const string CodeReadFailed = "report-read-failed";
    public const string CodeWriteFailed = "report-write-failed";
    public const string CodeExportPath = "report-export-path";
    public const string CodeExportFormat = "report-export-format";
    public const string CodeNoPdf = "report-pdf-unavailable";

    public static IReadOnlyList<string> Formats { get; } = ["pdf", "html", "csv", "xlsx"];

    /// <summary>How many lines one page of the preview holds - the page a PDF is too.</summary>
    public const int LinesPerPage = 48;

    /// <summary>The token the table is inserted at, so a template decides where the rows go.</summary>
    public const string TableToken = "Table";

    /// <summary>
    /// AC-2: the document with its tokens replaced. A token the report does not offer is a refusal naming it - the feature's own rule -
    /// rather than being left in the text or dropped, because either would print a document that is quietly wrong.
    /// </summary>
    public static string? Render(string body, IReadOnlyDictionary<string, string> tokens, out IReadOnlyList<string> unknown)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(tokens);

        var missing = new List<string>();
        var text = new StringBuilder();
        var at = 0;

        while (at < body.Length)
        {
            var open = body.IndexOf("{{", at, StringComparison.Ordinal);

            if (open < 0)
            {
                text.Append(body, at, body.Length - at);
                break;
            }

            text.Append(body, at, open - at);

            var close = body.IndexOf("}}", open + 2, StringComparison.Ordinal);

            if (close < 0)
            {
                // An unclosed marker is an unknown token too: the name is what follows '{{'.
                missing.Add(body[(open + 2)..].Trim());
                at = body.Length;
                break;
            }

            var name = body[(open + 2)..close].Trim();

            if (tokens.TryGetValue(name, out var value))
            {
                text.Append(value);
            }
            else
            {
                missing.Add(name);
            }

            at = close + 2;
        }

        unknown = missing;

        return missing.Count == 0 ? text.ToString() : null;
    }

    /// <summary>Splits the document into pages of a fixed number of lines - the pages the preview and the PDF share.</summary>
    public static IReadOnlyList<OgReportPage> Paginate(string? document, int linesPerPage = LinesPerPage)
    {
        var perPage = linesPerPage > 0 ? linesPerPage : LinesPerPage;
        var lines = (document ?? string.Empty).Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');

        var pages = new List<OgReportPage>();

        for (var start = 0; start < lines.Length; start += perPage)
        {
            var slice = lines.Skip(start).Take(perPage).ToList();

            pages.Add(new OgReportPage(pages.Count + 1, slice, string.Join(Environment.NewLine, slice)));
        }

        if (pages.Count == 0)
        {
            pages.Add(new OgReportPage(1, [], string.Empty));
        }

        return pages;
    }

    /// <summary>The layout used when no template was chosen: labelled, because a generated document must say so (the feature's state).</summary>
    public static string FallbackLayout() =>
        "{{Name}}" + Environment.NewLine +
        "{{Area}} · generated {{Generated}}" + Environment.NewLine +
        "Parameters: {{Parameters}}" + Environment.NewLine +
        new string('=', 72) + Environment.NewLine +
        "{{" + TableToken + "}}" + Environment.NewLine +
        new string('=', 72) + Environment.NewLine +
        "{{Count}} row(s)";

    /// <summary>AC-3: the sentence a report prints when it has no rows for the parameters - never an empty page.</summary>
    public static string NoData(string parameters) =>
        "No data for these parameters" + (string.IsNullOrWhiteSpace(parameters) ? "." : ": " + parameters + ".");

    /// <summary>
    /// AC-4: whether the person may run the report. An administrator always may; otherwise the area's read permission is required, and a
    /// permission source that could not be read is treated as a refusal rather than as a grant.
    /// </summary>
    public static bool Allowed(OgPermissions permissions, string permission)
    {
        ArgumentNullException.ThrowIfNull(permissions);

        if (permissions.IsSystemAdmin)
        {
            return true;
        }

        return permissions.PermissionSourceAvailable && permissions.HasPermission(permission);
    }

    /// <summary>A template name a file can carry.</summary>
    public static bool ValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) &&
        name.Trim().IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
        !name.Contains("..", StringComparison.Ordinal);

    public static string Figure(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
}

/// <summary>TRN… RPT-01: the report catalogue, its templates and its exports.</summary>
public interface IOgReports
{
    /// <summary>The reports the catalogue offers, grouped by area by the page.</summary>
    Task<OgResult<IReadOnlyList<OgReportDefinition>>> CatalogueAsync(CancellationToken cancellationToken = default);

    /// <summary>The stored templates and which is the default.</summary>
    Task<OgResult<IReadOnlyList<OgReportTemplate>>> TemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>AC-1..AC-4: runs a report and renders its document.</summary>
    Task<OgResult<OgReportResult>> RunAsync(
        string key,
        IReadOnlyDictionary<string, string> parameters,
        OgPermissions permissions,
        string? templateName = null,
        CancellationToken cancellationToken = default);

    /// <summary>AC-6: saves a template as a new version, and marks it the default when asked.</summary>
    Task<OgResult<OgReportTemplate>> SaveTemplateAsync(
        string name, string body, bool makeDefault, string actor, CancellationToken cancellationToken = default);

    /// <summary>AC-5: writes the report's document - the pages the preview shows.</summary>
    Task<OgResult<OgReportExport>> ExportAsync(
        OgReportResult report, string format, string path, CancellationToken cancellationToken = default);
}

/// <summary>
/// RPT-01 over the tables the application already reads, with the templates kept as files under the data root.
/// </summary>
/// <remarks>
/// <para>
/// A report is a definition - key, name, area, permission, parameters - and a source that reads one table through <see cref="OgUow"/>
/// and formats the rows. Nothing here composes SQL: the rows come from the entity models and the filtering is LINQ, so the desktop
/// assembly still contains no statement text.
/// </para>
/// <para>
/// The document is text with <c>{{Token}}</c> markers, rendered once and paginated once: the preview, the HTML, the CSV/XLSX table and
/// the PDF are all that same rendering, so they cannot disagree about what the report says.
/// </para>
/// </remarks>
public sealed class OgReportsService : IOgReports
{
    private readonly Func<IDataSource?> _source;
    private readonly IFailureReporter _failures;
    private readonly Func<string> _templatesFolder;
    private readonly IOgTextPdf? _pdf;

    public const string WellsRegister = "wells-register";
    public const string Wellbores = "wellbores";
    public const string FormationPicks = "formation-picks";
    public const string SurveyStations = "survey-stations";
    public const string DailyReports = "daily-reports";

    /// <summary>The catalogue, stated once so the page, the tests and the docs read the same list (the feature's open question).</summary>
    public static IReadOnlyList<OgReportDefinition> Definitions { get; } =
    [
        new(WellsRegister, "Wells register", "exploration",
            "Every well in the database, with its field, status and depth.",
            "wells.read",
            [new OgReportParameter("field", "Field", false, null, null, [])],
            TemplateRequired: false),

        new(Wellbores, "Wellbores of a well", "drilling",
            "The bores recorded for one well, with their kind, status and total depth.",
            "drilling.read",
            [new OgReportParameter("well", "Well UWI", true, null, null, [])],
            TemplateRequired: false),

        new(FormationPicks, "Formation picks", "stratigraphy",
            "The formation picks of one well, with their depth and interpreter.",
            "stratigraphy.read",
            [new OgReportParameter("well", "Well UWI", true, null, null, [])],
            TemplateRequired: false),

        new(SurveyStations, "Survey stations", "stratigraphy",
            "The directional survey stations of one well, in measured depth order.",
            "stratigraphy.read",
            [new OgReportParameter("well", "Well UWI", true, null, null, [])],
            TemplateRequired: false),

        new(DailyReports, "Daily drilling reports", "drilling",
            "The daily reports of one well over a period, with their depth at each end and their cost.",
            "drilling.read",
            [
                new OgReportParameter("well", "Well UWI", true, null, null, []),
                new OgReportParameter("from", "From", false, "date", null, []),
                new OgReportParameter("to", "To", false, "date", null, []),
            ],
            TemplateRequired: false),
    ];

    public OgReportsService(
        Func<IDataSource?> source,
        IFailureReporter failures,
        Func<string> templatesFolder,
        IOgTextPdf? pdf = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _templatesFolder = templatesFolder ?? throw new ArgumentNullException(nameof(templatesFolder));
        _pdf = pdf;
    }

    public Task<OgResult<IReadOnlyList<OgReportDefinition>>> CatalogueAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OgResult<IReadOnlyList<OgReportDefinition>>.Ok(Definitions));

    public Task<OgResult<IReadOnlyList<OgReportTemplate>>> TemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var folder = _templatesFolder();

            if (!Directory.Exists(folder))
            {
                return Task.FromResult(OgResult<IReadOnlyList<OgReportTemplate>>.Ok([]));
            }

            var templates = new List<OgReportTemplate>();

            foreach (var file in Directory.GetFiles(folder, "*.template.json").OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var loaded = Read(file, out var problem);

                if (loaded is not null)
                {
                    templates.Add(loaded);
                }
                else if (problem is not null)
                {
                    _failures.ReportHandled(
                        new InvalidDataException(problem), "reading the report template " + file,
                        "the template is left out of the list", FailureSeverity.Degraded);
                }
            }

            return Task.FromResult(OgResult<IReadOnlyList<OgReportTemplate>>.Ok(
                templates.OrderByDescending(template => template.IsDefault)
                    .ThenBy(template => template.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList()));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "listing the report templates", "the templates are not shown", FailureSeverity.Error);

            return Task.FromResult(OgResult<IReadOnlyList<OgReportTemplate>>.Failed(
                "The report templates could not be listed (reference " + reference + ").",
                reference ?? OgReportRules.CodeReadFailed));
        }
    }

    public async Task<OgResult<OgReportResult>> RunAsync(
        string key,
        IReadOnlyDictionary<string, string> parameters,
        OgPermissions permissions,
        string? templateName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);
        ArgumentNullException.ThrowIfNull(permissions);

        var definition = Definitions.FirstOrDefault(candidate =>
            string.Equals(candidate.Key, key?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (definition is null)
        {
            return OgResult<OgReportResult>.Failed(
                "'" + key + "' is not a report of the catalogue. The reports are " +
                string.Join(", ", Definitions.Select(candidate => candidate.Key)) + ".",
                OgReportRules.CodeUnknownReport);
        }

        // AC-4: the area's read permission is the gate, and an unreadable permission source is a refusal.
        if (!OgReportRules.Allowed(permissions, definition.Permission))
        {
            return OgResult<OgReportResult>.Failed(
                "Running '" + definition.Name + "' needs the " + definition.Permission + " permission" +
                (permissions.PermissionSourceAvailable
                    ? ", which this person does not have."
                    : ", and the permission source could not be read - so access is refused rather than assumed."),
                OgReportRules.CodePermission);
        }

        var missing = definition.Parameters
            .Where(parameter => parameter.Required &&
                                (!parameters.TryGetValue(parameter.Key, out var value) || string.IsNullOrWhiteSpace(value)))
            .Select(parameter => parameter.Key)
            .ToList();

        if (missing.Count > 0)
        {
            return OgResult<OgReportResult>.Failed(
                "The report needs " + string.Join(", ", missing) + " before it can run.",
                OgReportRules.CodeMissingParameter);
        }

        var source = _source();

        if (source is null)
        {
            return OgResult<OgReportResult>.Failed(
                "No connection is open, so the report cannot be read.", OgReportRules.CodeNoConnection);
        }

        try
        {
            var warnings = new List<string>();

            var table = await ReadAsync(definition, parameters, source, warnings, cancellationToken).ConfigureAwait(false);

            var (template, usedFallback) = ChooseTemplate(templateName, warnings);

            var generated = DateTime.UtcNow;
            var shownParameters = definition.Parameters
                .Where(parameter => parameters.TryGetValue(parameter.Key, out var value) && !string.IsNullOrWhiteSpace(value))
                .ToDictionary(parameter => parameter.Key, parameter => parameters[parameter.Key].Trim(),
                    StringComparer.OrdinalIgnoreCase);

            var parametersText = shownParameters.Count == 0
                ? "none"
                : string.Join(", ", shownParameters.Select(pair => pair.Key + "=" + pair.Value));

            var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Name"] = definition.Name,
                ["Key"] = definition.Key,
                ["Area"] = definition.Area,
                ["Generated"] = generated.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                ["Parameters"] = parametersText,
                ["Count"] = table.Count.ToString(CultureInfo.InvariantCulture),
                ["Table"] = table.Count == 0
                    ? OgReportRules.NoData(parametersText)
                    : string.Join(Environment.NewLine, table.Lines()),
            };

            foreach (var (parameter, value) in shownParameters)
            {
                tokens[parameter] = value;
            }

            var body = usedFallback ? OgReportRules.FallbackLayout() : template!.Body;

            var document = OgReportRules.Render(body, tokens, out var unknown);

            if (document is null)
            {
                // AC-2: the token is named, and nothing is rendered.
                return OgResult<OgReportResult>.Failed(
                    "The template '" + (usedFallback ? "generated layout" : template!.Name) + "' uses " +
                    unknown.Count.ToString(CultureInfo.InvariantCulture) + " token(s) this report does not offer: " +
                    string.Join(", ", unknown.Select(token => "{{" + token + "}}")) + ". The document is not rendered.",
                    OgReportRules.CodeUnknownToken);
            }

            var pages = OgReportRules.Paginate(document);

            return OgResult<OgReportResult>.Ok(new OgReportResult(
                definition.Key,
                definition.Name,
                definition.Area,
                shownParameters,
                table,
                pages,
                tokens.Keys.ToList(),
                usedFallback ? "(generated layout)" : template!.Name,
                usedFallback,
                usedFallback ? 0 : template!.Version,
                table.Count == 0,
                warnings,
                generated));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "running the report " + definition.Key, "the report is not shown", FailureSeverity.Error);

            return OgResult<OgReportResult>.Failed(
                "The report could not be read (reference " + reference + ").",
                reference ?? OgReportRules.CodeReadFailed);
        }
    }

    public Task<OgResult<OgReportTemplate>> SaveTemplateAsync(
        string name, string body, bool makeDefault, string actor, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(actor))
        {
            return Task.FromResult(OgResult<OgReportTemplate>.Failed(
                "Saving a template records who did it.", OgReportRules.CodeActorRequired));
        }

        if (!OgReportRules.ValidName(name))
        {
            return Task.FromResult(OgResult<OgReportTemplate>.Failed(
                "'" + name + "' is not usable as a template name: it carries a character a file name cannot.",
                OgReportRules.CodeTemplateName));
        }

        if (string.IsNullOrWhiteSpace(body))
        {
            return Task.FromResult(OgResult<OgReportTemplate>.Failed(
                "A template needs a body; none was given.", OgReportRules.CodeTemplateInvalid));
        }

        var clean = name.Trim();

        try
        {
            var folder = _templatesFolder();

            Directory.CreateDirectory(folder);

            var file = Path.Combine(folder, clean + ".template.json");
            var existing = File.Exists(file) ? Read(file, out _) : null;

            // AC-6: a save is a new version of the same template, not a replacement of an unnamed one.
            var saved = new OgReportTemplate(clean, body, makeDefault, (existing?.Version ?? 0) + 1, DateTime.UtcNow);

            // Only one default: choosing one clears the flag on the rest, which is what "the default template is explicit" means.
            if (makeDefault)
            {
                foreach (var other in Directory.GetFiles(folder, "*.template.json"))
                {
                    if (string.Equals(other, file, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var template = Read(other, out _);

                    if (template is { IsDefault: true })
                    {
                        Write(other, template with { IsDefault = false });
                    }
                }
            }

            Write(file, saved);

            return Task.FromResult(OgResult<OgReportTemplate>.Ok(saved));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "saving the report template " + clean, "the template is not saved", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgReportTemplate>.Failed(
                "The template could not be saved (reference " + reference + ").",
                reference ?? OgReportRules.CodeWriteFailed));
        }
    }

    public Task<OgResult<OgReportExport>> ExportAsync(
        OgReportResult report, string format, string path, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(report);

        if (string.IsNullOrWhiteSpace(path))
        {
            return Task.FromResult(OgResult<OgReportExport>.Failed(
                "An export writes a file; no path was given.", OgReportRules.CodeExportPath));
        }

        var chosen = (format ?? string.Empty).Trim().ToLowerInvariant();

        if (!OgReportRules.Formats.Contains(chosen, StringComparer.OrdinalIgnoreCase))
        {
            return Task.FromResult(OgResult<OgReportExport>.Failed(
                "'" + format + "' is not an export format; use " + string.Join(", ", OgReportRules.Formats) + ".",
                OgReportRules.CodeExportFormat));
        }

        try
        {
            var folder = Path.GetDirectoryName(Path.GetFullPath(path));

            if (!string.IsNullOrWhiteSpace(folder))
            {
                Directory.CreateDirectory(folder);
            }

            switch (chosen)
            {
                case "pdf":
                {
                    if (_pdf is null)
                    {
                        return Task.FromResult(OgResult<OgReportExport>.Failed(
                            "A PDF needs the drawing layer, which is not available here.", OgReportRules.CodeNoPdf));
                    }

                    // AC-5: the PDF is the pages the preview shows, one PDF page each.
                    var bytes = _pdf.Render(report.Pages.Select(page => page.Lines).ToList(), report.Title);

                    File.WriteAllBytes(path, bytes);

                    break;
                }

                case "html":
                {
                    var html = new StringBuilder()
                        .AppendLine("<!doctype html>")
                        .AppendLine("<html><head><meta charset=\"utf-8\"><title>" + Escape(report.Title) + "</title>")
                        .AppendLine("<style>body{font:12px/1.45 Consolas,monospace;margin:24px}pre{white-space:pre}</style>")
                        .AppendLine("</head><body>");

                    foreach (var page in report.Pages)
                    {
                        html.Append("<pre>").Append(Escape(page.Text)).AppendLine("</pre><hr/>");
                    }

                    html.AppendLine("</body></html>");

                    File.WriteAllText(path, html.ToString(), new UTF8Encoding(false));

                    break;
                }

                case "csv":
                {
                    var csv = new StringBuilder(string.Join(",",
                        report.Table.Columns.Select(column => Quote(column.Header)))).AppendLine();

                    foreach (var row in report.Table.Rows)
                    {
                        csv.AppendLine(string.Join(",", row.Select(Quote)));
                    }

                    File.WriteAllText(path, csv.ToString(), new UTF8Encoding(false));

                    break;
                }

                default:
                {
                    // The XLSX is the owner's writer, so the workbook is the same table the preview shows.
                    var header = report.Table.Columns.Select(column => column.Header).ToList();
                    var rows = report.Table.Rows
                        .Select(row => (IReadOnlyList<object?>)row.Cast<object?>().ToList())
                        .ToList();

                    using var file = File.Create(path);

                    XlsxWriter.Write(file, report.Title, header, rows);

                    break;
                }
            }

            var size = new FileInfo(path).Length;

            return Task.FromResult(OgResult<OgReportExport>.Ok(new OgReportExport(
                chosen, path, size, chosen.ToUpperInvariant() + " written: " + size.ToString(CultureInfo.InvariantCulture) + " bytes")));
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not OperationCanceledException)
        {
            var reference = _failures.ReportHandled(
                ex, "exporting the report to " + chosen, "the report is not written", FailureSeverity.Error);

            return Task.FromResult(OgResult<OgReportExport>.Failed(
                "The report could not be written to '" + path + "' (reference " + reference + ").",
                reference ?? OgReportRules.CodeWriteFailed));
        }
    }

    // ------------------------------------------------------------------ the sources

    private async Task<OgReportTable> ReadAsync(
        OgReportDefinition definition,
        IReadOnlyDictionary<string, string> parameters,
        IDataSource source,
        List<string> warnings,
        CancellationToken cancellationToken)
    {
        string Value(string key) => parameters.TryGetValue(key, out var value) ? value?.Trim() ?? string.Empty : string.Empty;

        switch (definition.Key)
        {
            case WellsRegister:
            {
                var field = Value("field");
                var wells = await OgUow.ReadAllAsync<WELL>(source).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var rows = wells
                    .Where(well => !string.Equals(well.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .Where(well => field.Length == 0 ||
                                   string.Equals(well.ASSIGNED_FIELD?.Trim(), field, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(well => well.UWI, StringComparer.Ordinal)
                    .Select(well => (IReadOnlyList<string>)
                    [
                        (well.UWI ?? string.Empty).Trim(),
                        well.WELL_NAME?.Trim() ?? string.Empty,
                        well.ASSIGNED_FIELD?.Trim() ?? string.Empty,
                        well.CURRENT_STATUS?.Trim() ?? string.Empty,
                        OgReportRules.Figure((double)well.DEEPEST_DEPTH),
                    ])
                    .ToList();

                return new OgReportTable(definition.Key, definition.Name,
                    [
                        new OgReportColumn("UWI", "uwi", false),
                        new OgReportColumn("Name", "name", false),
                        new OgReportColumn("Field", "field", false),
                        new OgReportColumn("Status", "status", false),
                        new OgReportColumn("Depth", "depth", true),
                    ],
                    rows);
            }

            case Wellbores:
            {
                var uwi = Value("well");
                var bores = await OgUow.ReadAllAsync<WELLBORE>(source).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var rows = bores
                    .Where(bore => string.Equals(bore.UWI?.Trim(), uwi, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(bore => bore.WELLBORE_ID, StringComparer.Ordinal)
                    .Select(bore => (IReadOnlyList<string>)
                    [
                        bore.WELLBORE_ID?.Trim() ?? string.Empty,
                        bore.WELLBORE_NAME?.Trim() ?? string.Empty,
                        bore.KIND?.Trim() ?? string.Empty,
                        bore.STATUS?.Trim() ?? string.Empty,
                        bore.TOTAL_DEPTH is { } depth ? OgReportRules.Figure((double)depth) : string.Empty,
                    ])
                    .ToList();

                return new OgReportTable(definition.Key, definition.Name,
                    [
                        new OgReportColumn("Bore", "bore", false),
                        new OgReportColumn("Name", "name", false),
                        new OgReportColumn("Kind", "kind", false),
                        new OgReportColumn("Status", "status", false),
                        new OgReportColumn("Depth", "depth", true),
                    ],
                    rows);
            }

            case FormationPicks:
            {
                var uwi = Value("well");
                var picks = await OgUow.ReadAllAsync<STRAT_WELL_SECTION>(source).ConfigureAwait(false);
                var units = await OgUow.ReadAllAsync<STRAT_UNIT>(source).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var names = units
                    .GroupBy(unit => (unit.STRAT_UNIT_ID ?? string.Empty).Trim(), StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(
                        group => group.Key,
                        group => string.IsNullOrWhiteSpace(group.First().LONG_NAME) ? group.Key : group.First().LONG_NAME!.Trim(),
                        StringComparer.OrdinalIgnoreCase);

                var rows = picks
                    .Where(pick => string.Equals(pick.UWI?.Trim(), uwi, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(pick => pick.PICK_DEPTH)
                    .Select(pick => (IReadOnlyList<string>)
                    [
                        names.TryGetValue((pick.STRAT_UNIT_ID ?? string.Empty).Trim(), out var name)
                            ? name
                            : (pick.STRAT_UNIT_ID ?? string.Empty).Trim(),
                        OgReportRules.Figure((double)pick.PICK_DEPTH),
                        pick.PICK_DEPTH_OUOM?.Trim() ?? string.Empty,
                        pick.INTERPRETER?.Trim() ?? string.Empty,
                    ])
                    .ToList();

                return new OgReportTable(definition.Key, definition.Name,
                    [
                        new OgReportColumn("Formation", "formation", false),
                        new OgReportColumn("Depth", "depth", true),
                        new OgReportColumn("Unit", "unit", false),
                        new OgReportColumn("Interpreter", "interpreter", false),
                    ],
                    rows);
            }

            case SurveyStations:
            {
                var uwi = Value("well");
                var stations = await OgUow.ReadAllAsync<WELL_DIR_SRVY_STATION>(source).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var rows = stations
                    .Where(station => string.Equals(station.UWI?.Trim(), uwi, StringComparison.OrdinalIgnoreCase))
                    .Where(station => !string.Equals(station.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .OrderBy(station => station.STATION_MD)
                    .Select(station => (IReadOnlyList<string>)
                    [
                        OgReportRules.Figure((double)station.STATION_MD),
                        OgReportRules.Figure((double)station.INCLINATION),
                        OgReportRules.Figure((double)station.AZIMUTH),
                        OgReportRules.Figure((double)station.STATION_TVD),
                    ])
                    .ToList();

                return new OgReportTable(definition.Key, definition.Name,
                    [
                        new OgReportColumn("MD", "md", true),
                        new OgReportColumn("Inclination", "inc", true),
                        new OgReportColumn("Azimuth", "azimuth", true),
                        new OgReportColumn("TVD", "tvd", true),
                    ],
                    rows);
            }

            default:
            {
                // Daily drilling reports: a period, so a date that cannot be read is refused rather than silently ignored.
                var uwi = Value("well");
                var from = Date(Value("from"), "from", warnings);
                var to = Date(Value("to"), "to", warnings);

                var reports = await OgUow.ReadAllAsync<DEVELOPMENT_DAILY_REPORT>(source).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var rows = reports
                    .Where(report => string.Equals(report.WELL_ID?.Trim(), uwi, StringComparison.OrdinalIgnoreCase))
                    .Where(report => !string.Equals(report.ACTIVE_IND, "N", StringComparison.OrdinalIgnoreCase))
                    .Where(report => from is null || report.REPORT_DATE is { } date && date.Date >= from.Value.Date)
                    .Where(report => to is null || report.REPORT_DATE is { } date && date.Date <= to.Value.Date)
                    .OrderBy(report => report.REPORT_DATE)
                    .Select(report => (IReadOnlyList<string>)
                    [
                        report.REPORT_DATE?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty,
                        report.STATE?.Trim() ?? string.Empty,
                        OgReportRules.Figure((double)report.DEPTH_START),
                        OgReportRules.Figure((double)report.DEPTH_END),
                        report.COST is { } cost ? cost.ToString("0.##", CultureInfo.InvariantCulture) : string.Empty,
                        report.CURRENCY?.Trim() ?? string.Empty,
                    ])
                    .ToList();

                return new OgReportTable(definition.Key, definition.Name,
                    [
                        new OgReportColumn("Date", "date", false),
                        new OgReportColumn("State", "state", false),
                        new OgReportColumn("From", "from", true),
                        new OgReportColumn("To", "to", true),
                        new OgReportColumn("Cost", "cost", true),
                        new OgReportColumn("Currency", "currency", false),
                    ],
                    rows);
            }
        }
    }

    /// <summary>A date parameter: read when present, and reported in the warnings when it cannot be - never silently dropped.</summary>
    private static DateTime? Date(string text, string key, List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var value))
        {
            return value;
        }

        warnings.Add("the '" + key + "' parameter '" + text + "' is not a date, so it was not applied");

        return null;
    }

    /// <summary>The template to use: the one named, else the default, else the labelled generated layout.</summary>
    private (OgReportTemplate? Template, bool UsedFallback) ChooseTemplate(string? name, List<string> warnings)
    {
        var folder = _templatesFolder();

        if (!Directory.Exists(folder))
        {
            return (null, true);
        }

        var all = Directory.GetFiles(folder, "*.template.json")
            .Select(file => (Path: file, Template: Read(file, out _)))
            .Where(pair => pair.Template is not null)
            .Select(pair => pair.Template!)
            .ToList();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var chosen = all.FirstOrDefault(template =>
                string.Equals(template.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (chosen is not null)
            {
                return (chosen, false);
            }

            warnings.Add("the template '" + name.Trim() + "' is not in the templates folder, so the default was used");
        }

        var fallback = all.FirstOrDefault(template => template.IsDefault);

        if (fallback is not null)
        {
            return (fallback, false);
        }

        return (null, true);
    }

    private OgReportTemplate? Read(string file, out string? problem)
    {
        problem = null;

        try
        {
            var stored = JsonSerializer.Deserialize<Stored>(File.ReadAllText(file), Stored.Json);

            if (stored is null || string.IsNullOrWhiteSpace(stored.Body))
            {
                problem = "'" + file + "' holds no template body";
                return null;
            }

            return new OgReportTemplate(
                stored.Name ?? Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(file)),
                stored.Body,
                stored.IsDefault,
                stored.Version,
                stored.UpdatedAt == default ? File.GetLastWriteTimeUtc(file) : stored.UpdatedAt);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or IOException)
        {
            // A template that cannot be read is left out of the list and reported, so the page can say a template is broken rather than
            // showing a shorter list that looks complete.
            _failures.ReportHandled(
                ex, "reading the report template " + file, "the template is left out", FailureSeverity.Degraded);

            problem = "'" + file + "' is not a readable template";
            return null;
        }
    }

    private static void Write(string file, OgReportTemplate template)
    {
        var stored = new Stored
        {
            Name = template.Name,
            Body = template.Body,
            IsDefault = template.IsDefault,
            Version = template.Version,
            UpdatedAt = template.UpdatedAt,
        };

        // Written to a temporary file and moved into place, so a failed write cannot leave half a template behind.
        var temporary = file + ".tmp";

        File.WriteAllText(temporary, JsonSerializer.Serialize(stored, Stored.Json), new UTF8Encoding(false));
        File.Move(temporary, file, overwrite: true);
    }

    private static string Quote(string value) => "\"" + value.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    private static string Escape(string value) =>
        value.Replace("&", "&amp;", StringComparison.Ordinal)
            .Replace("<", "&lt;", StringComparison.Ordinal)
            .Replace(">", "&gt;", StringComparison.Ordinal);

    /// <summary>The template as it is stored - a shape of its own, so the file format is not the public record.</summary>
    private sealed class Stored
    {
        public static readonly JsonSerializerOptions Json = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
        };

        [JsonPropertyName("name")] public string? Name { get; set; }

        [JsonPropertyName("body")] public string? Body { get; set; }

        [JsonPropertyName("isDefault")] public bool IsDefault { get; set; }

        [JsonPropertyName("version")] public int Version { get; set; }

        [JsonPropertyName("updatedAt")] public DateTime UpdatedAt { get; set; }
    }
}
