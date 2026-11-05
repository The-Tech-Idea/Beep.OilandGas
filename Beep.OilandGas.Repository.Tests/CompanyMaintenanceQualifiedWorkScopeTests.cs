using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Data;
using System.Reflection;
using System.Text.Json;
using Beep.OilandGas.ApiService.Tests;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
using Xunit;
namespace Beep.OilandGas.Repository.Tests;

public sealed partial class CompanyMaintenanceQualifiedLicensingTests
{
    [Theory][InlineData(false)][InlineData(true)]
    public async Task OriginalQualifiedWorkScopeUsesCompleteIdleAndOwnedIdentityWithoutReturnClearance(bool owned)
    {
        var ready=await ReadyQualifiedReturn(completeCommand:true);using var f=ready.Fixture;var writes=f.Native.Writes.Count;
        async Task Read()
        {
            var scope=await f.Owner.ReadMaintenanceQualifiedWorkAssetsAsync(f.Context,f.Order,f.Guard,owned);scope.Require(f.Order);
            Assert.Null(scope.Operating);Assert.Null(scope.Licensing);Assert.Null(scope.ReturnPolicy);
            CompanyMaintenanceQualifiedWorkScope.RequireSnapshot(scope,f.Order,f.Context,DateTime.UtcNow);
            CompanyMaintenanceQualifiedWorkScope.RequireRecorded(scope,f.Context.Company,new(CompanyMaintenanceProcessStart.InstanceIdFor(f.Context.Company,Guid.NewGuid()),f.Order.FieldId,f.Order.InstanceId),f.Context.ActorUserId,DateTime.UtcNow);
        }
        if(owned)await PPDMDataSourceTransaction.ExecuteAsync(f.Native.Source.Object,Read);else await Read();Assert.Equal(writes,f.Native.Writes.Count);
    }

