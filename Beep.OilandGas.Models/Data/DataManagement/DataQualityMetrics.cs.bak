using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class DataQualityMetrics : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private int TotalRecordsValue;

        public int TotalRecords

        {

            get { return this.TotalRecordsValue; }

            set { SetProperty(ref TotalRecordsValue, value); }

        }
        private int CompleteRecordsValue;

        public int CompleteRecords

        {

            get { return this.CompleteRecordsValue; }

            set { SetProperty(ref CompleteRecordsValue, value); }

        }
        private int IncompleteRecordsValue;

        public int IncompleteRecords

        {

            get { return this.IncompleteRecordsValue; }

            set { SetProperty(ref IncompleteRecordsValue, value); }

        }
        private double CompletenessScoreValue;

        public double CompletenessScore

        {

            get { return this.CompletenessScoreValue; }

            set { SetProperty(ref CompletenessScoreValue, value); }

        } // 0-100
        private double AccuracyScoreValue;

        public double AccuracyScore

        {

            get { return this.AccuracyScoreValue; }

            set { SetProperty(ref AccuracyScoreValue, value); }

        } // 0-100
        private double ConsistencyScoreValue;

        public double ConsistencyScore

        {

            get { return this.ConsistencyScoreValue; }

            set { SetProperty(ref ConsistencyScoreValue, value); }

        } // 0-100
        private double OverallQualityScoreValue;

        public double OverallQualityScore

        {

            get { return this.OverallQualityScoreValue; }

            set { SetProperty(ref OverallQualityScoreValue, value); }

        } // 0-100
        public Dictionary<string, FieldQualityMetrics> FieldMetrics { get; set; } = new Dictionary<string, FieldQualityMetrics>();
    }
}
