#if WORKFLOW_INBOX_FOCUSED
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
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
public sealed class CompanyPersonnelPolicyPanelTests
{
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"Administrator")],"app"))));}
    private sealed class Nav:NavigationManager{public Nav()=>Initialize("https://fixture/","https://fixture/admin/dashboard");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private sealed class Components:IComponentActivator{public CompanyPersonnelPolicyPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is CompanyPersonnelPolicyPanel panel)Panel=panel;return result;}}
    [Theory][InlineData("record")][InlineData("lost")][InlineData("fresh")][InlineData("actor")][InlineData("permission")][InlineData("invalid")][InlineData("employment-empty")][InlineData("positions-empty")][InlineData("restricted")]
    public async Task ActualAdministratorPolicyFormUsesBlankExplicitInputsRetainsBeforeWriteAndRecoversWithoutAnotherVersion(string mode)
    {
        var catalog=CompanyPersonnelPolicyClientTests.Catalog();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"administrator",new(catalog.Context.Company,["Administrator"],mode=="permission"?[]:["HSE.ConfigurePersonnelPolicy"])));using var state=new CompanyPersonnelPolicyState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewCompanyPersonnelPoliciesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(catalog);
        var original=new CompanyPersonnelPolicyCommand(catalog.Context,catalog.Revision,CompanyPersonnelPolicyClientTests.Draft(),1,Guid.NewGuid(),"Explicit original policy rationale");var nav=new Nav();var writes=0;var recoveries=0;
        if(mode=="fresh")nav.NavigateTo(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"personnelPolicyRecord",original.RecordId()},{"personnelPolicyIntent",original.IntentId.ToString("D")}}));
        client.Setup(c=>c.RecordCompanyPersonnelPolicyAsync(It.IsAny<CompanyPersonnelPolicyCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyPersonnelPolicyCommand command,CancellationToken _)=>{writes++;Assert.Contains("personnelPolicyRecord="+command.RecordId(),nav.Uri);Assert.Contains("personnelPolicyIntent="+command.IntentId.ToString("D"),nav.Uri);original=CompanyInventorySourceReview.Copy(command);if(mode=="lost")throw new IOException("Unconfirmed policy append");return Task.FromResult(command.Prepare(DateTime.UtcNow));});
        client.Setup(c=>c.RecoverCompanyPersonnelPolicyReferenceAsync(It.IsAny<CompanyPersonnelPolicyReference>(),It.IsAny<CancellationToken>())).Returns((CompanyPersonnelPolicyReference reference,CancellationToken _)=>{recoveries++;Assert.Equal(original.IntentId,reference.IntentId);return Task.FromResult(new CompanyPersonnelPolicyRecovery(original,original.Prepare(DateTime.UtcNow)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(client.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(mode=="actor"?"other":"administrator"));services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<CompanyPersonnelPolicyPanel>();var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            void Set(string name,object? value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            if(mode=="fresh"){await Invoke("RecoverAsync");Assert.Equal(1,recoveries);Assert.Equal(0,writes);Assert.Contains("Personnel policy recorded",root.ToHtmlString());return;}
            await Invoke("ReviewAsync");if(mode is "actor" or "permission"){Assert.Null(state.Catalog);Assert.Equal(0,writes);return;}
            Assert.Contains("No personnel policy versions",root.ToHtmlString());Assert.Contains("Accepted employment statuses",root.ToHtmlString());Assert.Equal("",panel.GetType().GetField("_employmentStatuses",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));Assert.False((bool)panel.GetType().GetField("_anyPosition",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel)!);Assert.Equal("",panel.GetType().GetField("_jurisdiction",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));var draft=CompanyPersonnelPolicyClientTests.Draft();
            Set("_duty",draft.DutyCode);Set("_orderType",draft.WorkOrderType);Set("_jurisdiction",draft.Jurisdiction);Set("_licenseType",draft.LicenseType);Set("_authority",draft.GrantingAuthority);Set("_statusType",draft.StatusType);Set("_statuses",string.Join(",",draft.AcceptedStatuses));Set("_employmentStatuses",mode=="employment-empty"?"":string.Join(",",draft.AcceptedEmploymentStatuses!));Set("_anyPosition",mode is not ("positions-empty" or "restricted"));if(mode=="restricted")Set("_employmentPositions","Recorded operator");Set("_from",mode=="invalid"?"2026-10-13":draft.EffectiveFromUtc.ToString("O"));Set("_to",draft.EffectiveToUtc.ToString("O"));Set("_version",1);Set("_source",draft.SourceReference);Set("_reason","Explicit original policy rationale");Set("_conditions",draft.Conditions.ToList());await Invoke("AppendAsync");
            if(mode is "invalid" or "employment-empty" or "positions-empty"){Assert.Equal(0,writes);Assert.Null(state.VisibleReference);return;}
            Assert.Equal(1,writes);if(mode=="lost"){Assert.Null(state.Receipt);Assert.False(state.CanReview);Assert.Contains(original.IntentId.ToString("D"),root.ToHtmlString());await Invoke("RecoverAsync");Assert.Equal(1,writes);Assert.Equal(1,recoveries);}
            original.RequireRecorded(state.Receipt!);Assert.Equal(new[]{"ACTIVE"},original.Policy.AcceptedEmploymentStatuses);if(mode=="restricted")Assert.Equal(new[]{"Recorded operator"},original.Policy.AcceptedEmploymentPositions);else Assert.Empty(original.Policy.AcceptedEmploymentPositions!);Assert.Contains("Personnel policy recorded",root.ToHtmlString());await Invoke("BeginNextAsync");Assert.True(state.CanReview);Assert.DoesNotContain("personnelPolicyIntent",nav.Uri);Assert.Equal("",panel.GetType().GetField("_duty",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));Assert.Equal(1,writes);
        });
    }
}
#endif
