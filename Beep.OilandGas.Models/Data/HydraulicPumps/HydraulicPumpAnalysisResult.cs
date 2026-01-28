using Beep.OilandGas.Models.Data.HydraulicPumps;
using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class HydraulicPumpAnalysisResult : ModelEntityBase
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
        
        // Performance results
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal PowerFluidRateValue;

        public decimal PowerFluidRate

        {

            get { return this.PowerFluidRateValue; }

            set { SetProperty(ref PowerFluidRateValue, value); }

        } // bbl/day
        private decimal PowerFluidPressureValue;

        public decimal PowerFluidPressure

        {

            get { return this.PowerFluidPressureValue; }

            set { SetProperty(ref PowerFluidPressureValue, value); }

        } // psia
        private decimal DischargePressureValue;

        public decimal DischargePressure

        {

            get { return this.DischargePressureValue; }

            set { SetProperty(ref DischargePressureValue, value); }

        } // psia
        private decimal SuctionPressureValue;

        public decimal SuctionPressure

        {

            get { return this.SuctionPressureValue; }

            set { SetProperty(ref SuctionPressureValue, value); }

        } // psia
        
        // Efficiency results
        private decimal HydraulicEfficiencyValue;

        public decimal HydraulicEfficiency

        {

            get { return this.HydraulicEfficiencyValue; }

            set { SetProperty(ref HydraulicEfficiencyValue, value); }

        } // fraction 0-1
        private decimal OverallEfficiencyValue;

        public decimal OverallEfficiency

        {

            get { return this.OverallEfficiencyValue; }

            set { SetProperty(ref OverallEfficiencyValue, value); }

        } // fraction 0-1
        private decimal PowerRequiredValue;

        public decimal PowerRequired

        {

            get { return this.PowerRequiredValue; }

            set { SetProperty(ref PowerRequiredValue, value); }

        } // horsepower
        
        // Design results
        private decimal? RecommendedNozzleSizeValue;

        public decimal? RecommendedNozzleSize

        {

            get { return this.RecommendedNozzleSizeValue; }

            set { SetProperty(ref RecommendedNozzleSizeValue, value); }

        } // inches
        private decimal? RecommendedThroatSizeValue;

        public decimal? RecommendedThroatSize

        {

            get { return this.RecommendedThroatSizeValue; }

            set { SetProperty(ref RecommendedThroatSizeValue, value); }

        } // inches
        private decimal? RecommendedPowerFluidRateValue;

        public decimal? RecommendedPowerFluidRate

        {

            get { return this.RecommendedPowerFluidRateValue; }

            set { SetProperty(ref RecommendedPowerFluidRateValue, value); }

        } // bbl/day
        
        // Additional metadata
        public HydraulicPumpAnalysisAdditionalResults? AdditionalResults { get; set; }
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
