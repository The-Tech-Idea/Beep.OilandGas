#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class RepositorySetupPageTests
{
    private sealed class State(ClaimsPrincipal initial) : AuthenticationStateProvider
    {
        private ClaimsPrincipal principal = initial;
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(principal));
        public void Change(ClaimsPrincipal value) { principal = value; NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }

    private sealed class Handler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            object result = request.RequestUri!.AbsolutePath.EndsWith("status", StringComparison.Ordinal)
                ? new SystemSetupRepositoryStatus("SchemaReady")
                : new SystemSetupReadResult("TOOLS", "DEFAULT", null);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    private sealed class EnvironmentHandler(string mode) : HttpMessageHandler
    {
        public int EnvironmentCalls;
        private CompanyEnvironmentConfiguration? saved;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.RequestUri!.AbsolutePath.EndsWith("/api/company-environment", StringComparison.Ordinal))
            {
                EnvironmentCalls++;
                var selected = request.Headers.GetValues(CompanyRequestHeaders.Company).Single();
                var owner = new CompanyScope("tenant", mode == "foreign" ? "foreign" : selected);
                saved = mode == "missing" ? null : CompanyEnvironmentIdentity.Prepare(owner, new("KW", "{}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null);
                result = new CompanyEnvironmentReadResult(owner, saved);
            }
            else if (request.RequestUri.AbsolutePath.EndsWith("/api/company-environment/effective", StringComparison.Ordinal))
            {
                var owner = mode == "effective-foreign" ? new CompanyScope("tenant", "foreign") : saved!.Company;
                var revision = mode == "effective-stale" ? new string('D', 64) : saved!.Revision;
                var settings = new CompanyEnvironmentPreview(owner, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
                result = new CompanyEnvironmentEffectiveReview(owner, revision, new string('A', 64), null, new string('B', 64), settings)
                    { ReviewStatus = mode == "review" ? "ReviewRequired" : mode == "unknown" ? "Unconfirmed" : "Current" };
            }
            else if (request.RequestUri.AbsolutePath.EndsWith("/api/company-environment/sources", StringComparison.Ordinal))
            {
                var owner = mode == "source-foreign" ? new CompanyScope("tenant", "foreign") : saved!.Company;
                var effectiveRevision = mode == "source-stale" ? new string('D', 64) : new string('B', 64);
                var sources = mode == "source-empty" ? Array.Empty<CompanyEnvironmentSource>() : new[]
                {
                    new CompanyEnvironmentSource("wells", "PPDM_CORE", "ppdm39", 0, 0, mode == "source-unbound" ? "Unavailable" : "Bound")
                    { MappingStatus = "ProfileChecked" }
                };
                result = new CompanyEnvironmentSourceReview(owner, saved!.Revision, sources, false, effectiveRevision);
            }
            else if (request.RequestUri.AbsolutePath.EndsWith("status", StringComparison.Ordinal)) result = new SystemSetupRepositoryStatus("SchemaReady");
            else result = new SystemSetupReadResult("TOOLS", "DEFAULT", null);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    [Theory]
    [InlineData("saved", "1 of 1 bound")]
    [InlineData("source-unbound", "0 of 1 bound")]
    [InlineData("source-empty", "No company data sources are configured")]
    [InlineData("source-foreign", "Company source bindings could not be confirmed")]
    [InlineData("source-stale", "Company source bindings could not be confirmed")]
    public void GuideShowsOnlyConfirmedBindingEvidence(string mode, string expected)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new EnvironmentHandler(mode); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test"));
        ui.Services.AddSingleton<AuthenticationStateProvider>(new State(principal)); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new RepositoryAccountClient(() => Task.FromResult(RepositoryReadiness.Ready)));
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<RepositorySetup>(p => p.AddCascadingValue(Task.FromResult(new AuthenticationState(principal))));
        page.WaitForAssertion(() => Assert.Contains(expected, page.Markup));
        if (mode is "saved" or "source-unbound") Assert.Contains("does not test source data or connectivity", page.Markup);
        Assert.Contains("activation is not confirmed", page.Markup);
    }

    [Theory]
    [InlineData("missing", "No environment is saved for the selected company")]
    [InlineData("saved", "shared settings match the company review")]
    [InlineData("review", "shared or country settings changed")]
    [InlineData("unknown", "environment could not be confirmed")]
    [InlineData("effective-foreign", "environment could not be confirmed")]
    [InlineData("effective-stale", "environment could not be confirmed")]
    [InlineData("foreign", "environment could not be confirmed")]
    public void GuideShowsOnlyConfirmedSelectedCompanyEnvironment(string mode, string expected)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new EnvironmentHandler(mode); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test")); var state = new State(principal);
        ui.Services.AddSingleton<AuthenticationStateProvider>(state); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new RepositoryAccountClient(() => Task.FromResult(RepositoryReadiness.Ready)));
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<RepositorySetup>(p => p.AddCascadingValue(Task.FromResult(new AuthenticationState(principal))));
        page.WaitForAssertion(() => Assert.Contains(expected, page.Markup)); Assert.Equal(1, handler.EnvironmentCalls);
        if (mode is "saved" or "review") Assert.Contains("activation is not confirmed", page.Markup);
        if (mode == "foreign") Assert.DoesNotContain("Country KW is saved", page.Markup);
        page.InvokeAsync(company.Clear); page.WaitForAssertion(() => Assert.DoesNotContain("Country KW is saved", page.Markup));
    }

    private sealed class DelayedCompanyHandler : HttpMessageHandler
    {
        public TaskCompletionSource<HttpResponseMessage> Pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int CompanyCalls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("/api/company-environment", StringComparison.Ordinal)) { CompanyCalls++; return Pending.Task; }
            object result = request.RequestUri.AbsolutePath.EndsWith("status", StringComparison.Ordinal) ? new SystemSetupRepositoryStatus("SchemaReady") : new SystemSetupReadResult("TOOLS", "DEFAULT", null);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    [Fact]
    public async Task LateCompanyEnvironmentCannotAppearAfterCompanyIsCleared()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new DelayedCompanyHandler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test")); var state = new State(principal);
        ui.Services.AddSingleton<AuthenticationStateProvider>(state); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new RepositoryAccountClient(() => Task.FromResult(RepositoryReadiness.Ready))); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<RepositorySetup>(p => p.AddCascadingValue(Task.FromResult(new AuthenticationState(principal))));
        page.WaitForAssertion(() => Assert.Equal(1, handler.CompanyCalls)); await page.InvokeAsync(company.Clear);
        var scope = new CompanyScope("tenant", "company"); var saved = CompanyEnvironmentIdentity.Prepare(scope, new("KW", "{}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null);
        handler.Pending.SetResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new CompanyEnvironmentReadResult(scope, saved)), Encoding.UTF8, "application/json") });
        page.WaitForAssertion(() => Assert.Contains("Select a company to review its environment", page.Markup));
        Assert.DoesNotContain("Country KW is saved", page.Markup);
    }

    private sealed class DelayedHandler : HttpMessageHandler
    {
        public TaskCompletionSource<HttpResponseMessage> Pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        { Calls++; return Pending.Task; }
    }

    [Theory]
    [InlineData("Administrator", true)]
    [InlineData("Operator", false)]
    public void RoutedPageLoadsSystemSetupOnlyForAdministrators(string role, bool administrator)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], []))); using var handler = new Handler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var principal = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], "test"));
        var state = new State(principal);
        ui.Services.AddSingleton<AuthenticationStateProvider>(state); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new RepositoryAccountClient(() => Task.FromResult(RepositoryReadiness.Ready)));
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<RepositorySetup>(p => p.AddCascadingValue(Task.FromResult(new AuthenticationState(principal))));
        page.WaitForAssertion(() => Assert.Contains(administrator ? "create the shared tool defaults" : "application administrator", page.Markup));
        Assert.Equal(administrator ? 3 : 0, handler.Calls);
    }

    [Fact]
    public void LosingAdministratorRoleClearsThePreviouslyLoadedStatus()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var admin = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test"));
        var state = new State(admin); ui.Services.AddSingleton<AuthenticationStateProvider>(state); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new RepositoryAccountClient(() => Task.FromResult(RepositoryReadiness.Ready)));
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<RepositorySetup>(p => p.AddCascadingValue(Task.FromResult(new AuthenticationState(admin))));
        page.WaitForAssertion(() => Assert.Contains("create the shared tool defaults", page.Markup));
        var operatorUser = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Operator")], "test"));
        page.InvokeAsync(() => state.Change(operatorUser));
        page.WaitForAssertion(() => Assert.DoesNotContain("create the shared tool defaults", page.Markup));
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task LateAdministratorResponseIsDiscardedAfterRoleChange()
    {
        using var company = new CompanyClientContext(); using var handler = new DelayedHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var admin = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test"));
        var state = new State(admin); ui.Services.AddSingleton<AuthenticationStateProvider>(state); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new RepositoryAccountClient(() => Task.FromResult(RepositoryReadiness.Ready)));
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<RepositorySetup>(p => p.AddCascadingValue(Task.FromResult(new AuthenticationState(admin))));
        page.WaitForAssertion(() => Assert.Equal(1, handler.Calls));
        await page.InvokeAsync(() => state.Change(new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Operator")], "test"))));
        handler.Pending.SetResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new SystemSetupRepositoryStatus("MigrationRequired")), Encoding.UTF8, "application/json") });
        await page.InvokeAsync(() => Task.CompletedTask);
        page.WaitForAssertion(() => Assert.DoesNotContain("Review and apply the system setup migration", page.Markup));
        Assert.Equal(1, handler.Calls);
    }
}
#endif
