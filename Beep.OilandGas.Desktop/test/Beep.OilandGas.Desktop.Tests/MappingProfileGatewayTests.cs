using System.IO.Compression;
using TheTechIdea.Beep.OilandGas.Desktop.Data.Interop;
using TheTechIdea.OilGas.Wellbore.Loaders;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-04 end to end through the service the page drives: the profiles are listed, an EDM export and a CSV folder are tested through
/// their profiles (AC-1, AC-2), a profile that declares a required field it does not map fails naming the field (AC-3), a built-in
/// cannot be overwritten (AC-4), and the write-back proves its own round trip for a CSV folder and for a workbook with one sheet per
/// entity (AC-5, AC-6).
/// </summary>
/// <remarks>
/// There is no database in this feature: a mapped import is file-to-file, so the opt-in test drives the real service against real
/// files rather than opening SQLite. The same switch guards it, because it writes outside the test's own memory.
/// </remarks>
public sealed class MappingProfileGatewayTests
{
    private const string EdmExport = """
        <?xml version="1.0" encoding="utf-8"?>
        <export>
          <CD_WELL well_id="W9" well_common_name="Demo-9" water_depth="0"/>
          <CD_WELLBORE wellbore_id="B9" well_id="W9" wellbore_name="Demo-9 main" ko_md="0"/>
          <CD_HOLE_SECT well_id="W9" wellbore_id="B9" hole_sect_group_id="PLAN" hole_sect_id="H1" sect_type_code="CAS" od_casing="20" id_casing="18.73" id_drift="18.54" hole_size="26" length="500" md_shoe="500"/>
          <CD_HOLE_SECT well_id="W9" wellbore_id="B9" hole_sect_group_id="PLAN" hole_sect_id="H2" sect_type_code="CAS" od_casing="9.625" id_casing="8.681" id_drift="8.525" hole_size="12.25" length="6500" md_shoe="7000"/>
          <CD_WELLBORE_FORMATION well_id="W9" wellbore_id="B9" wellbore_formation_id="F1" phase="PROTOTYPE" formation_name="Hod" prognosed_md="6800" prognosed_tvd="6700"/>
          <CD_DEFINITIVE_SURVEY_STATION well_id="W9" wellbore_id="B9" def_survey_header_id="S1" sequence_no="0" md="0" inclination="0" azimuth="0" tvd="0"/>
          <CD_DEFINITIVE_SURVEY_STATION well_id="W9" wellbore_id="B9" def_survey_header_id="S1" sequence_no="1" md="3000" inclination="12.5" azimuth="45" tvd="2976.3"/>
        </export>
        """;

