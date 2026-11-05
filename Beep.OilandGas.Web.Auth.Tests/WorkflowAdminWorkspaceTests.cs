using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39.Admin;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkflowAdminWorkspaceTests
{
    [Fact]
    public void AdminRouteRequiresStandardRoleAndShowsItsAssignedWork()
    {
        var type=typeof(WorkflowAdminWorkspace);Assert.Contains(type.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),r=>r.Template=="/lifecycle/workflow/admin");Assert.Contains(type.GetCustomAttributes(typeof(AuthorizeAttribute),true).Cast<AuthorizeAttribute>(),a=>a.Roles=="Administrator");
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"admin",new(new("tenant","company"),["Administrator"],[])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="WORKFLOW_ADMINISTRATOR",Name="Workflow administrator",DefaultRoute="/lifecycle/workflow/admin"});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="admin",PersonaCode="WORKFLOW_ADMINISTRATOR"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync("WORKFLOW_ADMINISTRATOR")).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="admin",PersonaCode="WORKFLOW_ADMINISTRATOR",CriticalTasks=[new(){TaskId="task",TaskType="REVIEW",Priority=1,WorkflowName="admin-process",StepName="review",ScopeKind="FIELD",AssignedRole="Administrator",EntityId="well",EntityType="WELL",Route="/ppdm39/process/instances/admin-process"}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(inbox.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));var page=ui.RenderComponent<WorkflowAdminWorkspace>();
        page.WaitForAssertion(()=>Assert.Contains("admin-process",page.Markup));Assert.Contains("Workflow totals are unavailable",page.Markup);company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("admin-process",page.Markup));
    }
}
#endif
