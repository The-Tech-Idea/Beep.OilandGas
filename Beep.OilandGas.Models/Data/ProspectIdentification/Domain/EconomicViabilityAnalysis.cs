using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class EconomicViabilityAnalysis : ModelEntityBase
     {
         private string AnalysisIdValue = string.Empty;

         public string AnalysisId

         {

             get { return this.AnalysisIdValue; }

             set { SetProperty(ref AnalysisIdValue, value); }

         }
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private DateTime AnalysisDateValue;

         public DateTime AnalysisDate

         {

             get { return this.AnalysisDateValue; }

             set { SetProperty(ref AnalysisDateValue, value); }

         }
         private decimal EstimatedCapitalCostValue;

         public decimal EstimatedCapitalCost

         {

             get { return this.EstimatedCapitalCostValue; }

             set { SetProperty(ref EstimatedCapitalCostValue, value); }

         }
         private decimal EstimatedOperatingCostValue;

         public decimal EstimatedOperatingCost

         {

             get { return this.EstimatedOperatingCostValue; }

             set { SetProperty(ref EstimatedOperatingCostValue, value); }

         }
         private decimal OilPriceValue;

         public decimal OilPrice

         {

             get { return this.OilPriceValue; }

             set { SetProperty(ref OilPriceValue, value); }

         }
         private decimal GasPriceValue;

         public decimal GasPrice

         {

             get { return this.GasPriceValue; }

             set { SetProperty(ref GasPriceValue, value); }

         }
         private decimal DiscountRateValue;

         public decimal DiscountRate

         {

             get { return this.DiscountRateValue; }

             set { SetProperty(ref DiscountRateValue, value); }

         }
         private int ProjectLifeYearsValue;

         public int ProjectLifeYears

         {

             get { return this.ProjectLifeYearsValue; }

             set { SetProperty(ref ProjectLifeYearsValue, value); }

         }
         private decimal NetPresentValueValue;

         public decimal NetPresentValue

         {

             get { return this.NetPresentValueValue; }

             set { SetProperty(ref NetPresentValueValue, value); }

         }
         private decimal InternalRateOfReturnValue;

         public decimal InternalRateOfReturn

         {

             get { return this.InternalRateOfReturnValue; }

             set { SetProperty(ref InternalRateOfReturnValue, value); }

         }
         private decimal PaybackPeriodYearsValue;

         public decimal PaybackPeriodYears

         {

             get { return this.PaybackPeriodYearsValue; }

             set { SetProperty(ref PaybackPeriodYearsValue, value); }

         }
         private decimal ProfitabilityIndexValue;

         public decimal ProfitabilityIndex

         {

             get { return this.ProfitabilityIndexValue; }

             set { SetProperty(ref ProfitabilityIndexValue, value); }

         }
         private string ViabilityStatusValue = string.Empty;

         public string ViabilityStatus

         {

             get { return this.ViabilityStatusValue; }

             set { SetProperty(ref ViabilityStatusValue, value); }

         } // Viable, Marginal, Non-Viable
     }
}
