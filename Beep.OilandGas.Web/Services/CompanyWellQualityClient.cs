using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

/// <summary>Reads the selected company's well review through the API.</summary>
public sealed class CompanyWellQualityClient(ApiClient api)
{
    public Task<CompanyWellQualitySample?> FindAsync(CancellationToken token = default) =>
        api.GetAsync<CompanyWellQualitySample>("/api/company-data-quality/wells/sample", token);
    public Task<CompanyWellQualityBatchReview?> ReviewAsync(CompanyWellQualityBatchRequest request, CancellationToken token = default) =>
        api.PostAsync<CompanyWellQualityBatchRequest, CompanyWellQualityBatchReview>("/api/company-data-quality/wells/review", request, token);
}
