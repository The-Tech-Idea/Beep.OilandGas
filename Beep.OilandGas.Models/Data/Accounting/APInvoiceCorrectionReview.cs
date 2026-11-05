using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using System.Text.Json;
namespace Beep.OilandGas.Models.Data.Accounting;

public sealed record APInvoiceCorrectionLine(string LineId,decimal Quantity,decimal UnitPrice,decimal LineTotal,string? Description);
/// <summary>Explicit proposed values; not a recorded source or financial approval.</summary>
public sealed record APInvoiceCorrectionDraft(string ExpectedInvoiceRevision,string ExpectedFinancialRevision,string DecisionIntentId,
    string ExpectedDecisionRevision,decimal TotalAmount,string Reason,IReadOnlyList<APInvoiceCorrectionLine> Lines)
{
    public void RequireShape()
    {
        if(new[]{ExpectedInvoiceRevision,ExpectedFinancialRevision,ExpectedDecisionRevision}.Any(value=>!ProcessDefinitionRevision.IsValid(value)) ||
            !Guid.TryParseExact(DecisionIntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=DecisionIntentId ||
            TotalAmount<=0 || string.IsNullOrWhiteSpace(Reason) || Reason.Length>1000 || Reason.Any(char.IsControl) || Lines is null || Lines.Count is <1 or >128 ||
            Lines.Any(line=>line is null || string.IsNullOrWhiteSpace(line.LineId) || line.LineId.Length>128 || line.LineId.Any(char.IsControl) ||
                line.Quantity<=0 || line.UnitPrice<0 || line.LineTotal<0 || line.Description is {} description && (description.Length>1024 || description.Any(char.IsControl))) ||
            Lines.Select(line=>line.LineId).Distinct(StringComparer.Ordinal).Count()!=Lines.Count)
            throw new InvalidOperationException("Exact correction evidence, controller request, reason and complete bounded line values are required.");
        try
        {
            if(Lines.Any(line=>line.Quantity*line.UnitPrice!=line.LineTotal) || Lines.Sum(line=>line.LineTotal)!=TotalAmount)
                throw new InvalidOperationException("Explicit correction line arithmetic and declared invoice total must agree; rounding cannot be inferred.");
        }
        catch(OverflowException invalid){throw new InvalidOperationException("Correction values exceed the decimal source contract.",invalid);}
    }
}
public sealed record APInvoiceCorrectionReview(APInvoiceFinancialReview Current,APInvoiceVarianceDecisionProof RecordedRequest,
    APInvoiceCorrectionDraft Draft,string BindingRevision,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static string DecisionRevision(string json)=>ProcessDefinitionRevision.FromJson(json);
    public static APInvoiceCorrectionReview Create(APInvoiceFinancialReview current,APInvoiceCorrectionDraft draft,string binding)
    {
        ArgumentNullException.ThrowIfNull(current);ArgumentNullException.ThrowIfNull(draft);
        current=PurchaseReceiptDecisionProof.Copy(current);draft=PurchaseReceiptDecisionProof.Copy(draft);
        var proof=APInvoiceVarianceDecisionProof.Require(current.Invoice.Company,current.Invoice.Invoice);
        var result=new APInvoiceCorrectionReview(current,proof,draft,binding,Digest(current,proof,draft,binding),DateTimeOffset.UtcNow);
        result.Require(current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,draft);return result;
    }
    public void Require(CompanyScope company,string actor,string id,APInvoiceCorrectionDraft expected)
    {
        ArgumentNullException.ThrowIfNull(expected);expected.RequireShape();
        if(Current?.Invoice is null || RecordedRequest?.Command is null || Draft is null || string.IsNullOrWhiteSpace(actor) || actor=="SYSTEM" || actor.Length>128 || actor.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(BindingRevision) || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("A complete addressed correction review and production binding are required.");
        Draft.RequireShape();Current.Require(company,actor,id,expected.ExpectedInvoiceRevision);
        var source=Current.Invoice;var proof=APInvoiceVarianceDecisionProof.Require(company,source.Invoice);var proposal=APInvoiceVarianceProof.Require(company,source.Invoice);
        if(actor!=proposal.Command.ActorUserId)throw new UnauthorizedAccessException("Only the recorded original proposal preparer can review invoice correction.");
        if(proof.Command.Disposition!="REQUEST_CORRECTION" || proof.Command.IntentId!=expected.DecisionIntentId || proof.Command.BindingRevision!=BindingRevision ||
            DecisionRevision(source.Invoice.VARIANCE_DECISION_JSON!)!=expected.ExpectedDecisionRevision || Current.Revision!=expected.ExpectedFinancialRevision ||
            Current.MatchingPolicies is null || Current.ReceiptConsumption is null || source.Invoice.STATUS!="DRAFT" || source.Invoice.ACTIVE_IND!="Y" ||
            JsonSerializer.Serialize(Draft)!=JsonSerializer.Serialize(expected) || JsonSerializer.Serialize(proof)!=JsonSerializer.Serialize(RecordedRequest) ||
            !source.Lines.Select(line=>line.AP_INVOICE_LINE_ID).Order(StringComparer.Ordinal).SequenceEqual(Draft.Lines.Select(line=>line.LineId).Order(StringComparer.Ordinal)) ||
            source.Lines.Any(line=>line.ACTIVE_IND!="Y") || Revision!=Digest(Current,RecordedRequest,Draft,BindingRevision))
            throw new InvalidOperationException("The correction differs from the current complete source, requested disposition, binding or exact proposed values.");
        if(source.Invoice.TOTAL_AMOUNT==Draft.TotalAmount && source.Lines.All(line=>{
            var proposed=Draft.Lines.Single(row=>row.LineId==line.AP_INVOICE_LINE_ID);
            return line.QUANTITY==proposed.Quantity && line.UNIT_PRICE==proposed.UnitPrice && line.LINE_TOTAL==proposed.LineTotal && line.ITEM_DESCRIPTION==proposed.Description;}))
            throw new InvalidOperationException("An unchanged invoice cannot be presented as a correction.");
    }
    private static string Digest(APInvoiceFinancialReview current,APInvoiceVarianceDecisionProof proof,APInvoiceCorrectionDraft draft,string binding)=>
        ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,current.Revision,proof,draft,binding}));
}
