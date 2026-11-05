using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
#if BEEP_SOURCE_TEST

namespace Beep.OilandGas.ApiService.Services;

public sealed record CompanyFieldObligationCommandScope(FieldSelectionReview Context, Func<Task> CheckCurrentAsync, Func<Task> ConfirmAsync);

// The native-reader test supplies company and binding authority; Beep and PPDM reads are real.
public sealed class CompanyModuleConnection(CompanyScope scope, AssetDatabaseScope database)
{
    public CompanyBoundary Company { get; } = new(scope);
    public Task<AssetDatabaseScope> ResolveAsync(string? queryConnection = null, string? bodyConnection = null, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(database);
    }
    public sealed class CompanyBoundary(CompanyScope scope)
    {
        public CompanyScope Scope { get; } = scope;
        public string UserId { get; } = "test-admin";
    }
}
#endif
