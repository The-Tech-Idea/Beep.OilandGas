using System.Text.Json;
using System.Globalization;
namespace TheTechIdea.Data.OilGas;

/// <summary>Recorded whole-invoice financial diagnostics, never matching approval or permission to post.</summary>
public sealed partial record APInvoiceFinancialReview(APInvoiceSourceReview Invoice,IReadOnlyList<APReceiptAllocationReview> PurchaseLines,
    string Revision,DateTimeOffset RetrievedAtUtc,CompanyMatchingPolicyReview? MatchingPolicies=null,IReadOnlyList<APReceiptConsumptionReview>? ReceiptConsumption=null)
{
    public static APInvoiceFinancialReview Create(APInvoiceSourceReview invoice,IReadOnlyList<APReceiptAllocationReview> purchases,CompanyMatchingPolicyReview? policies=null,IReadOnlyList<APReceiptConsumptionReview>? consumption=null)
    {
        var frozen=JsonSerializer.Deserialize<APInvoiceSourceReview>(JsonSerializer.Serialize(invoice))!;
        var lines=JsonSerializer.Deserialize<List<APReceiptAllocationReview>>(JsonSerializer.Serialize(purchases.OrderBy(row=>row.Source.InvoiceLineId,StringComparer.Ordinal)))!;
        var receipts=consumption is null?null:PurchaseReceiptDecisionProof.Copy(consumption.ToArray());
        var review=new APInvoiceFinancialReview(frozen,lines,Digest(frozen,lines,policies,receipts),DateTimeOffset.UtcNow,policies is null?null:PurchaseReceiptDecisionProof.Copy(policies),receipts);
        review.Require(invoice.Company,invoice.ActorUserId,invoice.Invoice.AP_INVOICE_ID,invoice.Revision);return review;
    }
    public void Require(CompanyScope company,string actor,string id,string expectedRevision)
    {
        if(Invoice is null || PurchaseLines is null || ReceiptConsumption?.Any(row=>row?.Source is null)==true)throw new InvalidOperationException("The whole invoice financial evidence is unavailable.");
        Invoice.Require(company,actor,id);
        var references=Invoice.Lines.Where(line=>!string.IsNullOrWhiteSpace(line.PURCHASE_ORDER_ID)).Select(line=>line.AP_INVOICE_LINE_ID).Order(StringComparer.Ordinal).ToArray();
        if(Invoice.Revision!=expectedRevision || Invoice.Lines.Count>128 || PurchaseLines.Any(row=>row?.Source is null) ||
            !references.SequenceEqual(PurchaseLines.Select(row=>row.Source.InvoiceLineId).Order(StringComparer.Ordinal)) ||
            RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero || Revision!=Digest(Invoice,PurchaseLines,MatchingPolicies,ReceiptConsumption))
            throw new InvalidOperationException("The financial review does not match the displayed complete invoice and purchase line set.");
        MatchingPolicies?.Require(company,actor);
        foreach(var row in PurchaseLines)row.Require(company,actor,id,row.Source.InvoiceLineId,expectedRevision);
        RequireReceiptConsumption(company,actor);
    }
    private static string Digest(APInvoiceSourceReview invoice,IReadOnlyList<APReceiptAllocationReview> purchases,CompanyMatchingPolicyReview? policies=null,IReadOnlyList<APReceiptConsumptionReview>? consumption=null)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{invoice.Revision,PolicyHistory=policies?.Revision,Lines=purchases.OrderBy(row=>row.Source.InvoiceLineId,StringComparer.Ordinal).Select(row=>new{row.Source.InvoiceLineId,row.Revision}),ReceiptConsumption=consumption?.OrderBy(row=>row.Source.InvoiceLineId,StringComparer.Ordinal).ThenBy(row=>row.ReceiptId,StringComparer.Ordinal).Select(row=>new{row.Source.InvoiceLineId,row.ReceiptId,row.Revision})}));
    public IReadOnlyList<APSourceComparison> Checks
    {
        get
        {
            Require(Invoice.Company,Invoice.ActorUserId,Invoice.Invoice.AP_INVOICE_ID,Invoice.Revision);
            var rows=new List<APSourceComparison>();var header=Invoice.Invoice;
            void Add(string code,string label,APSourceComparisonOutcome outcome,string evidence)=>rows.Add(new(code,label,outcome,evidence));
            string Number(decimal? value)=>value?.ToString(CultureInfo.InvariantCulture) ?? "Unavailable";
            var activity=Invoice.Lines.All(line=>line.ACTIVE_IND is "Y" or "N");
            var active=Invoice.Lines.Where(line=>line.ACTIVE_IND=="Y").ToArray();
            Add("activity","Recorded source activity",header.ACTIVE_IND=="Y" && activity && active.Length>0?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.Unavailable,
                $"Header: {header.ACTIVE_IND ?? "Unavailable"}; active lines: {active.Length}; all line activity indicators confirmed: {activity}.");
            Add("draft","Recorded invoice status",string.IsNullOrWhiteSpace(header.STATUS)?APSourceComparisonOutcome.Unavailable:header.STATUS=="DRAFT"?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference,
                $"Recorded status: {header.STATUS ?? "Unavailable"}. This does not prove financial approval or posting.");
            var maker=header.ROW_CREATED_BY;
            bool Audit(string? creator,DateTime? created,string? changedBy,DateTime? changed)=>!string.IsNullOrWhiteSpace(creator) && creator!="SYSTEM" && created is {} time && time!=default && time.Kind==DateTimeKind.Utc && string.IsNullOrWhiteSpace(changedBy) && changed is null;
            var knownMaker=Audit(maker,header.ROW_CREATED_DATE,header.ROW_CHANGED_BY,header.ROW_CHANGED_DATE) && Invoice.Lines.Count>0 &&
                Invoice.Lines.All(line=>line.ROW_CREATED_BY==maker && line.ROW_CREATED_DATE==header.ROW_CREATED_DATE && Audit(line.ROW_CREATED_BY,line.ROW_CREATED_DATE,line.ROW_CHANGED_BY,line.ROW_CHANGED_DATE));
            Add("maker","Recorded source maker and current account",!knownMaker?APSourceComparisonOutcome.Unavailable:maker==Invoice.ActorUserId?APSourceComparisonOutcome.RecordedDifference:APSourceComparisonOutcome.RecordedAgreement,
                $"Source creator: {maker ?? "Unavailable"}; current account: {Invoice.ActorUserId}. Complete unmodified header/line creation audit is required; this comparison grants no review authority.");
            decimal? total=null;
            if(activity && active.Length>0 && active.All(line=>line.LINE_TOTAL is >=0 && !string.IsNullOrWhiteSpace(header.CURRENCY_CODE) && line.CURRENCY_CODE==header.CURRENCY_CODE))
                try{total=active.Sum(line=>line.LINE_TOTAL!.Value);}catch(OverflowException){}
            Add("invoice-total","Recorded header and active line amounts",total is null || header.TOTAL_AMOUNT is null?APSourceComparisonOutcome.Unavailable:total==header.TOTAL_AMOUNT?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference,
                $"Header: {Number(header.TOTAL_AMOUNT)}; active line sum: {Number(total)}; currency: {header.CURRENCY_CODE ?? "Unavailable"}. No tax, rounding, exchange rate or tolerance is assumed.");
            foreach(var line in active)
            {
                decimal? calculated=null;
                if(line.QUANTITY is >0 && line.UNIT_PRICE is >=0 && !string.IsNullOrWhiteSpace(line.CURRENCY_CODE))
                    try{calculated=checked(line.QUANTITY.Value*line.UNIT_PRICE.Value);}catch(OverflowException){}
                Add("amount/"+line.AP_INVOICE_LINE_ID,"Recorded quantity and price amount for "+line.AP_INVOICE_LINE_ID,calculated is null || line.LINE_TOTAL is null?APSourceComparisonOutcome.Unavailable:calculated==line.LINE_TOTAL?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference,
                    $"Recorded line amount: {Number(line.LINE_TOTAL)}; exact quantity times price: {Number(calculated)}. No tax or rounding rule is inferred.");
                var purchase=PurchaseLines.SingleOrDefault(row=>row.Source.InvoiceLineId==line.AP_INVOICE_LINE_ID);
                if(purchase is null){Add("purchase/"+line.AP_INVOICE_LINE_ID,"Owned purchase reference for "+line.AP_INVOICE_LINE_ID,APSourceComparisonOutcome.Unavailable,"This line has no complete recorded purchase-order reference. Non-PO authorization requires separate recorded review.");continue;}
                var orderLine=purchase.Source.Order.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
                var sameUnits=!string.IsNullOrWhiteSpace(line.QUANTITY_UOM) && line.QUANTITY_UOM==orderLine.QUANTITY_UOM;
                var sameCurrency=!string.IsNullOrWhiteSpace(header.CURRENCY_CODE) && line.CURRENCY_CODE==header.CURRENCY_CODE && purchase.Source.Order.Header.CURRENCY_CODE==header.CURRENCY_CODE;
                Add("price/"+line.AP_INVOICE_LINE_ID,"Recorded PO price for "+line.AP_INVOICE_LINE_ID,!sameUnits || !sameCurrency || line.UNIT_PRICE is not >=0 || orderLine.UNIT_PRICE is not >=0?APSourceComparisonOutcome.Unavailable:line.UNIT_PRICE==orderLine.UNIT_PRICE?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference,
                    $"Invoice unit: {line.QUANTITY_UOM ?? "Unavailable"}; PO unit: {orderLine.QUANTITY_UOM ?? "Unavailable"}; invoice currency: {header.CURRENCY_CODE ?? "Unavailable"}; PO currency: {purchase.Source.Order.Header.CURRENCY_CODE ?? "Unavailable"}; invoice price: {Number(line.UNIT_PRICE)}; PO price: {Number(orderLine.UNIT_PRICE)}. No conversion is applied.");
                decimal? allocated=null;
                if(purchase.Allocations.All(row=>row.ACTIVE_IND is "Y" or "N" && (row.ACTIVE_IND!="Y" || row.QUANTITY_UOM==line.QUANTITY_UOM)) && !string.IsNullOrWhiteSpace(line.QUANTITY_UOM))
                    try{allocated=purchase.Allocations.Where(row=>row.ACTIVE_IND=="Y").Sum(row=>row.ALLOCATED_QUANTITY!.Value);}catch(OverflowException){}
                Add("allocation/"+line.AP_INVOICE_LINE_ID,"Recorded allocated quantity for "+line.AP_INVOICE_LINE_ID,allocated is null || line.QUANTITY is not >0?APSourceComparisonOutcome.Unavailable:allocated==line.QUANTITY?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.RecordedDifference,
                    $"Invoice quantity: {Number(line.QUANTITY)}; active line allocations: {Number(allocated)}. This is not global receipt availability or accepted quantity.");
            }
            var acceptance=APSourceComparisonOutcome.Unavailable;var verifiedDecisions=0;
            try
            {
                var complete=active.Length>0;var within=true;
                foreach(var line in active)
                {
                    var purchase=PurchaseLines.SingleOrDefault(item=>item.Source.InvoiceLineId==line.AP_INVOICE_LINE_ID);
                    var allocations=purchase?.Allocations.Where(item=>item.ACTIVE_IND=="Y").ToArray();
                    if(purchase is null || allocations is not {Length:>0} || purchase.Allocations.Any(item=>item.ACTIVE_IND is not ("Y" or "N"))){complete=false;continue;}
                    var orderLine=purchase.Source.Order.Lines.Single(item=>item.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
                    if(purchase.Source.Order.Header.ACTIVE_IND!="Y" || orderLine.ACTIVE_IND!="Y" || orderLine.QUANTITY_UOM!=line.QUANTITY_UOM){complete=false;continue;}
                    foreach(var group in allocations.GroupBy(item=>item.PO_RECEIPT_ID,StringComparer.Ordinal))
                    {
                        var receipt=purchase.Source.Order.Receipts.Single(item=>item.PO_RECEIPT_ID==group.Key);
                        var proof=PurchaseReceiptDecisionProof.Require(Invoice.Company,receipt);
                        if(receipt.ACTIVE_IND!="Y" || receipt.QUANTITY_UOM!=line.QUANTITY_UOM || group.Any(item=>item.QUANTITY_UOM!=receipt.QUANTITY_UOM))
                        {complete=false;continue;}
                        verifiedDecisions++;within &= checked(group.Sum(item=>item.ALLOCATED_QUANTITY!.Value))<=proof.Command.AcceptedQuantity;
                    }
                }
                if(complete)acceptance=within?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.RecordedDifference;
            }
            catch(InvalidOperationException){acceptance=APSourceComparisonOutcome.Unavailable;}
            catch(OverflowException){acceptance=APSourceComparisonOutcome.Unavailable;}
            Add("acceptance","Receipt acceptance",acceptance,
                $"Verified receiver decisions for allocated line/receipt groups: {verifiedDecisions}. Compares this invoice line's allocation with accepted quantity in exact recorded units. Missing or altered decision proof is unavailable. Complete consumption by other invoices, stock movement, matching policy and financial approval are separate evidence.");
            foreach(var (code,label,evidence) in new[]{
                ("ledger","Accounting book, period and ledger outcome","No owned invoice-to-book/period/journal posting evidence is established by this source review."),
                ("decision","Independent financial decision","No durable invoice matching/approval/payment decision is recorded by these diagnostics.")})
                Add(code,label,APSourceComparisonOutcome.Unavailable,evidence);
            rows.AddRange(PolicyChecks);
            rows.AddRange(ReceiptConsumptionChecks);
            return rows.AsReadOnly();
        }
    }
}
