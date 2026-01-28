using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.HydraulicPumps
{
    public partial class HYDRAULIC_JET_PUMP_PROPERTIES : ModelEntityBase {
        private String HYDRAULIC_JET_PUMP_PROPERTIES_IDValue;
        public String HYDRAULIC_JET_PUMP_PROPERTIES_ID
        {
            get { return this.HYDRAULIC_JET_PUMP_PROPERTIES_IDValue; }
            set { SetProperty(ref HYDRAULIC_JET_PUMP_PROPERTIES_IDValue, value); }
        }

        private String HYDRAULIC_PUMP_WELL_PROPERTIES_IDValue;
        public String HYDRAULIC_PUMP_WELL_PROPERTIES_ID
        {
            get { return this.HYDRAULIC_PUMP_WELL_PROPERTIES_IDValue; }
            set { SetProperty(ref HYDRAULIC_PUMP_WELL_PROPERTIES_IDValue, value); }
        }

        private Decimal? NOZZLE_DIAMETERValue;
        public Decimal? NOZZLE_DIAMETER
        {
            get { return this.NOZZLE_DIAMETERValue; }
            set { SetProperty(ref NOZZLE_DIAMETERValue, value); }
        }

        private Decimal? THROAT_DIAMETERValue;
        public Decimal? THROAT_DIAMETER
        {
            get { return this.THROAT_DIAMETERValue; }
            set { SetProperty(ref THROAT_DIAMETERValue, value); }
        }

        private Decimal? DIFFUSER_DIAMETERValue;
        public Decimal? DIFFUSER_DIAMETER
        {
            get { return this.DIFFUSER_DIAMETERValue; }
            set { SetProperty(ref DIFFUSER_DIAMETERValue, value); }
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


        private string? PRODUCTION_FLUID_TYPEValue;
        public string? PRODUCTION_FLUID_TYPE
        {
            get { return this.PRODUCTION_FLUID_TYPEValue; }
            set { SetProperty(ref PRODUCTION_FLUID_TYPEValue, value); }
        }

        private decimal? PUMP_DEPTHValue;
        public decimal? PUMP_DEPTH
        {
            get { return this.PUMP_DEPTHValue; }
            set { SetProperty(ref PUMP_DEPTHValue, value); }
        }

        private string? PUMP_IDValue;
        public string? PUMP_ID
        {
            get { return this.PUMP_IDValue; }
            set { SetProperty(ref PUMP_IDValue, value); }
        }

        private string? MANUFACTURERValue;
        public string? MANUFACTURER
        {
            get { return this.MANUFACTURERValue; }
            set { SetProperty(ref MANUFACTURERValue, value); }
        }
    }
}
