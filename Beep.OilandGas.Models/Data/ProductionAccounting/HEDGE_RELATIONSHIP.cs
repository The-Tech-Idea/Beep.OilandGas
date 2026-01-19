using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class HEDGE_RELATIONSHIP : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
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

        private string ACTIVE_INDValue;
        public string ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private string PPDM_GUIDValue;
        public string PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }
    }
}
