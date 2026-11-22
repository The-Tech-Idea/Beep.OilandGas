using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;

namespace TheTechIdea.Beep.OilandGas.Desktop.Security;

/// <summary>
/// REL-02: the licence in force, its enforcement, and the two operations that change it (import, enforcement).
/// </summary>
/// <remarks>
/// <para>
/// The service is the single reader of the licence file. It re-reads on <see cref="Reload"/>, so an imported
/// licence takes effect immediately (AC-6) — the navigation asks it on every build rather than caching the
/// answer.
/// </para>
/// <para>
/// A file that cannot be parsed is <b>not</b> a crash: the failure is reported, the person is shown a
/// reference, and the installation falls back to <see cref="OgEntitlement.Unlicensed"/>. Corrupt state must
/// never be able to stop the product or silently grant rights (AC-3).
/// </para>
/// </remarks>
public sealed class OgLicenceService : IOgEntitlementService
{
    private readonly string _licencePath;
    private readonly string _settingsPath;
    private readonly IReadOnlyDictionary<string, string> _trustedKeys;
    private readonly IOgAuditService _audit;
    private readonly IFailureReporter _failures;
    private readonly Func<DateTimeOffset> _now;

    private OgEntitlement _current = OgEntitlement.Unlicensed(OgLicenceEnforcement.Permissive);

    public OgLicenceService(
        IOgAuditService audit,
        IFailureReporter failures,
        string? licencePath = null,
        string? settingsPath = null,
        IReadOnlyDictionary<string, string>? trustedKeys = null,
        Func<DateTimeOffset>? now = null)
    {
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _failures = failures ?? throw new ArgumentNullException(nameof(failures));
        _licencePath = licencePath ?? OgPaths.LicencePath;
        _settingsPath = settingsPath ?? OgPaths.LicenceSettingsPath;
        _trustedKeys = trustedKeys ?? OgLicenceVerifier.Trusted;
        _now = now ?? (() => DateTimeOffset.UtcNow);

        Reload();
    }

    /// <summary>Raised whenever the licence or its enforcement changes, so the shell can rebuild itself.</summary>
    public event EventHandler? Changed;

    public OgEntitlement Current => _current;

    public OgLicenceEnforcement Enforcement => _current.Enforcement;

    public string LicencePath => _licencePath;

    public bool Allows(string moduleKey) => _current.Allows(moduleKey);

    public bool AllowsWriting(string moduleKey) => _current.AllowsWriting(moduleKey);

    /// <summary>The signature/verification state of the installed file, for the licence page.</summary>
    public OgLicenceVerification LastVerification { get; private set; } =
        OgLicenceVerification.Failed(OgLicenceVerifier.CodeUnsigned, "No licence has been read.");

    /// <summary>Re-reads the licence and the enforcement setting.</summary>
    public void Reload()
    {
        var enforcement = LoadEnforcement();

        if (!File.Exists(_licencePath))
        {
            LastVerification = OgLicenceVerification.Failed(
                OgLicenceVerifier.CodeUnsigned,
                "No licence file is installed.");

            Set(OgEntitlement.Unlicensed(enforcement));
            return;
        }

        try
        {
            var verification = OgLicenceVerifier.Verify(File.ReadAllText(_licencePath), _trustedKeys);
            LastVerification = verification;

            if (verification.IsAuthenticated && verification.Claims is { } claims)
            {
                Set(OgLicenceState.ToEntitlement(claims, verification.KeyId, enforcement, _now()));
                return;
            }

            // A file that is present but not authenticated is a claim, not a licence (AC-3).
            Set(OgEntitlement.Unlicensed(
                enforcement,
                $"{verification.Reason} ({verification.Code})",
                verification.KeyId));
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or JsonException
                                   or FormatException
                                   or CryptographicException)
        {
            var reference = _failures.ReportHandled(
                ex,
                "reading the installed licence",
                "the licence was treated as absent",
                FailureSeverity.Error);

            LastVerification = OgLicenceVerification.Failed(
                OgLicenceVerifier.CodeUnreadable,
                $"The licence file could not be read. Reference: {reference}");

            Set(OgEntitlement.Unlicensed(
                enforcement,
                $"The licence file could not be read. Reference: {reference}"));
        }
    }

