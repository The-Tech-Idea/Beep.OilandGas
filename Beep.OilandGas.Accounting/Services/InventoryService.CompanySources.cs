using TheTechIdea.Data.OilGas;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.Accounting.Services;

public partial class InventoryService
{
    private void RequireCompanySourceSchema(string connection,string table,bool stockEvidence=false)
    {
        if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("The PRODUCTION binding is unavailable.");
        var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("The inventory datasource is unavailable.");
        var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
        var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
        if(keys is not {Length:1} || keys[0].AllowDBNull || !string.Equals(keys[0].FieldName,table+"_ID",StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("A single native nonnullable inventory identity is required.");
        var columns=table=="INVENTORY_ITEM"?new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","WAREHOUSE_ID","LOCATION_ID","CURRENCY_CODE","UNIT_OF_MEASURE"}:new[]{"INVENTORY_ITEM_ID"};
        foreach(var column in columns)
            if(schema!.Fields.Count(field=>string.Equals(field.FieldName,column,StringComparison.OrdinalIgnoreCase))!=1)
                throw new InvalidOperationException("Company inventory metadata is unavailable. Review the PRODUCTION Beep migration.");

        if(stockEvidence)
        {
            var reference=schema!.Fields.SingleOrDefault(value=>value.FieldName=="REFERENCE_NUMBER");
            if(reference?.Fieldtype!=typeof(string).FullName)throw new InvalidOperationException("Native receipt-reference metadata is required for complete physical receipt membership.");
            var field=schema!.Fields.SingleOrDefault(value=>value.FieldName=="PURCHASE_RECEIPT_STOCK_JSON");
            if(field is null || !field.AllowDBNull || field.Size1<16384 || field.Fieldtype!=typeof(string).FullName)
                throw new InvalidOperationException("Nullable receipt stock provenance metadata is required. Review the PRODUCTION Beep migration.");
        }
    }
    public async Task<List<INVENTORY_ITEM>> GetCompanyInventoryItemsAsync(CompanyScope company)
    {
        PurchaseOrderOwnership.RequireScope(company);
        var repo=await GetRepoAsync<INVENTORY_ITEM>("INVENTORY_ITEM");
        var raw=await repo.GetAsync([
            new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
            new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
            new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]);
        var values=raw?.ToList();
        if(values is null || values.Any(value=>value is not INVENTORY_ITEM))throw new InvalidOperationException("Company inventory rows are unavailable.");
        var rows=values.Cast<INVENTORY_ITEM>().ToList();CompanyInventorySourceReview.RequireRows(company,rows);
        return rows.Select(CompanyInventorySourceReview.Copy).ToList();
    }
    public async Task<CompanyInventorySourceReview?> GetCompanyInventorySourceAsync(CompanyScope company,string itemId,string actor,bool stockEvidence=false)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(itemId) || string.IsNullOrWhiteSpace(actor))throw new InvalidOperationException("An original item and actor are required.");
        var repo=await GetRepoAsync<INVENTORY_ITEM>("INVENTORY_ITEM");
        var raw=await repo.GetAsync([
            new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},
            new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},
            new(){FieldName="INVENTORY_ITEM_ID",Operator="=",FilterValue=itemId}]);
        var values=raw?.ToList();
        if(values is null || values.Any(value=>value is not INVENTORY_ITEM) || values.Count>1)throw new InvalidOperationException("The owned inventory item is unavailable or ambiguous.");
        if(values.Count==0)return null;
        var item=CompanyInventorySourceReview.Copy((INVENTORY_ITEM)values.Single());CompanyInventorySourceReview.RequireItem(company,itemId,item);
        var historyRepo=await GetRepoAsync<INVENTORY_TRANSACTION>("INVENTORY_TRANSACTION",stockEvidence);
        var history=await historyRepo.GetAsync([new(){FieldName="INVENTORY_ITEM_ID",Operator="=",FilterValue=itemId}]);
        if(history is null || history.Any(value=>value is not INVENTORY_TRANSACTION))throw new InvalidOperationException("Recorded inventory history is unavailable.");
        return CompanyInventorySourceReview.Create(company,actor,item,history.Cast<INVENTORY_TRANSACTION>().ToArray());
    }
}
