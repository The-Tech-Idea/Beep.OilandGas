using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.DrillingAndConstruction.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DrillingExecutionAssignmentConflictStateTests
{
    private static DrillingExecutionAssignmentConflictReview Proof(DrillingExecutionAssignment original)
    {
        var other=original with{IntentId=Guid.NewGuid(),Reason="Different original reporting source and recipients"};var receipt=DrillingExecutionAssignmentStateTests.Receipt(other);
        var result=new DrillingExecutionAssignmentConflictReview(original.Revision(),original.InstanceId(),true,other,receipt,"",DateTimeOffset.UtcNow);result=result with{Revision=result.ContentRevision()};result.Require(original);return result;
    }
    private static async Task<DrillingExecutionAssignmentConflictReview> Pending(DrillingExecutionAssignmentStateTests.Fixture f)
    {
        await f.Prepare();f.Retained=CompanyInventorySourceReview.Copy(f.Native.Original);var proof=Proof(f.Retained);
        f.Client.Setup(c=>c.GetExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        f.Client.Setup(c=>c.ConfirmExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignmentConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        await f.State.LoadAsync(f.Retained.Source.Basis.ReviewInstanceId);return proof;
    }
    [Fact]public async Task InspectionKeepsOriginalAndOnlyExplicitRecheckedConfirmationClears()
    {
        using var f=new DrillingExecutionAssignmentStateTests.Fixture();var proof=await Pending(f);await f.State.InspectConflictAsync();Assert.Equal(proof.Revision,f.State.Conflict!.Revision);Assert.NotNull(f.Retained);Assert.False(f.State.ConflictAcknowledged);
        await f.State.AcknowledgeConflictAsync();Assert.True(f.State.ConflictAcknowledged);Assert.Null(f.Retained);Assert.Null(f.State.Pending);Assert.Null(f.State.Receipt);Assert.Equal(0,f.Writes);
        f.Client.Verify(c=>c.ConfirmExecutionAssignmentConflictAsync(It.Is<DrillingExecutionAssignmentConflictAcknowledgement>(x=>x.ReviewRevision==proof.Revision),It.IsAny<CancellationToken>()),Times.Once());
    }
    [Theory,InlineData("lookup"),InlineData("changed"),InlineData("clear")]
    public async Task FailedLookupChangedReviewOrCleanupFailurePreservesOriginal(string fault)
    {
        using var f=new DrillingExecutionAssignmentStateTests.Fixture();var proof=await Pending(f);await f.State.InspectConflictAsync();
        if(fault=="lookup")f.Client.Setup(c=>c.ConfirmExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignmentConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        if(fault=="changed")f.Client.Setup(c=>c.ConfirmExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignmentConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ReturnsAsync(Proof(f.Retained!));
        if(fault=="clear")f.Fault="clear";
        await f.State.AcknowledgeConflictAsync();Assert.NotNull(f.Retained);Assert.NotNull(f.State.Pending);Assert.NotNull(f.State.Error);Assert.Equal(fault=="clear",f.State.ConflictAcknowledged);Assert.Equal(0,f.Writes);
        if(fault=="clear")
        {
            Assert.Contains("again to recheck and finish",f.State.PendingExplanation);var at=f.State.Conflict!.CheckedAtUtc;
            f.Client.Setup(c=>c.ConfirmExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignmentConflictAcknowledgement>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
            await f.State.AcknowledgeConflictAsync();Assert.Equal(at,f.State.Conflict!.CheckedAtUtc);Assert.NotNull(f.Retained);
        }
    }
    [Theory,InlineData("company"),InlineData("persona"),InlineData("route"),InlineData("dispose")]
    public async Task LateConfirmationCannotPublishOrClearAfterContextChange(string change)
    {
        using var f=new DrillingExecutionAssignmentStateTests.Fixture();var proof=await Pending(f);await f.State.InspectConflictAsync();
        var response=new TaskCompletionSource<DrillingExecutionAssignmentConflictReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(c=>c.ConfirmExecutionAssignmentConflictAsync(It.IsAny<DrillingExecutionAssignmentConflictAcknowledgement>(),It.IsAny<CancellationToken>())).Returns(response.Task);var work=f.State.AcknowledgeConflictAsync();
        if(change=="company")f.Company.Clear();if(change=="persona")f.Personas.Raise(p=>p.Changed+=null);if(change=="route")await f.State.LoadAsync("another");if(change=="dispose")f.State.Dispose();response.SetResult(proof);await work;
        Assert.Null(f.State.Conflict);Assert.False(f.State.ConflictAcknowledged);Assert.NotNull(f.Retained);Assert.Equal(0,f.Writes);
    }
    [Theory,InlineData(false),InlineData(true)]public async Task ActualPageShowsExactRecipientsAndExplicitCleanupWithoutFalseReadOnlyMessage(bool earlierAbsent)
    {
        using var f=new DrillingExecutionAssignmentStateTests.Fixture();await Pending(f);f.Fault="clear";
        if(earlierAbsent)f.Client.Setup(c=>c.RecoverExecutionAssignmentAsync(It.IsAny<DrillingExecutionAssignment>(),It.IsAny<CancellationToken>())).ReturnsAsync((DrillingExecutionAssignment input,CancellationToken _)=>DrillingExecutionAssignmentStateTests.Receipt(input,false));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton(f.Journal.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<NavigationManager>().NavigateTo("/drilling/program-execution/assignment?reviewId="+Uri.EscapeDataString(f.Native.Original.Source.Basis.ReviewInstanceId));
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Drilling.DrillingExecutionAssignment>();page.WaitForAssertion(()=>Assert.Contains("Inspect a different recorded responsibility",page.Markup));
        if(earlierAbsent){await page.FindAll("button").Single(b=>b.TextContent=="Recover original reporting assignment").ClickAsync(new MouseEventArgs());Assert.Contains("This original reporting assignment was confirmed absent.",page.Markup);}
        await page.FindAll("button").Single(b=>b.TextContent=="Inspect a different recorded responsibility").ClickAsync(new MouseEventArgs());Assert.Contains("Recorded recipient site-recorder-1",page.Markup);Assert.NotNull(f.Retained);
        await page.FindAll("button").Single(b=>b.TextContent=="Keep recorded responsibility and clear my unrecorded original").ClickAsync(new MouseEventArgs());Assert.Contains("Clearing the tab input is not confirmed",page.Markup);Assert.NotNull(f.Retained);
        f.Fault=null;await page.FindAll("button").Single(b=>b.TextContent=="Keep recorded responsibility and clear my unrecorded original").ClickAsync(new MouseEventArgs());Assert.Null(f.Retained);Assert.Contains("You kept the exact different recorded responsibility",page.Markup);Assert.DoesNotContain("Read-only source.",page.Markup);Assert.Equal(0,f.Writes);
    }
}
#endif
