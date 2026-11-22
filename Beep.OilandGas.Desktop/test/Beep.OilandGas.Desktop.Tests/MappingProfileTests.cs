using System.IO.Compression;
using System.Text;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Wellbore.Data;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-04: a CSV folder and an EDM export load through a profile (AC-1, AC-2), a required field the profile does not map fails with
/// the field named (AC-3), a built-in is read-only (AC-4), the round-trip comparison is real (AC-5) and the workbook carries one sheet
/// per entity (AC-6) - against files, which is what this feature works on: it needs no database, so there is no gateway here.
/// </summary>
public sealed class MappingProfileTests
{
    private const string PageFileName = "MappingProfilesPageView.cs";

    /// <summary>A Landmark EDM export, the fixture the tools' own EdmProfileTests uses.</summary>
    private const string EdmExport = """
        <?xml version="1.0" encoding="utf-8"?>
        <export>
          <CD_WELL well_id="W9" well_common_name="Demo-9" water_depth="0"/>
          <CD_WELLBORE wellbore_id="B9" well_id="W9" wellbore_name="Demo-9 main" ko_md="0"/>
          <CD_HOLE_SECT well_id="W9" wellbore_id="B9" hole_sect_group_id="PLAN" hole_sect_id="H1" sect_type_code="CAS" od_casing="20" id_casing="18.73" id_drift="18.54" hole_size="26" length="500" md_shoe="500" catalog_key_desc="20 in surface"/>
          <CD_HOLE_SECT well_id="W9" wellbore_id="B9" hole_sect_group_id="PLAN" hole_sect_id="H2" sect_type_code="CAS" od_casing="9.625" id_casing="8.681" id_drift="8.525" hole_size="12.25" length="6500" md_shoe="7000" catalog_key_desc="9 5/8 in production"/>
          <CD_HOLE_SECT well_id="W9" wellbore_id="B9" hole_sect_group_id="PLAN" hole_sect_id="H3" sect_type_code="OPEN" hole_size="8.5" length="1500" md_shoe="8500"/>
          <CD_WELLBORE_FORMATION well_id="W9" wellbore_id="B9" wellbore_formation_id="F1" phase="PROTOTYPE" formation_name="Hod" prognosed_md="6800" prognosed_tvd="6700"/>
          <CD_DEFINITIVE_SURVEY_STATION well_id="W9" wellbore_id="B9" def_survey_header_id="S1" sequence_no="0" md="0" inclination="0" azimuth="0" tvd="0"/>
          <CD_DEFINITIVE_SURVEY_STATION well_id="W9" wellbore_id="B9" def_survey_header_id="S1" sequence_no="1" md="3000" inclination="12.5" azimuth="45" tvd="2976.3"/>
        </export>
        """;

    [Fact]
    public void A_csv_folder_loads_into_the_canonical_model_through_the_profile()
    {
        // AC-1: one CSV per table in a folder, mapped by the built-in "csv" profile.
        using var scope = new Scratch();

        WriteCsvFolder(scope.Folder);

        var profile = MappingProfile.LoadBuiltIn("csv");
        var model = new MappedWellDataProvider(new CsvTabularSource(scope.Folder), profile)
            .LoadWellAsync("W-1").GetAwaiter().GetResult().Model;

        Assert.Equal("Demo 1", model.Header.Name);
        Assert.Equal("30/1-A", model.Header.Uwi);
        Assert.Equal(8500, model.Header.TotalDepth!.In("ft"), 3);

        Assert.Equal(3, model.HoleSections.Count);
        Assert.Equal(2, model.Strings.Count);
        Assert.Equal(2, model.Survey.Count);
        Assert.Equal(7500, model.Tops.Single().Depth.In("ft"), 3);
    }

