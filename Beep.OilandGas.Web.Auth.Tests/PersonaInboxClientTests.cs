#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Text;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class PersonaInboxClientTests
{
    private sealed class Handler(Func<HttpRequestMessage,CancellationToken,Task<HttpResponseMessage>> read) : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token){Calls++;return read(request,token);}
    }
    private static CompanyClientContext Selected()
    {var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"])));return company;}
    private static UnifiedInbox Result()=>new(){Company=new("tenant","company"),ActorUserId="actor",PersonaCode="ACCOUNTANT",
        CriticalTasks=[new(){TaskId="task",TaskType="REVIEW",WorkflowName="process/حساب%2F",StepName="step",ScopeKind="COMPANY_ACCOUNTING",AssignedRole="ConfiguredCompanyRole",
            EntityType="AP_INVOICE",EntityId="invoice",Priority=1,Route=WorkflowTaskRoute.For("COMPANY_ACCOUNTING","process/حساب%2F","AP_INVOICE")}],Counts=new(){TotalPending=1,Critical=1,Reviews=1}};
    private static HttpResponseMessage Response(object value)=>new(HttpStatusCode.OK){Content=new StringContent(JsonSerializer.Serialize(value),Encoding.UTF8,"application/json")};
    [Theory][InlineData("company")][InlineData("actor")][InlineData("persona")][InlineData("role")][InlineData("route")][InlineData("count")]
    [InlineData("step-name-empty")][InlineData("step-name-long")][InlineData("step-name-control")]
    [InlineData("creator-empty")][InlineData("creator-long")][InlineData("creator-control")]
    public async Task TypedInboxRejectsUnconfirmedResponseBeforeAnyScreenConsumesIt(string fault)
    {
        using var company=Selected();var result=Result();
        switch(fault){case "company":result.Company=new("tenant","foreign");break;case "actor":result.ActorUserId="foreign";break;
            case "persona":result.PersonaCode="BUYER";break;case "role":result.CriticalTasks[0].AssignedRole="foreign";break;
            case "route":result.CriticalTasks[0].Route="https://foreign.invalid";break;case "count":result.Counts.Reviews=2;break;
            case "step-name-empty":result.CriticalTasks[0].StepDisplayName=" ";break;
            case "step-name-long":result.CriticalTasks[0].StepDisplayName=new string('x',513);break;
            case "step-name-control":result.CriticalTasks[0].StepDisplayName="step\u0001";break;
            case "creator-empty":result.CriticalTasks[0].CreatedByUserId=" ";break;
            case "creator-long":result.CriticalTasks[0].CreatedByUserId=new string('x',129);break;
            case "creator-control":result.CriticalTasks[0].CreatedByUserId="account\u0001";break;}
        using var handler=new Handler((request,token)=>Task.FromResult(Response(result)));using var http=new HttpClient(handler){BaseAddress=new("https://api.invalid")};
        var client=new UnifiedTaskInboxService(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetInboxAsync("ACCOUNTANT"));Assert.Equal(1,handler.Calls);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task FilteredTaskClientRejectsForeignRoleOrDuplicateIdentity(bool duplicate)
    {
        using var company=Selected();var row=Result().CriticalTasks[0];var rows=new List<UnifiedTask>{row};
        if(duplicate)rows.Add(row);else row.AssignedRole="foreign";
        using var handler=new Handler((request,token)=>Task.FromResult(Response(rows)));using var http=new HttpClient(handler){BaseAddress=new("https://api.invalid")};
        var client=new UnifiedTaskInboxService(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetFilteredTasksAsync(new(),"ACCOUNTANT"));
    }
    [Fact] public async Task FilteredTasksRefuseAmbiguousCutoffBeforeTransport()
    {
        using var company=Selected();using var handler=new Handler((request,token)=>Task.FromResult(Response(new List<UnifiedTask>())));
        using var http=new HttpClient(handler){BaseAddress=new("https://api.invalid")};var client=new UnifiedTaskInboxService(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        await Assert.ThrowsAsync<ArgumentException>(()=>client.GetFilteredTasksAsync(new(){DueBefore=new DateTime(2026,1,1,0,0,0,DateTimeKind.Unspecified)},"ACCOUNTANT"));
        Assert.Equal(0,handler.Calls);
        Assert.Empty(await client.GetFilteredTasksAsync(new(){DueBefore=new DateTime(2026,1,1,0,0,0,DateTimeKind.Utc)},"ACCOUNTANT")); Assert.Equal(1,handler.Calls);
    }
    [Fact] public async Task MissingCompanyDoesNotSendAnInboxRequest()
    {
        using var company=new CompanyClientContext();using var handler=new Handler((request,token)=>throw new InvalidOperationException("Unexpected transport."));
        using var http=new HttpClient(handler){BaseAddress=new("https://api.invalid")};var client=new UnifiedTaskInboxService(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>client.GetInboxAsync("ACCOUNTANT"));Assert.Equal(0,handler.Calls);
    }
    [Fact] public async Task LateAwayAndBackCompanyResponseIsCancelledByActualTransport()
    {
        using var company=Selected();var waiting=new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var handler=new Handler((request,token)=>{
            Assert.Equal("company",Assert.Single(request.Headers.GetValues(CompanyRequestHeaders.Company)));return waiting.Task;});
        using var http=new HttpClient(handler){BaseAddress=new("https://api.invalid")};var client=new UnifiedTaskInboxService(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        var load=client.GetInboxAsync("ACCOUNTANT");company.Clear();Assert.True(company.TrySelect(company.Version,"actor",new(new("tenant","company"),["ConfiguredCompanyRole"],["Accounting.View"])));
        waiting.SetResult(Response(Result()));await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>load);
    }
    [Fact] public async Task ConflictRemainsAnApiFailureRatherThanAnEmptySuccessfulQueue()
    {
        using var company=Selected();using var handler=new Handler((request,token)=>Task.FromResult(new HttpResponseMessage(HttpStatusCode.Conflict)
            {Content=new StringContent("{\"error\":\"Reload company access and the inbox.\"}",Encoding.UTF8,"application/json")}));
        using var http=new HttpClient(handler){BaseAddress=new("https://api.invalid")};var client=new UnifiedTaskInboxService(new ApiClient(http,Mock.Of<IFailureReporter>(),company));
        var failure=await Assert.ThrowsAsync<OilGasApiException>(()=>client.GetInboxAsync("ACCOUNTANT"));Assert.Equal(HttpStatusCode.Conflict,failure.StatusCode);
    }
}
#endif
