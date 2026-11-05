#if WORKFLOW_INBOX_FOCUSED
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Auth.Tests;
internal static class WorkOrderReturnAcceptanceFixture
{
    internal static CompanyWorkOrderReturnReview Create(string? wellId=null,bool assetReturn=false)
    {
        var source=WorkOrderMaintenanceReturnReviewTests.Review(assetReturn);var original=source.Completion;var task=original.Report.Query.Task;var handoff=DateTime.UtcNow;var utc=DateTime.UtcNow;
        var requirement=WorkOrderInspectionRules.ProjectWithRequirementProof(WorkOrderInspectionRules.PrepareRequirement(new(){FieldId=task.FieldId,CondSeq=1000,CondType="REGULATORY",ApplicableStage="RETURN_TO_SERVICE",RequiredText="Actual approved physical return observation",Jurisdiction="Recorded jurisdiction"},task.WorkOrderId,"requirement-maker","return-inspection",handoff.AddDays(-1))) with{Status="PASS",ResultText="Actual independent return observation",Inspector="return-approver",InspectDate=utc,SourceRevision=new string('D',64)};
        var order=CompanyInventorySourceReview.Copy(original.Order);if(wellId is not null && !assetReturn)order.Components=order.Components.Select(row=>row with{WellId=wellId,NativeRevision=new string('A',64)}).ToList();
        var rows=original.Requirements.Append(requirement).ToArray();var report=original.Report with{OriginalRequirementPopulation=CompanyWorkOrderExecutionRequirements.Population(task.FieldId,task.WorkOrderId,rows),OriginalOrderRevision=CompanyWorkOrderExecutionRequirements.OrderRevision(order)};
        var context=original.Context with{ActorUserId="return-approver",Revision="current-return-authority"};var person=new CompanyNativePersonFacts(context.Company,"return-person","Recorded return approver","PERSON",utc.AddDays(-1),utc.AddDays(1),new(context.Company.BusinessAssociateId,"return-person",1,"Return duty","ACTIVE",utc.AddDays(-1),utc.AddDays(1),new string('D',64)),new string('E',64));
        var link=new CompanyPersonnelIdentityVersion(context.Company,context.Database,CompanyPersonnelIdentityCommand.RecordIdFor(context.Company,context.ActorUserId,1),context.ActorUserId,person.PersonBaId,"PERSON",1,utc.AddDays(-1),utc.AddDays(1),"Original independent mapping","identity-administrator",handoff.AddMinutes(-1),new string('F',64));var catalog=CompanyPersonnelIdentityCatalog.Create(new(context.Company,context.Database,context.ActorUserId,new string('B',64)),original.Identities.Versions.Append(link).ToArray(),DateTime.UtcNow);var compared=DateTime.UtcNow;
        var completion=new CompanyWorkOrderCompletionReview(context,report,order,rows,catalog,person,compared,CompanyWorkOrderCompletionReview.GetBlockers(context,report,order,rows,catalog,person,compared));
        var verification=source.Decision with{Query=report.Query,ReportRevision=report.Revision,WorkSubjectRevision=completion.WorkSubjectRevision()};var stage=source.Stage with{Context=context,StepInstanceId="return-step-instance",StepId="return",StepName="Independent return approval",AssignedRole="FacilityRole"};var assignment=new CompanyMaintenanceReturnAssignment(stage,verification,"handoff-history",handoff,new string('C',64));
        var result=new CompanyWorkOrderReturnReview(assignment,completion,[person],CompanyWorkOrderReturnReview.GetBlockers(assignment,completion,[person]));result.RequireAcceptance();return result;
    }
}
#endif
