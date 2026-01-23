using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class FINANCIAL_REPORT : ModelEntityBase {
        private System.String FINANCIAL_REPORT_IDValue;
        public System.String FINANCIAL_REPORT_ID
        {
            get { return this.FINANCIAL_REPORT_IDValue; }
            set { SetProperty(ref FINANCIAL_REPORT_IDValue, value); }
        }

        private System.String REPORT_TYPEValue;
        public System.String REPORT_TYPE
        {
            get { return this.REPORT_TYPEValue; }
            set { SetProperty(ref REPORT_TYPEValue, value); }
        }

        private System.DateTime? PERIOD_STARTValue;
        public System.DateTime? PERIOD_START
        {
            get { return this.PERIOD_STARTValue; }
            set { SetProperty(ref PERIOD_STARTValue, value); }
        }

        private System.DateTime? PERIOD_ENDValue;
        public System.DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private System.String OWNER_BA_IDValue;
        public System.String OWNER_BA_ID
        {
            get { return this.OWNER_BA_IDValue; }
            set { SetProperty(ref OWNER_BA_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.Decimal? TOTAL_REVENUEValue;
        public System.Decimal? TOTAL_REVENUE
        {
            get { return this.TOTAL_REVENUEValue; }
            set { SetProperty(ref TOTAL_REVENUEValue, value); }
        }

        private System.Decimal? TOTAL_EXPENSESValue;
        public System.Decimal? TOTAL_EXPENSES
        {
            get { return this.TOTAL_EXPENSESValue; }
            set { SetProperty(ref TOTAL_EXPENSESValue, value); }
        }

        private System.Decimal? NET_INCOMEValue;
        public System.Decimal? NET_INCOME
        {
            get { return this.NET_INCOMEValue; }
            set { SetProperty(ref NET_INCOMEValue, value); }
        }

        private System.Decimal? TAXABLE_INCOMEValue;
        public System.Decimal? TAXABLE_INCOME
        {
            get { return this.TAXABLE_INCOMEValue; }
            set { SetProperty(ref TAXABLE_INCOMEValue, value); }
        }

        private System.Decimal? TAX_LIABILITYValue;
        public System.Decimal? TAX_LIABILITY
        {
            get { return this.TAX_LIABILITYValue; }
            set { SetProperty(ref TAX_LIABILITYValue, value); }
        }

        private System.Decimal? ROYALTY_AMOUNTValue;
        public System.Decimal? ROYALTY_AMOUNT
        {
            get { return this.ROYALTY_AMOUNTValue; }
            set { SetProperty(ref ROYALTY_AMOUNTValue, value); }
        }

        private System.Decimal? DEDUCTIONSValue;
        public System.Decimal? DEDUCTIONS
        {
            get { return this.DEDUCTIONSValue; }
            set { SetProperty(ref DEDUCTIONSValue, value); }
        }

        private System.Decimal? NET_PAYMENTValue;
        public System.Decimal? NET_PAYMENT
        {
            get { return this.NET_PAYMENTValue; }
            set { SetProperty(ref NET_PAYMENTValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


