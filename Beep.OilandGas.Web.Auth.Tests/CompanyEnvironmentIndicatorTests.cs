#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using System.Text;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.Shared;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyEnvironmentIndicatorTests
{
    private sealed class TestAuth : AuthenticationStateProvider
    {
        private string actor = "actor";
        public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", actor)], "test"))));
        public void Change(string value) { actor = value; NotifyAuthenticationStateChanged(GetAuthenticationStateAsync()); }
    }

    private sealed class Handler(bool foreign, string status = "Current") : HttpMessageHandler
    {
        public string Zone = "Asia/Kuwait";
        public string Diameter = "in";
        public string Status = status;
        public bool Fail;
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++;
            if (Fail) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            var company = new CompanyScope("tenant", "company");
            var result = new CompanyEnvironmentEffectiveReview(foreign ? new("tenant", "foreign") : company, new string('A', 64), new string('B', 64), null, new string('C', 64),
                new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "oilfield", "ft", "psi", Diameter, [], [], []) { TimeZoneId = Zone }) { ReviewStatus = Status };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    private sealed class SelectedCompanyHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var company = new CompanyScope("tenant", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            var settings = new CompanyEnvironmentPreview(company, "KW", "Company", "ar-KW", "metric", "m", "bar", "mm", [], [], []) { TimeZoneId = "Asia/Kuwait" };
            var result = new CompanyEnvironmentEffectiveReview(company, new string('A', 64), new string('B', 64), null, new string('C', 64), settings) { ReviewStatus = "Current" };
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    private sealed class DelayedHandler : HttpMessageHandler
    {
        public readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public readonly TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Calls++; Started.TrySetResult(); await Release.Task;
            var company = new CompanyScope("tenant", "company");
            var settings = new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []);
            var result = new CompanyEnvironmentEffectiveReview(company, new string('A', 64), new string('B', 64), null, new string('C', 64), settings) { ReviewStatus = "Current" };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task AccountChangeClearsDisplayAndRejectsLatePreviousAccountResponse()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var handler = new DelayedHandler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var auth = new TestAuth(); ui.Services.AddSingleton<AuthenticationStateProvider>(auth);
        ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        CompanyEnvironmentDisplay? seen = null;
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(p => p.Add(x => x.OnEnvironmentChanged, value => seen = value.Apply(company, seen)));
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await component.InvokeAsync(() => auth.Change("other"));
        component.WaitForAssertion(() => { Assert.Null(seen); Assert.Contains("needs review", component.Markup); });
        handler.Release.SetResult();
        await component.InvokeAsync(() => Task.CompletedTask);
        Assert.Null(seen); Assert.DoesNotContain("Kuwait", component.Markup); Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task AccountChangeRemovesPreviouslyDisplayedCompanySettings()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var handler = new Handler(false); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var auth = new TestAuth(); ui.Services.AddSingleton<AuthenticationStateProvider>(auth);
        ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        CompanyEnvironmentDisplay? seen = null;
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(p => p.Add(x => x.OnEnvironmentChanged, value => seen = value.Apply(company, seen)));
        component.WaitForAssertion(() => Assert.Contains("Kuwait", component.Markup));
        await component.InvokeAsync(() => auth.Change("other"));
        component.WaitForAssertion(() => { Assert.Null(seen); Assert.DoesNotContain("Kuwait", component.Markup); });
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task DelayedOldClearCannotEraseNewCompanyDisplay()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        var oldVersion = company.Version; var arrived = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using var http = new HttpClient(new SelectedCompanyHandler()) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        CompanyEnvironmentDisplay? seen = null;
        async Task Update(CompanyEnvironmentDisplayUpdate value)
        {
            if (value.CompanyVersion == oldVersion && value.Review is null) { arrived.TrySetResult(); await release.Task; }
            seen = value.Apply(company, seen);
        }
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(p => p.Add(x => x.OnEnvironmentChanged, Microsoft.AspNetCore.Components.EventCallback.Factory.Create<CompanyEnvironmentDisplayUpdate>(new object(), Update)));
        await arrived.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await component.InvokeAsync(() => { company.Clear(); Assert.True(company.TrySelect(company.Version, "actor", new(new("tenant", "other"), ["Engineer"], []))); });
        component.WaitForAssertion(() => Assert.Equal("other", seen?.Review.Company.BusinessAssociateId));
        release.TrySetResult(); await component.InvokeAsync(() => Task.CompletedTask);
        component.WaitForAssertion(() => Assert.Equal("other", seen?.Review.Company.BusinessAssociateId)); Assert.True(seen!.Matches(company));
    }

    [Fact]
    public void IndicatorPublishesOnlyTheCurrentCompanySelectionGeneration()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var http = new HttpClient(new Handler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        CompanyEnvironmentDisplay? seen = null;
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(p => p.Add(x => x.OnEnvironmentChanged, value => seen = value.Apply(company, seen)));
        component.WaitForAssertion(() => Assert.NotNull(seen)); Assert.True(seen!.Matches(company));
        company.Clear(); component.WaitForAssertion(() => Assert.Null(seen));
    }

    [Fact]
    public void LateClearOrResultCannotReplaceAnotherCompanySelection()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        var oldVersion = company.Version;
        var oldCompany = company.Selection!.Scope;
        var oldSettings = new CompanyEnvironmentPreview(oldCompany, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], []) { TimeZoneId = "Asia/Kuwait" };
        var old = new CompanyEnvironmentEffectiveReview(oldCompany, new string('A',64), new string('B',64), null, new string('C',64), oldSettings) { ReviewStatus = "Current" };
        company.Clear(); Assert.True(company.TrySelect(company.Version, "actor", new(new("tenant", "other"), ["Engineer"], [])));
        var newSettings = oldSettings with { Company = company.Selection!.Scope, TimeZoneId = "Europe/Oslo" };
        var current = old with { Company = company.Selection.Scope, Settings = newSettings };
        var display = new CompanyEnvironmentDisplayUpdate(company.Version, current).Apply(company, null);
        Assert.NotNull(display); Assert.Same(display, new CompanyEnvironmentDisplayUpdate(oldVersion, null).Apply(company, display));
        Assert.Same(display, new CompanyEnvironmentDisplayUpdate(oldVersion, old).Apply(company, display));
        Assert.Null(new CompanyEnvironmentDisplayUpdate(company.Version, null).Apply(company, display));
    }

    private sealed class ManualPollClock : CompanyEnvironmentPollClock
    {
        private readonly System.Threading.Channels.Channel<bool> ticks = System.Threading.Channels.Channel.CreateUnbounded<bool>();
        public override async Task WaitAsync(CancellationToken cancellationToken) => _ = await ticks.Reader.ReadAsync(cancellationToken);
        public void Tick() => ticks.Writer.TryWrite(true);
    }

    [Fact]
    public void OtherSessionChangesAreRecheckedWithoutLeakingTheOldDisplay()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var handler = new Handler(false); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var clock = new ManualPollClock(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton<CompanyEnvironmentPollClock>(clock); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        CompanyEnvironmentDisplay? seen = null;
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(p => p.Add(x => x.OnEnvironmentChanged, value => seen = value.Apply(company, seen)));
        component.WaitForAssertion(() => { Assert.Equal("Asia/Kuwait", seen?.Review.Settings.TimeZoneId); Assert.Contains("Time zone: Asia/Kuwait", component.Markup); Assert.Contains("Diameter: in", component.Markup); });
        handler.Zone = "Europe/Oslo"; clock.Tick(); component.WaitForAssertion(() => { Assert.Equal("Europe/Oslo", seen?.Review.Settings.TimeZoneId); Assert.Contains("Time zone: Europe/Oslo", component.Markup); });
        handler.Status = "ReviewRequired"; clock.Tick(); component.WaitForAssertion(() => { Assert.Null(seen); Assert.Contains("needs review", component.Markup); Assert.DoesNotContain("Europe/Oslo", component.Markup); });
        handler.Status = "Current"; clock.Tick(); component.WaitForAssertion(() => Assert.NotNull(seen));
        handler.Fail = true; clock.Tick(); component.WaitForAssertion(() => { Assert.Null(seen); Assert.Contains("needs review", component.Markup); });
        Assert.Equal(5, handler.Calls); component.Dispose(); clock.Tick(); Assert.Equal(5, handler.Calls);
    }

    [Fact]
    public void ConfirmedSetupChangeRefreshesTheCurrentCircuitDisplay()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var handler = new Handler(false); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var changes = new CompanyEnvironmentChangeSignal(); using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(changes); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        CompanyEnvironmentDisplay? seen = null;
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(p => p.Add(x => x.OnEnvironmentChanged, value => seen = value.Apply(company, seen)));
        component.WaitForAssertion(() => Assert.Equal("Asia/Kuwait", seen?.Review.Settings.TimeZoneId));
        handler.Zone = "Europe/Oslo"; component.InvokeAsync(changes.Notify);
        component.WaitForAssertion(() => Assert.Equal("Europe/Oslo", seen?.Review.Settings.TimeZoneId)); Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public void ChangedSharedSettingsShowReviewRequiredInsteadOfEffectiveUnits()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var http = new HttpClient(new Handler(false, "ReviewRequired")) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>(); component.WaitForAssertion(() => Assert.Contains("needs review", component.Markup)); Assert.DoesNotContain("Kuwait", component.Markup);
    }

    [Fact]
    public void IncompleteUnitsAreNotDisplayedAsCurrentCompanySetup()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var handler = new Handler(false) { Diameter = "" }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>();
        component.WaitForAssertion(() => Assert.Contains("needs review", component.Markup));
        Assert.DoesNotContain("Kuwait", component.Markup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void IndicatorShowsOnlyCurrentCompanyAndClearsOnSwitch(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Engineer"], [])));
        using var http = new HttpClient(new Handler(foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton<AuthenticationStateProvider>(new TestAuth()); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(new CompanyEnvironmentPollClock()); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var component = ui.RenderComponent<CompanyEnvironmentIndicator>();
        component.WaitForAssertion(() => Assert.Contains(foreign ? "needs review" : "Kuwait", component.Markup));
        company.Clear(); component.WaitForAssertion(() => Assert.DoesNotContain("Kuwait", component.Markup));
    }
}
#endif
