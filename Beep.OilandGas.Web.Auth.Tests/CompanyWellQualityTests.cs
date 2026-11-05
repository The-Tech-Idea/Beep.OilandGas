using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Production;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.ApiService.Controllers;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Web.Pages.PPDM39;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class CompanyWellQualityTests
{
    private static readonly CompanyScope Scope = new("tenant", "company");
    private static IReadOnlyDictionary<string, object?> Row(string uwi, string? name = "Name") =>
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase) { ["UWI"] = uwi, ["WELL_NAME"] = name, ["WELL_NUM"] = null };

    [Fact]
    public void CompanyWellReviewRouteRequiresAuthentication()
    {
        Assert.NotNull(typeof(CompanyWellQualityController).GetCustomAttribute<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>());
        Assert.Equal("wells/review", typeof(CompanyWellQualityController).GetMethod("ReviewWells")!
            .GetCustomAttribute<Microsoft.AspNetCore.Mvc.HttpPostAttribute>()!.Template);
    }

    [Fact]
    public void GenericDataManagementControllerDoesNotExposeUnscopedQualityRoutes()
    {
        var routes = typeof(DataManagementController).GetMethods()
            .SelectMany(method => method.GetCustomAttributes<Microsoft.AspNetCore.Mvc.Routing.HttpMethodAttribute>())
            .Select(attribute => attribute.Template).Where(template => template is not null);
        Assert.DoesNotContain(routes, template => template!.StartsWith("quality/", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ReviewerChecksOneWellTwiceAndReportsRecordedPresence()
    {
        var source = new Mock<ICompanyEnvironmentWellReader>(); var calls = 0; var guards = 0;
        source.Setup(x => x.ReadAsync(Scope, "well", It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => { calls++; return new[] { Row("well") }; });
        var result = await new CompanyWellQualityReviewer(source.Object).ReviewAsync(Scope, "well", () => { guards++; return Task.CompletedTask; }, default);
        Assert.Equal("Found", result.Status); Assert.Equal(2, calls); Assert.Equal(3, guards);
        Assert.Equal(new[] { true, true, false }, result.Fields.Select(x => x.Present));
    }

    [Theory]
    [InlineData("foreign")]
    [InlineData("changed")]
    [InlineData("missing-column")]
    public async Task ReviewerRefusesForeignChangingOrIncompleteSource(string failure)
    {
        var source = new Mock<ICompanyEnvironmentWellReader>(); var calls = 0;
        source.Setup(x => x.ReadAsync(Scope, "well", It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => { calls++; return failure switch {
                "foreign" => new[] { Row("another") },
                "changed" => new[] { Row("well", calls == 1 ? "first" : "second") },
                _ => new[] { (IReadOnlyDictionary<string, object?>)new Dictionary<string, object?> { ["UWI"] = "well" } }
            }; });
        await Assert.ThrowsAsync<RefusalException>(() => new CompanyWellQualityReviewer(source.Object).ReviewAsync(Scope, "well", () => Task.CompletedTask, default));
    }

    [Fact]
    public async Task ReviewerDistinguishesInvisibleWellAndStopsOnLostAuthority()
    {
        var source = new Mock<ICompanyEnvironmentWellReader>();
        source.Setup(x => x.ReadAsync(Scope, "well", It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<IReadOnlyDictionary<string, object?>>());
        var reviewer = new CompanyWellQualityReviewer(source.Object);
        var missing = await reviewer.ReviewAsync(Scope, "well", () => Task.CompletedTask, default);
        Assert.Equal("NotVisible", missing.Status); Assert.Empty(missing.Fields);
        var guards = 0;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => reviewer.ReviewAsync(Scope, "well", () =>
            ++guards == 2 ? throw new UnauthorizedAccessException() : Task.CompletedTask, default));
    }

    [Fact]
    public async Task BatchReviewsOnlyDistinctExplicitWellsAndRejectsInvalidRequestsBeforeReading()
    {
        var source = new Mock<ICompanyEnvironmentWellReader>(); var reads = 0;
        source.Setup(x => x.ReadAsync(Scope, It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyScope _, string well, IReadOnlyDictionary<string, string> filters, Func<Task> _, CancellationToken _) =>
            { reads++; Assert.Equal(well, filters["UWI"]); return new[] { Row(well) }; });
        var reviewer = new CompanyWellQualityReviewer(source.Object);
        var result = await reviewer.ReviewBatchAsync(Scope, new(["one", "two"]), () => Task.CompletedTask, default);
        Assert.Equal(new[] { "one", "two" }, result.Wells.Select(x => x.WellId)); Assert.Equal(4, reads);
        await Assert.ThrowsAsync<RefusalException>(() => reviewer.ReviewBatchAsync(Scope, new(["one", "one"]), () => Task.CompletedTask, default));
        await Assert.ThrowsAsync<RefusalException>(() => reviewer.ReviewBatchAsync(Scope, new(Enumerable.Range(0, 21).Select(x => x.ToString()).ToArray()), () => Task.CompletedTask, default));
        Assert.Equal(4, reads);
    }

    [Fact]
    public async Task BatchRejectsAnEarlierWellChangedWhileReadingAnotherWell()
    {
        var source = new Mock<ICompanyEnvironmentWellReader>(); var secondWellSeen = false;
        source.Setup(x => x.ReadAsync(Scope, It.IsAny<string>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CompanyScope _, string well, IReadOnlyDictionary<string, string> _, Func<Task> _, CancellationToken _) =>
            {
                if (well == "two") secondWellSeen = true;
                return new[] { Row(well, well == "one" && secondWellSeen ? "changed" : "original") };
            });
        await Assert.ThrowsAsync<RefusalException>(() => new CompanyWellQualityReviewer(source.Object)
            .ReviewBatchAsync(Scope, new(["one", "two"]), () => Task.CompletedTask, default));
    }

    private sealed class Handler : HttpMessageHandler
    {
        public bool Foreign; public bool InvalidUwi; public int Calls; public int SampleCalls; public TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release = new(TaskCreationOptions.RunContinuationsAsynchronously); public bool Delay;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get && request.RequestUri!.AbsolutePath == "/api/company-data-quality/wells/sample")
            {
                SampleCalls++;
                var sample = new CompanyWellQualitySample(Foreign ? new("tenant", "foreign") : Scope, ["well", "missing"]);
                return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(sample), Encoding.UTF8, "application/json") };
            }
            Assert.Equal("/api/company-data-quality/wells/review", request.RequestUri!.AbsolutePath);
            Assert.Equal(HttpMethod.Post, request.Method);
            var input = JsonSerializer.Deserialize<CompanyWellQualityBatchRequest>(await request.Content!.ReadAsStringAsync(cancellationToken), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
            Calls++;
            if (Delay) { Started.TrySetResult(); await Release.Task; }
            var result = new CompanyWellQualityBatchReview(Foreign ? new("tenant", "foreign") : Scope,
                input.WellIds.Select(id => id == "missing" ? new CompanyWellQualityReview(Scope, id, "NotVisible", []) :
                    new CompanyWellQualityReview(Scope, id, "Found", [new("UWI", !InvalidUwi), new("WELL_NAME", true), new("WELL_NUM", false)])).ToArray());
            return new(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }
    private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task VisibleWellSampleOnlyPrefillsTheMatchingCompany(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(Scope, [], [])));
        using var handler = new Handler { Foreign = foreign }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new CompanyWellQualityClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company)));
        var page = ui.RenderComponent<DataQuality>();
        await page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("FindWells", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render()); Assert.Equal(1, handler.SampleCalls); Assert.Equal(0, handler.Calls);
        if (foreign) { Assert.Contains("sample could not be confirmed", page.Markup); Assert.Equal("", typeof(DataQuality).GetField("_wellIds", Flags)!.GetValue(page.Instance)); }
        else { Assert.Contains("2 wells selected", page.Markup); Assert.Equal("well\nmissing", typeof(DataQuality).GetField("_wellIds", Flags)!.GetValue(page.Instance)); }
    }
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenAcceptsOnlyTheSelectedCompany(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(Scope, [], [])));
        using var handler = new Handler { Foreign = foreign }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new CompanyWellQualityClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company)));
        var page = ui.RenderComponent<DataQuality>();
        await page.InvokeAsync(() => typeof(DataQuality).GetMethod("ChangeWells", Flags)!.Invoke(page.Instance, ["well"]));
        await page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        if (foreign) { Assert.Contains("could not be confirmed", page.Markup); Assert.DoesNotContain("WELL_NUM: not recorded", page.Markup); }
        else { Assert.Contains("WELL_NUM: not recorded", page.Markup); Assert.Contains("not a table-wide quality score", page.Markup); }
    }

    [Fact]
    public async Task ScreenReviewsRequestedWellsAndRefusesDuplicatesBeforeTransport()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(Scope, [], [])));
        using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new CompanyWellQualityClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company)));
        var page = ui.RenderComponent<DataQuality>();
        await page.InvokeAsync(() => typeof(DataQuality).GetMethod("ChangeWells", Flags)!.Invoke(page.Instance, ["well\nwell"]));
        await page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!);
        Assert.Equal(0, handler.Calls);
        await page.InvokeAsync(() => typeof(DataQuality).GetMethod("ChangeWells", Flags)!.Invoke(page.Instance, ["well\nmissing"]));
        await page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render()); Assert.Equal(1, handler.Calls);
        Assert.Contains("WELL_NUM: not recorded", page.Markup); Assert.Contains("No well was visible", page.Markup);
        Assert.Contains("1 of 2 requested wells were visible", page.Markup); Assert.Contains("WELL_NUM: 0 of 1 visible wells recorded", page.Markup);
    }

    [Fact]
    public async Task ScreenDoesNotTurnInvisibleOrInconsistentWellsIntoZeroQuality()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(Scope, [], [])));
        using var handler = new Handler(); using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new CompanyWellQualityClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company)));
        var page = ui.RenderComponent<DataQuality>();
        await page.InvokeAsync(() => typeof(DataQuality).GetMethod("ChangeWells", Flags)!.Invoke(page.Instance, ["missing"]));
        await page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Field presence is unavailable", page.Markup); Assert.DoesNotContain("0 of 0 visible wells recorded", page.Markup);
        handler.InvalidUwi = true;
        await page.InvokeAsync(() => typeof(DataQuality).GetMethod("ChangeWells", Flags)!.Invoke(page.Instance, ["well"]));
        await page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("could not be confirmed", page.Markup); Assert.DoesNotContain("Presence in the requested wells", page.Markup);
    }

    [Fact]
    public async Task ChangingCompanyDiscardsAnOutstandingWellReview()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(Scope, [], [])));
        using var handler = new Handler { Delay = true }; using var http = new HttpClient(handler) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new CompanyWellQualityClient(new ApiClient(http, Mock.Of<IFailureReporter>(), company)));
        var page = ui.RenderComponent<DataQuality>();
        await page.InvokeAsync(() => typeof(DataQuality).GetMethod("ChangeWells", Flags)!.Invoke(page.Instance, ["well"]));
        var pending = page.InvokeAsync(() => (Task)typeof(DataQuality).GetMethod("Review", Flags)!.Invoke(page.Instance, null)!);
        await handler.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "other"), [], [])));
        handler.Release.SetResult(); await pending; await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain("WELL_NUM: not recorded", page.Markup);
    }
}
#endif
