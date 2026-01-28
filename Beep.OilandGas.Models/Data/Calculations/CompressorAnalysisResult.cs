using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class CompressorAnalysisResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

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

        }
        private string CompressorTypeValue = string.Empty;

        public string CompressorType

        {

            get { return this.CompressorTypeValue; }

            set { SetProperty(ref CompressorTypeValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        
        // Power results
        private decimal PolytropicHeadValue;

        public decimal PolytropicHead

        {

            get { return this.PolytropicHeadValue; }

            set { SetProperty(ref PolytropicHeadValue, value); }

        } // feet
        private decimal AdiabaticHeadValue;

        public decimal AdiabaticHead

        {

            get { return this.AdiabaticHeadValue; }

            set { SetProperty(ref AdiabaticHeadValue, value); }

        } // feet
        private decimal PowerRequiredValue;

        public decimal PowerRequired

        {

            get { return this.PowerRequiredValue; }

            set { SetProperty(ref PowerRequiredValue, value); }

        } // horsepower
        private decimal DischargeTemperatureValue;

        public decimal DischargeTemperature

        {

            get { return this.DischargeTemperatureValue; }

            set { SetProperty(ref DischargeTemperatureValue, value); }

        } // Rankine
        
        // Efficiency results
        private decimal PolytropicEfficiencyValue;

        public decimal PolytropicEfficiency

        {

            get { return this.PolytropicEfficiencyValue; }

            set { SetProperty(ref PolytropicEfficiencyValue, value); }

        } // fraction 0-1
        private decimal AdiabaticEfficiencyValue;

        public decimal AdiabaticEfficiency

        {

            get { return this.AdiabaticEfficiencyValue; }

            set { SetProperty(ref AdiabaticEfficiencyValue, value); }

        } // fraction 0-1
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        } // fraction 0-1
        
        // Pressure and flow results
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal CompressionRatioValue;

        public decimal CompressionRatio

        {

            get { return this.CompressionRatioValue; }

            set { SetProperty(ref CompressionRatioValue, value); }

        }
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day or ACFM
        
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
        
        // Additional metadata
        public CompressorAnalysisAdditionalResults? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
