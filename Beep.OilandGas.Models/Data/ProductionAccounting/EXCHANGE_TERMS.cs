using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_TERMS : ModelEntityBase {
        private System.String EXCHANGE_TERMS_IDValue;
        public System.String EXCHANGE_TERMS_ID
        {
            get { return this.EXCHANGE_TERMS_IDValue; }
            set { SetProperty(ref EXCHANGE_TERMS_IDValue, value); }
        }

        private System.String EXCHANGE_CONTRACT_IDValue;
        public System.String EXCHANGE_CONTRACT_ID
        {
            get { return this.EXCHANGE_CONTRACT_IDValue; }
            set { SetProperty(ref EXCHANGE_CONTRACT_IDValue, value); }
        }

        private System.Decimal? EXCHANGE_RATIOValue;
        public System.Decimal? EXCHANGE_RATIO
        {
            get { return this.EXCHANGE_RATIOValue; }
            set { SetProperty(ref EXCHANGE_RATIOValue, value); }
        }

        private System.Decimal? MINIMUM_VOLUMEValue;
        public System.Decimal? MINIMUM_VOLUME
        {
            get { return this.MINIMUM_VOLUMEValue; }
            set { SetProperty(ref MINIMUM_VOLUMEValue, value); }
        }

        private System.Decimal? MAXIMUM_VOLUMEValue;
        public System.Decimal? MAXIMUM_VOLUME
        {
            get { return this.MAXIMUM_VOLUMEValue; }
            set { SetProperty(ref MAXIMUM_VOLUMEValue, value); }
        }

        private System.String QUALITY_SPECIFICATIONSValue;
        public System.String QUALITY_SPECIFICATIONS
        {
            get { return this.QUALITY_SPECIFICATIONSValue; }
            set { SetProperty(ref QUALITY_SPECIFICATIONSValue, value); }
        }

        private System.Int32? NOTICE_PERIOD_DAYSValue;
        public System.Int32? NOTICE_PERIOD_DAYS
        {
            get { return this.NOTICE_PERIOD_DAYSValue; }
            set { SetProperty(ref NOTICE_PERIOD_DAYSValue, value); }
        }

        // Standard PPDM columns
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}




