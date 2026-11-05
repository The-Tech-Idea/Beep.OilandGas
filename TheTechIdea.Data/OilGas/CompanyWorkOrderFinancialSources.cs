using System.Text.Json;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;

public sealed record CompanyWorkOrderFinancialQuery(string WorkOrderId,string FieldId)
{
    public void Require(){if(new[]{WorkOrderId,FieldId}.Any(value=>!LeaseSummaryValidator.Identifier(value) || value.Length>128))throw new ArgumentException("Original native work-order and FIELD identities are required.");}
}
/// <summary>Discover complete original work-order financial sources; each amount retains its own recorded book/currency.</summary>
public sealed record CompanyWorkOrderFinancialSources(FieldSelectionReview Context,AssetDatabaseScope Database,CompanyWorkOrderFinancialQuery Query,
    WorkOrderDetailModel Order,CompanyLedgerBookReview Books,IReadOnlyList<CompanyWorkOrderActualCostReview> Budgets,DateTime ComparedAtUtc)
{
    public void Require()
    {
        if(Context is null || Database is null || Query is null || Order is null || Books is null || Budgets is null)throw new InvalidOperationException("Complete original financial work-order sources are required.");
        Query.Require();FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Books.Require(Context.Company,Context.ActorUserId);RecordedWorkOrderComponentChecks.Require(Order);
        if(!LeaseSummaryValidator.Identifier(Database.ConnectionName) || !LeaseSummaryValidator.Identifier(Database.Fingerprint))throw new InvalidOperationException("Original financial PRODUCTION binding is required.");
        if(Order.Source!="WORK_ORDER" || Order.InstanceId!=Query.WorkOrderId || Order.FieldId!=Query.FieldId || Context.FieldId!=Query.FieldId ||
            Budgets.Count>CompanyWorkOrderBudgetReview.MaximumBudgets || Budgets.Any(row=>row is null) || Budgets.Select(row=>row.Budget.Query.AfeId).Distinct(StringComparer.Ordinal).Count()!=Budgets.Count ||
            ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc==default)throw new InvalidOperationException("Original native order and complete unique budget membership differ.");
        foreach(var row in Budgets)
        {
            row.Require();var current=row.Budget.Current;
            if(current.Context!=Context || current.BudgetDatabase!=Database || current.Query.WorkOrderId!=Query.WorkOrderId || current.Query.FieldId!=Query.FieldId ||
                CompanyWorkOrderExecutionRequirements.OrderRevision(current.Order)!=CompanyWorkOrderExecutionRequirements.OrderRevision(Order) ||
                !Books.Books.Contains(current.Book) || ComparedAtUtc<row.ComparedAtUtc)throw new InvalidOperationException("Discovered financial evidence differs from the original current company/account/order/book.");
        }
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,Database,Query,Order=CompanyWorkOrderExecutionRequirements.OrderRevision(Order),Books,
        Budgets=Budgets.OrderBy(row=>row.Budget.Query.AfeId,StringComparer.Ordinal).Select(row=>row.SourceRevision())}));}
}
