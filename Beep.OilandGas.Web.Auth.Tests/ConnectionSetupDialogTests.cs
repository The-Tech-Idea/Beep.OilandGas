#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Web.Components;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ConnectionSetupDialogTests
{
    private sealed class Handler : HttpMessageHandler
    {
        public int Saves;
        public string? TestedBody;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.RequestUri!.AbsolutePath.EndsWith("test-connection")) TestedBody = await request.Content!.ReadAsStringAsync(token);
            else Saves++;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new ConnectionTestResult { Success = true, Message = "Connected" }), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task SqliteConnectionCanBeTestedAndChangedSettingsCannotBeSaved()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.Services.AddMudServices(); ui.Services.AddLogging();
        ui.Services.AddSingleton(Mock.Of<IFailureReporter>());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        ui.Services.AddSingleton(Mock.Of<IDataManagementService>()); ui.Services.AddSingleton<OilGasCallFailures>();
        var page = ui.RenderComponent<ConnectionSetupDialog>(p => p.Add(x => x.ConnectionOnly, true));
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        void Set(string name, object value) => typeof(ConnectionSetupDialog).GetField(name, flags)!.SetValue(page.Instance, value);
        await page.InvokeAsync(() => { Set("_selectedDatabaseType", "SQLite"); Set("_connectionName", "system"); Set("_host", ""); Set("_database", "C:\\data\\system.db"); });
        Assert.True((bool)typeof(ConnectionSetupDialog).GetMethod("CanTestConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(ConnectionSetupDialog).GetMethod("TestConnection", flags)!.Invoke(page.Instance, null)!);
        Assert.Contains("system.db", handler.TestedBody);
        Assert.True((bool)typeof(ConnectionSetupDialog).GetMethod("TestMatches", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => Set("_database", "C:\\data\\different.db"));
        await page.InvokeAsync(() => (Task)typeof(ConnectionSetupDialog).GetMethod("SaveConnection", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Saves);
    }
}
#endif
