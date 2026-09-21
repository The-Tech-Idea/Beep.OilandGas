using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>Uses the app's authoritative RBAC and resource services for subscription and delivery.</summary>
public interface INotificationPersonaReader
{
    Task<bool> IsCurrentPersonaAsync(string userId, string personaCode);
}

public sealed class RepositoryNotificationPersonaReader(RepositoryDbContext repository) : INotificationPersonaReader
{
    public Task<bool> IsCurrentPersonaAsync(string userId, string personaCode) =>
        (from user in repository.Users.AsNoTracking()
         join profile in repository.Set<AppUserPersona>() on user.Id equals profile.UserId
         join persona in repository.Set<AppPersona>() on profile.PersonaCode equals persona.Code
         where user.Id == userId && user.IsActive && persona.IsActive && persona.Code == personaCode
         select user.Id).AnyAsync();
}

public sealed class WorkflowNotificationAuthorization(
    IAccessControlService access, INotificationPersonaReader personas, IProcessService processes)
{
    public async Task<bool> CanAccessPersonaAsync(string userId, string personaCode, string fieldId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(personaCode) || string.IsNullOrWhiteSpace(fieldId))
            return false;
        var assigned = await access.GetUserRolesAsync(userId);
        return assigned.Count > 0 && await personas.IsCurrentPersonaAsync(userId, personaCode) &&
            (await access.CheckAssetAccessAsync(userId, fieldId, "FIELD", null)).HasAccess;
    }

    public async Task<bool> HasRoleAsync(string userId, string role) =>
        !string.IsNullOrWhiteSpace(role) && (await access.GetUserRolesAsync(userId)).Contains(role, StringComparer.Ordinal);

    public async Task<bool> CanAccessProcessAsync(string userId, string processInstanceId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(processInstanceId)) return false;
        var process = await processes.GetProcessInstanceAsync(processInstanceId);
        return process is not null && !string.IsNullOrWhiteSpace(process.FieldId) &&
            !string.IsNullOrWhiteSpace(process.EntityId) && !string.IsNullOrWhiteSpace(process.EntityType) &&
            (await access.CheckAssetAccessAsync(userId, process.FieldId, "FIELD", null)).HasAccess &&
            (await access.CheckAssetAccessAsync(userId, process.EntityId, process.EntityType, null)).HasAccess;
    }
}
