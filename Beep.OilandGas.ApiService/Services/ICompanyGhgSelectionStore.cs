using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;

namespace Beep.OilandGas.ApiService.Services;

public interface ICompanyGhgSelectionStore
{
    Task<CompanyGhgSelection?> ReadAsync(CompanyScope company, Func<Task> authority);
    Task<CompanyGhgSelection> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority);
}
