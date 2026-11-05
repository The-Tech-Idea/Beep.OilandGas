using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
#if WORKFLOW_INBOX_FOCUSED
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderSubmissionStateTests
{
    [Theory][InlineData("confirmed")][InlineData("unknown")][InlineData("before-transport")][InlineData("late-company")]
    public async Task OwnWorkReportRetainsOriginalBeforeTransportAndOnlyExactRecoveryCanResolveIt(string mode)
    {
        var f=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission();var proof=f.Command.Review;var context=proof.Inspections.Current.Context;var start=new CompanyWorkOrderExecutionStartReference(proof.Inspections.Start.Permit,f.Start.IntentId);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork"])));
        using var state=new WorkOrderSubmissionState(company);state.Attach(start);var client=new Mock<ICompanyMaintenanceHandoffClient>();var sends=0;var retained=false;CompanyWorkOrderSubmissionCommand? sent=null;WORK_ORDER_EXECUTION_SUBMISSION? recorded=null;
        client.Setup(value=>value.ReviewWorkOrderSubmissionAsync(start,It.IsAny<CancellationToken>())).ReturnsAsync(proof);
        client.Setup(value=>value.SubmitWorkOrderAsync(It.IsAny<CompanyWorkOrderSubmissionCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderSubmissionCommand command,CancellationToken _)=>
        {
            sends++;Assert.True(retained);Assert.Equal(command.IntentId,state.VisibleReference);sent=CompanyInventorySourceReview.Copy(command);recorded=command.Prepare(DateTime.UtcNow);
            if(mode=="unknown")throw new InvalidOperationException("Lost original work-report acknowledgement");if(mode=="late-company")company.Clear();
            return Task.FromResult(new CompanyWorkOrderSubmissionReceipt(context.Company,context.ActorUserId,command.IntentId,recorded.SUBMISSION_ID,recorded.START_ID,recorded.COMPLETED_AT_UTC,recorded.RECORDED_REVISION));
        });
        client.Setup(value=>value.RecoverWorkOrderSubmissionAsync(It.IsAny<CompanyWorkOrderSubmissionReference>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderSubmissionReference reference,CancellationToken _)=>
        {if(sent is null || recorded is null)throw new InvalidOperationException("No confirmed original report");Assert.Equal(sent.IntentId,reference.IntentId);return Task.FromResult(new CompanyWorkOrderSubmissionRecovery(sent,recorded));});
        await state.ReviewAsync(client.Object);Assert.True(state.CanSubmit);
        var error=await Record.ExceptionAsync(()=>state.SubmitAsync(client.Object,f.Command.CompletedAtUtc,"Explicit actual work",[],"Recorded rationale",id=>
        {retained=true;Assert.Equal(id,state.VisibleReference);Assert.NotNull(state.VisibleCommand);if(mode=="before-transport")throw new InvalidOperationException("Original reference could not be retained in route");return Task.CompletedTask;}));
        if(mode is "confirmed" or "late-company")Assert.Null(error);else Assert.NotNull(error);Assert.False(state.CanReview);Assert.Equal(mode=="before-transport"?0:1,sends);
        if(mode=="late-company"){Assert.Null(state.Receipt);Assert.Null(state.VisibleCommand);Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],["Maintenance.ExecuteWork"])));state.Attach(start);}
        var id=state.VisibleReference!.Value;var body=JsonSerializer.Serialize(state.VisibleCommand);state.VisibleCommand!.Review.Inspections.Current.Order.Instructions="Consumer mutation";Assert.Equal(body,JsonSerializer.Serialize(state.VisibleCommand));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.SubmitAsync(client.Object,f.Command.CompletedAtUtc,"Replacement",[],"Replacement",_=>Task.CompletedTask));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,Guid.NewGuid()));
        if(mode=="before-transport")await Assert.ThrowsAsync<InvalidOperationException>(()=>state.RecoverAsync(client.Object,id));else{await state.RecoverAsync(client.Object,id);Assert.NotNull(state.Receipt);Assert.Equal(body,JsonSerializer.Serialize(state.VisibleCommand));}Assert.Equal(mode=="before-transport"?0:1,sends);
    }
    private sealed class Response(object payload,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Calls++;Assert.Contains("/execution-permit/work-submission",request.RequestUri!.AbsolutePath);change();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(payload,payload.GetType())});}
    }
    [Theory][InlineData("review","current")][InlineData("submit","current")][InlineData("recover","current")]
    [InlineData("review","permission")][InlineData("submit","permission")][InlineData("recover","permission")]
    [InlineData("review","late-company")][InlineData("submit","late-company")][InlineData("recover","late-company")]
    public async Task ActualTypedSubmissionClientChecksOwnCompanyAndFencesLateResponses(string action,string mode)
    {
        var command=Beep.OilandGas.ApiService.Tests.WorkOrderExecutionStartFixture.Submission().Command;var context=command.Review.Inspections.Current.Context;var start=new CompanyWorkOrderExecutionStartReference(command.Review.Inspections.Start.Permit,command.Review.Inspections.Start.IntentId);var row=command.Prepare(DateTime.UtcNow);
        object payload=action switch{"review"=>command.Review,"submit"=>new CompanyWorkOrderSubmissionReceipt(context.Company,context.ActorUserId,command.IntentId,row.SUBMISSION_ID,row.START_ID,row.COMPLETED_AT_UTC,row.RECORDED_REVISION),_=>new CompanyWorkOrderSubmissionRecovery(command,row)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),context.ActorUserId,new(context.Company,["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ExecuteWork"])));
        var response=new Response(payload,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(action=="review")(await client.ReviewWorkOrderSubmissionAsync(start)).Require();else if(action=="submit")(await client.SubmitWorkOrderAsync(command)).Require(command);else(await client.RecoverWorkOrderSubmissionAsync(new(start,command.IntentId))).Require(context.Company,context.ActorUserId,new(start,command.IntentId));});
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
}
#endif
