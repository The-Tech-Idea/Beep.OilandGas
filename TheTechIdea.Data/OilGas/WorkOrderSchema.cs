namespace Beep.OilandGas.Models.Data.WorkOrder;
public static class WorkOrderSchema
{
    public static IReadOnlyList<Type> EntityTypes { get; } = Array.AsReadOnly(new[]{typeof(WORK_ORDER_INSPECTION),typeof(WORK_ORDER_EXECUTION_PERMIT),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_ASSIGNMENT),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_RELEASE),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_PERSONNEL_POLICY),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_MAINTENANCE_RETURN_POLICY),typeof(TheTechIdea.Data.OilGas.COMPANY_PERSONNEL_IDENTITY),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_EXECUTION_START),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_EXECUTION_SUBMISSION),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_LABOR_USAGE),typeof(TheTechIdea.Data.OilGas.WORK_ORDER_ASSET_RETURN)});
}
