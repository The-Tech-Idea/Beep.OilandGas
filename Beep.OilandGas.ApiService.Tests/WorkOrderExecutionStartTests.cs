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
using Detail=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
using Condition=actualData::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition;
using Permit=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class WorkOrderExecutionStartTests
{
    [Theory][InlineData("idle-shared")][InlineData("owned-shared")][InlineData("owned-separate")]
    [InlineData("foreign")][InlineData("source-drift")][InlineData("changed-start")][InlineData("context-drift")][InlineData("submitted")][InlineData("late-submission")]
    public async Task MaterialExecutionProofRetainsOriginalReservationAndDistinguishesActualProviderOwnership(string mode)
    {
        var original=Original(true);var command=original.Command;var before=command.Review.Prerequisites.Requirements;
        var context=before.Context;var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();
        var recorded=command.Prepare(DateTime.UtcNow);native.Rows.Add(recorded);native.Rows.Add(original.Recorded);
        var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_START")).Returns(true);
        native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(true);
        native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_SUBMISSION")).Returns(true);
        native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure shape,bool _)=>creator.ConvertToEntityStructure(shape.EntityName=="WORK_ORDER_EXECUTION_START"?typeof(WORK_ORDER_EXECUTION_START):shape.EntityName=="WORK_ORDER_EXECUTION_SUBMISSION"?typeof(WORK_ORDER_EXECUTION_SUBMISSION):typeof(Permit)));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(["START_ID"]);
        metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(["PERMIT_ID"]);
        metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_SUBMISSION")).ReturnsAsync(["SUBMISSION_ID"]);
        var owner=new WorkOrderExecutionPermitService(native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        var separate=new PersonaTaskRoutingTests.Fixture();const string stockName="physical-production";
        if(mode=="owned-separate")native.Editor.Setup(value=>value.GetDataSource(stockName)).Returns(separate.Source.Object);
        var calls=0;var ownedFlags=new List<bool>();
        void Report()
        {
            var utc=DateTime.UtcNow;var rows=before.Requirements.Select(row=>row.ApplicableStage=="WORK_SUBMISSION"?row with{Status="PASS",InspectDate=utc,Inspector=context.ActorUserId,ResultText="Actual controlled inspection",SourceRevision=new string('F',64)}:row).ToArray();
            var current=before with{Requirements=rows,ComparedAtUtc=DateTime.UtcNow};
            var review=CompanyWorkOrderSubmissionInspectionReview.Create(new(command,recorded),current,DateTime.UtcNow);
            var report=new CompanyWorkOrderSubmissionCommand(new(review,command.Review.Stage),Guid.NewGuid(),recorded.ROW_CREATED_DATE!.Value,"Actual work",[],"Awaiting independent verification");
            native.Rows.Add(report.Prepare(DateTime.UtcNow));
        }
        if(mode=="submitted")Report();
        Task<(Detail Order,IReadOnlyList<Condition> Requirements)> Current(bool owned)
        {
            calls++;ownedFlags.Add(owned);var utc=DateTime.UtcNow;var rows=before.Requirements;
            if(mode=="source-drift" && calls==2)rows=rows.Select(row=>row with{RequiredText=row.RequiredText+" changed"}).ToArray();
            if(mode=="changed-start")recorded.ROW_CHANGED_BY="other";
            if(mode=="late-submission" && calls==2)Report();
            return Task.FromResult((before.Order,rows));
        }
        CompanyWorkOrderMaterialUseExecutionProof? result=null;
        Task Read(bool owned)=>ReadCore(owned);
        async Task ReadCore(bool owned)
        {
            result=await owner.ReadMaterialUseExecutionProofAsync(mode=="foreign"?context with{ActorUserId="foreign"}:context,
                new(before.Approval.Permit,command.IntentId),stockName,owned,
                ()=>mode=="context-drift" && calls>0?Task.FromException(new UnauthorizedAccessException("Current execution authority changed")):Task.CompletedTask,Current);
        }
        var error=await Record.ExceptionAsync(async()=>
        {
            if(mode=="owned-shared")await PPDMDataSourceTransaction.ExecuteAsync(native.Source.Object,()=>Read(true));
            else if(mode=="owned-separate")await PPDMDataSourceTransaction.ExecuteAsync(separate.Source.Object,()=>Read(true));
            else await Read(false);
        });
        if(mode is "foreign" or "source-drift" or "changed-start" or "context-drift" or "submitted" or "late-submission")Assert.NotNull(error);
        else
        {
            Assert.Null(error);
            if(mode=="owned-separate"){Assert.Null(result);Assert.Empty(ownedFlags);Assert.Equal(0,calls);return;}
            Assert.NotNull(result);Assert.Equal(command.Review.Prerequisites.Material,result.Reservation);
            Assert.Equal(2,calls);
            Assert.All(ownedFlags,flag=>Assert.Equal(mode=="owned-shared",flag));Assert.Empty(result.Inspections.Current.Blockers);
            Assert.NotEmpty(result.Inspections.Blockers); // Future work-submission evidence is not required to issue material.
            Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(result));
        }
        Assert.Equal(0,native.Writes);Assert.Equal(0,separate.Writes);
    }
    [Theory][InlineData("current")][InlineData("unperformed")][InlineData("before-start")][InlineData("legacy")][InlineData("requirement-drift")][InlineData("source-drift")][InlineData("start-drift")][InlineData("foreign")][InlineData("context-drift")][InlineData("unordered")]
    public async Task SubmissionInspectionComparisonClosesOwnOriginalStartAndCompleteCurrentPopulationWithoutWrites(string mode)
    {
        var command=Original(mode!="legacy").Command;var before=command.Review.Prerequisites.Requirements;var context=before.Context;var query=before.Approval.Permit;
        var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();var recorded=command.Prepare(DateTime.UtcNow);native.Rows.Add(recorded);
        var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_START")).Returns(true);native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_START)));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_START",EntityTypeName=typeof(WORK_ORDER_EXECUTION_START).FullName!,PrimaryKeyColumn="START_ID"});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(["START_ID"]);
        var reads=0;
        Task<CompanyWorkOrderExecutionRequirements> Current()
        {
            reads++;var utc=DateTime.UtcNow;var rows=before.Requirements.Select(row=>row.ApplicableStage=="WORK_SUBMISSION" && mode!="unperformed"?row with{Status="PASS",ResultText="Actual operational inspection",Inspector=context.ActorUserId,InspectDate=mode=="before-start"?recorded.STARTED_AT_UTC.AddSeconds(-1):recorded.ROW_CREATED_DATE!.Value,SourceRevision=new string('F',64)}:row).ToArray();
            if(mode=="requirement-drift" || mode=="source-drift" && reads==2)rows=rows.Select(row=>row with{RequiredText=row.RequiredText+" changed"}).ToArray();
            var value=before with{Requirements=rows,ComparedAtUtc=utc,Blockers=CompanyWorkOrderExecutionRequirements.GetBlockers(before.Approval,before.OriginalRequirementPopulation,rows,before.OriginalOrderRevision,before.Order,utc)};
            if(mode=="context-drift")value=value with{Context=value.Context with{Revision="changed-execution-authority"}};
            if(mode=="unordered" && reads==2)value=value with{Requirements=rows.Reverse().ToArray()};
            if(mode=="start-drift")recorded.ROW_CHANGED_BY="changed executor";return Task.FromResult(value);
        }
        var owner=new WorkOrderExecutionPermitService(native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        var error=await Record.ExceptionAsync(async()=>
        {
            var reference=new CompanyWorkOrderExecutionStartReference(query,command.IntentId);
            var result=await owner.ReviewSubmissionInspectionsAsync(mode=="foreign"?context with{ActorUserId="other-executor"}:context,reference,()=>Task.CompletedTask,Current);
            result.Require(context.Company,context.ActorUserId,reference);Assert.Equal(command.IntentId,result.Start.IntentId);
            if(mode is "current" or "unordered")Assert.Empty(result.Blockers);else Assert.NotEmpty(result.Blockers);
            Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(result));
        });
        if(mode is "source-drift" or "start-drift" or "foreign" or "context-drift")Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected source adoption");else Assert.Null(error);
        Assert.Equal(0,native.Writes);Assert.Empty(native.Transactions);
    }
    [Theory][InlineData("current")][InlineData("empty")][InlineData("foreign")][InlineData("changed")][InlineData("authority")]
    public async Task AnotherAuthorizedPersonaReadsLimitedStartHistoryWithoutPrivateRecoveryOrWrites(string mode)
    {
        var f=Original();var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();var command=f.Command;var row=command.Prepare(DateTime.UtcNow);if(mode!="empty")native.Rows.Add(row);
        var context=command.Review.Prerequisites.Requirements.Context with{ActorUserId="independent-viewer",Revision="current-view-authority"};var query=command.Review.Prerequisites.Requirements.Approval.Permit;
        var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_START")).Returns(true);native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns(()=>creator.ConvertToEntityStructure(typeof(WORK_ORDER_EXECUTION_START)));
        var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_START",EntityTypeName=typeof(WORK_ORDER_EXECUTION_START).FullName!,PrimaryKeyColumn="START_ID"});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_START")).ReturnsAsync(["START_ID"]);
        if(mode=="foreign"){row.OWNER_COMPANY_BA_ID="foreign";row.COMMAND_JSON="malformed private command";native.Source.Setup(value=>value.GetEntityAsync("WORK_ORDER_EXECUTION_START",It.IsAny<List<TheTechIdea.Beep.Report.AppFilter>>())).ReturnsAsync(()=>new[]{row});}
        if(mode=="changed")row.STARTED_AT_UTC=row.STARTED_AT_UTC.AddSeconds(1);
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException("Current view capability revoked")):Task.CompletedTask;
        var owner=new WorkOrderExecutionPermitService(native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        var error=await Record.ExceptionAsync(async()=>
        {
            var result=await owner.ReadRecordedStartAsync(context,query,Guard);if(mode=="empty"){Assert.Null(result);return;}
            result!.Require(context.Company,query);Assert.Equal(command.IntentId,result.IntentId);Assert.Equal(command.Review.Stage.AssignedRole,result.AssignedRole);
            var json=JsonSerializer.Serialize(result);Assert.DoesNotContain(command.Reason,json);Assert.DoesNotContain("COMMAND_JSON",json);Assert.DoesNotContain("IdentityCatalogRevision",json);
            await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>owner.RecoverExecutionStartAsync(context,new(query,command.IntentId),()=>Task.CompletedTask));
        });
        if(mode is "current" or "empty")Assert.Null(error);else Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected public start facts");
        Assert.Equal(0,native.Writes);Assert.Empty(native.Transactions);
    }
    internal static (CompanyWorkOrderExecutionPermitDecision Approval,Permit Recorded,CompanyWorkOrderExecutionStartCommand Command) Original(bool phaseAware=false)=>WorkOrderExecutionStartFixture.Original(phaseAware);
    [Theory][InlineData("current")][InlineData("phase-aware")][InlineData("schema")][InlineData("authority")][InlineData("write")][InlineData("readback")]
    [InlineData("identity-before")][InlineData("identity-after")][InlineData("stock-after")][InlineData("unknown")][InlineData("source")][InlineData("isolation")]
    [InlineData("stage-before")][InlineData("stage-after")]
    public async Task StartOwnsOneNativeInsertWithFullProofAndOriginalNoReplayRecovery(string mode)
    {
        // Actual permit/start/transaction owners; native prerequisite owners have separate primitive gates.
        var original=Original(mode=="phase-aware");var command=original.Command;var context=command.Review.Prerequisites.Requirements.Context;
        var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();native.Rows.Add(original.Recorded);
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(native.Editor.Object);native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
        foreach(var (type,key) in new[]{(typeof(Permit),"PERMIT_ID"),(typeof(WORK_ORDER_EXECUTION_START),"START_ID")})
        {
            var table=type.Name;metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=key});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
            native.Source.Setup(value=>value.CheckEntityExist(table)).Returns(()=>mode!="schema" || table!="WORK_ORDER_EXECUTION_START");
            native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>creator.ConvertToEntityStructure(type));
        }
        native.Source.Setup(value=>value.InsertEntity("WORK_ORDER_EXECUTION_START",It.IsAny<object>())).Returns((string _,object value)=>
        {
            native.Writes++;if(mode=="write")return new ErrorsInfo{Flag=Errors.Failed};var row=CompanyInventorySourceReview.Copy((WORK_ORDER_EXECUTION_START)value);if(mode=="readback")row.ROW_CREATED_BY="changed executor";native.Rows.Add(row);return new ErrorsInfo{Flag=Errors.Ok};
        });
        var owner=new WorkOrderExecutionPermitService(native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        var personnel=new Mock<ICompanyPersonnelAcceptanceCoreProof>();var documents=new Mock<ICompanyWorkOrderDocumentContributorCore>();var identities=new Mock<ICompanyRecordedReviewerIdentityCore>();var executor=new Mock<ICompanyWorkOrderExecutorIdentityOwner>();
        var identityCalls=0;var stockCalls=0;var coreCalls=0;var stageCalls=0;var forbidProof=false;
        void Owned(){PPDMDataSourceTransaction.RequireOwned(native.Source.Object,IsolationLevel.Serializable);if(forbidProof)throw new Exception("Recovery must not replay live proof or work start");}
        personnel.Setup(value=>value.RequireExecutionPersonnelCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyPersonnelPolicyEvidenceReview>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview actual,CompanyPersonnelPolicyEvidenceReview _,Func<Task> guard)=>{Owned();Assert.Equal(context,actual);coreCalls++;await guard();});
        documents.Setup(value=>value.RequireExecutionDocumentCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyWorkOrderAcceptedDocumentReview>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview actual,CompanyWorkOrderAcceptedDocumentReview _,Func<Task> guard)=>{Owned();Assert.Equal(context,actual);await guard();});
        identities.Setup(value=>value.RequireExecutionReviewerIdentityCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyPersonnelIdentityCatalog>(),It.IsAny<CompanyNativePersonFacts>(),It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview actual,CompanyPersonnelIdentityCatalog _,CompanyNativePersonFacts __,string ___,DateTime ____,DateTime _____,Func<Task> guard)=>{Owned();Assert.Equal(context,actual);await guard();});
        executor.Setup(value=>value.RequireExecutorIdentityCoreAsync(It.IsAny<CompanyWorkOrderExecutorIdentity>(),It.IsAny<Func<Task>>())).Returns(async(CompanyWorkOrderExecutorIdentity value,Func<Task> guard)=>{Owned();Assert.Equal(command.Review.SourceRevision(),value.SourceRevision());identityCalls++;if(mode=="identity-before" || mode=="identity-after" && identityCalls==2)throw new InvalidOperationException("Current assigned executor identity changed");await guard();});
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException("Current operational role/capability revoked")):Task.CompletedTask;
        Task<(Detail,IReadOnlyList<Condition>)> Read(bool owned){if(forbidProof)throw new Exception("Recovery cannot refresh readiness");if(owned)Owned();var order=CompanyInventorySourceReview.Copy(original.Approval.Review.CurrentSource.Order);if(mode=="source")order.Instructions="Changed approved work scope";return Task.FromResult<(Detail,IReadOnlyList<Condition>)>((order,original.Approval.Review.CurrentSource.Inspections));}
        Task Stock(CompanyWorkOrderExecutionPermitApprovalReview _){PPDMDataSourceTransaction.RequireIdle(native.Source.Object);if(forbidProof)throw new Exception("Recovery cannot recheck current stock");if(++stockCalls==2 && mode=="stock-after")throw new InvalidOperationException("Stock changed after recorded core start");return Task.CompletedTask;}
        Task<bool> OwnedStock(CompanyWorkOrderExecutionPermitApprovalReview _){Owned();return Task.FromResult(true);}
        Task<CompanyMaintenanceAssignedStageReview> Stage(){PPDMDataSourceTransaction.RequireIdle(native.Source.Object);if(forbidProof)throw new Exception("Recovery cannot replay assigned-stage proof");stageCalls++;return Task.FromResult(mode=="stage-before" || mode=="stage-after" && stageCalls==2?command.Review.Stage with{AssignedRole="Changed configured role"}:command.Review.Stage);}
        if(mode=="unknown")native.FailCommit=true;if(mode=="isolation")native.Isolation=IsolationLevel.ReadCommitted;
        Task<CompanyWorkOrderExecutionStartReceipt> Start()=>owner.StartExecutionAsync(command,Guard,Read,personnel.Object,documents.Object,identities.Object,executor.Object,Stock,OwnedStock,Stage);
        var before=JsonSerializer.Serialize(original.Recorded);var error=await Record.ExceptionAsync(async()=>{var receipt=await Start();receipt.Require(command);});
        if(mode is "current" or "phase-aware")Assert.Null(error);else Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected start");
        if(mode is "current" or "phase-aware" or "unknown" or "stock-after" or "stage-after")
        {
            native.FailCommit=false;var row=Assert.Single(native.Rows.OfType<WORK_ORDER_EXECUTION_START>());command.RequireRecorded(row);Assert.Equal(1,native.Writes);Assert.Equal(2,identityCalls);Assert.Equal(2,coreCalls);forbidProof=true;
            var reference=new CompanyWorkOrderExecutionStartReference(command.Review.Prerequisites.Requirements.Approval.Permit,command.IntentId);var recovery=await owner.RecoverExecutionStartAsync(context,reference,()=>Task.CompletedTask);recovery.Require(context.Company,context.ActorUserId,reference);
            (await Start()).Require(command);Assert.Equal(1,native.Writes);await Assert.ThrowsAsync<InvalidOperationException>(()=>owner.StartExecutionAsync(command with{IntentId=Guid.NewGuid()},()=>Task.CompletedTask,Read,personnel.Object,documents.Object,identities.Object,executor.Object,Stock,OwnedStock,Stage));Assert.Equal(1,native.Writes);
        }
        else Assert.Empty(native.Rows.OfType<WORK_ORDER_EXECUTION_START>());
        Assert.Equal(before,JsonSerializer.Serialize(Assert.Single(native.Rows.OfType<Permit>())));PPDMDataSourceTransaction.RequireIdle(native.Source.Object);
    }
    [Theory][InlineData("self-map")][InlineData("other-person")][InlineData("company")][InlineData("employment")][InlineData("window")][InlineData("utc")][InlineData("audit")]
    public void StartCannotUsePersonaNamesSelfAssertedOrChangedIdentityAndAudit(string mode)
    {
        var f=Original();var command=f.Command;var review=command.Review;
        if(mode=="self-map")review=review with{Identity=review.Identity with{RecordedBy=review.Identity.UserId}};
        if(mode=="other-person")review=review with{Identity=review.Identity with{PersonBaId="other-person"}};
        if(mode=="company")review=review with{Person=review.Person with{Company=new("tenant","foreign")}};
        if(mode=="employment")review=review with{Person=review.Person with{Employment=review.Person.Employment with{Revision=new string('C',64)}}};
        if(mode=="window")review=review with{Identity=review.Identity with{EffectiveToUtc=review.ComparedAtUtc}};
        review=review with{Revision=CompanyWorkOrderExecutorIdentity.Digest(review)};command=command with{Review=review};if(mode=="utc")command=command with{StartedAtUtc=DateTime.SpecifyKind(command.StartedAtUtc,DateTimeKind.Unspecified)};
        Assert.ThrowsAny<Exception>(()=>{command.Require();var row=command.Prepare(DateTime.UtcNow);if(mode=="audit"){row.EXECUTOR_USER_ID="another-executor";row.RECORDED_REVISION=CompanyWorkOrderExecutionStartCommand.Digest(row);command.RequireRecorded(row);}});
    }
}
#endif
