using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Text.Json;
namespace Beep.OilandGas.ProductionOperations.Models;

/// <summary>Complete recorded native costs in the original book; no inferred conversion or draft-header totals.</summary>
public sealed record CompanyWorkOrderActualCostReview(CompanyWorkOrderBudgetDecisionStatus Budget,
    IReadOnlyList<CompanyWorkOrderActualCostFacts> Costs, DateTime ComparedAtUtc)
{
    public const int MaximumCosts=4096;
    public void Require()
    {
        if(Budget is null || Costs is null)throw new InvalidOperationException("Current budget and complete native cost membership are required.");
        Budget.Require();
        if(Costs.Count>MaximumCosts || Costs.Any(row=>row is null) || Costs.Select(row=>row.Id).Distinct(StringComparer.Ordinal).Count()!=Costs.Count ||
            Costs.Select(row=>(row.SourceKind,row.SourceId)).Distinct().Count()!=Costs.Count || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Budget.ComparedAtUtc)
            throw new InvalidOperationException("Complete unique recorded cost/source membership and current UTC observation are required.");
        foreach(var row in Costs)
        {
            row.Require();if(row.AfeId!=Budget.Query.AfeId || row.CurrencyCode!=Budget.Original().CurrencyCode || row.ValuedAtUtc>ComparedAtUtc)
                throw new InvalidOperationException("Original cost AFE/currency/date differs; conversion is not inferred.");
        }
    }
    public decimal RecordedTotal(){Require();try{return Costs.Sum(row=>row.Amount);}catch(OverflowException error){throw new InvalidOperationException("Recorded cost total exceeds supported currency capacity.",error);}}
    public decimal RemainingOriginalBudget(){try{return Budget.Original().BudgetAmount-RecordedTotal();}catch(OverflowException error){throw new InvalidOperationException("Original budget variance exceeds supported currency capacity.",error);}}
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Budget=Budget.SourceRevision(),Costs=Costs.OrderBy(row=>row.Id,StringComparer.Ordinal)}));}
}
