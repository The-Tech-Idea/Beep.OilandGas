#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
public sealed class WorkOrderReturnHandoffStateTests
{
    private static CompanyMaintenanceReturnHandoffReview Review()=>WorkOrderMaintenanceReturnReviewTests.Review();
    private static CompanyClientContext Company(CompanyMaintenanceReturnHandoffReview review,string[]? roles=null,string[]? permissions=null)
    {var result=new CompanyClientContext();Assert.True(result.TrySelect(result.Clear(),review.Stage.Context.ActorUserId,new(review.Completion.Report.Company,roles??[review.Stage.AssignedRole],permissions??["Maintenance.VerifyCompletion","Workflow.Approve"])));return result;}
    private static CompanyMaintenanceReturnHandoffRecovery Recorded(CompanyMaintenanceReturnHandoffCommand command)
    {var review=command.Review;return new(command,new(review.Stage.Context.Company,review.Stage.Context.ActorUserId,command.IntentId,review.Completion.Report.Query.Task.InstanceId,review.Completion.Report.Query.SubmissionId,review.Stage.StepInstanceId,review.NextStepInstanceId,review.NextRole,command.HistoryId(),["closed-task"],["return-task"],DateTime.UtcNow,new string('C',64)));}
    [Theory][InlineData("uncertain")][InlineData("late-company")][InlineData("late-input")]
    public async Task RetainedOriginalSurvivesUncertaintyAndRecoveryCannotReplayOrReplaceIt(string mode)
    {
        var review=Review();using var company=Company(review);using var state=new WorkOrderReturnHandoffState(company);state.Attach(review.Completion.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewReturnHandoffAsync(review.Completion.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);
        CompanyMaintenanceReturnHandoffCommand? sent=null;Guid retained=Guid.Empty;
        client.Setup(value=>value.RecordReturnHandoffAsync(It.IsAny<CompanyMaintenanceReturnHandoffCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceReturnHandoffCommand command,CancellationToken _) =>{sent=command;Assert.Equal(retained,command.IntentId);if(mode=="late-company")company.Clear();if(mode=="late-input")state.Attach(null);throw new IOException("Acknowledgement unavailable");});
        await Assert.ThrowsAsync<IOException>(()=>state.RecordAsync(client.Object,"Original executor handoff",id=>{retained=id;return Task.CompletedTask;}));Assert.NotNull(sent);Assert.Null(state.Receipt);Assert.False(state.CanReview);
        if(mode!="uncertain"){Assert.Null(state.VisibleReference);Assert.True(company.TrySelect(company.Clear(),review.Stage.Context.ActorUserId,new(review.Completion.Report.Company,[review.Stage.AssignedRole],["Maintenance.VerifyCompletion","Workflow.Approve"])));state.Attach(review.Completion.Report.Query);}
        Assert.Equal(retained,state.VisibleReference);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,Guid.NewGuid()));
        client.Setup(value=>value.RecoverReturnHandoffAsync(sent!.Reference(),It.IsAny<CancellationToken>())).ReturnsAsync(Recorded(sent!));await state.RecoverAsync(client.Object,retained);
        Assert.NotNull(state.Receipt);Assert.Equal(JsonSerializer.Serialize(sent),JsonSerializer.Serialize(state.Receipt!.Command));client.Verify(value=>value.RecordReturnHandoffAsync(It.IsAny<CompanyMaintenanceReturnHandoffCommand>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("current")][InlineData("wrong-role")][InlineData("permission")][InlineData("late-company")]
    public async Task FreshCircuitRecoveryRequiresOriginalReviewerRoleAndCurrentContext(string mode)
    {
        var review=Review();using var company=Company(review,mode=="wrong-role"?["OtherRole"]:null,mode=="permission"?["Maintenance.VerifyCompletion"]:null);using var state=new WorkOrderReturnHandoffState(company);state.Attach(review.Completion.Report.Query);
        var command=new CompanyMaintenanceReturnHandoffCommand(review,Guid.NewGuid(),"Original retained handoff");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.RecoverReturnHandoffAsync(command.Reference(),It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="late-company")company.Clear();return Task.FromResult(Recorded(command));});
        var error=await Record.ExceptionAsync(()=>state.RecoverAsync(client.Object,command.IntentId));if(mode=="current"){Assert.Null(error);Assert.NotNull(state.Receipt);}else{Assert.Null(state.Receipt);if(mode!="late-company")Assert.IsType<UnauthorizedAccessException>(error);}
        client.Verify(value=>value.RecordReturnHandoffAsync(It.IsAny<CompanyMaintenanceReturnHandoffCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyMaintenanceReturnHandoffReview review,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/return-handoff/review",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)});}}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("wrong-role")][InlineData("late-company")]
    public async Task TypedReviewEnforcesCapabilitiesConfiguredRoleAndResponseFence(string mode)
    {
        var review=Review();using var company=Company(review,mode=="wrong-role"?["OtherRole"]:null,mode=="permission"?["Workflow.Approve"]:null);var reply=new Reply(review,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(()=>client.ReviewReturnHandoffAsync(review.Completion.Report.Query));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor,string role):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,role)],"fixture"))));}
    private sealed class RecordedReply(CompanyMaintenanceReturnHandoffRecovery recorded,bool recover,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;if(recover){Assert.EndsWith("/return-handoff/recovery/reference",request.RequestUri!.AbsolutePath);Assert.Equal(recorded.Command.Reference(),await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReturnHandoffReference>(token));}
            else{Assert.EndsWith("/return-handoff",request.RequestUri!.AbsolutePath);Assert.Equal(JsonSerializer.Serialize(recorded.Command),JsonSerializer.Serialize(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceReturnHandoffCommand>(token)));}
            changed();return new(HttpStatusCode.OK){Content=JsonContent.Create(recorded)};
        }
    }
    [Theory][InlineData(false,"current")][InlineData(true,"current")][InlineData(false,"permission")][InlineData(true,"permission")][InlineData(false,"late-company")][InlineData(true,"late-company")][InlineData(false,"wrong-role")][InlineData(true,"wrong-role")]
    public async Task TypedWriteAndReferenceRecoveryPreserveOriginalBodyAndRejectForeignOrLateResponses(bool recover,string mode)
    {
        var review=Review();using var company=Company(review,mode=="wrong-role"?["OtherRole"]:null,mode=="permission"?["Maintenance.VerifyCompletion"]:null);var saved=Recorded(new(review,Guid.NewGuid(),"Original exact verified-work return handoff"));var reply=new RecordedReply(saved,recover,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(()=>recover?client.RecoverReturnHandoffAsync(saved.Command.Reference()):client.RecordReturnHandoffAsync(saved.Command));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission" || mode=="wrong-role" && !recover?0:1,reply.Calls);
    }
    private sealed class Nav:NavigationManager{internal void Start(CompanyWorkOrderCompletionQuery query)=>Initialize("http://fixture/","http://fixture/order?returnProcess="+System.Uri.EscapeDataString(query.Task.InstanceId)+"&returnSubmission="+System.Uri.EscapeDataString(query.SubmissionId));protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator{internal WorkOrderReturnHandoffPanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is WorkOrderReturnHandoffPanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("changed-input")][InlineData("field")]
    public async Task ActualStandalonePanelRetainsSourceAndIntentBeforeTransportAndRecoversInFreshCircuit(string mode)
    {
        var review=Review();using var company=Company(review);var nav=new Nav();nav.Start(review.Completion.Report.Query);var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":review.Completion.Report.Query.Task.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var sends=0;CompanyMaintenanceReturnHandoffRecovery? saved=null;
        client.Setup(value=>value.ReviewReturnHandoffAsync(review.Completion.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(review);});
        client.Setup(value=>value.RecordReturnHandoffAsync(It.IsAny<CompanyMaintenanceReturnHandoffCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceReturnHandoffCommand command,CancellationToken _)=>{sends++;Assert.Contains("returnHandoff="+command.IntentId.ToString("D"),nav.Uri);Assert.Contains("returnSubmission="+Uri.EscapeDataString(command.Review.Completion.Report.Query.SubmissionId),nav.Uri);saved=Recorded(command);if(mode=="unknown")throw new IOException("Lost handoff acknowledgement");return Task.FromResult(saved);});
        client.Setup(value=>value.RecoverReturnHandoffAsync(It.IsAny<CompanyMaintenanceReturnHandoffReference>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceReturnHandoffReference reference,CancellationToken _)=>{Assert.Equal(saved!.Command.Reference(),reference);return Task.FromResult(saved);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderReturnHandoffState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<NavigationManager>(nav);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Stage.Context.ActorUserId,review.Stage.AssignedRole));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();
        async Task RenderAndAct(bool recover)
        {
            await using var scope=provider.CreateAsyncScope();await using var renderer=new HtmlRenderer(scope.ServiceProvider,scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>
            {
                var root=await renderer.RenderComponentAsync<WorkOrderReturnHandoffPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",review.Completion.Report.Query.Task.WorkOrderId},{"FieldId",review.Completion.Report.Query.Task.FieldId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
                async Task Invoke(string method){await (Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();}
                if(recover){await Invoke("RecoverAsync");Assert.Contains("Recorded verified-work handoff",root.ToHtmlString());Assert.Contains("FacilityRole",root.ToHtmlString());return;}
                await Invoke("ReviewAsync");if(mode=="field"){Assert.Equal(0,reads);return;}Assert.Contains("FacilityRole",root.ToHtmlString());Assert.Contains(review.Decision.ApprovalId,root.ToHtmlString());Assert.Contains(review.Completion.Report.WorkPerformed,root.ToHtmlString());
                void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
                Set("_reason","Original handoff rationale");Set("_confirm",true);if(mode=="changed-input"){Set("_submission","changed-report");panel.GetType().GetMethod("InputsChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);}
                await Invoke("RecordAsync");Assert.Equal(mode=="changed-input"?0:1,sends);if(mode=="changed-input")Assert.Null(scope.ServiceProvider.GetRequiredService<WorkOrderReturnHandoffState>().Review);
                if(mode=="unknown")Assert.Null(scope.ServiceProvider.GetRequiredService<WorkOrderReturnHandoffState>().Receipt);
            });
        }
        await RenderAndAct(false);if(mode is "confirmed" or "unknown"){await RenderAndAct(true);Assert.Equal(1,sends);}client.Verify(value=>value.SubmitWorkOrderAsync(It.IsAny<CompanyWorkOrderSubmissionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
