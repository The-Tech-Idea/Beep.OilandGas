using System.Text.Json;
using Beep.OilandGas.Models.Data.ProductionAccounting;
namespace TheTechIdea.Data.OilGas;

public sealed record RevenueDeliveryAcceptanceCorrection(RevenueDeliveryAcceptanceRecord Previous,string PreviousNativeRevision,RevenueDeliveryAcceptancePreparation Preparation,string Reason)
{
    public void Require()
    {
        Previous.Require();Preparation.Require();Preparation.Facts.RequireQuantity(Preparation.Source);SalesContractTermsChecks.Text(Reason,4096);
        if(Previous.Decision?.Decision!=RevenueDeliveryAcceptanceDecision.Reject || !ProcessDefinitionRevision.IsValid(PreviousNativeRevision) || Preparation.Company!=Previous.Preparation.Company ||
            Preparation.ActorUserId!=Previous.Preparation.ActorUserId || Preparation.IntentId==Previous.Preparation.IntentId || Preparation.IntentId==Previous.Decision?.IntentId || JsonSerializer.Serialize(Preparation.Facts)==JsonSerializer.Serialize(Previous.Preparation.Facts))
            throw new InvalidOperationException("The original preparer's distinct correction of exact rejected receiving evidence and its reason are required.");
        Previous.Preparation.RequireCurrent(Preparation.Source);
    }
    public static RevenueDeliverySourceReview NormalizeSource(RevenueDeliverySourceReview current,RevenueDeliveryAcceptanceRecord previous)
    {
        current.Require(current.Source);previous.Require();var proof=RevenueDeliveryAcceptanceProof.RequireEnvelope(previous.Preparation.Company,current.Ticket);
        if(JsonSerializer.Serialize(proof.Record)!=JsonSerializer.Serialize(previous))throw new InvalidOperationException("The exact rejected receiving predecessor changed.");
        previous.Preparation.RequireCurrent(current);var row=CompanyInventorySourceReview.Copy(current.Ticket);row.DELIVERY_WORKFLOW_JSON=null;
        return RevenueDeliverySourceReview.Create(current.Source,current.Allocation,row);
    }
}
public sealed record RevenueDeliveryAcceptanceCorrectionOrigin(string PreviousRecordRevision,string PreviousNativeRevision,string Reason)
{
    public void Require(){SalesContractTermsChecks.Text(Reason,4096);if(!ProcessDefinitionRevision.IsValid(PreviousRecordRevision) || !ProcessDefinitionRevision.IsValid(PreviousNativeRevision))throw new InvalidOperationException("The complete recorded correction predecessor is required.");}
}
/// <summary>Complete flat predecessor evidence, excluding recursive history envelopes.</summary>
public sealed record RevenueDeliveryAcceptancePriorVersion(RevenueDeliveryAcceptanceRecord Record,string NativeRowJson,string Revision)
{
    [System.Text.Json.Serialization.JsonIgnore(Condition=System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)] public RevenueDeliveryAcceptanceCorrectionOrigin? Correction{get;init;}
}
