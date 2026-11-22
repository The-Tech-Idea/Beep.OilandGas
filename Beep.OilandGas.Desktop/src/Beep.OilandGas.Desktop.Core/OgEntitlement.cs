namespace TheTechIdea.Beep.OilandGas.Desktop.Core;

/// <summary>Whether a licence problem stops the product or is only reported (REL-02 §9).</summary>
/// <remarks>
/// Commercial terms are undecided, so enforcement is a setting rather than a compile-time constant: an
/// installation can run permissively while the business decides, and switch to enforced without a rebuild.
/// </remarks>
public enum OgLicenceEnforcement
{
    /// <summary>Everything works; the licence state is still shown and audited.</summary>
    Permissive,

    /// <summary>An unusable licence withholds the modules it does not grant and blocks writes after expiry.</summary>
    Enforced,
}

/// <summary>
/// The commercial rights in force, as the licence file states them (REL-02).
/// </summary>
/// <param name="Edition">The edition the licence names, or a placeholder when there is none.</param>
/// <param name="Customer">The customer the licence names, or a placeholder when there is none.</param>
/// <param name="Modules">The module keys granted. <c>*</c> grants every module.</param>
/// <param name="ExpiresAt">The expiry the licence names, when it names one.</param>
/// <param name="IsLicensed">True when an <b>authenticated</b> licence was read.</param>
/// <param name="IsExpired">True when the expiry has passed, whether or not the grace period still applies.</param>
/// <param name="IsInGrace">True while the grace period after expiry still gives full use.</param>
/// <param name="IsUsable">True when the licence is authenticated and either current or inside its grace.</param>
/// <param name="StatusText">One sentence for the shell, the report and the licence page.</param>
/// <param name="Error">Why the licence is not usable; null when it is.</param>
/// <param name="SignatureVerified">True when a signature was checked and matched.</param>
/// <param name="KeyId">The trusted key that verified the file, when one did.</param>
/// <param name="DaysRemaining">Whole days until expiry (negative after it), when there is an expiry.</param>
/// <param name="Seats">The seat count the licence names, when it names one.</param>
public sealed record OgEntitlement(
    string Edition,
    string Customer,
    IReadOnlyList<string> Modules,
    DateTimeOffset? ExpiresAt,
    bool IsLicensed,
    bool IsExpired,
    bool IsInGrace,
    bool IsUsable,
    string StatusText,
    string? Error,
    OgLicenceEnforcement Enforcement,
    bool SignatureVerified,
    string? KeyId,
    int? DaysRemaining,
    int? Seats)
{
    /// <summary>Core is never withheld: licensing must not be able to break the product's own housekeeping.</summary>
    public const string AllModules = "*";

    /// <summary>What an installation with no authenticated licence has.</summary>
    public static OgEntitlement Unlicensed(
        OgLicenceEnforcement enforcement,
        string? error = null,
        string? keyId = null) =>
        new(
            Edition: "(none)",
            Customer: "(none)",
            Modules: [OgModuleKeys.Core],
            ExpiresAt: null,
            IsLicensed: false,
            IsExpired: false,
            IsInGrace: false,
            IsUsable: false,
            StatusText: enforcement == OgLicenceEnforcement.Permissive
                ? "No licence is installed. Enforcement is off, so every module is available."
                : "No licence is installed. Only the core modules are available.",
            Error: error ?? "No licence is installed.",
            Enforcement: enforcement,
            SignatureVerified: false,
            KeyId: keyId,
            DaysRemaining: null,
            Seats: null);

    /// <summary>The modules a licence problem affects, for the shell banner.</summary>
    public IReadOnlyList<string> AffectedModules =>
        IsLicensed
            ? Modules.Where(module => !string.Equals(module, OgModuleKeys.Core, StringComparison.OrdinalIgnoreCase)).ToList()
            : OgModuleKeys.All.Where(module => !string.Equals(module, OgModuleKeys.Core, StringComparison.OrdinalIgnoreCase)).ToList();

    /// <summary>
    /// Whether the module may be <b>read</b>. Core always passes. An unauthenticated licence passes only when
    /// enforcement is off; an expired licence still lets the granted modules be read, because losing access to
    /// the customer's own records would be a worse failure than the licence (AC-2).
    /// </summary>
    public bool Allows(string moduleKey)
    {
        if (IsCore(moduleKey))
        {
            return true;
        }

        if (!IsLicensed)
        {
            return Enforcement == OgLicenceEnforcement.Permissive;
        }

        return Grants(moduleKey);
    }

    /// <summary>
    /// Whether the module may be <b>written</b>. An expired licence past its grace blocks writes when
    /// enforcement is on, while reads continue (AC-2). Core is never blocked.
    /// </summary>
    public bool AllowsWriting(string moduleKey)
    {
        if (IsCore(moduleKey))
        {
            return true;
        }

        if (!IsLicensed)
        {
            return Enforcement == OgLicenceEnforcement.Permissive;
        }

        return IsUsable
            ? Grants(moduleKey)
            : Enforcement == OgLicenceEnforcement.Permissive;
    }

    public bool Grants(string moduleKey)
    {
        if (IsCore(moduleKey))
        {
            return true;
        }

        return Modules.Any(module =>
            string.Equals(module, AllModules, StringComparison.Ordinal)
            || string.Equals(module, moduleKey, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsCore(string moduleKey) =>
        string.Equals(moduleKey, OgModuleKeys.Core, StringComparison.OrdinalIgnoreCase);
}
