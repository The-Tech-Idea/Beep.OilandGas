using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Core;

namespace TheTechIdea.Beep.OilandGas.Desktop.Security;

/// <summary>The rights an authenticated licence payload states.</summary>
public sealed record OgLicenceClaims(
    string Edition,
    string Customer,
    IReadOnlyList<string> Modules,
    DateTimeOffset? IssuedAt,
    DateTimeOffset? ExpiresAt,
    int GraceDays,
    int? Seats);

/// <summary>
/// What reading a licence file found. <paramref name="SignatureVerified"/> is the difference between "the file
/// says so" and "the vendor signed it"; only the second may grant a module (AC-3).
/// </summary>
public sealed record OgLicenceVerification(
    OgLicenceClaims? Claims,
    bool SignatureVerified,
    string? KeyId,
    string? Code,
    string? Reason)
{
    public bool IsAuthenticated => SignatureVerified && Claims is not null;

    public static OgLicenceVerification Failed(string code, string reason, string? keyId = null) =>
        new(null, false, keyId, code, reason);
}

/// <summary>
/// REL-02: reads a licence envelope and verifies its RSA signature over the payload's exact bytes.
/// </summary>
/// <remarks>
/// <para>
/// The envelope is <c>{ "schema": 2, "keyId": "…", "payload": "&lt;base64url&gt;", "signature": "&lt;base64url&gt;" }</c>.
/// The signature covers the decoded payload bytes verbatim, so the issuer and the verifier never have to agree
/// on a JSON serializer: what was signed is what is checked. A licence with the claims at the root and no
/// signature is reported as <see cref="CodeUnsigned"/> and grants nothing — it is a claim, not a licence.
/// </para>
/// <para>
/// Malformed JSON <b>throws</b>: the caller reports it through the failure reporter, so the parser's words reach
/// the store and the person is shown a reference (BEEP0004). A signature that simply does not match is an
/// ordinary answer and is returned, not thrown.
/// </para>
/// </remarks>
public static class OgLicenceVerifier
{
    public const string CodeUnsigned = "licence-unsigned";
    public const string CodeMalformed = "licence-malformed";
    public const string CodeKeyUnknown = "licence-key-unknown";
    public const string CodeSignatureInvalid = "licence-signature-invalid";
    public const string CodeUnreadable = "licence-unreadable";

    public static OgLicenceVerification Verify(
        string json,
        IReadOnlyDictionary<string, string> trustedPublicKeys)
    {
        ArgumentNullException.ThrowIfNull(json);
        ArgumentNullException.ThrowIfNull(trustedPublicKeys);

        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.ValueKind != JsonValueKind.Object)
        {
            return OgLicenceVerification.Failed(CodeMalformed, "The licence file must be a JSON object.");
        }

        if (!root.TryGetProperty("payload", out var payloadElement)
            || !root.TryGetProperty("signature", out var signatureElement))
        {
            // A legacy or hand-written file: parseable, but nobody vouched for it.
            return OgLicenceVerification.Failed(
                CodeUnsigned,
                "The licence is not signed, so its contents cannot be authenticated.");
        }

        var keyId = root.TryGetProperty("keyId", out var keyElement) && keyElement.ValueKind == JsonValueKind.String
            ? keyElement.GetString()
            : null;

        if (string.IsNullOrWhiteSpace(keyId))
        {
            return OgLicenceVerification.Failed(CodeMalformed, "The licence does not name the key that signed it.");
        }

        if (!trustedPublicKeys.TryGetValue(keyId, out var publicKeyPem) || string.IsNullOrWhiteSpace(publicKeyPem))
        {
            return OgLicenceVerification.Failed(
                CodeKeyUnknown,
                $"The licence was signed with key '{keyId}', which this build does not trust.",
                keyId);
        }

        if (payloadElement.ValueKind != JsonValueKind.String
            || signatureElement.ValueKind != JsonValueKind.String
            || !TryDecode(payloadElement.GetString()!, out var payloadBytes)
            || !TryDecode(signatureElement.GetString()!, out var signature))
        {
            return OgLicenceVerification.Failed(
                CodeMalformed,
                "The licence payload or signature is not valid base64url.",
                keyId);
        }

        if (!PemEncoding.TryFind(publicKeyPem, out _))
        {
            return OgLicenceVerification.Failed(
                CodeKeyUnknown,
                $"The trusted key '{keyId}' is not a usable public key in this build.",
                keyId);
        }

        using var rsa = RSA.Create();
        rsa.ImportFromPem(publicKeyPem);

