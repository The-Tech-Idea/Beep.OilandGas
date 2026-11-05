using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded item and complete movement history, without inferred availability or receipt linkage.</summary>
public sealed record CompanyInventorySourceReview(CompanyScope Company,string ActorUserId,INVENTORY_ITEM Item,
    IReadOnlyList<INVENTORY_TRANSACTION> Transactions,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static void RequireItem(CompanyScope company,string id,INVENTORY_ITEM? item)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(id) || item is null || item.INVENTORY_ITEM_ID!=id ||
            item.OWNER_TENANT_ID!=company.TenantId || item.OWNER_COMPANY_BA_ID!=company.BusinessAssociateId)
            throw new InvalidOperationException("The inventory item does not establish the selected owning company.");
        foreach(var (value,max) in new[]{(item.WAREHOUSE_ID,128),(item.LOCATION_ID,128),(item.CURRENCY_CODE,32),(item.UNIT_OF_MEASURE,64)})
            if(value is not null && (string.IsNullOrWhiteSpace(value) || value.Length>max || value.Any(char.IsControl)))
                throw new InvalidOperationException("Recorded warehouse, location, currency or unit evidence is malformed.");
    }
    public static void RequireRows(CompanyScope company,IReadOnlyList<INVENTORY_ITEM>? rows)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(rows is null)throw new InvalidOperationException("Company inventory evidence is unavailable.");
        foreach(var row in rows){RequireItem(company,row?.INVENTORY_ITEM_ID ?? "",row);if(row!.ACTIVE_IND!="Y")throw new InvalidOperationException("The inventory item does not match the active company query.");}
        if(rows.Select(row=>row.INVENTORY_ITEM_ID).Distinct(StringComparer.Ordinal).Count()!=rows.Count)
            throw new InvalidOperationException("Inventory item identities are ambiguous.");
    }
    public void Require(CompanyScope company,string actor,string itemId)
    {
        RequireItem(company,itemId,Item);
        if(Company!=company || string.IsNullOrWhiteSpace(actor) || ActorUserId!=actor || Transactions is null ||
            RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero || !ProcessDefinitionRevision.IsValid(Revision))
            throw new InvalidOperationException("The inventory review does not establish the original company, actor or history.");
        if(Transactions.Any(row=>row is null || string.IsNullOrWhiteSpace(row.INVENTORY_TRANSACTION_ID) || row.INVENTORY_ITEM_ID!=itemId) ||
            Transactions.Select(row=>row.INVENTORY_TRANSACTION_ID).Distinct(StringComparer.Ordinal).Count()!=Transactions.Count ||
            Revision!=Digest(company,Item,Transactions))
            throw new InvalidOperationException("The recorded inventory item or movement history changed or has ambiguous membership.");
    }
    public static CompanyInventorySourceReview Create(CompanyScope company,string actor,INVENTORY_ITEM item,IReadOnlyList<INVENTORY_TRANSACTION> transactions)
    {
        RequireItem(company,item?.INVENTORY_ITEM_ID ?? "",item);
        if(transactions is null || transactions.Any(row=>row is null))throw new InvalidOperationException("Recorded inventory movement history is unavailable.");
        var frozen=Copy(item!);var history=Array.AsReadOnly(transactions.OrderBy(row=>row.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal).Select(Copy).ToArray());
        var review=new CompanyInventorySourceReview(company,actor,frozen,history,Digest(company,frozen,history),DateTimeOffset.UtcNow);
        review.Require(company,actor,item!.INVENTORY_ITEM_ID);return review;
    }
    public static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.SerializeToUtf8Bytes(value))!;
    private static string Digest(CompanyScope company,INVENTORY_ITEM item,IReadOnlyList<INVENTORY_TRANSACTION> transactions)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company=company,Item=item,Transactions=transactions.OrderBy(row=>row.INVENTORY_TRANSACTION_ID,StringComparer.Ordinal)}));
}
