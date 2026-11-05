using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Security.Claims;
using System.Text.Json;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyWorkOrderCompletionQuery> DiscoverCompletionReportAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task)
    {
        task.Require();var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.VerifyCompletion");
        if(executionPermits is null)throw new InvalidOperationException("Existing native report owner is unavailable.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.VerifyCompletion")!=context)throw new UnauthorizedAccessException("Original report discovery authority changed.");}
        var result=await executionPermits.DiscoverSubmissionQueryAsync(context,task,Guard);result.Require();await Guard();return result;
    }
    public Task<CompanyWorkOrderCompletionReview> ReviewCompletionAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)=>ReadCompletionEvidenceAsync(user,query,false);
    private async Task<CompanyWorkOrderCompletionReview> ReadCompletionEvidenceAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query,bool owned,string permission="Maintenance.VerifyCompletion")
    {
        query.Require();var task=query.Task;var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);
        var context=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,permission);
        if(executionPermits is null || workOrders is null || inspections is not ICompanyWorkOrderRequirementSource requirements || contractors is not ICompanyPersonnelIdentityOwner identities)
            throw new InvalidOperationException("Existing native report/requirement/person identity owners are required for independent completion review.");
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,task.WorkOrderId,permission)!=context)throw new UnauthorizedAccessException("Original completion reviewer/company/FIELD/order/native authority changed.");}
        var identityContext=new CompanyPersonnelPolicyContext(context.Company,context.Database,context.ActorUserId,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{context.Revision,Permission=permission,context.Database})));
        async Task<CompanyWorkOrderCompletionReview> Observe()
        {
            await Guard();var report=await executionPermits.ReadSubmissionFactsAsync(context,query,Guard,owned);
            if(report.ExecutorUserId==context.ActorUserId)throw new UnauthorizedAccessException("An independent account must review the original executor's work report.");
            await Guard();var order=CompanyInventorySourceReview.Copy(await workOrders.GetByIdAsync(task.FieldId,task.WorkOrderId)??throw new InvalidOperationException("Original native work order is unavailable."));
            var rows=owned?await requirements.ReadRequirementProofCoreAsync(context,task.WorkOrderId,Guard):await requirements.ReadRequirementProofAsync(context,task.WorkOrderId,Guard);
            var coreIdentity=owned?contractors as ICompanyCompletionIdentityCore:null;if(owned && coreIdentity is null)throw new InvalidOperationException("Existing owned completion identity reader is required.");
            var catalog=owned?await coreIdentity!.ReadCompletionIdentitiesCoreAsync(identityContext,Guard):await identities.ReadPersonnelIdentitiesAsync(identityContext,Guard);var utc=DateTime.UtcNow;
            var links=catalog.Versions.Where(row=>row.UserId==context.ActorUserId && row.EffectiveFromUtc<=utc && row.EffectiveToUtc>utc).ToArray();CompanyNativePersonFacts? person=null;
            if(links.Length==1)person=owned?await coreIdentity!.ReadCompletionPersonCoreAsync(identityContext,links[0].PersonBaId,Guard):await identities.ReadIdentityPersonAsync(identityContext,links[0].PersonBaId,Guard);
            var comparison=DateTime.UtcNow;var result=new CompanyWorkOrderCompletionReview(context,report,order,rows,catalog,person,comparison,CompanyWorkOrderCompletionReview.GetBlockers(context,report,order,rows,catalog,person,comparison));result.Require();await Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original report/current work scope/inspections/reviewer identities changed during independent completion review.");await Guard();return CompanyInventorySourceReview.Copy(closing);
    }
}
