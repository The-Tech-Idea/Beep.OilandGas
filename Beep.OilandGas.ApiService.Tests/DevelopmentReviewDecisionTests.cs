#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.Repository;
using Beep.OilandGas.Models.Core.Interfaces;
using TheTechIdeaWeb.Diagnostics;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Data.OilGas;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentReviewDecisionTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentAssignedReviewTests.Fixture Receiver=new();
        internal DevelopmentReviewDecision Command=null!;
        internal int Mutations,FailAt,Guards,RevokeAt;
        internal Action? OnMutation;
        internal async Task Prepare(int stages=1)
        {
            await Receiver.Prepare(stages);Command=new(await Receiver.Read(),Guid.NewGuid(),"APPROVE","Independent recorded technical rationale.");Command.Require();
            var native=Receiver.Sender.Catalog.Workflow.Native;
            ErrorsInfo Write(string table,object value,bool insert)
            {
                native.Writes++;Mutations++;if(Mutations==FailAt)return new(){Flag=Errors.Failed};
                var copy=JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!;
                if(insert)native.Rows.Add(copy);
                else
                {
                    var key=table switch{"PROCESS_INSTANCE"=>"PROCESS_INSTANCE_ID","PROCESS_STEP_INSTANCE"=>"PROCESS_STEP_INSTANCE_ID","CROSS_PERSONA_TASK"=>"CROSS_TASK_ID",_=>throw new InvalidOperationException(table)};
                    var id=value.GetType().GetProperty(key)!.GetValue(value);var index=native.Rows.FindIndex(row=>row.GetType()==value.GetType() && Equals(row.GetType().GetProperty(key)!.GetValue(row),id));
                    if(index<0)throw new InvalidOperationException("Missing update row");native.Rows[index]=copy;
                }
                OnMutation?.Invoke();return new(){Flag=Errors.Ok};
            }
            native.Source.Setup(source=>source.InsertEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,true));
            native.Source.Setup(source=>source.UpdateEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>Write(table,value,false));
        }
        internal Task Guard(){Guards++;if(Guards==RevokeAt)throw new UnauthorizedAccessException("Review authority revoked");return Task.CompletedTask;}
        internal Task<DevelopmentPlanSourceReview> Source()
        {
            Assert.DoesNotContain("begin",Receiver.Sender.Catalog.Workflow.Native.Transactions.TakeLast(1));return Task.FromResult(Receiver.Sender.Original.Source);
        }
        internal Task<DevelopmentReviewDecisionReceipt> Decide(bool recover=false)=>recover?
            Receiver.Sender.Catalog.Workflow.Service.RecoverDevelopmentDecisionAsync(Command,Guard,Source):Receiver.Sender.Catalog.Workflow.Service.DecideDevelopmentReviewAsync(Command,Guard,Source);
        public void Dispose()=>Receiver.Dispose();
    }
    [Theory][InlineData("APPROVE","REVIEW_APPROVED")][InlineData("REJECT","REVIEW_REJECTED")]
    public async Task DecisionPersistsCanonicalApprovalAuditAndTaskClosureOnceAndRecoversReadOnly(string decision,string state)
    {
        using var f=new Fixture();await f.Prepare();f.Command=f.Command with{Decision=decision};var receipt=await f.Decide();receipt.Require(f.Command);
        Assert.True(receipt.Recorded);Assert.Equal(state,receipt.WorkflowState);Assert.Null(receipt.NextStepInstanceId);
        var rows=f.Receiver.Sender.Catalog.Workflow.Native.Rows;var approval=Assert.Single(rows.OfType<PROCESS_APPROVAL>());
        Assert.Equal(decision=="APPROVE"?"APPROVED":"REJECTED",approval.APPROVAL_STATUS);Assert.Equal("reviewer",approval.APPROVED_BY);Assert.Equal("planner",approval.REQUESTED_BY);
        Assert.Equal(2,rows.OfType<PROCESS_HISTORY>().Count());Assert.All(rows.OfType<CROSS_PERSONA_TASK>(),row=>Assert.Equal("COMPLETED",row.TASK_STATUS));
        var writes=f.Mutations;Assert.True((await f.Decide()).Recorded);Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);
        Assert.Equal("DRAFT",f.Receiver.Sender.Original.Source.Plan!.FDP_STATUS);
    }
    [Fact] public async Task OrderedThreeStageApprovalRoutesEachNextTaskAndEarlierIntentCanRecoverAfterProgress()
    {
        using var f=new Fixture();await f.Prepare(3);var original=f.Command;var first=await f.Decide();Assert.NotNull(first.NextStepInstanceId);Assert.Equal("stage-2",first.WorkflowState);
        var second=await f.Receiver.Read();Assert.Equal("stage-2",second.StepId);f.Command=new(second,Guid.NewGuid(),"APPROVE","Investment review rationale.");await f.Decide();
        var third=await f.Receiver.Read();Assert.Equal("stage-3",third.StepId);f.Command=new(third,Guid.NewGuid(),"APPROVE","Regulatory review rationale.");Assert.Equal("REVIEW_APPROVED",(await f.Decide()).WorkflowState);
        var writes=f.Mutations;f.Command=original;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);
        Assert.Equal(3,f.Receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>().Count());
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)][InlineData(5)][InlineData(6)][InlineData(7)][InlineData(8)]
    public async Task EveryDecisionMutationBoundaryRollsBackApprovalHistoryStepsAndTasks(int failure)
    {
        using var f=new Fixture();await f.Prepare(3);var native=f.Receiver.Sender.Catalog.Workflow.Native;var before=JsonSerializer.Serialize(native.Rows);f.FailAt=failure;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());Assert.Equal(before,JsonSerializer.Serialize(native.Rows));Assert.Equal("rollback",native.Transactions.Last());
    }
    [Fact] public async Task LostCommitResponseRecoversOriginalWithoutAnotherApprovalOrTask()
    {
        using var f=new Fixture();await f.Prepare(3);var native=f.Receiver.Sender.Catalog.Workflow.Native;native.FailCommit=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide());native.FailCommit=false;var writes=f.Mutations;Assert.True((await f.Decide(true)).Recorded);Assert.Equal(writes,f.Mutations);
        f.Command=f.Command with{Notes="Changed original"};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(writes,f.Mutations);
    }
    [Fact] public async Task CheckedAbsentDecisionRecoveryCannotCreateRowsAndStaleOtherIntentCannotClaimAbsence()
    {
        using var f=new Fixture();await f.Prepare();Assert.False((await f.Decide(true)).Recorded);Assert.Equal(0,f.Mutations);
        await f.Decide();f.Command=f.Command with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));
    }
    [Fact] public async Task MidWriteAuthorityRevocationRollsBackTheWholeDecision()
    {
        using var f=new Fixture();await f.Prepare(3);var native=f.Receiver.Sender.Catalog.Workflow.Native;var before=JsonSerializer.Serialize(native.Rows);
        f.OnMutation=()=>{if(f.Mutations==1)f.RevokeAt=f.Guards+1;};await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Decide());Assert.Equal(before,JsonSerializer.Serialize(native.Rows));
    }
    [Fact] public async Task ActualApiAuthorityRecordsDecisionAndRecoversAfterRoutingWithoutStartOrCreatePermission()
    {
        using var f=new Fixture();await f.Prepare(3);
        var receipt=await f.Receiver.Service.DecideAsync(f.Receiver.User,f.Command);receipt.Require(f.Command);Assert.Equal("stage-2",receipt.WorkflowState);
        Assert.DoesNotContain("Workflow.Start",f.Receiver.Tenant.Company!.Permissions);Assert.DoesNotContain("DevelopmentPlanning.Create",f.Receiver.Tenant.Company.Permissions);
        var writes=f.Mutations;Assert.True((await f.Receiver.Service.DecideAsync(f.Receiver.User,f.Command,true)).Recorded);Assert.Equal(writes,f.Mutations);
        Assert.Equal("stage-2",(await f.Receiver.Read()).StepId);
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("grant")][InlineData("source")]
    public async Task CurrentApiAuthorityAndOriginalNativeSourceAreRequiredBeforeDecisionMutation(string fault)
    {
        using var f=new Fixture();await f.Prepare();var db=f.Receiver.Sender.Catalog.Draft.Db;
        if(fault=="role")(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";
        if(fault=="permission")db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimValue=="Workflow.Approve"));
        if(fault=="grant")(await db.Set<AppUserAssetAccess>().SingleAsync()).IsActive=false;
        if(fault=="source")f.Receiver.Sender.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="changed";
        await db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.DecideAsync(f.Receiver.User,f.Command));Assert.Equal(0,f.Mutations);
    }
    internal static void ConfigureInvestmentRecipient(Fixture f)
    {
        f.Receiver.Sender.BeforeHandoffReview=()=>{
            var db=f.Receiver.Sender.Catalog.Draft.Db;
            db.Users.Add(new(){Id="investment-reviewer",IsActive=true});db.Roles.Add(new(){Id="investment-role",Name="InvestmentReviewer",NormalizedName="INVESTMENTREVIEWER"});
            db.Add(new AppCompanyMembership{TenantId="tenant",BusinessAssociateId="company",UserId="investment-reviewer",GrantedByUserId="administrator",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            db.UserRoles.Add(new(){UserId="investment-reviewer",RoleId="investment-role"});
            db.Add(new AppUserRoleExtension{UserId="investment-reviewer",RoleId="investment-role",TenantId="tenant",BusinessAssociateId="company",ApprovalStatus="Approved",EffectiveFromUtc=DateTime.UtcNow.AddDays(-1)});
            db.Add(new AppPersona{Code="INVESTMENT_REVIEWER",Name="Investment reviewer"});db.Add(new AppUserPersona{UserId="investment-reviewer",TenantId="tenant",BusinessAssociateId="company",PersonaCode="INVESTMENT_REVIEWER",ChangedBy="administrator",ChangedUtc=DateTime.UtcNow});
            foreach(var permission in new[]{"DevelopmentPlanning.View","DevelopmentPlanning.Approve","Workflow.View","Workflow.Approve"})
                db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType="permission",ClaimValue=permission});
            db.RoleClaims.Add(new(){RoleId="investment-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="INVESTMENT_REVIEWER"});
            db.Add(new AppUserAssetAccess{Id="investment-field",UserId="investment-reviewer",DatabaseScope=f.Receiver.Sender.Catalog.Draft.Context.Database.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="READ",IsActive=true});
            db.SaveChanges();
            var definition=f.Receiver.Sender.Catalog.Workflow.Definition;
            // The fixture uses the same explicit serialized step definitions as the production catalog.
            using var document=JsonDocument.Parse(definition.PROCESS_CONFIG_JSON!);
            var steps=document.RootElement.GetProperty("Steps").Deserialize<List<Beep.OilandGas.Models.Processes.ProcessStepDefinition>>()!;
            steps[1].RequiredRoles=["InvestmentReviewer"];steps[1].StepName="Independent investment review";definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=steps});
        };
    }
    [Fact] public async Task TechnicalDecisionMovesActualInboxToDifferentInvestmentPersonaAndKeepsEarlierRecoveryReadOnly()
    {
        using var f=new Fixture();ConfigureInvestmentRecipient(f);await f.Prepare(2);var technical=f.Command;
        var receipt=await f.Receiver.Service.DecideAsync(f.Receiver.User,technical);Assert.NotNull(receipt.NextStepInstanceId);
        var technicalInbox=Assert.IsType<UnifiedInbox>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await f.Receiver.Inbox().Inbox(null,default)).Value);Assert.Equal(0,technicalInbox.Counts.TotalPending);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Read());await f.Receiver.SelectReceiver("investment-reviewer",role:"InvestmentReviewer");
        var assigned=await f.Receiver.Read();Assert.Equal("INVESTMENT_REVIEWER",assigned.PersonaCode);Assert.Equal("stage-2",assigned.StepId);Assert.Equal(technical.Review.SourceRevision,assigned.SourceRevision);
        var inbox=Assert.IsType<UnifiedInbox>(Assert.IsType<Microsoft.AspNetCore.Mvc.OkObjectResult>(await f.Receiver.Inbox().Inbox(null,default)).Value);Assert.Equal(1,inbox.Counts.TotalPending);
        f.Command=new(assigned,Guid.NewGuid(),"APPROVE","Investment stage rationale");Assert.Equal("REVIEW_APPROVED",(await f.Receiver.Service.DecideAsync(f.Receiver.User,f.Command)).WorkflowState);
        await f.Receiver.SelectReceiver();var writes=f.Mutations;Assert.True((await f.Receiver.Service.DecideAsync(f.Receiver.User,technical,true)).Recorded);Assert.Equal(writes,f.Mutations);
    }
    [Theory][InlineData(false)][InlineData(true)]
    public async Task NextPersonaGrantRevocationBeforeOrDuringDecisionNeverLeavesPartialRouting(bool during)
    {
        using var f=new Fixture();ConfigureInvestmentRecipient(f);await f.Prepare(2);var db=f.Receiver.Sender.Catalog.Draft.Db;var native=f.Receiver.Sender.Catalog.Workflow.Native;
        var before=JsonSerializer.Serialize(native.Rows);void Revoke(){db.Set<AppUserAssetAccess>().Single(row=>row.UserId=="investment-reviewer").IsActive=false;db.SaveChanges();}
        if(during)f.OnMutation=()=>{if(f.Mutations==1)Revoke();};else Revoke();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Service.DecideAsync(f.Receiver.User,f.Command));Assert.Equal(before,JsonSerializer.Serialize(native.Rows));
        if(during)Assert.Equal("rollback",native.Transactions.Last());else Assert.Equal(0,f.Mutations);
    }
    [Fact] public async Task ActualTypedHttpDecisionAndReadOnlyRecoveryUseCanonicalApprovalHistoryAndTaskRows()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await AccountingSourceHttpTests.Start(configure:(services,context)=>{
            services.AddSingleton<RepositoryDbContext>(f.Receiver.Sender.Catalog.Draft.Db);services.AddSingleton<IRepositoryAccessService>(f.Receiver.Accounts);services.AddSingleton(f.Receiver.Tenant);services.AddSingleton(f.Receiver.Workspaces);
            services.AddSingleton(f.Receiver.Resources.Object);services.AddSingleton<IProcessService>(f.Receiver.Processes);services.AddSingleton<ICrossPersonaTaskRouter>(f.Receiver.Sender.Catalog.Workflow.Native.Router);
            services.AddSingleton<Beep.OilandGas.DevelopmentPlanning.Services.IDevelopmentPlanService>(f.Receiver.Sender.Catalog.Draft.Drafts.Service);services.AddSingleton<IDevelopmentPlanRequestAuthority>(f.Receiver.Sender.Catalog.Draft.Authority);
            services.AddScoped<IDevelopmentAssignedReviewReader>(provider=>f.Receiver.Build(provider.GetRequiredService<IHttpContextAccessor>()));
            services.AddScoped<IDevelopmentReviewDecisionService>(provider=>f.Receiver.Build(provider.GetRequiredService<IHttpContextAccessor>()));services.AddSingleton(Mock.Of<IFailureReporter>());
        });
        host.Client.DefaultRequestHeaders.Add("X-Test-Actor","reviewer");host.Client.DefaultRequestHeaders.Add("X-Test-Assigned-Role","ConfiguredReviewRole");
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"reviewer",new(f.Receiver.Tenant.Company!.Scope,f.Receiver.Tenant.Company.Roles.ToArray(),f.Receiver.Tenant.Company.Permissions.ToArray()));
        var api=new Beep.OilandGas.Web.Services.ApiClient(host.Client,Mock.Of<IFailureReporter>(),company);
        var client=new Beep.OilandGas.Web.Services.DevelopmentServiceClient(api,Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Web.Services.DevelopmentServiceClient>.Instance);
        var assigned=await client.GetAssignedDraftReviewAsync(f.Command.Review.InstanceId);var command=new DevelopmentReviewDecision(assigned,Guid.NewGuid(),"REJECT","Typed HTTP independent rejection");
        var receipt=await client.DecideDraftReviewAsync(command);receipt.Require(command);Assert.Equal("REVIEW_REJECTED",receipt.WorkflowState);var writes=f.Mutations;
        Assert.True((await client.RecoverDraftReviewDecisionAsync(command)).Recorded);Assert.Equal(writes,f.Mutations);Assert.Single(f.Receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>());
    }
    [Theory][InlineData("APPROVE",1)][InlineData("REJECT",3)][InlineData("APPROVE",3)]
    public async Task SenderOriginalSubmissionRecoveryConfirmsProgressedGraphWithoutRepairOrNewWork(string decision,int stages)
    {
        using var f=new Fixture();await f.Prepare(stages);f.Command=f.Command with{Decision=decision};var initial=f.Receiver.Sender.Original;await f.Decide();var writes=f.Mutations;
        var receipt=await f.Receiver.Sender.Start(true);receipt.Require(initial);Assert.True(receipt.Recorded);Assert.Equal(f.Command.Review.StepInstanceId,receipt.StepInstanceId);Assert.Contains(f.Command.Review.TaskId,receipt.TaskIds);Assert.Equal(writes,f.Mutations);
        f.Receiver.Sender.Catalog.Workflow.Native.Rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES="Changed canonical audit";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Receiver.Sender.Start(true));Assert.Equal(writes,f.Mutations);
    }
    [Theory][InlineData("approval")][InlineData("history")][InlineData("task")][InlineData("process")][InlineData("step")]
    public async Task ChangedCanonicalDecisionEvidenceCannotBeRecoveredAsSuccessOrRepaired(string fault)
    {
        using var f=new Fixture();await f.Prepare(2);await f.Decide();var rows=f.Receiver.Sender.Catalog.Workflow.Native.Rows;var writes=f.Mutations;
        switch(fault)
        {
            case "approval":rows.OfType<PROCESS_APPROVAL>().Single().APPROVAL_NOTES="Changed audit";break;
            case "history":rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="DEVELOPMENT_REVIEW_APPROVED").DETAILS="Changed history";break;
            case "task":rows.OfType<CROSS_PERSONA_TASK>().Single(row=>row.TASK_STATUS=="COMPLETED").ROW_CHANGED_BY="another";break;
            case "process":rows.OfType<PROCESS_INSTANCE>().Single().CURRENT_STATE="Changed";break;
            case "step":rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.STEP_ID=="stage-1").STATUS="PENDING";break;
        }
        var changed=JsonSerializer.Serialize(rows);await Assert.ThrowsAnyAsync<Exception>(()=>f.Decide(true));Assert.Equal(writes,f.Mutations);Assert.Equal(changed,JsonSerializer.Serialize(rows));
    }
}
#endif
