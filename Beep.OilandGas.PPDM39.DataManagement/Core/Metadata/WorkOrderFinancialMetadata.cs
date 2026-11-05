using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.PPDM39.Core.Metadata;
namespace Beep.OilandGas.PPDM39.DataManagement.Core.Metadata;

/// <summary>Canonical ownership for the existing financial tables, preserving recognized legacy metadata relationships.</summary>
public static class WorkOrderFinancialMetadata
{
    public static Dictionary<string,PPDMTableMetadata> AddTo(Dictionary<string,PPDMTableMetadata> metadata)
    {
        const string entityNamespace="Beep.OilandGas.Models.Data.ProductionAccounting.";
        var entries=new[]{(Name:"AFE",Key:"AFE_ID"),(Name:"AFE_LINE_ITEM",Key:"AFE_LINE_ITEM_ID"),(Name:"ACCOUNTING_COST",Key:"ACCOUNTING_COST_ID"),(Name:"APPROVAL_WORKFLOW",Key:"APPROVAL_WORKFLOW_ID")};
        // Validate all collisions before changing any original metadata entry.
        foreach(var (name,key) in entries)
            if(metadata.TryGetValue(name,out var existing) && (existing.PrimaryKeyColumn!=key ||
                existing.EntityTypeName!=name && existing.EntityTypeName!=entityNamespace+name ||
                !string.Equals(existing.TableName,name,StringComparison.OrdinalIgnoreCase) || existing.Module is not ("PRODUCTION" or "Accounting")))
                throw new InvalidOperationException("Existing work-order financial metadata has a different entity/key/module owner.");
        foreach(var (name,key) in entries)
        {
            if(metadata.TryGetValue(name,out var existing)){existing.EntityTypeName=entityNamespace+name;existing.Module="PRODUCTION";continue;}
            metadata.Add(name,new(){TableName=name,EntityTypeName=entityNamespace+name,PrimaryKeyColumn=key,Module="PRODUCTION",SubjectArea="Authorization for Expenditure",
                CommonColumns=["ACTIVE_IND","ROW_CREATED_BY","ROW_CREATED_DATE","ROW_CHANGED_BY","ROW_CHANGED_DATE","PPDM_GUID","ROW_EFFECTIVE_DATE","ROW_EXPIRY_DATE","ROW_QUALITY"],
                ForeignKeys=name=="AFE_LINE_ITEM"?[new(){ForeignKeyColumn="AFE_ID",ReferencedTable="AFE",ReferencedPrimaryKey="AFE_ID"}]:[]});
        }
        return metadata;
    }
}
