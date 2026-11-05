using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyWorkOrderCompletionDecisionStatus> ReadCompletionDecisionStatusAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        query.Require();var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var core=await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.VerifyCompletion");
        if(boundary.Process is not ICompanyWorkOrderCompletionDecisionReads owner || executionPermits is null)throw new InvalidOperationException("Existing native completion decision and original report read owners are required.");
        async Task Guard(){if(await Authority(user,boundary.Preference,query.Task.WorkOrderId)!=boundary.Context || await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.VerifyCompletion")!=core)throw new UnauthorizedAccessException("Original company/account/FIELD/order/role/CORE/lifecycle decision read authority changed.");}
        await Guard();var report=await executionPermits.ReadSubmissionFactsAsync(core,query,Guard);var first=await owner.ReadCompletionDecisionFactsAsync(boundary.Context,query,Guard);var closing=await owner.ReadCompletionDecisionFactsAsync(boundary.Context,query,Guard);var finalReport=await executionPermits.ReadSubmissionFactsAsync(core,query,Guard);
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(report)!=JsonSerializer.Serialize(finalReport) || closing is {} facts && (facts.CoreDatabase!=core.Database || facts.ReportRevision!=report.Revision))throw new InvalidOperationException("Original recorded decision/report/native bindings changed around public status observation.");
        var result=new CompanyWorkOrderCompletionDecisionStatus(core,query,closing);result.Require();await Guard();return result;
    }
}
