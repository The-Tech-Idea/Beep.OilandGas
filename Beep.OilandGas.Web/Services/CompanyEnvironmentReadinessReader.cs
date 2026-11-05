using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public enum CompanyEnvironmentReadiness { Missing, ReviewRequired, Current }
public sealed record CompanyEnvironmentReadinessResult(CompanyEnvironmentReadiness Status, string? CountryCode, string? CompanyRevision = null, string? EffectiveRevision = null);

/// <summary>Checks the selected company's saved and effective setup without claiming operational activation.</summary>
public static class CompanyEnvironmentReadinessReader
{
    public static async Task<CompanyEnvironmentReadinessResult> ReadAsync(ApiClient api, CompanyScope company)
    {
        var result = await api.GetAsync<CompanyEnvironmentReadResult>("/api/company-environment");
        if (result is null || result.Company != company) throw new InvalidOperationException("The company environment response is unconfirmed.");
        if (result.Configuration is null) return new(CompanyEnvironmentReadiness.Missing, null);
        CompanyEnvironmentIdentity.RequireValid(result.Configuration, company);
        var effective = await api.GetAsync<CompanyEnvironmentEffectiveReview>("/api/company-environment/effective");
        if (effective is null || effective.Company != company || effective.CompanyRevision != result.Configuration.Revision ||
            effective.Settings is null || effective.Settings.Company != company || effective.Settings.CountryCode != result.Configuration.CountryCode || effective.Settings.Warnings is null ||
            effective.ReviewStatus == "Current" && effective.Settings.Warnings.Length != 0 ||
            !CompanyEnvironmentIdentity.IsRevision(effective.SystemRevision) || !CompanyEnvironmentIdentity.IsRevision(effective.EffectiveRevision) ||
            effective.CountryRevision is not null && !CompanyEnvironmentIdentity.IsRevision(effective.CountryRevision) ||
            effective.ReviewStatus is not ("Current" or "ReviewRequired"))
            throw new InvalidOperationException("The effective company environment is unconfirmed.");
        return new(effective.ReviewStatus == "Current" ? CompanyEnvironmentReadiness.Current : CompanyEnvironmentReadiness.ReviewRequired,
            result.Configuration.CountryCode, result.Configuration.Revision, effective.EffectiveRevision);
    }
}
