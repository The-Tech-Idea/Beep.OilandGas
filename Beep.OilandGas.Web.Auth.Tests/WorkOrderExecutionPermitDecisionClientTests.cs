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
public sealed class WorkOrderExecutionPermitDecisionClientTests
{
    [Theory][InlineData("record")][InlineData("unknown")][InlineData("retain")][InlineData("late-company")][InlineData("late-field")][InlineData("missing-inventory")]
    public async Task CompleteApprovalRetainsOriginalBeforeTransportAndFencesUncertaintyAndAuthority(string mode)
    {
        var f=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create();var review=f.Review;var query=new CompanyWorkOrderExecutionPermitReference(review.Proposal.Task,review.Proposal.IntentId);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),review.Context.ActorUserId,new(review.Context.Company,["ConfiguredRole"],mode=="missing-inventory"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));
        using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(query,"original-source");var client=new Mock<ICompanyMaintenanceHandoffClient>();
        client.Setup(value=>value.ReviewWorkOrderExecutionPermitApprovalAsync(query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);
        Assert.Equal(mode!="missing-inventory",state.CanApprove);var retained=false;var writes=0;CompanyWorkOrderExecutionPermitDecision? original=null;
        CompanyWorkOrderExecutionPermitDecisionReceipt Receipt(CompanyWorkOrderExecutionPermitDecision command)=>new(command.Review.Context.Company,command.Review.Context.ActorUserId,command.IntentId,command.Review.Proposal.PermitId(),command.Outcome,new string('A',64));
        client.Setup(value=>value.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecision command,CancellationToken _)=>
        {
            Assert.True(retained);Assert.Equal("Approve",command.Outcome);Assert.Equal(System.Text.Json.JsonSerializer.Serialize(command),System.Text.Json.JsonSerializer.Serialize(state.VisibleDecision));original=command;writes++;
            if(mode=="late-company")company.Clear();if(mode=="late-field")state.Attach(null,null);
            return mode=="unknown"?Task.FromException<CompanyWorkOrderExecutionPermitDecisionReceipt>(new HttpRequestException("Unknown acknowledgement")):Task.FromResult(Receipt(command));
        });
        var error=await Record.ExceptionAsync(()=>state.ApproveAsync(client.Object,"Independent complete prerequisite review",id=>
        {
            retained=true;Assert.Equal(id,state.VisibleDecision!.IntentId);if(mode=="retain")throw new IOException("Retention failed");return Task.CompletedTask;
        }));
        Assert.Equal(mode is "retain" or "missing-inventory"?0:1,writes);Assert.False(state.CanApprove);
        if(mode=="record"){Assert.Null(error);Assert.Equal("Approve",state.Receipt!.Outcome);}
        if(mode=="missing-inventory"){Assert.IsType<InvalidOperationException>(error);Assert.Null(state.VisibleReference);Assert.False(retained);}
        if(mode is "late-company" or "late-field")Assert.Null(state.Receipt);
        if(mode=="unknown")
        {
            Assert.IsType<HttpRequestException>(error);Assert.Null(state.Receipt);await Assert.ThrowsAsync<InvalidOperationException>(()=>state.ApproveAsync(client.Object,"Replacement",_=>Task.CompletedTask));
            var reference=new CompanyWorkOrderExecutionPermitDecisionReference(query,original!.IntentId);client.Setup(value=>value.RecoverWorkOrderExecutionPermitDecisionReferenceAsync(reference,It.IsAny<CancellationToken>())).ReturnsAsync(new CompanyWorkOrderExecutionPermitDecisionRecovery(original,Receipt(original)));
            await state.RecoverAsync(client.Object,original.IntentId);Assert.Equal("Approve",state.Receipt!.Outcome);Assert.Equal(1,writes);
        }
    }
    [Theory][InlineData("late-company")][InlineData("late-field")][InlineData("before-transport")]
    public async Task CircuitDecisionStateHidesLateConfirmationAndStopsChangedContextBeforeTransport(string fault)
    {
        var review=WorkOrderExecutionPermitStateTests.ApprovalReview();var query=new CompanyWorkOrderExecutionPermitReference(review.Proposal.Task,review.Proposal.IntentId);using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,["ConfiguredRole"],["HSE.ApprovePermit"])));using var state=new WorkOrderExecutionPermitDecisionState(company);state.Attach(query,"source");var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewWorkOrderExecutionPermitApprovalAsync(query,It.IsAny<CancellationToken>())).ReturnsAsync(review);await state.ReviewAsync(client.Object);var writes=0;
        client.Setup(c=>c.DecideWorkOrderExecutionPermitAsync(It.IsAny<CompanyWorkOrderExecutionPermitDecision>(),It.IsAny<CancellationToken>())).Returns((CompanyWorkOrderExecutionPermitDecision decision,CancellationToken _)=>{writes++;Assert.Equal(decision.IntentId,state.VisibleReference);if(fault=="late-company")company.Clear();else state.Attach(null,null);return Task.FromResult(Receipt(decision));});
        var error=await Record.ExceptionAsync(()=>state.RejectAsync(client.Object,"Original rationale",id=>{Assert.Equal(id,state.VisibleDecision!.IntentId);if(fault=="before-transport")state.Attach(null,null);return Task.CompletedTask;}));
        if(fault=="before-transport")Assert.IsType<OperationCanceledException>(error);else Assert.Null(error);Assert.Equal(fault=="before-transport"?0:1,writes);Assert.Null(state.Receipt);Assert.Null(state.VisibleDecision);Assert.Null(state.VisibleReference);Assert.False(state.CanReview);
    }
    private sealed class Response(CompanyWorkOrderExecutionPermitDecision decision,string fault,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;if(fault=="reference"){Assert.EndsWith("/decision/recovery/reference",request.RequestUri!.AbsolutePath);var query=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderExecutionPermitDecisionReference>(token))!;Assert.Equal(decision.IntentId,query.IntentId);Assert.Equal(decision.ProposalReference(),query.Proposal);return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderExecutionPermitDecisionRecovery(decision,Receipt(decision)))};}
            Assert.EndsWith(fault=="recovery"?"/decision/recovery":"/decision",request.RequestUri!.AbsolutePath);var body=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderExecutionPermitDecision>(token))!;Assert.Equal(System.Text.Json.JsonSerializer.Serialize(decision),System.Text.Json.JsonSerializer.Serialize(body));if(fault=="late-company")change();var receipt=Receipt(decision);if(fault=="receipt")receipt=receipt with{Actor="foreign"};return new(HttpStatusCode.OK){Content=JsonContent.Create(receipt)};
        }
    }
    private static CompanyWorkOrderExecutionPermitDecisionReceipt Receipt(CompanyWorkOrderExecutionPermitDecision decision)=>new(decision.Review.Context.Company,decision.Review.Context.ActorUserId,decision.IntentId,decision.Review.Proposal.PermitId(),"Reject",new string('F',64));
    [Theory][InlineData("confirmed")][InlineData("recovery")][InlineData("reference")][InlineData("permission")][InlineData("actor")][InlineData("receipt")][InlineData("late-company")]
    public async Task FullTypedDecisionClientRetainsBodyAndRefusesForeignOrLateReceipts(string fault)
    {
        var decision=new CompanyWorkOrderExecutionPermitDecision(WorkOrderExecutionPermitStateTests.ApprovalReview(),Guid.NewGuid(),"Reject","Original independent rationale");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),fault=="actor"?"foreign":"independent",new(decision.Review.Context.Company,["ConfiguredRole"],fault=="permission"?[]:["HSE.ApprovePermit"])));var transport=new Response(decision,fault,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(fault=="reference"){var result=await client.RecoverWorkOrderExecutionPermitDecisionReferenceAsync(new(decision.ProposalReference(),decision.IntentId));result.Receipt.Require(decision);}else{var result=fault=="recovery"?await client.RecoverWorkOrderExecutionPermitDecisionAsync(decision):await client.DecideWorkOrderExecutionPermitAsync(decision);result.Require(decision);}});
        if(fault is "confirmed" or "recovery" or "reference")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString()??"Unexpected decision publication");Assert.Equal(fault is "permission" or "actor"?0:1,transport.Calls);
    }
}
#endif
