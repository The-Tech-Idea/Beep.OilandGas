using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Beep.OilandGas.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderCompletionReviewTests
{
    internal static CompanyWorkOrderCompletionReview Review(string mode="current",bool assetReturn=false)
    {
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(new("tenant","company"),Guid.NewGuid()),"field","order");
        WorkOrderDetailModel? assetOrder=assetReturn?new(){InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",State="",EvidenceUnavailableReason="No header execution state",WoSubType="CORRECTIVE",Components=[new(task.WorkOrderId,"component",task.FieldId,"original-well",""){NativeRevision=new string('A',64)},new(task.WorkOrderId,"isolation-component",task.FieldId,"",""){InformationItemId="ISOLATION-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('B',64)},new(task.WorkOrderId,"risk-component",task.FieldId,"",""){InformationItemId="RISK-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('C',64)}]}:null;
        var f=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(originalOrder:assetOrder,taskOverride:assetReturn?task:null);var stored=f.Command.Prepare(DateTime.UtcNow);var report=CompanyWorkOrderSubmissionFacts.From(stored);
        var context=f.Command.Review.Inspections.Current.Context with{ActorUserId="completion-reviewer",Revision="current-review-authority"};var utc=DateTime.UtcNow;
        var row=WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=context.FieldId!,CondSeq=999,CondType="CLOSE_OUT",ApplicableStage="COMPLETION_VERIFICATION",RequiredText="Observe actual completion",Jurisdiction="Recorded jurisdiction"},report.Query.Task.WorkOrderId,"requirement-maker","completion-check",report.CompletedAtUtc.AddDays(-1));
        var check=WorkOrderInspectionRules.ProjectWithRequirementProof(row) with{Status=mode=="unaccepted"?"UNRECORDED":"PASS",ResultText="Actual independent observation",Inspector=mode=="executor-inspector"?report.ExecutorUserId:context.ActorUserId,InspectDate=mode=="early"?report.CompletedAtUtc.AddSeconds(-1):utc,SourceRevision=new string('C',64)};
        if(mode=="unaccepted")check=WorkOrderInspectionRules.ProjectWithRequirementProof(row);
        var requirements=f.Command.Review.Inspections.Current.Requirements.Append(check).ToArray();var order=CompanyInventorySourceReview.Copy(f.Command.Review.Inspections.Current.Order);
        if(mode=="inspection-drift")requirements=requirements.Select(value=>value.ApplicableStage=="WORK_SUBMISSION"?value with{ResultText="Changed recorded operational result"}:value).ToArray();
        report=report with{OriginalRequirementPopulation=CompanyWorkOrderExecutionRequirements.Population(context.FieldId!,order.InstanceId,requirements)};
        if(mode=="defects")report=report with{OutstandingDefects=["Recorded unresolved defect"]};if(mode=="scope")order.Instructions="Changed submitted work scope";
        var personId=mode=="same-person"?report.EmployeeBaId:"reviewer-native-person";
        var person=new CompanyNativePersonFacts(context.Company,personId,"Recorded reviewer","PERSON",utc.AddDays(-1),utc.AddDays(1),new(context.Company.BusinessAssociateId,personId,1m,"Reported role","ACTIVE",utc.AddDays(-1),utc.AddDays(1),new string('D',64)),new string('E',64));
        var link=new CompanyPersonnelIdentityVersion(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),context.ActorUserId,personId,"PERSON",1,utc.AddDays(-1),utc.AddDays(1),"Explicit independent identity source",mode=="self-map"?context.ActorUserId:"identity-administrator",utc,new string('F',64));
        var identities=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(context))),mode=="missing-map"?[]:[link],DateTime.UtcNow);
        var comparison=DateTime.UtcNow;var result=new CompanyWorkOrderCompletionReview(context,report,order,requirements,identities,mode=="missing-map"?null:person,comparison,CompanyWorkOrderCompletionReview.GetBlockers(context,report,order,requirements,identities,mode=="missing-map"?null:person,comparison));result.Require();return result;
    }
    [Theory][InlineData("current")][InlineData("unaccepted")][InlineData("early")][InlineData("defects")][InlineData("scope")][InlineData("same-person")][InlineData("missing-map")][InlineData("self-map")][InlineData("executor-inspector")][InlineData("inspection-drift")]
    public void CompletionComparisonDoesNotInferAcceptedWorkIndependentPeopleOrRecordedVerification(string mode)
    {
        if(mode=="self-map"){Assert.Throws<InvalidOperationException>(()=>Review(mode));return;}
        var review=Review(mode);Assert.Contains(CompanyWorkOrderCompletionReview.PendingVerification,review.Blockers);
        if(mode=="current")Assert.Single(review.Blockers);else Assert.True(review.Blockers.Count>1);Assert.DoesNotContain("Reason",JsonSerializer.Serialize(review.Report));
    }
    private static CompanyClientContext Company(CompanyWorkOrderCompletionReview review,string[]? permissions=null)
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["SupervisorRole"],permissions??["Maintenance.VerifyCompletion"])));return company;}
    [Theory][InlineData("current")][InlineData("late-company")][InlineData("late-field")][InlineData("permission")]
    public async Task ReadOnlyCompletionStateFencesCurrentReviewerFieldAndCapability(string mode)
    {
        var review=Review();using var company=Company(review,mode=="permission"?[]:null);using var state=new WorkOrderCompletionReviewState(company);state.Attach(review.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewWorkOrderCompletionAsync(review.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="late-company")company.Clear();if(mode=="late-field")state.Clear();return Task.FromResult(review);});
        var error=await Record.ExceptionAsync(()=>state.ReadAsync(client.Object));if(mode=="current"){Assert.Null(error);Assert.NotNull(state.Current);}else{Assert.Null(state.Current);if(mode=="permission")Assert.IsType<UnauthorizedAccessException>(error);else Assert.Null(error);}
        client.Verify(value=>value.SubmitWorkOrderAsync(It.IsAny<CompanyWorkOrderSubmissionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Response(CompanyWorkOrderCompletionReview review,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/completion-review",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("late-company")]
    public async Task TypedCompletionReadRequiresDedicatedAuthorityAndRejectsLateReplies(string mode)
    {
        var review=Review();using var company=Company(review,mode=="permission"?[]:null);var response=new Response(review,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(()=>client.ReviewWorkOrderCompletionAsync(review.Report.Query));
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"SupervisorRole")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderCompletionReviewPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderCompletionReviewPanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("current")][InlineData("defects")][InlineData("same-person")][InlineData("late-company")][InlineData("field")][InlineData("permission")][InlineData("discover")][InlineData("discover-late-company")][InlineData("discover-late-input")]
    public async Task ActualReviewerPanelShowsRecordedWorkAndBlockersWithoutRecordingCompletion(string mode)
    {
        var review=Review(mode);using var company=Company(review,mode=="permission"?[]:null);var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":review.Context.FieldId);
        var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;client.Setup(value=>value.ReviewWorkOrderCompletionAsync(review.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="late-company")company.Clear();return Task.FromResult(review);});
        WorkOrderCompletionReviewPanel? captured=null;
        client.Setup(value=>value.DiscoverWorkOrderSubmissionAsync(review.Report.Query.Task,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="discover-late-company")company.Clear();if(mode=="discover-late-input")captured!.GetType().GetMethod("InputsChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(captured,null);return Task.FromResult(review.Report.Query);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderCompletionReviewState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=new Dictionary<string,object?>{{"WorkOrderId",review.Report.Query.Task.WorkOrderId},{"FieldId",review.Context.FieldId}};if(mode.StartsWith("discover"))parameters["OriginalTask"]=review.Report.Query.Task;
            var root=await renderer.RenderComponentAsync<WorkOrderCompletionReviewPanel>(ParameterView.FromDictionary(parameters));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;captured=panel;
            panel.GetType().GetField("_process",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,review.Report.Query.Task.InstanceId);panel.GetType().GetField("_submission",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,review.Report.Query.SubmissionId);
            await (Task)panel.GetType().GetMethod(mode.StartsWith("discover")?"DiscoverAsync":"ReadAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();var html=root.ToHtmlString();
            if(mode is "discover-late-company" or "discover-late-input"){Assert.Equal(0,reads);Assert.DoesNotContain(review.Report.WorkPerformed,html);return;}
            if(mode is "field" or "permission"){Assert.Equal(0,reads);return;}Assert.Equal(1,reads);if(mode=="late-company"){Assert.DoesNotContain(review.Report.WorkPerformed,html);return;}
            Assert.Contains(review.Report.WorkPerformed,html);Assert.Contains(CompanyWorkOrderCompletionReview.PendingVerification,html);Assert.Contains("Observe actual completion",html);Assert.DoesNotContain("Private",html);
            if(mode=="same-person")Assert.Contains("same native person",html);if(mode=="defects")Assert.Contains("Recorded unresolved defect",html);
        });
    }
}
#endif
