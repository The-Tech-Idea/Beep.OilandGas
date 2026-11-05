using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderBudgetClientTests
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
    public async Task TypedBudgetClientRequiresPreparationAndFencesCompanyChanges(string action,string mode)
    {
        var review=WorkOrderBudgetStateTests.Review();var command=new CompanyWorkOrderBudgetCommand(review,Guid.NewGuid(),"ExplicitAFE","Reviewed work budget",12.875m,DateTime.UtcNow,"Explicit scope and currency");
        var recovery=new CompanyWorkOrderBudgetRecovery(command,command.Prepare(DateTime.UtcNow));object payload=action=="review"?review:recovery;
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["FinanceRole"],mode=="permission"?["Accounting.View"]:["Accounting.View","Accounting.PrepareWorkOrderBudget"])));
        var response=new Response(payload,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};
        var client=new WorkOrderServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company));var reference=new CompanyWorkOrderBudgetReference(review.Query,command.IntentId);
        var error=await Record.ExceptionAsync(async()=>
        {
            if(action=="review")(await client.ReviewBudgetAsync(review.Query)).Require();
            else if(action=="create")(await client.CreateBudgetAsync(command)).Require(review.Context.Company,review.Context.ActorUserId,reference);
            else(await client.RecoverBudgetAsync(reference)).Require(review.Context.Company,review.Context.ActorUserId,reference);
        });
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
}
#endif
