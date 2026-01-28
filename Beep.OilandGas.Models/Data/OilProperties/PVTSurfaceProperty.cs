using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Beep.OilandGas.Models.Data
{
     public class PVTSurfaceProperty : ModelEntityBase
     {
         private string PropertyIdValue = string.Empty;

         public string PropertyId

         {

             get { return this.PropertyIdValue; }

             set { SetProperty(ref PropertyIdValue, value); }

         }
         private string CompositionIdValue = string.Empty;

         public string CompositionId

         {

             get { return this.CompositionIdValue; }

             set { SetProperty(ref CompositionIdValue, value); }

         }
         private DateTime PredictionDateValue;

         public DateTime PredictionDate

         {

             get { return this.PredictionDateValue; }

             set { SetProperty(ref PredictionDateValue, value); }

         }
         private decimal StockTankOilGravityValue;

         public decimal StockTankOilGravity

         {

             get { return this.StockTankOilGravityValue; }

             set { SetProperty(ref StockTankOilGravityValue, value); }

         }
         private decimal StockTankOilDensityValue;

         public decimal StockTankOilDensity

         {

             get { return this.StockTankOilDensityValue; }

             set { SetProperty(ref StockTankOilDensityValue, value); }

         }
         private decimal ResidualGasGravityValue;

         public decimal ResidualGasGravity

         {

             get { return this.ResidualGasGravityValue; }

             set { SetProperty(ref ResidualGasGravityValue, value); }

         }
         private decimal SeparationRatioValue;

         public decimal SeparationRatio

         {

             get { return this.SeparationRatioValue; }

             set { SetProperty(ref SeparationRatioValue, value); }

         }
         private decimal SolubilityAtSurfaceConditionsValue;

         public decimal SolubilityAtSurfaceConditions

         {

             get { return this.SolubilityAtSurfaceConditionsValue; }

             set { SetProperty(ref SolubilityAtSurfaceConditionsValue, value); }

         }
         private string AnalysisMethodValue = string.Empty;

         public string AnalysisMethod

         {

             get { return this.AnalysisMethodValue; }

             set { SetProperty(ref AnalysisMethodValue, value); }

         }
     }
}
