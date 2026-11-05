using System.Security.Claims;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyWorkOrderBudgetService
{
    private const string RecordActual="Accounting.RecordWorkOrderActualCost";
    private async Task<CompanyWorkOrderLaborCostReview> ReviewLaborCostCore(ClaimsPrincipal user,CompanyWorkOrderLaborCostQuery query,
        (FieldSelectionReview Context,AssetDatabaseScope Budget,Func<Task> Guard) captured)
    {
        if(labor is null || decisions is null)throw new InvalidOperationException("Existing actual labor and independent financial decision owners are required.");
        async Task<(CompanyWorkOrderBudgetDecisionStatus Budget,CompanyWorkOrderLaborUsageFacts Labor)> Basis()
        {
            var current=await ReviewCore(user,query.Budget.Budget,captured);var draft=current.Budgets.SingleOrDefault(row=>row.AfeId==query.Budget.AfeId)??throw new InvalidOperationException("Original budget is unavailable.");
            var decision=await decisions.ReadWorkOrderBudgetDecisionFactsAsync(current.Context,current.BudgetDatabase,query.Budget,draft,captured.Guard);
            var status=new CompanyWorkOrderBudgetDecisionStatus(current,query.Budget,decision,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,query.Budget,decision),DateTime.UtcNow);status.Require();
            var recorded=await labor.ReadLaborFactsAsync(captured.Context,query.Budget.Budget.WorkOrderId,query.LaborId,captured.Guard);await captured.Guard();return(status,recorded);
        }
        var first=await Basis();var result=await budgets.ReviewWorkOrderLaborCostAsync(first.Budget,query,first.Labor,captured.Guard);var closing=await Basis();
        if(first.Budget.SourceRevision()!=closing.Budget.SourceRevision() || first.Labor!=closing.Labor)throw new InvalidOperationException("Original labor/approved budget/current scope/book changed during cost review.");
        result.Require();await captured.Guard();return result;
    }
    public async Task<CompanyWorkOrderLaborCostReview> ReviewLaborCostAsync(ClaimsPrincipal user,CompanyWorkOrderLaborCostQuery query)
    {query.Require();return await ReviewLaborCostCore(user,query,await Capture(user,query.Budget.Budget,RecordActual));}
    public async Task<CompanyWorkOrderLaborCostRecovery> RecordLaborCostAsync(ClaimsPrincipal user,CompanyWorkOrderLaborCostCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await Capture(user,command.Review.Query.Budget.Budget,RecordActual);
        if(captured.Context!=command.Review.Budget.Current.Context || captured.Budget!=command.Review.Budget.Current.BudgetDatabase)throw new UnauthorizedAccessException("Original actual-cost recorder/company/FIELD and bindings are required.");
        var result=await budgets.RecordWorkOrderLaborCostAsync(command,captured.Guard,()=>ReviewLaborCostCore(user,command.Review.Query,captured));
        result.Require(captured.Context.Company,captured.Context.ActorUserId,command.Reference());await captured.Guard();return result;
    }
    public async Task<CompanyWorkOrderLaborCostRecovery> RecoverLaborCostAsync(ClaimsPrincipal user,CompanyWorkOrderLaborCostReference reference)
    {
        reference.Require();var captured=await Capture(user,reference.Query.Budget.Budget,RecordActual);
        var result=await budgets.RecoverWorkOrderLaborCostAsync(captured.Context,captured.Budget,reference,captured.Guard);
        result.Require(captured.Context.Company,captured.Context.ActorUserId,reference);await captured.Guard();return result;
    }
}
