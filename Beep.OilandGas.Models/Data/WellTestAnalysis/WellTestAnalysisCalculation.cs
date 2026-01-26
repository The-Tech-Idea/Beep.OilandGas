using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    /// <summary>
    /// Request for Well Test Analysis calculation
    /// </summary>
    public class WellTestAnalysisCalculationRequest : ModelEntityBase
    {
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        }
        private string AnalysisTypeValue = "BUILDUP";

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // BUILDUP, DRAWDOWN, MULTI_RATE
        private string? AnalysisMethodValue;

        public string? AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        } // HORNER, MDH, AGARWAL
        
        // Well test data (if provided directly)
        private List<WellTestDataPoint>? PressureTimeDataValue;

        public List<WellTestDataPoint>? PressureTimeData

        {

            get { return this.PressureTimeDataValue; }

            set { SetProperty(ref PressureTimeDataValue, value); }

        }
        
        // Well properties (if not in PPDM)
        private decimal? FlowRateValue;

        public decimal? FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        } // BPD or Mscf/day
        private decimal? WellboreRadiusValue;

        public decimal? WellboreRadius

        {

            get { return this.WellboreRadiusValue; }

            set { SetProperty(ref WellboreRadiusValue, value); }

        } // feet
        private decimal? FormationThicknessValue;

        public decimal? FormationThickness

        {

            get { return this.FormationThicknessValue; }

            set { SetProperty(ref FormationThicknessValue, value); }

        } // feet
        private decimal? PorosityValue;

        public decimal? Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private decimal? TotalCompressibilityValue;

        public decimal? TotalCompressibility

        {

            get { return this.TotalCompressibilityValue; }

            set { SetProperty(ref TotalCompressibilityValue, value); }

        } // psi^-1
        private decimal? OilViscosityValue;

        public decimal? OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        } // cp
        private decimal? OilFormationVolumeFactorValue;

        public decimal? OilFormationVolumeFactor

        {

            get { return this.OilFormationVolumeFactorValue; }

            set { SetProperty(ref OilFormationVolumeFactorValue, value); }

        } // RB/STB
        private decimal? ProductionTimeValue;

        public decimal? ProductionTime

        {

            get { return this.ProductionTimeValue; }

            set { SetProperty(ref ProductionTimeValue, value); }

        } // hours (for build-up)
        private bool? IsGasWellValue;

        public bool? IsGasWell

        {

            get { return this.IsGasWellValue; }

            set { SetProperty(ref IsGasWellValue, value); }

        }
        private decimal? GasSpecificGravityValue;

        public decimal? GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private decimal? ReservoirTemperatureValue;

        public decimal? ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        } // Fahrenheit
        private decimal? InitialReservoirPressureValue;

        public decimal? InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        } // psi
        
        // Additional parameters
        public WellTestAnalysisOptions? AdditionalParameters { get; set; }
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        }
    }

    /// <summary>
    /// Well test data point (pressure-time pair)
    /// </summary>
    public class WellTestDataPoint : ModelEntityBase
    {
        private decimal? TimeValue;

        public decimal? Time

        {

            get { return this.TimeValue; }

            set { SetProperty(ref TimeValue, value); }

        } // hours
        private decimal? PressureValue;

        public decimal? Pressure

        {

            get { return this.PressureValue; }

            set { SetProperty(ref PressureValue, value); }

        } // psi
    }
}


