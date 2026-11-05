using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public class PersonaWorkspaceTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        // The Web's principal after its claims transformation: the OilGas account the API resolved (party_id, issued here).
        public ClaimsPrincipal User = new(new ClaimsIdentity(new[]
        {
            new Claim(Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaimsTransformation<string>.ClaimType, "local"),
            new Claim(ClaimTypes.Role, "Reader")
        }, "test"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(User));
        public void SignOut() { User = new(new ClaimsIdentity()); NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }
    private sealed class Handler(Func<HttpRequestMessage, Task<HttpResponseMessage>> send) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => send(request);
    }
    private static HttpResponseMessage Json(object value) => new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value)) };
    private static PersonaContextService Context(HttpClient http, Auth auth)
    {
        var company = TestCompany.Selected("local");
        return new(new PersonaClient(new ApiClient(http, new RecordingFailureReporter(), company)), auth, company, Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaims.Find);
    }
    private static CompanyPersonaWorkspaceReview Review(string company = "test-company", string code = "ACCOUNTANT", string[]? roles = null) =>
        new(new("test-tenant", company), "local", roles ?? [], [new() { Code = code, Name = code, DefaultRoute = "/accounting/dashboard" }], new string('A',64));
    private static List<AppPersona> Catalog() => new() {
        new() { Code = "ACCOUNTANT", Name = "Accountant", DefaultRoute = "/accounting/dashboard" },
        new() { Code = "OLD", Name = "Inactive", IsActive = false }
    };

    [Fact]
    public async Task SavedPersonaUpdatesNavigationStateWithoutGrantingRoles()
    {
        var auth = new Auth();
        using var http = new HttpClient(new Handler(r => Task.FromResult(r.Method == HttpMethod.Put
            ? Json(new AppUserPersona { UserId = "local", PersonaCode = "ACCOUNTANT", ConcurrencyStamp = "new" })
            : r.RequestUri!.AbsolutePath == "/api/personas/workspaces/current" ? Json(Review()) : Json(new PersonaProfileResult(null))))) { BaseAddress = new("https://api.example") };
        using var context = Context(http, auth);
        await context.EnsureLoadedAsync();
        Assert.Single(context.Catalog);
        var changes = 0; context.Changed += () => changes++;
        await context.SwitchAsync("ACCOUNTANT");
        Assert.Equal("Accountant", context.CurrentPersona!.Name);
        Assert.Equal("new", context.CurrentProfile!.ConcurrencyStamp);
        Assert.True(changes > 0);
        Assert.True(auth.User.IsInRole("Reader")); Assert.False(auth.User.IsInRole("Accountant"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.SwitchAsync("OLD"));
    }

    [Fact]
    public async Task LateProfileCannotReappearAfterSignOut()
    {
        var auth = new Auth();
        var pending = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var http = new HttpClient(new Handler(r => {
            if (r.RequestUri!.AbsolutePath == "/api/personas/workspaces/current") return Task.FromResult(Json(Review()));
            entered.TrySetResult(); return pending.Task;
        })) { BaseAddress = new("https://api.example") };
        using var context = Context(http, auth);
        var load = context.EnsureLoadedAsync();
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        auth.SignOut();
        pending.SetResult(Json(new PersonaProfileResult(new AppUserPersona { UserId = "local", PersonaCode = "ACCOUNTANT" })));
        await load;
        Assert.Null(context.CurrentPersona); Assert.Null(context.CurrentProfile); Assert.Empty(context.Catalog);
    }

    [Fact]
    public async Task FailedReloadClearsPreviousPersonaAndCanRetry()
    {
        var fail = false;
        using var http = new HttpClient(new Handler(r => Task.FromResult(fail ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent("") }
            : r.RequestUri!.AbsolutePath == "/api/personas/workspaces/current" ? Json(Review()) : Json(new PersonaProfileResult(new AppUserPersona { UserId = "local", PersonaCode = "ACCOUNTANT" }))))) { BaseAddress = new("https://api.example") };
        using var context = Context(http, new Auth());
        await context.EnsureLoadedAsync(); Assert.NotNull(context.CurrentPersona);
        fail = true;
        await Assert.ThrowsAsync<HttpRequestException>(() => context.ReloadAsync());
        Assert.Null(context.CurrentPersona); Assert.Empty(context.Catalog);
        fail = false; await context.EnsureLoadedAsync(); Assert.NotNull(context.CurrentPersona);
    }

    [Theory]
    [InlineData(null, "/dashboard")]
    [InlineData("https://example.com", "/dashboard")]
    [InlineData("//example.com", "/dashboard")]
    [InlineData("/%2fexample.com", "/dashboard")]
    [InlineData("/\\example.com", "/dashboard")]
    [InlineData("/accounting/dashboard", "/accounting/dashboard")]
    public void WorkspaceNavigationStaysInsideTheApp(string? input, string expected)
        => Assert.Equal(expected, PersonaNavigation.HomeRoute(input));
    [Fact]
    public async Task CompanyChangeImmediatelyClearsWorkspaceAndReloadsCurrentCompanyData()
    {
        using var company = TestCompany.Selected("local"); var requests = 0;
        using var http = new HttpClient(new Handler(r => {
            requests++;
            var code = r.Headers.GetValues(CompanyRequestHeaders.Company).Single() == "test-company" ? "ACCOUNTANT" : "ENGINEER";
            return Task.FromResult(r.RequestUri!.AbsolutePath == "/api/personas/workspaces/current"
                ? Json(Review(r.Headers.GetValues(CompanyRequestHeaders.Company).Single(), code, company.Selection!.Roles.ToArray()))
                : Json(new PersonaProfileResult(new AppUserPersona { UserId = "local", PersonaCode = code })));
        })) { BaseAddress = new("https://api.example") };
        using var context = new PersonaContextService(new PersonaClient(new ApiClient(http, new RecordingFailureReporter(), company)), new Auth(), company, Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaims.Find);
        await context.EnsureLoadedAsync(); Assert.Equal("ACCOUNTANT", context.CurrentPersona!.Code);
        var version = company.Clear(); Assert.Null(context.CurrentProfile); Assert.Null(context.CurrentPersona); Assert.Empty(context.Catalog);
        Assert.True(company.TrySelect(version, "local", new(new("test-tenant", "other-company"), ["Reader"], [])));
        await context.EnsureLoadedAsync(); Assert.Equal("ENGINEER", context.CurrentPersona!.Code); Assert.Equal(4, requests);
    }
    [Fact]
    public async Task LateWorkspaceCannotReappearAfterCompanySwitchAwayAndBack()
    {
        using var company = TestCompany.Selected("local");
        var pending = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var http = new HttpClient(new Handler(r => { entered.TrySetResult(); return pending.Task; })) { BaseAddress = new("https://api.example") };
        using var context = new PersonaContextService(new PersonaClient(new ApiClient(http, new RecordingFailureReporter(), company)), new Auth(), company, Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaims.Find);
        var load = context.EnsureLoadedAsync(); await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var original = company.Selection!; var version = company.Clear();
        Assert.True(company.TrySelect(version, "local", new(new("test-tenant", "other-company"), [], [])));
        version = company.Clear(); Assert.True(company.TrySelect(version, "local", new(original.Scope, original.Roles.ToArray(), original.Permissions.ToArray())));
        pending.SetResult(Json(new PersonaProfileResult(new AppUserPersona { UserId = "local", PersonaCode = "ACCOUNTANT" })));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => load);
        Assert.Null(context.CurrentProfile); Assert.Null(context.CurrentPersona); Assert.Empty(context.Catalog);
    }
    [Theory] [InlineData(false)] [InlineData(true)]
    public async Task MissingOrDifferentAccountCompanyDoesNotLoadWorkspace(bool otherAccount)
    {
        using var company = new Beep.OilandGas.Client.App.CompanyClientContext();
        if (otherAccount) Assert.True(company.TrySelect(company.Clear(), "different", new(new("tenant", "company"), [], [])));
        using var http = new HttpClient(new Handler(_ => throw new InvalidOperationException("No API request is allowed."))) { BaseAddress = new("https://api.example") };
        using var context = new PersonaContextService(new PersonaClient(new ApiClient(http, new RecordingFailureReporter(), company)), new Auth(), company, Beep.Foundation.IdentityServer.Shared.Identity.PartyIdClaims.Find);
        await context.EnsureLoadedAsync(); Assert.Null(context.CurrentProfile); Assert.Empty(context.Catalog);
    }
    [Fact]
    public async Task ForeignAccountProfileIsRejectedBeforePublication()
    {
        using var http = new HttpClient(new Handler(r => Task.FromResult(r.RequestUri!.AbsolutePath == "/api/personas/workspaces/current"
            ? Json(Review()) : Json(new PersonaProfileResult(new AppUserPersona { UserId = "foreign", PersonaCode = "ACCOUNTANT" }))))) { BaseAddress = new("https://api.example") };
        using var context = Context(http, new Auth());
        await Assert.ThrowsAsync<InvalidOperationException>(() => context.EnsureLoadedAsync());
        Assert.Null(context.CurrentProfile); Assert.Empty(context.Catalog);
    }

}