    [Fact]
    public void An_edm_export_loads_the_documented_entities()
    {
        // AC-2: the same provider, the "edm" profile, the vendor's own element names.
        using var scope = new Scratch();
        var path = scope.File("well.edm.xml");
        File.WriteAllText(path, EdmExport);

        var profile = MappingProfile.LoadBuiltIn("edm");

        Assert.True(profile.Verify);
        Assert.Empty(profile.Validate());

        var loaded = new MappedWellDataProvider(EdmXmlTabularSource.FromFile(path), profile)
            .LoadWellAsync("W9").GetAwaiter().GetResult();

        Assert.Equal("Demo-9", loaded.Model.Header.Name);
        Assert.Equal(3, loaded.Model.HoleSections.Count);
        Assert.Equal(2, loaded.Model.Strings.Count);
        Assert.Equal(6800, loaded.Model.Tops.Single().Depth.In("ft"), 3);
        Assert.Equal(new[] { 0d, 3000d }, loaded.Model.Survey.Select(station => station.Md.In("ft")));

        // The casing role is not in the export, and the load says so rather than guessing one.
        Assert.Contains(loaded.Warnings, warning => warning.Contains("code mapping", StringComparison.Ordinal));
    }

    [Fact]
    public void A_required_field_the_profile_does_not_map_fails_the_sentence_and_names_the_field()
    {
        // AC-3: the requirement is declared, the mapping does not provide it, and the message says which entity and field.
        var profile = MappingProfile.LoadBuiltIn("csv");
        profile.Entities["well"].Required.Add("uwi");
        profile.Entities["well"].Fields.Remove("uwi");

        var problems = OgMappingRules.UnmappedRequired(profile);

        var problem = Assert.Single(problems);

        Assert.Contains("well:", problem, StringComparison.Ordinal);
        Assert.Contains("'uwi'", problem, StringComparison.Ordinal);
        Assert.Contains("not mapped", problem, StringComparison.Ordinal);

        // A field provided through the vertical property table counts as mapped, so the check is about the source, not the shape.
        profile.Entities["well"].Properties = new PropertyTableMap
        {
            Table = "well_properties",
            JoinColumn = "well_id",
            ParentColumn = "well_id",
            NameColumn = "property",
            ValueColumn = "value",
            Map = { ["uwi"] = "uwi" },
        };

        Assert.Empty(OgMappingRules.UnmappedRequired(profile));

        // And a required field the configuration does not know at all is caught by the profile's own validation.
        profile.Entities["well"].Required.Add("notAField");

        Assert.Contains(profile.Validate(), issue => issue.Contains("unknown required field 'notAField'", StringComparison.Ordinal));
    }

