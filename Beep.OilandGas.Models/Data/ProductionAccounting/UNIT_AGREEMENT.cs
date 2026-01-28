using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class UNIT_AGREEMENT : ModelEntityBase {
        private System.String UNIT_IDValue;
        public System.String UNIT_ID
        {
            get { return this.UNIT_IDValue; }
            set { SetProperty(ref UNIT_IDValue, value); }
        }

        private System.String UNIT_NAMEValue;
        public System.String UNIT_NAME
        {
            get { return this.UNIT_NAMEValue; }
            set { SetProperty(ref UNIT_NAMEValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.String UNIT_OPERATORValue;
        public System.String UNIT_OPERATOR
        {
            get { return this.UNIT_OPERATORValue; }
            set { SetProperty(ref UNIT_OPERATORValue, value); }
        }

        private System.String TERMS_AND_CONDITIONSValue;
        public System.String TERMS_AND_CONDITIONS
        {
            get { return this.TERMS_AND_CONDITIONSValue; }
            set { SetProperty(ref TERMS_AND_CONDITIONSValue, value); }
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
