using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.Models
{
    public partial class PRODUCTION_ALLOCATION : Entity
    {
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

        private DateTime ALLOCATION_DATEValue;
        public DateTime ALLOCATION_DATE
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

        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime?? ROW_CHANGED_DATEValue;
        public DateTime?? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime?? ROW_CREATED_DATEValue;
        public DateTime?? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private DateTime?? ROW_EFFECTIVE_DATEValue;
        public DateTime?? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime?? ROW_EXPIRY_DATEValue;
        public DateTime?? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}