using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class HEDGE_RELATIONSHIP : ModelEntityBase
    {
        private string HEDGE_RELATIONSHIP_IDValue;
        public string HEDGE_RELATIONSHIP_ID
        {
            get { return this.HEDGE_RELATIONSHIP_IDValue; }
            set { SetProperty(ref HEDGE_RELATIONSHIP_IDValue, value); }
        }

        private string HEDGE_TYPEValue;
        public string HEDGE_TYPE
        {
            get { return this.HEDGE_TYPEValue; }
            set { SetProperty(ref HEDGE_TYPEValue, value); }
        }

        private string INSTRUMENT_IDValue;
        public string INSTRUMENT_ID
        {
            get { return this.INSTRUMENT_IDValue; }
            set { SetProperty(ref INSTRUMENT_IDValue, value); }
        }

        private string HEDGED_ITEM_IDValue;
        public string HEDGED_ITEM_ID
        {
            get { return this.HEDGED_ITEM_IDValue; }
            set { SetProperty(ref HEDGED_ITEM_IDValue, value); }
        }

        private decimal? HEDGE_RATIOValue;
        public decimal? HEDGE_RATIO
        {
            get { return this.HEDGE_RATIOValue; }
            set { SetProperty(ref HEDGE_RATIOValue, value); }
        }

        private string EFFECTIVENESS_METHODValue;
        public string EFFECTIVENESS_METHOD
        {
            get { return this.EFFECTIVENESS_METHODValue; }
            set { SetProperty(ref EFFECTIVENESS_METHODValue, value); }
        }
    }
}


