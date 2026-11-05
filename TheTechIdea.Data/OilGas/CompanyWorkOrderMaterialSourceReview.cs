namespace TheTechIdea.Data.OilGas;

public sealed record CompanyWorkOrderMaterialSourceQuery(CompanyMaintenanceReceivingQuery Task,string ItemId,decimal RequiredQuantity,string QuantityUnit)
{
    public void Require()
    {
        if(Task is null || !LeaseSummaryValidator.Identifier(ItemId) || ItemId.Length>128 || string.IsNullOrWhiteSpace(QuantityUnit) || QuantityUnit.Length>64 || QuantityUnit.Any(char.IsControl) || RequiredQuantity<=0)throw new ArgumentException("Select the original work-order task, exact item and explicit positive required quantity/unit.");Task.Require();
    }
}
/// <summary>Current original order and owned inventory source; on-hand is not available or allocated stock.</summary>
public sealed record CompanyWorkOrderMaterialSourceReview(FieldSelectionReview Context,CompanyWorkOrderMaterialSourceQuery Query,
    CompanyMaintenanceInspectionReview WorkOrder,CompanyInventorySourceReview Inventory,IReadOnlyList<string> Blockers)
{
    public static string[] GetBlockers(CompanyWorkOrderMaterialSourceQuery query,CompanyInventorySourceReview source)
    {
        query.Require();source.Require(source.Company,source.ActorUserId,query.ItemId);var item=source.Item;var result=new List<string>();
        if(item.ACTIVE_IND!="Y")result.Add("The recorded inventory item is inactive.");
        if(string.IsNullOrWhiteSpace(item.WAREHOUSE_ID) || string.IsNullOrWhiteSpace(item.LOCATION_ID))result.Add("Recorded warehouse/location evidence is unavailable.");
        if(string.IsNullOrWhiteSpace(item.UNIT_OF_MEASURE) || item.UNIT_OF_MEASURE!=query.QuantityUnit)result.Add("The explicit required unit differs from the recorded item unit; no conversion is assumed.");
        if(item.QUANTITY_ON_HAND is null)result.Add("Recorded on-hand quantity is unavailable.");
        else if(item.QUANTITY_ON_HAND<query.RequiredQuantity)result.Add("Recorded on-hand quantity is below the explicit work requirement.");
        result.Add("Complete current work-order allocation and competing reservation evidence is unconfirmed; on-hand does not establish availability.");return result.ToArray();
    }
    public void Require(CompanyScope company,string actor,CompanyWorkOrderMaterialSourceQuery query)
    {
        query.Require();if(Context is null || Query is null || WorkOrder is null || Inventory is null || Blockers is null)throw new InvalidOperationException("The complete original order/material source review is unavailable.");FieldSelectionReviewValidator.RequireReview(Context,company,actor);WorkOrder.Require(company,actor,query.Task);Inventory.Require(company,actor,query.ItemId);
        if(Query!=query || Context.FieldId!=query.Task.FieldId || !Blockers.SequenceEqual(GetBlockers(query,Inventory),StringComparer.Ordinal))throw new InvalidOperationException("The original company/reviewer/task/item/quantity/unit or full material blockers changed.");
    }
}
