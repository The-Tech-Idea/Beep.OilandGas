using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Closed process evidence and a fresh explicit source capture for its original preparer.</summary>
public sealed record CompanyCommercialAmendmentReview(CompanyScope Company,string ActorUserId,string InstanceId,string ProcessRevision,
    string BindingRevision,SALES_CONTRACT PreviousNativeContract,SalesContractTermsRecord Previous,RevenueSalesContractReview Source)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        APInvoiceOwnership.RequireScope(company);new SalesContractTermsAmendmentProcessOrigin(InstanceId,ProcessRevision).Require();
        if(Company!=company || ActorUserId!=actor || InstanceId!=instance || !ProcessDefinitionRevision.IsValid(BindingRevision) ||
            Previous is null || Source is null || PreviousNativeContract is null)throw new InvalidOperationException("The original company amendment review is required.");
        Previous.Require();var proof=SalesContractTermsNativeProof.Require(company,PreviousNativeContract);
        Source.Require(company,actor,Source.Source.FieldId,Source.Source.From,Source.Source.Through,Source.TransactionId,Source.Source.Revision);
        if(Previous.Decision?.Decision!=SalesContractTermsDecision.Reject || Previous.Preparation.ActorUserId!=actor ||
            JsonSerializer.Serialize(proof.Record)!=JsonSerializer.Serialize(Previous) ||
            Source.Source.FieldId!=Previous.Preparation.Source.Source.FieldId || Source.TransactionId!=Previous.Preparation.Source.TransactionId ||
            Source.Contract.TERMS_WORKFLOW_JSON is not null || JsonSerializer.Serialize(Source.Contract)!=JsonSerializer.Serialize(Previous.Preparation.Source.Contract))
            throw new InvalidOperationException("The rejected predecessor, original preparer or reviewed source changed.");
    }
    public SalesContractTermsAmendment CreateIntent(string intent,SalesContractEffectiveTerms terms,string reason)
    {
        Require(Company,ActorUserId,InstanceId);
        var command=new SalesContractTermsAmendment(CompanyInventorySourceReview.Copy(Previous),SalesContractTermsNativeProof.Require(Company,PreviousNativeContract).Revision,
            new(intent,Company,ActorUserId,CompanyInventorySourceReview.Copy(Source),CompanyInventorySourceReview.Copy(terms)),reason,new(InstanceId,ProcessRevision));
        command.Require();return command;
    }
}
public sealed record CompanyCommercialAmendmentSubmission(CompanyCommercialAmendmentReview Review,SalesContractTermsAmendment Amendment)
{
    public void Require()
    {
        if(Review is null || Amendment is null)throw new InvalidOperationException("The retained amendment review and intent are required.");
        Review.Require(Review.Company,Review.ActorUserId,Review.InstanceId);Amendment.Require();
        if(JsonSerializer.Serialize(Review.CreateIntent(Amendment.Preparation.IntentId,Amendment.Preparation.Terms,Amendment.Reason))!=JsonSerializer.Serialize(Amendment))
            throw new InvalidOperationException("The amendment differs from its frozen process/predecessor/source review.");
    }
}
