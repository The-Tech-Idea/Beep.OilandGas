using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class APPROVAL_WORKFLOW : ModelEntityBase {
        private System.String APPROVAL_WORKFLOW_IDValue;
        public System.String APPROVAL_WORKFLOW_ID
        {
            get { return this.APPROVAL_WORKFLOW_IDValue; }
            set { SetProperty(ref APPROVAL_WORKFLOW_IDValue, value); }
        }

        private System.String ENTITY_NAMEValue;
        public System.String ENTITY_NAME
        {
            get { return this.ENTITY_NAMEValue; }
            set { SetProperty(ref ENTITY_NAMEValue, value); }
        }

        private System.String ENTITY_IDValue;
        public System.String ENTITY_ID
        {
            get { return this.ENTITY_IDValue; }
            set { SetProperty(ref ENTITY_IDValue, value); }
        }

        private System.Decimal? AMOUNTValue;
        public System.Decimal? AMOUNT
        {
            get { return this.AMOUNTValue; }
            set { SetProperty(ref AMOUNTValue, value); }
        }

        private System.String STATUSValue;
        public System.String STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        private System.String REQUESTED_BYValue;
        public System.String REQUESTED_BY
        {
            get { return this.REQUESTED_BYValue; }
            set { SetProperty(ref REQUESTED_BYValue, value); }
        }

        private System.DateTime? REQUESTED_DATEValue;
        public System.DateTime? REQUESTED_DATE
        {
            get { return this.REQUESTED_DATEValue; }
            set { SetProperty(ref REQUESTED_DATEValue, value); }
        }

        private System.String APPROVED_BYValue;
        public System.String APPROVED_BY
        {
            get { return this.APPROVED_BYValue; }
            set { SetProperty(ref APPROVED_BYValue, value); }
        }

        private System.DateTime? APPROVAL_DATEValue;
        public System.DateTime? APPROVAL_DATE
        {
            get { return this.APPROVAL_DATEValue; }
            set { SetProperty(ref APPROVAL_DATEValue, value); }
        }

        private System.String COMMENTSValue;
        public System.String COMMENTS
        {
            get { return this.COMMENTSValue; }
            set { SetProperty(ref COMMENTSValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