    [Theory][InlineData("missing-column")][InlineData("borrowed")][InlineData("revoked")][InlineData("idle-as-owned")]
    public async Task OriginalWorkScopeCannotAcceptIncompleteProviderRowsOrBorrowAnotherTransaction(string fault)
    {
        var ready=await ReadyQualifiedReturn(completeCommand:true);using var f=ready.Fixture;var writes=f.Native.Writes.Count;
        if(fault=="missing-column")
            f.Native.Source.Setup(value=>value.GetEntityAsync("FACILITY_EQUIPMENT",It.IsAny<List<AppFilter>>())).ReturnsAsync(()=>f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Select(row=>
                row.GetType().GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite && property.Name!="ROW_QUALITY").ToDictionary(property=>property.Name,property=>property.GetValue(row)??DBNull.Value)).ToArray());
        if(fault=="borrowed")f.Native.LeaseActiveTransaction=Mock.Of<IDbTransaction>();if(fault=="revoked")f.Revoked=true;
        await Assert.ThrowsAnyAsync<Exception>(()=>f.Owner.ReadMaintenanceQualifiedWorkAssetsAsync(f.Context,f.Order,f.Guard,fault=="idle-as-owned"));
        Assert.Equal(writes,f.Native.Writes.Count);
    }

    [Theory][InlineData("current")][InlineData("changed-before")][InlineData("changed-after")][InlineData("missing-proof")][InlineData("missing-owner")][InlineData("revoked-after")][InlineData("commit-unknown")]
    public async Task NativeQualifiedWorkStartCapturesOneOriginalScopeAndRechecksItInsideTheExistingTransaction(string mode)
    {
        // Actual start and qualified native owners; prerequisite/person/document/stock collaborators are explicit controlled proof seams.
        var ready=await ReadyQualifiedReturn(completeCommand:true);using var f=ready.Fixture;
        var originalScope=await f.Owner.ReadMaintenanceQualifiedWorkAssetsAsync(f.Context,f.Order,f.Guard,false);
        var task=new CompanyMaintenanceReceivingQuery(CompanyMaintenanceProcessStart.InstanceIdFor(f.Context.Company,Guid.NewGuid()),f.Order.FieldId,f.Order.InstanceId);
        var prototype=WorkOrderExecutionStartFixture.Original(true,f.Context.Database,f.Order.FieldId,f.Context.ActorUserId,originalOrder:f.Order,taskOverride:task,qualifiedWorkAssets:mode=="missing-proof"?null:originalScope);
        var command=prototype.Command;var context=command.Review.Prerequisites.Requirements.Context;var permitType=prototype.Recorded.GetType();
        foreach(var (type,key) in new[]{(permitType,"PERMIT_ID"),(typeof(WORK_ORDER_EXECUTION_START),"START_ID")})
        {
            var table=type.Name;f.Native.Metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=key});
            f.Native.Metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync([key]);
        }
        f.Native.Source.Setup(value=>value.GetEntityStructure(It.IsAny<EntityStructure>(),true)).Returns((EntityStructure request,bool _)=>
        {
            var type=request.EntityName=="WORK_ORDER_EXECUTION_START"?typeof(WORK_ORDER_EXECUTION_START):request.EntityName==permitType.Name?permitType:request.EntityName=="WORK_ORDER_MAINTENANCE_RETURN_POLICY"?typeof(WORK_ORDER_MAINTENANCE_RETURN_POLICY):typeof(FACILITY).Assembly.GetType("Beep.OilandGas.PPDM39.Models."+request.EntityName)!;
            var shape=f.Native.Editor.Object.classCreator.ConvertToEntityStructure(type);var keys=f.Native.Metadata.Object.GetPrimaryKeyColumnsAsync(request.EntityName).GetAwaiter().GetResult();foreach(var field in shape.Fields)field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal);return shape;
        });
        f.Native.Rows.Add(prototype.Recorded);var writes=f.Native.Writes.Count;var beforePermit=JsonSerializer.Serialize(prototype.Recorded);var sourceCalls=0;var forbidProof=false;
        var installation=f.Native.Rows.OfType<FACILITY_EQUIPMENT>().Single();if(mode=="changed-before")installation.SOURCE="changed-install-source";
        f.Native.AfterLeaseWrite=row=>{if(row is WORK_ORDER_EXECUTION_START){if(mode=="changed-after")installation.SOURCE="changed-after-insert";if(mode=="revoked-after")f.Revoked=true;}};
        if(mode=="commit-unknown"){f.Native.LeaseCommitFailed=true;f.Native.LeaseCommitAfterFailure=true;}
        void Owned(){PPDMDataSourceTransaction.RequireOwned(f.Native.Source.Object,IsolationLevel.Serializable);if(forbidProof)throw new Exception("Original recovery cannot replay current start proof.");}
        var personnel=new Mock<ICompanyPersonnelAcceptanceCoreProof>();var documents=new Mock<ICompanyWorkOrderDocumentContributorCore>();var identities=new Mock<ICompanyRecordedReviewerIdentityCore>();var executor=new Mock<ICompanyWorkOrderExecutorIdentityOwner>();
        personnel.Setup(value=>value.RequireExecutionPersonnelCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyPersonnelPolicyEvidenceReview>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview _,CompanyPersonnelPolicyEvidenceReview __,Func<Task> guard)=>{Owned();await guard();});
        documents.Setup(value=>value.RequireExecutionDocumentCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyWorkOrderAcceptedDocumentReview>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview _,CompanyWorkOrderAcceptedDocumentReview __,Func<Task> guard)=>{Owned();await guard();});
        identities.Setup(value=>value.RequireExecutionReviewerIdentityCoreAsync(It.IsAny<FieldSelectionReview>(),It.IsAny<CompanyPersonnelIdentityCatalog>(),It.IsAny<CompanyNativePersonFacts>(),It.IsAny<string>(),It.IsAny<DateTime>(),It.IsAny<DateTime>(),It.IsAny<Func<Task>>())).Returns(async(FieldSelectionReview _,CompanyPersonnelIdentityCatalog __,CompanyNativePersonFacts ___,string ____,DateTime _____,DateTime ______,Func<Task> guard)=>{Owned();await guard();});
        executor.Setup(value=>value.RequireExecutorIdentityCoreAsync(It.IsAny<CompanyWorkOrderExecutorIdentity>(),It.IsAny<Func<Task>>())).Returns(async(CompanyWorkOrderExecutorIdentity _,Func<Task> guard)=>{Owned();await guard();});
        var owner=new WorkOrderExecutionPermitService(f.Native.Editor.Object,f.Native.Columns.Object,f.Native.Defaults.Object,f.Native.Metadata.Object);
        Task<(Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel,IReadOnlyList<Beep.OilandGas.Models.Data.WorkOrder.InspectionCondition>)> Read(bool owned)
        {if(forbidProof)throw new Exception("Recovery cannot refresh start prerequisites.");if(owned)Owned();return Task.FromResult((prototype.Approval.Review.CurrentSource.Order,prototype.Approval.Review.CurrentSource.Inspections));}
        Task Stock(CompanyWorkOrderExecutionPermitApprovalReview _){PPDMDataSourceTransaction.RequireIdle(f.Native.Source.Object);return Task.CompletedTask;}
        Task<bool> OwnedStock(CompanyWorkOrderExecutionPermitApprovalReview _){Owned();return Task.FromResult(true);}
        async Task<CompanyMaintenanceQualifiedAssets> Work(bool owned){Owned();Assert.True(owned);sourceCalls++;return await f.Owner.ReadMaintenanceQualifiedWorkAssetsAsync(context,f.Order,f.Guard,owned);}
        Task<CompanyWorkOrderExecutionStartReceipt> Start()=>owner.StartExecutionAsync(command,f.Guard,Read,personnel.Object,documents.Object,identities.Object,executor.Object,Stock,OwnedStock,()=>Task.FromResult(command.Review.Stage),mode=="missing-owner"?null:Work);
        var failure=await Record.ExceptionAsync(async()=>{var receipt=await Start();receipt.Require(command);});
        if(mode=="current")Assert.Null(failure);else Assert.NotNull(failure);
        if(mode is "current" or "commit-unknown")
        {
            f.Native.LeaseCommitFailed=false;Assert.Equal(2,sourceCalls);var stored=Assert.Single(f.Native.Rows.OfType<WORK_ORDER_EXECUTION_START>());command.RequireRecorded(stored);Assert.Equal(writes+1,f.Native.Writes.Count);
            forbidProof=true;var recovered=await owner.RecoverExecutionStartAsync(context,new(command.Review.Prerequisites.Requirements.Approval.Permit,command.IntentId),f.Guard);Assert.Equal(command.Review.QualifiedWorkAssets!.IdentityRevision(),recovered.Command.Review.QualifiedWorkAssets!.IdentityRevision());
            (await Start()).Require(command);Assert.Equal(writes+1,f.Native.Writes.Count);
            var facts=await owner.ReadRecordedStartAsync(context with{ActorUserId="authorized-viewer"},command.Review.Prerequisites.Requirements.Approval.Permit,f.Guard);Assert.NotNull(facts!.QualifiedWorkAssets);Assert.DoesNotContain(command.Reason,JsonSerializer.Serialize(facts));
            var before=command.Review.Prerequisites.Requirements;var utc=DateTime.UtcNow;
            var current=before with{Requirements=before.Requirements.Select(row=>row.ApplicableStage=="WORK_SUBMISSION"?row with{Status="PASS",ResultText="Actual completed qualified work",Inspector=context.ActorUserId,InspectDate=utc,SourceRevision=new string('F',64)}:row).ToArray(),ComparedAtUtc=DateTime.UtcNow};
            var inspection=CompanyWorkOrderSubmissionInspectionReview.Create(recovered,current,DateTime.UtcNow);
            var reportCommand=new CompanyWorkOrderSubmissionCommand(new(inspection,command.Review.Stage),Guid.NewGuid(),utc,"Actual qualified work performed",[],"Private work report reason");
            var report=CompanyWorkOrderSubmissionFacts.From(reportCommand.Prepare(DateTime.UtcNow));
            var foreignStart=inspection.Start with{QualifiedWorkAssets=inspection.Start.QualifiedWorkAssets! with{Context=inspection.Start.QualifiedWorkAssets!.Context with{Database=new("foreign-core","foreign-fingerprint")}}};
            var foreignComparison=inspection with{Start=foreignStart,Revision=""};foreignComparison=foreignComparison with{Revision=CompanyWorkOrderSubmissionInspectionReview.Digest(foreignComparison)};
            Assert.Throws<InvalidOperationException>(()=>foreignComparison.Require(context.Company,context.ActorUserId,new(command.Review.Prerequisites.Requirements.Approval.Permit,command.IntentId)));
            Assert.Equal(stored.STARTED_AT_UTC,report.WorkStartedAtUtc);Assert.Equal(command.Review.QualifiedWorkAssets!.IdentityRevision(),report.QualifiedWorkAssets!.IdentityRevision());
            Assert.DoesNotContain(reportCommand.Reason,JsonSerializer.Serialize(report));

        }
        else{Assert.Empty(f.Native.Rows.OfType<WORK_ORDER_EXECUTION_START>());if(mode is "changed-after" or "revoked-after")Assert.Contains("rollback",f.Native.LeaseTransactions);else Assert.Equal(writes,f.Native.Writes.Count);}
        Assert.Equal(beforePermit,JsonSerializer.Serialize(prototype.Recorded));
    }
    [Theory][InlineData("current")][InlineData("missing")][InlineData("new-observation")][InlineData("native-history")][InlineData("date-zone")][InlineData("different-reader-rights")]
    public async Task QualifiedReturnBindsRecordedPreWorkScopeWithoutBackfillingFromCurrentInstallation(string mode)
    {
        var ready=await QualifiedCommand();using var f=ready.Fixture;var report=ready.Command.Review.Assets.Report;
        var current=ready.Command.Review.Assets.QualifiedAssets! with{Operating=null,Licensing=null,ReturnPolicy=null};
        if(mode=="missing")report=report with{QualifiedWorkAssets=null,WorkStartedAtUtc=null};
        if(mode is "new-observation" or "native-history")current=current with{Equipment=current.Equipment.Select(row=>row with{Installation=row.Installation with{InstallationObservation=mode=="new-observation"?row.Installation.InstallationObservation+1:row.Installation.InstallationObservation,PopulationRevision=mode=="native-history"?new string('A',64):row.Installation.PopulationRevision}}).ToArray()};
        if(mode=="date-zone")current=current with{DateTimeZoneId="UTC"};
        if(mode=="different-reader-rights")current=current with{Facilities=current.Facilities.Select(row=>row with{AccessLevel="READ"}).ToArray(),Equipment=current.Equipment.Select(row=>row with{AccessLevel="READ"}).ToArray()};
        var findings=CompanyMaintenanceQualifiedWorkScope.GetReturnBlockers(current,report,f.Order);
        if(mode is "current" or "different-reader-rights")Assert.Empty(findings);else Assert.NotEmpty(findings);
        if(mode=="missing")Assert.Contains(CompanyMaintenanceQualifiedWorkScope.MissingOriginal,findings);
    }

    [Theory][InlineData("late-capture")][InlineData("wrong-executor")][InlineData("wrong-core")][InlineData("missing-start")][InlineData("start-after-completion")][InlineData("null-context")][InlineData("null-facility")][InlineData("null-equipment")]
    public async Task OriginalQualifiedReportCannotAdoptLateForeignOrMissingStartEvidence(string mode)
    {
        var ready=await QualifiedCommand();using var f=ready.Fixture;var report=ready.Command.Review.Assets.Report;var scope=report.QualifiedWorkAssets!;
        if(mode=="late-capture")scope=scope with{ComparedAtUtc=report.WorkStartedAtUtc!.Value.AddTicks(1)};
        if(mode=="wrong-executor")scope=scope with{Context=scope.Context with{ActorUserId="other-executor"}};
        if(mode=="wrong-core")scope=scope with{Context=scope.Context with{Database=new("other-core","other-fingerprint")}};
        if(mode=="null-context")scope=scope with{Context=null!};if(mode=="null-facility")scope=scope with{Facilities=[null!]};if(mode=="null-equipment")scope=scope with{Equipment=[null!]};
        report=report with{QualifiedWorkAssets=scope,WorkStartedAtUtc=mode=="missing-start"?null:mode=="start-after-completion"?report.CompletedAtUtc.AddTicks(1):report.WorkStartedAtUtc};
        Assert.ThrowsAny<Exception>(report.Require);
    }

}
