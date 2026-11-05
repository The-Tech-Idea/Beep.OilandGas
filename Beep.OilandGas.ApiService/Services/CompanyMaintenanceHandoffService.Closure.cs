using System.Security.Claims;
using System.Text.Json;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyMaintenanceHandoffService
{
    private async Task<(FieldSelectionReview Lifecycle,FieldSelectionReview Core,ICompanyMaintenanceClosureOwner Owner,Func<string?,Task> Guard)> MaintenanceClosureAuthority(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query,bool execute)
    {
        var captured=await ReturnAcceptanceAuthority(user,query,false,execute);if(captured.Owner is not ICompanyMaintenanceClosureOwner owner)throw new InvalidOperationException("Existing native maintenance closure owner is unavailable.");
        return(captured.Lifecycle,captured.Core,owner,captured.Guard);
    }
    public async Task<CompanyMaintenanceClosureReview> ReviewMaintenanceClosureAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await MaintenanceClosureAuthority(user,query,true);var stage=await captured.Owner.ReviewMaintenanceClosureStageAsync(captured.Lifecycle,query,captured.Guard);var first=await ReadReturnEvidenceAsync(user,stage.Assignment,false);var closing=await ReadReturnEvidenceAsync(user,stage.Assignment,false);var final=await captured.Owner.ReviewMaintenanceClosureStageAsync(captured.Lifecycle,query,captured.Guard);
        if(first.Completion.Context!=captured.Core || first.SourceRevision()!=closing.SourceRevision() || JsonSerializer.Serialize(stage.Assignment)!=JsonSerializer.Serialize(final.Assignment) || stage.Acceptance!=final.Acceptance)throw new InvalidOperationException("Original final duty/physical acceptance/current return evidence changed during closure review.");var result=new CompanyMaintenanceClosureReview(final.Assignment.Stage,final.Acceptance,closing);result.Require();await captured.Guard(final.Assignment.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceClosureRecovery> CloseMaintenanceAsync(ClaimsPrincipal user,CompanyMaintenanceClosureCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await MaintenanceClosureAuthority(user,command.Reference().Query,true);var stage=command.Review.Stage;if(stage.Context!=captured.Lifecycle || command.Review.Current.Completion.Context!=captured.Core)throw new UnauthorizedAccessException("Original maintenance closure company/account/CORE/lifecycle differs.");
        async Task<CompanyWorkOrderReturnReview?> Evidence(bool owned)
        {
            CompanyWorkOrderReturnReview? evidence=null;await executionPermits!.ReadCompletionDecisionProofAsync(captured.Core,captured.Lifecycle.Database.ConnectionName,owned,()=>captured.Guard(stage.AssignedRole),async readOwned=>{evidence=await ReadReturnEvidenceAsync(user,command.Review.Current.Assignment,readOwned);return evidence.Completion;});return evidence;
        }
        var result=await captured.Owner.CloseMaintenanceAsync(command,captured.Guard,Evidence);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,command.Reference());await captured.Guard(stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceClosureRecovery> RecoverMaintenanceClosureAsync(ClaimsPrincipal user,CompanyMaintenanceClosureReference reference)
    {
        reference.Require();var captured=await MaintenanceClosureAuthority(user,reference.Query,true);var result=await captured.Owner.RecoverMaintenanceClosureAsync(captured.Lifecycle,reference,captured.Guard);result.Require(captured.Lifecycle.Company,captured.Lifecycle.ActorUserId,reference);if(result.Command.Review.Current.Completion.Context.Database!=captured.Core.Database)throw new UnauthorizedAccessException("Original maintenance closure CORE binding changed.");await captured.Guard(result.Command.Review.Stage.AssignedRole);return result;
    }
    public async Task<CompanyMaintenanceClosureStatus> ReadMaintenanceClosureStatusAsync(ClaimsPrincipal user,CompanyWorkOrderCompletionQuery query)
    {
        var captured=await MaintenanceClosureAuthority(user,query,false);var report=await executionPermits!.ReadSubmissionFactsAsync(captured.Core,query,()=>captured.Guard(null));var first=await captured.Owner.ReadMaintenanceClosureFactsAsync(captured.Lifecycle,query,()=>captured.Guard(null));var closing=await captured.Owner.ReadMaintenanceClosureFactsAsync(captured.Lifecycle,query,()=>captured.Guard(null));var finalReport=await executionPermits.ReadSubmissionFactsAsync(captured.Core,query,()=>captured.Guard(null));
        if(JsonSerializer.Serialize(first)!=JsonSerializer.Serialize(closing) || JsonSerializer.Serialize(report)!=JsonSerializer.Serialize(finalReport) || closing is {} facts && (facts.CoreDatabase!=captured.Core.Database || facts.ReportRevision!=report.Revision))throw new InvalidOperationException("Original public maintenance closure/report binding changed.");var result=new CompanyMaintenanceClosureStatus(captured.Core,query,closing);result.Require();await captured.Guard(null);return result;
    }
}
