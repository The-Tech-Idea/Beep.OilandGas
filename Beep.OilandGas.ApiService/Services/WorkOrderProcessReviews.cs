using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Processes;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.ApiService.Services;
public static class WorkOrderProcessReviews
{
    public static WorkOrderProcessReview Project(CompanyScope company,string actor,string field,string id,IReadOnlyList<ProcessInstance>? rows,IReadOnlyList<ProcessDefinition>? definitions)
    {
        if(string.IsNullOrWhiteSpace(actor) || string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(id) || rows is null || definitions is null ||
            rows.Any(x=>x is null || x.EntityId!=id || x.EntityType!="WORK_ORDER" || string.IsNullOrWhiteSpace(x.FieldId) || string.IsNullOrWhiteSpace(x.InstanceId) || string.IsNullOrWhiteSpace(x.ProcessId)) ||
            rows.Select(x=>x.InstanceId).Distinct(StringComparer.Ordinal).Count()!=rows.Count || definitions.Any(x=>x is null || string.IsNullOrWhiteSpace(x.ProcessId)) || definitions.Select(x=>x.ProcessId).Distinct(StringComparer.Ordinal).Count()!=definitions.Count)
            throw RefusalException.Conflict("The recorded work-order workflow relationships could not be confirmed.");
        var links=new List<WorkOrderProcessLink>();
        foreach(var row in rows.Where(x=>x.FieldId==field))
        {
            var definition=definitions.SingleOrDefault(x=>x.ProcessId==row.ProcessId);
            if(definition is null || definition.ProcessType!="WORK_ORDER" || definition.EntityType!="WORK_ORDER" || string.IsNullOrWhiteSpace(definition.ProcessName) || !Enum.IsDefined(row.Status))
                throw RefusalException.Conflict("A linked process has no confirmed work-order definition or recorded status.");
            links.Add(new(row.InstanceId,row.ProcessId,definition.ProcessName,row.CurrentState,row.CurrentStepId,row.Status.ToString()));
        }
        return new(company,actor,field,id,DateTimeOffset.UtcNow,links.OrderBy(x=>x.InstanceId,StringComparer.Ordinal).ToArray());
    }
}
