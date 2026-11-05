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
public sealed class CompanyMaintenanceReturnPolicyPanelTests
{
    private sealed class Auth(string actor,string role):AuthenticationStateProvider{private string currentActor=actor;public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id",currentActor),new(ClaimTypes.Role,role)],"app"))));public void ChangeActor(string value){currentActor=value;NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}}
    private sealed class Nav:NavigationManager{public Nav()=>Initialize("https://fixture/","https://fixture/admin/dashboard");protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Js:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private sealed class Components:IComponentActivator{public CompanyMaintenanceReturnPolicyPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is CompanyMaintenanceReturnPolicyPanel panel)Panel=panel;return result;}}
    [Theory][InlineData("record")][InlineData("lost")][InlineData("fresh")][InlineData("actor")][InlineData("permission")][InlineData("standard-role")][InlineData("invalid-date")][InlineData("missing-transition")][InlineData("zero-condition")][InlineData("late-auth")][InlineData("preserve-facet")][InlineData("area-nonapplicable")]
    public async Task ActualAdministratorReturnPolicyFormRetainsOriginalReferenceUsesExplicitChoicesAndRecovers(string mode)
    {
        var catalog=CompanyMaintenanceReturnPolicyClientTests.Catalog();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"administrator",new(catalog.Context.Company,["Administrator"],mode=="permission"?[]:["Maintenance.ConfigureReturnPolicy"])));using var state=new CompanyMaintenanceReturnPolicyState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewCompanyMaintenanceReturnPoliciesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(catalog);
        var original=new CompanyMaintenanceReturnPolicyCommand(catalog.Context,catalog.Revision,CompanyMaintenanceReturnPolicyClientTests.Draft(),1,Guid.NewGuid(),"Explicit original return policy rationale");var nav=new Nav();var writes=0;var recoveries=0;
        if(mode=="fresh")nav.NavigateTo(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"maintenancePolicyRecord",original.RecordId()},{"maintenancePolicyIntent",original.IntentId.ToString("D")}}));
        client.Setup(c=>c.RecordCompanyMaintenanceReturnPolicyAsync(It.IsAny<CompanyMaintenanceReturnPolicyCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceReturnPolicyCommand command,CancellationToken _)=>{writes++;Assert.Contains("maintenancePolicyRecord="+command.RecordId(),nav.Uri);Assert.Contains("maintenancePolicyIntent="+command.IntentId.ToString("D"),nav.Uri);original=CompanyInventorySourceReview.Copy(command);if(mode is "lost" or "late-auth")throw new IOException("Unconfirmed return policy append");return Task.FromResult(command.Prepare(DateTime.UtcNow));});
        client.Setup(c=>c.RecoverCompanyMaintenanceReturnPolicyReferenceAsync(It.IsAny<CompanyMaintenanceReturnPolicyReference>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceReturnPolicyReference reference,CancellationToken _)=>{recoveries++;Assert.Equal(original.IntentId,reference.IntentId);return Task.FromResult(new CompanyMaintenanceReturnPolicyRecovery(original,original.Prepare(DateTime.UtcNow)));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton<IJSRuntime,Js>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(company);services.AddSingleton(state);services.AddSingleton(client.Object);var auth=new Auth(mode=="actor"?"other":"administrator",mode=="standard-role"?"Other":"Administrator");services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<CompanyMaintenanceReturnPolicyPanel>();var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            void Set(string name,object? value)=>panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(panel,value);
            List<T> List<T>(string name)=>(List<T>)panel.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel)!;
            Task Invoke(string name)=>EventCallback.Factory.Create(panel,(Func<Task>)(()=> (Task)panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null)!)).InvokeAsync();
            Task Edit(string name)=>EventCallback.Factory.Create(panel,(Action)(()=>panel.GetType().GetMethod(name,BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(panel,null))).InvokeAsync();
            if(mode=="fresh"){await Invoke("RecoverAsync");Assert.Equal(1,recoveries);Assert.Equal(0,writes);Assert.Contains("Return-policy version recorded",root.ToHtmlString());return;}
            await Invoke("ReviewAsync");if(mode is "actor" or "permission" or "standard-role"){Assert.Null(state.Catalog);Assert.Equal(0,writes);return;}
            Assert.Contains("No recorded return-policy versions",root.ToHtmlString());Assert.Equal("",panel.GetType().GetField("_jurisdiction",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));Assert.Equal("",panel.GetType().GetField("_target",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));Assert.Null(panel.GetType().GetField("_value",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));var draft=CompanyMaintenanceReturnPolicyClientTests.Draft();
            Set("_areaDecision",mode=="area-nonapplicable"?"NOT_APPLICABLE":"APPLIES");Set("_orderType",draft.WorkOrderType);Set("_facilityType",draft.FacilityType);Set("_jurisdiction",draft.Jurisdiction);Set("_areaId",draft.AreaId);Set("_areaType",draft.AreaType);Set("_roles",string.Join("\n",draft.AuthorizedReturnRoles));Set("_from",mode=="invalid-date"?"2026-10-13":draft.EffectiveFromUtc.ToString("O"));Set("_to",draft.EffectiveToUtc.ToString("O"));Set("_version",1);Set("_source",draft.SourceReference);Set("_reason","Explicit original return policy rationale");
            if(mode is not ("missing-transition" or "area-nonapplicable"))List<CompanyMaintenanceReturnTransition>("_transitions").AddRange(draft.Transitions);
            if(mode=="preserve-facet"){Set("_assetType","FACILITY");Set("_disposition","PRESERVE");Set("_transitionType","NativeOtherFacet");Set("_sourceValues","KNOWN");Set("_transitionSource","Reviewed preservation");await Edit("AddTransition");Assert.Equal("PRESERVE",List<CompanyMaintenanceReturnTransition>("_transitions").Last().Disposition);}
            if(mode=="zero-condition")
            {
                var license=draft.Licenses[0];var condition=license.Conditions[0];var status=license.Statuses[0];var violation=license.Violations[0];
                Set("_issuer",license.IssuerBaId);Set("_licenseType",license.LicenseType);Set("_applicability",license.Applicability);Set("_fees",license.FeesDecision);Set("_licenseSource",license.SourceReference);
                Set("_licenseStatusType",status.StatusType);Set("_licenseStatuses",string.Join("\n",status.AcceptedValues));Set("_licenseStatusSource",status.SourceReference);await Edit("AddLicenseStatus");
                Set("_conditionType",condition.ConditionType);Set("_conditionCode",condition.ConditionCode);Set("_conditionDecision",condition.Decision);Set("_fulfilledBy","fulfilment-authority");Set("_dueCondition",condition.DueCondition);Set("_valueDecision",condition.Value.Decision);Set("_value",condition.Value.Value);Set("_valueUnit",condition.Value.UnitId);Set("_valueSource",condition.Value.SourceReference);Set("_termDecision",condition.DueTerm.Decision);Set("_termSource",condition.DueTerm.SourceReference);Set("_restrictionDecision",condition.RestrictionDecision);Set("_conditionSource",condition.SourceReference);await Edit("AddCondition");Assert.Equal(0m,Assert.Single(List<CompanyMaintenanceReturnConditionRule>("_conditions")).Value.Value);
                Set("_violationType",violation.ViolationType);Set("_resolutions",string.Join("\n",violation.AcceptedResolutionTypes));Set("_violationSource",violation.SourceReference);await Edit("AddViolation");await Edit("AddLicense");Assert.Single(List<CompanyMaintenanceReturnLicenseRule>("_licenses"));
            }
            else if(mode!="area-nonapplicable")List<CompanyMaintenanceReturnLicenseRule>("_licenses").AddRange(draft.Licenses);
            await Invoke("AppendAsync");if(mode is "invalid-date" or "missing-transition"){Assert.Equal(0,writes);Assert.Null(state.VisibleReference);return;}
            Assert.Equal(1,writes);if(mode=="late-auth"){auth.ChangeActor("other-account");await Task.Yield();Assert.Null(state.VisibleReference);Assert.Null(state.VisibleCommand);Assert.DoesNotContain(original.IntentId.ToString("D"),root.ToHtmlString());await Invoke("RecoverAsync");Assert.Equal(0,recoveries);Assert.Equal(1,writes);return;}if(mode=="lost"){Assert.Null(state.Receipt);Assert.False(state.CanReview);Assert.Contains(original.IntentId.ToString("D"),root.ToHtmlString());await Invoke("RecoverAsync");Assert.Equal(1,writes);Assert.Equal(1,recoveries);}
            original.RequireRecorded(state.Receipt!);Assert.Equal(new[]{"FacilityRole"},original.Policy.AuthorizedReturnRoles);if(mode=="area-nonapplicable"){Assert.Equal("NOT_APPLICABLE",original.Policy.AreaApplicability);Assert.Empty(original.Policy.Transitions);Assert.Empty(original.Policy.Licenses);}else Assert.Equal(0m,original.Policy.Licenses[0].Conditions[0].Value.Value);Assert.Contains("Return-policy version recorded",root.ToHtmlString());await Invoke("BeginNextAsync");Assert.True(state.CanReview);Assert.DoesNotContain("maintenancePolicyIntent",nav.Uri);Assert.Equal("",panel.GetType().GetField("_jurisdiction",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(panel));Assert.Equal(1,writes);
        });
    }
}
#endif
