using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using System.Globalization;
using System.Numerics;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed record APMatchingPolicySelection(string? PolicyId,CompanyMatchingPolicyRules? Rules,string Evidence);
public sealed partial record APInvoiceFinancialReview
{
    public APMatchingPolicySelection PolicySelection
    {
        get
        {
            Require(Invoice.Company,Invoice.ActorUserId,Invoice.Invoice.AP_INVOICE_ID,Invoice.Revision);
            APMatchingPolicySelection Missing(string reason)=>new(null,null,reason);
            if(MatchingPolicies is null)return Missing("Complete company matching-policy history is unavailable.");
            var invoice=Invoice.Invoice;
            if(invoice.ACTIVE_IND!="Y" || string.IsNullOrWhiteSpace(invoice.CURRENCY_CODE) || invoice.INVOICE_DATE is not {} date || date==default || date.Kind!=DateTimeKind.Utc)
                return Missing("An active invoice with explicit recorded currency and full UTC invoice date is required. No date or timezone is inferred.");
            var candidates=MatchingPolicies.Records.Where(row=>row.ACTIVE_IND=="Y" && row.MATCHING_POLICY_STATUS=="APPROVED" &&
                CompanyMatchingPolicyReview.Rules(row).CurrencyCode==invoice.CURRENCY_CODE && row.EFFECTIVE_DATE<=date && (row.EXPIRY_DATE is null || row.EXPIRY_DATE>=date)).ToArray();
            if(candidates.Length!=1)return Missing($"Applicable recorded approved policies for invoice currency/date: {candidates.Length}. Exactly one is required; no newest-row fallback is applied.");
            var row=candidates[0];
            if(string.IsNullOrWhiteSpace(row.MATCHING_POLICY_COMMAND_JSON))return Missing("The applicable recorded policy lacks complete verified independent approval evidence.");
            var proof=CompanyMatchingPolicyProof.Require(Invoice.Company,row);
            if(proof.Command.Action!="APPROVE")return Missing("Independent policy approval is unconfirmed.");
            return new(row.ACCOUNTING_METHOD_ID,CompanyMatchingPolicyReview.Rules(row),
                $"Verified applicable policy: {row.ACCOUNTING_METHOD_ID}; version: {proof.Command.Rules.PolicyVersion}; currency: {invoice.CURRENCY_CODE}; invoice UTC date: {date:O}; effective: {row.EFFECTIVE_DATE:O}; expires: {row.EXPIRY_DATE:O}; reviewer: {proof.Command.ActorUserId}; preparer: {proof.Original!.ROW_CREATED_BY}; complete policy history revision: {MatchingPolicies.Revision}. This selects diagnostic rules, not invoice financial authority.");
        }
    }
    private IReadOnlyList<APSourceComparison> PolicyChecks
    {
        get
        {
            var selected=PolicySelection;var rules=selected.Rules;var rows=new List<APSourceComparison>();
            rows.Add(new("policy","Verified effective company matching policy",rules is null?APSourceComparisonOutcome.Unavailable:APSourceComparisonOutcome.RecordedAgreement,selected.Evidence));
            foreach(var line in Invoice.Lines.Where(row=>row.ACTIVE_IND=="Y"))
            {
                var purchase=PurchaseLines.SingleOrDefault(row=>row.Source.InvoiceLineId==line.AP_INVOICE_LINE_ID);
                var order=purchase?.Source.Order;var orderLine=order?.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
                var units=!string.IsNullOrWhiteSpace(line.QUANTITY_UOM) && line.QUANTITY_UOM==orderLine?.QUANTITY_UOM;
                var currency=!string.IsNullOrWhiteSpace(Invoice.Invoice.CURRENCY_CODE) && line.CURRENCY_CODE==Invoice.Invoice.CURRENCY_CODE && order?.Header.CURRENCY_CODE==Invoice.Invoice.CURRENCY_CODE;
                var known=rules is not null && units && currency && order?.Header.ACTIVE_IND=="Y" && orderLine?.ACTIVE_IND=="Y";
                APSourceComparisonOutcome Compare(decimal? value,decimal? basis,decimal? tolerance)=>!known || value is not >=0 || basis is not >0 || tolerance is null?APSourceComparisonOutcome.Unavailable:
                    Within(value.Value,basis.Value,tolerance.Value)?APSourceComparisonOutcome.WithinRecordedLimit:APSourceComparisonOutcome.ExceedsRecordedLimit;
                string Number(decimal? value)=>value?.ToString(CultureInfo.InvariantCulture) ?? "Unavailable";
                rows.Add(new("policy-price/"+line.AP_INVOICE_LINE_ID,"Policy price variance for "+line.AP_INVOICE_LINE_ID,Compare(line.UNIT_PRICE,orderLine?.UNIT_PRICE,rules?.PriceTolerancePercent),
                    $"Policy: {selected.PolicyId ?? "Unavailable"}; invoice unit price: {Number(line.UNIT_PRICE)}; recorded PO unit price basis: {Number(orderLine?.UNIT_PRICE)}; price tolerance (%): {Number(rules?.PriceTolerancePercent)}; exact units: {units}; exact currencies: {currency}. Requires positive PO basis; no conversion or rounding is applied."));
                rows.Add(new("policy-quantity/"+line.AP_INVOICE_LINE_ID,"Policy quantity variance for "+line.AP_INVOICE_LINE_ID,Compare(line.QUANTITY is >0?line.QUANTITY:null,orderLine?.QUANTITY,rules?.QuantityTolerancePercent),
                    $"Policy: {selected.PolicyId ?? "Unavailable"}; invoice quantity: {Number(line.QUANTITY)}; full recorded PO line quantity basis: {Number(orderLine?.QUANTITY)}; quantity tolerance (%): {Number(rules?.QuantityTolerancePercent)}; exact units: {units}; exact currencies: {currency}. This compares the full PO line, not remaining PO balance, accepted receipt capacity or consumption by other invoices."));
            }
            rows.Add(new("policy-tax","Policy tax treatment",APSourceComparisonOutcome.Unavailable,rules is null?"Verified applicable policy is unavailable.":"The verified policy requires separate tax review; no tax decision or amount is inferred."));
            rows.Add(new("policy-non-po","Policy non-PO treatment",APSourceComparisonOutcome.Unavailable,rules is null?"Verified applicable policy is unavailable.":"The verified policy requires separate non-PO review; no non-PO invoice permission is inferred."));
            rows.Add(new("policy-rounding","Policy source amount rule",rules is null?APSourceComparisonOutcome.Unavailable:APSourceComparisonOutcome.RecordedAgreement,
                rules is null?"Verified applicable policy is unavailable.":"The verified policy requires exact source amounts. Existing header/line arithmetic checks remain authoritative diagnostics; no rounding adjustment is introduced."));
            return rows.AsReadOnly();
        }
    }
    private static bool Within(decimal observed,decimal basis,decimal tolerance)
    {
        static (BigInteger Value,int Scale) Parts(decimal value)
        {var bits=decimal.GetBits(value);return(((BigInteger)(uint)bits[2]<<64)+((BigInteger)(uint)bits[1]<<32)+(uint)bits[0],(bits[3]>>16)&255);}
        var value=Parts(observed);var reference=Parts(basis);var limit=Parts(tolerance);var scale=Math.Max(value.Scale,reference.Scale);
        var scaledValue=value.Value*BigInteger.Pow(10,scale-value.Scale);var scaledBasis=reference.Value*BigInteger.Pow(10,scale-reference.Scale);
        return BigInteger.Abs(scaledValue-scaledBasis)*100*BigInteger.Pow(10,limit.Scale)<=scaledBasis*limit.Value;
    }
}
