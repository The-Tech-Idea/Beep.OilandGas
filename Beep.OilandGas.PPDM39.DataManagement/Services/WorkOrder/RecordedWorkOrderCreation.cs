using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.PPDM39.Models;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
internal static class RecordedWorkOrderCreation
{
    public static (WORK_ORDER Header,WORK_ORDER_COMPONENT Component) Prepare(CreateWorkOrderRequest request,string actor,string id,DateTime now)
    {
        if(request is null || string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(id) || now.Kind!=DateTimeKind.Utc || now==default ||
           string.IsNullOrWhiteSpace(request.WorkOrderNumber) || string.IsNullOrWhiteSpace(request.InstanceName) || string.IsNullOrWhiteSpace(request.FieldId) || string.IsNullOrWhiteSpace(request.Jurisdiction) || string.IsNullOrWhiteSpace(request.WoSubType))
            throw RefusalException.Invalid("Confirm order number, title, FIELD, type, jurisdiction and authenticated actor before creating a recorded work order.");
        if(request.ProposedStart.HasValue || request.Duration.HasValue)
            throw RefusalException.Invalid("Scheduling requires its recorded planning workflow; it cannot be established by header creation.");
        var instructions=request.RecordedInstructions;
        return (new(){WORK_ORDER_ID=id,WORK_ORDER_NUMBER=request.WorkOrderNumber,WORK_ORDER_TYPE=request.WoSubType,INSTRUCTIONS=instructions,REQUEST_DATE=now,
            ACTIVE_IND="Y",SOURCE="OILGAS_WORK_ORDER",PPDM_GUID=Guid.NewGuid().ToString(),ROW_CREATED_BY=actor,ROW_CREATED_DATE=now},
            new(){WORK_ORDER_ID=id,COMPONENT_ID=Guid.NewGuid().ToString("N"),FIELD_ID=request.FieldId,UWI=string.IsNullOrWhiteSpace(request.WellId)?null:request.WellId,EQUIPMENT_ID=string.IsNullOrWhiteSpace(request.EquipmentId)?null:request.EquipmentId,
                ACTIVE_IND="Y",SOURCE="OILGAS_WORK_ORDER",PPDM_GUID=Guid.NewGuid().ToString(),ROW_CREATED_BY=actor,ROW_CREATED_DATE=now});
    }
    public static void RequireWellReference(string well,IReadOnlyList<object>? rows)
    {
        if(string.IsNullOrWhiteSpace(well) || rows is null || rows.Count!=1 || rows[0] is not WELL recorded || recorded.UWI!=well || recorded.ACTIVE_IND!="Y")
            throw RefusalException.Conflict("The requested recorded well is unavailable or ambiguous in this company datasource.");
    }
    public static void RequireReadback(CreateWorkOrderRequest request,string id,WorkOrderSummary? result)
    {
        if(result is null || result.InstanceId!=id || result.InstanceName!=request.WorkOrderNumber || result.FieldId!=request.FieldId || result.WoSubType!=request.WoSubType ||
           result.Instructions!=request.RecordedInstructions || result.Source!="WORK_ORDER" || result.State!=string.Empty || string.IsNullOrWhiteSpace(result.EvidenceUnavailableReason))
            throw RefusalException.Conflict("The recorded header and FIELD relationship did not confirm creation. Review its original outcome before retrying.");
        RecordedWorkOrderComponentChecks.RequireCreated(request,result);
    }
}
