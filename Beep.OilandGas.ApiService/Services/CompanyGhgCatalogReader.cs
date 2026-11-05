using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

/// <summary>The company must review its environment again before selecting a catalog.</summary>
public sealed class CompanyEnvironmentReviewRequiredException() : InvalidOperationException("The company environment requires review.");

/// <summary>Reviews an explicitly selected shared factor catalog for one authorized company.</summary>
public sealed class CompanyGhgCatalogReader(CompanyEnvironmentEffectiveResolver environments, ISystemSetupRecordReader records)
{
    public async Task<CompanyGhgCatalogReview> ReviewAsync(CompanyScope company, string jurisdiction, Func<Task> authority)
    {
        ArgumentNullException.ThrowIfNull(authority);
        _ = SystemSetupIdentity.ForKey("GHG_FACTORS", jurisdiction);
        var effective = await environments.ResolveAsync(company, authority);
        if (effective.ReviewStatus != "Current") throw new CompanyEnvironmentReviewRequiredException();
        var catalog = await records.ReadAsync("GHG_FACTORS", jurisdiction, authority)
            ?? throw new InvalidOperationException("The selected factor catalog is unavailable.");
        SystemSetupIdentity.RequireValid(catalog, "GHG_FACTORS", jurisdiction);
        _ = GhgFactorCatalogValidation.Parse(jurisdiction, catalog.SettingsJson);
        await authority();
        var current = await records.ReadAsync("GHG_FACTORS", jurisdiction, authority);
        var closing = await environments.ResolveAsync(company, authority);
        await authority();
        if (closing.ReviewStatus != "Current") throw new CompanyEnvironmentReviewRequiredException();
        if (catalog != current || effective.EffectiveRevision != closing.EffectiveRevision)
            throw new InvalidOperationException("The factor catalog or company environment changed during review.");
        return new(company, effective.CompanyRevision, effective.EffectiveRevision, catalog);
    }
}
