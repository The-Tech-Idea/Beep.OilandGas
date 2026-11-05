#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
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
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.JSInterop;
using Moq;
using MudBlazor.Services;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderCompletionDecisionStatusTests
{
    private static CompanyWorkOrderCompletionDecisionStatus Status(string mode="VERIFIED")
    {
        var source=WorkOrderCompletionReviewTests.Review();var context=mode=="executor-reader"?source.Context with{ActorUserId=source.Report.ExecutorUserId}:source.Context;
        var facts=new CompanyWorkOrderCompletionDecisionFacts(context.Company,context.Database,source.Report.Query,"original-independent-reviewer","recorded-reviewer-person","VerifierRole",mode=="REJECTED"?"REJECTED":"VERIFIED",DateTime.UtcNow,"recorded-approval","recorded-history",source.Report.Revision,source.SourceRevision(),new string('C',64));
        var result=new CompanyWorkOrderCompletionDecisionStatus(context,source.Report.Query,mode=="unrecorded"?null:facts);result.Require();return result;
    }
    private static CompanyClientContext Company(CompanyWorkOrderCompletionDecisionStatus status,bool permitted=true)
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Context.ActorUserId,new(status.Context.Company,["SupervisorRole"],permitted?["Maintenance.VerifyCompletion"]:[])));return company;}
    [Theory][InlineData("VERIFIED")][InlineData("REJECTED")][InlineData("unrecorded")][InlineData("permission")][InlineData("failed")][InlineData("late-company")][InlineData("late-source")][InlineData("executor-reader")]
    public async Task PublicStatusStateRequiresCurrentAuthorityAndNeverInfersAbsenceFromFailure(string mode)
    {
        var status=Status(mode);using var company=Company(status,mode!="permission");using var state=new WorkOrderCompletionReviewState(company);state.Attach(status.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReadCompletionDecisionStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="failed")throw new HttpRequestException("Status unavailable");if(mode=="late-company")company.Clear();if(mode=="late-source")state.Clear();return Task.FromResult(status);});
        var error=await Record.ExceptionAsync(()=>state.ReadDecisionStatusAsync(client.Object));if(mode is "VERIFIED" or "REJECTED" or "unrecorded" or "executor-reader"){Assert.Null(error);Assert.NotNull(state.DecisionStatus);Assert.Equal(status.Recorded?.Decision,state.DecisionStatus!.Recorded?.Decision);}else{Assert.Null(state.DecisionStatus);if(mode is "permission" or "failed")Assert.NotNull(error);}
        client.Verify(value=>value.RecordCompletionDecisionAsync(It.IsAny<CompanyWorkOrderCompletionDecisionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyWorkOrderCompletionDecisionStatus status,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/completion-decision/status",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(status)});}}
    [Theory][InlineData("VERIFIED")][InlineData("permission")][InlineData("late-company")][InlineData("executor-reader")]
    public async Task TypedPublicReadRequiresReadCapabilityAndFencesCompanyChangesWithoutApprovalCapability(string mode)
    {
        var status=Status(mode);using var company=Company(status,mode!="permission");var reply=new Reply(status,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(()=>client.ReadCompletionDecisionStatusAsync(status.Query));
        if(mode is "VERIFIED" or "executor-reader")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"SupervisorRole")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderCompletionReviewPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderCompletionReviewPanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("VERIFIED")][InlineData("REJECTED")][InlineData("unrecorded")][InlineData("failed")][InlineData("field")][InlineData("permission")][InlineData("late-company")][InlineData("executor-reader")]
    public async Task ActualCompletionScreenShowsPublicOutcomeOrExplicitAbsenceWithoutPrivateReviewerReference(string mode)
    {
        var status=Status(mode);using var company=Company(status,mode!="permission");var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":status.Context.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(value=>value.ReadCompletionDecisionStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="failed")throw new HttpRequestException("Recorded status unavailable");if(mode=="late-company")company.Clear();return Task.FromResult(status);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderCompletionReviewState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(status.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderCompletionReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",status.Query.Task.WorkOrderId},{"FieldId",status.Query.Task.FieldId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            panel.GetType().GetField("_process",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,status.Query.Task.InstanceId);panel.GetType().GetField("_submission",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,status.Query.SubmissionId);await (Task)panel.GetType().GetMethod("ReadDecisionStatusAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();var html=root.ToHtmlString();
            if(mode is "field" or "permission"){Assert.Equal(0,reads);return;}Assert.Equal(1,reads);if(mode is "failed" or "late-company"){Assert.DoesNotContain("Recorded independent completion outcome:",html);Assert.DoesNotContain("No recorded independent completion decision was found",html);return;}
            if(mode=="unrecorded"){Assert.Contains("No recorded independent completion decision was found",html);return;}Assert.Contains("Recorded independent completion outcome:",html);Assert.Contains(status.Recorded!.Decision,html);Assert.Contains(status.Recorded.ReviewerUserId,html);Assert.Contains(status.Recorded.ReviewerPersonBaId,html);Assert.DoesNotContain("Private",html);
        });
        client.Verify(value=>value.RecordCompletionDecisionAsync(It.IsAny<CompanyWorkOrderCompletionDecisionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
