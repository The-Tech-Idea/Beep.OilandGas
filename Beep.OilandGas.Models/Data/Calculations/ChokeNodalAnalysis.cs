using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.Calculations
{
    public class ChokeNodalAnalysis : ModelEntityBase
    {
        private string AnalysisIdValue = string.Empty;

        public string AnalysisId

        {

            get { return this.AnalysisIdValue; }

            set { SetProperty(ref AnalysisIdValue, value); }

        }
        private DateTime AnalysisDateValue;

        public DateTime AnalysisDate

        {

            get { return this.AnalysisDateValue; }

            set { SetProperty(ref AnalysisDateValue, value); }

        }
        private string WellUWIValue = string.Empty;

        public string WellUWI

        {

            get { return this.WellUWIValue; }

            set { SetProperty(ref WellUWIValue, value); }

        }
        private string NodalPointValue = string.Empty;

        public string NodalPoint

        {

            get { return this.NodalPointValue; }

            set { SetProperty(ref NodalPointValue, value); }

        } // Reservoir, Downhole, Wellhead, Surface
        private decimal ReservoirPressureValue;

        public decimal ReservoirPressure

        {

            get { return this.ReservoirPressureValue; }

            set { SetProperty(ref ReservoirPressureValue, value); }

        }
        private decimal BubblePointPressureValue;

        public decimal BubblePointPressure

        {

            get { return this.BubblePointPressureValue; }

            set { SetProperty(ref BubblePointPressureValue, value); }

        }
        private decimal TubingHeadPressureValue;

        public decimal TubingHeadPressure

        {

            get { return this.TubingHeadPressureValue; }

            set { SetProperty(ref TubingHeadPressureValue, value); }

        }
        private decimal SeparatorPressureValue;

        public decimal SeparatorPressure

        {

            get { return this.SeparatorPressureValue; }

            set { SetProperty(ref SeparatorPressureValue, value); }

        }
        private decimal CurrentProductionValue;

        public decimal CurrentProduction

        {

            get { return this.CurrentProductionValue; }

            set { SetProperty(ref CurrentProductionValue, value); }

        }
        private decimal OptimalProductionValue;

        public decimal OptimalProduction

        {

            get { return this.OptimalProductionValue; }

            set { SetProperty(ref OptimalProductionValue, value); }

        }
        private decimal ChokeBackPressureValue;

        public decimal ChokeBackPressure

        {

            get { return this.ChokeBackPressureValue; }

            set { SetProperty(ref ChokeBackPressureValue, value); }

        }
        private decimal TubingFrictionLossValue;

        public decimal TubingFrictionLoss

        {

            get { return this.TubingFrictionLossValue; }

            set { SetProperty(ref TubingFrictionLossValue, value); }

        }
        private decimal ElevationChangeValue;

        public decimal ElevationChange

        {

            get { return this.ElevationChangeValue; }

            set { SetProperty(ref ElevationChangeValue, value); }

        }
        private decimal AccelerationLossValue;

        public decimal AccelerationLoss

        {

            get { return this.AccelerationLossValue; }

            set { SetProperty(ref AccelerationLossValue, value); }

        }
        private string ConstrainedByValue = string.Empty;

        public string ConstrainedBy

        {

            get { return this.ConstrainedByValue; }

            set { SetProperty(ref ConstrainedByValue, value); }

        } // Reservoir, Choke, Tubing, Separator
        private string RecommendationValue = string.Empty;

        public string Recommendation

        {

            get { return this.RecommendationValue; }

            set { SetProperty(ref RecommendationValue, value); }

        }
        private List<NodalPoint> NodalPointDataValue = new();

        public List<NodalPoint> NodalPointData

        {

            get { return this.NodalPointDataValue; }

            set { SetProperty(ref NodalPointDataValue, value); }

        }
    }
}
