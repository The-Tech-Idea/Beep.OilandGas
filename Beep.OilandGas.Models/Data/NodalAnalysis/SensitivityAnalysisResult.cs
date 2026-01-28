using System;
using System.Collections.Generic;
using Beep.OilandGas.Models.Data.NodalAnalysis;
using Beep.OilandGas.Models.Data.NodalAnalysis;

namespace Beep.OilandGas.Models.Data
{
     public class EconomicSensitivityAnalysisResult : ModelEntityBase
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
         public Dictionary<string, decimal> SensitivityFactors { get; set; } = new();
         private string MostSensitiveParameterValue = string.Empty;

         public string MostSensitiveParameter

         {

             get { return this.MostSensitiveParameterValue; }

             set { SetProperty(ref MostSensitiveParameterValue, value); }

         }

        public List<double> PriceVariation { get; set; }
        public List<double> VolumeVariation { get; set; }
        public List<double> CostVariation { get; set; }
    }
}
