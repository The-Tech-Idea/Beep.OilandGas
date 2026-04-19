using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PRODUCTION_AVAILS : ModelEntityBase {
        private System.String PRODUCTION_AVAILS_IDValue;
        public System.String PRODUCTION_AVAILS_ID
        {
            get { return this.PRODUCTION_AVAILS_IDValue; }
            set { SetProperty(ref PRODUCTION_AVAILS_IDValue, value); }
        }

        private System.DateTime? PERIOD_STARTValue;
        public System.DateTime? PERIOD_START
        {
            get { return this.PERIOD_STARTValue; }
            set { SetProperty(ref PERIOD_STARTValue, value); }
        }

        private System.DateTime? PERIOD_ENDValue;
        public System.DateTime? PERIOD_END
        {
            get { return this.PERIOD_ENDValue; }
            set { SetProperty(ref PERIOD_ENDValue, value); }
        }

        private System.Decimal? ESTIMATED_PRODUCTIONValue;
        public System.Decimal? ESTIMATED_PRODUCTION
        {
            get { return this.ESTIMATED_PRODUCTIONValue; }
            set { SetProperty(ref ESTIMATED_PRODUCTIONValue, value); }
        }

        private System.Decimal? AVAILABLE_FOR_DELIVERYValue;
        public System.Decimal? AVAILABLE_FOR_DELIVERY
        {
            get { return this.AVAILABLE_FOR_DELIVERYValue; }
            set { SetProperty(ref AVAILABLE_FOR_DELIVERYValue, value); }
        }

       

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }

        private string AVAILS_IDValue;
        public string AVAILS_ID
        {
            get { return this.AVAILS_IDValue; }
            set { SetProperty(ref AVAILS_IDValue, value); }
        }

      
    }
}
