#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class SystemSetupRepositoryTests
{
    private sealed class SelectionHandler : HttpMessageHandler
    {
        public bool Selected;
        public bool LoseSelectionResponse;
        public bool EmptyConnections;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            if (request.Method == HttpMethod.Post && LoseSelectionResponse)
            {
                Selected = true;
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            }
            object result = request.Method == HttpMethod.Post
                ? (Selected = true, new SystemSetupConnectionSelection("system")).Item2
                : request.RequestUri!.AbsolutePath.EndsWith("status", StringComparison.Ordinal)
                    ? new SystemSetupRepositoryStatus(Selected ? "MigrationRequired" : "Unconfigured")
                    : EmptyConnections ? Array.Empty<string>() : new[] { "system" };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    [Fact]
    public void EmptySystemCatalogExplainsHowToMakeAConnectionEligible()
    {
        using var company = new CompanyClientContext(); using var handler = new SelectionHandler { EmptyConnections = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.Contains("No eligible Beep connections", page.Markup));
        Assert.Contains("unique name and a concrete supported database target", page.Markup);
        Assert.Contains("Selection checks connectivity and transaction support", page.Markup);
    }

    [Fact]
    public async Task LostSelectionResponseRequiresFreshStatusBeforeRetry()
    {
        using var company = new CompanyClientContext(); using var handler = new SelectionHandler { LoseSelectionResponse = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>(); page.WaitForAssertion(() => Assert.Contains("Verify and select connection", page.Markup));
        await page.InvokeAsync(() => typeof(SystemSetupRepository).GetField("_connection", flags)!.SetValue(page.Instance, "system"));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("SelectConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.True(handler.Selected);
        Assert.Contains("selection outcome is unconfirmed", page.Markup);
        Assert.DoesNotContain("Verify and select connection", page.Markup);
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("Refresh", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("MigrationRequired", page.Markup);
        Assert.DoesNotContain("Verify and select connection", page.Markup);
    }

    [Fact]
    public async Task AdministratorCanSelectARegisteredSystemConnection()
    {
        using var company = new CompanyClientContext(); using var handler = new SelectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.Contains("Verify and select connection", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        await page.InvokeAsync(() => typeof(SystemSetupRepository).GetField("_connection", flags)!.SetValue(page.Instance, "system"));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("SelectConnection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        page.WaitForAssertion(() => Assert.Contains("MigrationRequired", page.Markup));
        Assert.True(handler.Selected); Assert.Contains("System connection selected", page.Markup);
    }

    private sealed class Auth : AuthenticationStateProvider
    {
        private string actor = "actor";
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", actor), new Claim(ClaimTypes.Role, "Administrator")], "test"))));
        public void Change(string value) { actor = value; NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }
    private sealed class IntentMemory : ISystemSetupIntentStore
    {
        public SystemSetupIntent? Value;
        public bool Fail;
        public Task<SystemSetupIntent?> ReadSystemAsync(string actor) => Task.FromResult(Value?.Actor == actor ? Value : null);
        public Task RetainSystemAsync(SystemSetupIntent intent) { if (Fail) throw new InvalidOperationException(); if (Value is not null && Value != intent) throw new InvalidOperationException(); Value = intent; return Task.CompletedTask; }
        public Task ClearSystemAsync(SystemSetupIntent intent) { if (Value is not null && Value != intent) throw new InvalidOperationException(); Value = null; return Task.CompletedTask; }
    }
    private static void AddRecovery(TestContext ui, IntentMemory? intents = null, Auth? auth = null)
    {
        ui.Services.AddSingleton<AuthenticationStateProvider>(auth ?? new Auth());
        ui.Services.AddSingleton<ISystemSetupIntentStore>(intents ?? new IntentMemory());
    }
    private sealed class Handler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.False(request.Headers.Contains(CompanyRequestHeaders.Company));
            object result = request.Method == HttpMethod.Get ? new SystemSetupRepositoryStatus("MigrationRequired") :
                new SchemaMigrationPlanResult { Success = true, CanApply = true, PlanId = "plan", PlanHash = "plan-hash", ManifestHash = "manifest-hash", PolicyDecision = "review", TablesToCreate = 2 };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    private sealed class ApprovalHandler : HttpMessageHandler
    {
        public SchemaMigrationApprovalRequest? Approval;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.RequestUri!.AbsolutePath.EndsWith("/approve", StringComparison.Ordinal))
            {
                Approval = JsonSerializer.Deserialize<SchemaMigrationApprovalRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web));
                result = new SchemaMigrationApprovalResult { Success = true, PlanId = "plan", PlanHash = "plan-hash", ManifestHash = "manifest-hash" };
            }
            else if (request.Method == HttpMethod.Post)
                result = new SchemaMigrationPlanResult { Success = true, CanApply = true, PlanId = "plan", PlanHash = "plan-hash", ManifestHash = "manifest-hash" };
            else result = new SystemSetupRepositoryStatus("MigrationRequired");
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task ApprovalSendsExactReviewedPlanAndManifestHashes()
    {
        using var company = new CompanyClientContext(); using var handler = new ApprovalHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>(); page.WaitForAssertion(() => Assert.Contains("MigrationRequired", page.Markup));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("Plan", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => typeof(SystemSetupRepository).GetField("_reviewed", flags)!.SetValue(page.Instance, true));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("Approve", flags)!.Invoke(page.Instance, null)!);
        Assert.NotNull(handler.Approval); Assert.Equal("plan", handler.Approval.PlanId);
        Assert.Equal("plan-hash", handler.Approval.ExpectedPlanHash); Assert.Equal("manifest-hash", handler.Approval.ExpectedManifestHash);
    }

    [Fact]
    public async Task RefreshDiscardsReviewedAndApprovedMigrationPlan()
    {
        using var company = new CompanyClientContext(); using var handler = new ApprovalHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>(); page.WaitForAssertion(() => Assert.Contains("MigrationRequired", page.Markup));
        async Task Invoke(string name) => await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod(name, flags)!.Invoke(page.Instance, null)!);
        await Invoke("Plan");
        await page.InvokeAsync(() => typeof(SystemSetupRepository).GetField("_reviewed", flags)!.SetValue(page.Instance, true));
        await Invoke("Approve");
        Assert.True((bool)typeof(SystemSetupRepository).GetField("_approved", flags)!.GetValue(page.Instance)!);
        await Invoke("Refresh"); await page.InvokeAsync(() => page.Render());
        Assert.Null(typeof(SystemSetupRepository).GetField("_plan", flags)!.GetValue(page.Instance));
        Assert.False((bool)typeof(SystemSetupRepository).GetField("_approved", flags)!.GetValue(page.Instance)!);
        Assert.DoesNotContain("Execute approved plan", page.Markup);
    }

    [Fact]
    public async Task RefreshClearsAConnectionRemovedFromTheCatalog()
    {
        using var company = new CompanyClientContext(); using var handler = new SelectionHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>(); page.WaitForAssertion(() => Assert.Contains("Verify and select connection", page.Markup));
        await page.InvokeAsync(() => typeof(SystemSetupRepository).GetField("_connection", flags)!.SetValue(page.Instance, "removed"));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("Refresh", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal("", typeof(SystemSetupRepository).GetField("_connection", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("SelectConnection", flags)!.Invoke(page.Instance, null)!);
        Assert.False(handler.Selected);
    }

    private sealed class DelayedPlanHandler : HttpMessageHandler
    {
        public TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Post)
            {
                Started.TrySetResult(); await Release.Task;
                result = new SchemaMigrationPlanResult { Success = true, CanApply = true, PlanId = "old-plan", PlanHash = "hash", ManifestHash = "manifest", TablesToCreate = 2 };
            }
            else result = new SystemSetupRepositoryStatus("MigrationRequired");
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task ChangedAdministratorCannotSeeOrApproveAnEarlierAdministratorsDelayedPlan()
    {
        using var company = new CompanyClientContext(); using var handler = new DelayedPlanHandler(); var auth = new Auth();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, auth: auth);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.Contains("MigrationRequired", page.Markup));
        var plan = page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("Plan", flags)!.Invoke(page.Instance, null)!);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await page.InvokeAsync(() => auth.Change("other"));
        page.WaitForAssertion(() => Assert.Equal("other", typeof(SystemSetupRepository).GetField("_actor", flags)!.GetValue(page.Instance)));
        handler.Release.SetResult(); await plan;
        Assert.Null(typeof(SystemSetupRepository).GetField("_plan", flags)!.GetValue(page.Instance));
        Assert.DoesNotContain("Approve plan", page.Markup);
    }

    private sealed class RecordsHandler : HttpMessageHandler
    {
        public SystemSetupConfiguration? Record;
        public bool LoseResponse;
        public int Writes;
        public int Drafts;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Put)
            {
                var command = JsonSerializer.Deserialize<SystemSetupRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                Writes++;
                result = Record = SystemSetupIdentity.Prepare(command, Record);
                if (LoseResponse) throw new HttpRequestException("Response lost");
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("tools-default-draft", StringComparison.Ordinal))
            { Drafts++; result = new SystemSetupToolsDefaultDraft("1234567890abcdef1234567890abcdef", "{\"units\":{\"system\":\"Metric\"}}"); }
            else if (request.RequestUri!.AbsolutePath.EndsWith("status", StringComparison.Ordinal))
                result = new SystemSetupRepositoryStatus("SchemaReady");
            else result = new SystemSetupReadResult("TOOLS", "DEFAULT", Record);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task AdministratorPreparesToolDefaultsAsAReviewableDraftBeforeSaving()
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        async Task Invoke(string name) => await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod(name, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadRecord"); await Invoke("LoadToolsDraft"); await page.InvokeAsync(() => page.Render());
        Assert.Equal(1, handler.Drafts); Assert.Equal(0, handler.Writes);
        Assert.Contains("Review them before saving", page.Markup);
        Assert.Equal("1234567890abcdef1234567890abcdef", typeof(SystemSetupRepository).GetField("_recordVersion", flags)!.GetValue(page.Instance));
        await Invoke("LoadToolsDraft"); Assert.Equal(1, handler.Drafts);
        await Invoke("SaveRecord");
        Assert.Equal(1, handler.Writes); Assert.Contains("Metric", handler.Record!.SettingsJson);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FirstRunDefaultsActionRechecksForAnExistingRecordBeforePreparing(bool createdByAnotherAdministrator)
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.Contains("Prepare shared tool defaults", page.Markup));
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        if (createdByAnotherAdministrator)
            handler.Record = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{\"custom\":\"existing\"}", "", Guid.NewGuid().ToString("N")), null);
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("PrepareFirstRunDefaults", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(createdByAnotherAdministrator ? 0 : 1, handler.Drafts);
        Assert.Equal(0, handler.Writes);
        var json = (string)typeof(SystemSetupRepository).GetField("_recordJson", flags)!.GetValue(page.Instance)!;
        Assert.Contains(createdByAnotherAdministrator ? "existing" : "Metric", json);
    }

    private sealed class DelayedSaveHandler : HttpMessageHandler
    {
        public TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Put)
            {
                var command = JsonSerializer.Deserialize<SystemSetupRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                Started.SetResult(); await Release.Task;
                result = SystemSetupIdentity.Prepare(command, null);
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("status", StringComparison.Ordinal)) result = new SystemSetupRepositoryStatus("SchemaReady");
            else result = new SystemSetupReadResult("TOOLS", "DEFAULT", null);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task ChangedAdministratorDoesNotReceiveAnotherActorsSaveResult()
    {
        using var company = new CompanyClientContext(); using var handler = new DelayedSaveHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var intents = new IntentMemory(); var auth = new Auth();
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents, auth);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("LoadRecord", flags)!.Invoke(page.Instance, null)!);
        var save = page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("SaveRecord", flags)!.Invoke(page.Instance, null)!);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await page.InvokeAsync(() => auth.Change("other"));
        page.WaitForAssertion(() => Assert.Equal("other", typeof(SystemSetupRepository).GetField("_actor", flags)!.GetValue(page.Instance)));
        handler.Release.SetResult(); await save;
        Assert.Equal("actor", intents.Value?.Actor);
        Assert.Null(typeof(SystemSetupRepository).GetField("_recordPending", flags)!.GetValue(page.Instance));
        Assert.DoesNotContain("Shared record saved", page.Markup);
    }

    [Fact]
    public async Task AdministratorCanReviewSystemSchemaWithoutSelectingACompany()
    {
        using var company = new CompanyClientContext(); using var http = new HttpClient(new Handler()) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.Contains("MigrationRequired", page.Markup));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("Plan", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Tables to create: 2", page.Markup);
        Assert.Contains("not checked", page.Markup);
    }

    [Fact]
    public async Task AdministratorCanSaveSharedSetupWithoutClaimingItIsApplied()
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        var changes = new CompanyEnvironmentChangeSignal(); var notifications = 0; changes.Changed += () => notifications++;
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(changes);
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<SystemSetupRepository>(); page.WaitForAssertion(() => Assert.Contains("Shared tool defaults: missing", page.Markup));
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(page.Instance)!));
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        async Task Invoke(string name) => await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod(name, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadRecord"); await Invoke("SaveRecord"); await page.InvokeAsync(() => page.Render());
        Assert.Equal(1, notifications); Assert.NotNull(handler.Record); Assert.Contains("Review effective company settings", page.Markup);
        Assert.Contains("present; verify each company environment", page.Markup);
    }

    [Fact]
    public async Task SharedSourceEditorPreservesSettingsAndRefusesDuplicateNamesBeforeSaving()
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        handler.Record = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{\"custom\":\"keep\"}", "", Guid.NewGuid().ToString("N")), null);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        async Task Invoke(string name) => await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod(name, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadRecord");
        await page.InvokeAsync(() =>
        {
            typeof(SystemSetupRepository).GetField("_sourceName", flags)!.SetValue(page.Instance, "wells");
            typeof(SystemSetupRepository).GetField("_sourceModule", flags)!.SetValue(page.Instance, "PPDM_CORE");
            typeof(SystemSetupRepository).GetMethod("AddSharedSource", flags)!.Invoke(page.Instance, null);
        });
        var draft = (string)typeof(SystemSetupRepository).GetField("_recordJson", flags)!.GetValue(page.Instance)!;
        using (var json = JsonDocument.Parse(draft))
        {
            Assert.Equal("keep", json.RootElement.GetProperty("custom").GetString());
            Assert.Equal("PPDM_CORE", json.RootElement.GetProperty("dataSources").GetProperty("wells").GetProperty("path").GetString());
        }
        Assert.Equal(0, handler.Writes);
        await page.InvokeAsync(() =>
        {
            typeof(SystemSetupRepository).GetField("_sourceName", flags)!.SetValue(page.Instance, "WELLS");
            typeof(SystemSetupRepository).GetField("_sourceModule", flags)!.SetValue(page.Instance, "OTHER");
            typeof(SystemSetupRepository).GetMethod("AddSharedSource", flags)!.Invoke(page.Instance, null);
        });
        Assert.Equal(draft, typeof(SystemSetupRepository).GetField("_recordJson", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(() => typeof(SystemSetupRepository).GetMethod("ChangeSharedSource", flags)!.Invoke(page.Instance, null));
        draft = (string)typeof(SystemSetupRepository).GetField("_recordJson", flags)!.GetValue(page.Instance)!;
        using (var json = JsonDocument.Parse(draft))
        {
            Assert.Equal("OTHER", json.RootElement.GetProperty("dataSources").GetProperty("wells").GetProperty("path").GetString());
            Assert.Equal("keep", json.RootElement.GetProperty("custom").GetString());
        }
        await Invoke("SaveRecord");
        Assert.Equal(1, handler.Writes); Assert.Equal(draft, handler.Record!.SettingsJson);
    }

    [Fact]
    public async Task SharedSourceEditorHandlesLargeSettingsAndRemovesOnlyTheSelectedSource()
    {
        var large = new string('x', CompanyEnvironmentIdentity.MaxOverridesLength + 256);
        var settings = JsonSerializer.Serialize(new { custom = large, dataSources = new { keep = new { kind = "beep", path = "PPDM_CORE", profile = "ppdm39" } } });
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.AddBeepSource(settings, "wells", "PPDM_CORE"));
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        handler.Record = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", settings, "", Guid.NewGuid().ToString("N")), null);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        async Task Invoke(string name) => await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod(name, flags)!.Invoke(page.Instance, null)!);
        void Set(string name, string value) => typeof(SystemSetupRepository).GetField(name, flags)!.SetValue(page.Instance, value);
        await Invoke("LoadRecord");
        await page.InvokeAsync(() => { Set("_sourceName", "wells"); Set("_sourceModule", "PPDM_CORE"); typeof(SystemSetupRepository).GetMethod("AddSharedSource", flags)!.Invoke(page.Instance, null); });
        await page.InvokeAsync(() => { Set("_sourceName", "wells"); Set("_sourceModule", "OTHER"); typeof(SystemSetupRepository).GetMethod("ChangeSharedSource", flags)!.Invoke(page.Instance, null); });
        var draft = (string)typeof(SystemSetupRepository).GetField("_recordJson", flags)!.GetValue(page.Instance)!;
        using (var json = JsonDocument.Parse(draft)) Assert.Equal("OTHER", json.RootElement.GetProperty("dataSources").GetProperty("wells").GetProperty("path").GetString());
        await page.InvokeAsync(() => { Set("_sourceName", "WELLS"); typeof(SystemSetupRepository).GetMethod("RemoveSharedSource", flags)!.Invoke(page.Instance, null); });
        draft = (string)typeof(SystemSetupRepository).GetField("_recordJson", flags)!.GetValue(page.Instance)!;
        using (var json = JsonDocument.Parse(draft))
        {
            Assert.Equal(large, json.RootElement.GetProperty("custom").GetString());
            Assert.False(json.RootElement.GetProperty("dataSources").TryGetProperty("wells", out _));
            Assert.Equal("PPDM_CORE", json.RootElement.GetProperty("dataSources").GetProperty("keep").GetProperty("path").GetString());
        }
        Assert.Equal(0, handler.Writes);
        await Invoke("SaveRecord"); Assert.Equal(1, handler.Writes); Assert.Equal(draft, handler.Record!.SettingsJson);
        var oversized = "{\"custom\":\"" + new string('x', SystemSetupIdentity.MaximumSettingsLength) + "\"}";
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.AddBeepSource(oversized, "wells", "PPDM_CORE", EnvironmentSettingsScope.Shared));
    }

    [Fact]
    public async Task LostSharedSaveIsRecoveredAfterPageReload()
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler { LoseResponse = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var intents = new IntentMemory();
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("LoadRecord", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("SaveRecord", flags)!.Invoke(page.Instance, null)!);
        Assert.NotNull(intents.Value); Assert.Equal(1, handler.Writes);
        page.Dispose();
        var reloaded = ui.RenderComponent<SystemSetupRepository>();
        reloaded.WaitForAssertion(() => Assert.Contains("A previous shared setup save is awaiting confirmation", reloaded.Markup));
        await reloaded.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("LoadRecord", flags)!.Invoke(reloaded.Instance, null)!);
        Assert.Null(intents.Value); Assert.Equal(1, handler.Writes);
    }

    [Fact]
    public async Task UnavailableRecoveryStoragePreventsSendingSharedSave()
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var intents = new IntentMemory { Fail = true };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui, intents);
        var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance; var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(SystemSetupRepository).GetField("_recoveryReady", flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("LoadRecord", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(SystemSetupRepository).GetMethod("SaveRecord", flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Writes);
    }

    [Fact]
    public void InvalidSharedRecordIsNotReportedAsPresent()
    {
        var valid = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        using var handler = new RecordsHandler { Record = valid with { SettingsJson = "{\"changed\":true}" } };
        using var company = new CompanyClientContext(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); AddRecovery(ui);
        var page = ui.RenderComponent<SystemSetupRepository>();
        page.WaitForAssertion(() => Assert.Contains("unavailable; check the repository", page.Markup));
        Assert.DoesNotContain("present; verify each company environment", page.Markup);
    }

    [Fact]
    public async Task FirstRunReadinessShowsMissingPresentAndInvalidDefaults()
    {
        using var company = new CompanyClientContext(); using var handler = new RecordsHandler();
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var api = new ApiClient(http, Mock.Of<IFailureReporter>(), company);
        Assert.Contains("create the shared tool defaults", await SystemSetupReadinessReader.ReadAsync(api));
        handler.Record = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        Assert.Contains("review each company environment", await SystemSetupReadinessReader.ReadAsync(api));
        handler.Record = handler.Record with { SettingsJson = "{\"modified\":true}" };
        await Assert.ThrowsAsync<InvalidOperationException>(() => SystemSetupReadinessReader.ReadAsync(api));
        using var migrationHttp = new HttpClient(new Handler()) { BaseAddress = new("https://api.test") };
        Assert.Contains("apply the system setup migration", await SystemSetupReadinessReader.ReadAsync(new ApiClient(migrationHttp, Mock.Of<IFailureReporter>(), company)));
    }
}
#endif
