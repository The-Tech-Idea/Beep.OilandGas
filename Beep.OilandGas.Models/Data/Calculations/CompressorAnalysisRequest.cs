using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorAnalysisRequest : ModelEntityBase
    {
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

        } // FACILITY_EQUIPMENT ROW_ID
        private string CompressorTypeValue = "CENTRIFUGAL";

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        } // CENTRIFUGAL, RECIPROCATING
        private string AnalysisTypeValue = "POWER";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // POWER, PRESSURE, EFFICIENCY
        
        // Compressor properties (optional, will be retrieved from equipment if not provided)
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
        private decimal? SuctionTemperatureValue;

        public decimal? SuctionTemperature

        {

            get { return this.SuctionTemperatureValue; }

            set { SetProperty(ref SuctionTemperatureValue, value); }

        } // Rankine
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day or ACFM
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal? CompressionRatioValue;

        public decimal? CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }
        
        // Centrifugal compressor specific
        private decimal? PolytropicEfficiencyValue;

        public decimal? PolytropicEfficiency

        {

            get { return this.PolytropicEfficiencyValue; }

            set { SetProperty(ref PolytropicEfficiencyValue, value); }

        } // fraction 0-1
        private decimal? AdiabaticEfficiencyValue;

        public decimal? AdiabaticEfficiency

        {

            get { return this.AdiabaticEfficiencyValue; }

            set { SetProperty(ref AdiabaticEfficiencyValue, value); }

        } // fraction 0-1
        private int? NumberOfStagesValue;

        public int? NumberOfStages

        {

            get { return this.NumberOfStagesValue; }

            set { SetProperty(ref NumberOfStagesValue, value); }

        }
        
        // Reciprocating compressor specific
        private decimal? CylinderDisplacementValue;

        public decimal? CylinderDisplacement

        {

            get { return this.CylinderDisplacementValue; }

            set { SetProperty(ref CylinderDisplacementValue, value); }

        } // ACFM
        private decimal? VolumetricEfficiencyValue;

        public decimal? VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        } // fraction 0-1
        private int? NumberOfCylindersValue;

        public int? NumberOfCylinders

        {

            get { return this.NumberOfCylindersValue; }

            set { SetProperty(ref NumberOfCylindersValue, value); }

        }
        
        // Additional parameters
        public CompressorAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
