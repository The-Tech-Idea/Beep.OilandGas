using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CRUDE_OIL_PROPERTIES : ModelEntityBase
    {
        private System.String PROPERTIES_IDValue;
        public System.String PROPERTIES_ID
        {
            get { return this.PROPERTIES_IDValue; }
            set { SetProperty(ref PROPERTIES_IDValue, value); }
        }

        private System.String INVENTORY_IDValue;
        public System.String INVENTORY_ID
        {
            get { return this.INVENTORY_IDValue; }
            set { SetProperty(ref INVENTORY_IDValue, value); }
        }

        private System.Decimal? API_GRAVITYValue;
        public System.Decimal? API_GRAVITY
        {
            get { return this.API_GRAVITYValue; }
            set { SetProperty(ref API_GRAVITYValue, value); }
        }

        private System.Decimal? SULFUR_CONTENTValue;
        public System.Decimal? SULFUR_CONTENT
        {
            get { return this.SULFUR_CONTENTValue; }
            set { SetProperty(ref SULFUR_CONTENTValue, value); }
        }

        private System.Decimal? VISCOSITYValue;
        public System.Decimal? VISCOSITY
        {
            get { return this.VISCOSITYValue; }
            set { SetProperty(ref VISCOSITYValue, value); }
        }

        private System.Decimal? POUR_POINTValue;
        public System.Decimal? POUR_POINT
        {
            get { return this.POUR_POINTValue; }
            set { SetProperty(ref POUR_POINTValue, value); }
        }

        private System.Decimal? FLASH_POINTValue;
        public System.Decimal? FLASH_POINT
        {
            get { return this.FLASH_POINTValue; }
            set { SetProperty(ref FLASH_POINTValue, value); }
        }

        private System.Decimal? WATER_CONTENTValue;
        public System.Decimal? WATER_CONTENT
        {
            get { return this.WATER_CONTENTValue; }
            set { SetProperty(ref WATER_CONTENTValue, value); }
        }

        private System.Decimal? BSWValue;
        public System.Decimal? BSW
        {
            get { return this.BSWValue; }
            set { SetProperty(ref BSWValue, value); }
        }

        private System.Decimal? SALT_CONTENTValue;
        public System.Decimal? SALT_CONTENT
        {
            get { return this.SALT_CONTENTValue; }
            set { SetProperty(ref SALT_CONTENTValue, value); }
        }

        private System.Decimal? REID_VAPOR_PRESSUREValue;
        public System.Decimal? REID_VAPOR_PRESSURE
        {
            get { return this.REID_VAPOR_PRESSUREValue; }
            set { SetProperty(ref REID_VAPOR_PRESSUREValue, value); }
        }

        private System.Decimal? DENSITYValue;
        public System.Decimal? DENSITY
        {
            get { return this.DENSITYValue; }
            set { SetProperty(ref DENSITYValue, value); }
        }

        private System.Decimal? MEASUREMENT_TEMPERATUREValue;
        public System.Decimal? MEASUREMENT_TEMPERATURE
        {
            get { return this.MEASUREMENT_TEMPERATUREValue; }
            set { SetProperty(ref MEASUREMENT_TEMPERATUREValue, value); }
        }

        private System.DateTime? MEASUREMENT_DATEValue;
        public System.DateTime? MEASUREMENT_DATE
        {
            get { return this.MEASUREMENT_DATEValue; }
            set { SetProperty(ref MEASUREMENT_DATEValue, value); }
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
