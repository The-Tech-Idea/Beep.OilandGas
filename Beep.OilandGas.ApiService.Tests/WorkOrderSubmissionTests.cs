#if WORKFLOW_INBOX_FOCUSED
extern alias actualData;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Moq;
using TheTechIdea.Beep.ConfigUtil;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Data.OilGas;
using Xunit;
using Permit=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT;
using Condition=actualData::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition;
namespace Beep.OilandGas.ApiService.Tests;
public sealed class WorkOrderSubmissionTests
{
    [Theory][InlineData("current")][InlineData("schema")][InlineData("authority")][InlineData("foreign")][InlineData("binding")][InlineData("changed")][InlineData("identity")]
    public async Task IndependentReportFactsPreserveRecordedWorkWithoutPrivateBodyRationaleIntentOrWrites(string mode)
    {
        var f=WorkOrderExecutionStartFixture.Submission();var row=f.Command.Prepare(DateTime.UtcNow);var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();native.Rows.Add(row);
        var context=f.Command.Review.Inspections.Current.Context with{ActorUserId="independent-reviewer",Revision="completion-read-authority"};var query=new CompanyWorkOrderCompletionQuery(f.Command.Review.Inspections.Start.Permit.Task,row.SUBMISSION_ID);
        var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(mode!="schema");
        native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_SUBMISSION)));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        if(mode=="foreign")
        {
            var foreign=CompanyInventorySourceReview.Copy(row);foreign.OWNER_COMPANY_BA_ID="foreign";foreign.COMMAND_JSON="malformed private command";
            native.Source.Setup(value=>value.GetEntityAsync("WORK_ORDER_EXECUTION_SUBMISSION",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(new[]{foreign});
        }
        if(mode=="binding")context=context with{Database=context.Database with{Fingerprint="changed-core"}};if(mode=="identity")query=query with{SubmissionId="other-submission"};
        var calls=0;Task Guard(){if(mode=="authority")throw new UnauthorizedAccessException("Revoked reviewer");if(mode=="changed" && ++calls==3)row.COMPLETED_AT_UTC=row.COMPLETED_AT_UTC.AddSeconds(1);return Task.CompletedTask;}
        var owner=new WorkOrderExecutionPermitService(native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        if(mode!="current")await Assert.ThrowsAnyAsync<Exception>(()=>owner.ReadSubmissionFactsAsync(context,query,Guard));
        else
        {
            var facts=await owner.ReadSubmissionFactsAsync(context,query,Guard);facts.Require();Assert.Equal(f.Command.WorkPerformed,facts.WorkPerformed);Assert.Equal(f.Command.CompletedAtUtc,facts.CompletedAtUtc);Assert.Equal(row.RECORDED_REVISION,facts.Revision);
            var json=JsonSerializer.Serialize(facts);Assert.DoesNotContain(f.Command.Reason,json);Assert.DoesNotContain(f.Command.IntentId.ToString(),json);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("Prerequisites",json);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecoverSubmissionAsync(context,new(new(f.Command.Review.Inspections.Start.Permit,f.Command.Review.Inspections.Start.IntentId),f.Command.IntentId),Guard));
        }
        Assert.Equal(0,native.Writes);Assert.Empty(native.Transactions);
    }
    [Theory][InlineData("non-utc")][InlineData("before-start")][InlineData("future")][InlineData("empty-work")][InlineData("duplicate-defects")][InlineData("unaccepted")][InlineData("role")][InlineData("audit")]
    public void SubmissionCannotInferWorkTimeAcceptedInspectionsOrOriginalAudit(string mode)
    {
        var f=WorkOrderExecutionStartFixture.Submission();var command=f.Command;
        command=mode switch
        {
            "non-utc"=>command with{CompletedAtUtc=DateTime.SpecifyKind(command.CompletedAtUtc,DateTimeKind.Unspecified)},
            "before-start"=>command with{CompletedAtUtc=f.Start.StartedAtUtc.AddSeconds(-1)},
            "future"=>command with{CompletedAtUtc=command.Review.Inspections.ComparedAtUtc.AddSeconds(1)},
            "empty-work"=>command with{WorkPerformed=""},
            "duplicate-defects"=>command with{OutstandingDefects=["Original defect","Original defect"]},
            "unaccepted"=>command with{Review=command.Review with{Inspections=command.Review.Inspections with{Blockers=["Unaccepted work inspection"]}}},
            "role"=>command with{Review=command.Review with{Stage=command.Review.Stage with{AssignedRole="OtherRole"}}},
            _=>command
        };
        if(mode=="audit"){var row=command.Prepare(DateTime.UtcNow);row.ROW_CREATED_BY="another-author";Assert.Throws<InvalidOperationException>(()=>command.RequireRecorded(row));}
        else Assert.True(Record.Exception(command.Require) is ArgumentException or InvalidOperationException);
        f.Start.RequireRecorded(f.Started);
    }
    [Theory][InlineData("current")][InlineData("schema")][InlineData("write")][InlineData("readback")][InlineData("inspection-before")][InlineData("inspection-after")]
    [InlineData("stage-before")][InlineData("stage-after")][InlineData("unknown")][InlineData("isolation")][InlineData("foreign")]
    public async Task SubmissionUsesActualOriginalStartApprovalAndOneOwnedReportWithNoReplayRecovery(string mode)
    {
        var original=WorkOrderExecutionStartFixture.Original(true);var start=original.Command;var started=start.Prepare(DateTime.UtcNow);var baseline=start.Review.Prerequisites.Requirements;var context=baseline.Context;
        var utc=DateTime.UtcNow;var rows=baseline.Requirements.Select(row=>row.ApplicableStage=="WORK_SUBMISSION"?row with{Status="PASS",ResultText="Actual observed work",Inspector=context.ActorUserId,InspectDate=utc,SourceRevision=new string('F',64)}:row).ToArray();
        var current=baseline with{Requirements=rows,ComparedAtUtc=DateTime.UtcNow};
        var inspection=CompanyWorkOrderSubmissionInspectionReview.Create(new(start,started),current,DateTime.UtcNow);
        var command=new CompanyWorkOrderSubmissionCommand(new(inspection,start.Review.Stage),Guid.NewGuid(),started.ROW_CREATED_DATE!.Value,"Explicit actual work performed",[],"Own work report awaiting independent verification");command.Require();
        var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();native.Rows.Add(original.Recorded);native.Rows.Add(started);
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        foreach(var (type,key) in new[]{(typeof(Permit),"PERMIT_ID"),(typeof(WORK_ORDER_EXECUTION_START),"START_ID"),(typeof(WORK_ORDER_EXECUTION_SUBMISSION),"SUBMISSION_ID")})
        {
            var table=type.Name;metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=key});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
            native.Source.Setup(value=>value.CheckEntityExist(table)).Returns(()=>mode!="schema" || table!="WORK_ORDER_EXECUTION_SUBMISSION");native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>creator.ConvertToEntityStructure(type));
        }
        native.Source.Setup(value=>value.InsertEntity("WORK_ORDER_EXECUTION_SUBMISSION",It.IsAny<object>())).Returns((string _,object value)=>
        {
            native.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_EXECUTION_SUBMISSION)value);if(mode=="readback")row.ROW_CREATED_BY="another executor";native.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};
        });
        var owner=new WorkOrderExecutionPermitService(native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);var forbidProof=false;var stageReads=0;var coreReads=0;
        Task Guard()=>Task.CompletedTask;
        Task<CompanyMaintenanceAssignedStageReview> Stage()
        {if(forbidProof)throw new Exception("Recovery cannot review the live assignment");stageReads++;return Task.FromResult(mode=="stage-before" || mode=="stage-after" && stageReads==2?command.Review.Stage with{StepName="Changed configured stage"}:command.Review.Stage);}
        async Task<CompanyWorkOrderExecutionRequirements> Read(bool owned)
        {
            if(forbidProof)throw new Exception("Recovery cannot review live inspections");if(owned){PPDMDataSourceTransaction.RequireOwned(native.Source.Object,IsolationLevel.Serializable);coreReads++;}else PPDMDataSourceTransaction.RequireIdle(native.Source.Object);
            var observed=rows;if(mode=="inspection-before" || mode=="inspection-after" && native.Writes>0)observed=rows.Select(row=>row.ApplicableStage=="WORK_SUBMISSION"?row with{Status="FAIL"}:row).ToArray();
            async Task<(actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel Order,IReadOnlyList<Condition> Requirements)> Source(){await Task.CompletedTask;return(baseline.Order,observed);}
            return owned?await owner.ReadExecutionRequirementsCoreAsync(context,baseline.Approval.Permit,Guard,Source):await owner.ReadExecutionRequirementsAsync(context,baseline.Approval.Permit,Guard,Source);
        }
        if(mode=="unknown")native.FailCommit=true;if(mode=="isolation")native.Isolation=IsolationLevel.ReadCommitted;if(mode=="foreign")started.EXECUTOR_USER_ID="foreign";
        var error=await Record.ExceptionAsync(async()=>{var result=await owner.SubmitWorkAsync(command,Guard,Read,Stage);result.Require(command);});
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);
        if(mode is "current" or "unknown" or "stage-after")
        {
            native.FailCommit=false;var record=Assert.Single(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>());command.RequireRecorded(record);Assert.Equal(1,native.Writes);Assert.Equal(4,coreReads);forbidProof=true;
            var reference=new CompanyWorkOrderSubmissionReference(new(baseline.Approval.Permit,start.IntentId),command.IntentId);
            (await owner.RecoverSubmissionAsync(context,reference,Guard)).Require(context.Company,context.ActorUserId,reference);
            (await owner.SubmitWorkAsync(command,Guard,Read,Stage)).Require(command);Assert.Equal(1,native.Writes);
            await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.SubmitWorkAsync(command with{IntentId=Guid.NewGuid()},Guard,Read,Stage));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecoverSubmissionAsync(context with{ActorUserId="other"},reference,Guard));
        }
        else Assert.Empty(native.Rows.OfType<WORK_ORDER_EXECUTION_SUBMISSION>());
        Assert.Equal(JsonSerializer.Serialize(original.Recorded),JsonSerializer.Serialize(native.Rows.OfType<Permit>().Single()));
        Assert.DoesNotContain(native.Rows,value=>value.GetType().Name=="PROJECT");
    }
}
#endif
