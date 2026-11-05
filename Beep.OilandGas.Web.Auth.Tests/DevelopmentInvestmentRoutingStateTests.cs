#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentInvestmentRoutingStateTests
{
    private sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentStateTests.Fixture Investment=new();
        internal DevelopmentInvestmentRoutingReview Review;
        internal DevelopmentInvestmentRoutingState State;
        internal string Process=>Investment.Review.Current.Source.Review.InstanceId;
        internal string Afe=>Investment.Review.AfeId;
        internal Fixture()
        {
            var stage=new DevelopmentReviewRecipientStep("investment","InvestmentReviewer",[new("independent-finance-reviewer","FINANCE")],new string('A',64));
            var option=new DevelopmentInvestmentRoutingOption("configured-investment","Configured investment review","2",new string('B',64),[stage],[]);
            Review=new(Investment.Review,[option],"",DateTimeOffset.UtcNow);Review=Review with{Revision=Review.ContentRevision()};Select();
            Investment.Base.Client.Setup(client=>client.ReviewInvestmentRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Review);
            State=new(Investment.Base.Company,Investment.Base.Baseline.Reader.Personas.Object,Investment.Base.Client.Object,(_,_,text)=>text);State.SetSource(Process,Afe);
        }
        internal void Select(bool allowed=true)=>Investment.Base.Company.TrySelect(Investment.Base.Company.Clear(),"investor",new(Investment.Review.Current.Source.Review.Scope.Company,["InvestmentReviewer"],allowed?
            ["Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Start","Accounting.PrepareDevelopmentBudget"]:["Accounting.View","DevelopmentPlanning.View","Workflow.View"]));
        public void Dispose(){State.Dispose();Investment.Dispose();}
    }
    [Fact]public async Task CurrentSenderExplicitlyReadsAnExactSourceAndCannotMutateTheFrozenRoutingProjection()
    {
        using var f=new Fixture();Assert.Null(f.State.Current);f.Investment.Base.Client.Verify(client=>client.ReviewInvestmentRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
        await f.State.ReadAsync();Assert.True(Assert.Single(f.State.Current!.Options).Ready);var copy=f.State.Current!;((List<DevelopmentInvestmentRoutingOption>)copy.Options).Clear();Assert.Single(f.State.Current!.Options);
        f.Investment.Base.Client.Verify(client=>client.DecideAssignedInvestmentStageAsync(It.IsAny<DevelopmentInvestmentStageDecision>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("company")][InlineData("actor")][InlineData("process")][InlineData("afe")][InlineData("revision")][InlineData("self")][InlineData("duplicate")]
    public async Task ForeignOrAmbiguousRoutingEvidenceCannotReachTheScreen(string fault)
    {
        using var f=new Fixture();var source=f.Review.Source;var scope=source.Current.Source.Review.Scope;
        if(fault=="company")
        {
            var selection=f.Investment.Base.Company.Selection!;
            f.Investment.Base.Company.TrySelect(f.Investment.Base.Company.Clear(),selection.UserId,new(new("tenant","foreign"),selection.Roles.ToArray(),selection.Permissions.ToArray()));
        }
        if(fault=="actor")
        {
            var assigned=source.Current.Source.Review with{Scope=scope with{ActorUserId="other"}};
            assigned=assigned with{Revision=assigned.ContentRevision()};var current=source.Current with{Source=source.Current.Source with{Review=assigned}};current=current with{Revision=current.ContentRevision()};f.Review=f.Review with{Source=source with{Current=current}};
        }
        if(fault=="process")f.State.SetSource("other-process",f.Afe);if(fault=="afe")f.State.SetSource(f.Process,"other-afe");if(fault=="revision")f.Review=f.Review with{Revision=new string('C',64)};
        if(fault is "self" or "duplicate")
        {
            var option=f.Review.Options[0];var stage=option.Stages[0];f.Review=f.Review with{Options=[option with{Stages=[stage with{Recipients=fault=="self"?[new("investor","FINANCE")]:[stage.Recipients[0],stage.Recipients[0]]}]}]};
            f.Review=f.Review with{Revision=f.Review.ContentRevision()};
        }
        await f.State.ReadAsync();Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateReadCannotPublishOldRecipientsAfterContextChanges(string change)
    {
        using var f=new Fixture();var hold=new TaskCompletionSource<DevelopmentInvestmentRoutingReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Investment.Base.Client.Setup(client=>client.ReviewInvestmentRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(hold.Task);
        var task=f.State.ReadAsync();Assert.True(f.State.Busy);await f.State.ReadAsync();
        if(change=="company")f.Investment.Base.Company.Clear();if(change=="persona")f.Investment.Base.Baseline.Reader.Personas.Raise(personas=>personas.Changed+=null!);if(change=="route")f.State.SetSource("other-source",f.Afe);if(change=="dispose")f.State.Dispose();
        hold.SetResult(f.Review);await task;Assert.Null(f.State.Current);Assert.False(f.State.Busy);
        f.Investment.Base.Client.Verify(client=>client.ReviewInvestmentRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Fact]public async Task FailedReloadRemovesPreviouslyConfirmedRecipientsAndPermissionRevocationStopsReads()
    {
        using var f=new Fixture();await f.State.ReadAsync();Assert.NotNull(f.State.Current);
        f.Investment.Base.Client.Setup(client=>client.ReviewInvestmentRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Unavailable configured source"));
        await f.State.ReadAsync();Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);f.Select(false);await f.State.ReadAsync();Assert.Null(f.State.Current);Assert.False(f.State.CanRead);
        f.Investment.Base.Client.Verify(client=>client.ReviewInvestmentRoutingAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Exactly(2));
    }
    [Theory][InlineData("ready")][InlineData("blocked")][InlineData("empty")][InlineData("permission")]
    public void ActualSourceComponentShowsRealReadinessWithoutClaimingAnAssignment(string mode)
    {
        using var f=new Fixture();if(mode=="permission")f.Select(false);
        if(mode=="empty")f.Review=f.Review with{Options=[]};
        if(mode=="blocked"){var option=f.Review.Options[0];f.Review=f.Review with{Options=[option with{Stages=[option.Stages[0] with{Recipients=[]}],Blockers=["Configured role has no independent eligible reviewer."]}]};}
        f.Review=f.Review with{Revision=f.Review.ContentRevision()};using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Investment.Base.Company);ui.Services.AddSingleton(f.Investment.Base.Baseline.Reader.Personas.Object);ui.Services.AddSingleton(f.Investment.Base.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Investment.Base.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var panel=ui.RenderComponent<DevelopmentInvestmentPanel>(parameters=>parameters.Add(value=>value.ProcessId,f.Process).Add(value=>value.AfeId,f.Afe));
        if(mode=="permission"){Assert.DoesNotContain("Investment assignment readiness",panel.Markup);return;}
        Assert.DoesNotContain("Configured investment review",panel.Markup);panel.FindAll("button").Single(button=>button.TextContent.Contains("Review investment routing")).Click();
        panel.WaitForAssertion(()=>{
            Assert.Contains("Routing inspection does not record an assignment or notification",panel.Markup);
            if(mode=="ready"){Assert.Contains("independent-finance-reviewer",panel.Markup);Assert.Contains("Workspace FINANCE",panel.Markup);Assert.Contains("every configured stage",panel.Markup);}
            if(mode=="blocked"){Assert.Contains("no independent eligible reviewer",panel.Markup);Assert.DoesNotContain("independent-finance-reviewer",panel.Markup);}
            if(mode=="empty")Assert.Contains("No investment approval workflow is configured",panel.Markup);
        });
        f.Investment.Base.Company.Clear();panel.WaitForAssertion(()=>Assert.DoesNotContain("independent-finance-reviewer",panel.Markup));
    }
}
#endif
