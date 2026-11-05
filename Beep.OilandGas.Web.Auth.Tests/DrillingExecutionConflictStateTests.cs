#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DrillingExecutionConflictStateTests
{
    private static DrillingExecutionConflictReview Review(DrillingExecutionStart original)
    {
        var competing=(original with{IntentId=Guid.NewGuid(),Rationale="Different recorder original source confirmed"}).ToRecord(DateTime.UtcNow);
        var result=new DrillingExecutionConflictReview(original.Revision(),original.OperationId(),true,competing,"",DateTimeOffset.UtcNow);
        result=result with{Revision=result.ContentRevision()};result.Require(original);return result;
    }
    private static async Task<DrillingExecutionConflictReview> Pending(DrillingExecutionStateTests.Fixture f)
    {
        await f.Prepare();f.Journal.ExecutionOriginal=f.Native.Command;
        var proof=Review(f.Native.Command);
        f.Client.Setup(c=>c.GetExecutionConflictAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        f.Client.Setup(c=>c.ConfirmExecutionConflictAsync(It.IsAny<DrillingExecutionConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        await f.State.LoadAsync(f.Native.Current.ReviewInstanceId);return proof;
    }
    [Fact]public async Task InspectionPreservesOriginalAndOnlyExplicitRecheckedAcknowledgementClearsIt()
    {
        using var f=new DrillingExecutionStateTests.Fixture();var proof=await Pending(f);var revision=f.State.Pending!.Revision();
        await f.State.InspectConflictAsync();Assert.Equal(proof.Revision,f.State.Conflict!.Revision);Assert.Equal(revision,f.Journal.ExecutionOriginal!.Revision());
        Assert.False(f.State.ConflictAcknowledged);Assert.Null(f.State.Receipt);
        await f.State.AcknowledgeConflictAsync();Assert.True(f.State.ConflictAcknowledged);Assert.Null(f.State.Pending);Assert.Null(f.Journal.ExecutionOriginal);
        Assert.Contains("No execution, WELL, report or financial record was changed",f.State.Confirmation);
        f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
        f.Client.Verify(c=>c.ConfirmExecutionConflictAsync(It.IsAny<DrillingExecutionConflictAcknowledgement>(),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Theory,InlineData("lookup"),InlineData("changed-proof"),InlineData("clear")]
    public async Task UnavailableOrChangedProofAndClearFailureNeverSilentlyDiscardTheOriginal(string fault)
    {
        using var f=new DrillingExecutionStateTests.Fixture();var proof=await Pending(f);await f.State.InspectConflictAsync();
        if(fault=="lookup")f.Client.Setup(c=>c.ConfirmExecutionConflictAsync(It.IsAny<DrillingExecutionConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        if(fault=="changed-proof")f.Client.Setup(c=>c.ConfirmExecutionConflictAsync(It.IsAny<DrillingExecutionConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ReturnsAsync(Review(f.Native.Command));
        if(fault=="clear")f.Journal.FailClear=true;
        await f.State.AcknowledgeConflictAsync();Assert.NotNull(f.State.Pending);Assert.NotNull(f.Journal.ExecutionOriginal);Assert.NotNull(f.State.Error);
        Assert.Equal(fault=="clear",f.State.ConflictAcknowledged);
        if(fault=="clear"){Assert.Equal(proof.Revision,f.State.Conflict!.Revision);Assert.Contains("confirmed this original was absent",f.State.PendingExplanation);}
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateConflictConfirmationCannotPublishOrClearAfterContextChanges(string change)
    {
        using var f=new DrillingExecutionStateTests.Fixture();var proof=await Pending(f);await f.State.InspectConflictAsync();
        var response=new TaskCompletionSource<DrillingExecutionConflictReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.ConfirmExecutionConflictAsync(It.IsAny<DrillingExecutionConflictAcknowledgement>(),It.IsAny<CancellationToken>())).Returns(response.Task);
        var work=f.State.AcknowledgeConflictAsync();
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);
        if(change=="route")await f.State.LoadAsync("another");if(change=="dispose")f.State.Dispose();response.SetResult(proof);await work;
        Assert.Null(f.State.Conflict);Assert.False(f.State.ConflictAcknowledged);Assert.NotNull(f.Journal.ExecutionOriginal);
    }
    [Theory,InlineData(false),InlineData(true)]public async Task ActualPageRequiresReviewAndExplicitLocalClearAndRetainsKnownProofOnCleanupFailure(bool earlierAbsentReceipt)
    {
        using var f=new DrillingExecutionStateTests.Fixture();await Pending(f);f.Journal.FailClear=true;
        if(earlierAbsentReceipt){var original=f.Native.Command;f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DrillingExecutionStartReceipt(original.Revision(),original.OperationId(),null,DateTimeOffset.UtcNow));}
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);
        ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution?reviewId="+Uri.EscapeDataString(f.Native.Current.ReviewInstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecution>();
        page.WaitForAssertion(()=>Assert.Contains("Inspect a different recorded execution",page.Markup));
        if(earlierAbsentReceipt)
        {
            await page.FindAll("button").Single(b=>b.TextContent=="Recover original execution start").ClickAsync(new MouseEventArgs());
            Assert.Contains("The original execution start was confirmed absent.",page.Markup);Assert.NotNull(f.Journal.ExecutionOriginal);
        }
        await page.FindAll("button").Single(b=>b.TextContent=="Inspect a different recorded execution").ClickAsync(new MouseEventArgs());
        Assert.Contains("Different recorded execution requires your review",page.Markup);Assert.NotNull(f.Journal.ExecutionOriginal);
        Assert.Contains("Different recorder original source confirmed",page.Markup);
        await page.FindAll("button").Single(b=>b.TextContent=="Keep recorded execution and clear my unrecorded original").ClickAsync(new MouseEventArgs());
        Assert.Contains("Clearing the tab input is not confirmed",page.Markup);Assert.Contains("Different recorded execution requires your review",page.Markup);Assert.NotNull(f.Journal.ExecutionOriginal);
        f.Journal.FailClear=false;
        await page.FindAll("button").Single(b=>b.TextContent=="Keep recorded execution and clear my unrecorded original").ClickAsync(new MouseEventArgs());
        Assert.Null(f.Journal.ExecutionOriginal);Assert.Contains("confirmed-unrecorded original",page.Markup);
        Assert.DoesNotContain("Choose a company with drilling, workflow, planning and financial view access",page.Markup);
    }
    [Fact]public async Task EarlierAbsentReceiptWithFailedCleanupCannotTrapAnOriginalAfterAnotherExecutionAppears()
    {
        using var f=new DrillingExecutionStateTests.Fixture();await Pending(f);var original=f.State.Pending!;f.Journal.FailClear=true;
        f.Client.Setup(c=>c.RecoverExecutionStartAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>())).ReturnsAsync(new DrillingExecutionStartReceipt(original.Revision(),original.OperationId(),null,DateTimeOffset.UtcNow));
        await f.State.RecoverAsync();Assert.NotNull(f.State.Receipt);Assert.Null(f.State.Receipt.Recorded);Assert.NotNull(f.State.Pending);Assert.True(f.State.CanInspectConflict);
        await f.State.InspectConflictAsync();Assert.NotNull(f.State.Conflict);Assert.NotNull(f.Journal.ExecutionOriginal);f.Journal.FailClear=false;
        await f.State.AcknowledgeConflictAsync();Assert.True(f.State.ConflictAcknowledged);Assert.Null(f.Journal.ExecutionOriginal);Assert.Null(f.State.Receipt);
        f.Client.Verify(c=>c.StartExecutionAsync(It.IsAny<DrillingExecutionStart>(),It.IsAny<CancellationToken>()),Times.Never());
    }
}
#endif
