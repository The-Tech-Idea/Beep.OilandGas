using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
using Beep.OilandGas.PPDM39.Core;
using TheTechIdea.Beep.Report;
namespace Beep.OilandGas.ProductionAccounting.Services;
public partial class ProductionAccountingService
{
    /// <summary>Resolves only an exact prepared ticket whose native proof belongs to the current company.</summary>
    public async Task<RUN_TICKET> GetRecordedDeliveryAcceptanceTicketAsync(string ticketId,CompanyScope company,string connection)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(ticketId) || ticketId.Length>128 || ticketId.Any(char.IsControl) || string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("An exact prepared ticket and bound PRODUCTION datasource are required.");
        var datasource=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery datasource is unavailable.");
        RequireDeliveryAcceptanceSchema(connection);
        var keys=(await _metadata.GetPrimaryKeyColumnsAsync("RUN_TICKET"))?.ToArray();
        if(keys is not {Length:1} || keys[0]!="RUN_TICKET_ID")throw new InvalidOperationException("A unique native ticket identity is required.");
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(RUN_TICKET),connection,"RUN_TICKET");
        var rows=(await repo.GetAsync([new(){FieldName="RUN_TICKET_ID",Operator="=",FilterValue=ticketId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();
        if(rows is not {Length:1} || rows[0] is not RUN_TICKET ticket || ticket.RUN_TICKET_ID!=ticketId || ticket.ACTIVE_IND!="Y" || !ReferenceEquals(datasource,_editor.GetDataSource(connection)))
            throw new InvalidOperationException("A unique current prepared delivery ticket is unconfirmed.");
        var frozen=CompanyInventorySourceReview.Copy(ticket);
        _=RevenueDeliveryAcceptanceProof.Require(company,frozen);
        return frozen;
    }
    public async Task<RevenueDeliverySourceReview> GetRecordedRevenueDeliveryAsync(RevenueSalesContractReview review,string connection)
    {
        var original=review.Source;review.Require(original.Company,original.ActorUserId,original.FieldId,original.From,original.Through,review.TransactionId,original.Revision);
        if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("A bound PRODUCTION datasource is required.");
        var datasource=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The delivery datasource is unavailable.");
        async Task<T> Exact<T>(string table,string key,string id,List<AppFilter> additional) where T:class
        {
            if(string.IsNullOrWhiteSpace(id) || id.Length>128 || id.Any(char.IsControl))throw new InvalidOperationException("An exact recorded delivery identity is required.");
            var keys=(await _metadata.GetPrimaryKeyColumnsAsync(table))?.ToArray();
            if(keys is not {Length:1} || keys[0]!=key)throw new InvalidOperationException("Unique native delivery metadata is required.");
            var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(T),connection,table);
            var rows=(await repo.GetAsync([new(){FieldName=key,Operator="=",FilterValue=id},..additional]))?.ToArray();
            if(rows is not {Length:1} || rows[0] is not T row || !ReferenceEquals(datasource,_editor.GetDataSource(connection)))throw new InvalidOperationException("A unique native delivery record is unconfirmed.");
            return CompanyInventorySourceReview.Copy(row);
        }
        var revenue=original.Rows.Single(row=>row.REVENUE_TRANSACTION_ID==review.TransactionId);
        var allocation=await Exact<ALLOCATION_RESULT>("ALLOCATION_RESULT","ALLOCATION_RESULT_ID",revenue.ALLOCATION_RESULT_ID,[]);
        // AllocationEngine records RUN_TICKET_ID in ALLOCATION_REQUEST_ID. Never query an undeclared run-ticket allocation column or use ticket-number fallback.
        var ticket=await Exact<RUN_TICKET>("RUN_TICKET","RUN_TICKET_ID",allocation.ALLOCATION_REQUEST_ID,[new(){FieldName="FIELD_ID",Operator="=",FilterValue=original.FieldId}]);
        return RevenueDeliverySourceReview.Create(review,allocation,ticket);
    }
    public async Task<SALES_CONTRACT> GetRecordedSalesContractAsync(string contractId,CompanyScope company,string connection)
    {
        PurchaseOrderOwnership.RequireScope(company);
        if(string.IsNullOrWhiteSpace(contractId) || contractId.Length>128 || contractId.Any(char.IsControl) || string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("An exact recorded sales contract and bound datasource are required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The sales contract datasource is unavailable.");
        var keys=(await _metadata.GetPrimaryKeyColumnsAsync("SALES_CONTRACT"))?.ToArray();
        if(keys is not {Length:1} || keys[0]!="SALES_CONTRACT_ID")throw new InvalidOperationException("A unique native sales contract identity is required.");
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(SALES_CONTRACT),connection,"SALES_CONTRACT");
        var rows=(await repo.GetAsync([new(){FieldName="SALES_CONTRACT_ID",Operator="=",FilterValue=contractId},new(){FieldName="SELLER_BA_ID",Operator="=",FilterValue=company.BusinessAssociateId},new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();
        if(rows is not {Length:1} || rows[0] is not SALES_CONTRACT row || row.SALES_CONTRACT_ID!=contractId || row.SELLER_BA_ID!=company.BusinessAssociateId || row.ACTIVE_IND!="Y" || !ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("A unique current company-owned sales contract is unconfirmed.");
        return CompanyInventorySourceReview.Copy(row);
    }
    public async Task<List<REVENUE_TRANSACTION>> GetRecordedFieldRevenueAsync(string fieldId,DateTime from,DateTime through,string connection)
    {
        FieldRevenueSourceReview.RequireWindow(fieldId,from,through);
        if(string.IsNullOrWhiteSpace(connection))throw new InvalidOperationException("A request-bound PRODUCTION datasource is required.");
        var source=_editor.GetDataSource(connection) ??throw new InvalidOperationException("The recorded revenue datasource is unavailable.");
        var keys=(await _metadata.GetPrimaryKeyColumnsAsync("REVENUE_TRANSACTION"))?.ToArray();
        if(keys is not {Length:1} || keys[0]!="REVENUE_TRANSACTION_ID")throw new InvalidOperationException("A unique native recorded revenue identity is required.");
        var repo=new PPDMGenericRepository(_editor,_commonColumnHandler,_defaults,_metadata,typeof(REVENUE_TRANSACTION),connection,"REVENUE_TRANSACTION");
        async Task<REVENUE_TRANSACTION[]> Read(string column)
        {
            if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Recorded revenue datasource changed.");
            var raw=(await repo.GetAsync([
                new(){FieldName=column,Operator="=",FilterValue=fieldId},new(){FieldName="TRANSACTION_DATE",Operator=">=",FilterValue=from.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)},
                new(){FieldName="TRANSACTION_DATE",Operator="<",FilterValue=through.AddDays(1).ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)},
                new(){FieldName="ACTIVE_IND",Operator="=",FilterValue="Y"}]))?.ToArray();
            if(raw is null || raw.Any(row=>row is not REVENUE_TRANSACTION))throw new InvalidOperationException("Typed complete recorded revenue rows are unavailable.");
            var rows=raw.Cast<REVENUE_TRANSACTION>().Select(CompanyInventorySourceReview.Copy).ToArray();FieldRevenueSourceReview.RequireRows(fieldId,from,through,rows);return rows;
        }
        // Both existing association columns are read; an empty first query never hides the other membership.
        var property=await Read("PROPERTY_ID");var field=await Read("FIELD_ID");var rows=new List<REVENUE_TRANSACTION>();
        foreach(var group in property.Concat(field).GroupBy(row=>row.REVENUE_TRANSACTION_ID,StringComparer.Ordinal))
        {
            var copies=group.ToArray();if(copies.Length>2 || copies.Skip(1).Any(row=>JsonSerializer.Serialize(row)!=JsonSerializer.Serialize(copies[0])))throw new InvalidOperationException("Revenue identity is ambiguous across FIELD/property membership.");
            rows.Add(copies[0]);
        }
        if(!ReferenceEquals(source,_editor.GetDataSource(connection)))throw new InvalidOperationException("Recorded revenue datasource changed.");
        FieldRevenueSourceReview.RequireRows(fieldId,from,through,rows);return rows.OrderBy(row=>row.REVENUE_TRANSACTION_ID,StringComparer.Ordinal).ToList();
    }
}
