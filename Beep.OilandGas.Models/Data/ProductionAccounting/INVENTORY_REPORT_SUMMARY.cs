using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class INVENTORY_REPORT_SUMMARY : ModelEntityBase {
        private System.String INVENTORY_REPORT_SUMMARY_IDValue;
        public System.String INVENTORY_REPORT_SUMMARY_ID
        {
            get { return this.INVENTORY_REPORT_SUMMARY_IDValue; }
            set { SetProperty(ref INVENTORY_REPORT_SUMMARY_IDValue, value); }
        }

        private System.Decimal  OPENING_INVENTORYValue;
        public System.Decimal  OPENING_INVENTORY
        {
            get { return this.OPENING_INVENTORYValue; }
            set { SetProperty(ref OPENING_INVENTORYValue, value); }
        }

        private System.Decimal  RECEIPTSValue;
        public System.Decimal  RECEIPTS
        {
            get { return this.RECEIPTSValue; }
            set { SetProperty(ref RECEIPTSValue, value); }
        }

        private System.Decimal  DELIVERIESValue;
        public System.Decimal  DELIVERIES
        {
            get { return this.DELIVERIESValue; }
            set { SetProperty(ref DELIVERIESValue, value); }
        }

        private System.Decimal  CLOSING_INVENTORYValue;
        public System.Decimal  CLOSING_INVENTORY
        {
            get { return this.CLOSING_INVENTORYValue; }
            set { SetProperty(ref CLOSING_INVENTORYValue, value); }
        }

        // Standard PPDM columns

     

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

      
    }
}
