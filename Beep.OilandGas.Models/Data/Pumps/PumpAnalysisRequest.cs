using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.PlungerLift;
using Beep.OilandGas.Models.Data.SuckerRodPumping;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Pumps
{
    public class PumpAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

        } // WELL_EQUIPMENT or FACILITY_EQUIPMENT ROW_ID
        private string PumpTypeValue = "ESP";

        public string PumpType

        {

            get { return this.PumpTypeValue; }

            set { SetProperty(ref PumpTypeValue, value); }

        } // ESP, CENTRIFUGAL, POSITIVE_DISPLACEMENT, JET
        private string AnalysisTypeValue = "PERFORMANCE";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // PERFORMANCE, DESIGN, EFFICIENCY, NPSH, SYSTEM_CURVE
        
        // Pump properties (optional, will be retrieved from equipment if not provided)
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // GPM or bbl/day
        private decimal? HeadValue;

        public decimal? Head

        {

            get { return this.HeadValue; }

            set { SetProperty(ref HeadValue, value); }

        } // feet
        private decimal? PowerValue;

        public decimal? Power

        {

            get { return this.PowerValue; }

            set { SetProperty(ref PowerValue, value); }

        } // horsepower
        private decimal? EfficiencyValue;

        public decimal? Efficiency

        {

            get { return this.EfficiencyValue; }

            set { SetProperty(ref EfficiencyValue, value); }

        } // fraction 0-1
        private decimal? SpeedValue;

        public decimal? Speed

        {

            get { return this.SpeedValue; }

            set { SetProperty(ref SpeedValue, value); }

        } // RPM
        private decimal? ImpellerDiameterValue;

        public decimal? ImpellerDiameter

        {

            get { return this.ImpellerDiameterValue; }

            set { SetProperty(ref ImpellerDiameterValue, value); }

        } // inches
        private int? NumberOfStagesValue;

        public int? NumberOfStages

        {

            get { return this.NumberOfStagesValue; }

            set { SetProperty(ref NumberOfStagesValue, value); }

        } // for ESP
        
        // System properties
        private decimal? SuctionPressureValue;

        public decimal? SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        private decimal? DischargePressureValue;

        public decimal? DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal? FluidDensityValue;

        public decimal? FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        } // lb/ft³
        private decimal? FluidViscosityValue;

        public decimal? FluidViscosity

        {

            get { return this.FluidViscosityValue; }

            set { SetProperty(ref FluidViscosityValue, value); }

        } // cP
        
        // Additional parameters
        public PumpAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