    [Fact]
    public void A_required_field_whose_column_the_file_lacks_is_named_with_the_column()
    {
        // AC-3's second half: mapped, but the file does not carry the column, so the record would load blank.
        var profile = MappingProfile.LoadBuiltIn("csv");
        profile.Entities["well"].Required.Add("uwi");

        var problems = OgMappingRules.MissingColumns(profile, "well", new HashSet<string>(["well_id", "name"], StringComparer.OrdinalIgnoreCase));

        var problem = Assert.Single(problems);

        Assert.Contains("'uwi'", problem, StringComparison.Ordinal);
        Assert.Contains("'uwi'", problem, StringComparison.Ordinal);
        Assert.Contains("does not carry", problem, StringComparison.Ordinal);

        Assert.Empty(OgMappingRules.MissingColumns(profile, "well", new HashSet<string>(["well_id", "name", "uwi"], StringComparer.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task A_built_in_profile_cannot_be_overwritten_in_place()
    {
        // AC-4: the refusal names what to do instead, and the built-in on disk is untouched (it is not on disk at all).
        using var scope = new Scratch();
        var service = new OgMappingProfilesService(new Quiet(), () => scope.Folder);

        var refused = await service.SaveAsync("csv", MappingProfile.LoadBuiltIn("csv").ToJson(), "sara");

        Assert.False(refused.Success);
        Assert.Equal(OgMappingRules.CodeBuiltIn, refused.Reference);
        Assert.Contains("cannot be overwritten", refused.Message!, StringComparison.Ordinal);
        Assert.Contains("save the change as a copy", refused.Message!, StringComparison.OrdinalIgnoreCase);
        Assert.False(File.Exists(Path.Combine(scope.Folder, "csv.profile.json")));

        // A copy under another name is saved, and the custom one is what the list then offers beside the built-in.
        var copy = await service.SaveAsync("csv-company", MappingProfile.LoadBuiltIn("csv").ToJson(), "sara");

        Assert.True(copy.Success, copy.Message);
        Assert.False(copy.Value!.IsBuiltIn);
        Assert.True(File.Exists(Path.Combine(scope.Folder, "csv-company.profile.json")));

        var listed = await service.ListAsync();

        Assert.True(listed.Success, listed.Message);
        Assert.Contains(listed.Value!, profile => profile.Key == "csv" && profile.IsBuiltIn);
        Assert.Contains(listed.Value!, profile => profile.Key == "csv-company" && !profile.IsBuiltIn);

        // An actor is required, and a name a file cannot carry is refused.
        Assert.Equal(OgMappingRules.CodeActorRequired, (await service.SaveAsync("copy2", "{}", " ")).Reference);
        Assert.Equal(OgMappingRules.CodeProfileInvalid, (await service.SaveAsync("a/b", "{}", "sara")).Reference);
    }

    [Fact]
    public async Task An_invalid_profile_is_refused_and_the_version_on_disk_stays()
    {
        // The feature's state: invalid JSON refused, previous version retained.
        using var scope = new Scratch();
        var service = new OgMappingProfilesService(new Quiet(), () => scope.Folder);

        var first = await service.SaveAsync("company", MappingProfile.LoadBuiltIn("csv").ToJson(), "sara");

        Assert.True(first.Success, first.Message);

        var before = File.ReadAllText(Path.Combine(scope.Folder, "company.profile.json"));

        var broken = await service.SaveAsync("company", "{ not json", "sara");

        Assert.False(broken.Success);
        Assert.Equal(OgMappingRules.CodeProfileInvalid, broken.Reference);
        Assert.Contains("previous version", broken.Message!, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(before, File.ReadAllText(Path.Combine(scope.Folder, "company.profile.json")));

        // A profile that parses but does not hold together is refused too, so the folder cannot collect broken mappings.
        var structural = await service.SaveAsync("company", """{ "name": "x", "entities": { "nonsense": { "table": "t" } } }""", "sara");

        Assert.False(structural.Success);
        Assert.Contains("Unknown entity 'nonsense'", structural.Message!, StringComparison.Ordinal);
        Assert.Equal(before, File.ReadAllText(Path.Combine(scope.Folder, "company.profile.json")));
    }

    [Fact]
    public void The_round_trip_comparison_notices_a_lost_section()
    {
        // AC-5 depends on this comparison being real: an identical model reports nothing, and a lost section is named.
        var profile = MappingProfile.LoadBuiltIn("csv");

        Assert.NotNull(profile);

        using var scope = new Scratch();

        WriteCsvFolder(scope.Folder);

        var model = new MappedWellDataProvider(new CsvTabularSource(scope.Folder), profile)
            .LoadWellAsync("W-1").GetAwaiter().GetResult().Model;

        Assert.Empty(OgMappingRules.Differences(model, model, "ft"));

        var changed = new MappedWellDataProvider(new CsvTabularSource(scope.Folder), profile)
            .LoadWellAsync("W-1").GetAwaiter().GetResult().Model;

        changed.HoleSections.RemoveAt(0);

        var differences = OgMappingRules.Differences(model, changed, "ft");

        Assert.Contains(differences, difference => difference.Contains("hole sections", StringComparison.Ordinal));
    }

    [Fact]
    public void The_workbook_carries_one_sheet_per_entity()
    {
        // AC-6: the writer is the owner's, extended to several sheets; each profile table becomes its own sheet.
        using var scope = new Scratch();

        WriteCsvFolder(scope.Folder);

        var profile = MappingProfile.LoadBuiltIn("csv");
        var model = new MappedWellDataProvider(new CsvTabularSource(scope.Folder), profile)
            .LoadWellAsync("W-1").GetAwaiter().GetResult().Model;

        var warnings = new List<string>();
        var tables = TabularWriter.ToTables(model, profile, warnings);
        var path = scope.File("book.xlsx");

        using (var file = File.Create(path))
        {
            XlsxWriter.Write(file, tables
                .Select(table => (
                    SheetName: table.Name,
                    Header: (IReadOnlyList<string>)table.Columns.ToList(),
                    Rows: (IReadOnlyList<IReadOnlyList<object?>>)table.Rows
                        .Select(row => (IReadOnlyList<object?>)table.Columns.Select(column => (object?)row.GetValueOrDefault(column)).ToList())
                        .ToList()))
                .ToList());
        }

        using var zip = ZipFile.OpenRead(path);

        var sheets = zip.Entries
            .Where(entry => entry.FullName.StartsWith("xl/worksheets/sheet", StringComparison.Ordinal))
            .ToList();

        Assert.Equal(tables.Count, sheets.Count);
        Assert.True(sheets.Count >= 5, "one sheet per table the profile writes: " + sheets.Count);

        // The workbook reads back as tables named by their sheets, which is what makes the XLSX round trip possible.
        var reloaded = new ExcelTabularSource(path);

        Assert.Equal(model.HoleSections.Count,
            new MappedWellDataProvider(reloaded, profile).LoadWellAsync("W-1").GetAwaiter().GetResult().Model.HoleSections.Count);
    }

    [Fact]
    public async Task An_export_that_would_overwrite_says_so_before_writing_anything()
    {
        // The feature's overwrite state: the confirmation is required, and nothing is written without it.
        using var scope = new Scratch();

        WriteCsvFolder(scope.Folder);

        var output = scope.Sub("out");

        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "wells.csv"), "sentinel");

        var service = new OgMappingProfilesService(new Quiet(), () => scope.Sub("profiles"));

        var refused = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, scope.Folder, "W-1", output, Xlsx: false, Overwrite: false), "sara");

        Assert.False(refused.Success);
        Assert.Equal(OgMappingRules.CodeOverwrite, refused.Reference);
        Assert.Contains("would overwrite", refused.Message!, StringComparison.Ordinal);
        Assert.Equal("sentinel", File.ReadAllText(Path.Combine(output, "wells.csv")));

        // With the confirmation it writes, and it reloads identically through the same profile.
        var written = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, scope.Folder, "W-1", output, Xlsx: false, Overwrite: true), "sara");

