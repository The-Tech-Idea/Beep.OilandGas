using Beep.OilandGas.Models.Data.Company;
#if BEEP_SECURE_CONNECTIONS || BEEP_SOURCE_TEST
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Beep.ConfigUtil;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Reads Beep's portable credential encryption keys from the host's secret configuration.</summary>
public sealed class BeepCredentialKeyProvider(IConfiguration configuration) : IConnectionCredentialKeyProvider
{
    public string CurrentKeyId => configuration["Beep:CredentialProtection:CurrentKeyId"]
        ?? throw new CryptographicException("The Beep credential key identity is unavailable.");

    public ReadOnlyMemory<byte> GetKey(string keyId)
    {
        if (string.IsNullOrWhiteSpace(keyId) || keyId.Length > 128 || keyId.Any(c => !(char.IsAsciiLetterOrDigit(c) || c is '-' or '_')))
            throw new CryptographicException("The Beep credential key identity is invalid.");
        var encoded = configuration["Beep:CredentialProtection:Keys:" + keyId];
        if (string.IsNullOrWhiteSpace(encoded)) throw new CryptographicException("The Beep credential key is unavailable.");
        try
        {
            var key = Convert.FromBase64String(encoded);
            if (key.Length == 32) return key;
            CryptographicOperations.ZeroMemory(key);
            throw new CryptographicException("The Beep credential key must contain 256 bits.");
        }
        catch (FormatException) { throw new CryptographicException("The Beep credential key is invalid."); }
    }
}
#endif
