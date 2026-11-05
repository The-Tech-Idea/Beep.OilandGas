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
public sealed class WorkOrderMaterialReadinessTests
{
    internal static CompanyWorkOrderMaterialReadinessReview Review(string mode="original")
    {
        var permit=WorkOrderExecutionPermitStateTests.ApprovalReview();var item=CompanyInventorySourceReview.Copy(WorkOrderMaterialSourceTests.Source().Inventory.Item);var task=permit.Proposal.Task;var query=new CompanyWorkOrderMaterialSourceQuery(task,item.INVENTORY_ITEM_ID,2m,"EA");var start=permit.Proposal.ValidFromUtc.AddMinutes(-30);var company=permit.Context.Company;
        var originalInventory=CompanyInventorySourceReview.Create(company,permit.Proposal.Context.ActorUserId,item,[]);var originalSource=new CompanyWorkOrderMaterialSourceReview(permit.OriginalSource.Stage.Context with{Database=new("production","production-fingerprint")},query,permit.OriginalSource,originalInventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,originalInventory));var original=CompanyWorkOrderMaterialAllocationReview.Create(originalSource,[],start);var command=new CompanyWorkOrderMaterialAllocationCommand(original,Guid.NewGuid(),mode=="window"?permit.Proposal.ValidFromUtc.AddMinutes(1):start,mode=="expired"?permit.ComparedAtUtc.AddMinutes(-1):permit.Proposal.ValidToUtc,"Private original reservation rationale");var row=command.Prepare(start);var reservation=CompanyWorkOrderMaterialReservationFacts.From(row);
        permit=permit with{Proposal=permit.Proposal with{MaterialReference=mode=="reference"?"reported unrelated reference":row.ALLOCATION_ID}};permit=permit with{Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(permit.Proposal,permit.OriginalSource,permit.CurrentSource,permit.ComparedAtUtc,permit.PrerequisiteEvidenceVersion)};
        if(mode=="unknown-stock")item.QUANTITY_ON_HAND=null;if(mode=="insufficient-stock")item.QUANTITY_ON_HAND=1m;if(mode=="changed-unit")item.UNIT_OF_MEASURE="kg";
        var currentInventory=CompanyInventorySourceReview.Create(company,permit.Context.ActorUserId,item,[]);var currentSource=new CompanyWorkOrderMaterialSourceReview(permit.CurrentSource.Stage.Context with{Database=new("production","production-fingerprint")},query,permit.CurrentSource,currentInventory,CompanyWorkOrderMaterialSourceReview.GetBlockers(query,currentInventory));var releases=mode=="released"?new[]{new CompanyWorkOrderMaterialReleaseCommand(new(command,row),Guid.NewGuid(),"Private cancelled requirement rationale").Prepare(permit.ComparedAtUtc.AddSeconds(-1))}:Array.Empty<WORK_ORDER_MATERIAL_RELEASE>();var current=CompanyWorkOrderMaterialAllocationReview.Create(currentSource,[row],releases,permit.ComparedAtUtc);var utc=permit.ComparedAtUtc.AddSeconds(1);var request=new CompanyWorkOrderMaterialReadinessQuery(new(task,permit.Proposal.IntentId),command.IntentId);
        return new(request,permit,reservation,current,utc,CompanyWorkOrderMaterialReadinessReview.GetBlockers(permit,reservation,current,utc));
    }
    [Theory][InlineData("original")][InlineData("released")][InlineData("reference")][InlineData("window")][InlineData("expired")][InlineData("forged")][InlineData("foreign")]
    public void CurrentReadinessRequiresExactPermitReservationValidityReleaseAndFullSource(string mode)
    {
        var review=Review(mode);if(mode=="forged")review=review with{Blockers=["Invented blocker"]};if(mode=="foreign")review=review with{Reservation=review.Reservation with{Company=new("tenant","foreign")}};
        if(mode is "forged" or "foreign"){Assert.Throws<InvalidOperationException>(()=>review.Require(review.PermitReview.Context.Company,"independent",review.Query));return;}review.Require(review.PermitReview.Context.Company,"independent",review.Query);
        if(mode=="original")Assert.Empty(review.Blockers);else Assert.NotEmpty(review.Blockers);Assert.DoesNotContain("COMMAND_JSON",JsonSerializer.Serialize(review));Assert.DoesNotContain("Private original reservation rationale",JsonSerializer.Serialize(review));Assert.DoesNotContain("Private cancelled requirement rationale",JsonSerializer.Serialize(review));
    }
    [Theory][InlineData("unknown-stock")][InlineData("insufficient-stock")][InlineData("changed-unit")]
    public void UnconfirmedCurrentStockOrNativeUnitCannotPublishReadiness(string mode)
    {Assert.Throws<InvalidOperationException>(()=>Review(mode));}
    private sealed class Response(CompanyWorkOrderMaterialReadinessReview original,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {Calls++;Assert.EndsWith("/execution-permit/material-readiness",request.RequestUri!.AbsolutePath);Assert.Equal(original.Query,(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderMaterialReadinessQuery>(token))!);if(mode=="late-company")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="forged"?original with{Blockers=["Invented blocker"]}:original)};}
    }
    [Theory][InlineData("original")][InlineData("permission")][InlineData("forged")][InlineData("late-company")]
    public async Task FullTypedReadinessClientVerifiesCompleteOriginalContextAndFencesLateCompany(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"independent",new(review.PermitReview.Context.Company,["ConfiguredRole"],mode=="permission"?["HSE.ApprovePermit"]:["HSE.ApprovePermit","Inventory.View"])));var response=new Response(review,mode,()=>company.Clear());using var http=new HttpClient(response){BaseAddress=new("https://fixture.invalid/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderPermitMaterialReadinessAsync(review.Query);result.Require(review.PermitReview.Context.Company,"independent",review.Query);});if(mode=="original")Assert.Null(error);else Assert.True(error is UnauthorizedAccessException or InvalidOperationException or OperationCanceledException,error?.ToString());Assert.Equal(mode=="permission"?0:1,response.Calls);
    }
}
#endif