using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class REVENUE_DISTRIBUTION : ModelEntityBase {
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

        private String ROW_IDValue;
        public String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
