#if BEEP_SOURCE_TEST && WORKFLOW_INBOX_FOCUSED
// Unused controller collaborators and the resolved-account claim are test seams.
// The company resolver, preview resolver, controller and OilandGasTools are real.
using System.Security.Claims;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public static class CompanyPreviewTestAccount
{
    public static string? FindActingUserId(this ClaimsPrincipal principal) => principal.FindFirst("party_id")?.Value;
}
public sealed class CompanyEnvironmentRuntime { public Task<object> VerifyAsync(CompanyScope company, Func<Task> authority) => throw new NotSupportedException(); }
public sealed class CompanyGhgCatalogReader { public Task<object> ReviewAsync(CompanyScope company, string jurisdiction, Func<Task> authority) => throw new NotSupportedException(); }
public sealed class CompanyGhgSelectionStatusReader { public Task<object> ReadAsync(CompanyScope company, Func<Task> authority) => throw new NotSupportedException(); }
public interface ICompanyGhgSelectionStore { Task<object> SaveAsync(CompanyScope company, string actor, CompanyGhgSelectionRequest request, Func<Task> authority); }
public sealed class CompanyEnvironmentWellTestService { public Task<object> TestAsync(CompanyScope company, CompanyEnvironmentWellTestRequest request, Func<Task> authority, CancellationToken token) => throw new NotSupportedException(); }
#endif
