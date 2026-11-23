using Beep.OilandGas.Models.Data.Company;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Resolves a module through the existing tenant-owned Beep binding and checks its target fingerprint.</summary>
public sealed class CompanyEnvironmentBindingReader(ModuleConnectionResolver modules) : ICompanyEnvironmentBindingReader
{
    public async Task<string?> ResolveAsync(string module, CancellationToken cancellationToken)
    {
        var connection = await modules.FindAsync(module, cancellationToken);
        return connection is null ? null : await modules.GetMigrationBindingFingerprintAsync([module], connection);
    }
}
