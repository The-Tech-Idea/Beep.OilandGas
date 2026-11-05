using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;
namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

public static class WorkOrderMaterialMetadata
{
    public static Dictionary<string, PPDMTableMetadata> AddTo(Dictionary<string, PPDMTableMetadata> metadata)
    {
        var extensions = new[] { (Type: typeof(WORK_ORDER_MATERIAL_ALLOCATION), Key: "ALLOCATION_ID"),
            (Type: typeof(WORK_ORDER_MATERIAL_RELEASE), Key: "RELEASE_ID"), (Type: typeof(WORK_ORDER_MATERIAL_USAGE), Key: "USAGE_ID") };
        if (extensions.Any(entry => metadata.ContainsKey(entry.Type.Name)))
            throw new InvalidOperationException("Work-order material metadata already has another owner.");
        foreach (var (type, key) in extensions)
        {
            var entry = new PPDMTableMetadata { TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = key,
                Module = "PRODUCTION", SubjectArea = "Work Order Materials",
                CommonColumns = ["ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID"],
                ForeignKeys = [new() { ForeignKeyColumn = "INVENTORY_ITEM_ID", ReferencedTable = "INVENTORY_ITEM", ReferencedPrimaryKey = "INVENTORY_ITEM_ID" }] };
            if (type != typeof(WORK_ORDER_MATERIAL_ALLOCATION))
                entry.ForeignKeys.Add(new() { ForeignKeyColumn = "ALLOCATION_ID", ReferencedTable = "WORK_ORDER_MATERIAL_ALLOCATION", ReferencedPrimaryKey = "ALLOCATION_ID" });
            metadata.Add(type.Name, entry);
        }
        return metadata;
    }
}
