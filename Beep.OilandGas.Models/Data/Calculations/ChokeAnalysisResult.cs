using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeAnalysisResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? EquipmentIdValue;

        public string? EquipmentId

        {

            get { return this.EquipmentIdValue; }

            set { SetProperty(ref EquipmentIdValue, value); }

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
        
        // Choke properties used
        private decimal ChokeDiameterValue;

        public decimal ChokeDiameter

        {

            get { return this.ChokeDiameterValue; }

            set { SetProperty(ref ChokeDiameterValue, value); }

        } // inches
        private string ChokeTypeValue = string.Empty;

        public string ChokeType

        {

            get { return this.ChokeTypeValue; }

            set { SetProperty(ref ChokeTypeValue, value); }

        }
        private decimal DischargeCoefficientValue;

        public decimal DischargeCoefficient

        {

            get { return this.DischargeCoefficientValue; }

            set { SetProperty(ref DischargeCoefficientValue, value); }

        }
        
        // Flow results
        private decimal FlowRateValue;

        public decimal FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // Mscf/day
        private decimal UpstreamPressureValue;

        public decimal UpstreamPressure

        {

            get { return this.UpstreamPressureValue; }

            set { SetProperty(ref UpstreamPressureValue, value); }

        } // psia
        private decimal DownstreamPressureValue;

        public decimal DownstreamPressure

        {

            get { return this.DownstreamPressureValue; }

            set { SetProperty(ref DownstreamPressureValue, value); }

        } // psia
        private decimal PressureRatioValue;

        public decimal PressureRatio

        {

            get { return this.PressureRatioValue; }

            set { SetProperty(ref PressureRatioValue, value); }

        }
        private string FlowRegimeValue = string.Empty;

        public string FlowRegime

        {

            get { return this.FlowRegimeValue; }

            set { SetProperty(ref FlowRegimeValue, value); }

        } // SONIC, SUBSONIC
        private decimal CriticalPressureRatioValue;

        public decimal CriticalPressureRatio

        {

            get { return this.CriticalPressureRatioValue; }

            set { SetProperty(ref CriticalPressureRatioValue, value); }

        }
        
        // Additional metadata
        public ChokeAnalysisAdditionalResults? AdditionalResults { get; set; }
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
