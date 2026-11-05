using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Bunit.TestDoubles;
using Beep.OilandGas.Web.Components.Workflow;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Page=Beep.OilandGas.Web.Pages.PPDM39.Economics.AFEManagement;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentInvestmentStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentAfeStateTests.Fixture Base=new();internal DevelopmentInvestmentState State;internal DevelopmentInvestmentQueueState QueueState;
        internal DevelopmentInvestmentReview Review;internal DevelopmentInvestmentQueue Queue;internal DevelopmentAfeCommand Request;
        internal Fixture()
        {
            var current=Base.Review;Request=new(current,Guid.NewGuid(),"statutory","AFE-independent-1","Recorded funding request",25.125m,new(2027,2,3),"Original request maker basis");var facts=DevelopmentAfeCommand.Facts(Request.ToRecord(DateTime.UtcNow));
            var progress=current.Source.Review with{Scope=current.Source.Review.Scope with{ActorUserId="investor"}};progress=progress with{Revision=progress.ContentRevision()};current=current with{Source=current.Source with{Review=progress},Books=current.Books.AsReader("investor"),Afes=[facts]};current=current with{Revision=current.ContentRevision()};Base.Review=current;
            Review=new(current,facts.AfeId,null,[],DateTimeOffset.UtcNow);Review.Require();Select(true);
            var source=new DevelopmentAfeIndexItem(facts,progress.InstanceId,progress.Scope.FieldId,current.Source.Baseline!.FDP_NAME,current.Source.Baseline.FDP_VERSION);Queue=new(progress.Scope.Company,"investor",progress.Scope.CoreDatabase,current.BudgetDatabase,[new(source,null)],"",DateTimeOffset.UtcNow);Queue=Queue with{Revision=Queue.ContentRevision()};Queue.Require(Queue.Company,"investor");
            Base.Client.Setup(client=>client.ReviewInvestmentAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Review);
            Base.Client.Setup(client=>client.QueueAsync(It.IsAny<CancellationToken>())).ReturnsAsync(()=>Queue);
            State=NewState();QueueState=new(Base.Company,Base.Baseline.Reader.Personas.Object,Base.Client.Object,(_,_,text)=>text);
        }
        internal void Select(bool review)=>Base.Company.TrySelect(Base.Company.Clear(),"investor",new(Review.Current.Source.Review.Scope.Company,["InvestmentReviewer"],review?["Accounting.View","DevelopmentPlanning.View","Workflow.View","Accounting.ReviewDevelopmentBudget"]:["Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        internal DevelopmentInvestmentState NewState()=>new(Base.Company,Base.Baseline.Reader.Personas.Object,Base.Client.Object,(_,_,text)=>text);
        internal async Task Inspect(){await State.LoadAsync(Review.Current.Source.Review.InstanceId,Review.AfeId);await State.InspectAsync();Assert.NotNull(State.Review);}
        public void Dispose(){State.Dispose();QueueState.Dispose();Base.Dispose();}
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateReadOnlyEvidenceCannotPublishAcrossReceivingContextChanges(string change)
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review.Current.Source.Review.InstanceId,f.Review.AfeId);var waiting=new TaskCompletionSource<DevelopmentInvestmentReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Base.Client.Setup(c=>c.ReviewInvestmentAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(waiting.Task);var read=f.State.InspectAsync();
        if(change=="company")f.Base.Company.Clear();else if(change=="persona")f.Base.Baseline.Reader.Personas.Raise(c=>c.Changed+=null);else if(change=="dispose")f.State.Dispose();else await f.State.LoadAsync("other-process","other-afe");waiting.SetResult(f.Review);await read;Assert.Null(f.State.Review);
    }
    [Fact]public async Task ReadOnlyEvidenceIsDefensivelyCopiedAndFailedReloadClearsOldFacts()
    {
        using var f=new Fixture();await f.Inspect();f.State.Review!.Current.Source.Baseline!.FDP_NAME="changed consumer";Assert.NotEqual("changed consumer",f.State.Review!.Current.Source.Baseline!.FDP_NAME);
        f.Base.Client.Setup(c=>c.ReviewInvestmentAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());await f.State.InspectAsync();Assert.Null(f.State.Review);Assert.NotNull(f.State.Error);
    }
    [Fact] public async Task MakerOrRecordedOutcomeNeverEnablesAnotherDecisionAndQueueDistinguishesPendingFromRecorded()
    {
        using var f=new Fixture();var original=new DevelopmentInvestmentCommand(f.Review,Guid.NewGuid(),"APPROVE","Recorded independent decision");var recorded=DevelopmentInvestmentCommand.Facts(original.ToRecord(DateTime.UtcNow));f.Review=f.Review with{Recorded=recorded,Blockers=["The original independent decision is already recorded."]};
        await f.Inspect();Assert.Equal("APPROVE",f.State.Review!.Recorded!.Decision);
        await f.QueueState.LoadAsync();Assert.Null(f.QueueState.Current!.Items[0].Recorded);f.Queue=f.Queue with{Items=[f.Queue.Items[0] with{Recorded=recorded}]};f.Queue=f.Queue with{Revision=f.Queue.ContentRevision()};await f.QueueState.LoadAsync();Assert.Equal("APPROVE",f.QueueState.Current!.Items[0].Recorded!.Decision);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("null")][InlineData("failure")]
    public async Task ForeignMissingOrFailedQueueCannotBecomeSuccessfulEmptyWork(string fault)
    {
        using var f=new Fixture();f.Base.Client.Setup(client=>client.QueueAsync(It.IsAny<CancellationToken>())).Returns(()=>{
            if(fault=="null")return Task.FromResult<DevelopmentInvestmentQueue>(null!);if(fault=="failure")return Task.FromException<DevelopmentInvestmentQueue>(new HttpRequestException());var found=f.Queue with{Company=fault=="company"?new("tenant","foreign"):f.Queue.Company,ActorUserId=fault=="actor"?"another":f.Queue.ActorUserId};return Task.FromResult(found with{Revision=found.ContentRevision()});});
        await f.QueueState.LoadAsync();Assert.Null(f.QueueState.Current);Assert.NotNull(f.QueueState.Error);
    }
    [Theory][InlineData("company")][InlineData("persona")]
    public async Task LateQueueAfterContextChangeDoesNotExposeOldCompanyRequests(string change)
    {
        using var f=new Fixture();var paused=new TaskCompletionSource<DevelopmentInvestmentQueue>();f.Base.Client.Setup(client=>client.QueueAsync(It.IsAny<CancellationToken>())).Returns(paused.Task);var load=f.QueueState.LoadAsync();if(change=="company")f.Base.Company.Clear();else f.Base.Baseline.Reader.Personas.Raise(personas=>personas.Changed+=null);paused.SetResult(f.Queue);await load;Assert.Null(f.QueueState.Current);
    }
    [Fact] public async Task RequestedReloadDuringOldCompanyReadFetchesCurrentCompanyAfterFencingTheOldAnswer()
    {
        using var f=new Fixture();var foreign=new CompanyScope("tenant","foreign");var source=f.Queue.Items[0].Source with{Afe=f.Queue.Items[0].Source.Afe with{AfeId="foreign-afe",BaselineId="foreign-baseline",Number="FOREIGN-AFE"},ProcessId="foreign-process"};
        var next=f.Queue with{Company=foreign,Items=[new(source,null)]};next=next with{Revision=next.ContentRevision()};var paused=new TaskCompletionSource<DevelopmentInvestmentQueue>();var calls=0;
        f.Base.Client.Setup(client=>client.QueueAsync(It.IsAny<CancellationToken>())).Returns(()=>++calls==1?paused.Task:Task.FromResult(next));var load=f.QueueState.LoadAsync();
        f.Base.Company.TrySelect(f.Base.Company.Clear(),"investor",new(foreign,["InvestmentReviewer"],["Accounting.View","DevelopmentPlanning.View","Workflow.View"]));await f.QueueState.LoadAsync();paused.SetResult(f.Queue);await load;
        Assert.Equal(2,calls);Assert.Equal(foreign,f.QueueState.Current!.Company);Assert.Equal("FOREIGN-AFE",f.QueueState.Current.Items[0].Source.Afe.Number);Assert.False(f.QueueState.Busy);
    }
    [Fact] public void ActualAfeParentShowsReadOnlyFinancialFactsAndDirectsDecisionsToAssignedWork()
    {
        using var f=new Fixture();using var ui=Ui(f);var page=ui.RenderComponent<Page>();page.WaitForAssertion(()=>Assert.Contains("Inspect investment decision",page.Markup));Assert.Contains("AFE-independent-1",page.Markup);
        page.FindAll("button").Single(button=>button.TextContent.Contains("Inspect investment decision")).Click();page.WaitForAssertion(()=>Assert.Contains("Its absence was checked",page.Markup));
        Assert.Contains("Use the assigned company inbox",page.Markup);Assert.Empty(page.FindComponents<MudSelect<string>>());Assert.DoesNotContain("Confirm independent investment decision",page.Markup);
        f.Base.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("AFE-independent-1",page.Markup));
    }
    [Fact] public void ActualContextlessParentListsNativeFinanceWorkAndExactSourceLinksWithNoGuessedApproval()
    {
        using var f=new Fixture();using var ui=Ui(f,false);var page=ui.RenderComponent<Page>();page.WaitForAssertion(()=>Assert.Contains("Company development investment work",page.Markup));page.WaitForAssertion(()=>Assert.Contains("Awaiting independent investment decision",page.Markup));
        Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("AFE-independent-1",page.Markup);Assert.Contains("processId=development-process",page.Markup);Assert.DoesNotContain("Recorded budget decision: APPROVE",page.Markup);
        f.Base.Company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("AFE-independent-1",page.Markup));
    }
    [Fact] public async Task InvestmentQueueFiltersOnlyAlreadyAuthorizedFieldsAndResetsOnPersonaChange()
    {
        using var f=new Fixture();var original=f.Queue.Items[0];var second=original with{Source=original.Source with{FieldId="field-مصر",Afe=original.Source.Afe with{AfeId="different-afe",Number="AFE-OTHER"}}};
        f.Queue=f.Queue with{Items=[original,second]};f.Queue=f.Queue with{Revision=f.Queue.ContentRevision()};f.Queue.Require(f.Queue.Company,"investor");
        using var ui=Ui(f,false);var page=ui.RenderComponent<Page>();page.WaitForAssertion(()=>Assert.Contains("AFE-OTHER",page.Markup));Assert.Contains("AFE-independent-1",page.Markup);Assert.Contains("2 of 2 accessible AFE requests",page.Markup);
        await page.InvokeAsync(()=>page.FindComponents<MudSelect<string>>().Single(item=>item.Instance.Label=="Filter investment work by FIELD").Instance.ValueChanged.InvokeAsync("field-مصر"));
        page.WaitForAssertion(()=>Assert.Contains("1 of 2 accessible AFE requests",page.Markup));Assert.Contains("AFE-OTHER",page.Markup);Assert.DoesNotContain("AFE-independent-1",page.Markup);
        f.Queue=f.Queue with{Items=[original]};f.Queue=f.Queue with{Revision=f.Queue.ContentRevision()};page.FindAll("button").Single(button=>button.TextContent.Contains("Refresh investment work")).Click();
        page.WaitForAssertion(()=>Assert.Contains("1 of 1 accessible AFE requests",page.Markup));Assert.Contains("AFE-independent-1",page.Markup);Assert.DoesNotContain("AFE-OTHER",page.Markup);
        f.Queue=f.Queue with{Items=[original,second]};f.Queue=f.Queue with{Revision=f.Queue.ContentRevision()};f.Base.Baseline.Reader.Personas.Raise(value=>value.Changed+=null);page.WaitForAssertion(()=>Assert.Contains("2 of 2 accessible AFE requests",page.Markup));Assert.Contains("AFE-independent-1",page.Markup);Assert.Contains("AFE-OTHER",page.Markup);
    }

    [Fact] public void RecordedNativeQueueNamesAreEncodedByTheActualParent()
    {
        using var f=new Fixture();var request=f.Request with{Name="<script>unsafe source text</script>"};var facts=DevelopmentAfeCommand.Facts(request.ToRecord(DateTime.UtcNow));f.Queue=f.Queue with{Items=[new(f.Queue.Items[0].Source with{Afe=facts},null)]};f.Queue=f.Queue with{Revision=f.Queue.ContentRevision()};
        using var ui=Ui(f,false);var page=ui.RenderComponent<Page>();page.WaitForAssertion(()=>Assert.Contains("unsafe source text",page.Markup));Assert.DoesNotContain("<script>",page.Markup);Assert.Contains("&lt;script&gt;",page.Markup);
    }
    [Theory][InlineData(true,true)][InlineData(false,true)][InlineData(true,false)]
    public void ActualAccountingLandingRequiresStandardCurrentRoleAndViewsForInvestmentQueueEntry(bool views,bool role)
    {
        using var f=new Fixture();if(!views)f.Base.Company.TrySelect(f.Base.Company.Clear(),"investor",new(f.Review.Current.Source.Review.Scope.Company,["InvestmentReviewer"],["Accounting.View"]));
        using var ui=Ui(f,false,ui=>{
            var auth=ui.AddTestAuthorization();auth.SetAuthorized("investor");auth.SetRoles(role?"InvestmentReviewer":"UnrelatedRole");
        ui.ComponentFactories.AddStub<Beep.OilandGas.Web.Components.FieldSelector>();ui.ComponentFactories.AddStub<PersonaTaskInbox>();ui.ComponentFactories.AddStub<SalesInvoiceQueue>();ui.ComponentFactories.AddStub<WorkOrderFinancialWorkbench>();
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync((string?)null);ui.Services.AddSingleton(fields.Object);
        var accounting=new Mock<IAccountingServiceClient>();accounting.Setup(value=>value.GetCompanyAPInvoicesAsync(It.IsAny<string?>())).ReturnsAsync([]);ui.Services.AddSingleton(accounting.Object);ui.Services.AddSingleton(Mock.Of<IBusinessProcessServiceClient>());
        });
        var page=ui.RenderComponent<Beep.OilandGas.Web.Pages.PPDM39.Accounting.AccountingDashboard>();
        page.WaitForAssertion(()=>Assert.Equal(views && role,page.FindAll("a").Any(link=>link.GetAttribute("href")=="/economics/afe" && link.TextContent.Contains("Company development investment work"))));
    }
    private static Bunit.TestContext Ui(Fixture f,bool source=true,Action<Bunit.TestContext>? configure=null)
    {
        var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Base.Company);ui.Services.AddSingleton(f.Base.Baseline.Reader.Personas.Object);ui.Services.AddSingleton(f.Base.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Base.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));configure?.Invoke(ui);ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/economics/afe"+(source?"?processId="+f.Review.Current.Source.Review.InstanceId:""));return ui;
    }
}
