#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Pages.PPDM39.Production;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class ProductionPersonaRouteTests
{
    [Theory][InlineData(true)][InlineData(false)]
    public void SeededProductionPersonaOpensItsOwnAssignedWork(bool manager)
    {
        var route=manager?"/production/dashboard":"/lifecycle/production/dashboard";
        var type=manager?typeof(ProductionManagerDashboard):typeof(ProductionEngineerWorkspace);
        Assert.Contains(type.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),entry=>entry.Template==route);
        Assert.NotEmpty(type.GetCustomAttributes(typeof(AuthorizeAttribute),true));
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["Operator"],["Maintenance.ReturnToService"])));
        var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code="PRODUCTION",Name="Production",DefaultRoute=route});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="actor",PersonaCode="PRODUCTION"});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync("PRODUCTION")).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="actor",PersonaCode="PRODUCTION",CriticalTasks=[new(){TaskId="task",TaskType="REVIEW",Priority=1,WorkflowName="production-process",StepName="review",ScopeKind="FIELD",AssignedRole="Operator",EntityId="well",EntityType="WELL",Route="/ppdm39/process/instances/production-process"}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}});
        var authentication=new Mock<AuthenticationStateProvider>();authentication.Setup(x=>x.GetAuthenticationStateAsync()).ReturnsAsync(new AuthenticationState(new System.Security.Claims.ClaimsPrincipal()));
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(inbox.Object);ui.Services.AddSingleton(authentication.Object);ui.Services.AddSingleton(Mock.Of<ICompanyMaintenanceHandoffClient>());ui.Services.AddSingleton(Mock.Of<IFieldSelectionContext>());ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        void Verify<T>() where T:IComponent
        {
            var page=ui.RenderComponent<T>();page.WaitForAssertion(()=>Assert.Contains("production-process",page.Markup));Assert.Contains("Pending work: 1",page.Markup);Assert.Contains("Recorded maintenance work",page.Markup);company.Clear();page.WaitForAssertion(()=>Assert.DoesNotContain("production-process",page.Markup));Assert.DoesNotContain("Recorded maintenance work",page.Markup);
        }
        if(manager)Verify<ProductionManagerDashboard>();else Verify<ProductionEngineerWorkspace>();
    }
}
#endif
