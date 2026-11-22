using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Security;

/// <summary>The outcome of reading a licence file.</summary>
/// <param name="IsUsable">True when the signature verified, the claims are complete and the licence is not expired.</param>
/// <param name="Edition">The edition the file names, when present.</param>
/// <param name="Customer">The customer the file names, when present.</param>
/// <param name="Modules">The module keys the file grants.</param>
/// <param name="ExpiresAt">The expiry the file names, when present.</param>
/// <param name="Problems">Every reason the file is not usable, in the order they were found.</param>
/// <param name="SignatureVerified">True when the vendor's signature was checked and matched.</param>
/// <param name="KeyId">The trusted key that verified the file, when one did.</param>
/// <param name="Code">The stable code for a refusal, or null when the file is usable.</param>
public sealed record OgLicenceCheckResult(
    bool IsUsable,
    string? Edition,
    string? Customer,
    IReadOnlyList<string> Modules,
    DateTimeOffset? ExpiresAt,
    IReadOnlyList<string> Problems,
    bool SignatureVerified,
    string? KeyId,
    string? Code);

/// <summary>
/// Reads and <b>verifies</b> a licence file (REL-02).
/// </summary>
/// <remarks>
/// <para>
/// The signature is RSA-2048 / SHA-256 over the payload's exact bytes, checked against a key embedded in this
/// build. A file without a signature is reported as unauthenticated and is not usable — a licence nobody
/// signed is a claim, and treating a claim as a licence would be worse than refusing it.
/// </para>
/// <para>
/// An unreadable file or invalid JSON <b>throws</b> on purpose: the caller reports it through the failure
/// reporter, so the parser's words reach the store and the person is shown a reference (BEEP0004). A
/// signature that does not match is an ordinary answer and returns a refusal instead.
/// </para>
/// </remarks>
public static class OgLicenceFile
{
    /// <summary>The verification policy, stated wherever a licence is reported.</summary>
    public const string SignaturePolicy = "RSA-2048 / SHA-256 over the payload, embedded vendor key";

    public static OgLicenceCheckResult Read(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return Failed("No licence file was named.", OgLicenceVerifier.CodeUnreadable);
        }

        if (!File.Exists(path))
        {
            return Failed($"The licence file was not found: {path}", OgLicenceVerifier.CodeUnreadable);
        }

        var json = File.ReadAllText(path);
        var verification = OgLicenceVerifier.Verify(json, OgLicenceVerifier.Trusted);
        var claims = verification.Claims;

        if (!verification.IsAuthenticated || claims is null)
        {
            return new OgLicenceCheckResult(
                IsUsable: false,
                Edition: claims?.Edition,
                Customer: claims?.Customer,
                Modules: claims?.Modules ?? [],
                ExpiresAt: claims?.ExpiresAt,
                Problems: [verification.Reason ?? "The licence is not authenticated."],
                SignatureVerified: false,
                KeyId: verification.KeyId,
                Code: verification.Code);
        }

        var problems = new List<string>();

        if (claims.ExpiresAt is { } expiry && expiry < DateTimeOffset.UtcNow)
        {
            var graceEnds = expiry.AddDays(claims.GraceDays);

            problems.Add(graceEnds < DateTimeOffset.UtcNow
                ? $"The licence expired on {expiry:yyyy-MM-dd} and its grace period ended on {graceEnds:yyyy-MM-dd}."
                : $"The licence expired on {expiry:yyyy-MM-dd}; {graceEnds:yyyy-MM-dd} is the last grace day.");
        }

        return new OgLicenceCheckResult(
            IsUsable: problems.Count == 0,
            Edition: claims.Edition,
            Customer: claims.Customer,
            Modules: claims.Modules,
            ExpiresAt: claims.ExpiresAt,
            Problems: problems,
            SignatureVerified: true,
            KeyId: verification.KeyId,
            Code: problems.Count == 0 ? null : "licence-expired");
    }

    private static OgLicenceCheckResult Failed(string problem, string code) =>
        new(false, null, null, [], null, [problem], SignatureVerified: false, KeyId: null, Code: code);
}
