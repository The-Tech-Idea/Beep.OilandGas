using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Text.Json.Nodes;
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

public sealed class CompanyEffectiveMappingSelectionTests
{
    [Fact]
    public async Task InheritedMappingCanBeSelectedEditedAndStaleEvidenceCannotBeReused()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        using var http = new HttpClient(new Handler()) { BaseAddress = new("https://api.test") };
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        var environmentIntents = new Mock<ICompanyEnvironmentIntentStore>(); environmentIntents.Setup(x => x.ReadEnvironmentAsync(It.IsAny<CompanyScope>(), It.IsAny<string>())).ReturnsAsync((CompanyEnvironmentIntent?)null);
        var ghgIntents = new Mock<IGhgSelectionIntentStore>(); ghgIntents.Setup(x => x.ReadAsync(It.IsAny<CompanyScope>(), It.IsAny<string>())).ReturnsAsync((GhgSelectionIntent?)null);
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(new CompanyEnvironmentChangeSignal());
        ui.Services.AddSingleton(environmentIntents.Object); ui.Services.AddSingleton(ghgIntents.Object); ui.Services.AddSingleton(new ApiClient(http, Mock.Of<IFailureReporter>(), company));
        var page = ui.RenderComponent<CompanyEnvironmentSetup>(); var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var mapping = new CompanyEnvironmentCodeMapping("string", "kind", "X", "Tubing", "System");
        var source = new CompanyEnvironmentSource("well", "PPDM_CORE", "ppdm39", 1, 0, "Bound") { MappingStatus = "ProfileChecked", CodeMappings = [mapping] };
        var review = new CompanyEnvironmentSourceReview(company.Selection!.Scope, new string('A', 64), [source], false, new string('B', 64));
        await page.InvokeAsync(() =>
        {
            typeof(CompanyEnvironmentSetup).GetField("_settingsLoaded", flags)!.SetValue(page.Instance, true);
            typeof(CompanyEnvironmentSetup).GetField("_revision", flags)!.SetValue(page.Instance, review.Revision);
            typeof(CompanyEnvironmentSetup).GetField("_sources", flags)!.SetValue(page.Instance, review);
            page.Render();
        });
        Assert.Contains("Edit in company draft", page.Markup);
        await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod("SelectEffectiveMapping", flags)!.Invoke(page.Instance, [source, mapping]));
        Assert.Equal("well", typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetField("_editMappingCanonicalCode", flags)!.SetValue(page.Instance, "Casing"));
        await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod("SetSourceMapping", flags)!.Invoke(page.Instance, null));
        var json = (string)typeof(CompanyEnvironmentSetup).GetField("_overrides", flags)!.GetValue(page.Instance)!;
        var overlay = JsonNode.Parse(json)!["dataSources"]!["well"]!;
        Assert.Equal("Casing", overlay["codes"]!["string"]!["kind"]!["X"]!.GetValue<string>());
        Assert.Null(overlay["path"]); Assert.Null(typeof(CompanyEnvironmentSetup).GetField("_sources", flags)!.GetValue(page.Instance));
        await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.SetValue(page.Instance, "unchanged"));
        await page.InvokeAsync(() => typeof(CompanyEnvironmentSetup).GetMethod("SelectEffectiveMapping", flags)!.Invoke(page.Instance, [source, mapping]));
        Assert.Equal("unchanged", typeof(CompanyEnvironmentSetup).GetField("_editSourceName", flags)!.GetValue(page.Instance));
    }

    private sealed class Handler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable));
    }
}
#endif
