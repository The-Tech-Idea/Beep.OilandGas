using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PRODUCTION_SHARING_ENTITLEMENT : ModelEntityBase
    {
        private string PSA_ENTITLEMENT_IDValue;
        public string PSA_ENTITLEMENT_ID
        {
            get { return this.PSA_ENTITLEMENT_IDValue; }
            set { SetProperty(ref PSA_ENTITLEMENT_IDValue, value); }
        }

        private string PSA_IDValue;
        public string PSA_ID
        {
            get { return this.PSA_IDValue; }
            set { SetProperty(ref PSA_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private string ALLOCATION_DETAIL_IDValue;
        public string ALLOCATION_DETAIL_ID
        {
            get { return this.ALLOCATION_DETAIL_IDValue; }
            set { SetProperty(ref ALLOCATION_DETAIL_IDValue, value); }
        }

        private DateTime? PRODUCTION_DATEValue;
        public DateTime? PRODUCTION_DATE
        {
            get { return this.PRODUCTION_DATEValue; }
            set { SetProperty(ref PRODUCTION_DATEValue, value); }
        }

        private decimal? TOTAL_VOLUMEValue;
        public decimal? TOTAL_VOLUME
        {
            get { return this.TOTAL_VOLUMEValue; }
            set { SetProperty(ref TOTAL_VOLUMEValue, value); }
        }

        private decimal? COST_OIL_VOLUMEValue;
        public decimal? COST_OIL_VOLUME
        {
            get { return this.COST_OIL_VOLUMEValue; }
            set { SetProperty(ref COST_OIL_VOLUMEValue, value); }
        }

        private decimal? PROFIT_OIL_VOLUMEValue;
        public decimal? PROFIT_OIL_VOLUME
        {
            get { return this.PROFIT_OIL_VOLUMEValue; }
            set { SetProperty(ref PROFIT_OIL_VOLUMEValue, value); }
        }

        private decimal? CONTRACTOR_VOLUMEValue;
        public decimal? CONTRACTOR_VOLUME
        {
            get { return this.CONTRACTOR_VOLUMEValue; }
            set { SetProperty(ref CONTRACTOR_VOLUMEValue, value); }
        }

        private decimal? GOVERNMENT_VOLUMEValue;
        public decimal? GOVERNMENT_VOLUME
        {
            get { return this.GOVERNMENT_VOLUMEValue; }
            set { SetProperty(ref GOVERNMENT_VOLUMEValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


