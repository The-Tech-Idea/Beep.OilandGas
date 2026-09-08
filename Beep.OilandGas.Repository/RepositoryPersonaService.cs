using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Repository;

public sealed class RepositoryPersonaService(RepositoryDbContext db)
{
    public async Task<AppPersona> SaveCatalogAsync(string code, PersonaCatalogUpdate request, string actor, CancellationToken token = default)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        if (!System.Text.RegularExpressions.Regex.IsMatch(code, "^[A-Z0-9_]{1,64}$"))
            throw new ArgumentException("Persona code must contain uppercase letters, digits or underscores.");
        if (request.DefaultRoute is not null && (!request.DefaultRoute.StartsWith('/') ||
            request.DefaultRoute.StartsWith("//") || request.DefaultRoute.Contains('\\') || request.DefaultRoute.Any(char.IsControl)))
            throw new ArgumentException("The default route must be a local application path.");
        await ValidateUserAsync(actor, actor, token);
        var persona = await db.Set<AppPersona>().FindAsync([code], token);
        var before = persona is null ? null : JsonSerializer.Serialize(persona);
        if (persona is null) { persona = new AppPersona { Code = code }; db.Add(persona); }
        persona.Name = request.Name.Trim();
        persona.Description = request.Description;
        persona.DefaultRoute = request.DefaultRoute;
        persona.IsActive = request.IsActive;
        persona.DisplayOrder = request.DisplayOrder;
        Audit(actor, actor, "CatalogSaved", before, persona);
        await db.SaveChangesAsync(token);
        return persona;
    }

    public Task<List<AppPersona>> CatalogAsync(CancellationToken token = default) => db.Set<AppPersona>().AsNoTracking()
        .Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.Code).ToListAsync(token);

    public Task<AppUserPersona?> GetAsync(string userId, CancellationToken token = default) => db.Set<AppUserPersona>()
        .AsNoTracking().SingleOrDefaultAsync(x => x.UserId == userId, token);

    public async Task<AppUserPersona> SaveAsync(string userId, PersonaProfileUpdate request, string actor, CancellationToken token = default)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        await ValidateUserAsync(userId, actor, token);
        await ValidatePersonaAsync(request.PersonaCode, token);
        var profile = await db.Set<AppUserPersona>().SingleOrDefaultAsync(x => x.UserId == userId, token);
        CheckVersion(profile?.ConcurrencyStamp, request.ConcurrencyStamp);
        var before = profile is null ? null : JsonSerializer.Serialize(profile);
        if (profile is null) { profile = new AppUserPersona { UserId = userId }; db.Add(profile); }
        profile.PersonaCode = request.PersonaCode;
        profile.Locale = request.Locale;
        profile.TimeZone = request.TimeZone;
        profile.UnitSystem = request.UnitSystem;
        profile.DefaultFieldId = request.DefaultFieldId;
        profile.ChangedBy = actor;
        profile.ChangedUtc = DateTime.UtcNow;
        profile.ConcurrencyStamp = Guid.NewGuid().ToString();
        Audit(userId, actor, "ProfileSaved", before, profile);
        await db.SaveChangesAsync(token);
        return profile;
    }

    public Task<List<AppPersonaPreference>> PreferencesAsync(string userId, string personaCode, CancellationToken token = default) =>
        db.Set<AppPersonaPreference>().AsNoTracking().Where(x => x.UserId == userId && x.PersonaCode == personaCode)
            .OrderBy(x => x.ViewKey).ToListAsync(token);

    public async Task<AppPersonaPreference> SavePreferenceAsync(string userId, string personaCode, string viewKey,
        PersonaPreferenceUpdate request, string actor, CancellationToken token = default)
    {
        Validator.ValidateObject(request, new ValidationContext(request), true);
        if (string.IsNullOrWhiteSpace(viewKey) || viewKey.Length > 128) throw new ArgumentException("Invalid view key.");
        if (string.IsNullOrWhiteSpace(personaCode)) throw new ArgumentException("Persona is required.");
        await ValidateUserAsync(userId, actor, token);
        await ValidatePersonaAsync(personaCode, token);
        var preference = await db.Set<AppPersonaPreference>().FindAsync([userId, personaCode, viewKey], token);
        CheckVersion(preference?.ConcurrencyStamp, request.ConcurrencyStamp);
        var before = preference is null ? null : JsonSerializer.Serialize(preference);
        if (preference is null)
        {
            preference = new AppPersonaPreference { UserId = userId, PersonaCode = personaCode, ViewKey = viewKey };
            db.Add(preference);
        }
        preference.Value = request.Value;
        preference.ChangedBy = actor;
        preference.ChangedUtc = DateTime.UtcNow;
        preference.ConcurrencyStamp = Guid.NewGuid().ToString();
        Audit(userId, actor, "PreferenceSaved", before, preference);
        await db.SaveChangesAsync(token);
        return preference;
    }

    private async Task ValidateUserAsync(string userId, string actor, CancellationToken token)
    {
        if (!await db.Users.AnyAsync(x => x.Id == userId && x.IsActive, token) ||
            !await db.Users.AnyAsync(x => x.Id == actor && x.IsActive, token))
            throw new ArgumentException("An active repository account is required.");
    }

    private async Task ValidatePersonaAsync(string? code, CancellationToken token)
    {
        if (code is not null && !await db.Set<AppPersona>().AnyAsync(x => x.Code == code && x.IsActive, token))
            throw new ArgumentException("Select an active persona.");
    }

    private static void CheckVersion(string? current, string? supplied)
    {
        if (!string.Equals(current, supplied, StringComparison.Ordinal))
            throw new DbUpdateConcurrencyException("Persona settings changed. Reload before saving.");
    }

    private void Audit(string userId, string actor, string action, string? before, object after) =>
        db.Add(new AppPersonaAudit { UserId = userId, ActorUserId = actor, Action = action,
            BeforeJson = before, AfterJson = JsonSerializer.Serialize(after), ChangedUtc = DateTime.UtcNow });
}
