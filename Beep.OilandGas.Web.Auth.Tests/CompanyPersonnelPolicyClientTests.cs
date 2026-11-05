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
public sealed class CompanyPersonnelPolicyClientTests
{
    private sealed class EvidenceTransport(CompanyPersonnelPolicyEvidenceReview review,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;Assert.EndsWith("/personnel-assignments/policy-evidence",request.RequestUri!.AbsolutePath);Assert.Equal(review.Assignment.Query,await request.Content!.ReadFromJsonAsync<CompanyWorkOrderAssignedPersonnelQuery>(token));if(mode=="late")change();return new(HttpStatusCode.OK){Content=JsonContent.Create(mode=="forged"?review with{Blockers=[]}:review)};
        }
    }
    [Theory][InlineData("current")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("actor")][InlineData("role")]
    public async Task HsePolicyEvidenceClientNeedsCurrentSourceRoleButNoPolicyConfigurationGrant(string mode)
    {
        var review=Evidence();var context=review.Assignment.Roster.Context;using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":context.ActorUserId,new(context.Company,mode=="role"?["Other"]:["ConfiguredRole"],mode=="permission"?[]:["HSE.ApprovePermit"])));
        var transport=new EvidenceTransport(review,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://policy.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{var result=await client.ReviewWorkOrderPersonnelPolicyEvidenceAsync(review.Assignment.Query);result.Require(context.Company,context.ActorUserId,review.Assignment.Query);});
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode=="permission"?0:1,transport.Calls);
    }
    internal static CompanyPersonnelPolicyDraft Draft()
    {
        var utc=DateTime.UtcNow;return new("RecordedDuty","CORRECTIVE","Reported jurisdiction","Recorded certificate","reported-authority","CERTIFICATION",["VALID"],[new("TRAINING_HOURS","TRAINING","MINIMUM",8m,"HOUR","Required training document")],utc.AddDays(-1),utc.AddDays(2),"Explicit jurisdiction policy document"){AcceptedEmploymentStatuses=["ACTIVE"],AcceptedEmploymentPositions=[]};
    }
    internal static CompanyPersonnelPolicyCatalog Catalog()
    {
        var context=new CompanyPersonnelPolicyContext(new("tenant","company"),new("lifecycle","core-fingerprint"),"administrator",new string('A',64));return CompanyPersonnelPolicyCatalog.Create(context,[],DateTime.UtcNow);
    }
    internal static CompanyPersonnelPolicyEvidenceReview Evidence(string mode="current")
    {
        var assignment=WorkOrderAssignedPersonnelTests.Review("current","CORRECTIVE");var source=assignment.CurrentPersonnel.Personnel;var permit=assignment.CurrentPersonnel.PermitReview.Proposal;var utc=DateTime.UtcNow;
        var p=Draft() with{DutyCode=assignment.Assignment.DutyCode,Jurisdiction=permit.Jurisdiction,LicenseType=source.License!.Type!,GrantingAuthority=source.License.GrantingAuthority!};
        if(mode=="wrong-jurisdiction")p=p with{Jurisdiction="Another jurisdiction"};
        var ctx=new CompanyPersonnelPolicyContext(source.Context.Company,source.Context.Database,source.Context.ActorUserId,new string('B',64));var admin=ctx with{ActorUserId="administrator"};
        var baseline=CompanyPersonnelPolicyCatalog.Create(admin,[],utc);var command=new CompanyPersonnelPolicyCommand(admin,baseline.Revision,p,1,Guid.NewGuid(),"Private policy maker rationale");var version=CompanyPersonnelPolicyVersion.From(command.Prepare(utc));var policies=CompanyPersonnelPolicyCatalog.Create(ctx,mode=="missing-policy"?[]:[version],DateTime.UtcNow);
        var status=new CompanyPersonnelLicenseStatusFacts(source.Query.EmployeeBaId,source.Query.LicenseId,"status",mode=="wrong-status"?"REVOKED":"VALID","CERTIFICATION",utc.AddDays(-1),utc.AddDays(2),new string('C',64));
        var condition=new CompanyPersonnelLicenseConditionFacts(source.Query.EmployeeBaId,source.Query.LicenseId,"condition","TRAINING_HOURS","TRAINING",mode=="zero"?null:mode=="insufficient"?4m:8m,mode=="unit"?"DAY":"HOUR",mode=="exempt"?"Y":"N",mode=="timezone"?DateTime.SpecifyKind(utc.AddDays(-1),DateTimeKind.Unspecified):utc.AddDays(-1),utc.AddDays(2),"Recorded employee training reference",new string('D',64));
        var credentials=CompanyPersonnelCredentialFacts.Create(source,mode=="duplicate-status"?[status,status with{StatusId="status-other"}]:[status],mode=="missing-condition"?[]:[condition],DateTime.UtcNow);
        var releases=Array.Empty<CompanyWorkOrderPersonnelReleaseFacts>();var roster=CompanyWorkOrderPersonnelAssignmentRoster.Create(assignment.Roster.Context,assignment.Query.Permit,assignment.Roster.Assignments,DateTime.UtcNow,releases);
        assignment=assignment with{Roster=roster,Blockers=CompanyWorkOrderAssignedPersonnelReview.GetBlockers(assignment.Assignment,assignment.CurrentPersonnel,releases)};
        return new(assignment,policies,credentials,CompanyPersonnelPolicyEvidenceReview.GetBlockers(assignment,policies,credentials,1)){EligibilityEvidenceVersion=1};
    }
    [Theory][InlineData("current")][InlineData("missing-policy")][InlineData("wrong-jurisdiction")][InlineData("wrong-status")][InlineData("duplicate-status")][InlineData("zero")][InlineData("insufficient")][InlineData("unit")][InlineData("exempt")][InlineData("timezone")][InlineData("missing-condition")]
    public void ExplicitPolicyAndRecordedSourceNeverBecomeCountryLevelStatusOrMissingValueDefaults(string mode)
    {
        var review=Evidence(mode);var context=review.Assignment.Roster.Context;review.Require(context.Company,context.ActorUserId,review.Assignment.Query);
        Assert.Contains(review.Blockers,row=>row.Contains("Independent PERSONNEL acceptance"));
        if(mode=="current")Assert.Single(review.Blockers);else Assert.True(review.Blockers.Count>1);
        Assert.DoesNotContain("Private policy maker rationale",JsonSerializer.Serialize(review));Assert.Throws<InvalidOperationException>(()=>(review with{Blockers=[]}).Require(context.Company,context.ActorUserId,review.Assignment.Query));
        Assert.Throws<ArgumentException>(()=>(Draft() with{Conditions=[new("LEVEL","UNKNOWN","MINIMUM",0,"LEVEL","source")]}).Require());
    }
    private sealed class Transport(CompanyPersonnelPolicyCommand command,string mode,Action change):HttpMessageHandler
    {
        public int Calls;
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken token)
        {
            Calls++;if(mode=="catalog"){Assert.EndsWith("/personnel-policies/review",request.RequestUri!.AbsolutePath);return new(HttpStatusCode.OK){Content=JsonContent.Create(Catalog())};}
            var row=command.Prepare(DateTime.UtcNow);if(mode=="forged")row.VERSION_NO=99;if(mode=="late")change();
            if(mode=="reference"){Assert.EndsWith("/recovery/reference",request.RequestUri!.AbsolutePath);var query=await request.Content!.ReadFromJsonAsync<CompanyPersonnelPolicyReference>(token);Assert.Equal(command.IntentId,query!.IntentId);return new(HttpStatusCode.OK){Content=JsonContent.Create(new CompanyPersonnelPolicyRecovery(command,row))};}
            Assert.EndsWith(mode=="recovery"?"/recovery":"/record",request.RequestUri!.AbsolutePath);Assert.Equal(JsonSerializer.Serialize(command),JsonSerializer.Serialize(await request.Content!.ReadFromJsonAsync<CompanyPersonnelPolicyCommand>(token)));return new(HttpStatusCode.OK){Content=JsonContent.Create(row)};
        }
    }
    [Theory][InlineData("catalog")][InlineData("record")][InlineData("recovery")][InlineData("reference")][InlineData("forged")][InlineData("late")][InlineData("permission")][InlineData("role")][InlineData("actor")]
    public async Task FullPolicyClientRetainsExactCompanyAdminSourceCommandAndRecordedAudit(string mode)
    {
        var catalog=Catalog();var command=new CompanyPersonnelPolicyCommand(catalog.Context,catalog.Revision,Draft(),1,Guid.NewGuid(),"Explicit policy rationale");using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),mode=="actor"?"other":"administrator",new(catalog.Context.Company,mode=="role"?["Other"]:["Administrator"],mode=="permission"?[]:["HSE.ConfigurePersonnelPolicy"])));
        var transport=new Transport(command,mode,()=>company.Clear());using var http=new HttpClient(transport){BaseAddress=new("https://policy.test/")};var client=new BusinessProcessServiceClient(new ApiClient(http,Mock.Of<IFailureReporter>(),company),NullLogger<BusinessProcessServiceClient>.Instance);
        var error=await Record.ExceptionAsync(async()=>{if(mode=="catalog"){var result=await client.ReviewCompanyPersonnelPoliciesAsync();result.Require();}else if(mode=="reference"){var reference=new CompanyPersonnelPolicyReference(command.RecordId(),command.IntentId);(await client.RecoverCompanyPersonnelPolicyReferenceAsync(reference)).Require(catalog.Context.Company,"administrator",reference);}else{var row=mode=="recovery"?await client.RecoverCompanyPersonnelPolicyAsync(command):await client.RecordCompanyPersonnelPolicyAsync(command);command.RequireRecorded(row);}});
        if(mode is "catalog" or "record" or "recovery" or "reference")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(mode is "permission" or "role" or "actor"?0:1,transport.Calls);
    }
    [Theory][InlineData("lost")][InlineData("retain")][InlineData("company")][InlineData("late-recovery")]
    public async Task PolicyStateFreezesOriginalBeforeTransportAndRequiresExactRecoveryBeforeNextVersion(string mode)
    {
        var catalog=Catalog();using var company=new CompanyClientContext();Assert.True(company.TrySelect(company.Clear(),"administrator",new(catalog.Context.Company,["Administrator"],["HSE.ConfigurePersonnelPolicy"])));using var state=new CompanyPersonnelPolicyState(company);var client=new Mock<ICompanyMaintenanceHandoffClient>();client.Setup(c=>c.ReviewCompanyPersonnelPoliciesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(catalog);await state.ReviewAsync(client.Object);
        var writes=0;CompanyPersonnelPolicyCommand? sent=null;client.Setup(c=>c.RecordCompanyPersonnelPolicyAsync(It.IsAny<CompanyPersonnelPolicyCommand>(),It.IsAny<CancellationToken>())).Returns((CompanyPersonnelPolicyCommand command,CancellationToken _)=>{writes++;sent=CompanyInventorySourceReview.Copy(command);Assert.Equal(command.IntentId,state.VisibleReference!.IntentId);if(mode=="company")company.Clear();throw new IOException("Unconfirmed policy append");});
        await Assert.ThrowsAnyAsync<Exception>(()=>state.AppendAsync(client.Object,Draft(),1,"Explicit policy rationale",reference=>{Assert.NotNull(state.VisibleCommand);Assert.Equal(reference,state.VisibleReference);if(mode=="retain")throw new IOException("Retention failed");return Task.CompletedTask;}));Assert.False(state.CanAppend);Assert.ThrowsAny<Exception>(()=>state.BeginNextReview());Assert.Equal(mode=="retain"?0:1,writes);
        if(mode=="company"){Assert.Null(state.VisibleCommand);Assert.True(company.TrySelect(company.Clear(),"administrator",new(catalog.Context.Company,["Administrator"],["HSE.ConfigurePersonnelPolicy"])));}
        var retained=state.VisibleCommand!;var reference=state.VisibleReference!;client.Setup(c=>c.RecoverCompanyPersonnelPolicyReferenceAsync(reference,It.IsAny<CancellationToken>())).Returns(()=>{if(sent is not null)Assert.Equal(JsonSerializer.Serialize(sent),JsonSerializer.Serialize(retained));if(mode=="late-recovery")company.Clear();return Task.FromResult(new CompanyPersonnelPolicyRecovery(retained,retained.Prepare(DateTime.UtcNow)));});
        await state.RecoverReferenceAsync(client.Object,reference);Assert.Equal(mode=="retain"?0:1,writes);if(mode=="late-recovery")Assert.Null(state.Receipt);else{retained.RequireRecorded(state.Receipt!);state.BeginNextReview();Assert.True(state.CanReview);Assert.Null(state.VisibleReference);}
    }
}
#endif