    /// <summary>
    /// Installs a licence file. A file whose signature is not valid is refused and the installed licence is
    /// left exactly as it was, so a bad import cannot degrade a working installation (AC-3, AC-6).
    /// </summary>
    public OgResult Import(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return OgResult.Failed("No licence file was named.", "licence-path-missing");
        }

        if (!File.Exists(path))
        {
            return OgResult.Failed($"The licence file was not found: {path}", OgLicenceVerifier.CodeUnreadable);
        }

        OgLicenceVerification verification;

        try
        {
            verification = OgLicenceVerifier.Verify(File.ReadAllText(path), _trustedKeys);
        }
        catch (Exception ex) when (ex is IOException
                                   or UnauthorizedAccessException
                                   or JsonException
                                   or FormatException
                                   or CryptographicException)
        {
            var reference = _failures.ReportHandled(
                ex,
                $"importing the licence '{Path.GetFileName(path)}'",
                "the licence was not imported",
                FailureSeverity.Error);

            _audit.Add("licence.import", "licence", Path.GetFileName(path), "refused", $"unreadable; reference {reference}");
            return OgResult.Failed($"The licence file could not be read. Reference: {reference}", reference);
        }

        if (!verification.IsAuthenticated)
        {
            _audit.Add(
                "licence.import",
                "licence",
                Path.GetFileName(path),
                "refused",
                $"{verification.Code}: {verification.Reason}");

            return OgResult.Failed(
                $"{verification.Reason} The installed licence is unchanged.",
                verification.Code ?? "licence-refused");
        }

        Directory.CreateDirectory(Path.GetDirectoryName(_licencePath)!);
        File.Copy(path, _licencePath, overwrite: true);

        Reload();

        _audit.Add(
            "licence.import",
            "licence",
            Path.GetFileName(path),
            "ok",
            $"key {verification.KeyId}; {_current.StatusText}");

        Changed?.Invoke(this, EventArgs.Empty);
        return OgResult.Ok(_current.StatusText);
    }

    /// <summary>Switches enforcement. Audited with the previous and the new value (REL-02 §6).</summary>
    public OgResult SetEnforcement(OgLicenceEnforcement enforcement)
    {
        var previous = _current.Enforcement;

        SaveEnforcement(enforcement);
        Reload();

        _audit.Add(
            "licence.enforcement",
            "licence",
            enforcement.ToString(),
            "ok",
            $"previous={previous}; {_current.StatusText}");

        Changed?.Invoke(this, EventArgs.Empty);
        return OgResult.Ok(_current.StatusText);
    }

    private void Set(OgEntitlement entitlement) => _current = entitlement;

    private OgLicenceEnforcement LoadEnforcement()
    {
        if (!File.Exists(_settingsPath))
        {
            return OgLicenceEnforcement.Permissive;
        }

        try
        {
            using var document = JsonDocument.Parse(File.ReadAllText(_settingsPath));

            // Named comparisons only: a value this build does not know falls back to the permissive default
            // rather than to a denial the operator did not ask for.
            var text = document.RootElement.TryGetProperty("enforcement", out var element)
                       && element.ValueKind == JsonValueKind.String
                ? element.GetString()
                : null;

            return string.Equals(text, nameof(OgLicenceEnforcement.Enforced), StringComparison.OrdinalIgnoreCase)
                ? OgLicenceEnforcement.Enforced
                : OgLicenceEnforcement.Permissive;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            _failures.ReportHandled(
                ex,
                "reading the licence enforcement setting",
                "enforcement stays off",
                FailureSeverity.Degraded);

            return OgLicenceEnforcement.Permissive;
        }
    }

    private void SaveEnforcement(OgLicenceEnforcement enforcement)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

        var json = new StringBuilder()
            .Append("{\"enforcement\":\"")
            .Append(enforcement.ToString())
            .Append("\"}")
            .ToString();

        File.WriteAllText(_settingsPath, json);
    }
}
