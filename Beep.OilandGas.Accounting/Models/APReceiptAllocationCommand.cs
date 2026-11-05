using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Accounting.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Accounting.Models;

/// <summary>Original preparer intent. Recording this does not approve receipt acceptance, matching or posting.</summary>
public sealed record APReceiptAllocationCommand(CompanyScope Company,string ActorUserId,string InvoiceId,string LineId,
    APReceiptAllocationProposal Proposal,string ExpectedConsumptionRevision,string ExpectedProposalRevision,
    string BindingRevision,string IntentId,string Reason)
{
    [JsonIgnore] public string AllocationId=>"ap-allocation-"+ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{Company,ActorUserId,IntentId}));
    [JsonIgnore] public string ProofJson=>JsonSerializer.Serialize(this);
    public void Require(APReceiptAllocationProposalReview review)
    {
        APInvoiceOwnership.RequireScope(Company);
        static bool Text(string? value,int max)=>!string.IsNullOrWhiteSpace(value) && value.Length<=max && !value.Any(char.IsControl);
        if(!Text(ActorUserId,128) || ActorUserId=="SYSTEM" || !Text(InvoiceId,128) || !Text(LineId,128) ||
            !Text(BindingRevision,512) || !Text(Reason,512) || !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty ||
            IntentId!=intent.ToString("N") || Proposal is null || !ProcessDefinitionRevision.IsValid(ExpectedConsumptionRevision) ||
            !ProcessDefinitionRevision.IsValid(ExpectedProposalRevision) || review is null)
            throw new InvalidOperationException("An original company, actor, source, binding, reviewed receipt, intent and reason are required.");
        review.Require(Company,ActorUserId,InvoiceId,LineId,Proposal);
        if(review.Revision!=ExpectedProposalRevision || review.ReceiptConsumption?.Revision!=ExpectedConsumptionRevision || review.BindingRevision!=BindingRevision || ProofJson.Length>4000)
            throw new InvalidOperationException("The original proposal and complete receipt contributors must be reviewed again.");
        var source=review.Source.Source;
        var line=source.Invoice.Lines.Single(row=>row.AP_INVOICE_LINE_ID==LineId);
        var orderLine=source.Order.Lines.Single(row=>row.PO_LINE_ITEM_ID==line.PO_LINE_ITEM_ID);
        if(source.Invoice.Invoice.ACTIVE_IND!="Y" || source.Invoice.Invoice.STATUS!="DRAFT" || line.ACTIVE_IND!="Y" ||
            source.Order.Header.ACTIVE_IND!="Y" || orderLine.ACTIVE_IND!="Y")
            throw new InvalidOperationException("Allocation recording requires an active draft invoice line and active owned purchase source.");
        foreach(var code in new[]{"units","invoice-quantity","receipt-quantity","acceptance"})
        {
            var expected=code=="units"?APSourceComparisonOutcome.RecordedAgreement:APSourceComparisonOutcome.WithinRecordedLimit;
            if(review.Comparisons.Single(row=>row.Code==code).Outcome!=expected)
                throw new InvalidOperationException("The allocation requires exact recorded units, verified receiver acceptance and capacity across complete receipt contributors.");
        }
    }
}

public sealed record APReceiptAllocationSubmission(APReceiptAllocationCommand Command,APReceiptAllocationProposalReview Review);

