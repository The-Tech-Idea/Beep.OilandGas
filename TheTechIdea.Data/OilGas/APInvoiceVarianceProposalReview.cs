using System.Text.Json;
namespace TheTechIdea.Data.OilGas;

public sealed record APInvoiceVarianceExplanation(string CheckCode,string Reason);
public sealed record APInvoiceVarianceProposal(string Disposition,string ExpectedInvoiceRevision,string ExpectedFinancialRevision,
    IReadOnlyList<APInvoiceVarianceExplanation> Explanations,string Reason)
{
    public void Require(APInvoiceFinancialReview financial)
    {
        RequireShape();
        if(financial?.Invoice is null)throw new InvalidOperationException("Complete invoice financial evidence is required.");
        financial.Require(financial.Invoice.Company,financial.Invoice.ActorUserId,financial.Invoice.Invoice.AP_INVOICE_ID,ExpectedInvoiceRevision);
        if(Disposition is not ("REQUEST_VARIANCE_REVIEW" or "REQUEST_CORRECTION") || financial.Revision!=ExpectedFinancialRevision ||
            financial.Invoice.Invoice.VARIANCE_PROPOSAL_JSON is not null ||
            financial.ReceiptConsumption is null || financial.MatchingPolicies is null ||
            financial.Checks.Single(row=>row.Code=="activity").Outcome!=APSourceComparisonOutcome.RecordedAgreement ||
            financial.Checks.Single(row=>row.Code=="draft").Outcome!=APSourceComparisonOutcome.RecordedAgreement ||
            financial.Checks.Single(row=>row.Code=="maker").Outcome!=APSourceComparisonOutcome.RecordedAgreement)
            throw new InvalidOperationException("Variance preparation requires unchanged complete evidence, an active draft and verified source maker independence.");
        Text(Reason,1000);
        var differences=financial.Checks.Where(row=>row.Outcome is APSourceComparisonOutcome.RecordedDifference or APSourceComparisonOutcome.ExceedsRecordedLimit).Select(row=>row.Code).Order(StringComparer.Ordinal).ToArray();
        if(Explanations is not {Count:>0} || Explanations.Count>1024 || differences.Length==0 ||
            Explanations.Any(row=>row is null) || !differences.SequenceEqual(Explanations.Select(row=>row.CheckCode).Order(StringComparer.Ordinal)))
            throw new InvalidOperationException("Explain every recorded difference exactly once. Unknown evidence cannot be described as a confirmed variance.");
        foreach(var item in Explanations){Text(item.CheckCode,384);Text(item.Reason,1000);}
        if(Disposition=="REQUEST_VARIANCE_REVIEW" && financial.PolicySelection.Rules is null)
            throw new InvalidOperationException("A verified effective matching policy is required to request variance review; unavailable policy evidence requires correction.");
    }
    public void RequireShape()
    {
        if(Disposition is not ("REQUEST_VARIANCE_REVIEW" or "REQUEST_CORRECTION") || !ProcessDefinitionRevision.IsValid(ExpectedInvoiceRevision) ||
            !ProcessDefinitionRevision.IsValid(ExpectedFinancialRevision) || Explanations is not {Count:>0} || Explanations.Count>1024 || Explanations.Any(row=>row is null) ||
            Explanations.Select(row=>row.CheckCode).Distinct(StringComparer.Ordinal).Count()!=Explanations.Count)
            throw new InvalidOperationException("An explicit bounded original variance proposal is required.");
        Text(Reason,1000);foreach(var item in Explanations){Text(item.CheckCode,384);Text(item.Reason,1000);}
    }
    private static void Text(string? value,int maximum)
    {if(string.IsNullOrWhiteSpace(value) || value.Length>maximum || value.Any(char.IsControl))throw new InvalidOperationException("Explicit bounded variance explanations and reason are required.");}
}

/// <summary>Fresh authority-bound accountant preparation only. Not a persisted proposal or controller decision.</summary>
public sealed record APInvoiceVarianceProposalReview(APInvoiceFinancialReview Financial,APInvoiceVarianceProposal Proposal,
    string BindingRevision,string Revision,DateTimeOffset RetrievedAtUtc)
{
    public static APInvoiceVarianceProposalReview Create(APInvoiceFinancialReview financial,APInvoiceVarianceProposal proposal,string binding)
    {
        ArgumentNullException.ThrowIfNull(financial);ArgumentNullException.ThrowIfNull(proposal);
        proposal.Require(financial);var frozen=PurchaseReceiptDecisionProof.Copy(financial);var input=PurchaseReceiptDecisionProof.Copy(proposal);
        var result=new APInvoiceVarianceProposalReview(frozen,input,binding,Digest(frozen,input,binding),DateTimeOffset.UtcNow);
        result.Require(financial.Invoice.Company,financial.Invoice.ActorUserId,financial.Invoice.Invoice.AP_INVOICE_ID,proposal);return result;
    }
    public void Require(CompanyScope company,string actor,string invoiceId,APInvoiceVarianceProposal expected)
    {
        if(Financial?.Invoice is null || Proposal is null || expected is null || !ProcessDefinitionRevision.IsValid(BindingRevision) ||
            RetrievedAtUtc==default || RetrievedAtUtc.Offset!=TimeSpan.Zero || Revision!=Digest(Financial,Proposal,BindingRevision) ||
            JsonSerializer.Serialize(Proposal)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The original variance preparation or production binding changed.");
        Financial.Require(company,actor,invoiceId,Proposal.ExpectedInvoiceRevision);Proposal.Require(Financial);
    }
    public static string RevisionFor(CompanyScope company,string actor,string invoiceId,APInvoiceVarianceProposal proposal,string binding)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(new{
        Company=company,ActorUserId=actor,AP_INVOICE_ID=invoiceId,FinancialRevision=proposal.ExpectedFinancialRevision,binding,proposal}));
    private static string Digest(APInvoiceFinancialReview financial,APInvoiceVarianceProposal proposal,string binding)=>RevisionFor(financial.Invoice.Company,financial.Invoice.ActorUserId,financial.Invoice.Invoice.AP_INVOICE_ID,proposal,binding);
}
