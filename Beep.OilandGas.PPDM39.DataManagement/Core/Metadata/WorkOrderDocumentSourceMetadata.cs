using Beep.OilandGas.PPDM39.Core.Metadata;
namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;
/// <summary>Complete existing native composite relationships; no table or physical constraint creation.</summary>
public static class WorkOrderDocumentSourceMetadata
{
    public static Dictionary<string,PPDMTableMetadata> Complete(Dictionary<string,PPDMTableMetadata> metadata)
    {
        var pending=new List<(PPDMTableMetadata Definition,PPDMForeignKey Relationship)>();
        foreach(var table in new[]{"WORK_ORDER_COMPONENT","RM_FILE_CONTENT","RM_CREATOR"})
        {
            if(!metadata.TryGetValue(table,out var definition))continue;
            if(!metadata.TryGetValue("RM_INFORMATION_ITEM",out var parent))throw new InvalidOperationException("Native document relationship parent metadata is unavailable.");
            if(!parent.PrimaryKeyColumn.Split(',').Select(value=>value.Trim()).Order(StringComparer.Ordinal).SequenceEqual(new[]{"INFORMATION_ITEM_ID","INFO_ITEM_SUBTYPE"}.Order(StringComparer.Ordinal),StringComparer.Ordinal))throw new InvalidOperationException("Native document information-item composite key differs.");
            if(definition.ForeignKeys.Count(value=>value.ForeignKeyColumn=="INFO_ITEM_SUBTYPE" && value.ReferencedTable=="RM_INFORMATION_ITEM" && value.ReferencedPrimaryKey=="INFO_ITEM_SUBTYPE")!=1)throw new InvalidOperationException("Native document subtype relationship is unavailable or ambiguous.");
            var existing=definition.ForeignKeys.Where(value=>value.ForeignKeyColumn=="INFORMATION_ITEM_ID").ToArray();
            if(existing.Length>1 || existing.Length==1 && (existing[0].ReferencedTable!="RM_INFORMATION_ITEM" || existing[0].ReferencedPrimaryKey!="INFORMATION_ITEM_ID"))throw new InvalidOperationException("Native document information-item relationship has another owner or target.");
            if(existing.Length==0)pending.Add((definition,new(){ForeignKeyColumn="INFORMATION_ITEM_ID",ReferencedTable="RM_INFORMATION_ITEM",ReferencedPrimaryKey="INFORMATION_ITEM_ID",RelationshipType="OneToMany"}));
        }
        foreach(var (definition,relationship) in pending)definition.ForeignKeys.Add(relationship);return metadata;
    }
}
