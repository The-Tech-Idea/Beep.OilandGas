using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

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

