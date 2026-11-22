using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;
using TheTechIdea.Beep.OilandGas.Desktop.Core;
using TheTechIdea.Beep.OilandGas.Desktop.Security;
using TheTechIdea.Beep.OilandGas.Desktop.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-02: a signed licence, an altered one, expiry and grace, the two gates, and an import that takes effect
/// without a restart.
/// </summary>
public sealed class LicensingTests
{
    private const string KeyId = TestLicences.KeyId;

    [Fact]
    public void A_signed_licence_is_authenticated_and_reports_its_terms_exactly()
    {
        var json = TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z"));

        var verification = OgLicenceVerifier.Verify(json, OgLicenceVerifier.Trusted);

        // AC-4: the report repeats the file, it does not summarise or round it.
        Assert.True(verification.SignatureVerified);
        Assert.Equal(KeyId, verification.KeyId);
        Assert.NotNull(verification.Claims);
        Assert.Equal("Professional", verification.Claims!.Edition);
        Assert.Equal("Probe Energy", verification.Claims.Customer);
        Assert.Equal(["core", "data", "wells", "administration"], verification.Claims.Modules);
        Assert.Equal(DateTimeOffset.Parse("2030-12-31T00:00:00Z"), verification.Claims.ExpiresAt);
    }

    [Fact]
    public void An_altered_payload_is_treated_as_unlicensed_with_a_stated_reason()
    {
        // Sign one customer, then present another: the bytes no longer match the signature.
        var signed = TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z"));
        var altered = TestLicences.Signed(TestLicences.Claim("Someone Else", "2030-12-31T00:00:00Z"));
        var forged = ReplaceProperty(signed, "signature", Property(altered, "signature"));

        var verification = OgLicenceVerifier.Verify(forged, OgLicenceVerifier.Trusted);

        // AC-3: not an exception, not a licence — a refusal with the code.
        Assert.False(verification.IsAuthenticated);
        Assert.Equal(OgLicenceVerifier.CodeSignatureInvalid, verification.Code);

        var service = NewService(licenceJson: forged);
        Assert.False(service.Current.IsLicensed);
        Assert.Contains(OgLicenceVerifier.CodeSignatureInvalid, service.Current.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void A_licence_signed_by_a_key_this_build_does_not_trust_is_refused()
    {
        using var stranger = RSA.Create(2048);

        var verification = OgLicenceVerifier.Verify(
            TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z"), stranger, keyId: "not-ours"),
            OgLicenceVerifier.Trusted);

        Assert.False(verification.IsAuthenticated);
        Assert.Equal(OgLicenceVerifier.CodeKeyUnknown, verification.Code);
        Assert.Contains("not-ours", verification.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void An_unsigned_licence_grants_nothing()
    {
        // The root-claim shape the first licence checker accepted: readable, but nobody vouched for it.
        var unsigned = """
                       { "edition": "Professional", "customer": "Probe Energy", "modules": ["wells"] }
                       """;

        var verification = OgLicenceVerifier.Verify(unsigned, OgLicenceVerifier.Trusted);

        Assert.False(verification.IsAuthenticated);
        Assert.Equal(OgLicenceVerifier.CodeUnsigned, verification.Code);

        var service = NewService(licenceJson: unsigned);
        Assert.False(service.Current.IsLicensed);
    }

    [Fact]
    public void An_expired_licence_past_grace_allows_reads_and_blocks_writes_when_enforced()
    {
        var now = new DateTimeOffset(2031, 3, 1, 0, 0, 0, TimeSpan.Zero);

        var service = NewService(
            now,
            TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z", graceDays: 14)));

        service.SetEnforcement(OgLicenceEnforcement.Enforced);

        Assert.True(service.Current.IsLicensed);
        Assert.True(service.Current.IsExpired);
        Assert.False(service.Current.IsInGrace);
        Assert.False(service.Current.IsUsable);

        // AC-2: the customer keeps reading their own records; writing is what stops.
        Assert.True(service.Allows(OgModuleKeys.Wells));
        Assert.False(service.AllowsWriting(OgModuleKeys.Wells));

        // Core is never blocked: licensing must not break the product's own housekeeping.
        Assert.True(service.AllowsWriting(OgModuleKeys.Core));
    }

    [Fact]
    public void Inside_grace_everything_works_and_the_days_are_counted()
    {
        // Expired 2030-12-31 with 14 grace days: 2031-01-05 still has 9 days to run.
        var now = new DateTimeOffset(2031, 1, 5, 0, 0, 0, TimeSpan.Zero);

        var service = NewService(
            now,
            TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z", graceDays: 14)));

        service.SetEnforcement(OgLicenceEnforcement.Enforced);

        Assert.True(service.Current.IsExpired);
        Assert.True(service.Current.IsInGrace);
        Assert.True(service.Current.IsUsable);
        Assert.Equal(9, service.Current.DaysRemaining);
        Assert.True(service.AllowsWriting(OgModuleKeys.Wells));
        Assert.Contains("grace", service.Current.StatusText, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void With_no_licence_permissive_allows_everything_and_enforced_gives_core_only()
    {
        var permissive = NewService();
        Assert.False(permissive.Current.IsLicensed);
        Assert.True(permissive.Allows(OgModuleKeys.Wells));
        Assert.Contains("Enforcement is off", permissive.Current.StatusText, StringComparison.Ordinal);

        permissive.SetEnforcement(OgLicenceEnforcement.Enforced);

        // AC-1's second gate: without an entitlement no commercial module passes, whatever the role.
        Assert.False(permissive.Allows(OgModuleKeys.Wells));
        Assert.True(permissive.Allows(OgModuleKeys.Core));
        Assert.False(permissive.AllowsWriting(OgModuleKeys.Wells));
    }

    [Fact]
    public void An_entitled_module_still_needs_the_role()
    {
        var service = NewService(
            licenceJson: TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z")));

        var navigation = new OgNavigationService(service, new StubLoc());

        var entitledButNoRole = new OgPermissions(["Viewer"], [], permissionSourceAvailable: true);
        var entitledWithRole = new OgPermissions(
            [OgRoleNames.SystemAdmin],
            [],
            permissionSourceAvailable: true);

        // AC-1: the entitlement passes both times; the role decides.
        Assert.True(service.Allows(OgModuleKeys.Administration));
        Assert.False(navigation.CanAccess("licence", entitledButNoRole));
        Assert.True(navigation.CanAccess("licence", entitledWithRole));
    }

    [Fact]
    public void A_module_the_licence_does_not_grant_is_denied_even_for_an_administrator()
    {
        // A licence granting only core + wells, with enforcement on.
        var service = NewService(
            licenceJson: TestLicences.Signed(TestLicences.Claim("Probe Energy", "2030-12-31T00:00:00Z", modules: ["core", "wells"])));

        service.SetEnforcement(OgLicenceEnforcement.Enforced);

        var navigation = new OgNavigationService(service, new StubLoc());
        var admin = new OgPermissions([OgRoleNames.SystemAdmin], [], permissionSourceAvailable: true);

        Assert.True(navigation.CanAccess("wells", admin));
        Assert.False(navigation.CanAccess("settings", admin));
    }

    [Fact]
    public void Importing_a_valid_licence_takes_effect_without_a_restart()
    {
        var service = NewService();
        Assert.False(service.Current.IsLicensed);

        var file = TestLicences.Write(TestLicences.Signed(TestLicences.Claim("Second Customer", "2031-06-30T00:00:00Z")));

        Assert.True(service.Import(file).Success);

        // AC-6: the same instance now answers from the new file.
        Assert.True(service.Current.IsLicensed);
        Assert.Equal("Second Customer", service.Current.Customer);
        Assert.True(service.AllowsWriting(OgModuleKeys.Wells));
    }

    [Fact]
    public void Importing_a_tampered_licence_is_refused_and_leaves_the_installed_one_intact()
    {
        var service = NewService(
            licenceJson: TestLicences.Signed(TestLicences.Claim("Original Customer", "2030-12-31T00:00:00Z")));

        Assert.Equal("Original Customer", service.Current.Customer);

        var signed = TestLicences.Signed(TestLicences.Claim("Attacker", "2030-12-31T00:00:00Z"));
        var altered = TestLicences.Signed(TestLicences.Claim("Attacker Two", "2030-12-31T00:00:00Z"));
        var forged = ReplaceProperty(signed, "signature", Property(altered, "signature"));
        var file = TestLicences.Write(forged);

        var imported = service.Import(file);

        Assert.False(imported.Success);
        Assert.Equal(OgLicenceVerifier.CodeSignatureInvalid, imported.Reference);

        // A bad import must never degrade a working installation.
        Assert.Equal("Original Customer", service.Current.Customer);
        Assert.True(service.Current.IsLicensed);
    }

    [Fact]
    public void The_sample_licence_shipped_with_the_application_verifies()
    {
        var path = Path.Combine(
            AppContext.BaseDirectory,
            "Security",
            "Samples",
            "sample-licence.json");

        var verification = OgLicenceVerifier.Verify(File.ReadAllText(path), OgLicenceVerifier.Trusted);

        Assert.True(verification.SignatureVerified, verification.Reason);
        Assert.Equal("Probe Energy", verification.Claims!.Customer);
        Assert.Contains(OgModuleKeys.Wells, verification.Claims.Modules);
    }

    [Fact]
    public void A_licence_carries_no_secret_and_the_trusted_keys_are_public_only()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Security", "Samples", "sample-licence.json");
        var text = File.ReadAllText(path);

        // AC-5: the format has no field for a secret, and none was smuggled in.
        using var document = JsonDocument.Parse(text);
        var names = document.RootElement.EnumerateObject().Select(property => property.Name).ToArray();
        Assert.Equal(["schema", "keyId", "payload", "signature"], names);
        Assert.DoesNotContain("PRIVATE KEY", text, StringComparison.Ordinal);
        Assert.DoesNotContain("password", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("secret", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("token", text, StringComparison.OrdinalIgnoreCase);

        // The embedded trust anchors are public keys; the private half is not shipped.
        Assert.NotEmpty(OgLicenceVerifier.Trusted);
        Assert.All(OgLicenceVerifier.Trusted.Values, pem =>
        {
            Assert.Contains("BEGIN PUBLIC KEY", pem, StringComparison.Ordinal);
            Assert.DoesNotContain("PRIVATE", pem, StringComparison.Ordinal);
        });
    }

    // ---------------------------------------------------------------- helpers

    private static string Property(string json, string name)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty(name).GetString()!;
    }

    private static string ReplaceProperty(string json, string name, string value)
    {
        using var document = JsonDocument.Parse(json);
        var properties = document.RootElement.EnumerateObject()
            .Select(property => property.Name == name
                ? $"\"{name}\":\"{value}\""
                : $"\"{property.Name}\":{property.Value.GetRawText()}");

        return "{" + string.Join(',', properties) + "}";
    }


    private static OgLicenceService NewService(
        DateTimeOffset? now = null,
        string? licenceJson = null)
    {
        var directory = Path.Combine(OgPaths.DataRoot, "licence-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);

        var licencePath = Path.Combine(directory, "licence.json");

        if (licenceJson is not null)
        {
            File.WriteAllText(licencePath, licenceJson);
        }

        return new OgLicenceService(
            new QuietAudit(),
            new QuietFailures(),
            licencePath,
            Path.Combine(directory, "licence.settings.json"),
            OgLicenceVerifier.Trusted,
            now is { } instant ? () => instant : null);
    }

    private sealed class QuietAudit : IOgAuditService
    {
        public void Add(
            string verb,
            string subjectKind,
            string subjectId,
            string outcome,
            string? detail = null,
            string? fieldId = null)
        {
        }
    }

    private sealed class QuietFailures : IFailureReporter
    {
        public string ReportHandled(
            Exception exception,
            string operation,
            string consequence,
            FailureSeverity severity = FailureSeverity.Error) =>
            FailureReferenceFormat.New();
    }

    private sealed class StubLoc : ILocProvider
    {
        public string CultureName => "en";

        public bool IsRightToLeft => false;

        public IReadOnlyList<OgCultureOption> Cultures => [];

        public event EventHandler? LanguageChanged
        {
            add { }
            remove { }
        }

        public void SetCulture(string cultureName)
        {
        }

        public string Get(string key, params object?[] args) => key;
    }
}
