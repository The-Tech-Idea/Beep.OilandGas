using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CRUDE_OIL_PROPERTIES : Entity, Beep.OilandGas.Models.Core.Interfaces.IPPDMEntity
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

        private System.String ROW_QUALITYValue;
        public System.String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
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

        private System.DateTime? EFFECTIVE_DATEValue;
        public System.DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private System.DateTime? EXPIRY_DATEValue;
        public System.DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}
