using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class ASSET_SWAP_TRANSACTION : ModelEntityBase
    {
        private string ASSET_SWAP_IDValue;
        public string ASSET_SWAP_ID
        {
            get { return this.ASSET_SWAP_IDValue; }
            set { SetProperty(ref ASSET_SWAP_IDValue, value); }
        }

        private string PROPERTY_IDValue;
        public string PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private string COUNTERPARTY_BA_IDValue;
        public string COUNTERPARTY_BA_ID
        {
            get { return this.COUNTERPARTY_BA_IDValue; }
            set { SetProperty(ref COUNTERPARTY_BA_IDValue, value); }
        }

        private DateTime? SWAP_DATEValue;
        public DateTime? SWAP_DATE
        {
            get { return this.SWAP_DATEValue; }
            set { SetProperty(ref SWAP_DATEValue, value); }
        }

        private string ASSET_GIVEN_DESCValue;
        public string ASSET_GIVEN_DESC
        {
            get { return this.ASSET_GIVEN_DESCValue; }
            set { SetProperty(ref ASSET_GIVEN_DESCValue, value); }
        }

        private string ASSET_RECEIVED_DESCValue;
        public string ASSET_RECEIVED_DESC
        {
            get { return this.ASSET_RECEIVED_DESCValue; }
            set { SetProperty(ref ASSET_RECEIVED_DESCValue, value); }
        }

        private decimal? FAIR_VALUE_GIVENValue;
        public decimal? FAIR_VALUE_GIVEN
        {
            get { return this.FAIR_VALUE_GIVENValue; }
            set { SetProperty(ref FAIR_VALUE_GIVENValue, value); }
        }

        private decimal? FAIR_VALUE_RECEIVEDValue;
        public decimal? FAIR_VALUE_RECEIVED
        {
            get { return this.FAIR_VALUE_RECEIVEDValue; }
            set { SetProperty(ref FAIR_VALUE_RECEIVEDValue, value); }
        }

        private decimal? GAIN_LOSSValue;
        public decimal? GAIN_LOSS
        {
            get { return this.GAIN_LOSSValue; }
            set { SetProperty(ref GAIN_LOSSValue, value); }
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

        private string REMARKValue;
        public string REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private string SOURCEValue;
        public string SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
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

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}

