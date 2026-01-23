using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class HEDGE_MEASUREMENT : ModelEntityBase
    {
        private string HEDGE_MEASUREMENT_IDValue;
        public string HEDGE_MEASUREMENT_ID
        {
            get { return this.HEDGE_MEASUREMENT_IDValue; }
            set { SetProperty(ref HEDGE_MEASUREMENT_IDValue, value); }
        }

        private string HEDGE_RELATIONSHIP_IDValue;
        public string HEDGE_RELATIONSHIP_ID
        {
            get { return this.HEDGE_RELATIONSHIP_IDValue; }
            set { SetProperty(ref HEDGE_RELATIONSHIP_IDValue, value); }
        }

        private DateTime? MEASUREMENT_DATEValue;
        public DateTime? MEASUREMENT_DATE
        {
            get { return this.MEASUREMENT_DATEValue; }
            set { SetProperty(ref MEASUREMENT_DATEValue, value); }
        }

        private decimal? EFFECTIVE_PORTIONValue;
        public decimal? EFFECTIVE_PORTION
        {
            get { return this.EFFECTIVE_PORTIONValue; }
            set { SetProperty(ref EFFECTIVE_PORTIONValue, value); }
        }

        private decimal? INEFFECTIVE_PORTIONValue;
        public decimal? INEFFECTIVE_PORTION
        {
            get { return this.INEFFECTIVE_PORTIONValue; }
            set { SetProperty(ref INEFFECTIVE_PORTIONValue, value); }
        }

        private decimal? TOTAL_CHANGEValue;
        public decimal? TOTAL_CHANGE
        {
            get { return this.TOTAL_CHANGEValue; }
            set { SetProperty(ref TOTAL_CHANGEValue, value); }
        }
    }
}


