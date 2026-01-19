using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EMISSIONS_SETTLEMENT : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private string EMISSIONS_SETTLEMENT_IDValue;
        public string EMISSIONS_SETTLEMENT_ID
        {
            get { return this.EMISSIONS_SETTLEMENT_IDValue; }
            set { SetProperty(ref EMISSIONS_SETTLEMENT_IDValue, value); }
        }

        private string EMISSIONS_OBLIGATION_IDValue;
        public string EMISSIONS_OBLIGATION_ID
        {
            get { return this.EMISSIONS_OBLIGATION_IDValue; }
            set { SetProperty(ref EMISSIONS_OBLIGATION_IDValue, value); }
        }

        private DateTime? SETTLEMENT_DATEValue;
        public DateTime? SETTLEMENT_DATE
        {
            get { return this.SETTLEMENT_DATEValue; }
            set { SetProperty(ref SETTLEMENT_DATEValue, value); }
        }

        private decimal? ALLOWANCES_SURRENDEREDValue;
        public decimal? ALLOWANCES_SURRENDERED
        {
            get { return this.ALLOWANCES_SURRENDEREDValue; }
            set { SetProperty(ref ALLOWANCES_SURRENDEREDValue, value); }
        }

        private decimal? SETTLEMENT_VALUEValue;
        public decimal? SETTLEMENT_VALUE
        {
            get { return this.SETTLEMENT_VALUEValue; }
            set { SetProperty(ref SETTLEMENT_VALUEValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
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