        if (!rsa.VerifyData(payloadBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
        {
            return OgLicenceVerification.Failed(
                CodeSignatureInvalid,
                "The licence signature does not match its contents; the file has been altered or corrupted.",
                keyId);
        }

        using var payloadDocument = JsonDocument.Parse(payloadBytes);
        var problems = new List<string>();
        var claims = ReadClaims(payloadDocument.RootElement, problems);

        if (claims is null || problems.Count > 0)
        {
            return OgLicenceVerification.Failed(
                CodeMalformed,
                string.Join(" ", problems.Count > 0 ? problems : ["The licence payload has no claims."]),
                keyId);
        }

        return new OgLicenceVerification(claims, SignatureVerified: true, keyId, Code: null, Reason: null);
    }

    /// <summary>
    /// The trusted vendor keys this build ships, by key id.
    /// </summary>
    /// <remarks>
    /// Embedded rather than read from the data root: a key an attacker can edit is not a trust anchor. The
    /// development key is replaced by the vendor key before release (REL-01/REL-05); the private half never
    /// ships with the application.
    /// </remarks>
    public static IReadOnlyDictionary<string, string> Trusted { get; } = LoadTrusted();

    private static IReadOnlyDictionary<string, string> LoadTrusted()
    {
        using var stream = typeof(OgLicenceVerifier).Assembly
            .GetManifestResourceStream("TheTechIdea.Beep.OilandGas.Desktop.Security.licence-keys.json");

        if (stream is null)
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        using var reader = new StreamReader(stream);
        using var document = JsonDocument.Parse(reader.ReadToEnd());

        var keys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Value.ValueKind == JsonValueKind.String)
            {
                keys[property.Name] = property.Value.GetString()!;
            }
        }

        return keys;
    }

    private static OgLicenceClaims? ReadClaims(JsonElement root, List<string> problems)
    {
        if (root.ValueKind != JsonValueKind.Object)
        {
            problems.Add("The licence payload must be a JSON object.");
            return null;
        }

        var edition = Text(root, "edition");
        if (string.IsNullOrWhiteSpace(edition))
        {
            problems.Add("The licence does not name an edition.");
        }

        var customer = Text(root, "customer");
        if (string.IsNullOrWhiteSpace(customer))
        {
            problems.Add("The licence does not name a customer.");
        }

        var modules = ReadModules(root, problems);
        var expiresAt = ReadDate(root, "expiresAt", problems);
        var issuedAt = ReadDate(root, "issuedAt", problems);

        var graceDays = 0;
        if (root.TryGetProperty("graceDays", out var grace) && grace.ValueKind != JsonValueKind.Null)
        {
            if (grace.ValueKind == JsonValueKind.Number && grace.TryGetInt32(out var days) && days is >= 0 and <= 365)
            {
                graceDays = days;
            }
            else
            {
                problems.Add("'graceDays' must be a whole number of days between 0 and 365.");
            }
        }

        int? seats = null;
        if (root.TryGetProperty("seats", out var seatElement) && seatElement.ValueKind != JsonValueKind.Null)
        {
            if (seatElement.ValueKind == JsonValueKind.Number && seatElement.TryGetInt32(out var count) && count > 0)
            {
                seats = count;
            }
            else
            {
                problems.Add("'seats' must be a positive whole number.");
            }
        }

        return new OgLicenceClaims(
            edition ?? "(none)",
            customer ?? "(none)",
            modules,
            issuedAt,
            expiresAt,
            graceDays,
            seats);
    }

    private static IReadOnlyList<string> ReadModules(JsonElement root, List<string> problems)
    {
        if (!root.TryGetProperty("modules", out var element) || element.ValueKind != JsonValueKind.Array)
        {
            problems.Add("The licence has no 'modules' array.");
            return [];
        }

        var modules = new List<string>();

        foreach (var item in element.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String)
            {
                problems.Add("'modules' must contain only strings.");
                continue;
            }

            var key = item.GetString()!.Trim();

            if (string.Equals(key, OgEntitlement.AllModules, StringComparison.Ordinal))
            {
                modules.Add(key);
                continue;
            }

            if (!OgModuleKeys.All.Contains(key, StringComparer.OrdinalIgnoreCase))
            {
                // A typo here would silently withhold a module the customer paid for, so it is a problem.
                problems.Add($"'{key}' is not a known module key.");
                continue;
            }

            modules.Add(key);
        }

        if (modules.Count == 0 && problems.Count == 0)
        {
            problems.Add("The licence grants no modules.");
        }

        return modules;
    }

    private static DateTimeOffset? ReadDate(JsonElement root, string name, List<string> problems)
    {
        if (!root.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (value.ValueKind != JsonValueKind.String || !DateTimeOffset.TryParse(value.GetString(), out var parsed))
        {
            problems.Add($"'{name}' is not a date.");
            return null;
        }

        return parsed;
    }

    private static string? Text(JsonElement root, string name) =>
        root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;

    /// <summary>base64url without exceptions: <c>Convert.TryFromBase64String</c> answers instead of throwing.</summary>
    private static bool TryDecode(string value, out byte[] bytes)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');

        padded = (padded.Length % 4) switch
        {
            2 => padded + "==",
            3 => padded + "=",
            _ => padded,
        };

        var buffer = new byte[padded.Length + 8];

        if (!Convert.TryFromBase64String(padded, buffer, out var written) || written == 0)
        {
            bytes = [];
            return false;
        }

        bytes = buffer[..written];
        return true;
    }
}
