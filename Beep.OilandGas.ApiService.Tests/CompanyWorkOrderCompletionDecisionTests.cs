#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using System.Text.Json.Nodes;
using Beep.OilandGas.LifeCycle.Data.Tables;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.LifeCycle.Services.Processes;
using Moq;
using TheTechIdea.Beep;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class CompanyWorkOrderCompletionDecisionTests
{
    internal sealed class Fixture
    {
        internal readonly CompanyMaintenanceExecutionHandoffTests.Fixture Flow=new();
        internal CompanyWorkOrderCompletionDecisionReview Review=null!;
        internal CompanyMaintenanceExecutionHandoffRecovery Handoff=null!;
        internal bool Changed,ChangedOwned,Revoked;
        internal async Task Ready(string mode="current",bool realAudit=false,bool tamperBusiness=false,string? returnRole=null,bool returnAcceptance=false,bool assetReturn=false,bool multipleWells=false)
        {
            await Flow.Ready(true,returnRole,returnAcceptance,assetReturn,multipleWells);var handoff=new CompanyMaintenanceExecutionHandoffCommand(await Flow.Flow.Owner.Service.ReviewExecutionHandoffAsync(Flow.Stage,Flow.Report,Flow.Authority),Guid.NewGuid(),"Executor handoff");Handoff=await Flow.Flow.Owner.Service.RecordExecutionHandoffAsync(handoff,Flow.Authority,Flow.Read);
            var reviewer=Flow.Executor with{ActorUserId="independent-reviewer",Revision="independent-completion-authority"};var stage=await Flow.Flow.Owner.Service.ReviewMaintenanceAssignedStageAsync(reviewer,Flow.Stage.Task,Authority);
            var metadata=new Mock<IPPDMMetadataRepository>();foreach(var table in new[]{"PROCESS_INSTANCE","PROCESS_APPROVAL","PROCESS_HISTORY","PROCESS_STEP_INSTANCE","CROSS_PERSONA_TASK"})metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([table=="CROSS_PERSONA_TASK"?"CROSS_TASK_ID":table+"_ID"]);
            ICommonColumnHandler columns=realAudit?new Beep.OilandGas.PPDM39.DataManagement.Core.Common.CommonColumnHandler():Mock.Of<ICommonColumnHandler>();
            if(tamperBusiness){var mutable=new Mock<ICommonColumnHandler>();mutable.Setup(value=>value.PrepareForInsert(It.IsAny<Beep.OilandGas.PPDM.Models.IPPDMEntity>(),It.IsAny<string>())).Callback((Beep.OilandGas.PPDM.Models.IPPDMEntity row,string actor)=>{if(row is PROCESS_APPROVAL approval)approval.APPROVAL_STATUS="UNDECLARED";});columns=mutable.Object;}
            Flow.Flow.Owner.Service=new PPDMProcessService(Flow.Flow.Owner.Native.Editor.Object,columns,Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,reviewer.Database.ConnectionName,personaReader:Flow.Flow.Owner.Native.Personas.Object);
            Review=new(stage,Completion(reviewer,Flow.Report,mode,CompanyWorkOrderSubmissionCommand.Read(Flow.Flow.Owner.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single())));Review.Require();
        }
        internal Task Authority(string? role){if(Revoked)throw new UnauthorizedAccessException("Revoked current reviewer");if(role is not null)Assert.Equal("VerifierRole",role);return Task.CompletedTask;}
        internal Task<CompanyWorkOrderCompletionReview?> Read(bool owned)
        {if(owned)PPDMDataSourceTransaction.RequireOwned(Flow.Flow.Owner.Native.Source.Object,System.Data.IsolationLevel.Serializable);if(Changed || owned && ChangedOwned)throw new InvalidOperationException("Changed current report/inspection/person evidence");return Task.FromResult<CompanyWorkOrderCompletionReview?>(Review.Completion);}
    }
    internal static CompanyWorkOrderCompletionReview Completion(FieldSelectionReview lifecycle,CompanyWorkOrderSubmissionFacts report,string mode="current",CompanyWorkOrderSubmissionCommand? originalCommand=null)
    {
        var original=WorkOrderExecutionStartFixture.Submission(report.Database,report.Query.Task.FieldId,report.ExecutorUserId,report.ExecutionRole,taskOverride:report.Query.Task,completionVerification:true);var source=(originalCommand??original.Command).Review.Inspections.Current;var context=source.Context with{ActorUserId=lifecycle.ActorUserId,Revision=lifecycle.Revision};var utc=DateTime.UtcNow;
        // Canonical already-recorded completion comparison seam; native source reads have separate API gates.
        var requirements=source.Requirements.Select(check=>check.ApplicableStage=="COMPLETION_VERIFICATION"?check with{Status="PASS",ResultText="Actual accepted observation",Inspector=context.ActorUserId,InspectDate=utc,SourceRevision=new string('C',64)}:check).ToArray();
        if(mode=="defects")report=report with{OutstandingDefects=["Recorded unresolved defect"]};var personId=mode=="same-person"?report.EmployeeBaId:"reviewer-person";
        var person=new CompanyNativePersonFacts(context.Company,personId,"Recorded reviewer","PERSON",utc.AddDays(-1),utc.AddDays(2),new(context.Company.BusinessAssociateId,personId,1,"Recorded reviewer","ACTIVE",utc.AddDays(-1),utc.AddDays(2),new string('D',64)),new string('E',64));
        var link=new CompanyPersonnelIdentityVersion(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),context.ActorUserId,personId,"PERSON",1,utc.AddDays(-1),utc.AddDays(2),"Original identity evidence","identity-administrator",utc,new string('F',64));
        var identities=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,new string('A',64)),mode=="missing-person"?[]:[link],DateTime.UtcNow);var now=DateTime.UtcNow;
        return new(context,report,source.Order,requirements,identities,mode=="missing-person"?null:person,now,CompanyWorkOrderCompletionReview.GetBlockers(context,report,source.Order,requirements,identities,mode=="missing-person"?null:person,now));
    }
    [Theory][InlineData("VERIFIED")][InlineData("REJECTED")]
    public async Task ActualNativeDecisionRecordsImmutableApprovalHistoryAndPrivateRecoveryWithoutPhysicalMutation(string decision)
    {
        var f=new Fixture();await f.Ready(decision=="REJECTED"?"defects":"current");var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),decision,"Independent actual completion decision");command.Require();var before=JsonSerializer.Serialize(f.Flow.Flow.Owner.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single());var writes=f.Flow.Flow.Owner.Native.Writes;
        var saved=await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);saved.Require(f.Review.Stage.Context.Company,f.Review.Stage.Context.ActorUserId,command.Reference());Assert.Equal(writes+3,f.Flow.Flow.Owner.Native.Writes);
        var approval=f.Flow.Flow.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single(row=>row.APPROVAL_TYPE=="WORK_COMPLETION");Assert.Equal(decision,approval.APPROVAL_STATUS);Assert.Equal(command.Reason,approval.APPROVAL_NOTES);Assert.Equal(before,JsonSerializer.Serialize(f.Flow.Flow.Owner.Native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>().Single()));writes=f.Flow.Flow.Owner.Native.Writes;
        Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read)));Assert.Equal(JsonSerializer.Serialize(saved),JsonSerializer.Serialize(await f.Flow.Flow.Owner.Service.RecoverCompletionDecisionAsync(f.Review.Stage.Context,command.Reference(),f.Authority)));Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
        var current=await f.Flow.Flow.Owner.Service.ReviewMaintenanceAssignedStageAsync(f.Review.Stage.Context,f.Review.Stage.Task,f.Authority);Assert.Equal("VerifierRole",current.AssignedRole);Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Flow.Flow.Owner.Service.ReviewCompletionDecisionStageAsync(f.Review.Stage.Context,command.Reference().Query,f.Authority));Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command with{IntentId=Guid.NewGuid()},f.Authority,f.Read));Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
    }
    [Theory][InlineData("defects")][InlineData("same-person")][InlineData("missing-person")][InlineData("source")][InlineData("role")][InlineData("unknown")]
    public async Task VerificationRefusesMissingOrConflictingEvidenceAndUncertaintyNeverCreatesReplacement(string fault)
    {
        var f=new Fixture();await f.Ready(fault);var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Original independent decision");var writes=f.Flow.Flow.Owner.Native.Writes;
        if(fault=="source")f.ChangedOwned=true;if(fault=="role")f.Revoked=true;if(fault=="unknown")f.Flow.Flow.Owner.Native.FailCommit=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read));
        if(fault=="unknown"){var saved=await f.Flow.Flow.Owner.Service.RecoverCompletionDecisionAsync(f.Review.Stage.Context,command.Reference(),f.Authority);saved.Require(f.Review.Stage.Context.Company,f.Review.Stage.Context.ActorUserId,command.Reference());}else Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
    }
    [Theory][InlineData("actor")][InlineData("approval")][InlineData("history")][InlineData("reference")]
    public async Task PrivateRecoveryRefusesForeignReviewerOrChangedImmutableNativeAudit(string fault)
    {
        var f=new Fixture();await f.Ready();var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Original verified evidence");await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);var writes=f.Flow.Flow.Owner.Native.Writes;var context=f.Review.Stage.Context;var reference=command.Reference();
        if(fault=="actor")context=context with{ActorUserId="other-reviewer"};if(fault=="reference")reference=reference with{IntentId=Guid.NewGuid()};if(fault=="approval")f.Flow.Flow.Owner.Native.Rows.OfType<PROCESS_APPROVAL>().Single(row=>row.APPROVAL_TYPE=="WORK_COMPLETION").APPROVAL_NOTES="Changed original decision";if(fault=="history")f.Flow.Flow.Owner.Native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.EVENT_TYPE=="WORK_VERIFIED").USER_ID="other-reviewer";
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Flow.Owner.Service.RecoverCompletionDecisionAsync(context,reference,f.Authority));Assert.Equal(writes,f.Flow.Flow.Owner.Native.Writes);
    }
    [Theory][InlineData("separate")][InlineData("revoked")][InlineData("pointer")]
    public async Task OwnedLifecycleProofDoesNotBorrowSeparateCoreAndRejectsChangedAuthorityOrPointer(string fault)
    {
        var f=new Fixture();await f.Ready();var core=new Mock<IDataSource>();var editor=f.Flow.Flow.Owner.Native.Editor;editor.Setup(value=>value.GetDataSource("separate-core")).Returns(core.Object);var context=f.Review.Completion.Context with{Database=f.Review.Completion.Context.Database with{ConnectionName="separate-core"}};var reads=0;
        Task Guard(){if(fault=="revoked")throw new UnauthorizedAccessException("Current reviewer revoked");if(fault=="pointer")editor.Setup(value=>value.GetDataSource("separate-core")).Returns(new Mock<IDataSource>().Object);return Task.CompletedTask;}
        var error=await Record.ExceptionAsync(()=>PPDMDataSourceTransaction.ExecuteAsync(f.Flow.Flow.Owner.Native.Source.Object,async()=>
        {
            var proof=await f.Flow.Reports.ReadCompletionDecisionProofAsync(context,f.Review.Stage.Context.Database.ConnectionName,true,Guard,_=>{reads++;return Task.FromResult(f.Review.Completion);});
            Assert.Null(proof);
        }));
        if(fault=="separate")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(0,reads);Assert.Empty(core.Invocations);
    }
    [Theory][InlineData("step")][InlineData("task")][InlineData("extra-approval")][InlineData("delta-source")][InlineData("delta-row")][InlineData("audit-version")][InlineData("audit-downgrade")]
    public async Task RecordedDecisionDeltaRefusesChangedGraphPopulationAndCorruptBeforeAfterEvidence(string fault)
    {
        var f=new Fixture();await f.Ready();var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Original native graph decision");await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);var native=f.Flow.Flow.Owner.Native;var writes=native.Writes;
        if(fault=="step")native.Rows.OfType<PROCESS_STEP_INSTANCE>().Single(row=>row.PROCESS_STEP_INSTANCE_ID==f.Review.Stage.StepInstanceId).COMPLETED_BY="unrecorded-actor";
        if(fault=="task")native.Rows.OfType<CROSS_PERSONA_TASK>().First(row=>row.PROCESS_STEP_INSTANCE_ID==f.Review.Stage.StepInstanceId).TASK_STATUS="CANCELLED";
        if(fault=="extra-approval")native.Rows.Add(new PROCESS_APPROVAL{PROCESS_APPROVAL_ID="uncorrelated-approval",PROCESS_INSTANCE_ID=f.Review.Stage.Task.InstanceId,APPROVAL_TYPE="OTHER",ACTIVE_IND="Y"});
        if(fault is "delta-source" or "delta-row" or "audit-version" or "audit-downgrade")
        {
            var process=native.Rows.OfType<PROCESS_INSTANCE>().Single();var payload=JsonNode.Parse(process.INSTANCE_DATA_JSON!)!;var evidence=payload["Data"]!["CompanyWorkOrderCompletionDecision"]!;var delta=evidence["Delta"]!;
            if(fault=="delta-source")delta["BeforeRevision"]=new string('F',64);else if(fault=="delta-row")delta["Changes"]!.AsArray().Single(row=>row!["Section"]!.GetValue<string>()=="Process")!["After"]!["ROW_CREATED_BY"]="changed-starter";else if(fault=="audit-version")evidence["AuditVersion"]=2;else{evidence["AuditVersion"]=0;evidence.AsObject().Remove("Delta");}
            process.INSTANCE_DATA_JSON=payload.ToJsonString();
        }
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Flow.Flow.Owner.Service.RecoverCompletionDecisionAsync(f.Review.Stage.Context,command.Reference(),f.Authority));Assert.Equal(writes,native.Writes);
    }
    [Fact] public async Task RealCommonAuditHandlerCannotReplaceTheOwnedDecisionUtcGuidOrCreatorAudit()
    {
        var f=new Fixture();await f.Ready(realAudit:true);var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Explicit real audit decision");var native=f.Flow.Flow.Owner.Native;var process=native.Rows.OfType<PROCESS_INSTANCE>().Single();var creator=process.ROW_CREATED_BY;var created=process.ROW_CREATED_DATE;var writes=native.Writes;
        var result=await f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read);Assert.Equal(writes+3,native.Writes);var utc=result.Receipt.RecordedAtUtc;
        var approval=native.Rows.OfType<PROCESS_APPROVAL>().Single(row=>row.APPROVAL_TYPE=="WORK_COMPLETION");var history=native.Rows.OfType<PROCESS_HISTORY>().Single(row=>row.PROCESS_HISTORY_ID==command.HistoryId());
        Assert.True(Guid.TryParse(approval.PPDM_GUID,out _));Assert.True(Guid.TryParse(history.PPDM_GUID,out _));Assert.NotEqual(approval.PPDM_GUID,history.PPDM_GUID);Assert.Equal(utc,approval.ROW_CREATED_DATE);Assert.Equal(utc,approval.ROW_CHANGED_DATE);Assert.Equal(utc,history.ROW_CREATED_DATE);Assert.Equal(utc,history.ROW_CHANGED_DATE);var stored=native.Rows.OfType<PROCESS_INSTANCE>().Single();Assert.Equal(creator,stored.ROW_CREATED_BY);Assert.Equal(created,stored.ROW_CREATED_DATE);Assert.Equal(utc,stored.ROW_CHANGED_DATE);
        writes=native.Writes;var recovered=await f.Flow.Flow.Owner.Service.RecoverCompletionDecisionAsync(f.Review.Stage.Context,command.Reference(),f.Authority);Assert.Equal(JsonSerializer.Serialize(result),JsonSerializer.Serialize(recovered));Assert.Equal(writes,native.Writes);
    }
    [Fact] public async Task CommonHandlerBusinessMutationCannotPersistAnUndeclaredDecision()
    {
        var f=new Fixture();await f.Ready(tamperBusiness:true);var command=new CompanyWorkOrderCompletionDecisionCommand(f.Review,Guid.NewGuid(),"VERIFIED","Original declared decision");var native=f.Flow.Flow.Owner.Native;var writes=native.Writes;
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Flow.Flow.Owner.Service.RecordCompletionDecisionAsync(command,f.Authority,f.Read));Assert.Equal(writes,native.Writes);Assert.DoesNotContain(native.Rows.OfType<PROCESS_APPROVAL>(),row=>row.APPROVAL_TYPE=="WORK_COMPLETION");
    }
}
#endif
