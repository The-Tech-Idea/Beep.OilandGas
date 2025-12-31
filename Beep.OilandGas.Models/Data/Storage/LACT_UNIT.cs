using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Storage
{
    public partial class LACT_UNIT : Entity
    {
        private System.String LACT_UNIT_IDValue;
        public System.String LACT_UNIT_ID
        {
            get { return this.LACT_UNIT_IDValue; }
            set { SetProperty(ref LACT_UNIT_IDValue, value); }
        }

        private System.String LACT_NAMEValue;
        public System.String LACT_NAME
        {
            get { return this.LACT_NAMEValue; }
            set { SetProperty(ref LACT_NAMEValue, value); }
        }

        private System.String SERVICE_UNIT_IDValue;
        public System.String SERVICE_UNIT_ID
        {
            get { return this.SERVICE_UNIT_IDValue; }
            set { SetProperty(ref SERVICE_UNIT_IDValue, value); }
        }

        private System.String METER_TYPEValue;
        public System.String METER_TYPE
        {
            get { return this.METER_TYPEValue; }
            set { SetProperty(ref METER_TYPEValue, value); }
        }

        private System.Decimal? MAXIMUM_FLOW_RATEValue;
        public System.Decimal? MAXIMUM_FLOW_RATE
        {
            get { return this.MAXIMUM_FLOW_RATEValue; }
            set { SetProperty(ref MAXIMUM_FLOW_RATEValue, value); }
        }

        private System.Decimal? METER_FACTORValue;
        public System.Decimal? METER_FACTOR
        {
            get { return this.METER_FACTORValue; }
            set { SetProperty(ref METER_FACTORValue, value); }
        }

        private System.DateTime? LAST_CALIBRATION_DATEValue;
        public System.DateTime? LAST_CALIBRATION_DATE
        {
            get { return this.LAST_CALIBRATION_DATEValue; }
            set { SetProperty(ref LAST_CALIBRATION_DATEValue, value); }
        }

        private System.DateTime? NEXT_CALIBRATION_DATEValue;
        public System.DateTime? NEXT_CALIBRATION_DATE
        {
            get { return this.NEXT_CALIBRATION_DATEValue; }
            set { SetProperty(ref NEXT_CALIBRATION_DATEValue, value); }
        }

        private System.String IS_ACTIVEValue;
        public System.String IS_ACTIVE
        {
            get { return this.IS_ACTIVEValue; }
            set { SetProperty(ref IS_ACTIVEValue, value); }
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

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
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

