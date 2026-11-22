using System.IO;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;

/// <summary>One problem found in the environment or in a toolbox's settings.</summary>
public sealed record OgEnvironmentFinding(string Severity, string Source, string Message, string? File);

/// <summary>
/// Owns the active <see cref="WellboreServices"/>: it registers the toolboxes first, loads the customer's
/// environment, publishes the services process-wide, and keeps every warning and issue it found.
/// </summary>
public sealed class OgWellboreEnvironment
{
    private readonly object _gate = new();

    /// <summary>The file the services were loaded from, or null when running on built-in defaults.</summary>
    public string? Path { get; private set; }

    public bool IsLoaded { get; private set; }

    public bool IsUsingCountryPreset { get; private set; }

    /// <summary>
    /// True when the services came from the company database's shared settings rather than this workstation's
    /// file. Every user of that database then reads the same environment.
    /// </summary>
    public bool IsFromSharedSettings { get; private set; }

    private string? _sharedDocument;
    private string? _sharedBaseDirectory;

    /// <summary>The live services. Never null: defaults are used before anything is loaded.</summary>
    public WellboreServices Services { get; private set; } = WellboreServices.Global;

    public IReadOnlyList<OgEnvironmentFinding> Findings { get; private set; } = [];

    /// <summary>True when the environment produced at least one warning or error.</summary>
    public bool HasFindings => Findings.Count > 0;

    public int ErrorCount => Findings.Count(finding =>
        finding.Severity.Equals("Error", StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Registers the toolboxes, then loads <paramref name="environmentFile"/>. Passing null loads nothing
    /// and keeps the built-in defaults, which is what a first run does.
    /// </summary>
    public OgResult Load(string? environmentFile)
    {
        lock (_gate)
        {
            // VSB-02 ordering: the registry must know the toolboxes before the environment is read,
            // or the toolboxes section is not validated at all.
            OgToolboxBootstrap.RegisterAll();

            try
            {
                if (string.IsNullOrWhiteSpace(environmentFile))
                {
                    Services = WellboreServices.Global;
                    Path = null;
                    IsLoaded = false;
                    IsUsingCountryPreset = false;
                    IsFromSharedSettings = false;
                    _sharedDocument = null;
                    _sharedBaseDirectory = null;
                    Findings = Collect(Services, null);
                    return OgResult.Ok("No environment file was configured; built-in defaults are in use.");
                }

                if (!File.Exists(environmentFile))
                {
                    return OgResult.Failed(
                        $"The environment file was not found: {environmentFile}",
                        "environment-not-found");
                }

                var services = WellboreServices.Load(environmentFile);
                WellboreServices.Global = services;

                Services = services;
                Path = environmentFile;
                IsLoaded = true;
                IsUsingCountryPreset = false;
                IsFromSharedSettings = false;
                _sharedDocument = null;
                _sharedBaseDirectory = null;
                Findings = Collect(services, environmentFile);

                return OgResult.Ok($"Environment loaded from {environmentFile}.");
            }
            catch (Exception ex)
            {
                // The store's own failure is reported by the caller through OgDiagnosticsPipeline.
                Services = WellboreServices.Global;
                Findings = Collect(Services, environmentFile);
                throw new InvalidOperationException(
                    $"The wellbore environment '{environmentFile}' could not be loaded.", ex);
            }
        }
    }

    /// <summary>Reloads the file currently in use. Does nothing when running on defaults.</summary>
    public OgResult Reload() =>
        IsFromSharedSettings
            ? LoadSharedDocument(_sharedDocument, _sharedBaseDirectory)
            : Path is null
                ? OgResult.Ok("No environment file is in use; defaults are already active.")
                : Load(Path);

    /// <summary>
    /// Loads the environment from the company database's shared document instead of a workstation file.
    /// </summary>
    /// <remarks>
    /// The document is the same shape the file has, so the tool keeps one parser and one set of resources: relative
    /// resource paths resolve against <paramref name="baseDirectory"/>, which the caller passes as the folder this
    /// workstation can read them from. Nothing is written here — the document stays authoritative in the database.
    /// </remarks>
    public OgResult LoadSharedDocument(string? documentJson, string? baseDirectory)
    {
        lock (_gate)
        {
            OgToolboxBootstrap.RegisterAll();

            if (string.IsNullOrWhiteSpace(documentJson))
            {
                return OgResult.Failed(
                    "The shared environment document is empty.",
                    "environment-shared-empty");
            }

            var environment = WellboreEnvironment.FromJson(
                documentJson,
                out var warnings,
                baseDirectory,
                "shared settings");

            var services = WellboreServices.FromEnvironment(environment);
            services.Warnings.InsertRange(0, warnings.Select(warning => "environment: " + warning));
            WellboreServices.Global = services;

            Services = services;
            Path = null;
            IsLoaded = true;
            IsUsingCountryPreset = false;
            IsFromSharedSettings = true;
            _sharedDocument = documentJson;
            _sharedBaseDirectory = baseDirectory;
            Findings = Collect(services, null);

            return OgResult.Ok("Environment loaded from the shared company settings.");
        }
    }

    /// <summary>
    /// Applies one of the 62 built-in country presets. The preset is labelled as a preset — it is not a
    /// customer configuration — and the services are rebuilt from it so resource files are re-read.
    /// </summary>
    public OgResult LoadCountryPreset(string countryCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(countryCode);

        if (!CountryPresets.TryGet(countryCode, out _))
        {
            return OgResult.Failed(
                $"'{countryCode}' is not a built-in country preset.",
                "country-preset-unknown");
        }

        lock (_gate)
        {
            OgToolboxBootstrap.RegisterAll();

            var environment = CountryPresets.Load(countryCode);
            var services = WellboreServices.FromEnvironment(environment);
            WellboreServices.Global = services;

            Services = services;
            Path = null;
            IsLoaded = true;
            IsUsingCountryPreset = true;
            IsFromSharedSettings = false;
            _sharedDocument = null;
            _sharedBaseDirectory = null;
            Findings = Collect(services, null);

            return OgResult.Ok($"Country preset {countryCode} applied.");
        }
    }

    /// <summary>Flattens the services' warnings and coded issues into one list.</summary>
    public static IReadOnlyList<OgEnvironmentFinding> Collect(WellboreServices services, string? file)
    {
        var findings = new List<OgEnvironmentFinding>();

        foreach (var warning in services.Warnings)
        {
            findings.Add(new OgEnvironmentFinding("Warning", "environment", warning, file));
        }

        foreach (var issue in services.Issues)
        {
            findings.Add(new OgEnvironmentFinding(issue.Severity.ToString(), issue.Code, issue.Message, issue.File ?? file));
        }

        return findings;
    }

    /// <summary>
    /// Problems with the files and folders the registered toolboxes' settings point to. Separate from
    /// <see cref="Findings"/> because it touches the disk, so it is only run when asked.
    /// </summary>
    public IReadOnlyList<OgEnvironmentFinding> CheckToolboxResources()
    {
        var problems = ToolboxRegistry.CheckResources(Services.Environment);

        return problems
            .Select(problem => new OgEnvironmentFinding("Warning", "toolboxes", problem, Path))
            .ToList();
    }
}
