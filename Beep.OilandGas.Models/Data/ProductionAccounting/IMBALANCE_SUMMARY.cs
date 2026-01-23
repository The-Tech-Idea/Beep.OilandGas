using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
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

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


