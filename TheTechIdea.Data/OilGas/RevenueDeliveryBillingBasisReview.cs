using System.Text.Json;
using System.Text.Json.Serialization;
using System.Numerics;
namespace TheTechIdea.Data.OilGas;

public enum RevenueDeliveryBillingBasisStatus
{
    HistoricalReceivingEvidence,CommercialTermsRequired,CommercialApprovalRequired,CommercialTermsRejected,
    CommercialApplicabilityRequired,ReceivingPreparationRequired,ReceivingApprovalRequired,ReceivingRejected,
    AcceptedQuantityRequired,PricingMethodUnresolved,QuantityUnitMismatch,ZeroPricePolicyRequired,
    AmountOutOfRange,CompatibleFixedUnitPrice
}

/// <summary>One source-bound price/quantity prerequisite; invoice policies, tax, rounding and posting remain separate.</summary>
public sealed record RevenueDeliveryBillingBasisReview(CompanyScope Company,string ActorUserId,string TicketId,string TransactionId,
    RevenueDeliveryBillingBasisStatus Status,decimal? ApprovedQuantity,string? QuantityUnit,decimal? ApprovedUnitPrice,
    string? PriceUnit,string? PricingMethod,string? CurrencyCode,decimal? UnroundedAmount,string SourceRevision,
    string? TermsRevision,string? DeliveryRevision,string Revision)
{
    [JsonIgnore] public bool HasCompatiblePriceBasis=>Status==RevenueDeliveryBillingBasisStatus.CompatibleFixedUnitPrice;
    public static RevenueDeliveryBillingBasisReview Create(RevenueDeliveryAcceptanceReadReview original)
    {
        ArgumentNullException.ThrowIfNull(original);original.Require();var source=original.Source;var company=source.Source.Source.Company;
        var termsRecord=source.Source.Contract.TERMS_WORKFLOW_JSON is null?null:SalesContractTermsNativeProof.Require(company,source.Source.Contract).Record;
        var termsReview=new SalesContractTermsReadReview(source.Source,termsRecord);var applicability=termsReview.ReviewApplicability();
        var terms=termsRecord?.Preparation.Terms;var receiving=original.Recorded;
        decimal? quantity=receiving?.Decision?.Decision==RevenueDeliveryAcceptanceDecision.Approve?receiving.Preparation.Facts.AcceptedQuantity:null;
        string? quantityUnit=quantity is not null?receiving!.Preparation.Facts.AcceptedQuantityUnit:null;decimal? amount=null;
        var status=original.IsHistorical?RevenueDeliveryBillingBasisStatus.HistoricalReceivingEvidence:
            termsRecord is null?RevenueDeliveryBillingBasisStatus.CommercialTermsRequired:
            termsRecord.Decision is null?RevenueDeliveryBillingBasisStatus.CommercialApprovalRequired:
            termsRecord.Decision.Decision==SalesContractTermsDecision.Reject?RevenueDeliveryBillingBasisStatus.CommercialTermsRejected:
            applicability.Status!=SalesContractTermApplicability.ApprovedStoredDateAndCurrency?RevenueDeliveryBillingBasisStatus.CommercialApplicabilityRequired:
            receiving is null?RevenueDeliveryBillingBasisStatus.ReceivingPreparationRequired:
            receiving.Decision is null?RevenueDeliveryBillingBasisStatus.ReceivingApprovalRequired:
            receiving.Decision.Decision==RevenueDeliveryAcceptanceDecision.Reject?RevenueDeliveryBillingBasisStatus.ReceivingRejected:
            quantity is null?RevenueDeliveryBillingBasisStatus.AcceptedQuantityRequired:
            terms!.PricingMethod!=SalesContractEffectiveTerms.FixedUnitPriceMethod?RevenueDeliveryBillingBasisStatus.PricingMethodUnresolved:
            terms.PriceUnit!=quantityUnit?RevenueDeliveryBillingBasisStatus.QuantityUnitMismatch:
            terms.Price==0?RevenueDeliveryBillingBasisStatus.ZeroPricePolicyRequired:RevenueDeliveryBillingBasisStatus.CompatibleFixedUnitPrice;
        if(status==RevenueDeliveryBillingBasisStatus.CompatibleFixedUnitPrice)
        {
            try{amount=ExactExtension(quantity!.Value,terms!.Price);}catch(OverflowException){status=RevenueDeliveryBillingBasisStatus.AmountOutOfRange;}
        }
        var result=new RevenueDeliveryBillingBasisReview(company,source.Source.Source.ActorUserId,source.TicketId,source.Source.TransactionId,status,
            quantity,quantityUnit,termsRecord?.Decision?.Decision==SalesContractTermsDecision.Approve?terms!.Price:null,terms?.PriceUnit,terms?.PricingMethod,terms?.CurrencyCode,
            amount,source.Revision,termsRecord?.Revision,receiving?.Revision,string.Empty);
        return result with{Revision=Digest(result)};
    }
    public void Require(RevenueDeliveryAcceptanceReadReview original)
    {
        if(JsonSerializer.Serialize(this)!=JsonSerializer.Serialize(Create(original)))throw new InvalidOperationException("The receiving price basis differs from its complete current native source/approvals.");
    }
    public void RequireCompatible(RevenueDeliveryAcceptanceReadReview original)
    {Require(original);if(!HasCompatiblePriceBasis)throw new InvalidOperationException("An exact current approved fixed-unit receiving price basis is required.");}
    private static string Digest(RevenueDeliveryBillingBasisReview value)=>ProcessDefinitionRevision.FromJson(JsonSerializer.Serialize(value with{Revision=string.Empty}));
    private static decimal ExactExtension(decimal quantity,decimal price)
    {
        static (BigInteger Coefficient,int Scale) Parts(decimal value)
        {var bits=decimal.GetBits(value);return((BigInteger)(uint)bits[0]+((BigInteger)(uint)bits[1]<<32)+((BigInteger)(uint)bits[2]<<64),(bits[3]>>16)&255);}
        var left=Parts(quantity);var right=Parts(price);var product=left.Coefficient*right.Coefficient;var scale=left.Scale+right.Scale;
        while(scale>0 && product%10==0){product/=10;--scale;}
        if(product<=0 || product>((BigInteger.One<<96)-1) || scale>28)throw new OverflowException("The exact unrounded price extension is outside decimal precision.");
        return new decimal(unchecked((int)(uint)(product & uint.MaxValue)),unchecked((int)(uint)((product>>32)&uint.MaxValue)),unchecked((int)(uint)((product>>64)&uint.MaxValue)),false,(byte)scale);
    }
}
