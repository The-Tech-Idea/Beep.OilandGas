using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class AMORTIZATION_SPLIT : ModelEntityBase {
        private string AMORTIZATION_SPLIT_IDValue;
        public string AMORTIZATION_SPLIT_ID
        {
            get { return this.AMORTIZATION_SPLIT_IDValue; }
            set { SetProperty(ref AMORTIZATION_SPLIT_IDValue, value); }
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

        private decimal? OIL_DEPLETIONValue;
        public decimal? OIL_DEPLETION
        {
            get { return this.OIL_DEPLETIONValue; }
            set { SetProperty(ref OIL_DEPLETIONValue, value); }
        }

        private decimal? GAS_DEPLETIONValue;
        public decimal? GAS_DEPLETION
        {
            get { return this.GAS_DEPLETIONValue; }
            set { SetProperty(ref GAS_DEPLETIONValue, value); }
        }

        private decimal? WORKING_INTEREST_DEPLETIONValue;
        public decimal? WORKING_INTEREST_DEPLETION
        {
            get { return this.WORKING_INTEREST_DEPLETIONValue; }
            set { SetProperty(ref WORKING_INTEREST_DEPLETIONValue, value); }
        }

        private decimal? NON_WORKING_DEPLETIONValue;
        public decimal? NON_WORKING_DEPLETION
        {
            get { return this.NON_WORKING_DEPLETIONValue; }
            set { SetProperty(ref NON_WORKING_DEPLETIONValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}
