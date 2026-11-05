namespace TheTechIdea.Data.OilGas;
public sealed record SalesContractTermsReadReview(RevenueSalesContractReview Source,SalesContractTermsRecord? Recorded)
{
    public SalesContractTermApplicabilityReview ReviewApplicability()=>SalesContractTermApplicabilityReview.Create(this);
    public void Require(CompanyScope company,string actor,string field,DateTime from,DateTime through,string transaction)
    {
        Source.Require(company,actor,field,from,through,transaction,Source.Source.Revision);
        if(Source.Contract.TERMS_WORKFLOW_JSON is null)
        {if(Recorded is not null)throw new InvalidOperationException("A commercial result lacks native evidence.");return;}
        var native=SalesContractTermsNativeProof.Require(company,Source.Contract).Record;
        if(Recorded is null || Recorded.Revision!=native.Revision || System.Text.Json.JsonSerializer.Serialize(Recorded)!=System.Text.Json.JsonSerializer.Serialize(native) || Recorded.Preparation.Source.TransactionId!=transaction)
            throw new InvalidOperationException("The commercial recorded outcome does not match its exact native source.");
        Recorded.Require();Recorded.Preparation.RequireCurrentSource(SalesContractTermsNativeProof.RestoreOriginalReview(Source,Recorded));
    }
}
public sealed record SalesContractTermsDecisionSubmission(SalesContractTermsRecord Preparation,SalesContractTermsDecisionCommand Command)
{
    public void Require(){if(Preparation is null || Command is null)throw new InvalidOperationException("The original commercial decision review is required.");Command.Require(Preparation);}
}
