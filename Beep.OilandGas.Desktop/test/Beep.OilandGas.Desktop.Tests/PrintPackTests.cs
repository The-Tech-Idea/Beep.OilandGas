using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Reporting;
using TheTechIdea.OilGas.Wellbore;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// RPT-02: the pack's rules - deterministic unique file names (AC-4), the paper/scale/header/legend options on the owners' own type
/// (AC-5), the page count read out of a PDF's own page objects (AC-6), and the two permissions a run needs. Running a pack is the
/// gateway test, because it reads wells.
/// </summary>
public sealed class PrintPackTests
{
    private const string PageFileName = "PrintPacksPageView.cs";

    [Fact]
    public void A_file_name_is_unique_by_index_and_deterministic_by_well()
    {
        // AC-4: the same scope in the same order gives the same names, and two wells that clean to the same text are still told apart.
        Assert.Equal("001-wellview-W-1.pdf", OgPackRules.FileName(0, "wellview", "W-1"));
        Assert.Equal("002-wellview-W-2.pdf", OgPackRules.FileName(1, "wellview", "W-2"));

        // The same input is the same name, which is what makes a re-run overwrite its own files rather than litter the folder.
        Assert.Equal(OgPackRules.FileName(0, "wellview", "W-1"), OgPackRules.FileName(0, "wellview", "W-1"));

        // Two wells whose identifiers differ only in characters a file name cannot carry are told apart by the index.
        Assert.NotEqual(OgPackRules.FileName(0, "wellview", "30/1-A"), OgPackRules.FileName(1, "wellview", "30/1 A"));
        Assert.Equal("001-wellview-30-1-A.pdf", OgPackRules.FileName(0, "wellview", "30/1-A"));

        // A well with no identifier at all still gets a usable name.
        Assert.Equal("001-report-well.pdf", OgPackRules.FileName(0, "report", "  "));

        // A hundred wells are a hundred names.
        var names = Enumerable.Range(0, 100).Select(index => OgPackRules.FileName(index, "wellview", "W-" + index)).ToList();

        Assert.Equal(100, names.Distinct(StringComparer.OrdinalIgnoreCase).Count());
    }

    [Fact]
    public void The_pack_options_become_the_owners_print_options()
    {
        // AC-5: paper, scale, header block and legend are the owners' own settings, so the document is printed by their exporter.
        var options = OgPackRules.PrintOptions(new OgPackOptions("A3", 500f, IncludeHeaderBlock: true, IncludeLegend: false, "Field pack"));

        Assert.Equal(PaperSize.A3, options.Paper);
        Assert.Equal(500f, options.ScaleRatio);
        Assert.True(options.IncludeHeaderBlock);
        Assert.False(options.IncludeLegend);
        Assert.Equal("Field pack", options.Title);

        // A scale of 0 is the owner's own "fit the whole well on one page".
        Assert.Equal(0f, OgPackRules.PrintOptions(new OgPackOptions("A4", 0f, true, true, null)).ScaleRatio);

        // A paper name the owner does not define leaves the environment's default rather than throwing or inventing a size.
        var unknown = OgPackRules.PrintOptions(new OgPackOptions("Parchment", 0f, true, true, null));

        Assert.Equal(new WellborePrintOptions().Paper.Width, unknown.Paper.Width, 3);

        // Every paper the page offers is one the owner actually defines - PaperSize is a readonly struct with four static sizes, not an
        // enum, so the list must be exactly those names or the choice fails when it is used.
        Assert.Equal(["A4", "A3", "Letter", "Tabloid"], OgPackRules.Papers);

        foreach (var paper in OgPackRules.Papers)
        {
            var print = OgPackRules.PrintOptions(new OgPackOptions(paper, 0f, true, true, null));

            Assert.Equal(paper, print.Paper.Name);
        }
    }

