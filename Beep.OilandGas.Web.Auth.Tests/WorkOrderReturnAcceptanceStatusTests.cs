using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using Beep.OilandGas.Models.Core.Interfaces;
using Beep.OilandGas.Models.Data.Company;
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
public sealed class WorkOrderReturnAcceptanceStatusTests
{
    private static CompanyWorkOrderReturnStatus Status(bool recorded=true)
    {var review=WorkOrderReturnAcceptanceFixture.Create();var command=new CompanyWorkOrderReturnCommand(review,Guid.NewGuid(),DateTime.UtcNow,"Private independent return reason",true);var facts=CompanyWorkOrderReturnFacts.From(WorkOrderReturnAcceptanceStateTests.Recorded(command));var context=review.Completion.Context with{ActorUserId="authorized-return-reader"};var status=new CompanyWorkOrderReturnStatus(context,command.Reference().Query,recorded?facts:null);status.Require();return status;}
    private static CompanyClientContext Company(CompanyWorkOrderReturnStatus status,bool permitted=true)
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Context.ActorUserId,new(status.Context.Company,["ReturnReaderRole"],permitted?["Maintenance.ReturnToService"]:[])));return company;}
    [Theory][InlineData("recorded")][InlineData("absent")][InlineData("failed")][InlineData("permission")][InlineData("late-company")][InlineData("late-source")]
    public async Task AnotherAuthorizedReaderNeedsNoPrivateApprovalReferenceAndFailureCannotEstablishAbsence(string mode)
    {
        var status=Status(mode!="absent");using var company=Company(status,mode!="permission");using var state=new WorkOrderReturnAcceptanceState(company);state.Attach(status.Query);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReadReturnAcceptanceStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{if(mode=="failed")throw new HttpRequestException("Unavailable");if(mode=="late-company")company.Clear();if(mode=="late-source")state.Attach(null);return Task.FromResult(status);});
        var error=await Record.ExceptionAsync(()=>state.ReadStatusAsync(client.Object));if(mode is "recorded" or "absent"){Assert.Null(error);Assert.NotNull(state.Status);Assert.Equal(status.Recorded?.ApprovalId,state.Status!.Recorded?.ApprovalId);}else{Assert.Null(state.Status);if(mode is "failed" or "permission")Assert.NotNull(error);}Assert.Null(state.VisibleCommand);client.Verify(value=>value.RecordReturnAcceptanceAsync(It.IsAny<CompanyWorkOrderReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyWorkOrderReturnStatus status,bool discover,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith(discover?"/return-acceptance/report":"/return-acceptance/status",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=discover?JsonContent.Create(status.Query):JsonContent.Create(status)});}}
    [Theory][InlineData(false,"current")][InlineData(true,"current")][InlineData(false,"permission")][InlineData(true,"permission")][InlineData(false,"late-company")][InlineData(true,"late-company")]
    public async Task TypedPublicReadAndAssignmentDiscoveryUseDedicatedReturnReadCapabilityAndFenceLateCompany(bool discover,string mode)
    {
        var status=Status();using var company=Company(status,mode!="permission");var reply=new Reply(status,discover,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(async()=>{if(discover)await client.DiscoverReturnAcceptanceReportAsync(status.Query.Task);else await client.ReadReturnAcceptanceStatusAsync(status.Query);});if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth(string actor):AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ReturnReaderRole")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal WorkOrderReturnAcceptancePanel Panel=null!;public IComponent CreateInstance(Type type){var value=(IComponent)Activator.CreateInstance(type)!;if(value is WorkOrderReturnAcceptancePanel panel)Panel=panel;return value;}}
    private sealed class Nav:NavigationManager{public Nav(){Initialize("https://fixture.invalid/","https://fixture.invalid/order");}protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string id,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string id,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("recorded")][InlineData("absent")][InlineData("failed")][InlineData("permission")][InlineData("field")][InlineData("late-company")][InlineData("discover")][InlineData("discover-late-input")]
    public async Task ActualReadOnlyReturnScreenShowsPublicAuditAndDiscoversOriginalReportWithoutPrivateActions(string mode)
    {
        var status=Status(mode!="absent");using var company=Company(status,mode!="permission");var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;var discoveries=0;WorkOrderReturnAcceptancePanel? captured=null;
        client.Setup(value=>value.ReadReturnAcceptanceStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="failed")throw new HttpRequestException("Unavailable");if(mode=="late-company")company.Clear();return Task.FromResult(status);});
        client.Setup(value=>value.DiscoverReturnAcceptanceReportAsync(status.Query.Task,It.IsAny<CancellationToken>())).Returns(()=>{discoveries++;if(mode=="discover-late-input")captured!.GetType().GetMethod("InputsChanged",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(captured,null);return Task.FromResult(status.Query);});var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?"other-field":status.Context.FieldId);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderReturnAcceptanceState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<NavigationManager,Nav>();services.AddSingleton<AuthenticationStateProvider>(new Auth(status.Context.ActorUserId));services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var parameters=new Dictionary<string,object?>{{"WorkOrderId",status.Query.Task.WorkOrderId},{"FieldId",status.Query.Task.FieldId}};if(mode.StartsWith("discover"))parameters["OriginalTask"]=status.Query.Task;var root=await renderer.RenderComponentAsync<WorkOrderReturnAcceptancePanel>(ParameterView.FromDictionary(parameters));var panel=((Components)provider.GetRequiredService<IComponentActivator>()).Panel;captured=panel;
            if(mode.StartsWith("discover")){await (Task)panel.GetType().GetMethod("DiscoverAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;Assert.Equal(1,discoveries);var value=panel.GetType().GetField("_submission",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel);Assert.Equal(mode=="discover"?status.Query.SubmissionId:"",value);return;}
            panel.GetType().GetField("_process",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,status.Query.Task.InstanceId);panel.GetType().GetField("_submission",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,status.Query.SubmissionId);await (Task)panel.GetType().GetMethod("ReadStatusAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();var html=root.ToHtmlString();Assert.DoesNotContain("Recover original return acceptance without another write",html);
            if(mode is "permission" or "field"){Assert.Equal(0,reads);return;}Assert.Equal(1,reads);if(mode is "failed" or "late-company"){Assert.DoesNotContain("Recorded physical return acceptance",html);Assert.DoesNotContain("No recorded physical return acceptance was found",html);return;}if(mode=="absent")Assert.Contains("No recorded physical return acceptance was found",html);else{Assert.Contains("Recorded physical return acceptance",html);Assert.Contains(status.Recorded!.ApproverUserId,html);Assert.Contains(status.Recorded.ApprovalId,html);}
        });client.Verify(value=>value.RecordReturnAcceptanceAsync(It.IsAny<CompanyWorkOrderReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
