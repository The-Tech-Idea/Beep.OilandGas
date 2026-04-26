using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// Field Development Plan (FDP) header record.
    /// Per SPE PRMS 2018 Section 3.4 — basis for Developed Reserves booking.
    /// Each FDP version represents a formal plan submitted for regulatory / board approval.
    /// </summary>
    public partial class FIELD_DEVELOPMENT_PLAN : ModelEntityBase
    {
        private System.String FDP_IDValue = string.Empty;
        public System.String FDP_ID
        {
            get { return this.FDP_IDValue; }
            set { SetProperty(ref FDP_IDValue, value); }
        }

        private System.String FIELD_IDValue = string.Empty;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.String FDP_NAMEValue = string.Empty;
        public System.String FDP_NAME
        {
            get { return this.FDP_NAMEValue; }
            set { SetProperty(ref FDP_NAMEValue, value); }
        }

        private System.Int32 FDP_VERSIONValue;
        /// <summary>Version counter — v1 = original FDP; v2+ = revisions.</summary>
        public System.Int32 FDP_VERSION
        {
            get { return this.FDP_VERSIONValue; }
            set { SetProperty(ref FDP_VERSIONValue, value); }
        }

        private System.String FDP_STATUSValue = string.Empty;
        /// <summary>DRAFT / INTERNAL_REVIEW / REGULATORY_SUBMISSION / APPROVED / SUPERSEDED.</summary>
        public System.String FDP_STATUS
        {
            get { return this.FDP_STATUSValue; }
            set { SetProperty(ref FDP_STATUSValue, value); }
        }

        private System.DateTime? SUBMISSION_DATEValue;
        public System.DateTime? SUBMISSION_DATE
        {
            get { return this.SUBMISSION_DATEValue; }
            set { SetProperty(ref SUBMISSION_DATEValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
        }

        private System.String APPROVED_BYValue = string.Empty;
        public System.String APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
        }

        private System.DateTime? FIRST_PRODUCTION_DATEValue;
        /// <summary>Planned first-oil / first-gas date.</summary>
        public System.DateTime? FIRST_PRODUCTION_DATE
        {
            get { return this.FIRST_PRODUCTION_DATEValue; }
            set { SetProperty(ref FIRST_PRODUCTION_DATEValue, value); }
        }

        private System.DateTime? PEAK_PRODUCTION_DATEValue;
        public System.DateTime? PEAK_PRODUCTION_DATE
        {
            get { return this.PEAK_PRODUCTION_DATEValue; }
            set { SetProperty(ref PEAK_PRODUCTION_DATEValue, value); }
        }

        private System.DateTime? FIELD_LIFE_ENDValue;
        public System.DateTime? FIELD_LIFE_END
        {
            get { return this.FIELD_LIFE_ENDValue; }
            set { SetProperty(ref FIELD_LIFE_ENDValue, value); }
        }

        private System.Decimal? TOTAL_WELLS_PLANNEDValue;
        public System.Decimal? TOTAL_WELLS_PLANNED
        {
            get { return this.TOTAL_WELLS_PLANNEDValue; }
            set { SetProperty(ref TOTAL_WELLS_PLANNEDValue, value); }
        }

        private System.Decimal? TOTAL_CAPEX_MMValue;
        /// <summary>Total CAPEX in millions (currency defined by CAPEX_CURRENCY).</summary>
        public System.Decimal? TOTAL_CAPEX_MM
        {
            get { return this.TOTAL_CAPEX_MMValue; }
            set { SetProperty(ref TOTAL_CAPEX_MMValue, value); }
        }

        private System.String CAPEX_CURRENCYValue = string.Empty;
        public System.String CAPEX_CURRENCY
        {
            get { return this.CAPEX_CURRENCYValue; }
            set { SetProperty(ref CAPEX_CURRENCYValue, value); }
        }

        private System.Decimal? RECOVERABLE_OIL_MMBBLValue;
        /// <summary>Proved + probable recoverable oil (MMbbl) per FDP reserves estimate.</summary>
        public System.Decimal? RECOVERABLE_OIL_MMBBL
        {
            get { return this.RECOVERABLE_OIL_MMBBLValue; }
            set { SetProperty(ref RECOVERABLE_OIL_MMBBLValue, value); }
        }

        private System.Decimal? RECOVERABLE_GAS_BCFValue;
        public System.Decimal? RECOVERABLE_GAS_BCF
        {
            get { return this.RECOVERABLE_GAS_BCFValue; }
            set { SetProperty(ref RECOVERABLE_GAS_BCFValue, value); }
        }

        private System.String DEVELOPMENT_CONCEPT_DESCValue = string.Empty;
        /// <summary>Development concept narrative (subsea tieback, FPSO, onshore CPF, etc.).</summary>
        public System.String DEVELOPMENT_CONCEPT_DESC
        {
            get { return this.DEVELOPMENT_CONCEPT_DESCValue; }
            set { SetProperty(ref DEVELOPMENT_CONCEPT_DESCValue, value); }
        }

        public FIELD_DEVELOPMENT_PLAN() { }
    }
}