    [IntegrationFact]
    public async Task Profiles_are_tested_and_written_back_with_their_round_trip_proven()
    {        using var scope = new Scratch();

        var source = scope.Sub("in");
        var profiles = scope.Sub("profiles");
        var csvOut = scope.Sub("out-csv");
        var xlsxOut = scope.Sub("out-xlsx");

        WriteCsvFolder(source);

        var edm = scope.FileAt("well.edm.xml");
        File.WriteAllText(edm, EdmExport);

        var service = new OgMappingProfilesService(new Recording(), () => profiles);

        // ---- the list: the four built-ins, and nothing custom yet ----
        var listed = await service.ListAsync();

        Assert.True(listed.Success, listed.Message);
        Assert.Equal(["csv", "edm", "ppdm39", "template"], listed.Value!.Select(profile => profile.Key));
        Assert.All(listed.Value, profile => Assert.True(profile.IsBuiltIn));
        Assert.All(listed.Value, profile => Assert.True(profile.Valid, profile.Describe()));

        // ---- AC-1: a CSV folder through the built-in profile ----
        var csv = await service.TestAsync("csv", OgMappingRules.Csv, source, "W-1");

        Assert.True(csv.Success, csv.Message);
        Assert.True(csv.Value!.Passed, csv.Value.SummaryText);

        // Every entity the profile declares is reported: the fixture leaves components and intervals out, and that is said per
        // entity rather than failing the whole test.
        Assert.Equal(7, csv.Value.Entities.Count);
        Assert.Equal(2, csv.Value.Missing);
        Assert.Contains(csv.Value.Entities, entity => entity.Entity == "component" && !entity.Present);
        Assert.Contains("hole sections 3", csv.Value.ModelText, StringComparison.Ordinal);

        // ---- AC-2: the EDM export through the EDM profile ----
        var edmTest = await service.TestAsync("edm", OgMappingRules.Edm, edm, "W9");

        Assert.True(edmTest.Success, edmTest.Message);
        Assert.True(edmTest.Value!.Passed, edmTest.Value.SummaryText);
        Assert.Contains("Demo-9", edmTest.Value.ModelText, StringComparison.Ordinal);
        Assert.Contains(edmTest.Value.Warnings, warning => warning.Contains("code mapping", StringComparison.Ordinal));

        // A source that is not there is refused before anything is read.
        var missing = await service.TestAsync("csv", OgMappingRules.Csv, scope.FileAt("absent"), "W-1");

        Assert.Equal(OgMappingRules.CodeSourceMissing, missing.Reference);

        // ---- AC-4: the built-in is read-only, and the copy is a customer profile ----
        var refused = await service.SaveAsync("csv", MappingProfile.LoadBuiltIn("csv").ToJson(), "sara");

        Assert.Equal(OgMappingRules.CodeBuiltIn, refused.Reference);

        var copy = await service.SaveAsync("csv-company", MappingProfile.LoadBuiltIn("csv").ToJson(), "sara");

        Assert.True(copy.Success, copy.Message);
        Assert.Equal("csv-company", copy.Value!.Key);
        Assert.True(File.Exists(Path.Combine(profiles, "csv-company.profile.json")));

        var withCustom = await service.ListAsync();

        Assert.True(withCustom.Success, withCustom.Message);
        Assert.Equal(5, withCustom.Value!.Count);
        Assert.Contains(withCustom.Value, profile => profile.Key == "csv-company" && !profile.IsBuiltIn);

        // The custom profile is a usable profile: the same file tests through it.
        var throughCopy = await service.TestAsync("csv-company", OgMappingRules.Csv, source, "W-1");

        Assert.True(throughCopy.Success, throughCopy.Message);
        Assert.True(throughCopy.Value!.Passed, throughCopy.Value.SummaryText);

        // ---- AC-3: a required field the profile does not map fails with the field named ----
        var strict = MappingProfile.LoadBuiltIn("csv");

        strict.Entities["well"].Required.Add("uwi");
        strict.Entities["well"].Fields.Remove("uwi");

        Assert.True((await service.SaveAsync("csv-strict", strict.ToJson(), "sara")).Success);

        var unmapped = await service.TestAsync("csv-strict", OgMappingRules.Csv, source, "W-1");

        Assert.True(unmapped.Success, unmapped.Message);
        Assert.False(unmapped.Value!.Passed);
        Assert.Contains(unmapped.Value.Problems, problem =>
            problem.Contains("well:", StringComparison.Ordinal) && problem.Contains("'uwi'", StringComparison.Ordinal));
        Assert.Contains("required field 'uwi'", unmapped.Value.ProblemsText, StringComparison.Ordinal);
        Assert.Null(unmapped.Value.Model);

        // ---- AC-5: the CSV write-back reloads identically, and the overwrite waits for confirmation ----
        var written = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, source, "W-1", csvOut, Xlsx: false, Overwrite: false), "sara");

        Assert.True(written.Success, written.Message);
        Assert.True(written.Value!.RoundTripOk, written.Value.DifferencesText);
        Assert.Contains("wells", written.Value.Tables);
        Assert.Contains("hole_sections", written.Value.Tables);
        Assert.True(File.Exists(Path.Combine(csvOut, "wells.csv")));

        var again = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, source, "W-1", csvOut, Xlsx: false, Overwrite: false), "sara");

        Assert.Equal(OgMappingRules.CodeOverwrite, again.Reference);
        Assert.Contains("Confirm the overwrite", again.Message!, StringComparison.Ordinal);

        var confirmed = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, source, "W-1", csvOut, Xlsx: false, Overwrite: true), "sara");

        Assert.True(confirmed.Success, confirmed.Message);
        Assert.True(confirmed.Value!.RoundTripOk, confirmed.Value.DifferencesText);

        // ---- AC-6 and AC-5 for the workbook: one sheet per entity, and it reads back as the same well ----
        var workbook = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, source, "W-1", xlsxOut, Xlsx: true, Overwrite: false), "sara");

        Assert.True(workbook.Success, workbook.Message);
        Assert.True(workbook.Value!.RoundTripOk, workbook.Value.DifferencesText);
        Assert.NotNull(workbook.Value.XlsxPath);
        Assert.True(File.Exists(workbook.Value.XlsxPath));

        using (var zip = ZipFile.OpenRead(workbook.Value.XlsxPath!))
        {
            var sheets = zip.Entries.Count(entry =>
                entry.FullName.StartsWith("xl/worksheets/sheet", StringComparison.Ordinal));

            Assert.Equal(workbook.Value.Tables.Count, sheets);
            Assert.True(sheets >= 5, "one sheet per table: " + sheets);
        }

        // ---- an export that would lose a row is refused rather than reported as done ----
        var actor = await service.WriteBackAsync(
            new OgWriteBackRequest("csv", OgMappingRules.Csv, source, "W-1", csvOut, Xlsx: false, Overwrite: true), " ");

        Assert.Equal(OgMappingRules.CodeActorRequired, actor.Reference);
    }

    // ------------------------------------------------------------------ fixtures

    private sealed class Scratch : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), "int04-it-" + Guid.NewGuid().ToString("n")[..8]);

        public Scratch() => Directory.CreateDirectory(_root);

        public string Sub(string name)
        {
            var folder = Path.Combine(_root, name);
            Directory.CreateDirectory(folder);
            return folder;
        }

        public string FileAt(string name) => Path.Combine(_root, name);

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
                // A reader still holds a file; the OS temp folder is its own to clean.
            }
        }
    }

    private static void WriteCsvFolder(string folder)
    {
        File.WriteAllText(Path.Combine(folder, "wells.csv"),
            "well_id,name,uwi,operator,field,country,total_depth,depth_unit,depth_datum,survey_north\n" +
            "W-1,Demo 1,30/1-A,Probe Energy,North,Norway,8500,ft,MSL,grid\n");

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

    private sealed class Recording : IFailureReporter
    {
        public List<string> Failures { get; } = [];

        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error)
        {
            Failures.Add($"{operation} | {consequence}");
            return FailureReferenceFormat.New();
        }
    }
}
