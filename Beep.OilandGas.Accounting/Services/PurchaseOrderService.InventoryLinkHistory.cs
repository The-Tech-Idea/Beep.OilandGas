using System.Text.Json;
using TheTechIdea.Beep.DataBase;
using TheTechIdea.Data.OilGas;
namespace Beep.OilandGas.Accounting.Services;
public partial class PurchaseOrderService
{
    public async Task<PurchaseOrderInventoryLinkHistoryReview> GetInventoryLinkHistoryAsync(CompanyScope company,string orderId,string lineId,string actor,Func<Task> requireAuthority)
    {
        ArgumentNullException.ThrowIfNull(requireAuthority);PurchaseOrderOwnership.RequireScope(company);ArgumentException.ThrowIfNullOrWhiteSpace(actor);
        ArgumentException.ThrowIfNullOrWhiteSpace(orderId);ArgumentException.ThrowIfNullOrWhiteSpace(lineId);await requireAuthority();
        if(_resolveConnection is null)throw new InvalidOperationException("A request-bound PRODUCTION resolver is required.");
        var connection=await _resolveConnection();var source=_editor.GetDataSource(connection) ?? throw new InvalidOperationException("Original history datasource is unavailable.");
        async Task Guard(){await requireAuthority();if(string.IsNullOrWhiteSpace(connection) || await _resolveConnection()!=connection || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Original history binding changed.");}
        async Task<(PurchaseOrderInventoryLinkHistoryReview History,string Source)> Read()
        {
            await Guard();var purchase=await GetSourceReviewAsync(company,orderId,actor) ?? throw new InvalidOperationException("Original owned order is unavailable.");
            var line=purchase.Lines.SingleOrDefault(row=>row.PO_LINE_ITEM_ID==lineId) ?? throw new InvalidOperationException("Original line is outside the owned order.");
            var lineSchema=source.GetEntityStructure(new EntityStructure{EntityName="PO_LINE_ITEM",DatasourceEntityName="PO_LINE_ITEM"},true);
            var cursor=lineSchema?.Fields?.Where(field=>field.FieldName=="INVENTORY_LINK_HISTORY_REVISION").ToArray();
            if(cursor is not {Length:1} || !cursor[0].AllowDBNull || cursor[0].Fieldtype!=typeof(string).FullName || cursor[0].Size<64)throw new InvalidOperationException("Review the nullable active-line history cursor in the Beep migration.");
            const string table="PO_INVENTORY_LINK_HISTORY";
            var schema=source.GetEntityStructure(new EntityStructure{EntityName=table,DatasourceEntityName=table},true);
            var keys=schema?.Fields?.Where(field=>field.IsKey).ToArray();
            if(keys is not {Length:1} || keys[0].FieldName!="PO_INVENTORY_LINK_HISTORY_ID" || keys[0].AllowDBNull)throw new InvalidOperationException("Review the canonical cycle-history key in the Beep migration.");
            foreach(var name in new[]{"OWNER_TENANT_ID","OWNER_COMPANY_BA_ID","PURCHASE_ORDER_ID","PO_LINE_ITEM_ID","SEQUENCE_NUMBER","PREVIOUS_ENTRY_ID","PREVIOUS_REVISION","CYCLE_INTENT_ID","SOURCE_REVISION","BINDING_REVISION","REASON","ARCHIVED_LINE_JSON","RECORDED_REVISION"})
                if(schema!.Fields.Count(field=>field.FieldName==name)!=1)throw new InvalidOperationException("Cycle-history migration evidence is incomplete.");
            var snapshot=schema!.Fields.Single(field=>field.FieldName=="ARCHIVED_LINE_JSON");if(snapshot.Fieldtype!=typeof(string).FullName || snapshot.Size<4194304)throw new InvalidOperationException("Complete archived cycle bound is unavailable.");
            var repo=await GetRepoAsync<PO_INVENTORY_LINK_HISTORY>(table);
            var raw=(await repo.GetAsync([new(){FieldName="OWNER_TENANT_ID",Operator="=",FilterValue=company.TenantId},new(){FieldName="OWNER_COMPANY_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="PURCHASE_ORDER_ID",Operator="=",FilterValue=orderId},new(){FieldName="PO_LINE_ITEM_ID",Operator="=",FilterValue=lineId}]))?.ToArray();
            if(raw is null || raw.Any(row=>row is not PO_INVENTORY_LINK_HISTORY))throw new InvalidOperationException("Complete typed cycle history is unavailable.");
            var result=PurchaseOrderInventoryLinkHistoryReview.Create(company,actor,orderId,lineId,raw.Cast<PO_INVENTORY_LINK_HISTORY>().ToArray(),line.INVENTORY_LINK_HISTORY_REVISION);await Guard();return(result,purchase.Revision);
        }
        var first=await Read();var closing=await Read();await Guard();
        var finalSource=await GetSourceReviewAsync(company,orderId,actor) ?? throw new InvalidOperationException("Closing owned order is unavailable.");
        var finalLine=finalSource.Lines.Single(row=>row.PO_LINE_ITEM_ID==lineId);closing.History.Require(company,actor,orderId,lineId,finalLine.INVENTORY_LINK_HISTORY_REVISION);await Guard();
        if(first.Source!=closing.Source || closing.Source!=finalSource.Revision || first.History.Revision!=closing.History.Revision)throw new InvalidOperationException("Original purchase or complete cycle history changed.");return closing.History;
    }
}
