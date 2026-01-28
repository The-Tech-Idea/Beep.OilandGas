using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PRODUCTION_REPORT_SUMMARY : ModelEntityBase {
        private System.String PRODUCTION_REPORT_SUMMARY_IDValue;
        public System.String PRODUCTION_REPORT_SUMMARY_ID
        {
            get { return this.PRODUCTION_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref PRODUCTION_REPORT_SUMMARY_IDValue, value); }
        }

        private System.Decimal? TOTAL_PRODUCTIONValue;
        public System.Decimal? TOTAL_PRODUCTION
        {
            get { return this.TOTAL_PRODUCTIONValue; }
            set { SetProperty(ref TOTAL_PRODUCTIONValue, value); }
        }

        private System.Int32? PRODUCING_DAYSValue;
        public System.Int32? PRODUCING_DAYS
        {
            get { return this.PRODUCING_DAYSValue; }
            set { SetProperty(ref PRODUCING_DAYSValue, value); }
        }

        private System.Decimal? AVERAGE_DAILY_PRODUCTIONValue;
        public System.Decimal? AVERAGE_DAILY_PRODUCTION
        {
            get { return this.AVERAGE_DAILY_PRODUCTIONValue; }
            set { SetProperty(ref AVERAGE_DAILY_PRODUCTIONValue, value); }
        }

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

      
    }
}
