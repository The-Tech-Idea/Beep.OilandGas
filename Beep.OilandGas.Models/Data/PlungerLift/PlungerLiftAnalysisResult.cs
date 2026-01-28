using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    public class PlungerLiftAnalysisResult : ModelEntityBase
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
        private decimal LiquidProductionValue;

        public decimal LiquidProduction

        {

            get { return this.LiquidProductionValue; }

            set { SetProperty(ref LiquidProductionValue, value); }

        } // bbl/day
        private decimal GasProductionValue;

        public decimal GasProduction

        {

            get { return this.GasProductionValue; }

            set { SetProperty(ref GasProductionValue, value); }

        } // Mscf/day
        private decimal CycleEfficiencyValue;

        public decimal CycleEfficiency

        {

            get { return this.CycleEfficiencyValue; }

            set { SetProperty(ref CycleEfficiencyValue, value); }

        } // fraction 0-1
        private decimal LiquidLiftingEfficiencyValue;

        public decimal LiquidLiftingEfficiency

        {

            get { return this.LiquidLiftingEfficiencyValue; }

            set { SetProperty(ref LiquidLiftingEfficiencyValue, value); }

        }
        
        // Optimization results
        private decimal OptimalCycleTimeValue;

        public decimal OptimalCycleTime

        {

            get { return this.OptimalCycleTimeValue; }

            set { SetProperty(ref OptimalCycleTimeValue, value); }

        } // minutes
        private decimal OptimalFlowTimeValue;

        public decimal OptimalFlowTime

        {

            get { return this.OptimalFlowTimeValue; }

            set { SetProperty(ref OptimalFlowTimeValue, value); }

        } // minutes
        private decimal OptimalShutInTimeValue;

        public decimal OptimalShutInTime

        {

            get { return this.OptimalShutInTimeValue; }

            set { SetProperty(ref OptimalShutInTimeValue, value); }

        } // minutes
        private decimal CriticalVelocityValue;

        public decimal CriticalVelocity

        {

            get { return this.CriticalVelocityValue; }

            set { SetProperty(ref CriticalVelocityValue, value); }

        } // ft/sec
        
        // Additional metadata
        public PlungerLiftAnalysisAdditionalResults? AdditionalResults { get; set; }
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
        private decimal? ProductionRateValue;

        public decimal? ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        }
        private decimal? CycleTimeValue;

        public decimal? CycleTime

        {

            get { return this.CycleTimeValue; }

            set { SetProperty(ref CycleTimeValue, value); }

        }
        private decimal? GasFlowRateValue;

        public decimal? GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        }
        private decimal? PlungerVelocityValue;

        public decimal? PlungerVelocity

        {

            get { return this.PlungerVelocityValue; }

            set { SetProperty(ref PlungerVelocityValue, value); }

        }
        private decimal? OptimalGasFlowRateValue;

        public decimal? OptimalGasFlowRate

        {

            get { return this.OptimalGasFlowRateValue; }

            set { SetProperty(ref OptimalGasFlowRateValue, value); }

        }
        private decimal? MaximumProductionRateValue;

        public decimal? MaximumProductionRate

        {

            get { return this.MaximumProductionRateValue; }

            set { SetProperty(ref MaximumProductionRateValue, value); }

        }
        private decimal? FallTimeValue;

        public decimal? FallTime

        {

            get { return this.FallTimeValue; }

            set { SetProperty(ref FallTimeValue, value); }

        }
        private decimal? RiseTimeValue;

        public decimal? RiseTime

        {

            get { return this.RiseTimeValue; }

            set { SetProperty(ref RiseTimeValue, value); }

        }
        private decimal? ShutInTimeValue;

        public decimal? ShutInTime

        {

            get { return this.ShutInTimeValue; }

            set { SetProperty(ref ShutInTimeValue, value); }

        }
    }
}
