using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;
using TheTechIdea.Beep.OilandGas.Desktop.Views;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RPT-01: the rules the catalogue states - token rendering and the unknown token (AC-2), pagination (AC-1), the empty-result sentence
/// (AC-3), the area's read permission (AC-4), template versioning and the explicit default (AC-6) - and the exports, all without a
/// database. Running a report over real rows is the gateway test.
/// </summary>
public sealed class ReportsTests
{
    private const string PageFileName = "ReportsPageView.cs";

    private static OgPermissions Grant(params string[] permissions) =>
        new(["Viewer"], permissions, permissionSourceAvailable: true);

    [Fact]
    public void A_document_is_rendered_from_its_tokens_and_an_unknown_one_is_named()
    {
        // AC-2: every offered token is replaced, and a token the report does not offer fails the render with its name.
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = "Wells register",
            ["Area"] = "exploration",
            ["Count"] = "2",
            ["Table"] = "UWI  Name",
        };

        var rendered = OgReportRules.Render("{{Name}} ({{Area}})\n{{Table}}\n{{Count}} row(s)", tokens, out var unknown);

        Assert.Empty(unknown);
        Assert.Equal("Wells register (exploration)\nUWI  Name\n2 row(s)", rendered);

        var failed = OgReportRules.Render("{{Name}} {{Nonsense}}", tokens, out var missing);

        Assert.Null(failed);
        Assert.Equal(["Nonsense"], missing);

        // Two unknown tokens are both named, and an unclosed marker is one too rather than a silent truncation.
        OgReportRules.Render("{{A}} {{B}}", tokens, out var two);

        Assert.Equal(["A", "B"], two);

        OgReportRules.Render("{{Name", tokens, out var unclosed);

        Assert.Equal(["Name"], unclosed);

