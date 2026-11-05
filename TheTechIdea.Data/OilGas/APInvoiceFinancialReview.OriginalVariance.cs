namespace TheTechIdea.Data.OilGas;
public sealed partial record APInvoiceFinancialReview
{
    /// <summary>Reconstructs original evidence only from verified own command proof; all other current sources remain current.</summary>
    public APInvoiceFinancialReview OriginalVarianceEvidence(APInvoiceVarianceCommand command)
    {
        Require(command.Company,command.ActorUserId,command.InvoiceId,Invoice.Revision);
        var proof=APInvoiceVarianceProof.Require(command.Company,Invoice.Invoice);
        if(!proof.Command.Same(command))throw new InvalidOperationException("Another variance intent is recorded. Do not overwrite or repair it.");
        return RecordedVarianceEvidence();
    }
    /// <summary>Current evidence around the verified original header, addressed to the current reader; not historical-source approval.</summary>
    public APInvoiceFinancialReview RecordedVarianceEvidence()
    {
        var company=Invoice.Company;var actor=Invoice.ActorUserId;var id=Invoice.Invoice.AP_INVOICE_ID;Require(company,actor,id,Invoice.Revision);
        var proof=APInvoiceVarianceProof.Require(company,Invoice.Invoice);
        return RebuildInvoice(proof.OriginalInvoice);
    }
    public APInvoiceFinancialReview OriginalVarianceDecisionEvidence(APInvoiceVarianceDecisionCommand command)
    {
        Require(command.Company,command.ActorUserId,command.InvoiceId,Invoice.Revision);
        var proof=APInvoiceVarianceDecisionProof.Require(command.Company,Invoice.Invoice);
        if(!proof.Command.Same(command))throw new InvalidOperationException("Another controller intent is recorded. Do not replace or repair it.");
        return RebuildInvoice(proof.OriginalInvoice);
    }
    public APInvoiceFinancialReview OriginalCorrectionEvidence(APInvoiceCorrectionCommand command)
    {
        Require(command.Company,command.ActorUserId,command.InvoiceId,Invoice.Revision);
        var history=APInvoiceCorrectionHistory.Require(command.Company,Invoice.Invoice,Invoice.Lines);
        if(!history.Versions[^1].Command.Same(command))throw new InvalidOperationException("The latest recorded correction differs from this original intent. Do not overwrite or repair it.");
        var before=history.SourceBeforeLast();return RebuildInvoice(before.Invoice,before.Lines);
    }
    private APInvoiceFinancialReview RebuildInvoice(global::Beep.OilandGas.Models.Data.ProductionAccounting.AP_INVOICE header,
        IReadOnlyList<global::Beep.OilandGas.Models.Data.ProductionAccounting.AP_INVOICE_LINE>? originalLines=null)
    {
        var company=Invoice.Company;var actor=Invoice.ActorUserId;var id=Invoice.Invoice.AP_INVOICE_ID;
        var original=APInvoiceSourceReview.Create(company,actor,header,originalLines??Invoice.Lines);
        APLinePurchaseSourceReview Source(APLinePurchaseSourceReview source)=>source.Invoice.Invoice.AP_INVOICE_ID==id?new(original,source.InvoiceLineId,source.Order):source;
        APReceiptAllocationReview Allocation(APReceiptAllocationReview allocation)
        {
            var source=Source(allocation.Source);
            return APReceiptAllocationReview.Create(company,actor,source.Invoice.Invoice.AP_INVOICE_ID,source.InvoiceLineId,source.Invoice.Revision,source,allocation.Allocations);
        }
        var purchases=PurchaseLines.Select(Allocation).ToArray();
        var consumption=ReceiptConsumption?.Select(row=>APReceiptConsumptionReview.Create(company,actor,Source(row.Source),row.ReceiptId,row.Contributors.Select(Allocation).ToArray())).ToArray();
        return Create(original,purchases,MatchingPolicies,consumption);
    }
}
