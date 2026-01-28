using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class InjectionWellOptimization : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private int DesiredWellCountValue;

        public int DesiredWellCount

        {

            get { return this.DesiredWellCountValue; }

            set { SetProperty(ref DesiredWellCountValue, value); }

        }
        private double ReservoirAreaValue;

        public double ReservoirArea

        {

            get { return this.ReservoirAreaValue; }

            set { SetProperty(ref ReservoirAreaValue, value); }

        }
        private double ReservoirThicknessValue;

        public double ReservoirThickness

        {

            get { return this.ReservoirThicknessValue; }

            set { SetProperty(ref ReservoirThicknessValue, value); }

        }
        private double PermeabilityValue;

        public double Permeability

        {

            get { return this.PermeabilityValue; }

            set { SetProperty(ref PermeabilityValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private double OptimalWellSpacingValue;

        public double OptimalWellSpacing

        {

            get { return this.OptimalWellSpacingValue; }

            set { SetProperty(ref OptimalWellSpacingValue, value); }

        }
        private double AreaPerWellValue;

        public double AreaPerWell

        {

            get { return this.AreaPerWellValue; }

            set { SetProperty(ref AreaPerWellValue, value); }

        }
        private double MaxInjectionRatePerWellValue;

        public double MaxInjectionRatePerWell

        {

            get { return this.MaxInjectionRatePerWellValue; }

            set { SetProperty(ref MaxInjectionRatePerWellValue, value); }

        }
        private double TotalInjectionCapacityValue;

        public double TotalInjectionCapacity

        {

            get { return this.TotalInjectionCapacityValue; }

            set { SetProperty(ref TotalInjectionCapacityValue, value); }

        }
        private double EstimatedArealSweepValue;

        public double EstimatedArealSweep

        {

            get { return this.EstimatedArealSweepValue; }

            set { SetProperty(ref EstimatedArealSweepValue, value); }

        }
        private string SuggestedPlacementPatternValue;

        public string SuggestedPlacementPattern

        {

            get { return this.SuggestedPlacementPatternValue; }

            set { SetProperty(ref SuggestedPlacementPatternValue, value); }

        }
        private List<string> RiskFactorsValue = new();

        public List<string> RiskFactors

        {

            get { return this.RiskFactorsValue; }

            set { SetProperty(ref RiskFactorsValue, value); }

        }
    }
}
