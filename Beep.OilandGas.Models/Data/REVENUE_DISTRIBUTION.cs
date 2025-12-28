using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class REVENUE_DISTRIBUTION : Entity
    {
        private String REVENUE_DISTRIBUTION_IDValue;
        public String REVENUE_DISTRIBUTION_ID
        {
            get { return this.REVENUE_DISTRIBUTION_IDValue; }
            set { SetProperty(ref REVENUE_DISTRIBUTION_IDValue, value); }
        }

        private String REVENUE_TRANSACTION_IDValue;
        public String REVENUE_TRANSACTION_ID
        {
            get { return this.REVENUE_TRANSACTION_IDValue; }
            set { SetProperty(ref REVENUE_TRANSACTION_IDValue, value); }
        }

        private String INT_SET_PARTNER_IDValue;
        public String INT_SET_PARTNER_ID
        {
            get { return this.INT_SET_PARTNER_IDValue; }
            set { SetProperty(ref INT_SET_PARTNER_IDValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private Decimal? WORKING_INTEREST_PERCENTAGEValue;
        public Decimal? WORKING_INTEREST_PERCENTAGE
        {
            get { return this.WORKING_INTEREST_PERCENTAGEValue; }
            set { SetProperty(ref WORKING_INTEREST_PERCENTAGEValue, value); }
        }

        private Decimal? NET_REVENUE_INTEREST_PERCENTAGEValue;
        public Decimal? NET_REVENUE_INTEREST_PERCENTAGE
        {
            get { return this.NET_REVENUE_INTEREST_PERCENTAGEValue; }
            set { SetProperty(ref NET_REVENUE_INTEREST_PERCENTAGEValue, value); }
        }

        private Decimal REVENUE_AMOUNTValue;
        public Decimal REVENUE_AMOUNT
        {
            get { return this.REVENUE_AMOUNTValue; }
            set { SetProperty(ref REVENUE_AMOUNTValue, value); }
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

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
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

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
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

