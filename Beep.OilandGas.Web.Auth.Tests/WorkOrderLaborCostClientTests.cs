#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderLaborCostClientTests
{
    private sealed class Response(object payload,Action changed):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.Equal(HttpMethod.Post,request.Method);Assert.Contains("/budget",request.RequestUri!.AbsolutePath);changed();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(payload)});
        }
    }
    [Theory][InlineData("review","current")][InlineData("create","current")][InlineData("recover","current")]
    [InlineData("review","permission")][InlineData("create","permission")][InlineData("recover","permission")]
    [InlineData("review","late-company")][InlineData("create","late-company")][InlineData("recover","late-company")]
    public async Task TypedActualCostClientRequiresCapabilityAndFencesCompanyChanges(string action,string mode)
    {
        var review=WorkOrderLaborCostStateTests.Review();var command=new CompanyWorkOrderLaborCostCommand(review,Guid.NewGuid(),1m,review.Labor.Unit,review.Budget.Current.Book.FunctionalCurrency,review.ComparedAtUtc,"recorded-category","Explicit valuation basis");
        var recovery=new CompanyWorkOrderLaborCostRecovery(command,command.Prepare(DateTime.UtcNow));object payload=action=="review"?review:recovery;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Budget.Current.Context.ActorUserId,new(review.Budget.Current.Context.Company,["FinanceRole"],mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.RecordWorkOrderActualCost"])));
        var response=new Response(payload,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};
        var client=new WorkOrderServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));var reference=new CompanyWorkOrderLaborCostReference(review.Query,command.IntentId);
        var error=await Record.ExceptionAsync(async()=>
        {
            if(action=="review")(await client.ReviewLaborCostAsync(review.Query)).Require();
            else if(action=="create")(await client.RecordLaborCostAsync(command)).Require(review.Budget.Current.Context.Company,review.Budget.Current.Context.ActorUserId,reference);
            else(await client.RecoverLaborCostAsync(reference)).Require(review.Budget.Current.Context.Company,review.Budget.Current.Context.ActorUserId,reference);
        });
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
}
#endif
