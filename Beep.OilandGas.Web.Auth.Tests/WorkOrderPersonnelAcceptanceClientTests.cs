#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.Client.App;
using Beep.OilandGas.Web.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Data.OilGas;
using TheTechIdeaWeb.Diagnostics;
using Xunit;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderPersonnelAcceptanceClientTests
{
    [Theory][InlineData("author")][InlineData("reference")][InlineData("jurisdiction")][InlineData("result")]
    public void ExactIndependentPendingPersonnelRequirementCannotBeSubstituted(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();var row=CompanyInventorySourceReview.Copy(f.Pending);
        if(mode=="author")row.REQUIREMENT_ACTOR=f.Evidence.Assignment.Roster.Context.ActorUserId;
        if(mode=="reference")row.REGULATORY_REF="another-assignment";
        if(mode=="jurisdiction")row.JURISDICTION="Another reported jurisdiction";
        if(mode=="result")row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareResult(row,new(){FieldId=row.FIELD_ID,InspectionId=row.INSPECTION_ID,ExpectedSourceRevision=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Revision(row),Result="PASS",Notes="Historical result"},"historical-reviewer",DateTime.UtcNow);
        var current=f.Current(row);Assert.ThrowsAny<Exception>(()=>(f.Command with{Review=current}).Require());
    }
    [Theory][InlineData("current")][InlineData("condition")][InlineData("policy")][InlineData("assignment")]
    public void RecordedAcceptanceIsComparedSeparatelyAgainstCurrentSourceWithoutErasingOldReviewVersions(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);var facts=f.Command.Facts(row);var current=f.Current(row);var permit=current.Assignment.CurrentPersonnel.PermitReview;
        if(mode=="condition")
        {
            var conditions=current.Credentials.Conditions.Select(c=>c with{Value=9m}).ToArray();var credentials=CompanyPersonnelCredentialFacts.Create(current.Credentials.Personnel,current.Credentials.Statuses,conditions,DateTime.UtcNow);
            current=current with{Credentials=credentials,Blockers=CompanyPersonnelPolicyEvidenceReview.GetBlockers(current.Assignment,current.Policies,credentials,current.EligibilityEvidenceVersion)};
        }
        if(mode=="policy")facts=facts with{PolicyRevision=new string('F',64)};
        if(mode=="assignment")facts=facts with{AssignmentRevision=new string('F',64)};
        var blockers=CompanyWorkOrderAcceptedPersonnelReview.GetBlockers(permit.Proposal,facts,current,DateTime.UtcNow);
        if(mode=="current")
        {
            Assert.Empty(blockers);var accepted=new CompanyWorkOrderAcceptedPersonnelReview(facts,current,DateTime.UtcNow,blockers);accepted.Require(permit.Proposal,permit.CurrentSource);
            var utc=DateTime.UtcNow;var review=permit with{ComparedAtUtc=utc,PrerequisiteEvidenceVersion=3,CurrentPersonnelAcceptance=accepted,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(permit.Proposal,permit.OriginalSource,permit.CurrentSource,utc,3,null,accepted)};
            review.Require(permit.Context.Company,permit.Context.ActorUserId,current.Assignment.Query.Permit);Assert.Contains(review.Blockers,value=>value.Contains("source documents"));
            foreach(var version in new[]{0,1,2}){var old=permit with{PrerequisiteEvidenceVersion=version,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(permit.Proposal,permit.OriginalSource,permit.CurrentSource,permit.ComparedAtUtc,version)};old.Require(permit.Context.Company,permit.Context.ActorUserId,current.Assignment.Query.Permit);Assert.DoesNotContain("CurrentPersonnelAcceptance",JsonSerializer.Serialize(old));}
        }
        else Assert.NotEmpty(blockers);
    }
    [Theory][InlineData("original")][InlineData("legacy")][InlineData("wrong-reference")][InlineData("unsupported")]
    public void VersionedPersonnelAcceptanceUsesOriginalPermitReferenceAndPreservesLegacyRecovery(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture(requirementProof:mode!="legacy",originalReference:mode!="legacy");var command=mode=="wrong-reference"?f.Command with{RequirementReferenceVersion=0}:mode=="unsupported"?f.Command with{RequirementReferenceVersion=2}:f.Command;var error=Record.Exception(()=>{command.Require();var row=command.Prepare(f.Pending,DateTime.UtcNow);command.RequireRecorded(row);var facts=command.Facts(row);Assert.Equal(command.RequirementReferenceVersion,facts.RequirementReferenceVersion);if(mode=="legacy"){Assert.DoesNotContain("RequirementReferenceVersion",row.PERSONNEL_ACCEPTANCE_DATA_JSON!);Assert.DoesNotContain("RequirementReferenceVersion",JsonSerializer.Serialize(facts));}else{Assert.Equal(command.Review.Assignment.CurrentPersonnel.PermitReview.Proposal.PersonnelReference,command.OriginalInspection.RegulatoryRef);Assert.NotEqual(command.Review.Assignment.Assignment.AssignmentId,command.OriginalInspection.RegulatoryRef);Assert.NotNull(command.OriginalInspection.RequirementRevision);}var observed=f.Current(row);var utc=DateTime.UtcNow;var current=new CompanyWorkOrderAcceptedPersonnelReview(facts,observed,utc,CompanyWorkOrderAcceptedPersonnelReview.GetBlockers(command.Review.Assignment.CurrentPersonnel.PermitReview.Proposal,facts,observed,utc));current.Require(command.Review.Assignment.CurrentPersonnel.PermitReview.Proposal,observed.Assignment.CurrentPersonnel.PermitReview.CurrentSource);Assert.Empty(current.Blockers);});if(mode is "original" or "legacy")Assert.Null(error);else Assert.NotNull(error);
    }
    private sealed class Transport(WorkOrderPersonnelAcceptanceFixture f,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/personnel-acceptance"+(mode=="recovery"?"/recovery":""),request.RequestUri!.AbsolutePath);var command=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelAcceptanceCommand>(token))!;Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(command));command.Require();var row=command.Prepare(f.Pending,DateTime.UtcNow);if(mode=="forged")row.RESULT_NOTES="damaged";if(mode=="late")change();if(mode=="unknown")return new(HttpStatusCode.ServiceUnavailable){Content=new StringContent("Unconfirmed outcome")};return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("record")][InlineData("recovery")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")][InlineData("role")][InlineData("unknown")]
    public async Task FullPersonnelAcceptanceClientKeepsOriginalCompleteBodyAndRejectsChangedOwnerRoleOrOutcome(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"another-reviewer",new(f.Evidence.Assignment.Roster.Context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));var transport=new Transport(f,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{Row row=mode=="recovery"?await client.RecoverWorkOrderPersonnelAcceptanceAsync(f.Command):await client.RecordWorkOrderPersonnelAcceptanceAsync(f.Command);f.Command.RequireRecorded(row);});if(mode is "record" or "recovery")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "actor" or "role"?0:1,transport.Calls);f.Command.Require();
    }
    private sealed class ReferenceTransport(WorkOrderPersonnelAcceptanceFixture f,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/personnel-acceptance/recovery/reference",request.RequestUri!.AbsolutePath);var reference=(await request.Content!.ReadFromJsonAsync<CompanyWorkOrderPersonnelAcceptanceReference>(token))!;Assert.Equal(f.Command.IntentId,reference.IntentId);var row=f.Command.Prepare(f.Pending,DateTime.UtcNow);if(mode=="forged")row.RESULT_NOTES="changed";if(mode=="late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderPersonnelAcceptanceRecovery(f.Command,row))};
        }
    }
    [Theory][InlineData("original")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")]
    public async Task ReferenceClientRestoresOnlyExactOwnedCommandAndRejectsLateOrDamagedEvidence(string mode)
    {
        var f=new WorkOrderPersonnelAcceptanceFixture();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"another-reviewer",new(f.Evidence.Assignment.Roster.Context.Company,["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));var transport=new ReferenceTransport(f,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);var reference=new CompanyWorkOrderPersonnelAcceptanceReference(f.Command.Review.Assignment.Query.Permit,f.Command.InspectionId,f.Command.IntentId);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.RecoverWorkOrderPersonnelAcceptanceReferenceAsync(reference);result.Require(f.Evidence.Assignment.Roster.Context.Company,"another-reviewer",reference);Assert.Equal(JsonSerializer.Serialize(f.Command),JsonSerializer.Serialize(result.Command));});if(mode=="original")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
}
#endif
