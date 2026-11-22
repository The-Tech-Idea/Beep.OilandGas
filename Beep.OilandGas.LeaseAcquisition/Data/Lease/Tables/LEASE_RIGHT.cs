using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One right held in a lease acquisition (LSE-02 AC-3): the mineral, surface or working interest, its owner and
    /// the share held. Keeping the three as one typed matrix means a right is never confused with another kind.
    /// </summary>
    public class LEASE_RIGHT : ModelEntityBase
    {
        private string LEASE_IDValue = string.Empty;
        [System.ComponentModel.DataAnnotations.Key]
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        [Key, MaxLength(64)]
        public string RIGHT_ID
        {
            get { return this.RIGHT_IDValue; }
            set { SetProperty(ref RIGHT_IDValue, value); }
        }

        private string RIGHT_IDValue = string.Empty;

        private string? RIGHT_TYPEValue;
        public string? RIGHT_TYPE
        {
            get { return this.RIGHT_TYPEValue; }
            set { SetProperty(ref RIGHT_TYPEValue, value); }
        }

        private string? OWNERValue;
        public string? OWNER
        {
            get { return this.OWNERValue; }
            set { SetProperty(ref OWNERValue, value); }
        }

        private decimal INTEREST_PERCENTValue;
        public decimal INTEREST_PERCENT
        {
            get { return this.INTEREST_PERCENTValue; }
            set { SetProperty(ref INTEREST_PERCENTValue, value); }
        }

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}
