using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class PortfolioOptimizationResult : ModelEntityBase
     {
         private string OptimizationIdValue = string.Empty;

         public string OptimizationId

         {

             get { return this.OptimizationIdValue; }

             set { SetProperty(ref OptimizationIdValue, value); }

         }
         private DateTime OptimizationDateValue;

         public DateTime OptimizationDate

         {

             get { return this.OptimizationDateValue; }

             set { SetProperty(ref OptimizationDateValue, value); }

         }
         private List<string> RecommendedProspectsValue = new();

         public List<string> RecommendedProspects

         {

             get { return this.RecommendedProspectsValue; }

             set { SetProperty(ref RecommendedProspectsValue, value); }

         }
         private List<string> MarginallProspectsValue = new();

         public List<string> MarginallProspects

         {

             get { return this.MarginallProspectsValue; }

             set { SetProperty(ref MarginallProspectsValue, value); }

         }
         private List<string> RejectedProspectsValue = new();

         public List<string> RejectedProspects

         {

             get { return this.RejectedProspectsValue; }

             set { SetProperty(ref RejectedProspectsValue, value); }

         }
         private decimal TotalPortfolioRiskValue;

         public decimal TotalPortfolioRisk

         {

             get { return this.TotalPortfolioRiskValue; }

             set { SetProperty(ref TotalPortfolioRiskValue, value); }

         }
         private decimal TotalExpectedValueValue;

         public decimal TotalExpectedValue

         {

             get { return this.TotalExpectedValueValue; }

             set { SetProperty(ref TotalExpectedValueValue, value); }

         }
         private decimal RiskAdjustedReturnValue;

         public decimal RiskAdjustedReturn

         {

             get { return this.RiskAdjustedReturnValue; }

             set { SetProperty(ref RiskAdjustedReturnValue, value); }

         }
         private string OptimizationStrategyValue = string.Empty;

         public string OptimizationStrategy

         {

             get { return this.OptimizationStrategyValue; }

             set { SetProperty(ref OptimizationStrategyValue, value); }

         }
         private List<string> OptimizationRecommendationsValue = new();

         public List<string> OptimizationRecommendations

         {

             get { return this.OptimizationRecommendationsValue; }

             set { SetProperty(ref OptimizationRecommendationsValue, value); }

         }
     }
}
