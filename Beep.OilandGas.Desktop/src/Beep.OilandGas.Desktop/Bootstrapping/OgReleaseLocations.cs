namespace TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

/// <summary>
/// Where the release checklist's own artefacts are (REL-05): the source checkout a release is cut from, and its plan folder.
/// </summary>
/// <remarks>
/// A release build is made from a source checkout, so the checks that read project and screen sources need that checkout. An installed
/// copy has none, and the checklist then reports those items as not run rather than guessing - which is the point of the feature.
/// </remarks>
public static class OgReleaseLocations
{
    /// <summary>
    /// The desktop solution's root: the nearest folder above the executable that holds both <c>src</c> and <c>plan</c>, or null when this
    /// build is running from an installed copy.
    /// </summary>
    public static string? SourceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "src")) &&
                Directory.Exists(Path.Combine(directory.FullName, "plan")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    /// <summary>The plan folder of that checkout, or null when there is none.</summary>
    public static string? PlanFolder() =>
        SourceRoot() is { } root ? Path.Combine(root, "plan") : null;

    /// <summary>REL-05: the attestations and waivers, under the data root so an installed copy can record them.</summary>
    public static string AcceptanceStorePath => Path.Combine(OgPaths.DiagnosticsDirectory, "release-acceptance.json");

    /// <summary>REL-05's evidence pack, beside the failure store rather than beside the executable.</summary>
    public static string DefaultEvidencePackPath =>
        Path.Combine(OgPaths.ExportsDirectory,
            "release-evidence-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture) + ".zip");
}
