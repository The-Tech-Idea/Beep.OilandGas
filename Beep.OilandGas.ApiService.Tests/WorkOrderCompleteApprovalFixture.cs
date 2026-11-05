#if WORKFLOW_INBOX_FOCUSED || RECORDED_APPROVAL_HTTP_FOCUSED
extern alias actualData;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
using Row=actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_INSPECTION;
using Rules=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderInspectionRules;
using Detail=actualData::Beep.OilandGas.Models.Data.WorkOrder.WorkOrderDetailModel;
namespace Beep.OilandGas.ApiService.Tests;

/// <summary>Controlled canonical command composition; not a live/provider approval claim.</summary>
internal static class WorkOrderCompleteApprovalFixture
{
    internal static (CompanyWorkOrderExecutionPermitApprovalReview Review,actualData::Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_PERMIT Pending) Create(bool phaseAware=false,AssetDatabaseScope? coreDatabase=null,string fieldId="field",Action<IReadOnlyList<Row>>? captureInspectionRows=null,Detail? originalOrder=null,bool additionalMaterial=false,Action<WORK_ORDER_PERSONNEL_ASSIGNMENT>? captureAssignment=null,CompanyMaintenanceReceivingQuery? taskOverride=null,bool completionVerification=false,bool returnAcceptance=false)
    {
        var material=new WorkOrderMaterialAcceptanceFixture(originalOrder?.WoSubType??"CORRECTIVE",true,coreDatabase:coreDatabase,fieldId:fieldId,taskOverride:taskOverride);var utc=DateTime.UtcNow;var task=material.Permit.Task;
        Row Requirement(int sequence,string kind,string reference)=>Rules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=sequence,CondType="PRE_START",RequiredText="Original "+kind+" requirement",Jurisdiction=material.Permit.Jurisdiction,RegulatoryRef=reference,PermitPrerequisiteKind=kind,ApplicableStage=phaseAware?"PRE_START":null},task.WorkOrderId,"requirement-author",kind.ToLowerInvariant()+"-inspection",utc);
        var personnelRow=Requirement(2,"PERSONNEL",material.Permit.PersonnelReference);
        var isolationRow=Requirement(3,"ISOLATION",material.Permit.IsolationReference);
        var riskRow=Requirement(4,"RISK",material.Permit.RiskReference);
        var rows=new[]{material.Pending,personnelRow,isolationRow,riskRow};if(phaseAware){material.Pending.APPLICABLE_STAGE="PRE_START";rows=rows.Concat([Rules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=5,CondType="OPERATIONAL",RequiredText="Verify actual work before submission",Jurisdiction=material.Permit.Jurisdiction,ApplicableStage="WORK_SUBMISSION"},task.WorkOrderId,"requirement-author","future-operation",utc)]).ToArray();}
        if(additionalMaterial)
        {
            var general=Rules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=6,CondType="PRE_START",RequiredText="Original general material procedure",Jurisdiction=material.Permit.Jurisdiction,RegulatoryRef="general-material-procedure",PermitPrerequisiteKind="MATERIAL",ApplicableStage=phaseAware?"PRE_START":null},task.WorkOrderId,"requirement-author","general-material-inspection",utc);
            general=Rules.PrepareResult(general,new(){FieldId=task.FieldId,InspectionId=general.INSPECTION_ID,ExpectedSourceRevision=Rules.Revision(general),Result="PASS",Notes="Independent general material procedure inspection"},"general-inspector",DateTime.UtcNow);
            rows=rows.Concat([general]).ToArray();
        }
        if(completionVerification)rows=rows.Append(Rules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=999,CondType="CLOSE_OUT",RequiredText="Actual independent completion",Jurisdiction=material.Permit.Jurisdiction,ApplicableStage="COMPLETION_VERIFICATION"},task.WorkOrderId,"requirement-author","completion-check",utc)).ToArray();
        if(returnAcceptance)rows=rows.Append(Rules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=1000,CondType="REGULATORY",RequiredText="Actual independent physical return observation",Jurisdiction=material.Permit.Jurisdiction,ApplicableStage="RETURN_TO_SERVICE"},task.WorkOrderId,"requirement-author","return-check",utc)).ToArray();
        var projected=rows.Select(Rules.ProjectWithRequirementProof).ToArray();
        var order=CompanyInventorySourceReview.Copy(originalOrder??material.IssuedSource.Order);if(originalOrder is null)order.Components.AddRange([new(task.WorkOrderId,"isolation-component",task.FieldId,"",""){InformationItemId="ISOLATION-information",InformationItemSubtype="DOCUMENT"},new(task.WorkOrderId,"risk-component",task.FieldId,"",""){InformationItemId="RISK-information",InformationItemSubtype="DOCUMENT"}]);
        var original=material.IssuedSource with{Order=order,Inspections=projected,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(material.IssuedSource.Stage,material.IssuedSource.InspectionDatabase,order,projected)};
        var proposal=material.Permit with{InspectionRevision=original.SnapshotRevision,RequirementEvidenceVersion=phaseAware?2:1};var pending=proposal.Prepare(DateTime.UtcNow,original);
        var context=material.ReviewerStage.Context with{ActorUserId="another-reviewer",Revision="current-reviewer-authority"};var stage=material.ReviewerStage with{Context=context};
        CompanyWorkOrderExecutionPermitApprovalReview Permit()
        {
            var observations=rows.Select(Rules.ProjectWithRequirementProof).ToArray();var current=original with{Stage=stage,Inspections=observations,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(stage,original.InspectionDatabase,original.Order,observations)};
            var now=DateTime.UtcNow;return new(context,proposal,original,current,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(pending)),now,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,current,now,phaseAware?7:1)){PrerequisiteEvidenceVersion=phaseAware?7:1};
        }
        var permit=Permit();var stock=material.Command.Review.CurrentMaterial;
        var stockSource=stock.Source with{Context=stock.Source.Context with{ActorUserId=context.ActorUserId},WorkOrder=permit.CurrentSource,Inventory=CompanyInventorySourceReview.Create(context.Company,context.ActorUserId,material.Inventory,[])};
        var stockReview=CompanyWorkOrderMaterialAllocationReview.Create(stockSource,[material.Allocation],DateTime.UtcNow);
        var reservation=material.Command.Review.Reservation;var query=new CompanyWorkOrderMaterialReadinessQuery(new(task,proposal.IntentId),reservation.IntentId);utc=DateTime.UtcNow;
        var readiness=new CompanyWorkOrderMaterialReadinessReview(query,permit,reservation,stockReview,utc,CompanyWorkOrderMaterialReadinessReview.GetBlockers(permit,reservation,stockReview,utc));
        var materialCommand=new CompanyWorkOrderMaterialAcceptanceCommand(readiness,material.Pending.INSPECTION_ID,Guid.NewGuid(),"Independent original material proof");
        rows[0]=materialCommand.Prepare(rows[0],DateTime.UtcNow);var materialFacts=materialCommand.Facts(rows[0]);

        var template=new WorkOrderPersonnelAcceptanceFixture(originalReference:true,coreDatabase:coreDatabase,fieldId:fieldId,workOrderType:originalOrder?.WoSubType??"CORRECTIVE");var employee=template.Evidence.Credentials.Personnel.Employment;var license=template.Evidence.Credentials.Personnel.License;
        var personnelQuery=new CompanyWorkOrderPersonnelSourceQuery(new(task,proposal.IntentId),"employee",proposal.PersonnelReference);
        var maker=context with{ActorUserId="assignment-maker",Revision="assignment-authority"};var makerStage=stage with{Context=maker};var makerSource=original with{Stage=makerStage,SnapshotRevision=CompanyMaintenanceInspectionReview.Revision(makerStage,original.InspectionDatabase,original.Order,original.Inspections)};
        utc=DateTime.UtcNow;var makerPermit=new CompanyWorkOrderExecutionPermitApprovalReview(maker,proposal,original,makerSource,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(pending)),utc,CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,makerSource,utc,1)){PrerequisiteEvidenceVersion=1};
        var makerPerson=CompanyPersonnelSourceFacts.Create(maker,personnelQuery,employee,license,DateTime.UtcNow);
        var assignmentCommand=new CompanyWorkOrderPersonnelAssignmentCommand(new(makerPermit,makerPerson,CompanyWorkOrderPersonnelSourceReview.GetBlockers(proposal,makerPerson)),Guid.NewGuid(),"RecordedDuty",proposal.ValidFromUtc,proposal.ValidToUtc,"Original independent duty assignment");
        var assignmentRow=assignmentCommand.Prepare(DateTime.UtcNow);captureAssignment?.Invoke(CompanyInventorySourceReview.Copy(assignmentRow));var assignment=CompanyWorkOrderPersonnelAssignmentFacts.From(assignmentRow);
        CompanyPersonnelPolicyEvidenceReview Personnel()
        {
            var currentPermit=Permit();var person=CompanyPersonnelSourceFacts.Create(context,personnelQuery,employee,license,DateTime.UtcNow);
            var personReview=new CompanyWorkOrderPersonnelSourceReview(currentPermit,person,CompanyWorkOrderPersonnelSourceReview.GetBlockers(proposal,person));
            var roster=CompanyWorkOrderPersonnelAssignmentRoster.Create(context,personnelQuery.Permit,[assignment],DateTime.UtcNow,[]);
            var assigned=new CompanyWorkOrderAssignedPersonnelReview(new(personnelQuery.Permit,assignment.AssignmentId),roster,personReview,CompanyWorkOrderAssignedPersonnelReview.GetBlockers(assignment,personReview,[]));
            var credentials=CompanyPersonnelCredentialFacts.Create(person,template.Evidence.Credentials.Statuses,template.Evidence.Credentials.Conditions,DateTime.UtcNow);
            var policies=template.Evidence.Policies;
            return new(assigned,policies,credentials,CompanyPersonnelPolicyEvidenceReview.GetBlockers(assigned,policies,credentials,1)){EligibilityEvidenceVersion=1};
        }
        var personnelCommand=new CompanyWorkOrderPersonnelAcceptanceCommand(Personnel(),personnelRow.INSPECTION_ID,Guid.NewGuid(),"Independent original personnel proof"){RequirementReferenceVersion=1};
        rows[1]=personnelCommand.Prepare(rows[1],DateTime.UtcNow);var personnelFacts=personnelCommand.Facts(rows[1]);

        var sources=new List<(CompanyWorkOrderDocumentAcceptanceCommand Command,CompanyWorkOrderDocumentContributorReview Contributors)>();
        var identityUtc=DateTime.UtcNow;var admin=new CompanyPersonnelPolicyContext(context.Company,context.Database,"identity-administrator",new string('A',64));
        var nativePerson=new CompanyNativePersonFacts(context.Company,"reviewer-person","Recorded person","Recorded person type",identityUtc.AddDays(-1),identityUtc.AddDays(2),new(context.Company.BusinessAssociateId,"reviewer-person",1,null,null,identityUtc.AddDays(-1),identityUtc.AddDays(2),new string('B',64)),new string('C',64));
        var identityReview=CompanyPersonnelIdentityReview.Create(admin,new(context.ActorUserId,nativePerson.PersonBaId),new(context.Company,context.ActorUserId,"Recorded account",true,new string('D',64)),nativePerson,CompanyPersonnelIdentityCatalog.Create(admin,[],identityUtc),identityUtc);
        var identityCommand=new CompanyPersonnelIdentityCommand(identityReview,1,Guid.NewGuid(),proposal.ValidFromUtc.AddMinutes(-1),identityUtc.AddDays(1),"Recorded identity proof","Independent identity verification",true);
        var identities=CompanyPersonnelIdentityCatalog.Create(admin with{ActorUserId=context.ActorUserId},[CompanyPersonnelIdentityVersion.From(identityCommand.Prepare(identityUtc))],DateTime.UtcNow);
        var links=new[]{new CompanyWorkOrderDocumentLink("isolation-component","ISOLATION-information","DOCUMENT",identityUtc.AddDays(-1),identityUtc.AddDays(1),new string('A',64)),new CompanyWorkOrderDocumentLink("risk-component","RISK-information","DOCUMENT",identityUtc.AddDays(-1),identityUtc.AddDays(1),new string('B',64))};
        foreach(var index in new[]{2,3})
        {
            var kind=index==2?"ISOLATION":"RISK";var reference=index==2?proposal.IsolationReference:proposal.RiskReference;var now=DateTime.UtcNow;
            var documentQuery=new CompanyWorkOrderPermitDocumentQuery(new(task,proposal.IntentId),kind,kind.ToLowerInvariant()+"-component");
            var catalog=new CompanyWorkOrderDocumentCatalog(context,documentQuery.Permit,links,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(links)),now);
            var info=new CompanyPermitInformationItemFacts(kind+"-information","DOCUMENT",reference,"Recorded source information","2",null,null,now.AddDays(-1),now.AddDays(1),new string('C',64));
            var document=new CompanyPermitSourceDocumentFacts(reference,"Recorded source document","Reported procedure","2","Reported language","Reported publisher",now.AddDays(-1),now.AddDays(-1),now.AddDays(1),"document-preparer",new string('D',64));
            var file=new CompanyPermitFileFacts(kind+"-file",info.Id,"DOCUMENT","Reported format",3,3,"BYTE",Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(new byte[]{1,2,3})),now.AddDays(-1),now.AddDays(1),new string('E',64));
            var source=CompanyWorkOrderPermitDocumentFacts.Create(catalog,documentQuery,reference,info,document,file,DateTime.UtcNow);var currentPermit=Permit();
            var evidence=new CompanyWorkOrderPermitDocumentReview(currentPermit,source,CompanyWorkOrderPermitDocumentReview.GetBlockers(proposal,source));
            var contributor=new CompanyPermitDocumentContributorFacts(kind+"-creator","external-person","Reported author","Recorded person type","Recorded external person",now.AddDays(-1),now.AddDays(2),"Recorded person type",now.AddDays(-1),now.AddDays(2),new string('E',64),new string('F',64));
            var people=CompanyWorkOrderDocumentContributors.Create(source,[contributor],DateTime.UtcNow);var compared=DateTime.UtcNow;
            var contributorReview=new CompanyWorkOrderDocumentContributorReview(evidence,people,identities,nativePerson,compared,CompanyWorkOrderDocumentContributorReview.GetBlockers(evidence,people,identities,nativePerson,compared));
            var command=new CompanyWorkOrderDocumentAcceptanceCommand(evidence,rows[index].INSPECTION_ID,Guid.NewGuid(),"Independent original "+kind+" verification",true,true){ContributorReview=contributorReview};
            command.RequireCurrentAcceptance();rows[index]=command.Prepare(rows[index],DateTime.UtcNow);sources.Add((command,contributorReview));
        }
        permit=Permit();stockSource=stockSource with{WorkOrder=permit.CurrentSource};stockReview=CompanyWorkOrderMaterialAllocationReview.Create(stockSource,[material.Allocation],DateTime.UtcNow);utc=DateTime.UtcNow;
        var acceptedMaterial=new CompanyWorkOrderAcceptedMaterialReview(materialFacts,reservation,stockReview,utc,CompanyWorkOrderAcceptedMaterialReview.GetBlockers(proposal,materialFacts,reservation,stockReview,utc));
        var personEvidence=Personnel();utc=DateTime.UtcNow;var acceptedPersonnel=new CompanyWorkOrderAcceptedPersonnelReview(personnelFacts,personEvidence,utc,CompanyWorkOrderAcceptedPersonnelReview.GetBlockers(proposal,personnelFacts,personEvidence,utc));
        var acceptedDocuments=sources.Select((entry,index)=>
        {
            var facts=entry.Command.Facts(rows[index+2]);var evidence=entry.Command.Review with{PermitReview=permit};var proof=entry.Contributors;var compared=DateTime.UtcNow;
            return new CompanyWorkOrderAcceptedDocumentReview(facts,evidence,compared,CompanyWorkOrderAcceptedDocumentReview.GetBlockers(proposal,facts,evidence,compared,1,proof.Contributors,proof.Identities,proof.ReviewerPerson)){ContributorComparisonVersion=1,CurrentContributors=proof.Contributors,CurrentIdentities=proof.Identities,RecordedReviewerPerson=proof.ReviewerPerson};
        }).ToArray();
        utc=DateTime.UtcNow;var ready=permit with{ComparedAtUtc=utc,PrerequisiteEvidenceVersion=phaseAware?7:6,CurrentMaterialAcceptance=acceptedMaterial,CurrentPersonnelAcceptance=acceptedPersonnel,CurrentDocumentAcceptance=acceptedDocuments,Blockers=CompanyWorkOrderExecutionPermitApprovalReview.GetBlockers(proposal,original,permit.CurrentSource,utc,phaseAware?7:6,acceptedMaterial,acceptedPersonnel,acceptedDocuments)};
        ready.RequireCurrentPrerequisiteAcceptance(utc);captureInspectionRows?.Invoke(rows.Select(CompanyInventorySourceReview.Copy).ToArray());return(ready,pending);
    }
}
#endif
