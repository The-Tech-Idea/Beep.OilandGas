using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.LifeCycle.Models;
using Beep.OilandGas.Repository.Models;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Web.Services;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using AssignedPage=Beep.OilandGas.Web.Pages.PPDM39.Development.DevelopmentAssignedReview;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class DevelopmentAssignedReviewStateTests
{
    internal sealed class Fixture : IDisposable
    {
        internal CompanyClientContext Company=new();
        internal Mock<IPersonaContextService> Personas=new();
        internal Mock<IDevelopmentServiceClient> Client=new(MockBehavior.Strict);
        internal DevelopmentDraftWorkspaceTests.Journal Journal=new();
        internal DevelopmentAssignedReviewState State;
        internal DevelopmentAssignedReview Review;
        internal AppPersona Persona=new(){Code="RESERVOIR_ENGINEER",Name="Reservoir engineer"};
        internal Fixture(bool permitted=true)
        {
            var input=new CreateDevelopmentPlan{SelectionContext=new(new("tenant","company"),new("core","core-binding"),"planner","preference","original-field/واحد"),
                PlanningDatabase=new("planning","planning-binding"),IntentId=Guid.NewGuid(),FieldId="original-field/واحد",PlanName="Original <review> concept",Description="Recorded concept",EstimatedCapexMillions=12.125m,CapexCurrency="KWD"};
            var row=DevelopmentDraftWorkspaceTests.Recorded(input);
            var scope=new DevelopmentAssignedReviewScope(input.SelectionContext.Company,"reviewer",input.FieldId,input.SelectionContext.Database,input.PlanningDatabase,new("lifecycle","lifecycle-binding"));
            Review=new(scope,row,DevelopmentPlanSourceReview.RevisionFor(scope.Company,scope.CoreDatabase,scope.FieldId,scope.PlanningDatabase,row.FDP_ID,row),"development-process","Configured technical review","2.0","planner",
                "technical","technical-step","Independent technical review","ConfiguredReviewRole","RESERVOIR_ENGINEER","original-persona-task",new('A',64),new('B',64),new('C',64),"",DateTimeOffset.UtcNow);
            Review=Review with{Revision=Review.ContentRevision()};Review.Require(scope.Company,"reviewer",Review.InstanceId);
            Company.TrySelect(Company.Clear(),"reviewer",new(scope.Company,["ConfiguredReviewRole"],permitted?["DevelopmentPlanning.View","DevelopmentPlanning.Approve","Workflow.View","Workflow.Approve"]:["DevelopmentPlanning.View"]));
            Personas.SetupGet(personas=>personas.CurrentProfile).Returns(()=>new AppUserPersona{UserId="reviewer",PersonaCode=Persona.Code});
            Personas.SetupGet(personas=>personas.CurrentPersona).Returns(()=>Persona);Personas.Setup(personas=>personas.EnsureLoadedAsync()).Returns(Task.CompletedTask);
            Client.Setup(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync(()=>Review);
            State=new(Company,Personas.Object,Client.Object,Journal,(_,_,text)=>text);
        }
        public void Dispose(){State.Dispose();Company.Dispose();}
    }
    [Fact] public async Task ExactTaskScopeAndSourceRemainDetachedFromConsumers()
    {
        using var f=new Fixture();await f.State.LoadAsync(f.Review.InstanceId);Assert.Null(f.State.Error);Assert.NotNull(f.State.Current);
        Assert.Equal("original-field/واحد",f.State.Current.Scope.FieldId);f.State.Current.Plan.FDP_NAME="consumer mutation";
        Assert.Equal("Original <review> concept",f.State.Current.Plan.FDP_NAME);Assert.False(f.State.Loading);
    }
    [Theory][InlineData("failed")][InlineData("empty")][InlineData("foreign-company")][InlineData("wrong-workspace")][InlineData("changed-source")]
    public async Task FailedOrUnqualifiedTransportCannotBecomeSuccessfulSourceEvidence(string fault)
    {
        using var f=new Fixture();if(fault=="failed")f.Client.Setup(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        if(fault=="empty")f.Client.Setup(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ReturnsAsync((DevelopmentAssignedReview)null!);
        if(fault=="foreign-company")f.Review=f.Review with{Scope=f.Review.Scope with{Company=new("tenant","foreign")}};
        if(fault=="wrong-workspace")f.Review=f.Review with{PersonaCode="OTHER"};if(fault=="changed-source")f.Review.Plan.FDP_NAME="changed after review";
        await f.State.LoadAsync(f.Review.InstanceId);Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);Assert.False(f.State.Loading);
    }
    [Theory][InlineData("company")][InlineData("persona")][InlineData("route")]
    public async Task ChangedContextCannotPublishLateAssignedReview(string change)
    {
        using var f=new Fixture();var delayed=new TaskCompletionSource<DevelopmentAssignedReview>(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Client.Setup(client=>client.GetAssignedDraftReviewAsync(f.Review.InstanceId,It.IsAny<CancellationToken>())).Returns(delayed.Task);
        var load=f.State.LoadAsync(f.Review.InstanceId);
        if(change=="company")f.Company.Clear();else if(change=="persona"){f.Persona=new(){Code="OTHER"};f.Personas.Raise(personas=>personas.Changed+=null);}else await f.State.LoadAsync("another-process");
        delayed.SetResult(f.Review);await load;Assert.Null(f.State.Current);Assert.False(f.State.Loading);
    }
    [Fact] public async Task CompanyChangeDuringPersonaLoadCannotSendOldProcessRequest()
    {
        using var f=new Fixture();var delayed=new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        f.Personas.Setup(personas=>personas.EnsureLoadedAsync()).Returns(delayed.Task);
        var load=f.State.LoadAsync(f.Review.InstanceId);f.Company.Clear();delayed.SetResult();await load;
        f.Client.Verify(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);Assert.Null(f.State.Current);
    }
    [Fact] public async Task PersonaLoadFailureAfterClearingItsWorkspaceStillShowsFailureAndCannotRequestSource()
    {
        using var f=new Fixture();f.Personas.Setup(personas=>personas.EnsureLoadedAsync()).Returns(()=>{
            f.Personas.Raise(personas=>personas.Changed+=null);return Task.FromException(new HttpRequestException("Workspace load failed"));});
        await f.State.LoadAsync(f.Review.InstanceId);Assert.Null(f.State.Current);Assert.NotNull(f.State.Error);Assert.False(f.State.Loading);
        f.Client.Verify(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("allowed")][InlineData("failed")][InlineData("permission")]
    public void ActualAssignedPageRendersOriginalSourceOrCurrentFailureAndClearsAfterCompanyChange(string mode)
    {
        using var f=new Fixture(mode!="permission");if(mode=="failed")f.Client.Setup(client=>client.GetAssignedDraftReviewAsync(It.IsAny<string>(),It.IsAny<CancellationToken>())).ThrowsAsync(new HttpRequestException());
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(f.Company);ui.Services.AddSingleton(f.Personas.Object);
        ui.Services.AddSingleton(f.Client.Object);ui.Services.AddSingleton<IDevelopmentDraftIntentStore>(f.Journal);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        ui.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo("/development/fdp/assigned?processId="+Uri.EscapeDataString(f.Review.InstanceId));
        var page=ui.RenderComponent<AssignedPage>();page.WaitForAssertion(()=>Assert.DoesNotContain("Loading assigned development review",page.Markup));
        if(mode=="permission"){Assert.Contains("review permissions are required",page.Markup);Assert.DoesNotContain("Recorded concept",page.Markup);return;}
        if(mode=="failed"){Assert.Contains("could not be confirmed",page.Markup);Assert.DoesNotContain("Recorded concept",page.Markup);return;}
        Assert.Contains("Assignment FIELD: original-field/",page.Markup);Assert.Contains("Original &lt;review&gt; concept",page.Markup);Assert.Contains("12.125 million KWD",page.Markup);
        Assert.Contains("Assigned role: ConfiguredReviewRole",page.Markup);Assert.Contains("original-persona-task",page.Markup);Assert.Contains("does not record a technical",page.Markup);
        f.Company.Clear();page.WaitForAssertion(()=>Assert.Contains("Choose a company",page.Markup));Assert.DoesNotContain("Recorded concept",page.Markup);
    }
    [Fact] public async Task ActualAssignedWorkPanelFiltersDevelopmentReviewsAndLinksToTheExactReceivingPage()
    {
        using var f=new Fixture();var review=f.Review;var task=new UnifiedTask{TaskId=review.TaskId,TaskType="APPROVAL",Priority=2,ScopeKind="FIELD",AssignedRole=review.AssignedRole,
            WorkflowName=review.InstanceId,StepName=review.StepInstanceId,StepDisplayName=review.StepName,EntityType="FIELD_DEVELOPMENT_PLAN",EntityId=review.Plan.FDP_ID,
            EntityDescription=review.Plan.FDP_NAME,DevelopmentReviewRevision=review.Revision,Route=WorkflowTaskRoute.For("FIELD",review.InstanceId,"FIELD_DEVELOPMENT_PLAN")};
        var result=new UnifiedInbox{Company=review.Scope.Company,ActorUserId=review.Scope.ActorUserId,PersonaCode=review.PersonaCode,HighPriorityTasks=[task],Counts=new(){TotalPending=1,Approvals=1}};
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(service=>service.GetInboxAsync(review.PersonaCode)).ReturnsAsync(result);
        using var state=new PersonaTaskInboxState(inbox.Object,f.Company,f.Personas.Object);state.Start();await state.LoadAsync();
        using var ui=new Bunit.TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();
        var panel=ui.RenderComponent<Beep.OilandGas.Web.Components.Workflow.PersonaAssignedWorkPanel>(parameters=>parameters.Add(component=>component.Review,state));
        panel.FindAll("button").Single(button=>button.TextContent.Contains("Development plan reviews")).Click();
        panel.WaitForAssertion(()=>Assert.Contains("pending tasks for development plan reviews",panel.Markup));
        var link=panel.FindAll("a").Single(anchor=>anchor.TextContent.Contains("Open assigned development review"));Assert.Equal(task.Route,link.GetAttribute("href"));
        Assert.Contains("Original &lt;review&gt; concept",panel.Markup);Assert.DoesNotContain("FIELD_DEVELOPMENT_PLAN",panel.Markup);
    }
}
