using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data.FlashCalculations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class FlashCalculationResult : ModelEntityBase
    {
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        public string FlashCalculationResultId

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
        private string? FacilityIdValue;

        public string? FacilityId

        {

            get { return this.FacilityIdValue; }

            set { SetProperty(ref FacilityIdValue, value); }

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
        private decimal? PressureValue;

        public decimal? Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        }
        private decimal? TemperatureValue;

        public decimal? Temperature

        {

            get { return this.TemperatureValue; }

            set { SetProperty(ref TemperatureValue, value); }

        }
        private List<Component>? FeedCompositionValue;

        public List<Component>? FeedComposition

        {

            get { return this.FeedCompositionValue; }

            set { SetProperty(ref FeedCompositionValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        }
        
        // Flash results
        private decimal VaporFractionValue;

        public decimal VaporFraction

        {

            get { return this.VaporFractionValue; }

            set { SetProperty(ref VaporFractionValue, value); }

        } // 0-1
        private decimal LiquidFractionValue;

        public decimal LiquidFraction

        {

            get { return this.LiquidFractionValue; }

            set { SetProperty(ref LiquidFractionValue, value); }

        } // 0-1
        
        // Phase compositions (mole fractions)
        public List<FlashComponentFraction> VaporComposition { get; set; } = new();
        public List<FlashComponentFraction> LiquidComposition { get; set; } = new();
        
        // K-values (equilibrium ratios)
        public List<FlashComponentKValue> KValues { get; set; } = new();

        private string? FeedCompositionJsonValue;

        public string? FeedCompositionJson

        {

            get { return this.FeedCompositionJsonValue; }

            set { SetProperty(ref FeedCompositionJsonValue, value); }

        }
        private string? VaporCompositionJsonValue;

        public string? VaporCompositionJson

        {

            get { return this.VaporCompositionJsonValue; }

            set { SetProperty(ref VaporCompositionJsonValue, value); }

        }
        private string? LiquidCompositionJsonValue;

        public string? LiquidCompositionJson

        {

            get { return this.LiquidCompositionJsonValue; }

            set { SetProperty(ref LiquidCompositionJsonValue, value); }

        }
        private string? KValuesJsonValue;

        public string? KValuesJson

        {

            get { return this.KValuesJsonValue; }

            set { SetProperty(ref KValuesJsonValue, value); }

        }
        
        // Phase properties
        private PhasePropertiesData? VaporPropertiesValue;

        public PhasePropertiesData? VaporProperties

        {

            get { return this.VaporPropertiesValue; }

            set { SetProperty(ref VaporPropertiesValue, value); }

        }
        private PhasePropertiesData? LiquidPropertiesValue;

        public PhasePropertiesData? LiquidProperties

        {

            get { return this.LiquidPropertiesValue; }

            set { SetProperty(ref LiquidPropertiesValue, value); }

        }
        
        // Convergence information
        private int IterationsValue;

        public int Iterations

        {

            get { return this.IterationsValue; }

            set { SetProperty(ref IterationsValue, value); }

        }
        private bool ConvergedValue;

        public bool Converged

        {

            get { return this.ConvergedValue; }

            set { SetProperty(ref ConvergedValue, value); }

        }
        private decimal ConvergenceErrorValue;

        public decimal ConvergenceError

        {

            get { return this.ConvergenceErrorValue; }

            set { SetProperty(ref ConvergenceErrorValue, value); }

        }
        
        // Additional metadata
        public FlashCalculationAdditionalResults? AdditionalResults { get; set; }
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
        private bool IsSuccessfulValue;

        public bool IsSuccessful

        {

            get { return this.IsSuccessfulValue; }

            set { SetProperty(ref IsSuccessfulValue, value); }

        }
    }
}
