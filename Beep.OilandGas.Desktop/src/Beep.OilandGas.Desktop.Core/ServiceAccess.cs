namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>How the installation reaches its data.</summary>
/// <remarks>
/// <see cref="Auto"/> is a preference, not a mode: it is resolved to <see cref="Local"/> or
/// <see cref="Remote"/> at the moment it is used, so a laptop that is offline in the morning and on the
/// company network in the afternoon does not need a setting changed.
/// </remarks>
public enum OgAccessPreference
{
    Auto,
    Local,
    Remote,
}

/// <summary>The mode actually in force after resolution.</summary>
public enum OgResolvedAccess
{
    /// <summary>Neither a usable API nor a usable local database: business pages must refuse.</summary>
    NotConfigured,

    Local,

    Remote,
}

/// <summary>What a health check found.</summary>
public enum OgAccessHealth
{
    Ok,

    /// <summary>The API could not be reached at all (DNS, refused, timeout).</summary>
    Unreachable,

    /// <summary>The API answered but refused this caller (401/403, expired token).</summary>
    Unauthorized,

    /// <summary>The local database file or connection is missing.</summary>
    DatabaseMissing,

    /// <summary>There is nothing configured to check.</summary>
    NotConfigured,
}

/// <summary>
/// The facts resolution needs. Passed in rather than read from the environment, so the resolution rule can
/// be tested for every combination without a network, a database or a signed-in user.
/// </summary>
public sealed record OgServiceAccessSettings(
    OgAccessPreference Preference,
    string? CompanyApiBaseUrl,
    bool HasUsableToken,
    string? LocalConnectionName,
    bool LocalDatabaseExists)
{
    public bool HasApi => !string.IsNullOrWhiteSpace(CompanyApiBaseUrl) && HasUsableToken;

    public bool HasLocal => !string.IsNullOrWhiteSpace(LocalConnectionName) && LocalDatabaseExists;
}

/// <summary>The mode and the sentence explaining it, which the shell and the report both show.</summary>
public sealed record OgServiceAccessResolution(OgResolvedAccess Mode, string Reason);

/// <summary>One health reading, with the reason a person needs when it is not Ok.</summary>
public sealed record OgHealthSnapshot(OgResolvedAccess Mode, OgAccessHealth State, string Detail);

/// <summary>
/// Resolves the access mode and reports its health. The shell shows the mode; a business page asks the
/// mode before it reads, so "not configured" is a state the person sees rather than an empty list.
/// </summary>
public interface IOgServiceAccess
{
    OgServiceAccessSettings Settings { get; }

    /// <summary>The mode in force right now, resolved from <see cref="Settings"/>.</summary>
    OgResolvedAccess Mode { get; }

    /// <summary>The explanation for <see cref="Mode"/>.</summary>
    string Reason { get; }

    /// <summary>Changes the settings and audits the change with the previous and the new mode.</summary>
    Task<OgResult> ConfigureAsync(OgServiceAccessSettings settings, CancellationToken cancellationToken = default);

    Task<OgResult<OgHealthSnapshot>> CheckHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// The one resolution rule. Kept pure so it can be exhaustively tested, and so the UI and the headless
/// report cannot disagree about which mode is in force.
/// </summary>
public static class OgAccessResolver
{
    public static OgServiceAccessResolution Resolve(OgServiceAccessSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.Preference switch
        {
            // FND-02 AC-1: Remote only when the base URL AND a usable token are both present.
            OgAccessPreference.Remote => settings.HasApi
                ? new OgServiceAccessResolution(OgResolvedAccess.Remote, "Remote was chosen: the API base URL and token are usable.")
                : new OgServiceAccessResolution(
                    OgResolvedAccess.NotConfigured,
                    "Remote was requested but the API base URL or the token is missing."),

            // FND-02 AC-2: Local when a local connection exists and its database is present.
            OgAccessPreference.Local => settings.HasLocal
                ? new OgServiceAccessResolution(OgResolvedAccess.Local, "Local was chosen: a local connection and its database are present.")
                : new OgServiceAccessResolution(
                    OgResolvedAccess.NotConfigured,
                    "Local was requested but no local connection with a database is configured."),

            OgAccessPreference.Auto => Auto(settings),
        };
    }

    private static OgServiceAccessResolution Auto(OgServiceAccessSettings settings)
    {
        // Order is the plan's: API first, then local, then refuse.
        if (settings.HasApi)
        {
            return new OgServiceAccessResolution(
                OgResolvedAccess.Remote,
                "Auto chose Remote: the API base URL and token are usable.");
        }

        if (settings.HasLocal)
        {
            return new OgServiceAccessResolution(
                OgResolvedAccess.Local,
                "Auto chose Local: no API is configured and a local database is present.");
        }

        return new OgServiceAccessResolution(
            OgResolvedAccess.NotConfigured,
            "Neither an API nor a local database is configured; business pages will refuse.");
    }
}
