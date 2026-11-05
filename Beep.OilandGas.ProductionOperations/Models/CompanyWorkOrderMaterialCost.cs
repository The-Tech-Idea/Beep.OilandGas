using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Lease;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.WorkOrder;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.ProductionOperations.Models;

public sealed record CompanyWorkOrderMaterialCostQuery(CompanyWorkOrderBudgetDecisionQuery Budget,string UsageId)
{
    public void Require(){if(Budget is null || !LeaseSummaryValidator.Identifier(UsageId) || UsageId.Length>128)throw new ArgumentException("Original budget/order/book and material source identity are required.");Budget.Require();}
}
public sealed record CompanyWorkOrderMaterialCostReview(CompanyWorkOrderBudgetDecisionStatus Budget,CompanyWorkOrderMaterialCostQuery Query,
    CompanyWorkOrderMaterialCostSource Material,IReadOnlyList<CompanyWorkOrderActualCostFacts> Costs,DateTime ComparedAtUtc)
{
    public const int MaximumCosts=4096;
    public void Require()
    {
        if(Budget is null || Query is null || Material is null || Costs is null)throw new InvalidOperationException("Current approved budget and actual recorded material/cost population are required.");
        Budget.Require();Query.Require();Material.Require();var context=Budget.Current.Context;
        if(Query.Budget!=Budget.Query || Material.Company!=context.Company || Material.Database!=Budget.Current.BudgetDatabase || Material.WorkOrderId!=Query.Budget.Budget.WorkOrderId || Material.FieldId!=Query.Budget.Budget.FieldId ||
            Material.UsageId!=Query.UsageId || Costs.Count>MaximumCosts || Costs.Any(row=>row is null) || Costs.Select(row=>row.Id).Distinct(StringComparer.Ordinal).Count()!=Costs.Count ||
            ComparedAtUtc.Kind!=DateTimeKind.Utc || ComparedAtUtc<Budget.ComparedAtUtc || ComparedAtUtc<Material.RecordedAtUtc)
            throw new InvalidOperationException("Original current company/order/PRODUCTION material and complete cost membership differ.");
        foreach(var row in Costs){row.Require();if(row.AfeId!=Query.Budget.AfeId || row.CurrencyCode!=Budget.Current.Book.FunctionalCurrency)throw new InvalidOperationException("Original cost AFE/currency differs; conversion is not inferred.");}
    }
    public decimal RecordedTotal(){Require();try{return Costs.Sum(row=>row.Amount);}catch(OverflowException error){throw new InvalidOperationException("Complete actual cost total exceeds supported currency capacity.",error);}}
    public string SourceRevision(){Require();return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Budget=Budget.SourceRevision(),Query,Material,Costs=Costs.OrderBy(row=>row.Id,StringComparer.Ordinal)}));}
}
public sealed record CompanyWorkOrderMaterialCostCommand(CompanyWorkOrderMaterialCostReview Review,Guid IntentId,[property:JsonRequired]decimal Rate,[property:JsonRequired]string RateUnit,
    [property:JsonRequired]string CurrencyCode,[property:JsonRequired]DateTime ValuedAtUtc,string CostCategory,string Reason)
{
    public const int MaximumBodyLength=4*1024*1024;
    public void Require()
    {
        if(Review is null)throw new ArgumentException("Original reviewed material and independently approved budget are required.");Review.Require();var book=Review.Budget.Current.Book;
        if(!Review.Budget.IsApprovedForCurrentScope || IntentId==Guid.Empty || Rate<0 || RateUnit!=Review.Material.Unit || CurrencyCode!=book.FunctionalCurrency ||
            ValuedAtUtc.Kind!=DateTimeKind.Utc || ValuedAtUtc<Review.Material.UsedAtUtc || ValuedAtUtc>Review.ComparedAtUtc ||
            Review.Budget.Original().EffectiveAtUtc is not {} effective || effective.Kind!=DateTimeKind.Utc || ValuedAtUtc<effective ||
            !LeaseSummaryValidator.Identifier(CostCategory) || CostCategory.Length>128 || string.IsNullOrWhiteSpace(Reason) || Reason.Length>4096 || Reason.Any(char.IsControl) ||
            Review.Costs.Any(row=>row.SourceKind=="WORK_ORDER_MATERIAL_USAGE" && row.SourceId==Review.Material.UsageId))
            throw new ArgumentException("Retain an explicit rate in the exact recorded material unit/currency and actual UTC valuation date within approved scope; already costed material cannot be duplicated.");
        if(Amount()>Review.Budget.Original().BudgetAmount-Review.RecordedTotal())throw new InvalidOperationException("Actual material cost exceeds the remaining approved budget; review a controlled budget amendment before recording it.");
        if(JsonSerializer.Serialize(this).Length>MaximumBodyLength)throw new InvalidOperationException("Original actual-cost command exceeds its retained capacity.");
    }
    public decimal Amount()
    {
        Review.Require();if(Rate<0 || Review.Material.Quantity>1 && Rate>decimal.MaxValue/Review.Material.Quantity)throw new ArgumentException("Explicit nonnegative rate exceeds supported source quantity/currency capacity.");
        var book=Review.Budget.Current.Book;return decimal.Round(Review.Material.Quantity*Rate,book.CurrencyDecimals,book.RoundingPolicy=="TO_EVEN"?MidpointRounding.ToEven:MidpointRounding.AwayFromZero);
    }
    public string Id()=>"wo-cost-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Review.Budget.Current.Context.Company,IntentId}));
    public CompanyWorkOrderMaterialCostReference Reference()=>new(Review.Query,IntentId);
    public void RequireCurrent(CompanyWorkOrderMaterialCostReview current){Require();current.Require();if(current.ComparedAtUtc<Review.ComparedAtUtc || current.SourceRevision()!=Review.SourceRevision())throw new InvalidOperationException("Original material/approval/work scope/book or complete actual costs changed before valuation.");}
    public ACCOUNTING_COST Prepare(DateTime utc)
    {
        Require();if(utc.Kind!=DateTimeKind.Utc || utc<Review.ComparedAtUtc)throw new InvalidOperationException("Explicit current actual-cost audit UTC is required.");var context=Review.Budget.Current.Context;
        var row=new ACCOUNTING_COST{ACCOUNTING_COST_ID=Id(),AFE_ID=Review.Query.Budget.AfeId,FIELD_ID=Review.Material.FieldId,WORK_ORDER_ID=Review.Material.WorkOrderId,
            OWNER_TENANT_ID=context.Company.TenantId,OWNER_COMPANY_BA_ID=context.Company.BusinessAssociateId,BOOK_ID=Review.Budget.Current.Book.BookId,CURRENCY_CODE=CurrencyCode,
            COST_TYPE="MATERIAL",COST_CATEGORY=CostCategory,AMOUNT=Amount(),COST_DATE=ValuedAtUtc,DESCRIPTION=Reason,SOURCE_KIND="WORK_ORDER_MATERIAL_USAGE",SOURCE_ID=Review.Material.UsageId,
            SOURCE_REVISION=Review.Material.Revision,SOURCE_QUANTITY=Review.Material.Quantity,SOURCE_UNIT=Review.Material.Unit,UNIT_RATE=Rate,
            WORK_ORDER_COST_JSON=JsonSerializer.Serialize(this),ACTIVE_IND="Y",SOURCE="COMPANY_WORK_ORDER_MATERIAL_COST",PPDM_GUID=IntentId.ToString("D"),ROW_CREATED_BY=context.ActorUserId,
            ROW_CHANGED_BY=context.ActorUserId,ROW_CREATED_DATE=utc,ROW_CHANGED_DATE=utc};row.WORK_ORDER_COST_REVISION=Digest(row);return row;
    }
    public static string Digest(ACCOUNTING_COST row){var copy=CompanyInventorySourceReview.Copy(row);copy.WORK_ORDER_COST_REVISION=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public void RequireRecorded(ACCOUNTING_COST row){if(row?.ROW_CREATED_DATE is not {} utc || JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(Prepare(utc)))throw new InvalidOperationException("Original actual material cost/source/amount/currency/body/audit differs.");}
    public static CompanyWorkOrderMaterialCostCommand Read(ACCOUNTING_COST row)
    {
        if(row?.WORK_ORDER_COST_JSON is not {Length:>0 and <=MaximumBodyLength} body)throw new InvalidOperationException("Original bounded actual-cost proof is unavailable.");
        try{var command=JsonSerializer.Deserialize<CompanyWorkOrderMaterialCostCommand>(body)??throw new InvalidOperationException("Original actual-cost command is unavailable.");command.RequireRecorded(row);return command;}
        catch(JsonException error){throw new InvalidOperationException("Original actual-cost proof is malformed.",error);}
    }
    public static CompanyWorkOrderActualCostFacts Facts(ACCOUNTING_COST row){_=Read(row);var result=new CompanyWorkOrderActualCostFacts(row.ACCOUNTING_COST_ID,row.AFE_ID,row.SOURCE_KIND!,row.SOURCE_ID!,row.SOURCE_REVISION!,row.AMOUNT,row.CURRENCY_CODE!,row.COST_DATE!.Value,row.WORK_ORDER_COST_REVISION!);result.Require();return result;}
}
public sealed record CompanyWorkOrderMaterialCostReference(CompanyWorkOrderMaterialCostQuery Query,Guid IntentId)
{
    public void Require(){if(Query is null || IntentId==Guid.Empty)throw new ArgumentException("Original material cost source and intent reference are required.");Query.Require();}
}
public sealed record CompanyWorkOrderMaterialCostRecovery(CompanyWorkOrderMaterialCostCommand Command,ACCOUNTING_COST Recorded)
{
    public void Require(CompanyScope company,string actor,CompanyWorkOrderMaterialCostReference reference){reference.Require();if(Command?.Review?.Budget?.Current?.Context is not {} context || context.Company!=company || context.ActorUserId!=actor || Command.Reference()!=reference)throw new UnauthorizedAccessException("Original private actual cost belongs to another company/account/source/reference.");Command.RequireRecorded(Recorded);}
}
