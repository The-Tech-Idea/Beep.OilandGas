using System;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data
{
    public partial class PROSPECT : Entity
    {
        private string PROSPECT_IDValue;
        public string PROSPECT_ID
        {
            get { return this.PROSPECT_IDValue; }
            set { SetProperty(ref PROSPECT_IDValue, value); }
        }

        private string PROSPECT_NAMEValue;
        public string PROSPECT_NAME
        {
            get { return this.PROSPECT_NAMEValue; }
            set { SetProperty(ref PROSPECT_NAMEValue, value); }
        }

        private string FIELD_IDValue;
        public string FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private DateTime? EVALUATION_DATEValue;
        public DateTime? EVALUATION_DATE
        {
            get { return this.EVALUATION_DATEValue; }
            set { SetProperty(ref EVALUATION_DATEValue, value); }
        }

        private decimal? ESTIMATED_RESERVESValue;
        public decimal? ESTIMATED_RESERVES
        {
            get { return this.ESTIMATED_RESERVESValue; }
            set { SetProperty(ref ESTIMATED_RESERVESValue, value); }
        }

        private decimal? RISK_FACTORValue;
        public decimal? RISK_FACTOR
        {
            get { return this.RISK_FACTORValue; }
            set { SetProperty(ref RISK_FACTORValue, value); }
        }

        private string STATUSValue;
        public string STATUS
        {
            get { return this.STATUSValue; }
            set { SetProperty(ref STATUSValue, value); }
        }

        // Standard PPDM columns
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

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private string ROW_CREATED_BYValue;
        public string ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private string ROW_CHANGED_BYValue;
        public string ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }
    }
}

