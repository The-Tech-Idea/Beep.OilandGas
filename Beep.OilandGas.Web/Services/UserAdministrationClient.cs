using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed class UserAdministrationClient(ApiClient api)
{
    public Task<RepositoryUserSummary?> UpdateUserAsync(string userId, RepositoryUserUpdate update) =>
        api.PutAsync<RepositoryUserUpdate, RepositoryUserSummary>($"/api/identity/users/{Uri.EscapeDataString(userId)}", update);

    public Task<bool> CreateRoleAsync(string name, string? description) =>
        api.PostAsync("/api/identity/roles", new RepositoryRoleRequest(name, description));
    public async Task<List<RepositoryUserSummary>> GetUsersAsync() => await api.GetAsync<List<RepositoryUserSummary>>("/api/identity/users") ?? [];
    public async Task<List<RepositoryRoleSummary>> GetRolesAsync() => await api.GetAsync<List<RepositoryRoleSummary>>("/api/identity/roles") ?? [];
    public async Task<List<string>> GetUserRolesAsync(string userId) =>
        await api.GetAsync<List<string>>($"/api/identity/users/{Uri.EscapeDataString(userId)}/roles") ?? [];
    public Task<bool> AssignRoleAsync(string userId, string role) =>
        api.PostAsync($"/api/identity/users/{Uri.EscapeDataString(userId)}/roles", new UserRoleChangeRequest(role));
    public Task<bool> RemoveRoleAsync(string userId, string role) =>
        api.DeleteAsync($"/api/identity/users/{Uri.EscapeDataString(userId)}/roles/{Uri.EscapeDataString(role)}");
}
