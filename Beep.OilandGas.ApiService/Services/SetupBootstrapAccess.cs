using System.Security.Cryptography;
using System.Text;
using Beep.OilandGas.Repository;
using Microsoft.Extensions.Configuration;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Explicit, temporary operator access before an application administrator exists.</summary>
public sealed class SetupBootstrapAccess(IConfiguration configuration, IRepositoryReadinessService readiness)
{
    public async Task RequireAsync(string? suppliedKey, CancellationToken token)
    {
        var expected = configuration["SetupBootstrap:Key"];
        if (!configuration.GetValue<bool>("SetupBootstrap:Enabled") || expected is null || expected.Length < 32 ||
            suppliedKey is null || suppliedKey.Length < 32 || suppliedKey.Length > 1024 || expected.Length > 1024 ||
            !CryptographicOperations.FixedTimeEquals(SHA256.HashData(Encoding.UTF8.GetBytes(expected)), SHA256.HashData(Encoding.UTF8.GetBytes(suppliedKey))))
            throw new UnauthorizedAccessException("Setup access is unavailable.");

        var status = await readiness.CheckAsync(token);
        if (status is RepositoryReadiness.Ready or RepositoryReadiness.RecoveryRequired)
            throw new UnauthorizedAccessException("Setup access is unavailable.");
    }
}
