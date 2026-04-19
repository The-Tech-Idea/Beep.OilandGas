using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TheTechIdea.Beep.Editor;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.ProspectIdentification
{
    public class ProspectRanking : ModelEntityBase
     {
         private string ProspectIdValue = string.Empty;

         public string ProspectId

         {

             get { return this.ProspectIdValue; }

             set { SetProperty(ref ProspectIdValue, value); }

         }
         private string ProspectNameValue = string.Empty;

         public string ProspectName

         {

             get { return this.ProspectNameValue; }

             set { SetProperty(ref ProspectNameValue, value); }

         }
         private int RankValue;

         public int Rank

         {

             get { return this.RankValue; }

             set { SetProperty(ref RankValue, value); }

         }
         private decimal ScoreValue;

         public decimal Score

         {

             get { return this.ScoreValue; }

             set { SetProperty(ref ScoreValue, value); }

         }
         private decimal WeightedScoreValue;

         public decimal WeightedScore

         {

             get { return this.WeightedScoreValue; }

             set { SetProperty(ref WeightedScoreValue, value); }

         }
         private List<CriteriaScoring> CriteriaScoresValue = new();

         public List<CriteriaScoring> CriteriaScores

         {

             get { return this.CriteriaScoresValue; }

             set { SetProperty(ref CriteriaScoresValue, value); }

         }
     }
}
