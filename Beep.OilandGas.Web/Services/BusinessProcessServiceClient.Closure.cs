using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class BusinessProcessServiceClient
{
    private Beep.OilandGas.Client.App.CompanyClientSelection MaintenanceClosureActor(bool approve)
    {var selected=_apiClient.CompanySnapshot.Selection;if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.ReturnToService",StringComparer.Ordinal) || approve && !selected.Permissions.Contains("Workflow.Execute",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard return role and return/execution capability are required.");return selected;}
    public async Task<CompanyMaintenanceClosureReview> ReviewMaintenanceClosureAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(true);var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceClosureReview>("/api/field/current/maintenance-handoffs/execution-permit/closure/review",query,cancellationToken)??throw new InvalidOperationException("Current independent return evidence is unavailable.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original return company/account changed.");result.Require();var stage=result.Stage;
        if(stage.Context.Company!=selected.Scope || stage.Context.ActorUserId!=selected.UserId || result.Current.Completion.Report.Query!=query || !selected.Roles.Contains(stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured return role/company/account/report differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceClosureRecovery> CloseMaintenanceAsync(CompanyMaintenanceClosureCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(true);var stage=command.Review.Stage;if(stage.Context.Company!=selected.Scope || stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured closure actor is required.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceClosureCommand,CompanyMaintenanceClosureRecovery>("/api/field/current/maintenance-handoffs/execution-permit/closure",command,cancellationToken)??throw new InvalidOperationException("Maintenance workflow closure is unconfirmed; retain the original reference.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original return company/account changed.");result.Require(selected.Scope,selected.UserId,command.Reference());if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded maintenance closure differs from the retained original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceClosureRecovery> RecoverMaintenanceClosureAsync(CompanyMaintenanceClosureReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(true);var result=await _apiClient.PostAsync<CompanyMaintenanceClosureReference,CompanyMaintenanceClosureRecovery>("/api/field/current/maintenance-handoffs/execution-permit/closure/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original maintenance workflow closure remains unconfirmed.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original return company/account changed.");result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured closure actor role is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceClosureStatus> ReadMaintenanceClosureStatusAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(false);var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceClosureStatus>("/api/field/current/maintenance-handoffs/execution-permit/closure/status",query,cancellationToken)??throw new InvalidOperationException("Recorded maintenance closure status is unavailable; a failed read cannot establish absence.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original return company/account changed.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query)throw new UnauthorizedAccessException("Original public closure status company/account/report differs.");return CompanyInventorySourceReview.Copy(result);
    }
}
