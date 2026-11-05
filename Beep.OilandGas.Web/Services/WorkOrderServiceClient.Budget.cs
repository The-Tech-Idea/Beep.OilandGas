using System.Text.Json;
using Beep.OilandGas.Client.App;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Web.Services;
public partial class WorkOrderServiceClient
{
    private CompanyClientSelection BudgetActor()
    {
        var selected=_apiClient.CompanySnapshot.Selection;
        if(selected is null || selected.Roles.Count==0 || !selected.Permissions.Contains("Accounting.View",StringComparer.Ordinal) ||
            !selected.Permissions.Contains("Accounting.PrepareWorkOrderBudget",StringComparer.Ordinal))throw new UnauthorizedAccessException("Select the original company/account with current standard financial roles and explicit budget preparation capability.");return selected;
    }
    private static string BudgetRoute(string order)=>"/api/field/current/workorder/"+Uri.EscapeDataString(order)+"/budget";
    public async Task<CompanyWorkOrderBudgetReview> ReviewBudgetAsync(CompanyWorkOrderBudgetQuery query,CancellationToken cancellationToken=default)
    {
        query.Require();var snapshot=_apiClient.CompanySnapshot;var selected=BudgetActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetQuery,CompanyWorkOrderBudgetReview>(BudgetRoute(query.WorkOrderId)+"/review",query,cancellationToken)
            ??throw new InvalidOperationException("Actual native work-order budget review is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Budget company/account changed.");result.Require();
        if(result.Context.Company!=selected.Scope || result.Context.ActorUserId!=selected.UserId || result.Query!=query)throw new UnauthorizedAccessException("Original budget company/account/order/book is required.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderBudgetRecovery> CreateBudgetAsync(CompanyWorkOrderBudgetCommand original,CancellationToken cancellationToken=default)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var snapshot=_apiClient.CompanySnapshot;var selected=BudgetActor();
        if(command.Review.Context.Company!=selected.Scope || command.Review.Context.ActorUserId!=selected.UserId)throw new UnauthorizedAccessException("Original budget maker/company is required.");
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetCommand,CompanyWorkOrderBudgetRecovery>(BudgetRoute(command.Review.Query.WorkOrderId),command,cancellationToken)
            ??throw new InvalidOperationException("Original budget acknowledgement is unavailable.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original budget context changed.");result.Require(selected.Scope,selected.UserId,new(command.Review.Query,command.IntentId));
        if(JsonSerializer.Serialize(command)!=JsonSerializer.Serialize(result.Command))throw new InvalidOperationException("Recorded budget differs from its retained original body.");return CompanyInventorySourceReview.Copy(result);
    }
    public async Task<CompanyWorkOrderBudgetRecovery> RecoverBudgetAsync(CompanyWorkOrderBudgetReference reference,CancellationToken cancellationToken=default)
    {
        reference.Require();var snapshot=_apiClient.CompanySnapshot;var selected=BudgetActor();
        var result=await _apiClient.PostAsync<CompanyWorkOrderBudgetReference,CompanyWorkOrderBudgetRecovery>(BudgetRoute(reference.Query.WorkOrderId)+"/recovery/reference",reference,cancellationToken)
            ??throw new InvalidOperationException("Original budget remains unconfirmed; retain its reference.");
        if(snapshot!=_apiClient.CompanySnapshot)throw new OperationCanceledException("Original private budget context changed.");result.Require(selected.Scope,selected.UserId,reference);return CompanyInventorySourceReview.Copy(result);
    }
}
