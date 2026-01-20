using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_PERFORMANCE_POINT : ModelEntityBase {
        private String GAS_LIFT_PERFORMANCE_POINT_IDValue;
        public String GAS_LIFT_PERFORMANCE_POINT_ID
        {
            get { return this.GAS_LIFT_PERFORMANCE_POINT_IDValue; }
            set { SetProperty(ref GAS_LIFT_PERFORMANCE_POINT_IDValue, value); }
        }

        private String GAS_LIFT_POTENTIAL_RESULT_IDValue;
        public String GAS_LIFT_POTENTIAL_RESULT_ID
        {
            get { return this.GAS_LIFT_POTENTIAL_RESULT_IDValue; }
            set { SetProperty(ref GAS_LIFT_POTENTIAL_RESULT_IDValue, value); }
        }

        private Decimal? GAS_INJECTION_RATEValue;
        public Decimal? GAS_INJECTION_RATE
        {
            get { return this.GAS_INJECTION_RATEValue; }
            set { SetProperty(ref GAS_INJECTION_RATEValue, value); }
        }

        private Decimal? PRODUCTION_RATEValue;
        public Decimal? PRODUCTION_RATE
        {
            get { return this.PRODUCTION_RATEValue; }
            set { SetProperty(ref PRODUCTION_RATEValue, value); }
        }

        private Decimal? GAS_LIQUID_RATIOValue;
        public Decimal? GAS_LIQUID_RATIO
        {
            get { return this.GAS_LIQUID_RATIOValue; }
            set { SetProperty(ref GAS_LIQUID_RATIOValue, value); }
        }

        private Decimal? BOTTOM_HOLE_PRESSUREValue;
        public Decimal? BOTTOM_HOLE_PRESSURE
        {
            get { return this.BOTTOM_HOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_PRESSUREValue, value); }
        }

        private Int32? POINT_ORDERValue;
        public Int32? POINT_ORDER
        {
            get { return this.POINT_ORDERValue; }
            set { SetProperty(ref POINT_ORDERValue, value); }
        }

        // Standard PPDM columns
        private String ACTIVE_INDValue;
        public String ACTIVE_IND
        {
            get { return this.ACTIVE_INDValue; }
            set { SetProperty(ref ACTIVE_INDValue, value); }
        }

        private String PPDM_GUIDValue;
        public String PPDM_GUID
        {
            get { return this.PPDM_GUIDValue; }
            set { SetProperty(ref PPDM_GUIDValue, value); }
        }

        private String REMARKValue;
        public String REMARK
        {
            get { return this.REMARKValue; }
            set { SetProperty(ref REMARKValue, value); }
        }

        private String SOURCEValue;
        public String SOURCE
        {
            get { return this.SOURCEValue; }
            set { SetProperty(ref SOURCEValue, value); }
        }

        private DateTime? ROW_CREATED_DATEValue;
        public DateTime? ROW_CREATED_DATE
        {
            get { return this.ROW_CREATED_DATEValue; }
            set { SetProperty(ref ROW_CREATED_DATEValue, value); }
        }

        private String ROW_CREATED_BYValue;
        public String ROW_CREATED_BY
        {
            get { return this.ROW_CREATED_BYValue; }
            set { SetProperty(ref ROW_CREATED_BYValue, value); }
        }

        private DateTime? ROW_CHANGED_DATEValue;
        public DateTime? ROW_CHANGED_DATE
        {
            get { return this.ROW_CHANGED_DATEValue; }
            set { SetProperty(ref ROW_CHANGED_DATEValue, value); }
        }

        private String ROW_CHANGED_BYValue;
        public String ROW_CHANGED_BY
        {
            get { return this.ROW_CHANGED_BYValue; }
            set { SetProperty(ref ROW_CHANGED_BYValue, value); }
        }

        private DateTime? ROW_EFFECTIVE_DATEValue;
        public DateTime? ROW_EFFECTIVE_DATE
        {
            get { return this.ROW_EFFECTIVE_DATEValue; }
            set { SetProperty(ref ROW_EFFECTIVE_DATEValue, value); }
        }

        private DateTime? ROW_EXPIRY_DATEValue;
        public DateTime? ROW_EXPIRY_DATE
        {
            get { return this.ROW_EXPIRY_DATEValue; }
            set { SetProperty(ref ROW_EXPIRY_DATEValue, value); }
        }

        private String ROW_QUALITYValue;
        public String ROW_QUALITY
        {
            get { return this.ROW_QUALITYValue; }
            set { SetProperty(ref ROW_QUALITYValue, value); }
        }

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

        private DateTime? EFFECTIVE_DATEValue;
        public DateTime? EFFECTIVE_DATE
        {
            get { return this.EFFECTIVE_DATEValue; }
            set { SetProperty(ref EFFECTIVE_DATEValue, value); }
        }

        private DateTime? EXPIRY_DATEValue;
        public DateTime? EXPIRY_DATE
        {
            get { return this.EXPIRY_DATEValue; }
            set { SetProperty(ref EXPIRY_DATEValue, value); }
        }
    }
}



