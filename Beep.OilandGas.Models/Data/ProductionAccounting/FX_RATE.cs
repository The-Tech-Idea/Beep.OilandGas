using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class FX_RATE : ModelEntityBase
    {
        private string FX_RATE_IDValue;
        public string FX_RATE_ID
        {
            get { return this.FX_RATE_IDValue; }
            set { SetProperty(ref FX_RATE_IDValue, value); }
        }

        private string FROM_CURRENCYValue;
        public string FROM_CURRENCY
        {
            get { return this.FROM_CURRENCYValue; }
            set { SetProperty(ref FROM_CURRENCYValue, value); }
        }

        private string TO_CURRENCYValue;
        public string TO_CURRENCY
        {
            get { return this.TO_CURRENCYValue; }
            set { SetProperty(ref TO_CURRENCYValue, value); }
        }

        private DateTime? RATE_DATEValue;
        public DateTime? RATE_DATE
        {
            get { return this.RATE_DATEValue; }
            set { SetProperty(ref RATE_DATEValue, value); }
        }

        private decimal? RATEValue;
        public decimal? RATE
        {
            get { return this.RATEValue; }
            set { SetProperty(ref RATEValue, value); }
        }

        private string SOURCEValue;
        public string SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
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

