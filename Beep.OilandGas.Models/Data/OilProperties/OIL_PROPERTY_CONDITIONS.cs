using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

using Beep.OilandGas.Models.Data;
namespace Beep.OilandGas.Models.Data.OilProperties
{
    public partial class OIL_PROPERTY_CONDITIONS : ModelEntityBase {
        private String OIL_PROPERTY_CONDITIONS_IDValue;
        public String OIL_PROPERTY_CONDITIONS_ID
        {
            get { return this.OIL_PROPERTY_CONDITIONS_IDValue; }
            set { SetProperty(ref OIL_PROPERTY_CONDITIONS_IDValue, value); }
        }

        private Decimal? PRESSUREValue;
        public Decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private Decimal? TEMPERATUREValue;
        public Decimal? TEMPERATURE
        {
            get { return this.TEMPERATUREValue; }
            set { SetProperty(ref TEMPERATUREValue, value); }
        }

        private Decimal? API_GRAVITYValue;
        public Decimal? API_GRAVITY
        {
            get { return this.API_GRAVITYValue; }
            set { SetProperty(ref API_GRAVITYValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
        }

        private Decimal? SOLUTION_GAS_OIL_RATIOValue;
        public Decimal? SOLUTION_GAS_OIL_RATIO
        {
            get { return this.SOLUTION_GAS_OIL_RATIOValue; }
            set { SetProperty(ref SOLUTION_GAS_OIL_RATIOValue, value); }
        }

        private Decimal? BUBBLE_POINT_PRESSUREValue;
        public Decimal? BUBBLE_POINT_PRESSURE
        {
            get { return this.BUBBLE_POINT_PRESSUREValue; }
            set { SetProperty(ref BUBBLE_POINT_PRESSUREValue, value); }
        }

        // Standard PPDM columns

        // Optional PPDM properties
        private String AREA_IDValue;
        public String AREA_ID
        {
            get { return this.AREA_IDValue; }
            set { SetProperty(ref AREA_IDValue, value); }
        }

        private String AREA_TYPEValue;
        public String AREA_TYPE
        {
            get { return this.AREA_TYPEValue; }
            set { SetProperty(ref AREA_TYPEValue, value); }
        }

        private String BUSINESS_ASSOCIATE_IDValue;
        public String BUSINESS_ASSOCIATE_ID
        {
            get { return this.BUSINESS_ASSOCIATE_IDValue; }
            set { SetProperty(ref BUSINESS_ASSOCIATE_IDValue, value); }
        }

    }
}


