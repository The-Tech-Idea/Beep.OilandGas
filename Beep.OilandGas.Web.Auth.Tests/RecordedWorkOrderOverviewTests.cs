using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class RecordedWorkOrderOverviewTests
{
    private static FieldResponse Field() => new() { SelectionAvailable = true, FieldId = "field", SelectionContext = new(new("tenant", "company"), new("core", "binding"), "actor", "revision", "field") };
    private static WorkOrderSummary Row(string field) => new() { InstanceId = "order-1", InstanceName = "WO 1", FieldId = field, Source = "WORK_ORDER", WoSubType = "Inspection" };
    private static readonly BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Instance;

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task OverviewShowsOnlyOrdersForTheReviewedField(bool foreign)
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Field());
        var orders = new Mock<IWorkOrderServiceClient>(); orders.Setup(x => x.GetWorkOrderSampleAsync(It.IsAny<CancellationToken>())).ReturnsAsync([Row(foreign ? "other" : "field")]);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(orders.Object);
        var page = ui.RenderComponent<RecordedWorkOrderOverview>();
        await page.InvokeAsync(() => (Task)typeof(RecordedWorkOrderOverview).GetMethod("Load", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        if (foreign) { Assert.Contains("could not be confirmed", page.Markup); Assert.DoesNotContain("WO 1", page.Markup); }
        else { Assert.Contains("1 visible recorded work orders returned in the sample", page.Markup); Assert.Contains("/ppdm39/workorder/detail?workOrderId=order-1", page.Markup); Assert.Contains("not a complete FIELD or company-wide population", page.Markup); }
    }

    [Fact]
    public async Task OverviewDiscardsLateOrdersAfterCompanyChanges()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Field());
        var pending = new TaskCompletionSource<List<WorkOrderSummary>>(TaskCreationOptions.RunContinuationsAsynchronously);
        var orders = new Mock<IWorkOrderServiceClient>(); orders.Setup(x => x.GetWorkOrderSampleAsync(It.IsAny<CancellationToken>())).Returns(pending.Task);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(orders.Object);
        var page = ui.RenderComponent<RecordedWorkOrderOverview>();
        var load = page.InvokeAsync(() => (Task)typeof(RecordedWorkOrderOverview).GetMethod("Load", Flags)!.Invoke(page.Instance, null)!);
        page.WaitForAssertion(() => orders.Verify(x => x.GetWorkOrderSampleAsync(It.IsAny<CancellationToken>()), Times.Once));
        await page.InvokeAsync(company.Clear); pending.SetResult([Row("field")]); await load; await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain("WO 1", page.Markup); Assert.DoesNotContain("visible recorded work orders returned", page.Markup);
    }

    [Fact]
    public async Task OverviewRefusesAChangedFieldSelectionWithoutPublishingOrders()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var changed = Field(); changed.SelectionContext = changed.SelectionContext! with { Revision = "new-revision" };
        var fields = new Mock<IFieldSelectionContext>(); fields.SetupSequence(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Field()).ReturnsAsync(changed);
        var orders = new Mock<IWorkOrderServiceClient>(); orders.Setup(x => x.GetWorkOrderSampleAsync(It.IsAny<CancellationToken>())).ReturnsAsync([Row("field")]);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(orders.Object);
        var page = ui.RenderComponent<RecordedWorkOrderOverview>();
        await page.InvokeAsync(() => (Task)typeof(RecordedWorkOrderOverview).GetMethod("Load", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("could not be confirmed", page.Markup); Assert.DoesNotContain("WO 1", page.Markup);
    }

    [Fact]
    public async Task TypeBreakdownCountsOnlyRecordedSampleAndSeparatesMissingTypes()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Field());
        var first = Row("field"); var second = Row("field"); second.InstanceId = "order-2";
        var third = Row("field"); third.InstanceId = "order-3"; third.WoSubType = "";
        var orders = new Mock<IWorkOrderServiceClient>(); orders.Setup(x => x.GetWorkOrderSampleAsync(It.IsAny<CancellationToken>())).ReturnsAsync([first, second, third]);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(orders.Object);
        var page = ui.RenderComponent<RecordedWorkOrderOverview>();
        await page.InvokeAsync(() => (Task)typeof(RecordedWorkOrderOverview).GetMethod("Load", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("Recorded types in this sample", page.Markup);
        Assert.Contains("Inspection: 2", page.Markup); Assert.Contains("Type not recorded: 1", page.Markup);
        Assert.Contains("not a complete FIELD or company-wide population", page.Markup);
    }

    [Fact]
    public async Task EmptySampleDoesNotInventTypeCounts()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        var fields = new Mock<IFieldSelectionContext>(); fields.Setup(x => x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Field());
        var orders = new Mock<IWorkOrderServiceClient>(); orders.Setup(x => x.GetWorkOrderSampleAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(fields.Object); ui.Services.AddSingleton(orders.Object);
        var page = ui.RenderComponent<RecordedWorkOrderOverview>();
        await page.InvokeAsync(() => (Task)typeof(RecordedWorkOrderOverview).GetMethod("Load", Flags)!.Invoke(page.Instance, null)!);
        await page.InvokeAsync(() => page.Render());
        Assert.Contains("0 visible recorded work orders returned in the sample", page.Markup);
        Assert.DoesNotContain("Recorded types in this sample", page.Markup);
    }

    [Fact]
    public async Task ExactOrderLinkUsesEncodedIdentifierAndClearsOnCompanyChange()
    {
        using var company = new CompanyClientContext(); Assert.True(company.TrySelect(company.Clear(), "actor", new(new("tenant", "company"), [], [])));
        using var ui = new TestContext(); ui.JSInterop.Mode = JSRuntimeMode.Loose; ui.Services.AddLogging(); ui.Services.AddMudServices();
        ui.Services.AddSingleton(company); ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>()); ui.Services.AddSingleton(Mock.Of<IWorkOrderServiceClient>());
        var page = ui.RenderComponent<RecordedWorkOrderOverview>();
        await page.InvokeAsync(() => { typeof(RecordedWorkOrderOverview).GetField("_exactId", Flags)!.SetValue(page.Instance, "order/حقل?1"); page.Render(); });
        Assert.Contains("/ppdm39/workorder/detail?workOrderId=order%2F%D8%AD%D9%82%D9%84%3F1", page.Markup);
        await page.InvokeAsync(company.Clear); await page.InvokeAsync(() => page.Render());
        Assert.DoesNotContain("order%2F", page.Markup);
        Assert.Equal("", typeof(RecordedWorkOrderOverview).GetField("_exactId", Flags)!.GetValue(page.Instance));
    }
}
#endif
