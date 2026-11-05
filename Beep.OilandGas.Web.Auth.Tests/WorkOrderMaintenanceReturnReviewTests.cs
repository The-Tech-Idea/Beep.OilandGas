using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
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
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderMaintenanceReturnReviewTests
{
    internal static CompanyMaintenanceReturnHandoffReview Review(bool assetReturn=false)
    {
        var completion=WorkOrderCompletionReviewTests.Review(assetReturn:assetReturn);var stage=new CompanyMaintenanceAssignedStageReview(completion.Context,completion.Report.Query.Task,"verification-instance","verify","Independent completion review","SupervisorRole","receiver","receiving-history","receiving-approval",new string('A',64),new string('B',64));
        var at=DateTime.UtcNow;var facts=new CompanyWorkOrderCompletionDecisionFacts(completion.Context.Company,completion.Context.Database,completion.Report.Query,completion.Context.ActorUserId,completion.ReviewerPerson!.PersonBaId,stage.AssignedRole,"VERIFIED",at,"verification-approval","verification-history",completion.Report.Revision,completion.SourceRevision(),new string('C',64)){WorkSubjectRevision=completion.WorkSubjectRevision()};
        var now=DateTime.UtcNow;completion=completion with{ComparedAtUtc=now,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(completion.Context,completion.Report,completion.Order,completion.Requirements,completion.Identities,completion.ReviewerPerson,now)};
        var result=new CompanyMaintenanceReturnHandoffReview(stage,facts,completion,"return","return-instance","FacilityRole");result.Require();return result;
    }
    private static CompanyClientContext Company(CompanyMaintenanceReturnHandoffReview review,string mode)
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Stage.Context.ActorUserId,new(review.Stage.Context.Company,mode=="role"?["OtherRole"]:[review.Stage.AssignedRole],mode=="permission"?["Maintenance.VerifyCompletion"]:["Maintenance.VerifyCompletion","Workflow.Approve"])));return company;}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("role")][InlineData("failed")][InlineData("late-company")][InlineData("late-source")]
    public async Task StateRequiresApprovalCapabilityAndDiscardsStaleConfiguredRoleReview(string mode)
    {
        var review=Review();using var company=Company(review,mode);using var state=new WorkOrderCompletionReviewState(company);state.Attach(review.Completion.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewReturnHandoffAsync(review.Completion.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="failed")throw new HttpRequestException("Unavailable");if(mode=="late-company")company.Clear();if(mode=="late-source")state.Clear();return Task.FromResult(review);});
        var error=await Record.ExceptionAsync(()=>state.ReadReturnReviewAsync(client.Object));if(mode=="current"){Assert.Null(error);Assert.Equal("FacilityRole",state.ReturnReview!.NextRole);}else{Assert.Null(state.ReturnReview);if(mode is "permission" or "role" or "failed")Assert.NotNull(error);}Assert.False(state.Busy);
        client.Verify(value=>value.RecordCompletionDecisionAsync(It.IsAny<CompanyWorkOrderCompletionDecisionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyMaintenanceReturnHandoffReview review,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/return-handoff/review",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)});}}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("role")][InlineData("late-company")]
    public async Task TypedClientBindsConfiguredReviewToOriginalCompanyAccountAndRole(string mode)
    {
        var review=Review();using var company=Company(review,mode);var reply=new Reply(review,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(()=>client.ReviewReturnHandoffAsync(review.Completion.Report.Query));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor,string role):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,role)],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderCompletionReviewPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderCompletionReviewPanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private sealed class Nav:NavigationManager{public Nav(){Initialize("https://fixture.invalid/","https://fixture.invalid/order");}protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    [Theory][InlineData("current")][InlineData("failed")][InlineData("permission")][InlineData("field")][InlineData("late-company")]
    public async Task ActualScreenShowsResponsibleRoleAndExplicitStillBlockedState(string mode)
    {
        var review=Review();using var company=Company(review,mode);var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other-field":review.Stage.Context.FieldId);var reads=0;var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewReturnHandoffAsync(review.Completion.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="failed")throw new HttpRequestException("Unavailable");if(mode=="late-company")company.Clear();return Task.FromResult(review);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderCompletionReviewState>();services.AddScoped<WorkOrderCompletionDecisionState>();services.AddScoped<WorkOrderReturnHandoffState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Stage.Context.ActorUserId,review.Stage.AssignedRole));services.AddSingleton<NavigationManager,Nav>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderCompletionReviewPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",review.Stage.Task.WorkOrderId},{"FieldId",review.Stage.Task.FieldId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
            panel.GetType().GetField("_process",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,review.Stage.Task.InstanceId);panel.GetType().GetField("_submission",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,review.Completion.Report.Query.SubmissionId);await (Task)panel.GetType().GetMethod("ReadReturnReviewAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();var html=root.ToHtmlString();
            if(mode=="current"){Assert.Contains("Configured return approval role: FacilityRole",html);Assert.Contains("return assignment is still blocked",html);}else Assert.DoesNotContain("Configured return approval role: FacilityRole",html);Assert.Equal(mode is "permission" or "field"?0:1,reads);
        });
        client.Verify(value=>value.RecordCompletionDecisionAsync(It.IsAny<CompanyWorkOrderCompletionDecisionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
