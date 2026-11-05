using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Data.Company;
namespace Beep.OilandGas.Models.Data.WorkOrder;
public static class WorkOrderSchema
{
    public static IReadOnlyList<Type> EntityTypes { get; } = Array.AsReadOnly(new[]{typeof(WORK_ORDER_INSPECTION),typeof(WORK_ORDER_EXECUTION_PERMIT),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_PERSONNEL_ASSIGNMENT),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_PERSONNEL_RELEASE),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_PERSONNEL_POLICY),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_MAINTENANCE_RETURN_POLICY),typeof(Beep.OilandGas.Models.Data.WorkOrder.COMPANY_PERSONNEL_IDENTITY),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_START),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_EXECUTION_SUBMISSION),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_LABOR_USAGE),typeof(Beep.OilandGas.Models.Data.WorkOrder.WORK_ORDER_ASSET_RETURN)});
}
