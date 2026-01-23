using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_INTEREST : ModelEntityBase {
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

        private System.Decimal? INTEREST_PERCENTAGEValue;
        public System.Decimal? INTEREST_PERCENTAGE
        {
            get { return this.INTEREST_PERCENTAGEValue; }
            set { SetProperty(ref INTEREST_PERCENTAGEValue, value); }
        }

        private System.Decimal? ROYALTY_RATEValue;
        public System.Decimal? ROYALTY_RATE
        {
            get { return this.ROYALTY_RATEValue; }
            set { SetProperty(ref ROYALTY_RATEValue, value); }
        }

        private System.DateTime? EFFECTIVE_START_DATEValue;
        public System.DateTime? EFFECTIVE_START_DATE
        {
            get { return this.EFFECTIVE_START_DATEValue; }
            set { SetProperty(ref EFFECTIVE_START_DATEValue, value); }
        }

        private System.DateTime? EFFECTIVE_END_DATEValue;
        public System.DateTime? EFFECTIVE_END_DATE
        {
            get { return this.EFFECTIVE_END_DATEValue; }
            set { SetProperty(ref EFFECTIVE_END_DATEValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.String DIVISION_ORDER_IDValue;
        public System.String DIVISION_ORDER_ID
        {
            get { return this.DIVISION_ORDER_IDValue; }
            set { SetProperty(ref DIVISION_ORDER_IDValue, value); }
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


