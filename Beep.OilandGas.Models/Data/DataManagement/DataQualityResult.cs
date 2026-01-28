using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;
using Beep.OilandGas.Models.Data;

namespace Beep.OilandGas.Models.Data.DataManagement
{
    public class DataQualityResult : ModelEntityBase
    {
        private string TableNameValue = string.Empty;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        }
        public Dictionary<string, double> FieldQualityScores { get; set; } = new Dictionary<string, double>();
        private int TotalRowsValue;

        public int TotalRows

        {

            get { return this.TotalRowsValue; }

            set { SetProperty(ref TotalRowsValue, value); }

        }
        private int CompleteRowsValue;

        public int CompleteRows

        {

            get { return this.CompleteRowsValue; }

            set { SetProperty(ref CompleteRowsValue, value); }

        }
        private List<string> QualityIssuesValue = new List<string>();

        public List<string> QualityIssues

        {

            get { return this.QualityIssuesValue; }

            set { SetProperty(ref QualityIssuesValue, value); }

        }
    }
}
