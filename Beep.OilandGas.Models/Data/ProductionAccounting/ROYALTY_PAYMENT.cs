using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_PAYMENT : ModelEntityBase {
        private System.String ROYALTY_PAYMENT_IDValue;
        public System.String ROYALTY_PAYMENT_ID
        {
            get { return this.ROYALTY_PAYMENT_IDValue; }
            set { SetProperty(ref ROYALTY_PAYMENT_IDValue, value); }
        }

        private System.String ROYALTY_INTEREST_IDValue;
        public System.String ROYALTY_INTEREST_ID
        {
            get { return this.ROYALTY_INTEREST_IDValue; }
            set { SetProperty(ref ROYALTY_INTEREST_IDValue, value); }
        }

        private System.String ROYALTY_OWNER_IDValue;
        public System.String ROYALTY_OWNER_ID
        {
            get { return this.ROYALTY_OWNER_IDValue; }
            set { SetProperty(ref ROYALTY_OWNER_IDValue, value); }
        }

        private System.String PROPERTY_OR_LEASE_IDValue;
        public System.String PROPERTY_OR_LEASE_ID
        {
            get { return this.PROPERTY_OR_LEASE_IDValue; }
            set { SetProperty(ref PROPERTY_OR_LEASE_IDValue, value); }
        }

        private System.DateTime? PAYMENT_PERIOD_STARTValue;
        public System.DateTime? PAYMENT_PERIOD_START
        {
            get { return this.PAYMENT_PERIOD_STARTValue; }
            set { SetProperty(ref PAYMENT_PERIOD_STARTValue, value); }
        }

        private System.DateTime? PAYMENT_PERIOD_ENDValue;
        public System.DateTime? PAYMENT_PERIOD_END
        {
            get { return this.PAYMENT_PERIOD_ENDValue; }
            set { SetProperty(ref PAYMENT_PERIOD_ENDValue, value); }
        }

        private System.Decimal? ROYALTY_AMOUNTValue;
        public System.Decimal? ROYALTY_AMOUNT
        {
            get { return this.ROYALTY_AMOUNTValue; }
            set { SetProperty(ref ROYALTY_AMOUNTValue, value); }
        }

        private System.DateTime? PAYMENT_DATEValue;
        public System.DateTime? PAYMENT_DATE
        {
            get { return this.PAYMENT_DATEValue; }
            set { SetProperty(ref PAYMENT_DATEValue, value); }
        }

        private System.String PAYMENT_METHODValue;
        public System.String PAYMENT_METHOD
        {
            get { return this.PAYMENT_METHODValue; }
            set { SetProperty(ref PAYMENT_METHODValue, value); }
        }

        private System.String CHECK_NUMBERValue;
        public System.String CHECK_NUMBER
        {
            get { return this.CHECK_NUMBERValue; }
            set { SetProperty(ref CHECK_NUMBERValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.Decimal? NET_PAYMENT_AMOUNTValue;
        public System.Decimal? NET_PAYMENT_AMOUNT
        {
            get { return this.NET_PAYMENT_AMOUNTValue; }
            set { SetProperty(ref NET_PAYMENT_AMOUNTValue, value); }
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


