using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    public async Task<CompanyMaintenanceReturnHandoffRecovery> RecordReturnHandoffAsync(ClaimsPrincipal user,CompanyMaintenanceReturnHandoffCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await CompletionDecisionAuthority(user,command.Reference().Query);
        if(captured.Owner is not ICompanyMaintenanceReturnHandoffOwner owner || command.Review.Stage.Context!=captured.Lifecycle || command.Review.Completion.Context!=captured.Core)throw new UnauthorizedAccessException("Original reviewer and return handoff CORE/lifecycle bindings differ.");
        var result=await owner.RecordReturnHandoffAsync(command,captured.Guard,owned=>executionPermits!.ReadCompletionDecisionProofAsync(captured.Core,captured.Lifecycle.Database.ConnectionName,owned,()=>captured.Guard(command.Review.Stage.AssignedRole),readOwned=>ReadCompletionEvidenceAsync(user,command.Reference().Query,readOwned)));
        result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,command.Reference());await captured.Guard(command.Review.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceReturnHandoffRecovery> RecoverReturnHandoffAsync(ClaimsPrincipal user,CompanyMaintenanceReturnHandoffReference reference)
    {
        reference.Require();var captured=await CompletionDecisionAuthority(user,reference.Query);if(captured.Owner is not ICompanyMaintenanceReturnHandoffOwner owner)throw new InvalidOperationException("Existing native return handoff owner is required.");
        var result=await owner.RecoverReturnHandoffAsync(captured.Lifecycle,reference,captured.Guard);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,reference);
        if(result.Command.Review.Completion.Context.Database!=captured.Core.Database)throw new UnauthorizedAccessException("Original return handoff CORE binding differs from current recovery authority.");await captured.Guard(result.Command.Review.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceReturnHandoffReview> ReviewReturnHandoffAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await CompletionDecisionAuthority(user,query);
        if(captured.Owner is not ICompanyMaintenanceReturnHandoffOwner owner)throw new InvalidOperationException("Existing native configured return review owner is required.");
        var completion=await ReviewCompletionAsync(user,query);if(completion.Context!=captured.Core)throw new UnauthorizedAccessException("Original return review CORE authority differs.");
        var review=await owner.ReviewReturnHandoffAsync(captured.Lifecycle,query,completion,captured.Guard);var closing=await ReviewCompletionAsync(user,query);
        if(completion.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Current work report, inspection or reviewer identity changed around configured return review.");
        await captured.Guard(review.Stage.AssignedRole);review.Require();return review;
    }
    private async Task<(FieldSelectionReview Lifecycle,FieldSelectionReview Core,ICompanyWorkOrderCompletionDecisionOwner Owner,Func<string?,Task> Guard)> CompletionDecisionAuthority(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        query.Require();var boundary=await Boundary(user,query.Task.WorkOrderId,query.Task.FieldId);var core=await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.VerifyCompletion");
        if(boundary.Process is not ICompanyWorkOrderCompletionDecisionOwner owner || executionPermits is null)throw new InvalidOperationException("Existing native completion decision and report owners are required.");
        async Task Guard(string? role)
        {
            var access=await company.RefreshCompanyAsync();if(!access.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal) || !access.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal) || role is not null && (!user.IsInRole(role) || !access.Roles.Contains(role,StringComparer.Ordinal)) || await Authority(user,boundary.Preference,query.Task.WorkOrderId)!=boundary.Context || await PermitAuthority(user,boundary.Preference,query.Task.WorkOrderId,"Maintenance.VerifyCompletion")!=core)throw new UnauthorizedAccessException("Original configured independent reviewer/company/FIELD/order/role/approval capability changed.");
        }
        await Guard(null);return(boundary.Context,core,owner,Guard);
    }
    public async Task<CompanyWorkOrderCompletionDecisionReview> ReviewCompletionDecisionAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await CompletionDecisionAuthority(user,query);var stage=await captured.Owner.ReviewCompletionDecisionStageAsync(captured.Lifecycle,query,captured.Guard);var completion=await ReviewCompletionAsync(user,query);var closing=await captured.Owner.ReviewCompletionDecisionStageAsync(captured.Lifecycle,query,captured.Guard);
        if(stage.Context!=captured.Lifecycle || JsonSerializer.Serialize(stage)!=JsonSerializer.Serialize(closing))throw new InvalidOperationException("Original configured reviewer assignment changed around completion evidence.");await captured.Guard(stage.AssignedRole);var result=new CompanyWorkOrderCompletionDecisionReview(stage,completion);result.Require();return result;
    }
    public async Task<CompanyWorkOrderCompletionDecisionRecovery> RecordCompletionDecisionAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionDecisionCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await CompletionDecisionAuthority(user,command.Reference().Query);
        if(command.Review.Stage.Context!=captured.Lifecycle || command.Review.Completion.Context!=captured.Core)throw new UnauthorizedAccessException("Original independent reviewer and native completion bindings differ.");
        var result=await captured.Owner.RecordCompletionDecisionAsync(command,captured.Guard,owned=>executionPermits!.ReadCompletionDecisionProofAsync(captured.Core,captured.Lifecycle.Database.ConnectionName,owned,()=>captured.Guard(command.Review.Stage.AssignedRole),readOwned=>ReadCompletionEvidenceAsync(user,command.Reference().Query,readOwned)));
        result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,command.Reference());await captured.Guard(command.Review.Stage.AssignedRole);return result;
    }
    public async Task<CompanyWorkOrderCompletionDecisionRecovery> RecoverCompletionDecisionAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionDecisionReference reference)
    {
        reference.Require();var captured=await CompletionDecisionAuthority(user,reference.Query);var result=await captured.Owner.RecoverCompletionDecisionAsync(captured.Lifecycle,reference,captured.Guard);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,reference);
        if(result.Command.Review.Completion.Context.Database!=captured.Core.Database)throw new UnauthorizedAccessException("Original completion decision CORE binding differs from current recovery authority.");await captured.Guard(result.Command.Review.Stage.AssignedRole);return result;
    }
}
