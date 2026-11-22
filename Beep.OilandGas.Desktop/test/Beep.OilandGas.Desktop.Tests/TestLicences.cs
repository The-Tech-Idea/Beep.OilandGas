using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TheTechIdea.Beep.OilandGas.Desktop.Bootstrapping;

namespace TheTechIdea.Beep.OilandGas.Desktop.Tests;

/// <summary>
/// REL-02 test licences: the same envelope the vendor would ship, signed with the development key whose public
/// half is embedded in the application.
/// </summary>
/// <remarks>
/// The keys here are the development pair only. The private half lives in the test output folder and is never
/// part of the application's output; REL-01/REL-05 owns production key custody.
/// </remarks>
internal static class TestLicences
{
    public const string KeyId = "dev-2026";

    public static string Claim(
        string customer = "Probe Energy",
        string expiresAt = "2999-01-01T00:00:00Z",
        int graceDays = 14,
        IReadOnlyList<string>? modules = null)
    {
        var granted = modules ?? ["core", "data", "wells", "administration"];
        var list = string.Join(',', granted.Select(module => $"\"{module}\""));

        return $$"""
                 {"schema":2,"edition":"Professional","customer":"{{customer}}","modules":[{{list}}],"issuedAt":"2026-01-01T00:00:00Z","expiresAt":"{{expiresAt}}","graceDays":{{graceDays}},"seats":25}
                 """;
    }

    public static string Signed(string payloadJson, RSA? key = null, string keyId = KeyId)
    {
        var owned = key is null;
        var rsa = key ?? TestKey();

        try
        {
            var payload = Encoding.UTF8.GetBytes(payloadJson);
            var signature = rsa.SignData(payload, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            return JsonSerializer.Serialize(new
            {
                schema = 2,
                keyId,
                payload = Base64Url(payload),
                signature = Base64Url(signature),
            });
        }
        finally
        {
            if (owned)
            {
                rsa.Dispose();
            }
        }
    }

    public static RSA TestKey()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Fixtures", "dev-licence-key.pem");
        var rsa = RSA.Create();
        rsa.ImportFromPem(File.ReadAllText(path));
        return rsa;
    }

    public static string Base64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    /// <summary>Writes a licence to a disposable folder under the data root and returns its path.</summary>
    public static string Write(string json)
    {
        var directory = Path.Combine(OgPaths.DataRoot, "licence-import-" + Guid.NewGuid().ToString("n")[..8]);
        Directory.CreateDirectory(directory);

        var path = Path.Combine(directory, "licence.json");
        File.WriteAllText(path, json);
        return path;
    }
}
