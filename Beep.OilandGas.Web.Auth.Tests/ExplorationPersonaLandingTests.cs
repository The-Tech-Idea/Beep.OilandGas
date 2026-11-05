#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39.Exploration;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ExplorationPersonaLandingTests
{
    [Theory][InlineData("/exploration/prospect-board")][InlineData("/lifecycle/exploration/dashboard")]
    public void GeologistWorkspaceShowsAssignedWorkWithoutInventingMaturation(string route)
    {
        var type=typeof(ProspectBoard);Assert.NotEmpty(type.GetCustomAttributes(typeof(AuthorizeAttribute),true));Assert.Contains(type.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),entry=>entry.Template==route);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"geologist",new(new("tenant","company"),["Geologist"],[])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="EXPLORATION_GEOLOGIST",Name="Geologist",DefaultRoute=route});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="geologist",PersonaCode="EXPLORATION_GEOLOGIST"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync("EXPLORATION_GEOLOGIST")).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="geologist",PersonaCode="EXPLORATION_GEOLOGIST",CriticalTasks=[new(){TaskId="task",TaskType="REVIEW",Priority=1,WorkflowName="prospect-process",StepName="review",ScopeKind="FIELD",AssignedRole="Geologist",EntityId="prospect",EntityType="PROSPECT",Route="/ppdm39/process/instances/prospect-process"}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(inbox.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        var page=ui.RenderComponent<ProspectBoard>();page.WaitForAssertion(()=>Assert.Contains("prospect-process",page.Markup));Assert.Contains("Area is not a resource volume",page.Markup);Assert.Contains("/ppdm39/exploration/prospects",page.Markup);Assert.DoesNotContain("Chance (Pg)",page.Markup);Assert.DoesNotContain("Advance →",page.Markup);
        company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("prospect-process",page.Markup));
    }
}
#endif
