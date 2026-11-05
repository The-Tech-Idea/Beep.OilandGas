using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;

public sealed record APInvoiceVarianceCommand(CompanyScope Company,string ActorUserId,string InvoiceId,APInvoiceVarianceProposal Proposal,
    string ExpectedPreparationRevision,string BindingRevision,string IntentId)
{
    public void RequireShape()
    {
        PurchaseOrderOwnership.RequireScope(Company);
        static bool Text(string? value)=>!string.IsNullOrWhiteSpace(value) && value.Length<=128 && !value.Any(char.IsControl);
        if(!Text(ActorUserId) || ActorUserId=="SYSTEM" || !Text(InvoiceId) || Proposal is null ||
            !ProcessDefinitionRevision.IsValid(ExpectedPreparationRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) ||
            !Guid.TryParseExact(IntentId,"N",out var intent) || intent==Guid.Empty || intent.ToString("N")!=IntentId)
            throw new InvalidOperationException("The original variance company, account, invoice, preparation, binding and intent are required.");
        Proposal.RequireShape();
        if(ExpectedPreparationRevision!=APInvoiceVarianceProposalReview.RevisionFor(Company,ActorUserId,InvoiceId,Proposal,BindingRevision))
            throw new InvalidOperationException("The original variance preparation revision does not match its exact account, binding and proposal.");
    }
    public void Require(APInvoiceVarianceProposalReview review)
    {
        RequireShape();review.Require(Company,ActorUserId,InvoiceId,Proposal);
        if(review.Revision!=ExpectedPreparationRevision || review.BindingRevision!=BindingRevision || review.Financial.Invoice.Invoice.VARIANCE_PROPOSAL_JSON is not null)
            throw new InvalidOperationException("The exact original preparation is required and cannot replace a recorded variance proposal.");
    }
    public bool Same(APInvoiceVarianceCommand other)=>JsonSerializer.Serialize(this)==JsonSerializer.Serialize(other);
}

/// <summary>Verified recorded proposal only; no controller disposition or invoice financial approval.</summary>
public sealed record APInvoiceVarianceProof(APInvoiceVarianceCommand Command,AP_INVOICE OriginalInvoice,string RowRevision)
{
    public const int MaximumLength=32768;
    private static readonly JsonSerializerOptions Options=new(){UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};
    private static string Digest(AP_INVOICE row){var copy=PurchaseReceiptDecisionProof.Copy(row);copy.VARIANCE_PROPOSAL_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(APInvoiceVarianceCommand command,AP_INVOICE original,AP_INVOICE recorded)
    {
        command.RequireShape();APInvoiceOwnership.Require(command.Company,command.InvoiceId,original);
        if(original.VARIANCE_PROPOSAL_JSON is not null || original.STATUS!="DRAFT" || original.ACTIVE_IND!="Y" || string.IsNullOrWhiteSpace(original.ROW_CREATED_BY) ||
            original.ROW_CREATED_BY is "SYSTEM" || original.ROW_CREATED_BY==command.ActorUserId || original.ROW_CREATED_DATE is not {} created || created==default || created.Kind!=DateTimeKind.Utc ||
            !string.IsNullOrWhiteSpace(original.ROW_CHANGED_BY) || original.ROW_CHANGED_DATE is not null ||
            recorded.ROW_CHANGED_BY!=command.ActorUserId || recorded.ROW_CHANGED_DATE is not {} changed || changed==default || changed.Kind!=DateTimeKind.Utc)
            throw new InvalidOperationException("An untouched owned draft and exact accountant change audit are required.");
        var expected=PurchaseReceiptDecisionProof.Copy(original);expected.ROW_CHANGED_BY=recorded.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=recorded.ROW_CHANGED_DATE;
        if(Digest(expected)!=Digest(recorded))throw new InvalidOperationException("Variance submission cannot change invoice financial or source values.");
        var value=JsonSerializer.Serialize(new APInvoiceVarianceProof(command,PurchaseReceiptDecisionProof.Copy(original),Digest(recorded)));
        if(value.Length>MaximumLength)throw new InvalidOperationException("The complete variance command exceeds the bounded Beep proposal column.");return value;
    }
    public static APInvoiceVarianceProof Require(CompanyScope company,AP_INVOICE row)
    {
        if(row?.VARIANCE_DECISION_JSON is not null)
            return Require(company,APInvoiceVarianceDecisionProof.Require(company,row).OriginalInvoice);
        if(row?.VARIANCE_PROPOSAL_JSON is not {Length:>0} json || json.Length>MaximumLength)throw new InvalidOperationException("The recorded variance proposal proof is unavailable.");
        APInvoiceVarianceProof proof;
        try{proof=JsonSerializer.Deserialize<APInvoiceVarianceProof>(json,Options)??throw new JsonException();}catch(JsonException){throw new InvalidOperationException("The recorded variance proposal proof is incompatible.");}
        if(proof.Command is null || proof.OriginalInvoice is null)throw new InvalidOperationException("The original variance command and invoice are required.");
        proof.Command.RequireShape();APInvoiceOwnership.Require(company,proof.Command.InvoiceId,row);
        if(proof.Command.Company!=company || proof.RowRevision!=Digest(row) || Create(proof.Command,proof.OriginalInvoice,row)!=json)
            throw new InvalidOperationException("The recorded variance proposal differs from its complete original command and invoice.");
        return proof;
    }
}
public sealed record APInvoiceVarianceSubmission(APInvoiceVarianceCommand Command,APInvoiceVarianceProposalReview Review);
public sealed record APInvoiceVarianceResult(APInvoiceVarianceCommand Command,AP_INVOICE Invoice)
{
    public void Require(APInvoiceVarianceSubmission submission)
    {
        submission.Command.Require(submission.Review);var proof=APInvoiceVarianceProof.Require(submission.Command.Company,Invoice);
        if(Command is null || !Command.Same(submission.Command) || !proof.Command.Same(Command) ||
            JsonSerializer.Serialize(proof.OriginalInvoice)!=JsonSerializer.Serialize(submission.Review.Financial.Invoice.Invoice))
            throw new InvalidOperationException("The complete original variance proposal outcome is unconfirmed.");
    }
}
