using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using Bunit;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.PPDM39.Models;
using Beep.OilandGas.Web.Pages.PPDM39;
using Beep.OilandGas.Web.Pages.PPDM39.Decommissioning;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class FieldEngineerWorkspaceTests
{
    [Theory][InlineData("valid",false)][InlineData("foreign",false)][InlineData("denied",false)][InlineData("permission",false)][InlineData("valid",true)][InlineData("foreign",true)][InlineData("denied",true)][InlineData("permission",true)]
    public void WorkspacesShowOnlyQualifiedFacilities(string mode,bool decommissioning)
    {
        var type=decommissioning?typeof(DecommissioningWorkspace):typeof(FieldEngineerWorkspace);var route=decommissioning?"/lifecycle/decommissioning/dashboard":"/field/dashboard";Assert.NotEmpty(type.GetCustomAttributes(typeof(AuthorizeAttribute),true));Assert.Contains(type.GetCustomAttributes(typeof(RouteAttribute),true).Cast<RouteAttribute>(),r=>r.Template==route);
        const string field="field/الكويت";using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"engineer",new(new("tenant","company"),["Engineer"],mode=="permission"?[]:["DevelopmentPlanning.View"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(x=>x.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new FieldResponse{FieldId=field,FieldName="Field name",SelectionAvailable=true,SelectionContext=new(new("tenant","company"),new("core","binding"),"engineer","revision",field)});
        var api=new Mock<IDevelopmentServiceClient>();if(mode=="denied")api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),field)).ThrowsAsync(new HttpRequestException("unavailable"));else api.Setup(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),field)).ReturnsAsync([new FACILITY{FACILITY_ID="plant",FACILITY_TYPE="PROCESSING",PRIMARY_FIELD_ID=mode=="foreign"?"other":field,FACILITY_LONG_NAME="Recorded plant"}]);
        var code=decommissioning?"DECOMMISSIONING_COORDINATOR":"FIELD_ENGINEER";var personas=new Mock<IPersonaContextService>();personas.SetupGet(x=>x.CurrentPersona).Returns(new AppPersona{Code=code,Name="Field specialist",DefaultRoute=route});personas.SetupGet(x=>x.CurrentProfile).Returns(new AppUserPersona{UserId="engineer",PersonaCode=code});personas.Setup(x=>x.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(x=>x.GetInboxAsync(code)).ReturnsAsync(new UnifiedInbox{Company=new("tenant","company"),ActorUserId="engineer",PersonaCode=code});
        using var ui=new TestContext();ui.JSInterop.Mode=JSRuntimeMode.Loose;ui.Services.AddMudServices();ui.Services.AddSingleton(company);ui.Services.AddSingleton(fields.Object);ui.Services.AddSingleton(api.Object);ui.Services.AddSingleton(personas.Object);ui.Services.AddSingleton(inbox.Object);ui.Services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        void Verify<T>() where T:IComponent
        {
        var page=ui.RenderComponent<T>();
        if(mode=="valid")
        {
            page.WaitForAssertion(()=>Assert.Contains("Recorded plant",page.Markup));Assert.Contains("do not establish equipment condition",page.Markup);var link=page.FindAll("a").Single(a=>a.TextContent.Contains("Open facility evidence")).GetAttribute("href");Assert.Contains("fieldId="+Uri.EscapeDataString(field),link);fields.Raise(x=>x.CurrentFieldChanged+=null!,"other");page.WaitForAssertion(()=>Assert.DoesNotContain("Recorded plant",page.Markup));
        }
        else if(mode=="permission"){Assert.Contains("cannot view",page.Markup);api.Verify(x=>x.GetFacilitiesAsync(It.IsAny<CancellationToken>(),It.IsAny<string>()),Times.Never);}
        else{page.WaitForAssertion(()=>Assert.Contains("Development facility evidence could not be confirmed",page.Markup));Assert.DoesNotContain("Recorded plant",page.Markup);Assert.DoesNotContain("No facility rows were returned",page.Markup);}
        if(decommissioning)Assert.Contains("does not establish an approved decommissioning program",page.Markup);
        }
        if(decommissioning)Verify<DecommissioningWorkspace>();else Verify<FieldEngineerWorkspace>();
    }
}
#endif
