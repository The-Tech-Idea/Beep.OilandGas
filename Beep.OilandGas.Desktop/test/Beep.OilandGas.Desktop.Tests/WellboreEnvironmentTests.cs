using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;
using TheTechIdea.OilGas.Wellbore.Data;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// VSB-02: the toolboxes are registered before the environment is read, the environment loads, and its
/// findings are reported rather than swallowed.
/// </summary>
public sealed class WellboreEnvironmentTests
{
    private const string ProbeToolbox = "vsb02Probe";

    [Fact]
    public void Registering_every_toolbox_leaves_none_missing()
    {
        OgToolboxBootstrap.RegisterAll();

        Assert.True(OgToolboxBootstrap.HasRun);
        Assert.Empty(OgToolboxBootstrap.Missing());
        Assert.InRange(OgToolboxBootstrap.RegisteredNames.Count, OgToolboxBootstrap.Expected.Count, 64);
        Assert.Contains(OgToolboxBootstrap.RegisteredNames, name =>
            string.Equals(name, "integrity", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Registering_twice_does_not_duplicate_a_toolbox()
    {
        OgToolboxBootstrap.RegisterAll();
        var first = OgToolboxBootstrap.RegisteredNames.Count;

        OgToolboxBootstrap.RegisterAll();

        Assert.Equal(first, OgToolboxBootstrap.RegisteredNames.Count);
    }

    [Fact]
    public void A_toolbox_section_is_validated_only_after_the_toolbox_is_registered()
    {
        // The probe name is ours alone, so this proves the ordering without disturbing the six real toolboxes.
        ToolboxRegistry.Unregister(ProbeToolbox);

        var root = Path.Combine(OgPaths.DataRoot, "vsb02-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "environment.json");

        File.WriteAllText(file,
            $$"""
              {
                "region": { "country": "KW" },
                "toolboxes": {
                  "{{ProbeToolbox}}": { "anything": 1 }
                }
              }
              """);

        try
        {
            var services = WellboreServices.Load(file);

            var before = services.Environment.Validate();
            Assert.DoesNotContain(before, line =>
                line.Contains(ProbeToolbox, StringComparison.OrdinalIgnoreCase));

            ToolboxRegistry.Register<ProbeSettings>(
                ProbeToolbox,
                _ => ["probe: deliberately invalid setting"],
                "VSB-02 ordering probe");

            var after = services.Environment.Validate();
            Assert.Contains(after, line =>
                line.Contains($"toolboxes.{ProbeToolbox}", StringComparison.OrdinalIgnoreCase)
                && line.Contains("deliberately invalid setting", StringComparison.Ordinal));
        }
        finally
        {
            ToolboxRegistry.Unregister(ProbeToolbox);
            TryDelete(root);
        }
    }

    [Fact]
    public void Defaults_are_used_when_no_environment_file_is_configured()
    {
        var environment = new OgWellboreEnvironment();

        var result = environment.Load(null);

        Assert.True(result.Success);
        Assert.False(environment.IsLoaded);
        Assert.Null(environment.Path);
        Assert.NotNull(environment.Services);

        var snapshot = new OgEnvironmentStore(environment).Current();
        Assert.True(snapshot.UsingDefaults);
        Assert.Empty(snapshot.MissingToolboxes);
        Assert.True(snapshot.UnitCount > 0, "The default environment should carry built-in units.");
    }

    [Fact]
    public void A_missing_environment_file_is_refused_with_a_reason()
    {
        var environment = new OgWellboreEnvironment();
        var missing = Path.Combine(OgPaths.DataRoot, "does-not-exist-" + Guid.NewGuid().ToString("n")[..6] + ".json");

        var result = environment.Load(missing);

        Assert.False(result.Success);
        Assert.Equal("environment-not-found", result.Reference);
        Assert.False(environment.IsLoaded);
    }

    [Fact]
    public void A_valid_environment_file_loads_and_reports_its_country()
    {
        var root = Path.Combine(OgPaths.DataRoot, "vsb02-ok-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(root);
        var file = Path.Combine(root, "environment.json");
        File.WriteAllText(file, """{ "region": { "country": "KW", "name": "Kuwait" } }""");

        try
        {
            var environment = new OgWellboreEnvironment();

            var result = environment.Load(file);

            Assert.True(result.Success, result.Message);
            Assert.True(environment.IsLoaded);
            Assert.Equal(file, environment.Path);

            var snapshot = new OgEnvironmentStore(environment).Current();
            Assert.Equal("KW", snapshot.CountryCode);
            Assert.Equal("Kuwait", snapshot.RegionName);
            Assert.False(snapshot.UsingDefaults);
        }
        finally
        {
            TryDelete(root);
        }
    }

    [Fact]
    public void CheckResources_reports_a_missing_folder_for_a_registered_toolbox()
    {
        const string probe = "vsb02Resources";
        ToolboxRegistry.Unregister(probe);

        var root = Path.Combine(OgPaths.DataRoot, "vsb02-res-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(root);
        var existing = Path.Combine(root, "present");
        Directory.CreateDirectory(existing);
        var file = Path.Combine(root, "environment.json");

        File.WriteAllText(file,
            $$"""
              {
                "toolboxes": {
                  "{{probe}}": { "folder": "{{Path.Combine(root, "absent").Replace("\\", "/")}}" }
                }
              }
              """);

        try
        {
            ToolboxRegistry.Register<ProbeResourceSettings>(
                probe,
                validate: null,
                description: "VSB-02 resource probe",
                check: (settings, _) => settings.Folder is { Length: > 0 } folder && !Directory.Exists(folder)
                    ? [$"folder '{folder}' does not exist"]
                    : []);

            var environment = new OgWellboreEnvironment();
            Assert.True(environment.Load(file).Success, "The probe environment did not load.");

            var store = new OgEnvironmentStore(environment);
            var problems = store.CheckResources();

            Assert.Contains(problems, finding =>
                finding.Source == "toolboxes"
                && finding.Message.Contains(probe, StringComparison.OrdinalIgnoreCase)
                && finding.Message.Contains("does not exist", StringComparison.OrdinalIgnoreCase));

            // The same toolbox pointed at a folder that exists reports nothing.
            File.WriteAllText(file,
                $$"""
                  {
                    "toolboxes": {
                      "{{probe}}": { "folder": "{{existing.Replace("\\", "/")}}" }
                    }
                  }
                  """);

            Assert.True(environment.Load(file).Success);
            Assert.DoesNotContain(store.CheckResources(), finding =>
                finding.Message.Contains(probe, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            ToolboxRegistry.Unregister(probe);
            TryDelete(root);
        }
    }

    [Fact]
    public void A_country_preset_is_applied_and_labelled_as_a_preset()
    {
        var environment = new OgWellboreEnvironment();

        var result = environment.LoadCountryPreset("KW");

        Assert.True(result.Success, result.Message);
        Assert.True(environment.IsUsingCountryPreset);
        Assert.True(environment.IsLoaded);

        var snapshot = new OgEnvironmentStore(environment).Current();
        Assert.Equal("KW", snapshot.CountryCode);
        Assert.False(snapshot.UsingDefaults);
    }

    [Fact]
    public void An_unknown_country_preset_is_refused()
    {
        var environment = new OgWellboreEnvironment();

        var result = environment.LoadCountryPreset("ZZ");

        Assert.False(result.Success);
        Assert.Equal("country-preset-unknown", result.Reference);
    }

    [Fact]
    public void Toolboxes_are_usable_when_no_resource_problem_is_reported()
    {
        OgToolboxBootstrap.RegisterAll();

        var environment = new OgWellboreEnvironment();
        environment.Load(null);
        var store = new OgEnvironmentStore(environment);

        var availability = store.ToolboxAvailability();

        Assert.Equal(OgToolboxBootstrap.RegisteredNames.Count, availability.Count);
        Assert.All(availability, pair => Assert.Empty(pair.Value));
        Assert.True(store.IsToolboxUsable("integrity"));
        Assert.True(store.IsToolboxUsable("no-such-toolbox"));
    }

    [Fact]
    public void A_reload_announces_the_change_and_describes_it()
    {
        var lifetime = new TheTechIdea.Beep.OilandGas.Desktop.Services.OgWellboreLifetime();
        var announced = 0;
        lifetime.EnvironmentChanged += (_, _) => announced++;

        var result = lifetime.Reload();

        Assert.True(result.Success, result.Message);
        Assert.Equal(1, announced);
        Assert.False(string.IsNullOrWhiteSpace(lifetime.LastChangeSummary));
        Assert.True(lifetime.IsCreated);
    }

    private static void TryDelete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
            // A provider may still hold the file; the test root is disposable either way.
        }
    }

    /// <summary>Settings type for the ordering probe.</summary>
    public sealed class ProbeSettings
    {
        public int Anything { get; set; }
    }

    /// <summary>Settings type for the resource probe: a folder that must exist.</summary>
    public sealed class ProbeResourceSettings
    {
        public string? Folder { get; set; }
    }
}
