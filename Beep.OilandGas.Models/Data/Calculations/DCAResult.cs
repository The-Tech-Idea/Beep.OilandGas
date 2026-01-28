using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class DCAResult : ModelEntityBase
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
        private string? PoolIdValue;

        public string? PoolId

        {

            get { return this.PoolIdValue; }

            set { SetProperty(ref PoolIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private string CalculationTypeValue = string.Empty;

        public string CalculationType

        {

            get { return this.CalculationTypeValue; }

            set { SetProperty(ref CalculationTypeValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        private string? ProductionFluidTypeValue;

        public string? ProductionFluidType

        {

            get { return this.ProductionFluidTypeValue; }

            set { SetProperty(ref ProductionFluidTypeValue, value); }

        }
        
        // Decline curve parameters
        private decimal? InitialRateValue;

        public decimal? InitialRate

        {

            get { return this.InitialRateValue; }

            set { SetProperty(ref InitialRateValue, value); }

        }
        private decimal? DeclineRateValue;

        public decimal? DeclineRate

        {

            get { return this.DeclineRateValue; }

            set { SetProperty(ref DeclineRateValue, value); }

        }
        private decimal? DeclineConstantValue;

        public decimal? DeclineConstant

        {

            get { return this.DeclineConstantValue; }

            set { SetProperty(ref DeclineConstantValue, value); }

        }
        private decimal? NominalDeclineRateValue;

        public decimal? NominalDeclineRate

        {

            get { return this.NominalDeclineRateValue; }

            set { SetProperty(ref NominalDeclineRateValue, value); }

        }
        private decimal? EffectiveDeclineRateValue;

        public decimal? EffectiveDeclineRate

        {

            get { return this.EffectiveDeclineRateValue; }

            set { SetProperty(ref EffectiveDeclineRateValue, value); }

        }
        private decimal? HyperbolicExponentValue;

        public decimal? HyperbolicExponent

        {

            get { return this.HyperbolicExponentValue; }

            set { SetProperty(ref HyperbolicExponentValue, value); }

        }
        
        // Forecasted production
        private List<DCAForecastPoint> ForecastPointsValue = new List<DCAForecastPoint>();

        public List<DCAForecastPoint> ForecastPoints

        {

            get { return this.ForecastPointsValue; }

            set { SetProperty(ref ForecastPointsValue, value); }

        }
        
        // Statistical metrics
        private decimal? RMSEValue;

        public decimal? RMSE

        {

            get { return this.RMSEValue; }

            set { SetProperty(ref RMSEValue, value); }

        } // Root Mean Square Error
        private decimal? R2Value;

        public decimal? R2

        {

            get { return this.R2Value; }

            set { SetProperty(ref R2Value, value); }

        } // Coefficient of determination
        private decimal? CorrelationCoefficientValue;

        public decimal? CorrelationCoefficient

        {

            get { return this.CorrelationCoefficientValue; }

            set { SetProperty(ref CorrelationCoefficientValue, value); }

        }
        
        // Estimated reserves
        private decimal? EstimatedEURValue;

        public decimal? EstimatedEUR

        {

            get { return this.EstimatedEURValue; }

            set { SetProperty(ref EstimatedEURValue, value); }

        } // Estimated Ultimate Recovery
        private decimal? RemainingReservesValue;

        public decimal? RemainingReserves

        {

            get { return this.RemainingReservesValue; }

            set { SetProperty(ref RemainingReservesValue, value); }

        }
        
        // Additional metadata
        public DcaAdditionalResults? AdditionalResults { get; set; }
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // SUCCESS, FAILED, PARTIAL
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
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string DeclineTypeValue;

        public string DeclineType

        {

            get { return this.DeclineTypeValue; }

            set { SetProperty(ref DeclineTypeValue, value); }

        }
        private List<decimal> ForecastedProductionValue;

        public List<decimal> ForecastedProduction

        {

            get { return this.ForecastedProductionValue; }

            set { SetProperty(ref ForecastedProductionValue, value); }

        }
        private bool IsSuccessfulValue;

        public bool IsSuccessful

        {

            get { return this.IsSuccessfulValue; }

            set { SetProperty(ref IsSuccessfulValue, value); }

        }
    }
}
