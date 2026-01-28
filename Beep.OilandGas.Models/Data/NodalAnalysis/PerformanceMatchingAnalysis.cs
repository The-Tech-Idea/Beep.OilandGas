using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class PerformanceMatchingAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string WellUWIValue = string.Empty;

         public string WellUWI

         {

             get { return this.WellUWIValue; }

             set { SetProperty(ref WellUWIValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal CurrentFlowRateValue;

         public decimal CurrentFlowRate

         {

             get { return this.CurrentFlowRateValue; }

             set { SetProperty(ref CurrentFlowRateValue, value); }

         }
         private decimal CurrentBottomholePressureValue;

         public decimal CurrentBottomholePressure

         {

             get { return this.CurrentBottomholePressureValue; }

             set { SetProperty(ref CurrentBottomholePressureValue, value); }

         }
         private decimal MarginToBubblePointValue;

         public decimal MarginToBubblePoint

         {

             get { return this.MarginToBubblePointValue; }

             set { SetProperty(ref MarginToBubblePointValue, value); }

         }
         private string SurfaceBottleneckValue = string.Empty;

         public string SurfaceBottleneck

         {

             get { return this.SurfaceBottleneckValue; }

             set { SetProperty(ref SurfaceBottleneckValue, value); }

         }
         private string ReservoirBottleneckValue = string.Empty;

         public string ReservoirBottleneck

         {

             get { return this.ReservoirBottleneckValue; }

             set { SetProperty(ref ReservoirBottleneckValue, value); }

         }
         private decimal ForecastedDeclineValue;

         public decimal ForecastedDecline

         {

             get { return this.ForecastedDeclineValue; }

             set { SetProperty(ref ForecastedDeclineValue, value); }

         }
     }
}
