using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Preparer input for read-only source review; not a persisted allocation or matching approval.</summary>
public sealed record APReceiptAllocationProposal(string ReceiptId,decimal Quantity,string Unit,
    string ExpectedInvoiceRevision,string ExpectedOrderRevision,string ExpectedAllocationRevision)
{
    public void Require(CompanyScope company,string actor,string invoiceId,string lineId,APReceiptAllocationReview source)
    {
        if(source is null || !ProcessDefinitionRevision.IsValid(ExpectedInvoiceRevision) ||
            !ProcessDefinitionRevision.IsValid(ExpectedOrderRevision) || !ProcessDefinitionRevision.IsValid(ExpectedAllocationRevision))
            throw new InvalidOperationException("Current reviewed invoice, order and allocation evidence is required.");
        source.Require(company,actor,invoiceId,lineId,ExpectedInvoiceRevision);
        if(source.Source.Order.Revision!=ExpectedOrderRevision || source.Revision!=ExpectedAllocationRevision)
            throw new InvalidOperationException("The displayed order or allocation history changed.");
        if(string.IsNullOrWhiteSpace(ReceiptId) || ReceiptId.Length>128 || ReceiptId.Any(char.IsControl) || Quantity<=0 ||
            string.IsNullOrWhiteSpace(Unit) || Unit.Length>64 || Unit.Any(char.IsControl))
            throw new InvalidOperationException("An explicit receipt, positive quantity and recorded unit are required.");
        var line=source.Source.Invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==lineId);
        var receipt=source.Source.Order.Receipts.SingleOrDefault(row=>row.PO_RECEIPT_ID==ReceiptId && row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
        if(receipt is null || receipt.ACTIVE_IND!="Y" || receipt.RECEIVED_QUANTITY is not >0 || receipt.QUANTITY_UOM!=Unit || Quantity>receipt.RECEIVED_QUANTITY)
            throw new InvalidOperationException("The proposal does not fit this active receipt's recorded line, quantity and unit.");
    }
}

public sealed record APReceiptAllocationProposalReview(APReceiptAllocationProposal Proposal,APReceiptAllocationReview Source,
    string Revision,DateTimeOffset RetrievedAtUtc)
{
    public APReceiptConsumptionReview? ReceiptConsumption {get;init;}
    public string? BindingRevision {get;init;}
    public IReadOnlyList<APSourceComparison> Comparisons=>APReceiptAllocationComparisons.Evaluate(this);
    public static APReceiptAllocationProposalReview Create(CompanyScope company,string actor,string invoiceId,string lineId,
        APReceiptAllocationProposal proposal,APReceiptAllocationReview source,APReceiptConsumptionReview? consumption=null,string? bindingRevision=null)
    {
        proposal.Require(company,actor,invoiceId,lineId,source);
        var frozen=JsonSerializer.Deserialize<APReceiptAllocationReview>(JsonSerializer.Serialize(source))!;
        var frozenConsumption=consumption is null?null:JsonSerializer.Deserialize<APReceiptConsumptionReview>(JsonSerializer.Serialize(consumption));
        frozenConsumption?.Require(company,actor,frozen.Source,proposal.ReceiptId);
        return new(proposal,frozen,Digest(proposal,frozen,frozenConsumption,bindingRevision),DateTimeOffset.UtcNow){ReceiptConsumption=frozenConsumption,BindingRevision=bindingRevision};
    }
    public void Require(CompanyScope company,string actor,string invoiceId,string lineId,APReceiptAllocationProposal expected)
    {
        if(Proposal is null || Proposal!=expected || Source is null)throw new InvalidOperationException("The returned proposal does not match the preparer's input.");
        Proposal.Require(company,actor,invoiceId,lineId,Source);
        ReceiptConsumption?.Require(company,actor,Source.Source,Proposal.ReceiptId);
        if(BindingRevision is not null && (string.IsNullOrWhiteSpace(BindingRevision) || BindingRevision.Length>512 || BindingRevision.Any(char.IsControl)))
            throw new InvalidOperationException("The proposal binding evidence is invalid.");
        if(!ProcessDefinitionRevision.IsValid(Revision) || Revision!=Digest(Proposal,Source,ReceiptConsumption,BindingRevision) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The proposal source review changed or its provenance is unavailable.");
    }
    private static string Digest(APReceiptAllocationProposal proposal,APReceiptAllocationReview source,APReceiptConsumptionReview? consumption,string? bindingRevision)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{proposal,SourceRevision=source.Revision,ConsumptionRevision=consumption?.Revision,bindingRevision}));
}
