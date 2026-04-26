using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    public partial class DECOMMISSIONING_STATUS : ModelEntityBase
    {
        private System.String DECOMMISSIONING_STATUS_IDValue;
        public System.String DECOMMISSIONING_STATUS_ID
        {
            get
            {
                return this.DECOMMISSIONING_STATUS_IDValue;
            }
            set { SetProperty(ref DECOMMISSIONING_STATUS_IDValue, value); }
        }

        private System.String FACILITY_DECOMMISSIONING_IDValue;
        public System.String FACILITY_DECOMMISSIONING_ID
        {
            get
            {
                return this.FACILITY_DECOMMISSIONING_IDValue;
            }
            set { SetProperty(ref FACILITY_DECOMMISSIONING_IDValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get
            {
                return this.STATUSValue;
            }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.DateTime? STATUS_DATEValue;
        public System.DateTime? STATUS_DATE
        {
            get
            {
                return this.STATUS_DATEValue;
            }
            set { SetProperty(ref STATUS_DATEValue, value); }
        }

        private System.String STATUS_CHANGED_BYValue;
        public System.String STATUS_CHANGED_BY
        {
            get
            {
                return this.STATUS_CHANGED_BYValue;
            }
            set { SetProperty(ref STATUS_CHANGED_BYValue, value); }
        }

        private System.String NOTESValue;
        public System.String NOTES
        {
            get
            {
                return this.NOTESValue;
            }
            set { SetProperty(ref NOTESValue, value); }
        }

        // ── Best-practice additions (PPDM 3.9 / ISO 16530-2 / FASB ASC 410 ARO) ──────────

        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String FACILITY_IDValue;
        public System.String FACILITY_ID
        {
            get { return this.FACILITY_IDValue; }
            set { SetProperty(ref FACILITY_IDValue, value); }
        }

        // DECOM_PHASE: PLANNING / ENGINEERING / EXECUTION / VERIFICATION / COMPLETED (ISO 16530-2)
        private System.String DECOM_PHASEValue;
        public System.String DECOM_PHASE
        {
            get { return this.DECOM_PHASEValue; }
            set { SetProperty(ref DECOM_PHASEValue, value); }
        }

        // REGULATORY_AUTHORITY: governing body (BSEE / NOPSEMA / HSE / national regulator)
        private System.String REGULATORY_AUTHORITYValue;
        public System.String REGULATORY_AUTHORITY
        {
            get { return this.REGULATORY_AUTHORITYValue; }
            set { SetProperty(ref REGULATORY_AUTHORITYValue, value); }
        }

        private System.String REGULATORY_APPROVAL_NUMBERValue;
        public System.String REGULATORY_APPROVAL_NUMBER
        {
            get { return this.REGULATORY_APPROVAL_NUMBERValue; }
            set { SetProperty(ref REGULATORY_APPROVAL_NUMBERValue, value); }
        }

        // ESTIMATED_COST / ACTUAL_COST: FASB ASC 410 Asset Retirement Obligation (ARO) tracking
        private System.Decimal? ESTIMATED_COSTValue;
        public System.Decimal? ESTIMATED_COST
        {
            get { return this.ESTIMATED_COSTValue; }
            set { SetProperty(ref ESTIMATED_COSTValue, value); }
        }

        private System.Decimal? ACTUAL_COSTValue;
        public System.Decimal? ACTUAL_COST
        {
            get { return this.ACTUAL_COSTValue; }
            set { SetProperty(ref ACTUAL_COSTValue, value); }
        }

        private System.String COST_CURRENCYValue;
        public System.String COST_CURRENCY
        {
            get { return this.COST_CURRENCYValue; }
            set { SetProperty(ref COST_CURRENCYValue, value); }
        }

        public DECOMMISSIONING_STATUS() { }
    }
}
