using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Lease
{
    /// <summary>
    /// One recorded stage transition of a lease acquisition (LSE-02). Append-only: the current stage is the row
    /// with the highest sequence, and a declined opportunity keeps every row it passed through (AC-4).
    /// </summary>
    public class LEASE_PIPELINE_STAGE : ModelEntityBase
    {
        [Key, MaxLength(64)]
        public string STAGE_ID
        {
            get => StageIdValue;
            set => SetProperty(ref StageIdValue, value);
        }

        private string StageIdValue = string.Empty;

        [MaxLength(64)]
        public string LEASE_ID
        {
            get { return this.LEASE_IDValue; }
            set { SetProperty(ref LEASE_IDValue, value); }
        }

        private string LEASE_IDValue = string.Empty;

        private int STAGE_SEQ_NOValue;
        public int STAGE_SEQ_NO
        {
            get { return this.STAGE_SEQ_NOValue; }
            set { SetProperty(ref STAGE_SEQ_NOValue, value); }
        }

        private string? STAGEValue;
        public string? STAGE
        {
            get { return this.STAGEValue; }
            set { SetProperty(ref STAGEValue, value); }
        }

        private string? OUTCOMEValue;
        public string? OUTCOME
        {
            get { return this.OUTCOMEValue; }
            set { SetProperty(ref OUTCOMEValue, value); }
        }

        private string? RATIONALEValue;
        public string? RATIONALE
        {
            get { return this.RATIONALEValue; }
            set { SetProperty(ref RATIONALEValue, value); }
        }

        private string? ACTORValue;
        public string? ACTOR
        {
            get { return this.ACTORValue; }
            set { SetProperty(ref ACTORValue, value); }
        }

        private DateTime? DECIDED_ATValue;
        public DateTime? DECIDED_AT
        {
            get { return this.DECIDED_ATValue; }
            set { SetProperty(ref DECIDED_ATValue, value); }
        }
    }
}
