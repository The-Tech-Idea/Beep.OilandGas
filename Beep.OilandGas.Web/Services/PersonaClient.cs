using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public sealed class PersonaClient(ApiClient api)
{
    private static string Segment(string value) => Uri.EscapeDataString(value);
    public async Task<List<AppPersona>> CatalogAsync() => await api.GetAsync<List<AppPersona>>("/api/personas") ?? [];
    public async Task<AppUserPersona?> GetAsync(string userId) =>
        (await api.GetAsync<PersonaProfileResult>($"/api/personas/users/{Segment(userId)}"))?.Profile;
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