    [Fact]
    public void The_page_count_is_read_out_of_the_pdf_itself()
    {
        // AC-6: the manifest's pages come from the file, so they cannot disagree with it. A one-page document has one page object plus
        // the page-tree node, whose marker contains the page's own.
        const string onePage = "1 0 obj << /Type /Pages /Kids [2 0 R] >> 2 0 obj << /Type /Page >>";
        const string threePages = "1 0 obj << /Type /Pages /Kids [] >> 2 0 obj << /Type /Page >> 3 0 obj << /Type /Page >> 4 0 obj << /Type /Page >>";

        Assert.Equal(1, OgPackRules.PageCount(System.Text.Encoding.Latin1.GetBytes(onePage)));
        Assert.Equal(3, OgPackRules.PageCount(System.Text.Encoding.Latin1.GetBytes(threePages)));

        // Something that is not a PDF reports zero rather than a guess.
        Assert.Equal(0, OgPackRules.PageCount(System.Text.Encoding.Latin1.GetBytes("not a pdf")));
        Assert.Equal(0, OgPackRules.PageCount([]));
    }

    [Fact]
    public void A_run_needs_both_permissions_and_the_scope_is_stated()
    {
        // The feature's command table: reporting.pack.run and data.export.
        var both = new OgPermissions(["Viewer"], ["reporting.pack.run", "data.export"], true);

        Assert.True(OgPackRules.Permitted(both));
        Assert.True(OgPackRules.Permitted(new OgPermissions(["SystemAdmin"], [], true)));
        Assert.False(OgPackRules.Permitted(new OgPermissions(["Viewer"], ["reporting.pack.run"], true)));
        Assert.False(OgPackRules.Permitted(new OgPermissions(["Viewer"], ["data.export"], true)));
        Assert.False(OgPackRules.Permitted(new OgPermissions([], [], permissionSourceAvailable: false)));

        // The scope says which wells, in words a person can check against what they chose.
        Assert.Equal("every well", new OgPackScope(OgPackScope.All, null, []).Describe());
        Assert.Equal("the wells of field 'North'", new OgPackScope(OgPackScope.FieldScope, "North", []).Describe());
        Assert.Equal("2 selected well(s)", new OgPackScope(OgPackScope.Selected, null, ["W-1", "W-2"]).Describe());
    }

