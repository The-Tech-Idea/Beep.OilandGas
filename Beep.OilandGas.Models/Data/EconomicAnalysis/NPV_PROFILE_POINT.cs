using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.EconomicAnalysis
{
    public partial class NPV_PROFILE_POINT : ModelEntityBase {
        private String NPV_PROFILE_POINT_IDValue;
        public String NPV_PROFILE_POINT_ID
        {
            get { return this.NPV_PROFILE_POINT_IDValue; }
            set { SetProperty(ref NPV_PROFILE_POINT_IDValue, value); }
        }

        private String ECONOMIC_ANALYSIS_RESULT_IDValue;
        public String ECONOMIC_ANALYSIS_RESULT_ID
        {
            get { return this.ECONOMIC_ANALYSIS_RESULT_IDValue; }
            set { SetProperty(ref ECONOMIC_ANALYSIS_RESULT_IDValue, value); }
        }

        private Decimal? RATEValue;
        public Decimal? RATE
        {
            get { return this.RATEValue; }
            set { SetProperty(ref RATEValue, value); }
        }

        private Decimal? NPVValue;
        public Decimal? NPV
        {
            get { return this.NPVValue; }
            set { SetProperty(ref NPVValue, value); }
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


        private double DISCOUNT_RATEValue;

        public NPV_PROFILE_POINT(double rate, double npv)
        {
            RATE = (decimal?)rate;
            NPV = (decimal?)npv;
        }

        public double DISCOUNT_RATE
        {
            get { return this.DISCOUNT_RATEValue; }
            set { SetProperty(ref DISCOUNT_RATEValue, value); }
        }
    }
}
