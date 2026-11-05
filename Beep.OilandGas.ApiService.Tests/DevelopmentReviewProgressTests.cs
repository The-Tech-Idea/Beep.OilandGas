using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentReviewProgressTests
{
    internal static async Task SelectPlanner(DevelopmentReviewDecisionTests.Fixture f)
    {
        var db=f.Receiver.Sender.Catalog.Draft.Db;
        db.Roles.Add(new(){Id="planner-role",Name="ConfiguredPlanner",NormalizedName="CONFIGUREDPLANNER"});db.UserRoles.Add(new(){UserId="planner",RoleId="planner-role"});
        db.Add(new AppUserRoleExtension{UserId="planner",RoleId="planner-role",TenantId="tenant",BusinessAssociateId="company",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        foreach(var permission in new[]{"DevelopmentPlanning.View","Workflow.View"})db.RoleClaims.Add(new(){RoleId="planner-role",ClaimType="permission",ClaimValue=permission});
        db.Add(new AppUserAssetAccess{Id="planner-field",UserId="planner",DatabaseScope=f.Receiver.Sender.Catalog.Draft.Context.Database.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="READ",IsActive=true});
        await db.SaveChangesAsync();await f.Receiver.SelectReceiver("planner",role:"ConfiguredPlanner");
    }
    [Theory][InlineData("initial")][InlineData("progress")][InlineData("approved")][InlineData("rejected")]
    public async Task OriginalRecipientReadsCanonicalRecordedStagesAndHistoryWithoutPendingTaskOrRecoveryInput(string state)
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare(state=="progress"?2:1);
        if(state!="initial"){f.Command=f.Command with{Decision=state=="rejected"?"REJECT":"APPROVE"};await f.Decide();}
        var writes=f.Mutations;var result=await f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Command.Review.InstanceId);result.Require(f.Receiver.Tenant.Company!.Scope,"reviewer",result.InstanceId);
        Assert.Equal(state switch{"initial"=>"INITIAL","progress"=>"stage-2","approved"=>"REVIEW_APPROVED",_=>"REVIEW_REJECTED"},result.WorkflowState);
        Assert.Equal(state=="initial"?1:2,result.History.Count);Assert.Equal("DRAFT",result.Plan.FDP_STATUS);Assert.Equal(writes,f.Mutations);
        if(state!="initial"){var stage=result.Stages[0];Assert.Equal("reviewer",stage.ActorUserId);Assert.Equal(f.Command.Notes,stage.Rationale);Assert.NotNull(stage.ApprovalId);Assert.NotNull(stage.DecidedAtUtc);}
        Assert.DoesNotContain("DevelopmentReviewDecisions",JsonSerializer.Serialize(result));Assert.DoesNotContain("ReceivingSteps",JsonSerializer.Serialize(result));
    }
    [Fact] public async Task ViewOnlyRequesterRediscoversCompletedReviewWithoutApproverWorkspaceOrStartPermission()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await f.Decide();await SelectPlanner(f);var source=f.Receiver.Sender.Original.Source;var writes=f.Mutations;
        var result=await f.Receiver.Service.ReadForSourceAsync(f.Receiver.User,source);Assert.NotNull(result);Assert.Equal("REVIEW_APPROVED",result.WorkflowState);Assert.Equal("planner",result.Scope.ActorUserId);
        Assert.Null(await f.Receiver.Sender.Catalog.Draft.Db.Set<AppUserPersona>().SingleOrDefaultAsync(row=>row.UserId=="planner"));
        Assert.DoesNotContain("DevelopmentPlanning.Approve",f.Receiver.Tenant.Company!.Permissions);Assert.DoesNotContain("Workflow.Start",f.Receiver.Tenant.Company.Permissions);Assert.Equal(writes,f.Mutations);
    }
    [Fact] public async Task ReviewerCanReadHistoryWithCurrentViewAuthorityAfterApprovalPermissionIsRemoved()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await f.Decide();var db=f.Receiver.Sender.Catalog.Draft.Db;
        db.RemoveRange(await db.RoleClaims.Where(row=>row.ClaimValue=="DevelopmentPlanning.Approve" || row.ClaimValue=="Workflow.Approve").ToArrayAsync());await db.SaveChangesAsync();await f.Receiver.SelectReceiver();
        Assert.Equal("REVIEW_APPROVED",(await f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Command.Review.InstanceId)).WorkflowState);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.DecideAsync(f.Receiver.User,f.Command,true));
    }
    [Theory][InlineData("role")][InlineData("view")][InlineData("grant")][InlineData("workspace")][InlineData("source")][InlineData("audit")][InlineData("binding")]
    public async Task HistoricalRecipientStillNeedsCurrentQualifiedAuthorityAndUnchangedCanonicalEvidence(string fault)
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await f.Decide();Assert.NotNull(await f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Command.Review.InstanceId));var db=f.Receiver.Sender.Catalog.Draft.Db;
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";
        if(fault=="view")db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimValue=="Workflow.View"));
        if(fault=="grant")(await db.Set<AppUserAssetAccess>().SingleAsync()).IsActive=false;
        if(fault=="workspace")db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimType==Beep.OilandGas.Repository.RepositoryPersonaService.WorkspaceClaimType));
        if(fault=="source")f.Receiver.Sender.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="Changed source";
        if(fault=="audit")f.Receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES="Changed audit";
        if(fault=="binding")f.Receiver.Sender.Catalog.Binding.ConcurrencyStamp="Changed";
        await db.SaveChangesAsync();var writes=f.Mutations;await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Command.Review.InstanceId));Assert.Equal(writes,f.Mutations);
    }
    [Fact] public async Task LookupDistinguishesCheckedAbsenceFromNativeFailureAndDuplicateOwnershipWithoutWrites()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await SelectPlanner(f);var native=f.Receiver.Sender.Catalog.Workflow.Native;var process=native.Rows.OfType<PROCESS_INSTANCE>().Single();native.Rows.Remove(process);
        Assert.Null(await f.Receiver.Service.ReadForSourceAsync(f.Receiver.User,f.Receiver.Sender.Original.Source));native.Rows.Add(process);native.Rows.Add(process);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.ReadForSourceAsync(f.Receiver.User,f.Receiver.Sender.Original.Source));Assert.Equal(0,f.Mutations);
    }
    [Fact] public async Task ForeignCompanyCannotReadRecordedHistoryOrCountItAsItsSourceReview()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();var db=f.Receiver.Sender.Catalog.Draft.Db;
        db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="foreign",UserId="reviewer",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserRoleExtension{UserId="reviewer",RoleId="review-role",TenantId="tenant",BusinessAssociateId="foreign",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});await db.SaveChangesAsync();await f.Receiver.SelectReceiver(selected:new("tenant","foreign"));
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Command.Review.InstanceId));Assert.Equal(0,f.Mutations);
    }
    [Fact] public async Task ActualTypedHttpPlannerFindsCompletedReviewAfterClearingOriginalInputWithViewPermissionsOnly()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await f.Decide();await SelectPlanner(f);var source=f.Receiver.Sender.Original.Source;
        f.Receiver.Sender.Catalog.Draft.Permissions=["DevelopmentPlanning.View","Workflow.View"];
        var draft=f.Receiver.Sender.Catalog.Draft;
        // A new HTTP request must select the freshly reduced permission set, not reuse the sender's earlier write selection.
        var companies=new Beep.OilandGas.Repository.RepositoryCompanyContextResolver(draft.Accounts.Object);var tenant=new Beep.OilandGas.Repository.RepositoryTenantContext();
        tenant.SelectCompany((await companies.ResolveAsync("planner",source.Context.Company))!);var modules=new ModuleConnectionResolver(draft.Db,draft.Drafts.Native.Editor.Object,tenant);
        var selected=new CompanyModuleConnection(modules,tenant,companies);
        draft.Authority=new(new RepositoryRequestAccount(draft.Http,user=>user.FindFirst("party_id")?.Value,draft.Accounts.Object,null!),selected,modules,draft.Fields.Object,draft.Preference.Object,draft.Assets.Object,draft.Grants.Object);
        await using var host=await DevelopmentDraftHttpTests.Start(f.Receiver.Sender.Catalog.Draft,progress:accessor=>f.Receiver.Build(accessor));
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(source.Context.Company,["ConfiguredPlanner"],["DevelopmentPlanning.View","Workflow.View"]));
        var client=new Beep.OilandGas.Web.Services.DevelopmentServiceClient(new(host.Http,Moq.Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company),Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Web.Services.DevelopmentServiceClient>.Instance);
        var writes=f.Mutations;var result=await client.GetDraftReviewStatusAsync(source.PlanId,source.Revision);result.Require(source);Assert.Equal("REVIEW_APPROVED",result.Review!.WorkflowState);
        Assert.Equal(result.Review.Revision,(await client.GetDraftReviewProgressAsync(result.Review.InstanceId)).Revision);Assert.Equal(writes,f.Mutations);
        var denied=await Assert.ThrowsAsync<Beep.OilandGas.Web.Services.OilGasApiException>(()=>client.GetDraftReviewStatusAsync(source.PlanId,new('F',64)));Assert.Equal(System.Net.HttpStatusCode.Conflict,denied.StatusCode);
    }
    [Theory][InlineData("anonymous")][InlineData("grant")][InlineData("audit")]
    public async Task ActualHistoryHttpPreservesAuthenticationDenialAndConflictWithoutExposingPrivateEvidence(string fault)
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await f.Decide();await SelectPlanner(f);
        if(fault=="grant"){var db=f.Receiver.Sender.Catalog.Draft.Db;(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="planner")).IsActive=false;await db.SaveChangesAsync();}
        if(fault=="audit")f.Receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES="Private changed audit";
        await using var host=await DevelopmentDraftHttpTests.Start(f.Receiver.Sender.Catalog.Draft,authenticate:fault!="anonymous",progress:accessor=>f.Receiver.Build(accessor));
        var response=await host.Http.GetAsync("/api/field/current/development-planning/review-handoffs/"+f.Command.Review.InstanceId+"/history");
        Assert.Equal(fault switch{"anonymous"=>System.Net.HttpStatusCode.Unauthorized,"grant"=>System.Net.HttpStatusCode.Forbidden,_=>System.Net.HttpStatusCode.Conflict},response.StatusCode);
        Assert.DoesNotContain("Private changed audit",await response.Content.ReadAsStringAsync());
    }
    [Fact] public async Task ClosingSourceChangeCannotPublishEarlierHistoricalOutcome()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await f.Decide();var reads=f.Receiver.FieldReads;
        f.Receiver.OnFieldRead=()=>{if(f.Receiver.FieldReads==reads+2)f.Receiver.Sender.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="Changed after initial history source";};
        var writes=f.Mutations;await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.ReadProgressAsync(f.Receiver.User,f.Command.Review.InstanceId));Assert.Equal(writes,f.Mutations);
    }
    [Fact] public async Task FailedNativeLookupCannotBeReportedAsCheckedAbsence()
    {
        using var f=new DevelopmentReviewDecisionTests.Fixture();await f.Prepare();await SelectPlanner(f);
        f.Receiver.Sender.Catalog.Workflow.Native.Source.Setup(source=>source.GetEntityAsync("PROCESS_INSTANCE",Moq.It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ThrowsAsync(new InvalidOperationException("Native lookup unavailable"));
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.ReadForSourceAsync(f.Receiver.User,f.Receiver.Sender.Original.Source));Assert.Equal(0,f.Mutations);
    }
}
#endif
