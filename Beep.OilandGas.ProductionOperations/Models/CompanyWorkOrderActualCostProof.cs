using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.ProductionOperations.Models;

/// <summary>Dispatch the original proof for the existing cost ledger; source kinds remain explicit.</summary>
public static class CompanyWorkOrderActualCostProof
{
    public static CompanyWorkOrderBudgetReview Budget(ACCOUNTING_COST row)=>row.SOURCE_KIND switch
    {
        "WORK_ORDER_LABOR_USAGE"=>CompanyWorkOrderLaborCostCommand.Read(row).Review.Budget.Current,
        "WORK_ORDER_MATERIAL_USAGE"=>CompanyWorkOrderMaterialCostCommand.Read(row).Review.Budget.Current,
        _=>throw new InvalidOperationException("Original recorded actual-cost source kind is unsupported; a legacy record cannot be silently omitted.")
    };
    public static CompanyWorkOrderActualCostFacts Facts(ACCOUNTING_COST row)
    {
        _=Budget(row);var result=new CompanyWorkOrderActualCostFacts(row.ACCOUNTING_COST_ID,row.AFE_ID,row.SOURCE_KIND!,row.SOURCE_ID!,row.SOURCE_REVISION!,row.AMOUNT,row.CURRENCY_CODE!,row.COST_DATE!.Value,row.WORK_ORDER_COST_REVISION!);result.Require();return result;
    }
}
