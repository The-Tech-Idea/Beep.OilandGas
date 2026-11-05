using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using Bunit;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Web.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class FirstRunNavigationGateTests
{
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static MainLayout Create(TestContext ui, Func<Task<RepositoryReadiness>> read)
    {
        var layout = new MainLayout();
        typeof(MainLayout).GetProperty("NavigationManager", Flags)!.SetValue(layout, ui.Services.GetRequiredService<NavigationManager>());
        typeof(MainLayout).GetProperty("RepositoryClient", Flags)!.SetValue(layout, new RepositoryAccountClient(read));
        typeof(MainLayout).GetProperty("Company", Flags)!.SetValue(layout, new Beep.OilandGas.Client.App.CompanyClientContext());
        return layout;
    }
    private static Task<bool> Check(MainLayout layout, string path) => (Task<bool>)typeof(MainLayout).GetMethod("EnsureFirstRunSetupGateAsync", Flags)!.Invoke(layout, [path])!;
    private static bool Allowed(MainLayout layout) => (bool)typeof(MainLayout).GetField("_setupGateAllowed", Flags)!.GetValue(layout)!;

    private static Task<AuthenticationState> User(string id) => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", id)], "test"))));
    private static Task SetAuthentication(MainLayout layout, Task<AuthenticationState> state)
    {
        typeof(MainLayout).GetProperty("AuthenticationState", Flags)!.SetValue(layout, state);
        return (Task)typeof(MainLayout).GetMethod("OnParametersSetAsync", Flags)!.Invoke(layout, null)!;
    }

    [Fact]
    public async Task CompanyDisplayIsRemovedDuringAccountChangeAndForAnotherAccount()
    {
        using var ui = new TestContext(); var layout = Create(ui, () => Task.FromResult(RepositoryReadiness.Ready));
        var company = (Beep.OilandGas.Client.App.CompanyClientContext)typeof(MainLayout).GetProperty("Company", Flags)!.GetValue(layout)!;
        Assert.True(company.TrySelect(company.Clear(), "alice", new(new("tenant", "company"), ["Engineer"], [])));
        await SetAuthentication(layout, User("alice"));
        var settings = new CompanyEnvironmentPreview(company.Selection!.Scope, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
        var review = new CompanyEnvironmentEffectiveReview(company.Selection.Scope, new string('A', 64), new string('B', 64), null, new string('C', 64), settings) { ReviewStatus = "Current" };
        typeof(MainLayout).GetMethod("SetCompanyEnvironment", Flags)!.Invoke(layout, [new CompanyEnvironmentDisplayUpdate(company.Version, review)]);
        var display = typeof(MainLayout).GetProperty("CurrentCompanyEnvironment", Flags)!;
        Assert.NotNull(display.GetValue(layout));
        var pending = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var switching = SetAuthentication(layout, pending.Task);
        Assert.Null(display.GetValue(layout));
        pending.SetResult(await User("bob")); await switching;
        Assert.Null(display.GetValue(layout));
        Assert.False((bool)typeof(MainLayout).GetProperty("_companyMatchesAccount", Flags)!.GetValue(layout)!);
    }

    [Fact]
    public async Task LateAuthenticationCannotRestoreAnOldAccount()
    {
        using var ui = new TestContext(); var layout = Create(ui, () => Task.FromResult(RepositoryReadiness.Ready));
        var pending = new TaskCompletionSource<AuthenticationState>(TaskCreationOptions.RunContinuationsAsynchronously);
        var old = SetAuthentication(layout, pending.Task);
        await SetAuthentication(layout, User("bob"));
        pending.SetResult(await User("alice")); await old;
        Assert.Equal("bob", typeof(MainLayout).GetField("_authenticatedActor", Flags)!.GetValue(layout));
    }

    [Theory]
    [InlineData("/", true)]
    [InlineData("/?return=setup", true)]
    [InlineData("/index?source=setup", true)]
    [InlineData("/setup/repository/", false)]
    [InlineData("/setup/repository/?tab=drivers", false)]
    [InlineData("/ppdm39/database-management/", false)]
    public void OnlyLandingRoutesHideApplicationNavigation(string path, bool landing)
    {
        using var ui = new TestContext();
        var layout = Create(ui, () => Task.FromResult(RepositoryReadiness.Ready));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo(path);
        typeof(MainLayout).GetMethod("CheckIfLandingPage", Flags)!.Invoke(layout, null);
        Assert.Equal(landing, (bool)typeof(MainLayout).GetField("_isLandingPage", Flags)!.GetValue(layout)!);
    }

    [Theory]
    [InlineData("/setup/repository/")]
    [InlineData("/setup/bootstrap-drivers/")]
    public async Task FirstRunRoutesRemainAccessibleWithTrailingSlash(string path)
    {
        using var ui = new TestContext();
        var calls = 0;
        var layout = Create(ui, () => { calls++; return Task.FromResult(RepositoryReadiness.Unavailable); });
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo(path);
        Assert.True(await Check(layout, path));
        Assert.True(Allowed(layout));
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("/ppdm39/database-management/")]
    [InlineData("/ppdm39/data-management/schema/")]
    public void RepositorySetupRoutesKeepRepositoryRolesWithTrailingSlash(string path)
    {
        Assert.True(CompanyUiRoutes.IsRepositorySurface(path));
        Assert.False(CompanyUiRoutes.IsRepositorySurface(path + "unexpected"));
    }

    [Fact]
    public async Task OperationalContentIsHeldUntilReadinessIsConfirmed()
    {
        using var ui = new TestContext(); var pending = new TaskCompletionSource<RepositoryReadiness>(TaskCreationOptions.RunContinuationsAsynchronously);
        var layout = Create(ui, () => pending.Task);
        var check = Check(layout, "/"); Assert.False(Allowed(layout)); Assert.False(check.IsCompleted);
        pending.SetResult(RepositoryReadiness.Ready); Assert.True(await check); Assert.True(Allowed(layout));
    }

    [Fact]
    public async Task OldFailedCheckCannotRedirectAwayFromNewSetupRoute()
    {
        using var ui = new TestContext(); var pending = new TaskCompletionSource<RepositoryReadiness>(TaskCreationOptions.RunContinuationsAsynchronously);
        var layout = Create(ui, () => pending.Task); var nav = ui.Services.GetRequiredService<NavigationManager>();
        var old = Check(layout, "/"); nav.NavigateTo("/setup/repository");
        Assert.True(await Check(layout, "/setup/repository")); Assert.True(Allowed(layout));
        pending.SetResult(RepositoryReadiness.MigrationRequired); Assert.False(await old);
        Assert.EndsWith("/setup/repository", nav.Uri); Assert.True(Allowed(layout));
    }

    [Fact]
    public async Task FailedReadinessRoutesCurrentNavigationToSetup()
    {
        using var ui = new TestContext(); var layout = Create(ui, () => Task.FromResult(RepositoryReadiness.BootstrapRequired));
        Assert.False(await Check(layout, "/")); Assert.False(Allowed(layout));
        Assert.EndsWith("/setup/repository", ui.Services.GetRequiredService<NavigationManager>().Uri);
    }

    [Fact]
    public async Task DisposedLayoutDiscardsLateReadyResponse()
    {
        using var ui = new TestContext(); var pending = new TaskCompletionSource<RepositoryReadiness>(TaskCreationOptions.RunContinuationsAsynchronously);
        var layout = Create(ui, () => pending.Task); var check = Check(layout, "/"); layout.Dispose();
        pending.SetResult(RepositoryReadiness.Ready); Assert.False(await check); Assert.False(Allowed(layout));
    }
}
#endif
