using Beep.OilandGas.Repository.Models;

namespace Beep.OilandGas.Web.Services;

public sealed class PersonaClient(ApiClient api) : IPersonaClient
{
    public async Task<CompanyPersonaWorkspaceReview> ReviewWorkspacesAsync() =>
        await api.GetAsync<CompanyPersonaWorkspaceReview>("/api/personas/workspaces/current")
        ?? throw new InvalidOperationException("Company workspace review was empty.");
    private static string Segment(string value) => Uri.EscapeDataString(value);
    public async Task<RoleWorkspaceReview> ReviewRoleWorkspacesAsync(string roleId) =>
        await api.GetAsync<RoleWorkspaceReview>($"/api/personas/roles/{Segment(roleId)}/workspaces")
        ?? throw new InvalidOperationException("Role workspace review was empty.");
    public Task SaveRoleWorkspaceAsync(string roleId, string code, bool enabled, string expectedRevision) =>
        api.PutAsync($"/api/personas/roles/{Segment(roleId)}/workspaces/{Segment(code)}", new RoleWorkspaceMappingRequest(enabled, expectedRevision));

    public async Task<List<AppPersona>> CatalogAsync() => await api.GetAsync<List<AppPersona>>("/api/personas") ?? throw new InvalidOperationException("Persona catalog response was empty.");
    public async Task<AppUserPersona?> GetAsync(string userId) =>
        (await api.GetAsync<PersonaProfileResult>($"/api/personas/users/{Segment(userId)}") ?? throw new InvalidOperationException("Persona profile response was empty.")).Profile;
    public async Task<AppUserPersona> SaveAsync(string userId, PersonaProfileUpdate request) =>
        await api.PutAsync<PersonaProfileUpdate, AppUserPersona>($"/api/personas/users/{Segment(userId)}", request)
        ?? throw new InvalidOperationException("Profile save returned no result.");
    public async Task<AppPersona> SaveCatalogAsync(string code, PersonaCatalogUpdate request) =>
        await api.PutAsync<PersonaCatalogUpdate, AppPersona>($"/api/personas/{Segment(code)}", request)
        ?? throw new InvalidOperationException("Persona save returned no result.");
    public async Task<List<AppPersonaPreference>> PreferencesAsync(string userId, string code) =>
        await api.GetAsync<List<AppPersonaPreference>>($"/api/personas/users/{Segment(userId)}/preferences/{Segment(code)}") ?? [];
    public async Task<AppPersonaPreference> SavePreferenceAsync(string userId, string code, string viewKey, PersonaPreferenceUpdate request) =>
        await api.PutAsync<PersonaPreferenceUpdate, AppPersonaPreference>(
            $"/api/personas/users/{Segment(userId)}/preferences/{Segment(code)}/{Segment(viewKey)}", request)
        ?? throw new InvalidOperationException("Preference save returned no result.");
}
