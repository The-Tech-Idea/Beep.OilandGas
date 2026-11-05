using Beep.OilandGas.Models.Data.WorkOrder;
namespace Beep.OilandGas.Models.Core.Interfaces;
public interface IInspectionService
{
    Task<InspectionCondition> AddRequirementAsync(string instanceId,AddWorkOrderInspectionRequirementRequest request,string userId);
    Task<InspectionCondition> RecordResultAsync(string instanceId,int condSeq,RecordInspectionResultRequest request,string userId);
    Task<bool> AllConditionsPassedAsync(string instanceId,string fieldId,IEnumerable<string> condTypes);
    Task<List<InspectionCondition>> GetChecklistAsync(string instanceId,string fieldId);
}
