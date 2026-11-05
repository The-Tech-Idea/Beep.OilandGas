using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.WorkOrder;
#if WORKFLOW_INBOX_FOCUSED || RECORDED_APPROVAL_HTTP_FOCUSED
namespace Beep.OilandGas.ApiService.Tests;
internal static class WorkOrderPersonnelReviewFixture
{
    internal static CompanyWorkOrderPersonnelAssignmentCommand Original(string workOrderType="",bool requirementProof=false,AssetDatabaseScope? coreDatabase=null,string fieldId="field")
    {
        var f=new WorkOrderMaterialAcceptanceFixture(workOrderType,requirementProof,coreDatabase:coreDatabase,fieldId:fieldId);var permit=f.Command.Review.PermitReview;var query=new CompanyWorkOrderPersonnelSourceQuery(f.Command.Review.Query.Permit,"employee",permit.Proposal.PersonnelReference);var utc=DateTime.UtcNow;
        var employee=new CompanyEmployeeSourceFacts("company","employee",1,"Recorded operator","ACTIVE",utc.AddDays(-2),utc.AddDays(2),new string('A',64));
        var license=new CompanyPersonnelLicenseFacts("employee",query.LicenseId,"Recorded licence","original-number","recorded-authority",utc.AddDays(-1),utc.AddDays(-1),utc.AddDays(2),new string('B',64));
        var source=CompanyPersonnelSourceFacts.Create(permit.Context,query,employee,license,utc);var review=new CompanyWorkOrderPersonnelSourceReview(permit,source,CompanyWorkOrderPersonnelSourceReview.GetBlockers(permit.Proposal,source));
        return new(review,Guid.NewGuid(),"RecordedDuty",permit.Proposal.ValidFromUtc,permit.Proposal.ValidToUtc,"Private original personnel rationale");
    }
}
#endif
