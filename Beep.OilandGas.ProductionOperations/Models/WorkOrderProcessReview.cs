using Beep.OilandGas.Models.Data.AccessControl;
namespace Beep.OilandGas.ProductionOperations.Models;
public sealed record WorkOrderProcessLink(string InstanceId,string ProcessId,string ProcessName,string State,string StepId,string Status)
{
    public string Route=>"/ppdm39/process/instances/"+Uri.EscapeDataString(InstanceId);
}
public sealed record WorkOrderProcessReview(CompanyScope Company,string Actor,string FieldId,string WorkOrderId,DateTimeOffset RetrievedAtUtc,IReadOnlyList<WorkOrderProcessLink> Links);
