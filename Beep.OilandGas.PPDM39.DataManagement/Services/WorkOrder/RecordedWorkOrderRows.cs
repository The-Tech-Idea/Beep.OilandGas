using Beep.OilandGas.Models.Core.Refusals;
using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Models;
using TheTechIdea.Data.OilGas;
using System.Reflection;
using System.Text.Json;
namespace Beep.OilandGas.PPDM39.DataManagement.Services.WorkOrder;
internal static class RecordedWorkOrderRows
{
    public static List<WorkOrderSummary> Project(string field,IReadOnlyList<object> components,IReadOnlyList<object> orders)
    {
        ValidateComponents(field,components);
        if(orders is null)throw RefusalException.Conflict("Recorded work-order headers are unavailable.");
        var links=components.Cast<WORK_ORDER_COMPONENT>().ToArray();
        if(links.Select(x=>(x.WORK_ORDER_ID,x.COMPONENT_ID)).Distinct().Count()!=links.Length ||
           orders.Any(x=>x is not WORK_ORDER row || row.ACTIVE_IND!="Y" || string.IsNullOrWhiteSpace(row.WORK_ORDER_ID) || !links.Any(c=>c.WORK_ORDER_ID==row.WORK_ORDER_ID)) ||
           orders.Cast<WORK_ORDER>().Select(x=>x.WORK_ORDER_ID).Distinct(StringComparer.Ordinal).Count()!=orders.Count ||
           links.Any(link=>!orders.Cast<WORK_ORDER>().Any(row=>row.WORK_ORDER_ID==link.WORK_ORDER_ID)))
            throw RefusalException.Conflict("The recorded work-order headers or component identities are inconsistent.");
        return orders.Cast<WORK_ORDER>().OrderBy(x=>x.WORK_ORDER_ID,StringComparer.Ordinal).Select(row=>new WorkOrderSummary{
            InstanceId=row.WORK_ORDER_ID,InstanceName=row.WORK_ORDER_NUMBER ?? string.Empty,FieldId=field,
            WoSubType=row.WORK_ORDER_TYPE ?? string.Empty,State=string.Empty,Source="WORK_ORDER",
            Components=links.Where(c=>c.WORK_ORDER_ID==row.WORK_ORDER_ID).Select(Component).ToList(),
            RequestDate=row.REQUEST_DATE,DueDate=row.DUE_DATE,ActualEnd=row.COMPLETE_DATE,Instructions=row.INSTRUCTIONS,
            EvidenceUnavailableReason="Workflow state, approved scheduling, inspection, permits, costs and supervisor acceptance are not established by the recorded WORK_ORDER header."
        }).ToList();
    }
    private static WorkOrderComponentReference Component(WORK_ORDER_COMPONENT row)
    {
        static string? Present(string? value)=>string.IsNullOrWhiteSpace(value)?null:value;
        var properties=typeof(WORK_ORDER_COMPONENT).GetProperties(BindingFlags.Public|BindingFlags.Instance|BindingFlags.DeclaredOnly).Where(property=>property.CanRead && property.CanWrite).OrderBy(property=>property.Name,StringComparer.Ordinal).ToArray();
        var known=new HashSet<string>(StringComparer.Ordinal){"WORK_ORDER_ID","COMPONENT_ID","FIELD_ID","UWI","EQUIPMENT_ID","FACILITY_ID","INFORMATION_ITEM_ID"};
        var unresolved=properties.Where(property=>property.Name.EndsWith("_ID",StringComparison.Ordinal) && !known.Contains(property.Name) && property.PropertyType==typeof(string) && !string.IsNullOrWhiteSpace((string?)property.GetValue(row))).Select(property=>property.Name).ToArray();
        return new(row.WORK_ORDER_ID,row.COMPONENT_ID,row.FIELD_ID,row.UWI??string.Empty,row.EQUIPMENT_ID??string.Empty){FacilityId=Present(row.FACILITY_ID),FacilityType=Present(row.FACILITY_TYPE),InformationItemId=Present(row.INFORMATION_ITEM_ID),InformationItemSubtype=Present(row.INFO_ITEM_SUBTYPE),ComponentType=Present(row.COMPONENT_TYPE),NativeRevision=ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(properties.Select(property=>new object?[]{property.Name,property.GetValue(row)}))),UnresolvedReferenceKinds=unresolved.Length==0?null:unresolved};
    }
    public static void ValidateComponents(string field,IReadOnlyList<object> components)
    {
        if(string.IsNullOrWhiteSpace(field) || components is null ||
            components.Any(x=>x is not WORK_ORDER_COMPONENT c || c.FIELD_ID!=field || string.IsNullOrWhiteSpace(c.WORK_ORDER_ID) || string.IsNullOrWhiteSpace(c.COMPONENT_ID) || c.ACTIVE_IND!="Y"))
            throw RefusalException.Conflict("The recorded work-order FIELD relationships could not be confirmed.");
        var links=components.Cast<WORK_ORDER_COMPONENT>().ToArray();
        if(links.Select(x=>(x.WORK_ORDER_ID,x.COMPONENT_ID)).Distinct().Count()!=links.Length)
            throw RefusalException.Conflict("Recorded work-order component identities are duplicated.");
    }
}
