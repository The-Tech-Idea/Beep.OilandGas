using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public sealed partial class BusinessProcessServiceClient
{
    public async Task<CompanyMaintenanceRecordedWorkPage> ReadMaintenanceRecordedWorkPageAsync(CompanyMaintenanceRecordedWorkWindow window,int page,int size,CancellationToken cancellationToken=default)
    {
        window.Require();CompanyMaintenanceRecordedWorkPage.RequirePage(page,size);var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(false);
        var result=await _apiClient.PostAsync<CompanyMaintenanceRecordedWorkWindow,CompanyMaintenanceRecordedWorkPage>("/api/field/current/maintenance-handoffs/recorded-work/pages/"+page.ToString(System.Globalization.CultureInfo.InvariantCulture)+"/"+size.ToString(System.Globalization.CultureInfo.InvariantCulture),window,cancellationToken)??throw new InvalidOperationException("Recorded-work membership is unavailable; a failed read cannot establish an empty queue.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during recorded-work discovery.");result.Require(selected.Scope,selected.UserId,window,page,size);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAssetReturnStatus> ReadMaintenanceAssetReturnStatusAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(false);
        var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceAssetReturnStatus>("/api/field/current/maintenance-handoffs/execution-permit/asset-return/status",query,cancellationToken)??throw new InvalidOperationException("Recorded asset-return status is unavailable; a failed read cannot establish absence.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during public asset-return observation.");result.Require();
        if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query)throw new UnauthorizedAccessException("Original public asset-return company/account/report differs.");return CompanyInventorySourceReview.Copy(result);
    }
    private Beep.OilandGas.Client.App.CompanyClientSelection AssetReturnActor(CompanyMaintenanceAssetEvidence? assets=null)
    {var selected=MaintenanceClosureActor(true);if(assets is not null && CompanyMaintenanceAssetReturnCapabilities.ForOrder(assets.Order).Any(permission=>!selected.Permissions.Contains(permission,StringComparer.Ordinal)))throw new UnauthorizedAccessException("Current company capabilities for every affected return target are required.");return selected;}
    public async Task<CompanyMaintenanceAssetReturnReview> ReviewMaintenanceAssetReturnAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=AssetReturnActor();var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceAssetReturnReview>("/api/field/current/maintenance-handoffs/execution-permit/asset-return/review",query,cancellationToken)??throw new InvalidOperationException("Current original asset-return evidence is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during asset-return review.");result.Require();
        if(result.Assets.Context.Company!=selected.Scope || result.Assets.Context.ActorUserId!=selected.UserId || result.Assets.Report.Query!=query || !selected.Roles.Contains(result.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original company/account/report/configured return role differs.");_=AssetReturnActor(result.Assets);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecordMaintenanceAssetReturnAsync(CompanyMaintenanceAssetReturnCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=AssetReturnActor(command.Review.Assets);
        if(command.Review.Assets.Context.Company!=selected.Scope || command.Review.Assets.Context.ActorUserId!=selected.UserId || !selected.Roles.Contains(command.Review.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured asset-return actor is required.");
        var result=await _apiClient.PostAsync<CompanyMaintenanceAssetReturnCommand,CompanyMaintenanceAssetReturnRecovery>("/api/field/current/maintenance-handoffs/execution-permit/asset-return",command,cancellationToken)??throw new InvalidOperationException("Asset return remains unconfirmed; retain the original reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during asset return.");result.Require(selected.Scope,selected.UserId,command.Reference());
        if(System.Text.Json.JsonSerializer.Serialize(result.Command)!=System.Text.Json.JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded asset return differs from the retained original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAssetReturnRecovery> RecoverMaintenanceAssetReturnAsync(CompanyMaintenanceAssetReturnReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=AssetReturnActor();var result=await _apiClient.PostAsync<CompanyMaintenanceAssetReturnReference,CompanyMaintenanceAssetReturnRecovery>("/api/field/current/maintenance-handoffs/execution-permit/asset-return/recovery/reference",reference,cancellationToken)??throw new InvalidOperationException("Original asset return remains unconfirmed.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during asset-return recovery.");result.Require(selected.Scope,selected.UserId,reference);
        if(!selected.Roles.Contains(result.Command.Review.Physical.Assignment.Stage.AssignedRole,StringComparer.Ordinal))throw new UnauthorizedAccessException("Original configured asset-return role is required.");_=AssetReturnActor(result.Command.Review.Assets);return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyMaintenanceAssetEvidence> ReadMaintenanceAssetEvidenceAsync(CompanyWorkOrderCompletionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=MaintenanceClosureActor(false);var result=await _apiClient.PostAsync<CompanyWorkOrderCompletionQuery,CompanyMaintenanceAssetEvidence>("/api/field/current/maintenance-handoffs/execution-permit/asset-return/evidence",query,cancellationToken)??throw new InvalidOperationException("Original closed work-order asset evidence is unavailable; a failed read cannot establish operating state.");if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original company/account changed during asset evidence review.");result.Require();if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Report.Query!=query)throw new UnauthorizedAccessException("Original company/account/report asset evidence differs.");return CompanyInventorySourceReview.Copy(result);
    }
}
