#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Web.Components.Setup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using System.Security.Claims;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class FirstRunSetupGuideTests
{
    [Theory]
    [InlineData(RepositoryReadiness.BootstrapRequired, true)]
    [InlineData(RepositoryReadiness.MigrationRequired, false)]
    [InlineData(RepositoryReadiness.RecoveryRequired, false)]
    [InlineData(RepositoryReadiness.Unavailable, false)]
    public void BootstrapActionsOnlyAppearWhenRepositoryAllowsThem(RepositoryReadiness status, bool allowed)
    {
        using var ui = new TestContext();
        ui.Services.AddMudServices();
        ui.Services.AddLogging();
        var page = ui.RenderComponent<FirstRunSetupGuide>(p => p.Add(x => x.Status, status)
            .Add(x => x.LoginUrl, "/signin-return").Add(x => x.RegisterUrl, "/register-return"));
        Assert.Equal(allowed, page.Markup.Contains("/signin-return"));
        Assert.Equal(allowed, page.Markup.Contains("/register-return"));
        Assert.DoesNotContain("/admin/module-databases", page.Markup);
        Assert.Equal(status != RepositoryReadiness.RecoveryRequired, page.Markup.Contains("/setup/bootstrap-drivers"));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void AdministratorActionsRequireAdministratorRole(bool administrator)
    {
        using var ui = new TestContext();
        ui.Services.AddMudServices();
        ui.Services.AddLogging();
        ui.Services.RemoveAll<IAuthorizationService>();
        ui.Services.AddAuthorizationCore();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, administrator ? "Administrator" : "Operator")], "test");
        var state = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        var page = ui.RenderComponent<FirstRunSetupGuide>(p => p.Add(x => x.Status, RepositoryReadiness.Ready).AddCascadingValue(state));
        Assert.Equal(administrator, page.Markup.Contains("/admin/module-databases"));
        if (administrator) Assert.Contains("Operational verification and activation remain separate steps", page.Markup);
        else Assert.Contains("application administrator", page.Markup);
    }

    [Fact]
    public void AdministratorSeesTheSystemSetupNextAction()
    {
        using var ui = new TestContext(); ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, "Administrator")], "test");
        var state = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity)));
        var page = ui.RenderComponent<FirstRunSetupGuide>(p => p.Add(x => x.Status, RepositoryReadiness.Ready)
            .Add(x => x.SystemSetupStatus, "System schema ready; create the shared tool defaults.").AddCascadingValue(state));
        Assert.Contains("System schema ready; create the shared tool defaults.", page.Markup);
        Assert.Contains("An active company membership is required", page.Markup);
        Assert.Contains("/setup/initial-company", page.Markup);
        Assert.Contains("/setup/drivers", page.Markup);
        Assert.Contains("/ppdm39/database-management", page.Markup);
    }

    [Fact]
    public void SignedOutUserCanSignInToContinueSetupAfterRepositoryIsReady()
    {
        using var ui = new TestContext(); ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.Services.RemoveAll<IAuthorizationService>(); ui.Services.AddAuthorizationCore();
        var state = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
        var page = ui.RenderComponent<FirstRunSetupGuide>(p => p.Add(x => x.Status, RepositoryReadiness.Ready)
            .Add(x => x.LoginUrl, "/signin-return").AddCascadingValue(state));
        Assert.Contains("/signin-return", page.Markup);
        Assert.Contains("Sign in to continue setup", page.Markup);
        Assert.DoesNotContain("/admin/module-databases", page.Markup);
    }
}
#endif
