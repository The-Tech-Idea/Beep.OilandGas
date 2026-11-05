#if WORKFLOW_INBOX_FOCUSED
using System.Text.Json;
using Beep.OilandGas.PPDM39.Core;
using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.PPDM39.DataManagement.Core;
using Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Tools;
using TheTechIdea.Data.OilGas;
using Xunit;
namespace Beep.OilandGas.ApiService.Tests;

public sealed class WorkOrderExecutionPersonnelTests
{
    [Theory]
    [InlineData("current")][InlineData("company")][InlineData("database")][InlineData("field")]
    [InlineData("actor-binding")][InlineData("authority")][InlineData("idle")]
    [InlineData("assignment")][InlineData("policy")][InlineData("credential")][InlineData("membership")][InlineData("legacy")]
    public async Task ExecutorClosesOriginalPersonnelUnderOwnIdentityWithoutChangingReviewerHistory(string mode)
    {
        var native=new PersonaTaskRoutingTests.Fixture();native.Rows.Clear();
        var assignmentCommand=WorkOrderPersonnelReviewFixture.Original("CORRECTIVE",true);
        var actor=assignmentCommand.Review.Personnel.Context;var utc=DateTime.UtcNow;
        var query=assignmentCommand.Review.Personnel.Query;
        var employee=new BA_EMPLOYEE{EMPLOYER_BA_ID=actor.Company.BusinessAssociateId,EMPLOYEE_BA_ID=query.EmployeeBaId,EMPLOYEE_OBS_NO=1,ACTIVE_IND="Y",EMPLOYEE_POSITION="Recorded operator",STATUS="ACTIVE",EFFECTIVE_DATE=utc.AddDays(-2),EXPIRY_DATE=utc.AddDays(2)};
        var license=new BA_LICENSE{LICENSEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,ACTIVE_IND="Y",LICENSE_TYPE="Recorded licence",LICENSE_NUM="original-number",GRANTED_BY_BA_ID="recorded-authority",GRANTED_DATE=utc.AddDays(-1),EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        var status=new BA_LICENSE_STATUS{LICENSEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,STATUS_ID="status",ACTIVE_IND="Y",LICENSE_STATUS="VALID",LICENSE_STATUS_TYPE="CERTIFICATION",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        var condition=new BA_LICENSE_COND{LICENSEE_BA_ID=query.EmployeeBaId,LICENSE_ID=query.LicenseId,CONDITION_ID="condition",ACTIVE_IND="Y",CONDITION_CODE="TRAINING_HOURS",CONDITION_TYPE="TRAINING",CONDITION_VALUE=8m,CONDITION_VALUE_UOM="HOUR",EXEMPT_IND="N",PPDM_GUID="Recorded employee training reference",EFFECTIVE_DATE=utc.AddDays(-1),EXPIRY_DATE=utc.AddDays(2)};
        native.Rows.AddRange([employee,license,status,condition]);
        var metadata=new Mock<IPPDMMetadataRepository>();var creator=new ClassCreator(native.Editor.Object);
        foreach(var (type,keys) in new[]{(typeof(BA_EMPLOYEE),new[]{"EMPLOYER_BA_ID","EMPLOYEE_BA_ID","EMPLOYEE_OBS_NO"}),(typeof(BA_LICENSE),new[]{"LICENSEE_BA_ID","LICENSE_ID"}),(typeof(BA_LICENSE_STATUS),new[]{"LICENSEE_BA_ID","LICENSE_ID","STATUS_ID"}),(typeof(BA_LICENSE_COND),new[]{"LICENSEE_BA_ID","LICENSE_ID","CONDITION_ID"}),(typeof(WORK_ORDER_PERSONNEL_ASSIGNMENT),new[]{"ASSIGNMENT_ID"}),(typeof(WORK_ORDER_PERSONNEL_RELEASE),new[]{"RELEASE_ID"}),(typeof(WORK_ORDER_PERSONNEL_POLICY),new[]{"POLICY_RECORD_ID"})})
        {
            var table=type.Name;
            metadata.Setup(value=>value.GetTableMetadataAsync(table)).ReturnsAsync(new PPDMTableMetadata{TableName=table,EntityTypeName=type.FullName!,PrimaryKeyColumn=string.Join(',',keys)});
            metadata.Setup(value=>value.GetPrimaryKeyColumnsAsync(table)).ReturnsAsync(keys);
            native.Source.Setup(value=>value.CheckEntityExist(table)).Returns(true);
            native.Source.Setup(value=>value.GetEntityStructure(It.Is<EntityStructure>(shape=>shape.EntityName==table),true)).Returns(()=>
            {
                var shape=creator.ConvertToEntityStructure(type);foreach(var field in shape.Fields){field.IsKey=keys.Contains(field.FieldName,StringComparer.Ordinal);if(field.IsKey)field.AllowDBNull=false;}return shape;
            });
        }
        ContractorManagementService CreateOwner()=>new(native.Editor.Object,Mock.Of<ICommonColumnHandler>(),Mock.Of<IPPDM39DefaultsRepository>(),metadata.Object,()=>Task.FromResult((actor.Company,actor.ActorUserId,actor.Database)),NullLogger<ContractorManagementService>.Instance);
        var owner=CreateOwner();
        var personnel=await owner.ReadPersonnelSourceAsync(actor,query,()=>Task.CompletedTask);
        assignmentCommand=assignmentCommand with{Review=new(assignmentCommand.Review.PermitReview,personnel,CompanyWorkOrderPersonnelSourceReview.GetBlockers(assignmentCommand.Review.PermitReview.Proposal,personnel))};
        var stored=assignmentCommand.Prepare(DateTime.UtcNow);native.Rows.Add(stored);
        var previous=assignmentCommand.Review.PermitReview;actor=actor with{ActorUserId="personnel-reviewer",Revision="recorded-review-authority"};
        owner=CreateOwner(); // Each authenticated request owns a separate binding latch.
        var stage=previous.CurrentSource.Stage with{Context=actor};var source=previous.CurrentSource with{Stage=stage};
        source=source with{SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,source.InspectionDatabase,source.Order,source.Inspections)};
        var compared=DateTime.UtcNow;var permit=previous with{Context=actor,CurrentSource=source,ComparedAtUtc=compared,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(previous.Proposal,previous.OriginalSource,source,compared)};
        var credentials=await owner.ReadPersonnelCredentialsAsync(actor,query,()=>Task.CompletedTask);
        var currentPersonnel=new CompanyWorkOrderPersonnelSourceReview(permit,credentials.Personnel,CompanyWorkOrderPersonnelSourceReview.GetBlockers(permit.Proposal,credentials.Personnel));
        var fact=CompanyWorkOrderPersonnelAssignmentFacts.From(stored);var roster=CompanyWorkOrderPersonnelAssignmentRoster.Create(actor,query.Permit,[fact],DateTime.UtcNow,[]);
        var assigned=new CompanyWorkOrderAssignedPersonnelReview(new(query.Permit,fact.AssignmentId),roster,currentPersonnel,CompanyWorkOrderAssignedPersonnelReview.GetBlockers(fact,currentPersonnel,[]));
        var policy=new WorkOrderPersonnelAcceptanceFixture().Evidence.Policies.Versions.Single().Policy;
        var policyContext=new CompanyPersonnelPolicyContext(actor.Company,actor.Database,"policy-administrator",new string('A',64));
        var policyCommand=new CompanyPersonnelPolicyCommand(policyContext,CompanyPersonnelPolicyCatalog.Create(policyContext,[],DateTime.UtcNow).Revision,policy,1,Guid.NewGuid(),"Private policy rationale");
        var policyRow=policyCommand.Prepare(DateTime.UtcNow);native.Rows.Add(policyRow);
        var policies=await owner.ReadPersonnelPoliciesAsync(policyContext with{ActorUserId=actor.ActorUserId},()=>Task.CompletedTask);
        var original=new CompanyPersonnelPolicyEvidenceReview(assigned,policies,credentials,CompanyPersonnelPolicyEvidenceReview.GetBlockers(assigned,policies,credentials,1)){EligibilityEvidenceVersion=1};
        original.Require(actor.Company,actor.ActorUserId,assigned.Query);
        Assert.Equal(CompanyPersonnelPolicyEvidenceReview.PendingAcceptanceBlocker,Assert.Single(original.Blockers));
        var retained=JsonSerializer.Serialize(original);actor=actor with{ActorUserId="executor",Revision="actual-executor-authority"};var executor=actor;
        if(mode=="company")executor=executor with{Company=new("tenant","foreign")};
        if(mode=="database")executor=executor with{Database=new("foreign","binding")};if(mode=="field")executor=executor with{FieldId="foreign"};
        if(mode=="actor-binding")actor=actor with{ActorUserId="foreign-actor"};
        owner=CreateOwner();
        if(mode=="assignment")stored.DUTY_CODE="Changed duty";if(mode=="policy")policyRow.POLICY_JSON="{}";
        if(mode=="credential")status.LICENSE_STATUS="REVOKED";if(mode=="membership")employee.STATUS="INACTIVE";
        if(mode=="legacy")original=original with{EligibilityEvidenceVersion=0,Blockers=CompanyPersonnelPolicyEvidenceReview.GetBlockers(assigned,policies,credentials,0)};
        var captured=JsonSerializer.Serialize(original);
        Task Guard()=>mode=="authority"?Task.FromException(new UnauthorizedAccessException("Current execution capability revoked")):Task.CompletedTask;
        Task Proof()=>owner.RequireExecutionPersonnelCoreAsync(executor,original,Guard);
        var error=await Record.ExceptionAsync(()=>mode=="idle"?Proof():PPDMDataSourceTransaction.ExecuteAsync(native.Source.Object,Proof));
        if(mode=="current")Assert.Null(error);else Assert.NotNull(error);
        Assert.Equal(captured,JsonSerializer.Serialize(original));if(mode!="legacy")Assert.Equal(retained,captured);
        Assert.Equal("personnel-reviewer",original.Credentials.Personnel.Context.ActorUserId);
        Assert.Equal(0,native.Writes);PPDMDataSourceTransaction.RequireIdle(native.Source.Object);
    }
}
#endif
