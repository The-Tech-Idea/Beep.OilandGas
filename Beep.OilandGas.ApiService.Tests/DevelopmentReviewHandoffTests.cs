using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.DevelopmentPlanning.Models;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Repository.Models;
#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.ApiService.Services;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Beep.OilandGas.Repository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Beep.OilandGas.ApiService.Tests;

public sealed class DevelopmentReviewHandoffTests
{
    internal sealed class Fixture : IDisposable
    {
        internal DevelopmentReviewCatalogTests.Fixture Catalog=new(true);
        internal DevelopmentReviewHandoffService Service=null!;
        internal DevelopmentReviewStart Original=null!;
        internal RepositoryAssetAccessStore Grants=null!;
        internal DevelopmentReviewRecipientAuthority Recipients=null!;
        internal Action? BeforeHandoffReview;
        internal async Task Prepare(int stages=1)
        {
            if(stages>1)Catalog.Workflow.Definition.PROCESS_CONFIG_JSON=JsonSerializer.Serialize(new{Steps=Enumerable.Range(1,stages).Select(index=>new Beep.OilandGas.Models.Processes.ProcessStepDefinition{
                StepId="stage-"+index,StepName="Configured review "+index,SequenceNumber=index,StepType="APPROVAL",RequiresApproval=true,RequiredRoles=["ConfiguredReviewRole"]}).ToArray()});
            await Catalog.Prepare();await Catalog.UseCompanyRecipients();var db=Catalog.Draft.Db;
            foreach(var permission in new[]{"DevelopmentPlanning.View","DevelopmentPlanning.Approve","Workflow.View","Workflow.Approve"})
                db.RoleClaims.Add(new(){RoleId="review-role",ClaimType="permission",ClaimValue=permission});
            db.RoleClaims.Add(new(){RoleId="review-role",ClaimType=RepositoryPersonaService.WorkspaceClaimType,ClaimValue="RESERVOIR_ENGINEER"});
            db.Add(new AppUserAssetAccess{Id="reviewer-field",UserId="reviewer",DatabaseScope=Catalog.Draft.Context.Database.Fingerprint,OrganizationId="company",AssetType="FIELD",AssetId="field",AccessLevel="READ",IsActive=true});
            await db.SaveChangesAsync();
            Grants=new(db,Catalog.Company,Mock.Of<IAssetGrantTargetVerifier>());
            Recipients=new(new RepositoryAccessService(db),new RepositoryPersonaService(db),Grants);
            Service=new(Catalog.Draft.Authority,Catalog.Draft.Drafts.Service,Catalog.Service,Recipients,Catalog.Company,Catalog.Modules,
                new BoundProcessService(()=>Task.FromResult("lifecycle"),_=>Catalog.Workflow.Service),Catalog.Draft.Drafts.Native.Editor.Object);
            BeforeHandoffReview?.Invoke();
            var read=await Catalog.Read();Original=await Service.PrepareAsync(Catalog.Draft.User,read.Source.PlanId,new(read.Source.Revision,read.Revision,read.Choices[0].ProcessId));
        }
        internal Task<DevelopmentReviewStartReceipt> Start(bool recover=false)=>Service.SubmitAsync(Catalog.Draft.User,Original,recover);
        public void Dispose()=>Catalog.Dispose();
    }
    [Fact] public async Task ActualQualifiedSourceDefinitionRecipientsAndNativeGraphCommitOnceAndRecoverWithoutWrites()
    {
        using var f=new Fixture();await f.Prepare();f.Original.Require();Assert.Equal("reviewer",Assert.Single(Assert.Single(f.Original.ReceivingSteps).Recipients).UserId);
        var receipt=await f.Start();receipt.Require(f.Original);Assert.True(receipt.Recorded);Assert.Single(receipt.TaskIds);Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
        Assert.Equal(new[]{"begin","commit"},f.Catalog.Workflow.Native.Transactions);
        var process=Assert.Single(f.Catalog.Workflow.Native.Rows.OfType<PROCESS_INSTANCE>());
        Assert.Equal(f.Original.Revision(),DevelopmentReviewStart.FromPayload(process.INSTANCE_DATA_JSON!,process.PROCESS_INSTANCE_ID!,process.FIELD_ID!,process.ENTITY_ID!).Revision());
        Assert.Equal("RESERVOIR_ENGINEER",Assert.Single(f.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>()).TARGET_PERSONA_CODE);
        Assert.True((await f.Start()).Recorded);Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
        Assert.Equal(1,f.Catalog.Draft.Drafts.Native.Writes);Assert.Equal("DRAFT",f.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_STATUS);
    }
    [Fact] public async Task ReadOnlyAbsentRecoveryCannotCreateOrRepairAssignment()
    {
        using var f=new Fixture();await f.Prepare();var absent=await f.Start(true);absent.Require(f.Original);Assert.False(absent.Recorded);Assert.Equal(0,f.Catalog.Workflow.Native.Writes);
        await f.Start();f.Catalog.Workflow.Native.Rows.RemoveAll(row=>row is CROSS_PERSONA_TASK);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start(true));await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
    }
    [Theory][InlineData(1)][InlineData(2)][InlineData(3)][InlineData(4)]
    public async Task EveryNativeGraphHistoryAndTaskFailureRollsBackTheWholeHandoff(int write)
    {
        using var f=new Fixture();await f.Prepare();f.Catalog.Workflow.FailWrite=write;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());
        Assert.Single(f.Catalog.Workflow.Native.Rows);Assert.Equal("rollback",f.Catalog.Workflow.Native.Transactions.Last());
    }
    [Fact] public async Task UncertainCommitUsesOriginalReadOnlyRecoveryAndNoSecondProcess()
    {
        using var f=new Fixture();await f.Prepare();f.Catalog.Workflow.Native.FailCommit=true;await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());
        f.Catalog.Workflow.Native.FailCommit=false;Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
        var original=f.Original;f.Original=original with{IntentId=Guid.NewGuid()};await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
    }
    [Theory][InlineData("role")][InlineData("permission")][InlineData("grant")][InlineData("workspace")][InlineData("definition")][InlineData("source")][InlineData("binding")][InlineData("actor")]
    public async Task ChangedOriginalAuthorityNeverCreatesSuccessfulHandoff(string fault)
    {
        using var f=new Fixture();await f.Prepare();var db=f.Catalog.Draft.Db;
        switch(fault)
        {
            case "role":(await db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";break;
            case "permission":db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimValue=="Workflow.Approve"));break;
            case "grant":(await db.Set<AppUserAssetAccess>().SingleAsync()).IsActive=false;break;
            case "workspace":db.Remove(await db.RoleClaims.SingleAsync(row=>row.ClaimType==RepositoryPersonaService.WorkspaceClaimType));break;
            case "definition":f.Catalog.Workflow.Definition.VERSION="changed";break;
            case "source":f.Catalog.Draft.Drafts.Native.Rows.OfType<Beep.OilandGas.Models.Data.DevelopmentPlanning.FIELD_DEVELOPMENT_PLAN>().Single().FDP_NAME="changed";break;
            case "binding":f.Catalog.Binding.ConcurrencyStamp="changed";break;case "actor":f.Catalog.Draft.Active=false;break;
        }
        await db.SaveChangesAsync();await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Equal(0,f.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task MakerOrRequestingActorCannotBeReceivingReviewer()
    {
        using var f=new Fixture();await f.Prepare();
        var step=Assert.Single(f.Original.ReceivingSteps);var invalid=f.Original with{ReceivingSteps=[step with{Recipients=[new("planner","RESERVOIR_ENGINEER")]}]};
        Assert.Throws<InvalidOperationException>(invalid.Require);Assert.Equal(0,f.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task SourceReadsOccurOutsideLifecycleTransactionAndClosingSourceChangeIsUnconfirmed()
    {
        using var f=new Fixture();await f.Prepare();var reads=0;
        Task<DevelopmentPlanSourceReview> Read()
        {
            Assert.DoesNotContain("begin",f.Catalog.Workflow.Native.Transactions.TakeLast(1));reads++;
            if(reads>1)throw new InvalidOperationException("The source changed after lifecycle commit.");return Task.FromResult(f.Original.Source);
        }
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Catalog.Workflow.Service.StartDevelopmentReviewAsync(f.Original,()=>Task.CompletedTask,Read));
        Assert.Equal(4,f.Catalog.Workflow.Native.Writes);Assert.Equal("commit",f.Catalog.Workflow.Native.Transactions.Last());Assert.True((await f.Start(true)).Recorded);
    }
    [Fact] public async Task ReadOnlyOriginalRecoveryCanConfirmRecordedAssignmentAfterRecipientRevocation()
    {
        using var f=new Fixture();await f.Prepare();await f.Start();
        (await f.Catalog.Draft.Db.Set<AppUserRoleExtension>().SingleAsync(row=>row.UserId=="reviewer")).ApprovalStatus="Revoked";await f.Catalog.Draft.Db.SaveChangesAsync();
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.True((await f.Start(true)).Recorded);Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task ActualTypedHttpClientPreparesSubmitsAndRecoversOriginalQualifiedNativeHandoff()
    {
        using var f=new Fixture();await f.Prepare();await using var host=await DevelopmentDraftHttpTests.Start(f.Catalog.Draft,catalog:f.Catalog.Service,handoffs:f.Service);
        using var company=new Beep.OilandGas.Client.App.CompanyClientContext();company.TrySelect(company.Clear(),"planner",new(f.Catalog.Draft.Context.Company,["ConfiguredPlanner"],f.Catalog.Draft.Permissions));
        var client=new Beep.OilandGas.Web.Services.DevelopmentServiceClient(new(host.Http,Mock.Of<TheTechIdeaWeb.Diagnostics.IFailureReporter>(),company),Microsoft.Extensions.Logging.Abstractions.NullLogger<Beep.OilandGas.Web.Services.DevelopmentServiceClient>.Instance);
        var catalog=await client.GetDraftReviewWorkflowsAsync(f.Original.Source.PlanId,f.Original.Source.Revision);
        var original=await client.PrepareDraftReviewAsync(f.Original.Source.PlanId,new(catalog.Source.Revision,catalog.Revision,catalog.Choices[0].ProcessId));original.Require();
        var receipt=await client.SubmitDraftReviewAsync(original);receipt.Require(original);Assert.True(receipt.Recorded);
        Assert.True((await client.RecoverDraftReviewAsync(original)).Recorded);Assert.Equal(4,f.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task ThreeConfiguredReviewStagesRetainOrderAndOnlyTheInitialStageReceivesTasks()
    {
        using var f=new Fixture();await f.Prepare(3);var receipt=await f.Start();receipt.Require(f.Original);
        var steps=f.Catalog.Workflow.Native.Rows.OfType<PROCESS_STEP_INSTANCE>().OrderBy(step=>step.STEP_SEQUENCE).ToArray();
        Assert.Equal(3,steps.Length);Assert.Equal(new[]{"PENDING","BLOCKED","BLOCKED"},steps.Select(step=>step.STATUS));
        Assert.Equal("stage-1",steps[0].STEP_ID);Assert.Single(f.Catalog.Workflow.Native.Rows.OfType<CROSS_PERSONA_TASK>());Assert.Equal(6,f.Catalog.Workflow.Native.Writes);
    }
    [Fact] public async Task RecipientGrantRevokedAfterFirstInsertRollsBackGraphAndTasks()
    {
        using var f=new Fixture();await f.Prepare();var native=f.Catalog.Workflow.Native;
        native.Source.Setup(source=>source.InsertEntity(It.IsAny<string>(),It.IsAny<object>())).Returns((string table,object value)=>{
            native.Writes++;native.Rows.Add(JsonSerializer.Deserialize(JsonSerializer.Serialize(value),value.GetType())!);
            if(native.Writes==1){f.Catalog.Draft.Db.Set<AppUserAssetAccess>().Single().IsActive=false;f.Catalog.Draft.Db.SaveChanges();}
            return new TheTechIdea.Beep.ConfigUtil.ErrorsInfo{Flag=TheTechIdea.Beep.ConfigUtil.Errors.Ok};});
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Start());Assert.Single(native.Rows);Assert.Equal("rollback",native.Transactions.Last());
    }
}
#endif
