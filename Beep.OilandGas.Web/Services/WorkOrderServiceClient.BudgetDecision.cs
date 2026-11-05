using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class WorkOrderServiceClient
{
    public async Task<CompanyWorkOrderFinancialSources> ReadFinancialSourcesAsync(CompanyWorkOrderFinancialQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;
        if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current financial company/account visibility is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderFinancialQuery,CompanyWorkOrderFinancialSources>(BudgetRoute(query.WorkOrderId)+"/financial-sources",query,cancellationToken)
            ??throw new InvalidOperationException("Original work-order financial sources are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original financial company/account changed.");result.Require();
        if(result.Query!=query || result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original financial source company/account/order differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderActualCostReview> ReadActualCostsAsync(CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;
        if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current financial company/account visibility is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetDecisionQuery,CompanyWorkOrderActualCostReview>(BudgetRoute(query.Budget.WorkOrderId)+"/actual-costs",query,cancellationToken)
            ??throw new InvalidOperationException("Complete native actual costs are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original financial company/account changed.");result.Require();
        if(result.Budget.Query!=query || result.Budget.Current.Context.Company!=selected.Scope || result.Budget.Current.Context.ActorUserId!=selected.UserId)
            throw new UnauthorizedAccessException("Original public cost company/account/order/book differs.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderBudgetDecisionStatus> ReadBudgetDecisionStatusAsync(CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=snapshot.Selection;
        if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal))throw new UnauthorizedAccessException("Current standard financial roles and company accounting visibility are required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetDecisionQuery,CompanyWorkOrderBudgetDecisionStatus>(BudgetRoute(query.Budget.WorkOrderId)+"/decision/status",query,cancellationToken)
            ??throw new InvalidOperationException("Recorded decision and current budget scope validity are unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original budget status company/account changed.");result.Require();
        if(result.Query!=query || result.Current.Context.Company!=selected.Scope || result.Current.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original public budget status company/account/order/book is required.");return CompanyInventorySourceReview.Copy(result);
    }
    private CompanyClientSelection BudgetDecisionActor()
    {
        var selected=_apiClient.CompanySnapshot.Selection;if(selected is null || selected.Roles.Count==0 ||
            !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) || !selected.Permissions.Contains("Accounting.ReviewWorkOrderBudget",StringComparer.Ordinal))
            throw new UnauthorizedAccessException("Current standard financial roles and independent work-order budget review capability are required.");return selected;
    }
    public async Task<CompanyWorkOrderBudgetDecisionReview> ReviewBudgetDecisionAsync(CompanyWorkOrderBudgetDecisionQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=BudgetDecisionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetDecisionQuery,CompanyWorkOrderBudgetDecisionReview>(BudgetRoute(query.Budget.WorkOrderId)+"/decision/review",query,cancellationToken)
            ??throw new InvalidOperationException("Original independent budget review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Independent budget reviewer context changed.");result.Require();
        if(result.Query!=query || result.Current.Context.Company!=selected.Scope || result.Current.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original independent budget reviewer/company/order/AFE is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderBudgetDecisionRecovery> RecordBudgetDecisionAsync(CompanyWorkOrderBudgetDecisionCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=BudgetDecisionActor();
        if(command.Review.Current.Context.Company!=selected.Scope || command.Review.Current.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original independent budget reviewer/company is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetDecisionCommand,CompanyWorkOrderBudgetDecisionRecovery>(BudgetRoute(command.Review.Query.Budget.WorkOrderId)+"/decision",command,cancellationToken)
            ??throw new InvalidOperationException("Original independent budget decision acknowledgement is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original independent decision context changed.");result.Require(selected.Scope,selected.UserId,command.Reference());
        if(JsonSerializer.Serialize(result.Command)!=JsonSerializer.Serialize(command))throw new InvalidOperationException("Recorded independent decision differs from its retained original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderBudgetDecisionRecovery> RecoverBudgetDecisionAsync(CompanyWorkOrderBudgetDecisionReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=BudgetDecisionActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetDecisionReference,CompanyWorkOrderBudgetDecisionRecovery>(BudgetRoute(reference.Query.Budget.WorkOrderId)+"/decision/recovery/reference",reference,cancellationToken)
            ??throw new InvalidOperationException("Original independent budget decision remains unconfirmed; retain its reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private independent decision recovery context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
}
