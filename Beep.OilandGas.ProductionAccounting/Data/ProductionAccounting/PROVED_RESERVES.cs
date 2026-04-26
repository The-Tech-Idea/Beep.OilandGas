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

        // ── Best-practice additions (SEC Reg S-X Rule 4-10 / FASB ASC 932) ──────────

        private System.String FIELD_IDValue;
        public System.String FIELD_ID
        {
            get { return this.FIELD_IDValue; }
            set { SetProperty(ref FIELD_IDValue, value); }
        }

        private System.Int32 FISCAL_YEARValue;
        public System.Int32 FISCAL_YEAR
        {
            get { return this.FISCAL_YEARValue; }
            set { SetProperty(ref FISCAL_YEARValue, value); }
        }

        // RESERVES_CATEGORY: 1P=PROVED / 2P=PROVED_PROBABLE / 3P=PROVED_PROBABLE_POSSIBLE
        private System.String RESERVES_CATEGORYValue;
        public System.String RESERVES_CATEGORY
        {
            get { return this.RESERVES_CATEGORYValue; }
            set { SetProperty(ref RESERVES_CATEGORYValue, value); }
        }

        // PROVED_DEVELOPED_PRODUCING_OIL (PDP sub-category — required for SEC roll-forward)
        private System.Decimal? PROVED_DEVELOPED_PRODUCING_OILValue;
        public System.Decimal? PROVED_DEVELOPED_PRODUCING_OIL
        {
            get { return this.PROVED_DEVELOPED_PRODUCING_OILValue; }
            set { SetProperty(ref PROVED_DEVELOPED_PRODUCING_OILValue, value); }
        }

        // PROVED_DEVELOPED_NONPRODUCING_OIL (PDNP — required for SEC roll-forward)
        private System.Decimal? PROVED_DEVELOPED_NONPRODUCING_OILValue;
        public System.Decimal? PROVED_DEVELOPED_NONPRODUCING_OIL
        {
            get { return this.PROVED_DEVELOPED_NONPRODUCING_OILValue; }
            set { SetProperty(ref PROVED_DEVELOPED_NONPRODUCING_OILValue, value); }
        }

        private System.String OIL_VOLUME_OUOMValue;
        public System.String OIL_VOLUME_OUOM
        {
            get { return this.OIL_VOLUME_OUOMValue; }
            set { SetProperty(ref OIL_VOLUME_OUOMValue, value); }
        }

        private System.String GAS_VOLUME_OUOMValue;
        public System.String GAS_VOLUME_OUOM
        {
            get { return this.GAS_VOLUME_OUOMValue; }
            set { SetProperty(ref GAS_VOLUME_OUOMValue, value); }
        }

        // PRICE_DECK_TYPE: SEC_12MO_AVG / NYMEX_STRIP / COMPANY_FORECAST (SEC Reg S-X)
        private System.String PRICE_DECK_TYPEValue;
        public System.String PRICE_DECK_TYPE
        {
            get { return this.PRICE_DECK_TYPEValue; }
            set { SetProperty(ref PRICE_DECK_TYPEValue, value); }
        }

        // ID of the certifying reserve engineer (SEC requires qualified person)
        private System.String ENGINEER_CERTIFICATION_IDValue;
        public System.String ENGINEER_CERTIFICATION_ID
        {
            get { return this.ENGINEER_CERTIFICATION_IDValue; }
            set { SetProperty(ref ENGINEER_CERTIFICATION_IDValue, value); }
        }

    }
}
