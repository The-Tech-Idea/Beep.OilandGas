#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Pages.PPDM39.BusinessProcess;
using Beep.OilandGas.Web.Components.Workflow;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ProcessAnalyticsUnavailableTests
{
    [Fact]
    public void ProcessAnalyticsStatesWhatIsUnavailableAndLinksToCompanyWork()
    {
        using var ui = new TestContext(); ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.ComponentFactories.AddStub<PersonaTaskInbox>();
        ui.ComponentFactories.AddStub<RecordedWorkOrderOverview>();
        var page = ui.RenderComponent<ProcessAnalyticsDashboard>();
        Assert.Contains("Process performance and safety totals are unavailable", page.Markup);
        Assert.Contains("/tasks/inbox", page.Markup); Assert.Contains("/ppdm39/data-quality", page.Markup);
        Assert.DoesNotContain("Completion Rate", page.Markup);
        Assert.NotNull(typeof(ProcessAnalyticsDashboard).GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault());
    }

    [Fact]
    public void ReservesReportDoesNotPresentProductionAsReserves()
    {
        using var ui = new TestContext(); ui.Services.AddLogging(); ui.Services.AddMudServices();
        var page = ui.RenderComponent<ReservesMaturationReport>();
        Assert.Contains("Reserves maturation is unavailable", page.Markup);
        Assert.DoesNotContain("Proved Reserves", page.Markup);
        Assert.NotNull(typeof(ReservesMaturationReport).GetCustomAttributes(typeof(AuthorizeAttribute), true).SingleOrDefault());
    }
}
#endif
