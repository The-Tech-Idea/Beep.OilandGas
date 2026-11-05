using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;

namespace Beep.OilandGas.Web.Services;

/// <summary>Standard Blazor authentication state with API-granted company roles on business routes.</summary>
public sealed class CompanyAuthenticationStateProvider : AuthenticationStateProvider,
    IHostEnvironmentAuthenticationStateProvider, IDisposable
{
    private readonly AuthenticationStateProvider inner;
    private readonly CompanyClientContext company;
    private readonly NavigationManager navigation;
    private readonly Func<ClaimsPrincipal, string?> resolveAccount;
    private Task<AuthenticationState>? accountState;
    private bool disposed;

    public CompanyAuthenticationStateProvider(AuthenticationStateProvider inner, CompanyClientContext company,
        NavigationManager navigation, Func<ClaimsPrincipal, string?> resolveAccount)
    {
        this.inner = inner; this.company = company; this.navigation = navigation; this.resolveAccount = resolveAccount;
        inner.AuthenticationStateChanged += AccountChanged;
        company.Changed += ScopeChanged;
        navigation.LocationChanged += LocationChanged;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var pending = accountState ??= inner.GetAuthenticationStateAsync();
        var state = await pending;
        if (!ReferenceEquals(pending, accountState)) return await GetAuthenticationStateAsync();
        if (CompanyUiRoutes.IsRepositorySurface(new Uri(navigation.Uri).AbsolutePath)) return state;
        var selected = company.Selection;
        var current = state.User.Identity?.IsAuthenticated == true ? resolveAccount(state.User) : null;
        var matched = selected is not null && selected.UserId == current;
        var result = new ClaimsPrincipal();
        var first = true;
        foreach (var identity in state.User.Identities)
        {
            var claims = identity.Claims.Where(x => x.Type is not "role" and not "roles" and not "permission" and
                not "permissions" and not "elevated_permissions" && x.Type != ClaimTypes.Role && x.Type != identity.RoleClaimType).ToList();
            if (first && matched)
            {
                claims.AddRange(selected!.Roles.Select(x => new Claim(ClaimTypes.Role, x)));
                claims.AddRange(selected.Permissions.Select(x => new Claim("permission", x)));
            }
            result.AddIdentity(new ClaimsIdentity(claims, identity.AuthenticationType, identity.NameClaimType, ClaimTypes.Role));
            first = false;
        }
        return new(result);
    }

    public void SetAuthenticationState(Task<AuthenticationState> state)
    {
        accountState = state;
        if (inner is IHostEnvironmentAuthenticationStateProvider host) host.SetAuthenticationState(state);
        else AccountChanged(state);
    }

    private void AccountChanged(Task<AuthenticationState> state) { accountState = state; if (!disposed) company.Clear(); }
    private void ScopeChanged() { if (!disposed) NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    private void LocationChanged(object? sender, LocationChangedEventArgs args) => ScopeChanged();
    public void Dispose()
    {
        disposed = true;
        inner.AuthenticationStateChanged -= AccountChanged;
        company.Changed -= ScopeChanged;
        navigation.LocationChanged -= LocationChanged;
        if (inner is IDisposable disposable) disposable.Dispose();
    }
}
