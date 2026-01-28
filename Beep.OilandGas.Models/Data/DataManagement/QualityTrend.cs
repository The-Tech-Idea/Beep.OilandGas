using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class QualityTrend : ModelEntityBase
    {
        private DateTime DateValue;

        public DateTime Date

        {

            get { return this.DateValue; }

            set { SetProperty(ref DateValue, value); }

        }
        private double QualityScoreValue;

        public double QualityScore

        {

            get { return this.QualityScoreValue; }

            set { SetProperty(ref QualityScoreValue, value); }

        }
        private int RecordCountValue;

        public int RecordCount

        {

            get { return this.RecordCountValue; }

            set { SetProperty(ref RecordCountValue, value); }

        }
        private int IssueCountValue;

        public int IssueCount

        {

            get { return this.IssueCountValue; }

            set { SetProperty(ref IssueCountValue, value); }

        }
    }
}
