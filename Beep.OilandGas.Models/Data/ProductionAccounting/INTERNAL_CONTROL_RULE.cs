using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INTERNAL_CONTROL_RULE : ModelEntityBase {
        private System.String INTERNAL_CONTROL_RULE_IDValue;
        public System.String INTERNAL_CONTROL_RULE_ID
        {
            get { return this.INTERNAL_CONTROL_RULE_IDValue; }
            set { SetProperty(ref INTERNAL_CONTROL_RULE_IDValue, value); }
        }

        private System.String ENTITY_NAMEValue;
        public System.String ENTITY_NAME
        {
            get { return this.ENTITY_NAMEValue; }
            set { SetProperty(ref ENTITY_NAMEValue, value); }
        }

        private System.String RULE_TYPEValue;
        public System.String RULE_TYPE
        {
            get { return this.RULE_TYPEValue; }
            set { SetProperty(ref RULE_TYPEValue, value); }
        }

        private System.Decimal? THRESHOLD_AMOUNTValue;
        public System.Decimal? THRESHOLD_AMOUNT
        {
            get { return this.THRESHOLD_AMOUNTValue; }
            set { SetProperty(ref THRESHOLD_AMOUNTValue, value); }
        }

        private System.String REQUIRE_SECOND_APPROVERValue;
        public System.String REQUIRE_SECOND_APPROVER
        {
            get { return this.REQUIRE_SECOND_APPROVERValue; }
            set { SetProperty(ref REQUIRE_SECOND_APPROVERValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}


