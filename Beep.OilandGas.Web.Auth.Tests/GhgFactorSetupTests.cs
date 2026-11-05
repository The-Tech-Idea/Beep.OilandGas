#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class GhgFactorSetupTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        private string actor = "actor";
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", actor), new Claim(ClaimTypes.Role, "Administrator")], "test"))));
        public void Change(string value) { actor = value; NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }
    private sealed class IntentMemory : ISystemSetupIntentStore
    {
        public SystemSetupIntent? Value; public bool Fail;
        public Task<SystemSetupIntent?> ReadSystemAsync(string actor) => Task.FromResult(Value?.Actor == actor ? Value : null);
        public Task RetainSystemAsync(SystemSetupIntent intent) { if (Fail || Value is not null && Value != intent) throw new InvalidOperationException(); Value = intent; return Task.CompletedTask; }
        public Task ClearSystemAsync(SystemSetupIntent intent) { if (Value is not null && Value != intent) throw new InvalidOperationException(); Value = null; return Task.CompletedTask; }
    }
    private static void AddRecovery(TestContext ui, IntentMemory? intents = null, Auth? auth = null)
    {
        ui.Services.AddSingleton<AuthenticationStateProvider>(auth ?? new Auth());
        ui.Services.AddSingleton<ISystemSetupIntentStore>(intents ?? new IntentMemory());
    }
    private static void WaitReady(IRenderedComponent<GhgFactorSetup> page)
    {
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        page.WaitForAssertion(() => Assert.True((bool)typeof(GhgFactorSetup).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
    }
    private sealed class Handler : HttpMessageHandler
    {
        public SystemSetupConfiguration? Record; public int Writes; public bool LoseResponse;
        public TaskCompletionSource? Started; public TaskCompletionSource? Release;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("/api/setup/system-repository/records/GHG_FACTORS/KW", request.RequestUri!.AbsolutePath);
            object result;
            if (request.Method == HttpMethod.Put)
            {
                var change = JsonSerializer.Deserialize<SystemSetupRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                _ = GhgFactorCatalogValidation.Parse(change.Key, change.SettingsJson);
                result = Record = SystemSetupIdentity.Prepare(change, Record); Writes++;
                if (Started is not null) { Started.SetResult(); await Release!.Task; }
                if (LoseResponse) throw new HttpRequestException("Response lost after commit.");
            }
            else result = new SystemSetupReadResult("GHG_FACTORS", "KW", Record);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AdministratorCanSaveOrRecoverTheSameReviewedCatalog(bool loseResponse)
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { LoseResponse = loseResponse };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<GhgFactorSetup>(); WaitReady(page);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance; var type = typeof(GhgFactorSetup);
        async Task Call(string name, params object[] args) => await page.InvokeAsync(() => type.GetMethod(name, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        void Set(string name, object value) => type.GetField(name, flags)!.SetValue(page.Instance, value);
        await Call("ChangeKey", "KW"); await Call("Load");
        Set("_id", "test-factor"); Set("_category", "flare"); Set("_input", "volume"); Set("_output", "mass"); Set("_source", "test-source");
        Set("_factor", 0m); Set("_from", new DateTime(2026, 1, 1)); Set("_through", new DateTime(2026, 12, 31));
        await Call("Add"); await Call("Save");
        Assert.Equal(1, handler.Writes); Assert.NotNull(handler.Record);
        if (loseResponse) { await Call("Load"); await page.InvokeAsync(() => page.Render()); Assert.Contains("original catalog save was confirmed", page.Markup); }
        else { await page.InvokeAsync(() => page.Render()); Assert.Contains("Catalog saved", page.Markup); }
        Assert.Contains("does not approve it", page.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LostResponseCanBeConfirmedAfterReload()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { LoseResponse = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var intents = new IntentMemory();
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents);
        var flags = BindingFlags.NonPublic | BindingFlags.Instance; var type = typeof(GhgFactorSetup); var page = ui.RenderComponent<GhgFactorSetup>(); WaitReady(page);
        async Task Call(IRenderedComponent<GhgFactorSetup> target, string name, params object[] args) => await target.InvokeAsync(() => type.GetMethod(name, flags)!.Invoke(target.Instance, args) as Task ?? Task.CompletedTask);
        await Call(page, "ChangeKey", "KW"); await Call(page, "Load");
        type.GetField("_id", flags)!.SetValue(page.Instance, "f"); type.GetField("_category", flags)!.SetValue(page.Instance, "flare"); type.GetField("_input", flags)!.SetValue(page.Instance, "volume"); type.GetField("_output", flags)!.SetValue(page.Instance, "mass"); type.GetField("_source", flags)!.SetValue(page.Instance, "source");
        type.GetField("_factor", flags)!.SetValue(page.Instance, 0m); type.GetField("_from", flags)!.SetValue(page.Instance, new DateTime(2026, 1, 1)); type.GetField("_through", flags)!.SetValue(page.Instance, new DateTime(2026, 12, 31));
        await Call(page, "Add"); await Call(page, "Save"); Assert.Equal(1, handler.Writes); Assert.NotNull(intents.Value);
        page.Dispose(); var reloaded = ui.RenderComponent<GhgFactorSetup>();
        reloaded.WaitForAssertion(() => Assert.Contains("A previous catalog save is awaiting confirmation", reloaded.Markup));
        await Call(reloaded, "Load"); Assert.Null(intents.Value); Assert.Equal(1, handler.Writes);
    }

    [Fact]
    public void OtherSharedRequestMustBeRecoveredInSystemSetup()
    {
        using var company = new CompanyClientContext(); using var http = new HttpClient(new Handler()) { BaseAddress = new("https://api.test") };
        var original = new SystemSetupIntent("actor", new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N"))); var intents = new IntentMemory { Value = original };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents);
        var page = ui.RenderComponent<GhgFactorSetup>();
        page.WaitForAssertion(() => Assert.Contains("A different shared setup save awaits confirmation", page.Markup));
        Assert.Equal(original, intents.Value); Assert.False((bool)typeof(GhgFactorSetup).GetField("_recoveryReady", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(page.Instance)!);
    }

    [Fact]
    public async Task ChangedAdministratorDoesNotReceivePreviousSaveResult()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { Started = new(TaskCreationOptions.RunContinuationsAsynchronously), Release = new(TaskCreationOptions.RunContinuationsAsynchronously) };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var auth = new Auth();
        var json = JsonSerializer.Serialize(new GhgFactorCatalog("KW", [new GhgFactorDefinition("f", "flare", "volume", "mass", 0m, new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), "source")]), new JsonSerializerOptions(JsonSerializerDefaults.Web));
        var intent = new SystemSetupIntent("actor", new("GHG_FACTORS", "KW", "1", json, "", Guid.NewGuid().ToString("N"))); var intents = new IntentMemory { Value = intent };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents, auth);
        var page = ui.RenderComponent<GhgFactorSetup>(); WaitReady(page); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        var save = page.InvokeAsync(() => (Task)typeof(GhgFactorSetup).GetMethod("Save", flags)!.Invoke(page.Instance, null)!);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5)); await page.InvokeAsync(() => auth.Change("other"));
        page.WaitForAssertion(() => Assert.Equal("other", typeof(GhgFactorSetup).GetField("_actor", flags)!.GetValue(page.Instance)));
        handler.Release.SetResult(); await save;
        Assert.Equal(intent, intents.Value); Assert.Null(typeof(GhgFactorSetup).GetField("_pending", flags)!.GetValue(page.Instance));
        Assert.DoesNotContain("Catalog saved", page.Markup);
    }

    [Fact]
    public async Task FailedRetentionDoesNotSendCatalog()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var intents = new IntentMemory { Fail = true }; using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents);
        var page = ui.RenderComponent<GhgFactorSetup>(); WaitReady(page); var flags = BindingFlags.NonPublic | BindingFlags.Instance; var type = typeof(GhgFactorSetup);
        await page.InvokeAsync(() => type.GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["KW"]));
        await page.InvokeAsync(() => (Task)type.GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        type.GetField("_id", flags)!.SetValue(page.Instance, "f"); type.GetField("_category", flags)!.SetValue(page.Instance, "flare"); type.GetField("_input", flags)!.SetValue(page.Instance, "volume"); type.GetField("_output", flags)!.SetValue(page.Instance, "mass"); type.GetField("_source", flags)!.SetValue(page.Instance, "source");
        type.GetField("_factor", flags)!.SetValue(page.Instance, 0m); type.GetField("_from", flags)!.SetValue(page.Instance, new DateTime(2026, 1, 1)); type.GetField("_through", flags)!.SetValue(page.Instance, new DateTime(2026, 12, 31));
        await page.InvokeAsync(() => type.GetMethod("Add", flags)!.Invoke(page.Instance, null));
        await page.InvokeAsync(() => (Task)type.GetMethod("Save", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Writes); Assert.Null(intents.Value);
    }

    [Fact]
    public async Task MissingValuesDoNotCreateAFactor()
    {
        using var company = new CompanyClientContext(); using var http = new HttpClient(new Handler()) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<GhgFactorSetup>(); WaitReady(page); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(GhgFactorSetup).GetMethod("ChangeKey", flags)!.Invoke(page.Instance, ["KW"]));
        await page.InvokeAsync(() => (Task)typeof(GhgFactorSetup).GetMethod("Load", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => typeof(GhgFactorSetup).GetMethod("Add", flags)!.Invoke(page.Instance, null));
        await page.InvokeAsync(() => page.Render()); Assert.Contains("Enter a factor value and both effective dates", page.Markup);
    }
}
#endif
