using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class IMBALANCE_SUMMARY : ModelEntityBase {
        private System.String IMBALANCE_SUMMARY_IDValue;
        public System.String IMBALANCE_SUMMARY_ID
        {
            get { return this.IMBALANCE_SUMMARY_IDValue; }
            set { SetProperty(ref IMBALANCE_SUMMARY_IDValue, value); }
        }

        private System.String IMBALANCE_STATEMENT_IDValue;
        public System.String IMBALANCE_STATEMENT_ID
        {
            get { return this.IMBALANCE_STATEMENT_IDValue; }
            set { SetProperty(ref IMBALANCE_STATEMENT_IDValue, value); }
        }

        private System.String SUMMARY_TYPEValue;
        public System.String SUMMARY_TYPE
        {
            get { return this.SUMMARY_TYPEValue; }
            set { SetProperty(ref SUMMARY_TYPEValue, value); }
        }

        private System.Decimal? TOTAL_VOLUMEValue;
        public System.Decimal? TOTAL_VOLUME
        {
            get { return this.TOTAL_VOLUMEValue; }
            set { SetProperty(ref TOTAL_VOLUMEValue, value); }
        }

        private System.Int32? TRANSACTION_COUNTValue;
        public System.Int32? TRANSACTION_COUNT
        {
            get { return this.TRANSACTION_COUNTValue; }
            set { SetProperty(ref TRANSACTION_COUNTValue, value); }
        }

        private System.Decimal? AVERAGE_DAILY_VOLUMEValue;
        public System.Decimal? AVERAGE_DAILY_VOLUME
        {
            get { return this.AVERAGE_DAILY_VOLUMEValue; }
            set { SetProperty(ref AVERAGE_DAILY_VOLUMEValue, value); }
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




