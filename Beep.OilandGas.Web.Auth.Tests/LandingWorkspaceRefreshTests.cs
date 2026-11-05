using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using System.Net;
using System.Net.Http.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components;
using Beep.OilandGas.Web.Components.Shared;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class LandingWorkspaceRefreshTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        public Task<AuthenticationState> State = For("alice");
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => State;
        public void Change(string actor, bool administrator = false) { State = For(actor, administrator); NotifyAuthenticationStateChanged(State); }
        public static Task<AuthenticationState> For(string actor, bool administrator = false) => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(administrator ? [new Claim("party_id", actor), new Claim(ClaimTypes.Role, "Administrator")] : [new Claim("party_id", actor)], "test"))));
    }
    [Fact]
    public async Task FirstTimeSetupIsAvailableToAdministratorWithoutSelectedCompany()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var auth = new Auth { State = Auth.For("alice", administrator: true) };
        var page = Render(ui, auth, new Personas(), company);
        page.WaitForAssertion(() => Assert.Contains("Open first-time setup", page.Markup));
        Assert.Contains("/setup/repository", page.Markup);
        await page.InvokeAsync(() => auth.Change("bob"));
        page.WaitForAssertion(() => Assert.DoesNotContain("Open first-time setup", page.Markup));
    }
    private sealed class Personas : IPersonaContextService
    {
        public AppUserPersona? CurrentProfile => null;
        public AppPersona? CurrentPersona { get; set; }
        public IReadOnlyList<AppPersona> Catalog => [];
        public event Action? Changed { add { } remove { } }
        public int Loads;
        public Func<Task> Load = () => Task.CompletedTask;
        public Task EnsureLoadedAsync() => ReloadAsync();
        public Task ReloadAsync() { Loads++; return Load(); }
        public Task SwitchAsync(string code) => Task.CompletedTask;
    }
    private sealed class SetupStatusHandler : HttpMessageHandler
    {
        public string Status = "SchemaReady";
        public TaskCompletionSource? Started;
        public TaskCompletionSource? Release;
        public TaskCompletionSource? CompanyStarted;
        public TaskCompletionSource? CompanyRelease;
        public int Calls;
        public bool Unavailable;
        public bool MissingDefaults;
        public bool SentCompanyHeader;
        public bool CompanyHeaderConfirmed;
        public string CompanyMode = "current";
        private CompanyEnvironmentConfiguration? saved;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++; Started?.TrySetResult();
            var companyEndpoint = request.RequestUri!.AbsolutePath.StartsWith("/api/company-environment", StringComparison.Ordinal);
            if (!companyEndpoint) SentCompanyHeader |= request.Headers.Contains(CompanyRequestHeaders.Company) || request.Headers.Contains(CompanyRequestHeaders.Tenant);
            if (Release is not null) await Release.Task;
            if (Unavailable) return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            if (companyEndpoint)
            {
                CompanyStarted?.TrySetResult();
                if (CompanyRelease is not null) await CompanyRelease.Task;
                CompanyHeaderConfirmed = request.Headers.TryGetValues(CompanyRequestHeaders.Company, out var companyNames) && companyNames.Single() == "company" &&
                    request.Headers.TryGetValues(CompanyRequestHeaders.Tenant, out var tenants) && tenants.Single() == "tenant";
                var owner = new CompanyScope("tenant", CompanyMode == "foreign" ? "foreign" : "company");
                if (request.RequestUri.AbsolutePath.EndsWith("/effective", StringComparison.Ordinal))
                {
                    var preview = new CompanyEnvironmentPreview(owner, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
                    var effective = new CompanyEnvironmentEffectiveReview(owner, saved!.Revision, new string('A', 64), null, new string('B', 64), preview)
                    { ReviewStatus = CompanyMode == "review" ? "ReviewRequired" : "Current" };
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(effective) };
                }
                saved = CompanyMode == "missing" ? null : CompanyEnvironmentIdentity.Prepare(owner, new("KW", "{}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new CompanyEnvironmentReadResult(owner, saved)) };
            }
            if (request.RequestUri!.AbsolutePath.EndsWith("/records/TOOLS/DEFAULT", StringComparison.Ordinal))
            {
                var saved = MissingDefaults ? null : SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new SystemSetupReadResult("TOOLS", "DEFAULT", saved)) };
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new SystemSetupRepositoryStatus(Status)) };
        }
    }
    [Fact]
    public void NonAdministratorDoesNotQueryTheSystemSetupEndpoint()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var handler = new SetupStatusHandler { Status = "Unconfigured" };
        var page = Render(ui, new Auth(), new Personas(), company, setup: handler);
        page.WaitForAssertion(() => Assert.Contains("Choose the company", page.Markup));
        Assert.Equal(0, handler.Calls);
    }
    [Fact]
    public void UnavailableSetupCheckKeepsTheWorkspaceAndOffersRecovery()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, new Personas(), company, setup: new SetupStatusHandler { Unavailable = true });
        page.WaitForAssertion(() => Assert.Contains("Open first-time setup", page.Markup));
        Assert.Contains("System setup could not be confirmed", page.Markup);
        Assert.DoesNotContain("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
    private static IRenderedComponent<Landing> Render(TestContext ui, Auth auth, Personas personas, CompanyClientContext company, ICompanyAccessClient? access = null, SetupStatusHandler? setup = null)
    {
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton<AuthenticationStateProvider>(auth); ui.Services.AddSingleton<IPersonaContextService>(personas);
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.AddSingleton(new ApiClient(new HttpClient(setup ?? new SetupStatusHandler()) { BaseAddress = new("https://api.test") }, Mock.Of<IFailureReporter>(), company));
        ui.Services.AddSingleton(new CompanySelectionService(access ?? Mock.Of<ICompanyAccessClient>(), auth, company, user => user.FindFirst("party_id")?.Value));
        ui.ComponentFactories.AddStub<CompanySelector>(); ui.ComponentFactories.AddStub<PersonaSelector>();
        return ui.RenderComponent<Landing>();
    }
    [Theory]
    [InlineData("Unconfigured", true)]
    [InlineData("MigrationRequired", true)]
    [InlineData("SchemaReady", false)]
    [InlineData("Unavailable", false)]
    public void AdministratorEntersSetupOnlyForConfirmedIncompleteSystemSchema(string status, bool redirects)
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var handler = new SetupStatusHandler { Status = status };
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, new Personas(), company, setup: handler);
        if (redirects) page.WaitForAssertion(() => Assert.EndsWith("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri));
        else page.WaitForAssertion(() => Assert.Contains("Open first-time setup", page.Markup));
        Assert.Equal(status == "SchemaReady" ? 2 : 1, handler.Calls);
        if (!redirects) Assert.DoesNotContain("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
    [Fact]
    public void AdministratorEntersSetupWhenSharedToolDefaultsAreMissing()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var handler = new SetupStatusHandler { MissingDefaults = true };
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, new Personas(), company, setup: handler);
        page.WaitForAssertion(() => Assert.EndsWith("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri));
        Assert.Equal(2, handler.Calls);
    }
    [Theory]
    [InlineData("Unconfigured", false, true)]
    [InlineData("SchemaReady", true, true)]
    [InlineData("SchemaReady", false, false)]
    public void SelectedCompanyDoesNotBypassSharedSetup(string status, bool missingDefaults, bool redirects)
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new CompanyScope("tenant", "company"), ["Administrator"], [])));
        var handler = new SetupStatusHandler { Status = status, MissingDefaults = missingDefaults };
        var personas = new Personas();
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, personas, company, setup: handler);
        if (redirects)
        {
            page.WaitForAssertion(() => Assert.EndsWith("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri));
            Assert.Equal(0, personas.Loads);
        }
        else page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        Assert.False(handler.SentCompanyHeader);
        if (!redirects) Assert.True(handler.CompanyHeaderConfirmed);
    }
    [Theory]
    [InlineData("missing", true)]
    [InlineData("review", true)]
    [InlineData("current", false)]
    public void CompanyAdministratorEntersSetupForMissingOrOutdatedCompanyEnvironment(string mode, bool redirects)
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new CompanyScope("tenant", "company"), ["Administrator"], [])));
        var handler = new SetupStatusHandler { CompanyMode = mode }; var personas = new Personas();
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, personas, company, setup: handler);
        if (redirects) page.WaitForAssertion(() => Assert.EndsWith("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri));
        else page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        Assert.True(handler.CompanyHeaderConfirmed);
    }
    [Fact]
    public void ForeignCompanyEnvironmentIsNotAcceptedAsSetupEvidence()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new CompanyScope("tenant", "company"), ["Administrator"], [])));
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, new Personas(), company, setup: new SetupStatusHandler { CompanyMode = "foreign" });
        page.WaitForAssertion(() => Assert.Contains("Company setup could not be confirmed", page.Markup));
        Assert.DoesNotContain("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
    [Fact]
    public void GlobalAdministratorWithoutCompanyAdministratorRoleDoesNotReadCompanySetup()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new CompanyScope("tenant", "company"), ["Viewer"], [])));
        var handler = new SetupStatusHandler(); var personas = new Personas();
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, personas, company, setup: handler);
        page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        Assert.Equal(2, handler.Calls); Assert.False(handler.CompanyHeaderConfirmed);
    }
    [Fact]
    public async Task AccountChangeDuringCompanyReadCannotRouteTheNewAccountToSetup()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new CompanyScope("tenant", "company"), ["Administrator"], [])));
        var handler = new SetupStatusHandler { CompanyMode = "missing", CompanyStarted = new(TaskCreationOptions.RunContinuationsAsynchronously), CompanyRelease = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var auth = new Auth { State = Auth.For("alice", administrator: true) };
        var page = Render(ui, auth, new Personas(), company, setup: handler);
        await handler.CompanyStarted.Task;
        await page.InvokeAsync(() => auth.Change("bob"));
        handler.CompanyRelease.SetResult();
        page.WaitForAssertion(() => Assert.Contains("Choose a company for your current account", page.Markup));
        Assert.DoesNotContain("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
    [Fact]
    public void ForeignAccountCannotUseSelectedCompanyToCheckSetup()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "other", new(new CompanyScope("tenant", "company"), ["Administrator"], [])));
        var handler = new SetupStatusHandler { Status = "Unconfigured" };
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, new Personas(), company, setup: handler);
        page.WaitForAssertion(() => Assert.Contains("Choose a company for your current account", page.Markup));
        Assert.Equal(0, handler.Calls);
    }
    [Fact]
    public void SelectedAdministratorSeesUnconfirmedSetupBeforeAutomaticWorkspaceNavigation()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new CompanyScope("tenant", "company"), ["Administrator"], [])));
        var personas = new Personas { CurrentPersona = new AppPersona { Code = "p", Name = "Production", DefaultRoute = "/production", IsActive = true } };
        var page = Render(ui, new Auth { State = Auth.For("alice", administrator: true) }, personas, company, setup: new SetupStatusHandler { Unavailable = true });
        page.WaitForAssertion(() => Assert.Contains("System setup could not be confirmed", page.Markup));
        Assert.Contains("Open first-time setup", page.Markup);
        Assert.DoesNotContain("/production", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
    [Fact]
    public async Task AccountChangeDuringSetupCheckCannotRedirectTheNewUser()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var handler = new SetupStatusHandler { Status = "Unconfigured", Started = new(TaskCreationOptions.RunContinuationsAsynchronously), Release = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        var auth = new Auth { State = Auth.For("alice", administrator: true) };
        var page = Render(ui, auth, new Personas(), company, setup: handler);
        await handler.Started.Task;
        await page.InvokeAsync(() => auth.Change("bob"));
        handler.Release.SetResult();
        page.WaitForAssertion(() => Assert.Contains("Choose the company", page.Markup));
        Assert.DoesNotContain("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri);
        Assert.DoesNotContain("Open first-time setup", page.Markup);
    }
    [Fact]
    public async Task SelectionDuringAccountLookupIsLoadedAfterTheLookupCompletes()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext();
        var pending = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var auth = new Auth { State = pending.Task }; var personas = new Personas(); var page = Render(ui, auth, personas, company);
        var scope = new CompanyScope("tenant", "company");
        await page.InvokeAsync(() => Assert.True(company.TrySelect(company.Clear(), "alice", new(scope, ["Administrator"], []))));
        pending.SetResult(await Auth.For("alice"));
        page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        page.WaitForAssertion(() => Assert.Contains("tenant / company", page.Markup));
    }
    [Fact]
    public async Task AccountChangeDuringWorkspaceReviewCannotNavigateToOldAccountWorkspace()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var scope = new CompanyScope("tenant", "company");
        Assert.True(company.TrySelect(company.Clear(), "alice", new(scope, ["Administrator"], [])));
        var pending = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var auth = new Auth(); var personas = new Personas { CurrentPersona = new AppPersona { Code = "old", Name = "Old", DefaultRoute = "/production", IsActive = true }, Load = () => pending.Task };
        var page = Render(ui, auth, personas, company); page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        await page.InvokeAsync(() => auth.Change("bob")); pending.SetResult();
        page.WaitForAssertion(() => Assert.Contains("Choose a company for your current account", page.Markup));
        Assert.DoesNotContain("/production", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task RefreshCompanyAccessRechecksMembershipAndReloadsWorkspaces(bool permitted)
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var scope = new CompanyScope("tenant", "company");
        Assert.True(company.TrySelect(company.Clear(), "alice", new(scope, ["Viewer"], [])));
        var access = new Mock<ICompanyAccessClient>();
        if (permitted) access.Setup(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(scope, ["Engineer"], []));
        else access.Setup(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>())).ThrowsAsync(new UnauthorizedAccessException());
        var auth = new Auth(); var personas = new Personas(); var page = Render(ui, auth, personas, company, access.Object);
        page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        await page.InvokeAsync(() => (Task)typeof(Landing).GetMethod("RefreshCompanyAccess", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        if (permitted)
        {
            Assert.Equal("Engineer", Assert.Single(company.Selection!.Roles));
            page.WaitForAssertion(() => Assert.Equal(2, personas.Loads));
        }
        else { Assert.Null(company.Selection); Assert.Contains("Company access could not be confirmed", page.Markup); }
        access.Verify(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>()), Times.Once);
    }
    [Fact]
    public async Task AccountChangeDuringAccessRefreshCannotSelectOldAccountsCompany()
    {
        using var company = new CompanyClientContext(); using var ui = new TestContext(); var scope = new CompanyScope("tenant", "company");
        Assert.True(company.TrySelect(company.Clear(), "alice", new(scope, ["Viewer"], [])));
        var pending = new TaskCompletionSource<RepositoryCompanyAccess>(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var access = new Mock<ICompanyAccessClient>(); access.Setup(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>())).Returns(() => { started.TrySetResult(); return pending.Task; });
        var auth = new Auth(); var personas = new Personas(); var page = Render(ui, auth, personas, company, access.Object);
        page.WaitForAssertion(() => Assert.Equal(1, personas.Loads));
        var refresh = page.InvokeAsync(() => (Task)typeof(Landing).GetMethod("RefreshCompanyAccess", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.Invoke(page.Instance, null)!);
        await started.Task;
        await page.InvokeAsync(() => auth.Change("bob"));
        pending.SetResult(new(scope, ["Administrator"], []));
        await refresh;
        Assert.Null(company.Selection);
        Assert.DoesNotContain("/production", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }
}
#endif
