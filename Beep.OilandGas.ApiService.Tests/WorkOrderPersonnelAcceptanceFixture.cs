#if WORKFLOW_INBOX_FOCUSED || RECORDED_APPROVAL_HTTP_FOCUSED
extern alias actualData;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
using Rules=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
namespace Beep.OilandGas.ApiService.Tests;
internal sealed class WorkOrderPersonnelAcceptanceFixture
{
    internal readonly Row Pending;
    internal readonly CompanyPersonnelPolicyEvidenceReview Evidence;
    internal readonly CompanyWorkOrderPersonnelAcceptanceCommand Command;
    internal readonly actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT StoredPermit;
    internal WorkOrderPersonnelAcceptanceFixture(bool historical=false,bool requirementProof=false,bool originalReference=false,AssetDatabaseScope? coreDatabase=null,string fieldId="field",string workOrderType="CORRECTIVE")
    {
        var original=WorkOrderPersonnelReviewFixture.Original(workOrderType,originalReference,coreDatabase,fieldId);var assigned=original.Prepare(DateTime.UtcNow);var facts=CompanyWorkOrderPersonnelAssignmentFacts.From(assigned);var utc=DateTime.UtcNow;
        var previous=original.Review.PermitReview;var context=previous.Context with{ActorUserId="another-reviewer",Revision="current-reviewer-authority"};var stage=previous.CurrentSource.Stage with{Context=context};
        Pending=Rules.PrepareRequirement(new(){FieldId=previous.Proposal.Task.FieldId,CondSeq=2,CondType="PRE_START",RequiredText="Accept exact personnel assignment",Jurisdiction=previous.Proposal.Jurisdiction,RegulatoryRef=originalReference?previous.Proposal.PersonnelReference:facts.AssignmentId,PermitPrerequisiteKind="PERSONNEL"},"order-الكويت","requirement-author","personnel-inspection",utc);
        var rows=previous.CurrentSource.Inspections.Concat([requirementProof?Rules.ProjectWithRequirementProof(Pending):Rules.Project(Pending)]).ToArray();var current=previous.CurrentSource with{Stage=stage,Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,previous.CurrentSource.InspectionDatabase,previous.CurrentSource.Order,rows)};
        var permit=previous with{Context=context,CurrentSource=current,ComparedAtUtc=utc,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(previous.Proposal,previous.OriginalSource,current,utc)};
        StoredPermit=previous.Proposal.Prepare(previous.Proposal.ValidFromUtc.AddMinutes(-1),previous.OriginalSource);
        // Match the original native permit audit used by the canonical comparison.
        permit=permit with{RecordedRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(StoredPermit))};
        var personnel=CompanyPersonnelSourceFacts.Create(context,original.Review.Personnel.Query,original.Review.Personnel.Employment,original.Review.Personnel.License,utc);
        var source=new CompanyWorkOrderPersonnelSourceReview(permit,personnel,CompanyWorkOrderPersonnelSourceReview.GetBlockers(permit.Proposal,personnel));
        var roster=CompanyWorkOrderPersonnelAssignmentRoster.Create(context,personnel.Query.Permit,[facts],utc,[]);var query=new CompanyWorkOrderAssignedPersonnelQuery(personnel.Query.Permit,facts.AssignmentId);
        var assignment=new CompanyWorkOrderAssignedPersonnelReview(query,roster,source,CompanyWorkOrderAssignedPersonnelReview.GetBlockers(facts,source,[]));
        var p=new CompanyPersonnelPolicyDraft(facts.DutyCode,workOrderType,permit.Proposal.Jurisdiction,personnel.License!.Type!,personnel.License.GrantingAuthority!,"CERTIFICATION",["VALID"],[new("TRAINING_HOURS","TRAINING","MINIMUM",8m,"HOUR","policy-source")],utc.AddDays(-1),utc.AddDays(2),"Original policy document"){AcceptedEmploymentStatuses=historical?null:["ACTIVE"],AcceptedEmploymentPositions=historical?null:[]};
        var admin=new CompanyPersonnelPolicyContext(context.Company,context.Database,"administrator",new string('B',64));var baseline=CompanyPersonnelPolicyCatalog.Create(admin,[],utc);var policyCommand=new CompanyPersonnelPolicyCommand(admin,baseline.Revision,p,1,Guid.NewGuid(),"Private policy maker rationale");
        var policies=CompanyPersonnelPolicyCatalog.Create(admin with{ActorUserId=context.ActorUserId},[CompanyPersonnelPolicyVersion.From(policyCommand.Prepare(utc))],utc);
        var status=new CompanyPersonnelLicenseStatusFacts("employee",personnel.Query.LicenseId,"status","VALID","CERTIFICATION",utc.AddDays(-1),utc.AddDays(2),new string('C',64));
        var condition=new CompanyPersonnelLicenseConditionFacts("employee",personnel.Query.LicenseId,"condition","TRAINING_HOURS","TRAINING",8m,"HOUR","N",utc.AddDays(-1),utc.AddDays(2),"Recorded employee training reference",new string('D',64));
        var credentials=CompanyPersonnelCredentialFacts.Create(personnel,[status],[condition],utc);
        Evidence=new(assignment,policies,credentials,CompanyPersonnelPolicyEvidenceReview.GetBlockers(assignment,policies,credentials,historical?0:1)){EligibilityEvidenceVersion=historical?0:1};Command=new(Evidence,Pending.INSPECTION_ID,Guid.NewGuid(),"Private original independent personnel acceptance"){RequirementReferenceVersion=originalReference?1:0};Command.Require();
    }
    internal CompanyPersonnelPolicyEvidenceReview Current(Row? own=null)
    {
        var permit=Evidence.Assignment.CurrentPersonnel.PermitReview;var source=permit.CurrentSource;var rows=source.Inspections.Select(row=>row.InspectionId==Pending.INSPECTION_ID?Rules.ProjectForRequirement(own??Pending,Command.OriginalInspection):row).ToArray();
        source=source with{Inspections=rows,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(source.Stage,source.InspectionDatabase,source.Order,rows)};var utc=DateTime.UtcNow;
        permit=permit with{CurrentSource=source,ComparedAtUtc=utc,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(permit.Proposal,permit.OriginalSource,source,utc)};
        var personnel=Evidence.Assignment.CurrentPersonnel with{PermitReview=permit};var assignment=Evidence.Assignment with{CurrentPersonnel=personnel,Blockers=CompanyWorkOrderAssignedPersonnelReview.GetBlockers(Evidence.Assignment.Assignment,personnel,[])};
        var credentials=CompanyPersonnelCredentialFacts.Create(personnel.Personnel,Evidence.Credentials.Statuses,Evidence.Credentials.Conditions,utc);
        return Evidence with{Assignment=assignment,Credentials=credentials,Blockers=CompanyPersonnelPolicyEvidenceReview.GetBlockers(assignment,Evidence.Policies,credentials,Evidence.EligibilityEvidenceVersion)};
    }
}
#endif
