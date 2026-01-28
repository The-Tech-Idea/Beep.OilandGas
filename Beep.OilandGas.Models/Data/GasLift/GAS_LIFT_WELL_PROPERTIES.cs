using Beep.OilandGas.Models.Data;
using Beep.OilandGas.Models.Data.Calculations;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;

namespace Beep.OilandGas.Models.Data.GasLift
{
    public partial class GAS_LIFT_WELL_PROPERTIES : ModelEntityBase {
        private String GAS_LIFT_WELL_PROPERTIES_IDValue;
        public String GAS_LIFT_WELL_PROPERTIES_ID
        {
            get { return this.GAS_LIFT_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref GAS_LIFT_WELL_PROPERTIES_IDValue, value); }
        }

        private Decimal? WELL_DEPTHValue;
        public Decimal? WELL_DEPTH
        {
            get { return this.WELL_DEPTHValue; }
            set { SetProperty(ref WELL_DEPTHValue, value); }
        }

        private Decimal? WELLHEAD_PRESSUREValue;
        public Decimal? WELLHEAD_PRESSURE
        {
            get { return this.WELLHEAD_PRESSUREValue; }
            set { SetProperty(ref WELLHEAD_PRESSUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_PRESSUREValue;
        public Decimal? BOTTOM_HOLE_PRESSURE
        {
            get { return this.BOTTOM_HOLE_PRESSUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_PRESSUREValue, value); }
        }

        private Decimal? GAS_OIL_RATIOValue;
        public Decimal? GAS_OIL_RATIO
        {
            get { return this.GAS_OIL_RATIOValue; }
            set { SetProperty(ref GAS_OIL_RATIOValue, value); }
        }

        private Decimal? DESIRED_PRODUCTION_RATEValue;
        public Decimal? DESIRED_PRODUCTION_RATE
        {
            get { return this.DESIRED_PRODUCTION_RATEValue; }
            set { SetProperty(ref DESIRED_PRODUCTION_RATEValue, value); }
        }

        private Decimal? OIL_GRAVITYValue;
        public Decimal? OIL_GRAVITY
        {
            get { return this.OIL_GRAVITYValue; }
            set { SetProperty(ref OIL_GRAVITYValue, value); }
        }

        private Decimal? WATER_CUTValue;
        public Decimal? WATER_CUT
        {
            get { return this.WATER_CUTValue; }
            set { SetProperty(ref WATER_CUTValue, value); }
        }

        private Decimal? WELLHEAD_TEMPERATUREValue;
        public Decimal? WELLHEAD_TEMPERATURE
        {
            get { return this.WELLHEAD_TEMPERATUREValue; }
            set { SetProperty(ref WELLHEAD_TEMPERATUREValue, value); }
        }

        private Decimal? BOTTOM_HOLE_TEMPERATUREValue;
        public Decimal? BOTTOM_HOLE_TEMPERATURE
        {
            get { return this.BOTTOM_HOLE_TEMPERATUREValue; }
            set { SetProperty(ref BOTTOM_HOLE_TEMPERATUREValue, value); }
        }

        private Decimal? GAS_SPECIFIC_GRAVITYValue;
        public Decimal? GAS_SPECIFIC_GRAVITY
        {
            get { return this.GAS_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref GAS_SPECIFIC_GRAVITYValue, value); }
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


        private string WELL_UWIValue;
        public string WELL_UWI
        {
            get { return this.WELL_UWIValue; }
            set { SetProperty(ref WELL_UWIValue, value); }
        }

        private string WELL_TYPEValue;
        public string WELL_TYPE
        {
            get { return this.WELL_TYPEValue; }
            set { SetProperty(ref WELL_TYPEValue, value); }
        }

        private decimal RESERVOIR_PRESSUREValue;
        public decimal RESERVOIR_PRESSURE
        {
            get { return this.RESERVOIR_PRESSUREValue; }
            set { SetProperty(ref RESERVOIR_PRESSUREValue, value); }
        }

        private decimal CURRENT_PRODUCTION_RATEValue;
        public decimal CURRENT_PRODUCTION_RATE
        {
            get { return this.CURRENT_PRODUCTION_RATEValue; }
            set { SetProperty(ref CURRENT_PRODUCTION_RATEValue, value); }
        }

        private int TUBING_DIAMETERValue;
        public int TUBING_DIAMETER
        {
            get { return this.TUBING_DIAMETERValue; }
            set { SetProperty(ref TUBING_DIAMETERValue, value); }
        }

        private decimal TUBING_PRESSURE_RATINGValue;
        public decimal TUBING_PRESSURE_RATING
        {
            get { return this.TUBING_PRESSURE_RATINGValue; }
            set { SetProperty(ref TUBING_PRESSURE_RATINGValue, value); }
        }

        private decimal CASING_PRESSURE_RATINGValue;
        public decimal CASING_PRESSURE_RATING
        {
            get { return this.CASING_PRESSURE_RATINGValue; }
            set { SetProperty(ref CASING_PRESSURE_RATINGValue, value); }
        }

        private decimal TUBING_IDValue;
        public decimal TUBING_ID
        {
            get { return this.TUBING_IDValue; }
            set { SetProperty(ref TUBING_IDValue, value); }
        }

        private decimal TUBING_THICKNESSValue;
        public decimal TUBING_THICKNESS
        {
            get { return this.TUBING_THICKNESSValue; }
            set { SetProperty(ref TUBING_THICKNESSValue, value); }
        }

        private decimal CO_2_CONTENTValue;
        public decimal CO_2_CONTENT
        {
            get { return this.CO_2_CONTENTValue; }
            set { SetProperty(ref CO_2_CONTENTValue, value); }
        }

        private decimal H_2_SCONTENTValue;
        public decimal H_2_SCONTENT
        {
            get { return this.H_2_SCONTENTValue; }
            set { SetProperty(ref H_2_SCONTENTValue, value); }
        }

        private decimal PERMEABILITYValue;
        public decimal PERMEABILITY
        {
            get { return this.PERMEABILITYValue; }
            set { SetProperty(ref PERMEABILITYValue, value); }
        }

        private decimal POROSITYValue;
        public decimal POROSITY
        {
            get { return this.POROSITYValue; }
            set { SetProperty(ref POROSITYValue, value); }
        }

        private decimal NET_PAY_THICKNESSValue;
        public decimal NET_PAY_THICKNESS
        {
            get { return this.NET_PAY_THICKNESSValue; }
            set { SetProperty(ref NET_PAY_THICKNESSValue, value); }
        }

        private decimal CASING_DIAMETERValue;
        public decimal CASING_DIAMETER
        {
            get { return this.CASING_DIAMETERValue; }
            set { SetProperty(ref CASING_DIAMETERValue, value); }
        }

        public decimal OPTIMAL_GAS_INJECTION_RATE { get; set; }
        public decimal MAXIMUM_PRODUCTION_RATE { get; set; }
        public decimal OPTIMAL_GAS_LIQUID_RATIO { get; set; }
        public List<GasLiftPerformancePoint> PERFORMANCE_POINTS { get; set; } = new List<GasLiftPerformancePoint>();
    }
}
