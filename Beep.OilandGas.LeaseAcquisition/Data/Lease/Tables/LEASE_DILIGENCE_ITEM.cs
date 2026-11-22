using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One due-diligence question on a lease acquisition (LSE-02 AC-2). An item with no outcome is open and blocks
    /// an advance; a closed item carries its evidence and the reviewer who closed it.
    /// </summary>
    public class LEASE_DILIGENCE_ITEM : ModelEntityBase
    {
        private string LEASE_IDValue = string.Empty;
        [System.ComponentModel.DataAnnotations.Key]
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        [Key, MaxLength(64)]
        public string ITEM_ID
        {
            get { return this.ITEM_IDValue; }
            set { SetProperty(ref ITEM_IDValue, value); }
        }

        private string ITEM_IDValue = string.Empty;

        private string? CATEGORYValue;
        public string? CATEGORY
        {
            get { return this.CATEGORYValue; }
            set { SetProperty(ref CATEGORYValue, value); }
        }

        private string? QUESTIONValue;
        public string? QUESTION
        {
            get { return this.QUESTIONValue; }
            set { SetProperty(ref QUESTIONValue, value); }
        }

        private string? OUTCOMEValue;
        public string? OUTCOME
        {
            get { return this.OUTCOMEValue; }
            set { SetProperty(ref OUTCOMEValue, value); }
        }

        private string? EVIDENCE_REFValue;
        public string? EVIDENCE_REF
        {
            get { return this.EVIDENCE_REFValue; }
            set { SetProperty(ref EVIDENCE_REFValue, value); }
        }

        private string? REVIEWERValue;
        public string? REVIEWER
        {
            get { return this.REVIEWERValue; }
            set { SetProperty(ref REVIEWERValue, value); }
        }

        private DateTime? REVIEWED_ATValue;
        public DateTime? REVIEWED_AT
        {
            get { return this.REVIEWED_ATValue; }
            set { SetProperty(ref REVIEWED_ATValue, value); }
        }
    }
}
