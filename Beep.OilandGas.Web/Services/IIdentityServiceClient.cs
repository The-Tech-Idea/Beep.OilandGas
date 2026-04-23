using Beep.OilandGas.UserManagement.Models.Identity;
using Beep.OilandGas.UserManagement.Models.Profile;

namespace Beep.OilandGas.Web.Services;

/// <summary>
/// Typed client for identity, persona profile, and role/permission management.
/// All persona profile and access control operations route through this client.
/// No pages or components call API endpoints directly; this client is the facade.
/// </summary>
public interface IIdentityServiceClient
{
    /// <summary>
    /// Get a user's current persona profile.
    /// </summary>
    Task<UserPersonaProfile?> GetProfileAsync(string userId);

    /// <summary>
    /// Upsert a user's persona profile.
    /// </summary>
    Task<UserPersonaProfile> UpsertProfileAsync(UserPersonaProfile profile, string actorUserId);

    /// <summary>
    /// Switch user's active persona and record the switch as an audit event.
    /// </summary>
    Task<UserPersonaProfile> SwitchPersonaAsync(string userId, string personaCode, string actorUserId);

    /// <summary>
    /// Get the catalog of all available personas.
    /// </summary>
    Task<List<PersonaDefinition>> GetPersonaCatalogAsync();

    /// <summary>
    /// Get view preferences for a user and persona.
    /// </summary>
    Task<PersonaViewPreference?> GetViewPreferencesAsync(string userId, string personaCode);

    /// <summary>
    /// Set or update view preferences for a user and persona.
    /// </summary>
    Task<PersonaViewPreference> SetViewPreferenceAsync(PersonaViewPreference preference, string actorUserId);

    /// <summary>
    /// Get the catalog of all available roles.
    /// </summary>
    Task<List<AppRole>> GetRoleCatalogAsync();

    /// <summary>
    /// Get the catalog of all available permissions.
    /// </summary>
    Task<List<AppPermission>> GetPermissionCatalogAsync();

    /// <summary>
    /// Get all role assignments for a user.
    /// </summary>
    Task<List<AppUserRole>> GetUserRoleAssignmentsAsync(string userId);

    /// <summary>
    /// Assign a role to a user.
    /// </summary>
    Task<AppUserRole> AssignRoleAsync(string userId, string roleId, string? reason, string actorUserId);

    /// <summary>
    /// Revoke a role assignment from a user.
    /// </summary>
    Task RevokeRoleAsync(string userRoleId, string actorUserId);

    /// <summary>
    /// Get all permission assignments for a role.
    /// </summary>
    Task<List<AppRolePermission>> GetRolePermissionsAsync(string roleId);

    /// <summary>
    /// Grant a permission to a role.
    /// </summary>
    Task<AppRolePermission> GrantPermissionToRoleAsync(string roleId, string permissionId, string actorUserId);

    /// <summary>
    /// Revoke a permission from a role.
    /// </summary>
    Task RevokePermissionFromRoleAsync(string rolePermissionId, string actorUserId);
}
