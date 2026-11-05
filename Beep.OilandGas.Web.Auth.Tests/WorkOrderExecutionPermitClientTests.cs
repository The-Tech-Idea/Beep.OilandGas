#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;

public sealed class WorkOrderExecutionPermitClientTests
{
    private sealed class ReferenceResponse(CompanyWorkOrderExecutionPermitIntent original,string fault,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.Equal("/api/field/current/maintenance-handoffs/execution-permit/recovery/reference",request.RequestUri!.AbsolutePath);
            var query=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderExecutionPermitReference>(token))!;Assert.Equal(original.IntentId,query.IntentId);Assert.Equal(original.Task,query.Task);
            if(fault=="late-company")change();var intent=fault=="actor"?original with{Context=original.Context with{ActorUserId="foreign"}}:original;
            return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderExecutionPermitRecovery(intent,WorkOrderExecutionPermitStateTests.Receipt(intent)))};
        }
    }
    [Theory][InlineData("original")][InlineData("actor")][InlineData("permission")][InlineData("late-company")]
    public async Task PrivateOriginalReferenceClientRefusesForeignOrLateRecovery(string fault)
    {
        var review=WorkOrderExecutionPermitStateTests.Review();var original=WorkOrderExecutionPermitStateTests.Proposal(review) with{IntentId=Guid.NewGuid()};using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"issuer",new(review.Context.Company,["ConfiguredRole"],fault=="permission"?[]:["HSE.IssuePermit"])));
        var transport=new ReferenceResponse(original,fault,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var query=new CompanyWorkOrderExecutionPermitReference(original.Task,original.IntentId);var error=await Record.ExceptionAsync(async()=>{var result=await client.RecoverWorkOrderExecutionPermitReferenceAsync(query);Assert.Equal(original,result.Intent);});
        if(fault=="original")Assert.Null(error);else Assert.True(error is InvalidOperationException or UnauthorizedAccessException or OperationCanceledException,error?.ToString()??"Unexpected reference publication");Assert.Equal(fault=="permission"?0:1,transport.Calls);
    }
    private sealed class Response(string fault,Action change):HttpMessageHandler
    {
        internal int Calls;internal CompanyWorkOrderExecutionPermitIntent? Body;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Body=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderExecutionPermitIntent>(token))!;
            Assert.Equal(fault=="recovery"?"/api/field/current/maintenance-handoffs/execution-permit/recovery":"/api/field/current/maintenance-handoffs/execution-permit",request.RequestUri!.AbsolutePath);
            if(fault is "late-company" or "company-return")change();
            var result=new CompanyWorkOrderExecutionPermitReceipt(Body.Context.Company,Body.Context.ActorUserId,Body.IntentId,Body.PermitId(),new string('A',64));if(fault=="receipt")result=result with{Actor="another"};
            return new(HttpStatusCode.OK){Content=JsonContent.Create(result)};
        }
    }
    [Theory][InlineData("allowed")][InlineData("recovery")][InlineData("company")][InlineData("actor")][InlineData("permission")][InlineData("receipt")][InlineData("late-company")][InlineData("company-return")]
    public async Task OriginalPendingPermitClientRetainsBodyAndRefusesForeignOrLateConfirmation(string fault)
    {
        using var company=new CompanyClientContext();var scope=new CompanyScope("tenant","company");var permissions=fault=="permission"?Array.Empty<string>():new[]{"HSE.IssuePermit"};Assert.True(company.TrySelect(company.Clear(),"issuer",new(scope,["ConfiguredRole"],permissions)));
        var intent=new CompanyWorkOrderExecutionPermitIntent(new(scope,new("core","fingerprint"),"issuer","authority","field"),new(CompanyMaintenanceProcessStart.InstanceIdFor(scope,Guid.NewGuid()),"field","order/العالم%2F"),Guid.NewGuid(),new string('B',64),"ElectricalIsolation","Explicit authority","Recorded scope","isolation","risk","personnel","material",DateTime.UtcNow,DateTime.UtcNow.AddHours(8));
        if(fault=="company")intent=intent with{Context=intent.Context with{Company=new("tenant","foreign")}};if(fault=="actor")intent=intent with{Context=intent.Context with{ActorUserId="another"}};
        var transport=new Response(fault,()=>{company.Clear();if(fault=="company-return")Assert.True(company.TrySelect(company.Version,"issuer",new(scope,["ConfiguredRole"],permissions)));});using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        Task<CompanyWorkOrderExecutionPermitReceipt> Call()=>fault=="recovery"?client.RecoverWorkOrderExecutionPermitAsync(intent):client.IssueWorkOrderExecutionPermitAsync(intent);
        if(fault is "allowed" or "recovery"){var receipt=await Call();receipt.Require(intent);Assert.Equal(intent,transport.Body);}
        else {var error=await Record.ExceptionAsync(Call);Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString()??"Unexpected permit confirmation");}
        Assert.Equal(fault is "company" or "actor" or "permission"?0:1,transport.Calls);
    }
}
#endif
