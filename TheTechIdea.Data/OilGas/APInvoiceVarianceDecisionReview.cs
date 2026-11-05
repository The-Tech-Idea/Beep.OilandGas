using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

/// <summary>Independent fresh disposition review; no durable decision, matching approval or posting authority.</summary>
public sealed record APInvoiceVarianceDecisionReview(APInvoiceFinancialReview Current,APInvoiceFinancialReview Evidence,
    APInvoiceVarianceProof RecordedProposal,string BindingRevision,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static APInvoiceVarianceDecisionReview Create(APInvoiceFinancialReview current,string binding)
    {
        ArgumentNullException.ThrowIfNull(current);current=PurchaseReceiptDecisionProof.Copy(current);
        if(current.Invoice.Invoice.VARIANCE_DECISION_JSON is not null)throw new InvalidOperationException("A controller disposition is already recorded. Reconcile its original intent before further lifecycle actions.");
        var proposal=APInvoiceVarianceProof.Require(current.Invoice.Company,current.Invoice.Invoice);var evidence=current.RecordedVarianceEvidence();
        var result=new APInvoiceVarianceDecisionReview(current,evidence,proposal,binding,Digest(current,evidence,proposal,binding),DateTimeOffset.UtcNow);
        result.Require(current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,current.Invoice.Revision);return result;
    }
    public void Require(CompanyScope company,string actor,string id,string currentInvoiceRevision)
    {
        if(Current?.Invoice is null || Evidence?.Invoice is null || RecordedProposal?.Command is null ||
            string.IsNullOrWhiteSpace(actor) || actor=="SYSTEM" || actor.Length>128 || actor.Any(char.IsControl) ||
            !ProcessDefinitionRevision.IsValid(BindingRevision) || BindingRevision!=RecordedProposal.Command.BindingRevision || RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero)
            throw new InvalidOperationException("Complete original-company controller review and production binding are required.");
        Current.Require(company,actor,id,currentInvoiceRevision);Evidence.Require(company,actor,id,Evidence.Invoice.Revision);
        if(Current.Invoice.Invoice.VARIANCE_DECISION_JSON is not null)throw new InvalidOperationException("Review cannot replace a recorded controller disposition.");
        var proof=APInvoiceVarianceProof.Require(company,Current.Invoice.Invoice);
        if(actor==proof.Command.ActorUserId || actor==proof.OriginalInvoice.ROW_CREATED_BY || JsonSerializer.Serialize(proof)!=JsonSerializer.Serialize(RecordedProposal) ||
            Evidence.Revision!=Current.RecordedVarianceEvidence().Revision || Evidence.ReceiptConsumption is null || Evidence.MatchingPolicies is null ||
            Revision!=Digest(Current,Evidence,RecordedProposal,BindingRevision))
            throw new InvalidOperationException("An independent source-maker/proposal-preparer reviewer and unchanged complete current evidence are required.");
    }
    public bool EvidenceMatchesProposal=>Evidence.Revision==RecordedProposal.Command.Proposal.ExpectedFinancialRevision;
    public IReadOnlyList<string> EligibleDispositions
    {
        get
        {
            Require(Current.Invoice.Company,Current.Invoice.ActorUserId,Current.Invoice.Invoice.AP_INVOICE_ID,Current.Invoice.Revision);
            var result=new List<string>{"REQUEST_CORRECTION"};
            var checks=Evidence.Checks;
            var required=checks.Where(row=>row.Code is "activity" or "draft" or "maker" or "invoice-total" or "acceptance" ||
                row.Code.StartsWith("amount/",StringComparison.Ordinal) || row.Code.StartsWith("allocation/",StringComparison.Ordinal) ||
                row.Code.StartsWith("global-consumption",StringComparison.Ordinal) || row.Code.StartsWith("policy-price/",StringComparison.Ordinal) || row.Code.StartsWith("policy-quantity/",StringComparison.Ordinal)).ToArray();
            var differences=checks.Where(row=>row.Outcome is APSourceComparisonOutcome.RecordedDifference or APSourceComparisonOutcome.ExceedsRecordedLimit).ToArray();
            if(EvidenceMatchesProposal && Evidence.PolicySelection.Rules is not null && required.Length>0 &&
                required.All(row=>row.Outcome is APSourceComparisonOutcome.RecordedAgreement or APSourceComparisonOutcome.WithinRecordedLimit) &&
                differences.Length>0 && differences.All(row=>row.Code.StartsWith("price/",StringComparison.Ordinal)))result.Add("ACCEPT_WITHIN_POLICY_VARIANCE");
            return result.AsReadOnly();
        }
    }
    private static string Digest(APInvoiceFinancialReview current,APInvoiceFinancialReview evidence,APInvoiceVarianceProof proposal,string binding)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        current.Invoice.Company,current.Invoice.ActorUserId,current.Invoice.Invoice.AP_INVOICE_ID,CurrentRevision=current.Revision,EvidenceRevision=evidence.Revision,proposal,binding}));
}
