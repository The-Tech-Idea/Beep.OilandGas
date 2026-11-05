using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

/// <summary>Recorded company-owned header, lines and receipt membership. No matching, stock or approval result.</summary>
public sealed record PurchaseOrderSourceReview(CompanyScope Company,string ActorUserId,PURCHASE_ORDER Header,
    IReadOnlyList<PO_LINE_ITEM> Lines,IReadOnlyList<PO_RECEIPT> Receipts,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public void Require(CompanyScope company,string actor,string order)
    {
        if(Company!=company || string.IsNullOrWhiteSpace(actor) || ActorUserId!=actor ||
            !ProcessDefinitionRevision.IsValid(Revision) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The PO source review does not match the original company and actor.");
        PurchaseOrderOwnership.Require(company,order,Header);PurchaseOrderOwnership.RequireLines(order,Lines);
        PurchaseOrderOwnership.RequireReceipts(order,Receipts);
        RequireRecordedCode(Header.CURRENCY_CODE,32);
        foreach(var line in Lines){RequireRecordedCode(line.QUANTITY_UOM,64);RequireRecordedCode(line.INVENTORY_ITEM_ID,128);if(line.INVENTORY_LINK_HISTORY_REVISION is not null && !ProcessDefinitionRevision.IsValid(line.INVENTORY_LINK_HISTORY_REVISION))throw new InvalidOperationException("Recorded cycle-history cursor is malformed.");}
        foreach(var receipt in Receipts)RequireRecordedCode(receipt.QUANTITY_UOM,64);
        var ids=Lines.Select(row=>row.PO_LINE_ITEM_ID).ToHashSet(StringComparer.Ordinal);
        if(Receipts.Any(row=>!ids.Contains(row.PO_LINE_ITEM_ID)))
            throw new InvalidOperationException("A recorded receipt has no confirmed line belonging to this order.");
        if(Revision!=Digest(company,Header,Lines,Receipts))
            throw new InvalidOperationException("The recorded source values do not match their content review.");
    }
    public static PurchaseOrderSourceReview Create(CompanyScope company,string actor,PURCHASE_ORDER header,
        IReadOnlyList<PO_LINE_ITEM> lines,IReadOnlyList<PO_RECEIPT> receipts)
    {
        PurchaseOrderOwnership.Require(company,header?.PURCHASE_ORDER_ID ?? "",header);
        PurchaseOrderOwnership.RequireLines(header!.PURCHASE_ORDER_ID,lines);PurchaseOrderOwnership.RequireReceipts(header.PURCHASE_ORDER_ID,receipts);
        var frozenHeader=Snapshot(header);
        var frozenLines=Array.AsReadOnly(lines.OrderBy(row=>row.PO_LINE_ITEM_ID,StringComparer.Ordinal).Select(Snapshot).ToArray());
        var frozenReceipts=Array.AsReadOnly(receipts.OrderBy(row=>row.PO_RECEIPT_ID,StringComparer.Ordinal).Select(Snapshot).ToArray());
        var review=new PurchaseOrderSourceReview(company,actor,frozenHeader,frozenLines,frozenReceipts,
            Digest(company,frozenHeader,frozenLines,frozenReceipts),DateTimeOffset.UtcNow);
        review.Require(company,actor,header.PURCHASE_ORDER_ID);return review;
    }
    private static T Snapshot<T>(T row)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(row))!;
    public IReadOnlyList<PO_LINE_ITEM> WorkflowMakerLines(IReadOnlyList<PurchaseOrderInventoryLinkHistoryReview> histories)
    {
        Require(Company,ActorUserId,Header.PURCHASE_ORDER_ID);
        if(histories is null || histories.Count!=Lines.Count(row=>row.INVENTORY_LINK_HISTORY_REVISION is not null) || histories.Select(row=>row.LineId).Distinct(StringComparer.Ordinal).Count()!=histories.Count)
            throw new InvalidOperationException("Complete original cycle history is required for workflow maker provenance.");
        var makers=new List<PO_LINE_ITEM>();
        foreach(var line in Lines)
        {
            PO_LINE_ITEM prior=line;
            if(line.INVENTORY_LINK_HISTORY_REVISION is not null)
            {
                var history=histories.SingleOrDefault(row=>row.LineId==line.PO_LINE_ITEM_ID) ?? throw new InvalidOperationException("Original line history is unavailable.");
                history.Require(Company,ActorUserId,Header.PURCHASE_ORDER_ID,line.PO_LINE_ITEM_ID,line.INVENTORY_LINK_HISTORY_REVISION);
                var last=history.Entries.Last();prior=PurchaseOrderInventoryLinkHistoryReview.RequireEntry(Company,Header.PURCHASE_ORDER_ID,line.PO_LINE_ITEM_ID,last);
                prior=Snapshot(prior);prior.INVENTORY_LINK_REVIEW_JSON=null;prior.INVENTORY_LINK_DECISION_JSON=null;prior.INVENTORY_LINK_HISTORY_REVISION=last.RECORDED_REVISION;prior.ROW_CHANGED_BY=last.ROW_CREATED_BY;prior.ROW_CHANGED_DATE=last.ROW_CREATED_DATE;
                var prepared=line.INVENTORY_LINK_REVIEW_JSON is not null || line.INVENTORY_LINK_DECISION_JSON is not null
                    ?PurchaseOrderInventoryLinkProposalProof.Require(Company,line).Review.Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID):line;
                if(JsonSerializer.Serialize(prepared)!=JsonSerializer.Serialize(prior))throw new InvalidOperationException("The active line does not continue its exact archived cycle.");
                var first=PurchaseOrderInventoryLinkHistoryReview.RequireEntry(Company,Header.PURCHASE_ORDER_ID,line.PO_LINE_ITEM_ID,history.Entries[0]);
                prior=PurchaseOrderInventoryLinkProposalProof.Require(Company,first).Review.Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
            }
            else if(line.INVENTORY_LINK_REVIEW_JSON is not null || line.INVENTORY_LINK_DECISION_JSON is not null)
                prior=PurchaseOrderInventoryLinkProposalProof.Require(Company,line).Review.Purchase.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
            var expected=Snapshot(prior);expected.INVENTORY_ITEM_ID=line.INVENTORY_ITEM_ID;expected.INVENTORY_LINK_REVIEW_JSON=line.INVENTORY_LINK_REVIEW_JSON;expected.INVENTORY_LINK_DECISION_JSON=line.INVENTORY_LINK_DECISION_JSON;
            expected.INVENTORY_LINK_HISTORY_REVISION=line.INVENTORY_LINK_HISTORY_REVISION;expected.ROW_CHANGED_BY=line.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=line.ROW_CHANGED_DATE;
            if(JsonSerializer.Serialize(expected)!=JsonSerializer.Serialize(line))throw new InvalidOperationException("Unreviewed purchase-line changes require maker reconciliation.");
            makers.Add(Snapshot(prior));
        }
        return Array.AsReadOnly(makers.ToArray());
    }
    private static void RequireRecordedCode(string? value,int maximum)
    {
        if(value is not null && (value.Length>maximum || value.Any(char.IsControl)))
            throw new InvalidOperationException("A recorded currency or quantity-unit value exceeds its source contract.");
    }
    private static object Audit(ModelEntityBase row)=>new{row.ACTIVE_IND,row.ROW_CREATED_BY,row.ROW_CREATED_DATE,
        row.ROW_CHANGED_BY,row.ROW_CHANGED_DATE,row.ROW_EFFECTIVE_DATE,row.ROW_EXPIRY_DATE,row.ROW_QUALITY,row.PPDM_GUID,
        row.EFFECTIVE_DATE,row.EXPIRY_DATE,row.SOURCE,row.REMARK};
    private static System.Text.Json.Nodes.JsonNode LineEvidence(PO_LINE_ITEM row)
    {
        var value=JsonSerializer.SerializeToNode(new{row.PO_LINE_ITEM_ID,row.PURCHASE_ORDER_ID,row.LINE_NUMBER,row.ITEM_DESCRIPTION,
            row.QUANTITY,row.QUANTITY_UOM,row.UNIT_PRICE,row.LINE_TOTAL,row.ROW_ID,Audit=Audit(row)})!;
        if(row.INVENTORY_ITEM_ID is not null)value["INVENTORY_ITEM_ID"]=row.INVENTORY_ITEM_ID;
        if(row.INVENTORY_LINK_REVIEW_JSON is not null)value["INVENTORY_LINK_REVIEW_JSON"]=row.INVENTORY_LINK_REVIEW_JSON;
        if(row.INVENTORY_LINK_DECISION_JSON is not null)value["INVENTORY_LINK_DECISION_JSON"]=row.INVENTORY_LINK_DECISION_JSON;
        if(row.INVENTORY_LINK_HISTORY_REVISION is not null)value["INVENTORY_LINK_HISTORY_REVISION"]=row.INVENTORY_LINK_HISTORY_REVISION;
        return value;
    }
    private static string Digest(CompanyScope company,PURCHASE_ORDER header,IReadOnlyList<PO_LINE_ITEM> lines,IReadOnlyList<PO_RECEIPT> receipts)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{company,
            Header=new{header.PURCHASE_ORDER_ID,header.OWNER_TENANT_ID,header.OWNER_COMPANY_BA_ID,header.VENDOR_BA_ID,
                header.PO_NUMBER,header.PO_DATE,header.EXPECTED_DELIVERY_DATE,header.TOTAL_AMOUNT,header.CURRENCY_CODE,header.STATUS,header.DESCRIPTION,header.ROW_ID,Audit=Audit(header)},
            Lines=lines.OrderBy(row=>row.PO_LINE_ITEM_ID,StringComparer.Ordinal).Select(LineEvidence),
            Receipts=receipts.OrderBy(row=>row.PO_RECEIPT_ID,StringComparer.Ordinal).Select(row=>new{row.PO_RECEIPT_ID,row.PURCHASE_ORDER_ID,
                row.PO_LINE_ITEM_ID,row.RECEIPT_DATE,row.RECEIVED_QUANTITY,row.QUANTITY_UOM,row.RECEIVED_BY,row.RECEIPT_STATUS,row.DESCRIPTION,row.ROW_ID,
                row.ACCEPTED_QUANTITY,row.REJECTED_QUANTITY,row.RECEIVER_DECISION_JSON,Audit=Audit(row)})}));
}
