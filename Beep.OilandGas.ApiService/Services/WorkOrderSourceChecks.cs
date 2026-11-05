using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.ApiService.Services;
/// <summary>Original-order orchestration; authority is supplied by the existing FIELD access owner.</summary>
public static class WorkOrderSourceChecks
{
    public static async Task<WorkOrderDetailModel> RequireAsync(string field,string id,Func<Task<bool>> authorized,Func<Task<WorkOrderDetailModel?>> read,bool requireState=true)
    {
        if(string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(id))
            throw RefusalException.Invalid("Confirm the current FIELD and work-order identity before accessing its evidence.");
        if(!await authorized())throw RefusalException.Forbidden("Current FIELD access for this operation is required.");
        var source=await read();
        if(source is null)throw RefusalException.NotFound("The work order is unavailable in the selected FIELD.");
        if(source.InstanceId!=id || source.FieldId!=field || (string.IsNullOrWhiteSpace(source.WoSubType) && requireState) || (string.IsNullOrWhiteSpace(source.State) && (requireState || source.Source!="WORK_ORDER" || string.IsNullOrWhiteSpace(source.EvidenceUnavailableReason))))
            throw RefusalException.Conflict("The work-order source could not be confirmed for the original FIELD and identity.");
        if(!await authorized())throw RefusalException.Forbidden("FIELD authority changed while reading the work-order source.");
        return source;
    }
}