    [Fact]
    public void A_pack_is_described_by_what_it_will_print()
    {
        var pack = new OgPrintPack("wellview",
            new OgPackScope(OgPackScope.FieldScope, "North", []),
            new OgPackOptions("A4", 500f, true, true, "North field"));

        var description = pack.Describe();

        Assert.Contains("wellview", description, StringComparison.Ordinal);
        Assert.Contains("field 'North'", description, StringComparison.Ordinal);
        Assert.Contains("1:500", description, StringComparison.Ordinal);
        Assert.Contains("header on", description, StringComparison.Ordinal);
        Assert.Contains("legend on", description, StringComparison.Ordinal);
        Assert.Contains("North field", description, StringComparison.Ordinal);

        // A fitted pack says so, because "1:0" would be nonsense.
        Assert.Contains("fitted to one page",
            new OgPackOptions("A4", 0f, true, true, null).Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_manifest_carries_the_status_of_every_well_and_exports_as_csv()
    {
        // AC-1, AC-2 and AC-6 in one record: a written item, a skipped one with its reason, and the totals.
        var manifest = new OgPackManifest(
            "C:/out", "wellview", "every well", "A4 · fitted",
            [
                new OgPackItem("W-1", "001-wellview-W-1.pdf", 3, 4000, OgPackRules.Written, null, []),
                new OgPackItem("W-2", null, 0, 0, OgPackRules.Skipped, "no survey (2+ stations)", []),
                new OgPackItem("W-3", "003-wellview-W-3.pdf", 2, 3000, OgPackRules.Written, null, ["a conversion warning"]),
            ],
            5, 7000, false, false, "C:/out/manifest.json", "C:/out/manifest.csv", DateTime.UtcNow);

        Assert.Equal(2, manifest.Written);
        Assert.Equal(1, manifest.Skipped);
        Assert.Equal(0, manifest.Failed);
        Assert.Equal(5, manifest.Pages);
        Assert.Contains("2 document(s)", manifest.SummaryText, StringComparison.Ordinal);
        Assert.Contains("1 skipped", manifest.SummaryText, StringComparison.Ordinal);
        Assert.Contains("5 page(s)", manifest.SummaryText, StringComparison.Ordinal);

        var csv = manifest.ItemsCsv().Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        Assert.Equal("well,file,pages,bytes,status,reason,warnings", csv[0]);
        Assert.Equal(4, csv.Length);
        Assert.Contains("\"skipped\"", csv[2], StringComparison.Ordinal);
        Assert.Contains("\"no survey (2+ stations)\"", csv[2], StringComparison.Ordinal);
        Assert.Contains("\"a conversion warning\"", csv[3], StringComparison.Ordinal);
    }

    [Fact]
    public async Task A_pack_is_refused_without_permissions_a_scope_or_a_connection()
    {
        var service = new OgPrintPacksService(() => null, new Quiet(), new NoReports());
        var pack = new OgPrintPack("wellview", new OgPackScope(OgPackScope.All, null, []),
            new OgPackOptions("A4", 0f, true, true, null));

        var refused = await service.EstimateAsync(pack, new OgPermissions(["Viewer"], [], true));

        Assert.False(refused.Success);
        Assert.Equal(OgPackRules.CodeNotPermitted, refused.Reference);
        Assert.Contains("reporting.pack.run and data.export", refused.Message!, StringComparison.Ordinal);

        var allowed = new OgPermissions(["Viewer"], ["reporting.pack.run", "data.export"], true);

        var noConnection = await service.EstimateAsync(pack, allowed);

        Assert.False(noConnection.Success);
        Assert.Equal(OgPackRules.CodeNoConnection, noConnection.Reference);

        var badType = await service.EstimateAsync(pack with { ContentType = "hologram" }, allowed);

        Assert.Equal(OgPackRules.CodeContentType, badType.Reference);
        Assert.Contains("use wellview, report", badType.Message!, StringComparison.Ordinal);

        // A run needs a folder before anything else is read.
        var noFolder = await service.RunAsync(pack, " ", allowed, false, null);

        Assert.False(noFolder.Success);
        Assert.Equal(OgPackRules.CodeNoFolder, noFolder.Reference);
    }

    [Fact]
    public void The_page_states_the_preview_the_progress_the_cancel_and_the_manifest()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        // The preview is separate from the run: nothing is written by an estimate.
        Assert.Contains("Nothing was written: this is the preview", page, StringComparison.Ordinal);
        Assert.Contains("estimated.Value.NamesText", page, StringComparison.Ordinal);
        Assert.Contains("estimated.Value.SkippedText", page, StringComparison.Ordinal);

        // Progress, cancellation and the manifest's own paths are on the page.
        Assert.Contains("report.Describe()", page, StringComparison.Ordinal);
        Assert.Contains("_running.Cancel()", page, StringComparison.Ordinal);
        Assert.Contains("ran.Value.ManifestPath", page, StringComparison.Ordinal);
        Assert.Contains("ran.Value.Cancelled", page, StringComparison.Ordinal);

        // A refused run clears the manifest grid rather than leaving the last pack's items standing.
        Assert.Contains("_itemGrid.ItemsSource = null", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ fixtures

    private sealed class NoReports : IOgReports
    {
        public Task<OgResult<IReadOnlyList<OgReportDefinition>>> CatalogueAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<OgReportDefinition>>.Ok(OgReportsService.Definitions));

        public Task<OgResult<IReadOnlyList<OgReportTemplate>>> TemplatesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<IReadOnlyList<OgReportTemplate>>.Ok([]));

        public Task<OgResult<OgReportResult>> RunAsync(string key, IReadOnlyDictionary<string, string> parameters,
            OgPermissions permissions, string? templateName = null, CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgReportResult>.Failed("not used", "not used"));

        public Task<OgResult<OgReportTemplate>> SaveTemplateAsync(string name, string body, bool makeDefault, string actor,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgReportTemplate>.Failed("not used", "not used"));

        public Task<OgResult<OgReportExport>> ExportAsync(OgReportResult report, string format, string path,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(OgResult<OgReportExport>.Failed("not used", "not used"));
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
