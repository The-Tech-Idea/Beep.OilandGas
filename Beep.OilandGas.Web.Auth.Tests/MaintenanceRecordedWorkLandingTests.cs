using System.Security.Claims;
using System.Reflection;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.Workflow;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class MaintenanceRecordedWorkLandingTests
{
    private sealed class Components:IComponentActivator
    {
        internal MaintenanceRecordedWorkQueue? Queue;
        public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is MaintenanceRecordedWorkQueue queue)Queue=queue;return component;}
    }
    private sealed class Auth:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id","reader"),new(ClaimTypes.Role,"ReadRole")],"fixture"))));}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("/ppdm39/workorder","current")][InlineData("/production/operations","current")][InlineData("/production/engineer-workspace","current")][InlineData("/ppdm39/workorder","permission")][InlineData("/ppdm39/workorder","profile")][InlineData("/ppdm39/workorder","route")]
    [InlineData("/ppdm39/workorder","report")][InlineData("/production/operations","report")][InlineData("/production/engineer-workspace","report")]
    public async Task ActualPersonaInboxShowsRecordedReportsOnlyForItsEligibleReadWorkspace(string home,string mode)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reader",new(new("tenant","company"),["ReadRole"],mode=="permission"?[]:["Maintenance.ReturnToService"])));
        var persona=new AppPersona{Code="MAINTENANCE",Name="Maintenance",DefaultRoute=home};var personas=new Mock<IPersonaContextService>();personas.SetupGet(value=>value.CurrentPersona).Returns(persona);personas.SetupGet(value=>value.CurrentProfile).Returns(new AppUserPersona{UserId=mode=="profile"?"other":"reader",PersonaCode=persona.Code});personas.SetupGet(value=>value.Catalog).Returns([persona]);personas.Setup(value=>value.EnsureLoadedAsync()).Returns(Task.CompletedTask);
        var inbox=new Mock<IUnifiedTaskInboxService>();inbox.Setup(value=>value.GetInboxAsync(persona.Code)).ReturnsAsync(new UnifiedInbox{Company=company.Selection!.Scope,ActorUserId="reader",PersonaCode=persona.Code});var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");var reports=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        var query=new CompanyWorkOrderCompletionQuery(new(CompanyMaintenanceProcessStart.InstanceIdFor(company.Selection!.Scope,Guid.NewGuid()),"field","original-order"),"original-submission");
        reports.Setup(value=>value.ReadMaintenanceRecordedWorkPageAsync(It.IsAny<CompanyMaintenanceRecordedWorkWindow>(),1,8,It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceRecordedWorkWindow window,int page,int size,CancellationToken _)=>
        {
            reads++;var context=new FieldSelectionReview(company.Selection!.Scope,new("core","binding"),"reader","review","field");var now=DateTime.UtcNow;
            return Task.FromResult(mode=="report"?new CompanyMaintenanceRecordedWorkPage(context,window,page,size,1,[new(query,"executor","ExecutionRole","Reported work",0,now.AddMinutes(-2),now.AddMinutes(-1),new string('A',64))],now,new string('B',64)):CompanyMaintenanceRecordedWorkPage.Create(context,window,page,size,[],now));
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<MaintenanceAssetEvidenceState>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(personas.Object);services.AddSingleton(inbox.Object);services.AddSingleton(fields.Object);services.AddSingleton(reports.Object);services.AddSingleton<AuthenticationStateProvider,Auth>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<PersonaTaskInbox>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkspaceRoutes",new[]{mode=="route"?"/unrelated":home}}}));var html=root.ToHtmlString();
            if(mode is "current" or "report")
            {
                Assert.Contains("Recorded maintenance work",html);Assert.Contains("Authorized work reports in this UTC period: "+(mode=="report"?1:0),html);Assert.Equal(1,reads);
                if(mode=="report")
                {
                    var queue=((Components)provider.GetRequiredService<IComponentActivator>()).Queue!;
                    typeof(MaintenanceRecordedWorkQueue).GetMethod("ShowEvidence",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queue,[query]);
                    typeof(MaintenanceRecordedWorkQueue).GetMethod("Changed",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queue,null);await Task.Yield();
                    Assert.Contains("Selected work order original-order",root.ToHtmlString());Assert.Contains("Read recorded asset return and current asset evidence",root.ToHtmlString());
                    Assert.Contains("Pending work: 0",root.ToHtmlString());
                    Assert.DoesNotContain("Maintenance work-order assignments:",root.ToHtmlString());
                }
            }
            else{Assert.DoesNotContain("Recorded maintenance work",html);Assert.Equal(0,reads);}
        });
        reports.Verify(value=>value.ReadMaintenanceAssetReturnStatusAsync(It.IsAny<CompanyWorkOrderCompletionQuery>(),It.IsAny<CancellationToken>()),Times.Never);
        reports.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
