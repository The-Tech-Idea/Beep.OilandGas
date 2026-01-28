using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PRODUCTION_SHARING_AGREEMENT : ModelEntityBase
    {
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

        private decimal? COST_RECOVERY_LIMIT_PCTValue;
        public decimal? COST_RECOVERY_LIMIT_PCT
        {
            get { return this.COST_RECOVERY_LIMIT_PCTValue; }
            set { SetProperty(ref COST_RECOVERY_LIMIT_PCTValue, value); }
        }

        private decimal? GOVERNMENT_PROFIT_SPLIT_PCTValue;
        public decimal? GOVERNMENT_PROFIT_SPLIT_PCT
        {
            get { return this.GOVERNMENT_PROFIT_SPLIT_PCTValue; }
            set { SetProperty(ref GOVERNMENT_PROFIT_SPLIT_PCTValue, value); }
        }

        private decimal? CONTRACTOR_PROFIT_SPLIT_PCTValue;
        public decimal? CONTRACTOR_PROFIT_SPLIT_PCT
        {
            get { return this.CONTRACTOR_PROFIT_SPLIT_PCTValue; }
            set { SetProperty(ref CONTRACTOR_PROFIT_SPLIT_PCTValue, value); }
        }

        private decimal? TAX_RATEValue;
        public decimal? TAX_RATE
        {
            get { return this.TAX_RATEValue; }
            set { SetProperty(ref TAX_RATEValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
