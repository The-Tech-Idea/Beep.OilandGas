using Beep.OilandGas.Models.Data.WorkOrder;
using Beep.OilandGas.PPDM39.Core.Metadata;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

public static class ProductionInterventionNativeMetadata
{
    public static Dictionary<string, PPDMTableMetadata> AddTo(Dictionary<string, PPDMTableMetadata> metadata)
    {
        foreach (var type in ProductionInterventionNativeSchema.EntityTypes)
        {
            if (metadata.ContainsKey(type.Name)) throw new InvalidOperationException("The original intervention journal has more than one metadata owner.");
            metadata.Add(type.Name, new()
            {
                TableName = type.Name, EntityTypeName = type.FullName!, PrimaryKeyColumn = "INTENT_ID", Module = "PPDM_CORE", SubjectArea = "Well intervention decisions",
                CommonColumns = ["ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY"],
                ForeignKeys = type == typeof(WELL_INTERVENTION_INTENT)
                    ? [new() { ForeignKeyColumn = "UWI", ReferencedTable = "WELL", ReferencedPrimaryKey = "UWI" }, new() { ForeignKeyColumn = "FIELD_ID", ReferencedTable = "FIELD", ReferencedPrimaryKey = "FIELD_ID" }]
                    : [new() { ForeignKeyColumn = "INTENT_ID", ReferencedTable = "WELL_INTERVENTION_INTENT", ReferencedPrimaryKey = "INTENT_ID" }]
            });
        }
        return metadata;
    }
}
