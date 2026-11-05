#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class InitialCompanySetupTests
{
    private sealed class Auth : AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor"), new Claim(ClaimTypes.Role, "Administrator")], "test"))));
    }
    private sealed class Memory : IInitialCompanyIntentStore
    {
        public InitialCompanyIntent? Value; public bool Fail; public bool FailClear;
        public Task<InitialCompanyIntent?> ReadInitialCompanyAsync(string actor) => Task.FromResult(Value?.Actor == actor ? Value : null);
        public Task RetainInitialCompanyAsync(InitialCompanyIntent intent) { if (Fail || Value is not null && Value != intent) throw new InvalidOperationException(); Value = intent; return Task.CompletedTask; }
        public Task ClearInitialCompanyAsync(InitialCompanyIntent intent) { if (FailClear) throw new InvalidOperationException(); if (Value is not null && Value != intent) throw new InvalidOperationException(); Value = null; return Task.CompletedTask; }
    }
    private sealed class Handler : HttpMessageHandler
    {
        public InitialCompanyProvisionRequest? Original; public int Calls; public int Reviews; public bool LoseResponse; public string ReviewStatus = "Recorded";
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var isReview = request.RequestUri!.AbsolutePath.EndsWith("/review", StringComparison.Ordinal);
            Assert.Equal(isReview ? "/api/setup/companies/initial/review" : "/api/setup/companies/initial", request.RequestUri.AbsolutePath);
            var input = JsonSerializer.Deserialize<InitialCompanyProvisionRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
            if (Original is not null) Assert.Equal(Original, input);
            if (isReview) { Reviews++; var review = new InitialCompanyProvisionReview(input.IntentId, ReviewStatus, ReviewStatus == "Recorded" ? new(input.TenantId, input.BusinessAssociateId) : null); return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(review), Encoding.UTF8, "application/json") }; }
            Original = input; Calls++;
            if (LoseResponse) { LoseResponse = false; throw new HttpRequestException("Response lost"); }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new CompanyScope(input.TenantId, input.BusinessAssociateId)), Encoding.UTF8, "application/json") };
        }
    }
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static IRenderedComponent<InitialCompanySetup> Render(TestContext ui, HttpClient http, CompanyClientContext company, Memory memory, ICompanyAccessClient? access = null)
    {
        ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var auth = new Auth(); ui.Services.AddSingleton<AuthenticationStateProvider>(auth); ui.Services.AddSingleton<IInitialCompanyIntentStore>(memory);
        ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new CompanySelectionService(access ?? Mock.Of<ICompanyAccessClient>(), auth, company, user => user.FindFirst("party_id")?.Value));
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<InitialCompanySetup>();
        page.WaitForAssertion(() => Assert.True((bool)typeof(InitialCompanySetup).GetField("_ready", Flags)!.GetValue(page.Instance)!)); return page;
    }
    private static void Fill(IRenderedComponent<InitialCompanySetup> page)
    {
        foreach (var (name, value) in new[] { ("_tenantId", "tenant"), ("_tenantName", "Tenant"), ("_companyId", "BA"), ("_reason", "Initial setup") }) typeof(InitialCompanySetup).GetField(name, Flags)!.SetValue(page.Instance, value);
    }
    private static Task Save(IRenderedComponent<InitialCompanySetup> page) => page.InvokeAsync(() => (Task)typeof(InitialCompanySetup).GetMethod("Save", Flags)!.Invoke(page.Instance, null)!);
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreatedCompanyCanContinueOnlyAfterCurrentAccessIsConfirmed(bool permitted)
    {
        using var company = new CompanyClientContext(); using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var scope = new CompanyScope("tenant", "BA"); var access = new Mock<ICompanyAccessClient>();
        if (permitted) access.Setup(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(scope, ["Administrator"], []));
        else access.Setup(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>())).ThrowsAsync(new UnauthorizedAccessException());
        using var ui = new TestContext(); var page = Render(ui, http, company, new Memory(), access.Object); Fill(page); await Save(page);
        await page.InvokeAsync(() => (Task)typeof(InitialCompanySetup).GetMethod("ContinueToEnvironment", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        var navigation = ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();
        if (permitted) { Assert.Equal(scope, company.Selection?.Scope); Assert.EndsWith("/setup/company-environment", navigation.Uri); }
        else { Assert.Null(company.Selection); Assert.DoesNotContain("/setup/company-environment", navigation.Uri); Assert.Contains("current access could not be confirmed", page.Markup); }
        Assert.Equal(1, handler.Calls);
    }
    [Fact]
    public async Task CreatedCompanyCannotOpenEnvironmentWithoutCurrentCompanyAdministratorRole()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var scope = new CompanyScope("tenant", "BA"); var access = new Mock<ICompanyAccessClient>();
        access.Setup(x => x.GetAccessAsync(scope, It.IsAny<CancellationToken>())).ReturnsAsync(new RepositoryCompanyAccess(scope, ["Viewer"], []));
        using var ui = new TestContext(); var page = Render(ui, http, company, new Memory(), access.Object); Fill(page); await Save(page);
        await page.InvokeAsync(() => (Task)typeof(InitialCompanySetup).GetMethod("ContinueToEnvironment", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal(scope, company.Selection?.Scope);
        Assert.DoesNotContain("/setup/company-environment", ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().Uri);
        Assert.Contains("does not include the administrator role", page.Markup);
    }
    [Fact]
    public async Task LostResponseIsRetriedWithTheOriginalRequestAfterReload()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { LoseResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var memory = new Memory();
        using var ui = new TestContext(); var page = Render(ui, http, company, memory); Fill(page); await Save(page);
        Assert.NotNull(memory.Value); page.Dispose(); var reloaded = ui.RenderComponent<InitialCompanySetup>();
        reloaded.WaitForAssertion(() => Assert.Contains("earlier request awaits confirmation", reloaded.Markup)); await Save(reloaded);
        Assert.Null(memory.Value); Assert.Equal(2, handler.Calls); await reloaded.InvokeAsync(() => reloaded.Render());
        Assert.Contains("Company access was created", reloaded.Markup);
    }
    [Theory]
    [InlineData("Recorded")]
    [InlineData("NotRecorded")]
    [InlineData("Unknown")]
    public async Task OriginalRequestReviewClearsOnlyConfirmedRecordedEvidence(string status)
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { LoseResponse = true, ReviewStatus = status }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var memory = new Memory();
        using var ui = new TestContext(); var page = Render(ui, http, company, memory); Fill(page); await Save(page); var original = memory.Value; Assert.NotNull(original);
        await page.InvokeAsync(() => (Task)typeof(InitialCompanySetup).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!); await page.InvokeAsync(() => page.Render());
        Assert.Equal(1, handler.Calls); Assert.Equal(1, handler.Reviews);
        if (status == "Recorded") { Assert.Null(memory.Value); Assert.Contains("creation was recorded", page.Markup); }
        else { Assert.Equal(original, memory.Value); Assert.Contains(status == "NotRecorded" ? "may still finish" : "could not be confirmed", page.Markup); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClearingRequiresAcknowledgementAndKeepsRequestIfStorageFails(bool failClear)
    {
        using var company = new CompanyClientContext(); using var handler = new Handler { LoseResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var memory = new Memory();
        using var ui = new TestContext(); var page = Render(ui, http, company, memory); Fill(page); await Save(page);
        var original = memory.Value; Assert.NotNull(original);
        Task Clear() => page.InvokeAsync(() => (Task)typeof(InitialCompanySetup).GetMethod("ClearRetained", Flags)!.Invoke(page.Instance, null)!);
        await Clear(); Assert.Equal(original, memory.Value); Assert.Equal(1, handler.Calls);
        typeof(InitialCompanySetup).GetField("_clearAcknowledged", Flags)!.SetValue(page.Instance, true); memory.FailClear = failClear;
        await Clear(); await page.InvokeAsync(() => page.Render());
        if (failClear) { Assert.Equal(original, memory.Value); Assert.Contains("could not be cleared", page.Markup); }
        else { Assert.Null(memory.Value); Assert.Contains("server-side changes remain", page.Markup); }
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task FailedRetentionPreventsTransport()
    {
        using var company = new CompanyClientContext(); using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") }; var memory = new Memory { Fail = true };
        using var ui = new TestContext(); var page = Render(ui, http, company, memory); Fill(page); await Save(page);
        Assert.Equal(0, handler.Calls); Assert.Null(memory.Value);
    }
}
#endif