        // A token with surrounding spaces is the same token, which is what makes a hand-written template forgiving about spacing.
        Assert.Equal("Wells register", OgReportRules.Render("{{ Name }}", tokens, out _));
    }

    [Fact]
    public void A_document_is_paginated_once_and_the_pages_hold_what_was_rendered()
    {
        // AC-1: the page count and the text of each page come from one rendering, so the preview and the export cannot disagree.
        var document = string.Join(Environment.NewLine,
            Enumerable.Range(1, 100).Select(number => "line " + number.ToString(System.Globalization.CultureInfo.InvariantCulture)));

        var pages = OgReportRules.Paginate(document, linesPerPage: 48);

        Assert.Equal(3, pages.Count);
        Assert.Equal([1, 2, 3], pages.Select(page => page.Number));
        Assert.Equal(48, pages[0].Lines.Count);
        Assert.Equal(48, pages[1].Lines.Count);
        Assert.Equal(4, pages[2].Lines.Count);
        Assert.StartsWith("line 1", pages[0].Lines[0], StringComparison.Ordinal);
        Assert.Equal("line 100", pages[2].Lines[^1]);

        // The joined pages are the source, line for line - nothing is dropped at a boundary.
        Assert.Equal(100, pages.Sum(page => page.Lines.Count));
        Assert.Equal(document.Replace("\r\n", "\n", StringComparison.Ordinal),
            string.Join("\n", pages.SelectMany(page => page.Lines)));

        // An empty document still has one page, so a preview is never blank and pageless.
        Assert.Single(OgReportRules.Paginate(string.Empty));
        Assert.Single(OgReportRules.Paginate(null));
    }

    [Fact]
    public void An_empty_result_states_that_there_is_no_data_for_the_parameters()
    {
        // AC-3: the sentence names the parameters, so a reader knows what was asked rather than wondering whether the report ran.
        var sentence = OgReportRules.NoData("well=W-404");

        Assert.Contains("No data for these parameters", sentence, StringComparison.Ordinal);
        Assert.Contains("well=W-404", sentence, StringComparison.Ordinal);
        Assert.EndsWith(".", sentence, StringComparison.Ordinal);

        Assert.Equal("No data for these parameters.", OgReportRules.NoData(string.Empty));

        // The generated layout puts the table where the template says, so an empty table prints the sentence in place of the rows.
        var layout = OgReportRules.FallbackLayout();

        Assert.Contains("{{" + OgReportRules.TableToken + "}}", layout, StringComparison.Ordinal);

        var rendered = OgReportRules.Render(layout, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Name"] = "Wells register",
            ["Area"] = "exploration",
            ["Generated"] = "2026-11-09 08:00:00",
            ["Parameters"] = "well=W-404",
            ["Count"] = "0",
            [OgReportRules.TableToken] = sentence,
        }, out var unknown);

        Assert.Empty(unknown);
        Assert.Contains("No data for these parameters: well=W-404.", rendered!, StringComparison.Ordinal);
    }

    [Fact]
    public void The_areas_read_permission_is_the_gate_and_an_unreadable_permission_source_refuses()
    {
        // AC-4: an administrator may always run a report; otherwise the area's permission is required.
        Assert.True(OgReportRules.Allowed(new OgPermissions(["SystemAdmin"], [], true), "wells.read"));
        Assert.True(OgReportRules.Allowed(Grant("wells.read"), "wells.read"));
        Assert.True(OgReportRules.Allowed(Grant("WELLS.READ"), "wells.read"));
        Assert.False(OgReportRules.Allowed(Grant("drilling.read"), "wells.read"));
        Assert.False(OgReportRules.Allowed(Grant(), "wells.read"));

        // A permission source that could not be read is a refusal, not an empty grant.
        Assert.False(OgReportRules.Allowed(new OgPermissions([], [], permissionSourceAvailable: false), "wells.read"));

        // Every report in the catalogue names a permission, and each area's is the one it draws on.
        Assert.All(OgReportsService.Definitions, report => Assert.False(string.IsNullOrWhiteSpace(report.Permission)));
        Assert.Equal("wells.read",
            OgReportsService.Definitions.Single(report => report.Key == OgReportsService.WellsRegister).Permission);
        Assert.Equal("drilling.read",
            OgReportsService.Definitions.Single(report => report.Key == OgReportsService.DailyReports).Permission);
    }

    [Fact]
    public async Task A_report_is_refused_without_its_parameter_or_its_permission()
    {
        var service = new OgReportsService(() => null, new Quiet(), () => Path.Combine(Path.GetTempPath(), "rpt01-none"));

        // The catalogue is the definition list itself, so the page and the tests read one source.
        var catalogue = await service.CatalogueAsync();

        Assert.True(catalogue.Success);
        Assert.Equal(5, catalogue.Value!.Count);
        Assert.Equal(3, catalogue.Value.Select(report => report.Area).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        var noPermission = await service.RunAsync(OgReportsService.WellsRegister,
            new Dictionary<string, string>(), Grant("drilling.read"));

        Assert.False(noPermission.Success);
        Assert.Equal(OgReportRules.CodePermission, noPermission.Reference);
        Assert.Contains("wells.read", noPermission.Message!, StringComparison.Ordinal);

        // With the permission but no parameter, the missing key is named and nothing is read.
        var noParameter = await service.RunAsync(OgReportsService.Wellbores,
            new Dictionary<string, string>(), Grant("drilling.read"));

        Assert.False(noParameter.Success);
        Assert.Equal(OgReportRules.CodeMissingParameter, noParameter.Reference);
        Assert.Contains("well", noParameter.Message!, StringComparison.Ordinal);

        var unknown = await service.RunAsync("no-such-report", new Dictionary<string, string>(), Grant("wells.read"));

        Assert.False(unknown.Success);
        Assert.Equal(OgReportRules.CodeUnknownReport, unknown.Reference);
        Assert.Contains("wells-register", unknown.Message!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_template_is_a_new_version_each_save_and_only_one_is_the_default()
    {
        // AC-6: the version rises, and choosing a default clears the flag on the others rather than leaving two.
        using var scope = new Scratch();

        var service = new OgReportsService(() => null, new Quiet(), () => scope.Folder);

        var first = await service.SaveTemplateAsync("house", "{{Name}}\n{{Table}}", makeDefault: false, "sara");

        Assert.True(first.Success, first.Message);
        Assert.Equal(1, first.Value!.Version);
        Assert.False(first.Value.IsDefault);

        var second = await service.SaveTemplateAsync("house", "{{Name}}\n{{Table}}\n{{Count}}", makeDefault: true, "sara");

        Assert.True(second.Success, second.Message);
        Assert.Equal(2, second.Value!.Version);
        Assert.True(second.Value.IsDefault);

        // A second template made the default takes the flag off the first.
        var other = await service.SaveTemplateAsync("client", "{{Name}}", makeDefault: true, "sara");

        Assert.True(other.Success, other.Message);

        var listed = await service.TemplatesAsync();

        Assert.True(listed.Success);
        Assert.Equal(2, listed.Value!.Count);
        Assert.Single(listed.Value.Where(template => template.IsDefault));
        Assert.Equal("client", listed.Value.Single(template => template.IsDefault).Name);

        // A third save of the first template is version 3, not a reset.
        var third = await service.SaveTemplateAsync("house", "{{Name}}", makeDefault: false, "sara");

        Assert.Equal(3, third.Value!.Version);

        // A name a file cannot carry, an empty body and no actor are each refused.
        Assert.Equal(OgReportRules.CodeTemplateName,
            (await service.SaveTemplateAsync("a/b", "x", false, "sara")).Reference);
        Assert.Equal(OgReportRules.CodeTemplateInvalid,
            (await service.SaveTemplateAsync("blank", " ", false, "sara")).Reference);
        Assert.Equal(OgReportRules.CodeActorRequired,
            (await service.SaveTemplateAsync("house", "x", false, " ")).Reference);

        // A template file that is not readable is left out of the list rather than breaking it.
        File.WriteAllText(Path.Combine(scope.Folder, "broken.template.json"), "{ not json");

        var afterBroken = await service.TemplatesAsync();

        Assert.True(afterBroken.Success);
        Assert.Equal(2, afterBroken.Value!.Count);
    }

    [Fact]
    public async Task An_export_needs_a_report_a_path_and_a_format()
    {
        using var scope = new Scratch();

        var service = new OgReportsService(() => null, new Quiet(), () => scope.Folder);

        var report = new OgReportResult(
            "wells-register", "Wells register", "exploration",
            new Dictionary<string, string> { ["field"] = "North" },
            new OgReportTable("wells-register", "Wells register",
                [new OgReportColumn("UWI", "uwi", false), new OgReportColumn("Depth", "depth", true)],
                [["W-1", "2000"], ["W-2", "1800"]]),
            OgReportRules.Paginate("Wells register\nW-1"),
            ["Name", "Table"],
            "house", false, 3, false, [], DateTime.UtcNow);

        var path = Path.Combine(scope.Root, "report.csv");

        var csv = await service.ExportAsync(report, "csv", path);

        Assert.True(csv.Success, csv.Message);
        Assert.Equal(3, File.ReadAllLines(path).Length);
        Assert.Contains("\"UWI\",\"Depth\"", File.ReadAllText(path), StringComparison.Ordinal);
        Assert.Contains("\"W-1\",\"2000\"", File.ReadAllText(path), StringComparison.Ordinal);

        // The HTML holds the preview's own text, so the document and the screen are one rendering.
        var html = Path.Combine(scope.Root, "report.html");

        Assert.True((await service.ExportAsync(report, "html", html)).Success);
        Assert.Contains("W-1", File.ReadAllText(html), StringComparison.Ordinal);
        Assert.Contains("<pre>", File.ReadAllText(html), StringComparison.Ordinal);

        // The workbook is the owner's writer, and it opens with the zip signature a workbook has.
        var xlsx = Path.Combine(scope.Root, "report.xlsx");

        Assert.True((await service.ExportAsync(report, "xlsx", xlsx)).Success);
        Assert.Equal(new byte[] { 0x50, 0x4B }, File.ReadAllBytes(xlsx)[..2]);

        // A PDF without the drawing layer is refused for what it is, not silently written as something else.
        Assert.Equal(OgReportRules.CodeNoPdf, (await service.ExportAsync(report, "pdf", Path.Combine(scope.Root, "r.pdf"))).Reference);

        Assert.Equal(OgReportRules.CodeExportPath, (await service.ExportAsync(report, "csv", " ")).Reference);
        Assert.Equal(OgReportRules.CodeExportFormat, (await service.ExportAsync(report, "docx", "x.docx")).Reference);
    }

    [Fact]
    public void The_page_states_the_page_count_and_keeps_the_parameters_after_a_refusal()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // AC-1: the preview shows which page of how many.
        Assert.Contains("of ", page, StringComparison.Ordinal);
        Assert.Contains("_result.PageCount", page, StringComparison.Ordinal);
        Assert.Contains("page.Number", page, StringComparison.Ordinal);

        // A refused run clears the preview rather than leaving the last report on screen, and the parameters stay available to fix.
        Assert.Contains("_preview.Text = string.Empty", page, StringComparison.Ordinal);
        Assert.Contains("_paramText", page, StringComparison.Ordinal);

        // AC-4 and AC-5 are the service's, and the page hands it the session's permissions and the report it rendered.
        Assert.Contains("_application.Permissions", page, StringComparison.Ordinal);
        Assert.Contains("_result, format, path", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "rpt01-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Root => _root;

        public string Folder
        {
            get
            {
                var folder = Path.Combine(_root, "templates");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_root))
                {
                    Directory.Delete(_root, recursive: true);
                }
            }
            catch (IOException)
            {
                // The OS temp folder is its own to clean.
            }
        }
    }

    private sealed class Quiet : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) => FailureReferenceFormat.New();
    }
}
