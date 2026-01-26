using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.SuckerRodPumping
{
    /// <summary>
    /// Request for Sucker Rod Pumping Analysis calculation
    /// </summary>
    public class SuckerRodAnalysisRequest : ModelEntityBase
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
        private string AnalysisTypeValue = "LOAD";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // LOAD, POWER, PUMP_CARD, OPTIMIZATION
        
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
        private decimal? RodStringLengthValue;

        public decimal? RodStringLength

        {

            get { return this.RodStringLengthValue; }

            set { SetProperty(ref RodStringLengthValue, value); }

        } // feet
        private decimal? RodStringWeightValue;

        public decimal? RodStringWeight

        {

            get { return this.RodStringWeightValue; }

            set { SetProperty(ref RodStringWeightValue, value); }

        } // lb
        private decimal? PumpDepthValue;

        public decimal? PumpDepth

        {

            get { return this.PumpDepthValue; }

            set { SetProperty(ref PumpDepthValue, value); }

        } // feet
        private decimal? PumpDiameterValue;

        public decimal? PumpDiameter

        {

            get { return this.PumpDiameterValue; }

            set { SetProperty(ref PumpDiameterValue, value); }

        } // inches
        private decimal? StrokeLengthValue;

        public decimal? StrokeLength

        {

            get { return this.StrokeLengthValue; }

            set { SetProperty(ref StrokeLengthValue, value); }

        } // inches
        private decimal? StrokeRateValue;

        public decimal? StrokeRate

        {

            get { return this.StrokeRateValue; }

            set { SetProperty(ref StrokeRateValue, value); }

        } // strokes/minute
        
        // Fluid properties
        private decimal? FluidLevelValue;

        public decimal? FluidLevel

        {

            get { return this.FluidLevelValue; }

            set { SetProperty(ref FluidLevelValue, value); }

        } // feet
        private decimal? FluidDensityValue;

        public decimal? FluidDensity

        {

            get { return this.FluidDensityValue; }

            set { SetProperty(ref FluidDensityValue, value); }

        } // lb/ft³
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
        
        // Production parameters
        private decimal? ProductionRateValue;

        public decimal? ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal? VolumetricEfficiencyValue;

        public decimal? VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        } // fraction 0-1
        
        // Additional parameters
        public SuckerRodAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Result of Sucker Rod Pumping Analysis calculation
    /// </summary>
    public class SuckerRodAnalysisResult : ModelEntityBase
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
        
        // Load analysis
        private decimal PeakLoadValue;

        public decimal PeakLoad

        {

            get { return this.PeakLoadValue; }

            set { SetProperty(ref PeakLoadValue, value); }

        } // lb
        private decimal MinimumLoadValue;

        public decimal MinimumLoad

        {

            get { return this.MinimumLoadValue; }

            set { SetProperty(ref MinimumLoadValue, value); }

        } // lb
        private decimal RodStringWeightValue;

        public decimal RodStringWeight

        {

            get { return this.RodStringWeightValue; }

            set { SetProperty(ref RodStringWeightValue, value); }

        } // lb
        private decimal FluidLoadValue;

        public decimal FluidLoad

        {

            get { return this.FluidLoadValue; }

            set { SetProperty(ref FluidLoadValue, value); }

        } // lb
        private decimal DynamicLoadValue;

        public decimal DynamicLoad

        {

            get { return this.DynamicLoadValue; }

            set { SetProperty(ref DynamicLoadValue, value); }

        } // lb
        private decimal MaximumStressValue;

        public decimal MaximumStress

        {

            get { return this.MaximumStressValue; }

            set { SetProperty(ref MaximumStressValue, value); }

        } // psi
        private decimal SafetyFactorValue;

        public decimal SafetyFactor

        {

            get { return this.SafetyFactorValue; }

            set { SetProperty(ref SafetyFactorValue, value); }

        }
        
        // Power analysis
        private decimal PolishedRodHorsepowerValue;

        public decimal PolishedRodHorsepower

        {

            get { return this.PolishedRodHorsepowerValue; }

            set { SetProperty(ref PolishedRodHorsepowerValue, value); }

        } // HP
        private decimal HydraulicHorsepowerValue;

        public decimal HydraulicHorsepower

        {

            get { return this.HydraulicHorsepowerValue; }

            set { SetProperty(ref HydraulicHorsepowerValue, value); }

        } // HP
        private decimal FrictionHorsepowerValue;

        public decimal FrictionHorsepower

        {

            get { return this.FrictionHorsepowerValue; }

            set { SetProperty(ref FrictionHorsepowerValue, value); }

        } // HP
        private decimal TotalPowerRequiredValue;

        public decimal TotalPowerRequired

        {

            get { return this.TotalPowerRequiredValue; }

            set { SetProperty(ref TotalPowerRequiredValue, value); }

        } // HP
        
        // Production analysis
        private decimal ProductionRateValue;

        public decimal ProductionRate

        {

            get { return this.ProductionRateValue; }

            set { SetProperty(ref ProductionRateValue, value); }

        } // bbl/day
        private decimal PumpDisplacementValue;

        public decimal PumpDisplacement

        {

            get { return this.PumpDisplacementValue; }

            set { SetProperty(ref PumpDisplacementValue, value); }

        } // bbl/day
        private decimal VolumetricEfficiencyValue;

        public decimal VolumetricEfficiency

        {

            get { return this.VolumetricEfficiencyValue; }

            set { SetProperty(ref VolumetricEfficiencyValue, value); }

        } // fraction 0-1
        
        // Pump card data (load vs position)
        private List<PumpCardPoint>? PumpCardValue;

        public List<PumpCardPoint>? PumpCard

        {

            get { return this.PumpCardValue; }

            set { SetProperty(ref PumpCardValue, value); }

        }

        private string? PumpCardJsonValue;

        public string? PumpCardJson

        {

            get { return this.PumpCardJsonValue; }

            set { SetProperty(ref PumpCardJsonValue, value); }

        }
        
        // Optimization results
        private decimal? RecommendedStrokeLengthValue;

        public decimal? RecommendedStrokeLength

        {

            get { return this.RecommendedStrokeLengthValue; }

            set { SetProperty(ref RecommendedStrokeLengthValue, value); }

        } // inches
        private decimal? RecommendedStrokeRateValue;

        public decimal? RecommendedStrokeRate

        {

            get { return this.RecommendedStrokeRateValue; }

            set { SetProperty(ref RecommendedStrokeRateValue, value); }

        } // strokes/minute
        private decimal? RecommendedPumpSizeValue;

        public decimal? RecommendedPumpSize

        {

            get { return this.RecommendedPumpSizeValue; }

            set { SetProperty(ref RecommendedPumpSizeValue, value); }

        } // inches
        
        // Additional metadata
        public SuckerRodAnalysisAdditionalResults? AdditionalResults { get; set; }
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


