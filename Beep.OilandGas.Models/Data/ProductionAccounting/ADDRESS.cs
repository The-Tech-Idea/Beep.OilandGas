using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ADDRESS : ModelEntityBase {
        private System.String ADDRESS_IDValue;
        public System.String ADDRESS_ID
        {
            get { return this.ADDRESS_IDValue; }
            set { SetProperty(ref ADDRESS_IDValue, value); }
        }

        private System.String OWNER_INFORMATION_IDValue;
        public System.String OWNER_INFORMATION_ID
        {
            get { return this.OWNER_INFORMATION_IDValue; }
            set { SetProperty(ref OWNER_INFORMATION_IDValue, value); }
        }

        private System.String STREET_ADDRESSValue;
        public System.String STREET_ADDRESS
        {
            get { return this.STREET_ADDRESSValue; }
            set { SetProperty(ref STREET_ADDRESSValue, value); }
        }

        private System.String CITYValue;
        public System.String CITY
        {
            get { return this.CITYValue; }
            set { SetProperty(ref CITYValue, value); }
        }

        private System.String STATEValue;
        public System.String STATE
        {
            get { return this.STATEValue; }
            set { SetProperty(ref STATEValue, value); }
        }

        private System.String ZIP_CODEValue;
        public System.String ZIP_CODE
        {
            get { return this.ZIP_CODEValue; }
            set { SetProperty(ref ZIP_CODEValue, value); }
        }

        private System.String COUNTRYValue;
        public System.String COUNTRY
        {
            get { return this.COUNTRYValue; }
            set { SetProperty(ref COUNTRYValue, value); }
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


