using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyWorkOrderCompletionQuery> DiscoverReturnAcceptanceReportAsync(ClaimsPrincipal user,CompanyMaintenanceReceivingQuery task)
    {
        task.Require();var boundary=await Boundary(user,task.WorkOrderId,task.FieldId);var core=await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ReturnToService");if(executionPermits is null)throw new InvalidOperationException("Existing recorded work report discovery owner is unavailable.");
        async Task Guard(){if(await Authority(user,boundary.Preference,task.WorkOrderId)!=boundary.Context || await PermitAuthority(user,boundary.Preference,task.WorkOrderId,"Maintenance.ReturnToService")!=core)throw new UnauthorizedAccessException("Original return report company/FIELD/order/CORE authority changed.");}
        var query=await executionPermits.DiscoverSubmissionQueryAsync(core,task,Guard);query.Require();await Guard();return query;
    }
    private async Task<(FieldSelectionReview Lifecycle,FieldSelectionReview Core,ICompanyWorkOrderReturnOwner Owner,Func<string?,Task> Guard)> ReturnAcceptanceAuthority(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query,bool approve,bool execute=false)
    {
        query.Require();var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var core=await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.ReturnToService");if(boundary.Process is not ICompanyWorkOrderReturnOwner owner || executionPermits is null)throw new InvalidOperationException("Existing native return decision/report owner is unavailable.");
        async Task Guard(string? role)
        {
            var current=await PermitAuthorities(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.ReturnToService",role,approve?"Workflow.Approve":execute?"Workflow.Execute":null);
            if(current.Lifecycle!=boundary.Context || current.Core!=core)throw new UnauthorizedAccessException("Current original return approver/company/FIELD/order/capability/CORE/lifecycle authority changed.");
        }
        await Guard(null);return(boundary.Context,core,owner,Guard);
    }
    private async Task<CompanyWorkOrderReturnReview> ReadReturnEvidenceAsync(ClaimsPrincipal user,CompanyMaintenanceReturnAssignment assignment,bool owned)
    {
        assignment.Require();var query=assignment.Verification.Query;var completion=await ReadCompletionEvidenceAsync(user,query,owned,"Maintenance.ReturnToService");var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var context=completion.Context;
        async Task Guard(){if(await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.ReturnToService")!=context)throw new UnauthorizedAccessException("Original physical return evidence authority changed.");}
        if(contractors is not ICompanyPersonnelIdentityOwner identities || owned && contractors is not ICompanyCompletionIdentityCore)throw new InvalidOperationException("Existing native return inspector identity/person owner is unavailable.");
        var identityContext=completion.Identities.Context;var people=new List<CompanyNativePersonFacts>();
        foreach(var check in completion.Requirements.Where(row=>row.ApplicableStage=="RETURN_TO_SERVICE" && row.Status=="PASS"))
        {
            if(check.InspectDate is not {} at || at.Kind!=DateTimeKind.Utc)continue;var links=completion.Identities.Versions.Where(row=>row.UserId==check.Inspector && row.EffectiveFromUtc<=at && row.EffectiveToUtc>at && row.RecordedAtUtc<=at).ToArray();if(links.Length!=1 || people.Any(row=>row.PersonBaId==links[0].PersonBaId))continue;
            await Guard();people.Add(owned?await ((ICompanyCompletionIdentityCore)contractors).ReadCompletionPersonCoreAsync(identityContext,links[0].PersonBaId,Guard):await identities.ReadIdentityPersonAsync(identityContext,links[0].PersonBaId,Guard));
        }
        var utc=DateTime.UtcNow;completion=completion with{ComparedAtUtc=utc,Blockers=CompanyWorkOrderCompletionReview.GetBlockers(completion.Context,completion.Report,completion.Order,completion.Requirements,completion.Identities,completion.ReviewerPerson,utc)};
        var result=new CompanyWorkOrderReturnReview(assignment,completion,people.ToArray(),CompanyWorkOrderReturnReview.GetBlockers(assignment,completion,people));result.Require();await Guard();return result;
    }
    public async Task<CompanyWorkOrderReturnReview> ReviewReturnAcceptanceAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await ReturnAcceptanceAuthority(user,query,true);var assignment=await captured.Owner.ReadReturnAssignmentAsync(captured.Lifecycle,query,captured.Guard);var first=await ReadReturnEvidenceAsync(user,assignment,false);var closing=await ReadReturnEvidenceAsync(user,assignment,false);var stage=await captured.Owner.ReadReturnAssignmentAsync(captured.Lifecycle,query,captured.Guard);
        if(first.Completion.Context!=captured.Core || first.SourceRevision()!=closing.SourceRevision() || JsonSerializer.Serialize(assignment)!=JsonSerializer.Serialize(stage))throw new InvalidOperationException("Original return assignment/work/inspection/person evidence changed around review.");await captured.Guard(stage.Stage.AssignedRole);return closing;
    }
    public async Task<CompanyWorkOrderReturnRecovery> RecordReturnAcceptanceAsync(ClaimsPrincipal user,CompanyWorkOrderReturnCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await ReturnAcceptanceAuthority(user,command.Reference().Query,true);var stage=command.Review.Assignment.Stage;
        if(stage.Context!=captured.Lifecycle || command.Review.Completion.Context!=captured.Core)throw new UnauthorizedAccessException("Original return acceptance company/account/CORE/lifecycle differs.");
        async Task<CompanyWorkOrderReturnReview?> Evidence(bool owned)
        {
            CompanyWorkOrderReturnReview? evidence=null;await executionPermits!.ReadCompletionDecisionProofAsync(captured.Core,captured.Lifecycle.Database.ConnectionName,owned,()=>captured.Guard(stage.AssignedRole),async readOwned=>{evidence=await ReadReturnEvidenceAsync(user,command.Review.Assignment,readOwned);return evidence.Completion;});return evidence;
        }
        var result=await captured.Owner.RecordReturnAcceptanceAsync(command,captured.Guard,Evidence);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,command.Reference());await captured.Guard(stage.AssignedRole);return result;
    }
    public async Task<CompanyWorkOrderReturnRecovery> RecoverReturnAcceptanceAsync(ClaimsPrincipal user,CompanyWorkOrderReturnReference reference)
    {
        reference.Require();var captured=await ReturnAcceptanceAuthority(user,reference.Query,true);var result=await captured.Owner.RecoverReturnAcceptanceAsync(captured.Lifecycle,reference,captured.Guard);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,reference);
        if(result.Command.Review.Completion.Context.Database!=captured.Core.Database)throw new UnauthorizedAccessException("Original return acceptance CORE binding changed.");await captured.Guard(result.Command.Review.Assignment.Stage.AssignedRole);return result;
    }
    public async Task<CompanyWorkOrderReturnStatus> ReadReturnAcceptanceStatusAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await ReturnAcceptanceAuthority(user,query,false);var report=await executionPermits!.ReadSubmissionFactsAsync(captured.Core,query,()=>captured.Guard(null));var first=await captured.Owner.ReadReturnAcceptanceFactsAsync(captured.Lifecycle,query,()=>captured.Guard(null));var closing=await captured.Owner.ReadReturnAcceptanceFactsAsync(captured.Lifecycle,query,()=>captured.Guard(null));var finalReport=await executionPermits.ReadSubmissionFactsAsync(captured.Core,query,()=>captured.Guard(null));
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(report)!=JsonSerializer.Serialize(finalReport) || closing is {} facts && (facts.CoreDatabase!=captured.Core.Database || facts.ReportRevision!=report.Revision))throw new InvalidOperationException("Original public return acceptance/report binding changed.");var result=new CompanyWorkOrderReturnStatus(captured.Core,query,closing);result.Require();await captured.Guard(null);return result;
    }
}
