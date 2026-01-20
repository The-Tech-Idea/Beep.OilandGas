using System;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public class CreateSalesAgreementRequest : ModelEntityBase
    {
        public string AgreementName { get; set; }
        public string SellerBaId { get; set; }
        public string PurchaserBaId { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string PricingMethod { get; set; }
        public decimal? BasePrice { get; set; }
        public string PriceIndex { get; set; }
        public decimal? Differential { get; set; }
        public int PaymentTermsDays { get; set; }
    }

    public class CreateTransportationAgreementRequest : ModelEntityBase
    {
        public string CarrierBaId { get; set; }
        public string OriginPoint { get; set; }
        public string DestinationPoint { get; set; }
        public DateTime EffectiveDate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public decimal TariffRate { get; set; }
        public decimal? MinimumVolumeCommitment { get; set; }
        public decimal? MaximumCapacity { get; set; }
    }
}





