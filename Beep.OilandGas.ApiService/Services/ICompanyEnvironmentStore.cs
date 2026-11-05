using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;

namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyEnvironmentStore
{
    Task<CompanyEnvironmentConfiguration?> ReadAsync(CompanyScope company, Func<Task> authority);
    Task<CompanyEnvironmentConfiguration> SaveAsync(CompanyScope company, string actor, CompanyEnvironmentReviewedSaveRequest request, Func<Task> authority);
}
