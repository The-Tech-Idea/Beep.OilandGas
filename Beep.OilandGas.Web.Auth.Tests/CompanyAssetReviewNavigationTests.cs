#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.Admin.AccessControl;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyAssetReviewNavigationTests
{
    [Fact]
    public void SelectedSubjectIsPrefilledWithoutLoadingGrantsAndInvalidInputClearsIt()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        var reads = 0;
        using var state = new AssetGrantAdministrationState(company,
            (_, _) => { reads++; return Task.FromResult<List<Beep.OilandGas.Models.Data.AssetAccess>?>([]); },
            (_, _, _) => Task.FromResult(true), (_, _, _) => Task.FromResult(true),
            (_, _, _) => "unavailable", Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(state);
        var navigation = ui.Services.GetRequiredService<NavigationManager>();
        navigation.NavigateTo("/admin/access-control/asset-access?user=disabled-user");
        var page = ui.RenderComponent<AssetAccess>();
        Assert.Equal("disabled-user", state.UserId); Assert.Equal(0, reads); Assert.False(state.Loaded);
        navigation.NavigateTo("/admin/access-control/asset-access?user=invalid%0Auser");
        page.WaitForAssertion(() => Assert.Equal("", state.UserId)); Assert.Equal(0, reads);
        navigation.NavigateTo("/admin/access-control/asset-access?user=another-user");
        page.WaitForAssertion(() => Assert.Equal("another-user", state.UserId));
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "other"), ["Administrator"], [])));
        page.WaitForAssertion(() => Assert.Equal("", state.UserId)); Assert.Equal(0, reads);
    }
}
#endif
