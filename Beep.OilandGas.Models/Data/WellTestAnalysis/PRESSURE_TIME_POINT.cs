using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public partial class PRESSURE_TIME_POINT : ModelEntityBase {
        private String PRESSURE_TIME_POINT_IDValue;
        public String PRESSURE_TIME_POINT_ID
        {
            get { return this.PRESSURE_TIME_POINT_IDValue; }
            set { SetProperty(ref PRESSURE_TIME_POINT_IDValue, value); }
        }

        private String WELL_TEST_DATA_IDValue;
        public String WELL_TEST_DATA_ID
        {
            get { return this.WELL_TEST_DATA_IDValue; }
            set { SetProperty(ref WELL_TEST_DATA_IDValue, value); }
        }

        private Decimal? TIMEValue;
        public Decimal? TIME
        {
            get { return this.TIMEValue; }
            set { SetProperty(ref TIMEValue, value); }
        }

        private Decimal? PRESSUREValue;
        public Decimal? PRESSURE
        {
            get { return this.PRESSUREValue; }
            set { SetProperty(ref PRESSUREValue, value); }
        }

        private Decimal? PRESSURE_DERIVATIVEValue;
        public Decimal? PRESSURE_DERIVATIVE
        {
            get { return this.PRESSURE_DERIVATIVEValue; }
            set { SetProperty(ref PRESSURE_DERIVATIVEValue, value); }
        }

        private Int32? POINT_ORDERValue;
        public Int32? POINT_ORDER
        {
            get { return this.POINT_ORDERValue; }
            set { SetProperty(ref POINT_ORDERValue, value); }
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
