using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.PPDM39.Models
{
    public partial class REVENUE_TRANSACTION : Entity
    {
        private String REVENUE_TRANSACTION_IDValue;
        public String REVENUE_TRANSACTION_ID
        {
            get { return this.REVENUE_TRANSACTION_IDValue; }
            set { SetProperty(ref REVENUE_TRANSACTION_IDValue, value); }
        }

        private String FINANCE_IDValue;
        public String FINANCE_ID
        {
            get { return this.FINANCE_IDValue; }
            set { SetProperty(ref FINANCE_IDValue, value); }
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

        private String PDEN_IDValue;
        public String PDEN_ID
        {
            get { return this.PDEN_IDValue; }
            set { SetProperty(ref PDEN_IDValue, value); }
        }

        private DateTime PRODUCTION_DATEValue;
        public DateTime PRODUCTION_DATE
        {
            get { return this.PRODUCTION_DATEValue; }
            set { SetProperty(ref PRODUCTION_DATEValue, value); }
        }

        private Decimal?? OIL_VOLUMEValue;
        public Decimal?? OIL_VOLUME
        {
            get { return this.OIL_VOLUMEValue; }
            set { SetProperty(ref OIL_VOLUMEValue, value); }
        }

        private Decimal?? GAS_VOLUMEValue;
        public Decimal?? GAS_VOLUME
        {
            get { return this.GAS_VOLUMEValue; }
            set { SetProperty(ref GAS_VOLUMEValue, value); }
        }

        private Decimal?? OIL_PRICEValue;
        public Decimal?? OIL_PRICE
        {
            get { return this.OIL_PRICEValue; }
            set { SetProperty(ref OIL_PRICEValue, value); }
        }

        private Decimal?? GAS_PRICEValue;
        public Decimal?? GAS_PRICE
        {
            get { return this.GAS_PRICEValue; }
            set { SetProperty(ref GAS_PRICEValue, value); }
        }

        private Decimal GROSS_REVENUEValue;
        public Decimal GROSS_REVENUE
        {
            get { return this.GROSS_REVENUEValue; }
            set { SetProperty(ref GROSS_REVENUEValue, value); }
        }

        private Decimal?? NET_REVENUEValue;
        public Decimal?? NET_REVENUE
        {
            get { return this.NET_REVENUEValue; }
            set { SetProperty(ref NET_REVENUEValue, value); }
        }

        private String REVENUE_RECOGNITION_STATUSValue;
        public String REVENUE_RECOGNITION_STATUS
        {
            get { return this.REVENUE_RECOGNITION_STATUSValue; }
            set { SetProperty(ref REVENUE_RECOGNITION_STATUSValue, value); }
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