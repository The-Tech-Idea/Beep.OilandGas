using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INTERNAL_CONTROL_RULE : Entity, IPPDMEntity
    {
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
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }
    }
}
