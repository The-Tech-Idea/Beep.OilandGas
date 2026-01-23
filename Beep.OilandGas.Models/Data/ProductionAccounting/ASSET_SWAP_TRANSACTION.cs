using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
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

        private string ROW_IDValue;
        public string ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


