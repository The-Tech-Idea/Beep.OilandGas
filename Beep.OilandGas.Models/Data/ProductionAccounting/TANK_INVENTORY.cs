using System.ComponentModel;
using System.Runtime.CompilerServices;
using Beep.OilandGas.PPDM.Models;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class TANK_INVENTORY : Entity, IPPDMEntity
    {
        private System.String TANK_INVENTORY_IDValue;
        public System.String TANK_INVENTORY_ID
        {
            get { return this.TANK_INVENTORY_IDValue; }
            set { SetProperty(ref TANK_INVENTORY_IDValue, value); }
        }

        private System.DateTime? INVENTORY_DATEValue;
        public System.DateTime? INVENTORY_DATE
        {
            get { return this.INVENTORY_DATEValue; }
            set { SetProperty(ref INVENTORY_DATEValue, value); }
        }

        private System.String TANK_BATTERY_IDValue;
        public System.String TANK_BATTERY_ID
        {
            get { return this.TANK_BATTERY_IDValue; }
            set { SetProperty(ref TANK_BATTERY_IDValue, value); }
        }

        private System.Decimal? OPENING_INVENTORYValue;
        public System.Decimal? OPENING_INVENTORY
        {
            get { return this.OPENING_INVENTORYValue; }
            set { SetProperty(ref OPENING_INVENTORYValue, value); }
        }

        private System.Decimal? RECEIPTSValue;
        public System.Decimal? RECEIPTS
        {
            get { return this.RECEIPTSValue; }
            set { SetProperty(ref RECEIPTSValue, value); }
        }

        private System.Decimal? DELIVERIESValue;
        public System.Decimal? DELIVERIES
        {
            get { return this.DELIVERIESValue; }
            set { SetProperty(ref DELIVERIESValue, value); }
        }

        private System.Decimal? ADJUSTMENTSValue;
        public System.Decimal? ADJUSTMENTS
        {
            get { return this.ADJUSTMENTSValue; }
            set { SetProperty(ref ADJUSTMENTSValue, value); }
        }

        private System.Decimal? SHRINKAGEValue;
        public System.Decimal? SHRINKAGE
        {
            get { return this.SHRINKAGEValue; }
            set { SetProperty(ref SHRINKAGEValue, value); }
        }

        private System.Decimal? THEFT_LOSSValue;
        public System.Decimal? THEFT_LOSS
        {
            get { return this.THEFT_LOSSValue; }
            set { SetProperty(ref THEFT_LOSSValue, value); }
        }

        private System.Decimal? ACTUAL_CLOSING_INVENTORYValue;
        public System.Decimal? ACTUAL_CLOSING_INVENTORY
        {
            get { return this.ACTUAL_CLOSING_INVENTORYValue; }
            set { SetProperty(ref ACTUAL_CLOSING_INVENTORYValue, value); }
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





