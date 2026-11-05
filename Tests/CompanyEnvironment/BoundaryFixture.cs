using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.DataManagement;
using System.Security.Claims;

namespace Beep.OilandGas.Repository
{
    // Narrow test boundary: the production resolver uses the repository's complete interface.
    public interface IRepositoryAccessService
    {
        Task<RepositoryCompanyAccess?> GetCompanyAccessAsync(string userId, CompanyScope scope, CancellationToken token = default);
    }
}

namespace Beep.OilandGas.ApiService.Services
{
    // Organization verification has its own provider-focused tests; this harness exercises environment services.
    public sealed class CompanyOrganizationVerifier
    {
        public Task<CompanyOrganizationReview> ReviewAsync(CompanyScope company, Func<Task> authority, CancellationToken token) => throw new NotSupportedException();
    }
    public static class FixtureActingUser
    {
        public static string? FindActingUserId(this ClaimsPrincipal user) => user.FindFirst("party_id")?.Value;
    }
}
