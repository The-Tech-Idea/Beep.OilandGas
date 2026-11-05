using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class SetupDriversTests
{
    [Fact]
    public async Task BootstrapSearchFindsConfiguredConnectorAndExplainsNoMatches()
    {
        using var http = new HttpClient(new ConnectionHandler()) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        typeof(SetupBootstrapDrivers).GetField("_items", flags)!.SetValue(page.Instance, new List<SetupDriverItem>
        {
            new("one", "SqlProvider", "SqlServer", "RDBMS", "1.0", false, false) { ConnectorPackage = "Beep.SqlConnector", CanInstall = true },
            new("two", "OtherProvider", "Oracle", "RDBMS", "1.0", false, false) { ConnectorPackage = "Beep.OracleConnector", CanInstall = true }
        });
        typeof(SetupBootstrapDrivers).GetField("_driverSearch", flags)!.SetValue(page.Instance, " sqlconnector ");
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Beep.SqlConnector", page.Markup);
        Assert.DoesNotContain("Beep.OracleConnector", page.Markup);
        typeof(SetupBootstrapDrivers).GetField("_driverSearch", flags)!.SetValue(page.Instance, "missing");
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("No drivers match this search", page.Markup);
        Assert.DoesNotContain("Beep.SqlConnector", page.Markup);
    }

    [Fact]
    public async Task BootstrapCatalogExplainsWhenSqlServerCannotBackSystemRepository()
    {
        using var http = new HttpClient(new ConnectionHandler()) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>();
        var field = typeof(SetupBootstrapDrivers).GetField("_items", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var item = new SetupDriverItem("sql", "provider", "SqlServer", "RDBMS", "1.0", true, false)
            { ConnectorLoaded = true, ProviderLoaded = true, SqlServerOwnedTransactionContract = false };
        field.SetValue(page.Instance, new List<SetupDriverItem> { item });
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("lacks the owned transaction contract", page.Markup);
        field.SetValue(page.Instance, new List<SetupDriverItem> { item with { SqlServerOwnedTransactionContract = true } });
        await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain("lacks the owned transaction contract", page.Markup);
    }
    private static async Task SelectLoadedDriver(IRenderedComponent<SetupBootstrapDrivers> page, string provider)
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var type = provider switch { "SQLSERVER" => "SqlServer", "POSTGRESQL" => "Postgre", "ORACLE" => "Oracle", _ => throw new ArgumentOutOfRangeException(nameof(provider)) };
        var item = new SetupDriverItem("selected-driver", "Test.Provider", type, "RDBMS", "1.0", true, false)
            { ConnectorLoaded = true, ProviderLoaded = true };
        typeof(SetupBootstrapDrivers).GetField("_items", flags)!.SetValue(page.Instance, new List<SetupDriverItem> { item });
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("SelectDriver", flags)!.Invoke(page.Instance, [item]));
    }
    private sealed class ConnectionHandler : HttpMessageHandler
    {
        public SetupRepositoryConnectionSelection? Selected;
        public SetupRepositoryConnectionCreate? Created;
        public bool CatalogUnavailable;
        public bool WrongCatalogProvider;
        public bool LoseCreateResponse;
        public bool TestConnected;
        public string? TestKey;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Get)
            {
                if (CatalogUnavailable) return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                result = new[] { new SetupRepositoryConnectionOption("identity_db", WrongCatalogProvider ? "OTHER" : Created?.Provider ?? "SQLSERVER") };
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("/connections/test", StringComparison.Ordinal))
            {
                TestKey = request.Headers.GetValues("X-Setup-Key").Single();
                var selection = await request.Content!.ReadFromJsonAsync<SetupRepositoryConnectionSelection>(cancellationToken: token);
                result = new SetupRepositoryConnectionTest(selection!.ConnectionName, TestConnected);
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("/connections", StringComparison.Ordinal))
            { Created = await request.Content!.ReadFromJsonAsync<SetupRepositoryConnectionCreate>(cancellationToken: token); Selected = new(Created!.ConnectionName); if (LoseCreateResponse) return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable); result = Selected; }
            else { Selected = await request.Content!.ReadFromJsonAsync<SetupRepositoryConnectionSelection>(cancellationToken: token); result = Selected!; }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Theory]
    [InlineData(true, "opened successfully")]
    [InlineData(false, "could not be opened")]
    public async Task BootstrapTestsSelectedConnectionWithTemporaryKey(bool connected, string message)
    {
        using var handler = new ConnectionHandler { TestConnected = connected };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("LoadConnections", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeConnection", flags)!.Invoke(page.Instance, ["identity_db"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("TestConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal("temporary-key", handler.TestKey); Assert.Contains(message, page.Markup);
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeConnection", flags)!.Invoke(page.Instance, [""]));
        await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain(message, page.Markup);
    }

    [Fact]
    public async Task UnconfirmedConnectionCreationDirectsTheOperatorToCheckTheCatalogFirst()
    {
        using var handler = new ConnectionHandler { LoseCreateResponse = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await SelectLoadedDriver(page, "SQLSERVER");
        typeof(SetupBootstrapDrivers).GetField("_newName", flags)!.SetValue(page.Instance, "identity_db");
        typeof(SetupBootstrapDrivers).GetField("_newHost", flags)!.SetValue(page.Instance, "db.example");
        typeof(SetupBootstrapDrivers).GetField("_newDatabase", flags)!.SetValue(page.Instance, "reservoir");
        typeof(SetupBootstrapDrivers).GetField("_newAuthentication", flags)!.SetValue(page.Instance, "PASSWORD");
        typeof(SetupBootstrapDrivers).GetField("_newUser", flags)!.SetValue(page.Instance, "setup_user");
        typeof(SetupBootstrapDrivers).GetField("_newPassword", flags)!.SetValue(page.Instance, "temporary-password");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("CreateConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.NotNull(handler.Created);
        Assert.Contains("Find registered connections before retrying", page.Markup);
        Assert.Contains("server credential protection", page.Markup);
        Assert.Equal("", typeof(SetupBootstrapDrivers).GetField("_newPassword", flags)!.GetValue(page.Instance));
        Assert.False((bool)typeof(SetupBootstrapDrivers).GetField("_connectionsLoaded", flags)!.GetValue(page.Instance)!);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("LoadConnections", flags)!.Invoke(page.Instance, null)!);
        Assert.True((bool)typeof(SetupBootstrapDrivers).GetField("_connectionsLoaded", flags)!.GetValue(page.Instance)!);
    }
    private sealed class MigrationHandler : HttpMessageHandler
    {
        public SetupRepositoryMigrationRequest? Applied; public string? SetupKey; public bool Conflict;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            SetupKey = request.Headers.GetValues("X-Setup-Key").Single();
            if (Conflict) return new HttpResponseMessage(HttpStatusCode.Conflict);
            object result;
            if (request.Method == HttpMethod.Get) result = new SetupRepositoryMigrationPlan("provider", "target", true, ["first", "second"]);
            else { Applied = await request.Content!.ReadFromJsonAsync<SetupRepositoryMigrationRequest>(cancellationToken: token); result = new SetupRepositoryMigrationResult(RepositoryReadiness.BootstrapRequired); }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }
    [Fact]
    public async Task BootstrapExplainsMigrationConflictWithoutOfferingApply()
    {
        using var handler = new MigrationHandler { Conflict = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("ReviewRepository", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("account-wide persona settings may need a reviewed company assignment", page.Markup);
        Assert.Null(typeof(SetupBootstrapDrivers).GetField("_plan", flags)!.GetValue(page.Instance));
        Assert.DoesNotContain("Apply reviewed migrations", page.Markup);
    }
    private sealed class Handler : HttpMessageHandler
    {
        public int Installs; public bool Confirm; public bool Bundled; public bool ChangeConnectorVersion; public bool MissingVersion; public bool EmptyCatalog; public string? SetupKey; public string Type = "SqlLite"; public SetupDriverItem? Reviewed;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Headers.TryGetValues("X-Setup-Key", out var keys)) SetupKey = keys.Single();
            object value;
            if (request.Method == HttpMethod.Get) value = EmptyCatalog ? Array.Empty<SetupDriverItem>() : new[] { new SetupDriverItem("id", "Beep.Driver", Type, "RDBMS", "1.2", Bundled, false) { CanInstall = !MissingVersion, ConnectorPackage = "Beep.Connector", ConnectorVersion = MissingVersion ? null : "4.0", BundledConnector = Bundled, ConnectorLoaded = Bundled, ProviderLoaded = Bundled } };
            else { Installs++; Reviewed = await request.Content!.ReadFromJsonAsync<SetupDriverItem>(cancellationToken: token); value = new SetupDriverItem("id", "Beep.Driver", Type, "RDBMS", "1.2", Confirm, true) { CanInstall = true, ConnectorPackage = "Beep.Connector", ConnectorVersion = ChangeConnectorVersion ? "5.0" : "4.0", ConnectorLoaded = Confirm, ProviderLoaded = Confirm }; }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json") };
        }
    }
    [Fact]
    public async Task EmptyBootstrapCatalogExplainsHowToRecoverAndClearsOnKeyChange()
    {
        using var handler = new Handler { EmptyCatalog = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Assert.DoesNotContain("driver catalog is empty", page.Markup);
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("driver catalog is empty", page.Markup);
        Assert.Contains("register database drivers", page.Markup);
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["other-key"]));
        await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain("driver catalog is empty", page.Markup);
    }
    [Fact]
    public async Task BootstrapExplainsWhyUnversionedConnectorCannotBeInstalled()
    {
        using var handler = new Handler { MissingVersion = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Connector version: not specified", page.Markup);
        Assert.Contains("Configure exact package names and versions", page.Markup);
        var item = Assert.Single((List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [item])!);
        Assert.Equal(0, handler.Installs);
    }
    [Fact]
    public async Task BootstrapDistinguishesConnectorsThatCannotHostIdentity()
    {
        using var handler = new Handler { Bundled = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("This connector is not available for the identity repository", page.Markup);
        Assert.DoesNotContain("Use for identity connection", page.Markup);
    }
    [Fact]
    public async Task BootstrapIdentifiesBundledConnectorWithoutOfferingDownload()
    {
        using var handler = new Handler { Bundled = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("bundled with the API server", page.Markup);
        Assert.DoesNotContain("Beep connector: Beep.Connector", page.Markup);
        var items = (List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [items[0]])!);
        Assert.Equal(0, handler.Installs);
    }
    [Theory][InlineData(true)][InlineData(false)]
    public async Task InstallationRequiresConfirmedLoadedResult(bool confirmed)
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { Confirm = confirmed }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<SetupDrivers>(); page.WaitForAssertion(() => Assert.Contains("Beep.Driver", page.Markup));
        Assert.Contains("Beep connector: Beep.Connector", page.Markup);
        Assert.Contains("Connector version: 4.0", page.Markup);
        Assert.Contains("Database provider: Beep.Driver", page.Markup);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance; var items = (List<SetupDriverItem>)typeof(SetupDrivers).GetField("_items", flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => (Task)typeof(SetupDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [items[0]])!); await page.InvokeAsync(() => page.Render());
        Assert.Equal(1, handler.Installs); Assert.Equal(items[0], handler.Reviewed); Assert.Contains(confirmed ? "Driver loaded" : "installation was not confirmed", page.Markup);
        if (!confirmed)
        {
            Assert.Empty((List<SetupDriverItem>)typeof(SetupDrivers).GetField("_items", flags)!.GetValue(page.Instance)!);
            await page.InvokeAsync(() => (Task)typeof(SetupDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [items[0]])!);
            Assert.Equal(1, handler.Installs);
            await page.InvokeAsync(() => (Task)typeof(SetupDrivers).GetMethod("Reload", flags)!.Invoke(page.Instance, null)!);
            Assert.Single((List<SetupDriverItem>)typeof(SetupDrivers).GetField("_items", flags)!.GetValue(page.Instance)!);
        }
    }

    [Fact]
    public async Task BootstrapRequiresTheConnectorVersionShownInTheCatalog()
    {
        using var handler = new Handler { Confirm = true, ChangeConnectorVersion = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        var item = Assert.Single((List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [item])!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Installation was not confirmed", page.Markup);
        Assert.Empty((List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [item])!);
        Assert.Equal(1, handler.Installs);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        Assert.Single((List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!);
    }

    [Fact]
    public async Task BootstrapListsAndInstallsBothPackagesWithTheTemporaryKey()
    {
        using var handler = new Handler { Confirm = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Beep connector: Beep.Connector", page.Markup);
        Assert.Contains("Database provider: Beep.Driver", page.Markup);
        var items = (List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [items[0]])!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal("temporary-key", handler.SetupKey);
        Assert.Equal(1, handler.Installs);
        Assert.Equal(items[0], handler.Reviewed);
        Assert.Contains("Connector and provider loaded", page.Markup);
    }

    [Theory]
    [InlineData("SqlServer", "SQLSERVER")]
    [InlineData("Postgre", "POSTGRESQL")]
    [InlineData("Oracle", "ORACLE")]
    public async Task InstalledDriverCanBeSelectedForIdentityConnection(string type, string provider)
    {
        using var handler = new Handler { Confirm = true, Type = type };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        var items = (List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("SelectDriver", flags)!.Invoke(page.Instance, [items[0]]));
        Assert.Equal("SQLSERVER", typeof(SetupBootstrapDrivers).GetField("_newProvider", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("Install", flags)!.Invoke(page.Instance, [items[0]])!);
        Assert.Equal(provider, typeof(SetupBootstrapDrivers).GetField("_newProvider", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Enter the identity repository connection details", page.Markup);
        items = (List<SetupDriverItem>)typeof(SetupBootstrapDrivers).GetField("_items", flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("SelectDriver", flags)!.Invoke(page.Instance, [items[0]]));
        await page.InvokeAsync(() => page.Render());
        Assert.Equal(provider, typeof(SetupBootstrapDrivers).GetField("_newProvider", flags)!.GetValue(page.Instance));
        Assert.Contains("Driver selected", page.Markup);
    }

    [Fact]
    public async Task BootstrapRequiresReviewAndSubmitsTheExactMigrationPlan()
    {
        using var handler = new MigrationHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("ReviewRepository", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("first", page.Markup);
        Assert.Contains("will create it", page.Markup);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("ApplyRepository", flags)!.Invoke(page.Instance, null)!);
        Assert.Null(handler.Applied);
        typeof(SetupBootstrapDrivers).GetField("_reviewed", flags)!.SetValue(page.Instance, true);
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("ApplyRepository", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal("temporary-key", handler.SetupKey);
        Assert.Equal("target", handler.Applied!.TargetToken);
        Assert.True(handler.Applied.CreatesDatabase);
        Assert.Equal(new[] { "first", "second" }, handler.Applied.PendingMigrations);
        Assert.Contains("BootstrapRequired", page.Markup);
    }

    [Fact]
    public async Task BootstrapSelectsOnlyAnEntryFromTheRegisteredCatalog()
    {
        using var handler = new ConnectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("LoadConnections", flags)!.Invoke(page.Instance, null)!);
        typeof(SetupBootstrapDrivers).GetField("_connection", flags)!.SetValue(page.Instance, "other");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("SelectConnection", flags)!.Invoke(page.Instance, null)!);
        Assert.Null(handler.Selected);
        typeof(SetupBootstrapDrivers).GetField("_connection", flags)!.SetValue(page.Instance, "identity_db");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("SelectConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal("identity_db", handler.Selected!.ConnectionName);
        Assert.Contains("Identity repository connection selected", page.Markup);
    }

    [Theory]
    [InlineData("SQLSERVER")]
    [InlineData("POSTGRESQL")]
    [InlineData("ORACLE")]
    public async Task BootstrapCreatesConnectionAndClearsTheEnteredSecretAfterConfirmation(string provider)
    {
        using var handler = new ConnectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>();
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeProvider", flags)!.Invoke(page.Instance, [provider]));
        await SelectLoadedDriver(page, provider);
        typeof(SetupBootstrapDrivers).GetField("_newName", flags)!.SetValue(page.Instance, "identity_db");
        typeof(SetupBootstrapDrivers).GetField("_newHost", flags)!.SetValue(page.Instance, "db.example");
        typeof(SetupBootstrapDrivers).GetField("_newDatabase", flags)!.SetValue(page.Instance, "reservoir");
        typeof(SetupBootstrapDrivers).GetField("_newAuthentication", flags)!.SetValue(page.Instance, "PASSWORD");
        typeof(SetupBootstrapDrivers).GetField("_newUser", flags)!.SetValue(page.Instance, "operator");
        typeof(SetupBootstrapDrivers).GetField("_newPassword", flags)!.SetValue(page.Instance, "secret;quoted");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("CreateConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal("identity_db", handler.Selected!.ConnectionName);
        Assert.Equal("selected-driver", handler.Created!.DriverId);
        Assert.Equal("identity_db", typeof(SetupBootstrapDrivers).GetField("_connection", flags)!.GetValue(page.Instance));
        Assert.True((bool)typeof(SetupBootstrapDrivers).GetField("_connectionsLoaded", flags)!.GetValue(page.Instance)!);
        var fields = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = handler.Created!.ConnectionString };
        Assert.Equal("secret;quoted", fields["Password"]);
        if (provider == "SQLSERVER")
            Assert.Equal("db.example", new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(handler.Created.ConnectionString).DataSource);
        if (provider == "POSTGRESQL")
            Assert.Equal("db.example", new Npgsql.NpgsqlConnectionStringBuilder(handler.Created.ConnectionString).Host);
        if (provider == "ORACLE") Assert.Equal("//db.example:1521/reservoir", fields["Data Source"]);
        Assert.Equal("", typeof(SetupBootstrapDrivers).GetField("_newPassword", flags)!.GetValue(page.Instance));
        Assert.DoesNotContain("secret;quoted", page.Markup);
        Assert.Contains("Beep connection created", page.Markup);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task CreatedConnectionRequiresMatchingCatalogBeforeItCanBeSelected(bool unavailable, bool wrongProvider)
    {
        using var handler = new ConnectionHandler { CatalogUnavailable = unavailable, WrongCatalogProvider = wrongProvider };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await SelectLoadedDriver(page, "SQLSERVER");
        typeof(SetupBootstrapDrivers).GetField("_newName", flags)!.SetValue(page.Instance, "identity_db");
        typeof(SetupBootstrapDrivers).GetField("_newHost", flags)!.SetValue(page.Instance, "db.example");
        typeof(SetupBootstrapDrivers).GetField("_newDatabase", flags)!.SetValue(page.Instance, "reservoir");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("CreateConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.NotNull(handler.Created);
        Assert.False((bool)typeof(SetupBootstrapDrivers).GetField("_connectionsLoaded", flags)!.GetValue(page.Instance)!);
        Assert.Equal("", typeof(SetupBootstrapDrivers).GetField("_connection", flags)!.GetValue(page.Instance));
        Assert.Contains("connection created, but the connection catalog could not confirm it", page.Markup);
    }

    [Fact]
    public async Task BootstrapRequiresASelectedDriverAndClearsItWhenProviderChanges()
    {
        using var handler = new ConnectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await SelectLoadedDriver(page, "SQLSERVER");
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeProvider", flags)!.Invoke(page.Instance, ["POSTGRESQL"]));
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("CreateConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Null(handler.Created);
        Assert.Contains("Select a loaded driver for the chosen database provider", page.Markup);
    }

    [Fact]
    public async Task BootstrapRejectsMissingConnectionFieldsBeforeSending()
    {
        using var handler = new ConnectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        await SelectLoadedDriver(page, "SQLSERVER");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("CreateConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Null(handler.Created);
        Assert.Contains("Enter a valid connection name", page.Markup);
    }

    [Theory]
    [InlineData("SQLSERVER", "db.example")]
    [InlineData("SQLSERVER", "::1")]
    [InlineData("POSTGRESQL", "db.example")]
    public async Task BootstrapBuildsExternalAuthenticationWithoutAStoredPassword(string provider, string host)
    {
        using var handler = new ConnectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new SetupBootstrapDriverClient(http));
        var page = ui.RenderComponent<SetupBootstrapDrivers>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["temporary-key"]));
        typeof(SetupBootstrapDrivers).GetField("_newPassword", flags)!.SetValue(page.Instance, "old-secret");
        await page.InvokeAsync(() => typeof(SetupBootstrapDrivers).GetMethod("ChangeProvider", flags)!.Invoke(page.Instance, [provider]));
        Assert.Equal("", typeof(SetupBootstrapDrivers).GetField("_newPassword", flags)!.GetValue(page.Instance));
        await SelectLoadedDriver(page, provider);
        typeof(SetupBootstrapDrivers).GetField("_newName", flags)!.SetValue(page.Instance, "identity_db");
        typeof(SetupBootstrapDrivers).GetField("_newHost", flags)!.SetValue(page.Instance, host);
        typeof(SetupBootstrapDrivers).GetField("_newDatabase", flags)!.SetValue(page.Instance, "reservoir");
        await page.InvokeAsync(() => (Task)typeof(SetupBootstrapDrivers).GetMethod("CreateConnection", flags)!.Invoke(page.Instance, null)!);
        var fields = new System.Data.Common.DbConnectionStringBuilder { ConnectionString = handler.Created!.ConnectionString };
        Assert.False(fields.ContainsKey("Password"));
        if (provider == "SQLSERVER")
        {
            var sql = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(handler.Created.ConnectionString);
            Assert.True(sql.IntegratedSecurity);
            Assert.Equal(host.Contains(':') ? $"[{host}]" : host, sql.DataSource);
        }
        else Assert.Equal("reservoir", new Npgsql.NpgsqlConnectionStringBuilder(handler.Created.ConnectionString).Database);
    }
}
#endif
