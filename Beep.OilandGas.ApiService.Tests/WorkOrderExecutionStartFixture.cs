#if WORKFLOW_INBOX_FOCUSED || RECORDED_APPROVAL_HTTP_FOCUSED
extern alias actualData;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Permit=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT;
namespace Beep.OilandGas.ApiService.Tests;
internal static class WorkOrderExecutionStartFixture
{
    // Controlled client/form projection. The protected API must independently prove the original start's reservation binding.
    internal static CompanyWorkOrderMaterialUsageCommand Usage()
    {
        var original=Original(true);var startCommand=original.Command;var started=startCommand.Prepare(DateTime.UtcNow);
        var context=startCommand.Review.Prerequisites.Requirements.Context;
        var before=original.Approval.Review.CurrentMaterialAcceptance!.Material.Source;
        var stage=before.WorkOrder.Stage with{Context=before.WorkOrder.Stage.Context with{ActorUserId=context.ActorUserId}};
        var order=before.WorkOrder with{Stage=stage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,before.WorkOrder.InspectionDatabase,before.WorkOrder.Order,before.WorkOrder.Inspections)};
        var inventory=CompanyInventorySourceReview.Create(context.Company,context.ActorUserId,before.Inventory.Item,before.Inventory.Transactions);
        var source=before with{Context=before.Context with{ActorUserId=context.ActorUserId},WorkOrder=order,Inventory=inventory,
            Blockers=CompanyWorkOrderMaterialSourceReview.GetBlockers(before.Query,inventory)};
        var basis=CompanyWorkOrderMaterialAllocationReview.Create(source,[],DateTime.UtcNow);
        var allocation=new CompanyWorkOrderMaterialAllocationCommand(basis,Guid.NewGuid(),basis.ComparedAtUtc,startCommand.Review.Prerequisites.Requirements.Approval.ValidToUtc,"Controlled original reservation");
        var reserved=allocation.Prepare(DateTime.UtcNow);var material=CompanyWorkOrderMaterialAllocationReview.Create(source,[reserved],DateTime.UtcNow);
        var comparison=CompanyWorkOrderSubmissionInspectionReview.Create(new(startCommand,started),startCommand.Review.Prerequisites.Requirements,DateTime.UtcNow);
        var review=new CompanyWorkOrderMaterialUsageReview(comparison.Start,CompanyWorkOrderMaterialReservationFacts.From(reserved),material,allocation.ValidToUtc,DateTime.UtcNow);
        var result=new CompanyWorkOrderMaterialUsageCommand(review,Guid.NewGuid(),1m,reserved.QUANTITY_UNIT,DateTime.UtcNow,"Actual physical use");result.Require();return result;
    }
    internal static (CompanyWorkOrderSubmissionCommand Command,CompanyWorkOrderExecutionStartCommand Start,WORK_ORDER_EXECUTION_START Started,Permit Approved) Submission(AssetDatabaseScope? database=null,string fieldId="field",string actor="executor",string role="ConfiguredRole",actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel? originalOrder=null,Action<IReadOnlyList<actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>>? captureInspectionRows=null,CompanyMaintenanceReceivingQuery? taskOverride=null,bool completionVerification=false,bool returnAcceptance=false,CompanyMaintenanceQualifiedAssets? qualifiedWorkAssets=null)
    {
        var original=Original(true,database,fieldId,actor,role,originalOrder:originalOrder,captureInspectionRows:captureInspectionRows,taskOverride:taskOverride,completionVerification:completionVerification,returnAcceptance:returnAcceptance,qualifiedWorkAssets:qualifiedWorkAssets);var start=original.Command;var started=start.Prepare(DateTime.UtcNow);var before=start.Review.Prerequisites.Requirements;
        var utc=DateTime.UtcNow;var rows=before.Requirements.Select(row=>row.ApplicableStage=="WORK_SUBMISSION"?row with{Status="PASS",ResultText="Actual controlled operational inspection",Inspector=before.Context.ActorUserId,InspectDate=utc,SourceRevision=new string('F',64)}:row).ToArray();
        var current=before with{Requirements=rows,ComparedAtUtc=DateTime.UtcNow};var inspection=CompanyWorkOrderSubmissionInspectionReview.Create(new(start,started),current,DateTime.UtcNow);
        var command=new CompanyWorkOrderSubmissionCommand(new(inspection,start.Review.Stage),Guid.NewGuid(),started.ROW_CREATED_DATE!.Value,"Explicit actual work performed",[],"Own report awaiting independent verification");command.Require();return(command,start,started,original.Recorded);
    }
    internal static (CompanyWorkOrderExecutionPermitDecision Approval,Permit Recorded,CompanyWorkOrderExecutionStartCommand Command) Original(bool phaseAware=false,AssetDatabaseScope? database=null,string fieldId="field",string actor="executor",string role="ConfiguredRole",Action<WORK_ORDER_PERSONNEL_ASSIGNMENT>? captureAssignment=null,actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel? originalOrder=null,Action<IReadOnlyList<actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION>>? captureInspectionRows=null,CompanyMaintenanceReceivingQuery? taskOverride=null,bool completionVerification=false,bool returnAcceptance=false,CompanyMaintenanceQualifiedAssets? qualifiedWorkAssets=null)
    {
        var complete=WorkOrderCompleteApprovalFixture.Create(phaseAware,coreDatabase:database,fieldId:fieldId,captureAssignment:captureAssignment,originalOrder:originalOrder,captureInspectionRows:captureInspectionRows,taskOverride:taskOverride,completionVerification:completionVerification,returnAcceptance:returnAcceptance);var review=complete.Review;var proposal=review.Proposal;
        var decision=new CompanyWorkOrderExecutionPermitDecision(review,Guid.NewGuid(),"Approve","Private original HSE rationale");var recorded=decision.Prepare(complete.Pending,DateTime.UtcNow);
        var task=proposal.Task;var context=review.Context with{ActorUserId=actor,Revision="actual-execution-authority"};var query=decision.ProposalReference();
        var approval=new CompanyWorkOrderRecordedApproval(context.Company,query,proposal.PermitId(),decision.IntentId,proposal.Context.ActorUserId,review.Context.ActorUserId,recorded.DECIDED_AT_UTC!.Value,proposal.ValidFromUtc,proposal.ValidToUtc,proposal.PermitType,proposal.Jurisdiction,proposal.ScopeDescription,proposal.RequirementEvidenceVersion,review.PrerequisiteEvidenceVersion,review.OriginalSource.RequirementFactsRevision(),ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(recorded)));
        var requirements=new CompanyWorkOrderExecutionRequirements(context,approval,review.CurrentSource.Order,review.CurrentSource.Inspections,CompanyWorkOrderExecutionRequirements.Population(task.FieldId,task.WorkOrderId,review.OriginalSource.Inspections),CompanyWorkOrderExecutionRequirements.OrderRevision(review.OriginalSource.Order),DateTime.UtcNow,[]);
        var prerequisites=new CompanyWorkOrderExecutionPrerequisites(requirements,review.CurrentMaterialAcceptance!.Reservation,review.CurrentPersonnelAcceptance!.Evidence.Assignment.Assignment,review.CurrentDocumentAcceptance!.Select(row=>row.Acceptance).ToArray(),review.PrerequisiteProofRevision(),true,DateTime.UtcNow,"");
        prerequisites=prerequisites with{Revision=CompanyWorkOrderExecutionPrerequisites.Digest(prerequisites)};
        var employment=review.CurrentPersonnelAcceptance.Evidence.Credentials.Personnel.Employment!;
        var person=new CompanyNativePersonFacts(context.Company,prerequisites.Personnel.EmployeeBaId,"Recorded assigned employee","PERSON",proposal.ValidFromUtc.AddDays(-1),proposal.ValidToUtc.AddDays(1),employment,new string('A',64));
        var link=new CompanyPersonnelIdentityVersion(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),context.ActorUserId,person.PersonBaId,"PERSON",1,proposal.ValidFromUtc.AddDays(-1),proposal.ValidToUtc.AddDays(1),"Independent actual person mapping","identity-administrator",DateTime.UtcNow,new string('B',64));
        var catalog=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(context))),[link],DateTime.UtcNow);
        var stage=review.CurrentSource.Stage with{Context=review.CurrentSource.Stage.Context with{ActorUserId=context.ActorUserId},AssignedRole=role};
        var identity=new CompanyWorkOrderExecutorIdentity(prerequisites,stage,catalog.Revision,link,person,DateTime.UtcNow,"");identity=identity with{Revision=CompanyWorkOrderExecutorIdentity.Digest(identity)};
        if(qualifiedWorkAssets is not null)
        {
            var assets=qualifiedWorkAssets with{Context=context,Operating=null,Licensing=null,ReturnPolicy=null};
            identity=identity with{QualifiedWorkAssets=assets,ComparedAtUtc=DateTime.UtcNow,Revision=""};identity=identity with{Revision=CompanyWorkOrderExecutorIdentity.Digest(identity)};
        }
        var command=new CompanyWorkOrderExecutionStartCommand(identity,Guid.NewGuid(),DateTime.UtcNow,"Actual work start after reviewing original scope and current prerequisites");command.Require();return(decision,recorded,command);
    }
}
#endif
