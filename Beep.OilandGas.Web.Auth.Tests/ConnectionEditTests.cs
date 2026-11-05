#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Net;
using System.Net.Http.Json;
using Bunit;
using Beep.OilandGas.Web.Components;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Models.Data;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdeaWeb.Diagnostics.Notifications;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class ConnectionEditTests
{
    private sealed class Handler : HttpMessageHandler
    {
        public int Tests, Updates; public UpdateConnectionRequest? Request;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Post) { Tests++; return new(HttpStatusCode.OK) { Content = JsonContent.Create(new ConnectionTestResult { Success = true }) }; }
            Updates++; Request = await request.Content!.ReadFromJsonAsync<UpdateConnectionRequest>(cancellationToken: token);
            return new(HttpStatusCode.OK) { Content = JsonContent.Create(new SaveConnectionResult { Success = true }) };
        }
    }
    [Fact]
    public async Task EditingRequiresFreshTestAndSubmitsOnlyTestedValues()
    {
        using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var company = new CompanyClientContext(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        var reporter = Mock.Of<IFailureReporter>();
        ui.Services.AddLogging(); ui.Services.AddLocalization(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new ApiClient(http, reporter, company));
        ui.Services.AddSingleton(reporter); ui.Services.AddSingleton<FailureWording>(); ui.Services.AddSingleton<OilGasApiFailures>(); ui.Services.AddSingleton<OilGasCallFailures>();
        var dialog = new Mock<IMudDialogInstance>();
        var page = ui.RenderComponent<PPDM39DatabaseWizard>(p => p.AddCascadingValue(dialog.Object)
            .Add(x => x.Connection, new ConnectionConfig { ConnectionName = "original", DatabaseType = "SqlServer", Host = "db", Database = "system" }));
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => (Task)typeof(PPDM39DatabaseWizard).GetMethod("Save", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Updates);
        await page.InvokeAsync(() => (Task)typeof(PPDM39DatabaseWizard).GetMethod("Test", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(1, handler.Tests);
        typeof(PPDM39DatabaseWizard).GetField("_database", flags)!.SetValue(page.Instance, "changed");
        await page.InvokeAsync(() => (Task)typeof(PPDM39DatabaseWizard).GetMethod("Save", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Updates);
        await page.InvokeAsync(() => (Task)typeof(PPDM39DatabaseWizard).GetMethod("Test", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(PPDM39DatabaseWizard).GetMethod("Save", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(1, handler.Updates);
        Assert.Equal("original", handler.Request!.OriginalConnectionName);
        Assert.Equal("changed", handler.Request.Connection!.Database);
        dialog.Verify(x => x.Close(It.IsAny<DialogResult>()), Times.Once);
    }
}
#endif
