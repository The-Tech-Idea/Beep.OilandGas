using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.PlungerLift
{
    /// <summary>
    /// Request for Plunger Lift Analysis calculation
    /// </summary>
    public class PlungerLiftAnalysisRequest : ModelEntityBase
    {
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

        } // WELL_EQUIPMENT ROW_ID
        private string AnalysisTypeValue = "PERFORMANCE";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // PERFORMANCE, OPTIMIZATION, CYCLE_TIME
        
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
        
        // Plunger properties
        private decimal? PlungerWeightValue;

        public decimal? PlungerWeight

        {

            get { return this.PlungerWeightValue; }

            set { SetProperty(ref PlungerWeightValue, value); }

        } // lb
        private decimal? PlungerLengthValue;

        public decimal? PlungerLength

        {

            get { return this.PlungerLengthValue; }

            set { SetProperty(ref PlungerLengthValue, value); }

        } // feet
        private decimal? PlungerDiameterValue;

        public decimal? PlungerDiameter

        {

            get { return this.PlungerDiameterValue; }

            set { SetProperty(ref PlungerDiameterValue, value); }

        } // inches
        
        // Production parameters
        private decimal? GasFlowRateValue;

        public decimal? GasFlowRate

        {

            get { return this.GasFlowRateValue; }

            set { SetProperty(ref GasFlowRateValue, value); }

        } // Mscf/day
        private decimal? LiquidProductionRateValue;

        public decimal? LiquidProductionRate

        {

            get { return this.LiquidProductionRateValue; }

            set { SetProperty(ref LiquidProductionRateValue, value); }

        } // bbl/day
        private decimal? CycleTimeValue;

        public decimal? CycleTime

        {

            get { return this.CycleTimeValue; }

            set { SetProperty(ref CycleTimeValue, value); }

        } // minutes
        
        // Fluid properties
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
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
        
        // Additional parameters
        public Dictionary<string, object>? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of Plunger Lift Analysis calculation
    /// </summary>
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
        public Dictionary<string, object>? AdditionalResults { get; set; }
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


