using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CRUDE_OIL_SPECIFICATIONS : Entity, Beep.OilandGas.PPDM.Models.IPPDMEntity
    {
        private System.String SPECIFICATIONS_IDValue;
        public System.String SPECIFICATIONS_ID
        {
            get { return this.SPECIFICATIONS_IDValue; }
            set { SetProperty(ref SPECIFICATIONS_IDValue, value); }
        }

        private System.String AGREEMENT_IDValue;
        public System.String AGREEMENT_ID
        {
            get { return this.AGREEMENT_IDValue; }
            set { SetProperty(ref AGREEMENT_IDValue, value); }
        }

        private System.Decimal? MINIMUM_API_GRAVITYValue;
        public System.Decimal? MINIMUM_API_GRAVITY
        {
            get { return this.MINIMUM_API_GRAVITYValue; }
            set { SetProperty(ref MINIMUM_API_GRAVITYValue, value); }
        }

        private System.Decimal? MAXIMUM_API_GRAVITYValue;
        public System.Decimal? MAXIMUM_API_GRAVITY
        {
            get { return this.MAXIMUM_API_GRAVITYValue; }
            set { SetProperty(ref MAXIMUM_API_GRAVITYValue, value); }
        }

        private System.Decimal? MAXIMUM_SULFUR_CONTENTValue;
        public System.Decimal? MAXIMUM_SULFUR_CONTENT
        {
            get { return this.MAXIMUM_SULFUR_CONTENTValue; }
            set { SetProperty(ref MAXIMUM_SULFUR_CONTENTValue, value); }
        }

        private System.Decimal? MAXIMUM_BSWValue;
        public System.Decimal? MAXIMUM_BSW
        {
            get { return this.MAXIMUM_BSWValue; }
            set { SetProperty(ref MAXIMUM_BSWValue, value); }
        }

        private System.Decimal? MAXIMUM_WATER_CONTENTValue;
        public System.Decimal? MAXIMUM_WATER_CONTENT
        {
            get { return this.MAXIMUM_WATER_CONTENTValue; }
            set { SetProperty(ref MAXIMUM_WATER_CONTENTValue, value); }
        }

        private System.Decimal? MAXIMUM_SALT_CONTENTValue;
        public System.Decimal? MAXIMUM_SALT_CONTENT
        {
            get { return this.MAXIMUM_SALT_CONTENTValue; }
            set { SetProperty(ref MAXIMUM_SALT_CONTENTValue, value); }
        }

        private System.Decimal? MAXIMUM_VISCOSITYValue;
        public System.Decimal? MAXIMUM_VISCOSITY
        {
            get { return this.MAXIMUM_VISCOSITYValue; }
            set { SetProperty(ref MAXIMUM_VISCOSITYValue, value); }
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




