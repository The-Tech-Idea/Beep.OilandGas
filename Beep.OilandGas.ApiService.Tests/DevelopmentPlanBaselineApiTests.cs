using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.Models.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using TheTechIdeaWeb.Diagnostics;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentPlanBaselineApiTests
{
    private static async Task WriteAccess(DevelopmentPlanBaselineNativeTests.Fixture f)
    {
        var db=f.Decision.Receiver.Sender.Catalog.Draft.Db;(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="reviewer")).AccessLevel="WRITE";await db.SaveChangesAsync();
        f.Decision.Receiver.Resources.Setup(service=>service.CheckAssetAccessAsync("reviewer","field","FIELD","WRITE",null)).ReturnsAsync(()=>{
            Assert.DoesNotContain("begin",f.Planning.Native.Transactions.TakeLast(1));Assert.DoesNotContain("begin",f.Decision.Receiver.Sender.Catalog.Workflow.Native.Transactions.TakeLast(1));return new AccessCheckResponse{HasAccess=true};});
    }
    [Fact] public async Task ActualCurrentFinalRecipientApprovesReadsAndRecoversWithoutStartOrCreatePermission()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;var writes=f.Planning.Native.Writes;
        var receipt=await receiver.Service.ApproveBaselineAsync(receiver.User,f.Original);receipt.Require(f.Original);Assert.NotNull(receipt.Result.Baseline);Assert.Equal(writes+1,f.Planning.Native.Writes);
        var read=await receiver.Service.ReadBaselineAsync(receiver.User,f.Original.Review.InstanceId,f.Original.Review.Revision);read.Require(f.Original.Review);
        Assert.NotNull((await receiver.Service.ApproveBaselineAsync(receiver.User,f.Original,true)).Result.Baseline);Assert.Equal(writes+1,f.Planning.Native.Writes);
        Assert.DoesNotContain("Workflow.Start",receiver.Tenant.Company!.Permissions);Assert.DoesNotContain("DevelopmentPlanning.Create",receiver.Tenant.Company.Permissions);
    }
    [Fact] public async Task OriginalRecoveryAndInspectionNeedViewNotNewApprovalOrFieldWriteRights()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;await receiver.Service.ApproveBaselineAsync(receiver.User,f.Original);
        var db=receiver.Sender.Catalog.Draft.Db;db.RemoveRange(await db.RoleClaims.Where(row=>row.ClaimValue=="DevelopmentPlanning.Approve" || row.ClaimValue=="Workflow.Approve").ToArrayAsync());
        (await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="reviewer")).AccessLevel="READ";await db.SaveChangesAsync();await receiver.SelectReceiver();
        var writes=f.Planning.Native.Writes;Assert.NotNull((await receiver.Service.ApproveBaselineAsync(receiver.User,f.Original,true)).Result.Baseline);
        Assert.NotNull((await receiver.Service.ReadBaselineAsync(receiver.User,f.Original.Review.InstanceId,f.Original.Review.Revision)).Baseline);
        await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));Assert.Equal(writes,f.Planning.Native.Writes);
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("grant")][InlineData("write-resource")][InlineData("source")]
    public async Task ChangedCurrentAuthorityCannotWriteFdpBaseline(string fault)
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;var db=receiver.Sender.Catalog.Draft.Db;
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimValue=="DevelopmentPlanning.Approve"));
        if(fault=="grant")(await db.Set<AppUserAssetAccess>().SingleAsync(row=>row.UserId=="reviewer")).AccessLevel="READ";
        if(fault=="write-resource")receiver.Resources.Setup(service=>service.CheckAssetAccessAsync("reviewer","field","FIELD","WRITE",null)).ReturnsAsync(new AccessCheckResponse{HasAccess=false});
        if(fault=="source")f.Planning.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="Changed source";
        await db.SaveChangesAsync();var writes=f.Planning.Native.Writes;await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));Assert.Equal(writes,f.Planning.Native.Writes);
    }
    [Fact] public async Task CurrentGrantRevokedAfterInsertRollsBackActualPlanningBaseline()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;var before=JsonSerializer.Serialize(f.Planning.Native.Rows);
        f.Planning.OnInsert=()=>{var db=receiver.Sender.Catalog.Draft.Db;db.Set<AppUserAssetAccess>().Single(row=>row.UserId=="reviewer").IsActive=false;db.SaveChanges();};
        await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));Assert.Equal(before,JsonSerializer.Serialize(f.Planning.Native.Rows));Assert.Equal("rollback",f.Planning.Native.Transactions.Last());
    }
    [Fact] public async Task ChangedClosingLifecycleAuditLeavesOriginalOutcomeUnconfirmedUntilReadOnlyReconciliation()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;
        var approval=receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>().Single();var notes=approval.APPROVAL_NOTES;f.Planning.OnInsert=()=>approval.APPROVAL_NOTES="Changed after review before closing confirmation";
        await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));Assert.Equal("commit",f.Planning.Native.Transactions.Last());var writes=f.Planning.Native.Writes;
        await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original,true));receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES=notes;
        Assert.NotNull((await receiver.Service.ApproveBaselineAsync(receiver.User,f.Original,true)).Result.Baseline);Assert.Equal(writes,f.Planning.Native.Writes);
    }
    [Fact] public async Task ActualTypedHttpApprovesThenReloadsExactOriginalBaselineAndRecoversWithoutDuplicate()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;
        await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            services.AddSingleton<Beep.OilandGas.DevelopmentPlanning.Services.IDevelopmentPlanService>(f.Planning.Service);services.AddSingleton<IDevelopmentPlanRequestAuthority>(receiver.Sender.Catalog.Draft.Authority);
            services.AddScoped<IDevelopmentPlanBaselineService>(provider=>receiver.Build(provider.GetRequiredService<IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredReviewRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(receiver.Tenant.Company!.Scope,receiver.Tenant.Company.Roles.ToArray(),receiver.Tenant.Company.Permissions.ToArray()));
        var client=new Beep.OilandGas.Web.Services.DevelopmentServiceClient(new(host.Client,Mock.Of<IFailureReporter>(),company),Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Web.Services.DevelopmentServiceClient>.Instance);
        Assert.Null((await client.GetDraftBaselineAsync(f.Original.Review.InstanceId,f.Original.Review.Revision)).Baseline);
        var receipt=await client.ApproveDraftBaselineAsync(f.Original);receipt.Require(f.Original);var writes=f.Planning.Native.Writes;
        Assert.NotNull((await client.GetDraftBaselineAsync(f.Original.Review.InstanceId,f.Original.Review.Revision)).Baseline);Assert.NotNull((await client.RecoverDraftBaselineAsync(f.Original)).Result.Baseline);Assert.Equal(writes,f.Planning.Native.Writes);
    }
    [Fact] public async Task EarlierStageRecipientCannotApproveBaselineWithoutTheConfiguredFinalRole()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();DevelopmentReviewDecisionTests.ConfigureInvestmentRecipient(f.Decision);await f.Decision.Prepare(2);await f.Decision.Decide();
        var receiver=f.Decision.Receiver;await receiver.SelectReceiver("investment-reviewer",role:"InvestmentReviewer");
        f.Decision.Command=new(await receiver.Read(),Guid.NewGuid(),"APPROVE","Final investment review stage approved");await f.Decision.Decide();await receiver.SelectReceiver();
        f.Original=new(await receiver.Service.ReadProgressAsync(receiver.User,f.Decision.Command.Review.InstanceId),Guid.NewGuid(),"Attempted earlier-role baseline approval");await WriteAccess(f);
        var writes=f.Planning.Native.Writes;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));Assert.Equal(writes,f.Planning.Native.Writes);
    }
    [Fact] public async Task AnotherAuthorizedCompanyWithSameActorRoleAndFieldCannotApproveOrInspectOriginalBaseline()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;var db=receiver.Sender.Catalog.Draft.Db;
        db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="foreign",UserId="reviewer",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
        db.Add(new AppUserRoleExtension{UserId="reviewer",RoleId="review-role",TenantId="tenant",BusinessAssociateId="foreign",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});await db.SaveChangesAsync();await receiver.SelectReceiver(selected:new("tenant","foreign"));
        var writes=f.Planning.Native.Writes;await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));
        await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ReadBaselineAsync(receiver.User,f.Original.Review.InstanceId,f.Original.Review.Revision));Assert.Equal(writes,f.Planning.Native.Writes);
    }
    [Fact] public async Task OriginalMakerCannotApproveEvenIfTheDisplayedReviewActorIsChangedToTheMaker()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();var review=f.Original.Review with{Scope=f.Original.Review.Scope with{ActorUserId="planner"}};review=review with{Revision=review.ContentRevision()};
        var command=f.Original with{Review=review};Assert.Throws<InvalidOperationException>(command.Require);Assert.Equal(1,f.Planning.Native.Writes);
    }
    [Fact] public async Task NativeCommitResponseLossAfterApiAdmitsWriteRecoversOriginalWithoutAnotherBaseline()
    {
        using var f=new DevelopmentPlanBaselineNativeTests.Fixture();await f.Prepare();await WriteAccess(f);var receiver=f.Decision.Receiver;
        f.Planning.OnInsert=()=>f.Planning.Native.FailCommit=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>receiver.Service.ApproveBaselineAsync(receiver.User,f.Original));Assert.Equal(2,f.Planning.Native.Writes);
        f.Planning.Native.FailCommit=false;f.Planning.OnInsert=null;var writes=f.Planning.Native.Writes;
        Assert.NotNull((await receiver.Service.ApproveBaselineAsync(receiver.User,f.Original,true)).Result.Baseline);Assert.Equal(writes,f.Planning.Native.Writes);
    }
}
#endif
