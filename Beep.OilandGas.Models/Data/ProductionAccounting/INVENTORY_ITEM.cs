using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_ITEM : Entity,IPPDMEntity
    {
        private System.String INVENTORY_ITEM_IDValue;
        public System.String INVENTORY_ITEM_ID
        {
            get { return this.INVENTORY_ITEM_IDValue; }
            set { SetProperty(ref INVENTORY_ITEM_IDValue, value); }
        }

        private System.String ITEM_NUMBERValue;
        public System.String ITEM_NUMBER
        {
            get { return this.ITEM_NUMBERValue; }
            set { SetProperty(ref ITEM_NUMBERValue, value); }
        }

        private System.String ITEM_NAMEValue;
        public System.String ITEM_NAME
        {
            get { return this.ITEM_NAMEValue; }
            set { SetProperty(ref ITEM_NAMEValue, value); }
        }

        private System.String ITEM_TYPEValue;
        public System.String ITEM_TYPE
        {
            get { return this.ITEM_TYPEValue; }
            set { SetProperty(ref ITEM_TYPEValue, value); }
        }

        private System.String UNIT_OF_MEASUREValue;
        public System.String UNIT_OF_MEASURE
        {
            get { return this.UNIT_OF_MEASUREValue; }
            set { SetProperty(ref UNIT_OF_MEASUREValue, value); }
        }

        private System.Decimal? QUANTITY_ON_HANDValue;
        public System.Decimal? QUANTITY_ON_HAND
        {
            get { return this.QUANTITY_ON_HANDValue; }
            set { SetProperty(ref QUANTITY_ON_HANDValue, value); }
        }

        private System.Decimal? UNIT_COSTValue;
        public System.Decimal? UNIT_COST
        {
            get { return this.UNIT_COSTValue; }
            set { SetProperty(ref UNIT_COSTValue, value); }
        }

        private System.Decimal? TOTAL_VALUEValue;
        public System.Decimal? TOTAL_VALUE
        {
            get { return this.TOTAL_VALUEValue; }
            set { SetProperty(ref TOTAL_VALUEValue, value); }
        }

        private System.String VALUATION_METHODValue;
        public System.String VALUATION_METHOD
        {
            get { return this.VALUATION_METHODValue; }
            set { SetProperty(ref VALUATION_METHODValue, value); }
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

