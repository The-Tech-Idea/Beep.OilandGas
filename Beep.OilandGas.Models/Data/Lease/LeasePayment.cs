using System;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class LeasePayment : ModelEntityBase
    {
        public string PaymentId { get; set; } = string.Empty;
        public string LeaseId { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentType { get; set; } = string.Empty; // e.g., Bonus, Rental, Royalty
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