/// <summary>Confirmed allocation evidence, separate from receipt acceptance and any financial decision.</summary>
public sealed record APReceiptAllocationReceipt(APReceiptAllocationCommand Command,AP_RECEIPT_ALLOCATION Allocation,DateTimeOffset ConfirmedAtUtc)
{
    public static APReceiptAllocationReceipt Create(APReceiptAllocationCommand command,APReceiptAllocationProposalReview review,AP_RECEIPT_ALLOCATION row)
    {
        var result=new APReceiptAllocationReceipt(command,JsonSerializer.Deserialize<AP_RECEIPT_ALLOCATION>(JsonSerializer.Serialize(row))!,DateTimeOffset.UtcNow);
        result.Require(command,review);return result;
    }
    public void Require(APReceiptAllocationCommand expected,APReceiptAllocationProposalReview review)
    {
        expected.Require(review);
        if(Command!=expected || Allocation is null || ConfirmedAtUtc==default || ConfirmedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("The allocation outcome does not match the original intent.");
        APReceiptAllocationRecordedProof.Require(expected,Allocation);
        APReceiptAllocationReview.RequireOwnership(expected.Company,[Allocation]);
        APReceiptAllocationReview.RequireRows(expected.InvoiceId,expected.LineId,[Allocation]);
        var line=review.Source.Source.Invoice.Lines.Single(item=>item.AP_INVOICE_LINE_ID==expected.LineId);
        if(Allocation.AP_RECEIPT_ALLOCATION_ID!=expected.AllocationId || Allocation.PURCHASE_ORDER_ID!=line.PURCHASE_ORDER_ID ||
            Allocation.PO_LINE_ITEM_ID!=line.PO_LINE_ITEM_ID || Allocation.PO_RECEIPT_ID!=expected.Proposal.ReceiptId ||
            Allocation.ALLOCATED_QUANTITY!=expected.Proposal.Quantity || Allocation.QUANTITY_UOM!=expected.Proposal.Unit || Allocation.ACTIVE_IND!="Y" ||
            Allocation.ROW_CREATED_BY!=expected.ActorUserId || Allocation.ROW_CREATED_DATE is null || Allocation.ROW_CREATED_DATE==default(DateTime) ||
            Allocation.ROW_CREATED_DATE.Value.Kind!=DateTimeKind.Utc || Allocation.ROW_CHANGED_BY!=expected.ActorUserId ||
            Allocation.ROW_CHANGED_DATE is null || Allocation.ROW_CHANGED_DATE.Value.Kind!=DateTimeKind.Utc || Allocation.ROW_CHANGED_DATE<Allocation.ROW_CREATED_DATE ||
            Allocation.REMARK!=expected.Reason || Allocation.PPDM_GUID!=Guid.ParseExact(expected.IntentId,"N").ToString() ||
            Allocation.SOURCE!="AP_RECEIPT_ALLOCATION" || Allocation.EFFECTIVE_DATE is not null || Allocation.EXPIRY_DATE is not null ||
            Allocation.ROW_EFFECTIVE_DATE is not null || Allocation.ROW_EXPIRY_DATE is not null || !string.IsNullOrEmpty(Allocation.ROW_QUALITY))
            throw new InvalidOperationException("The allocation outcome lacks exact unchanged original source and audit evidence.");
    }
}

/// <summary>Durable original intent and exact canonical row content after Beep common-column preparation.</summary>
public sealed record APReceiptAllocationRecordedProof(APReceiptAllocationCommand Command,string RowRevision)
{
    public static string Create(APReceiptAllocationCommand command,AP_RECEIPT_ALLOCATION row)
    {
        var json=JsonSerializer.Serialize(new APReceiptAllocationRecordedProof(command,Digest(row)));
        if(json.Length>4000)throw new InvalidOperationException("The allocation proof exceeds its migrated bound.");
        return json;
    }
    public static void Require(APReceiptAllocationCommand command,AP_RECEIPT_ALLOCATION row)
    {
        if(string.IsNullOrEmpty(row.REVIEW_COMMAND_JSON) || row.REVIEW_COMMAND_JSON.Length>4000)
            throw new InvalidOperationException("The allocation has no bounded original command proof.");
        APReceiptAllocationRecordedProof? proof;
        try{proof=JsonSerializer.Deserialize<APReceiptAllocationRecordedProof>(row.REVIEW_COMMAND_JSON);}
        catch(JsonException){throw new InvalidOperationException("The allocation command proof is malformed.");}
        if(proof?.Command!=command || proof.RowRevision!=Digest(row) || Create(command,row)!=row.REVIEW_COMMAND_JSON)
            throw new InvalidOperationException("The original allocation intent or recorded row content changed.");
    }
    private static string Digest(AP_RECEIPT_ALLOCATION row)
    {
        var snapshot=JsonSerializer.Deserialize<AP_RECEIPT_ALLOCATION>(JsonSerializer.Serialize(row))!;
        snapshot.REVIEW_COMMAND_JSON=null;
        return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(snapshot));
    }
}
