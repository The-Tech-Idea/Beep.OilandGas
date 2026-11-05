using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Page=Beep.OilandGas.Web.Pages.PPDM39.Economics.AFEManagement;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DevelopmentInvestmentAssignedStateTests
{
    internal sealed class Fixture:IDisposable
    {
        internal DevelopmentInvestmentStateTests.Fixture Source=new();
        internal Mock<IPersonaContextService> Personas=new();
        internal DevelopmentInvestmentAssignedReview Review;
        internal DevelopmentInvestmentAssignedState State;
        internal Fixture()
        {
            Review=new(Source.Review,DevelopmentInvestmentHandoff.InstancePrefix+"assigned-original","Configured investment review","1.0","budget-sender",
                "investment","original-step","Independent investment review","InvestmentReviewer","FINANCE","original-task",new('A',64),new('B',64),new('C',64),"",DateTimeOffset.UtcNow){FundingRationale=Source.Request.Rationale,StageNumber=1,StageCount=1};
            Review=Review with{Revision=Review.ContentRevision()};Review.Require(Review.Source.Current.Source.Review.Scope.Company,"investor",Review.InstanceId);
            Source.Base.Company.TrySelect(Source.Base.Company.Clear(),"investor",new(Review.Source.Current.Source.Review.Scope.Company,["InvestmentReviewer"],
                ["Accounting.View","DevelopmentPlanning.View","Workflow.View","Workflow.Approve","Accounting.ReviewDevelopmentBudget"]));
            Personas.SetupGet(value=>value.CurrentPersona).Returns(new AppPersona{Code="FINANCE",Name="Investment finance"});
            Personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId="investor",PersonaCode="FINANCE"});Personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Source.Base.Client.Setup(value=>value.ReadAssignedInvestmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Review);
            State=new(Source.Base.Company,Personas.Object,Source.Base.Client.Object,Source.Base.Journal,(_,_,message)=>message);
        }
        public void Dispose(){State.Dispose();Source.Dispose();}
    }
    [Fact]public async Task ExactAssignedAfeAndApprovedBasisAreDefensivelyCopied()
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review.InstanceId);Assert.Equal(f.Review.TaskId,f.State.Current!.TaskId);
        f.State.Current!.Source.Current.Source.Baseline!.FDP_NAME="Changed consumer";
        Assert.NotEqual("Changed consumer",f.State.Current!.Source.Current.Source.Baseline!.FDP_NAME);
    }
    [Theory][InlineData("role")][InlineData("persona")][InlineData("process")]
    public async Task WrongReceivingEnvelopeCannotBecomeCurrent(string fault)
    {
        using var f=new Fixture();var requested=f.Review.InstanceId;
        f.Review=fault switch{"role"=>f.Review with{AssignedRole="AnotherRole"},"persona"=>f.Review with{PersonaCode="OTHER"},_=>f.Review with{InstanceId=DevelopmentInvestmentHandoff.InstancePrefix+"another"}};
        f.Review=f.Review with{Revision=f.Review.ContentRevision()};await f.State.LoadAsync(requested);Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")][InlineData("dispose")]
    public async Task LateSourceCannotPublishAcrossContextRouteOrDisposal(string change)
    {
        using var f=new Fixture();var waiting=new TaskCompletionSource<DevelopmentInvestmentAssignedReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Source.Base.Client.Setup(value=>value.ReadAssignedInvestmentAsync(f.Review.InstanceId,It.IsAny<CancellationToken>())).Returns(waiting.Task);var load=f.State.LoadAsync(f.Review.InstanceId);
        if(change=="company")f.Source.Base.Company.Clear();else if(change=="persona")f.Personas.Raise(value=>value.Changed+=null);else if(change=="dispose")f.State.Dispose();
        else await f.State.LoadAsync("bad-route");
        waiting.SetResult(f.Review);await load;Assert.Null(f.State.Current);
    }
    [Fact]public async Task FailedReloadClearsEvidenceAndMissingPermissionDoesNotReadSource()
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review.InstanceId);Assert.NotNull(f.State.Current);
        f.Source.Base.Client.Setup(value=>value.ReadAssignedInvestmentAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException("Unavailable source"));
        await f.State.LoadAsync(f.Review.InstanceId);Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);
        f.Source.Base.Company.Clear();await f.State.LoadAsync(f.Review.InstanceId);f.Source.Base.Client.Verify(value=>value.ReadAssignedInvestmentAsync(f.Review.InstanceId,It.IsAny<CancellationToken>()),Times.Exactly(2));
    }
    [Fact]public void ActualAfeParentResolvesTheAssignmentQueryWithoutPreparingAnotherAfeOrDecision()
    {
        using var f=new Fixture();using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        ui.Services.AddSingleton(f.Source.Base.Company);ui.Services.AddSingleton(f.Personas.Object);ui.Services.AddSingleton(f.Source.Base.Client.Object);
        ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Source.Base.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/economics/afe?investmentHandoff="+Uri.EscapeDataString(f.Review.InstanceId));
        var page=ui.RenderComponent<Page>();
        page.WaitForAssertion(()=>Assert.Contains("Configured investment review",page.Markup));Assert.Contains(f.Review.Source.Afe().Number,page.Markup);Assert.Contains("Independent investment review",page.Markup);
        Assert.Contains("25.125 KWD",page.Markup);Assert.Contains("Approved development basis",page.Markup);Assert.Contains(f.Review.FundingRationale,page.Markup);Assert.DoesNotContain("Prepare DRAFT AFE request",page.Markup);
        f.Source.Base.Client.Verify(value=>value.ReadAssignedInvestmentAsync(f.Review.InstanceId,It.IsAny<CancellationToken>()),Times.Once);f.Source.Base.Client.VerifyNoOtherCalls();
    }
}
#endif
