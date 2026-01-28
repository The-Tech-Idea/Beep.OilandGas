using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    public partial class HYDRAULIC_PISTON_PUMP_PROPERTIES : ModelEntityBase {
        private String HYDRAULIC_PISTON_PUMP_PROPERTIES_IDValue;
        public String HYDRAULIC_PISTON_PUMP_PROPERTIES_ID
        {
            get { return this.HYDRAULIC_PISTON_PUMP_PROPERTIES_IDValue; }
            set { SetProperty(ref HYDRAULIC_PISTON_PUMP_PROPERTIES_IDValue, value); }
        }

        private String HYDRAULIC_PUMP_WELL_PROPERTIES_IDValue;
        public String HYDRAULIC_PUMP_WELL_PROPERTIES_ID
        {
            get { return this.HYDRAULIC_PUMP_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref HYDRAULIC_PUMP_WELL_PROPERTIES_IDValue, value); }
        }

        private Decimal? PISTON_DIAMETERValue;
        public Decimal? PISTON_DIAMETER
        {
            get { return this.PISTON_DIAMETERValue; }
            set { SetProperty(ref PISTON_DIAMETERValue, value); }
        }

        private Decimal? ROD_DIAMETERValue;
        public Decimal? ROD_DIAMETER
        {
            get { return this.ROD_DIAMETERValue; }
            set { SetProperty(ref ROD_DIAMETERValue, value); }
        }

        private Decimal? STROKE_LENGTHValue;
        public Decimal? STROKE_LENGTH
        {
            get { return this.STROKE_LENGTHValue; }
            set { SetProperty(ref STROKE_LENGTHValue, value); }
        }

        private Decimal? STROKES_PER_MINUTEValue;
        public Decimal? STROKES_PER_MINUTE
        {
            get { return this.STROKES_PER_MINUTEValue; }
            set { SetProperty(ref STROKES_PER_MINUTEValue, value); }
        }

        private Decimal? POWER_FLUID_PRESSUREValue;
        public Decimal? POWER_FLUID_PRESSURE
        {
            get { return this.POWER_FLUID_PRESSUREValue; }
            set { SetProperty(ref POWER_FLUID_PRESSUREValue, value); }
        }

        private Decimal? POWER_FLUID_RATEValue;
        public Decimal? POWER_FLUID_RATE
        {
            get { return this.POWER_FLUID_RATEValue; }
            set { SetProperty(ref POWER_FLUID_RATEValue, value); }
        }

        private Decimal? POWER_FLUID_SPECIFIC_GRAVITYValue;
        public Decimal? POWER_FLUID_SPECIFIC_GRAVITY
        {
            get { return this.POWER_FLUID_SPECIFIC_GRAVITYValue; }
            set { SetProperty(ref POWER_FLUID_SPECIFIC_GRAVITYValue, value); }
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


        private decimal? DISPLACEMENTValue;
        public decimal? DISPLACEMENT
        {
            get { return this.DISPLACEMENTValue; }
            set { SetProperty(ref DISPLACEMENTValue, value); }
        }

        private decimal VOLUMETRIC_EFFICIENCYValue;
        public decimal VOLUMETRIC_EFFICIENCY
        {
            get { return this.VOLUMETRIC_EFFICIENCYValue; }
            set { SetProperty(ref VOLUMETRIC_EFFICIENCYValue, value); }
        }

        private decimal MECHANICAL_EFFICIENCYValue;
        public decimal MECHANICAL_EFFICIENCY
        {
            get { return this.MECHANICAL_EFFICIENCYValue; }
            set { SetProperty(ref MECHANICAL_EFFICIENCYValue, value); }
        }

        private string? MANUFACTURERValue;
        public string? MANUFACTURER
        {
            get { return this.MANUFACTURERValue; }
            set { SetProperty(ref MANUFACTURERValue, value); }
        }

        private string? PUMP_IDValue;
        public string? PUMP_ID
        {
            get { return this.PUMP_IDValue; }
            set { SetProperty(ref PUMP_IDValue, value); }
        }
    }
}
