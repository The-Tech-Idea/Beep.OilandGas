using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Repository;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>The selected company's PPDM core binding, revalidated before business access.</summary>
public sealed class CompanyModuleConnection(ModuleConnectionResolver modules, RepositoryTenantContext tenant,
    RepositoryCompanyContextResolver companies) : ICompanyAccountingReadContext
{
    private readonly object captureGate = new();
    private AssetDatabaseScope? capturedTarget;
    public RepositoryCompanyContext Company => tenant.Company ??
        throw RefusalException.Forbidden("Select an authorized company before accessing business data.");

    public async Task<(RepositoryCompanyAccess Access,string Binding,string Actor)> CaptureAsync(System.Security.Claims.ClaimsPrincipal user)
    {
        var actor=user.ActingUserId();var current=await RefreshCompanyAsync();RequireActor(actor);
        var connection=await modules.ResolveAsync("PRODUCTION");
        var binding=await modules.GetMigrationBindingFingerprintAsync(["PRODUCTION"],connection);
        return(current.ToAccess(),binding,actor);
    }

    public async Task<AssetDatabaseScope> ResolveAsync(string? queryConnection = null, string? bodyConnection = null,
        CancellationToken token = default)
    {
        await RefreshCompanyAsync(token);
        var target = await modules.ResolveAssetScopeAsync();
        token.ThrowIfCancellationRequested();
        RequireAssertion(queryConnection, target.ConnectionName);
        RequireAssertion(bodyConnection, target.ConnectionName);
        lock (captureGate)
        {
            if (capturedTarget is not null && capturedTarget != target)
                throw RefusalException.Conflict("The database binding changed during this operation. Review before submitting more work.");
            capturedTarget = target;
        }
        return target;
    }

    public async Task<RepositoryCompanyContext> RefreshCompanyAsync(CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var captured = Company;
        var current = await companies.ResolveAsync(captured.UserId, captured.Scope, token);
        if (!CompanyBackgroundScope.HasCapturedGrants(captured, current))
            throw RefusalException.Forbidden("Company access changed. Refresh your selection before accessing business data.");
        tenant.SelectCompany(current!);
        return current!;
    }

    public async Task<RepositoryCompanyContext?> ResolveSubjectAsync(string userId, CancellationToken token = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        var actor = await RefreshCompanyAsync(token);
        if (userId == actor.UserId) return actor;
        if (!actor.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A company Administrator is required to inspect another account's access.");
        return await companies.ResolveAsync(userId, actor.Scope, token);
    }

    public async Task<string> ResolveHierarchyCompanyAsync(string? subjectUserId)
    {
        var actor = await RefreshCompanyAsync();
        if (subjectUserId is null && !actor.Roles.Contains("Administrator", StringComparer.Ordinal))
            throw RefusalException.Forbidden("A current company Administrator is required to inspect the asset hierarchy.");
        if (subjectUserId is not null && await ResolveSubjectAsync(subjectUserId) is null)
            throw RefusalException.Forbidden("The hierarchy subject has no active membership in the selected company.");
        return actor.Scope.BusinessAssociateId;
    }

    public void RequireActor(string actor)
    {
        if (!string.Equals(Company.UserId, actor, StringComparison.Ordinal))
            throw RefusalException.Forbidden("Business changes require the selected company's authenticated account.");
    }

    public async Task RequireJobAsync(CsvImportJob job, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (job.Company != Company.Scope) throw RefusalException.Forbidden("The queued import belongs to another company.");
        RequireActor(job.UserId);
        var current = await ResolveAsync(job.Database.ConnectionName, token: token);
        if (current != job.Database)
            throw RefusalException.Conflict("The queued import's database binding changed. Review and submit the import again.");
    }

    private static void RequireAssertion(string? requested, string actual)
    {
        if (requested is null) return;
        if (string.IsNullOrWhiteSpace(requested)) throw RefusalException.Invalid("A supplied connection assertion cannot be empty.");
        if (!string.Equals(requested, actual, StringComparison.OrdinalIgnoreCase))
            throw RefusalException.Conflict("The requested connection does not match the selected company's PPDM module binding.");
    }
}
