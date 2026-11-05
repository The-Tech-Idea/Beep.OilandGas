using Beep.OilandGas.Models.Data.WorkOrder;
namespace TheTechIdea.Data.OilGas;
/// <summary>Recorded maintenance source captured with the existing process action review; not completion evidence.</summary>
public sealed record WorkOrderSourceReview(string WorkOrderId,string FieldId,string RecordedNumber,string OrderType,
    DateTime? RequestDate,DateTime? DueDate,string? Instructions,IReadOnlyList<WorkOrderComponentReference> Components,
    IReadOnlyList<InspectionCondition> Inspections,DateTimeOffset RetrievedAtUtc)
{
    public string Route=>"/ppdm39/workorder/"+Uri.EscapeDataString(WorkOrderId);
    public void Require(string field,string order)
    {
        if(string.IsNullOrWhiteSpace(field) || string.IsNullOrWhiteSpace(order) || WorkOrderId!=order || FieldId!=field || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero || Components is null ||
            !WorkOrderParentAccess.Fields(order,Components).Contains(field,StringComparer.Ordinal))
            throw new InvalidOperationException("The maintenance source review does not match the original FIELD and order.");
        WorkOrderInspectionRules.RequireProjected(field,order,Inspections);
    }
}
