using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public partial class TRANSPORTATION_AGREEMENT : ModelEntityBase
    {
        private System.String AGREEMENT_IDValue;
        public System.String AGREEMENT_ID
        {
            get { return this.AGREEMENT_IDValue; }
            set { SetProperty(ref AGREEMENT_IDValue, value); }
        }

        private System.String CARRIER_BA_IDValue;
        public System.String CARRIER_BA_ID
        {
            get { return this.CARRIER_BA_IDValue; }
            set { SetProperty(ref CARRIER_BA_IDValue, value); }
        }

        private System.String ORIGIN_POINTValue;
        public System.String ORIGIN_POINT
        {
            get { return this.ORIGIN_POINTValue; }
            set { SetProperty(ref ORIGIN_POINTValue, value); }
        }

        private System.String DESTINATION_POINTValue;
        public System.String DESTINATION_POINT
        {
            get { return this.DESTINATION_POINTValue; }
            set { SetProperty(ref DESTINATION_POINTValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.Decimal  TARIFF_RATEValue;
        public System.Decimal  TARIFF_RATE
        {
            get { return this.TARIFF_RATEValue; }
            set { SetProperty(ref TARIFF_RATEValue, value); }
        }

        private System.Decimal  MINIMUM_VOLUME_COMMITMENTValue;
        public System.Decimal  MINIMUM_VOLUME_COMMITMENT
        {
            get { return this.MINIMUM_VOLUME_COMMITMENTValue; }
            set { SetProperty(ref MINIMUM_VOLUME_COMMITMENTValue, value); }
        }

        private System.Decimal  MAXIMUM_CAPACITYValue;
        public System.Decimal  MAXIMUM_CAPACITY
        {
            get { return this.MAXIMUM_CAPACITYValue; }
            set { SetProperty(ref MAXIMUM_CAPACITYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

    }
}
