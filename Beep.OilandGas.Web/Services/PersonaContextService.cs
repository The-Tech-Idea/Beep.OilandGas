using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.Web.Services;

public interface IPersonaContextService
{
    AppUserPersona? CurrentProfile { get; }
    AppPersona? CurrentPersona { get; }
    Task EnsureLoadedAsync();
    Task ReloadAsync();
}

public sealed class PersonaContextService(PersonaClient client, AuthenticationStateProvider auth) : IPersonaContextService
{
    private string? _loadedUser;
    public AppUserPersona? CurrentProfile { get; private set; }
    public AppPersona? CurrentPersona { get; private set; }

    public async Task EnsureLoadedAsync()
    {
        var user = (await auth.GetAuthenticationStateAsync()).User;
        var id = user.Identity?.IsAuthenticated == true ? user.FindFirstValue(ClaimTypes.NameIdentifier) : null;
        if (id is null) { _loadedUser = null; CurrentProfile = null; CurrentPersona = null; return; }
        if (_loadedUser == id) return;
        CurrentProfile = await client.GetAsync(id);
        CurrentPersona = (await client.CatalogAsync()).FirstOrDefault(x => x.Code == CurrentProfile?.PersonaCode);
        _loadedUser = id;
    }

    public Task ReloadAsync() { _loadedUser = null; return EnsureLoadedAsync(); }
}
