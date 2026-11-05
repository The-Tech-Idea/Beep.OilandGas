using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.ProductionOperations.Models;
using System.Text.Json;
namespace Beep.OilandGas.Web.Services;
public sealed partial class BusinessProcessServiceClient
{
    public async Task<CompanyMaintenanceReturnHandoffRecovery> RecordReturnHandoffAsync(CompanyMaintenanceReturnHandoffCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=CompletionDecisionActor();if(command.Review.Stage.Context.Company!=selected.Scope || command.Review.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured reviewer company/account is required for return handoff.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceReturnHandoffCommand,CompanyMaintenanceReturnHandoffRecovery>("/api/field/current/maintenance-handoffs/execution-permit/return-handoff",command,cancellationToken)??throw new InvalidOperationException("Return handoff acknowledgement is unconfirmed; retain its original reference.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reviewer company/account changed.");result.Require(selected.Scope,selected.UserId,command.Reference());if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded return handoff differs from its original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceReturnHandoffRecovery> RecoverReturnHandoffAsync(CompanyMaintenanceReturnHandoffReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=CompletionDecisionActor();var result=await _apiClient.PostAsync<CompanyMaintenanceReturnHandoffReference,CompanyMaintenanceReturnHandoffRecovery>("/api/field/current/maintenance-handoffs/execution-permit/return-handoff/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original return handoff remains unconfirmed.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reviewer company/account changed.");result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured reviewer role is required for return handoff recovery.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceReturnHandoffReview> ReviewReturnHandoffAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=CompletionDecisionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceReturnHandoffReview>("/api/field/current/maintenance-handoffs/execution-permit/return-handoff/review",query,cancellationToken)??throw new InvalidOperationException("Configured return approval review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during configured return review.");result.Require();
        if(result.Completion.Report.Query!=query || result.Stage.Context.Company!=selected.Scope || result.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Configured return review differs from the original company/account/report/reviewer role.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderCompletionDecisionStatus> ReadCompletionDecisionStatusAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard company completion evidence read authority is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyWorkOrderCompletionDecisionStatus>("/api/field/current/maintenance-handoffs/execution-permit/completion-decision/status",query,cancellationToken)??throw new InvalidOperationException("Recorded decision status is unavailable; an error cannot establish that no decision exists.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during completion decision status read.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query)throw new UnauthorizedAccessException("Recorded decision status belongs to another company/account/source query.");return CompanyInventorySourceReview.Copy(result);
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection CompletionDecisionActor()
    {
        var selected=_apiClient.CompanySnapshot.Selection;if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Maintenance.VerifyCompletion",StringComparer.Ordinal) || !selected.Permissions.Contains("Workflow.Approve",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard reviewer role, completion verification and workflow approval capabilities are required.");return selected;
    }
    public async Task<CompanyWorkOrderCompletionDecisionReview> ReviewCompletionDecisionAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=CompletionDecisionActor();var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyWorkOrderCompletionDecisionReview>("/api/field/current/maintenance-handoffs/execution-permit/completion-decision/review",query,cancellationToken)??throw new InvalidOperationException("Original completion decision review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reviewer company/account changed.");result.Require();if(result.Completion.Report.Query!=query || result.Stage.Context.Company!=selected.Scope || result.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(result.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original independent reviewer/source/configured role differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderCompletionDecisionRecovery> RecordCompletionDecisionAsync(CompanyWorkOrderCompletionDecisionCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=CompletionDecisionActor();if(command.Review.Stage.Context.Company!=selected.Scope || command.Review.Stage.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured independent reviewer company/account is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionDecisionCommand,CompanyWorkOrderCompletionDecisionRecovery>("/api/field/current/maintenance-handoffs/execution-permit/completion-decision",command,cancellationToken)??throw new InvalidOperationException("Completion decision acknowledgement is unconfirmed; retain its original reference.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reviewer company/account changed.");result.Require(selected.Scope,selected.UserId,command.Reference());if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded decision differs from its original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderCompletionDecisionRecovery> RecoverCompletionDecisionAsync(CompanyWorkOrderCompletionDecisionReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=CompletionDecisionActor();var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionDecisionReference,CompanyWorkOrderCompletionDecisionRecovery>("/api/field/current/maintenance-handoffs/execution-permit/completion-decision/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original completion decision remains unconfirmed.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original reviewer company/account changed.");result.Require(selected.Scope,selected.UserId,reference);if(!selected.Roles.Contains(result.Command.Review.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured reviewer role is required.");return CompanyInventorySourceReview.Copy(result);
    }
}
