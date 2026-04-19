using System;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class BonusPaymentRequest
    {
        public string LeaseId { get; set; } = string.Empty;
        public decimal Acreage { get; set; }
        public decimal BonusPerAcre { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }
}
