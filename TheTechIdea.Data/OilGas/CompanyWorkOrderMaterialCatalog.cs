using Beep.OilandGas.Models.Data.ProductionAccounting;
using System.Text.Json;
namespace TheTechIdea.Data.OilGas;
/// <summary>Current owned item choices for one verified assigned maintenance task; recorded stock is not availability.</summary>
public sealed record CompanyWorkOrderMaterialCatalog(CompanyMaintenanceAssignedStageReview Stage,IReadOnlyList<INVENTORY_ITEM> Items,DateTime ComparedAtUtc,string Revision)
{
    public const int MaximumItems=4096;
    public static CompanyWorkOrderMaterialCatalog Create(CompanyMaintenanceAssignedStageReview stage,IReadOnlyList<INVENTORY_ITEM> items,DateTime utc)
    {
        if(stage is null)throw new InvalidOperationException("Original assigned task evidence is required.");stage.Require(stage.Context.Company,stage.Context.ActorUserId,stage.Task);
        if(items is null || items.Count>MaximumItems || items.Any(row=>row is null) || utc==default || utc.Kind!=DateTimeKind.Utc)throw new InvalidOperationException("Complete bounded company inventory choices and current UTC are required; no choices are truncated.");
        CompanyInventorySourceReview.RequireRows(stage.Context.Company,items);if(items.Any(row=>row.ACTIVE_IND!="Y"))throw new InvalidOperationException("Only currently recorded active company items belong to this catalog.");
        var rows=items.OrderBy(row=>row.INVENTORY_ITEM_ID,StringComparer.Ordinal).Select(CompanyInventorySourceReview.Copy).ToArray();return new(CompanyInventorySourceReview.Copy(stage),rows,utc,ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Stage=stage,Items=rows,ComparedAtUtc=utc})));
    }
    public void Require(CompanyScope company,string actor,CompanyMaintenanceReceivingQuery task)
    {
        if(Stage is null || Items is null)throw new InvalidOperationException("Complete original assigned material catalog is unavailable.");Stage.Require(company,actor,task);if(Revision!=Create(Stage,Items,ComparedAtUtc).Revision)throw new InvalidOperationException("Original catalog membership, item facts or assigned authority changed.");
    }
}
