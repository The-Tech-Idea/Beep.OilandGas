#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.ApiService.Controllers.Compliance;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.PPDM39.DataManagement.Services.Compliance;
using Beep.OilandGas.Web.Pages.PPDM39.Compliance;
using Bunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdea.Data.OilGas;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Models.Data;
using System.Text.Json;
using System.Text;
using System.Net;
using System.Reflection;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class GHGReportingAvailabilityTests
{
    private sealed class SourcesHandler(FieldGhgProductionSources result) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token) => Task.FromResult(
            new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") });
    }
    private sealed class DispositionHandler(FieldGhgProductionSources parents, bool foreign) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.Method == HttpMethod.Post)
            {
                var query = JsonSerializer.Deserialize<GhgDispositionQuery>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                result = new FieldGhgDispositions(parents.Field, foreign ? query with { ProductionEntityId = "other" } : query,
                    [new("measured", "recorded", "gas", "month", 1, 0, 12.5m, "unit", null, null)], new string('B', 64));
            }
            else result = parents;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }
    private sealed class CheckHandler(FieldGhgProductionSources parents, bool foreign) : HttpMessageHandler
    {
        private readonly GhgDispositionFact row = new("measured", "flare", "gas", "month", 1, 0, 12m, "volume", new(2026, 2, 1), new(2026, 2, 28));
        private readonly GhgFactorDefinition factor = new("factor", "flare", "volume", "mass", 1.25m, new(2026, 1, 1), new(2026, 12, 31), "source-reference");
        private readonly CompanyGhgSelection selected = CompanyGhgSelectionIdentity.Prepare(new("tenant", "company"), "actor",
            new("KW", new string('A', 64), new string('B', 64), "reviewed", "", Guid.NewGuid().ToString("N")), null);
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            object result;
            if (request.RequestUri!.AbsolutePath.EndsWith("calculation-basis", StringComparison.Ordinal))
                result = new GhgCalculationBasis(parents.Field, selected, new GhgFactorCatalog("KW", [factor]));
            else if (request.RequestUri.AbsolutePath.EndsWith("calculation-check", StringComparison.Ordinal))
            {
                var command = JsonSerializer.Deserialize<GhgArithmeticRequest>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                var check = GhgArithmetic.Check(row, factor);
                result = new FieldGhgArithmeticCheck(parents.Field, command, foreign ? new string('C', 64) : selected.CatalogRevision, check);
            }
            else if (request.Method == HttpMethod.Post)
            {
                var query = JsonSerializer.Deserialize<GhgDispositionQuery>(await request.Content!.ReadAsStringAsync(token), new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
                result = new FieldGhgDispositions(parents.Field, query, [row], new string('D', 64));
            }
            else result = parents;
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(result), Encoding.UTF8, "application/json") };
        }
    }
    [Fact]
    public async Task ApiDoesNotReturnZeroOrInventAJurisdiction()
    {
        var field = new Mock<IFieldOrchestrator>(); field.SetupGet(x => x.CurrentFieldId).Returns("field");
        var controller = new ComplianceController(Mock.Of<IComplianceService>(), Mock.Of<IRoyaltyCalculationService>(), new GHGReportingService(), field.Object)
        { ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity([new Claim("party_id", "actor")], "test")) } } };
        Assert.IsType<BadRequestObjectResult>((await controller.GenerateGHGReportAsync(2026, "")).Result);
        Assert.Equal(503, Assert.IsType<ObjectResult>((await controller.GenerateGHGReportAsync(2026, "Kuwait")).Result).StatusCode);
        Assert.Equal(503, Assert.IsType<ObjectResult>((await controller.GetTotalEmissionsAsync(2026, "Kuwait")).Result).StatusCode);
    }

    [Fact]
    public void ScreenExplainsWhyReportingIsUnavailable()
    {
        using var ui = new TestContext(); ui.Services.AddLogging(); ui.Services.AddMudServices();
        using var company = new CompanyClientContext(); using var http = new HttpClient { BaseAddress = new("https://api.test") };
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>());
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<GHGReport>();
        Assert.Contains("GHG reporting is unavailable", page.Markup);
        Assert.DoesNotContain("USA", page.Markup);
    }

    [Fact]
    public void ProductionSourceProjectionRejectsForeignOrDuplicateRecordedIdentities()
    {
        var field = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "A");
        var row = new PDEN { PDEN_SUBTYPE = "WELL", PDEN_ID = "pden-1", SOURCE = "source", FIELD_ID = "A", ACTIVE_IND = "Y", PDEN_LONG_NAME = "Recorded name" };
        var result = GhgProductionSourceMapper.Project(field, [row], new string('A', 64));
        Assert.Equal("pden-1", Assert.Single(result.Entities).ProductionEntityId);
        Assert.Throws<InvalidOperationException>(() => GhgProductionSourceMapper.Project(field, [row, row], new string('A', 64)));
        row.FIELD_ID = "B";
        Assert.Throws<InvalidOperationException>(() => GhgProductionSourceMapper.Project(field, [row], new string('A', 64)));
    }

    [Fact]
    public void DispositionProjectionPreservesUnitsAndRejectsForeignOrDuplicateRows()
    {
        var field = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "A");
        var query = new GhgDispositionQuery("WELL", "pden-1", "source", new string('A', 64));
        var row = new PDEN_VOL_DISPOSITION { PDEN_SUBTYPE = "WELL", PDEN_ID = "pden-1", PDEN_SOURCE = "source", ACTIVE_IND = "Y",
            VOLUME_METHOD = "measured", ACTIVITY_TYPE = "recorded", PRODUCT_TYPE = "gas", PERIOD_TYPE = "month", DISPOSITION_OBS_NO = 1, AMENDMENT_SEQ_NO = 0, VOLUME = 12.5m, VOLUME_UOM = "recorded-unit" };
        var result = GhgProductionSourceMapper.ProjectDispositions(field, query, [row], new string('B', 64));
        var fact = Assert.Single(result.Dispositions); Assert.Equal(12.5m, fact.Volume); Assert.Equal("recorded-unit", fact.Unit);
        Assert.Throws<InvalidOperationException>(() => GhgProductionSourceMapper.ProjectDispositions(field, query, [row, row], new string('B', 64)));
        row.PDEN_SOURCE = "foreign";
        Assert.Throws<InvalidOperationException>(() => GhgProductionSourceMapper.ProjectDispositions(field, query, [row], new string('B', 64)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenShowsOnlyCurrentCompanyFieldSources(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Compliance"], [])));
        var field = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "A");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "A", SelectionAvailable = true, SelectionContext = field });
        var result = new FieldGhgProductionSources(foreign ? field with { FieldId = "B" } : field, [new("WELL", "pden-1", "source", "Recorded name", null)], new string('A', 64));
        using var http = new HttpClient(new SourcesHandler(result)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<GHGReport>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => (Task)typeof(GHGReport).GetMethod("LoadSources", flags)!.Invoke(page.Instance, null)!); await page.InvokeAsync(() => page.Render());
        if (foreign) Assert.Contains("could not be confirmed", page.Markup);
        else { Assert.Contains("pden-1", page.Markup); Assert.Contains("do not establish emissions", page.Markup); }
        company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("pden-1", page.Markup));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenChecksDispositionSourceBeforeDisplayingRecordedVolume(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Compliance"], [])));
        var field = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "A");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "A", SelectionAvailable = true, SelectionContext = field });
        var item = new GhgProductionEntity("WELL", "pden-1", "source", "Recorded name", null);
        var parents = new FieldGhgProductionSources(field, [item], new string('A', 64));
        using var http = new HttpClient(new DispositionHandler(parents, foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<GHGReport>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        await page.InvokeAsync(() => (Task)typeof(GHGReport).GetMethod("LoadSources", flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => (Task)typeof(GHGReport).GetMethod("LoadDispositions", flags)!.Invoke(page.Instance, [item])!); await page.InvokeAsync(() => page.Render());
        if (foreign) Assert.Contains("Recorded dispositions could not be confirmed", page.Markup);
        else { Assert.Contains("12.5", page.Markup); Assert.Contains("not classified emissions", page.Markup); }
        company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("12.5", page.Markup));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ScreenShowsOnlyTheConfirmedNonReportingArithmeticCheck(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Compliance"], [])));
        var field = new FieldSelectionReview(new("tenant", "company"), new("core", "binding"), "actor", "revision", "A");
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse { FieldId = "A", SelectionAvailable = true, SelectionContext = field });
        var item = new GhgProductionEntity("WELL", "pden-1", "source", null, null); var parents = new FieldGhgProductionSources(field, [item], new string('D', 64));
        using var http = new HttpClient(new CheckHandler(parents, foreign)) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices(); ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object);
        ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<GHGReport>(); var flags = BindingFlags.NonPublic | BindingFlags.Instance; var type = typeof(GHGReport);
        async Task Call(string name, params object[] args) => await page.InvokeAsync(() => type.GetMethod(name, flags)!.Invoke(page.Instance, args) as Task ?? Task.CompletedTask);
        await Call("LoadSources"); await Call("LoadDispositions", item); await Call("LoadBasis");
        var rows = (FieldGhgDispositions)type.GetField("_dispositions", flags)!.GetValue(page.Instance)!; var basis = (GhgCalculationBasis)type.GetField("_basis", flags)!.GetValue(page.Instance)!;
        await Call("CheckArithmetic", rows.Dispositions[0], basis.Catalog.Factors[0]); await page.InvokeAsync(() => page.Render());
        if (foreign) Assert.Contains("could not be confirmed", page.Markup);
        else { Assert.Contains("Arithmetic check only", page.Markup); Assert.Contains("not an approved emissions report", page.Markup); }
        company.Clear(); page.WaitForAssertion(() => Assert.DoesNotContain("Arithmetic check only", page.Markup));
    }
}
#endif
