using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.Web.Components;
using Beep.OilandGas.Web.Services;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using MudBlazor;
using System.Net.Http.Json;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ConnectionSetupDialogTests
{
    // Blind spot: rendered Mud dialog and real API client with a recording HTTP transport; live LocalDB/browser evidence is separate.
    private static async Task<IRenderedComponent<ConnectionSetupDialog>> Open(TestContext ui)
    {
        ui.RenderComponent<MudPopoverProvider>();
        var host = ui.RenderComponent<MudDialogProvider>();
        await host.InvokeAsync(() => ui.Services.GetRequiredService<IDialogService>().ShowAsync<ConnectionSetupDialog>("Connection setup"));
        return host.FindComponent<ConnectionSetupDialog>();
    }
    private static SetupDriverItem Driver(string type = "SqlServer", bool loaded = true) => new(type + "-driver", "Test.Provider." + type, type, "RDBMS", "1.0.0", loaded, false)
    { ConnectorLoaded = loaded, ProviderLoaded = loaded, ConnectorPackage = "Test.Connector." + type, ConnectorVersion = "1.0.0", CanInstall = true };
    private static void Continue(IRenderedComponent<ConnectionSetupDialog> page) => page.FindAll("button").Single(x => x.TextContent.Trim() == "Continue").Click();
    private static void DriverChecked(IRenderedComponent<ConnectionSetupDialog> page, string type = "SqlServer") =>
        page.WaitForAssertion(() => Assert.Contains($"Driver ready for {type}", page.Markup));

    private sealed class DriverHandler : HttpMessageHandler
    {
        public bool SqlLoaded, CatalogFailure, WrongInstallReply, FailInstall, NoMatchingDriver;
        public bool InstallConfigured = true;
        public int Catalogs, Installs, DatabaseCalls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var path = request.RequestUri!.AbsolutePath;
            if (request.Method == HttpMethod.Get && path == "/api/setup/drivers")
            {
                Catalogs++;
                if (NoMatchingDriver) return Task.FromResult(Reply(new[] { Driver("Postgre") }));
                return Task.FromResult(CatalogFailure ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) : Reply(new[]
                { Driver(loaded: SqlLoaded) with { CanInstall = InstallConfigured }, Driver("Postgre", false) with { CanInstall = InstallConfigured } }));
            }
            if (path == "/api/setup/drivers/SqlServer-driver/install")
            {
                Installs++; SqlLoaded = true;
                if (FailInstall) throw new HttpRequestException("Response lost after server loaded packages");
                return Task.FromResult(Reply(WrongInstallReply ? Driver() with { Id = "different-driver" } : Driver()));
            }
            DatabaseCalls++;
            throw new InvalidOperationException("No database operation should run while checking driver readiness.");
        }
        private static HttpResponseMessage Reply<T>(T item) => new(HttpStatusCode.OK) { Content = JsonContent.Create(item) };
    }
    private static TestContext DriverUi(HttpClient http, CompanyClientContext company)
    {
        var ui = new TestContext(); ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        var reporter = Mock.Of<IFailureReporter>(); ui.Services.AddSingleton(reporter);
        ui.Services.AddSingleton(new ApiClient(http, reporter, company)); ui.Services.AddSingleton(Mock.Of<IDataManagementService>());
        ui.Services.AddSingleton(TestFailures.Calls(reporter)); return ui;
    }
    private static bool ContinueDisabled(IRenderedComponent<ConnectionSetupDialog> page) => page.FindAll("button").Single(x => x.TextContent.Trim() == "Continue").HasAttribute("disabled");

    [Fact]
    public async Task No_matching_registered_driver_does_not_accept_a_loaded_driver_for_another_database()
    {
        using var company = new CompanyClientContext(); using var handler = new DriverHandler { NoMatchingDriver = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = DriverUi(http, company);
        var page = await Open(ui); Continue(page);
        page.WaitForAssertion(() => Assert.Contains("No registered driver packages are available for SqlServer", page.Markup));
        Assert.True(ContinueDisabled(page)); Assert.Empty(page.FindAll("button[aria-label='Download and load']"));
        Assert.Equal(0, handler.Installs); Assert.Equal(0, handler.DatabaseCalls);
    }

    [Fact]
    public async Task Missing_driver_is_downloaded_inside_wizard_before_connection_details()
    {
        using var company = new CompanyClientContext(); using var handler = new DriverHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = DriverUi(http, company);
        var page = await Open(ui); Continue(page);
        page.WaitForAssertion(() => Assert.Contains("Driver required for SqlServer", page.Markup));
        Assert.True(ContinueDisabled(page)); Assert.DoesNotContain("Test.Provider.Postgre", page.Markup);
        Assert.DoesNotContain("Create and test connections", page.Markup);
        Assert.Equal(0, handler.DatabaseCalls);
        page.Find("button[aria-label='Download and load']").Click(); DriverChecked(page);
        Assert.Equal(1, handler.Installs); Assert.False(ContinueDisabled(page));
        Continue(page); Assert.Contains("Connection name", page.Markup); Assert.Equal(0, handler.DatabaseCalls);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Unconfirmed_install_blocks_continue_until_explicit_catalog_refresh(bool lostResponse)
    {
        using var company = new CompanyClientContext(); using var handler = new DriverHandler { FailInstall = lostResponse, WrongInstallReply = !lostResponse };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = DriverUi(http, company);
        var page = await Open(ui); Continue(page);
        page.WaitForAssertion(() => Assert.Contains("Driver required", page.Markup));
        page.Find("button[aria-label='Download and load']").Click();
        page.WaitForAssertion(() => Assert.Contains("installation was not confirmed", page.Markup));
        Assert.True(ContinueDisabled(page)); Assert.Equal(1, handler.Installs); Assert.Equal(1, handler.Catalogs);
        page.Find("button[aria-label='Refresh drivers']").Click(); DriverChecked(page);
        Assert.False(ContinueDisabled(page)); Assert.Equal(1, handler.Installs); Assert.Equal(2, handler.Catalogs);
        Assert.Equal(0, handler.DatabaseCalls);
    }

    [Fact]
    public async Task Catalog_outage_blocks_continue_and_refresh_recovers_without_losing_target()
    {
        using var company = new CompanyClientContext(); using var handler = new DriverHandler { CatalogFailure = true, SqlLoaded = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = DriverUi(http, company);
        var page = await Open(ui); Continue(page);
        page.WaitForAssertion(() => Assert.Contains("catalog could not be loaded", page.Markup));
        Assert.True(ContinueDisabled(page)); Assert.Equal(0, handler.Installs);
        handler.CatalogFailure = false; page.Find("button[aria-label='Refresh drivers']").Click(); DriverChecked(page);
        Assert.False(ContinueDisabled(page)); Assert.Equal(0, handler.DatabaseCalls);
    }

    [Fact]
    public async Task Changing_database_type_cannot_reuse_another_types_ready_driver()
    {
        using var company = new CompanyClientContext(); using var handler = new DriverHandler { SqlLoaded = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = DriverUi(http, company);
        var page = await Open(ui); Continue(page); DriverChecked(page);
        page.FindAll("button").Single(x => x.TextContent.Trim() == "Back").Click();
        var selector = page.FindComponent<MudSelect<string>>();
        await selector.InvokeAsync(() => selector.Instance.ValueChanged.InvokeAsync("PostgreSQL"));
        Continue(page);
        page.WaitForAssertion(() => Assert.Contains("Driver required for Postgre", page.Markup));
        Assert.True(ContinueDisabled(page)); Assert.DoesNotContain("Test.Provider.SqlServer", page.Markup);
        Assert.Equal(0, handler.Installs); Assert.Equal(0, handler.DatabaseCalls);
    }

    [Fact]
    public async Task Missing_server_package_configuration_is_explained_and_cannot_install()
    {
        using var company = new CompanyClientContext(); using var handler = new DriverHandler { InstallConfigured = false };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; using var ui = DriverUi(http, company);
        var page = await Open(ui); Continue(page);
        page.WaitForAssertion(() => Assert.Contains("Configure exact package names and versions", page.Markup));
        Assert.True(page.Find("button[aria-label='Download and load']").HasAttribute("disabled"));
        Assert.True(ContinueDisabled(page)); Assert.Equal(0, handler.Installs); Assert.Equal(0, handler.DatabaseCalls);
    }
    private sealed class TargetHandler(bool uncertain = false, bool wrongReview = false) : HttpMessageHandler
    {
        public int Creates, Tests, Saves;
        public bool Owned;
        public string? Saved;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var path = request.RequestUri!.AbsolutePath;
            if (path == "/api/setup/drivers") return Reply(new[] { Driver(), Driver("SqlLite") });
            if (path.EndsWith("review"))
            {
                var target = (await request.Content!.ReadFromJsonAsync<LocalDbDatabaseTargetRequest>(token))!;
                if (wrongReview) target = target with { Database = "DifferentDatabase" };
                return Reply(new LocalDbDatabaseTargetReview(target, "review-token", Owned, Owned, false, Owned ? "Ownership confirmed" : "Database missing"));
            }
            if (path.EndsWith("create"))
            {
                Creates++; Owned = true;
                if (uncertain) throw new HttpRequestException("Lost response after native creation");
                var intent = (await request.Content!.ReadFromJsonAsync<LocalDbDatabaseTargetCreate>(token))!;
                return Reply(new LocalDbDatabaseTargetResult(intent.Target, intent.ReviewToken, true));
            }
            if (path.EndsWith("test-connection")) { Tests++; return Reply(new ConnectionTestResult { Success = true, Message = "Connected" }); }
            Saves++; Saved = await request.Content!.ReadAsStringAsync(token);
            return Reply(new SaveConnectionResult { Success = true, Message = "Saved" });
        }
        private static HttpResponseMessage Reply<T>(T result) => new(HttpStatusCode.OK) { Content = JsonContent.Create(result) };
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Exact_review_creation_and_real_connection_test_are_required_before_save(bool uncertain)
    {
        using var company = new CompanyClientContext(); using var handler = new TargetHandler(uncertain);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        var reporter = Mock.Of<IFailureReporter>();
        ui.Services.AddSingleton(reporter); ui.Services.AddSingleton(new ApiClient(http, reporter, company));
        ui.Services.AddSingleton(Mock.Of<IDataManagementService>()); ui.Services.AddSingleton(TestFailures.Calls(reporter));
        var page = await Open(ui);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        void Set(string name, object value) => typeof(ConnectionSetupDialog).GetField(name, flags)!.SetValue(page.Instance, value);
        async Task Call(string method) => await page.InvokeAsync(() => (Task)typeof(ConnectionSetupDialog).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => { Set("_newLocalDb", true); Set("_connectionName", "NewModule"); Set("_database", "NewModule"); });
        await Call("TestConnection"); await Call("SaveConnection"); Assert.Equal(0, handler.Tests); Assert.Equal(0, handler.Saves);
        page.Render();
        Continue(page); DriverChecked(page); Continue(page); Continue(page);
        page.FindAll("button").Single(x => x.TextContent.Trim() == "Review target").Click();
        page.WaitForAssertion(() => Assert.Contains("Database missing", page.Markup));
        await Call("CreateTarget"); Assert.Equal(0, handler.Creates);
        await page.InvokeAsync(() => Set("_confirmed", true));
        await Call("CreateTarget"); await Call("CreateTarget"); Assert.Equal(1, handler.Creates);
        if (uncertain)
        {
            await Call("TestConnection"); await Call("SaveConnection"); Assert.Equal(0, handler.Tests); Assert.Equal(0, handler.Saves);
            page.Render();
            Assert.Contains("Review creation outcome", page.Markup);
            await Call("ReviewTarget");
        }
        await Call("SaveConnection"); Assert.Equal(0, handler.Saves);
        await Call("TestConnection"); Assert.Equal(1, handler.Tests);
        await Call("SaveConnection"); Assert.Equal(1, handler.Saves);
        Assert.Contains("\"openAfterSave\":false", handler.Saved!, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("NewModule", handler.Saved!);
    }

    [Fact]
    public async Task Review_for_a_different_target_never_authorizes_creation()
    {
        using var company = new CompanyClientContext(); using var handler = new TargetHandler(wrongReview: true);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.Services.AddMudServices(); ui.Services.AddLogging(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        var reporter = Mock.Of<IFailureReporter>(); ui.Services.AddSingleton(reporter);
        ui.Services.AddSingleton(new ApiClient(http, reporter, company)); ui.Services.AddSingleton(Mock.Of<IDataManagementService>()); ui.Services.AddSingleton(TestFailures.Calls(reporter));
        var page = await Open(ui);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        void Set(string name, object value) => typeof(ConnectionSetupDialog).GetField(name, flags)!.SetValue(page.Instance, value);
        async Task Call(string method) => await page.InvokeAsync(() => (Task)typeof(ConnectionSetupDialog).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => { Set("_newLocalDb", true); Set("_connectionName", "Module"); Set("_database", "Module"); });
        Continue(page); DriverChecked(page);
        await Call("ReviewTarget"); await page.InvokeAsync(() => Set("_confirmed", true)); await Call("CreateTarget");
        Assert.Equal(0, handler.Creates); Assert.Equal(0, handler.Tests); Assert.Equal(0, handler.Saves);
    }

    private sealed class Handler : HttpMessageHandler
    {
        public int Saves;
        public string? TestedBody;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.RequestUri!.AbsolutePath == "/api/setup/drivers") return new(HttpStatusCode.OK) { Content = JsonContent.Create(new[] { Driver(), Driver("SqlLite") }) };
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
        ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddSingleton(Mock.Of<IDataManagementService>()); ui.Services.AddSingleton(TestFailures.Calls(Mock.Of<IFailureReporter>()));
        var page = await Open(ui);
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        void Set(string name, object value) => typeof(ConnectionSetupDialog).GetField(name, flags)!.SetValue(page.Instance, value);
        await page.InvokeAsync(() => { Set("_selectedDatabaseType", "SQLite"); Set("_connectionName", "system"); Set("_host", ""); Set("_database", "C:\\data\\system.db"); });
        page.Render(); Continue(page); DriverChecked(page, "SqlLite");
        Assert.True((bool)typeof(ConnectionSetupDialog).GetMethod("CanTestConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(ConnectionSetupDialog).GetMethod("TestConnection", flags)!.Invoke(page.Instance, null)!);
        Assert.Contains("system.db", handler.TestedBody);
        Assert.True((bool)typeof(ConnectionSetupDialog).GetMethod("TestMatches", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => Set("_database", "C:\\data\\different.db"));
        await page.InvokeAsync(() => (Task)typeof(ConnectionSetupDialog).GetMethod("SaveConnection", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Saves);
    }
}
