using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Compliance;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39.HSE;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ComplianceCalendarSourceTests
{
    [Fact]
    public void ComplianceDashboardUsesTheCurrentFieldRecordedOwner()
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        var context = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "revision", "field");
        var field = new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", FieldName = "Current FIELD", SelectionAvailable = true, SelectionContext = context };
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(field);
        var client = new Mock<IComplianceServiceClient>();
        client.Setup(x => x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldObligationSourceReview(context,
            [new("recorded-case", 1, "REPORT", "Recorded evidence", null, null, null, [new(1, null, null, null, null, null)])]));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object);
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Compliance.ComplianceDashboard>();
        page.WaitForAssertion(() => Assert.Contains("recorded-case", page.Markup));
        Assert.DoesNotContain("On-Time Rate", page.Markup);
        client.Verify(x => x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>()), Times.Once);
        client.Verify(x => x.GetUpcomingObligationsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void ProcessDashboardShowsCompanyWorkWithoutLegacyComplianceTotals()
    {
        using var company = new CompanyClientContext();
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>()); ui.Services.AddSingleton(Mock.Of<IWorkOrderServiceClient>());
        var page = ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.BusinessProcess.ProcessDashboard>();
        Assert.Contains("Open assigned work", page.Markup); Assert.Contains("Review FIELD obligations", page.Markup);
        Assert.Contains("Load visible work-order sample", page.Markup); Assert.DoesNotContain("Compliance Due (30d)", page.Markup);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CalendarUsesRecordedFieldOwnerAndDoesNotOfferUnsupportedCreation(bool creationHandoff)
    {
        using var company = new CompanyClientContext();
        Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), ["Administrator"], [])));
        var context = new FieldSelectionReview(company.Selection!.Scope, new("core", "binding"), "actor", "revision", "field");
        var field = new Beep.OilandGas.Models.Data.FieldResponse { FieldId = "field", FieldName = "Selected field", SelectionAvailable = true, SelectionContext = context };
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(field);
        var client = new Mock<IComplianceServiceClient>();
        client.Setup(x => x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldObligationSourceReview(context,
            [new("recorded-case", 1, "REPORT", "Actual PPDM record", new DateTime(2027, 1, 2), null, null, [new(1, "FIELD", null, null, null, null)])]));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object);
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo($"/hse/compliance?openCreate={creationHandoff.ToString().ToLowerInvariant()}&returnTo=%2Foperations");
        var page = ui.RenderComponent<ComplianceCalendar>();
        page.WaitForAssertion(() => Assert.Contains("recorded-case", page.Markup));
        Assert.Contains("Actual PPDM record", page.Markup);
        Assert.Contains("not a determination of regulatory deadlines", page.Markup);
        if (creationHandoff) { Assert.Contains("has not created an obligation", page.Markup); Assert.Contains("/ppdm39/compliance/obligations/new", page.Markup); }
        Assert.Contains("/operations", page.Markup);
        client.Verify(x => x.GetRecordedFieldObligationsAsync(It.IsAny<CancellationToken>()), Times.Once);
        client.Verify(x => x.GetAllObligationsAsync(It.IsAny<int?>(), It.IsAny<CancellationToken>()), Times.Never);
        client.Verify(x => x.CreateObligationAsync(It.IsAny<Beep.OilandGas.Models.Data.Compliance.CreateObligationRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void CalendarDoesNotRenderAnExternalReturnLink()
    {
        using var company = new CompanyClientContext();
        var fields = new Mock<IFieldSelectionContext>(); var client = new Mock<IComplianceServiceClient>();
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(client.Object);
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/hse/compliance?returnTo=%2F%2Foutside.example");
        var page = ui.RenderComponent<ComplianceCalendar>();
        Assert.DoesNotContain("outside.example", page.Markup);
    }
}
#endif
