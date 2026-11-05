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
public sealed class WorkOrderExecutionPermitApprovalClientTests
{
    internal static CompanyWorkOrderExecutionPrerequisites ExecutionProof(CompanyWorkOrderExecutionPermitApprovalReview review,CompanyWorkOrderRecordedApproval approval)
    {
        var task=approval.Permit.Task;var requirements=new CompanyWorkOrderExecutionRequirements(review.Context with{ActorUserId="executor"},approval,review.CurrentSource.Order,review.CurrentSource.Inspections,
            CompanyWorkOrderExecutionRequirements.Population(task.FieldId,task.WorkOrderId,review.OriginalSource.Inspections),CompanyWorkOrderExecutionRequirements.OrderRevision(review.OriginalSource.Order),DateTime.UtcNow,[]);
        var result=new CompanyWorkOrderExecutionPrerequisites(requirements,review.CurrentMaterialAcceptance!.Reservation,review.CurrentPersonnelAcceptance!.Evidence.Assignment.Assignment,review.CurrentDocumentAcceptance!.Select(row=>row.Acceptance).ToArray(),review.PrerequisiteProofRevision(),false,DateTime.UtcNow,"");
        return result with{Revision=CompanyWorkOrderExecutionPrerequisites.Digest(result)};
    }
    private sealed class PrerequisiteResponse(CompanyWorkOrderExecutionPrerequisites result,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Calls++;Assert.EndsWith("/execution-permit/execution-prerequisites",request.RequestUri!.AbsolutePath);change();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(result)});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("inventory")][InlineData("actor")][InlineData("documents")][InlineData("late-company")]
    public async Task ExecutionPrerequisitesClientRequiresBothCapabilitiesCompleteFactsAndCurrentCompany(string mode)
    {
        var complete=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create(true);var review=complete.Review;var proposal=review.Proposal;var query=new CompanyWorkOrderExecutionPermitReference(proposal.Task,proposal.IntentId);
        var approval=new CompanyWorkOrderRecordedApproval(review.Context.Company,query,proposal.PermitId(),Guid.NewGuid(),proposal.Context.ActorUserId,review.Context.ActorUserId,DateTime.UtcNow,proposal.ValidFromUtc,proposal.ValidToUtc,proposal.PermitType,proposal.Jurisdiction,proposal.ScopeDescription,2,7,review.OriginalSource.RequirementFactsRevision(),new string('F',64));
        var result=ExecutionProof(review,approval);if(mode=="actor")result=result with{Requirements=result.Requirements with{Context=result.Requirements.Context with{ActorUserId="foreign"}}};if(mode=="documents")result=result with{Documents=result.Documents.Take(1).ToArray()};result=result with{Revision=CompanyWorkOrderExecutionPrerequisites.Digest(result)};
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"executor",new(review.Context.Company,["ConfiguredRole"],mode=="permission"?["Inventory.View"]:mode=="inventory"?["Maintenance.ExecuteWork"]:["Maintenance.ExecuteWork","Inventory.View"])));
        var response=new PrerequisiteResponse(result,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var current=await client.ReadWorkOrderExecutionPrerequisitesAsync(query);current.Require(review.Context.Company,"executor",query);Assert.NotSame(result,current);});
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "inventory"?0:1,response.Calls);
    }
    private sealed class RequirementsResponse(CompanyWorkOrderExecutionRequirements requirements,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {Calls++;Assert.EndsWith("/execution-permit/execution-requirements",request.RequestUri!.AbsolutePath);change();return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=JsonContent.Create(requirements)});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("actor")][InlineData("forged-blockers")][InlineData("late-company")]
    public async Task ExecutionRequirementsClientRequiresCurrentActorCompleteComparisonAndCompanyFence(string mode)
    {
        var complete=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create(true);var review=complete.Review;var proposal=review.Proposal;var query=new CompanyWorkOrderExecutionPermitReference(proposal.Task,proposal.IntentId);
        var approval=new CompanyWorkOrderRecordedApproval(review.Context.Company,query,proposal.PermitId(),Guid.NewGuid(),proposal.Context.ActorUserId,review.Context.ActorUserId,DateTime.UtcNow,proposal.ValidFromUtc,proposal.ValidToUtc,proposal.PermitType,proposal.Jurisdiction,proposal.ScopeDescription,2,7,review.OriginalSource.RequirementFactsRevision(),new string('F',64));
        var rows=review.CurrentSource.Inspections.Select(row=>mode=="forged-blockers"?row with{RequiredText="Changed requirement"}:row).ToArray();var result=new CompanyWorkOrderExecutionRequirements(review.Context with{ActorUserId=mode=="actor"?"foreign":"executor"},approval,review.CurrentSource.Order,rows,CompanyWorkOrderExecutionRequirements.Population(query.Task.FieldId,query.Task.WorkOrderId,review.OriginalSource.Inspections),CompanyWorkOrderExecutionRequirements.OrderRevision(review.OriginalSource.Order),DateTime.UtcNow,[]);
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"executor",new(review.Context.Company,["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ViewApprovedPermit"])));var response=new RequirementsResponse(result,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var requirements=await client.ReadWorkOrderExecutionRequirementsAsync(query);requirements.Require(review.Context.Company,"executor",query);Assert.Equal("UNRECORDED",requirements.Requirements.Single(row=>row.InspectionId=="future-operation").Status);});if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
    private sealed class RecordedResponse(CompanyWorkOrderRecordedApproval approval,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {Calls++;Assert.EndsWith("/execution-permit/recorded-approval",request.RequestUri!.AbsolutePath);change();return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK){Content=System.Net.Http.Json.JsonContent.Create(approval)});}
    }
    [Theory][InlineData("current")][InlineData("permission")][InlineData("foreign")][InlineData("late-company")]
    public async Task RecordedApprovalClientUsesPublicFactsWithIndependentCapabilityAndCompanyFence(string mode)
    {
        var complete=Beep.OilandGas.ApiService.Tests.WorkOrderCompleteApprovalFixture.Create();var review=complete.Review;var proposal=review.Proposal;var query=new CompanyWorkOrderExecutionPermitReference(proposal.Task,proposal.IntentId);
        var approval=new CompanyWorkOrderRecordedApproval(mode=="foreign"?new("tenant","foreign"):review.Context.Company,query,proposal.PermitId(),Guid.NewGuid(),proposal.Context.ActorUserId,review.Context.ActorUserId,DateTime.UtcNow,proposal.ValidFromUtc,proposal.ValidToUtc,proposal.PermitType,proposal.Jurisdiction,proposal.ScopeDescription,1,6,review.OriginalSource.RequirementFactsRevision(),new string('F',64));
        using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"executor",new(review.Context.Company,["ConfiguredRole"],mode=="permission"?[]:["Maintenance.ViewApprovedPermit"])));var response=new RecordedResponse(approval,()=>{if(mode=="late-company")company.Clear();});using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.ReadRecordedWorkOrderPermitApprovalAsync(query);result.Require(review.Context.Company,query);Assert.Equal(approval,result);});if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
    private sealed class Response(CompanyWorkOrderExecutionPermitApprovalReview review,string fault,Action change):HttpMessageHandler
    {
        internal int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.Equal("/api/field/current/maintenance-handoffs/execution-permit/approval/review",request.RequestUri!.AbsolutePath);var query=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderExecutionPermitReference>(token))!;Assert.Equal(review.Proposal.Task,query.Task);Assert.Equal(review.Proposal.IntentId,query.IntentId);
            if(fault=="late-company")change();var result=fault=="blockers"?review with{Blockers=[]}:fault=="actor"?review with{Context=review.Context with{ActorUserId="foreign"}}:review;return new(HttpStatusCode.OK){Content=JsonContent.Create(result)};
        }
    }
    [Theory][InlineData("original")][InlineData("permission")][InlineData("role")][InlineData("actor")][InlineData("blockers")][InlineData("late-company")]
    public async Task IndependentTypedReviewRequiresCompleteActorRoleSourceAndBlockers(string fault)
    {
        var review=WorkOrderExecutionPermitStateTests.ApprovalReview();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.Context.Company,fault=="role"?["ForeignRole"]:["ConfiguredRole"],fault=="permission"?[]:["HSE.ApprovePermit"])));var transport=new Response(review,fault,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var query=new CompanyWorkOrderExecutionPermitReference(review.Proposal.Task,review.Proposal.IntentId);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderExecutionPermitApprovalAsync(query);result.Require(review.Context.Company,"independent",query);Assert.Equal(review.RecordedRevision,result.RecordedRevision);});if(fault=="original")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString()??"Unexpected independent review");Assert.Equal(fault=="permission"?0:1,transport.Calls);
    }
}
#endif
