using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.ProductionAccounting
{
    public partial class CRUDE_OIL_SPECIFICATIONS : ModelEntityBase
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


