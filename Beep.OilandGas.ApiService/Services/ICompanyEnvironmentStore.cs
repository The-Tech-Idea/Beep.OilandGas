using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyEnvironmentStore
{
    Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority);
    Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority);
}
