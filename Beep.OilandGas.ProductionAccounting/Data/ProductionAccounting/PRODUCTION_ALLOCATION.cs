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

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
