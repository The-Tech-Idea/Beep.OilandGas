namespace Beep.OilandGas.Models.Data.WellTestAnalysis
{
    using System;

using Beep.OilandGas.Models.Data;    using System.Collections.Generic;
    /// <summary>
    /// Represents the result of a well test analysis
    /// DTO for calculations - Entity class: WELL_TEST_ANALYSIS_RESULT
    /// </summary>
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
        
        public Dictionary<string, object>? AdditionalResults { get; set; } // Added from Calculations.cs
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






