using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>Resolves a module through the existing tenant-owned Beep binding and checks its target fingerprint.</summary>
public sealed class CompanyEnvironmentBindingReader(ModuleConnectionResolver modules) : ICompanyEnvironmentBindingReader
{
    public async Task<string?> ResolveAsync(string module, CancellationToken cancellationToken)
    {
        try
        {
            var connection = await modules.ResolveAsync(module, cancellationToken);
            return await modules.GetMigrationBindingFingerprintAsync([module], connection);
        }
        catch (RefusalException refusal) when (refusal.Kind is RefusalKind.Conflict or RefusalKind.NotFound)
        { return null; }
    }
}
