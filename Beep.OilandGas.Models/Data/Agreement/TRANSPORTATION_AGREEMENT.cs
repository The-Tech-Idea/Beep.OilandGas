using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public partial class TRANSPORTATION_AGREEMENT : Entity
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
        public System.DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.Decimal? TARIFF_RATEValue;
        public System.Decimal? TARIFF_RATE
        {
            get { return this.TARIFF_RATEValue; }
            set { SetProperty(ref TARIFF_RATEValue, value); }
        }

        private System.Decimal? MINIMUM_VOLUME_COMMITMENTValue;
        public System.Decimal? MINIMUM_VOLUME_COMMITMENT
        {
            get { return this.MINIMUM_VOLUME_COMMITMENTValue; }
            set { SetProperty(ref MINIMUM_VOLUME_COMMITMENTValue, value); }
        }

        private System.Decimal? MAXIMUM_CAPACITYValue;
        public System.Decimal? MAXIMUM_CAPACITY
        {
            get { return this.MAXIMUM_CAPACITYValue; }
            set { SetProperty(ref MAXIMUM_CAPACITYValue, value); }
        }

        // Standard PPDM columns
        private System.String ACTIVE_INDValue;
        public System.String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private System.String PPDM_GUIDValue;
        public System.String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private System.String REMARKValue;
        public System.String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private System.String SOURCEValue;
        public System.String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private System.DateTime? ROW_CREATED_DATEValue;
        public System.DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private System.String ROW_CREATED_BYValue;
        public System.String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private System.DateTime? ROW_CHANGED_DATEValue;
        public System.DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private System.String ROW_CHANGED_BYValue;
        public System.String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private System.DateTime? ROW_EFFECTIVE_DATEValue;
        public System.DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? ROW_EXPIRY_DATEValue;
        public System.DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }
    }
}




