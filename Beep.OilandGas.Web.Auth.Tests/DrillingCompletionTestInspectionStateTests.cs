#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingCompletionTestInspectionStateTests
{
    private static async Task<DrillingCompletionTestInspection> Result(DrillingCompletionTestSelection selection)
    {var source=new DrillingCompletionTestInspectionTests.Fixture();await source.Prepare(selection.Review);source.Selection=selection;source.Row.SOURCE=selection.Source;source.Row.TEST_TYPE=selection.TestType;source.Row.RUN_NUM=selection.RunNumber;source.Row.TEST_NUM=selection.TestNumber;return await source.Inspect();}
    [Theory,InlineData("current"),InlineData("invalid"),InlineData("unavailable"),InlineData("mismatch")]
    public async Task ExplicitReadOnlySelectionValidatesOriginalEvidenceAndDoesNotWriteDecision(string mode)
    {
        using var f=new DrillingCompletionDecisionStateTests.Fixture();await f.Prepare();
        f.Client.Setup(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionTestSelection selection,CancellationToken _)=>
        {if(mode=="unavailable")throw new IOException("Native source unavailable");var result=await Result(selection);return mode=="mismatch"?result with{Selection=selection with{TestNumber="other-test"}}:result;});
        await f.State.InspectTestAsync(mode=="invalid"?-1:0,"LAB-CERTIFICATES","PRESSURE_TEST","run-1","test-1");
        if(mode=="current")Assert.NotNull(f.State.TestInspection);else{Assert.Null(f.State.TestInspection);Assert.NotNull(f.State.Error);}
        Assert.False(f.State.Busy);f.Client.Verify(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never());Assert.Null(f.Journal.CompletionDecisionOriginal);
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LatePrivateNativeTestCannotEscapeItsOriginalCompanyPersonaRouteOrOwner(string change)
    {
        using var f=new DrillingCompletionDecisionStateTests.Fixture();await f.Prepare();var pending=new TaskCompletionSource<DrillingCompletionTestInspection>(TaskCreationOptions.RunContinuationsAsynchronously);DrillingCompletionTestSelection? original=null;
        f.Client.Setup(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>())).Returns((DrillingCompletionTestSelection selection,CancellationToken _)=>{original=selection;return pending.Task;});
        var request=f.State.InspectTestAsync(0,"LAB-CERTIFICATES","PRESSURE_TEST","run-1","test-1");Assert.True(f.State.Busy);
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);if(change=="route")await f.State.LoadAsync("another-review");if(change=="dispose")f.State.Dispose();
        pending.SetResult(await Result(original!));await request;Assert.Null(f.State.TestInspection);Assert.False(f.State.Busy);
    }
    [Fact]public async Task ActualReviewerScreenShowsExplicitLookupAuditAndMismatchWithoutAcceptance()
    {
        using var f=new DrillingCompletionDecisionStateTests.Fixture();await f.Prepare();f.Client.Setup(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>())).Returns(async(DrillingCompletionTestSelection selection,CancellationToken _)=>{var found=await Result(selection);found=found with{Native=found.Native with{ReportReference="different-native-report"}};return found with{Revision=found.ContentRevision()};});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo(WorkflowTaskRoute.For("FIELD",f.Native.Command.Review.InstanceId,"DRILLING_DAILY_REPORT"));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingCompletionAssigned>();page.WaitForAssertion(()=>Assert.Contains("Inspect a recorded native completion test",page.Markup));
        await page.FindAll("button").Single(button=>button.TextContent=="Inspect exact native test").ClickAsync(new MouseEventArgs());f.Client.Verify(c=>c.InspectCompletionTestAsync(It.IsAny<DrillingCompletionTestSelection>(),It.IsAny<CancellationToken>()),Times.Never());
        var panel=page.FindComponent<Beep.OilandGas.Web.Components.Workflow.DrillingCompletionTestPanel>();var select=panel.FindComponent<MudSelect<int>>();await page.InvokeAsync(()=>select.Instance.ValueChanged.InvokeAsync(0));
        var values=new[]{"LAB-CERTIFICATES","PRESSURE_TEST","run-1","test-1"};
        for(var index=0;index<values.Length;index++){var input=panel.FindComponents<MudTextField<string>>()[index];var value=values[index];await page.InvokeAsync(()=>input.Instance.ValueChanged.InvokeAsync(value));}
        await page.FindAll("button").Single(button=>button.TextContent=="Inspect exact native test").ClickAsync(new MouseEventArgs());
        page.WaitForAssertion(()=>Assert.Contains("Checked native test comparison",page.Markup));Assert.Contains("different-native-report",page.Markup);Assert.Contains("lab-recorder",page.Markup);Assert.Contains("Resolve the discrepancy before acceptance",page.Markup);Assert.Contains("not accepted physical tests",page.Markup);
        f.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("different-native-report",page.Markup));f.Client.Verify(c=>c.DecideCompletionReviewAsync(It.IsAny<DrillingCompletionReviewDecision>(),It.IsAny<CancellationToken>()),Times.Never());
    }
}
#endif
