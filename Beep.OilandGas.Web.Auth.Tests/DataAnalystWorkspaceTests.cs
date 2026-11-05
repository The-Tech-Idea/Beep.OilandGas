using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class DataAnalystWorkspaceTests
{
    [Fact]
    public void AnalystSeesAssignedWorkAndUnconfirmedMetricsStayUnavailable()
    {
        var type=typeof(DataAnalystWorkspace);Assert.NotEmpty(type.GetCustomAttributes(typeof(AuthorizeAttribute),true));Assert.Contains(type.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),r=>r.Template=="/analytics/dashboard");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"analyst",new(new("tenant","company"),["DataAnalyst"],[])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="DATA_ANALYST",Name="Data analyst",DefaultRoute="/analytics/dashboard"});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="analyst",PersonaCode="DATA_ANALYST"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync("DATA_ANALYST")).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="analyst",PersonaCode="DATA_ANALYST",CriticalTasks=[new(){TaskId="task",TaskType="REVIEW",Priority=1,WorkflowName="data-process",StepName="review",ScopeKind="FIELD",AssignedRole="DataAnalyst",EntityId="well",EntityType="WELL",Route="/ppdm39/process/instances/data-process"}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(inbox.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));var page=ui.RenderComponent<DataAnalystWorkspace>();
        page.WaitForAssertion(()=>Assert.Contains("data-process",page.Markup));Assert.Contains("totals are unavailable",page.Markup);company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("data-process",page.Markup));
    }
}
#endif
