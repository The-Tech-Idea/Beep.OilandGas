using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class DEVELOPMENT_COSTS : Entity
    {
        private System.String DEVELOPMENT_COST_IDValue;
        public System.String DEVELOPMENT_COST_ID
        {
            get { return this.DEVELOPMENT_COST_IDValue; }
            set { SetProperty(ref DEVELOPMENT_COST_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.Decimal? DEVELOPMENT_WELL_DRILLING_COSTSValue;
        public System.Decimal? DEVELOPMENT_WELL_DRILLING_COSTS
        {
            get { return this.DEVELOPMENT_WELL_DRILLING_COSTSValue; }
            set { SetProperty(ref DEVELOPMENT_WELL_DRILLING_COSTSValue, value); }
        }

        private System.Decimal? DEVELOPMENT_WELL_EQUIPMENTValue;
        public System.Decimal? DEVELOPMENT_WELL_EQUIPMENT
        {
            get { return this.DEVELOPMENT_WELL_EQUIPMENTValue; }
            set { SetProperty(ref DEVELOPMENT_WELL_EQUIPMENTValue, value); }
        }

        private System.Decimal? SUPPORT_EQUIPMENT_AND_FACILITIESValue;
        public System.Decimal? SUPPORT_EQUIPMENT_AND_FACILITIES
        {
            get { return this.SUPPORT_EQUIPMENT_AND_FACILITIESValue; }
            set { SetProperty(ref SUPPORT_EQUIPMENT_AND_FACILITIESValue, value); }
        }

        private System.Decimal? SERVICE_WELL_COSTSValue;
        public System.Decimal? SERVICE_WELL_COSTS
        {
            get { return this.SERVICE_WELL_COSTSValue; }
            set { SetProperty(ref SERVICE_WELL_COSTSValue, value); }
        }

        private System.Decimal? TOTAL_DEVELOPMENT_COSTSValue;
        public System.Decimal? TOTAL_DEVELOPMENT_COSTS
        {
            get { return this.TOTAL_DEVELOPMENT_COSTSValue; }
            set { SetProperty(ref TOTAL_DEVELOPMENT_COSTSValue, value); }
        }

        private System.DateTime? COST_DATEValue;
        public System.DateTime? COST_DATE
        {
            get { return this.COST_DATEValue; }
            set { SetProperty(ref COST_DATEValue, value); }
        }

        private System.String COST_CENTER_IDValue;
        public System.String COST_CENTER_ID
        {
            get { return this.COST_CENTER_IDValue; }
            set { SetProperty(ref COST_CENTER_IDValue, value); }
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

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}

