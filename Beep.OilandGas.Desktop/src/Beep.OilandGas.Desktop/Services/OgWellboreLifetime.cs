using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.WellTools.Environment;

namespace TheTechIdea.Beep.OilandGas.Desktop.Services;

/// <summary>
/// Creates the wellbore environment on first use and keeps it for the process lifetime.
/// </summary>
/// <remarks>
/// Lazy for the same reason as <see cref="OgBeepLifetime"/>: startup must not fail because a customer
/// environment file is missing or malformed, and the first visual screen should pay the cost. The path
/// comes from <see cref="OgPaths"/> here, never from the data layer.
/// </remarks>
public sealed class OgWellboreLifetime
{
    private readonly Lazy<OgWellboreEnvironment> _environment;

    public OgWellboreLifetime()
    {
        _environment = new Lazy<OgWellboreEnvironment>(Create);
    }

    /// <summary>Raised after a reload or a preset change, so open views can be rebuilt.</summary>
    public event EventHandler? EnvironmentChanged;

    /// <summary>What the last reload did, for the settings page.</summary>
    public string? LastChangeSummary { get; private set; }

    public OgWellboreEnvironment Environment => _environment.Value;

    public bool IsCreated => _environment.IsValueCreated;

    /// <summary>
    /// Re-reads the configured environment file (defaults when there is none). This is the host's policy
    /// for where the file lives, so the page does not have to know.
    /// </summary>
    public OgResult Reload()
    {
        var path = OgPaths.WellboreEnvironmentPath;
        var result = Environment.Load(File.Exists(path) ? path : null);
        Announce(Describe(result, path));
        return result;
    }

    /// <summary>Applies one of the built-in country presets and announces the change.</summary>
    public OgResult ApplyCountryPreset(string countryCode)
    {
        var result = Environment.LoadCountryPreset(countryCode);
        Announce(Describe(result, path: null));
        return result;
    }

    /// <summary>
    /// Loads the environment every user of the company database shares, instead of this workstation's file.
    /// The document stays authoritative in the database; nothing is written here.
    /// </summary>
    public OgResult LoadSharedDocument(string? documentJson)
    {
        var result = Environment.LoadSharedDocument(documentJson, baseDirectory: null);
        Announce(Describe(result, path: null));
        return result;
    }

    private void Announce(string summary)
    {
        LastChangeSummary = summary;
        EnvironmentChanged?.Invoke(this, EventArgs.Empty);
    }

    private string Describe(OgResult result, string? path)
    {
        if (!result.Success)
        {
            return $"reload failed: {result.Message}";
        }

        var source = Environment.IsUsingCountryPreset
            ? "country preset"
            : Environment.IsFromSharedSettings
                ? "shared company settings"
                : path is null
                    ? "built-in defaults"
                    : path;

        return $"{source} · {Environment.Services.Warnings.Count} warning(s) · {Environment.Findings.Count} finding(s)";
    }

    private static OgWellboreEnvironment Create()
    {
        var environment = new OgWellboreEnvironment();
        var path = OgPaths.WellboreEnvironmentPath;

        // Defaults when the customer has no file yet; a present file is loaded and its warnings kept.
        environment.Load(File.Exists(path) ? path : null);
        return environment;
    }
}
