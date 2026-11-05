using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Repository;
using Microsoft.EntityFrameworkCore;

namespace Beep.OilandGas.ApiService.Hubs;

/// <summary>Uses the app's authoritative RBAC and resource services for subscription and delivery.</summary>
public interface INotificationPersonaReader
{
    Task<bool> IsCurrentPersonaAsync(string userId, string personaCode);
}

public sealed class RepositoryNotificationPersonaReader(RepositoryDbContext repository, RepositoryTenantContext tenant) : INotificationPersonaReader
{
    public Task<bool> IsCurrentPersonaAsync(string userId, string personaCode)
    {
        var company = tenant.Company;
        if (company is null || company.UserId != userId) return Task.FromResult(false);
        return (from user in repository.Users.AsNoTracking()
         join profile in repository.Set<AppUserPersona>() on user.Id equals profile.UserId
         join persona in repository.Set<AppPersona>() on profile.PersonaCode equals persona.Code
         where user.Id == userId && user.IsActive && persona.IsActive && persona.Code == personaCode &&
             profile.TenantId == company.Scope.TenantId && profile.BusinessAssociateId == company.Scope.BusinessAssociateId
         select user.Id).AnyAsync();
    }
}

public sealed class WorkflowNotificationAuthorization(
    IAccessControlService access, INotificationPersonaReader personas, IProcessService processes, RepositoryTenantContext tenant)
{
    public async Task<bool> CanAccessPersonaAsync(string userId, string personaCode, string fieldId)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(personaCode) || string.IsNullOrWhiteSpace(fieldId))
            return false;
        return tenant.Company is { } company && company.UserId == userId && company.Roles.Count > 0 &&
            await personas.IsCurrentPersonaAsync(userId, personaCode) &&
            (await access.CheckAssetAccessAsync(userId, fieldId, "FIELD", null)).HasAccess;
    }

    public Task<bool> HasRoleAsync(string userId, string role) => Task.FromResult(
        !string.IsNullOrWhiteSpace(role) && tenant.Company is { } company && company.UserId == userId &&
        company.Roles.Contains(role, StringComparer.Ordinal));

    public async Task<bool> CanAccessProcessAsync(string userId, string processInstanceId)
    {
        if (tenant.Company?.UserId != userId || string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(processInstanceId)) return false;
        var process = await processes.GetProcessInstanceAsync(processInstanceId);
        return process is not null && !string.IsNullOrWhiteSpace(process.FieldId) &&
            !string.IsNullOrWhiteSpace(process.EntityId) && !string.IsNullOrWhiteSpace(process.EntityType) &&
            (await access.CheckAssetAccessAsync(userId, process.FieldId, "FIELD", null)).HasAccess &&
            (await access.CheckAssetAccessAsync(userId, process.EntityId, process.EntityType, null)).HasAccess;
    }
}
