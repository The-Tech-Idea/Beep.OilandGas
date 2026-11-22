using TheTechIdea.OilGas.Wellbore.Data;

namespace TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;

/// <summary>
/// The environment's findings for the diagnostics and settings screens: what was loaded, which toolboxes
/// are registered, and every warning the environment produced.
/// </summary>
/// <remarks>
/// It reads, it does not repair. A missing resource or an invalid toolboxes section is reported so the
/// operator can fix the file; the desktop never rewrites a customer's environment silently.
/// </remarks>
public sealed class OgEnvironmentStore
{
    private readonly OgWellboreEnvironment _environment;

    public OgEnvironmentStore(OgWellboreEnvironment environment)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    public OgEnvironmentSnapshot Current()
    {
        var services = _environment.Services;

        return new OgEnvironmentSnapshot(
            Path: _environment.Path,
            IsLoaded: _environment.IsLoaded,
            UsingDefaults: _environment.Path is null && !_environment.IsUsingCountryPreset && !_environment.IsFromSharedSettings,
            UsingCountryPreset: _environment.IsUsingCountryPreset,
            CountryCode: services.Environment.Region.Country ?? string.Empty,
            RegionName: services.Environment.Region.Name ?? string.Empty,
            UnitCount: services.Environment.Units.Definitions.Count,
            RegisteredToolboxes: OgToolboxBootstrap.RegisteredNames,
            MissingToolboxes: OgToolboxBootstrap.Missing(),
            Findings: _environment.Findings,
            UsingSharedSettings: _environment.IsFromSharedSettings);
    }

    /// <summary>Runs the disk-touching resource check for the registered toolboxes.</summary>
    public IReadOnlyList<OgEnvironmentFinding> CheckResources() => _environment.CheckToolboxResources();

    /// <summary>
    /// Which registered toolboxes have a resource problem, keyed by toolbox name. A toolbox with an empty
    /// list is usable; one with problems is reported as unusable with the reasons, so a screen that depends
    /// on it can refuse to open instead of failing half-way.
    /// </summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> ToolboxAvailability()
    {
        var availability = new Dictionary<string, IReadOnlyList<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var name in RegisteredToolboxes)
        {
            availability[name] = [];
        }

        foreach (var finding in _environment.CheckToolboxResources())
        {
            // CheckResources prefixes each message with "toolboxes.<name>: ".
            const string prefix = "toolboxes.";
            if (!finding.Message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var separator = finding.Message.IndexOf(':', prefix.Length);
            if (separator <= prefix.Length)
            {
                continue;
            }

            var name = finding.Message[prefix.Length..separator].Trim();
            var reason = finding.Message[(separator + 1)..].Trim();

            var existing = availability.TryGetValue(name, out var list) ? list : [];
            availability[name] = [.. existing, reason];
        }

        return availability;
    }

    /// <summary>True when the toolbox has no resource problem; unknown toolboxes are treated as usable.</summary>
    public bool IsToolboxUsable(string toolboxName) =>
        !ToolboxAvailability().TryGetValue(toolboxName, out var problems) || problems.Count == 0;

    /// <summary>The registered toolbox names — the fact that proves registration ran before a load.</summary>
    public static IReadOnlyList<string> RegisteredToolboxes => OgToolboxBootstrap.RegisteredNames;
}

/// <summary>What the settings and diagnostics pages show about the wellbore environment.</summary>
public sealed record OgEnvironmentSnapshot(
    string? Path,
    bool IsLoaded,
    bool UsingDefaults,
    bool UsingCountryPreset,
    string CountryCode,
    string RegionName,
    int UnitCount,
    IReadOnlyList<string> RegisteredToolboxes,
    IReadOnlyList<string> MissingToolboxes,
    IReadOnlyList<OgEnvironmentFinding> Findings,
    bool UsingSharedSettings = false);
