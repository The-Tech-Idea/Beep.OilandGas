using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Agreement
{
    public partial class STORAGE_AGREEMENT : ModelEntityBase
    {
        private System.String AGREEMENT_IDValue;
        public System.String AGREEMENT_ID
        {
            get { return this.AGREEMENT_IDValue; }
            set { SetProperty(ref AGREEMENT_IDValue, value); }
        }

        private System.String STORAGE_FACILITY_IDValue;
        public System.String STORAGE_FACILITY_ID
        {
            get { return this.STORAGE_FACILITY_IDValue; }
            set { SetProperty(ref STORAGE_FACILITY_IDValue, value); }
        }

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRATION_DATEValue;
        public System.DateTime? EXPIRATION_DATE
        {
            get { return this.EXPIRATION_DATEValue; }
            set { SetProperty(ref EXPIRATION_DATEValue, value); }
        }

        private System.Decimal  STORAGE_FEEValue;
        public System.Decimal  STORAGE_FEE
        {
            get { return this.STORAGE_FEEValue; }
            set { SetProperty(ref STORAGE_FEEValue, value); }
        }

        private System.Decimal  RESERVED_CAPACITYValue;
        public System.Decimal  RESERVED_CAPACITY
        {
            get { return this.RESERVED_CAPACITYValue; }
            set { SetProperty(ref RESERVED_CAPACITYValue, value); }
        }

        // Standard PPDM columns

        private System.String REMARKValue;

        private System.String SOURCEValue;

        // Additional IPPDMEntity properties
        private System.String AREA_IDValue;
        public System.String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private System.String AREA_TYPEValue;
        public System.String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private System.String BUSINESS_ASSOCIATE_IDValue;
        public System.String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

        private System.DateTime? EXPIRY_DATEValue;

    }
}
