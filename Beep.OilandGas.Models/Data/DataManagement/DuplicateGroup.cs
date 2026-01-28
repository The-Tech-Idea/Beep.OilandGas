using System;
using System.Collections.Generic;
using TheTechIdea.Beep.Report;

namespace Beep.OilandGas.Models.Data
{
    public class DuplicateGroup : ModelEntityBase
    {
        private string TableNameValue;

        public string TableName

        {

            get { return this.TableNameValue; }

            set { SetProperty(ref TableNameValue, value); }

        }
        private List<DuplicateRecord> RecordsValue = new List<DuplicateRecord>();

        public List<DuplicateRecord> Records

        {

            get { return this.RecordsValue; }

            set { SetProperty(ref RecordsValue, value); }

        }
        private double SimilarityScoreValue;

        public double SimilarityScore

        {

            get { return this.SimilarityScoreValue; }

            set { SetProperty(ref SimilarityScoreValue, value); }

        }
        private List<string> MatchFieldsValue = new List<string>();

        public List<string> MatchFields

        {

            get { return this.MatchFieldsValue; }

            set { SetProperty(ref MatchFieldsValue, value); }

        }
    }
}
