using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.Repositories;
using Beep.OilandGas.UserManagement.Contracts.Services;
using Beep.OilandGas.UserManagement.Models.Audit;
using Beep.OilandGas.UserManagement.Models.Profile;
using Microsoft.Extensions.Logging;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.UserManagement.Services;

public class PersonaProfileService : IPersonaProfileService
{
    private readonly IDMEEditor _editor;
    private readonly ICommonColumnHandler _commonColumnHandler;
    private readonly IPPDM39DefaultsRepository _defaults;
    private readonly IPPDMMetadataRepository _metadata;
    private readonly string _connectionName;
    private readonly ILogger<PersonaProfileService>? _logger;

    public PersonaProfileService(
        IDMEEditor editor,
        ICommonColumnHandler commonColumnHandler,
        IPPDM39DefaultsRepository defaults,
        IPPDMMetadataRepository metadata,
        string connectionName,
        ILogger<PersonaProfileService>? logger = null)
    {
        _editor = editor;
        _commonColumnHandler = commonColumnHandler;
        _defaults = defaults;
        _metadata = metadata;
        _connectionName = connectionName;
        _logger = logger;
    }

    private PPDMGenericRepository GetRepo<T>(string tableName) =>
        new PPDMGenericRepository(_editor, _commonColumnHandler, _defaults, _metadata,
            typeof(T), _connectionName, tableName, null);

    public async Task<UserPersonaProfile?> GetProfileAsync(string userId)
    {
        var repo = GetRepo<UserPersonaProfile>("USER_PERSONA_PROFILE");
        var results = await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId }
        });
        return results.OfType<UserPersonaProfile>().FirstOrDefault();
    }

    public async Task<UserPersonaProfile> UpsertProfileAsync(UserPersonaProfile profile, string actorUserId)
    {
        var repo = GetRepo<UserPersonaProfile>("USER_PERSONA_PROFILE");
        var existing = await GetProfileAsync(profile.USER_ID);

        string? beforeJson = existing is not null
            ? System.Text.Json.JsonSerializer.Serialize(existing)
            : null;

        if (string.IsNullOrEmpty(profile.PROFILE_ID))
            profile.PROFILE_ID = Guid.NewGuid().ToString();

        profile.PROFILE_VERSION = (existing?.PROFILE_VERSION ?? 0) + 1;
        profile.LAST_ACCESSED_UTC = DateTime.UtcNow;

        if (existing is null)
            await repo.InsertAsync(profile, actorUserId);
        else
            await repo.UpdateAsync(profile, actorUserId);

        await WriteProfileAuditEventAsync(profile.USER_ID, actorUserId,
            existing is null ? "ProfileCreated" : "ProfileUpdated",
            beforeJson, System.Text.Json.JsonSerializer.Serialize(profile));

        return profile;
    }

    public async Task<bool> SwitchPersonaAsync(string userId, string personaCode, string actorUserId)
    {
        var profile = await GetProfileAsync(userId);
        if (profile is null)
            return false;

        var beforeJson = System.Text.Json.JsonSerializer.Serialize(profile);
        profile.PRIMARY_PERSONA = personaCode;
        profile.LAST_ACCESSED_UTC = DateTime.UtcNow;
        profile.PROFILE_VERSION++;

        var repo = GetRepo<UserPersonaProfile>("USER_PERSONA_PROFILE");
        await repo.UpdateAsync(profile, actorUserId);

        await WriteProfileAuditEventAsync(userId, actorUserId, "PersonaSwitched",
            beforeJson, System.Text.Json.JsonSerializer.Serialize(profile));
        return true;
    }

    public async Task<List<PersonaDefinition>> GetPersonaCatalogAsync()
    {
        var repo = GetRepo<PersonaDefinition>("PERSONA_DEFINITION");
        var results = await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "ACTIVE_FLAG", Operator = "=", FilterValue = "Y" }
        });
        return results.OfType<PersonaDefinition>().ToList();
    }

    public async Task<List<PersonaViewPreference>> GetViewPreferencesAsync(string userId, string personaCode)
    {
        var repo = GetRepo<PersonaViewPreference>("PERSONA_VIEW_PREFERENCE");
        var results = await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = userId },
            new AppFilter { FieldName = "PERSONA_CODE", Operator = "=", FilterValue = personaCode }
        });
        return results.OfType<PersonaViewPreference>().ToList();
    }

    public async Task<bool> SetViewPreferenceAsync(PersonaViewPreference preference, string actorUserId)
    {
        var repo = GetRepo<PersonaViewPreference>("PERSONA_VIEW_PREFERENCE");
        var existing = (await repo.GetAsync(new List<AppFilter>
        {
            new AppFilter { FieldName = "USER_ID", Operator = "=", FilterValue = preference.USER_ID },
            new AppFilter { FieldName = "PERSONA_CODE", Operator = "=", FilterValue = preference.PERSONA_CODE },
            new AppFilter { FieldName = "VIEW_KEY", Operator = "=", FilterValue = preference.VIEW_KEY }
        })).OfType<PersonaViewPreference>().FirstOrDefault();

        if (string.IsNullOrEmpty(preference.PREFERENCE_ID))
            preference.PREFERENCE_ID = Guid.NewGuid().ToString();
        preference.UPDATED_UTC = DateTime.UtcNow;

        if (existing is null)
            await repo.InsertAsync(preference, actorUserId);
        else
        {
            preference.PREFERENCE_ID = existing.PREFERENCE_ID;
            await repo.UpdateAsync(preference, actorUserId);
        }
        return true;
    }

    private async Task WriteProfileAuditEventAsync(
        string userId, string changedByUserId, string changeType,
        string? beforeJson, string? afterJson)
    {
        try
        {
            var auditRepo = GetRepo<UserProfileAuditEvent>("USER_PROFILE_AUDIT_EVENT");
            var ev = new UserProfileAuditEvent
            {
                EVENT_ID = Guid.NewGuid().ToString(),
                USER_ID = userId,
                CHANGED_BY_USER_ID = changedByUserId,
                EVENT_UTC = DateTime.UtcNow,
                CHANGE_TYPE = changeType,
                BEFORE_JSON = beforeJson,
                AFTER_JSON = afterJson,
                CORRELATION_ID = Guid.NewGuid().ToString()
            };
            await auditRepo.InsertAsync(ev, changedByUserId);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Failed to write profile audit event for user {UserId}", userId);
        }
    }
}
