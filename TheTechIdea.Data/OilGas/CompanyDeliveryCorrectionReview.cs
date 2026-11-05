using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

/// <summary>Exact closed original rejection review and current unchanged receiving source.</summary>
public sealed record CompanyDeliveryCorrectionReview(CompanyScope Company,string ActorUserId,string InstanceId,string ProcessRevision,
    string BindingRevision,RUN_TICKET PreviousNativeTicket,RevenueDeliveryAcceptanceRecord Previous,RevenueDeliverySourceReview Source)
{
    public void Require(CompanyScope company,string actor,string instance)
    {
        APInvoiceOwnership.RequireScope(company);SalesContractTermsChecks.Text(InstanceId,128);SalesContractTermsChecks.Text(ActorUserId,128);
        if(Company!=company || ActorUserId!=actor || InstanceId!=instance || !ProcessDefinitionRevision.IsValid(ProcessRevision) || !ProcessDefinitionRevision.IsValid(BindingRevision) || Previous is null || PreviousNativeTicket is null || Source is null)
            throw new InvalidOperationException("The complete original company/process correction review is required.");
        Previous.Require();Source.Require(Source.Source);var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(company,PreviousNativeTicket);
        if(Previous.Decision?.Decision!=RevenueDeliveryAcceptanceDecision.Reject || Previous.Preparation.ActorUserId!=actor || Source.Source.Source.Company!=company || Source.Source.Source.ActorUserId!=actor ||
            JsonSerializer.Serialize(proof.Record)!=JsonSerializer.Serialize(Previous) || Source.Ticket.DELIVERY_WORKFLOW_JSON is not null)
            throw new InvalidOperationException("The exact rejected receiving predecessor and original preparer are required.");
        Previous.Preparation.RequireCurrent(Source);
        var expected=CompanyInventorySourceReview.Copy(PreviousNativeTicket);expected.DELIVERY_WORKFLOW_JSON=null;
        if(JsonSerializer.Serialize(Source.Ticket)!=JsonSerializer.Serialize(expected))throw new InvalidOperationException("The original rejected ticket/audit changed in correction review.");
    }
    public RevenueDeliveryAcceptanceCorrection CreateIntent(string intent,RevenueDeliveryAcceptanceFacts facts,string reason)
    {
        Require(Company,ActorUserId,InstanceId);var command=new RevenueDeliveryAcceptanceCorrection(CompanyInventorySourceReview.Copy(Previous),RevenueDeliveryAcceptanceProof.RequireEnvelope(Company,PreviousNativeTicket).Revision,
            new(intent,Company,ActorUserId,CompanyInventorySourceReview.Copy(Source),CompanyInventorySourceReview.Copy(facts)),reason);command.Require();return command;
    }
}
public sealed record CompanyDeliveryCorrectionSubmission(CompanyDeliveryCorrectionReview Review,RevenueDeliveryAcceptanceCorrection Correction)
{
    public void Require()
    {
        if(Review is null || Correction is null)throw new InvalidOperationException("The original correction review and command are required.");
        Review.Require(Review.Company,Review.ActorUserId,Review.InstanceId);Correction.Require();
        if(JsonSerializer.Serialize(Review.CreateIntent(Correction.Preparation.IntentId,Correction.Preparation.Facts,Correction.Reason))!=JsonSerializer.Serialize(Correction))throw new InvalidOperationException("The correction differs from its complete original process/source review.");
    }
}
