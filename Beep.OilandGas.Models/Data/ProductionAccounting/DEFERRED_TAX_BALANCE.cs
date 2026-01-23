using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class DEFERRED_TAX_BALANCE : ModelEntityBase {
        private string DEFERRED_TAX_BALANCE_IDValue;
        public string DEFERRED_TAX_BALANCE_ID
        {
            get { return this.DEFERRED_TAX_BALANCE_IDValue; }
            set { SetProperty(ref DEFERRED_TAX_BALANCE_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private DateTime? PERIOD_END_DATEValue;
        public DateTime? PERIOD_END_DATE
        {
            get { return this.PERIOD_END_DATEValue; }
            set { SetProperty(ref PERIOD_END_DATEValue, value); }
        }

        private decimal? DEFERRED_TAX_ASSETValue;
        public decimal? DEFERRED_TAX_ASSET
        {
            get { return this.DEFERRED_TAX_ASSETValue; }
            set { SetProperty(ref DEFERRED_TAX_ASSETValue, value); }
        }

        private decimal? DEFERRED_TAX_LIABILITYValue;
        public decimal? DEFERRED_TAX_LIABILITY
        {
            get { return this.DEFERRED_TAX_LIABILITYValue; }
            set { SetProperty(ref DEFERRED_TAX_LIABILITYValue, value); }
        }

        private string NOTESValue;
        public string NOTES
        {
            get { return this.NOTESValue; }
            set { SetProperty(ref NOTESValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


