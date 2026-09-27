using Beep.Foundation.IdentityServer.Shared.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public interface IPersonaContextService
{
    AppUserPersona? CurrentProfile { get; }
    AppPersona? CurrentPersona { get; }
    IReadOnlyList<AppPersona> Catalog { get; }
    event Action? Changed;
    Task EnsureLoadedAsync();
    Task ReloadAsync();
    Task SwitchAsync(string code);
}

public sealed class PersonaContextService : IPersonaContextService, IDisposable
{
    private readonly PersonaClient _client;
    private readonly AuthenticationStateProvider _auth;
    private string? _loadedUser;
    private int _version;
    private bool _disposed;
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    public AppUserPersona? CurrentProfile { get; private set; }
    public AppPersona? CurrentPersona { get; private set; }
    public IReadOnlyList<AppPersona> Catalog { get; private set; } = [];
    public event Action? Changed;

    public PersonaContextService(PersonaClient client, AuthenticationStateProvider auth)
    {
        _client = client; _auth = auth;
        _auth.AuthenticationStateChanged += AuthenticationChanged;
    }

    public async Task EnsureLoadedAsync()
    {
        await _loadGate.WaitAsync();
        try { await LoadCoreAsync(); }
        finally { _loadGate.Release(); }
    }

    private async Task LoadCoreAsync()
    {
        if (_disposed) return;
        var version = _version;
        var user = (await _auth.GetAuthenticationStateAsync()).User;
        if (_disposed || version != _version) return;
        var id = user.Identity?.IsAuthenticated == true ? PartyIdClaims.Find(user) : null;
        if (id is null) { Clear(); return; }
        if (_loadedUser == id) return;
        Clear();
        version = _version;
        var profile = await _client.GetAsync(id);
        var catalog = await _client.CatalogAsync();
        if (_disposed || version != _version) return;
        CurrentProfile = profile;
        Catalog = catalog.Where(x => x.IsActive).ToList();
        CurrentPersona = Catalog.FirstOrDefault(x => x.Code == profile?.PersonaCode);
        _loadedUser = id;
        Changed?.Invoke();
    }

    public Task ReloadAsync() { Clear(); return EnsureLoadedAsync(); }

    public async Task SwitchAsync(string code)
    {
        await EnsureLoadedAsync();
        var persona = Catalog.SingleOrDefault(x => x.Code == code)
            ?? throw new InvalidOperationException("Select an available persona.");
        var user = _loadedUser ?? throw new InvalidOperationException("Sign in before selecting a persona.");
        var version = _version;
        var profile = CurrentProfile;
        var saved = await _client.SaveAsync(user, new(code, profile?.Locale, profile?.TimeZone,
            profile?.UnitSystem, profile?.DefaultFieldId, profile?.ConcurrencyStamp));
        if (_disposed || version != _version) throw new OperationCanceledException("The signed-in session changed.");
        CurrentProfile = saved;
        CurrentPersona = saved.PersonaCode == persona.Code ? persona : Catalog.FirstOrDefault(x => x.Code == saved.PersonaCode);
        Changed?.Invoke();
    }

    private void AuthenticationChanged(Task<AuthenticationState> _) => Clear();
    private void Clear()
    {
        ++_version; _loadedUser = null; CurrentProfile = null; CurrentPersona = null; Catalog = [];
        Changed?.Invoke();
    }
    public void Dispose() { _disposed = true; _auth.AuthenticationStateChanged -= AuthenticationChanged; Clear(); }
}

public static class PersonaNavigation
{
    public static string HomeRoute(string? route)
    {
        if (string.IsNullOrWhiteSpace(route)) return "/dashboard";
        // Only application-relative paths. Encoded path separators and control characters are rejected too.
        if (route[0] != '/' || route.StartsWith("//", StringComparison.Ordinal) ||
            route.Any(c => char.IsControl(c) || c == '\\') || route.Contains('%')) return "/dashboard";
        return route;
    }
}
