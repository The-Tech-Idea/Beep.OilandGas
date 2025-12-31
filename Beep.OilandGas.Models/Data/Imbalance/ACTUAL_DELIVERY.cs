using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.Imbalance
{
    public partial class ACTUAL_DELIVERY : Entity
    {
        private System.String ACTUAL_DELIVERY_IDValue;
        public System.String ACTUAL_DELIVERY_ID
        {
            get { return this.ACTUAL_DELIVERY_IDValue; }
            set { SetProperty(ref ACTUAL_DELIVERY_IDValue, value); }
        }

        private System.String NOMINATION_IDValue;
        public System.String NOMINATION_ID
        {
            get { return this.NOMINATION_IDValue; }
            set { SetProperty(ref NOMINATION_IDValue, value); }
        }

        private System.DateTime? DELIVERY_DATEValue;
        public System.DateTime? DELIVERY_DATE
        {
            get { return this.DELIVERY_DATEValue; }
            set { SetProperty(ref DELIVERY_DATEValue, value); }
        }

        private System.Decimal? ACTUAL_VOLUMEValue;
        public System.Decimal? ACTUAL_VOLUME
        {
            get { return this.ACTUAL_VOLUMEValue; }
            set { SetProperty(ref ACTUAL_VOLUMEValue, value); }
        }

        private System.String DELIVERY_POINTValue;
        public System.String DELIVERY_POINT
        {
            get { return this.DELIVERY_POINTValue; }
            set { SetProperty(ref DELIVERY_POINTValue, value); }
        }

        private System.String ALLOCATION_METHODValue;
        public System.String ALLOCATION_METHOD
        {
            get { return this.ALLOCATION_METHODValue; }
            set { SetProperty(ref ALLOCATION_METHODValue, value); }
        }

        private System.String RUN_TICKET_NUMBERValue;
        public System.String RUN_TICKET_NUMBER
        {
            get { return this.RUN_TICKET_NUMBERValue; }
            set { SetProperty(ref RUN_TICKET_NUMBERValue, value); }
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

