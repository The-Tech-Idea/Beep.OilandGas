using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Page=Beep.OilandGas.Web.Pages.PPDM39.Economics.AFEManagement;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentAfeStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentPlanReleaseStateTests.Fixture Baseline=new();
        internal CompanyClientContext Company=>Baseline.Reader.Company;
        internal DevelopmentDraftWorkspaceTests.Journal Journal=>Baseline.Reader.Journal;
        internal Mock<IAfeServiceClient> Client=new(MockBehavior.Strict);
        internal DevelopmentAfeReview Review;
        internal DevelopmentAfeState State;
        internal Fixture(bool prepare=true)
        {
            var progress=Baseline.Review;var release=new DevelopmentPlanRelease(progress,Guid.NewGuid(),"Actual approved baseline");var source=new DevelopmentPlanBaselineReview(progress,release.ToRecord(DateTime.UtcNow),DateTimeOffset.UtcNow);
            var configuration=new CompanyLedgerBookConfigurationCommand(Guid.NewGuid().ToString("N"),progress.Scope.Company,progress.Scope.ActorUserId,Guid.NewGuid().ToString("N"),"statutory","Company statutory book","KWD",3,"TO_EVEN","gregorian","Asia/Kuwait","Explicit book configuration",new('A',64),new('B',64));
            var row=configuration.CreateRow();row.ROW_CREATED_BY=configuration.ActorUserId;row.ROW_CHANGED_BY=configuration.ActorUserId;row.ROW_CREATED_DATE=DateTime.UtcNow;row.ROW_CHANGED_DATE=row.ROW_CREATED_DATE;row.CONFIGURATION_JSON=CompanyLedgerBookConfigurationProof.Create(configuration,row);
            var books=CompanyLedgerBookReview.Create(progress.Scope.Company,progress.Scope.ActorUserId,[row]);
            Review=new(source,new("production","production-binding"),books,[],"",DateTimeOffset.UtcNow);Review=Review with{Revision=Review.ContentRevision()};Review.Require();
            Select(prepare);Client.Setup(client=>client.ReviewDevelopmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Review);
            Client.Setup(client=>client.QueueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>{
                var queue=new DevelopmentInvestmentQueue(Review.Source.Review.Scope.Company,Review.Source.Review.Scope.ActorUserId,Review.Source.Review.Scope.CoreDatabase,Review.BudgetDatabase,[],"",DateTimeOffset.UtcNow);return queue with{Revision=queue.ContentRevision()};});
            State=NewState();
        }
        internal void Select(bool prepare)=>Company.TrySelect(Company.Clear(),"reviewer",new(Review.Source.Review.Scope.Company,["ConfiguredReviewRole"],prepare?["Accounting.View","DevelopmentPlanning.View","Workflow.View","Accounting.PrepareDevelopmentBudget"]:["Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        internal DevelopmentAfeState NewState()=>new(Company,Baseline.Reader.Personas.Object,Client.Object,Journal,(_,_,text)=>text);
        internal async Task Prepare(){await State.LoadAsync(Review.Source.Review.InstanceId);State.Prepare("statutory","AFE-2027-1","Explicit financial request",12.125m,new(2027,2,3),"Private budget basis / أساس الميزانية");Assert.NotNull(State.Prepared);}
        internal static DevelopmentAfeReceipt Receipt(DevelopmentAfeCommand original,bool recorded=true)=>new(original.Revision(),recorded?original.ToRecord(DateTime.UtcNow):null,DateTimeOffset.UtcNow);
        public void Dispose(){State.Dispose();Baseline.Dispose();}
    }
    [Fact] public async Task ExplicitSourceBookCurrencyPrecisionAmountAndCalendarAreRequiredBeforeAnyTransport()
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review.Source.Review.InstanceId);Assert.True(f.State.CanPrepare);
        foreach(var fault in new[]{"book","amount","date","precision","name","rationale"})
        {
            f.State.Prepare(fault=="book"?null:"statutory","AFE-1",fault=="name"?" ":"Request",fault=="amount"?null:fault=="precision"?1.2345m:0m,fault=="date"?null:new(2027,2,3),fault=="rationale"?" ":"Explicit zero budget basis");Assert.Null(f.State.Prepared);Assert.NotNull(f.State.Error);
        }
        f.State.Prepare("statutory","AFE-1","Request",0m,new(2027,2,3),"Explicit zero amount request");Assert.NotNull(f.State.Prepared);Assert.Equal(0m,f.State.Prepared!.Amount);Assert.Null(f.Journal.AfeOriginal);
        f.Client.Verify(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Fact] public async Task FrozenOriginalIsEncryptedJournalInputBeforeWriteAndReceiptShowsOnlyRecordedDraftRequest()
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;
        f.Client.Setup(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>())).Returns((DevelopmentAfeCommand command,CancellationToken _) =>{
            Assert.Equal(command.Revision(),f.Journal.AfeOriginal!.Revision());return Task.FromResult(Fixture.Receipt(command));});
        await f.State.SubmitAsync();Assert.Null(f.Journal.AfeOriginal);Assert.Null(f.State.Pending);Assert.Equal("DRAFT",f.State.Recorded!.Status);Assert.Equal(12.125m,f.State.Recorded.RequestedAmount);Assert.Equal("KWD",f.State.Recorded.CurrencyCode);
        Assert.False(f.State.CanPrepare);Assert.Contains("Independent investment approval remains separate",f.State.Confirmation);
        var detached=f.State.Prepared;Assert.Null(detached);Assert.Equal(original.Number,f.State.Recorded.Number);
    }
    [Theory][InlineData("retain")][InlineData("response")][InlineData("receipt")][InlineData("clear")]
    public async Task UncertainRequestKeepsOriginalBlocksNewWriteAndRecoversWithoutRepair(string fault)
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;f.Journal.FailRetain=fault=="retain";f.Journal.FailClear=fault=="clear";
        f.Client.Setup(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>())).Returns((DevelopmentAfeCommand command,CancellationToken _) =>fault=="response"?Task.FromException<DevelopmentAfeReceipt>(new HttpRequestException()):Task.FromResult(fault=="receipt"?Fixture.Receipt(command) with{IntentRevision=new('F',64)}:Fixture.Receipt(command)));
        await f.State.SubmitAsync();Assert.NotNull(f.State.Pending);Assert.Equal(original.Revision(),f.State.Pending!.Revision());Assert.Null(f.State.Recorded);Assert.NotNull(f.State.Error);await f.State.SubmitAsync();
        f.Client.Verify(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>()),fault=="retain"?Times.Never():Times.Once());
        f.Journal.FailRetain=false;f.Journal.FailClear=false;f.Client.Setup(client=>client.RecoverDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original,fault!="retain"));await f.State.RecoverAsync();
        Assert.Null(f.State.Pending);Assert.Null(f.Journal.AfeOriginal);Assert.NotNull(f.State.Confirmation);Assert.False(f.State.CanPrepare);
    }
    [Fact] public async Task NewOwnerRecoversViewOnlyWithoutReloadingBooksOrNewWriteRights()
    {
        using var f=new Fixture();await f.Prepare();var original=f.State.Prepared!;f.Journal.AfeOriginal=original;f.Select(false);f.State.Dispose();f.State=f.NewState();await f.State.LoadAsync(f.Review.Source.Review.InstanceId);
        Assert.True(f.State.CanRecover);Assert.False(f.State.CanPrepare);f.Client.Setup(client=>client.RecoverDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>())).ReturnsAsync(Fixture.Receipt(original));
        await f.State.RecoverAsync();Assert.NotNull(f.State.Recorded);f.Client.Verify(client=>client.ReviewDevelopmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task ContextChangeDuringRetentionStopsUnsentRequestAndPreservesItsOriginalInput(string change)
    {
        using var f=new Fixture();await f.Prepare();var paused=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);f.Journal.BeforeReviewRetain=()=>paused.Task;
        var submit=f.State.SubmitAsync();if(change=="company")f.Company.Clear();else if(change=="persona")f.Baseline.Reader.Personas.Raise(personas=>personas.Changed+=null);else await f.State.LoadAsync("other-process");
        paused.SetResult();await submit;Assert.NotNull(f.Journal.AfeOriginal);Assert.Null(f.State.Recorded);Assert.Null(f.State.Pending);
        f.Client.Verify(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("process")]
    public async Task ForeignSuccessfulReviewCannotExposeItsSourceOrForm(string fault)
    {
        using var f=new Fixture();var progress=f.Review.Source.Review;var scope=progress.Scope;
        progress=progress with{Scope=scope with{Company=fault=="company"?new("tenant","foreign"):scope.Company,ActorUserId=fault=="actor"?"someone-else":scope.ActorUserId},InstanceId=fault=="process"?"another-process":progress.InstanceId};progress=progress with{Revision=progress.ContentRevision()};
        // Even a mismatched full response must not be accepted as a successful empty workspace.
        f.Review=f.Review with{Source=f.Review.Source with{Review=progress}};await f.State.LoadAsync("development-process");Assert.Null(f.State.Review);Assert.NotNull(f.State.Error);Assert.False(f.State.CanPrepare);
    }
    [Fact] public async Task LateReadAndLateWriteAfterCompanyChangeCannotPublishSourceOrReceipt()
    {
        using var f=new Fixture();var read=new TaskCompletionSource<DevelopmentAfeReview>();f.Client.Setup(client=>client.ReviewDevelopmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(read.Task);
        var load=f.State.LoadAsync(f.Review.Source.Review.InstanceId);f.Company.Clear();read.SetResult(f.Review);await load;Assert.Null(f.State.Review);
        f.Select(true);f.Client.Setup(client=>client.ReviewDevelopmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(f.Review);await f.Prepare();var original=f.State.Prepared!;
        var write=new TaskCompletionSource<DevelopmentAfeReceipt>();f.Client.Setup(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>())).Returns(write.Task);var save=f.State.SubmitAsync();f.Company.Clear();write.SetResult(Fixture.Receipt(original));await save;
        Assert.Null(f.State.Recorded);Assert.NotNull(f.Journal.AfeOriginal);
    }
    [Fact] public void ActualAfePageSelectsBookEntersExplicitCalendarAndMoneyThenConfirmsCanonicalReceipt()
    {
        using var f=new Fixture();f.Client.Setup(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>())).Returns((DevelopmentAfeCommand command,CancellationToken _) =>Task.FromResult(Fixture.Receipt(command)));
        using var ui=Ui(f);var page=ui.RenderComponent<Page>();page.WaitForAssertion(()=>Assert.Contains("Prepare DRAFT AFE request",page.Markup));Assert.Contains("Approved FDP source",page.Markup);Assert.DoesNotContain("Approved USD",page.Markup);
        var select=page.FindComponent<MudSelect<string>>();page.InvokeAsync(()=>select.Instance.ValueChanged.InvokeAsync("statutory"));
        page.FindAll("input").Single(input=>input.GetAttribute("maxlength")=="128").Change("AFE-screen");page.FindAll("input").Single(input=>input.GetAttribute("maxlength")=="256").Change("Screen expenditure request");
        page.InvokeAsync(()=>page.FindComponent<MudNumericField<decimal?>>().Instance.ValueChanged.InvokeAsync(24.125m));page.InvokeAsync(()=>page.FindComponent<MudDatePicker>().Instance.DateChanged.InvokeAsync(new DateTime(2027,2,3)));
        page.Find("textarea").Change("Actual financial screen rationale");page.FindAll("button").Single(button=>button.TextContent.Contains("Review AFE request")).Click();page.WaitForAssertion(()=>Assert.Contains("Confirm DRAFT AFE request",page.Markup));Assert.Contains("24.125 KWD",page.Markup);Assert.Empty(page.FindAll("textarea"));
        f.Client.Verify(client=>client.RecordDevelopmentAsync(It.IsAny<DevelopmentAfeCommand>(),It.IsAny<CancellationToken>()),Times.Never);
        page.FindAll("button").Single(button=>button.TextContent=="Confirm AFE request").Click();page.WaitForAssertion(()=>Assert.Contains("Recorded DRAFT AFE request",page.Markup));Assert.Contains("Screen expenditure request",page.Markup);Assert.Contains("Actual expenditure",page.Markup,StringComparison.OrdinalIgnoreCase);Assert.Null(f.Journal.AfeOriginal);
        f.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("Screen expenditure request",page.Markup));
    }
    [Fact] public void ContextlessAndViewOnlyScreensOfferRealSourcesWithoutUnqualifiedCreateOrFakeTotals()
    {
        using var f=new Fixture(false);using var ui=Ui(f,false);var page=ui.RenderComponent<Page>();page.WaitForAssertion(()=>Assert.Contains("Open an approved baseline",page.Markup));
        Assert.Contains("href=\"/development/fdp\"",page.Markup);Assert.Contains("href=\"/ppdm39/workorder\"",page.Markup);Assert.Empty(page.FindAll("input"));Assert.DoesNotContain("Committed",page.Markup);
        f.Client.Verify(client=>client.ReviewDevelopmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private static Bunit.TestContext Ui(Fixture f,bool source=true)
    {
        var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Baseline.Reader.Personas.Object);ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/economics/afe"+(source?"?processId="+f.Review.Source.Review.InstanceId:""));return ui;
    }
}
