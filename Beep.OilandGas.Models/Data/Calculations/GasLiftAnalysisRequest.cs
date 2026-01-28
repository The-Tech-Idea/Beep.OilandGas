using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.GasLift;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class GasLiftAnalysisRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string AnalysisTypeValue = "POTENTIAL";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // POTENTIAL, VALVE_DESIGN, VALVE_SPACING
        
        // Well properties (optional, will be retrieved from WELL if not provided)
        private decimal? WellDepthValue;

        public decimal? WellDepth

        {

            get { return this.WellDepthValue; }

            set { SetProperty(ref WellDepthValue, value); }

        } // feet
        private decimal? TubingDiameterValue;

        public decimal? TubingDiameter

        {

            get { return this.TubingDiameterValue; }

            set { SetProperty(ref TubingDiameterValue, value); }

        } // inches
        private decimal? CasingDiameterValue;

        public decimal? CasingDiameter

        {

            get { return this.CasingDiameterValue; }

            set { SetProperty(ref CasingDiameterValue, value); }

        } // inches
        private decimal? WellheadPressureValue;

        public decimal? WellheadPressure

        {

            get { return this.WellheadPressureValue; }

            set { SetProperty(ref WellheadPressureValue, value); }

        } // psia
        private decimal? BottomHolePressureValue;

        public decimal? BottomHolePressure

        {

            get { return this.BottomHolePressureValue; }

            set { SetProperty(ref BottomHolePressureValue, value); }

        } // psia
        private decimal? WellheadTemperatureValue;

        public decimal? WellheadTemperature

        {

            get { return this.WellheadTemperatureValue; }

            set { SetProperty(ref WellheadTemperatureValue, value); }

        } // Rankine
        private decimal? BottomHoleTemperatureValue;

        public decimal? BottomHoleTemperature

        {

            get { return this.BottomHoleTemperatureValue; }

            set { SetProperty(ref BottomHoleTemperatureValue, value); }

        } // Rankine
        private decimal? OilGravityValue;

        public decimal? OilGravity

        {

            get { return this.OilGravityValue; }

            set { SetProperty(ref OilGravityValue, value); }

        } // API
        private decimal? WaterCutValue;

        public decimal? WaterCut

        {

            get { return this.WaterCutValue; }

            set { SetProperty(ref WaterCutValue, value); }

        } // fraction 0-1
        private decimal? GasOilRatioValue;

        public decimal? GasOilRatio

        {

            get { return this.GasOilRatioValue; }

            set { SetProperty(ref GasOilRatioValue, value); }

        } // scf/bbl
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal? DesiredProductionRateValue;

        public decimal? DesiredProductionRate

        {

            get { return this.DesiredProductionRateValue; }

            set { SetProperty(ref DesiredProductionRateValue, value); }

        } // bbl/day
        
        // Analysis parameters
        private decimal? MinGasInjectionRateValue;

        public decimal? MinGasInjectionRate

        {

            get { return this.MinGasInjectionRateValue; }

            set { SetProperty(ref MinGasInjectionRateValue, value); }

        } // Mscf/day
        private decimal? MaxGasInjectionRateValue;

        public decimal? MaxGasInjectionRate

        {

            get { return this.MaxGasInjectionRateValue; }

            set { SetProperty(ref MaxGasInjectionRateValue, value); }

        } // Mscf/day
        private int? NumberOfPointsValue;

        public int? NumberOfPoints

        {

            get { return this.NumberOfPointsValue; }

            set { SetProperty(ref NumberOfPointsValue, value); }

        } // for performance curve
        
        // Additional parameters
        public GasLiftAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
