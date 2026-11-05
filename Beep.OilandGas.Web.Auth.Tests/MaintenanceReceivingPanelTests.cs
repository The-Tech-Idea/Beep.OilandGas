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
public sealed class MaintenanceReceivingPanelTests
{
    private sealed class Auth(string role="MaintenanceRole"):AuthenticationStateProvider
    {
        private ClaimsPrincipal user=new(new ClaimsIdentity([new(ClaimTypes.NameIdentifier,"receiver"),new("party_id","receiver"),new(ClaimTypes.Role,role)],"fixture"));
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(user));
        internal void Revoke(){user=new(new ClaimsIdentity());NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());}
    }
    private sealed class Nav:NavigationManager
    {
        internal Nav()=>Initialize("http://fixture/","http://fixture/ppdm39/process/instances/maintenance-process-fixture");
        protected override void NavigateToCore(string uri,bool forceLoad)=>Uri=ToAbsoluteUri(uri).ToString();
        protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();
        internal void Move(string uri){Uri=ToAbsoluteUri(uri).ToString();NotifyLocationChanged(false);}
    }
    private sealed class NoBrowser:IJSRuntime
    {
        public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);
        public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);
    }
    private sealed class Components:IComponentActivator
    {
        internal CompanyMaintenanceReceiving? Panel;
        internal CompanyMaintenanceAssignedStage? AssignedStage;
        internal WorkOrderMaterialAllocationPanel? MaterialPanel;
        internal WorkOrderExecutionPermitIssuer? PermitIssuer;
        internal WorkOrderExecutionPermitApprovalReviewPanel? PermitReviewer;
        public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is CompanyMaintenanceReceiving panel)Panel=panel;if(result is CompanyMaintenanceAssignedStage stage)AssignedStage=stage;if(result is WorkOrderMaterialAllocationPanel material)MaterialPanel=material;if(result is WorkOrderExecutionPermitIssuer issuer)PermitIssuer=issuer;if(result is WorkOrderExecutionPermitApprovalReviewPanel reviewer)PermitReviewer=reviewer;return result;}
    }
    private static Task Invoke(CompanyMaintenanceReceiving panel,string method)=>(Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;
#if RECEIVING_PARENT_FOCUSED
    private sealed class MaterialReviewerAuth:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity([new("party_id","independent"),new(ClaimTypes.Role,"ConfiguredRole")],"app"))));}
    [Theory][InlineData(false)][InlineData(true)] public async Task ActualProcessRouterRestoresPrivateMaterialAcceptanceWithoutReloadingProcessOrReplacingReviewer(bool recoveryError)
    {
        var f=new Beep.OilandGas.ApiService.Tests.WorkOrderMaterialAcceptanceFixture();var task=f.Permit.Task;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(f.Permit.Context.Company,["ConfiguredRole"],["Workflow.Approve","HSE.ApprovePermit","Inventory.View"])));
        var process=new Beep.OilandGas.Models.Processes.ProcessInstance{InstanceId=task.InstanceId,ProcessId="workflow",EntityType="WORK_ORDER",EntityId=task.WorkOrderId,FieldId=task.FieldId,StartedBy="issuer",StartDate=DateTime.UtcNow,CurrentState="INITIAL",CurrentStepId="execute",Status=Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS};process.StepInstances.Add(new Beep.OilandGas.Models.Processes.ProcessStepInstance{InstanceId=task.InstanceId,StepInstanceId="received",StepId="receive",Status=Beep.OilandGas.Models.Processes.StepStatus.COMPLETED});
        var records=new Mock<IRecordedProcessClient>();var reads=0;records.Setup(c=>c.GetInstanceAsync(task.InstanceId,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult<Beep.OilandGas.Models.Processes.ProcessInstance?>(process);});records.Setup(c=>c.GetHistoryAsync(task.InstanceId,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(c=>c.GetCurrentFieldIdAsync()).ReturnsAsync("field");var workflows=new Mock<ICompanyMaintenanceHandoffClient>();workflows.Setup(c=>c.ReviewMaintenanceAssignedStageAsync(task,It.IsAny<CancellationToken>())).ReturnsAsync(f.Command.Review.PermitReview.CurrentSource.Stage);workflows.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(f.Command.Review.PermitReview);var referenceReads=0;workflows.Setup(c=>c.RecoverWorkOrderMaterialAcceptanceReferenceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAcceptanceReference query,CancellationToken _)=>{referenceReads++;Assert.Equal(f.Command.IntentId,query.IntentId);if(recoveryError)throw new HttpRequestException("Original reference unavailable");return Task.FromResult(new CompanyWorkOrderMaterialAcceptanceRecovery(f.Command,f.Command.Prepare(f.Pending,DateTime.UtcNow)));});
        var nav=new Nav();nav.Move("/ppdm39/process/instances/"+task.InstanceId);var services=new ServiceCollection();services.AddLogging();services.AddRazorComponents();services.AddAuthorizationCore();services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());services.AddMudServices();services.AddSingleton(company);services.AddSingleton<IRecordedProcessClient>(records.Object);services.AddSingleton(workflows.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider,MaterialReviewerAuth>();var materialEnvironment=new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();materialEnvironment.SetupGet(value=>value.EnvironmentName).Returns("Development");materialEnvironment.SetupGet(value=>value.ApplicationName).Returns(typeof(MaintenanceReceivingPanelTests).Assembly.GetName().Name!);materialEnvironment.SetupGet(value=>value.ContentRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());materialEnvironment.SetupGet(value=>value.WebRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());services.AddSingleton(materialEnvironment.Object);services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(materialEnvironment.Object);services.AddSingleton(Mock.Of<IWorkOrderServiceClient>());services.AddSingleton(Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());services.AddScoped<CompanyMaintenanceReceivingState>();services.AddScoped<WorkOrderExecutionPermitState>();services.AddScoped<WorkOrderMaterialAllocationState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddScoped(_=>new FieldSelectionState(company,fields.Object,(_,_,message)=>message));services.AddSingleton<NavigationManager>(nav);services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Fixtures.MaintenanceProcessRouteHost>();var components=(Components)provider.GetRequiredService<IComponentActivator>();var stage=components.AssignedStage!;var reviewer=components.PermitReviewer!;Assert.NotNull(stage);Assert.NotNull(reviewer);Assert.Equal(1,reads);
            nav.Move(nav.GetUriWithQueryParameters(new Dictionary<string,object?>{{"permitReviewId",f.Permit.IntentId},{"materialAcceptanceId",f.Command.IntentId},{"materialInspectionId",f.Command.InspectionId}}));await Task.Yield();Assert.Same(stage,components.AssignedStage);Assert.Same(reviewer,components.PermitReviewer);Assert.Equal(1,reads);Assert.Equal(1,referenceReads);if(recoveryError){Assert.Contains("outcome is unconfirmed",root.ToHtmlString());Assert.DoesNotContain(f.Command.Reason,root.ToHtmlString());}else{Assert.Contains("original material inspection acceptance is recorded",root.ToHtmlString());Assert.Contains(f.Command.Reason,root.ToHtmlString());}
            workflows.Verify(c=>c.RecordWorkOrderMaterialAcceptanceAsync(It.IsAny<CompanyWorkOrderMaterialAcceptanceCommand>(),It.IsAny<CancellationToken>()),Times.Never);
        });
    }
    [Theory][InlineData("fresh")][InlineData("submit")][InlineData("field")][InlineData("successor")][InlineData("successor-field")][InlineData("successor-auth")][InlineData("successor-empty")][InlineData("successor-stale-stage")][InlineData("successor-permit")][InlineData("successor-permit-review")][InlineData("successor-material")]
    public async Task ActualIncomingProcessRouterBindsAcceptanceReferenceAndKeepsNativeLaneOnQueryChanges(string fault)
    {
        var successor=fault.StartsWith("successor",StringComparison.Ordinal);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"receiver",new(new("tenant","company"),[successor?"SupervisorRole":"MaintenanceRole"],fault=="successor-material"?["Workflow.Approve","Inventory.View","Inventory.ReserveWorkOrderMaterial"]:fault=="successor-permit"?["Workflow.Approve","HSE.IssuePermit"]:fault=="successor-permit-review"?["Workflow.Approve","HSE.ApprovePermit"]:["Workflow.Approve"])));
        var source=new ProductionInterventionMaintenanceLink(new("tenant","company"),"receiver","field","order/العالم%2F","well/الكويت%2F",Guid.NewGuid(),"production-reviewer","Original scope",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow);
        var assignment=new CompanyMaintenanceRecoveryQuery(Guid.NewGuid(),source.DecisionId,"field",source.WellId,source.WorkOrderId);var id=CompanyMaintenanceProcessStart.InstanceIdFor(source.Company,assignment.IntentId);
        var task=new CompanyMaintenanceReceivingQuery(id,"field",source.WorkOrderId);var review=new CompanyMaintenanceAcceptanceReview(new(source.Company,new("lifecycle","fingerprint"),"receiver","revision","field"),assignment,id,"workflow",new string('C',64),new string('D',64),"step","MaintenanceRole","starter",source);
        var command=new CompanyMaintenanceAcceptanceCommand(review,Guid.NewGuid(),"Original receiving rationale");
        CompanyMaintenanceAcceptanceReceipt Receipt(CompanyMaintenanceAcceptanceCommand value)=>new(source.Company,"receiver",value.IntentId,id,"step","history","approval",null,["task"],[],new string('E',64));
        var process=new Beep.OilandGas.Models.Processes.ProcessInstance{InstanceId=id,ProcessId="workflow",EntityType="WORK_ORDER",EntityId=source.WorkOrderId,FieldId="field",StartedBy="starter",StartDate=DateTime.UtcNow,CurrentState="INITIAL",CurrentStepId="receive",Status=Beep.OilandGas.Models.Processes.ProcessStatus.IN_PROGRESS};
        var records=new Mock<IRecordedProcessClient>();var reads=0;records.Setup(value=>value.GetInstanceAsync(id,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult<Beep.OilandGas.Models.Processes.ProcessInstance?>(process);});records.Setup(value=>value.GetHistoryAsync(id,It.IsAny<CancellationToken>())).ReturnsAsync([]);
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("field");fields.Setup(value=>value.GetAvailableFieldsAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);fields.Setup(value=>value.GetCurrentFieldSelectionAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new Beep.OilandGas.Models.Data.FieldResponse{SelectionAvailable=true,FieldId="field",SelectionContext=new(source.Company,new("core","fingerprint"),"receiver","revision","field")});
        if(successor)process.StepInstances.Add(new Beep.OilandGas.Models.Processes.ProcessStepInstance{InstanceId=id,StepInstanceId="receiving-step",StepId="receive",StepName="Initial receiving",Status=Beep.OilandGas.Models.Processes.StepStatus.COMPLETED});
        var assignedReview=new CompanyMaintenanceAssignedStageReview(review.Context,task,"next-step","execute","Configured next duty","SupervisorRole","receiving-actor","receiving-history","receiving-approval",new string('C',64),new string('D',64));
        var receiving=new Mock<ICompanyMaintenanceHandoffClient>();receiving.Setup(value=>value.ReviewMaintenanceAssignedStageAsync(task,It.IsAny<CancellationToken>())).ReturnsAsync(assignedReview);receiving.Setup(value=>value.ReviewMaintenanceReceivingTaskAsync(task,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        var recordedOrder=new WorkOrderDetailModel{InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",State="",EvidenceUnavailableReason="No recorded order state",Components=[new(task.WorkOrderId,"component",task.FieldId,source.WellId,null)]};
        var condition=WorkOrderInspectionRules.Project(WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=1,CondType="PRE_START",RequiredText="Recorded isolation check",Jurisdiction="Reported authority"},task.WorkOrderId,"planner","inspection",DateTime.UtcNow));
        var inspectionStage=fault=="successor-stale-stage"?assignedReview with{ProcessRevision=new string('E',64)}:assignedReview;
        var inspectionRows=fault=="successor-empty"?Array.Empty<InspectionCondition>():new[]{condition};
        var inspectionReview=new CompanyMaintenanceInspectionReview(inspectionStage,new("core","fingerprint"),recordedOrder,inspectionRows,CompanyMaintenanceInspectionReview.Revision(inspectionStage,new("core","fingerprint"),recordedOrder,inspectionRows),DateTimeOffset.UtcNow);
        receiving.Setup(value=>value.ReviewMaintenanceAssignedInspectionsAsync(task,It.IsAny<CancellationToken>())).ReturnsAsync(inspectionReview);
        var issueReview=new CompanyWorkOrderExecutionPermitIssueReview(assignedReview.Context,inspectionReview);var issue= new CompanyWorkOrderExecutionPermitIntent(issueReview.Context,task,Guid.NewGuid(),inspectionReview.SnapshotRevision,"ElectricalIsolation","Reported jurisdiction","Original scope","isolation","risk","personnel","material",DateTime.UtcNow,DateTime.UtcNow.AddHours(8));var privateReads=0;
        receiving.Setup(value=>value.RecoverWorkOrderExecutionPermitReferenceAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitReference query,CancellationToken _)=>{privateReads++;Assert.Equal(task,query.Task);Assert.Equal(issue.IntentId,query.IntentId);return Task.FromResult(new CompanyWorkOrderExecutionPermitRecovery(issue,WorkOrderExecutionPermitStateTests.Receipt(issue)));});
        var issuerStage=assignedReview with{Context=assignedReview.Context with{ActorUserId="independent-issuer"}};var issuerSource=inspectionReview with{Stage=issuerStage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(issuerStage,inspectionReview.InspectionDatabase,inspectionReview.Order,inspectionReview.Inspections)};var independentProposal=issue with{Context=issue.Context with{ActorUserId="independent-issuer",Database=inspectionReview.InspectionDatabase},InspectionRevision=issuerSource.SnapshotRevision};var reviewUtc=issue.ValidFromUtc.AddMinutes(1);var approvalReview=new CompanyWorkOrderExecutionPermitApprovalReview(assignedReview.Context with{Database=inspectionReview.InspectionDatabase},independentProposal,issuerSource,inspectionReview,new string('F',64),reviewUtc,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(independentProposal,issuerSource,inspectionReview,reviewUtc));var approvalReads=0;
        receiving.Setup(value=>value.ReviewWorkOrderExecutionPermitApprovalAsync(It.IsAny<CompanyWorkOrderExecutionPermitReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitReference query,CancellationToken _)=>{approvalReads++;Assert.Equal(task,query.Task);Assert.Equal(issue.IntentId,query.IntentId);return Task.FromResult(approvalReview);});
        var materialSource=WorkOrderMaterialSourceTests.Source();var item=materialSource.Inventory.Item;var inventory=CompanyInventorySourceReview.Create(source.Company,"receiver",item,[]);var materialQuery=new CompanyWorkOrderMaterialSourceQuery(task,item.INVENTORY_ITEM_ID,2m,"EA");var materials=new CompanyWorkOrderMaterialSourceReview(review.Context with{Database=new("production","fingerprint")},materialQuery,inspectionReview,inventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(materialQuery,inventory));var materialUtc=DateTime.UtcNow;var materialReview=CompanyWorkOrderMaterialAllocationReview.Create(materials,[],materialUtc);var materialCommand=new CompanyWorkOrderMaterialAllocationCommand(materialReview,Guid.NewGuid(),materialUtc,materialUtc.AddHours(1),"Original material requirement");var materialReads=0;
        receiving.Setup(c=>c.RecoverWorkOrderMaterialAllocationReferenceAsync(It.IsAny<CompanyWorkOrderMaterialAllocationReferenceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderMaterialAllocationReferenceQuery query,CancellationToken _)=>{materialReads++;Assert.Equal(task,query.Task);Assert.Equal(materialCommand.IntentId,query.IntentId);return Task.FromResult(new CompanyWorkOrderMaterialAllocationRecovery(materialCommand,materialCommand.Prepare(materialUtc)));});
        var nav=new Nav();nav.Move("http://fixture/ppdm39/process/instances/"+id+(fault=="fresh"?$"?acceptanceId={command.IntentId:D}":string.Empty));var writes=0;
        receiving.Setup(value=>value.AcceptMaintenanceAsync(It.IsAny<CompanyMaintenanceAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAcceptanceCommand original,CancellationToken _)=>{writes++;Assert.Contains("acceptanceId="+original.IntentId.ToString("D"),nav.Uri);return Task.FromResult(Receipt(original));});
        receiving.Setup(value=>value.RecoverMaintenanceAcceptanceReferenceAsync(It.IsAny<CompanyMaintenanceAcceptanceReferenceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAcceptanceReferenceQuery query,CancellationToken _)=>{Assert.Equal(task,query.Task);Assert.Equal(command.IntentId,query.IntentId);return Task.FromResult(new CompanyMaintenanceAcceptanceRecovery(command,Receipt(command)));});
        var services=new ServiceCollection();services.AddLogging();services.AddRazorComponents();services.AddAuthorizationCore();services.AddMudServices();services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build());
        var environment=new Mock<Microsoft.AspNetCore.Hosting.IWebHostEnvironment>();environment.SetupGet(value=>value.EnvironmentName).Returns("Development");environment.SetupGet(value=>value.ApplicationName).Returns(typeof(MaintenanceReceivingPanelTests).Assembly.GetName().Name!);environment.SetupGet(value=>value.ContentRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());environment.SetupGet(value=>value.WebRootFileProvider).Returns(new Microsoft.Extensions.FileProviders.NullFileProvider());services.AddSingleton(environment.Object);services.AddSingleton<Microsoft.Extensions.Hosting.IHostEnvironment>(environment.Object);
        services.AddSingleton(Mock.Of<IWorkOrderServiceClient>());services.AddSingleton(Mock.Of<Beep.OilandGas.Client.App.Services.AccessControl.IAccessControlService>());services.AddSingleton(company);services.AddSingleton<AuthenticationStateProvider>(new Auth(successor?"SupervisorRole":"MaintenanceRole"));services.AddSingleton(records.Object);services.AddSingleton(receiving.Object);services.AddSingleton(fields.Object);services.AddScoped<CompanyMaintenanceReceivingState>();services.AddScoped<WorkOrderExecutionPermitState>();services.AddScoped<WorkOrderMaterialAllocationState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddScoped(_=>new FieldSelectionState(company,fields.Object,(_,_,message)=>message));services.AddSingleton<NavigationManager>(nav);services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<Fixtures.MaintenanceProcessRouteHost>();
            if(successor)
            {
                var components=(Components)provider.GetRequiredService<IComponentActivator>();Assert.Null(components.Panel);var stage=components.AssignedStage!;Assert.NotNull(stage);
                Assert.Equal(fault=="successor-permit",components.PermitIssuer is not null);Assert.Equal(fault=="successor-permit-review",components.PermitReviewer is not null);
                await EventCallback.Factory.Create(stage,(Func<Task>)(()=> (Task)stage.GetType().GetMethod("ReviewAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(stage,null)!)).InvokeAsync();
                var html=root.ToHtmlString();Assert.Contains("Recorded next maintenance duty",html);Assert.Contains("Assigned role: SupervisorRole",html);Assert.Contains("receiving-history",html);Assert.Contains("/ppdm39/workorder/detail?workOrderId=order%2F",html);Assert.DoesNotContain("Legacy transitions are outside",html);Assert.DoesNotContain("Reviewed actions",html);Assert.Equal(0,writes);
                await EventCallback.Factory.Create(stage,(Func<Task>)(()=> (Task)stage.GetType().GetMethod("ReviewInspectionsAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(stage,null)!)).InvokeAsync();
                html=root.ToHtmlString();if(fault=="successor-stale-stage"){Assert.DoesNotContain("Recorded isolation check",html);Assert.Contains("Inspection prerequisites could not be confirmed",html);}else if(fault=="successor-empty"){Assert.Contains("No recorded inspection requirements",html);Assert.Contains("does not confirm",html);}else{Assert.Contains("Recorded isolation check",html);Assert.Contains("UNRECORDED",html);}Assert.DoesNotContain("Record result</",html);Assert.DoesNotContain("Save requirement",html);
                if(fault=="successor-field"){fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("foreign-field");fields.Raise(value=>value.CurrentFieldChanged+=null!,"foreign-field");}
                if(fault=="successor-auth")((Auth)provider.GetRequiredService<AuthenticationStateProvider>()).Revoke();
                if(fault is "successor-field" or "successor-auth"){await System.Threading.Tasks.Task.Yield();Assert.DoesNotContain("receiving-history",root.ToHtmlString());Assert.DoesNotContain("Recorded isolation check",root.ToHtmlString());}
                if(fault=="successor-material")
                {
                    var reservationPanel=components.MaterialPanel!;Assert.NotNull(reservationPanel);nav.Move(nav.GetUriWithQueryParameter("materialAllocationId",materialCommand.IntentId));await System.Threading.Tasks.Task.Yield();Assert.Same(stage,components.AssignedStage);Assert.Same(reservationPanel,components.MaterialPanel);Assert.Equal(1,reads);Assert.Equal(1,materialReads);Assert.Contains(materialCommand.AllocationId(),root.ToHtmlString());Assert.Contains("Original material requirement",root.ToHtmlString());Assert.Equal(0,writes);
                    receiving.Verify(c=>c.RecordWorkOrderMaterialAllocationAsync(It.IsAny<CompanyWorkOrderMaterialAllocationCommand>(),It.IsAny<CancellationToken>()),Times.Never);
                }
                if(fault=="successor-permit")
                {
                    var originalStage=stage;var issuer=components.PermitIssuer!;Assert.NotNull(issuer);nav.Move(nav.GetUriWithQueryParameter("permitIssueId",issue.IntentId));await System.Threading.Tasks.Task.Yield();
                    Assert.Same(originalStage,components.AssignedStage);Assert.Same(issuer,components.PermitIssuer);Assert.Equal(1,reads);Assert.Equal(1,privateReads);Assert.Contains(issue.PermitId(),root.ToHtmlString());Assert.Equal(0,writes);
                }
                if(fault=="successor-permit-review")
                {
                    var reviewer=components.PermitReviewer!;Assert.NotNull(reviewer);nav.Move(nav.GetUriWithQueryParameter("permitReviewId",issue.IntentId));await System.Threading.Tasks.Task.Yield();Assert.Same(stage,components.AssignedStage);Assert.Same(reviewer,components.PermitReviewer);Assert.Equal(1,reads);Assert.Equal(1,approvalReads);Assert.Equal(0,privateReads);Assert.Contains("Independent reviewer: receiver",root.ToHtmlString());Assert.Contains("Original issue-source facts",root.ToHtmlString());Assert.Equal(0,writes);
                    var decisions=0;CompanyWorkOrderExecutionPermitDecision? retained=null;
                    receiving.Setup(value=>value.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecision decision,CancellationToken _)=>{retained=decision;decisions++;Assert.Contains("permitDecisionId="+decision.IntentId.ToString("D"),nav.Uri);nav.Move(nav.Uri);return Task.FromResult(new CompanyWorkOrderExecutionPermitDecisionReceipt(source.Company,"receiver",decision.IntentId,independentProposal.PermitId(),"Reject",new string('F',64)));});
                    reviewer.GetType().GetField("_rationale",BindingFlags.NonPublic|BindingFlags.Instance)!.SetValue(reviewer,"Original independent rationale");await EventCallback.Factory.Create(reviewer,(Func<Task>)(()=> (Task)reviewer.GetType().GetMethod("RejectAsync",BindingFlags.NonPublic|BindingFlags.Instance)!.Invoke(reviewer,null)!)).InvokeAsync();
                    Assert.NotNull(retained);Assert.Equal(1,decisions);Assert.Equal(1,reads);Assert.Same(stage,components.AssignedStage);Assert.Same(reviewer,components.PermitReviewer);Assert.Contains("Recorded decision: Reject",root.ToHtmlString());Assert.Equal(0,privateReads);
                }
                return;
            }
            var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel!;
            Assert.NotNull(panel);Assert.DoesNotContain("Legacy transitions are outside",root.ToHtmlString());Assert.DoesNotContain("Reviewed actions",root.ToHtmlString());Assert.Equal(1,reads);
            if(fault=="fresh"){Assert.Contains(command.IntentId.ToString("D"),root.ToHtmlString());Assert.Contains("Independent receiving acceptance confirmed.",root.ToHtmlString());Assert.Equal(0,writes);}
            else
            {
                await Invoke(panel,"ReviewAsync");panel.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,"Original receiving rationale");await Invoke(panel,"AcceptAsync");Assert.Equal(1,writes);
                nav.Move(nav.Uri);await System.Threading.Tasks.Task.Yield();Assert.Equal(1,reads);Assert.Contains("Independent receiving acceptance confirmed.",root.ToHtmlString());
                if(fault=="field"){fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync("other-field");fields.Raise(value=>value.CurrentFieldChanged+=null!,"other-field");await System.Threading.Tasks.Task.Yield();Assert.DoesNotContain("Independent receiving acceptance confirmed.",root.ToHtmlString());}
            }
        });
    }
#endif
    [Theory][InlineData("allowed")][InlineData("unknown")][InlineData("fresh")][InlineData("missing")][InlineData("company")][InlineData("signout")]
    public async Task ActualReceivingPanelRetainsAcceptanceBeforeTransportAndRestoresReadOnlyAfterReconnect(string fault)
    {
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"receiver",new(new("tenant","company"),["MaintenanceRole"],["Workflow.Approve"])));
        var source=new ProductionInterventionMaintenanceLink(new("tenant","company"),"receiver","field","order/العالم%2F","well/الكويت%2F",Guid.NewGuid(),"production-reviewer","Original approved scope",DateTime.UtcNow,new string('A',64),new string('B',64),DateTimeOffset.UtcNow);
        var assignment=new CompanyMaintenanceRecoveryQuery(Guid.NewGuid(),source.DecisionId,"field",source.WellId,source.WorkOrderId);var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(source.Company,assignment.IntentId),"field",source.WorkOrderId);
        var review=new CompanyMaintenanceAcceptanceReview(new(source.Company,new("lifecycle","fingerprint"),"receiver","revision","field"),assignment,task.InstanceId,"configured-workflow",new string('C',64),new string('D',64),"receiving-step","MaintenanceRole","starter",source);
        CompanyMaintenanceAcceptanceCommand? original=fault is "fresh" or "missing"?new(review,Guid.NewGuid(),"Original receiving rationale"):null;
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var auth=new Auth();var nav=new Nav();var writes=0;
        CompanyMaintenanceAcceptanceReceipt Receipt(CompanyMaintenanceAcceptanceCommand command)=>new(source.Company,"receiver",command.IntentId,task.InstanceId,"receiving-step","recorded-history","recorded-approval",null,["closed-task"],[],new string('E',64));
        client.Setup(value=>value.ReviewMaintenanceReceivingTaskAsync(task,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(value=>value.AcceptMaintenanceAsync(It.IsAny<CompanyMaintenanceAcceptanceCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAcceptanceCommand command,CancellationToken _) =>
        {
            Assert.Contains("acceptanceId="+command.IntentId.ToString("D"),nav.Uri);original=command;writes++;
            return fault=="unknown"?Task.FromException<CompanyMaintenanceAcceptanceReceipt>(new HttpRequestException("Unknown commit")):Task.FromResult(Receipt(command));
        });
        client.Setup(value=>value.RecoverMaintenanceAcceptanceReferenceAsync(It.IsAny<CompanyMaintenanceAcceptanceReferenceQuery>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceAcceptanceReferenceQuery query,CancellationToken _) =>
        {
            Assert.NotNull(original);Assert.Equal(original.IntentId,query.IntentId);Assert.Equal(task,query.Task);
            return fault=="missing"?Task.FromException<CompanyMaintenanceAcceptanceRecovery>(new InvalidOperationException("Original acceptance missing")):Task.FromResult(new CompanyMaintenanceAcceptanceRecovery(original,Receipt(original)));
        });
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton(client.Object);services.AddScoped<CompanyMaintenanceReceivingState>();services.AddScoped<WorkOrderExecutionPermitState>();services.AddScoped<WorkOrderMaterialAllocationState>();services.AddScoped<WorkOrderExecutionPermitDecisionState>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<NavigationManager>(nav);services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));services.AddSingleton<IComponentActivator,Components>();
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<CompanyMaintenanceReceiving>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Task",task},{"SourceRevision","source-revision"},{"AcceptanceReference",original?.IntentId}}));
            var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel!;
            if(fault is "fresh" or "missing")
            {
                Assert.Equal(0,writes);Assert.Contains(original!.IntentId.ToString("D"),root.ToHtmlString());Assert.Equal(fault=="fresh",root.ToHtmlString().Contains("Independent receiving acceptance confirmed.",StringComparison.Ordinal));return;
            }
            await Invoke(panel,"ReviewAsync");Assert.Contains("MaintenanceRole",root.ToHtmlString());Assert.Contains("production-reviewer",root.ToHtmlString());Assert.Contains("Original approved scope",root.ToHtmlString());
            panel.GetType().GetField("_notes",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,"Original receiving rationale");await Invoke(panel,"AcceptAsync");Assert.Equal(1,writes);
            if(fault=="unknown"){Assert.Contains("Receiving acceptance is unconfirmed",root.ToHtmlString());await Invoke(panel,"AcceptAsync");Assert.Equal(1,writes);await Invoke(panel,"RecoverAsync");Assert.Equal(1,writes);}
            Assert.Contains("Independent receiving acceptance confirmed.",root.ToHtmlString());Assert.Contains("recorded-approval",root.ToHtmlString());Assert.Contains("closed-task",root.ToHtmlString());
            if(fault=="company")company.Clear();if(fault=="signout")auth.Revoke();
            if(fault is "company" or "signout"){Assert.DoesNotContain("Independent receiving acceptance confirmed.",root.ToHtmlString());Assert.DoesNotContain(original!.IntentId.ToString("D"),root.ToHtmlString());}
            Assert.Contains("accepted return to service require separate evidence",root.ToHtmlString());
        });
    }
}
#endif
