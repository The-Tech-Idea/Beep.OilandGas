using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyGhgSelectionStore
{
    Task<CompanyGhgSelection?> ReadAsync(CompanyScope company, Func<Task> authority);
    Task<CompanyGhgSelection> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority);
}
