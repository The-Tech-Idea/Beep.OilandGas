using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One round of a lease negotiation (LSE-02): who the counterparty was, what was offered, in which currency,
    /// and who recorded it. A round without a counterparty is refused by the service, not stored as an anonymous
    /// entry.
    /// </summary>
    public class LEASE_NEGOTIATION : ModelEntityBase
    {
        private string LEASE_IDValue = string.Empty;
        [System.ComponentModel.DataAnnotations.Key]
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        [Key, MaxLength(64)]
        public string NEGOTIATION_ID
        {
            get { return this.NEGOTIATION_IDValue; }
            set { SetProperty(ref NEGOTIATION_IDValue, value); }
        }

        private string NEGOTIATION_IDValue = string.Empty;

        private int ROUND_NOValue;
        public int ROUND_NO
        {
            get { return this.ROUND_NOValue; }
            set { SetProperty(ref ROUND_NOValue, value); }
        }

        private string? COUNTERPARTYValue;
        public string? COUNTERPARTY
        {
            get { return this.COUNTERPARTYValue; }
            set { SetProperty(ref COUNTERPARTYValue, value); }
        }

        private decimal OFFER_AMOUNTValue;
        public decimal OFFER_AMOUNT
        {
            get { return this.OFFER_AMOUNTValue; }
            set { SetProperty(ref OFFER_AMOUNTValue, value); }
        }

        private string? CURRENCYValue;
        public string? CURRENCY
        {
            get { return this.CURRENCYValue; }
            set { SetProperty(ref CURRENCYValue, value); }
        }

        private string? NOTESValue;
        public string? NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        private string? ACTORValue;
        public string? ACTOR
        {
            get { return this.ACTORValue; }
            set { SetProperty(ref ACTORValue, value); }
        }

        private DateTime? RECORDED_ATValue;
        public DateTime? RECORDED_AT
        {
            get { return this.RECORDED_ATValue; }
            set { SetProperty(ref RECORDED_ATValue, value); }
        }
    }
}
