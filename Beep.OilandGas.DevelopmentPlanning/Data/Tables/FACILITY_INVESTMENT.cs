using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DevelopmentPlanning
{
    /// <summary>
    /// Surface facility investment record — CAPEX tracking per facility component in an FDP.
    /// Per PPDM 3.9 FACILITY table complement; captures planned vs actual CAPEX by scope.
    /// Conforms to AACE International cost-classification practice (Class 1-5).
    /// </summary>
    public partial class FACILITY_INVESTMENT : ModelEntityBase
    {
        private System.String FACILITY_INV_IDValue = string.Empty;
        public System.String FACILITY_INV_ID
        {
            get { return this.FACILITY_INV_IDValue; }
            set { SetProperty(ref FACILITY_INV_IDValue, value); }
        }

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

        private System.String FACILITY_IDValue = string.Empty;
        /// <summary>FK to PPDM FACILITY table (if facility already exists).</summary>
        public System.String FACILITY_ID
        {
            get { return this.FACILITY_IDValue; }
            set { SetProperty(ref FACILITY_IDValue, value); }
        }

        private System.String FACILITY_NAMEValue = string.Empty;
        public System.String FACILITY_NAME
        {
            get { return this.FACILITY_NAMEValue; }
            set { SetProperty(ref FACILITY_NAMEValue, value); }
        }

        private System.String FACILITY_TYPEValue = string.Empty;
        /// <summary>WELLHEAD / FLOWLINE / MANIFOLD / SEPARATOR / COMPRESSOR / PIPELINE / FPSO / PLATFORM / CPF.</summary>
        public System.String FACILITY_TYPE
        {
            get { return this.FACILITY_TYPEValue; }
            set { SetProperty(ref FACILITY_TYPEValue, value); }
        }

        private System.String INVESTMENT_PHASEValue = string.Empty;
        /// <summary>FEED / DETAILED_ENGINEERING / PROCUREMENT / CONSTRUCTION / COMMISSIONING / STARTUP.</summary>
        public System.String INVESTMENT_PHASE
        {
            get { return this.INVESTMENT_PHASEValue; }
            set { SetProperty(ref INVESTMENT_PHASEValue, value); }
        }

        private System.Int32 ESTIMATE_CLASSValue;
        /// <summary>AACE cost-estimate class: 1 (±3-15%) to 5 (±20-100% order-of-magnitude).</summary>
        public System.Int32 ESTIMATE_CLASS
        {
            get { return this.ESTIMATE_CLASSValue; }
            set { SetProperty(ref ESTIMATE_CLASSValue, value); }
        }

        private System.Decimal? CAPEX_PLANNED_MMValue;
        public System.Decimal? CAPEX_PLANNED_MM
        {
            get { return this.CAPEX_PLANNED_MMValue; }
            set { SetProperty(ref CAPEX_PLANNED_MMValue, value); }
        }

        private System.Decimal? CAPEX_ACTUAL_MMValue;
        public System.Decimal? CAPEX_ACTUAL_MM
        {
            get { return this.CAPEX_ACTUAL_MMValue; }
            set { SetProperty(ref CAPEX_ACTUAL_MMValue, value); }
        }

        private System.String COST_CURRENCYValue = string.Empty;
        public System.String COST_CURRENCY
        {
            get { return this.COST_CURRENCYValue; }
            set { SetProperty(ref COST_CURRENCYValue, value); }
        }

        private System.Decimal? CAPACITY_PLANNED_BOPDValue;
        /// <summary>Facility throughput capacity (bopd or MMscfd).</summary>
        public System.Decimal? CAPACITY_PLANNED_BOPD
        {
            get { return this.CAPACITY_PLANNED_BOPDValue; }
            set { SetProperty(ref CAPACITY_PLANNED_BOPDValue, value); }
        }

        private System.String CAPACITY_OUOMValue = string.Empty;
        public System.String CAPACITY_OUOM
        {
            get { return this.CAPACITY_OUOMValue; }
            set { SetProperty(ref CAPACITY_OUOMValue, value); }
        }

        private System.DateTime? PLANNED_START_DATEValue;
        public System.DateTime? PLANNED_START_DATE
        {
            get { return this.PLANNED_START_DATEValue; }
            set { SetProperty(ref PLANNED_START_DATEValue, value); }
        }

        private System.DateTime? PLANNED_COMPLETION_DATEValue;
        public System.DateTime? PLANNED_COMPLETION_DATE
        {
            get { return this.PLANNED_COMPLETION_DATEValue; }
            set { SetProperty(ref PLANNED_COMPLETION_DATEValue, value); }
        }

        public FACILITY_INVESTMENT() { }
    }
}
