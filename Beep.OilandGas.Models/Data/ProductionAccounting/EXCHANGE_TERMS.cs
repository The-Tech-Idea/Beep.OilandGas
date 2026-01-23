using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.PPDM.Models;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class EXCHANGE_TERMS : ModelEntityBase {
        private System.String EXCHANGE_TERMS_IDValue;
        public System.String EXCHANGE_TERMS_ID
        {
            get { return this.EXCHANGE_TERMS_IDValue; }
            set { SetProperty(ref EXCHANGE_TERMS_IDValue, value); }
        }

        private System.String EXCHANGE_CONTRACT_IDValue;
        public System.String EXCHANGE_CONTRACT_ID
        {
            get { return this.EXCHANGE_CONTRACT_IDValue; }
            set { SetProperty(ref EXCHANGE_CONTRACT_IDValue, value); }
        }

        private System.Decimal? EXCHANGE_RATIOValue;
        public System.Decimal? EXCHANGE_RATIO
        {
            get { return this.EXCHANGE_RATIOValue; }
            set { SetProperty(ref EXCHANGE_RATIOValue, value); }
        }

        private System.Decimal? MINIMUM_VOLUMEValue;
        public System.Decimal? MINIMUM_VOLUME
        {
            get { return this.MINIMUM_VOLUMEValue; }
            set { SetProperty(ref MINIMUM_VOLUMEValue, value); }
        }

        private System.Decimal? MAXIMUM_VOLUMEValue;
        public System.Decimal? MAXIMUM_VOLUME
        {
            get { return this.MAXIMUM_VOLUMEValue; }
            set { SetProperty(ref MAXIMUM_VOLUMEValue, value); }
        }

        private System.String QUALITY_SPECIFICATIONSValue;
        public System.String QUALITY_SPECIFICATIONS
        {
            get { return this.QUALITY_SPECIFICATIONSValue; }
            set { SetProperty(ref QUALITY_SPECIFICATIONSValue, value); }
        }

        private System.Int32? NOTICE_PERIOD_DAYSValue;
        public System.Int32? NOTICE_PERIOD_DAYS
        {
            get { return this.NOTICE_PERIOD_DAYSValue; }
            set { SetProperty(ref NOTICE_PERIOD_DAYSValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        private System.String ROW_IDValue;
        public System.String ROW_ID
        {
            get { return this.ROW_IDValue; }
            set { SetProperty(ref ROW_IDValue, value); }
        }
    }
}


