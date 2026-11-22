using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Bridging;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.OilGas.WellLogs;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// INT-01: LAS 2.0 loads, LAS 3.0 passes the compliance checker and reloads identically, DLIS frames load with
/// array channels reported, decimation states its ratio, the encoding is reported, and an export never
/// overwrites by accident.
/// </summary>
public sealed class LasInteropTests
{
    [Fact]
    public void A_las_2_file_loads_and_assigns_curves_to_a_log_set()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(OgLasWorkspace.ExampleLas, "DEMO-LAS-1");

        // AC-1
        Assert.Equal("LAS 2.0", loaded.Preview.Standard);
        Assert.NotNull(loaded.Log);
        Assert.Equal(LogIndexType.Depth, loaded.Log!.IndexType);
        Assert.Equal(9, loaded.Preview.Rows);

        Assert.Contains("GR", loaded.Preview.Curves);
        Assert.Contains("RHOB", loaded.Preview.Curves);

        // The index is separate from the curves, and the unit came from the file.
        Assert.Equal("m", loaded.Preview.IndexUnit, ignoreCase: true);
        Assert.Equal(1000, loaded.Preview.Top, 3);
        Assert.Equal(1004, loaded.Preview.Bottom, 3);

        Assert.False(loaded.Preview.HasErrors);
    }

    [Fact]
    public void An_imported_las_becomes_a_drawable_log_document()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(OgLasWorkspace.ExampleLas);
        var document = WellLogDocuments.FromModel(loaded.Model);

        // The import feeds the visual suite rather than stopping at a model object.
        Assert.NotEmpty(document.Tracks);
        Assert.Contains(
            document.Tracks.OfType<WellLogTrack>().SelectMany(track => track.Curves),
            curve => curve.Mnemonic.Equals("GR", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void A_las_3_export_passes_the_checker_and_reloads_identically()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(OgLasWorkspace.ExampleLas);

        var text = OgLasWorkspace.ExportText(loaded.Model, loaded.Log, OgLasWorkspace.Las30);
        Assert.True(text.Success, text.Message);
        Assert.Contains("3.0", text.Value!, StringComparison.Ordinal);

        // AC-2: the tools' own compliance checker finds nothing.
        Assert.Empty(OgLasWorkspace.CheckLas3(text.Value!));

        // AC-6: reloading gives the same curves and the same rows, value for value.
        var reloaded = OgLasWorkspace.LoadText(text.Value!);

        Assert.Equal(loaded.Preview.Rows, reloaded.Preview.Rows);
        Assert.Equal(loaded.Preview.Curves, reloaded.Preview.Curves);

        for (var curve = 0; curve < loaded.Log!.Curves.Count; curve++)
        {
            Assert.Equal(
                loaded.Log.Curves[curve].Values.Select(value => value ?? double.NaN),
                reloaded.Log!.Curves[curve].Values.Select(value => value ?? double.NaN));
        }
    }

    [Fact]
    public void A_las_2_file_does_not_pass_the_las_3_checker_so_the_check_is_not_always_empty()
    {
        UseDefaultEnvironment();

        // If the checker accepted everything, its silence on the exported file would prove nothing.
        Assert.NotEmpty(OgLasWorkspace.CheckLas3(OgLasWorkspace.ExampleLas));
    }

    [Fact]
    public void A_las_3_export_written_to_disk_reloads_from_disk()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(OgLasWorkspace.ExampleLas);
        var directory = Path.Combine(OgPaths.DataRoot, "las-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "round-trip.las");

        var written = OgLasWorkspace.ExportFile(loaded.Model, loaded.Log, OgLasWorkspace.Las30, path);
        Assert.True(written.Success, written.Message);
        Assert.True(File.Exists(path));

        var reloaded = OgLasWorkspace.Load(path);

        Assert.Equal("LAS 3.0", reloaded.Preview.Standard);
        Assert.Equal(loaded.Preview.Rows, reloaded.Preview.Rows);
        Assert.Equal(loaded.Preview.Curves, reloaded.Preview.Curves);
        Assert.Empty(OgLasWorkspace.CheckLas3(File.ReadAllText(path)));
    }

    [Fact]
    public void An_export_refuses_to_overwrite_unless_asked()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(OgLasWorkspace.ExampleLas);
        var directory = Path.Combine(OgPaths.DataRoot, "las-" + Guid.NewGuid().ToString("n")[..8]);
        var path = Path.Combine(directory, "exists.las");

        Assert.True(OgLasWorkspace.ExportFile(loaded.Model, loaded.Log, OgLasWorkspace.Las20, path).Success);
        var before = File.ReadAllText(path);

        var second = OgLasWorkspace.ExportFile(loaded.Model, loaded.Log, OgLasWorkspace.Las20, path);
        Assert.False(second.Success);
        Assert.Equal(OgLasWorkspace.CodeExportExists, second.Reference);
        Assert.Equal(before, File.ReadAllText(path));

        Assert.True(OgLasWorkspace.ExportFile(loaded.Model, loaded.Log, OgLasWorkspace.Las20, path, overwrite: true).Success);
    }

    [Fact]
    public void A_decimated_load_states_both_counts_and_the_ratio()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(OgLasWorkspace.ExampleLas, maxSamples: 4);

        // AC-4: fewer rows, and the reduction is said out loud rather than hidden.
        Assert.True(loaded.Preview.Decimated);
        Assert.True(loaded.Preview.Rows < 9, $"Decimation kept all {loaded.Preview.Rows} rows.");
        Assert.NotNull(loaded.Preview.DecimationNote);
        Assert.Contains("source row(s)", loaded.Preview.DecimationNote!, StringComparison.Ordinal);
        Assert.Contains(":1", loaded.Preview.DecimationNote!, StringComparison.Ordinal);
        Assert.Contains("Decimated", loaded.Preview.Describe(), StringComparison.Ordinal);
    }

    [Fact]
    public void The_encoding_is_reported_and_can_be_overridden()
    {
        UseDefaultEnvironment();

        var directory = Path.Combine(OgPaths.DataRoot, "las-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "encoded.las");
        File.WriteAllText(path, OgLasWorkspace.ExampleLas);

        var detected = OgLasWorkspace.Load(path);
        Assert.False(string.IsNullOrWhiteSpace(detected.Preview.Encoding));

        var forced = OgLasWorkspace.Load(path, encoding: "windows-1252");

        // AC-5: the override is what the file was read with, and the page can show it.
        Assert.Equal("windows-1252", forced.Preview.Encoding, ignoreCase: true);
        Assert.Equal(detected.Preview.Rows, forced.Preview.Rows);
    }

    [Fact]
    public void A_dlis_file_loads_frames_and_reports_array_channels()
    {
        UseDefaultEnvironment();

        var path = DlisFixture.WriteFile();
        var dlis = OgLasWorkspace.LoadDlis(path);

        // AC-3: the frame became a log set…
        Assert.Equal(1, dlis.Frames);
        var set = Assert.Single(dlis.LogSets);
        Assert.Contains(set.Curves, curve => curve.Mnemonic == "GR");

        // …the array channel is a warning, not a silently dropped curve…
        Assert.DoesNotContain(set.Curves, curve => curve.Mnemonic == "WAVE");
        Assert.Contains(dlis.Warnings, warning =>
            warning.Contains("array channels", StringComparison.OrdinalIgnoreCase)
            && warning.Contains("WAVE", StringComparison.Ordinal));
    }

    [Fact]
    public void An_unreadable_las_is_an_error_with_a_warning_not_an_exception()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText("{ this is not a LAS file }");

        Assert.True(loaded.Preview.HasErrors);
        Assert.NotEmpty(loaded.Preview.Warnings);
        Assert.Equal(0, loaded.Preview.Rows);
    }

    [Fact]
    public void An_empty_las_is_refused_before_it_becomes_an_empty_log()
    {
        UseDefaultEnvironment();

        var loaded = OgLasWorkspace.LoadText(string.Empty);

        Assert.True(loaded.Preview.HasErrors);
        Assert.Contains(loaded.Preview.Warnings, warning =>
            warning.Contains("empty", StringComparison.OrdinalIgnoreCase));
    }

    private static void UseDefaultEnvironment()
    {
        OgToolboxBootstrap.RegisterAll();
        WellboreServices.Global = new WellboreServices();
    }
}
