using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class PROVED_RESERVES : ModelEntityBase
    {
        private System.String RESERVES_IDValue;
        public System.String RESERVES_ID
        {
            get { return this.RESERVES_IDValue; }
            set { SetProperty(ref RESERVES_IDValue, value); }
        }

        private System.String PROPERTY_IDValue;
        public System.String PROPERTY_ID
        {
            get { return this.PROPERTY_IDValue; }
            set { SetProperty(ref PROPERTY_IDValue, value); }
        }

        private System.Decimal? PROVED_DEVELOPED_OIL_RESERVESValue;
        public System.Decimal? PROVED_DEVELOPED_OIL_RESERVES
        {
            get { return this.PROVED_DEVELOPED_OIL_RESERVESValue; }
            set { SetProperty(ref PROVED_DEVELOPED_OIL_RESERVESValue, value); }
        }

        private System.Decimal? PROVED_UNDEVELOPED_OIL_RESERVESValue;
        public System.Decimal? PROVED_UNDEVELOPED_OIL_RESERVES
        {
            get { return this.PROVED_UNDEVELOPED_OIL_RESERVESValue; }
            set { SetProperty(ref PROVED_UNDEVELOPED_OIL_RESERVESValue, value); }
        }

        private System.Decimal? PROVED_DEVELOPED_GAS_RESERVESValue;
        public System.Decimal? PROVED_DEVELOPED_GAS_RESERVES
        {
            get { return this.PROVED_DEVELOPED_GAS_RESERVESValue; }
            set { SetProperty(ref PROVED_DEVELOPED_GAS_RESERVESValue, value); }
        }

        private System.Decimal? PROVED_UNDEVELOPED_GAS_RESERVESValue;
        public System.Decimal? PROVED_UNDEVELOPED_GAS_RESERVES
        {
            get { return this.PROVED_UNDEVELOPED_GAS_RESERVESValue; }
            set { SetProperty(ref PROVED_UNDEVELOPED_GAS_RESERVESValue, value); }
        }

        private System.DateTime? RESERVE_DATEValue;
        public System.DateTime? RESERVE_DATE
        {
            get { return this.RESERVE_DATEValue; }
            set { SetProperty(ref RESERVE_DATEValue, value); }
        }

        private System.Decimal? OIL_PRICEValue;
        public System.Decimal? OIL_PRICE
        {
            get { return this.OIL_PRICEValue; }
            set { SetProperty(ref OIL_PRICEValue, value); }
        }

        private System.Decimal? GAS_PRICEValue;
        public System.Decimal? GAS_PRICE
        {
            get { return this.GAS_PRICEValue; }
            set { SetProperty(ref GAS_PRICEValue, value); }
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

        private System.DateTime? EFFECTIVE_DATEValue;

        private System.DateTime? EXPIRY_DATEValue;

    }
}
