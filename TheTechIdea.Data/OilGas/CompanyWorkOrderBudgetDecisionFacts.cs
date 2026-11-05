using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded outcome and audit only; no private command, rationale or recovery intent.</summary>
public sealed record CompanyWorkOrderBudgetDecisionFacts(CompanyScope Company,CompanyWorkOrderBudgetDecisionQuery Query,
    string WorkflowId,string OriginalAfeRevision,string BudgetAuthor,string Reviewer,WorkOrderBudgetDecision Decision,
    decimal BudgetAmount,string CurrencyCode,DateTime DecidedAtUtc,string RecordedRevision)
{
    public void Require(CompanyScope company,CompanyWorkOrderBudgetDecisionQuery query,CompanyWorkOrderBudgetFacts draft)
    {
        query.Require();draft.Require();if(Company!=company || Query!=query || WorkflowId!=CompanyWorkOrderBudgetDecisionCommand.IdFor(company,query.AfeId) ||
            draft.AfeId!=query.AfeId || OriginalAfeRevision!=draft.RecordedRevision || BudgetAuthor!=draft.CreatedBy || !LeaseSummaryValidator.Identifier(Reviewer) || Reviewer==BudgetAuthor ||
            Decision is not (WorkOrderBudgetDecision.Approve or WorkOrderBudgetDecision.Reject) || BudgetAmount!=draft.BudgetAmount || CurrencyCode!=draft.CurrencyCode ||
            DecidedAtUtc.Kind!=DateTimeKind.Utc || DecidedAtUtc<draft.CreatedAtUtc || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("Recorded independent decision does not match the complete original budget/currency/author/audit.");
    }
    public static CompanyWorkOrderBudgetDecisionFacts From(APPROVAL_WORKFLOW row)
    {
        var command=CompanyWorkOrderBudgetDecisionCommand.Read(row);var context=command.Review.Current.Context;
        var result=new CompanyWorkOrderBudgetDecisionFacts(context.Company,command.Review.Query,row.APPROVAL_WORKFLOW_ID,row.ORIGINAL_AFE_REVISION!,row.REQUESTED_BY,row.APPROVED_BY,
            command.Decision,row.AMOUNT,row.CURRENCY_CODE!,row.APPROVAL_DATE!.Value,row.WORK_ORDER_DECISION_REVISION!);
        result.Require(context.Company,command.Review.Query,command.Review.Original());return result;
    }
}
/// <summary>Recorded approval is distinguished from current scope/book validity. No absent outcome becomes approval.</summary>
public sealed record CompanyWorkOrderBudgetDecisionStatus(CompanyWorkOrderBudgetReview Current,CompanyWorkOrderBudgetDecisionQuery Query,
    CompanyWorkOrderBudgetDecisionFacts? Recorded,IReadOnlyList<string> Blockers,DateTime ComparedAtUtc)
{
    public CompanyWorkOrderBudgetFacts Original()=>Current.Budgets.Single(row=>row.AfeId==Query.AfeId);
    public bool IsApprovedForCurrentScope{get{Require();return Blockers.Count==0;}}
    public static string[] GetBlockers(CompanyWorkOrderBudgetReview current,CompanyWorkOrderBudgetDecisionQuery query,CompanyWorkOrderBudgetDecisionFacts? recorded)
    {
        current.Require();query.Require();if(current.Query!=query.Budget || current.Budgets.Count(row=>row.AfeId==query.AfeId)!=1)throw new InvalidOperationException("One original budget is required for decision visibility.");
        var draft=current.Budgets.Single(row=>row.AfeId==query.AfeId);recorded?.Require(current.Context.Company,query,draft);var blockers=new List<string>();
        if(recorded is null)blockers.Add("Independent budget decision is not recorded.");
        else if(recorded.Decision!=WorkOrderBudgetDecision.Approve)blockers.Add("The original work-order budget was independently rejected.");
        if(draft.OriginalOrderRevision!=CompanyWorkOrderExecutionRequirements.OrderRevision(current.Order))blockers.Add("Original budget work scope changed.");
        if(draft.OriginalBookRevision!=current.Book.NativeRevision)blockers.Add("Original budget company book changed.");return blockers.ToArray();
    }
    public void Require()
    {
        if(Current is null || Query is null || Blockers is null || ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Current.ComparedAtUtc)
            throw new InvalidOperationException("Current original budget/decision/source observation is required.");
        var expected=GetBlockers(Current,Query,Recorded);if(!expected.SequenceEqual(Blockers,StringComparer.Ordinal))throw new InvalidOperationException("Recorded budget decision/current validity differs from actual source blockers.");
        if(ComparedAtUtc<Original().CreatedAtUtc || Recorded is {} fact && ComparedAtUtc<fact.DecidedAtUtc)throw new InvalidOperationException("Current observation must follow original budget and decision audits.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Current=Current.SourceRevision(),Query,Recorded,Blockers}));}
}
