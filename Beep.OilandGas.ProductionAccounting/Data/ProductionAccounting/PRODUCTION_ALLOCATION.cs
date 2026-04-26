using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PRODUCTION_ALLOCATION : ModelEntityBase {
        private String PRODUCTION_ALLOCATION_IDValue;
        public String PRODUCTION_ALLOCATION_ID
        {
            get { return this.PRODUCTION_ALLOCATION_IDValue; }
            set { SetProperty(ref PRODUCTION_ALLOCATION_IDValue, value); }
        }

        private String PDEN_IDValue;
        public String PDEN_ID
        {
            get { return this.PDEN_IDValue; }
            set { SetProperty(ref PDEN_IDValue, value); }
        }
        private String FIELD_IDValue;
        public String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private String WELL_IDValue;
        public String WELL_ID
        {
            get { return this.WELL_IDValue; }
            set { SetProperty(ref WELL_IDValue, value); }
        }

        private String POOL_IDValue;
        public String POOL_ID
        {
            get { return this.POOL_IDValue; }
            set { SetProperty(ref POOL_IDValue, value); }
        }

        private DateTime? ALLOCATION_DATEValue;
        public DateTime? ALLOCATION_DATE
        {
            get { return this.ALLOCATION_DATEValue; }
            set { SetProperty(ref ALLOCATION_DATEValue, value); }
        }

        private Decimal TOTAL_PRODUCTIONValue;
        public Decimal TOTAL_PRODUCTION
        {
            get { return this.TOTAL_PRODUCTIONValue; }
            set { SetProperty(ref TOTAL_PRODUCTIONValue, value); }
        }

        private String ALLOCATION_METHODValue;
        public String ALLOCATION_METHOD
        {
            get { return this.ALLOCATION_METHODValue; }
            set { SetProperty(ref ALLOCATION_METHODValue, value); }
        }

        private String ALLOCATION_RESULTS_JSONValue;
        public String ALLOCATION_RESULTS_JSON
        {
            get { return this.ALLOCATION_RESULTS_JSONValue; }
            set { SetProperty(ref ALLOCATION_RESULTS_JSONValue, value); }
        }

        private String DESCRIPTIONValue;
        public String DESCRIPTION
        {
            get { return this.DESCRIPTIONValue; }
            set { SetProperty(ref DESCRIPTIONValue, value); }
        }

        // ── Best-practice additions (PPDM 3.9 PDEN / API MPMS allocation) ──────

        // FLUID_TYPE: OIL / GAS / NGL / CONDENSATE / WATER
        private String FLUID_TYPEValue;
        public String FLUID_TYPE
        {
            get { return this.FLUID_TYPEValue; }
            set { SetProperty(ref FLUID_TYPEValue, value); }
        }

        private DateTime? PRODUCTION_PERIOD_STARTValue;
        public DateTime? PRODUCTION_PERIOD_START
        {
            get { return this.PRODUCTION_PERIOD_STARTValue; }
            set { SetProperty(ref PRODUCTION_PERIOD_STARTValue, value); }
        }

        private DateTime? PRODUCTION_PERIOD_ENDValue;
        public DateTime? PRODUCTION_PERIOD_END
        {
            get { return this.PRODUCTION_PERIOD_ENDValue; }
            set { SetProperty(ref PRODUCTION_PERIOD_ENDValue, value); }
        }

        // ALLOCATED_VOLUME: the well/lease share resulting from the allocation run
        private Decimal? ALLOCATED_VOLUMEValue;
        public Decimal? ALLOCATED_VOLUME
        {
            get { return this.ALLOCATED_VOLUMEValue; }
            set { SetProperty(ref ALLOCATED_VOLUMEValue, value); }
        }

        // ALLOCATED_VOLUME_OUOM: BBL / MCF / MMSCFD etc.
        private String ALLOCATED_VOLUME_OUOMValue;
        public String ALLOCATED_VOLUME_OUOM
        {
            get { return this.ALLOCATED_VOLUME_OUOMValue; }
            set { SetProperty(ref ALLOCATED_VOLUME_OUOMValue, value); }
        }

        // ALLOCATION_FACTOR: decimal fraction applied to total measured volume (0.0–1.0)
        private Decimal? ALLOCATION_FACTORValue;
        public Decimal? ALLOCATION_FACTOR
        {
            get { return this.ALLOCATION_FACTORValue; }
            set { SetProperty(ref ALLOCATION_FACTORValue, value); }
        }

        // ALLOCATION_BASIS: TEST_RATE / METER_READING / GAUGE / CALCULATED
        private String ALLOCATION_BASISValue;
        public String ALLOCATION_BASIS
        {
            get { return this.ALLOCATION_BASISValue; }
            set { SetProperty(ref ALLOCATION_BASISValue, value); }
        }

        // ALLOCATION_STATUS: PROVISIONAL / FINAL / ADJUSTED / DISPUTED
        private String ALLOCATION_STATUSValue;
        public String ALLOCATION_STATUS
        {
            get { return this.ALLOCATION_STATUSValue; }
            set { SetProperty(ref ALLOCATION_STATUSValue, value); }
        }
    }
}
