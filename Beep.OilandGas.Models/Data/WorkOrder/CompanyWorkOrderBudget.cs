using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Field;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.WorkOrder;

public sealed record CompanyWorkOrderBudgetQuery(string WorkOrderId,string FieldId,string BookId)
{
    public void Require(){foreach(var value in new[]{WorkOrderId,FieldId,BookId})if(!LeaseSummaryValidator.Identifier(value) || value.Length>128)throw new ArgumentException("An explicit native work order, FIELD and company book are required.");}
}
public sealed record CompanyWorkOrderBudgetFacts(string AfeId,string Number,string Name,string BookId,string CurrencyCode,
    int CurrencyDecimals,decimal BudgetAmount,string Status,string CreatedBy,DateTime CreatedAtUtc,string RecordedRevision)
{
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? OriginalOrderRevision{get;init;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public string? OriginalBookRevision{get;init;}
    [JsonIgnore(Condition=JsonIgnoreCondition.WhenWritingNull)]public DateTime? EffectiveAtUtc{get;init;}
    public void Require()
    {
        if(new[]{AfeId,Number,Name,BookId,CreatedBy}.Any(value=>!LeaseSummaryValidator.Identifier(value)) || Name.Length>256 || Number.Length>128 ||
            CurrencyCode is not {Length:3} || CurrencyCode.Any(value=>value is <'A' or >'Z') || CurrencyDecimals is <0 or >8 || BudgetAmount<0 || decimal.Round(BudgetAmount,CurrencyDecimals)!=BudgetAmount ||
            Status!="DRAFT" || CreatedAtUtc.Kind!=DateTimeKind.Utc || CreatedAtUtc==default || !ProcessDefinitionRevision.IsValid(RecordedRevision))
            throw new InvalidOperationException("Complete original draft AFE identity, explicit currency/budget precision and producer audit are required.");
    }
}
/// <summary>One current native work-order scope, exact configured company book and complete original budget population.</summary>
public sealed record CompanyWorkOrderBudgetReview(FieldSelectionReview Context,AssetDatabaseScope BudgetDatabase,CompanyWorkOrderBudgetQuery Query,
    WorkOrderDetailModel Order,CompanyLedgerBookFacts Book,IReadOnlyList<CompanyWorkOrderBudgetFacts> Budgets,DateTime ComparedAtUtc)
{
    public const int MaximumBudgets=128;
    public void Require()
    {
        if(Context is null || BudgetDatabase is null || Query is null || Order is null || Book is null || Budgets is null)throw new InvalidOperationException("Current company/order/book/budget source is required.");
        FieldSelectionReviewValidator.RequireReview(Context,Context.Company,Context.ActorUserId);Query.Require();
        if(Context.FieldId!=Query.FieldId || Order.InstanceId!=Query.WorkOrderId || Order.FieldId!=Query.FieldId || Order.Source!="WORK_ORDER" ||
            Book.BookId!=Query.BookId || !ProcessDefinitionRevision.IsValid(Book.NativeRevision) || Book.CreatedAtUtc.Kind!=DateTimeKind.Utc ||
            !LeaseSummaryValidator.Identifier(BudgetDatabase.ConnectionName) || !LeaseSummaryValidator.Identifier(BudgetDatabase.Fingerprint) ||
            ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc==default || Budgets.Count>MaximumBudgets || Budgets.Any(row=>row is null) ||
            Budgets.Select(row=>row.AfeId).Distinct(StringComparer.Ordinal).Count()!=Budgets.Count)throw new InvalidOperationException("Original native company/order/book/currency and complete unique AFE membership differ.");
        CompanyLedgerBookReview.BookValues(Book.BookId,Book.Name,Book.FunctionalCurrency,Book.CurrencyDecimals,Book.RoundingPolicy,Book.CalendarId,Book.TimeZoneId);
        foreach(var row in Budgets){row.Require();if(row.BookId!=Book.BookId || row.CurrencyCode!=Book.FunctionalCurrency || row.CurrencyDecimals!=Book.CurrencyDecimals)throw new InvalidOperationException("Original budget book/currency differs; conversion is not inferred.");}
    }
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Context,BudgetDatabase,Query,Order=CompanyWorkOrderExecutionRequirements.OrderRevision(Order),Book,Budgets=Budgets.OrderBy(row=>row.AfeId,StringComparer.Ordinal)}));}
}
public sealed record CompanyWorkOrderBudgetCommand(CompanyWorkOrderBudgetReview Review,Guid IntentId,string AfeNumber,string AfeName,decimal BudgetAmount,DateTime EffectiveAtUtc,string Reason)
{
    public const int MaximumBodyLength=2*1024*1024;
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed order and company book are required.");Review.Require();
        if(IntentId==Guid.Empty || !LeaseSummaryValidator.Identifier(AfeNumber) || AfeNumber.Length>128 || !LeaseSummaryValidator.Identifier(AfeName) || AfeName.Length>256 ||
            BudgetAmount<0 || decimal.Round(BudgetAmount,Review.Book.CurrencyDecimals)!=BudgetAmount || EffectiveAtUtc==default || EffectiveAtUtc.Kind!=DateTimeKind.Utc ||
            string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl) || Review.Budgets.Count!=0 || JsonSerializer.Serialize(this).Length>MaximumBodyLength)
            throw new ArgumentException("Retain an explicit first work-order AFE number/name, budget in the exact configured currency precision, UTC effective date and rationale.");
    }
    public string AfeId()=>"wo-afe-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Context.Company,IntentId}));
    public void RequireCurrent(CompanyWorkOrderBudgetReview current){Require();current.Require();if(current.ComparedAtUtc<Review.ComparedAtUtc || current.SourceRevision()!=Review.SourceRevision())throw new InvalidOperationException("Original company/order/book or budget membership changed before budget creation.");}
    public AFE Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc)throw new InvalidOperationException("Explicit current budget producer audit UTC is required.");
        var row=new AFE{AFE_ID=AfeId(),AFE_NUMBER=AfeNumber,AFE_NAME=AfeName,FIELD_ID=Review.Query.FieldId,WORK_ORDER_ID=Review.Query.WorkOrderId,
            OWNER_TENANT_ID=Review.Context.Company.TenantId,OWNER_COMPANY_BA_ID=Review.Context.Company.BusinessAssociateId,BOOK_ID=Review.Book.BookId,
            CURRENCY_CODE=Review.Book.FunctionalCurrency,ESTIMATED_COST=BudgetAmount,STATUS="DRAFT",DESCRIPTION=Reason,ROW_EFFECTIVE_DATE=EffectiveAtUtc,
            ACTIVE_IND="Y",SOURCE="COMPANY_WORK_ORDER_BUDGET",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=Review.Context.ActorUserId,ROW_CHANGED_BY=Review.Context.ActorUserId,
            ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc,WORK_ORDER_BUDGET_JSON=JsonSerializer.Serialize(this)};
        row.WORK_ORDER_BUDGET_REVISION=Digest(row);return row;
    }
    public static string Digest(AFE row){var copy=CompanyInventorySourceReview.Copy(row);copy.WORK_ORDER_BUDGET_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(AFE row){if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original AFE budget/company/order/book/currency/body or producer audit changed.");}
    public static CompanyWorkOrderBudgetCommand Read(AFE row)
    {
        if(row?.WORK_ORDER_BUDGET_JSON is not {Length:>0 and <=MaximumBodyLength} body)throw new InvalidOperationException("Original bounded work-order AFE budget proof is unavailable.");
        try{var command=JsonSerializer.Deserialize<CompanyWorkOrderBudgetCommand>(body)??throw new InvalidOperationException("Original budget proof is unavailable.");command.RequireRecorded(row);return command;}
        catch(JsonException error){throw new InvalidOperationException("Original budget proof is malformed.",error);}
    }
    public static CompanyWorkOrderBudgetFacts Facts(AFE row)
    {
        var command=Read(row);var result=new CompanyWorkOrderBudgetFacts(row.AFE_ID,row.AFE_NUMBER,row.AFE_NAME,row.BOOK_ID!,row.CURRENCY_CODE!,command.Review.Book.CurrencyDecimals,
            row.ESTIMATED_COST!.Value,row.STATUS,row.ROW_CREATED_BY!,row.ROW_CREATED_DATE!.Value,row.WORK_ORDER_BUDGET_REVISION!){OriginalOrderRevision=CompanyWorkOrderExecutionRequirements.OrderRevision(command.Review.Order),OriginalBookRevision=command.Review.Book.NativeRevision,EffectiveAtUtc=command.EffectiveAtUtc};result.Require();return result;
    }
}
public sealed record CompanyWorkOrderBudgetReference(CompanyWorkOrderBudgetQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original budget order/book and intent reference are required.");Query.Require();}
}
public sealed record CompanyWorkOrderBudgetRecovery(CompanyWorkOrderBudgetCommand Command,AFE Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderBudgetReference reference)
    {
        reference.Require();if(Command?.Review?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Review.Query!=reference.Query || Command.IntentId!=reference.IntentId)
            throw new UnauthorizedAccessException("The private original budget belongs to another company/account/order/book/reference.");Command.RequireRecorded(Recorded);
    }
}
