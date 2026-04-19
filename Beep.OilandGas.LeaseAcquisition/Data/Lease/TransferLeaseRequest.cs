using System;

namespace Beep.OilandGas.Models.Data.Lease
{
    public class TransferLeaseRequest
    {
        public string LeaseId { get; set; } = string.Empty;
        public string FromOwnerId { get; set; } = string.Empty;
        public string ToOwnerId { get; set; } = string.Empty;
        public decimal TransferAmount { get; set; }
        public DateTime TransferDate { get; set; }
        public string TransferReason { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
