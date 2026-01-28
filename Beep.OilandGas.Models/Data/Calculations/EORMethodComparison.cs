using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class EORMethodComparison : ModelEntityBase
    {
        private string FieldIdValue;

        public string FieldId

        {

            get { return this.FieldIdValue; }

            set { SetProperty(ref FieldIdValue, value); }

        }
        private int MethodsComparedValue;

        public int MethodsCompared

        {

            get { return this.MethodsComparedValue; }

            set { SetProperty(ref MethodsComparedValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        public List<EORMethodScore> MethodScores { get; set; } = new();
        private List<RankedEORMethod> RankedMethodsValue = new();

        public List<RankedEORMethod> RankedMethods

        {

            get { return this.RankedMethodsValue; }

            set { SetProperty(ref RankedMethodsValue, value); }

        }
        private string RecommendedMethodValue;

        public string RecommendedMethod

        {

            get { return this.RecommendedMethodValue; }

            set { SetProperty(ref RecommendedMethodValue, value); }

        }
        private double SynergyPotentialValue;

        public double SynergyPotential

        {

            get { return this.SynergyPotentialValue; }

            set { SetProperty(ref SynergyPotentialValue, value); }

        }
    }
}
