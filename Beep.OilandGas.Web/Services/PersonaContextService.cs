using Beep.OilandGas.Client.App;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public interface IPersonaClient
{
    Task<CompanyPersonaWorkspaceReview> ReviewWorkspacesAsync();
    Task<AppUserPersona?> GetAsync(string userId);
    Task<AppUserPersona> SaveAsync(string userId, PersonaProfileUpdate request);
}

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
    private readonly IPersonaClient _client;
    private readonly CompanyClientContext _company;
    private readonly Func<ClaimsPrincipal, string?> _resolveAccount;
    private readonly AuthenticationStateProvider _auth;
    private string? _loadedUser;
    private string? _workspaceRevision;
    private int _version;
    private bool _disposed;
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    public AppUserPersona? CurrentProfile { get; private set; }
    public AppPersona? CurrentPersona { get; private set; }
    public IReadOnlyList<AppPersona> Catalog { get; private set; } = [];
    public event Action? Changed;

    public PersonaContextService(IPersonaClient client, AuthenticationStateProvider auth, CompanyClientContext company, Func<ClaimsPrincipal, string?> resolveAccount)
    {
        _client = client; _auth = auth; _company = company; _resolveAccount = resolveAccount;
        _auth.AuthenticationStateChanged += AuthenticationChanged;
        _company.Changed += CompanyChanged;
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
        var company = _company.Snapshot;
        var user = (await _auth.GetAuthenticationStateAsync()).User;
        if (_disposed || version != _version) return;
        var id = user.Identity?.IsAuthenticated == true ? _resolveAccount(user) : null;
        if (id is null || company.Selection is null || company.Selection.UserId != id) { Clear(); return; }
        if (_loadedUser == id) return;
        Clear();
        version = _version;
        var profile = await _client.GetAsync(id);
        if (_disposed || version != _version || company != _company.Snapshot) return;
        var review = await _client.ReviewWorkspacesAsync();
        if (_disposed || version != _version || company != _company.Snapshot) return;
        if (profile is not null && (profile.UserId != id || profile.TenantId != company.Selection.Scope.TenantId ||
            profile.BusinessAssociateId != company.Selection.Scope.BusinessAssociateId))
            throw new InvalidOperationException("The persona profile does not match the signed-in account and company.");
        ValidateReview(review, company.Selection);
        CurrentProfile = profile;
        Catalog = Array.AsReadOnly(review.Personas.Select(ClonePersona).ToArray());
        _workspaceRevision = review.Revision;
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
        var company = _company.Snapshot;
        var review = await _client.ReviewWorkspacesAsync();
        if (_disposed || version != _version || company != _company.Snapshot) throw new OperationCanceledException("Company workspace selection changed.");
        ValidateReview(review, company.Selection!);
        if (review.Revision != _workspaceRevision || !review.Personas.Any(row => row.Code == code))
            throw new InvalidOperationException("Company roles or workspace choices changed. Reload workspace settings before choosing again.");
        var saved = await _client.SaveAsync(user, new(code, profile?.Locale, profile?.TimeZone,
            profile?.UnitSystem, profile?.DefaultFieldId, profile?.ConcurrencyStamp));
        if (_disposed || version != _version) throw new OperationCanceledException("The signed-in session changed.");
        if (saved.UserId != user || saved.PersonaCode != code || company.Selection is null || saved.TenantId != company.Selection.Scope.TenantId ||
            saved.BusinessAssociateId != company.Selection.Scope.BusinessAssociateId) throw new InvalidOperationException("The saved workspace preference does not match the requested account, company and persona.");
        CurrentProfile = saved;
        CurrentPersona = saved.PersonaCode == persona.Code ? persona : Catalog.FirstOrDefault(x => x.Code == saved.PersonaCode);
        Changed?.Invoke();
    }

    private static AppPersona ClonePersona(AppPersona row) => new() { Code = row.Code, Name = row.Name, Description = row.Description, DefaultRoute = row.DefaultRoute, IsActive = row.IsActive, DisplayOrder = row.DisplayOrder };
    private static void ValidateReview(CompanyPersonaWorkspaceReview review, CompanyClientSelection selection)
    {
        if (review.Company != selection.Scope || review.ActorUserId != selection.UserId || review.Roles is null ||
            !review.Roles.Order(StringComparer.Ordinal).SequenceEqual(selection.Roles.Order(StringComparer.Ordinal)) || review.Personas is null ||
            review.Revision is not { Length: 64 } || review.Revision.Any(value => !char.IsAsciiHexDigit(value) || char.IsAsciiLetterLower(value)) ||
            review.Personas.Count > 128 || review.Personas.Any(row => row is null || !row.IsActive || string.IsNullOrWhiteSpace(row.Code) || string.IsNullOrWhiteSpace(row.Name)) ||
            review.Personas.Select(row => row.Code).Distinct(StringComparer.Ordinal).Count() != review.Personas.Count)
            throw new InvalidOperationException("The workspace review does not match current account/company roles. Reload company access.");
    }

    private void AuthenticationChanged(Task<AuthenticationState> _) => Clear();
    private void CompanyChanged() => Clear();
    private void Clear()
    {
        ++_version; _loadedUser = null; _workspaceRevision = null; CurrentProfile = null; CurrentPersona = null; Catalog = [];
        Changed?.Invoke();
    }
    public void Dispose() { _disposed = true; _auth.AuthenticationStateChanged -= AuthenticationChanged; _company.Changed -= CompanyChanged; Clear(); }
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
