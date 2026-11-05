using System.Security.Claims;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public sealed partial class CompanyWorkOrderBudgetService
{
    private const string ReviewBudget="Accounting.ReviewWorkOrderBudget";
    public async Task<CompanyWorkOrderFinancialSources> ReadFinancialSourcesAsync(ClaimsPrincipal user,CompanyWorkOrderFinancialQuery query)
    {
        query.Require();var captured=await CaptureOrder(user,query,"Accounting.View","READ");
        async Task<CompanyWorkOrderFinancialSources> Observe()
        {
            await captured.Guard();var order=CompanyInventorySourceReview.Copy(await orders.GetByIdAsync(query.FieldId,query.WorkOrderId)??throw new InvalidOperationException("Original native order is unavailable."));
            await captured.Guard();var configured=CompanyInventorySourceReview.Copy(await books.GetCompanyBooksAsync(captured.Context.Company,captured.Context.ActorUserId,captured.Guard));configured.Require(captured.Context.Company,captured.Context.ActorUserId);
            var original=await budgets.ReadWorkOrderBudgetFactsAsync(captured.Context,captured.Budget,query,captured.Guard);
            var financial=new List<CompanyWorkOrderActualCostReview>();
            foreach(var draft in original)
            {
                var row=await ReadActualCostsAsync(user,new(new(query.WorkOrderId,query.FieldId,draft.BookId),draft.AfeId));
                if(row.Budget.Original()!=draft)throw new InvalidOperationException("Original budget changed during financial discovery.");financial.Add(row);
            }
            var result=new CompanyWorkOrderFinancialSources(captured.Context,captured.Budget,query,order,configured,financial,DateTime.UtcNow);result.Require();await captured.Guard();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original work scope/books or complete budget/decision/cost population changed during discovery.");
        await captured.Guard();return CompanyInventorySourceReview.Copy(closing);
    }
    public async Task<CompanyWorkOrderActualCostReview> ReadActualCostsAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetDecisionQuery query)
    {
        query.Require();var captured=await Capture(user,query.Budget,"Accounting.View","READ");
        async Task<CompanyWorkOrderActualCostReview> Observe()
        {
            var budget=await ReadDecisionStatusAsync(user,query);
            if(budget.Current.Context!=captured.Context || budget.Current.BudgetDatabase!=captured.Budget)throw new UnauthorizedAccessException("Original financial company/account/FIELD and native bindings are required.");
            return await budgets.ReadWorkOrderActualCostsAsync(budget,captured.Guard);
        }
        var first=await Observe();var closing=await Observe();
        if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Original budget/decision/book or complete recorded costs changed during observation.");
        await captured.Guard();return closing;
    }
    public async Task<CompanyWorkOrderBudgetDecisionStatus> ReadDecisionStatusAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetDecisionQuery query)
    {
        query.Require();var captured=await Capture(user,query.Budget,"Accounting.View","READ");
        if(decisions is null)throw new InvalidOperationException("The existing native independent decision read owner is unavailable.");
        async Task<CompanyWorkOrderBudgetDecisionStatus> Observe()
        {
            var current=await ReviewCore(user,query.Budget,captured);var draft=current.Budgets.SingleOrDefault(row=>row.AfeId==query.AfeId)??throw new InvalidOperationException("Original native draft AFE is unavailable.");
            var recorded=await decisions.ReadWorkOrderBudgetDecisionFactsAsync(current.Context,current.BudgetDatabase,query,draft,captured.Guard);
            var result=new CompanyWorkOrderBudgetDecisionStatus(current,query,recorded,CompanyWorkOrderBudgetDecisionStatus.GetBlockers(current,query,recorded),DateTime.UtcNow);result.Require();return result;
        }
        var first=await Observe();var closing=await Observe();if(first.SourceRevision()!=closing.SourceRevision())throw new InvalidOperationException("Recorded decision or original current budget scope/book changed during status observation.");await captured.Guard();return closing;
    }
    public async Task<CompanyWorkOrderBudgetDecisionReview> ReviewDecisionAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetDecisionQuery query)
    {
        query.Require();var captured=await Capture(user,query.Budget,ReviewBudget);
        var current=await ReviewCore(user,query.Budget,captured);if(decisions is null)throw new InvalidOperationException("The existing native independent approval owner is unavailable.");
        var result=await decisions.ReviewWorkOrderBudgetDecisionAsync(current,query,captured.Guard);result.Require();await captured.Guard();return result;
    }
    public async Task<CompanyWorkOrderBudgetDecisionRecovery> DecideAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetDecisionCommand original)
    {
        var command=CompanyInventorySourceReview.Copy(original);command.Require();var query=command.Review.Query;var captured=await Capture(user,query.Budget,ReviewBudget);
        if(command.Review.Current.Context!=captured.Context || command.Review.Current.BudgetDatabase!=captured.Budget)throw new UnauthorizedAccessException("Original independent reviewer/company/FIELD and bindings are required.");
        if(decisions is null)throw new InvalidOperationException("The existing native independent approval owner is unavailable.");
        async Task<CompanyWorkOrderBudgetDecisionReview> Current()
        {
            var current=await ReviewCore(user,query.Budget,captured);var result=new CompanyWorkOrderBudgetDecisionReview(current,query);result.Require();return result;
        }
        var outcome=await decisions.RecordWorkOrderBudgetDecisionAsync(command,captured.Guard,Current);
        outcome.Require(captured.Context.Company,captured.Context.ActorUserId,command.Reference());await captured.Guard();return outcome;
    }
    public async Task<CompanyWorkOrderBudgetDecisionRecovery> RecoverDecisionAsync(ClaimsPrincipal user,CompanyWorkOrderBudgetDecisionReference reference)
    {
        reference.Require();var captured=await Capture(user,reference.Query.Budget,ReviewBudget);
        if(decisions is null)throw new InvalidOperationException("The existing native independent decision recovery owner is unavailable.");
        var result=await decisions.RecoverWorkOrderBudgetDecisionAsync(captured.Context,captured.Budget,reference,captured.Guard);
        result.Require(captured.Context.Company,captured.Context.ActorUserId,reference);await captured.Guard();return result;
    }
}