        Assert.True(written.Success, written.Message);
        Assert.True(written.Value!.RoundTripOk, written.Value.DifferencesText);
        Assert.NotEqual("sentinel", File.ReadAllText(Path.Combine(output, "wells.csv")));
        Assert.Contains("reloads identically", written.Value.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task The_source_kind_and_the_file_are_checked_before_anything_is_read()
    {
        using var scope = new Scratch();
        var service = new OgMappingProfilesService(new Quiet(), () => scope.Folder);

        Assert.Equal(OgMappingRules.CodeProfileRequired, (await service.TestAsync(" ", OgMappingRules.Csv, scope.Folder, null)).Reference);
        Assert.Equal(OgMappingRules.CodeSourceRequired, (await service.TestAsync("csv", OgMappingRules.Csv, " ", null)).Reference);

        var unknownKind = await service.TestAsync("csv", "shapefile", scope.Folder, null);

        Assert.Equal(OgMappingRules.CodeSourceKindUnknown, unknownKind.Reference);
        Assert.Contains("use csv, edm, xlsx", unknownKind.Message!, StringComparison.Ordinal);

        // A path that was never created: the folder check has to fail rather than reading an empty folder as an empty well.
        var missing = await service.TestAsync("csv", OgMappingRules.Csv, scope.File("nothing-here"), null);

        Assert.Equal(OgMappingRules.CodeSourceMissing, missing.Reference);
        Assert.Contains("No CSV folder was found", missing.Message!, StringComparison.Ordinal);

        var unknownProfile = await service.TestAsync("no-such-profile", OgMappingRules.Csv, scope.Folder, null);

        Assert.Equal(OgMappingRules.CodeProfileUnknown, unknownProfile.Reference);
    }

    [Fact]
    public void The_page_shows_the_entities_the_model_and_the_export()
    {
        var page = File.ReadAllText(
            SourceRoot.AllSources().Single(source => source.EndsWith(PageFileName, StringComparison.Ordinal)));

        Assert.Contains("_entityGrid.ItemsSource = tested.Value.Entities", page, StringComparison.Ordinal);
        Assert.Contains("tested.Value.ProblemsText", page, StringComparison.Ordinal);
        Assert.Contains("tested.Value.ModelText", page, StringComparison.Ordinal);
        Assert.Contains("written.Value.DifferencesText", page, StringComparison.Ordinal);
        Assert.Contains("_xlsx.IsChecked == true", page, StringComparison.Ordinal);
        Assert.Contains("_overwrite.IsChecked == true", page, StringComparison.Ordinal);

        // A refused test clears the entities and the preview rather than leaving the last file's model on screen.
        Assert.Contains("_entityGrid.ItemsSource = null", page, StringComparison.Ordinal);
        Assert.Contains("_model.Text = string.Empty", page, StringComparison.Ordinal);
    }

    // ------------------------------------------------------------------ the fixtures

    /// <summary>A temporary folder tree, deleted when the test ends.</summary>
    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "int04-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Folder
        {
            get
            {
                var folder = Path.Combine(_root, "in");
                Directory.CreateDirectory(folder);
                return folder;
            }
        }

        public string Sub(string name)
        {
            var folder = Path.Combine(_root, name);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public string File(string name) => Path.Combine(_root, name);

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
                // A file still held by a reader: the temporary folder is the OS's to clean up.
            }
        }
    }

    /// <summary>A CSV folder matching the built-in "csv" profile: one file per table, header row, spare columns preserved.</summary>
    private static void WriteCsvFolder(string folder)
    {
        File.WriteAllText(Path.Combine(folder, "wells.csv"),
            "well_id,name,uwi,operator,field,country,latitude,longitude,crs,survey_north,total_depth,depth_unit,depth_datum\n" +
            "W-1,Demo 1,30/1-A,Probe Energy,North,Norway,58.5,2.1,EPSG:4326,grid,8500,ft,MSL\n");

        File.WriteAllText(Path.Combine(folder, "hole_sections.csv"),
            "well_id,section_id,top,bottom,bit_size,diameter_unit,depth_unit\n" +
            "W-1,H1,0,1500,26,in,ft\n" +
            "W-1,H2,1500,7000,12.25,in,ft\n" +
            "W-1,H3,7000,8500,8.5,in,ft\n");

        File.WriteAllText(Path.Combine(folder, "strings.csv"),
            "well_id,string_id,kind,name,top,bottom,od,id_diameter,grade,depth_unit,diameter_unit\n" +
            "W-1,S1,Casing,Surface,0,1500,20,18.73,K55,ft,in\n" +
            "W-1,S2,Casing,Production,0,7000,9.625,8.681,L80,ft,in\n");

        File.WriteAllText(Path.Combine(folder, "tops.csv"),
            "well_id,name,depth,lithology,depth_unit\n" +
            "W-1,Hod,7500,Sandstone,ft\n");

        File.WriteAllText(Path.Combine(folder, "survey.csv"),
            "well_id,md,inclination,azimuth,tvd,depth_unit\n" +
            "W-1,0,0,0,0,ft\n" +
            "W-1,3000,12.5,45,2976.3,ft\n");
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
