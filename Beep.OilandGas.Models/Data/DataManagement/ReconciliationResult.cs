using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class ReconciliationResult : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private int Source1RecordCountValue;

        public int Source1RecordCount

        {

            get { return this.Source1RecordCountValue; }

            set { SetProperty(ref Source1RecordCountValue, value); }

        }
        private int Source2RecordCountValue;

        public int Source2RecordCount

        {

            get { return this.Source2RecordCountValue; }

            set { SetProperty(ref Source2RecordCountValue, value); }

        }
        private int MatchingRecordsValue;

        public int MatchingRecords

        {

            get { return this.MatchingRecordsValue; }

            set { SetProperty(ref MatchingRecordsValue, value); }

        }
        private int Source1OnlyRecordsValue;

        public int Source1OnlyRecords

        {

            get { return this.Source1OnlyRecordsValue; }

            set { SetProperty(ref Source1OnlyRecordsValue, value); }

        }
        private int Source2OnlyRecordsValue;

        public int Source2OnlyRecords

        {

            get { return this.Source2OnlyRecordsValue; }

            set { SetProperty(ref Source2OnlyRecordsValue, value); }

        }
        private int DifferingRecordsValue;

        public int DifferingRecords

        {

            get { return this.DifferingRecordsValue; }

            set { SetProperty(ref DifferingRecordsValue, value); }

        }
        private List<ReconciliationDifference> DifferencesValue = new List<ReconciliationDifference>();

        public List<ReconciliationDifference> Differences

        {

            get { return this.DifferencesValue; }

            set { SetProperty(ref DifferencesValue, value); }

        }
    }
}
