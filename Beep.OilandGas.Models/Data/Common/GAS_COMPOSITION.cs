using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Common
{
    public partial class GAS_COMPOSITION : Entity
    {
        private System.String GAS_COMPOSITION_IDValue;
        public System.String GAS_COMPOSITION_ID
        {
            get { return this.GAS_COMPOSITION_IDValue; }
            set { SetProperty(ref GAS_COMPOSITION_IDValue, value); }
        }

        private System.String COMPOSITION_NAMEValue;
        public System.String COMPOSITION_NAME
        {
            get { return this.COMPOSITION_NAMEValue; }
            set { SetProperty(ref COMPOSITION_NAMEValue, value); }
        }

        private System.DateTime? COMPOSITION_DATEValue;
        public System.DateTime? COMPOSITION_DATE
        {
            get { return this.COMPOSITION_DATEValue; }
            set { SetProperty(ref COMPOSITION_DATEValue, value); }
        }

        private System.Decimal? TOTAL_MOLE_FRACTIONValue;
        public System.Decimal? TOTAL_MOLE_FRACTION
        {
            get { return this.TOTAL_MOLE_FRACTIONValue; }
            set { SetProperty(ref TOTAL_MOLE_FRACTIONValue, value); }
        }

        private System.Decimal? MOLECULAR_WEIGHTValue;
        public System.Decimal? MOLECULAR_WEIGHT
        {
            get { return this.MOLECULAR_WEIGHTValue; }
            set { SetProperty(ref MOLECULAR_WEIGHTValue, value); }
        }

        private System.Decimal? SPECIFIC_GRAVITYValue;
        public System.Decimal? SPECIFIC_GRAVITY
        {
            get { return this.SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref SPECIFIC_GRAVITYValue, value); }
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

