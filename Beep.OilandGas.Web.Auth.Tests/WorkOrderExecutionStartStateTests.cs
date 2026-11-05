#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Web.Components.BusinessProcess;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor.Services;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderExecutionStartStateTests
{
    internal static CompanyWorkOrderExecutionStartCommand QualifiedStart(bool includeScope=true)
    {
        var company=new CompanyScope("tenant","company");var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),"field","order");
        var order=new WorkOrderDetailModel{InstanceId=task.WorkOrderId,FieldId=task.FieldId,Source="WORK_ORDER",State="",EvidenceUnavailableReason="No inferred header execution state",WoSubType="CORRECTIVE",Components=[
            new(task.WorkOrderId,"equipment-component",task.FieldId,"","pump"){NativeRevision=new string('A',64)},
            new(task.WorkOrderId,"isolation-component",task.FieldId,"",""){InformationItemId="ISOLATION-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('B',64)},
            new(task.WorkOrderId,"risk-component",task.FieldId,"",""){InformationItemId="RISK-information",InformationItemSubtype="DOCUMENT",NativeRevision=new string('C',64)}]};
        var prototype=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(true,originalOrder:order,taskOverride:task);if(!includeScope)return prototype.Command;
        var review=prototype.Command.Review;var context=review.Prerequisites.Requirements.Context;var utc=DateTime.UtcNow;
        var scope=new CompanyMaintenanceQualifiedAssets(context,CompanyWorkOrderExecutionRequirements.OrderRevision(order),"UTC",
            [new("plant","gas",task.FieldId,company.BusinessAssociateId,"READ",utc.AddDays(-1),null,new string('A',64))],
            [new("pump",company.BusinessAssociateId,"READ",utc.AddDays(-1),null,null,null,new string('B',64),new("pump","plant","gas",7,utc.AddDays(-1),utc.AddDays(-1),null,null,"recorded-installation",new string('C',64),1,new string('D',64)))],utc);
        review=review with{QualifiedWorkAssets=scope,ComparedAtUtc=DateTime.UtcNow,Revision=""};review=review with{Revision=CompanyWorkOrderExecutorIdentity.Digest(review)};
        var command=prototype.Command with{Review=review,StartedAtUtc=DateTime.UtcNow};command.RequireQualifiedWorkCapture();return command;
    }
    [Fact]
    public async Task MissingQualifiedScopeCannotRetainOrTransportANewWorkStart()
    {
        var command=QualifiedStart(false);var context=command.Review.Prerequisites.Requirements.Context;var query=command.Review.Prerequisites.Requirements.Approval.Permit;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,[command.Review.Stage.AssignedRole],["Maintenance.ExecuteWork","Inventory.View"])));
        using var state=new WorkOrderExecutionStartState(company);state.Attach(query);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewWorkOrderExecutionStartAsync(query,It.IsAny<CancellationToken>())).ReturnsAsync(command.Review);
        await state.ReviewAsync(client.Object);Assert.True(state.QualifiedWorkScopeMissing);Assert.False(state.CanStart);var retained=false;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.StartAsync(client.Object,"Actual work",_=>{retained=true;return Task.CompletedTask;}));Assert.False(retained);Assert.Null(state.VisibleReference);
        client.Verify(value=>value.StartWorkOrderExecutionAsync(It.IsAny<CompanyWorkOrderExecutionStartCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class ScopeAuth(string actor,string role):AuthenticationStateProvider
    {public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,role)],"fixture"))));}
    private sealed class ScopeJs:IJSRuntime
    {public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    private sealed class ScopeNav:NavigationManager
    {public ScopeNav(){Initialize("https://fixture.invalid/","https://fixture.invalid/order");}protected override void NavigateToCore(string uri,NavigationOptions options)=>Uri=ToAbsoluteUri(uri).ToString();}
    private sealed class ScopeComponents:IComponentActivator
    {internal WorkOrderExecutionStartPanel Panel=null!;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is WorkOrderExecutionStartPanel panel)Panel=panel;return component;}}
    [Theory][InlineData("current")][InlineData("missing")][InlineData("recovered")][InlineData("company-changed")]
    public async Task ActualOperatorStartScreenShowsCapturedScopeAndHidesItAfterContextChange(string mode)
    {
        var command=QualifiedStart(mode!="missing");var review=command.Review;var context=review.Prerequisites.Requirements.Context;var query=review.Prerequisites.Requirements.Approval.Permit;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,[review.Stage.AssignedRole],["Maintenance.ExecuteWork","Inventory.View"])));
        var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(value=>value.ReviewWorkOrderExecutionStartAsync(query,It.IsAny<CancellationToken>())).ReturnsAsync(review);
        client.Setup(value=>value.RecoverWorkOrderExecutionStartAsync(It.IsAny<CompanyWorkOrderExecutionStartReference>(),It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderExecutionStartRecovery(command,command.Prepare(DateTime.UtcNow)));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(context.FieldId);
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<WorkOrderExecutionStartState>();services.AddScoped<WorkOrderMaterialUsageState>();services.AddScoped<WorkOrderLaborUsageState>();services.AddScoped<WorkOrderSubmissionState>();services.AddScoped<WorkOrderExecutionHandoffState>();services.AddScoped<WorkOrderCompletionReviewState>();services.AddScoped<WorkOrderCompletionDecisionState>();services.AddScoped<WorkOrderReturnHandoffState>();
        services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(new ScopeAuth(context.ActorUserId,review.Stage.AssignedRole));services.AddSingleton<NavigationManager,ScopeNav>();services.AddSingleton<IJSRuntime,ScopeJs>();services.AddSingleton<IComponentActivator,ScopeComponents>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<WorkOrderExecutionStartPanel>(ParameterView.FromDictionary(new Dictionary<string,object?>{{"Permit",query}}));var panel=((ScopeComponents)provider.GetRequiredService<IComponentActivator>()).Panel;
            panel.GetType().GetField("_confirm",BindingFlags.Instance|BindingFlags.NonPublic)!.SetValue(panel,true);
            await (Task)panel.GetType().GetMethod("ReviewAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(panel,null)!;await Task.Yield();
            Assert.False((bool)panel.GetType().GetField("_confirm",BindingFlags.Instance|BindingFlags.NonPublic)!.GetValue(panel)!);
            if(mode=="recovered"){var state=provider.GetRequiredService<WorkOrderExecutionStartState>();await state.RecoverAsync(client.Object,command.IntentId);await Task.Yield();}
            if(mode=="company-changed"){company.Clear();await Task.Yield();}
            var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            if(mode=="missing"){Assert.Contains("scope is unavailable",html);Assert.False(provider.GetRequiredService<WorkOrderExecutionStartState>().CanStart);}
            else if(mode=="company-changed")Assert.DoesNotContain("Equipment pump",html);
            else{Assert.Contains("Equipment pump",html);Assert.Contains("Equipment installation parent",html);Assert.Contains("type gas",html);Assert.Contains("recorded-installation",html);Assert.Contains("source time zone UTC",html);}
            Assert.DoesNotContain("IdentityCatalogRevision",html);
        });
        client.Verify(value=>value.StartWorkOrderExecutionAsync(It.IsAny<CompanyWorkOrderExecutionStartCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task OriginalStartIsRetainedBeforeTransportAndOnlyOriginalReadOnlyRecoveryCanResolveIt(string mode)
    {
        var fixture=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original();var proof=fixture.Command.Review;var context=proof.Prerequisites.Requirements.Context;var permit=proof.Prerequisites.Requirements.Approval.Permit;
        using var company=new CompanyClientContext();var roles=new[]{"ConfiguredRole"};var permissions=new[]{"Maintenance.ExecuteWork","Inventory.View"};
        Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,roles,permissions)));
        using var state=new WorkOrderExecutionStartState(company);state.Attach(permit);var client=new Mock<ICompanyMaintenanceHandoffClient>();var sends=0;var retained=false;CompanyWorkOrderExecutionStartCommand? sent=null;WORK_ORDER_EXECUTION_START? recorded=null;
        client.Setup(value=>value.ReviewWorkOrderExecutionStartAsync(permit,It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        client.Setup(value=>value.StartWorkOrderExecutionAsync(It.IsAny<CompanyWorkOrderExecutionStartCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionStartCommand command,CancellationToken _)=>
        {
            sends++;Assert.True(retained);Assert.Equal(state.VisibleReference,command.IntentId);sent=CompanyInventorySourceReview.Copy(command);recorded=command.Prepare(DateTime.UtcNow);
            if(mode=="unknown")throw new InvalidOperationException("Lost acknowledgement; original may be recorded");if(mode=="late-company")company.Clear();
            return Task.FromResult(new CompanyWorkOrderExecutionStartReceipt(context.Company,context.ActorUserId,command.IntentId,recorded.START_ID,recorded.PERMIT_ID,recorded.STARTED_AT_UTC,recorded.RECORDED_REVISION));
        });
        client.Setup(value=>value.RecoverWorkOrderExecutionStartAsync(It.IsAny<CompanyWorkOrderExecutionStartReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionStartReference reference,CancellationToken _)=>
        {if(sent is null || recorded is null)throw new InvalidOperationException("No confirmed original start");Assert.Equal(sent.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderExecutionStartRecovery(sent,recorded));});
        await state.ReviewAsync(client.Object);Assert.True(state.CanStart);
        var error=await Record.ExceptionAsync(()=>state.StartAsync(client.Object,"Explicit actual start",id=>
        {Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);retained=true;if(mode=="before-transport")throw new InvalidOperationException("Reference could not be retained in the route");return Task.CompletedTask;}));
        Assert.True(retained);Assert.Equal(mode=="before-transport"?0:1,sends);Assert.False(state.CanReview);
        if(mode is "confirmed" or "late-company")Assert.Null(error);else Assert.NotNull(error);
        if(mode=="late-company")
        {
            Assert.Null(state.Receipt);Assert.Null(state.VisibleCommand);Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,roles,permissions)));state.Attach(permit);
        }
        var id=state.VisibleReference!.Value;var retainedBody=JsonSerializer.Serialize(state.VisibleCommand);var copy=state.VisibleCommand!;copy.Review.Prerequisites.Requirements.Order.Instructions="Consumer mutation";Assert.Equal(retainedBody,JsonSerializer.Serialize(state.VisibleCommand));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.StartAsync(client.Object,"Replacement",_=>Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,Guid.NewGuid()));
        if(mode=="before-transport")await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,id));
        else{await state.RecoverAsync(client.Object,id);Assert.NotNull(state.Receipt);Assert.Equal(retainedBody,JsonSerializer.Serialize(state.VisibleCommand));}
        Assert.Equal(mode=="before-transport"?0:1,sends);
    }
    private sealed class Response(object? payload,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Calls++;Assert.Contains("/execution-permit/work-",request.RequestUri!.AbsolutePath);change();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(payload,payload?.GetType()??typeof(CompanyWorkOrderRecordedStart))});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("role")][InlineData("late-company")][InlineData("wrong-start")]
    public async Task SubmissionInspectionClientRequiresOwnOriginalStartAndCurrentExecutionAuthority(string mode)
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original(true).Command;var before=command.Review.Prerequisites.Requirements;var row=command.Prepare(DateTime.UtcNow);
        var review=CompanyWorkOrderSubmissionInspectionReview.Create(new(command,row),before,DateTime.UtcNow);var reference=new CompanyWorkOrderExecutionStartReference(before.Approval.Permit,mode=="wrong-start"?Guid.NewGuid():command.IntentId);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),before.Context.ActorUserId,new(before.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ExecuteWork"])));
        var response=new Response(review,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderSubmissionInspectionsAsync(reference);Assert.NotEmpty(result.Blockers);Assert.Equal(command.IntentId,result.Start.IntentId);});
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
    [Theory][InlineData("current")][InlineData("empty")][InlineData("permission")][InlineData("late-company")][InlineData("foreign")]
    public async Task PublicRecordedStartClientUsesViewingCapabilityAndKeepsHistoricalAuthorSeparate(string mode)
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original().Command;
        var context=command.Review.Prerequisites.Requirements.Context;var query=command.Review.Prerequisites.Requirements.Approval.Permit;var row=command.Prepare(DateTime.UtcNow);
        var facts=new CompanyWorkOrderRecordedStart(context.Company,query,command.IntentId,row.START_ID,row.PERMIT_ID,context.ActorUserId,command.Review.Person.PersonBaId,command.Review.Stage.AssignedRole,command.Review.Stage.StepId,row.STARTED_AT_UTC,row.ROW_CREATED_DATE!.Value,row.PREREQUISITE_REVISION,row.RECORDED_REVISION);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"separate-history-viewer",new(context.Company,["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ViewApprovedPermit"])));
        var response=new Response(mode=="empty"?null:mode=="foreign"?facts with{Company=context.Company with{BusinessAssociateId="foreign"}}:facts,()=>{if(mode=="late-company")company.Clear();});
        using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        CompanyWorkOrderRecordedStart? result=null;var error=await Record.ExceptionAsync(async()=>result=await client.ReadRecordedWorkOrderStartAsync(query));
        if(mode is "current" or "empty"){Assert.Null(error);if(mode=="empty")Assert.Null(result);else{Assert.Equal(facts,result);Assert.NotEqual(company.Selection!.UserId,result!.ExecutorUserId);}}
        else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
    [Theory][InlineData("review","current")][InlineData("start","current")][InlineData("recovery","current")]
    [InlineData("review","permission")][InlineData("start","permission")][InlineData("recovery","permission")]
    [InlineData("review","late-company")][InlineData("start","late-company")][InlineData("recovery","late-company")]
    public async Task ActualTypedStartClientRequiresOriginalCompanyCapabilitiesAndRejectsLateResponses(string action,string mode)
    {
        var fixture=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Original();var command=fixture.Command;var context=command.Review.Prerequisites.Requirements.Context;var query=command.Review.Prerequisites.Requirements.Approval.Permit;var row=command.Prepare(DateTime.UtcNow);
        object payload=action switch{"review"=>command.Review,"start"=>new CompanyWorkOrderExecutionStartReceipt(context.Company,context.ActorUserId,command.IntentId,row.START_ID,row.PERMIT_ID,row.STARTED_AT_UTC,row.RECORDED_REVISION),_=>new CompanyWorkOrderExecutionStartRecovery(command,row)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ExecuteWork","Inventory.View"])));
        var response=new Response(payload,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(action=="review")(await client.ReviewWorkOrderExecutionStartAsync(query)).Require();else if(action=="start")(await client.StartWorkOrderExecutionAsync(command)).Require(command);else(await client.RecoverWorkOrderExecutionStartAsync(new(query,command.IntentId))).Require(context.Company,context.ActorUserId,new(query,command.IntentId));});
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
}
#endif
