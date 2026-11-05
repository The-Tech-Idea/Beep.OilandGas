#if WORKFLOW_INBOX_FOCUSED
extern alias actualOwners;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.DataManagement.Models;
using System.Data;
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Moq;
using TheTechIdea.Beep.Editor;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Utilities;
using TheTechIdea.Beep.Tools;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using TheTechIdea.Beep.ConfigUtil;
using Xunit;
using Permit=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT;
using Detail=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class WorkOrderExecutionPermitTests
{
    [Theory][InlineData("current")][InlineData("phase-aware")][InlineData("separate-stock")][InlineData("issuer")][InlineData("additional-material")]
    [InlineData("personnel-after")][InlineData("document-after")][InlineData("identity-after")]
    [InlineData("stock-after")][InlineData("stock-boundary")][InlineData("source-after")][InlineData("recorded-after")][InlineData("authority")]
    public async Task ExecutionPrerequisitesCloseExecutorOwnedCoreAndSeparateStockWithoutPublishingPrivateApproval(string mode)
    {
        var f=new Fixture();var complete=WorkOrderCompleteApprovalFixture.Create(mode=="phase-aware",additionalMaterial:mode=="additional-material");
        var decision=new CompanyWorkOrderExecutionPermitDecision(complete.Review,Guid.NewGuid(),"Approve","Private original HSE approval rationale");
        var stored=decision.Prepare(complete.Pending,DateTime.UtcNow);f.Native.Rows.Add(stored);
        var executor=complete.Review.Context with{ActorUserId=mode=="issuer"?complete.Review.Proposal.Context.ActorUserId:"executor",Revision="current-executor-authority"};
        var personnel=new Mock<ICompanyPersonnelAcceptanceCoreProof>();var documents=new Mock<ICompanyWorkOrderDocumentContributorCore>();var identities=new Mock<ICompanyRecordedReviewerIdentityCore>();
        var personnelReads=0;var documentReads=0;var identityReads=0;var stockReads=0;var ownedStockReads=0;var sourceReads=0;
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException("Actual execution permission revoked")):Task.CompletedTask;
        void Owned(FieldSelectionReview context){Assert.Equal(executor,context);PPDMDataSourceTransaction.RequireOwned(f.Native.Source.Object,IsolationLevel.Serializable);}
        personnel.Setup(value=>value.RequireExecutionPersonnelCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyPersonnelPolicyEvidenceReview>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview context,CompanyPersonnelPolicyEvidenceReview original,Func<Task> guard)=>
        {Owned(context);Assert.Equal(JsonSerializer.Serialize(complete.Review.CurrentPersonnelAcceptance!.Evidence),JsonSerializer.Serialize(original));await guard();if(++personnelReads==2 && mode=="personnel-after")throw new InvalidOperationException("Changed current personnel");});
        documents.Setup(value=>value.RequireExecutionDocumentCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyWorkOrderAcceptedDocumentReview>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview context,CompanyWorkOrderAcceptedDocumentReview original,Func<Task> guard)=>
        {Owned(context);Assert.Contains(original.Acceptance,complete.Review.CurrentDocumentAcceptance!.Select(row=>row.Acceptance));await guard();if(++documentReads==3 && mode=="document-after")throw new InvalidOperationException("Changed current document");});
        identities.Setup(value=>value.RequireExecutionReviewerIdentityCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyPersonnelIdentityCatalog>(),It.IsAny<CompanyNativePersonFacts>(),It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview context,CompanyPersonnelIdentityCatalog catalog,CompanyNativePersonFacts person,string reviewer,DateTime from,DateTime to,Func<Task> guard)=>
        {Owned(context);Assert.NotEqual(context.ActorUserId,reviewer);await guard();if(++identityReads==3 && mode=="identity-after")throw new InvalidOperationException("Changed current reviewer identity");});
        Task<(Detail,IReadOnlyList<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>)> Read(bool owned)
        {
            if(owned)PPDMDataSourceTransaction.RequireOwned(f.Native.Source.Object,IsolationLevel.Serializable);else PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);
            var order=CompanyInventorySourceReview.Copy(complete.Review.CurrentSource.Order);if(++sourceReads==5 && mode=="source-after")order.Instructions="Changed original scope";
            return Task.FromResult<(Detail,IReadOnlyList<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>)>((order,complete.Review.CurrentSource.Inspections));
        }
        Task Stock(CompanyWorkOrderExecutionPermitApprovalReview original){PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);Assert.Equal(JsonSerializer.Serialize(decision.Review),JsonSerializer.Serialize(original));if(++stockReads==2 && mode=="stock-after")throw new InvalidOperationException("Separate stock changed after core snapshot");return Task.CompletedTask;}
        Task<bool> OwnedStock(CompanyWorkOrderExecutionPermitApprovalReview original){Owned(executor);ownedStockReads++;if(mode=="recorded-after" && ownedStockReads==2)stored.SCOPE_DESCRIPTION="Changed approved scope";return Task.FromResult(mode=="separate-stock"?false:mode=="stock-boundary"?ownedStockReads==1:true);}
        var originalJson=JsonSerializer.Serialize(decision);CompanyWorkOrderExecutionPrerequisites? result=null;
        var error=await Record.ExceptionAsync(async()=>result=await f.Owner.ReadExecutionPrerequisitesAsync(executor,decision.ProposalReference(),Guard,Read,personnel.Object,documents.Object,identities.Object,Stock,OwnedStock));
        if(mode is "current" or "phase-aware" or "separate-stock" or "issuer" or "additional-material")
        {
            Assert.Null(error);result!.Require(executor.Company,executor.ActorUserId,decision.ProposalReference());Assert.Equal(mode!="separate-stock",result.StockSharesCore);
            Assert.Equal(2,personnelReads);Assert.Equal(4,documentReads);Assert.Equal(4,identityReads);Assert.Equal(2,stockReads);Assert.Equal(2,ownedStockReads);
            var json=JsonSerializer.Serialize(result);Assert.DoesNotContain(decision.Rationale,json);Assert.DoesNotContain("CurrentIdentities",json);Assert.DoesNotContain("DECISION_DATA_JSON",json);
            if(mode=="additional-material")Assert.Equal(2,result.Requirements.Requirements.Count(row=>row.PermitPrerequisiteKind=="MATERIAL"));
        }
        else{Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected execution readiness publication");Assert.Null(result);}
        Assert.Equal(originalJson,JsonSerializer.Serialize(decision));Assert.Equal(0,f.Native.Writes);PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);
    }
    [Fact]
    public async Task ExecutionRequirementPublicationDetachesNativeObjectsBeforeClosingAuthority()
    {
        var f=new Fixture();var complete=WorkOrderCompleteApprovalFixture.Create();
        var decision=new CompanyWorkOrderExecutionPermitDecision(complete.Review,Guid.NewGuid(),"Approve","Private approval rationale");
        f.Native.Rows.Add(decision.Prepare(complete.Pending,DateTime.UtcNow));
        var context=complete.Review.Context with{ActorUserId="executor",Revision="current-execution-authority"};
        Detail? borrowed=null;var reads=0;var changed=false;
        Task Guard()
        {
            if(reads==2 && !changed){borrowed!.Instructions="Provider object changed during closing authority";changed=true;}
            return Task.CompletedTask;
        }
        Task<(Detail Order,IReadOnlyList<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition> Requirements)> Current()
        {
            reads++;borrowed=CompanyInventorySourceReview.Copy(complete.Review.CurrentSource.Order);
            return Task.FromResult<(Detail,IReadOnlyList<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>)>((borrowed,complete.Review.CurrentSource.Inspections.ToArray()));
        }
        var result=await f.Owner.ReadExecutionRequirementsAsync(context,decision.ProposalReference(),Guard,Current);
        result.Require(context.Company,"executor",decision.ProposalReference());
        Assert.True(changed);Assert.Empty(result.Blockers);
        Assert.Equal(complete.Review.CurrentSource.Order.Instructions,result.Order.Instructions);
        Assert.NotSame(borrowed,result.Order);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("current")][InlineData("phase-aware")][InlineData("changed")][InlineData("drift")][InlineData("revoked")][InlineData("failed-pre-start")][InlineData("order-changed")][InlineData("order-reordered")]
    public async Task ExecutionRequirementComparisonUsesCurrentActorAndNeverAdoptsChangedPopulation(string mode)
    {
        var f=new Fixture();var complete=WorkOrderCompleteApprovalFixture.Create(mode=="phase-aware");var decision=new CompanyWorkOrderExecutionPermitDecision(complete.Review,Guid.NewGuid(),"Approve","Private approver rationale");f.Native.Rows.Add(decision.Prepare(complete.Pending,DateTime.UtcNow));var context=complete.Review.Context with{ActorUserId="executor",Revision="execution-view-authority"};var reads=0;
        Task Guard()=>mode=="revoked"?Task.FromException(new UnauthorizedAccessException("Revoked current execution-view authority")):Task.CompletedTask;
        Task<(Detail Order,IReadOnlyList<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition> Requirements)> Current()
        {
            reads++;var rows=complete.Review.CurrentSource.Inspections.Select(row=>mode=="changed" || mode=="drift" && reads==2?row with{RequiredText="Changed native requirement"}:mode=="failed-pre-start" && row.CondSeq==1?row with{Status="FAIL",ResultText="Current pre-start failure"}:row).ToArray();var order=CompanyInventorySourceReview.Copy(complete.Review.CurrentSource.Order);if(mode=="order-changed")order.Instructions="Changed approved order scope";if(mode=="order-reordered" && reads==2)order.Components.Reverse();return Task.FromResult<(Detail,IReadOnlyList<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>)>((order,rows));
        }
        var error=await Record.ExceptionAsync(async()=>{var result=await f.Owner.ReadExecutionRequirementsAsync(context,decision.ProposalReference(),Guard,Current);result.Require(context.Company,"executor",decision.ProposalReference());Assert.Equal(mode is "changed" or "failed-pre-start" or "order-changed",result.Blockers.Count>0);Assert.DoesNotContain(decision.Rationale,JsonSerializer.Serialize(result));if(mode=="phase-aware")Assert.Equal("UNRECORDED",result.Requirements.Single(row=>row.InspectionId=="future-operation").Status);});
        if(mode is "drift" or "revoked")Assert.NotNull(error);else Assert.Null(error);Assert.Equal(mode=="revoked"?0:2,reads);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("current")][InlineData("phase-aware")][InlineData("foreign")][InlineData("drift")][InlineData("revoked")][InlineData("pending")][InlineData("reject")]
    public async Task RecordedApprovalCanBeReadByAnotherAuthorizedActorWithoutPrivateDecisionOrWrites(string mode)
    {
        var f=new Fixture();var complete=WorkOrderCompleteApprovalFixture.Create(mode=="phase-aware");var decision=new CompanyWorkOrderExecutionPermitDecision(complete.Review,Guid.NewGuid(),mode=="reject"?"Reject":"Approve","Private original reviewer rationale");
        var recorded=mode=="pending"?CompanyInventorySourceReview.Copy(complete.Pending):decision.Prepare(complete.Pending,DateTime.UtcNow);if(mode=="foreign")recorded.BUSINESS_ASSOCIATE_ID="foreign";f.Native.Rows.Add(recorded);var context=complete.Review.Context with{ActorUserId="executor",Revision="current-execution-authority"};var calls=0;
        Task Guard(){calls++;if(mode=="revoked")throw new UnauthorizedAccessException("Revoked execution viewer");if(mode=="drift" && calls==3)recorded.SCOPE_DESCRIPTION="Changed recorded scope";return Task.CompletedTask;}
        var error=await Record.ExceptionAsync(async()=>{var result=await f.Owner.ReadRecordedApprovalAsync(context,decision.ProposalReference(),Guard);result.Require(context.Company,decision.ProposalReference());Assert.Equal(decision.IntentId,result.DecisionId);Assert.Equal(decision.Review.Context.ActorUserId,result.Approver);Assert.Equal(decision.Review.OriginalSource.RequirementFactsRevision(),result.OriginalRequirementRevision);var json=JsonSerializer.Serialize(result);Assert.DoesNotContain(decision.Rationale,json);Assert.DoesNotContain("DECISION_DATA_JSON",json);Assert.DoesNotContain("CurrentMaterialAcceptance",json);});
        if(mode is "current" or "phase-aware")Assert.Null(error);else Assert.NotNull(error);Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Transactions);
    }
    [Theory][InlineData("record")][InlineData("missing-current")][InlineData("missing-core")][InlineData("before-current")][InlineData("before-core")][InlineData("after-core")][InlineData("after-current")][InlineData("unknown")][InlineData("committed")][InlineData("committed-failure")]
    public async Task CompleteApprovalOwnerClosesDelegatedProofAndRecoversExactCommitWithoutReplaying(string mode)
    {
        // Real native decision/transaction owner with controlled proof callbacks; full API proof is a separate gate.
        var f=new Fixture();var complete=WorkOrderCompleteApprovalFixture.Create();f.Native.Rows.Add(CompanyInventorySourceReview.Copy(complete.Pending));
        var decision=new CompanyWorkOrderExecutionPermitDecision(complete.Review,Guid.NewGuid(),"Approve","Independent complete original approval");var before=JsonSerializer.Serialize(complete.Pending);var currentCalls=0;var coreCalls=0;
        Task<CompanyMaintenanceInspectionReview> Source(){PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);return Task.FromResult(complete.Review.CurrentSource);}
        Task Current()
        {
            PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);currentCalls++;complete.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
            if(mode=="before-current" || mode=="after-current" && currentCalls==2)throw new InvalidOperationException("Current proof changed");return Task.CompletedTask;
        }
        Task Core()
        {
            PPDMDataSourceTransaction.RequireOwned(f.Native.Source.Object,IsolationLevel.Serializable);coreCalls++;complete.Review.RequireCurrentPrerequisiteAcceptance(DateTime.UtcNow);
            if(mode=="before-core" || mode=="after-core" && coreCalls==2)throw new InvalidOperationException("Owned proof changed");return Task.CompletedTask;
        }
        var committedCalls=0;Task Committed(){PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);committedCalls++;Assert.Equal(1,currentCalls);Assert.Equal(2,coreCalls);decision.RequireRecorded(Assert.Single(f.Native.Rows.OfType<Permit>()));if(mode=="committed-failure")throw new InvalidOperationException("Separate provider closure changed");return Task.CompletedTask;}
        if(mode=="unknown")f.Native.FailCommit=true;
        var error=await Record.ExceptionAsync(()=>f.Owner.DecideAsync(decision,f.Authority,Source,requireCurrentPrerequisiteProof:mode=="missing-current"?null:Current,requireCorePrerequisiteProof:mode=="missing-core"?null:Core,requireCommittedPrerequisiteProof:mode is "committed" or "committed-failure"?Committed:null));
        if(mode is "record" or "committed")Assert.Null(error);else Assert.IsType<InvalidOperationException>(error);
        var recorded=Assert.Single(f.Native.Rows.OfType<Permit>());
        if(mode is "record" or "unknown" or "after-current" or "committed" or "committed-failure")
        {
            decision.RequireRecorded(recorded);Assert.Equal("APPROVED",recorded.STATUS);Assert.Equal(1,f.Native.Writes);f.Native.FailCommit=false;
            var recovered=await f.Owner.ReadOriginalDecisionAsync(decision.Review.Context,new(decision.ProposalReference(),decision.IntentId),f.Authority);
            Assert.Equal(JsonSerializer.Serialize(decision),JsonSerializer.Serialize(recovered));
            var receipt=await f.Owner.DecideAsync(recovered,f.Authority,Source,true);receipt.Require(decision);Assert.Equal(1,f.Native.Writes);
            Assert.Equal(before,JsonSerializer.Serialize(recovered.ReadOriginalPending(recorded)));
            if(mode=="record"){Assert.Equal(2,currentCalls);Assert.Equal(2,coreCalls);}if(mode is "committed" or "committed-failure"){Assert.Equal(1,committedCalls);Assert.Equal(1,currentCalls);Assert.Equal(2,coreCalls);}
        }
        else
        {
            Assert.Equal(before,JsonSerializer.Serialize(recorded));Assert.Equal(mode=="after-core"?1:0,f.Native.Writes);
            if(mode is "before-core" or "after-core")Assert.Contains("rollback",f.Native.Transactions);
            if(mode is "missing-current" or "missing-core")Assert.Empty(f.Native.Transactions);
        }
    }
    private sealed class Fixture
    {
        internal PersonaTaskRoutingTests.Fixture Native=new();
        internal CompanyWorkOrderExecutionPermitIntent Intent;
        internal CompanyMaintenanceInspectionReview Evidence;
        internal WorkOrderExecutionPermitService Owner;
        internal int Guards,RevokeAt,Reads;
        internal bool MissingSchema,ChangedSchema,ChangedSource,FailWrite;
        internal Fixture()
        {
            Native.Rows.Clear();var company=new CompanyScope("tenant","company");var database=new AssetDatabaseScope("lifecycle","fingerprint");
            var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(company,Guid.NewGuid()),"field","order/العالم%2F");
            var stage=new CompanyMaintenanceAssignedStageReview(new(company,database,"issuer","authority","field"),task,"step","execute","Configured duty","ConfiguredRole","receiver","history","approval",new string('A',64),new string('B',64));
            var order=new Detail{InstanceId=task.WorkOrderId,FieldId="field",Source="WORK_ORDER",State="",EvidenceUnavailableReason="No execution status",Components=[new(task.WorkOrderId,"component","field",null,null)]};
            Evidence=new(stage,database,order,[],CompanyMaintenanceInspectionReview.Revision(stage,database,order,[]),DateTimeOffset.UtcNow);
            Intent=new(stage.Context,task,Guid.NewGuid(),Evidence.SnapshotRevision,"ElectricalIsolation","Explicit reported jurisdiction","Original work scope","isolation-document","risk-document","personnel-document","material-document",DateTime.UtcNow,DateTime.UtcNow.AddHours(8));
            var creator=new ClassCreator(Native.Editor.Object);Native.Editor.SetupGet(value=>value.classCreator).Returns(creator);
            Native.Source.Setup(value=>value.CheckEntityExist("WORK_ORDER_EXECUTION_PERMIT")).Returns(()=>!MissingSchema);
            Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure _,bool __)=>
            {
                var schema=creator.ConvertToEntityStructure(typeof(Permit));foreach(var field in schema.Fields){field.IsKey=field.FieldName=="PERMIT_ID";if(field.IsKey)field.AllowDBNull=false;}
                if(ChangedSchema)schema.Fields.RemoveAll(field=>field.FieldName=="ISOLATION_REFERENCE");return schema;
            });
            var metadata=new Mock<IPPDMMetadataRepository>();metadata.Setup(value=>value.GetTableMetadataAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(new PPDMTableMetadata{TableName="WORK_ORDER_EXECUTION_PERMIT",EntityTypeName=typeof(Permit).FullName!,PrimaryKeyColumn="PERMIT_ID"});metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync("WORK_ORDER_EXECUTION_PERMIT")).ReturnsAsync(["PERMIT_ID"]);
            Native.Source.Setup(value=>value.InsertEntity("WORK_ORDER_EXECUTION_PERMIT",It.IsAny<object>())).Returns((string _,object row)=>{Native.Writes++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows.Add(JsonSerializer.Deserialize<Permit>(JsonSerializer.Serialize(row))!);return new ErrorsInfo{Flag=Errors.Ok};});
            Native.Source.Setup(value=>value.UpdateEntity("WORK_ORDER_EXECUTION_PERMIT",It.IsAny<object>())).Returns((string _,object row)=>{Native.Writes++;if(FailWrite)return new ErrorsInfo{Flag=Errors.Failed};var changed=JsonSerializer.Deserialize<Permit>(JsonSerializer.Serialize(row))!;var index=Native.Rows.FindIndex(value=>value is Permit existing && existing.PERMIT_ID==changed.PERMIT_ID);if(index<0)return new ErrorsInfo{Flag=Errors.Failed};Native.Rows[index]=changed;return new ErrorsInfo{Flag=Errors.Ok};});
            Owner=new(Native.Editor.Object,Mock.Of<Beep.OilandGas.Models.Core.Interfaces.ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object);
        }
        internal Task Authority(){if(++Guards==RevokeAt)throw new UnauthorizedAccessException("Current HSE issue authority changed");return Task.CompletedTask;}
        internal Task<CompanyMaintenanceInspectionReview> Read(){PPDMDataSourceTransaction.RequireIdle(Native.Source.Object);Reads++;return Task.FromResult(ChangedSource?Evidence with{SnapshotRevision=new string('D',64)}:Evidence);}
        internal Task<CompanyWorkOrderExecutionPermitReceipt> Issue()=>Owner.IssueAsync(Intent,Authority,Read);
        internal Task<CompanyWorkOrderExecutionPermitReceipt> Recover()=>Owner.RecoverAsync(Intent,Authority,Read);
    }
    [Fact] public async Task OriginalIssueCommitsOnePendingProposalAndExactRetryOrRecoveryNeverWritesAgain()
    {
        var f=new Fixture();var receipt=await f.Issue();receipt.Require(f.Intent);Assert.Equal(1,f.Native.Writes);var row=Assert.Single(f.Native.Rows.OfType<Permit>());f.Intent.RequireRecorded(row);Assert.Equal("ISSUED_PENDING_APPROVAL",row.STATUS);Assert.Equal("issuer",row.ISSUED_BY);
        var retry=await f.Issue();var recovered=await f.Recover();Assert.Equal(receipt.RecordedRevision,retry.RecordedRevision);Assert.Equal(receipt.RecordedRevision,recovered.RecordedRevision);Assert.Equal(1,f.Native.Writes);Assert.True(f.Reads>=6);PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);
    }
    [Theory][InlineData("current")][InlineData("missing-reader")][InlineData("missing-proof")]
    public async Task VersionedNativeIssueRetainsRequirementProofAndUsesOriginalVersionForIndependentReview(string mode)
    {
        var f=new Fixture();var pending=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=f.Intent.Task.FieldId,CondSeq=1,CondType="PRE_START",RequiredText="Original immutable requirement",Jurisdiction=f.Intent.Jurisdiction,RegulatoryRef=f.Intent.IsolationReference,PermitPrerequisiteKind="ISOLATION"},f.Intent.Task.WorkOrderId,"requirement-author","requirement",DateTime.UtcNow);var rows=new[]{actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(pending)};f.Evidence=f.Evidence with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(f.Evidence.Stage,f.Evidence.InspectionDatabase,f.Evidence.Order,rows)};f.Intent=f.Intent with{InspectionRevision=f.Evidence.SnapshotRevision,RequirementEvidenceVersion=1};var receipt=await f.Issue();receipt.Require(f.Intent);f.Intent.RequireRecorded(Assert.Single(f.Native.Rows.OfType<Permit>()));Assert.Equal(1,f.Native.Writes);
        var current=f.Intent.Context with{ActorUserId="independent",Revision="review-authority"};var stage=f.Evidence.Stage with{Context=current};var observed=f.Evidence with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,f.Evidence.InspectionDatabase,f.Evidence.Order,rows)};if(mode=="missing-proof"){var missing=rows.Select(row=>row with{RequirementRevision=null}).ToArray();observed=observed with{Inspections=missing,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,observed.InspectionDatabase,observed.Order,missing)};}
        Func<Task<CompanyMaintenanceInspectionReview>>? readVersioned=mode=="missing-reader"?null:()=>Task.FromResult(observed);var error=await Record.ExceptionAsync(async()=>{var review=await f.Owner.ReviewApprovalAsync(current,new(f.Intent.Task,f.Intent.IntentId),f.Authority,()=>throw new Exception("Cannot substitute legacy source for versioned proposal"),readVersioned);review.Require(current.Company,current.ActorUserId,new(f.Intent.Task,f.Intent.IntentId));Assert.Equal(rows[0].RequirementRevision,Assert.Single(review.CurrentSource.Inspections).RequirementRevision);});if(mode=="current")Assert.Null(error);else Assert.IsType<InvalidOperationException>(error);Assert.Equal(1,f.Native.Writes);
    }
    [Theory][InlineData(0)][InlineData(1)]
    public async Task RetainedIssuePoliciesCannotAdoptExplicitPhaseApplicability(int version)
    {
        var f=new Fixture();var pending=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=f.Intent.Task.FieldId,CondSeq=1,CondType="OPERATIONAL",RequiredText="Work performed before submission",Jurisdiction=f.Intent.Jurisdiction,ApplicableStage="WORK_SUBMISSION"},f.Intent.Task.WorkOrderId,"requirement-author","requirement",DateTime.UtcNow);
        var rows=new[]{actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(pending)};f.Evidence=f.Evidence with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(f.Evidence.Stage,f.Evidence.InspectionDatabase,f.Evidence.Order,rows)};f.Intent=f.Intent with{InspectionRevision=f.Evidence.SnapshotRevision,RequirementEvidenceVersion=version};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Issue());Assert.Equal(0,f.Native.Writes);Assert.Empty(f.Native.Rows);
    }
    [Fact]
    public async Task PhaseAwareNativeIssueAndOriginalRecoveryKeepFutureRequirementsUnperformed()
    {
        var f=new Fixture();var pending=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=f.Intent.Task.FieldId,CondSeq=1,CondType="PRE_START",RequiredText="Original classified pre-start",Jurisdiction=f.Intent.Jurisdiction,ApplicableStage="PRE_START"},f.Intent.Task.WorkOrderId,"requirement-author","pre",DateTime.UtcNow);
        var future=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=f.Intent.Task.FieldId,CondSeq=2,CondType="OPERATIONAL",RequiredText="Inspect actual work before submission",Jurisdiction=f.Intent.Jurisdiction,ApplicableStage="WORK_SUBMISSION"},f.Intent.Task.WorkOrderId,"requirement-author","future",DateTime.UtcNow);
        var rows=new[]{actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(pending),actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.ProjectWithRequirementProof(future)};f.Evidence=f.Evidence with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(f.Evidence.Stage,f.Evidence.InspectionDatabase,f.Evidence.Order,rows)};f.Intent=f.Intent with{InspectionRevision=f.Evidence.SnapshotRevision,RequirementEvidenceVersion=2};var receipt=await f.Issue();receipt.Require(f.Intent);Assert.Equal(receipt,await f.Recover());Assert.Equal(1,f.Native.Writes);
        var reviewer=f.Intent.Context with{ActorUserId="independent",Revision="independent-authority"};var query=new CompanyWorkOrderExecutionPermitReference(f.Intent.Task,f.Intent.IntentId);Task<CompanyMaintenanceInspectionReview> Current(){var stage=f.Evidence.Stage with{Context=f.Evidence.Stage.Context with{ActorUserId="independent"}};return Task.FromResult(f.Evidence with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,f.Evidence.InspectionDatabase,f.Evidence.Order,f.Evidence.Inspections)});}
        var review=await f.Owner.ReviewApprovalAsync(reviewer,query,()=>Task.CompletedTask,()=>throw new InvalidOperationException("Phase-aware review cannot fall back to legacy source"),Current);Assert.Equal(7,review.PrerequisiteEvidenceVersion);Assert.Equal(2,review.Proposal.RequirementEvidenceVersion);Assert.Equal("UNRECORDED",review.CurrentSource.Inspections.Single(row=>row.InspectionId=="future").Status);Assert.Equal(1,f.Native.Writes);
    }
    [Fact] public async Task AbsentOriginalIssueCannotBeCreatedByReadOnlyRecovery(){var f=new Fixture();await Assert.ThrowsAsync<InvalidOperationException>(f.Recover);Assert.Empty(f.Native.Rows);Assert.Equal(0,f.Native.Writes);}
    [Theory][InlineData("accepted")][InlineData("unlabelled")][InlineData("issuer")][InlineData("reference")][InlineData("jurisdiction")][InlineData("duplicate")][InlineData("future")]
    public void PrerequisiteProjectionUsesExactIndependentRecordedReferencesAndKeepsInventorySeparate(string fault)
    {
        var f=new Fixture();var utc=DateTime.UtcNow;var rows=new List<actualOwners::Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>();var seq=0;
        foreach(var pair in new[]{("ISOLATION",f.Intent.IsolationReference),("RISK",f.Intent.RiskReference),("PERSONNEL",f.Intent.PersonnelReference),("MATERIAL",f.Intent.MaterialReference)})
        {
            var row=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareRequirement(new(){FieldId="field",CondSeq=++seq,CondType="PRE_START",RequiredText="Explicit prerequisite",Jurisdiction=fault=="jurisdiction"?"other":f.Intent.Jurisdiction,RegulatoryRef=fault=="reference"?"other":pair.Item2,PermitPrerequisiteKind=fault=="unlabelled"?null:pair.Item1},f.Intent.Task.WorkOrderId,"planner","inspection-"+seq,utc.AddMinutes(-2));
            var result=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.PrepareResult(row,new(){FieldId="field",InspectionId=row.INSPECTION_ID,ExpectedSourceRevision=actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Revision(row),Result="PASS",Notes="Recorded evidence"},fault=="issuer"?"issuer":"independent",fault=="future"?utc.AddMinutes(2):utc.AddMinutes(-1));rows.Add(actualOwners::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules.Project(result));
        }
        if(fault=="duplicate")rows.Add(rows[0] with{InspectionId="another-inspection",CondSeq=5});var source=f.Evidence with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(f.Evidence.Stage,f.Evidence.InspectionDatabase,f.Evidence.Order,rows)};var proposal=f.Intent with{InspectionRevision=source.SnapshotRevision};var evidence=CompanyWorkOrderPermitPrerequisiteEvidence.Read(proposal,source,utc);Assert.Equal(4,evidence.Count);Assert.Equal(fault=="accepted",evidence.All(value=>value.Accepted));if(fault is not ("accepted" or "duplicate"))Assert.All(evidence,item=>Assert.False(item.Accepted));
        var blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,source,source,utc,1);Assert.Contains(blockers,value=>value.Contains("inventory quantity",StringComparison.Ordinal));if(fault=="accepted")Assert.DoesNotContain(blockers,value=>value.Contains("inspection acceptance is not yet",StringComparison.Ordinal));
    }
    private static async Task<(Fixture Fixture,CompanyWorkOrderExecutionPermitDecision Decision,CompanyMaintenanceInspectionReview Source)> DecisionFixture()
    {
        var f=new Fixture();await f.Issue();var context=f.Intent.Context with{ActorUserId="independent",Revision="independent-authority"};var stage=f.Evidence.Stage with{Context=f.Evidence.Stage.Context with{ActorUserId=context.ActorUserId,Revision="independent-stage"}};
        var source=f.Evidence with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,f.Evidence.InspectionDatabase,f.Evidence.Order,f.Evidence.Inspections)};
        var review=await f.Owner.ReviewApprovalAsync(context,new(f.Intent.Task,f.Intent.IntentId),f.Authority,()=>Task.FromResult(source));return(f,new(review,Guid.NewGuid(),"Reject","Recorded independent rejection rationale"),source);
    }
    [Fact] public async Task IndependentRejectionPreservesOriginalProposalAndExactRecoveryDoesNotWriteAgain()
    {
        var (f,decision,source)=await DecisionFixture();var writes=f.Native.Writes;var originalIssue=Assert.Single(f.Native.Rows.OfType<Permit>()).ISSUE_DATA_JSON;Task<CompanyMaintenanceInspectionReview> Read(){PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);return Task.FromResult(source);}
        var receipt=await f.Owner.DecideAsync(decision,f.Authority,Read);receipt.Require(decision);var row=Assert.Single(f.Native.Rows.OfType<Permit>());decision.RequireRecorded(row);Assert.Equal("REJECTED",row.STATUS);Assert.Equal(originalIssue,row.ISSUE_DATA_JSON);Assert.Equal(writes+1,f.Native.Writes);
        Assert.Equal(receipt,await f.Owner.DecideAsync(decision,f.Authority,Read));Assert.Equal(receipt,await f.Owner.DecideAsync(decision,f.Authority,Read,true));Assert.Equal(writes+1,f.Native.Writes);
        var restored=await f.Owner.ReadOriginalDecisionAsync(decision.Review.Context,new(decision.ProposalReference(),decision.IntentId),f.Authority);Assert.Equal(JsonSerializer.Serialize(decision),JsonSerializer.Serialize(restored));Assert.Equal(writes+1,f.Native.Writes);
        var pending=restored.ReadOriginalPending(row);restored.RequirePending(pending);Assert.Equal(originalIssue,pending.ISSUE_DATA_JSON);Assert.Equal("ISSUED_PENDING_APPROVAL",pending.STATUS);Assert.Null(pending.DECISION_DATA_JSON);
        var current=await f.Owner.ReviewApprovalAsync(decision.Review.Context,decision.ProposalReference(),f.Authority,Read,recordedDecision:restored);
        Assert.Equal(decision.Review.RecordedRevision,current.RecordedRevision);Assert.Equal(source.SnapshotRevision,current.CurrentSource.SnapshotRevision);Assert.Equal(writes+1,f.Native.Writes);Assert.Equal("REJECTED",row.STATUS);
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Owner.ReviewApprovalAsync(decision.Review.Context with{ActorUserId="foreign"},decision.ProposalReference(),f.Authority,Read,recordedDecision:restored));
    }
    [Fact] public async Task HistoricalReviewPolicyPreservesExactRejectedIntentJsonAndReferenceRecovery()
    {
        var (f,decision,source)=await DecisionFixture();var review=decision.Review with{PrerequisiteEvidenceVersion=0,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(decision.Review.Proposal,decision.Review.OriginalSource,decision.Review.CurrentSource,decision.Review.ComparedAtUtc)};decision=decision with{Review=review};Assert.DoesNotContain("PrerequisiteEvidenceVersion",JsonSerializer.Serialize(decision));
        var receipt=await f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source));var recovered=await f.Owner.ReadOriginalDecisionAsync(decision.Review.Context,new(decision.ProposalReference(),decision.IntentId),f.Authority);Assert.Equal(JsonSerializer.Serialize(decision),JsonSerializer.Serialize(recovered));Assert.Equal(receipt,await f.Owner.DecideAsync(recovered,f.Authority,()=>Task.FromResult(source),true));
        var invalid=review with{PrerequisiteEvidenceVersion=99};Assert.Throws<InvalidOperationException>(()=>invalid.Require(review.Context.Company,review.Context.ActorUserId,decision.ProposalReference()));
    }
    [Theory][InlineData("missing")][InlineData("authority")][InlineData("source")][InlineData("borrowed")][InlineData("isolation")][InlineData("write")][InlineData("approve")][InlineData("rationale")]
    public async Task UnconfirmedDecisionRecoveryAndChangedAuthoritySourceOrInvalidInputCannotReject(string fault)
    {
        var (f,decision,source)=await DecisionFixture();var writes=f.Native.Writes;if(fault=="authority")f.RevokeAt=f.Guards+3;if(fault=="borrowed")f.Native.Borrowed=true;if(fault=="isolation")f.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="write")f.FailWrite=true;if(fault=="approve")decision=decision with{Outcome="Approve"};if(fault=="rationale")decision=decision with{Rationale=""};
        var error=await Record.ExceptionAsync(()=>f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(fault=="source"?source with{SnapshotRevision=new string('F',64)}:source),fault=="missing"));Assert.True(error is ArgumentException or InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected independent rejection");Assert.Equal("ISSUED_PENDING_APPROVAL",Assert.Single(f.Native.Rows.OfType<Permit>()).STATUS);Assert.Equal(writes+(fault=="write"?1:0),f.Native.Writes);
    }
    [Theory][InlineData("body")][InlineData("actor")][InlineData("audit")][InlineData("proposal")][InlineData("identity")]
    public async Task OriginalDecisionCannotAdoptChangedStoredBodiesActorsAuditsOrProposals(string fault)
    {
        var (f,decision,source)=await DecisionFixture();await f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source));var writes=f.Native.Writes;var row=Assert.Single(f.Native.Rows.OfType<Permit>());
        if(fault=="body")row.DECISION_DATA_JSON=row.DECISION_DATA_JSON!.Replace("Recorded independent rejection rationale","Changed rationale");if(fault=="actor")row.DECIDED_BY="foreign";if(fault=="audit")row.ROW_CHANGED_BY="foreign";if(fault=="proposal")row.RISK_REFERENCE="changed";if(fault=="identity")decision=decision with{IntentId=Guid.NewGuid()};
        await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source),true));Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task LostIndependentDecisionCommitCanOnlyBeConfirmedByOriginalReadOnlyRecovery()
    {
        var (f,decision,source)=await DecisionFixture();f.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(()=>f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source)));Assert.Equal("REJECTED",Assert.Single(f.Native.Rows.OfType<Permit>()).STATUS);var writes=f.Native.Writes;f.Native.FailCommit=false;var result=await f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source),true);result.Require(decision);Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("actor")][InlineData("company")][InlineData("task")][InlineData("reference")]
    public async Task PrivateDecisionReferenceRefusesForeignScopeBeforeMalformedBodyIsDecoded(string fault)
    {
        var (f,decision,source)=await DecisionFixture();await f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source));var writes=f.Native.Writes;var row=Assert.Single(f.Native.Rows.OfType<Permit>());row.DECISION_DATA_JSON="{";var context=decision.Review.Context;var query=new CompanyWorkOrderExecutionPermitDecisionReference(decision.ProposalReference(),decision.IntentId);
        if(fault=="actor")context=context with{ActorUserId="foreign"};if(fault=="company")row.BUSINESS_ASSOCIATE_ID="foreign";if(fault=="task")query=query with{Proposal=query.Proposal with{Task=query.Proposal.Task with{WorkOrderId="foreign"}}};if(fault=="reference")query=query with{IntentId=Guid.NewGuid()};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Owner.ReadOriginalDecisionAsync(context,query,f.Authority));Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task AuthorityRevocationAfterDecisionWriteRollsBackItsBodyStatusAndAudit()
    {
        var (f,decision,source)=await DecisionFixture();f.RevokeAt=f.Guards+4;await Assert.ThrowsAsync<UnauthorizedAccessException>(()=>f.Owner.DecideAsync(decision,f.Authority,()=>Task.FromResult(source)));var row=Assert.Single(f.Native.Rows.OfType<Permit>());Assert.Equal("ISSUED_PENDING_APPROVAL",row.STATUS);Assert.Null(row.DECISION_DATA_JSON);Assert.Null(row.DECIDED_BY);Assert.Null(row.ROW_CHANGED_DATE);f.Intent.RequireRecorded(row);
    }
    [Theory][InlineData("original")][InlineData("changed-facts")][InlineData("issuer")][InlineData("legacy")][InlineData("malformed-source")][InlineData("authority")][InlineData("missing-column")][InlineData("foreign-company")][InlineData("source-drift")][InlineData("future")][InlineData("expired")]
    public async Task IndependentPendingReviewRetainsProvenanceRefusesSelfApprovalAndDoesNotWrite(string fault)
    {
        var f=new Fixture();if(fault=="future")f.Intent=f.Intent with{ValidFromUtc=DateTime.UtcNow.AddDays(1),ValidToUtc=DateTime.UtcNow.AddDays(2)};if(fault=="expired")f.Intent=f.Intent with{ValidFromUtc=DateTime.UtcNow.AddDays(-2),ValidToUtc=DateTime.UtcNow.AddDays(-1)};await f.Issue();var writes=f.Native.Writes;var row=Assert.Single(f.Native.Rows.OfType<Permit>());var stored=f.Intent.RequireRecordedSource(row);Assert.Equal(f.Evidence.SnapshotRevision,stored.SnapshotRevision);
        var current=f.Intent.Context with{ActorUserId=fault=="issuer"?"issuer":"independent",Revision="independent-authority"};
        var stage=f.Evidence.Stage with{Context=f.Evidence.Stage.Context with{ActorUserId=current.ActorUserId,Revision="independent-stage-authority"}};var order=CompanyInventorySourceReview.Copy(f.Evidence.Order);if(fault=="changed-facts")order.Instructions="Changed work scope";
        var observed=new CompanyMaintenanceInspectionReview(stage,f.Evidence.InspectionDatabase,order,f.Evidence.Inspections,CompanyMaintenanceInspectionReview.Revision(stage,f.Evidence.InspectionDatabase,order,f.Evidence.Inspections),DateTimeOffset.UtcNow);
        if(fault=="original"){Assert.NotEqual(stored.SnapshotRevision,observed.SnapshotRevision);Assert.Equal(stored.FactsRevision(),observed.FactsRevision());}
        if(fault=="legacy")row.INSPECTION_DATA_JSON=null;if(fault=="malformed-source")row.INSPECTION_DATA_JSON="{";if(fault=="authority")f.RevokeAt=f.Guards+1;if(fault=="foreign-company")row.BUSINESS_ASSOCIATE_ID="foreign";
        if(fault=="missing-column")f.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure _,bool __)=>{var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(typeof(Permit));shape.Fields.RemoveAll(field=>field.FieldName=="INSPECTION_DATA_JSON");return shape;});
        var reads=0;Task<CompanyMaintenanceInspectionReview> Read(){PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);return Task.FromResult(++reads==2 && fault=="source-drift"?observed with{SnapshotRevision=new string('F',64)}:observed);}
        CompanyWorkOrderExecutionPermitApprovalReview? result=null;var error=await Record.ExceptionAsync(async()=>result=await f.Owner.ReviewApprovalAsync(current,new(f.Intent.Task,f.Intent.IntentId),f.Authority,Read));
        if(fault is "original" or "changed-facts" or "future" or "expired")
        {
            Assert.Null(error);result!.Require(current.Company,current.ActorUserId,new(f.Intent.Task,f.Intent.IntentId));Assert.Equal(fault=="changed-facts",result.Blockers.Any(value=>value.Contains("facts changed",StringComparison.Ordinal)));Assert.Contains(result.Blockers,value=>value.Contains("not yet recorded",StringComparison.Ordinal));
            if(fault=="future")Assert.Contains(result.Blockers,value=>value.Contains("not started",StringComparison.Ordinal));if(fault=="expired")Assert.Contains(result.Blockers,value=>value.Contains("expired",StringComparison.Ordinal));
        }
        else {Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected independent review");Assert.Null(result);}
        if(fault=="legacy"){f.Intent.RequireRecorded(row);await f.Recover();Assert.DoesNotContain("INSPECTION_DATA_JSON",JsonSerializer.Serialize(row));}
        Assert.Equal(writes,f.Native.Writes);Assert.Equal("ISSUED_PENDING_APPROVAL",row.STATUS);
    }
    [Theory][InlineData("original")][InlineData("actor")][InlineData("company")][InlineData("task")][InlineData("reference")][InlineData("malformed")][InlineData("body")][InlineData("authority")]
    public async Task PrivateReferenceReaderOnlyRestoresOriginalIssuerProposalWithoutWrites(string fault)
    {
        var f=new Fixture();await f.Issue();var writes=f.Native.Writes;var current=f.Intent.Context;var query=new CompanyWorkOrderExecutionPermitReference(f.Intent.Task,f.Intent.IntentId);var row=Assert.Single(f.Native.Rows.OfType<Permit>());
        if(fault=="actor")current=current with{ActorUserId="foreign"};if(fault=="company")row.BUSINESS_ASSOCIATE_ID="foreign";if(fault=="task")query=query with{Task=query.Task with{WorkOrderId="foreign-order"}};if(fault=="reference")query=query with{IntentId=Guid.NewGuid()};if(fault=="malformed")row.ISSUE_DATA_JSON="{";if(fault=="body")row.RISK_REFERENCE="changed";if(fault=="authority")f.RevokeAt=f.Guards+1;
        var error=await Record.ExceptionAsync(async()=>{var restored=await f.Owner.ReadOriginalAsync(current,query,f.Authority);Assert.Equal(f.Intent,restored);});
        if(fault=="original")Assert.Null(error);else Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected private disclosure");Assert.Equal(writes,f.Native.Writes);PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);
    }
    [Theory][InlineData("schema")][InlineData("column")][InlineData("source")][InlineData("authority")][InlineData("borrowed")][InlineData("isolation")][InlineData("write")]
    public async Task MissingSchemaChangedEvidenceOrUnsupportedTransactionCannotPublishPendingPermit(string fault)
    {
        var f=new Fixture();if(fault=="schema")f.MissingSchema=true;if(fault=="column")f.ChangedSchema=true;if(fault=="source")f.ChangedSource=true;if(fault=="authority")f.RevokeAt=3;if(fault=="borrowed")f.Native.Borrowed=true;if(fault=="isolation")f.Native.Isolation=IsolationLevel.ReadCommitted;if(fault=="write")f.FailWrite=true;
        var error=await Record.ExceptionAsync(f.Issue);Assert.True(error is InvalidOperationException or UnauthorizedAccessException,error?.ToString()??"Unexpected pending permit publication");Assert.Empty(f.Native.Rows);
    }
    [Theory][InlineData("body")][InlineData("actor")][InlineData("company")][InlineData("approval")][InlineData("audit")]
    public async Task OriginalRecoveryCannotAdoptChangedBodiesOwnersApprovalsOrAudit(string fault)
    {
        var f=new Fixture();await f.Issue();var writes=f.Native.Writes;var row=Assert.Single(f.Native.Rows.OfType<Permit>());
        if(fault=="body")row.ISOLATION_REFERENCE="changed";if(fault=="actor")row.ISSUED_BY="another";if(fault=="company")row.BUSINESS_ASSOCIATE_ID="foreign";if(fault=="approval")row.STATUS="APPROVED";if(fault=="audit")row.ROW_CHANGED_BY="another";
        await Assert.ThrowsAsync<InvalidOperationException>(f.Recover);Assert.Equal(writes,f.Native.Writes);
    }
    [Fact] public async Task LostCommitResponseCanOnlyConfirmTheSameRetainedProposalReadOnly()
    {
        var f=new Fixture();f.Native.FailCommit=true;await Assert.ThrowsAsync<InvalidOperationException>(f.Issue);Assert.Single(f.Native.Rows);var writes=f.Native.Writes;f.Native.FailCommit=false;var receipt=await f.Recover();receipt.Require(f.Intent);Assert.Equal(writes,f.Native.Writes);
    }
    [Theory][InlineData("type")][InlineData("numeric-type")][InlineData("jurisdiction")][InlineData("isolation")][InlineData("risk")][InlineData("personnel")][InlineData("materials")][InlineData("time-zone")][InlineData("window")][InlineData("identity")]
    public async Task UnreviewedPermitTypesReferencesOrValidityCannotReachNativeStorage(string fault)
    {
        var f=new Fixture();f.Intent=fault switch
        {
            "type"=>f.Intent with{PermitType="Unknown"},"numeric-type"=>f.Intent with{PermitType="0"},"jurisdiction"=>f.Intent with{Jurisdiction=""},"isolation"=>f.Intent with{IsolationReference=""},"risk"=>f.Intent with{RiskReference=""},"personnel"=>f.Intent with{PersonnelReference=""},"materials"=>f.Intent with{MaterialReference=""},"time-zone"=>f.Intent with{ValidFromUtc=DateTime.SpecifyKind(f.Intent.ValidFromUtc,DateTimeKind.Unspecified)},"window"=>f.Intent with{ValidToUtc=f.Intent.ValidFromUtc},_=>f.Intent with{IntentId=Guid.Empty}
        };
        await Assert.ThrowsAsync<ArgumentException>(f.Issue);Assert.Empty(f.Native.Rows);Assert.Equal(0,f.Native.Writes);Assert.Equal(0,f.Guards);
    }
}
#endif
