using Beep.OilandGas.PPDM39.Core.Metadata;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

/// <summary>Canonical ownership for the existing financial tables, preserving recognized legacy metadata relationships.</summary>
public static class WorkOrderFinancialMetadata
{
    public static Dictionary<string,PPDMTableMetadata> AddTo(Dictionary<string,PPDMTableMetadata> metadata)
    {
        var entries=new[]{(Type:typeof(AFE),Key:"AFE_ID"),(Type:typeof(AFE_LINE_ITEM),Key:"AFE_LINE_ITEM_ID"),(Type:typeof(ACCOUNTING_COST),Key:"ACCOUNTING_COST_ID"),(Type:typeof(APPROVAL_WORKFLOW),Key:"APPROVAL_WORKFLOW_ID")};
        // Validate all collisions before changing any original metadata entry.
        foreach(var (type,key) in entries)
            if(metadata.TryGetValue(type.Name,out var existing) && (existing.PrimaryKeyColumn!=key ||
                existing.EntityTypeName!=type.Name && existing.EntityTypeName!=type.FullName ||
                !string.Equals(existing.TableName,type.Name,StringComparison.OrdinalIgnoreCase) || existing.Module is not ("PRODUCTION" or "Accounting")))
                throw new InvalidOperationException("Existing work-order financial metadata has a different entity/key/module owner.");
        foreach(var (type,key) in entries)
        {
            if(metadata.TryGetValue(type.Name,out var existing)){existing.EntityTypeName=type.FullName!;existing.Module="PRODUCTION";continue;}
            metadata.Add(type.Name,new(){TableName=type.Name,EntityTypeName=type.FullName!,PrimaryKeyColumn=key,Module="PRODUCTION",SubjectArea="Authorization for Expenditure",
                CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"],
                ForeignKeys=type==typeof(AFE_LINE_ITEM)?[new(){ForeignKeyColumn="AFE_ID",ReferencedTable="AFE",ReferencedPrimaryKey="AFE_ID"}]:[]});
        }
        return metadata;
    }
}
