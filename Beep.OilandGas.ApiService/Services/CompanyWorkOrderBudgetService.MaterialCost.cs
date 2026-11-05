using System.Security.Claims;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyWorkOrderBudgetService
{

    private async Task<CompanyWorkOrderMaterialCostReview> ReviewMaterialCostCore(ClaimsPrincipal user,CompanyWorkOrderMaterialCostQuery query,
        (FieldSelectionReview Context,AssetDatabaseScope Budget,Func<Task> Guard) captured)
    {
        if(material is null || decisions is null)throw new InvalidOperationException("Existing actual material and independent financial decision owners are required.");
        async Task<(CompanyWorkOrderBudgetDecisionStatus Budget,CompanyWorkOrderMaterialCostSource Material)> Basis()
        {
            var current=await ReviewCore(user,query.Budget.Budget,captured);var draft=current.Budgets.SingleOrDefault(row=>row.AfeId==query.Budget.AfeId)??throw new InvalidOperationException("Original budget is unavailable.");
            var decision=await decisions.ReadWorkOrderBudgetDecisionFactsAsync(current.Context,current.BudgetDatabase,query.Budget,draft,captured.Guard);
            var status=new CompanyWorkOrderBudgetDecisionStatus(current,query.Budget,decision,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,query.Budget,decision),DateTime.UtcNow);status.Require();
            var recorded=await material.ReadWorkOrderMaterialCostSourceAsync(captured.Context,captured.Budget,query.Budget.Budget.WorkOrderId,query.UsageId,captured.Guard);await captured.Guard();return(status,recorded);
        }
        var first=await Basis();var result=await budgets.ReviewWorkOrderMaterialCostAsync(first.Budget,query,first.Material,captured.Guard);var closing=await Basis();
        if(first.Budget.SourceRevision()!=closing.Budget.SourceRevision() || first.Material!=closing.Material)throw new InvalidOperationException("Original material/approved budget/current scope/book changed during cost review.");
        result.Require();await captured.Guard();return result;
    }
    public async Task<CompanyWorkOrderMaterialCostReview> ReviewMaterialCostAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialCostQuery query)
    {query.Require();return await ReviewMaterialCostCore(user,query,await Capture(user,query.Budget.Budget,RecordActual));}
    public async Task<CompanyWorkOrderMaterialCostRecovery> RecordMaterialCostAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialCostCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var captured=await Capture(user,command.Review.Query.Budget.Budget,RecordActual);
        if(captured.Context!=command.Review.Budget.Current.Context || captured.Budget!=command.Review.Budget.Current.BudgetDatabase)throw new UnauthorizedAccessException("Original actual-cost recorder/company/FIELD and bindings are required.");
        var result=await budgets.RecordWorkOrderMaterialCostAsync(command,captured.Guard,()=>ReviewMaterialCostCore(user,command.Review.Query,captured),()=>material!.ReadWorkOrderMaterialCostSourceAsync(captured.Context,captured.Budget,command.Review.Query.Budget.Budget.WorkOrderId,command.Review.Query.UsageId,captured.Guard,true));
        result.Require(captured.Context.Company,captured.Context.ActorUserId,command.Reference());await captured.Guard();return result;
    }
    public async Task<CompanyWorkOrderMaterialCostRecovery> RecoverMaterialCostAsync(ClaimsPrincipal user,CompanyWorkOrderMaterialCostReference reference)
    {
        reference.Require();var captured=await Capture(user,reference.Query.Budget.Budget,RecordActual);
        var result=await budgets.RecoverWorkOrderMaterialCostAsync(captured.Context,captured.Budget,reference,captured.Guard);
        result.Require(captured.Context.Company,captured.Context.ActorUserId,reference);await captured.Guard();return result;
    }
}
