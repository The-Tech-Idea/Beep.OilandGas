using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Web.Pages.PPDM39.WorkOrder;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderInterventionDetailTests
{
    private sealed class Auth:AuthenticationStateProvider
    {
        internal ClaimsPrincipal User=new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"receiver"),new("party_id","receiver"),new(ClaimTypes.Role,"Maintenance")],"fixture"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(User));
        internal void Revoke(){User=new(new ClaimsIdentity());NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string identifier,object?[]? args)=>throw new InvalidOperationException("This detail composition has no browser.");
        public ValueTask<T> InvokeAsync<T>(string identifier,CancellationToken token,object?[]? args)=>InvokeAsync<T>(identifier,args);
    }
    private sealed class Nav:NavigationManager
    {
        public Nav(string uri)=>Initialize("http://fixture/",uri);
        protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();
    }
    [Theory][InlineData("source-valid")][InlineData("source-tenant")][InlineData("source-company")][InlineData("source-field")][InlineData("allowed")][InlineData("finance")][InlineData("finance-review")][InlineData("finance-view")][InlineData("finance-cost")][InlineData("completion-review")][InlineData("completion-decision")][InlineData("return-acceptance")][InlineData("return-status")][InlineData("closure")][InlineData("execution-handoff")][InlineData("handoff-reference")][InlineData("legacy")][InlineData("conflicting-route")][InlineData("source-failure")][InlineData("wrong-field")][InlineData("field")][InlineData("field-after-confirmed")][InlineData("company")][InlineData("signout")]
    public async Task ActualDetailComposesRecordedSourceWithInspectionAndWorkflowEvidenceWithoutMutation(string change)
    {
        const string well="well/الكويت%2F";var order=change=="legacy"?"legacy-order":"order/العالم%2F";
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"receiver",new(new("tenant","company"),["Maintenance"],change=="finance"?["Accounting.View","Accounting.PrepareWorkOrderBudget"]:change=="finance-review"?["Accounting.View","Accounting.ReviewWorkOrderBudget"]:change=="finance-view"?["Accounting.View"]:change=="finance-cost"?["Accounting.View","Accounting.RecordWorkOrderActualCost"]:change=="completion-review"?["Maintenance.VerifyCompletion"]:change=="completion-decision"?["Maintenance.VerifyCompletion","Workflow.Approve"]:change=="execution-handoff"?["Maintenance.ExecuteWork","Workflow.Execute"]:change=="closure"?["Maintenance.ReturnToService","Workflow.Execute"]:change=="return-acceptance"?["Maintenance.ReturnToService","Workflow.Approve"]:change=="return-status"?["Maintenance.ReturnToService"]:[])));
        var auth=new Auth();var id=Guid.NewGuid();var assignment=Guid.NewGuid();
        var detail=new WorkOrderDetailModel{InstanceId=order,FieldId="field",InstanceName="Recorded corrective order",Source="WORK_ORDER",WoSubType=WorkOrderSubType.Corrective,EvidenceUnavailableReason="Workflow state is unavailable",Components=[new(order,"component","field",well,"equipment")]};
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(change=="wrong-field"?"another-field":"field");
        var orders=new Mock<IWorkOrderServiceClient>();orders.Setup(value=>value.GetWorkOrderAsync(order)).ReturnsAsync(detail);orders.Setup(value=>value.GetChecklistAsync(order)).ReturnsAsync([]);
        orders.Setup(value=>value.GetProcessesAsync(order)).ReturnsAsync(new WorkOrderProcessReview(new("tenant","company"),"receiver","field",order,DateTimeOffset.UtcNow,[]));
        var access=new Mock<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>();
        access.Setup(value=>value.CheckAssetAccessAsync(It.IsAny<string>(),It.IsAny<string>(),It.IsAny<string>(),It.IsAny<CancellationToken>(),It.IsAny<string?>(),It.IsAny<string?>())).ReturnsAsync(false);
        var started=new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);var hold=new TaskCompletionSource<ProductionInterventionMaintenanceLink>(TaskCreationOptions.RunContinuationsAsynchronously);
        var production=new Mock<IProductionServiceClient>();production.Setup(value=>value.GetInterventionMaintenanceLinkAsync(well,id,order,It.IsAny<CancellationToken>())).Returns(()=>{started.TrySetResult(true);return hold.Task;});
        var handoffs=new Mock<ICompanyMaintenanceHandoffClient>();
        handoffs.Setup(value=>value.RecoverMaintenanceHandoffReferenceAsync(It.IsAny<CompanyMaintenanceRecoveryQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceRecoveryQuery query,CancellationToken _) =>
        {
            Assert.Equal(assignment,query.IntentId);Assert.Equal(id,query.DecisionId);Assert.Equal(order,query.WorkOrderId);Assert.Equal(well,query.WellId);Assert.Equal("field",query.FieldId);
            var original=new CompanyMaintenanceProcessStart(new(new("tenant","company"),new("lifecycle","fingerprint"),"receiver","revision","field"),assignment,"recorded-maintenance",new string('C',64),hold.Task.GetAwaiter().GetResult());
            return Task.FromResult(new CompanyMaintenanceRecovery(original,new(original.Context.Company,"receiver","field",assignment,original.InstanceId(),order,id,"original-step",["original-task"],new string('D',64))));
        });
        var services=new ServiceCollection();services.AddLogging();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        var environment=new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();environment.SetupGet(value=>value.EnvironmentName).Returns("Development");environment.SetupGet(value=>value.ApplicationName).Returns(typeof(WorkOrderInterventionDetailTests).Assembly.GetName().Name!);environment.SetupGet(value=>value.ContentRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());environment.SetupGet(value=>value.WebRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());services.AddSingleton(environment.Object);services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(environment.Object);
        var route=change=="legacy"?$"/ppdm39/workorder/{order}?unused=1":change=="conflicting-route"?$"/ppdm39/workorder/foreign-order?workOrderId={Uri.EscapeDataString(order)}":WorkOrderNavigation.Detail(order);
        if(change.StartsWith("source-",StringComparison.Ordinal) && change!="source-failure")route+="&sourceTenant="+(change=="source-tenant"?"other":"tenant")+"&sourceCompany="+(change=="source-company"?"other":"company")+"&sourceField="+(change=="source-field"?"other":"field");
        services.AddRazorComponents();services.AddAuthorizationCore();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton(fields.Object);services.AddSingleton(orders.Object);services.AddSingleton(access.Object);services.AddSingleton(production.Object);services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager>(new Nav("http://fixture"+route+$"&interventionId={id:D}&wellId={Uri.EscapeDataString(well)}"+(change=="handoff-reference"?$"&maintenanceIntent={assignment:D}":string.Empty)));services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        services.AddSingleton(handoffs.Object);services.AddScoped<CompanyMaintenanceHandoffState>();services.AddScoped<WorkOrderBudgetState>();services.AddScoped<WorkOrderBudgetDecisionState>();services.AddScoped<WorkOrderBudgetDecisionStatusState>();services.AddScoped<WorkOrderLaborCostState>();services.AddScoped<WorkOrderMaterialCostState>();services.AddScoped<WorkOrderCompletionReviewState>();services.AddScoped<WorkOrderCompletionDecisionState>();services.AddScoped<WorkOrderReturnHandoffState>();services.AddScoped<WorkOrderReturnAcceptanceState>();services.AddScoped<MaintenanceClosureState>();services.AddScoped<MaintenanceAssetEvidenceState>();services.AddScoped<WorkOrderExecutionHandoffState>();services.AddSingleton<ICompanyLedgerBookClient>(Mock.Of<ICompanyLedgerBookClient>());
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        var rendering=renderer.Dispatcher.InvokeAsync(()=>renderer.RenderComponentAsync<Fixtures.WorkOrderDetailRouteHost>());
        if(change is not ("wrong-field" or "conflicting-route" or "source-tenant" or "source-company" or "source-field"))
        {
            if(await Task.WhenAny(started.Task,rendering).WaitAsync(TimeSpan.FromSeconds(5))==rendering)await rendering;
            await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await renderer.Dispatcher.InvokeAsync(()=>{if(change=="company")company.Clear();if(change=="signout")auth.Revoke();if(change=="field")fields.Raise(value=>value.CurrentFieldChanged+=null!,"other-field");});
            if(change=="source-failure")hold.SetException(new HttpRequestException("Recorded source unavailable"));
            else hold.SetResult(new(new("tenant","company"),"receiver","field",order,well,id,"original-reviewer","Recorded <approval rationale>",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow));
        }
        var root=await rendering;
        await renderer.Dispatcher.InvokeAsync(()=>
        {
            if(change=="field-after-confirmed"){Assert.Contains("original-reviewer",root.ToHtmlString());fields.Raise(value=>value.CurrentFieldChanged+=null!,"other-field");}
            if(change is "source-tenant" or "source-company" or "source-field")orders.Verify(value=>value.GetWorkOrderAsync(It.IsAny<string>()),Times.Never());
            var html=root.ToHtmlString();Assert.DoesNotContain("AFE Linkage",html);Assert.DoesNotContain("Create / Link AFE",html);Assert.DoesNotContain("Budget vs Actual",html);
            if(change is "source-valid" or "allowed" or "legacy" or "handoff-reference" or "finance" or "finance-review" or "finance-view" or "finance-cost" or "completion-review" or "completion-decision" or "execution-handoff" or "return-acceptance" or "return-status" or "closure")
            {
                Assert.Contains("Recorded work-order header",html);Assert.Contains("original-reviewer",html);Assert.Contains("Recorded &lt;approval rationale&gt;",html);Assert.Contains("Recorded inspection requirements and results",html);Assert.Contains("No explicit WORK_ORDER workflow relationship",html);Assert.Contains("does not establish task assignment",html);
                if(change=="execution-handoff"){Assert.Contains("Hand recorded work to the configured independent reviewer",html);Assert.DoesNotContain("Value recorded work-order",html);}
                if(change=="closure"){Assert.Contains("Close the final maintenance return duty",html);Assert.Contains("Recover original maintenance closure",html);}
                if(change is "return-acceptance" or "return-status"){Assert.Contains("Accept observed physical return to service",html);Assert.Contains("Read recorded return acceptance outcome",html);if(change=="return-acceptance")Assert.Contains("Recover original return acceptance without another write",html);else Assert.DoesNotContain("Recover original return acceptance without another write",html);}
                if(change=="completion-decision"){Assert.Contains("Record independent completion verification or rejection",html);Assert.Contains("Review configured return approval role",html);Assert.Contains("Hand verified work to the configured return approver",html);Assert.Contains("Completion decision source: "+System.Text.Encodings.Web.HtmlEncoder.Default.Encode(order),html);Assert.DoesNotContain("Value recorded work-order",html);}
                if(change=="completion-review"){Assert.Contains("Independent completion evidence review",html);Assert.Contains("Read recorded decision outcome",html);Assert.DoesNotContain("Hand verified work to the configured return approver",html);Assert.DoesNotContain("Review configured return approval role",html);Assert.DoesNotContain("Record independent completion verification or rejection",html);Assert.DoesNotContain("Value recorded work-order",html);}
                if(change=="finance-cost"){Assert.Contains("Value recorded work-order labor",html);Assert.Contains("Value recorded work-order material",html);Assert.DoesNotContain("Work-order budget preparation",html);Assert.DoesNotContain("Independent work-order budget review",html);}
                if(change=="finance-view"){Assert.Contains("Recorded budget decision and current validity",html);Assert.DoesNotContain("Independent work-order budget review",html);Assert.DoesNotContain("Work-order budget preparation",html);}
                if(change=="finance-review"){Assert.Contains("Independent work-order budget review",html);Assert.DoesNotContain("Work-order budget preparation",html);Assert.DoesNotContain("Budget vs Actual",html);}
                if(change=="finance"){Assert.Contains("Work-order budget preparation",html);Assert.Contains("Read configured company books",html);Assert.DoesNotContain("Budget vs Actual",html);}
                if(change=="handoff-reference"){Assert.Contains(assignment.ToString("D"),html);Assert.Contains("Original receiving assignment confirmed.",html);Assert.Contains("original-task",html);}
            }
            else Assert.DoesNotContain("original-reviewer",html);
            if(change=="source-failure")Assert.Contains("could not be confirmed",html);
            if(change=="wrong-field")Assert.DoesNotContain("Recorded work-order header",html);
            if(change is "field" or "field-after-confirmed"){Assert.DoesNotContain("Recorded work-order header",html);Assert.Contains("current FIELD changed",html);}
            if(change=="conflicting-route")Assert.Contains("route and query identities must agree",html);
        });
        orders.Verify(value=>value.TransitionAsync(It.IsAny<string>(),It.IsAny<TransitionWorkOrderRequest>()),Times.Never);
        production.Verify(value=>value.RecordInterventionDecisionAsync(It.IsAny<string>(),It.IsAny<ProductionInterventionDecisionRequest>(),It.IsAny<CancellationToken>()),Times.Never);
        handoffs.Verify(value=>value.StartMaintenanceHandoffAsync(It.IsAny<CompanyMaintenanceProcessStart>(),It.IsAny<CancellationToken>()),Times.Never);
        if(change is "wrong-field" or "conflicting-route")production.Verify(value=>value.GetInterventionMaintenanceLinkAsync(It.IsAny<string>(),It.IsAny<Guid>(),It.IsAny<string>(),It.IsAny<CancellationToken>()),Times.Never);
        if(change=="conflicting-route")orders.Verify(value=>value.GetWorkOrderAsync(It.IsAny<string>()),Times.Never);
    }
}
#endif
