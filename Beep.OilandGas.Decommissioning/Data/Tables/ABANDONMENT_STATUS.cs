using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Decommissioning
{
    public partial class ABANDONMENT_STATUS : ModelEntityBase
    {
        private System.String ABANDONMENT_STATUS_IDValue;
        public System.String ABANDONMENT_STATUS_ID
        {
            get
            {
                return this.ABANDONMENT_STATUS_IDValue;
            }
            set { SetProperty(ref ABANDONMENT_STATUS_IDValue, value); }
        }

        private System.String WELL_ABANDONMENT_IDValue;
        public System.String WELL_ABANDONMENT_ID
        {
            get
            {
                return this.WELL_ABANDONMENT_IDValue;
            }
            set { SetProperty(ref WELL_ABANDONMENT_IDValue, value); }
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

        // ── Best-practice additions (PPDM 3.9 WELL_ABANDONMENT / BSEE NTL 2018 / API RP 5CT) ──────

        // UWI: FK to PPDM 3.9 WELL.UWI (correct PK for WELL, not WELL_ID)
        private System.String UWIValue;
        public System.String UWI
        {
            get { return this.UWIValue; }
            set { SetProperty(ref UWIValue, value); }
        }

        private System.String WELLBORE_IDValue;
        public System.String WELLBORE_ID
        {
            get { return this.WELLBORE_IDValue; }
            set { SetProperty(ref WELLBORE_IDValue, value); }
        }

        // ABANDONMENT_TYPE: TEMPORARY / PERMANENT (BSEE 30 CFR Part 250 / state requirements)
        private System.String ABANDONMENT_TYPEValue;
        public System.String ABANDONMENT_TYPE
        {
            get { return this.ABANDONMENT_TYPEValue; }
            set { SetProperty(ref ABANDONMENT_TYPEValue, value); }
        }

        private System.String REGULATORY_PERMIT_NUMBERValue;
        public System.String REGULATORY_PERMIT_NUMBER
        {
            get { return this.REGULATORY_PERMIT_NUMBERValue; }
            set { SetProperty(ref REGULATORY_PERMIT_NUMBERValue, value); }
        }

        // PLUG_BACK_DEPTH / OUOM: regulatory requirement per BSEE NTL 2010-N06 and state regs
        private System.Decimal? PLUG_BACK_DEPTH_MDValue;
        public System.Decimal? PLUG_BACK_DEPTH_MD
        {
            get { return this.PLUG_BACK_DEPTH_MDValue; }
            set { SetProperty(ref PLUG_BACK_DEPTH_MDValue, value); }
        }

        private System.String PLUG_BACK_DEPTH_OUOMValue;
        public System.String PLUG_BACK_DEPTH_OUOM
        {
            get { return this.PLUG_BACK_DEPTH_OUOMValue; }
            set { SetProperty(ref PLUG_BACK_DEPTH_OUOMValue, value); }
        }

        // VERIFIED_BY / VERIFICATION_DATE: independent verification required by regulators
        private System.String VERIFIED_BYValue;
        public System.String VERIFIED_BY
        {
            get { return this.VERIFIED_BYValue; }
            set { SetProperty(ref VERIFIED_BYValue, value); }
        }

        private System.DateTime? VERIFICATION_DATEValue;
        public System.DateTime? VERIFICATION_DATE
        {
            get { return this.VERIFICATION_DATEValue; }
            set { SetProperty(ref VERIFICATION_DATEValue, value); }
        }

        public ABANDONMENT_STATUS() { }
    }
}
