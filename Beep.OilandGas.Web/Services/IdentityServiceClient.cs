using Beep.OilandGas.UserManagement.Models.Identity;
using Beep.OilandGas.UserManagement.Models.Profile;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Typed HTTP client for identity, persona profile, and role/permission management.
/// Wraps REST calls to PersonaProfileController and RoleAssignmentController.
/// All web pages and components route identity operations through this client.
/// </summary>
public class IdentityServiceClient : IIdentityServiceClient
{
    private readonly ApiClient _apiClient;

    public IdentityServiceClient(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<UserPersonaProfile?> GetProfileAsync(string userId)
    {
        return await _apiClient.GetAsync<UserPersonaProfile>($"/api/identity/profile/{userId}");
    }

    public async Task<UserPersonaProfile> UpsertProfileAsync(UserPersonaProfile profile, string actorUserId)
    {
        var response = await _apiClient.PutAsync<UserPersonaProfile, UserPersonaProfile>(
            $"/api/identity/profile/{profile.USER_ID}",
            profile);
        return response ?? throw new InvalidOperationException("Failed to upsert persona profile");
    }

    public async Task<UserPersonaProfile> SwitchPersonaAsync(string userId, string personaCode, string actorUserId)
    {
        var request = new SwitchPersonaRequest(personaCode);
        var response = await _apiClient.PostAsync<SwitchPersonaRequest, UserPersonaProfile>(
            $"/api/identity/profile/{userId}/switch-persona",
            request);
        return response ?? throw new InvalidOperationException("Failed to switch persona");
    }

    public async Task<List<PersonaDefinition>> GetPersonaCatalogAsync()
    {
        var response = await _apiClient.GetAsync<List<PersonaDefinition>>("/api/identity/profile/catalog");
        return response ?? [];
    }

    public async Task<PersonaViewPreference?> GetViewPreferencesAsync(string userId, string personaCode)
    {
        return await _apiClient.GetAsync<PersonaViewPreference>(
            $"/api/identity/profile/{userId}/preferences/{personaCode}");
    }

    public async Task<PersonaViewPreference> SetViewPreferenceAsync(PersonaViewPreference preference, string actorUserId)
    {
        var response = await _apiClient.PutAsync<PersonaViewPreference, PersonaViewPreference>(
            $"/api/identity/profile/{preference.USER_ID}/preferences",
            preference);
        return response ?? throw new InvalidOperationException("Failed to set view preference");
    }

    public async Task<List<AppRole>> GetRoleCatalogAsync()
    {
        var response = await _apiClient.GetAsync<List<AppRole>>("/api/identity/roles/catalog");
        return response ?? [];
    }

    public async Task<List<AppPermission>> GetPermissionCatalogAsync()
    {
        var response = await _apiClient.GetAsync<List<AppPermission>>("/api/identity/roles/permissions/catalog");
        return response ?? [];
    }

    public async Task<List<AppUserRole>> GetUserRoleAssignmentsAsync(string userId)
    {
        var response = await _apiClient.GetAsync<List<AppUserRole>>(
            $"/api/identity/roles/users/{userId}/assignments");
        return response ?? [];
    }

    public async Task<AppUserRole> AssignRoleAsync(string userId, string roleId, string? reason, string actorUserId)
    {
        var request = new AssignRoleRequest(roleId, reason);
        var response = await _apiClient.PostAsync<AssignRoleRequest, AppUserRole>(
            $"/api/identity/roles/users/{userId}/assignments",
            request);
        return response ?? throw new InvalidOperationException("Failed to assign role");
    }

    public async Task RevokeRoleAsync(string userRoleId, string actorUserId)
    {
        await _apiClient.DeleteAsync($"/api/identity/roles/assignments/{userRoleId}");
    }

    public async Task<List<AppRolePermission>> GetRolePermissionsAsync(string roleId)
    {
        var response = await _apiClient.GetAsync<List<AppRolePermission>>(
            $"/api/identity/roles/{roleId}/permissions");
        return response ?? [];
    }

    public async Task<AppRolePermission> GrantPermissionToRoleAsync(string roleId, string permissionId, string actorUserId)
    {
        var request = new GrantPermissionRequest(permissionId);
        var response = await _apiClient.PostAsync<GrantPermissionRequest, AppRolePermission>(
            $"/api/identity/roles/{roleId}/permissions",
            request);
        return response ?? throw new InvalidOperationException("Failed to grant permission");
    }

    public async Task RevokePermissionFromRoleAsync(string rolePermissionId, string actorUserId)
    {
        await _apiClient.DeleteAsync($"/api/identity/roles/permissions/{rolePermissionId}");
    }

    // Request DTOs that match the API controller signatures
    private sealed record SwitchPersonaRequest(string PersonaCode);
    private sealed record AssignRoleRequest(string RoleId, string? Reason);
    private sealed record GrantPermissionRequest(string PermissionId);
}
