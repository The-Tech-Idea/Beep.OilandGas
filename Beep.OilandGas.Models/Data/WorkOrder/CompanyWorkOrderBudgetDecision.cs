using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public enum WorkOrderBudgetDecision { Unspecified,Approve,Reject }
public sealed record CompanyWorkOrderBudgetDecisionQuery(CompanyWorkOrderBudgetQuery Budget,string AfeId)
{
    public void Require(){if(Budget is null || !LeaseSummaryValidator.Identifier(AfeId) || AfeId.Length>128)throw new ArgumentException("The original order/book and AFE identity are required.");Budget.Require();}
}
/// <summary>Public original draft and exact current scope/book; the maker's private budget command remains in its owner.</summary>
public sealed record CompanyWorkOrderBudgetDecisionReview(CompanyWorkOrderBudgetReview Current,CompanyWorkOrderBudgetDecisionQuery Query)
{
    public CompanyWorkOrderBudgetFacts Original()=>Current.Budgets.Single(row=>row.AfeId==Query.AfeId);
    public void Require()
    {
        if(Current is null || Query is null)throw new InvalidOperationException("Current original AFE/order/book review is required.");Current.Require();Query.Require();
        if(Current.Query!=Query.Budget || Current.Budgets.Count(row=>row.AfeId==Query.AfeId)!=1)throw new InvalidOperationException("One original AFE in the complete native budget population is required.");
        var original=Original();if(original.CreatedBy==Current.Context.ActorUserId)throw new UnauthorizedAccessException("The work-order budget author cannot review their own budget.");
        if(original.OriginalOrderRevision!=CompanyWorkOrderExecutionRequirements.OrderRevision(Current.Order) || original.OriginalBookRevision!=Current.Book.NativeRevision)
            throw new InvalidOperationException("Original draft work scope or configured book changed; independent review cannot silently adopt it.");
        if(Current.ComparedAtUtc<original.CreatedAtUtc)throw new InvalidOperationException("Current review must follow the original draft audit.");
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Current=Current.SourceRevision(),Query}));}
}
public sealed record CompanyWorkOrderBudgetDecisionCommand(CompanyWorkOrderBudgetDecisionReview Review,Guid IntentId,WorkOrderBudgetDecision Decision,string Rationale)
{
    public const int MaximumBodyLength=4*1024*1024;
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original independent budget review is required.");Review.Require();
        if(IntentId==Guid.Empty || Decision is not (WorkOrderBudgetDecision.Approve or WorkOrderBudgetDecision.Reject) || string.IsNullOrWhiteSpace(Rationale) ||
            Rationale.Length>4096 || Rationale.Any(char.IsControl) || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new ArgumentException("Retain an explicit independent approve/reject intent and rationale.");
    }
    public static string IdFor(CompanyScope company,string afe)=>"wo-budget-decision-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Afe=afe}));
    public string Id()=>IdFor(Review.Current.Context.Company,Review.Query.AfeId);
    public CompanyWorkOrderBudgetDecisionReference Reference()=>new(Review.Query,IntentId);
    public void RequireCurrent(CompanyWorkOrderBudgetDecisionReview current){Require();current.Require();if(current.Current.ComparedAtUtc<Review.Current.ComparedAtUtc || current.SourceRevision()!=Review.SourceRevision())throw new InvalidOperationException("Original AFE/order/book or complete budget population changed before independent decision.");}
    public APPROVAL_WORKFLOW Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.Current.ComparedAtUtc)throw new InvalidOperationException("Explicit current budget decision audit UTC is required.");
        var context=Review.Current.Context;var original=Review.Original();var query=Review.Query.Budget;
        var row=new APPROVAL_WORKFLOW{APPROVAL_WORKFLOW_ID=Id(),ENTITY_NAME="AFE",ENTITY_ID=Review.Query.AfeId,AMOUNT=original.BudgetAmount,
            STATUS=Decision==WorkOrderBudgetDecision.Approve?"APPROVED":"REJECTED",REQUESTED_BY=original.CreatedBy,REQUESTED_DATE=original.CreatedAtUtc,APPROVED_BY=context.ActorUserId,APPROVAL_DATE=utc,
            COMMENTS=Rationale,OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,FIELD_ID=query.FieldId,WORK_ORDER_ID=query.WorkOrderId,
            BOOK_ID=query.BookId,CURRENCY_CODE=original.CurrencyCode,ORIGINAL_AFE_REVISION=original.RecordedRevision,WORK_ORDER_DECISION_JSON=JsonSerializer.Serialize(this),
            ACTIVE_IND="Y",SOURCE="COMPANY_WORK_ORDER_BUDGET_DECISION",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=context.ActorUserId,ROW_CHANGED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};
        row.WORK_ORDER_DECISION_REVISION=Digest(row);return row;
    }
    public static string Digest(APPROVAL_WORKFLOW row){var copy=CompanyInventorySourceReview.Copy(row);copy.WORK_ORDER_DECISION_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(APPROVAL_WORKFLOW row){if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original independent budget decision/scope/amount/reviewer/body/audit changed.");}
    public static CompanyWorkOrderBudgetDecisionCommand Read(APPROVAL_WORKFLOW row)
    {
        if(row?.WORK_ORDER_DECISION_JSON is not {Length:>0 and <=MaximumBodyLength} body)throw new InvalidOperationException("Original bounded independent decision proof is unavailable.");
        try{var command=JsonSerializer.Deserialize<CompanyWorkOrderBudgetDecisionCommand>(body)??throw new InvalidOperationException("Original independent decision command is unavailable.");command.RequireRecorded(row);return command;}
        catch(JsonException error){throw new InvalidOperationException("Original independent decision proof is malformed.",error);}
    }
}
public sealed record CompanyWorkOrderBudgetDecisionReference(CompanyWorkOrderBudgetDecisionQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original budget decision AFE and intent reference are required.");Query.Require();}
}
public sealed record CompanyWorkOrderBudgetDecisionRecovery(CompanyWorkOrderBudgetDecisionCommand Command,APPROVAL_WORKFLOW Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderBudgetDecisionReference reference)
    {
        reference.Require();if(Command?.Review?.Current?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)
            throw new UnauthorizedAccessException("The private original budget decision belongs to another company/account/AFE/reference.");Command.RequireRecorded(Recorded);
    }
}
