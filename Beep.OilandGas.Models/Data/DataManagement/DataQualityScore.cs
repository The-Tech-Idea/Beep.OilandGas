using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class DataQualityScore : ModelEntityBase
    {
        private object EntityValue;

        public object Entity

        {

            get { return this.EntityValue; }

            set { SetProperty(ref EntityValue, value); }

        }
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private double OverallScoreValue;

        public double OverallScore

        {

            get { return this.OverallScoreValue; }

            set { SetProperty(ref OverallScoreValue, value); }

        } // 0-100
        public Dictionary<string, double> FieldScores { get; set; } = new Dictionary<string, double>();
        private List<DataQualityIssue> IssuesValue = new List<DataQualityIssue>();

        public List<DataQualityIssue> Issues

        {

            get { return this.IssuesValue; }

            set { SetProperty(ref IssuesValue, value); }

        }
    }
}
