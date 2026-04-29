using Beep.OilandGas.Models.Data.ProductionAccounting;

namespace Beep.OilandGas.ApiService.Controllers.Accounting.Royalty
{
    /// <summary>Request for recording a royalty payment from an existing calculation.</summary>
    public class RecordRoyaltyPaymentFromCalculationRequest
    {
        public ROYALTY_CALCULATION? RoyaltyCalculation { get; set; }
        public decimal PaymentAmount { get; set; }
    }
}
