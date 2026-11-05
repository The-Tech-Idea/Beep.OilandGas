using Beep.OilandGas.Models.Data.Process;
using Beep.OilandGas.Models.Data.Accounting;
using Beep.OilandGas.Models.Data.Company;
using Beep.OilandGas.Models.Data.AccessControl;
using Beep.OilandGas.PPDM39.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace Beep.OilandGas.Models.Data.Accounting;
public sealed record APInvoiceVarianceDecisionCommand(CompanyScope Company,string ActorUserId,string InvoiceId,string ProposalIntentId,
    string Disposition,string Reason,string ExpectedInvoiceRevision,string ExpectedReviewRevision,string BindingRevision,string IntentId)
{
    public void RequireShape()
    {
        PurchaseOrderOwnership.RequireScope(Company);
        static bool Id(string? text)=>!string.IsNullOrWhiteSpace(text) && text.Length<=128 && !text.Any(char.IsControl);
        static bool Intent(string? text)=>Guid.TryParseExact(text,"N",out var id) && id!=Guid.Empty && id.ToString("N")==text;
        if(!Id(ActorUserId) || ActorUserId=="SYSTEM" || !Id(InvoiceId) || !Intent(ProposalIntentId) || !Intent(IntentId) ||
            Disposition is not ("REQUEST_CORRECTION" or "ACCEPT_WITHIN_POLICY_VARIANCE") || string.IsNullOrWhiteSpace(Reason) || Reason.Length>1000 || Reason.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(ExpectedInvoiceRevision) || !ProcessDefinitionRevision.IsValid(ExpectedReviewRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision))
            throw new InvalidOperationException("Exact controller account, proposal, disposition, reason, evidence, binding and original intent are required.");
    }
    public void Require(APInvoiceVarianceDecisionReview review)
    {
        RequireShape();ArgumentNullException.ThrowIfNull(review);review.Require(Company,ActorUserId,InvoiceId,ExpectedInvoiceRevision);
        if(review.Revision!=ExpectedReviewRevision || review.BindingRevision!=BindingRevision || review.RecordedProposal.Command.IntentId!=ProposalIntentId ||
            !review.EligibleDispositions.Contains(Disposition,StringComparer.Ordinal))throw new InvalidOperationException("The disposition differs from its exact independently reviewed current evidence.");
    }
    public bool Same(APInvoiceVarianceDecisionCommand other)=>JsonSerializer.Serialize(this)==JsonSerializer.Serialize(other);
}
/// <summary>Immutable first controller disposition; preserves proposal evidence and predecision audit, not financial approval.</summary>
public sealed record APInvoiceVarianceDecisionProof(APInvoiceVarianceDecisionCommand Command,AP_INVOICE OriginalInvoice,string RowRevision)
{
    public const int MaximumLength=65536;
    private static readonly JsonSerializerOptions Options=new(){UnmappedMemberHandling=JsonUnmappedMemberHandling.Disallow};
    private static string Digest(AP_INVOICE row){var copy=PurchaseReceiptDecisionProof.Copy(row);copy.VARIANCE_DECISION_JSON=null;return ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(copy));}
    public static string Create(APInvoiceVarianceDecisionCommand command,AP_INVOICE original,AP_INVOICE recorded)
    {
        command.RequireShape();APInvoiceOwnership.Require(command.Company,command.InvoiceId,original);
        if(original.VARIANCE_DECISION_JSON is not null)throw new InvalidOperationException("The recorded first controller disposition cannot be replaced.");
        var proposal=APInvoiceVarianceProof.Require(command.Company,original);
        if(proposal.Command.IntentId!=command.ProposalIntentId || command.BindingRevision!=proposal.Command.BindingRevision || command.ActorUserId==proposal.Command.ActorUserId ||
            command.ActorUserId==proposal.OriginalInvoice.ROW_CREATED_BY || original.STATUS!="DRAFT" || original.ACTIVE_IND!="Y" ||
            recorded.ROW_CHANGED_BY!=command.ActorUserId || recorded.ROW_CHANGED_DATE is not {} changed || changed==default || changed.Kind!=DateTimeKind.Utc)
            throw new InvalidOperationException("Verified original proposal, independent controller and exact controller change audit are required.");
        var expected=PurchaseReceiptDecisionProof.Copy(original);expected.ROW_CHANGED_BY=recorded.ROW_CHANGED_BY;expected.ROW_CHANGED_DATE=recorded.ROW_CHANGED_DATE;
        if(Digest(expected)!=Digest(recorded))throw new InvalidOperationException("Controller disposition cannot change invoice financial, source or proposal values.");
        var json=JsonSerializer.Serialize(new APInvoiceVarianceDecisionProof(command,PurchaseReceiptDecisionProof.Copy(original),Digest(recorded)));
        if(json.Length>MaximumLength)throw new InvalidOperationException("Complete controller history exceeds the bounded Beep decision column.");return json;
    }
    public static APInvoiceVarianceDecisionProof Require(CompanyScope company,AP_INVOICE row)
    {
        if(row?.CORRECTION_HISTORY_JSON is not null)
            return Require(company,APInvoiceCorrectionHistory.Require(company,row).OriginalInvoice);
        if(row?.VARIANCE_DECISION_JSON is not {Length:>0} json || json.Length>MaximumLength)throw new InvalidOperationException("Recorded controller disposition is unavailable.");
        APInvoiceVarianceDecisionProof proof;
        try{proof=JsonSerializer.Deserialize<APInvoiceVarianceDecisionProof>(json,Options)??throw new JsonException();}catch(JsonException){throw new InvalidOperationException("Recorded controller disposition is incompatible.");}
        if(proof.Command is null || proof.OriginalInvoice is null || proof.OriginalInvoice.VARIANCE_DECISION_JSON is not null)throw new InvalidOperationException("Complete first controller intent and original invoice are required.");
        proof.Command.RequireShape();APInvoiceOwnership.Require(company,proof.Command.InvoiceId,row);
        if(proof.Command.Company!=company || proof.RowRevision!=Digest(row) || Create(proof.Command,proof.OriginalInvoice,row)!=json)
            throw new InvalidOperationException("Recorded controller disposition differs from its original intent and invoice.");return proof;
    }
}
public sealed record APInvoiceVarianceDecisionSubmission(APInvoiceVarianceDecisionCommand Command,APInvoiceVarianceDecisionReview Review);
public sealed record APInvoiceVarianceDecisionResult(APInvoiceVarianceDecisionCommand Command,AP_INVOICE Invoice)
{
    public void Require(APInvoiceVarianceDecisionSubmission submission)
    {
        submission.Command.Require(submission.Review);var proof=APInvoiceVarianceDecisionProof.Require(submission.Command.Company,Invoice);
        if(Command is null || !Command.Same(submission.Command) || !proof.Command.Same(Command) ||
            JsonSerializer.Serialize(proof.OriginalInvoice)!=JsonSerializer.Serialize(submission.Review.Current.Invoice.Invoice))throw new InvalidOperationException("Exact original controller outcome is unconfirmed.");
    }
}
