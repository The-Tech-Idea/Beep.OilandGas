using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class WorkOrderServiceClient
{
    public async Task<CompanyWorkOrderMaterialCostReview> ReviewMaterialCostAsync(CompanyWorkOrderMaterialCostQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=ActualCostActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialCostQuery,CompanyWorkOrderMaterialCostReview>(BudgetRoute(query.Budget.Budget.WorkOrderId)+"/material-cost/review",query,cancellationToken)
            ??throw new InvalidOperationException("Actual recorded material/approved budget/cost review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original cost company/account changed.");result.Require();
        if(result.Query!=query || result.Budget.Current.Context.Company!=selected.Scope || result.Budget.Current.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original actual-cost company/account/source is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialCostRecovery> RecordMaterialCostAsync(CompanyWorkOrderMaterialCostCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=ActualCostActor();
        if(command.Review.Budget.Current.Context.Company!=selected.Scope || command.Review.Budget.Current.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original actual-cost recorder/company is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialCostCommand,CompanyWorkOrderMaterialCostRecovery>(BudgetRoute(command.Review.Query.Budget.Budget.WorkOrderId)+"/material-cost",command,cancellationToken)
            ??throw new InvalidOperationException("Original actual-cost acknowledgement is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original actual-cost context changed.");result.Require(selected.Scope,selected.UserId,command.Reference());
        if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded valuation differs from its retained original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderMaterialCostRecovery> RecoverMaterialCostAsync(CompanyWorkOrderMaterialCostReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=ActualCostActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderMaterialCostReference,CompanyWorkOrderMaterialCostRecovery>(BudgetRoute(reference.Query.Budget.Budget.WorkOrderId)+"/material-cost/recovery/reference",reference,cancellationToken)
            ??throw new InvalidOperationException("Original actual cost remains unconfirmed; retain its original reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private cost recovery context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
}
