#if WORKFLOW_INBOX_FOCUSED
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
namespace Beep.OilandGas.Web.Auth.Tests;
public sealed class WorkOrderAssignedPersonnelTests
{
    internal static CompanyWorkOrderAssignedPersonnelReview Review(string mode="current",string workOrderType="")
    {
        var original=WorkOrderPersonnelReviewFixture.Original(workOrderType);var utc=DateTime.UtcNow;
        if(mode=="not-started")original=original with{ValidFromUtc=utc.AddMinutes(10)};
        var row=original.Prepare(utc);var facts=CompanyWorkOrderPersonnelAssignmentFacts.From(row);
        var actor=mode=="maker"?"independent":"another-reviewer";var context=original.Review.Personnel.Context with{ActorUserId=actor,Revision="current-reviewer-authority"};
        var initial=original.Review.PermitReview.CurrentSource;var stage=initial.Stage with{Context=initial.Stage.Context with{ActorUserId=actor,Revision="current-reviewer-authority"}};
        var source=initial with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,initial.InspectionDatabase,initial.Order,initial.Inspections)};
        var comparison=mode=="expired"?original.ValidToUtc.AddMinutes(1):DateTime.UtcNow;
        var permit=original.Review.PermitReview with{Context=context,CurrentSource=source,ComparedAtUtc=comparison,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(original.Review.PermitReview.Proposal,original.Review.PermitReview.OriginalSource,source,comparison)};
        var employee=mode=="unlinked"?null:original.Review.Personnel.Employment;
        var license=mode=="unlinked"?null:original.Review.Personnel.License;
        if(mode=="changed-license")license=license! with{Type="Changed licence",Revision=new string('C',64)};
        var current=CompanyPersonnelSourceFacts.Create(context,original.Review.Personnel.Query,employee,license,comparison);
        var personnel=new CompanyWorkOrderPersonnelSourceReview(permit,current,CompanyWorkOrderPersonnelSourceReview.GetBlockers(permit.Proposal,current));
        var releases=mode=="cancelled"?new[]{CompanyWorkOrderPersonnelReleaseFacts.From(new CompanyWorkOrderPersonnelReleaseCommand(new(original,row),Guid.NewGuid(),"Private cancellation rationale").Prepare(comparison),row)}:null;
        var roster=CompanyWorkOrderPersonnelAssignmentRoster.Create(context,current.Query.Permit,[facts],comparison,releases);
        var query=new CompanyWorkOrderAssignedPersonnelQuery(current.Query.Permit,facts.AssignmentId);
        return new(query,roster,personnel,CompanyWorkOrderAssignedPersonnelReview.GetBlockers(facts,personnel,roster.Releases));
    }
    [Theory][InlineData("current")][InlineData("unlinked")][InlineData("changed-license")][InlineData("expired")][InlineData("not-started")][InlineData("maker")][InlineData("cancelled")]
    public void PublicOriginalAssignmentAndCurrentEvidenceRemainDistinctWithoutPrivateIntentOrQualificationDefaults(string mode)
    {
        var review=Review(mode);var context=review.Roster.Context;review.Require(context.Company,context.ActorUserId,review.Query);
        Assert.Equal("independent",review.Assignment.AssignedBy);Assert.Contains(review.Blockers,value=>value.Contains("Configured duty/competency/jurisdiction"));
        if(mode=="unlinked"){Assert.Null(review.CurrentPersonnel.Personnel.Employment);Assert.Contains(review.Blockers,value=>value.Contains("employment evidence changed"));}
        if(mode=="changed-license")Assert.Contains(review.Blockers,value=>value.Contains("licence evidence changed"));
        if(mode=="expired")Assert.Contains(review.Blockers,value=>value.Contains("assignment window has expired"));
        if(mode=="not-started")Assert.Contains(review.Blockers,value=>value.Contains("assignment window has not started"));
        if(mode=="maker")Assert.Contains(review.Blockers,value=>value.Contains("assignment maker"));if(mode=="cancelled"){Assert.Single(review.Roster.Releases!);Assert.Contains(review.Blockers,value=>value.Contains("assignment was cancelled"));}
        var body=JsonSerializer.Serialize(review);Assert.DoesNotContain("Private original personnel rationale",body);Assert.DoesNotContain("COMMAND_JSON",body);
        Assert.Throws<InvalidOperationException>(()=>(review with{Blockers=[]}).Require(context.Company,context.ActorUserId,review.Query));
    }
    private sealed class Transport(CompanyWorkOrderAssignedPersonnelReview review,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;if(mode=="roster"){Assert.EndsWith("/personnel-assignments/roster",request.RequestUri!.AbsolutePath);Assert.Equal(review.Query.Permit,await request.Content!.ReadFromJsonAsync<CompanyWorkOrderExecutionPermitReference>(token));return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyWorkOrderPersonnelAssignmentRosterReview(review.CurrentPersonnel.PermitReview,review.Roster))};}
            Assert.EndsWith("/personnel-assignments/current",request.RequestUri!.AbsolutePath);Assert.Equal(review.Query,await request.Content!.ReadFromJsonAsync<CompanyWorkOrderAssignedPersonnelQuery>(token));if(mode=="late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="forged"?review with{Blockers=[]}:review)};
        }
    }
    [Theory][InlineData("current")][InlineData("roster")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")][InlineData("role")]
    public async Task FullRosterAndAssignedPersonnelClientRequireCurrentAccountHseRoleAndExactPublicProof(string mode)
    {
        var review=Review();using var company=new CompanyClientContext();var context=review.Roster.Context;
        Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":context.ActorUserId,new(context.Company,mode=="role"?["OtherRole"]:["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));
        var transport=new Transport(review,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://personnel.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(mode=="roster"){var roster=await client.ReviewWorkOrderPersonnelAssignmentsAsync(review.Query.Permit);roster.Require(context.Company,context.ActorUserId,review.Query.Permit);}else{var current=await client.ReviewWorkOrderAssignedPersonnelAsync(review.Query);current.Require(context.Company,context.ActorUserId,review.Query);}});
        if(mode is "current" or "roster")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
}
#endif
