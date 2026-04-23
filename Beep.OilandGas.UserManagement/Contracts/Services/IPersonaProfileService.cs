using Beep.OilandGas.UserManagement.Models.Profile;

namespace Beep.OilandGas.UserManagement.Contracts.Services;

public interface IPersonaProfileService
{
    Task<UserPersonaProfile?> GetProfileAsync(string userId);
    Task<UserPersonaProfile> UpsertProfileAsync(UserPersonaProfile profile, string actorUserId);
    Task<bool> SwitchPersonaAsync(string userId, string personaCode, string actorUserId);
    Task<List<PersonaDefinition>> GetPersonaCatalogAsync();
    Task<List<PersonaViewPreference>> GetViewPreferencesAsync(string userId, string personaCode);
    Task<bool> SetViewPreferenceAsync(PersonaViewPreference preference, string actorUserId);
}
