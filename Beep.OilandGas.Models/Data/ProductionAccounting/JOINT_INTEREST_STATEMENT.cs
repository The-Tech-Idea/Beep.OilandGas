using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class JOINT_INTEREST_STATEMENT : ModelEntityBase {
        private System.String JOINT_INTEREST_STATEMENT_IDValue;
        public System.String JOINT_INTEREST_STATEMENT_ID
        {
            get { return this.JOINT_INTEREST_STATEMENT_IDValue; }
            set { SetProperty(ref JOINT_INTEREST_STATEMENT_IDValue, value); }
        }

        private System.String REPORT_TYPEValue;
        public System.String REPORT_TYPE
        {
            get { return this.REPORT_TYPEValue; }
            set { SetProperty(ref REPORT_TYPEValue, value); }
        }

        private System.DateTime? REPORT_PERIOD_STARTValue;
        public System.DateTime? REPORT_PERIOD_START
        {
            get { return this.REPORT_PERIOD_STARTValue; }
            set { SetProperty(ref REPORT_PERIOD_STARTValue, value); }
        }

        private System.DateTime? REPORT_PERIOD_ENDValue;
        public System.DateTime? REPORT_PERIOD_END
        {
            get { return this.REPORT_PERIOD_ENDValue; }
            set { SetProperty(ref REPORT_PERIOD_ENDValue, value); }
        }

        private System.DateTime? GENERATION_DATEValue;
        public System.DateTime? GENERATION_DATE
        {
            get { return this.GENERATION_DATEValue; }
            set { SetProperty(ref GENERATION_DATEValue, value); }
        }

        private System.String GENERATED_BYValue;
        public System.String GENERATED_BY
        {
            get { return this.GENERATED_BYValue; }
            set { SetProperty(ref GENERATED_BYValue, value); }
        }

        private System.String JIB_IDValue;
        public System.String JIB_ID
        {
            get { return this.JIB_IDValue; }
            set { SetProperty(ref JIB_IDValue, value); }
        }

        private System.String OPERATORValue;
        public System.String OPERATOR
        {
            get { return this.OPERATORValue; }
            set { SetProperty(ref OPERATORValue, value); }
        }

        private System.Decimal? TOTAL_CHARGESValue;
        public System.Decimal? TOTAL_CHARGES
        {
            get { return this.TOTAL_CHARGESValue; }
            set { SetProperty(ref TOTAL_CHARGESValue, value); }
        }

        private System.Decimal? TOTAL_CREDITSValue;
        public System.Decimal? TOTAL_CREDITS
        {
            get { return this.TOTAL_CREDITSValue; }
            set { SetProperty(ref TOTAL_CREDITSValue, value); }
        }

        private System.Decimal? NET_AMOUNTValue;
        public System.Decimal? NET_AMOUNT
        {
            get { return this.NET_AMOUNTValue; }
            set { SetProperty(ref NET_AMOUNTValue, value); }
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

        private string STATEMENT_IDValue;
        public string STATEMENT_ID
        {
            get { return this.STATEMENT_IDValue; }
            set { SetProperty(ref STATEMENT_IDValue, value); }
        }

        private string PROPERTY_OR_LEASE_IDValue;
        public string PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private DateTime STATEMENT_PERIOD_STARTValue;
        public DateTime STATEMENT_PERIOD_START
        {
            get { return this.STATEMENT_PERIOD_STARTValue; }
            set { SetProperty(ref STATEMENT_PERIOD_STARTValue, value); }
        }

        private DateTime STATEMENT_PERIOD_ENDValue;
        public DateTime STATEMENT_PERIOD_END
        {
            get { return this.STATEMENT_PERIOD_ENDValue; }
            set { SetProperty(ref STATEMENT_PERIOD_ENDValue, value); }
        }

        private DateTime GENERATED_DATEValue;
        public DateTime GENERATED_DATE
        {
            get { return this.GENERATED_DATEValue; }
            set { SetProperty(ref GENERATED_DATEValue, value); }
        }

        private ProductionSummary? PRODUCTION_SUMMARYValue;
        public ProductionSummary? PRODUCTION_SUMMARY
        {
            get { return this.PRODUCTION_SUMMARYValue; }
            set { SetProperty(ref PRODUCTION_SUMMARYValue, value); }
        }

        private RevenueSummary? REVENUE_SUMMARYValue;
        public RevenueSummary? REVENUE_SUMMARY
        {
            get { return this.REVENUE_SUMMARYValue; }
            set { SetProperty(ref REVENUE_SUMMARYValue, value); }
        }
    }
}
