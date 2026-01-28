
namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    public class WellTestAnalysisResult : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private string CalculationIdValue = string.Empty;

        public string CalculationId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        } // Added from Calculations.cs
        public string WellTestAnalysisResultId

        {

            get { return this.CalculationIdValue; }

            set { SetProperty(ref CalculationIdValue, value); }

        }
        private string? WellIdValue;

        public string? WellId

        {

            get { return this.WellIdValue; }

            set { SetProperty(ref WellIdValue, value); }

        } // Matches Calculations.cs (nullable there)
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string? TestIdValue;

        public string? TestId

        {

            get { return this.TestIdValue; }

            set { SetProperty(ref TestIdValue, value); }

        } // Added from Calculations.cs
        private string? WellTestDataIdValue;

        public string? WellTestDataId

        {

            get { return this.WellTestDataIdValue; }

            set { SetProperty(ref WellTestDataIdValue, value); }

        }
        private string? FieldIdValue;

        public string? FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        } // Added from Calculations.cs
        private DateTime AnalysisDateValue = DateTime.UtcNow;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private DateTime CalculationDateValue;

        public DateTime CalculationDate

        {

            get { return this.CalculationDateValue; }

            set { SetProperty(ref CalculationDateValue, value); }

        } // Added from Calculations.cs
        private string AnalysisByUserValue = string.Empty;

        public string AnalysisByUser

        {

            get { return this.AnalysisByUserValue; }

            set { SetProperty(ref AnalysisByUserValue, value); }

        }

        private double PermeabilityValue;


        public double Permeability


        {


            get { return this.PermeabilityValue; }


            set { SetProperty(ref PermeabilityValue, value); }


        } // double vs decimal in Calculations.cs - sticking to double as it's more common for technical analysis, or change to decimal if Money/Financial? Engineering usually uses double. Calculations used decimal. Let's support decimal? No, usually heavy math like this uses double. But the other file used decimal. I'll stick to double for now but maybe I should standardize.
        // Actually, Calculations.cs used decimal. Models usually use decimal for financial, double for scientific.
        // Let's keep existing types here (double) but add the extra fields.

        private double SkinFactorValue;


        public double SkinFactor


        {


            get { return this.SkinFactorValue; }


            set { SetProperty(ref SkinFactorValue, value); }


        }
        private double ReservoirPressureValue;

        public double ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private double ProductivityIndexValue;

        public double ProductivityIndex

        {

            get { return this.ProductivityIndexValue; }

            set { SetProperty(ref ProductivityIndexValue, value); }

        }
        private double FlowEfficiencyValue;

        public double FlowEfficiency

        {

            get { return this.FlowEfficiencyValue; }

            set { SetProperty(ref FlowEfficiencyValue, value); }

        }
        private double DamageRatioValue;

        public double DamageRatio

        {

            get { return this.DamageRatioValue; }

            set { SetProperty(ref DamageRatioValue, value); }

        }
        private double RadiusOfInvestigationValue;

        public double RadiusOfInvestigation

        {

            get { return this.RadiusOfInvestigationValue; }

            set { SetProperty(ref RadiusOfInvestigationValue, value); }

        }

        private ReservoirModel IdentifiedModelValue = ReservoirModel.InfiniteActing;


        public ReservoirModel IdentifiedModel


        {


            get { return this.IdentifiedModelValue; }


            set { SetProperty(ref IdentifiedModelValue, value); }


        }
        private string IdentifiedModelStringValue = "INFINITE_ACTING";

        public string IdentifiedModelString

        {

            get { return this.IdentifiedModelStringValue; }

            set { SetProperty(ref IdentifiedModelStringValue, value); }

        } // For compatibility if needed

        private double RSquaredValue;


        public double RSquared


        {


            get { return this.RSquaredValue; }


            set { SetProperty(ref RSquaredValue, value); }


        }
        private string AnalysisMethodValue = string.Empty;

        public string AnalysisMethod

        {

            get { return this.AnalysisMethodValue; }

            set { SetProperty(ref AnalysisMethodValue, value); }

        }
        private string AnalysisTypeValue = string.Empty;

        public string AnalysisType

        {

            get { return this.AnalysisTypeValue; }

            set { SetProperty(ref AnalysisTypeValue, value); }

        } // Added from Calculations.cs
        private double FlowRateValue;

        public double FlowRate

        {

            get { return this.FlowRateValue; }

            set { SetProperty(ref FlowRateValue, value); }

        }
        private double WellboreRadiusValue;

        public double WellboreRadius

        {

            get { return this.WellboreRadiusValue; }

            set { SetProperty(ref WellboreRadiusValue, value); }

        }
        private double FormationThicknessValue;

        public double FormationThickness

        {

            get { return this.FormationThicknessValue; }

            set { SetProperty(ref FormationThicknessValue, value); }

        }
        private double PorosityValue;

        public double Porosity

        {

            get { return this.PorosityValue; }

            set { SetProperty(ref PorosityValue, value); }

        }
        private double TotalCompressibilityValue;

        public double TotalCompressibility

        {

            get { return this.TotalCompressibilityValue; }

            set { SetProperty(ref TotalCompressibilityValue, value); }

        }
        private double OilViscosityValue;

        public double OilViscosity

        {

            get { return this.OilViscosityValue; }

            set { SetProperty(ref OilViscosityValue, value); }

        }
        private double OilFormationVolumeFactorValue;

        public double OilFormationVolumeFactor

        {

            get { return this.OilFormationVolumeFactorValue; }

            set { SetProperty(ref OilFormationVolumeFactorValue, value); }

        }
        private double ProductionTimeValue;

        public double ProductionTime

        {

            get { return this.ProductionTimeValue; }

            set { SetProperty(ref ProductionTimeValue, value); }

        }
        private bool IsGasWellValue;

        public bool IsGasWell

        {

            get { return this.IsGasWellValue; }

            set { SetProperty(ref IsGasWellValue, value); }

        }
        private double GasSpecificGravityValue;

        public double GasSpecificGravity

        {

            get { return this.GasSpecificGravityValue; }

            set { SetProperty(ref GasSpecificGravityValue, value); }

        }
        private double ReservoirTemperatureValue;

        public double ReservoirTemperature

        {

            get { return this.ReservoirTemperatureValue; }

            set { SetProperty(ref ReservoirTemperatureValue, value); }

        }
        private double InitialReservoirPressureValue;

        public double InitialReservoirPressure

        {

            get { return this.InitialReservoirPressureValue; }

            set { SetProperty(ref InitialReservoirPressureValue, value); }

        }

        private List<WellTestDataPoint>? DiagnosticPointsValue;


        public List<WellTestDataPoint>? DiagnosticPoints


        {


            get { return this.DiagnosticPointsValue; }


            set { SetProperty(ref DiagnosticPointsValue, value); }


        } // Added from Calculations.cs
        private List<WellTestDataPoint>? DerivativePointsValue;

        public List<WellTestDataPoint>? DerivativePoints

        {

            get { return this.DerivativePointsValue; }

            set { SetProperty(ref DerivativePointsValue, value); }

        } // Added from Calculations.cs

        private string? DiagnosticDataJsonValue;

        public string? DiagnosticDataJson

        {

            get { return this.DiagnosticDataJsonValue; }

            set { SetProperty(ref DiagnosticDataJsonValue, value); }

        }
        private string? DerivativeDataJsonValue;

        public string? DerivativeDataJson

        {

            get { return this.DerivativeDataJsonValue; }

            set { SetProperty(ref DerivativeDataJsonValue, value); }

        }

        public WellTestAnalysisAdditionalResults? AdditionalResults { get; set; } // Added from Calculations.cs
        private string? StatusValue;

        public string? Status

        {

            get { return this.StatusValue; }

            set { SetProperty(ref StatusValue, value); }

        } // Added from Calculations.cs
        private string? ErrorMessageValue;

        public string? ErrorMessage

        {

            get { return this.ErrorMessageValue; }

            set { SetProperty(ref ErrorMessageValue, value); }

        } // Added from Calculations.cs
        private string? UserIdValue;

        public string? UserId

        {

            get { return this.UserIdValue; }

            set { SetProperty(ref UserIdValue, value); }

        } // Added from Calculations.cs
        private bool IsSuccessfulValue;

        public bool IsSuccessful

        {

            get { return this.IsSuccessfulValue; }

            set { SetProperty(ref IsSuccessfulValue, value); }

        } // Added from Calculations.cs
    }
}
