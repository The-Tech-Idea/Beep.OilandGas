using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class COST_REPORT_SUMMARY : ModelEntityBase {
        private System.String COST_REPORT_SUMMARY_IDValue;
        public System.String COST_REPORT_SUMMARY_ID
        {
            get { return this.COST_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref COST_REPORT_SUMMARY_IDValue, value); }
        }

        private System.Decimal? TOTAL_LIFTING_COSTSValue;
        public System.Decimal? TOTAL_LIFTING_COSTS
        {
            get { return this.TOTAL_LIFTING_COSTSValue; }
            set { SetProperty(ref TOTAL_LIFTING_COSTSValue, value); }
        }

        private System.Decimal? TOTAL_OPERATING_COSTSValue;
        public System.Decimal? TOTAL_OPERATING_COSTS
        {
            get { return this.TOTAL_OPERATING_COSTSValue; }
            set { SetProperty(ref TOTAL_OPERATING_COSTSValue, value); }
        }

        private System.Decimal? TOTAL_MARKETING_COSTSValue;
        public System.Decimal? TOTAL_MARKETING_COSTS
        {
            get { return this.TOTAL_MARKETING_COSTSValue; }
            set { SetProperty(ref TOTAL_MARKETING_COSTSValue, value); }
        }

        private System.Decimal? TOTAL_COSTSValue;
        public System.Decimal? TOTAL_COSTS
        {
            get { return this.TOTAL_COSTSValue; }
            set { SetProperty(ref TOTAL_COSTSValue, value); }
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


