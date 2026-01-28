using System;
using System.Collections.Generic;

namespace Beep.OilandGas.Models.Data
{
    public class RiskAssessmentRequest : ModelEntityBase
    {
        private string? ProspectIdValue;

        public string? ProspectId

        {

            get { return this.ProspectIdValue; }

            set { SetProperty(ref ProspectIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        
        // Risk parameters
        private string? RiskModelValue;

        public string? RiskModel

        {

            get { return this.RiskModelValue; }

            set { SetProperty(ref RiskModelValue, value); }

        } // e.g., "VOLUMETRIC", "MONTE_CARLO", "DETERMINISTIC"
        private decimal? TrapRiskValue;

        public decimal? TrapRisk

        {

            get { return this.TrapRiskValue; }

            set { SetProperty(ref TrapRiskValue, value); }

        } // Probability (0-1)
        private decimal? ReservoirRiskValue;

        public decimal? ReservoirRisk

        {

            get { return this.ReservoirRiskValue; }

            set { SetProperty(ref ReservoirRiskValue, value); }

        } // Probability (0-1)
        private decimal? SealRiskValue;

        public decimal? SealRisk

        {

            get { return this.SealRiskValue; }

            set { SetProperty(ref SealRiskValue, value); }

        } // Probability (0-1)
        private decimal? SourceRiskValue;

        public decimal? SourceRisk

        {

            get { return this.SourceRiskValue; }

            set { SetProperty(ref SourceRiskValue, value); }

        } // Probability (0-1)
        private decimal? TimingRiskValue;

        public decimal? TimingRisk

        {

            get { return this.TimingRiskValue; }

            set { SetProperty(ref TimingRiskValue, value); }

        } // Probability (0-1)
        
        // Volume estimates (for volumetric risk)
        private decimal? LowEstimateOilValue;

        public decimal? LowEstimateOil

        {

            get { return this.LowEstimateOilValue; }

            set { SetProperty(ref LowEstimateOilValue, value); }

        } // P90
        private decimal? BestEstimateOilValue;

        public decimal? BestEstimateOil

        {

            get { return this.BestEstimateOilValue; }

            set { SetProperty(ref BestEstimateOilValue, value); }

        } // P50
        private decimal? HighEstimateOilValue;

        public decimal? HighEstimateOil

        {

            get { return this.HighEstimateOilValue; }

            set { SetProperty(ref HighEstimateOilValue, value); }

        } // P10
        private decimal? LowEstimateGasValue;

        public decimal? LowEstimateGas

        {

            get { return this.LowEstimateGasValue; }

            set { SetProperty(ref LowEstimateGasValue, value); }

        } // P90
        private decimal? BestEstimateGasValue;

        public decimal? BestEstimateGas

        {

            get { return this.BestEstimateGasValue; }

            set { SetProperty(ref BestEstimateGasValue, value); }

        } // P50
        private decimal? HighEstimateGasValue;

        public decimal? HighEstimateGas

        {

            get { return this.HighEstimateGasValue; }

            set { SetProperty(ref HighEstimateGasValue, value); }

        } // P10
        
        // Economic parameters (optional)
        private decimal? OilPriceValue;

        public decimal? OilPrice

        {

            get { return this.OilPriceValue; }

            set { SetProperty(ref OilPriceValue, value); }

        }
        private decimal? GasPriceValue;

        public decimal? GasPrice

        {

            get { return this.GasPriceValue; }

            set { SetProperty(ref GasPriceValue, value); }

        }
        private decimal? DevelopmentCostValue;

        public decimal? DevelopmentCost

        {

            get { return this.DevelopmentCostValue; }

            set { SetProperty(ref DevelopmentCostValue, value); }

        }
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }
}
