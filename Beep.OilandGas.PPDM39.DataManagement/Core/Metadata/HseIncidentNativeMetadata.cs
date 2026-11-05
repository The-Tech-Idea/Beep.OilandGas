using Beep.OilandGas.PPDM39.Core.Metadata;
using TheTechIdea.Data.OilGas;

namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

/// <summary>Metadata for the private app-owned incident submission retained beside PPDM records.</summary>
public static class HseIncidentNativeMetadata
{
    public static Dictionary<string, PPDMTableMetadata> AddTo(Dictionary<string, PPDMTableMetadata> metadata)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        const string name = nameof(HSE_INCIDENT_REPORT_INTENT);
        if (metadata.ContainsKey(name)) throw new InvalidOperationException("Incident report intent metadata already has another owner.");
        metadata.Add(name, new PPDMTableMetadata
        {
            TableName = name,
            EntityTypeName = typeof(HSE_INCIDENT_REPORT_INTENT).FullName!,
            PrimaryKeyColumn = nameof(HSE_INCIDENT_REPORT_INTENT.HSE_INCIDENT_REPORT_INTENT_ID),
            Module = "PPDM_CORE",
            SubjectArea = "Health Safety & Environment",
            CommonColumns = ["ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY"],
            ForeignKeys = [new() { ForeignKeyColumn = nameof(HSE_INCIDENT_REPORT_INTENT.FIELD_ID), ReferencedTable = "FIELD", ReferencedPrimaryKey = "FIELD_ID" },
                new() { ForeignKeyColumn = nameof(HSE_INCIDENT_REPORT_INTENT.INCIDENT_ID), ReferencedTable = "HSE_INCIDENT", ReferencedPrimaryKey = "INCIDENT_ID" }]
        });
        const string rca = nameof(HSE_INCIDENT_RCA_INTENT);
        if (metadata.ContainsKey(rca)) throw new InvalidOperationException("Incident RCA intent metadata already has another owner.");
        metadata.Add(rca, new PPDMTableMetadata
        {
            TableName = rca, EntityTypeName = typeof(HSE_INCIDENT_RCA_INTENT).FullName!,
            PrimaryKeyColumn = nameof(HSE_INCIDENT_RCA_INTENT.HSE_INCIDENT_RCA_INTENT_ID), Module = "PPDM_CORE", SubjectArea = "Health Safety & Environment",
            CommonColumns = ["ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY"],
            ForeignKeys = [new() { ForeignKeyColumn = nameof(HSE_INCIDENT_RCA_INTENT.FIELD_ID), ReferencedTable = "FIELD", ReferencedPrimaryKey = "FIELD_ID" },
                new() { ForeignKeyColumn = nameof(HSE_INCIDENT_RCA_INTENT.INCIDENT_ID), ReferencedTable = "HSE_INCIDENT", ReferencedPrimaryKey = "INCIDENT_ID" }]
        });
        const string verification = nameof(HSE_CA_VERIFICATION);
        if (metadata.ContainsKey(verification)) throw new InvalidOperationException("Corrective-action verification metadata already has another owner.");
        metadata.Add(verification, new PPDMTableMetadata
        {
            TableName = verification, EntityTypeName = typeof(HSE_CA_VERIFICATION).FullName!,
            PrimaryKeyColumn = nameof(HSE_CA_VERIFICATION.HSE_CA_VERIFICATION_ID), Module = "PPDM_CORE", SubjectArea = "Health Safety & Environment",
            CommonColumns = ["ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY"],
            ForeignKeys = [new() { ForeignKeyColumn = nameof(HSE_CA_VERIFICATION.FIELD_ID), ReferencedTable = "FIELD", ReferencedPrimaryKey = "FIELD_ID" },
                new() { ForeignKeyColumn = nameof(HSE_CA_VERIFICATION.INCIDENT_ID), ReferencedTable = "HSE_INCIDENT", ReferencedPrimaryKey = "INCIDENT_ID" },
                new() { ForeignKeyColumn = nameof(HSE_CA_VERIFICATION.PROJECT_ID), ReferencedTable = "PROJECT", ReferencedPrimaryKey = "PROJECT_ID" }]
        });
        const string closure = nameof(HSE_INCIDENT_CLOSURE);
        if (metadata.ContainsKey(closure)) throw new InvalidOperationException("Incident closure metadata already has another owner.");
        metadata.Add(closure, new PPDMTableMetadata
        {
            TableName = closure, EntityTypeName = typeof(HSE_INCIDENT_CLOSURE).FullName!, PrimaryKeyColumn = nameof(HSE_INCIDENT_CLOSURE.HSE_INCIDENT_CLOSURE_ID), Module = "PPDM_CORE", SubjectArea = "Health Safety & Environment",
            CommonColumns = ["ACTIVE_IND", "ROW_CREATED_BY", "ROW_CREATED_DATE", "ROW_CHANGED_BY", "ROW_CHANGED_DATE", "PPDM_GUID", "ROW_EFFECTIVE_DATE", "ROW_EXPIRY_DATE", "ROW_QUALITY"],
            ForeignKeys = [new() { ForeignKeyColumn = "FIELD_ID", ReferencedTable = "FIELD", ReferencedPrimaryKey = "FIELD_ID" }, new() { ForeignKeyColumn = "INCIDENT_ID", ReferencedTable = "HSE_INCIDENT", ReferencedPrimaryKey = "INCIDENT_ID" }, new() { ForeignKeyColumn = "PROJECT_ID", ReferencedTable = "PROJECT", ReferencedPrimaryKey = "PROJECT_ID" }]
        });
        return metadata;
    }
}
