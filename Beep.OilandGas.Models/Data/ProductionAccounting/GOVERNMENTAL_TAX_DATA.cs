using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class GOVERNMENTAL_TAX_DATA : ModelEntityBase {
        private System.String GOVERNMENTAL_TAX_DATA_IDValue;
        public System.String GOVERNMENTAL_TAX_DATA_ID
        {
            get { return this.GOVERNMENTAL_TAX_DATA_IDValue; }
            set { SetProperty(ref GOVERNMENTAL_TAX_DATA_IDValue, value); }
        }

        private System.Decimal? SEVERANCE_TAXValue;
        public System.Decimal? SEVERANCE_TAX
        {
            get { return this.SEVERANCE_TAXValue; }
            set { SetProperty(ref SEVERANCE_TAXValue, value); }
        }

        private System.Decimal? AD_VALOREM_TAXValue;
        public System.Decimal? AD_VALOREM_TAX
        {
            get { return this.AD_VALOREM_TAXValue; }
            set { SetProperty(ref AD_VALOREM_TAXValue, value); }
        }

        private System.Decimal? TOTAL_TAXESValue;
        public System.Decimal? TOTAL_TAXES
        {
            get { return this.TOTAL_TAXESValue; }
            set { SetProperty(ref TOTAL_TAXESValue, value); }
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


