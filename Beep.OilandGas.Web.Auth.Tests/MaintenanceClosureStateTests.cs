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
public sealed class MaintenanceClosureStateTests
{
    internal static CompanyMaintenanceClosureReview Review(string? wellId=null,bool assetReturn=false)
    {
        var physical=WorkOrderReturnAcceptanceFixture.Create(wellId,assetReturn);var acceptance=CompanyWorkOrderReturnFacts.From(WorkOrderReturnAcceptanceStateTests.Recorded(new(physical,Guid.NewGuid(),DateTime.UtcNow,"Original physical acceptance",true)));
        var stage=physical.Assignment.Stage with{ProcessRevision=new string('E',64)};var completion=physical.Completion with{ComparedAtUtc=DateTime.UtcNow};var current=physical with{Assignment=physical.Assignment with{Stage=stage},Completion=completion};var result=new CompanyMaintenanceClosureReview(stage,acceptance,current);result.Require();return result;
    }
    private static CompanyClientContext Company(CompanyMaintenanceClosureReview review,string[]? roles=null,string[]? permissions=null)
    {var result=new CompanyClientContext();Assert.True(result.TrySelect(result.Clear(),review.Current.Assignment.Stage.Context.ActorUserId,new(review.Current.Completion.Report.Company,roles??[review.Current.Assignment.Stage.AssignedRole],permissions??["Maintenance.ReturnToService","Workflow.Execute"])));return result;}
    internal static CompanyMaintenanceClosureRecovery Recorded(CompanyMaintenanceClosureCommand command)
    {var stage=command.Review.Stage;return new(command,new(stage.Context.Company,stage.Context.ActorUserId,command.IntentId,stage.Task.InstanceId,command.Reference().Query.SubmissionId,stage.StepInstanceId,command.HistoryId(),["terminal-task"],DateTime.UtcNow,new string('C',64)));}
    [Theory][InlineData("uncertain")][InlineData("late-company")][InlineData("late-input")]
    public async Task RetainedOriginalSurvivesUncertaintyAndRecoveryCannotReplayOrReplaceIt(string mode)
    {
        var review=Review();using var company=Company(review);using var state=new MaintenanceClosureState(company);state.Attach(review.Current.Completion.Report.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewMaintenanceClosureAsync(review.Current.Completion.Report.Query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);
        CompanyMaintenanceClosureCommand? sent=null;Guid retained=Guid.Empty;
        client.Setup(value=>value.CloseMaintenanceAsync(It.IsAny<CompanyMaintenanceClosureCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceClosureCommand command,CancellationToken _) =>{sent=command;Assert.Equal(retained,command.IntentId);if(mode=="late-company")company.Clear();if(mode=="late-input")state.Attach(null);throw new IOException("Acknowledgement unavailable");});
        await Assert.ThrowsAsync<IOException>(()=>state.RecordAsync(client.Object,"Original executor handoff",id=>{retained=id;return Task.CompletedTask;}));Assert.NotNull(sent);Assert.Null(state.Receipt);Assert.False(state.CanReview);
        if(mode!="uncertain"){Assert.Null(state.VisibleReference);Assert.True(company.TrySelect(company.Clear(),review.Current.Assignment.Stage.Context.ActorUserId,new(review.Current.Completion.Report.Company,[review.Current.Assignment.Stage.AssignedRole],["Maintenance.ReturnToService","Workflow.Execute"])));state.Attach(review.Current.Completion.Report.Query);}
        Assert.Equal(retained,state.VisibleReference);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ReviewAsync(client.Object));await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,Guid.NewGuid()));
        client.Setup(value=>value.RecoverMaintenanceClosureAsync(sent!.Reference(),It.IsAny<CancellationToken>())).ReturnsAsync(Recorded(sent!));await state.RecoverAsync(client.Object,retained);
        Assert.NotNull(state.Receipt);Assert.Equal(JsonSerializer.Serialize(sent),JsonSerializer.Serialize(state.Receipt!.Command));client.Verify(value=>value.CloseMaintenanceAsync(It.IsAny<CompanyMaintenanceClosureCommand>(),It.IsAny<CancellationToken>()),Times.Once);
    }
    [Theory][InlineData("current")][InlineData("wrong-role")][InlineData("permission")][InlineData("late-company")]
    public async Task FreshCircuitRecoveryRequiresOriginalReviewerRoleAndCurrentContext(string mode)
    {
        var review=Review();using var company=Company(review,mode=="wrong-role"?["OtherRole"]:null,mode=="permission"?["Maintenance.ReturnToService"]:null);using var state=new MaintenanceClosureState(company);state.Attach(review.Current.Completion.Report.Query);
        var command=new CompanyMaintenanceClosureCommand(review,Guid.NewGuid(),"Original retained handoff",true);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.RecoverMaintenanceClosureAsync(command.Reference(),It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="late-company")company.Clear();return Task.FromResult(Recorded(command));});
        var error=await Record.ExceptionAsync(()=>state.RecoverAsync(client.Object,command.IntentId));if(mode=="current"){Assert.Null(error);Assert.NotNull(state.Receipt);}else{Assert.Null(state.Receipt);if(mode!="late-company")Assert.IsType<UnauthorizedAccessException>(error);}
        client.Verify(value=>value.CloseMaintenanceAsync(It.IsAny<CompanyMaintenanceClosureCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyMaintenanceClosureReview review,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/closure/review",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(review)});}}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("wrong-role")][InlineData("late-company")]
    public async Task TypedReviewEnforcesCapabilitiesConfiguredRoleAndResponseFence(string mode)
    {
        var review=Review();using var company=Company(review,mode=="wrong-role"?["OtherRole"]:null,mode=="permission"?["Workflow.Execute"]:null);var reply=new Reply(review,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};
        var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(()=>client.ReviewMaintenanceClosureAsync(review.Current.Completion.Report.Query));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor,string role):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,role)],"fixture"))));}
    private sealed class RecordedReply(CompanyMaintenanceClosureRecovery recorded,bool recover,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;if(recover){Assert.EndsWith("/closure/recovery/reference",request.RequestUri!.AbsolutePath);Assert.Equal(recorded.Command.Reference(),await request.Content!.ReadFromJsonAsync<CompanyMaintenanceClosureReference>(token));}
            else{Assert.EndsWith("/closure",request.RequestUri!.AbsolutePath);Assert.Equal(JsonSerializer.Serialize(recorded.Command),JsonSerializer.Serialize(await request.Content!.ReadFromJsonAsync<CompanyMaintenanceClosureCommand>(token)));}
            changed();return new(HttpStatusCode.OK){Content=JsonContent.Create(recorded)};
        }
    }
    [Theory][InlineData(false,"current")][InlineData(true,"current")][InlineData(false,"permission")][InlineData(true,"permission")][InlineData(false,"late-company")][InlineData(true,"late-company")][InlineData(false,"wrong-role")][InlineData(true,"wrong-role")]
    public async Task TypedWriteAndReferenceRecoveryPreserveOriginalBodyAndRejectForeignOrLateResponses(bool recover,string mode)
    {
        var review=Review();using var company=Company(review,mode=="wrong-role"?["OtherRole"]:null,mode=="permission"?["Maintenance.ReturnToService"]:null);var saved=Recorded(new(review,Guid.NewGuid(),"Original exact verified-work return handoff",true));var reply=new RecordedReply(saved,recover,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(()=>recover?client.RecoverMaintenanceClosureAsync(saved.Command.Reference()):client.CloseMaintenanceAsync(saved.Command));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission" || mode=="wrong-role" && !recover?0:1,reply.Calls);
    }
    private sealed class Nav:NavigationManager{internal void Start(CompanyWorkOrderCompletionQuery query)=>Initialize("http://fixture/","http://fixture/order?closureProcess="+System.Uri.EscapeDataString(query.Task.InstanceId)+"&closureSubmission="+System.Uri.EscapeDataString(query.SubmissionId));protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class Components:IComponentActivator{internal MaintenanceClosurePanel Panel=null!;public IComponent CreateInstance(Type type){var result=(IComponent)Activator.CreateInstance(type)!;if(result is MaintenanceClosurePanel panel)Panel=panel;return result;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("changed-input")][InlineData("field")]
    public async Task ActualStandalonePanelRetainsSourceAndIntentBeforeTransportAndRecoversInFreshCircuit(string mode)
    {
        var review=Review();using var company=Company(review);var nav=new Nav();nav.Start(review.Current.Completion.Report.Query);var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other":review.Current.Completion.Report.Query.Task.FieldId);var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var sends=0;CompanyMaintenanceClosureRecovery? saved=null;
        client.Setup(value=>value.ReviewMaintenanceClosureAsync(review.Current.Completion.Report.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;return Task.FromResult(review);});
        client.Setup(value=>value.CloseMaintenanceAsync(It.IsAny<CompanyMaintenanceClosureCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceClosureCommand command,CancellationToken _)=>{sends++;Assert.Contains("maintenanceClosure="+command.IntentId.ToString("D"),nav.Uri);Assert.Contains("closureSubmission="+Uri.EscapeDataString(command.Review.Current.Completion.Report.Query.SubmissionId),nav.Uri);saved=Recorded(command);if(mode=="unknown")throw new IOException("Lost handoff acknowledgement");return Task.FromResult(saved);});
        client.Setup(value=>value.RecoverMaintenanceClosureAsync(It.IsAny<CompanyMaintenanceClosureReference>(),It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceClosureReference reference,CancellationToken _)=>{Assert.Equal(saved!.Command.Reference(),reference);return Task.FromResult(saved);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<MaintenanceClosureState>();services.AddScoped<MaintenanceAssetEvidenceState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<NavigationManager>(nav);services.AddSingleton<AuthenticationStateProvider>(new Auth(review.Current.Assignment.Stage.Context.ActorUserId,review.Current.Assignment.Stage.AssignedRole));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();
        async Task RenderAndAct(bool recover)
        {
            await using var scope=provider.CreateAsyncScope();await using var renderer=new HtmlRenderer(scope.ServiceProvider,scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async()=>
            {
                var root=await renderer.RenderComponentAsync<MaintenanceClosurePanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"WorkOrderId",review.Current.Completion.Report.Query.Task.WorkOrderId},{"FieldId",review.Current.Completion.Report.Query.Task.FieldId}}));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;
                async Task Invoke(string method){await (Task)panel.GetType().GetMethod(method,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();}
                if(recover){await Invoke("RecoverAsync");Assert.Contains("Recorded maintenance workflow completion",root.ToHtmlString());Assert.Contains("final tasks",root.ToHtmlString());return;}
                await Invoke("ReviewAsync");if(mode=="field"){Assert.Equal(0,reads);return;}Assert.Contains("FacilityRole",root.ToHtmlString());Assert.Contains(review.Acceptance.ApprovalId,root.ToHtmlString());Assert.Contains(review.Current.Completion.Report.WorkPerformed,root.ToHtmlString());
                void Set(string key,object value)=>panel.GetType().GetField(key,BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,value);
                Set("_reason","Original handoff rationale");Set("_confirm",true);if(mode=="changed-input"){Set("_submission","changed-report");panel.GetType().GetMethod("InputsChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null);}
                await Invoke("RecordAsync");Assert.Equal(mode is "changed-input"?0:1,sends);if(mode=="changed-input")Assert.Null(scope.ServiceProvider.GetRequiredService<MaintenanceClosureState>().Review);
                if(mode=="unknown")Assert.Null(scope.ServiceProvider.GetRequiredService<MaintenanceClosureState>().Receipt);
            });
        }
        await RenderAndAct(false);if(mode is "confirmed" or "unknown"){await RenderAndAct(true);Assert.Equal(1,sends);}client.Verify(value=>value.SubmitWorkOrderAsync(It.IsAny<CompanyWorkOrderSubmissionCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
