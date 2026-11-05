using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.DataManagement;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.HSE.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyEnvironmentSetupTests
{
    private sealed class IntentMemory : IGhgSelectionIntentStore
    {
        public GhgSelectionIntent? Value;
        public bool FailRetain;
        public Task<GhgSelectionIntent?> ReadAsync(CompanyScope company, string actor) => Task.FromResult(Value?.Company == company && Value.Actor == actor ? Value : null);
        public Task RetainAsync(GhgSelectionIntent intent) { if (FailRetain) throw new InvalidOperationException("Storage unavailable"); if (Value is not null && Value != intent) throw new InvalidOperationException(); Value = intent; return Task.CompletedTask; }
        public Task ClearAsync(GhgSelectionIntent intent) { if (Value is not null && Value != intent) throw new InvalidOperationException(); Value = null; return Task.CompletedTask; }
    }
    private sealed class EnvironmentIntentMemory : ICompanyEnvironmentIntentStore
    {
        public CompanyEnvironmentIntent? Value;
        public bool FailRetain;
        public Task<CompanyEnvironmentIntent?> ReadEnvironmentAsync(CompanyScope company, string actor) => Task.FromResult(Value?.Company == company && Value.Actor == actor ? Value : null);
        public Task RetainEnvironmentAsync(CompanyEnvironmentIntent intent) { if (FailRetain) throw new InvalidOperationException("Storage unavailable"); if (Value is not null && JsonSerializer.Serialize(Value) != JsonSerializer.Serialize(intent)) throw new InvalidOperationException(); Value = intent; return Task.CompletedTask; }
        public Task ClearEnvironmentAsync(CompanyEnvironmentIntent intent) { if (Value is not null && JsonSerializer.Serialize(Value) != JsonSerializer.Serialize(intent)) throw new InvalidOperationException(); Value = null; return Task.CompletedTask; }
    }
    private sealed class DelayedIntentStore : IGhgSelectionIntentStore
    {
        public TaskCompletionSource<GhgSelectionIntent?> Read = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task<GhgSelectionIntent?> ReadAsync(CompanyScope company, string actor) => Read.Task;
        public Task RetainAsync(GhgSelectionIntent intent) => throw new NotSupportedException();
        public Task ClearAsync(GhgSelectionIntent intent) => Task.CompletedTask;
    }
    private sealed class OrganizationHandler(bool foreign) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.EndsWith("/api/company-environment/organization", request.RequestUri!.AbsolutePath);
            var value = new CompanyOrganizationReview(new("tenant", foreign ? "other" : "company"), new string('A', 64), "Active");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json") });
        }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CompanyOrganizationScreenAcceptsOnlySelectedCompanyAndClearsOnSwitch(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new OrganizationHandler(foreign); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod("ReviewOrganization", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains(foreign ? "organization could not be confirmed" : "active PPDM business-associate record", page.Markup);
        await page.InvokeAsync(company.Clear);
        page.WaitForAssertion(() => Assert.DoesNotContain("active PPDM business-associate record", page.Markup));
    }

    private sealed class DelayedEnvironmentHandler : HttpMessageHandler
    {
        public TaskCompletionSource<bool> Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<bool> Release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Started.TrySetResult(true); await Release.Task;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(new CompanyEnvironmentReadResult(new("tenant", "company"), null)), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task PendingRecoverySurvivesIndependentEnvironmentRead()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        var original = new GhgSelectionIntent(new("tenant", "company"), "actor", new("KW", new string('A', 64), new string('B', 64), "Original", "", Guid.NewGuid().ToString("N")));
        var intents = new DelayedIntentStore(); using var handler = new GhgHandler(false); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(intents); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod("LoadSaved", flags)!.Invoke(page.Instance, null)!);
        intents.Read.SetResult(original);
        page.WaitForAssertion(() => Assert.Contains("A previous selection is awaiting confirmation", page.Markup));
        Assert.Equal(original.Request, typeof(CompanyEnvironmentSetup).GetField("_ghgPending", flags)!.GetValue(page.Instance));
    }

    [Fact]
    public async Task OverlappingActionDoesNotInvalidateOrStrandActiveRead()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new DelayedEnvironmentHandler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        Task Call(string method) => page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        var read = Call("LoadSaved"); await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await Call("LoadGhgSelection"); handler.Release.SetResult(true); await read;
        Assert.False((bool)typeof(CompanyEnvironmentSetup).GetField("_busy", flags)!.GetValue(page.Instance)!);
        Assert.True((bool)typeof(CompanyEnvironmentSetup).GetField("_settingsLoaded", flags)!.GetValue(page.Instance)!);
    }
    private sealed class TabStorage : IJSRuntime
    {
        public Dictionary<string, string> Values = [];
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, default, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken token, object?[]? args)
        {
            var key = (string)args![0]!;
            if (identifier == "sessionStorage.setItem") Values[key] = (string)args[1]!;
            else if (identifier == "sessionStorage.removeItem") Values.Remove(key);
            else if (identifier == "sessionStorage.getItem") return ValueTask.FromResult(Values.TryGetValue(key, out var value) ? JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value))! : default!);
            else throw new InvalidOperationException(identifier);
            return ValueTask.FromResult(default(T)!);
        }
    }

    [Fact]
    public async Task ProtectedMembershipRemovalRetainsExactActorAndCompanyRequest()
    {
        var storage = new TabStorage(); var provider = new EphemeralDataProtectionProvider();
        var first = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var second = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var scope = new CompanyScope("tenant", "company");
        var intent = new CompanyMembershipRetirementIntent(scope, "actor", "member", new(Guid.NewGuid().ToString("N"), "revision", "Transfer complete"));
        await first.RetainMembershipRetirementAsync(intent);
        Assert.Equal(intent, await second.ReadMembershipRetirementAsync(scope, "actor"));
        Assert.Null(await second.ReadMembershipRetirementAsync(scope, "other"));
        Assert.Null(await second.ReadMembershipRetirementAsync(new("tenant", "other"), "actor"));
        Assert.DoesNotContain("member", string.Join(" ", storage.Values.Values));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.RetainMembershipRetirementAsync(intent with { Request = intent.Request with { Reason = "Different" } }));
        await second.ClearMembershipRetirementAsync(intent); Assert.Null(await first.ReadMembershipRetirementAsync(scope, "actor"));
    }

    [Fact]
    public async Task ProtectedRoleRemovalIsScopedAndPreservesUnconfirmedRequest()
    {
        var storage = new TabStorage(); var provider = new EphemeralDataProtectionProvider();
        var first = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var second = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var scope = new CompanyScope("tenant", "company");
        var intent = new CompanyRoleRevocationIntent(scope, "actor", "member", "assignment");
        await first.RetainRevocationAsync(intent);
        Assert.Equal(intent, await second.ReadRevocationAsync(scope, "actor"));
        Assert.Null(await second.ReadRevocationAsync(scope, "other"));
        Assert.Null(await second.ReadRevocationAsync(new("tenant", "other"), "actor"));
        Assert.DoesNotContain("member", string.Join(" ", storage.Values.Values));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.RetainRevocationAsync(intent with { AssignmentId = "other" }));
        await second.ClearRevocationAsync(intent); Assert.Null(await first.ReadRevocationAsync(scope, "actor"));
    }

    [Fact]
    public async Task ProtectedRoleRequestIsScopedAndCannotReplaceAnUnconfirmedRequest()
    {
        var storage = new TabStorage(); var provider = new EphemeralDataProtectionProvider();
        var first = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var second = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var scope = new CompanyScope("tenant", "company");
        var intent = new CompanyRoleIntent(scope, "actor", "member", new("role", "Approved"));
        await first.RetainRoleAsync(intent);
        Assert.Equal(intent, await second.ReadRoleAsync(scope, "actor"));
        Assert.Null(await second.ReadRoleAsync(scope, "other"));
        Assert.Null(await second.ReadRoleAsync(new("tenant", "other"), "actor"));
        Assert.DoesNotContain("member", string.Join(" ", storage.Values.Values));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.RetainRoleAsync(intent with { Request = intent.Request with { Reason = "Different" } }));
        await second.ClearRoleAsync(intent); Assert.Null(await first.ReadRoleAsync(scope, "actor"));
    }

    [Fact]
    public async Task ProtectedMembershipRequestIsScopedAndPreservesOriginal()
    {
        var storage = new TabStorage(); var provider = new EphemeralDataProtectionProvider();
        var first = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var second = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var scope = new CompanyScope("tenant", "company");
        var intent = new CompanyMembershipIntent(scope, "actor", new("member", "role", "Approved", Guid.NewGuid().ToString("N")));
        await first.RetainMembershipAsync(intent);
        Assert.Equal(intent, await second.ReadMembershipAsync(scope, "actor"));
        Assert.Null(await second.ReadMembershipAsync(scope, "other"));
        Assert.Null(await second.ReadMembershipAsync(new("tenant", "other"), "actor"));
        Assert.DoesNotContain("member", string.Join(" ", storage.Values.Values));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.RetainMembershipAsync(intent with { Request = intent.Request with { IntentId = Guid.NewGuid().ToString("N") } }));
        await second.ClearMembershipAsync(intent);
        Assert.Null(await first.ReadMembershipAsync(scope, "actor"));
    }

    [Fact]
    public async Task ProtectedInitialCompanyRequestSurvivesReloadAndIsActorScoped()
    {
        var storage = new TabStorage(); var provider = new EphemeralDataProtectionProvider();
        var first = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var second = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var intent = new InitialCompanyIntent("actor", new("tenant", "Tenant", "BA", "Setup", Guid.NewGuid().ToString("N")));
        await first.RetainInitialCompanyAsync(intent);
        Assert.Equal(intent, await second.ReadInitialCompanyAsync("actor")); Assert.Null(await second.ReadInitialCompanyAsync("other"));
        Assert.DoesNotContain("tenant", string.Join(" ", storage.Values.Values));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.RetainInitialCompanyAsync(intent with { Request = intent.Request with { IntentId = Guid.NewGuid().ToString("N") } }));
        await second.ClearInitialCompanyAsync(intent); Assert.Null(await first.ReadInitialCompanyAsync("actor"));
    }

    [Fact]
    public async Task ProtectedSystemIntentIsScopedToActorAndPreservesOriginal()
    {
        var storage = new TabStorage(); var provider = new EphemeralDataProtectionProvider();
        var first = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var second = new CompanySetupIntentStore(new ProtectedSessionStorage(storage, provider));
        var intent = new SystemSetupIntent("actor", new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")));
        await first.RetainSystemAsync(intent);
        Assert.Equal(intent, await second.ReadSystemAsync("actor"));
        Assert.Null(await second.ReadSystemAsync("other"));
        Assert.DoesNotContain("TOOLS", string.Join(" ", storage.Values.Values));
        await Assert.ThrowsAsync<InvalidOperationException>(() => second.RetainSystemAsync(intent with { Request = intent.Request with { IntentId = Guid.NewGuid().ToString("N") } }));
        await second.ClearSystemAsync(intent);
        Assert.Null(await first.ReadSystemAsync("actor"));
    }

    [Fact]
    public async Task ProtectedSelectionIntentIsEncryptedScopedAndPreservesOriginal()
    {
        var tab = new TabStorage(); var protection = new EphemeralDataProtectionProvider();
        CompanySetupIntentStore Store() => new(new ProtectedSessionStorage(tab, protection));
        var original = new GhgSelectionIntent(new("tenant", "company"), "actor", new("KW", new string('A', 64), new string('B', 64), "Private rationale", "", Guid.NewGuid().ToString("N")));
        await Store().RetainAsync(original); Assert.DoesNotContain("Private rationale", Assert.Single(tab.Values).Value);
        Assert.Equal(original, await Store().ReadAsync(original.Company, original.Actor));
        Assert.Null(await Store().ReadAsync(new("tenant", "other"), original.Actor)); Assert.Null(await Store().ReadAsync(original.Company, "other"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainAsync(original with { Request = original.Request with { Rationale = "Changed" } }));
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().ClearAsync(original with { Request = original.Request with { Rationale = "Changed" } }));
        await Store().ClearAsync(original); Assert.Empty(tab.Values);
        await Store().RetainAsync(original); tab.Values[Assert.Single(tab.Values).Key] = "corrupted";
        await Assert.ThrowsAnyAsync<Exception>(() => Store().ReadAsync(original.Company, original.Actor));
    }
    [Theory]
    [InlineData("KW", "Asia/Kuwait")]
    [InlineData("US", "America/Denver")]
    public void GuidedTimeZonePreservesLocaleAndIsValidatedByTheToolEnvironment(string country, string zone)
    {
        var json = CompanyEnvironmentOverrideEditor.Apply("{\"locale\":{\"calendar\":\"Gregorian\"},\"units\":{\"depthUnit\":\"m\"}}", null, null, null, null, zone);
        using var document = JsonDocument.Parse(json); Assert.Equal("Gregorian", document.RootElement.GetProperty("locale").GetProperty("calendar").GetString());
        var preview = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(new("tenant", "company"), new(country, json));
        Assert.Equal(zone, preview.TimeZoneId);
    }

    [Theory]
    [InlineData("KW", "ar-KW", "Asia/Kuwait")]
    [InlineData("NO", "nb-NO", "Europe/Oslo")]
    public void GuidedDisplayFormatsPreserveTimeZoneAndResolveThroughTheTool(string country, string culture, string zone)
    {
        var json = CompanyEnvironmentOverrideEditor.Apply("{}", null, null, null, null, zone,
            displayCulture: culture, dateFormat: "dd/MM/yyyy", dateTimeFormat: "dd/MM/yyyy HH:mm", calendar: "Gregorian");
        var company = new CompanyScope("tenant", "company");
        var preview = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(company, new(country, json));
        Assert.Equal(culture, preview.DisplayCulture); Assert.Equal("dd/MM/yyyy", preview.DateFormat);
        Assert.Equal("dd/MM/yyyy HH:mm", preview.DateTimeFormat); Assert.Equal("Gregorian", preview.Calendar); Assert.Equal(zone, preview.TimeZoneId);
        var inherited = CompanyEnvironmentOverrideEditor.UseInheritedDisplayFormats(json);
        using var document = JsonDocument.Parse(inherited); var locale = document.RootElement.GetProperty("locale");
        Assert.Equal(zone, locale.GetProperty("timeZone").GetString()); Assert.False(locale.TryGetProperty("culture", out _));
        Assert.False(locale.TryGetProperty("dateFormat", out _)); Assert.False(locale.TryGetProperty("dateTimeFormat", out _)); Assert.False(locale.TryGetProperty("calendar", out _));
    }

    [Fact]
    public void TimeZoneDoesNotFallBackToTheApiHostForMissingOrInvalidValues()
    {
        var company = new CompanyScope("tenant", "company");
        var missing = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(company, new("KW", "{\"locale\":{\"timeZone\":\"\"}}"));
        Assert.Null(missing.TimeZoneId);
        var json = CompanyEnvironmentOverrideEditor.Apply("{}", null, null, null, null, "not/a-real-zone");
        Assert.Throws<ArgumentException>(() => Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(company, new("KW", json)));
    }

    [Fact]
    public void CommonOverridesPreserveOtherSettingsAndMatchOilandGasToolsSchema()
    {
        var json = CompanyEnvironmentOverrideEditor.Apply("{\"Units\":{\"weightUnit\":\"kg/m\"},\"custom\":{\"items\":[1,2]}}", "ar-KW", "ft", "psi", "in");
        using var document = JsonDocument.Parse(json); var root = document.RootElement;
        Assert.Equal("kg/m", root.GetProperty("Units").GetProperty("weightUnit").GetString());
        Assert.Equal(2, root.GetProperty("custom").GetProperty("items").GetArrayLength());
        var preview = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.Preview(new("tenant", "company"), new("KW", json));
        Assert.Equal("ar-KW", preview.Culture); Assert.Equal("ft", preview.DepthUnit); Assert.Equal("psi", preview.PressureUnit); Assert.Equal("in", preview.DiameterUnit);
    }

    [Theory]
    [InlineData("KW", "kg/m", "kN")]
    [InlineData("US", "lbm/ft", "klbf")]
    public void GuidedWeightAndForceOverridesReachTheToolPreviewAndEffectiveEnvironment(string country, string weight, string force)
    {
        var json = CompanyEnvironmentOverrideEditor.Apply("{\"locale\":{\"calendar\":\"Gregorian\"},\"culture\":\"en-US\"}", null, null, null, null,
            unitSystem: "oilfield", weight: weight, force: force);
        using var document = JsonDocument.Parse(json);
        Assert.Equal("Gregorian", document.RootElement.GetProperty("locale").GetProperty("calendar").GetString());
        var company = new CompanyScope("tenant", "company");
        var preview = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.Preview(company, new(country, json));
        Assert.True(preview.Warnings.Length == 0, string.Join(" | ", preview.Warnings));
        Assert.Equal("oilfield", preview.UnitSystem); Assert.Equal(weight, preview.WeightUnit); Assert.Equal(force, preview.ForceUnit);
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var environment = Beep.OilandGas.ApiService.Services.CompanyEnvironmentEffectiveResolver.BuildEnvironment(country, json, baseline, null);
        Assert.Equal(weight, environment.Units.WeightUnit); Assert.Equal(force, environment.Units.ForceUnit);
    }

    [Fact]
    public void RegionalOverridesReachToolPreviewAndCanIndependentlyReturnToCountryDefaults()
    {
        var company = new CompanyScope("tenant", "company");
        var country = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(company, new("KW", "{}"));
        var json = CompanyEnvironmentOverrideEditor.ApplyRegionLists("{\"Region\":{\"name\":\"Kuwait\"},\"culture\":\"ar-KW\"}", "EPSG:32638\nEPSG:4326", "Company regulator");
        var preview = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(company, new("KW", json));
        Assert.Equal(new[] { "EPSG:32638", "EPSG:4326" }, preview.CoordinateSystems);
        Assert.Equal(new[] { "Company regulator" }, preview.Regulators);
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var effective = Beep.OilandGas.ApiService.Services.CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json, baseline, null);
        Assert.Equal(preview.CoordinateSystems, effective.Region.CoordinateSystems);
        Assert.Equal(preview.Regulators, effective.Region.Regulators);
        var inheritedCoordinates = CompanyEnvironmentOverrideEditor.UseInheritedRegionList(json, true);
        var restored = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(company, new("KW", inheritedCoordinates));
        Assert.Equal(country.CoordinateSystems, restored.CoordinateSystems);
        Assert.Equal(preview.Regulators, restored.Regulators);
        using var document = JsonDocument.Parse(CompanyEnvironmentOverrideEditor.UseInheritedRegionList(inheritedCoordinates, false));
        Assert.Equal("Kuwait", document.RootElement.GetProperty("Region").GetProperty("name").GetString());
        Assert.Equal("ar-KW", document.RootElement.GetProperty("culture").GetString());
    }

    [Theory]
    [InlineData("EPSG:4326\nepsg:4326")]
    [InlineData("bad\tentry")]
    public void RegionalListsRejectDuplicatesAndControlCharacters(string value)
    {
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ApplyRegionLists("{}", value, null));
    }

    [Fact]
    public void RegionalListsRejectOversizedOrAmbiguousInput()
    {
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ApplyRegionLists("{}", new string('a', 129), null));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ApplyRegionLists("{}", string.Join('\n', Enumerable.Range(0, 33)), null));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ApplyRegionLists("{\"region\":{},\"REGION\":{}}", "EPSG:4326", null));
    }

    [Fact]
    public async Task RegionalDraftChangesInvalidatePreviewAndCompanyChangeClearsInputs()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("Preview");
        Assert.NotNull(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        typeof(CompanyEnvironmentSetup).GetField("_editCoordinateSystems", flags)!.SetValue(page.Instance, "EPSG:4326");
        typeof(CompanyEnvironmentSetup).GetField("_editRegulators", flags)!.SetValue(page.Instance, "Regulator");
        await Call("ApplyRegionLists");
        Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        var json = (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!;
        Assert.Contains("EPSG:4326", json); Assert.Contains("Regulator", json);
        await Call("UseInheritedCoordinateSystems");
        json = (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!;
        Assert.DoesNotContain("EPSG:4326", json); Assert.Contains("Regulator", json);
        typeof(CompanyEnvironmentSetup).GetField("_editRegulators", flags)!.SetValue(page.Instance, "unsaved");
        await page.InvokeAsync(company.Clear);
        page.WaitForAssertion(() => Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editRegulators", flags)!.GetValue(page.Instance)));
    }

    [Fact]
    public void InheritingCommonSettingsPreservesUnrelatedCompanyConfiguration()
    {
        var json = CompanyEnvironmentOverrideEditor.UseInheritedCommonSettings("{\"Culture\":\"ar-KW\",\"Locale\":{\"TimeZone\":\"Asia/Kuwait\",\"calendar\":\"Gregorian\"},\"Units\":{\"System\":\"oilfield\",\"DepthUnit\":\"ft\",\"PressureUnit\":\"psi\",\"DiameterUnit\":\"in\",\"WeightUnit\":\"lbm/ft\",\"ForceUnit\":\"klbf\",\"custom\":\"keep\"},\"dataSources\":{\"well\":{\"path\":\"PPDM_CORE\"}}}");
        using var document = JsonDocument.Parse(json); var root = document.RootElement;
        Assert.False(root.TryGetProperty("Culture", out _));
        Assert.False(root.GetProperty("Locale").TryGetProperty("TimeZone", out _));
        Assert.Equal("Gregorian", root.GetProperty("Locale").GetProperty("calendar").GetString());
        Assert.Equal("keep", root.GetProperty("Units").GetProperty("custom").GetString());
        Assert.Single(root.GetProperty("Units").EnumerateObject());
        Assert.Equal("PPDM_CORE", root.GetProperty("dataSources").GetProperty("well").GetProperty("path").GetString());
    }

    [Theory]
    [InlineData("{\"units\":{},\"UNITS\":{}}")]
    [InlineData("{\"units\":\"invalid\"}")]
    [InlineData("[]")]
    public void InheritingCommonSettingsRejectsInvalidData(string json)
    {
        Assert.ThrowsAny<Exception>(() => CompanyEnvironmentOverrideEditor.UseInheritedCommonSettings(json));
    }

    [Theory]
    [InlineData("{\"units\":{},\"UNITS\":{}}")]
    [InlineData("{\"units\":\"invalid\"}")]
    [InlineData("[]")]
    [InlineData("{broken")]
    public void CommonOverrideEditorRejectsAmbiguousOrInvalidExistingData(string json)
    {
        Assert.ThrowsAny<Exception>(() => CompanyEnvironmentOverrideEditor.Apply(json, null, "ft", null, null));
    }

    [Fact]
    public async Task ApplyingCommonOverridesInvalidatesPreviousPreview()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("Preview"); await page.InvokeAsync(() => page.Render()); Assert.Contains("ar-KW", page.Markup);
        typeof(CompanyEnvironmentSetup).GetField("_editDepth", flags)!.SetValue(page.Instance, "ft");
        typeof(CompanyEnvironmentSetup).GetField("_editWeight", flags)!.SetValue(page.Instance, "lbm/ft");
        typeof(CompanyEnvironmentSetup).GetField("_editForce", flags)!.SetValue(page.Instance, "klbf");
        typeof(CompanyEnvironmentSetup).GetField("_editDisplayCulture", flags)!.SetValue(page.Instance, "ar-KW");
        typeof(CompanyEnvironmentSetup).GetField("_editDateFormat", flags)!.SetValue(page.Instance, "dd/MM/yyyy");
        await Call("ApplyCommonOverrides"); await page.InvokeAsync(() => page.Render());
        Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        Assert.Contains("Preview the settings before saving", page.Markup);
        Assert.Contains("ft", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
        Assert.Contains("klbf", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
        Assert.Contains("dd/MM/yyyy", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
        await Call("UseInheritedDisplayFormats");
        Assert.DoesNotContain("dd/MM/yyyy", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
        Assert.Contains("klbf", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
        typeof(CompanyEnvironmentSetup).GetField("_editUnitSystem", flags)!.SetValue(page.Instance, "oilfield");
        typeof(CompanyEnvironmentSetup).GetField("_editWeight", flags)!.SetValue(page.Instance, "lbm/ft");
        typeof(CompanyEnvironmentSetup).GetField("_editForce", flags)!.SetValue(page.Instance, "klbf");
        typeof(CompanyEnvironmentSetup).GetField("_editDisplayCulture", flags)!.SetValue(page.Instance, "ar-KW");
        typeof(CompanyEnvironmentSetup).GetField("_editDateFormat", flags)!.SetValue(page.Instance, "dd/MM/yyyy");
        await page.InvokeAsync(company.Clear);
        page.WaitForAssertion(() => Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editForce", flags)!.GetValue(page.Instance)));
        Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editUnitSystem", flags)!.GetValue(page.Instance));
        Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editWeight", flags)!.GetValue(page.Instance));
        Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editDisplayCulture", flags)!.GetValue(page.Instance));
        Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editDateFormat", flags)!.GetValue(page.Instance));
    }

    [Fact]
    public void GuidedCodeMappingPreservesOtherSettingsAndRestoresInheritedValue()
    {
        var shared = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"X\":\"Tubing\"}}}}}}", "", Guid.NewGuid().ToString("N")), null);
        var json = "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"Y\":\"Casing\"}}}}},\"culture\":\"ar-KW\"}";
        json = CompanyEnvironmentOverrideEditor.SetSourceCodeMapping(json, "well", "string", "kind", "X", "Casing");
        json = CompanyEnvironmentOverrideEditor.SetSourceCodeMapping(json, "WELL", "STRING", "KIND", "x", "Liner");
        var environment = Beep.OilandGas.ApiService.Services.CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json, shared, null);
        Assert.Empty(Beep.OilandGas.ApiService.Services.CompanyEnvironmentSources.ReviewEnvironment(environment));
        Assert.Equal("Liner", environment.DataSources["well"].Codes["string"]["kind"]["X"]);
        Assert.Equal("Casing", environment.DataSources["well"].Codes["string"]["kind"]["Y"]);
        json = CompanyEnvironmentOverrideEditor.RemoveSourceCodeMapping(json, "well", "string", "kind", "x");
        environment = Beep.OilandGas.ApiService.Services.CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", json, shared, null);
        Assert.Equal("Tubing", environment.DataSources["well"].Codes["string"]["kind"]["X"]);
        Assert.Equal("Casing", environment.DataSources["well"].Codes["string"]["kind"]["Y"]);
        using var parsed = JsonDocument.Parse(json);
        Assert.Equal("ar-KW", parsed.RootElement.GetProperty("culture").GetString());
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.RemoveSourceCodeMapping(json, "well", "string", "kind", "X"));
    }

    [Theory]
    [InlineData("well", "bad.entity", "kind", "X", "Tubing")]
    [InlineData("well", "string", "kind", "X\tbad", "Tubing")]
    [InlineData("well", "string", "kind", "X", "")]
    public void GuidedCodeMappingRejectsInvalidInput(string source, string entity, string field, string code, string value)
    {
        const string json = "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}";
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.SetSourceCodeMapping(json, source, entity, field, code, value));
    }

    [Fact]
    public void GuidedCodeMappingStillRequiresRealProfileReview()
    {
        const string json = "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}";
        var draft = CompanyEnvironmentOverrideEditor.SetSourceCodeMapping(json, "well", "string", "unknownField", "X", "Tubing");
        var shared = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{}", "", Guid.NewGuid().ToString("N")), null);
        var environment = Beep.OilandGas.ApiService.Services.CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", draft, shared, null);
        Assert.Contains(Beep.OilandGas.ApiService.Services.CompanyEnvironmentSources.ReviewEnvironment(environment), x => x.Contains("unknownField", StringComparison.Ordinal));
    }

    [Fact]
    public void CompanyMappingListRejectsAmbiguousOrMalformedDrafts()
    {
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ReadCompanyCodeMappings("{\"dataSources\":{\"well\":{\"codes\":{\"string\":{\"kind\":{\"X\":42}}}}}}"));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ReadCompanyCodeMappings("{\"dataSources\":{\"well\":{\"codes\":{\"string\":{\"kind\":{\"X\":\"Tubing\",\"x\":\"Liner\"}}}}}}"));
        Assert.Empty(CompanyEnvironmentOverrideEditor.ReadCompanyCodeMappings("{\"dataSources\":{\"well\":{\"kind\":\"beep\"}}}"));
    }

    [Fact]
    public async Task MappingListShowsOnlyCurrentDraftAndCanSelectAnExplicitMapping()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries");
        await Call("OverridesChanged", "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"string\":{\"kind\":{\"X\":\"Tubing\"}}}}}}");
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Explicit company mappings in this draft", page.Markup);
        Assert.Contains("Tubing", page.Markup);
        var rows = (IReadOnlyList<CompanySourceCodeMapping>)typeof(CompanyEnvironmentSetup).GetField("_mappingRows", flags)!.GetValue(page.Instance)!;
        var original = Assert.Single(rows);
        await Call("SelectMapping", original);
        Assert.Equal("well", typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.GetValue(page.Instance));
        Assert.Equal("X", typeof(CompanyEnvironmentSetup).GetField("_editMappingSourceCode", flags)!.GetValue(page.Instance));
        await Call("OverridesChanged", "{broken"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("Company mappings cannot be listed", page.Markup);
        Assert.Empty((IReadOnlyList<CompanySourceCodeMapping>)typeof(CompanyEnvironmentSetup).GetField("_mappingRows", flags)!.GetValue(page.Instance)!);
        typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.SetValue(page.Instance, "unchanged");
        await Call("SelectMapping", original);
        Assert.Equal("unchanged", typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(company.Clear);
        page.WaitForAssertion(() => Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.GetValue(page.Instance)));
    }

    [Fact]
    public async Task MappingDraftInvalidatesReviewAndCompanyChangeClearsUnsavedInput()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("Preview");
        typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.SetValue(page.Instance, "{\"dataSources\":{\"well\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}");
        typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.SetValue(page.Instance, "well");
        typeof(CompanyEnvironmentSetup).GetField("_editMappingEntity", flags)!.SetValue(page.Instance, "string");
        typeof(CompanyEnvironmentSetup).GetField("_editMappingField", flags)!.SetValue(page.Instance, "kind");
        typeof(CompanyEnvironmentSetup).GetField("_editMappingSourceCode", flags)!.SetValue(page.Instance, "X");
        typeof(CompanyEnvironmentSetup).GetField("_editMappingCanonicalCode", flags)!.SetValue(page.Instance, "Tubing");
        await Call("SetSourceMapping");
        Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        Assert.Contains("Tubing", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
        typeof(CompanyEnvironmentSetup).GetField("_editMappingSourceCode", flags)!.SetValue(page.Instance, "unsaved");
        await page.InvokeAsync(company.Clear);
        page.WaitForAssertion(() => Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editMappingSourceCode", flags)!.GetValue(page.Instance)));
    }

    [Fact]
    public void AddingBeepSourcePreservesMappingsAndPassesActualToolValidation()
    {
        var json = CompanyEnvironmentOverrideEditor.AddBeepSource("{\"dataSources\":{\"existing\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"WELL\":{\"STATUS\":{\"A\":\"ACTIVE\"}}}}},\"culture\":\"ar-KW\"}", "secondary", "PPDM_CORE");
        using var document = JsonDocument.Parse(json); var sources = document.RootElement.GetProperty("dataSources");
        Assert.Equal("ACTIVE", sources.GetProperty("existing").GetProperty("codes").GetProperty("WELL").GetProperty("STATUS").GetProperty("A").GetString());
        Assert.Equal("beep", sources.GetProperty("secondary").GetProperty("kind").GetString());
        Assert.Equal("PPDM_CORE", sources.GetProperty("secondary").GetProperty("path").GetString());
        Assert.Equal("ppdm39", sources.GetProperty("secondary").GetProperty("profile").GetString());
        var preview = Beep.OilandGas.ApiService.Services.CompanyEnvironmentPreviewer.ValidateDraft(new("tenant", "company"), new("KW", json));
        Assert.Empty(preview.Warnings);
    }

    [Theory]
    [InlineData("existing", "PPDM_CORE")]
    [InlineData("a/b", "PPDM_CORE")]
    [InlineData("new", "SECURITY")]
    [InlineData("new", "ppdm_core")]
    [InlineData("new", "C:/db")]
    public void AddingBeepSourceRejectsConflictsAndInvalidIdentifiers(string name, string module)
    {
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.AddBeepSource("{\"dataSources\":{\"Existing\":{}}}", name, module));
    }

    [Fact]
    public void ChangingBeepSourceModulePreservesOtherSettingsAndRequiresExplicitBeepSource()
    {
        const string original = "{\"DataSources\":{\"Wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\",\"codes\":{\"WELL\":{\"STATUS\":{\"A\":\"ACTIVE\"}}}}},\"culture\":\"ar-KW\"}";
        var updated = CompanyEnvironmentOverrideEditor.ChangeBeepSourceModule(original, "wells", "PPDM_SECONDARY");
        using var document = JsonDocument.Parse(updated); var root = document.RootElement; var source = root.GetProperty("DataSources").GetProperty("Wells");
        Assert.Equal("PPDM_SECONDARY", source.GetProperty("path").GetString());
        Assert.Equal("ACTIVE", source.GetProperty("codes").GetProperty("WELL").GetProperty("STATUS").GetProperty("A").GetString());
        Assert.Equal("ar-KW", root.GetProperty("culture").GetString());
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ChangeBeepSourceModule(original, "missing", "PPDM_CORE"));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ChangeBeepSourceModule(original, "wells", "SECURITY"));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.ChangeBeepSourceModule("{\"dataSources\":{\"wells\":{\"kind\":\"other\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}", "wells", "PPDM_CORE"));
    }

    [Fact]
    public void RemovingCompanySourceOverrideRestoresInheritedSourceAndPreservesOtherSettings()
    {
        const string original = "{\"DataSources\":{\"Wells\":{\"kind\":\"beep\",\"path\":\"OTHER\",\"profile\":\"ppdm39\"},\"secondary\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}},\"culture\":\"ar-KW\"}";
        var updated = CompanyEnvironmentOverrideEditor.RemoveSourceOverride(original, "wells");
        using var document = JsonDocument.Parse(updated);
        Assert.False(document.RootElement.GetProperty("DataSources").TryGetProperty("Wells", out _));
        Assert.Equal("ar-KW", document.RootElement.GetProperty("culture").GetString());
        Assert.Equal("PPDM_CORE", document.RootElement.GetProperty("DataSources").GetProperty("secondary").GetProperty("path").GetString());
        var baseline = SystemSetupIdentity.Prepare(new("TOOLS", "DEFAULT", "1", "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}", "", Guid.NewGuid().ToString("N")), null);
        var environment = Beep.OilandGas.ApiService.Services.CompanyEnvironmentEffectiveResolver.BuildEnvironment("KW", updated, baseline, null);
        Assert.Equal("PPDM_CORE", environment.DataSources["wells"].Path);
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.RemoveSourceOverride(original, "missing"));
        Assert.Throws<ArgumentException>(() => CompanyEnvironmentOverrideEditor.RemoveSourceOverride("{\"dataSources\":{},\"DATASOURCES\":{}}", "wells"));
    }

    [Fact]
    public async Task RemovingCompanySourceInvalidatesPreviewAndDoesNotSave()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler(false); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries"); await Call("CountryChanged", "KW");
        await Call("OverridesChanged", "{\"dataSources\":{\"wells\":{\"kind\":\"beep\",\"path\":\"PPDM_CORE\",\"profile\":\"ppdm39\"}}}");
        await Call("Preview"); Assert.NotNull(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.SetValue(page.Instance, "wells");
        await Call("RemoveCompanySource"); await page.InvokeAsync(() => page.Render());
        Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        Assert.Contains("Preview the effective settings before saving", page.Markup);
        Assert.Equal("{}", typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance));
        await Call("Save"); Assert.Equal(0, handler.Writes);
    }

    [Fact]
    public async Task AddingCompanySourceInvalidatesPriorPreview()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("Preview");
        Assert.NotNull(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.SetValue(page.Instance, "wells"); typeof(CompanyEnvironmentSetup).GetField("_editSourceModule", flags)!.SetValue(page.Instance, "PPDM_CORE");
        await Call("AddCompanySource"); await page.InvokeAsync(() => page.Render());
        Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance)); Assert.Contains("Source draft updated", page.Markup);
        Assert.Contains("PPDM_CORE", (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!);
    }

    private static CompanyEnvironmentDraftReview Review(CompanyScope company)
    {
        var settings = new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", ["UTM"], ["regulator"], []);
        var system = new string('A', 64); const string version = "build";
        return new(settings, system, null, version, CompanyEnvironmentDraftIdentity.ForReview(company, new("KW", "{}"), system, null, version));
    }
    private sealed class Handler(bool foreign) : HttpMessageHandler
    {
        public int Writes;
        public bool SharedSetupUnavailable;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.Equal("company", request.Headers.GetValues(CompanyRequestHeaders.Company).Single());
            if (request.Method == HttpMethod.Put) Writes++;
            if (request.Method == HttpMethod.Post && SharedSetupUnavailable) return new HttpResponseMessage(HttpStatusCode.Conflict);
            object result;
            if (request.Method == HttpMethod.Get) result = new[] { new CompanyEnvironmentCountry("KW", "Kuwait") };
            else
            {
                var company = new CompanyScope("tenant", foreign ? "other" : "company");
                var draft = JsonSerializer.Deserialize<CompanyEnvironmentPreviewRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                var review = Review(company);
                result = review with { DraftRevision = CompanyEnvironmentDraftIdentity.ForReview(company, draft, review.SystemRevision, review.CountryRevision, review.ToolsVersion) };
            }
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task PreviewConflictDirectsAdministratorToSharedSystemSetup()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new Handler(false) { SharedSetupUnavailable = true };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("Preview"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("The combined settings could not be confirmed", page.Markup);
        Assert.Contains("/setup/system-repository", page.Markup);
        Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_preview", flags)!.GetValue(page.Instance));
        Assert.Equal(0, handler.Writes);
    }

    private sealed class SaveHandler : HttpMessageHandler
    {
        public CompanyEnvironmentConfiguration? Saved;
        public int Writes;
        public bool LoseResponse = true;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            var company = new CompanyScope("tenant", "company");
            object result;
            if (request.Method == HttpMethod.Put)
            {
                var command = JsonSerializer.Deserialize<CompanyEnvironmentReviewedSaveRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                Saved = CompanyEnvironmentIdentity.Prepare(company, command.Change, "build", Saved, command.Review.SystemRevision, command.Review.CountryRevision); Writes++;
                if (LoseResponse) throw new HttpRequestException("The response was lost after committing.");
                result = Saved;
            }
            else if (request.Method == HttpMethod.Post)
            {
                var draft = JsonSerializer.Deserialize<CompanyEnvironmentPreviewRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                var review = Review(company);
                result = review with { DraftRevision = CompanyEnvironmentDraftIdentity.ForReview(company, draft, review.SystemRevision, review.CountryRevision, review.ToolsVersion) };
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("countries", StringComparison.Ordinal))
                result = new[] { new CompanyEnvironmentCountry("KW", "Kuwait") };
            else result = new CompanyEnvironmentReadResult(company, Saved);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task StartCompanySetupLoadsSavedSettingsAndCountriesWithoutWriting(bool existing)
    {
        using var company = new CompanyClientContext();
        var scope = new CompanyScope("tenant", "company");
        Assert.True(company.TrySelect(company.Clear(), "actor", new(scope, ["Administrator"], [])));
        using var handler = new SaveHandler();
        if (existing) handler.Saved = CompanyEnvironmentIdentity.Prepare(scope, new("KW", "{}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyEnvironmentSetup).GetField("_environmentRecoveryReady", flags)!.GetValue(page.Instance)!));
        await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod("BeginCompanySetup", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Equal(0, handler.Writes);
        Assert.NotNull(typeof(CompanyEnvironmentSetup).GetField("_countries", flags)!.GetValue(page.Instance));
        Assert.Equal(existing ? "KW" : "", typeof(CompanyEnvironmentSetup).GetField("_country", flags)!.GetValue(page.Instance));
        Assert.Contains(existing ? "Saved settings loaded" : "Choose a country", page.Markup);
    }

    private sealed class VerificationHandler(bool foreign, bool invalidMapping = false, bool invalidReview = false, bool malformedMappings = false) : HttpMessageHandler
    {
        public string EffectiveRevision = new('A', 64);
        public string SourceRevision = new('A', 64);
        public string VerificationRevision = new('A', 64);
        public int MappingCount = 1;
        public string MappingOrigin = "System";
        private readonly CompanyScope company = new("tenant", "company");
        private readonly CompanyEnvironmentConfiguration saved = CompanyEnvironmentIdentity.Prepare(new("tenant", "company"),
            new("KW", "{}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null);
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result = request.Method == HttpMethod.Post && request.RequestUri!.AbsolutePath.EndsWith("test-well", StringComparison.Ordinal)
                ? new CompanyEnvironmentWellTest(foreign ? new("tenant", "other") : company, saved.Revision, "wells", "well-1", "Found", "North well", "field", false, "loader-build", new string('A', 64))
                : request.Method == HttpMethod.Post
                ? new CompanyEnvironmentVerification(foreign ? new("tenant", "other") : company, saved.Revision, saved.ToolsVersion,
                    ["Review time zone"], [new("Unit reference", "unit", "Passed", null)], false, VerificationRevision)
                : request.RequestUri!.AbsolutePath.EndsWith("sources", StringComparison.Ordinal)
                    ? new CompanyEnvironmentSourceReview(foreign ? new("tenant", "other") : company, saved.Revision,
                        [new CompanyEnvironmentSource("wells", "PPDM_CORE", "ppdm39", MappingCount, 1, "Bound") { MappingStatus = invalidMapping ? "ReviewRequired" : "ProfileChecked", MappingIssues = invalidMapping ? ["Invalid mapping"] : [], CodeMappings = malformedMappings ? [] : Enumerable.Range(0, MappingCount).Select(i => new CompanyEnvironmentCodeMapping("string", "kind", $"X{i:D3}", "Tubing", MappingOrigin)).ToArray() }], false, SourceRevision)
                    : request.RequestUri!.AbsolutePath.EndsWith("effective", StringComparison.Ordinal)
                        ? new CompanyEnvironmentEffectiveReview(foreign ? new("tenant", "other") : company, saved.Revision, new string('B', 64), null, EffectiveRevision,
                            new CompanyEnvironmentPreview(company, "KW", "Kuwait", "ar-KW", "metric", "m", "bar", "mm", [], [], [])) { ReviewStatus = invalidReview ? "Unconfirmed" : "Current" }
                    : new CompanyEnvironmentReadResult(company, saved);
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SourceReviewShowsOnlyConfirmedEffectiveMappingDetails(bool malformed)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new VerificationHandler(false, malformedMappings: malformed);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Call("LoadSaved"); await Call("ReviewSources"); await page.InvokeAsync(() => page.Render());
        Assert.Contains(malformed ? "saved source bindings could not be reviewed" : "Effective configured code overrides for wells", page.Markup);
        if (!malformed) { Assert.Contains("Tubing", page.Markup); Assert.Contains("ProfileChecked", page.Markup); Assert.Contains("shared defaults", page.Markup); }
        else Assert.DoesNotContain("Effective configured code overrides for wells", page.Markup);
        await page.InvokeAsync(company.Clear);
        page.WaitForAssertion(() => Assert.DoesNotContain("Effective configured code overrides for wells", page.Markup));
    }

    [Fact]
    public async Task SourceReviewPagesThroughAllEffectiveMappings()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new VerificationHandler(false) { MappingCount = 30 };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Call("LoadSaved"); await Call("ReviewSources"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("Showing 25 of 30 mappings", page.Markup); Assert.DoesNotContain("X029", page.Markup);
        var review = (CompanyEnvironmentSourceReview)typeof(CompanyEnvironmentSetup).GetField("_sources", flags)!.GetValue(page.Instance)!;
        await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod("ShowMoreMappings", flags)!.Invoke(page.Instance, [review.Sources[0]]));
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("X029", page.Markup); Assert.DoesNotContain("Showing 25 of 30 mappings", page.Markup);
    }

    [Fact]
    public async Task SourceReviewRejectsAnUnrecognizedMappingOrigin()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new VerificationHandler(false) { MappingOrigin = "Unconfirmed" };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Call("LoadSaved"); await Call("ReviewSources"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("saved source bindings could not be reviewed", page.Markup);
        Assert.DoesNotContain("Effective configured code overrides for wells", page.Markup);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NewEvidenceInvalidatesEarlierEvidenceFromAnotherEffectiveRevision(bool sourcesFirst)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new VerificationHandler(false);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadSaved");
        if (sourcesFirst) { await Invoke("ReviewSources"); handler.VerificationRevision = new string('C', 64); await Invoke("Verify"); }
        else { await Invoke("Verify"); handler.SourceRevision = new string('C', 64); await Invoke("ReviewSources"); }
        await page.InvokeAsync(() => page.Render());
        if (sourcesFirst) { Assert.Contains("Unit reference", page.Markup); Assert.DoesNotContain("Saved source bindings", page.Markup); }
        else { Assert.Contains("Saved source bindings", page.Markup); Assert.DoesNotContain("Unit reference", page.Markup); }
    }

    [Fact]
    public async Task RefreshingEffectiveSettingsClearsEarlierSourceAndWellEvidence()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new VerificationHandler(false);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company);
        ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method, params object[] args) => await page.InvokeAsync(() =>
            typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Invoke("LoadSaved"); await Invoke("ReviewSources"); await Invoke("TestSourceChanged", "wells");
        await Invoke("TestWellChanged", "well-1"); await Invoke("TestWell"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("North well", page.Markup); Assert.Contains("Saved source bindings", page.Markup);
        handler.EffectiveRevision = new string('C', 64);
        await Invoke("ReviewEffective"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("Effective saved settings", page.Markup);
        Assert.DoesNotContain("North well", page.Markup); Assert.DoesNotContain("Saved source bindings", page.Markup);
    }

    private sealed class GhgHandler(bool foreign) : HttpMessageHandler
    {
        public CompanyGhgSelection? Selection;
        public bool LoseResponse;
        public int Writes;
        public string? StatusOverride;
        private readonly CompanyScope company = new("tenant", "company");
        private readonly CompanyEnvironmentConfiguration saved = CompanyEnvironmentIdentity.Prepare(new("tenant", "company"),
            new("KW", "{}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.RequestUri!.AbsolutePath.EndsWith("ghg-selection", StringComparison.Ordinal) && request.Method == HttpMethod.Put)
            {
                var change = JsonSerializer.Deserialize<CompanyGhgSelectionRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                result = Selection = CompanyGhgSelectionIdentity.Prepare(company, "actor", change, Selection); Writes++;
                if (LoseResponse) throw new HttpRequestException("Response lost after commit.");
            }
            else if (request.RequestUri!.AbsolutePath.EndsWith("ghg-selection", StringComparison.Ordinal))
                result = new CompanyGhgSelectionReadResult(company, Selection, Selection is null ? "NotSelected" : StatusOverride ?? "Current");
            else if (request.RequestUri!.AbsolutePath.Contains("ghg-catalog", StringComparison.Ordinal))
            {
                var factor = new GhgFactorDefinition("test-factor", "flare", "volume", "mass", 1m, new(2026, 1, 1), new(2026, 12, 31), "test-source");
                var catalog = SystemSetupIdentity.Prepare(new("GHG_FACTORS", "KW", "1", JsonSerializer.Serialize(new GhgFactorCatalog("KW", [factor])), "", Guid.NewGuid().ToString("N")), null);
                result = new CompanyGhgCatalogReview(foreign ? new("tenant", "other") : company, saved.Revision, new string('B', 64), catalog);
            }
            else result = new CompanyEnvironmentReadResult(company, saved);
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }

    [Fact]
    public async Task CompanyCatalogSelectionCanRecoverALostSaveResponseAfterReopening()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new GhgHandler(false) { LoseResponse = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var intents = new IntentMemory(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        TestContext Context() { var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(intents); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); return ui; }
        using (var ui = Context())
        {
            var page = ui.RenderComponent<CompanyEnvironmentSetup>();
            async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
            await Call("LoadSaved"); await Call("GhgJurisdictionChanged", "KW"); await Call("ReviewGhgCatalog"); await Call("LoadGhgSelection");
            typeof(CompanyEnvironmentSetup).GetField("_ghgRationale", flags)!.SetValue(page.Instance, "Reviewed company needs");
            await Call("SaveGhgSelection"); Assert.NotNull(intents.Value);
        }
        using (var ui = Context())
        {
            var page = ui.RenderComponent<CompanyEnvironmentSetup>();
            await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod("LoadGhgSelection", flags)!.Invoke(page.Instance, null)!);
            await page.InvokeAsync(() => page.Render());
            Assert.Equal(1, handler.Writes); Assert.Null(intents.Value); Assert.Contains("original company selection was confirmed", page.Markup); Assert.Contains("not approval for reporting", page.Markup);
        }
    }

    [Fact]
    public async Task UnavailableRecoveryStoragePreventsSelectionWrite()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new GhgHandler(false); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory { FailRetain = true });
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadSaved"); await Call("GhgJurisdictionChanged", "KW"); await Call("ReviewGhgCatalog"); await Call("LoadGhgSelection");
        typeof(CompanyEnvironmentSetup).GetField("_ghgRationale", flags)!.SetValue(page.Instance, "Reviewed company needs");
        await Call("SaveGhgSelection"); await page.InvokeAsync(() => page.Render());
        Assert.Equal(0, handler.Writes); Assert.Contains("no save was sent", page.Markup);
    }

    [Fact]
    public async Task ChangedSelectionTellsAdministratorToReviewAgain()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new GhgHandler(false) { StatusOverride = "ReviewRequired" };
        handler.Selection = CompanyGhgSelectionIdentity.Prepare(new("tenant", "company"), "actor", new("KW", new string('A', 64), new string('B', 64), "reviewed", "", Guid.NewGuid().ToString("N")), null);
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod("LoadGhgSelection", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Review the selection again", page.Markup); Assert.Contains("not approval for reporting", page.Markup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task FactorCatalogReviewIsBoundToTheSelectedCompany(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new GhgHandler(foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadSaved"); await Call("GhgJurisdictionChanged", "KW"); await Call("ReviewGhgCatalog"); await page.InvokeAsync(() => page.Render());
        if (foreign) Assert.Contains("could not be confirmed", page.Markup);
        else { Assert.Contains("test-factor", page.Markup); Assert.Contains("approval have not been established", page.Markup); }
        company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("test-factor", page.Markup));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewShowsOnlyResultsForTheSelectedCompany(bool foreign)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler(foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method, params object[] args) => await page.InvokeAsync(() =>
        {
            var value = typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args);
            return value as Task ?? Task.CompletedTask;
        });
        await Invoke("LoadCountries"); await Invoke("CountryChanged", "KW"); await Invoke("Preview");
        await page.InvokeAsync(() => page.Render());
        if (foreign) Assert.Contains("could not be previewed", page.Markup);
        else Assert.Contains("ar-KW", page.Markup);
        Assert.Contains("does not activate the configuration", page.Markup);
        company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("ar-KW", page.Markup));
    }

    [Fact]
    public async Task InvalidSourceMappingShowsIssueAndDoesNotOfferWellTest()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new VerificationHandler(false, true)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Call("LoadSaved"); await Call("ReviewSources"); await page.InvokeAsync(() => page.Render());
        Assert.Contains("Invalid mapping", page.Markup); Assert.DoesNotContain("Test selected well header", page.Markup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LostEnvironmentSaveIsRecoveredAfterReopening(bool retry)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new SaveHandler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        var intents = new EnvironmentIntentMemory(); var changes = new CompanyEnvironmentChangeSignal(); var notifications = 0; changes.Changed += () => notifications++; var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        TestContext Context() { var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(changes); ui.Services.AddSingleton(company); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(intents); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company)); return ui; }
        using (var ui = Context())
        {
            var page = ui.RenderComponent<CompanyEnvironmentSetup>();
            async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
            await Call("LoadSaved"); await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("OverridesChanged", "{\"culture\":\"en-GB\"}"); await Call("Preview"); await Call("Save");
            Assert.NotNull(intents.Value); Assert.Equal(1, handler.Writes); Assert.Equal(0, notifications);
        }
        using (var ui = Context())
        {
            var page = ui.RenderComponent<CompanyEnvironmentSetup>();
            page.WaitForAssertion(() => Assert.Contains("A previous environment save is awaiting confirmation", page.Markup));
            typeof(CompanyEnvironmentSetup).GetField("_editCulture", flags)!.SetValue(page.Instance, "stale-input");
            handler.LoseResponse = false;
            await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(retry ? "Save" : "LoadSaved", flags)!.Invoke(page.Instance, null)!); await page.InvokeAsync(() => page.Render());
            Assert.Contains(retry ? "Settings saved" : "original save was confirmed", page.Markup);
            Assert.Null(intents.Value); Assert.Equal(retry ? 2 : 1, handler.Writes); Assert.Equal(1, notifications);
            Assert.Equal("KW", typeof(CompanyEnvironmentSetup).GetField("_country", flags)!.GetValue(page.Instance));
            Assert.Equal("{\"culture\":\"en-GB\"}", typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance));
            Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField("_editCulture", flags)!.GetValue(page.Instance));
        }
    }

    [Fact]
    public async Task ReloadingSavedEnvironmentClearsUnappliedInputs()
    {
        using var company = new CompanyClientContext();
        var scope = new CompanyScope("tenant", "company");
        Assert.True(company.TrySelect(company.Clear(), "actor", new(scope, ["Administrator"], [])));
        using var handler = new SaveHandler { Saved = CompanyEnvironmentIdentity.Prepare(scope, new("KW", "{\"culture\":\"ar-KW\"}", "", Guid.NewGuid().ToString("N")), "build", null, new string('A', 64), null) };
        using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        page.WaitForAssertion(() => Assert.True((bool)typeof(CompanyEnvironmentSetup).GetField("_environmentRecoveryReady", flags)!.GetValue(page.Instance)!));
        var inputs = new[] { "_editCulture", "_editTimeZone", "_editCoordinateSystems", "_editSourceName", "_editMappingSourceCode" };
        foreach (var name in inputs) typeof(CompanyEnvironmentSetup).GetField(name, flags)!.SetValue(page.Instance, "unapplied");
        await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod("LoadSaved", flags)!.Invoke(page.Instance, null)!);
        foreach (var name in inputs) Assert.Equal("", typeof(CompanyEnvironmentSetup).GetField(name, flags)!.GetValue(page.Instance));
        Assert.Equal("KW", typeof(CompanyEnvironmentSetup).GetField("_country", flags)!.GetValue(page.Instance));
        Assert.Equal("{\"culture\":\"ar-KW\"}", typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance));
        Assert.Equal(0, handler.Writes);
    }

    [Fact]
    public async Task EnvironmentIntentStorageFailurePreventsSave()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var handler = new SaveHandler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory()); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory { FailRetain = true }); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Call(string method, params object[] args) => await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadSaved"); await Call("LoadCountries"); await Call("CountryChanged", "KW"); await Call("Preview"); await Call("Save"); await page.InvokeAsync(() => page.Render());
        Assert.Equal(0, handler.Writes); Assert.Contains("no save was sent", page.Markup);
    }

    [Fact]
    public async Task ProtectedEnvironmentIntentIsScopedAndRejectsReplacement()
    {
        var tab = new TabStorage(); var protection = new EphemeralDataProtectionProvider(); CompanySetupIntentStore Store() => new(new ProtectedSessionStorage(tab, protection));
        var company = new CompanyScope("tenant", "company"); var original = new CompanyEnvironmentIntent(company, "actor", new(new("KW", "{}", "", Guid.NewGuid().ToString("N")), Review(company)));
        await Store().RetainEnvironmentAsync(original); Assert.DoesNotContain(original.Request.Change.IntentId, Assert.Single(tab.Values).Value);
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(await Store().ReadEnvironmentAsync(company, "actor")));
        Assert.Null(await Store().ReadEnvironmentAsync(new("tenant", "other"), "actor")); Assert.Null(await Store().ReadEnvironmentAsync(company, "other"));
        var other = original with { Request = original.Request with { Change = original.Request.Change with { IntentId = Guid.NewGuid().ToString("N") } } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => Store().RetainEnvironmentAsync(other)); await Assert.ThrowsAsync<InvalidOperationException>(() => Store().ClearEnvironmentAsync(other));
        await Store().ClearEnvironmentAsync(original); Assert.Empty(tab.Values);
        await Store().RetainEnvironmentAsync(original); tab.Values[Assert.Single(tab.Values).Key] = "corrupted";
        await Assert.ThrowsAnyAsync<Exception>(() => Store().ReadEnvironmentAsync(company, "actor"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VerificationIsShownOnlyForTheReviewedCompanyAndRevision(bool foreign)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new VerificationHandler(foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadSaved"); await Invoke("Verify"); await page.InvokeAsync(() => page.Render());
        if (foreign) { Assert.Contains("could not be verified", page.Markup); Assert.DoesNotContain("Unit reference", page.Markup); }
        else { Assert.Contains("Unit reference", page.Markup); Assert.Contains("source connections and coverage were not verified", page.Markup); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SourceReviewIsBoundToCompanyAndDoesNotClaimADataTest(bool foreign)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new VerificationHandler(foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadSaved"); await Invoke("ReviewSources"); await page.InvokeAsync(() => page.Render());
        if (foreign) { Assert.Contains("source bindings could not be reviewed", page.Markup); Assert.DoesNotContain("wells —", page.Markup); }
        else { Assert.Contains("PPDM_CORE", page.Markup); Assert.Contains("mapping coverage have not been tested", page.Markup); }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WellCheckRequiresTheOriginalCompanyAndIdentifiesLimitedCoverage(bool foreign)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        // The source review itself must be current before submitting a well check.
        using var http = new HttpClient(new VerificationHandler(false)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>();
        var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method, params object[] args) => await page.InvokeAsync(() =>
        { var result = typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, args); return result as Task ?? Task.CompletedTask; });
        await Invoke("LoadSaved"); await Invoke("ReviewSources"); await Invoke("TestSourceChanged", "wells"); await Invoke("TestWellChanged", "well-1");
        if (foreign) company.Clear();
        await Invoke("TestWell"); await page.InvokeAsync(() => page.Render());
        if (foreign) Assert.DoesNotContain("North well", page.Markup);
        else { Assert.Contains("North well", page.Markup); Assert.Contains("Related equipment, surveys, source coverage", page.Markup); }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task EffectiveSettingsAreBoundToTheOriginalCompany(bool foreign, bool invalidReview)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new VerificationHandler(foreign, invalidReview: invalidReview)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose;
        ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal()); ui.Services.AddSingleton(company); ui.Services.AddSingleton<ICompanyEnvironmentIntentStore>(new EnvironmentIntentMemory()); ui.Services.AddSingleton<IGhgSelectionIntentStore>(new IntentMemory());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
        async Task Invoke(string method) => await page.InvokeAsync(() => (Task)typeof(CompanyEnvironmentSetup).GetMethod(method, flags)!.Invoke(page.Instance, null)!);
        await Invoke("LoadSaved"); await Invoke("ReviewEffective"); await page.InvokeAsync(() => page.Render());
        if (foreign || invalidReview) Assert.Contains("Effective settings could not be confirmed", page.Markup);
        else Assert.Contains("Effective saved settings", page.Markup);
    }
}
#endif
