#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Security.Claims;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Components.Workflow;
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
public sealed class MaintenanceRecordedWorkTests
{
    private static readonly FieldSelectionReview Context=new(new("tenant","company"),new("core","binding"),"reader","scope","field");
    private static CompanyClientContext Company(bool allowed=true){var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"reader",new(Context.Company,["ReadRole"],allowed?["Maintenance.ReturnToService"]:[])));return company;}
    private static CompanyWorkOrderSubmissionFacts[] Reports(int count=1)=>Enumerable.Range(0,count).Select(_=>
    {var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(Context.Company,Guid.NewGuid()),"field","order-الكويت");var report=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission(Context.Database,taskOverride:task);return CompanyWorkOrderSubmissionFacts.From(report.Command.Prepare(DateTime.UtcNow));}).ToArray();
    private static CompanyMaintenanceRecordedWorkWindow Window()=>new("field",DateTime.UtcNow.AddDays(-1),DateTime.UtcNow.AddDays(1));
    [Theory][InlineData("current")][InlineData("permission")][InlineData("failed")][InlineData("foreign-company")][InlineData("wrong-window")][InlineData("late-company")][InlineData("late-source")]
    public async Task StateKeepsOnlyTheOriginalAuthorizedPeriodPageAndNeverTreatsFailuresAsAnEmptyQueue(string mode)
    {
        var reports=Reports();var window=Window();using var company=Company(mode!="permission");using var state=new MaintenanceRecordedWorkState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReadMaintenanceRecordedWorkPageAsync(window,1,8,It.IsAny<CancellationToken>())).Returns(()=>
        {if(mode=="failed")throw new HttpRequestException("Read unavailable");if(mode=="late-company")company.Clear();if(mode=="late-source")state.Clear();var result=CompanyMaintenanceRecordedWorkPage.Create(Context,window,1,8,reports,DateTime.UtcNow);if(mode=="foreign-company")result=result with{Context=Context with{Company=new("tenant","other")}};if(mode=="wrong-window")result=result with{Window=window with{FromUtc=window.FromUtc.AddHours(-1)}};return Task.FromResult(result);});
        var error=await Record.ExceptionAsync(()=>state.ReadAsync(client.Object,window,1,8));if(mode=="current"){Assert.Null(error);Assert.NotNull(state.Current);}else Assert.Null(state.Current);client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class Reply(CompanyMaintenanceRecordedWorkPage result,Action changed):HttpMessageHandler
    {internal int Calls;protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;Assert.EndsWith("/recorded-work/pages/1/8",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(result)});}}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("late-company")]
    public async Task ActualClientRequiresReadCapabilityAndRejectsLatePageResponses(string mode)
    {
        var window=Window();var page=CompanyMaintenanceRecordedWorkPage.Create(Context,window,1,8,Reports(),DateTime.UtcNow);using var company=Company(mode!="permission");var reply=new Reply(page,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(reply){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(()=>client.ReadMaintenanceRecordedWorkPageAsync(window,1,8));if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,reply.Calls);
    }
    private sealed class Auth:AuthenticationStateProvider{public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id","reader"),new(ClaimTypes.Role,"ReadRole")],"fixture"))));}
    private sealed class Components:IComponentActivator{internal MaintenanceRecordedWorkQueue Queue=null!;internal MaintenanceAssetEvidencePanel? Evidence;public IComponent CreateInstance(Type type){var component=(IComponent)Activator.CreateInstance(type)!;if(component is MaintenanceRecordedWorkQueue queue)Queue=queue;if(component is MaintenanceAssetEvidencePanel evidence)Evidence=evidence;return component;}}
    private sealed class NoBrowser:IJSRuntime{public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)=>ValueTask.FromResult(default(T)!);public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>ValueTask.FromResult(default(T)!);}
    [Theory][InlineData("current")][InlineData("permission")][InlineData("field")][InlineData("failed")][InlineData("late-field")][InlineData("late-company")]
    public async Task ActualQueuePagesOriginalReportsAndClearsUnavailableOrChangedContext(string mode)
    {
        var reports=Reports(mode=="current"?9:1);using var company=Company(mode!="permission");var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(mode=="field"?null:"field");var client=new Mock<ICompanyMaintenanceHandoffClient>();var reads=0;
        client.Setup(value=>value.ReadMaintenanceRecordedWorkPageAsync(It.IsAny<CompanyMaintenanceRecordedWorkWindow>(),It.IsAny<int>(),8,It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceRecordedWorkWindow window,int page,int _,CancellationToken _)=>{reads++;if(mode=="failed")throw new HttpRequestException("Unavailable");if(mode=="late-field")fields.Raise(value=>value.CurrentFieldChanged+=null!,"other");if(mode=="late-company")company.Clear();return Task.FromResult(CompanyMaintenanceRecordedWorkPage.Create(Context,window,page,8,reports,DateTime.UtcNow));});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider,Auth>();services.AddSingleton<IComponentActivator,Components>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceRecordedWorkQueue>();var html=root.ToHtmlString();if(mode=="current")
            {Assert.Contains("Authorized work reports in this UTC period: 9",html);Assert.Contains("closureSubmission=",html);Assert.Contains("sourceTenant=tenant",html);Assert.Contains("sourceCompany=company",html);Assert.Contains("sourceField=field",html);Assert.DoesNotContain("COMMAND_JSON",html);var queue=((Components)provider.GetRequiredService<IComponentActivator>()).Queue;await (Task)typeof(MaintenanceRecordedWorkQueue).GetMethod("LoadAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queue,[2])!;await Task.Yield();Assert.Contains("page 2 of 2",root.ToHtmlString());Assert.Equal(2,reads);}
            else{Assert.DoesNotContain("Open original report and return evidence",html);Assert.DoesNotContain("Authorized work reports in this UTC period: 0",html);if(mode is "permission" or "field")Assert.Equal(0,reads);}
        });
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
    }
    private sealed class EvidenceAuth(string actor):AuthenticationStateProvider
    {
        public override Task<AuthenticationState> GetAuthenticationStateAsync()=>Task.FromResult(new AuthenticationState(new(new ClaimsIdentity([new("party_id",actor),new(ClaimTypes.Role,"ReadRole")],"fixture"))));
        internal void Changed()=>NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
    [Theory]
    [InlineData("E","current")][InlineData("F","current")][InlineData("WEF","current")]
    [InlineData("E","absent")][InlineData("E","failed")][InlineData("E","permission")][InlineData("E","foreign-row")]
    [InlineData("E","close")][InlineData("E","page")][InlineData("E","inputs")]
    [InlineData("E","late-close")][InlineData("E","late-company")][InlineData("E","late-field")][InlineData("E","late-auth")][InlineData("E","late-selection")][InlineData("E","late-account")]
    public async Task LandingOpensOneOriginalReportsExistingReadOnlyEvidenceAndFencesClosedOrChangedViews(string targets,string mode)
    {
        // Public report/status projections are controlled here; native/API acceptance remains separately qualified.
        var status=MaintenanceAssetReturnStatusTests.QualifiedStatus(targets,recorded:mode!="absent");
        var report=MaintenanceAssetReturnStateTests.Review().Assets.Report with{Company=status.Context.Company,Database=status.Context.Database,Query=status.Query};
        var second=report with{Query=report.Query with{SubmissionId="another-original-report"}};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),status.Context.ActorUserId,new(status.Context.Company,["ReadRole"],["Maintenance.ReturnToService"])));
        var fields=new Mock<IFieldSelectionContext>();fields.Setup(value=>value.GetCurrentFieldIdAsync()).ReturnsAsync(status.Context.FieldId);
        var auth=new EvidenceAuth(status.Context.ActorUserId);var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReadMaintenanceRecordedWorkPageAsync(It.IsAny<CompanyMaintenanceRecordedWorkWindow>(),1,8,It.IsAny<CancellationToken>())).Returns((CompanyMaintenanceRecordedWorkWindow window,int page,int size,CancellationToken _)=>Task.FromResult(CompanyMaintenanceRecordedWorkPage.Create(status.Context,window,page,size,mode=="late-selection"?[report,second]:[report],DateTime.UtcNow)));
        var pending=new TaskCompletionSource<CompanyMaintenanceAssetReturnStatus>(TaskCreationOptions.RunContinuationsAsynchronously);var reads=0;
        client.Setup(value=>value.ReadMaintenanceAssetReturnStatusAsync(status.Query,It.IsAny<CancellationToken>())).Returns(()=>{reads++;if(mode=="failed")throw new HttpRequestException("Unavailable native source");return mode.StartsWith("late-",StringComparison.Ordinal)?pending.Task:Task.FromResult(status);});
        var services=new ServiceCollection();services.AddLogging();services.AddMudServices();services.AddSingleton(company);services.AddScoped<MaintenanceAssetEvidenceState>();services.AddSingleton(client.Object);services.AddSingleton(fields.Object);services.AddSingleton<AuthenticationStateProvider>(auth);services.AddSingleton<IComponentActivator,Components>();services.AddSingleton<IJSRuntime,NoBrowser>();services.AddSingleton(new OilGasCallFailures(Mock.Of<IFailureReporter>()));
        await using var provider=services.BuildServiceProvider();await using var renderer=new HtmlRenderer(provider,provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async()=>
        {
            var root=await renderer.RenderComponentAsync<MaintenanceRecordedWorkQueue>();var components=(Components)provider.GetRequiredService<IComponentActivator>();var queue=components.Queue;
            void Change(string name,params object?[] args){typeof(MaintenanceRecordedWorkQueue).GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queue,args);typeof(MaintenanceRecordedWorkQueue).GetMethod("Changed",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queue,null);}
            Assert.Equal(0,reads);Assert.Null(components.Evidence);
            var selected=mode=="foreign-row"?status.Query with{SubmissionId="not-on-current-page"}:status.Query;
            Change("ShowEvidence",selected);await Task.Yield();
            if(mode=="foreign-row"){Assert.Null(components.Evidence);Assert.DoesNotContain("Close asset evidence",root.ToHtmlString());return;}
            Assert.NotNull(components.Evidence);Assert.Contains("Selected work order",root.ToHtmlString());Assert.Equal(0,reads);
            if(mode=="permission")company.TrySelect(company.Clear(),status.Context.ActorUserId,new(status.Context.Company,["ReadRole"],[]));
            var read=(Task)typeof(MaintenanceAssetEvidencePanel).GetMethod("ReadStatusAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(components.Evidence,null)!;
            if(mode.StartsWith("late-",StringComparison.Ordinal))
            {
                if(mode=="late-close")Change("CloseEvidence");
                if(mode=="late-company")company.Clear();
                if(mode=="late-field")fields.Raise(value=>value.CurrentFieldChanged+=null!,"another-field");
                if(mode=="late-auth")auth.Changed();
                if(mode=="late-selection")Change("ShowEvidence",second.Query);
                if(mode=="late-account")Assert.True(company.TrySelect(company.Clear(),"another-account",new(status.Context.Company,["ReadRole"],["Maintenance.ReturnToService"])));
                await Task.Yield();pending.SetResult(status);
            }
            await read;await Task.Yield();var html=System.Net.WebUtility.HtmlDecode(root.ToHtmlString());
            if(mode is "current" or "absent")
            {
                Assert.Contains("Current native facility and equipment evidence",html);Assert.Contains(mode=="absent"?"No recorded asset return was found":"Recorded asset return",html);
                Assert.Contains(targets.Contains('F')?"Affected facility":"Installation parent (read only)",html);
                if(targets.Contains('E'))Assert.Contains("EquipmentFacet: RECORDED_TARGET",html);
                Assert.Contains("Authorized work reports in this UTC period: 1",html);
            }
            if(mode is "close" or "page" or "inputs")
            {
                Assert.Contains("Current native facility and equipment evidence",html);
                if(mode=="page")await (Task)typeof(MaintenanceRecordedWorkQueue).GetMethod("LoadAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(queue,[1])!;
                else Change(mode=="close"?"CloseEvidence":"InputsChanged");
                await Task.Yield();html=root.ToHtmlString();
            }
            if(mode is not ("current" or "absent"))
            {
                Assert.DoesNotContain("Current native facility and equipment evidence",html);Assert.DoesNotContain("No recorded asset return was found",html);
                Assert.Null(provider.GetRequiredService<MaintenanceAssetEvidenceState>().ReturnStatus);
            }
            Assert.DoesNotContain("Private retained original return rationale",html);Assert.DoesNotContain("COMMAND_JSON",html);
            if(mode=="permission")Assert.Equal(0,reads);
        });
        client.Verify(value=>value.RecordMaintenanceAssetReturnAsync(It.IsAny<CompanyMaintenanceAssetReturnCommand>(),It.IsAny<CancellationToken>()),Times.Never);
        client.Verify(value=>value.ReviewMaintenanceAssetReturnAsync(It.IsAny<CompanyWorkOrderCompletionQuery>(),It.IsAny<CancellationToken>()),Times.Never);
    }
}
#endif
