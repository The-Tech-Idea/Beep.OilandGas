using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ROYALTY_DEDUCTIONS : ModelEntityBase {
        private System.String ROYALTY_DEDUCTIONS_IDValue;
        public System.String ROYALTY_DEDUCTIONS_ID
        {
            get { return this.ROYALTY_DEDUCTIONS_IDValue; }
            set { SetProperty(ref ROYALTY_DEDUCTIONS_IDValue, value); }
        }

        private System.Decimal? PRODUCTION_TAXESValue;
        public System.Decimal? PRODUCTION_TAXES
        {
            get { return this.PRODUCTION_TAXESValue; }
            set { SetProperty(ref PRODUCTION_TAXESValue, value); }
        }

        private System.Decimal? TRANSPORTATION_COSTSValue;
        public System.Decimal? TRANSPORTATION_COSTS
        {
            get { return this.TRANSPORTATION_COSTSValue; }
            set { SetProperty(ref TRANSPORTATION_COSTSValue, value); }
        }

        private System.Decimal? PROCESSING_COSTSValue;
        public System.Decimal? PROCESSING_COSTS
        {
            get { return this.PROCESSING_COSTSValue; }
            set { SetProperty(ref PROCESSING_COSTSValue, value); }
        }

        private System.Decimal? MARKETING_COSTSValue;
        public System.Decimal? MARKETING_COSTS
        {
            get { return this.MARKETING_COSTSValue; }
            set { SetProperty(ref MARKETING_COSTSValue, value); }
        }

        private System.Decimal? OTHER_DEDUCTIONSValue;
        public System.Decimal? OTHER_DEDUCTIONS
        {
            get { return this.OTHER_DEDUCTIONSValue; }
            set { SetProperty(ref OTHER_DEDUCTIONSValue, value); }
        }

        private System.Decimal? TOTAL_DEDUCTIONSValue;
        public System.Decimal? TOTAL_DEDUCTIONS
        {
            get { return this.TOTAL_DEDUCTIONSValue; }
            set { SetProperty(ref TOTAL_DEDUCTIONSValue, value); }
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


